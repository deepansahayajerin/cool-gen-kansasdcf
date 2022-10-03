// Program: SI_FADS_FOREIGN_ADDRESS_MAINT, ID: 371801487, model: 746.
// Short name: SWEFADSP
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
/// A program: SI_FADS_FOREIGN_ADDRESS_MAINT.
/// </para>
/// <para>
/// RESP: SRVINIT
/// This procedure adds, updates and deletes
/// addresses for APs and ARs.  It also allows requests for postmaster letters 
/// and updates any information pertaining to these letters.
/// </para>
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class SiFadsForeignAddressMaint: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_FADS_FOREIGN_ADDRESS_MAINT program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiFadsForeignAddressMaint(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiFadsForeignAddressMaint.
  /// </summary>
  public SiFadsForeignAddressMaint(IContext context, Import import,
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
    // Date	  Author		Reason
    // 09/14/95  Sid			Initial Development.
    // 02/16/96  Lewis			Complete Development
    // 05/01/96  Rao			Changes to Prompts
    // 				IDCR# 127 & 132
    // 11/01/96  G. Lofton		Add new security.
    // 07/17/97  Sid			Fix Bugs. Add events.
    // 01/07/98  Sid			Change Address Type to "M" from "F".
    // ------------------------------------------------------------
    // 12/17/98  W.Campbell            Made several changes
    //                                 
    // to the screen and logic
    // including:
    //                                 
    // Removed START_DATE and
    //                                 
    // VERIFIED_CODE from the Screen
    //                                 
    // and logic.  Added LAST_UPDATE
    //                                 
    // timestamp to the screen and
    // logic.
    //                                 
    // START_DATE and VERIFIED_CODE
    //                                 
    // are to be removed from the model
    //                                 
    // under IDCR454.
    // -------------------------------------------------------
    // 12/22/98  W.Campbell            Logic to prevent
    //                                 
    // ADD or UPDATE on a closed CASE
    //                                 
    // or an inactive AP was disabled.
    //                                 
    // Work done on IDCR454.
    // ---------------------------------------------
    // 12/22/98  W.Campbell            Duplicate logic to
    //                                 
    // validate SEND DATE was disabled.
    //                                 
    // Work done on IDCR454.
    // ---------------------------------------------
    // 12/22/98  W.Campbell            Logic added to
    //                                 
    // validate that the user has
    //                                 
    // entered one or more of
    //                                 
    // SEND DATE, VERIFIED DATE
    //                                 
    // or END DATE on an ADD
    //                                 
    // or UPDATE.
    //                                 
    // Work done on IDCR454.
    // ---------------------------------------------
    // 12/22/98  W.Campbell            Disabled duplicate logic
    //                                 
    // dealing with SEND DATE
    //                                 
    // validation and change
    //                                 
    // on an UPDATE.
    //                                 
    // Work done on IDCR454.
    // ---------------------------------------------
    // 01/04/99 W.Campbell             Logic added to make
    //                                 
    // sure END CODE is entered
    //                                 
    // with END DATE.
    //                                 
    // Work done on IDCR454.
    // ---------------------------------------------
    // 01/27/99 W.Campbell             The logic was
    //                                 
    // modified to only provide a
    //                                 
    // required data missing error
    //                                 
    // if there is no ap and it has
    //                                 
    // been selected.
    // --------------------------------------------
    // 02/06/1999 M Ramirez		Added creation of document trigger.
    // --------------------------------------------
    // 02/18/99 M Ramirez & W.Campbell
    //         Disabled statements
    //         dealing with closing monitored documents,
    //         as it has been determined that the best way
    //         to handle them will be in Batch.
    // ---------------------------------------------
    // 03/29/99 W.Campbell             Added attribute to
    //                                 
    // local sp_doc_key view for the
    //                                 
    // address identifier and a set
    //                                 
    // statement to set the attribute
    //                                 
    // to the address identifier for
    //                                 
    // passing it to the called CAB -
    //                                 
    // SP_CREATE_DOCUMENT_INFRASTRUCT.
    //                                 
    // This was to fix a problem with
    //                                 
    // producing the POSTMASTER
    //                                 
    // letter.
    // ---------------------------------------------
    // 05/03/99 W.Campbell             Added code to send
    //                                 
    // selection needed msg to COMP.
    //                                 
    // BE SURE TO MATCH
    //                                 
    // next_tran_info ON THE
    //                                 
    // DIALOG FLOW.
    // -----------------------------------------------
    // 05/20/99 W.Campbell             Inserted additional use of
    //                                 
    // rollback eab to prevent problems
    //                                 
    // with DB modifications which
    //                                 
    // should not be committed.
    // --------------------------------------------
    // 05/20/99 W.Campbell             Added code to
    //                                 
    // process interstate event for AP
    //                                 
    // Foreign address located (LSOUT)
    //                                 
    // with no end date.
    // ---------------------------------------------
    // 05/20/99 W.Campbell             Added code to
    //                                 
    // process interstate event for AR
    //                                 
    // Foreign address located (GSPAD)
    //                                 
    // with no end date.
    // ---------------------------------------------
    // 05/20/99 W.Campbell             Replaced
    //                                 
    // ZDelete exit states.
    // -----------------------------------------------------------
    // 05/20/99 W.Campbell             Inserted logic to save
    //                                 
    // and restore the exit state
    //                                 
    // when dealing with
    //                                 
    // interstate processing.
    // 01/03/00	PMcElderry
    // Changed "POSTMAST" document creation from batch to an
    // online process; added print return logic
    // -----------------------------------------------------------
    // 07/28/00 M.Lachowicz             Call CSNET every time
    //                                  
    // when valid address is updated.
    //                                  
    // PR 100337.
    // -----------------------------------------------------------
    // 11/22/00 M.Lachowicz           Changed header line.
    //                                
    // WR298.
    // -------------------------------------------------------------------------
    // 11/22/00 M.Lachowicz           Do not display address
    //                                
    // if particular person has family
    // violence.
    //                                
    // PR106104.
    // -------------------------------------------------------------------------
    // 06/06/01 M.Lachowicz           Do not allow future date greater than
    //                                
    // current date plus 6 months.
    // WR283.
    // -------------------------------------------------------------------------
    // 03/06/02   K. Cole		Do not allow addresses to be added for organizations.
    // PR136960
    // -----------------------------------------------------------------------------------
    // 12/10/2007      M. Fan	  PR 00187149 / CQ416   Changed to display user's 
    // selection for AP or AR when ADDR flows to FADS.
    // ------------------------------------------------------------------------------------------------------
    // 08/02/10 JHuss			Updated code to allow scrolling to next page when group 
    // has 3 entries.
    // ------------------------------------------------------------------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";
    UseOeCabSetMnemonics();
    MoveNextTranInfo(import.HiddenNextTranInfo, export.HiddenNextTranInfo);

    // 06/06/2001 M.L Start
    local.Current.Date = Now().Date;
    local.TodayPlus6Months.Date = AddMonths(local.Current.Date, 6);

    // 06/06/2001 M.L End
    UseSpDocSetLiterals();

    if (Equal(global.Command, "CLEAR"))
    {
      return;
    }

    // ---------------------------------------------
    // Move Imports to Exports
    // ---------------------------------------------
    local.SendDateUpdated.Flag = "N";
    export.Case1.Number = import.Case1.Number;
    export.Next.Number = import.Next.Number;
    export.HiddenNext.Number = import.HiddenNext.Number;
    MoveCsePersonsWorkSet(import.ApCsePersonsWorkSet, export.ApCsePersonsWorkSet);
      
    MoveCsePersonsWorkSet(import.ArCsePersonsWorkSet, export.ArCsePersonsWorkSet);
      
    MoveCsePersonsWorkSet(import.ArFromCaseRole, export.ArFromCaseRole);
    export.ApCommon.SelectChar = import.ApCommon.SelectChar;
    export.ArCommon.SelectChar = import.ArCommon.SelectChar;
    MoveCsePersonAddress5(import.Last, export.Last);
    export.Start.Number = import.Search.Number;
    export.ScrollMinus.Text1 = import.ScrollMinus.Text1;
    export.ScrollPlus.Text1 = import.ScrollPlus.Text1;
    export.PromptCaseComp.SelectChar = import.PromptCaseComp.SelectChar;
    export.PromptRoleCase.SelectChar = import.PromptRoleCase.SelectChar;
    export.Standard.NextTransaction = import.Standard.NextTransaction;
    MoveOffice(import.Office, export.Office);
    export.ServiceProvider.LastName = import.ServiceProvider.LastName;

    // 11/22/00 M.L Start
    export.HeaderLine.Text35 = import.HeaderLine.Text35;

    // 11/22/00 M.L End
    export.ApActive.Flag = import.ApActive.Flag;
    export.CaseOpen.Flag = import.CaseOpen.Flag;
    export.HiddenCsePerson.Number = import.HiddenCsePerson.Number;

    if (!import.Import1.IsEmpty)
    {
      for(import.Import1.Index = 0; import.Import1.Index < import
        .Import1.Count; ++import.Import1.Index)
      {
        if (!import.Import1.CheckSize())
        {
          break;
        }

        export.Export1.Index = import.Import1.Index;
        export.Export1.CheckSize();

        export.Export1.Update.DetailCommon.SelectChar =
          import.Import1.Item.DetailCommon.SelectChar;
        MoveCsePersonAddress1(import.Import1.Item.DetailCsePersonAddress,
          export.Export1.Update.DetailCsePersonAddress);
        export.Export1.Update.PromptCountry.SelectChar =
          import.Import1.Item.PromptCountry.SelectChar;
        export.Export1.Update.PromptEndCode.SelectChar =
          import.Import1.Item.PromptEndCode.SelectChar;
        export.Export1.Update.PromptSourceCode.SelectChar =
          import.Import1.Item.PromptSourceCode.SelectChar;
        export.Export1.Update.Hdet.Assign(import.Import1.Item.Hdet);
      }

      import.Import1.CheckIndex();
    }

    // ---------------------------------------------
    // Move Hidden Import Views to Hidden Export Views
    // ---------------------------------------------
    if (!import.Hidden.IsEmpty)
    {
      for(import.Hidden.Index = 0; import.Hidden.Index < import.Hidden.Count; ++
        import.Hidden.Index)
      {
        if (!import.Hidden.CheckSize())
        {
          break;
        }

        export.Hidden.Index = import.Hidden.Index;
        export.Hidden.CheckSize();

        export.Hidden.Update.HiddenUpd.SendDate =
          import.Hidden.Item.HiddenUpd.SendDate;
      }

      import.Hidden.CheckIndex();
    }

    MoveStandard(import.HiddenStandard, export.HiddenStandard);
    export.HpromptLineNo.Count = import.HpromptLineNo.Count;

    if (!import.HiddenPageKeys.IsEmpty)
    {
      for(import.HiddenPageKeys.Index = 0; import.HiddenPageKeys.Index < import
        .HiddenPageKeys.Count; ++import.HiddenPageKeys.Index)
      {
        if (!import.HiddenPageKeys.CheckSize())
        {
          break;
        }

        export.HiddenPageKeys.Index = import.HiddenPageKeys.Index;
        export.HiddenPageKeys.CheckSize();

        MoveCsePersonAddress5(import.HiddenPageKeys.Item.HiddenPageKey,
          export.HiddenPageKeys.Update.HiddenPageKey);
      }

      import.HiddenPageKeys.CheckIndex();
    }

    UseCabZeroFillNumber();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      var field = GetField(export.Next, "number");

      field.Error = true;

      return;
    }

    // ---------------------------------------------
    //         	N E X T   T R A N
    // ---------------------------------------------
    // if the next tran info is not equal to spaces, this implies the user 
    // requested a next tran action. now validate
    if (!IsEmpty(import.Standard.NextTransaction))
    {
      export.HiddenNextTranInfo.CaseNumber = export.Next.Number;
      UseScCabNextTranPut1();

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

      if (IsExitState("ACO_NN0000_ALL_OK"))
      {
        export.Next.Number = export.HiddenNextTranInfo.CaseNumber ?? Spaces(10);
        export.Case1.Number = export.HiddenNextTranInfo.CaseNumber ?? Spaces
          (10);
        export.ApCsePersonsWorkSet.Number =
          export.HiddenNextTranInfo.CsePersonNumber ?? Spaces(10);
        UseCabZeroFillNumber();
      }

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        var field = GetField(export.Next, "number");

        field.Error = true;

        return;
      }

      global.Command = "DISPLAY";
    }

    if (Equal(global.Command, "XXFMMENU"))
    {
      global.Command = "DISPLAY";
    }

    // ---------------------------------------------
    //        S E C U R I T Y   L O G I C
    // ---------------------------------------------
    // to validate action level security
    if (Equal(global.Command, "RETCDVL") || Equal(global.Command, "PRINTRET"))
    {
    }
    else
    {
      UseScCabTestSecurity();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        // 01/12/01 M.L Start
        if (IsExitState("SC0000_DATA_NOT_DISPLAY_FOR_FV"))
        {
          if (Equal(global.Command, "DISPLAY"))
          {
            local.FvCheck.Flag = "Y";
            ExitState = "ACO_NN0000_ALL_OK";
          }
          else
          {
            if (AsChar(export.ArCommon.SelectChar) == 'S')
            {
              local.FvTest.Number = export.ArCsePersonsWorkSet.Number;
            }
            else
            {
              local.FvTest.Number = export.ApCsePersonsWorkSet.Number;
            }

            ExitState = "ACO_NN0000_ALL_OK";
            UseScSecurityValidAuthForFv1();

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              return;
            }
          }
        }

        // 01/12/01 M.L End
      }
    }

    // ---------------------------------------------
    //        P F K E Y   P R O C E S S I N G
    // ---------------------------------------------
    if (Equal(global.Command, "EXIT"))
    {
      ExitState = "ECO_LNK_RETURN_TO_MENU";

      return;
    }

    // ---------------------------------------------
    // 12/22/98 W.Campbell - Logic to prevent
    // ADD or UPDATE on a closed CASE
    // or an inactive AP was disabled.
    // Work done on IDCR454.
    // ---------------------------------------------
    // ---------------------------------------------
    // When the control is returned from a promting
    // screen, populate the data appropriately.
    // ---------------------------------------------
    if (Equal(global.Command, "RETCDVL"))
    {
      if (export.HpromptLineNo.Count != 0)
      {
        export.Export1.Index = export.HpromptLineNo.Count - 1;
        export.Export1.CheckSize();

        if (AsChar(export.Export1.Item.PromptCountry.SelectChar) == 'S')
        {
          if (!IsEmpty(import.Selected.Cdvalue))
          {
            export.Export1.Update.DetailCsePersonAddress.Country =
              import.Selected.Cdvalue;
          }

          var field =
            GetField(export.Export1.Item.DetailCsePersonAddress, "country");

          field.Protected = false;
          field.Focused = true;
        }
        else if (AsChar(export.Export1.Item.PromptEndCode.SelectChar) == 'S')
        {
          if (!IsEmpty(import.Selected.Cdvalue))
          {
            export.Export1.Update.DetailCsePersonAddress.EndCode =
              import.Selected.Cdvalue;
          }

          var field =
            GetField(export.Export1.Item.DetailCsePersonAddress, "endCode");

          field.Protected = false;
          field.Focused = true;
        }
        else if (AsChar(export.Export1.Item.PromptSourceCode.SelectChar) == 'S')
        {
          if (!IsEmpty(import.Selected.Cdvalue))
          {
            export.Export1.Update.DetailCsePersonAddress.Source =
              import.Selected.Cdvalue;
          }

          var field =
            GetField(export.Export1.Item.DetailCsePersonAddress, "source");

          field.Protected = false;
          field.Focused = true;
        }
      }

      export.HpromptLineNo.Count = 0;

      for(export.Export1.Index = 0; export.Export1.Index < export
        .Export1.Count; ++export.Export1.Index)
      {
        if (!export.Export1.CheckSize())
        {
          break;
        }

        export.Export1.Update.PromptCountry.SelectChar = "";
        export.Export1.Update.PromptEndCode.SelectChar = "";
        export.Export1.Update.PromptSourceCode.SelectChar = "";
      }

      export.Export1.CheckIndex();

      return;
    }

    if (Equal(global.Command, "RETURN"))
    {
      if (AsChar(export.PromptRoleCase.SelectChar) == 'S')
      {
        export.PromptRoleCase.SelectChar = "";
        global.Command = "DISPLAY";

        goto Test;
      }

      ExitState = "ACO_NE0000_RETURN";

      return;
    }

Test:

    if (!Equal(export.HiddenNext.Number, export.Next.Number))
    {
      if (IsEmpty(export.Next.Number))
      {
        export.Next.Number = import.HiddenNext.Number;
      }
      else if (!Equal(global.Command, "DISPLAY"))
      {
        var field = GetField(export.Next, "number");

        field.Error = true;

        ExitState = "ACO_NW0000_MUST_DISPLAY_FIRST";

        return;
      }
      else
      {
        // 12/10/2007      M. Fan	  PR 00187149 / CQ416   Commented out the 
        // below SET to retain the value of 'select char' for ar.
        if (!IsEmpty(export.HiddenNext.Number))
        {
          export.ApCsePersonsWorkSet.Number = "";
        }
      }
    }

    // ---------------------------------------------
    // Do not allow scrolling when a selection has
    // been made.
    // ---------------------------------------------
    for(export.Export1.Index = 0; export.Export1.Index < export.Export1.Count; ++
      export.Export1.Index)
    {
      if (!export.Export1.CheckSize())
      {
        break;
      }

      switch(AsChar(export.Export1.Item.DetailCommon.SelectChar))
      {
        case '*':
          break;
        case 'S':
          ++local.Common.Count;

          break;
        case ' ':
          break;
        default:
          var field = GetField(export.Export1.Item.DetailCommon, "selectChar");

          field.Error = true;

          local.Error.Count = local.Common.Count + 1;

          break;
      }
    }

    export.Export1.CheckIndex();

    if ((local.Common.Count > 0 || local.Error.Count > 0) && (
      Equal(global.Command, "PREV") || Equal(global.Command, "NEXT")))
    {
      ExitState = "ACO_NE0000_SCROLL_INVALID_W_SEL";

      return;
    }

    if (local.Common.Count > 1)
    {
      // ---------------------------------------------
      // 05/20/99 W.Campbell - Replaced
      // ZDelete exit states.
      // ---------------------------------------------
      ExitState = "ACO_NE0000_INVALID_MULTIPLE_SEL";

      return;
    }

    if (local.Error.Count > 0)
    {
      // ---------------------------------------------
      // 05/20/99 W.Campbell - Replaced
      // ZDelete exit states.
      // ---------------------------------------------
      ExitState = "ACO_NE0000_INVALID_SELECT_CODE1";

      return;
    }

    if (import.HiddenStandard.PageNumber == 0 || Equal
      (global.Command, "DISPLAY"))
    {
      export.HiddenStandard.PageNumber = 1;
    }

    // --------------------------------------------------
    // Return logic after the print process has completed
    // --------------------------------------------------
    if (Equal(global.Command, "PRINTRET"))
    {
      // -------------------------------------------------------------
      // After the document is Printed (the user may still be looking
      // at WordPerfect), control is returned here.  Any cleanup
      // processing which is necessary after a print, should be done
      // now.
      // -------------------------------------------------------------
      UseScCabNextTranGet();

      // ----------------------------------
      // Extract identifiers from next tran
      // ----------------------------------
      export.Next.Number = export.HiddenNextTranInfo.CaseNumber ?? Spaces(10);
      local.Position.Count =
        Find(String(
          export.HiddenNextTranInfo.MiscText1,
        NextTranInfo.MiscText1_MaxLength),
        TrimEnd(local.SpDocLiteral.IdPrNumber));

      if (local.Position.Count > 0)
      {
        local.SpDocKey.KeyPerson =
          Substring(export.HiddenNextTranInfo.MiscText1, local.Position.Count +
          7, 10);
      }

      local.Position.Count =
        Find(String(
          export.HiddenNextTranInfo.MiscText1,
        NextTranInfo.MiscText1_MaxLength),
        TrimEnd(local.SpDocLiteral.IdPersonAddress));

      if (local.Position.Count > 0)
      {
        local.BatchTimestampWorkArea.TextTimestamp =
          Substring(export.HiddenNextTranInfo.MiscText1, local.Position.Count +
          6, 26);
        local.Position.Count =
          Verify(local.BatchTimestampWorkArea.TextTimestamp, "0123456789.-");

        if (local.Position.Count <= 0)
        {
          local.Asterisk.Identifier =
            Timestamp(local.BatchTimestampWorkArea.TextTimestamp);
        }
      }

      export.ApCsePersonsWorkSet.Number =
        export.HiddenNextTranInfo.CsePersonNumberAp ?? Spaces(10);

      if (Equal(export.ApCsePersonsWorkSet.Number, local.SpDocKey.KeyPerson))
      {
        export.ApCommon.SelectChar = "S";
        export.ArCommon.SelectChar = "";
      }
      else
      {
        export.ApCommon.SelectChar = "";
        export.ArCommon.SelectChar = "S";
      }

      export.HiddenNext.Number = export.Next.Number;
      global.Command = "DISPLAY";
    }

    switch(TrimEnd(global.Command))
    {
      case "SIGNOFF":
        UseScCabSignoff();

        return;
      case "NEXT":
        if (import.HiddenStandard.PageNumber == Import
          .HiddenPageKeysGroup.Capacity)
        {
          ExitState = "MAX_PAGES_REACHED_SYSTEM_ERROR";

          return;
        }

        // ---------------------------------------------
        // Ensure that there is another page of details
        // to retrieve.
        // ---------------------------------------------
        if (IsEmpty(import.ScrollPlus.Text1))
        {
          ExitState = "ACO_NE0000_INVALID_FORWARD";

          return;
        }

        export.HiddenPageKeys.Index = export.HiddenStandard.PageNumber;
        export.HiddenPageKeys.CheckSize();

        ++export.HiddenStandard.PageNumber;
        MoveCsePersonAddress5(export.HiddenPageKeys.Item.HiddenPageKey,
          export.Last);
        UseSiListForeignAddresses1();

        if (export.Export1.Count >= 3)
        {
          export.ScrollPlus.Text1 = "+";

          ++export.HiddenPageKeys.Index;
          export.HiddenPageKeys.CheckSize();

          export.Export1.Index = Export.ExportGroup.Capacity - 1;
          export.Export1.CheckSize();

          MoveCsePersonAddress5(export.Export1.Item.DetailCsePersonAddress,
            export.HiddenPageKeys.Update.HiddenPageKey);
        }
        else
        {
          export.ScrollPlus.Text1 = "";
        }

        export.ScrollMinus.Text1 = "-";

        for(export.Export1.Index = 0; export.Export1.Index < export
          .Export1.Count; ++export.Export1.Index)
        {
          if (!export.Export1.CheckSize())
          {
            break;
          }

          if (Equal(export.Export1.Item.DetailCsePersonAddress.EndDate,
            local.MaxDate.ExpirationDate))
          {
            export.Export1.Update.DetailCsePersonAddress.EndDate =
              local.Zero.Date;
          }
        }

        export.Export1.CheckIndex();

        break;
      case "PREV":
        if (export.HiddenStandard.PageNumber == 1)
        {
          // ---------------------------------------------
          // 05/20/99 W.Campbell - Replaced
          // ZDelete exit states.
          // ---------------------------------------------
          ExitState = "ACO_NE0000_INVALID_BACKWARD";

          return;
        }

        --export.HiddenStandard.PageNumber;

        export.HiddenPageKeys.Index = export.HiddenStandard.PageNumber - 1;
        export.HiddenPageKeys.CheckSize();

        MoveCsePersonAddress5(export.HiddenPageKeys.Item.HiddenPageKey,
          export.Last);
        UseSiListForeignAddresses1();

        if (export.HiddenStandard.PageNumber > 1)
        {
          export.ScrollMinus.Text1 = "-";
        }
        else
        {
          export.ScrollMinus.Text1 = "";
        }

        export.ScrollPlus.Text1 = "+";

        for(export.Export1.Index = 0; export.Export1.Index < export
          .Export1.Count; ++export.Export1.Index)
        {
          if (!export.Export1.CheckSize())
          {
            break;
          }

          if (Equal(export.Export1.Item.DetailCsePersonAddress.EndDate,
            local.MaxDate.ExpirationDate))
          {
            export.Export1.Update.DetailCsePersonAddress.EndDate =
              local.Zero.Date;
          }
        }

        export.Export1.CheckIndex();

        break;
      case "LIST":
        if (IsEmpty(export.Next.Number))
        {
          var field = GetField(export.Next, "number");

          field.Error = true;

          ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";

          return;
        }

        if (AsChar(import.PromptCaseComp.SelectChar) == 'S' && AsChar
          (import.PromptRoleCase.SelectChar) == 'S')
        {
          var field1 = GetField(export.PromptCaseComp, "selectChar");

          field1.Error = true;

          var field2 = GetField(export.PromptRoleCase, "selectChar");

          field2.Error = true;

          // ---------------------------------------------
          // 05/20/99 W.Campbell - Replaced
          // ZDelete exit states.
          // ---------------------------------------------
          ExitState = "ACO_NE0000_INVALID_MULTIPLE_SEL";

          return;
        }

        if (AsChar(import.PromptCaseComp.SelectChar) == 'S')
        {
          ExitState = "ECO_LNK_TO_CASE_COMPOSITION";

          return;
        }
        else if (AsChar(import.PromptRoleCase.SelectChar) == 'S')
        {
          ExitState = "ECO_LNK_TO_CASE_ROLE_MAINTENANCE";

          return;
        }
        else
        {
          if (!IsEmpty(import.PromptCaseComp.SelectChar))
          {
            var field = GetField(export.PromptCaseComp, "selectChar");

            field.Error = true;

            // ---------------------------------------------------
            // 05/20/99 W.Campbell - Replaced ZDelete exit states.
            // ---------------------------------------------------
            ExitState = "ACO_NE0000_INVALID_SELECT_CODE1";
          }

          if (!IsEmpty(import.PromptRoleCase.SelectChar))
          {
            // ---------------------------------------------------
            // 05/20/99 W.Campbell - Replaced ZDelete exit states.
            // ---------------------------------------------------
            var field = GetField(export.PromptRoleCase, "selectChar");

            field.Error = true;

            ExitState = "ACO_NE0000_INVALID_SELECT_CODE";
          }

          if (IsEmpty(import.PromptCaseComp.SelectChar) && IsEmpty
            (import.PromptRoleCase.SelectChar))
          {
            ExitState = "ACO_NE0000_NO_SELECTION_MADE";
          }
        }

        for(export.Export1.Index = 0; export.Export1.Index < export
          .Export1.Count; ++export.Export1.Index)
        {
          if (!export.Export1.CheckSize())
          {
            break;
          }

          ++export.HpromptLineNo.Count;

          if (AsChar(export.Export1.Item.DetailCommon.SelectChar) == 'S')
          {
            if (AsChar(export.Export1.Item.PromptCountry.SelectChar) == 'S')
            {
              export.Prompt.CodeName = local.Country.CodeName;
              ExitState = "ECO_LNK_TO_LIST_CODE_VALUE";

              return;
            }
            else if (!IsEmpty(export.Export1.Item.PromptCountry.SelectChar))
            {
              var field =
                GetField(export.Export1.Item.PromptCountry, "selectChar");

              field.Error = true;

              // ---------------------------------------------------
              // 05/20/99 W.Campbell - Replaced ZDelete exit states.
              // ---------------------------------------------------
              ExitState = "ACO_NE0000_INVALID_SELECT_CODE";
            }
            else
            {
              ExitState = "ACO_NE0000_NO_SELECTION_MADE";

              if (AsChar(export.Export1.Item.PromptEndCode.SelectChar) == 'S')
              {
                export.Prompt.CodeName = local.AddressEndCode.CodeName;
                ExitState = "ECO_LNK_TO_LIST_CODE_VALUE";

                return;
              }
              else if (!IsEmpty(export.Export1.Item.PromptEndCode.SelectChar))
              {
                var field =
                  GetField(export.Export1.Item.PromptEndCode, "selectChar");

                field.Error = true;

                // ---------------------------------------------------
                // 05/20/99 W.Campbell - Replaced ZDelete exit states.
                // ---------------------------------------------------
                ExitState = "ACO_NE0000_INVALID_SELECT_CODE";
              }
              else
              {
                ExitState = "ACO_NE0000_NO_SELECTION_MADE";

                if (AsChar(export.Export1.Item.PromptSourceCode.SelectChar) == 'S'
                  )
                {
                  export.Prompt.CodeName = local.AddressSource.CodeName;
                  ExitState = "ECO_LNK_TO_LIST_CODE_VALUE";

                  return;
                }
                else if (!IsEmpty(export.Export1.Item.PromptSourceCode.
                  SelectChar))
                {
                  var field =
                    GetField(export.Export1.Item.PromptSourceCode, "selectChar");
                    

                  field.Error = true;

                  // ---------------------------------------------------
                  // 05/20/99 W.Campbell - Replaced ZDelete exit states.
                  // ---------------------------------------------------
                  ExitState = "ACO_NE0000_INVALID_SELECT_CODE";
                }
                else
                {
                  ExitState = "ACO_NE0000_NO_SELECTION_MADE";
                }
              }
            }
          }
        }

        export.Export1.CheckIndex();

        break;
      case "DISPLAY":
        if (IsEmpty(export.Next.Number))
        {
          var field = GetField(export.Next, "number");

          field.Error = true;

          ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";

          return;
        }

        if (AsChar(export.ArCommon.SelectChar) == 'S' && AsChar
          (export.ApCommon.SelectChar) == 'S')
        {
          var field1 = GetField(export.ApCommon, "selectChar");

          field1.Error = true;

          var field2 = GetField(export.ArCommon, "selectChar");

          field2.Error = true;

          ExitState = "INVALID_MUTUALLY_EXCLUSIVE_FIELD";

          return;
        }

        export.Case1.Number = export.Next.Number;
        UseSiReadCaseHeaderInformation();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          if (IsExitState("NO_APS_ON_A_CASE"))
          {
            if (AsChar(export.ArCommon.SelectChar) != 'S')
            {
              export.ApCommon.SelectChar = "";
              export.ArCommon.SelectChar = "S";
            }

            ExitState = "ACO_NN0000_ALL_OK";
          }
          else
          {
            return;
          }
        }

        if (AsChar(local.MultipleAps.Flag) == 'Y' && IsEmpty
          (export.ApCsePersonsWorkSet.Number))
        {
          // -----------------------------------------------
          // 05/03/99 W.Campbell - Added code to send
          // selection needed msg to COMP.  BE SURE
          // TO MATCH next_tran_info ON THE DIALOG
          // FLOW.
          // -----------------------------------------------
          export.HiddenNextTranInfo.MiscText1 = "SELECTION NEEDED";
          export.HiddenNext.Number = "";
          ExitState = "ECO_LNK_TO_CASE_COMPOSITION";

          return;
        }

        UseSiReadOfficeOspHeader();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }

        if (!IsEmpty(export.ArFromCaseRole.Number))
        {
          if (Equal(export.Next.Number, export.HiddenNext.Number))
          {
            UseSiReadArInformation();

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              return;
            }
          }
          else
          {
            export.ArFromCaseRole.Number = "";
          }
        }

        export.PromptCaseComp.SelectChar = "";
        export.PromptRoleCase.SelectChar = "";

        if (AsChar(export.ArCommon.SelectChar) == 'S')
        {
          export.Start.Number = export.ArCsePersonsWorkSet.Number;
        }
        else
        {
          export.Start.Number = export.ApCsePersonsWorkSet.Number;
          export.ApCommon.SelectChar = "S";
        }

        // 01/12/01 M.L Start
        if (AsChar(local.FvCheck.Flag) == 'Y')
        {
          UseScSecurityValidAuthForFv2();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            export.HiddenNext.Number = export.Next.Number;
            export.Export1.Count = 0;

            return;
          }
        }

        // 01/12/01 M.L End
        export.Last.Identifier = Now();
        export.Last.EndDate = local.MaxDate.ExpirationDate;
        UseSiListForeignAddresses2();

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          if (AsChar(export.CaseOpen.Flag) == 'N')
          {
            ExitState = "DISPLAY_OK_FOR_CLOSED_CASE";
          }
          else if (AsChar(export.ApActive.Flag) == 'N' && AsChar
            (export.ApCommon.SelectChar) == 'S')
          {
            ExitState = "DISPLAY_OK_FOR_INACTIVE_AP";
          }
          else
          {
            ExitState = "ACO_NI0000_SUCCESSFUL_DISPLAY";

            if (export.Export1.IsEmpty)
            {
              ExitState = "ACO_NI0000_NO_DATA_TO_DISPLAY";
            }
          }
        }

        export.ScrollMinus.Text1 = "";
        export.HiddenStandard.PageNumber = 1;

        if (export.Export1.Count >= 3)
        {
          export.ScrollPlus.Text1 = "+";

          export.Export1.Index = 0;
          export.Export1.CheckSize();

          export.HiddenPageKeys.Index = 0;
          export.HiddenPageKeys.CheckSize();

          MoveCsePersonAddress5(export.Last,
            export.HiddenPageKeys.Update.HiddenPageKey);

          ++export.HiddenPageKeys.Index;
          export.HiddenPageKeys.CheckSize();

          export.Export1.Index = Export.ExportGroup.Capacity - 1;
          export.Export1.CheckSize();

          MoveCsePersonAddress5(export.Export1.Item.DetailCsePersonAddress,
            export.HiddenPageKeys.Update.HiddenPageKey);
        }
        else
        {
          export.ScrollPlus.Text1 = "";
        }

        for(export.Export1.Index = 0; export.Export1.Index < export
          .Export1.Count; ++export.Export1.Index)
        {
          if (!export.Export1.CheckSize())
          {
            break;
          }

          if (Equal(export.Export1.Item.DetailCsePersonAddress.EndDate,
            local.MaxDate.ExpirationDate))
          {
            export.Export1.Update.DetailCsePersonAddress.EndDate =
              local.Zero.Date;
          }

          export.Hidden.Index = export.Export1.Index;
          export.Hidden.CheckSize();

          export.Hidden.Update.HiddenUpd.SendDate =
            export.Export1.Item.DetailCsePersonAddress.SendDate;

          if (Equal(export.Export1.Item.DetailCsePersonAddress.Identifier,
            local.Asterisk.Identifier))
          {
            export.Export1.Update.DetailCommon.SelectChar = "*";
          }
          else
          {
            // -------------------
            // Continue processing
            // -------------------
          }
        }

        export.Export1.CheckIndex();
        export.HiddenNext.Number = export.Next.Number;

        break;
      case "CREATE":
        // ---------------------------------------------
        //      C R E A T E   P R O C E S S I N G
        // --------------------------------------------
        if (local.Common.Count == 0)
        {
          ExitState = "ACO_NE0000_NO_SELECTION_MADE";

          return;
        }

        if (IsEmpty(export.Next.Number))
        {
          var field = GetField(export.Next, "number");

          field.Error = true;

          ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";
        }

        // ---------------------------------------------
        // 01/27/99 W.Campbell - The following logic was
        // modified to only provide a required data missing
        // error if there is no ap and it has been selected.
        // --------------------------------------------
        if (AsChar(export.ApCommon.SelectChar) == 'S')
        {
          if (IsEmpty(export.ApCsePersonsWorkSet.Number))
          {
            var field = GetField(export.ApCsePersonsWorkSet, "number");

            field.Error = true;

            ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";
          }
        }

        if (IsEmpty(export.ArCsePersonsWorkSet.Number))
        {
          var field = GetField(export.ArCsePersonsWorkSet, "number");

          field.Error = true;

          ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";
        }

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }

        if (IsEmpty(export.ApCommon.SelectChar) && IsEmpty
          (export.ArCommon.SelectChar))
        {
          var field1 = GetField(export.ApCommon, "selectChar");

          field1.Error = true;

          var field2 = GetField(export.ArCommon, "selectChar");

          field2.Error = true;

          ExitState = "ACO_NE0000_NO_SELECTION_MADE";

          return;
        }
        else if (!IsEmpty(export.ApCommon.SelectChar) && !
          IsEmpty(export.ArCommon.SelectChar))
        {
          var field1 = GetField(export.ApCommon, "selectChar");

          field1.Error = true;

          var field2 = GetField(export.ArCommon, "selectChar");

          field2.Error = true;

          // ---------------------------------------------
          // 05/20/99 W.Campbell - Replaced
          // ZDelete exit states.
          // ---------------------------------------------
          ExitState = "ACO_NE0000_INVALID_MULTIPLE_SEL";

          return;
        }

        for(export.Export1.Index = 0; export.Export1.Index < export
          .Export1.Count; ++export.Export1.Index)
        {
          if (!export.Export1.CheckSize())
          {
            break;
          }

          if (AsChar(export.Export1.Item.DetailCommon.SelectChar) == 'S')
          {
            break;
          }
        }

        export.Export1.CheckIndex();

        if (AsChar(export.ArCommon.SelectChar) == 'S')
        {
          if (CharAt(export.ArCsePersonsWorkSet.Number, 10) == 'O')
          {
            ExitState = "FN0000_PERS_IS_ORGZ";

            return;
          }
        }

        if (IsEmpty(export.Export1.Item.DetailCsePersonAddress.Country))
        {
          var field =
            GetField(export.Export1.Item.DetailCsePersonAddress, "country");

          field.Error = true;

          ExitState = "ADDRESS_INCOMPLETE";
        }

        if (IsEmpty(export.Export1.Item.DetailCsePersonAddress.City))
        {
          var field =
            GetField(export.Export1.Item.DetailCsePersonAddress, "city");

          field.Error = true;

          ExitState = "ADDRESS_INCOMPLETE";
        }

        if (IsEmpty(export.Export1.Item.DetailCsePersonAddress.Street1))
        {
          var field =
            GetField(export.Export1.Item.DetailCsePersonAddress, "street1");

          field.Error = true;

          ExitState = "ADDRESS_INCOMPLETE";
        }

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }

        export.Export1.Update.DetailCsePersonAddress.Type1 = "M";

        if (!IsEmpty(export.Export1.Item.DetailCsePersonAddress.Country))
        {
          local.Verify.Cdvalue =
            export.Export1.Item.DetailCsePersonAddress.Country ?? Spaces(10);
          UseCabValidateCodeValue1();

          if (AsChar(local.ReturnCode.Flag) == 'N')
          {
            var field =
              GetField(export.Export1.Item.DetailCsePersonAddress, "country");

            field.Error = true;

            ExitState = "ACO_NE0000_INVALID_CODE_PRES_PF4";
          }
        }

        if (!IsEmpty(export.Export1.Item.DetailCsePersonAddress.Source))
        {
          local.Verify.Cdvalue =
            export.Export1.Item.DetailCsePersonAddress.Source ?? Spaces(10);
          UseCabValidateCodeValue2();

          if (AsChar(local.ReturnCode.Flag) == 'N')
          {
            var field =
              GetField(export.Export1.Item.DetailCsePersonAddress, "source");

            field.Error = true;

            ExitState = "ACO_NE0000_INVALID_CODE_PRES_PF4";
          }
        }

        if (!IsEmpty(export.Export1.Item.DetailCsePersonAddress.EndCode))
        {
          if (Equal(export.Export1.Item.DetailCsePersonAddress.EndDate,
            local.Zero.Date))
          {
            var field1 =
              GetField(export.Export1.Item.DetailCsePersonAddress, "endCode");

            field1.Error = true;

            var field2 =
              GetField(export.Export1.Item.DetailCsePersonAddress, "endDate");

            field2.Error = true;

            ExitState = "MUST_ENTER_END_DATE_WITH_CODE";

            return;
          }

          // 06/06/01 M.L Start
          if (Lt(local.TodayPlus6Months.Date,
            export.Export1.Item.DetailCsePersonAddress.EndDate) && Lt
            (export.Export1.Item.DetailCsePersonAddress.EndDate,
            local.MaxDate.ExpirationDate))
          {
            var field =
              GetField(export.Export1.Item.DetailCsePersonAddress, "endDate");

            field.Error = true;

            ExitState = "SI_DTE_GREATER_THAN_TODAY_6MONTH";

            return;
          }

          // 06/06/01 M.L End
          local.Verify.Cdvalue =
            export.Export1.Item.DetailCsePersonAddress.EndCode ?? Spaces(10);
          UseCabValidateCodeValue3();

          if (AsChar(local.ReturnCode.Flag) == 'N')
          {
            var field =
              GetField(export.Export1.Item.DetailCsePersonAddress, "endCode");

            field.Error = true;

            ExitState = "ACO_NE0000_INVALID_CODE_PRES_PF4";
          }
        }
        else
        {
          // ---------------------------------------------
          // 01/04/99 W.Campbell - Logic added to make
          // sure END CODE is entered with END DATE.
          // Work done on IDCR454.
          // ---------------------------------------------
          if (!Equal(export.Export1.Item.DetailCsePersonAddress.EndDate,
            local.Zero.Date))
          {
            var field1 =
              GetField(export.Export1.Item.DetailCsePersonAddress, "endCode");

            field1.Error = true;

            var field2 =
              GetField(export.Export1.Item.DetailCsePersonAddress, "endDate");

            field2.Error = true;

            ExitState = "MUST_ENTER_END_DATE_WITH_CODE";

            return;
          }
        }

        // ---------------------------------------------
        // 12/22/98 W.Campbell - The following duplicate
        // logic to validate SEND DATE was disabled
        // Work done on IDCR454.
        // ---------------------------------------------
        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }

        if (Equal(export.Export1.Item.DetailCsePersonAddress.SendDate,
          local.Zero.Date))
        {
        }
        else if (Lt(export.Export1.Item.DetailCsePersonAddress.SendDate,
          local.Current.Date))
        {
          var field =
            GetField(export.Export1.Item.DetailCsePersonAddress, "sendDate");

          field.Error = true;

          ExitState = "ACO_NE0000_DATE_LESS_CURRENT_DT";

          return;
        }
        else
        {
          local.SendDateUpdated.Flag = "Y";
        }

        // 06/06/01 M.L Start
        if (Lt(local.TodayPlus6Months.Date,
          export.Export1.Item.DetailCsePersonAddress.VerifiedDate))
        {
          var field =
            GetField(export.Export1.Item.DetailCsePersonAddress, "verifiedDate");
            

          field.Error = true;

          ExitState = "SI_DTE_GREATER_THAN_TODAY_6MONTH";

          return;
        }

        // 06/06/01 M.L End
        // ---------------------------------------------
        // 12/22/98 W.Campbell - Logic added to
        // validate that the user has entered one
        // or more of SEND DATE, VERIFIED DATE
        // or END DATE on an ADD or UPDATE.
        // Work done on IDCR454.
        // ---------------------------------------------
        if (Equal(export.Export1.Item.DetailCsePersonAddress.SendDate,
          local.Zero.Date) && Equal
          (export.Export1.Item.DetailCsePersonAddress.VerifiedDate,
          local.Zero.Date) && Equal
          (export.Export1.Item.DetailCsePersonAddress.EndDate, local.Zero.Date))
        {
          var field1 =
            GetField(export.Export1.Item.DetailCsePersonAddress, "sendDate");

          field1.Error = true;

          var field2 =
            GetField(export.Export1.Item.DetailCsePersonAddress, "verifiedDate");
            

          field2.Error = true;

          var field3 =
            GetField(export.Export1.Item.DetailCsePersonAddress, "endDate");

          field3.Error = true;

          ExitState = "MUST_ENTER_SEND_VERIFY_OR_END_DT";

          return;
        }

        if (AsChar(export.ArCommon.SelectChar) == 'S')
        {
          export.Start.Number = export.ArCsePersonsWorkSet.Number;
        }
        else
        {
          export.Start.Number = export.ApCsePersonsWorkSet.Number;
        }

        UseSiCreatePersonForeignAddress();

        if (IsExitState("ACO_NI0000_SUCCESSFUL_ADD"))
        {
          ExitState = "ACO_NN0000_ALL_OK";
          export.Export1.Update.DetailCommon.SelectChar = "*";
        }

        if (Equal(export.Export1.Item.DetailCsePersonAddress.EndDate,
          local.MaxDate.ExpirationDate))
        {
          export.Export1.Update.DetailCsePersonAddress.EndDate =
            local.Zero.Date;
        }

        if (AsChar(local.SendDateUpdated.Flag) == 'Y')
        {
          // mjr
          // ------------------------------------------------
          // 02/06/1999
          // Added creation of document trigger
          // -------------------------------------------------------------
          // -------------------------------------
          // Changed document from batch to online
          // -------------------------------------
          local.Document.Name = "POSTMAST";
          local.SpDocKey.KeyCase = export.Case1.Number;

          if (!IsEmpty(export.ApCommon.SelectChar))
          {
            local.SpDocKey.KeyPerson = export.ApCsePersonsWorkSet.Number;
          }
          else
          {
            local.SpDocKey.KeyPerson = export.ArCsePersonsWorkSet.Number;
          }

          local.SpDocKey.KeyPersonAddress =
            export.Export1.Item.DetailCsePersonAddress.Identifier;
        }

        ExitState = "ACO_NI0000_SUCCESSFUL_ADD";

        break;
      case "UPDATE":
        // ---------------------------------------------
        //       U P D A T E   P R O C E S S I N G
        // --------------------------------------------
        if (local.Common.Count == 0)
        {
          ExitState = "ACO_NE0000_NO_SELECTION_MADE";

          return;
        }

        if (IsEmpty(export.Next.Number))
        {
          var field = GetField(export.Next, "number");

          field.Error = true;

          ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";

          return;
        }

        if (!Equal(export.Next.Number, export.Case1.Number))
        {
          var field = GetField(export.Next, "number");

          field.Error = true;

          ExitState = "ACO_NE0000_DISPLAY_BEFORE_UPDATE";

          return;
        }

        if (IsEmpty(export.ApCommon.SelectChar) && IsEmpty
          (export.ArCommon.SelectChar))
        {
          var field1 = GetField(export.ApCommon, "selectChar");

          field1.Error = true;

          var field2 = GetField(export.ArCommon, "selectChar");

          field2.Error = true;

          ExitState = "ACO_NE0000_NO_SELECTION_MADE";

          return;
        }
        else if (!IsEmpty(export.ApCommon.SelectChar) && !
          IsEmpty(export.ArCommon.SelectChar))
        {
          var field1 = GetField(export.ApCommon, "selectChar");

          field1.Error = true;

          var field2 = GetField(export.ArCommon, "selectChar");

          field2.Error = true;

          // ---------------------------------------------
          // 05/20/99 W.Campbell - Replaced
          // ZDelete exit states.
          // ---------------------------------------------
          ExitState = "ACO_NE0000_INVALID_MULTIPLE_SEL";

          return;
        }

        export.Export1.Index = 0;

        for(var limit = export.Export1.Count; export.Export1.Index < limit; ++
          export.Export1.Index)
        {
          if (!export.Export1.CheckSize())
          {
            break;
          }

          if (AsChar(export.Export1.Item.DetailCommon.SelectChar) == 'S')
          {
            if (IsEmpty(export.Export1.Item.DetailCsePersonAddress.Country))
            {
              var field =
                GetField(export.Export1.Item.DetailCsePersonAddress, "country");
                

              field.Error = true;

              ExitState = "ADDRESS_INCOMPLETE";
            }

            if (IsEmpty(export.Export1.Item.DetailCsePersonAddress.City))
            {
              var field =
                GetField(export.Export1.Item.DetailCsePersonAddress, "city");

              field.Error = true;

              ExitState = "ADDRESS_INCOMPLETE";
            }

            if (IsEmpty(export.Export1.Item.DetailCsePersonAddress.Street1))
            {
              var field =
                GetField(export.Export1.Item.DetailCsePersonAddress, "street1");
                

              field.Error = true;

              ExitState = "ADDRESS_INCOMPLETE";
            }

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              return;
            }

            export.Export1.Update.DetailCsePersonAddress.Type1 = "M";

            if (!IsEmpty(export.Export1.Item.DetailCsePersonAddress.Country))
            {
              local.Verify.Cdvalue =
                export.Export1.Item.DetailCsePersonAddress.Country ?? Spaces
                (10);
              UseCabValidateCodeValue1();

              if (AsChar(local.ReturnCode.Flag) == 'N')
              {
                var field =
                  GetField(export.Export1.Item.DetailCsePersonAddress, "country");
                  

                field.Error = true;

                ExitState = "ACO_NE0000_INVALID_CODE_PRES_PF4";
              }
            }

            if (!IsEmpty(export.Export1.Item.DetailCsePersonAddress.Source))
            {
              local.Verify.Cdvalue =
                export.Export1.Item.DetailCsePersonAddress.Source ?? Spaces
                (10);
              UseCabValidateCodeValue2();

              if (AsChar(local.ReturnCode.Flag) == 'N')
              {
                var field =
                  GetField(export.Export1.Item.DetailCsePersonAddress, "source");
                  

                field.Error = true;

                ExitState = "ACO_NE0000_INVALID_CODE_PRES_PF4";
              }
            }

            if (!IsEmpty(export.Export1.Item.DetailCsePersonAddress.EndCode))
            {
              if (Equal(export.Export1.Item.DetailCsePersonAddress.EndDate,
                local.Zero.Date))
              {
                var field1 =
                  GetField(export.Export1.Item.DetailCsePersonAddress, "endCode");
                  

                field1.Error = true;

                var field2 =
                  GetField(export.Export1.Item.DetailCsePersonAddress, "endDate");
                  

                field2.Error = true;

                ExitState = "MUST_ENTER_END_DATE_WITH_CODE";

                return;
              }

              local.Verify.Cdvalue =
                export.Export1.Item.DetailCsePersonAddress.EndCode ?? Spaces
                (10);
              UseCabValidateCodeValue3();

              if (AsChar(local.ReturnCode.Flag) == 'N')
              {
                var field =
                  GetField(export.Export1.Item.DetailCsePersonAddress, "endCode");
                  

                field.Error = true;

                ExitState = "ACO_NE0000_INVALID_CODE_PRES_PF4";
              }
            }
            else
            {
              // ---------------------------------------------
              // 01/04/99 W.Campbell - Logic added to make
              // sure END CODE is entered with END DATE.
              // Work done on IDCR454.
              // ---------------------------------------------
              if (!Equal(export.Export1.Item.DetailCsePersonAddress.EndDate,
                local.Zero.Date))
              {
                var field1 =
                  GetField(export.Export1.Item.DetailCsePersonAddress, "endCode");
                  

                field1.Error = true;

                var field2 =
                  GetField(export.Export1.Item.DetailCsePersonAddress, "endDate");
                  

                field2.Error = true;

                ExitState = "MUST_ENTER_END_DATE_WITH_CODE";

                return;
              }
            }

            // 06/06/01 M.L Start
            if (Lt(local.TodayPlus6Months.Date,
              export.Export1.Item.DetailCsePersonAddress.EndDate) && !
              Equal(export.Export1.Item.DetailCsePersonAddress.EndDate,
              local.MaxDate.ExpirationDate))
            {
              var field =
                GetField(export.Export1.Item.DetailCsePersonAddress, "endDate");
                

              field.Error = true;

              ExitState = "SI_DTE_GREATER_THAN_TODAY_6MONTH";

              return;
            }

            // 06/06/01 M.L End
            // 06/06/01 M.L Start
            if (Lt(local.TodayPlus6Months.Date,
              export.Export1.Item.DetailCsePersonAddress.VerifiedDate))
            {
              var field =
                GetField(export.Export1.Item.DetailCsePersonAddress,
                "verifiedDate");

              field.Error = true;

              ExitState = "SI_DTE_GREATER_THAN_TODAY_6MONTH";

              return;
            }

            // 06/06/01 M.L End
            export.Hidden.Index = export.Export1.Index;
            export.Hidden.CheckSize();

            // ---------------------------------------------
            // 12/22/98 W.Campbell - Disabled duplicate logic
            // dealing with send date validation and change
            // on an UPDATE.  Work done on IDCR454.
            // ---------------------------------------------
            if (Equal(export.Export1.Item.DetailCsePersonAddress.SendDate,
              local.Zero.Date) || Equal
              (export.Export1.Item.DetailCsePersonAddress.SendDate,
              export.Hidden.Item.HiddenUpd.SendDate))
            {
            }
            else if (Lt(export.Export1.Item.DetailCsePersonAddress.SendDate,
              Now().Date))
            {
              var field =
                GetField(export.Export1.Item.DetailCsePersonAddress, "sendDate");
                

              field.Error = true;

              ExitState = "ACO_NE0000_DATE_LESS_CURRENT_DT";
            }
            else
            {
              local.SendDateUpdated.Flag = "Y";
            }

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              return;
            }

            // ---------------------------------------------
            // 12/22/98 W.Campbell - Logic added to
            // validate that the user has entered one
            // or more of SEND DATE, VERIFIED DATE
            // or END DATE on an ADD or UPDATE.
            // Work done on IDCR454.
            // ---------------------------------------------
            if (Equal(export.Export1.Item.DetailCsePersonAddress.SendDate,
              local.Zero.Date) && Equal
              (export.Export1.Item.DetailCsePersonAddress.VerifiedDate,
              local.Zero.Date) && Equal
              (export.Export1.Item.DetailCsePersonAddress.EndDate,
              local.Zero.Date))
            {
              var field1 =
                GetField(export.Export1.Item.DetailCsePersonAddress, "sendDate");
                

              field1.Error = true;

              var field2 =
                GetField(export.Export1.Item.DetailCsePersonAddress,
                "verifiedDate");

              field2.Error = true;

              var field3 =
                GetField(export.Export1.Item.DetailCsePersonAddress, "endDate");
                

              field3.Error = true;

              ExitState = "MUST_ENTER_SEND_VERIFY_OR_END_DT";

              return;
            }

            if (AsChar(export.ArCommon.SelectChar) == 'S')
            {
              export.Start.Number = export.ArCsePersonsWorkSet.Number;
            }
            else
            {
              export.Start.Number = export.ApCsePersonsWorkSet.Number;
            }

            local.CsePerson.Number = export.Start.Number;
            export.HiddenCsePerson.Number = local.CsePerson.Number;
            UseSiUpdatePersonForeignAddress();

            if (IsExitState("ACO_NI0000_SUCCESSFUL_UPDATE"))
            {
              export.Export1.Update.DetailCommon.SelectChar = "*";
              ExitState = "ACO_NN0000_ALL_OK";
            }

            if (AsChar(local.SendDateUpdated.Flag) == 'Y')
            {
              // mjr
              // ------------------------------------------------
              // 02/06/1999
              // Added creation of document trigger
              // -------------------------------------------------------------
              // -------------------------------------
              // Changed document from batch to online
              // -------------------------------------
              local.Document.Name = "POSTMAST";
              local.SpDocKey.KeyCase = export.Case1.Number;

              if (!IsEmpty(export.ApCommon.SelectChar))
              {
                local.SpDocKey.KeyPerson = export.ApCsePersonsWorkSet.Number;
              }
              else
              {
                local.SpDocKey.KeyPerson = export.ArCsePersonsWorkSet.Number;
              }

              local.SpDocKey.KeyPersonAddress =
                export.Export1.Item.DetailCsePersonAddress.Identifier;
            }

            ExitState = "ACO_NI0000_SUCCESSFUL_UPDATE";

            // 09/29/00 M.L Comment out the changes
            // 09/29/00 M.L End
          }
        }

        export.Export1.CheckIndex();

        break;
      default:
        // ---------------------------------------------
        // 05/20/99 W.Campbell - Replaced
        // ZDelete exit states.
        // ---------------------------------------------
        ExitState = "ACO_NE0000_INVALID_PF_KEY";

        break;
    }

    // ---------------------------------------------
    // Start of code to raise Events.
    // ---------------------------------------------
    if (IsExitState("ACO_NI0000_SUCCESSFUL_ADD") || IsExitState
      ("ACO_NI0000_SUCCESSFUL_UPDATE"))
    {
      // --------------------------------------------
      // The below code may be redundant, however it is to ensure
      // that either ap/ar values are    selected and not both.
      // --------------------------------------------
      if (AsChar(export.ApCommon.SelectChar) == 'S' && AsChar
        (export.ArCommon.SelectChar) == 'S')
      {
        var field1 = GetField(export.ApCommon, "selectChar");

        field1.Error = true;

        var field2 = GetField(export.ArCommon, "selectChar");

        field2.Error = true;

        ExitState = "INVALID_MUTUALLY_EXCLUSIVE_FIELD";

        // --------------------------------------------
        // 05/20/99 W.Campbell - Added the use of
        // rollback eab here to prevent problems
        // with DB modifications which should not
        // have been committed.
        // --------------------------------------------
        UseEabRollbackCics();

        return;
      }

      local.Infrastructure.UserId = "FADS";
      local.Infrastructure.EventId = 10;
      local.Infrastructure.BusinessObjectCd = "CPA";
      local.Infrastructure.SituationNumber = 0;
      local.DetailText1.Text1 = ",";
      export.Export1.Index = 0;

      for(var limit = export.Export1.Count; export.Export1.Index < limit; ++
        export.Export1.Index)
      {
        if (!export.Export1.CheckSize())
        {
          break;
        }

        if (AsChar(export.ApCommon.SelectChar) == 'S')
        {
          local.AparSelection.Text1 = "P";
        }

        if (AsChar(export.ArCommon.SelectChar) == 'S')
        {
          local.AparSelection.Text1 = "R";
        }

        if (AsChar(export.Export1.Item.DetailCommon.SelectChar) == '*')
        {
          local.Infrastructure.DenormTimestamp =
            export.Export1.Item.DetailCsePersonAddress.Identifier;

          for(local.NumberOfEvents.TotalInteger = 1; local
            .NumberOfEvents.TotalInteger <= 3; ++
            local.NumberOfEvents.TotalInteger)
          {
            // --------------------------------------------
            // Close monitored flag code added 1/23/97:11:55
            // --------------------------------------------
            // ---------------------------------------------
            // 02/18/99 M Ramirez & W.Campbell
            // Disabled statements
            // dealing with closing monitored documents,
            // as it has been determined that the best way
            // to handle them will be in Batch.
            // ---------------------------------------------
            local.RaiseEventFlag.Text1 = "N";
            local.Infrastructure.Detail =
              Spaces(Infrastructure.Detail_MaxLength);

            if (local.NumberOfEvents.TotalInteger == 1)
            {
              if (!Equal(export.Export1.Item.DetailCsePersonAddress.EndDate,
                export.Export1.Item.Hdet.EndDate) && Lt
                (local.Zero.Date,
                export.Export1.Item.DetailCsePersonAddress.EndDate))
              {
                local.RaiseEventFlag.Text1 = "Y";
                local.Infrastructure.ReferenceDate =
                  export.Export1.Item.DetailCsePersonAddress.EndDate;

                switch(AsChar(local.AparSelection.Text1))
                {
                  case 'P':
                    local.CsePerson.Number = export.ApCsePersonsWorkSet.Number;
                    export.HiddenCsePerson.Number = local.CsePerson.Number;
                    local.Infrastructure.ReasonCode = "FADDREXPDAP";
                    local.DetailText30.Text30 = "AP Foreign Address ended :";

                    break;
                  case 'R':
                    local.CsePerson.Number = export.ArCsePersonsWorkSet.Number;
                    export.HiddenCsePerson.Number = local.CsePerson.Number;
                    local.Infrastructure.ReasonCode = "FADDREXPDAR";
                    local.DetailText30.Text30 = "AR Foreign Address ended :";

                    break;
                  default:
                    break;
                }

                local.Infrastructure.Detail =
                  TrimEnd(local.DetailText30.Text30) + (
                    export.Export1.Item.DetailCsePersonAddress.Street1 ?? "");
                local.Infrastructure.Detail =
                  TrimEnd(local.Infrastructure.Detail) + local
                  .DetailText1.Text1;
                local.Infrastructure.Detail =
                  TrimEnd(local.Infrastructure.Detail) + (
                    export.Export1.Item.DetailCsePersonAddress.City ?? "");
                local.Infrastructure.Detail =
                  TrimEnd(local.Infrastructure.Detail) + local
                  .DetailText1.Text1;
                local.Infrastructure.Detail =
                  TrimEnd(local.Infrastructure.Detail) + (
                    export.Export1.Item.DetailCsePersonAddress.Country ?? "");
              }
            }
            else if (local.NumberOfEvents.TotalInteger == 2)
            {
              if (!Equal(export.Export1.Item.DetailCsePersonAddress.
                VerifiedDate, export.Export1.Item.Hdet.VerifiedDate))
              {
                // --------------------------------------------
                // Close monitored flag code added 1/23/97:11:55
                // --------------------------------------------
                // ---------------------------------------------
                // 02/18/99 M Ramirez & W.Campbell
                // Disabled statements
                // dealing with closing monitored documents,
                // as it has been determined that the best way
                // to handle them will be in Batch.
                // ---------------------------------------------
                local.RaiseEventFlag.Text1 = "Y";
                local.Infrastructure.ReferenceDate =
                  export.Export1.Item.DetailCsePersonAddress.VerifiedDate;

                switch(AsChar(local.AparSelection.Text1))
                {
                  case 'P':
                    local.CsePerson.Number = export.ApCsePersonsWorkSet.Number;
                    export.HiddenCsePerson.Number = local.CsePerson.Number;
                    local.Infrastructure.ReasonCode = "FADDRVRFDAP";
                    local.DetailText30.Text30 = "AP Foreign Address Verified :";

                    // ---------------------------------------------
                    // 05/20/99 W.Campbell - Added code to
                    // process interstate event for AP
                    // Foreign address located (LSOUT)
                    // with no end date.
                    // ---------------------------------------------
                    if (Equal(export.Export1.Item.DetailCsePersonAddress.
                      EndDate, local.Zero.Date))
                    {
                      // ---------------------------------------------
                      // 05/20/99 W.Campbell - Save the state
                      // of the current exit state.
                      // ---------------------------------------------
                      if (IsExitState("ACO_NI0000_SUCCESSFUL_ADD"))
                      {
                        local.Save.State = "AD";
                      }
                      else if (IsExitState("ACO_NI0000_SUCCESSFUL_UPDATE"))
                      {
                        local.Save.State = "UP";
                      }
                      else
                      {
                        UseEabRollbackCics();

                        return;
                      }

                      local.ScreenIdentification.Command = "FADS LSOUT";
                      local.Csenet.Number = export.ApCsePersonsWorkSet.Number;

                      // 09/29/00 M.L Comment out the changes
                      // 09/29/00 M.L End
                      UseSiCreateAutoCsenetTrans1();

                      if (IsExitState("ACO_NN0000_ALL_OK"))
                      {
                        // ---------------------------------------------
                        // 05/20/99 W.Campbell - Restore the
                        // exit state from the saved state.
                        // ---------------------------------------------
                        switch(TrimEnd(local.Save.State))
                        {
                          case "AD":
                            ExitState = "ACO_NI0000_SUCCESSFUL_ADD";

                            break;
                          case "UP":
                            ExitState = "ACO_NI0000_SUCCESSFUL_UPDATE";

                            break;
                          default:
                            ExitState = "SYSTEM_ERROR_HAS_OCCURRED_RB";
                            UseEabRollbackCics();

                            return;
                        }
                      }
                      else
                      {
                        UseEabRollbackCics();

                        return;
                      }
                    }

                    // ---------------------------------------------
                    // 05/20/99 W.Campbell - End of code added
                    // to process interstate event for AP
                    // Foreign address located (LSOUT)
                    // with no end date.
                    // ---------------------------------------------
                    break;
                  case 'R':
                    local.CsePerson.Number = export.ArCsePersonsWorkSet.Number;
                    export.HiddenCsePerson.Number = local.CsePerson.Number;
                    local.Infrastructure.ReasonCode = "FADDRVRFDAR";
                    local.DetailText30.Text30 = "AR Foreign Address Verified :";

                    // ---------------------------------------------
                    // 05/20/99 W.Campbell - Added code to
                    // process interstate event for AR
                    // Foreign address located (GSPAD)
                    // with no end date.
                    // ---------------------------------------------
                    if (Equal(export.Export1.Item.DetailCsePersonAddress.
                      EndDate, local.Zero.Date))
                    {
                      // ---------------------------------------------
                      // 05/20/99 W.Campbell - Save the state
                      // of the current exit state.
                      // ---------------------------------------------
                      if (IsExitState("ACO_NI0000_SUCCESSFUL_ADD"))
                      {
                        local.Save.State = "AD";
                      }
                      else if (IsExitState("ACO_NI0000_SUCCESSFUL_UPDATE"))
                      {
                        local.Save.State = "UP";
                      }
                      else
                      {
                        UseEabRollbackCics();

                        return;
                      }

                      local.ScreenIdentification.Command = "FADS GSPAD";
                      local.Csenet.Number = export.ArCsePersonsWorkSet.Number;

                      // 09/29/00 M.L Comment out the changes
                      // 09/29/00 M.L End
                      UseSiCreateAutoCsenetTrans2();

                      if (IsExitState("ACO_NN0000_ALL_OK"))
                      {
                        // ---------------------------------------------
                        // 05/20/99 W.Campbell - Restore the
                        // exit state from the saved state.
                        // ---------------------------------------------
                        switch(TrimEnd(local.Save.State))
                        {
                          case "AD":
                            ExitState = "ACO_NI0000_SUCCESSFUL_ADD";

                            break;
                          case "UP":
                            ExitState = "ACO_NI0000_SUCCESSFUL_UPDATE";

                            break;
                          default:
                            ExitState = "SYSTEM_ERROR_HAS_OCCURRED_RB";
                            UseEabRollbackCics();

                            return;
                        }
                      }
                      else
                      {
                        UseEabRollbackCics();

                        return;
                      }
                    }

                    // ---------------------------------------------
                    // 05/20/99 W.Campbell - End of code added
                    // to process interstate event for AR
                    // Foreign address located (GSPAD)
                    // with no end date.
                    // ---------------------------------------------
                    break;
                  default:
                    break;
                }

                local.Infrastructure.Detail =
                  TrimEnd(local.DetailText30.Text30) + (
                    export.Export1.Item.DetailCsePersonAddress.Street1 ?? "");
                local.Infrastructure.Detail =
                  TrimEnd(local.Infrastructure.Detail) + local
                  .DetailText1.Text1;
                local.Infrastructure.Detail =
                  TrimEnd(local.Infrastructure.Detail) + (
                    export.Export1.Item.DetailCsePersonAddress.City ?? "");
                local.Infrastructure.Detail =
                  TrimEnd(local.Infrastructure.Detail) + local
                  .DetailText1.Text1;
                local.Infrastructure.Detail =
                  TrimEnd(local.Infrastructure.Detail) + (
                    export.Export1.Item.DetailCsePersonAddress.Country ?? "");

                // ***************************************************
                // Send external alert for verified foreign address.
                // ***************************************************
                if (!Equal(export.Export1.Item.DetailCsePersonAddress.Source,
                  "AE"))
                {
                  local.InterfaceAlert.AlertCode = "45";
                  UseSpAddrExternalAlert();
                }
              }

              // 09/29/00 M.L Comment out the changes
              // 09/29/00 M.L End
            }
            else
            {
              local.RaiseEventFlag.Text1 = "N";
            }

            if (AsChar(local.RaiseEventFlag.Text1) == 'Y')
            {
              // --------------------------------------------
              // This is to aid the event processor to
              //    gather events from a single situation
              // This is an extremely important piece of code
              // --------------------------------------------
              UseSiAddrRaiseEvent();

              if (IsExitState("ACO_NI0000_SUCCESSFUL_ADD") || IsExitState
                ("ACO_NI0000_SUCCESSFUL_UPDATE"))
              {
              }
              else
              {
                UseEabRollbackCics();

                return;
              }

              // --------------------------------------------
              // Locate is person specific event. So the event
              // has to be raised for all Case Units that the
              // Located Person participates as an AP and an AR.
              // --------------------------------------------
              if (local.NumberOfEvents.TotalInteger == 2)
              {
                switch(AsChar(local.AparSelection.Text1))
                {
                  case 'P':
                    local.Infrastructure.ReasonCode = "FADDRVRFDAR";
                    local.DetailText30.Text30 = "AR Foreign Address Verified :";
                    local.AparSelection.Text1 = "R";

                    break;
                  case 'R':
                    local.Infrastructure.ReasonCode = "FADDRVRFDAP";
                    local.DetailText30.Text30 = "AP Foreign Address Verified :";
                    local.AparSelection.Text1 = "P";

                    break;
                  default:
                    break;
                }

                UseSiAddrRaiseEvent();

                if (IsExitState("ACO_NI0000_SUCCESSFUL_ADD") || IsExitState
                  ("ACO_NI0000_SUCCESSFUL_UPDATE"))
                {
                }
                else
                {
                  UseEabRollbackCics();

                  return;
                }
              }
              else if (local.NumberOfEvents.TotalInteger == 1)
              {
                switch(AsChar(local.AparSelection.Text1))
                {
                  case 'P':
                    local.Infrastructure.ReasonCode = "FADDREXPDAR";
                    local.DetailText30.Text30 = "AR Foreign Address ended :";
                    local.AparSelection.Text1 = "R";

                    break;
                  case 'R':
                    local.Infrastructure.ReasonCode = "FADDREXPDAP";
                    local.DetailText30.Text30 = "AP Foreign Address ended :";
                    local.AparSelection.Text1 = "P";

                    break;
                  default:
                    break;
                }

                UseSiAddrRaiseEvent();

                if (IsExitState("ACO_NI0000_SUCCESSFUL_ADD") || IsExitState
                  ("ACO_NI0000_SUCCESSFUL_UPDATE"))
                {
                }
                else
                {
                  UseEabRollbackCics();

                  return;
                }
              }
              else
              {
              }

              // Reset the field values for the next loop.
              local.AparSelection.Text1 = "";
              local.Infrastructure.ReasonCode = "";
              local.DetailText30.Text30 = "";
            }
          }

          // --------------------------------------------
          // Code to call cab to close monitored doc
          // --------------------------------------------
          // ---------------------------------------------
          // 02/18/99 M Ramirez & W.Campbell
          // Disabled statements
          // dealing with closing monitored documents,
          // as it has been determined that the best way
          // to handle them will be in Batch.
          // ---------------------------------------------
        }
        else
        {
        }
      }

      export.Export1.CheckIndex();

      for(export.Export1.Index = 0; export.Export1.Index < export
        .Export1.Count; ++export.Export1.Index)
      {
        if (!export.Export1.CheckSize())
        {
          break;
        }

        MoveCsePersonAddress3(export.Export1.Item.DetailCsePersonAddress,
          export.Export1.Update.Hdet);
      }

      export.Export1.CheckIndex();
    }

    // -------------------------------------
    // Changed document from batch to online
    // -------------------------------------
    if (IsExitState("ACO_NI0000_SUCCESSFUL_ADD") || IsExitState
      ("ACO_NI0000_SUCCESSFUL_UPDATE"))
    {
      if (AsChar(local.SendDateUpdated.Flag) == 'Y')
      {
        // ------------------------------
        // Verify that the AR is a client
        // ------------------------------
        if (Equal(local.SpDocKey.KeyPerson, export.ArCsePersonsWorkSet.Number))
        {
          if (CharAt(export.ArCsePersonsWorkSet.Number, 10) == 'O')
          {
            return;
          }
        }

        // -----------------
        // Case must be open
        // -----------------
        if (AsChar(export.CaseOpen.Flag) == 'N')
        {
          return;
        }

        MoveNextTranInfo(local.Null1, export.HiddenNextTranInfo);
        export.Standard.NextTransaction = "DKEY";
        export.HiddenNextTranInfo.MiscText2 =
          TrimEnd(local.SpDocLiteral.IdDocument) + local.Document.Name;

        // --------------------------------
        // Place identifiers into next tran
        // --------------------------------
        export.HiddenNextTranInfo.CaseNumber = local.SpDocKey.KeyCase;
        export.HiddenNextTranInfo.MiscText1 =
          TrimEnd(local.SpDocLiteral.IdPrNumber) + local.SpDocKey.KeyPerson;
        local.BatchTimestampWorkArea.IefTimestamp =
          local.SpDocKey.KeyPersonAddress;
        UseLeCabConvertTimestamp();
        export.HiddenNextTranInfo.MiscText1 =
          TrimEnd(export.HiddenNextTranInfo.MiscText1) + ";" + TrimEnd
          (local.SpDocLiteral.IdPersonAddress) + local
          .BatchTimestampWorkArea.TextTimestamp;

        // ---------------------------------------------------------
        // Puts AP in next tran for Re-Display upon return from Print
        // process
        // ----------------------------------------------------------
        export.HiddenNextTranInfo.CsePersonNumberAp =
          export.ApCsePersonsWorkSet.Number;
        local.PrintProcess.Flag = "Y";
        local.PrintProcess.Command = "PRINT";
        UseScCabNextTranPut2();
      }
    }

    // ---------------------------------------------
    // End of code
    // ---------------------------------------------
  }

  private static void MoveBatchTimestampWorkArea(BatchTimestampWorkArea source,
    BatchTimestampWorkArea target)
  {
    target.IefTimestamp = source.IefTimestamp;
    target.TextTimestamp = source.TextTimestamp;
  }

  private static void MoveCommon(Common source, Common target)
  {
    target.Flag = source.Flag;
    target.Command = source.Command;
  }

  private static void MoveCsePersonAddress1(CsePersonAddress source,
    CsePersonAddress target)
  {
    target.LocationType = source.LocationType;
    target.LastUpdatedTimestamp = source.LastUpdatedTimestamp;
    target.ZdelStartDate = source.ZdelStartDate;
    target.SendDate = source.SendDate;
    target.Source = source.Source;
    target.Identifier = source.Identifier;
    target.Street1 = source.Street1;
    target.Street2 = source.Street2;
    target.City = source.City;
    target.Type1 = source.Type1;
    target.WorkerId = source.WorkerId;
    target.VerifiedDate = source.VerifiedDate;
    target.EndDate = source.EndDate;
    target.EndCode = source.EndCode;
    target.ZdelVerifiedCode = source.ZdelVerifiedCode;
    target.Street3 = source.Street3;
    target.Street4 = source.Street4;
    target.Province = source.Province;
    target.PostalCode = source.PostalCode;
    target.Country = source.Country;
  }

  private static void MoveCsePersonAddress2(CsePersonAddress source,
    CsePersonAddress target)
  {
    target.LocationType = source.LocationType;
    target.LastUpdatedTimestamp = source.LastUpdatedTimestamp;
    target.SendDate = source.SendDate;
    target.Source = source.Source;
    target.Identifier = source.Identifier;
    target.Street1 = source.Street1;
    target.Street2 = source.Street2;
    target.City = source.City;
    target.Type1 = source.Type1;
    target.WorkerId = source.WorkerId;
    target.VerifiedDate = source.VerifiedDate;
    target.EndDate = source.EndDate;
    target.EndCode = source.EndCode;
    target.Street3 = source.Street3;
    target.Street4 = source.Street4;
    target.Province = source.Province;
    target.PostalCode = source.PostalCode;
    target.Country = source.Country;
  }

  private static void MoveCsePersonAddress3(CsePersonAddress source,
    CsePersonAddress target)
  {
    target.LocationType = source.LocationType;
    target.Source = source.Source;
    target.Street1 = source.Street1;
    target.Street2 = source.Street2;
    target.City = source.City;
    target.VerifiedDate = source.VerifiedDate;
    target.EndDate = source.EndDate;
    target.EndCode = source.EndCode;
    target.ZdelVerifiedCode = source.ZdelVerifiedCode;
    target.Street3 = source.Street3;
    target.Street4 = source.Street4;
    target.Province = source.Province;
    target.PostalCode = source.PostalCode;
    target.Country = source.Country;
  }

  private static void MoveCsePersonAddress4(CsePersonAddress source,
    CsePersonAddress target)
  {
    target.LocationType = source.LocationType;
    target.Source = source.Source;
    target.Street1 = source.Street1;
    target.Street2 = source.Street2;
    target.City = source.City;
    target.VerifiedDate = source.VerifiedDate;
    target.ZdelVerifiedCode = source.ZdelVerifiedCode;
    target.State = source.State;
    target.ZipCode = source.ZipCode;
    target.PostalCode = source.PostalCode;
    target.Country = source.Country;
  }

  private static void MoveCsePersonAddress5(CsePersonAddress source,
    CsePersonAddress target)
  {
    target.Identifier = source.Identifier;
    target.EndDate = source.EndDate;
  }

  private static void MoveCsePersonAddress6(CsePersonAddress source,
    CsePersonAddress target)
  {
    target.VerifiedDate = source.VerifiedDate;
    target.EndDate = source.EndDate;
    target.EndCode = source.EndCode;
    target.ZdelVerifiedCode = source.ZdelVerifiedCode;
  }

  private static void MoveCsePersonsWorkSet(CsePersonsWorkSet source,
    CsePersonsWorkSet target)
  {
    target.Number = source.Number;
    target.FormattedName = source.FormattedName;
  }

  private static void MoveGroupToExport1(SiListForeignAddresses.Export.
    GroupGroup source, Export.ExportGroup target)
  {
    MoveCsePersonAddress6(source.Ghet, target.Hdet);
    target.PromptCountry.SelectChar = source.GpromptCountry.SelectChar;
    target.PromptSourceCode.SelectChar = source.GpromptSourceCode.SelectChar;
    target.PromptEndCode.SelectChar = source.GpromptEndCode.SelectChar;
    target.PromptReturnCode.SelectChar = source.GpromptReturnCode.SelectChar;
    target.DetailCommon.SelectChar = source.GdetailCommon.SelectChar;
    MoveCsePersonAddress1(source.GdetailCsePersonAddress,
      target.DetailCsePersonAddress);
  }

  private static void MoveInfrastructure1(Infrastructure source,
    Infrastructure target)
  {
    target.SituationNumber = source.SituationNumber;
    target.ProcessStatus = source.ProcessStatus;
    target.EventId = source.EventId;
    target.BusinessObjectCd = source.BusinessObjectCd;
    target.InitiatingStateCode = source.InitiatingStateCode;
    target.ReferenceDate = source.ReferenceDate;
    target.Detail = source.Detail;
  }

  private static void MoveInfrastructure2(Infrastructure source,
    Infrastructure target)
  {
    target.SituationNumber = source.SituationNumber;
    target.EventId = source.EventId;
    target.ReasonCode = source.ReasonCode;
    target.BusinessObjectCd = source.BusinessObjectCd;
    target.DenormNumeric12 = source.DenormNumeric12;
    target.DenormDate = source.DenormDate;
    target.DenormTimestamp = source.DenormTimestamp;
    target.UserId = source.UserId;
    target.ReferenceDate = source.ReferenceDate;
    target.Detail = source.Detail;
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

  private static void MoveOffice(Office source, Office target)
  {
    target.TypeCode = source.TypeCode;
    target.Name = source.Name;
  }

  private static void MoveSpDocLiteral(SpDocLiteral source, SpDocLiteral target)
  {
    target.IdDocument = source.IdDocument;
    target.IdPersonAddress = source.IdPersonAddress;
    target.IdPrNumber = source.IdPrNumber;
  }

  private static void MoveStandard(Standard source, Standard target)
  {
    target.NextTransaction = source.NextTransaction;
    target.PageNumber = source.PageNumber;
  }

  private void UseCabValidateCodeValue1()
  {
    var useImport = new CabValidateCodeValue.Import();
    var useExport = new CabValidateCodeValue.Export();

    useImport.Code.CodeName = local.Country.CodeName;
    useImport.CodeValue.Cdvalue = local.Verify.Cdvalue;

    Call(CabValidateCodeValue.Execute, useImport, useExport);

    local.ReturnCode.Flag = useExport.ValidCode.Flag;
  }

  private void UseCabValidateCodeValue2()
  {
    var useImport = new CabValidateCodeValue.Import();
    var useExport = new CabValidateCodeValue.Export();

    useImport.CodeValue.Cdvalue = local.Verify.Cdvalue;
    useImport.Code.CodeName = local.AddressSource.CodeName;

    Call(CabValidateCodeValue.Execute, useImport, useExport);

    local.ReturnCode.Flag = useExport.ValidCode.Flag;
  }

  private void UseCabValidateCodeValue3()
  {
    var useImport = new CabValidateCodeValue.Import();
    var useExport = new CabValidateCodeValue.Export();

    useImport.CodeValue.Cdvalue = local.Verify.Cdvalue;
    useImport.Code.CodeName = local.AddressEndCode.CodeName;

    Call(CabValidateCodeValue.Execute, useImport, useExport);

    local.ReturnCode.Flag = useExport.ValidCode.Flag;
  }

  private void UseCabZeroFillNumber()
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

  private void UseLeCabConvertTimestamp()
  {
    var useImport = new LeCabConvertTimestamp.Import();
    var useExport = new LeCabConvertTimestamp.Export();

    MoveBatchTimestampWorkArea(local.BatchTimestampWorkArea,
      useImport.BatchTimestampWorkArea);

    Call(LeCabConvertTimestamp.Execute, useImport, useExport);

    MoveBatchTimestampWorkArea(useExport.BatchTimestampWorkArea,
      local.BatchTimestampWorkArea);
  }

  private void UseOeCabSetMnemonics()
  {
    var useImport = new OeCabSetMnemonics.Import();
    var useExport = new OeCabSetMnemonics.Export();

    Call(OeCabSetMnemonics.Execute, useImport, useExport);

    local.Country.CodeName = useExport.Country.CodeName;
    local.AddressSource.CodeName = useExport.AddressSource.CodeName;
    local.AddressEndCode.CodeName = useExport.AddressEndCode.CodeName;
    local.MaxDate.ExpirationDate = useExport.MaxDate.ExpirationDate;
  }

  private void UseScCabNextTranGet()
  {
    var useImport = new ScCabNextTranGet.Import();
    var useExport = new ScCabNextTranGet.Export();

    Call(ScCabNextTranGet.Execute, useImport, useExport);

    export.HiddenNextTranInfo.Assign(useExport.NextTranInfo);
  }

  private void UseScCabNextTranPut1()
  {
    var useImport = new ScCabNextTranPut.Import();
    var useExport = new ScCabNextTranPut.Export();

    useImport.Standard.NextTransaction = import.Standard.NextTransaction;
    useImport.NextTranInfo.Assign(export.HiddenNextTranInfo);

    Call(ScCabNextTranPut.Execute, useImport, useExport);
  }

  private void UseScCabNextTranPut2()
  {
    var useImport = new ScCabNextTranPut.Import();
    var useExport = new ScCabNextTranPut.Export();

    MoveCommon(local.PrintProcess, useImport.PrintProcess);
    useImport.Standard.NextTransaction = export.Standard.NextTransaction;
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

    Call(ScCabTestSecurity.Execute, useImport, useExport);
  }

  private void UseScSecurityValidAuthForFv1()
  {
    var useImport = new ScSecurityValidAuthForFv.Import();
    var useExport = new ScSecurityValidAuthForFv.Export();

    useImport.CsePersonsWorkSet.Number = local.FvTest.Number;

    Call(ScSecurityValidAuthForFv.Execute, useImport, useExport);
  }

  private void UseScSecurityValidAuthForFv2()
  {
    var useImport = new ScSecurityValidAuthForFv.Import();
    var useExport = new ScSecurityValidAuthForFv.Export();

    useImport.CsePersonsWorkSet.Number = export.Start.Number;

    Call(ScSecurityValidAuthForFv.Execute, useImport, useExport);
  }

  private void UseSiAddrRaiseEvent()
  {
    var useImport = new SiAddrRaiseEvent.Import();
    var useExport = new SiAddrRaiseEvent.Export();

    MoveInfrastructure2(local.Infrastructure, useImport.Infrastructure);
    useImport.AparSelection.Text1 = local.AparSelection.Text1;
    useImport.CsePerson.Number = local.CsePerson.Number;

    Call(SiAddrRaiseEvent.Execute, useImport, useExport);

    MoveInfrastructure1(useExport.Infrastructure, local.Infrastructure);
  }

  private void UseSiCreateAutoCsenetTrans1()
  {
    var useImport = new SiCreateAutoCsenetTrans.Import();
    var useExport = new SiCreateAutoCsenetTrans.Export();

    useImport.ScreenIdentification.Command = local.ScreenIdentification.Command;
    useImport.Case1.Number = export.Case1.Number;

    Call(SiCreateAutoCsenetTrans.Execute, useImport, useExport);
  }

  private void UseSiCreateAutoCsenetTrans2()
  {
    var useImport = new SiCreateAutoCsenetTrans.Import();
    var useExport = new SiCreateAutoCsenetTrans.Export();

    useImport.ScreenIdentification.Command = local.ScreenIdentification.Command;
    useImport.CsePerson.Number = local.Csenet.Number;
    useImport.Case1.Number = export.Case1.Number;

    Call(SiCreateAutoCsenetTrans.Execute, useImport, useExport);
  }

  private void UseSiCreatePersonForeignAddress()
  {
    var useImport = new SiCreatePersonForeignAddress.Import();
    var useExport = new SiCreatePersonForeignAddress.Export();

    useImport.CsePersonsWorkSet.Number = export.Start.Number;
    MoveCsePersonAddress1(export.Export1.Item.DetailCsePersonAddress,
      useImport.CsePersonAddress);

    Call(SiCreatePersonForeignAddress.Execute, useImport, useExport);

    MoveCsePersonAddress1(useExport.CsePersonAddress,
      export.Export1.Update.DetailCsePersonAddress);
  }

  private void UseSiListForeignAddresses1()
  {
    var useImport = new SiListForeignAddresses.Import();
    var useExport = new SiListForeignAddresses.Export();

    useImport.Search.Number = import.Search.Number;
    MoveCsePersonAddress5(export.Last, useImport.LastAddr);

    Call(SiListForeignAddresses.Execute, useImport, useExport);

    MoveCsePersonAddress5(useExport.LastAddr, export.Last);
    useExport.Group.CopyTo(export.Export1, MoveGroupToExport1);
  }

  private void UseSiListForeignAddresses2()
  {
    var useImport = new SiListForeignAddresses.Import();
    var useExport = new SiListForeignAddresses.Export();

    MoveCsePersonAddress5(export.Last, useImport.LastAddr);
    useImport.Search.Number = export.Start.Number;

    Call(SiListForeignAddresses.Execute, useImport, useExport);

    useExport.Group.CopyTo(export.Export1, MoveGroupToExport1);
  }

  private void UseSiReadArInformation()
  {
    var useImport = new SiReadArInformation.Import();
    var useExport = new SiReadArInformation.Export();

    useImport.Ar.Number = export.ArFromCaseRole.Number;
    useImport.Case1.Number = export.Case1.Number;

    Call(SiReadArInformation.Execute, useImport, useExport);

    MoveCsePersonsWorkSet(useExport.Ar, export.ArCsePersonsWorkSet);
  }

  private void UseSiReadCaseHeaderInformation()
  {
    var useImport = new SiReadCaseHeaderInformation.Import();
    var useExport = new SiReadCaseHeaderInformation.Export();

    useImport.Case1.Number = export.Case1.Number;
    useImport.Ap.Number = export.ApCsePersonsWorkSet.Number;

    Call(SiReadCaseHeaderInformation.Execute, useImport, useExport);

    local.MultipleAps.Flag = useExport.MultipleAps.Flag;
    export.ApCsePersonsWorkSet.Assign(useExport.Ap);
    MoveCsePersonsWorkSet(useExport.Ar, export.ArCsePersonsWorkSet);
    export.CaseOpen.Flag = useExport.CaseOpen.Flag;
    export.ApActive.Flag = useExport.ApActive.Flag;
  }

  private void UseSiReadOfficeOspHeader()
  {
    var useImport = new SiReadOfficeOspHeader.Import();
    var useExport = new SiReadOfficeOspHeader.Export();

    useImport.Case1.Number = export.Case1.Number;

    Call(SiReadOfficeOspHeader.Execute, useImport, useExport);

    export.HeaderLine.Text35 = useExport.HeaderLine.Text35;
    MoveOffice(useExport.Office, export.Office);
    export.ServiceProvider.LastName = useExport.ServiceProvider.LastName;
  }

  private void UseSiUpdatePersonForeignAddress()
  {
    var useImport = new SiUpdatePersonForeignAddress.Import();
    var useExport = new SiUpdatePersonForeignAddress.Export();

    useImport.CsePersonsWorkSet.Number = export.Start.Number;
    MoveCsePersonAddress1(export.Export1.Item.DetailCsePersonAddress,
      useImport.CsePersonAddress);

    Call(SiUpdatePersonForeignAddress.Execute, useImport, useExport);

    MoveCsePersonAddress2(useExport.CsePersonAddress,
      export.Export1.Update.DetailCsePersonAddress);
  }

  private void UseSpAddrExternalAlert()
  {
    var useImport = new SpAddrExternalAlert.Import();
    var useExport = new SpAddrExternalAlert.Export();

    useImport.InterfaceAlert.AlertCode = local.InterfaceAlert.AlertCode;
    useImport.CsePerson.Number = local.CsePerson.Number;
    MoveCsePersonAddress4(export.Export1.Item.DetailCsePersonAddress,
      useImport.CsePersonAddress);

    Call(SpAddrExternalAlert.Execute, useImport, useExport);
  }

  private void UseSpDocSetLiterals()
  {
    var useImport = new SpDocSetLiterals.Import();
    var useExport = new SpDocSetLiterals.Export();

    Call(SpDocSetLiterals.Execute, useImport, useExport);

    MoveSpDocLiteral(useExport.SpDocLiteral, local.SpDocLiteral);
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
    /// <summary>A ImportGroup group.</summary>
    [Serializable]
    public class ImportGroup
    {
      /// <summary>
      /// A value of Hdet.
      /// </summary>
      [JsonPropertyName("hdet")]
      public CsePersonAddress Hdet
      {
        get => hdet ??= new();
        set => hdet = value;
      }

      /// <summary>
      /// A value of PromptCountry.
      /// </summary>
      [JsonPropertyName("promptCountry")]
      public Common PromptCountry
      {
        get => promptCountry ??= new();
        set => promptCountry = value;
      }

      /// <summary>
      /// A value of PromptSourceCode.
      /// </summary>
      [JsonPropertyName("promptSourceCode")]
      public Common PromptSourceCode
      {
        get => promptSourceCode ??= new();
        set => promptSourceCode = value;
      }

      /// <summary>
      /// A value of PromptReturnCode.
      /// </summary>
      [JsonPropertyName("promptReturnCode")]
      public Common PromptReturnCode
      {
        get => promptReturnCode ??= new();
        set => promptReturnCode = value;
      }

      /// <summary>
      /// A value of PromptEndCode.
      /// </summary>
      [JsonPropertyName("promptEndCode")]
      public Common PromptEndCode
      {
        get => promptEndCode ??= new();
        set => promptEndCode = value;
      }

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
      /// A value of DetailCsePersonAddress.
      /// </summary>
      [JsonPropertyName("detailCsePersonAddress")]
      public CsePersonAddress DetailCsePersonAddress
      {
        get => detailCsePersonAddress ??= new();
        set => detailCsePersonAddress = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 4;

      private CsePersonAddress hdet;
      private Common promptCountry;
      private Common promptSourceCode;
      private Common promptReturnCode;
      private Common promptEndCode;
      private Common detailCommon;
      private CsePersonAddress detailCsePersonAddress;
    }

    /// <summary>A HiddenPageKeysGroup group.</summary>
    [Serializable]
    public class HiddenPageKeysGroup
    {
      /// <summary>
      /// A value of HiddenPageKey.
      /// </summary>
      [JsonPropertyName("hiddenPageKey")]
      public CsePersonAddress HiddenPageKey
      {
        get => hiddenPageKey ??= new();
        set => hiddenPageKey = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 10;

      private CsePersonAddress hiddenPageKey;
    }

    /// <summary>A HiddenGroup group.</summary>
    [Serializable]
    public class HiddenGroup
    {
      /// <summary>
      /// A value of HiddenUpd.
      /// </summary>
      [JsonPropertyName("hiddenUpd")]
      public CsePersonAddress HiddenUpd
      {
        get => hiddenUpd ??= new();
        set => hiddenUpd = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 4;

      private CsePersonAddress hiddenUpd;
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
    /// A value of ArFromCaseRole.
    /// </summary>
    [JsonPropertyName("arFromCaseRole")]
    public CsePersonsWorkSet ArFromCaseRole
    {
      get => arFromCaseRole ??= new();
      set => arFromCaseRole = value;
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
    /// A value of HpromptLineNo.
    /// </summary>
    [JsonPropertyName("hpromptLineNo")]
    public Common HpromptLineNo
    {
      get => hpromptLineNo ??= new();
      set => hpromptLineNo = value;
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
    /// A value of PromptCaseComp.
    /// </summary>
    [JsonPropertyName("promptCaseComp")]
    public Common PromptCaseComp
    {
      get => promptCaseComp ??= new();
      set => promptCaseComp = value;
    }

    /// <summary>
    /// A value of PromptRoleCase.
    /// </summary>
    [JsonPropertyName("promptRoleCase")]
    public Common PromptRoleCase
    {
      get => promptRoleCase ??= new();
      set => promptRoleCase = value;
    }

    /// <summary>
    /// A value of Search.
    /// </summary>
    [JsonPropertyName("search")]
    public CsePersonsWorkSet Search
    {
      get => search ??= new();
      set => search = value;
    }

    /// <summary>
    /// A value of Last.
    /// </summary>
    [JsonPropertyName("last")]
    public CsePersonAddress Last
    {
      get => last ??= new();
      set => last = value;
    }

    /// <summary>
    /// A value of ScrollMinus.
    /// </summary>
    [JsonPropertyName("scrollMinus")]
    public WorkArea ScrollMinus
    {
      get => scrollMinus ??= new();
      set => scrollMinus = value;
    }

    /// <summary>
    /// A value of ScrollPlus.
    /// </summary>
    [JsonPropertyName("scrollPlus")]
    public WorkArea ScrollPlus
    {
      get => scrollPlus ??= new();
      set => scrollPlus = value;
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
    /// A value of Next.
    /// </summary>
    [JsonPropertyName("next")]
    public Case1 Next
    {
      get => next ??= new();
      set => next = value;
    }

    /// <summary>
    /// A value of ApCommon.
    /// </summary>
    [JsonPropertyName("apCommon")]
    public Common ApCommon
    {
      get => apCommon ??= new();
      set => apCommon = value;
    }

    /// <summary>
    /// A value of ArCommon.
    /// </summary>
    [JsonPropertyName("arCommon")]
    public Common ArCommon
    {
      get => arCommon ??= new();
      set => arCommon = value;
    }

    /// <summary>
    /// A value of ArCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("arCsePersonsWorkSet")]
    public CsePersonsWorkSet ArCsePersonsWorkSet
    {
      get => arCsePersonsWorkSet ??= new();
      set => arCsePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of ApCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("apCsePersonsWorkSet")]
    public CsePersonsWorkSet ApCsePersonsWorkSet
    {
      get => apCsePersonsWorkSet ??= new();
      set => apCsePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of HiddenStandard.
    /// </summary>
    [JsonPropertyName("hiddenStandard")]
    public Standard HiddenStandard
    {
      get => hiddenStandard ??= new();
      set => hiddenStandard = value;
    }

    /// <summary>
    /// A value of HiddenNext.
    /// </summary>
    [JsonPropertyName("hiddenNext")]
    public Case1 HiddenNext
    {
      get => hiddenNext ??= new();
      set => hiddenNext = value;
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
    /// Gets a value of Import1.
    /// </summary>
    [JsonIgnore]
    public Array<ImportGroup> Import1 =>
      import1 ??= new(ImportGroup.Capacity, 0);

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
    /// Gets a value of HiddenPageKeys.
    /// </summary>
    [JsonIgnore]
    public Array<HiddenPageKeysGroup> HiddenPageKeys => hiddenPageKeys ??= new(
      HiddenPageKeysGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of HiddenPageKeys for json serialization.
    /// </summary>
    [JsonPropertyName("hiddenPageKeys")]
    [Computed]
    public IList<HiddenPageKeysGroup> HiddenPageKeys_Json
    {
      get => hiddenPageKeys;
      set => HiddenPageKeys.Assign(value);
    }

    /// <summary>
    /// Gets a value of Hidden.
    /// </summary>
    [JsonIgnore]
    public Array<HiddenGroup> Hidden => hidden ??= new(HiddenGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Hidden for json serialization.
    /// </summary>
    [JsonPropertyName("hidden")]
    [Computed]
    public IList<HiddenGroup> Hidden_Json
    {
      get => hidden;
      set => Hidden.Assign(value);
    }

    /// <summary>
    /// A value of ApActive.
    /// </summary>
    [JsonPropertyName("apActive")]
    public Common ApActive
    {
      get => apActive ??= new();
      set => apActive = value;
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
    /// A value of HeaderLine.
    /// </summary>
    [JsonPropertyName("headerLine")]
    public WorkArea HeaderLine
    {
      get => headerLine ??= new();
      set => headerLine = value;
    }

    private CsePerson hiddenCsePerson;
    private CsePersonsWorkSet arFromCaseRole;
    private Standard standard;
    private Common hpromptLineNo;
    private CodeValue selected;
    private Common promptCaseComp;
    private Common promptRoleCase;
    private CsePersonsWorkSet search;
    private CsePersonAddress last;
    private WorkArea scrollMinus;
    private WorkArea scrollPlus;
    private Case1 case1;
    private Case1 next;
    private Common apCommon;
    private Common arCommon;
    private CsePersonsWorkSet arCsePersonsWorkSet;
    private CsePersonsWorkSet apCsePersonsWorkSet;
    private Standard hiddenStandard;
    private Case1 hiddenNext;
    private NextTranInfo hiddenNextTranInfo;
    private Office office;
    private ServiceProvider serviceProvider;
    private Array<ImportGroup> import1;
    private Array<HiddenPageKeysGroup> hiddenPageKeys;
    private Array<HiddenGroup> hidden;
    private Common apActive;
    private Common caseOpen;
    private WorkArea headerLine;
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
      /// A value of Hdet.
      /// </summary>
      [JsonPropertyName("hdet")]
      public CsePersonAddress Hdet
      {
        get => hdet ??= new();
        set => hdet = value;
      }

      /// <summary>
      /// A value of PromptCountry.
      /// </summary>
      [JsonPropertyName("promptCountry")]
      public Common PromptCountry
      {
        get => promptCountry ??= new();
        set => promptCountry = value;
      }

      /// <summary>
      /// A value of PromptSourceCode.
      /// </summary>
      [JsonPropertyName("promptSourceCode")]
      public Common PromptSourceCode
      {
        get => promptSourceCode ??= new();
        set => promptSourceCode = value;
      }

      /// <summary>
      /// A value of PromptEndCode.
      /// </summary>
      [JsonPropertyName("promptEndCode")]
      public Common PromptEndCode
      {
        get => promptEndCode ??= new();
        set => promptEndCode = value;
      }

      /// <summary>
      /// A value of PromptReturnCode.
      /// </summary>
      [JsonPropertyName("promptReturnCode")]
      public Common PromptReturnCode
      {
        get => promptReturnCode ??= new();
        set => promptReturnCode = value;
      }

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
      /// A value of DetailCsePersonAddress.
      /// </summary>
      [JsonPropertyName("detailCsePersonAddress")]
      public CsePersonAddress DetailCsePersonAddress
      {
        get => detailCsePersonAddress ??= new();
        set => detailCsePersonAddress = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 4;

      private CsePersonAddress hdet;
      private Common promptCountry;
      private Common promptSourceCode;
      private Common promptEndCode;
      private Common promptReturnCode;
      private Common detailCommon;
      private CsePersonAddress detailCsePersonAddress;
    }

    /// <summary>A HiddenPageKeysGroup group.</summary>
    [Serializable]
    public class HiddenPageKeysGroup
    {
      /// <summary>
      /// A value of HiddenPageKey.
      /// </summary>
      [JsonPropertyName("hiddenPageKey")]
      public CsePersonAddress HiddenPageKey
      {
        get => hiddenPageKey ??= new();
        set => hiddenPageKey = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 10;

      private CsePersonAddress hiddenPageKey;
    }

    /// <summary>A HiddenGroup group.</summary>
    [Serializable]
    public class HiddenGroup
    {
      /// <summary>
      /// A value of HiddenUpd.
      /// </summary>
      [JsonPropertyName("hiddenUpd")]
      public CsePersonAddress HiddenUpd
      {
        get => hiddenUpd ??= new();
        set => hiddenUpd = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 4;

      private CsePersonAddress hiddenUpd;
    }

    /// <summary>A ZdelGroup group.</summary>
    [Serializable]
    public class ZdelGroup
    {
      /// <summary>
      /// A value of ZdelExportGrpDetCommon.
      /// </summary>
      [JsonPropertyName("zdelExportGrpDetCommon")]
      public Common ZdelExportGrpDetCommon
      {
        get => zdelExportGrpDetCommon ??= new();
        set => zdelExportGrpDetCommon = value;
      }

      /// <summary>
      /// A value of ZdelExportGrpDetCsePersonAddress.
      /// </summary>
      [JsonPropertyName("zdelExportGrpDetCsePersonAddress")]
      public CsePersonAddress ZdelExportGrpDetCsePersonAddress
      {
        get => zdelExportGrpDetCsePersonAddress ??= new();
        set => zdelExportGrpDetCsePersonAddress = value;
      }

      /// <summary>
      /// A value of ZdelExportGrpStatePrmt.
      /// </summary>
      [JsonPropertyName("zdelExportGrpStatePrmt")]
      public WorkArea ZdelExportGrpStatePrmt
      {
        get => zdelExportGrpStatePrmt ??= new();
        set => zdelExportGrpStatePrmt = value;
      }

      /// <summary>
      /// A value of ZdelExportGrpEnddtCdPrmt.
      /// </summary>
      [JsonPropertyName("zdelExportGrpEnddtCdPrmt")]
      public WorkArea ZdelExportGrpEnddtCdPrmt
      {
        get => zdelExportGrpEnddtCdPrmt ??= new();
        set => zdelExportGrpEnddtCdPrmt = value;
      }

      /// <summary>
      /// A value of ZdelExportGrpSrcePrmt.
      /// </summary>
      [JsonPropertyName("zdelExportGrpSrcePrmt")]
      public WorkArea ZdelExportGrpSrcePrmt
      {
        get => zdelExportGrpSrcePrmt ??= new();
        set => zdelExportGrpSrcePrmt = value;
      }

      /// <summary>
      /// A value of ZdelExportGrpReturnPrmt.
      /// </summary>
      [JsonPropertyName("zdelExportGrpReturnPrmt")]
      public WorkArea ZdelExportGrpReturnPrmt
      {
        get => zdelExportGrpReturnPrmt ??= new();
        set => zdelExportGrpReturnPrmt = value;
      }

      /// <summary>
      /// A value of ZdelExportGrpTypePrmt.
      /// </summary>
      [JsonPropertyName("zdelExportGrpTypePrmt")]
      public WorkArea ZdelExportGrpTypePrmt
      {
        get => zdelExportGrpTypePrmt ??= new();
        set => zdelExportGrpTypePrmt = value;
      }

      /// <summary>
      /// A value of ZdelExportGrpCntyPrmt.
      /// </summary>
      [JsonPropertyName("zdelExportGrpCntyPrmt")]
      public WorkArea ZdelExportGrpCntyPrmt
      {
        get => zdelExportGrpCntyPrmt ??= new();
        set => zdelExportGrpCntyPrmt = value;
      }

      /// <summary>
      /// A value of ZdelExportGrpHiddenDet.
      /// </summary>
      [JsonPropertyName("zdelExportGrpHiddenDet")]
      public CsePersonAddress ZdelExportGrpHiddenDet
      {
        get => zdelExportGrpHiddenDet ??= new();
        set => zdelExportGrpHiddenDet = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 5;

      private Common zdelExportGrpDetCommon;
      private CsePersonAddress zdelExportGrpDetCsePersonAddress;
      private WorkArea zdelExportGrpStatePrmt;
      private WorkArea zdelExportGrpEnddtCdPrmt;
      private WorkArea zdelExportGrpSrcePrmt;
      private WorkArea zdelExportGrpReturnPrmt;
      private WorkArea zdelExportGrpTypePrmt;
      private WorkArea zdelExportGrpCntyPrmt;
      private CsePersonAddress zdelExportGrpHiddenDet;
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
    /// A value of ArFromCaseRole.
    /// </summary>
    [JsonPropertyName("arFromCaseRole")]
    public CsePersonsWorkSet ArFromCaseRole
    {
      get => arFromCaseRole ??= new();
      set => arFromCaseRole = value;
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
    /// A value of HpromptLineNo.
    /// </summary>
    [JsonPropertyName("hpromptLineNo")]
    public Common HpromptLineNo
    {
      get => hpromptLineNo ??= new();
      set => hpromptLineNo = value;
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
    /// A value of PromptCaseComp.
    /// </summary>
    [JsonPropertyName("promptCaseComp")]
    public Common PromptCaseComp
    {
      get => promptCaseComp ??= new();
      set => promptCaseComp = value;
    }

    /// <summary>
    /// A value of PromptRoleCase.
    /// </summary>
    [JsonPropertyName("promptRoleCase")]
    public Common PromptRoleCase
    {
      get => promptRoleCase ??= new();
      set => promptRoleCase = value;
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
    /// A value of Next.
    /// </summary>
    [JsonPropertyName("next")]
    public Case1 Next
    {
      get => next ??= new();
      set => next = value;
    }

    /// <summary>
    /// A value of ApCommon.
    /// </summary>
    [JsonPropertyName("apCommon")]
    public Common ApCommon
    {
      get => apCommon ??= new();
      set => apCommon = value;
    }

    /// <summary>
    /// A value of ArCommon.
    /// </summary>
    [JsonPropertyName("arCommon")]
    public Common ArCommon
    {
      get => arCommon ??= new();
      set => arCommon = value;
    }

    /// <summary>
    /// A value of ApCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("apCsePersonsWorkSet")]
    public CsePersonsWorkSet ApCsePersonsWorkSet
    {
      get => apCsePersonsWorkSet ??= new();
      set => apCsePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of ArCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("arCsePersonsWorkSet")]
    public CsePersonsWorkSet ArCsePersonsWorkSet
    {
      get => arCsePersonsWorkSet ??= new();
      set => arCsePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of HiddenStandard.
    /// </summary>
    [JsonPropertyName("hiddenStandard")]
    public Standard HiddenStandard
    {
      get => hiddenStandard ??= new();
      set => hiddenStandard = value;
    }

    /// <summary>
    /// A value of ScrollPlus.
    /// </summary>
    [JsonPropertyName("scrollPlus")]
    public WorkArea ScrollPlus
    {
      get => scrollPlus ??= new();
      set => scrollPlus = value;
    }

    /// <summary>
    /// A value of ScrollMinus.
    /// </summary>
    [JsonPropertyName("scrollMinus")]
    public WorkArea ScrollMinus
    {
      get => scrollMinus ??= new();
      set => scrollMinus = value;
    }

    /// <summary>
    /// A value of Last.
    /// </summary>
    [JsonPropertyName("last")]
    public CsePersonAddress Last
    {
      get => last ??= new();
      set => last = value;
    }

    /// <summary>
    /// A value of Start.
    /// </summary>
    [JsonPropertyName("start")]
    public CsePersonsWorkSet Start
    {
      get => start ??= new();
      set => start = value;
    }

    /// <summary>
    /// A value of HiddenNext.
    /// </summary>
    [JsonPropertyName("hiddenNext")]
    public Case1 HiddenNext
    {
      get => hiddenNext ??= new();
      set => hiddenNext = value;
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
    /// Gets a value of Export1.
    /// </summary>
    [JsonIgnore]
    public Array<ExportGroup> Export1 =>
      export1 ??= new(ExportGroup.Capacity, 0);

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
    /// Gets a value of HiddenPageKeys.
    /// </summary>
    [JsonIgnore]
    public Array<HiddenPageKeysGroup> HiddenPageKeys => hiddenPageKeys ??= new(
      HiddenPageKeysGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of HiddenPageKeys for json serialization.
    /// </summary>
    [JsonPropertyName("hiddenPageKeys")]
    [Computed]
    public IList<HiddenPageKeysGroup> HiddenPageKeys_Json
    {
      get => hiddenPageKeys;
      set => HiddenPageKeys.Assign(value);
    }

    /// <summary>
    /// Gets a value of Hidden.
    /// </summary>
    [JsonIgnore]
    public Array<HiddenGroup> Hidden => hidden ??= new(HiddenGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Hidden for json serialization.
    /// </summary>
    [JsonPropertyName("hidden")]
    [Computed]
    public IList<HiddenGroup> Hidden_Json
    {
      get => hidden;
      set => Hidden.Assign(value);
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
    /// A value of ApActive.
    /// </summary>
    [JsonPropertyName("apActive")]
    public Common ApActive
    {
      get => apActive ??= new();
      set => apActive = value;
    }

    /// <summary>
    /// A value of EmptyAddr.
    /// </summary>
    [JsonPropertyName("emptyAddr")]
    public CsePersonAddress EmptyAddr
    {
      get => emptyAddr ??= new();
      set => emptyAddr = value;
    }

    /// <summary>
    /// Gets a value of Zdel.
    /// </summary>
    [JsonIgnore]
    public Array<ZdelGroup> Zdel => zdel ??= new(ZdelGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Zdel for json serialization.
    /// </summary>
    [JsonPropertyName("zdel")]
    [Computed]
    public IList<ZdelGroup> Zdel_Json
    {
      get => zdel;
      set => Zdel.Assign(value);
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

    private CsePerson hiddenCsePerson;
    private CsePersonsWorkSet arFromCaseRole;
    private Standard standard;
    private Common hpromptLineNo;
    private Code prompt;
    private Common promptCaseComp;
    private Common promptRoleCase;
    private Case1 case1;
    private Case1 next;
    private Common apCommon;
    private Common arCommon;
    private CsePersonsWorkSet apCsePersonsWorkSet;
    private CsePersonsWorkSet arCsePersonsWorkSet;
    private Standard hiddenStandard;
    private WorkArea scrollPlus;
    private WorkArea scrollMinus;
    private CsePersonAddress last;
    private CsePersonsWorkSet start;
    private Case1 hiddenNext;
    private NextTranInfo hiddenNextTranInfo;
    private Office office;
    private ServiceProvider serviceProvider;
    private Array<ExportGroup> export1;
    private Array<HiddenPageKeysGroup> hiddenPageKeys;
    private Array<HiddenGroup> hidden;
    private Common caseOpen;
    private Common apActive;
    private CsePersonAddress emptyAddr;
    private Array<ZdelGroup> zdel;
    private WorkArea headerLine;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of TodayPlus6Months.
    /// </summary>
    [JsonPropertyName("todayPlus6Months")]
    public DateWorkArea TodayPlus6Months
    {
      get => todayPlus6Months ??= new();
      set => todayPlus6Months = value;
    }

    /// <summary>
    /// A value of FvTest.
    /// </summary>
    [JsonPropertyName("fvTest")]
    public CsePersonsWorkSet FvTest
    {
      get => fvTest ??= new();
      set => fvTest = value;
    }

    /// <summary>
    /// A value of FvCheck.
    /// </summary>
    [JsonPropertyName("fvCheck")]
    public Common FvCheck
    {
      get => fvCheck ??= new();
      set => fvCheck = value;
    }

    /// <summary>
    /// A value of CallCsnet.
    /// </summary>
    [JsonPropertyName("callCsnet")]
    public Common CallCsnet
    {
      get => callCsnet ??= new();
      set => callCsnet = value;
    }

    /// <summary>
    /// A value of Null1.
    /// </summary>
    [JsonPropertyName("null1")]
    public NextTranInfo Null1
    {
      get => null1 ??= new();
      set => null1 = value;
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
    /// A value of Csenet.
    /// </summary>
    [JsonPropertyName("csenet")]
    public CsePerson Csenet
    {
      get => csenet ??= new();
      set => csenet = value;
    }

    /// <summary>
    /// A value of Document.
    /// </summary>
    [JsonPropertyName("document")]
    public Document Document
    {
      get => document ??= new();
      set => document = value;
    }

    /// <summary>
    /// A value of SpDocKey.
    /// </summary>
    [JsonPropertyName("spDocKey")]
    public SpDocKey SpDocKey
    {
      get => spDocKey ??= new();
      set => spDocKey = value;
    }

    /// <summary>
    /// A value of InterfaceAlert.
    /// </summary>
    [JsonPropertyName("interfaceAlert")]
    public InterfaceAlert InterfaceAlert
    {
      get => interfaceAlert ??= new();
      set => interfaceAlert = value;
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
    /// A value of Country.
    /// </summary>
    [JsonPropertyName("country")]
    public Code Country
    {
      get => country ??= new();
      set => country = value;
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
    /// A value of Verify.
    /// </summary>
    [JsonPropertyName("verify")]
    public CodeValue Verify
    {
      get => verify ??= new();
      set => verify = value;
    }

    /// <summary>
    /// A value of AddressSource.
    /// </summary>
    [JsonPropertyName("addressSource")]
    public Code AddressSource
    {
      get => addressSource ??= new();
      set => addressSource = value;
    }

    /// <summary>
    /// A value of AddressEndCode.
    /// </summary>
    [JsonPropertyName("addressEndCode")]
    public Code AddressEndCode
    {
      get => addressEndCode ??= new();
      set => addressEndCode = value;
    }

    /// <summary>
    /// A value of AddressVerifiedCode.
    /// </summary>
    [JsonPropertyName("addressVerifiedCode")]
    public Code AddressVerifiedCode
    {
      get => addressVerifiedCode ??= new();
      set => addressVerifiedCode = value;
    }

    /// <summary>
    /// A value of MaxDate.
    /// </summary>
    [JsonPropertyName("maxDate")]
    public Code MaxDate
    {
      get => maxDate ??= new();
      set => maxDate = value;
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
    /// A value of Common.
    /// </summary>
    [JsonPropertyName("common")]
    public Common Common
    {
      get => common ??= new();
      set => common = value;
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
    /// A value of Infrastructure.
    /// </summary>
    [JsonPropertyName("infrastructure")]
    public Infrastructure Infrastructure
    {
      get => infrastructure ??= new();
      set => infrastructure = value;
    }

    /// <summary>
    /// A value of DetailText1.
    /// </summary>
    [JsonPropertyName("detailText1")]
    public WorkArea DetailText1
    {
      get => detailText1 ??= new();
      set => detailText1 = value;
    }

    /// <summary>
    /// A value of AparSelection.
    /// </summary>
    [JsonPropertyName("aparSelection")]
    public WorkArea AparSelection
    {
      get => aparSelection ??= new();
      set => aparSelection = value;
    }

    /// <summary>
    /// A value of RaiseEventFlag.
    /// </summary>
    [JsonPropertyName("raiseEventFlag")]
    public WorkArea RaiseEventFlag
    {
      get => raiseEventFlag ??= new();
      set => raiseEventFlag = value;
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
    /// A value of DetailText10.
    /// </summary>
    [JsonPropertyName("detailText10")]
    public TextWorkArea DetailText10
    {
      get => detailText10 ??= new();
      set => detailText10 = value;
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
    /// A value of NumberOfEvents.
    /// </summary>
    [JsonPropertyName("numberOfEvents")]
    public Common NumberOfEvents
    {
      get => numberOfEvents ??= new();
      set => numberOfEvents = value;
    }

    /// <summary>
    /// A value of SendDateUpdated.
    /// </summary>
    [JsonPropertyName("sendDateUpdated")]
    public Common SendDateUpdated
    {
      get => sendDateUpdated ??= new();
      set => sendDateUpdated = value;
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
    /// A value of Addr4CloseDoc.
    /// </summary>
    [JsonPropertyName("addr4CloseDoc")]
    public CsePersonAddress Addr4CloseDoc
    {
      get => addr4CloseDoc ??= new();
      set => addr4CloseDoc = value;
    }

    /// <summary>
    /// A value of CloseMonitoredDoc.
    /// </summary>
    [JsonPropertyName("closeMonitoredDoc")]
    public WorkArea CloseMonitoredDoc
    {
      get => closeMonitoredDoc ??= new();
      set => closeMonitoredDoc = value;
    }

    /// <summary>
    /// A value of Save.
    /// </summary>
    [JsonPropertyName("save")]
    public Common Save
    {
      get => save ??= new();
      set => save = value;
    }

    /// <summary>
    /// A value of BatchTimestampWorkArea.
    /// </summary>
    [JsonPropertyName("batchTimestampWorkArea")]
    public BatchTimestampWorkArea BatchTimestampWorkArea
    {
      get => batchTimestampWorkArea ??= new();
      set => batchTimestampWorkArea = value;
    }

    /// <summary>
    /// A value of PrintProcess.
    /// </summary>
    [JsonPropertyName("printProcess")]
    public Common PrintProcess
    {
      get => printProcess ??= new();
      set => printProcess = value;
    }

    /// <summary>
    /// A value of SpDocLiteral.
    /// </summary>
    [JsonPropertyName("spDocLiteral")]
    public SpDocLiteral SpDocLiteral
    {
      get => spDocLiteral ??= new();
      set => spDocLiteral = value;
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
    /// A value of Asterisk.
    /// </summary>
    [JsonPropertyName("asterisk")]
    public CsePersonAddress Asterisk
    {
      get => asterisk ??= new();
      set => asterisk = value;
    }

    private DateWorkArea todayPlus6Months;
    private CsePersonsWorkSet fvTest;
    private Common fvCheck;
    private Common callCsnet;
    private NextTranInfo null1;
    private Common screenIdentification;
    private CsePerson csenet;
    private Document document;
    private SpDocKey spDocKey;
    private InterfaceAlert interfaceAlert;
    private DateWorkArea zero;
    private Code country;
    private Common returnCode;
    private CodeValue verify;
    private Code addressSource;
    private Code addressEndCode;
    private Code addressVerifiedCode;
    private Code maxDate;
    private Common error;
    private Common common;
    private Common multipleAps;
    private Infrastructure infrastructure;
    private WorkArea detailText1;
    private WorkArea aparSelection;
    private WorkArea raiseEventFlag;
    private TextWorkArea detailText30;
    private TextWorkArea detailText10;
    private CsePerson csePerson;
    private Common numberOfEvents;
    private Common sendDateUpdated;
    private DateWorkArea current;
    private CsePersonAddress addr4CloseDoc;
    private WorkArea closeMonitoredDoc;
    private Common save;
    private BatchTimestampWorkArea batchTimestampWorkArea;
    private Common printProcess;
    private SpDocLiteral spDocLiteral;
    private Common position;
    private CsePersonAddress asterisk;
  }
#endregion
}
