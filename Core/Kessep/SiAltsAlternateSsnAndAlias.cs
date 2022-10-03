// Program: SI_ALTS_ALTERNATE_SSN_AND_ALIAS, ID: 372713653, model: 746.
// Short name: SWEALTSP
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
/// A program: SI_ALTS_ALTERNATE_SSN_AND_ALIAS.
/// </para>
/// <para>
/// RESP: SRVINIT
/// This procedure adds, updates and deletes information about an CSE Person's 
/// Aliases and alternate SSNs.  This information will be transferred to the
/// ADABAS files.  Nicknames will be held on the DB2 system.
/// </para>
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class SiAltsAlternateSsnAndAlias: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_ALTS_ALTERNATE_SSN_AND_ALIAS program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiAltsAlternateSsnAndAlias(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiAltsAlternateSsnAndAlias.
  /// </summary>
  public SiAltsAlternateSsnAndAlias(IContext context, Import import,
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
    //       M A I N T E N A N C E   L O G
    //   Date    Developer	Description
    // 08-01-95  H Sharland	Initial development
    // 09-06-95  K Evans	Continue development
    // 02-13-96  Lewis		Complete development
    // 10/31/96  G. Lofton	Added new security and removed old.
    // 06/05/97  Sid Chowdhary Cleanup and Fixes.
    // 06/17/97  M.D.Wheaton   Removed datenum
    // ------------------------------------------------------------
    // 03/03/99 W.Campbell     Added PFK9 (RETURN)
    //                         capability to the screen and
    //                         PSTEP actblk logic.
    //                         Also, removed 4 make statements
    //                         which were causing consistency
    //                         check errors because the
    //                         attributes were not on the screen.
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
    // 05/26/99 W.Campbell     Replaced zd exit states.
    // -----------------------------------------------
    // 07/27/99 M.Lachowicz    Changed exit state
    // -----------------------------------------------
    // 12/06/00 M.Lachowicz    Changed header line. WR298.
    // -----------------------------------------------
    // 17/05/01 M.Lachowicz    PR119968.
    //                         Allow to create alternate SSN
    //                         without first and last name.
    // -----------------------------------------------
    // 01/27/02 GVandy  	PR160417 - Validate first, last and middle name.
    // ------------------------------------------------------------------------------------
    // 02/21/08   LSS  Added IF statement for length of SSN to error out if ssn 
    // not complete.
    //                 Added IF statement to verify ssn to error out if ssn 
    // contains a non-numeric character.
    // ------------------------------------------------------------------------------------------------------------------------------
    // 06/05/209   DDupree     Added check when updating or adding a ssn against
    // the invalid ssn table. Part of CQ7189.
    // __________________________________________________________________________________
    // 06/03/11 RMathews SSN Randomization changes. Valid SSN's now range from 
    // 001 through 899, excluding 666.
    // ---------------------------------------------------------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";
    MoveNextTranInfo(import.Hidden, export.Hidden);

    if (Equal(global.Command, "CLEAR"))
    {
      return;
    }

    // ---------------------------------------------
    // Move Imports to Exports
    // ---------------------------------------------
    export.Case1.Number = import.Case1.Number;
    export.Next.Number = import.Next.Number;
    export.HiddenNext.Number = import.HiddenNext.Number;
    export.ApCsePersonsWorkSet.Assign(import.ApCsePersonsWorkSet);
    export.HiddenAp.Assign(import.HiddenAp);
    export.ArCsePersonsWorkSet.Assign(import.ArCsePersonsWorkSet);
    export.ChCsePersonsWorkSet.Assign(import.ChCsePersonsWorkSet);
    export.ApMinus.Text1 = import.ApMinus.Text1;
    export.ApPlus.Text1 = import.ApPlus.Text1;
    export.ArMinus.Text1 = import.ArMinus.Text1;
    export.ArPlus.Text1 = import.ArPlus.Text1;
    export.ChMinus.Text1 = import.ChMinus.Text1;
    export.ChPlus.Text1 = import.ChPlus.Text1;
    export.ApPrompt.Text1 = import.ApPrompt.Text1;
    export.ArPrompt.Text1 = import.ArPrompt.Text1;
    export.ChPrompt.Text1 = import.ChPrompt.Text1;
    export.ApStandard.PageNumber = import.ApStandard.PageNumber;
    export.ArStandard.PageNumber = import.ArStandard.PageNumber;
    export.ChStandard.PageNumber = import.ChStandard.PageNumber;
    export.ApMaint.Flag = import.ApMaint.Flag;
    export.ArMaint.Flag = import.ArMaint.Flag;
    export.ChMaint.Flag = import.ChMaint.Flag;
    export.NextKeyAp.UniqueKey = import.NextKeyAp.UniqueKey;
    export.NextKeyAr.UniqueKey = import.NextKeyAr.UniqueKey;
    export.NextKeyCh.UniqueKey = import.NextKeyCh.UniqueKey;
    export.Standard.NextTransaction = import.Standard.NextTransaction;
    MoveOffice(import.Office, export.Office);
    export.ServiceProvider.LastName = import.ServiceProvider.LastName;
    export.ApActive.Flag = import.ApActive.Flag;
    export.CaseOpen.Flag = import.CaseOpen.Flag;
    export.HiddenFlowViaPrompt.Flag = import.HiddenFlowViaPrompt.Flag;
    export.HiddenAp.Assign(import.HiddenAp);
    export.HiddenCh.Assign(import.HiddenCh);

    // 12/07/00 M.L Start
    export.HeaderLine.Text35 = import.HeaderLine.Text35;

    // 12/07/00 M.L End
    // ---------------------------------------------
    // AP details
    // ---------------------------------------------
    local.Error.Count = 0;
    local.ApCommon.Count = 0;

    if (!import.Ap.IsEmpty)
    {
      for(import.Ap.Index = 0; import.Ap.Index < import.Ap.Count; ++
        import.Ap.Index)
      {
        if (!import.Ap.CheckSize())
        {
          break;
        }

        export.Ap.Index = import.Ap.Index;
        export.Ap.CheckSize();

        export.Ap.Update.DetApCommon.SelectChar =
          import.Ap.Item.DetApCommon.SelectChar;
        export.Ap.Update.DetApCsePersonsWorkSet.Assign(
          import.Ap.Item.DetApCsePersonsWorkSet);
        export.Ap.Update.ApSsn3.Text3 = import.Ap.Item.ApSsn3.Text3;
        export.Ap.Update.ApSsn2.Text3 = import.Ap.Item.ApSsn2.Text3;
        export.Ap.Update.ApSsn4.Text4 = import.Ap.Item.ApSsn4.Text4;
        export.Ap.Update.ApAe.Flag = import.Ap.Item.ApAe.Flag;
        export.Ap.Update.ApCse.Flag = import.Ap.Item.ApCse.Flag;
        export.Ap.Update.ApKanpay.Flag = import.Ap.Item.ApKanpay.Flag;
        export.Ap.Update.ApKscares.Flag = import.Ap.Item.ApKscares.Flag;
        export.Ap.Update.ApFa.Flag = import.Ap.Item.ApFa.Flag;
        export.Ap.Update.ApDbOccurrence.Flag =
          import.Ap.Item.ApDbOccurrence.Flag;
        export.Ap.Update.ApActiveOnKscares.Flag =
          import.Ap.Item.ApActiveOnKscares.Flag;
        export.Ap.Update.ApActiveOnKanpay.Flag =
          import.Ap.Item.ApActiveOnKanpay.Flag;
        export.Ap.Update.ApActiveOnCse.Flag = import.Ap.Item.ApActiveOnCse.Flag;
        export.Ap.Update.ApActiveOnAe.Flag = import.Ap.Item.ApActiveOnAe.Flag;
        export.Ap.Update.ApActiveOnFacts.Flag =
          import.Ap.Item.ApActiveOnFacts.Flag;
        export.Ap.Update.DetApPrev.Ssn = import.Ap.Item.DetApPrev.Ssn;
      }

      import.Ap.CheckIndex();
    }

    // ---------------------------------------------
    // AR details
    // ---------------------------------------------
    local.ArCommon.Count = 0;

    if (!import.Ar.IsEmpty)
    {
      for(import.Ar.Index = 0; import.Ar.Index < import.Ar.Count; ++
        import.Ar.Index)
      {
        if (!import.Ar.CheckSize())
        {
          break;
        }

        export.Ar.Index = import.Ar.Index;
        export.Ar.CheckSize();

        export.Ar.Update.DetArCommon.SelectChar =
          import.Ar.Item.DetArCommon.SelectChar;
        export.Ar.Update.DetArCsePersonsWorkSet.Assign(
          import.Ar.Item.DetArCsePersonsWorkSet);
        export.Ar.Update.ArSsn3.Text3 = import.Ar.Item.ArSsn3.Text3;
        export.Ar.Update.ArSsn2.Text3 = import.Ar.Item.ArSsn2.Text3;
        export.Ar.Update.ArSsn4.Text4 = import.Ar.Item.ArSsn4.Text4;
        export.Ar.Update.ArAe.Flag = import.Ar.Item.ArAe.Flag;
        export.Ar.Update.ArCse.Flag = import.Ar.Item.ArCse.Flag;
        export.Ar.Update.ArKanpay.Flag = import.Ar.Item.ArKanpay.Flag;
        export.Ar.Update.ArKscares.Flag = import.Ar.Item.ArKscares.Flag;
        export.Ar.Update.ArFa.Flag = import.Ar.Item.ArFa.Flag;
        export.Ar.Update.ArDbOccurrence.Flag =
          import.Ar.Item.ArDbOccurrence.Flag;
        export.Ar.Update.ArActiveOnKscares.Flag =
          import.Ar.Item.ArActiveOnKscares.Flag;
        export.Ar.Update.ArActiveOnKanpay.Flag =
          import.Ar.Item.ArActiveOnKanpay.Flag;
        export.Ar.Update.ArActiveOnCse.Flag = import.Ar.Item.ArActiveOnCse.Flag;
        export.Ar.Update.ArActiveOnAe.Flag = import.Ar.Item.ArActiveOnAe.Flag;
        export.Ar.Update.ArActiveOnFacts.Flag =
          import.Ar.Item.ArActiveOnFacts.Flag;
        export.Ar.Update.DetArPrev.Ssn = import.Ar.Item.DetArPrev.Ssn;
      }

      import.Ar.CheckIndex();
    }

    // -------------------------------------------------------------------------
    // Per WR# 020259,  the following code is added.
    //                                                        
    // Vithal (05/15/2002)
    // -------------------------------------------------------------------------
    // ---------------------------------------------
    //                     Start Code
    // ---------------------------------------------
    // ---------------------------------------------
    // CH details
    // ---------------------------------------------
    local.ChCommon.Count = 0;

    if (!import.Ch.IsEmpty)
    {
      for(import.Ch.Index = 0; import.Ch.Index < import.Ch.Count; ++
        import.Ch.Index)
      {
        if (!import.Ch.CheckSize())
        {
          break;
        }

        export.Ch.Index = import.Ch.Index;
        export.Ch.CheckSize();

        export.Ch.Update.DetChCommon.SelectChar =
          import.Ch.Item.DetChCommon.SelectChar;
        export.Ch.Update.DetChCsePersonsWorkSet.Assign(
          import.Ch.Item.DetChCsePersonsWorkSet);
        export.Ch.Update.ChSsn3.Text3 = import.Ch.Item.ChSsn3.Text3;
        export.Ch.Update.ChSsn2.Text3 = import.Ch.Item.ChSsn2.Text3;
        export.Ch.Update.ChSsn4.Text4 = import.Ch.Item.ChSsn4.Text4;
        export.Ch.Update.ChAe.Flag = import.Ch.Item.ChAe.Flag;
        export.Ch.Update.ChCse.Flag = import.Ch.Item.ChCse.Flag;
        export.Ch.Update.ChKanpay.Flag = import.Ch.Item.ChKanpay.Flag;
        export.Ch.Update.ChKscares.Flag = import.Ch.Item.ChKscares.Flag;
        export.Ch.Update.ChFa.Flag = import.Ch.Item.ChFa.Flag;
        export.Ch.Update.ChDbOccurrence.Flag =
          import.Ch.Item.ChDbOccurrence.Flag;
        export.Ch.Update.ChActiveOnKscares.Flag =
          import.Ch.Item.ChActiveOnKscares.Flag;
        export.Ch.Update.ChActiveOnKanpay.Flag =
          import.Ch.Item.ChActiveOnKanpay.Flag;
        export.Ch.Update.ChActiveOnCse.Flag = import.Ch.Item.ChActiveOnCse.Flag;
        export.Ch.Update.ChActiveOnAe.Flag = import.Ch.Item.ChActiveOnAe.Flag;
        export.Ch.Update.ChActiveOnFacts.Flag =
          import.Ch.Item.ChActiveOnFacts.Flag;
        export.Ch.Update.DetChPrev.Ssn = import.Ch.Item.DetChPrev.Ssn;
      }

      import.Ch.CheckIndex();
    }

    // ----------------------------------------------
    //                 End Code
    // ----------------------------------------------
    // ---------------------------------------------
    // AP details
    // ---------------------------------------------
    if (!import.PageKeysAp.IsEmpty)
    {
      for(import.PageKeysAp.Index = 0; import.PageKeysAp.Index < import
        .PageKeysAp.Count; ++import.PageKeysAp.Index)
      {
        if (!import.PageKeysAp.CheckSize())
        {
          break;
        }

        export.PageKeysAp.Index = import.PageKeysAp.Index;
        export.PageKeysAp.CheckSize();

        MoveCsePersonsWorkSet1(import.PageKeysAp.Item.PageKeyAp,
          export.PageKeysAp.Update.PageKeyAp);
      }

      import.PageKeysAp.CheckIndex();
    }

    // ---------------------------------------------
    // AR details
    // ---------------------------------------------
    if (!import.PageKeysAr.IsEmpty)
    {
      for(import.PageKeysAr.Index = 0; import.PageKeysAr.Index < import
        .PageKeysAr.Count; ++import.PageKeysAr.Index)
      {
        if (!import.PageKeysAr.CheckSize())
        {
          break;
        }

        export.PageKeysAr.Index = import.PageKeysAr.Index;
        export.PageKeysAr.CheckSize();

        MoveCsePersonsWorkSet1(import.PageKeysAr.Item.PageKeyAr,
          export.PageKeysAr.Update.PageKeyAr);
      }

      import.PageKeysAr.CheckIndex();
    }

    // -------------------------------------------------------------------------
    // Per WR# 020259,  the following code is added.
    //                                                        
    // Vithal (05/15/2002)
    // -------------------------------------------------------------------------
    // ---------------------------------------------
    //                     Start Code
    // ---------------------------------------------
    // ---------------------------------------------
    // CH details
    // ---------------------------------------------
    if (!import.PageKeysCh.IsEmpty)
    {
      for(import.PageKeysCh.Index = 0; import.PageKeysCh.Index < import
        .PageKeysCh.Count; ++import.PageKeysCh.Index)
      {
        if (!import.PageKeysCh.CheckSize())
        {
          break;
        }

        export.PageKeysCh.Index = import.PageKeysCh.Index;
        export.PageKeysCh.CheckSize();

        MoveCsePersonsWorkSet1(import.PageKeysCh.Item.PageKeyCh,
          export.PageKeysCh.Update.PageKeyCh);
      }

      import.PageKeysCh.CheckIndex();
    }

    // ----------------------------------------------
    //                 End Code
    // ----------------------------------------------
    UseCabZeroFillNumber2();

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
      export.Hidden.CaseNumber = export.Next.Number;
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

      if (IsExitState("ACO_NN0000_ALL_OK"))
      {
        export.ApCsePersonsWorkSet.Number = export.Hidden.CsePersonNumberAp ?? Spaces
          (10);
        export.Next.Number = export.Hidden.CaseNumber ?? Spaces(10);
        UseCabZeroFillNumber2();
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

    // ---------------------------------------------------------------------------------------
    // Per WR# 020259, the following code is commented. User must be able to add
    // /update/delete aliases for closed case. Also user must be able to modify
    // the AP alias even if the AP is inactive.
    // --------------------------------------------------------------------------------------
    // ---------------------------------------------
    //        S E C U R I T Y   L O G I C
    // ---------------------------------------------
    // to validate action level security
    switch(TrimEnd(global.Command))
    {
      case "APDS":
        break;
      case "ARDS":
        break;
      case "CHDS":
        break;
      case "APNX":
        break;
      case "APPV":
        break;
      case "ARPV":
        break;
      case "ARNX":
        break;
      default:
        UseScCabTestSecurity();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }

        break;
    }

    // ---------------------------------------------
    //        P F K E Y   P R O C E S S I N G
    // ---------------------------------------------
    for(export.Ap.Index = 0; export.Ap.Index < export.Ap.Count; ++
      export.Ap.Index)
    {
      if (!export.Ap.CheckSize())
      {
        break;
      }

      switch(AsChar(export.Ap.Item.DetApCommon.SelectChar))
      {
        case ' ':
          break;
        case 'S':
          ++local.ApCommon.Count;

          break;
        case '*':
          export.Ap.Update.DetApCommon.SelectChar = "";

          break;
        default:
          var field = GetField(export.Ap.Item.DetApCommon, "selectChar");

          field.Error = true;

          ++local.Error.Count;

          break;
      }
    }

    export.Ap.CheckIndex();

    for(export.Ar.Index = 0; export.Ar.Index < export.Ar.Count; ++
      export.Ar.Index)
    {
      if (!export.Ar.CheckSize())
      {
        break;
      }

      switch(AsChar(export.Ar.Item.DetArCommon.SelectChar))
      {
        case ' ':
          break;
        case 'S':
          ++local.ArCommon.Count;

          break;
        case '*':
          export.Ar.Update.DetArCommon.SelectChar = "";

          break;
        default:
          var field = GetField(export.Ar.Item.DetArCommon, "selectChar");

          field.Error = true;

          ++local.Error.Count;

          break;
      }
    }

    export.Ar.CheckIndex();

    // -------------------------------------------------------------------------
    // Per WR# 020259,  the following code is added.
    //                                                        
    // Vithal (05/15/2002)
    // -------------------------------------------------------------------------
    // ---------------------------------------------
    //                     Start Code
    // ---------------------------------------------
    for(export.Ch.Index = 0; export.Ch.Index < export.Ch.Count; ++
      export.Ch.Index)
    {
      if (!export.Ch.CheckSize())
      {
        break;
      }

      switch(AsChar(export.Ch.Item.DetChCommon.SelectChar))
      {
        case ' ':
          break;
        case 'S':
          ++local.ChCommon.Count;

          break;
        case '*':
          export.Ch.Update.DetChCommon.SelectChar = "";

          break;
        default:
          var field = GetField(export.Ch.Item.DetChCommon, "selectChar");

          field.Error = true;

          ++local.Error.Count;

          break;
      }
    }

    export.Ch.CheckIndex();

    // ----------------------------------------------
    //                 End Code
    // ----------------------------------------------
    if (local.ApCommon.Count > 0 || local.ArCommon.Count > 0 || local
      .ChCommon.Count > 0)
    {
      if (Equal(global.Command, "APNX") || Equal(global.Command, "APPV") || Equal
        (global.Command, "ARNX") || Equal(global.Command, "ARPV") || Equal
        (global.Command, "CHPV") || Equal(global.Command, "CHNX"))
      {
        ExitState = "ACO_NE0000_SCROLL_INVALID_W_SEL";

        return;
      }
    }

    if (local.Error.Count > 0)
    {
      // -----------------------------------------------
      // 05/26/99 W.Campbell -  Replaced zd exit states.
      // -----------------------------------------------
      ExitState = "ACO_NE0000_INVALID_SELECT_CODE1";

      return;
    }

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
      else if (Equal(export.ApCsePersonsWorkSet.Number, export.HiddenAp.Number))
      {
        export.ApCsePersonsWorkSet.Number = "";
        export.HiddenAp.Number = "";
      }
    }

    if (!Equal(export.HiddenAp.Number, export.ApCsePersonsWorkSet.Number))
    {
      if (IsEmpty(export.ApCsePersonsWorkSet.Number))
      {
        export.ApCsePersonsWorkSet.Number = export.HiddenAp.Number;
      }
      else if (!Equal(global.Command, "DISPLAY"))
      {
        var field = GetField(export.ApCsePersonsWorkSet, "number");

        field.Error = true;

        ExitState = "ACO_NW0000_MUST_DISPLAY_FIRST";

        return;
      }
    }

    switch(TrimEnd(global.Command))
    {
      case "RETURN":
        // ---------------------------------------------
        // 03/03/99 W.Campbell - Added PFK9 (RETURN)
        // capability to the screen and PSTEP actblk logic.
        // Also, removed 4 make statements which were
        // causing consistency check errors because the
        // attributes were not on the screen.
        // ---------------------------------------------
        ExitState = "ACO_NE0000_RETURN";

        break;
      case "EXIT":
        ExitState = "ECO_LNK_RETURN_TO_MENU";

        break;
      case "DISPLAY":
        if (IsEmpty(export.Case1.Number))
        {
          if (IsEmpty(export.Next.Number))
          {
            var field = GetField(export.Next, "number");

            field.Error = true;

            ExitState = "CASE_NUMBER_REQUIRED";

            break;
          }

          export.Case1.Number = export.Next.Number;
        }

        if (!IsEmpty(export.Next.Number) && !
          Equal(export.Next.Number, export.Case1.Number))
        {
          export.Case1.Number = export.Next.Number;
          export.ApCsePersonsWorkSet.Number = "";
          export.ArCsePersonsWorkSet.Number = "";
          export.ChCsePersonsWorkSet.Number = "";
        }

        UseCabZeroFillNumber1();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          var field = GetField(export.ApCsePersonsWorkSet, "number");

          field.Error = true;

          break;
        }

        export.Ap.Count = 0;
        export.Ar.Count = 0;
        export.Ch.Count = 0;
        export.ApPlus.Text1 = "";
        export.ApMinus.Text1 = "";
        export.ArPlus.Text1 = "";
        export.ArMinus.Text1 = "";
        export.ChPlus.Text1 = "";
        export.ChMinus.Text1 = "";

        if (AsChar(export.HiddenFlowViaPrompt.Flag) == 'Y')
        {
          // ---------------------------------------------------------------------------------
          // User went to COMP by prompting (PF4) on AP or CH and coming back. 
          // It is decided that user can select only AP or CH and come back to
          // ALTS. But on dialog flow both AP and CH selected on COMP are
          // matched and sent to ALTS. If the user did not select AP or CH, (s/
          // he can do this only if prompted from ALTS,  not for first time
          // display(PF2), ) the export view on ALTS will be blanked out. So
          // check if the user prompted from ALTS (
          // export_hidden_flow_via_prompt flag = 'Y') , then if export AP or CH
          // is blank, move the export_hidden to export view.
          // --------------------------------------------------------------------------------------
          if (IsEmpty(export.ApCsePersonsWorkSet.Number))
          {
            MoveCsePersonsWorkSet3(export.HiddenAp, export.ApCsePersonsWorkSet);
            export.HiddenCh.Assign(export.ChCsePersonsWorkSet);
          }

          if (IsEmpty(export.ChCsePersonsWorkSet.Number))
          {
            MoveCsePersonsWorkSet3(export.HiddenCh, export.ChCsePersonsWorkSet);
            export.HiddenAp.Assign(export.ApCsePersonsWorkSet);
          }
        }

        UseSiAltsReadCaseHeaderInform();

        if (IsExitState("ACO_NN0000_ALL_OK") || IsExitState
          ("NO_APS_ON_A_CASE") || IsExitState("SI0000_NO_CHS_ON_A_CASE"))
        {
          if (AsChar(export.HiddenFlowViaPrompt.Flag) == 'Y')
          {
          }
          else
          {
            export.HiddenAp.Assign(export.ApCsePersonsWorkSet);
            export.HiddenCh.Assign(export.ChCsePersonsWorkSet);
          }
        }
        else
        {
          break;
        }

        if (AsChar(local.MultipleAps.Flag) == 'Y' && IsEmpty
          (export.ApCsePersonsWorkSet.Number) || AsChar
          (local.MultipleChs.Flag) == 'Y' && IsEmpty
          (export.ChCsePersonsWorkSet.Number))
        {
          // -----------------------------------------------
          // 05/03/99 W.Campbell - Added code to send
          // selection needed msg to COMP.  BE SURE
          // TO MATCH next_tran_info ON THE DIALOG
          // FLOW.
          // -----------------------------------------------
          if (AsChar(export.HiddenFlowViaPrompt.Flag) == 'Y')
          {
            // ---------------------------------------------------------------------------------
            // User went to COMP by prompting (PF4) on AP or CH and coming back.
            // It is decided that user can select only AP or CH and come back
            // to ALTS. But on dialog flow both AP and CH selected on COMP are
            // matched and sent to ALTS. If the user did not select AP or CH, (s
            // /he can do this only if prompted from ALTS,  not for first time
            // display(PF2), ) the export view on ALTS will be blanked out. So
            // check if the user prompted from ALTS (
            // export_hidden_flow_via_prompt flag = 'Y') , then if export AP or
            // CH is blank, move the export_hidden to export view.
            // --------------------------------------------------------------------------------------
            if (IsEmpty(export.ApCsePersonsWorkSet.Number))
            {
              MoveCsePersonsWorkSet3(export.HiddenAp, export.ApCsePersonsWorkSet);
                
              export.HiddenCh.Assign(export.ChCsePersonsWorkSet);
            }

            if (IsEmpty(export.ChCsePersonsWorkSet.Number))
            {
              MoveCsePersonsWorkSet3(export.HiddenCh, export.ChCsePersonsWorkSet);
                
              export.HiddenAp.Assign(export.ApCsePersonsWorkSet);
            }

            goto Test1;
          }

          export.Hidden.MiscText1 = "SELECTION NEEDED";
          export.HiddenFromAlts.Flag = "Y";
          ExitState = "ECO_LNK_TO_CASE_COMPOSITION";

          break;
        }

Test1:

        UseSiReadOfficeOspHeader();

        if (IsExitState("ACO_NN0000_ALL_OK") || IsExitState
          ("NO_APS_ON_A_CASE") || IsExitState("SI0000_NO_CHS_ON_A_CASE"))
        {
        }
        else
        {
          break;
        }

        local.ApCsePersonsWorkSet.Number = export.ApCsePersonsWorkSet.Number;
        local.ArCsePersonsWorkSet.Number = export.ArCsePersonsWorkSet.Number;
        local.ChCsePersonsWorkSet.Number = export.ChCsePersonsWorkSet.Number;
        UseSiAltsBuildAliasAndSsn1();

        if (IsExitState("ACO_NN0000_ALL_OK") || IsExitState
          ("NO_APS_ON_A_CASE") || IsExitState("SI0000_NO_CHS_ON_A_CASE"))
        {
        }
        else
        {
          break;
        }

        if (export.Ar.IsEmpty && !
          IsEmpty(export.ArCsePersonsWorkSet.Number) && export.Ap.IsEmpty && !
          IsEmpty(export.ApCsePersonsWorkSet.Number) && export.Ch.IsEmpty && !
          IsEmpty(export.ChCsePersonsWorkSet.Number))
        {
          ExitState = "SI0000_NO_ALIASES_FOR_AP_AR_CH";
        }
        else
        {
          if (export.Ap.IsEmpty && !
            IsEmpty(export.ApCsePersonsWorkSet.Number) && export.Ar.IsEmpty && !
            IsEmpty(export.ArCsePersonsWorkSet.Number))
          {
            ExitState = "NO_ALIASES_FOR_AP_OR_AR";

            goto Test2;
          }

          if (export.Ap.IsEmpty && !
            IsEmpty(export.ApCsePersonsWorkSet.Number) && export.Ch.IsEmpty && !
            IsEmpty(export.ChCsePersonsWorkSet.Number))
          {
            ExitState = "SI0000_NO_ALIAS_FOR_AP_OR_CH";

            goto Test2;
          }

          if (export.Ar.IsEmpty && !
            IsEmpty(export.ArCsePersonsWorkSet.Number) && export.Ch.IsEmpty && !
            IsEmpty(export.ChCsePersonsWorkSet.Number))
          {
            ExitState = "SI0000_NO_ALIASES_FOR_AR_OR_CH";

            goto Test2;
          }

          if (export.Ap.IsEmpty && !IsEmpty(export.ApCsePersonsWorkSet.Number))
          {
            ExitState = "NO_AP_ALIASES_FOUND";
          }

          if (export.Ar.IsEmpty && !IsEmpty(export.ArCsePersonsWorkSet.Number))
          {
            ExitState = "NO_AR_ALIASES_FOUND";
          }

          if (export.Ch.IsEmpty && !IsEmpty(export.ChCsePersonsWorkSet.Number))
          {
            ExitState = "SI0000_NO_CH_ALIASES_FOUND";
          }

          if (!export.Ar.IsEmpty && !export.Ap.IsEmpty && !export.Ch.IsEmpty)
          {
            ExitState = "ACO_NI0000_SUCCESSFUL_DISPLAY";
          }
        }

Test2:

        if (AsChar(export.CaseOpen.Flag) == 'N')
        {
          ExitState = "DISPLAY_OK_FOR_CLOSED_CASE";
        }
        else if (AsChar(export.ApActive.Flag) == 'N')
        {
          ExitState = "DISPLAY_OK_FOR_INACTIVE_AP";
        }
        else if (AsChar(export.ChActive.Flag) == 'N')
        {
          ExitState = "SI0000_DISPLAY_OK_INACTIVE_CH";
        }

        export.ApStandard.PageNumber = 1;
        export.ArStandard.PageNumber = 1;
        export.ChStandard.PageNumber = 1;

        if (!export.Ap.IsEmpty)
        {
          for(export.Ap.Index = 0; export.Ap.Index < export.Ap.Count; ++
            export.Ap.Index)
          {
            if (!export.Ap.CheckSize())
            {
              break;
            }

            export.Ap.Update.DetApPrev.Ssn =
              export.Ap.Item.DetApCsePersonsWorkSet.Ssn;
          }

          export.Ap.CheckIndex();
        }

        if (!export.Ar.IsEmpty)
        {
          for(export.Ar.Index = 0; export.Ar.Index < export.Ar.Count; ++
            export.Ar.Index)
          {
            if (!export.Ar.CheckSize())
            {
              break;
            }

            export.Ar.Update.DetArPrev.Ssn =
              export.Ar.Item.DetArCsePersonsWorkSet.Ssn;
          }

          export.Ar.CheckIndex();
        }

        if (!export.Ch.IsEmpty)
        {
          for(export.Ch.Index = 0; export.Ch.Index < export.Ch.Count; ++
            export.Ch.Index)
          {
            if (!export.Ch.CheckSize())
            {
              break;
            }

            export.Ch.Update.DetChPrev.Ssn =
              export.Ch.Item.DetChCsePersonsWorkSet.Ssn;
          }

          export.Ch.CheckIndex();
        }

        if (export.Ap.IsFull)
        {
          export.ApPlus.Text1 = "+";

          export.PageKeysAp.Index = 0;
          export.PageKeysAp.CheckSize();

          export.Ap.Index = 0;
          export.Ap.CheckSize();

          export.PageKeysAp.Update.PageKeyAp.Number =
            export.ApCsePersonsWorkSet.Number;
          export.PageKeysAp.Update.PageKeyAp.UniqueKey =
            export.Ap.Item.DetApCsePersonsWorkSet.UniqueKey;

          ++export.PageKeysAp.Index;
          export.PageKeysAp.CheckSize();

          export.PageKeysAp.Update.PageKeyAp.UniqueKey =
            export.NextKeyAp.UniqueKey;
          export.PageKeysAp.Update.PageKeyAp.Number =
            export.ApCsePersonsWorkSet.Number;
        }

        if (export.Ar.IsFull)
        {
          export.ArPlus.Text1 = "+";

          export.PageKeysAr.Index = 0;
          export.PageKeysAr.CheckSize();

          export.Ar.Index = 0;
          export.Ar.CheckSize();

          export.PageKeysAr.Update.PageKeyAr.Number =
            export.ArCsePersonsWorkSet.Number;
          export.PageKeysAr.Update.PageKeyAr.UniqueKey =
            export.Ar.Item.DetArCsePersonsWorkSet.UniqueKey;

          ++export.PageKeysAr.Index;
          export.PageKeysAr.CheckSize();

          export.PageKeysAr.Update.PageKeyAr.UniqueKey =
            export.NextKeyAr.UniqueKey;
          export.PageKeysAr.Update.PageKeyAr.Number =
            export.ArCsePersonsWorkSet.Number;
        }

        if (export.Ch.IsFull)
        {
          export.ChPlus.Text1 = "+";

          export.PageKeysCh.Index = 0;
          export.PageKeysCh.CheckSize();

          export.Ch.Index = 0;
          export.Ch.CheckSize();

          export.PageKeysCh.Update.PageKeyCh.Number =
            export.ChCsePersonsWorkSet.Number;
          export.PageKeysCh.Update.PageKeyCh.UniqueKey =
            export.Ch.Item.DetChCsePersonsWorkSet.UniqueKey;

          ++export.PageKeysCh.Index;
          export.PageKeysCh.CheckSize();

          export.PageKeysCh.Update.PageKeyCh.UniqueKey =
            export.NextKeyCh.UniqueKey;
          export.PageKeysCh.Update.PageKeyCh.Number =
            export.ChCsePersonsWorkSet.Number;
        }

        export.HiddenNext.Number = export.Next.Number;
        export.HiddenAp.Assign(export.ApCsePersonsWorkSet);

        break;
      case "APNX":
        if (import.PageKeysAp.IsFull)
        {
          ExitState = "MAX_PAGES_REACHED_SYSTEM_ERROR";

          break;
        }

        if (IsEmpty(import.ApPlus.Text1))
        {
          ExitState = "ACO_NE0000_INVALID_FORWARD";

          break;
        }

        ++export.ApStandard.PageNumber;

        export.PageKeysAp.Index = export.ApStandard.PageNumber - 1;
        export.PageKeysAp.CheckSize();

        MoveCsePersonsWorkSet1(export.PageKeysAp.Item.PageKeyAp,
          local.ApCsePersonsWorkSet);

        // ------------------------
        // Show blank page if there
        // are not any more
        // ------------------------
        if (IsEmpty(local.ApCsePersonsWorkSet.UniqueKey))
        {
          export.Ap.Count = 0;
          export.ApPlus.Text1 = "";
          export.ApMinus.Text1 = "-";

          break;
        }

        UseSiAltsBuildAliasAndSsn3();

        if (export.Ap.IsFull)
        {
          if (!IsEmpty(export.NextKeyAp.UniqueKey))
          {
            export.ApPlus.Text1 = "+";

            ++export.PageKeysAp.Index;
            export.PageKeysAp.CheckSize();

            export.PageKeysAp.Update.PageKeyAp.UniqueKey =
              export.NextKeyAp.UniqueKey;
            export.PageKeysAp.Update.PageKeyAp.Number =
              export.ApCsePersonsWorkSet.Number;
          }
        }
        else
        {
          export.ApPlus.Text1 = "";
        }

        if (!export.Ap.IsEmpty)
        {
          for(export.Ap.Index = 0; export.Ap.Index < export.Ap.Count; ++
            export.Ap.Index)
          {
            if (!export.Ap.CheckSize())
            {
              break;
            }

            export.Ap.Update.DetApPrev.Ssn =
              export.Ap.Item.DetApCsePersonsWorkSet.Ssn;
          }

          export.Ap.CheckIndex();
        }

        export.ApMinus.Text1 = "-";

        break;
      case "APPV":
        if (IsEmpty(import.ApMinus.Text1))
        {
          ExitState = "ACO_NE0000_INVALID_BACKWARD";

          break;
        }

        --export.ApStandard.PageNumber;

        export.PageKeysAp.Index = export.ApStandard.PageNumber - 1;
        export.PageKeysAp.CheckSize();

        MoveCsePersonsWorkSet1(export.PageKeysAp.Item.PageKeyAp,
          local.ApCsePersonsWorkSet);
        UseSiAltsBuildAliasAndSsn4();

        if (export.ApStandard.PageNumber > 1)
        {
          export.ApMinus.Text1 = "-";
        }
        else
        {
          export.ApMinus.Text1 = "";
        }

        if (!export.Ap.IsEmpty)
        {
          for(export.Ap.Index = 0; export.Ap.Index < export.Ap.Count; ++
            export.Ap.Index)
          {
            if (!export.Ap.CheckSize())
            {
              break;
            }

            export.Ap.Update.DetApPrev.Ssn =
              export.Ap.Item.DetApCsePersonsWorkSet.Ssn;
          }

          export.Ap.CheckIndex();
        }

        export.ApPlus.Text1 = "+";

        break;
      case "ARNX":
        if (import.PageKeysAr.IsFull)
        {
          ExitState = "MAX_PAGES_REACHED_SYSTEM_ERROR";

          break;
        }

        if (IsEmpty(import.ArPlus.Text1))
        {
          ExitState = "ACO_NE0000_INVALID_FORWARD";

          break;
        }

        ++export.ArStandard.PageNumber;

        export.PageKeysAr.Index = export.ArStandard.PageNumber - 1;
        export.PageKeysAr.CheckSize();

        MoveCsePersonsWorkSet1(export.PageKeysAr.Item.PageKeyAr,
          local.ArCsePersonsWorkSet);

        // ------------------------
        // Show blank page if there
        // are not any more
        // ------------------------
        if (IsEmpty(local.ArCsePersonsWorkSet.UniqueKey))
        {
          export.Ar.Count = 0;
          export.ArPlus.Text1 = "";
          export.ArMinus.Text1 = "-";

          break;
        }

        UseSiAltsBuildAliasAndSsn5();

        if (export.Ar.IsFull)
        {
          if (!IsEmpty(export.NextKeyAr.UniqueKey))
          {
            export.ArPlus.Text1 = "+";

            ++export.PageKeysAr.Index;
            export.PageKeysAr.CheckSize();

            export.PageKeysAr.Update.PageKeyAr.UniqueKey =
              export.NextKeyAr.UniqueKey;
            export.PageKeysAr.Update.PageKeyAr.Number =
              export.ArCsePersonsWorkSet.Number;
          }
        }
        else
        {
          export.ArPlus.Text1 = "";
        }

        if (!export.Ar.IsEmpty)
        {
          for(export.Ar.Index = 0; export.Ar.Index < export.Ar.Count; ++
            export.Ar.Index)
          {
            if (!export.Ar.CheckSize())
            {
              break;
            }

            export.Ar.Update.DetArPrev.Ssn =
              export.Ar.Item.DetArCsePersonsWorkSet.Ssn;
          }

          export.Ar.CheckIndex();
        }

        export.ArMinus.Text1 = "-";

        break;
      case "ARPV":
        if (IsEmpty(import.ArMinus.Text1))
        {
          ExitState = "ACO_NE0000_INVALID_BACKWARD";

          break;
        }

        --export.ArStandard.PageNumber;

        export.PageKeysAr.Index = export.ArStandard.PageNumber - 1;
        export.PageKeysAr.CheckSize();

        MoveCsePersonsWorkSet1(export.PageKeysAr.Item.PageKeyAr,
          local.ArCsePersonsWorkSet);
        UseSiAltsBuildAliasAndSsn6();

        if (export.ArStandard.PageNumber > 1)
        {
          export.ArMinus.Text1 = "-";
        }
        else
        {
          export.ArMinus.Text1 = "";
        }

        if (!export.Ar.IsEmpty)
        {
          for(export.Ar.Index = 0; export.Ar.Index < export.Ar.Count; ++
            export.Ar.Index)
          {
            if (!export.Ar.CheckSize())
            {
              break;
            }

            export.Ar.Update.DetArPrev.Ssn =
              export.Ar.Item.DetArCsePersonsWorkSet.Ssn;
          }

          export.Ar.CheckIndex();
        }

        export.ArPlus.Text1 = "+";

        break;
      case "CHNX":
        if (import.PageKeysCh.IsFull)
        {
          ExitState = "MAX_PAGES_REACHED_SYSTEM_ERROR";

          break;
        }

        if (IsEmpty(import.ChPlus.Text1))
        {
          ExitState = "ACO_NE0000_INVALID_FORWARD";

          break;
        }

        ++export.ChStandard.PageNumber;

        export.PageKeysCh.Index = export.ChStandard.PageNumber - 1;
        export.PageKeysCh.CheckSize();

        MoveCsePersonsWorkSet1(export.PageKeysCh.Item.PageKeyCh,
          local.ChCsePersonsWorkSet);

        // ------------------------
        // Show blank page if there
        // are not any more
        // ------------------------
        if (IsEmpty(local.ChCsePersonsWorkSet.UniqueKey))
        {
          export.Ch.Count = 0;
          export.ChPlus.Text1 = "";
          export.ChMinus.Text1 = "-";

          break;
        }

        UseSiAltsBuildAliasAndSsn8();

        if (export.Ch.IsFull)
        {
          if (!IsEmpty(export.NextKeyCh.UniqueKey))
          {
            export.ChPlus.Text1 = "+";

            ++export.PageKeysCh.Index;
            export.PageKeysCh.CheckSize();

            export.PageKeysCh.Update.PageKeyCh.UniqueKey =
              export.NextKeyCh.UniqueKey;
            export.PageKeysCh.Update.PageKeyCh.Number =
              export.ChCsePersonsWorkSet.Number;
          }
        }
        else
        {
          export.ChPlus.Text1 = "";
        }

        if (!export.Ch.IsEmpty)
        {
          for(export.Ch.Index = 0; export.Ch.Index < export.Ch.Count; ++
            export.Ch.Index)
          {
            if (!export.Ch.CheckSize())
            {
              break;
            }

            export.Ch.Update.DetChPrev.Ssn =
              export.Ch.Item.DetChCsePersonsWorkSet.Ssn;
          }

          export.Ch.CheckIndex();
        }

        export.ChMinus.Text1 = "-";

        break;
      case "CHPV":
        if (IsEmpty(import.ChMinus.Text1))
        {
          ExitState = "ACO_NE0000_INVALID_BACKWARD";

          break;
        }

        --export.ChStandard.PageNumber;

        export.PageKeysCh.Index = export.ChStandard.PageNumber - 1;
        export.PageKeysCh.CheckSize();

        MoveCsePersonsWorkSet1(export.PageKeysCh.Item.PageKeyCh,
          local.ChCsePersonsWorkSet);
        UseSiAltsBuildAliasAndSsn7();

        if (export.ChStandard.PageNumber > 1)
        {
          export.ChMinus.Text1 = "-";
        }
        else
        {
          export.ChMinus.Text1 = "";
        }

        if (!export.Ch.IsEmpty)
        {
          for(export.Ch.Index = 0; export.Ch.Index < export.Ch.Count; ++
            export.Ch.Index)
          {
            if (!export.Ch.CheckSize())
            {
              break;
            }

            export.Ch.Update.DetChPrev.Ssn =
              export.Ch.Item.DetChCsePersonsWorkSet.Ssn;
          }

          export.Ch.CheckIndex();
        }

        export.ChPlus.Text1 = "+";

        break;
      case "ADD":
        if (local.ApCommon.Count == 0 && local.ArCommon.Count == 0 && local
          .ChCommon.Count == 0)
        {
          ExitState = "ACO_NE0000_NO_SELECTION_MADE";

          break;
        }

        export.Ap.Index = 0;

        for(var limit = export.Ap.Count; export.Ap.Index < limit; ++
          export.Ap.Index)
        {
          if (!export.Ap.CheckSize())
          {
            break;
          }

          if (AsChar(export.Ap.Item.DetApCommon.SelectChar) == 'S' && !
            IsEmpty(export.ApCsePersonsWorkSet.Number))
          {
            // ---------------------------------------------------------------
            //              An Organization can not have an alias.
            // ---------------------------------------------------------------
            if (CharAt(export.ApCsePersonsWorkSet.Number, 10) == 'O')
            {
              var field1 = GetField(export.ApCsePersonsWorkSet, "number");

              field1.Error = true;

              var field2 = GetField(export.Ap.Item.DetApCommon, "selectChar");

              field2.Error = true;

              ExitState = "SI0000_NO_ALIAS_FOR_ORGANIZATION";

              goto Test3;
            }

            // -------------------------------------------------------------------------
            // Per WR# 020259, BR#20, the following code is added.
            //                                                        
            // Vithal (05/15/2002)
            // -------------------------------------------------------------------------
            // -----------------------------------------------------------
            //                BR# 20: Start Code
            // -----------------------------------------------------------
            if (IsEmpty(export.ApCsePersonsWorkSet.Sex))
            {
              var field1 =
                GetField(export.ApCsePersonsWorkSet, "formattedName");

              field1.Error = true;

              var field2 = GetField(export.ApCsePersonsWorkSet, "number");

              field2.Error = true;

              ExitState = "SI0000_SEX_MANDATORY";

              goto Test3;
            }

            // -----------------------------------------------------------
            //                BR# 20: End Code
            // -----------------------------------------------------------
            if (AsChar(export.Ap.Item.ApDbOccurrence.Flag) == 'Y')
            {
              var field = GetField(export.Ap.Item.DetApCommon, "selectChar");

              field.Error = true;

              ExitState = "ACO_NE0000_INVALID_ACTION";

              goto Test3;
            }

            if (IsEmpty(export.Ap.Item.DetApCsePersonsWorkSet.FirstName) && IsEmpty
              (export.Ap.Item.DetApCsePersonsWorkSet.LastName))
            {
              // 05/17/01 M.L Start
              export.Ap.Update.DetApCsePersonsWorkSet.Ssn =
                export.Ap.Item.ApSsn3.Text3 + Substring
                (export.Ap.Item.ApSsn2.Text3, WorkArea.Text3_MaxLength, 1, 2) +
                export.Ap.Item.ApSsn4.Text4;

              // 05/17/01 M.L End
              if (IsEmpty(export.Ap.Item.DetApCsePersonsWorkSet.Ssn))
              {
                var field1 =
                  GetField(export.Ap.Item.DetApCsePersonsWorkSet, "firstName");

                field1.Error = true;

                var field2 =
                  GetField(export.Ap.Item.DetApCsePersonsWorkSet, "lastName");

                field2.Error = true;

                // -------------------------------------------------------------------------
                // Per WR# 020259,  the following code is commented. SSN no 
                // longer mandatory.
                //                                                        
                // Vithal (05/15/2002)
                // -------------------------------------------------------------------------
                ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";

                goto Test3;
              }
              else
              {
                export.Ap.Update.DetApCsePersonsWorkSet.FirstName =
                  export.ApCsePersonsWorkSet.FirstName;
                export.Ap.Update.DetApCsePersonsWorkSet.LastName =
                  export.ApCsePersonsWorkSet.LastName;
                export.Ap.Update.DetApCsePersonsWorkSet.MiddleInitial =
                  export.ApCsePersonsWorkSet.MiddleInitial;
              }
            }
            else if (IsEmpty(export.Ap.Item.DetApCsePersonsWorkSet.FirstName) &&
              !IsEmpty(export.Ap.Item.DetApCsePersonsWorkSet.LastName))
            {
              var field =
                GetField(export.Ap.Item.DetApCsePersonsWorkSet, "firstName");

              field.Error = true;

              ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";

              goto Test3;
            }
            else if (!IsEmpty(export.Ap.Item.DetApCsePersonsWorkSet.FirstName) &&
              IsEmpty(export.Ap.Item.DetApCsePersonsWorkSet.LastName))
            {
              var field =
                GetField(export.Ap.Item.DetApCsePersonsWorkSet, "lastName");

              field.Error = true;

              ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";

              goto Test3;
            }

            // -- Validate name.
            UseSiCheckName1();

            if (IsExitState("SI0000_INVALID_NAME"))
            {
              var field1 =
                GetField(export.Ap.Item.DetApCsePersonsWorkSet, "firstName");

              field1.Error = true;

              var field2 =
                GetField(export.Ap.Item.DetApCsePersonsWorkSet, "lastName");

              field2.Error = true;

              var field3 =
                GetField(export.Ap.Item.DetApCsePersonsWorkSet, "middleInitial");
                

              field3.Error = true;

              ExitState = "SI0001_INVALID_NAME";

              goto Test3;
            }

            if (!IsEmpty(export.Ap.Item.ApSsn3.Text3) && !
              IsEmpty(Substring(export.Ap.Item.ApSsn2.Text3, 1, 2)) && !
              IsEmpty(export.Ap.Item.ApSsn4.Text4))
            {
              if (Lt(export.Ap.Item.ApSsn3.Text3, "001") || Lt
                ("899", export.Ap.Item.ApSsn3.Text3) || Equal
                (export.Ap.Item.ApSsn3.Text3, "666"))
              {
                var field = GetField(export.Ap.Item.ApSsn3, "text3");

                field.Error = true;

                ExitState = "ACO_NE0000_INVALID_SSN3";

                goto Test3;
              }

              if (Lt(export.Ap.Item.ApSsn2.Text3, "01") || Lt
                ("99", export.Ap.Item.ApSsn2.Text3))
              {
                var field = GetField(export.Ap.Item.ApSsn2, "text3");

                field.Error = true;

                ExitState = "ACO_NE0000_INVALID_SSN2";

                goto Test3;
              }

              if (Lt(export.Ap.Item.ApSsn4.Text4, "0001") || Lt
                ("9999", export.Ap.Item.ApSsn4.Text4))
              {
                var field = GetField(export.Ap.Item.ApSsn4, "text4");

                field.Error = true;

                ExitState = "ACO_NE0000_INVALID_SSN4";

                goto Test3;
              }

              export.Ap.Update.DetApCsePersonsWorkSet.Ssn =
                export.Ap.Item.ApSsn3.Text3 + Substring
                (export.Ap.Item.ApSsn2.Text3, WorkArea.Text3_MaxLength, 1, 2) +
                export.Ap.Item.ApSsn4.Text4;

              // 02/21/08   LSS   Added length check on ssn
              if (Length(TrimEnd(export.Ap.Item.DetApCsePersonsWorkSet.Ssn)) < 9
                )
              {
                var field1 = GetField(export.Ap.Item.ApSsn3, "text3");

                field1.Error = true;

                var field2 = GetField(export.Ap.Item.ApSsn2, "text3");

                field2.Error = true;

                var field3 = GetField(export.Ap.Item.ApSsn4, "text4");

                field3.Error = true;

                ExitState = "SSN_NOT_COMPLETE";

                goto Test3;
              }

              // 02/21/08   LSS   Added verify on ssn
              if (Verify(export.Ap.Item.DetApCsePersonsWorkSet.Ssn, "0123456789")
                != 0 && !IsEmpty(export.Ap.Item.DetApCsePersonsWorkSet.Ssn))
              {
                var field1 = GetField(export.Ap.Item.ApSsn3, "text3");

                field1.Error = true;

                var field2 = GetField(export.Ap.Item.ApSsn2, "text3");

                field2.Error = true;

                var field3 = GetField(export.Ap.Item.ApSsn4, "text4");

                field3.Error = true;

                ExitState = "LE0000_SSN_CONTAINS_NONNUM";

                goto Test3;
              }

              if (!IsEmpty(export.Ap.Item.DetApCsePersonsWorkSet.Ssn))
              {
                local.Check.SsnNum9 =
                  (int)StringToNumber(export.Ap.Item.DetApCsePersonsWorkSet.Ssn);
                  

                if (ReadInvalidSsn1())
                {
                  var field1 = GetField(export.ApCsePersonsWorkSet, "number");

                  field1.Error = true;

                  var field2 = GetField(export.Ap.Item.ApSsn3, "text3");

                  field2.Error = true;

                  var field3 = GetField(export.Ap.Item.ApSsn2, "text3");

                  field3.Error = true;

                  var field4 = GetField(export.Ap.Item.ApSsn4, "text4");

                  field4.Error = true;

                  ExitState = "INVALID_SSN";

                  goto Test3;
                }
                else
                {
                  // this is fine, the cse person number and ssn combination 
                  // being added is not on
                  // the invalid ssn table
                  export.Ap.Update.DetApPrev.Ssn =
                    export.Ap.Item.DetApCsePersonsWorkSet.Ssn;
                }
              }
            }
            else
            {
              // -------------------------------------------------------------------------
              // Per WR# 020259, BR#16, the following code is added.
              //                                                        
              // Vithal (05/15/2002)
              // -------------------------------------------------------------------------
              // -----------------------------------------------------------
              //                BR# 16: Start Code
              // -----------------------------------------------------------
              export.Ap.Update.DetApCsePersonsWorkSet.Ssn = "000000000";

              // -----------------------------------------------------------
              //                BR# 16: End Code
              // -----------------------------------------------------------
            }

            MoveCsePersonsWorkSet2(export.Ap.Item.DetApCsePersonsWorkSet,
              local.CsePersonsWorkSet);
            local.CsePersonsWorkSet.Number = export.ApCsePersonsWorkSet.Number;
            local.CsePersonsWorkSet.Dob = export.ApCsePersonsWorkSet.Dob;
            local.CsePersonsWorkSet.Sex = export.ApCsePersonsWorkSet.Sex;
            UseSiAltsCabCreateAlias();

            if (IsExitState("ACO_NN0000_ALL_OK") || IsExitState
              ("ACO_NI0000_SUCCESSFUL_ADD"))
            {
            }
            else
            {
              goto Test3;
            }

            export.Ap.Update.DetApCommon.SelectChar = "*";
            export.Ap.Update.DetApCsePersonsWorkSet.UniqueKey =
              local.CsePersonsWorkSet.UniqueKey;
            export.Ap.Update.ApDbOccurrence.Flag = "Y";
            ExitState = "ACO_NI0000_SUCCESSFUL_ADD";
          }
        }

        export.Ap.CheckIndex();
        export.Ar.Index = 0;

        for(var limit = export.Ar.Count; export.Ar.Index < limit; ++
          export.Ar.Index)
        {
          if (!export.Ar.CheckSize())
          {
            break;
          }

          if (AsChar(export.Ar.Item.DetArCommon.SelectChar) == 'S' && !
            IsEmpty(export.ArCsePersonsWorkSet.Number))
          {
            // ---------------------------------------------------------------
            //              An Organization can not have an alias.
            // ---------------------------------------------------------------
            if (CharAt(export.ArCsePersonsWorkSet.Number, 10) == 'O')
            {
              var field1 = GetField(export.ArCsePersonsWorkSet, "number");

              field1.Error = true;

              var field2 = GetField(export.Ar.Item.DetArCommon, "selectChar");

              field2.Error = true;

              ExitState = "SI0000_NO_ALIAS_FOR_ORGANIZATION";

              goto Test3;
            }

            // -------------------------------------------------------------------------
            // Per WR# 020259, BR#20, the following code is added.
            //                                                        
            // Vithal (05/15/2002)
            // -------------------------------------------------------------------------
            // -----------------------------------------------------------
            //                BR# 20: Start Code
            // -----------------------------------------------------------
            if (IsEmpty(export.ArCsePersonsWorkSet.Sex))
            {
              var field1 =
                GetField(export.ArCsePersonsWorkSet, "formattedName");

              field1.Error = true;

              var field2 = GetField(export.ArCsePersonsWorkSet, "number");

              field2.Error = true;

              ExitState = "SI0000_SEX_MANDATORY";

              goto Test3;
            }

            // -----------------------------------------------------------
            //                BR# 20: End Code
            // -----------------------------------------------------------
            if (AsChar(export.Ar.Item.ArDbOccurrence.Flag) == 'Y')
            {
              var field = GetField(export.Ar.Item.DetArCommon, "selectChar");

              field.Error = true;

              ExitState = "ACO_NE0000_INVALID_ACTION";

              goto Test3;
            }

            if (IsEmpty(export.Ar.Item.DetArCsePersonsWorkSet.FirstName) && IsEmpty
              (export.Ar.Item.DetArCsePersonsWorkSet.LastName))
            {
              // 05/17/01 M.L Start
              export.Ar.Update.DetArCsePersonsWorkSet.Ssn =
                export.Ar.Item.ArSsn3.Text3 + Substring
                (export.Ar.Item.ArSsn2.Text3, WorkArea.Text3_MaxLength, 1, 2) +
                export.Ar.Item.ArSsn4.Text4;

              // 05/17/01 M.L End
              if (IsEmpty(export.Ar.Item.DetArCsePersonsWorkSet.Ssn))
              {
                var field1 =
                  GetField(export.Ar.Item.DetArCsePersonsWorkSet, "firstName");

                field1.Error = true;

                var field2 =
                  GetField(export.Ar.Item.DetArCsePersonsWorkSet, "lastName");

                field2.Error = true;

                // -------------------------------------------------------------------------
                // Per WR# 020259,  the following code is commented. SSN no 
                // longer mandatory.
                //                                                        
                // Vithal (05/15/2002)
                // -------------------------------------------------------------------------
                ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";

                goto Test3;
              }
              else
              {
                export.Ar.Update.DetArCsePersonsWorkSet.FirstName =
                  export.ArCsePersonsWorkSet.FirstName;
                export.Ar.Update.DetArCsePersonsWorkSet.LastName =
                  export.ArCsePersonsWorkSet.LastName;
              }
            }
            else if (IsEmpty(export.Ar.Item.DetArCsePersonsWorkSet.FirstName) &&
              !IsEmpty(export.Ar.Item.DetArCsePersonsWorkSet.LastName))
            {
              var field =
                GetField(export.Ar.Item.DetArCsePersonsWorkSet, "firstName");

              field.Error = true;

              ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";

              goto Test3;
            }
            else if (!IsEmpty(export.Ar.Item.DetArCsePersonsWorkSet.FirstName) &&
              IsEmpty(export.Ar.Item.DetArCsePersonsWorkSet.LastName))
            {
              var field =
                GetField(export.Ar.Item.DetArCsePersonsWorkSet, "lastName");

              field.Error = true;

              ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";

              goto Test3;
            }

            // -- Validate name.
            UseSiCheckName2();

            if (IsExitState("SI0000_INVALID_NAME"))
            {
              var field1 =
                GetField(export.Ar.Item.DetArCsePersonsWorkSet, "firstName");

              field1.Error = true;

              var field2 =
                GetField(export.Ar.Item.DetArCsePersonsWorkSet, "lastName");

              field2.Error = true;

              var field3 =
                GetField(export.Ar.Item.DetArCsePersonsWorkSet, "middleInitial");
                

              field3.Error = true;

              ExitState = "SI0001_INVALID_NAME";

              goto Test3;
            }

            if (!IsEmpty(export.Ar.Item.ArSsn3.Text3) && !
              IsEmpty(Substring(export.Ar.Item.ArSsn2.Text3, 1, 2)) && !
              IsEmpty(export.Ar.Item.ArSsn4.Text4))
            {
              if (Lt(export.Ar.Item.ArSsn3.Text3, "001") || Lt
                ("899", export.Ar.Item.ArSsn3.Text3) || Equal
                (export.Ar.Item.ArSsn3.Text3, "666"))
              {
                var field = GetField(export.Ar.Item.ArSsn3, "text3");

                field.Error = true;

                ExitState = "ACO_NE0000_INVALID_SSN3";

                goto Test3;
              }

              if (Lt(export.Ar.Item.ArSsn2.Text3, "01") || Lt
                ("99", export.Ar.Item.ArSsn2.Text3))
              {
                var field = GetField(export.Ar.Item.ArSsn2, "text3");

                field.Error = true;

                ExitState = "ACO_NE0000_INVALID_SSN2";

                goto Test3;
              }

              if (Lt(export.Ar.Item.ArSsn4.Text4, "0001") || Lt
                ("9999", export.Ar.Item.ArSsn4.Text4))
              {
                var field = GetField(export.Ar.Item.ArSsn4, "text4");

                field.Error = true;

                ExitState = "ACO_NE0000_INVALID_SSN4";

                goto Test3;
              }

              export.Ar.Update.DetArCsePersonsWorkSet.Ssn =
                export.Ar.Item.ArSsn3.Text3 + Substring
                (export.Ar.Item.ArSsn2.Text3, WorkArea.Text3_MaxLength, 1, 2) +
                export.Ar.Item.ArSsn4.Text4;

              // 02/21/08   LSS   Added length check on ssn
              if (Length(TrimEnd(export.Ar.Item.DetArCsePersonsWorkSet.Ssn)) < 9
                )
              {
                var field1 = GetField(export.Ar.Item.ArSsn3, "text3");

                field1.Error = true;

                var field2 = GetField(export.Ar.Item.ArSsn2, "text3");

                field2.Error = true;

                var field3 = GetField(export.Ar.Item.ArSsn4, "text4");

                field3.Error = true;

                ExitState = "SSN_NOT_COMPLETE";

                goto Test3;
              }

              // 02/21/08   LSS   Added verify on ssn
              if (Verify(export.Ar.Item.DetArCsePersonsWorkSet.Ssn, "0123456789")
                != 0 && !IsEmpty(export.Ar.Item.DetArCsePersonsWorkSet.Ssn))
              {
                var field1 = GetField(export.Ar.Item.ArSsn3, "text3");

                field1.Error = true;

                var field2 = GetField(export.Ar.Item.ArSsn2, "text3");

                field2.Error = true;

                var field3 = GetField(export.Ar.Item.ArSsn4, "text4");

                field3.Error = true;

                ExitState = "LE0000_SSN_CONTAINS_NONNUM";

                goto Test3;
              }

              if (!IsEmpty(export.Ar.Item.DetArCsePersonsWorkSet.Ssn))
              {
                local.Check.SsnNum9 =
                  (int)StringToNumber(export.Ar.Item.DetArCsePersonsWorkSet.Ssn);
                  

                if (ReadInvalidSsn2())
                {
                  var field1 = GetField(export.ArCsePersonsWorkSet, "number");

                  field1.Error = true;

                  var field2 = GetField(export.Ar.Item.ArSsn3, "text3");

                  field2.Error = true;

                  var field3 = GetField(export.Ar.Item.ArSsn2, "text3");

                  field3.Error = true;

                  var field4 = GetField(export.Ar.Item.ArSsn4, "text4");

                  field4.Error = true;

                  ExitState = "INVALID_SSN";

                  goto Test3;
                }
                else
                {
                  // this is fine, the cse person number and ssn combination 
                  // being added is not on
                  // the invalid ssn table
                  export.Ar.Update.DetArPrev.Ssn =
                    export.Ar.Item.DetArCsePersonsWorkSet.Ssn;
                }
              }
            }
            else
            {
              // -------------------------------------------------------------------------
              // Per WR# 020259, BR#16, the following code is added.
              //                                                        
              // Vithal (05/15/2002)
              // -------------------------------------------------------------------------
              // -----------------------------------------------------------
              //                BR# 16: Start Code
              // -----------------------------------------------------------
              export.Ar.Update.DetArCsePersonsWorkSet.Ssn = "000000000";

              // -----------------------------------------------------------
              //                BR# 16: End Code
              // -----------------------------------------------------------
            }

            MoveCsePersonsWorkSet2(export.Ar.Item.DetArCsePersonsWorkSet,
              local.CsePersonsWorkSet);
            local.CsePersonsWorkSet.Number = export.ArCsePersonsWorkSet.Number;
            local.CsePersonsWorkSet.Dob = export.ArCsePersonsWorkSet.Dob;
            local.CsePersonsWorkSet.Sex = export.ArCsePersonsWorkSet.Sex;
            UseSiAltsCabCreateAlias();

            if (IsExitState("ACO_NN0000_ALL_OK") || IsExitState
              ("ACO_NI0000_SUCCESSFUL_ADD"))
            {
            }
            else
            {
              goto Test3;
            }

            export.Ar.Update.DetArCommon.SelectChar = "*";
            export.Ar.Update.DetArCsePersonsWorkSet.UniqueKey =
              local.CsePersonsWorkSet.UniqueKey;
            export.Ar.Update.ArDbOccurrence.Flag = "Y";
            ExitState = "ACO_NI0000_SUCCESSFUL_ADD";
          }
        }

        export.Ar.CheckIndex();

        // -------------------------------------------------------------------------
        // Per WR# 020259,  the following code is added.
        //                                                        
        // Vithal (05/15/2002)
        // -------------------------------------------------------------------------
        // ---------------------------------------------
        //                     Start Code
        // ---------------------------------------------
        export.Ch.Index = 0;

        for(var limit = export.Ch.Count; export.Ch.Index < limit; ++
          export.Ch.Index)
        {
          if (!export.Ch.CheckSize())
          {
            break;
          }

          if (AsChar(export.Ch.Item.DetChCommon.SelectChar) == 'S' && !
            IsEmpty(export.ChCsePersonsWorkSet.Number))
          {
            // ---------------------------------------------------------------
            //              An Organization can not have an alias.
            // ---------------------------------------------------------------
            if (CharAt(export.ChCsePersonsWorkSet.Number, 10) == 'O')
            {
              var field1 = GetField(export.ChCsePersonsWorkSet, "number");

              field1.Error = true;

              var field2 = GetField(export.Ch.Item.DetChCommon, "selectChar");

              field2.Error = true;

              ExitState = "SI0000_NO_ALIAS_FOR_ORGANIZATION";

              goto Test3;
            }

            if (IsEmpty(export.ChCsePersonsWorkSet.Sex))
            {
              var field1 =
                GetField(export.ChCsePersonsWorkSet, "formattedName");

              field1.Error = true;

              var field2 = GetField(export.ChCsePersonsWorkSet, "number");

              field2.Error = true;

              ExitState = "SI0000_SEX_MANDATORY";

              goto Test3;
            }

            if (AsChar(export.Ch.Item.ChDbOccurrence.Flag) == 'Y')
            {
              var field = GetField(export.Ch.Item.DetChCommon, "selectChar");

              field.Error = true;

              ExitState = "ACO_NE0000_INVALID_ACTION";

              goto Test3;
            }

            if (IsEmpty(export.Ch.Item.DetChCsePersonsWorkSet.FirstName) && IsEmpty
              (export.Ch.Item.DetChCsePersonsWorkSet.LastName))
            {
              export.Ch.Update.DetChCsePersonsWorkSet.Ssn =
                export.Ch.Item.ChSsn3.Text3 + Substring
                (export.Ch.Item.ChSsn2.Text3, WorkArea.Text3_MaxLength, 1, 2) +
                export.Ch.Item.ChSsn4.Text4;

              if (IsEmpty(export.Ch.Item.DetChCsePersonsWorkSet.Ssn))
              {
                var field1 =
                  GetField(export.Ch.Item.DetChCsePersonsWorkSet, "firstName");

                field1.Error = true;

                var field2 =
                  GetField(export.Ch.Item.DetChCsePersonsWorkSet, "lastName");

                field2.Error = true;

                // -------------------------------------------------------------------------
                // Per WR# 020259,  the following code is commented. SSN no 
                // longer mandatory.
                //                                                        
                // Vithal (05/15/2002)
                // -------------------------------------------------------------------------
                ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";

                goto Test3;
              }
              else
              {
                export.Ch.Update.DetChCsePersonsWorkSet.FirstName =
                  export.ChCsePersonsWorkSet.FirstName;
                export.Ch.Update.DetChCsePersonsWorkSet.LastName =
                  export.ChCsePersonsWorkSet.LastName;
              }
            }
            else if (IsEmpty(export.Ch.Item.DetChCsePersonsWorkSet.FirstName) &&
              !IsEmpty(export.Ch.Item.DetChCsePersonsWorkSet.LastName))
            {
              var field =
                GetField(export.Ch.Item.DetChCsePersonsWorkSet, "firstName");

              field.Error = true;

              ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";

              goto Test3;
            }
            else if (!IsEmpty(export.Ch.Item.DetChCsePersonsWorkSet.FirstName) &&
              IsEmpty(export.Ch.Item.DetChCsePersonsWorkSet.LastName))
            {
              var field =
                GetField(export.Ch.Item.DetChCsePersonsWorkSet, "lastName");

              field.Error = true;

              ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";

              goto Test3;
            }

            // -- Validate name.
            UseSiCheckName3();

            if (IsExitState("SI0000_INVALID_NAME"))
            {
              var field1 =
                GetField(export.Ch.Item.DetChCsePersonsWorkSet, "firstName");

              field1.Error = true;

              var field2 =
                GetField(export.Ch.Item.DetChCsePersonsWorkSet, "lastName");

              field2.Error = true;

              var field3 =
                GetField(export.Ch.Item.DetChCsePersonsWorkSet, "middleInitial");
                

              field3.Error = true;

              ExitState = "SI0001_INVALID_NAME";

              goto Test3;
            }

            if (!IsEmpty(export.Ch.Item.ChSsn3.Text3) && !
              IsEmpty(Substring(export.Ch.Item.ChSsn2.Text3, 1, 2)) && !
              IsEmpty(export.Ch.Item.ChSsn4.Text4))
            {
              if (Lt(export.Ch.Item.ChSsn3.Text3, "001") || Lt
                ("899", export.Ch.Item.ChSsn3.Text3) || Equal
                (export.Ch.Item.ChSsn3.Text3, "666"))
              {
                var field = GetField(export.Ch.Item.ChSsn3, "text3");

                field.Error = true;

                ExitState = "ACO_NE0000_INVALID_SSN3";

                goto Test3;
              }

              if (Lt(export.Ch.Item.ChSsn2.Text3, "01") || Lt
                ("99", export.Ch.Item.ChSsn2.Text3))
              {
                var field = GetField(export.Ch.Item.ChSsn2, "text3");

                field.Error = true;

                ExitState = "ACO_NE0000_INVALID_SSN2";

                goto Test3;
              }

              if (Lt(export.Ch.Item.ChSsn4.Text4, "0001") || Lt
                ("9999", export.Ch.Item.ChSsn4.Text4))
              {
                var field = GetField(export.Ch.Item.ChSsn4, "text4");

                field.Error = true;

                ExitState = "ACO_NE0000_INVALID_SSN4";

                goto Test3;
              }

              export.Ch.Update.DetChCsePersonsWorkSet.Ssn =
                export.Ch.Item.ChSsn3.Text3 + Substring
                (export.Ch.Item.ChSsn2.Text3, WorkArea.Text3_MaxLength, 1, 2) +
                export.Ch.Item.ChSsn4.Text4;

              // 02/21/08   LSS   Added length check on ssn
              if (Length(TrimEnd(export.Ch.Item.DetChCsePersonsWorkSet.Ssn)) < 9
                )
              {
                var field1 = GetField(export.Ch.Item.ChSsn3, "text3");

                field1.Error = true;

                var field2 = GetField(export.Ch.Item.ChSsn2, "text3");

                field2.Error = true;

                var field3 = GetField(export.Ch.Item.ChSsn4, "text4");

                field3.Error = true;

                ExitState = "SSN_NOT_COMPLETE";

                goto Test3;
              }

              // 02/21/08   LSS   Added verify on ssn
              if (Verify(export.Ch.Item.DetChCsePersonsWorkSet.Ssn, "0123456789")
                != 0 && !IsEmpty(export.Ch.Item.DetChCsePersonsWorkSet.Ssn))
              {
                var field1 = GetField(export.Ch.Item.ChSsn3, "text3");

                field1.Error = true;

                var field2 = GetField(export.Ch.Item.ChSsn2, "text3");

                field2.Error = true;

                var field3 = GetField(export.Ch.Item.ChSsn4, "text4");

                field3.Error = true;

                ExitState = "LE0000_SSN_CONTAINS_NONNUM";

                goto Test3;
              }

              if (!IsEmpty(export.Ch.Item.DetChCsePersonsWorkSet.Ssn))
              {
                local.Check.SsnNum9 =
                  (int)StringToNumber(export.Ch.Item.DetChCsePersonsWorkSet.Ssn);
                  

                if (ReadInvalidSsn3())
                {
                  var field1 = GetField(export.ChCsePersonsWorkSet, "number");

                  field1.Error = true;

                  var field2 = GetField(export.Ch.Item.ChSsn3, "text3");

                  field2.Error = true;

                  var field3 = GetField(export.Ch.Item.ChSsn2, "text3");

                  field3.Error = true;

                  var field4 = GetField(export.Ch.Item.ChSsn4, "text4");

                  field4.Error = true;

                  ExitState = "INVALID_SSN";

                  goto Test3;
                }
                else
                {
                  // this is fine, the cse person number and ssn combination 
                  // being added is not on
                  // the invalid ssn table
                  export.Ch.Update.DetChPrev.Ssn =
                    export.Ch.Item.DetChCsePersonsWorkSet.Ssn;
                }
              }
            }
            else
            {
              // -------------------------------------------------------------------------
              // Per WR# 020259, BR#16, the following code is added.
              //                                                        
              // Vithal (05/15/2002)
              // -------------------------------------------------------------------------
              // -----------------------------------------------------------
              //                BR# 16: Start Code
              // -----------------------------------------------------------
              export.Ch.Update.DetChCsePersonsWorkSet.Ssn = "000000000";

              // -----------------------------------------------------------
              //                BR# 16: End Code
              // -----------------------------------------------------------
            }

            MoveCsePersonsWorkSet2(export.Ch.Item.DetChCsePersonsWorkSet,
              local.CsePersonsWorkSet);
            local.CsePersonsWorkSet.Number = export.ChCsePersonsWorkSet.Number;
            local.CsePersonsWorkSet.Dob = export.ChCsePersonsWorkSet.Dob;
            local.CsePersonsWorkSet.Sex = export.ChCsePersonsWorkSet.Sex;
            UseSiAltsCabCreateAlias();

            if (IsExitState("ACO_NN0000_ALL_OK") || IsExitState
              ("ACO_NI0000_SUCCESSFUL_ADD"))
            {
            }
            else
            {
              goto Test3;
            }

            export.Ch.Update.DetChCommon.SelectChar = "*";
            export.Ch.Update.DetChCsePersonsWorkSet.UniqueKey =
              local.CsePersonsWorkSet.UniqueKey;
            export.Ch.Update.ChDbOccurrence.Flag = "Y";
            ExitState = "ACO_NI0000_SUCCESSFUL_ADD";
          }
          else if (AsChar(export.Ch.Item.DetChCommon.SelectChar) == 'S' && IsEmpty
            (export.ChCsePersonsWorkSet.Number))
          {
            var field1 = GetField(export.ChCsePersonsWorkSet, "number");

            field1.Error = true;

            var field2 = GetField(export.Ch.Item.DetChCommon, "selectChar");

            field2.Error = true;

            ExitState = "ACO_NE0000_INVALID_ACTION";

            goto Test3;
          }
        }

        export.Ch.CheckIndex();

        // ----------------------------------------------
        //                 End Code
        // ----------------------------------------------
        if (IsExitState("ACO_NI0000_SUCCESSFUL_ADD"))
        {
          export.Ap.Count = 0;
          export.Ar.Count = 0;
          export.Ch.Count = 0;
          export.ApPlus.Text1 = "";
          export.ApMinus.Text1 = "";
          export.ArPlus.Text1 = "";
          export.ArMinus.Text1 = "";
          export.ChPlus.Text1 = "";
          export.ChMinus.Text1 = "";
          local.ApCsePersonsWorkSet.Number = export.ApCsePersonsWorkSet.Number;
          local.ArCsePersonsWorkSet.Number = export.ArCsePersonsWorkSet.Number;
          local.ChCsePersonsWorkSet.Number = export.ChCsePersonsWorkSet.Number;
          UseSiAltsBuildAliasAndSsn1();
          export.ApStandard.PageNumber = 1;
          export.ArStandard.PageNumber = 1;
          export.ChStandard.PageNumber = 1;

          if (export.Ap.IsFull)
          {
            export.ApPlus.Text1 = "+";

            export.PageKeysAp.Index = 0;
            export.PageKeysAp.CheckSize();

            export.Ap.Index = 0;
            export.Ap.CheckSize();

            export.PageKeysAp.Update.PageKeyAp.Number =
              export.ApCsePersonsWorkSet.Number;
            export.PageKeysAp.Update.PageKeyAp.UniqueKey =
              export.Ap.Item.DetApCsePersonsWorkSet.UniqueKey;

            ++export.PageKeysAp.Index;
            export.PageKeysAp.CheckSize();

            export.PageKeysAp.Update.PageKeyAp.UniqueKey =
              export.NextKeyAp.UniqueKey;
            export.PageKeysAp.Update.PageKeyAp.Number =
              export.ApCsePersonsWorkSet.Number;
          }

          if (export.Ar.IsFull)
          {
            export.ArPlus.Text1 = "+";

            export.PageKeysAr.Index = 0;
            export.PageKeysAr.CheckSize();

            export.Ar.Index = 0;
            export.Ar.CheckSize();

            export.PageKeysAr.Update.PageKeyAr.Number =
              export.ArCsePersonsWorkSet.Number;
            export.PageKeysAr.Update.PageKeyAr.UniqueKey =
              export.Ar.Item.DetArCsePersonsWorkSet.UniqueKey;

            ++export.PageKeysAr.Index;
            export.PageKeysAr.CheckSize();

            export.PageKeysAr.Update.PageKeyAr.UniqueKey =
              export.NextKeyAr.UniqueKey;
            export.PageKeysAr.Update.PageKeyAr.Number =
              export.ArCsePersonsWorkSet.Number;
          }

          if (export.Ch.IsFull)
          {
            export.ChPlus.Text1 = "+";

            export.PageKeysCh.Index = 0;
            export.PageKeysCh.CheckSize();

            export.Ch.Index = 0;
            export.Ch.CheckSize();

            export.PageKeysCh.Update.PageKeyCh.Number =
              export.ChCsePersonsWorkSet.Number;
            export.PageKeysCh.Update.PageKeyCh.UniqueKey =
              export.Ch.Item.DetChCsePersonsWorkSet.UniqueKey;

            ++export.PageKeysCh.Index;
            export.PageKeysCh.CheckSize();

            export.PageKeysCh.Update.PageKeyCh.UniqueKey =
              export.NextKeyCh.UniqueKey;
            export.PageKeysCh.Update.PageKeyCh.Number =
              export.ChCsePersonsWorkSet.Number;
          }
        }

        break;
      case "DELETE":
        if (local.ApCommon.Count == 0 && local.ArCommon.Count == 0 && local
          .ChCommon.Count == 0)
        {
          ExitState = "ACO_NE0000_NO_SELECTION_MADE";

          break;
        }

        export.Ap.Index = 0;

        for(var limit = export.Ap.Count; export.Ap.Index < limit; ++
          export.Ap.Index)
        {
          if (!export.Ap.CheckSize())
          {
            break;
          }

          if (AsChar(export.Ap.Item.DetApCommon.SelectChar) == 'S')
          {
            if (AsChar(export.Ap.Item.ApDbOccurrence.Flag) != 'Y')
            {
              var field = GetField(export.Ap.Item.DetApCommon, "selectChar");

              field.Error = true;

              ExitState = "ACO_NE0000_INVALID_ACTION";

              goto Test3;
            }

            if (AsChar(export.Ap.Item.ApAe.Flag) == 'Y' || AsChar
              (export.Ap.Item.ApKscares.Flag) == 'Y' || AsChar
              (export.Ap.Item.ApKanpay.Flag) == 'Y' || AsChar
              (export.Ap.Item.ApFa.Flag) == 'Y')
            {
              var field = GetField(export.Ap.Item.DetApCommon, "selectChar");

              field.Error = true;

              ExitState = "ACO_ADABAS_DELETE_ERROR";

              goto Test3;
            }

            MoveCsePersonsWorkSet2(export.Ap.Item.DetApCsePersonsWorkSet,
              local.CsePersonsWorkSet);
            local.CsePersonsWorkSet.Number = export.ApCsePersonsWorkSet.Number;
            UseSiAltsCabDeleteAlias();

            if (IsExitState("ACO_NN0000_ALL_OK") || IsExitState
              ("ACO_NI0000_SUCCESSFUL_DELETE"))
            {
            }
            else
            {
              goto Test3;
            }

            export.Ap.Update.DetApCommon.SelectChar = "";
            export.Ap.Update.ApDbOccurrence.Flag = "";
            export.Ap.Update.DetApCsePersonsWorkSet.Assign(
              local.CsePersonsWorkSet);
            export.Ap.Update.ApSsn3.Text3 = local.Ssn3.Text3;
            export.Ap.Update.ApSsn2.Text3 = local.Ssn2.Text3;
            export.Ap.Update.ApSsn4.Text4 = local.Ssn4.Text4;
            ExitState = "ACO_NI0000_SUCCESSFUL_DELETE";
          }
        }

        export.Ap.CheckIndex();
        export.Ar.Index = 0;

        for(var limit = export.Ar.Count; export.Ar.Index < limit; ++
          export.Ar.Index)
        {
          if (!export.Ar.CheckSize())
          {
            break;
          }

          if (AsChar(export.Ar.Item.DetArCommon.SelectChar) == 'S')
          {
            if (AsChar(export.Ar.Item.ArDbOccurrence.Flag) != 'Y')
            {
              var field = GetField(export.Ar.Item.DetArCommon, "selectChar");

              field.Error = true;

              ExitState = "ACO_NE0000_INVALID_ACTION";

              goto Test3;
            }

            if (AsChar(export.Ar.Item.ArAe.Flag) == 'Y' || AsChar
              (export.Ar.Item.ArKscares.Flag) == 'Y' || AsChar
              (export.Ar.Item.ArKanpay.Flag) == 'Y' || AsChar
              (export.Ar.Item.ArFa.Flag) == 'Y')
            {
              var field = GetField(export.Ar.Item.DetArCommon, "selectChar");

              field.Error = true;

              ExitState = "ACO_ADABAS_DELETE_ERROR";

              goto Test3;
            }

            MoveCsePersonsWorkSet2(export.Ar.Item.DetArCsePersonsWorkSet,
              local.CsePersonsWorkSet);
            local.CsePersonsWorkSet.Number = export.ArCsePersonsWorkSet.Number;
            UseSiAltsCabDeleteAlias();

            if (IsExitState("ACO_NN0000_ALL_OK") || IsExitState
              ("ACO_NI0000_SUCCESSFUL_DELETE"))
            {
            }
            else
            {
              goto Test3;
            }

            export.Ar.Update.DetArCommon.SelectChar = "";
            export.Ar.Update.ArDbOccurrence.Flag = "";
            export.Ar.Update.DetArCsePersonsWorkSet.Assign(
              local.CsePersonsWorkSet);
            export.Ar.Update.ArSsn3.Text3 = local.Ssn3.Text3;
            export.Ar.Update.ArSsn2.Text3 = local.Ssn2.Text3;
            export.Ar.Update.ArSsn4.Text4 = local.Ssn4.Text4;
            ExitState = "ACO_NI0000_SUCCESSFUL_DELETE";
          }
        }

        export.Ar.CheckIndex();

        // -------------------------------------------------------------------------
        // Per WR# 020259,  the following code is added.
        //                                                        
        // Vithal (05/15/2002)
        // -------------------------------------------------------------------------
        // ---------------------------------------------
        //                     Start Code
        // ---------------------------------------------
        export.Ch.Index = 0;

        for(var limit = export.Ch.Count; export.Ch.Index < limit; ++
          export.Ch.Index)
        {
          if (!export.Ch.CheckSize())
          {
            break;
          }

          if (AsChar(export.Ch.Item.DetChCommon.SelectChar) == 'S')
          {
            if (AsChar(export.Ch.Item.ChDbOccurrence.Flag) != 'Y')
            {
              var field = GetField(export.Ch.Item.DetChCommon, "selectChar");

              field.Error = true;

              ExitState = "ACO_NE0000_INVALID_ACTION";

              goto Test3;
            }

            if (AsChar(export.Ch.Item.ChAe.Flag) == 'Y' || AsChar
              (export.Ch.Item.ChKscares.Flag) == 'Y' || AsChar
              (export.Ch.Item.ChKanpay.Flag) == 'Y' || AsChar
              (export.Ch.Item.ChFa.Flag) == 'Y')
            {
              var field = GetField(export.Ch.Item.DetChCommon, "selectChar");

              field.Error = true;

              ExitState = "ACO_ADABAS_DELETE_ERROR";

              goto Test3;
            }

            MoveCsePersonsWorkSet2(export.Ch.Item.DetChCsePersonsWorkSet,
              local.CsePersonsWorkSet);
            local.CsePersonsWorkSet.Number = export.ChCsePersonsWorkSet.Number;
            UseSiAltsCabDeleteAlias();

            if (IsExitState("ACO_NN0000_ALL_OK") || IsExitState
              ("ACO_NI0000_SUCCESSFUL_DELETE"))
            {
            }
            else
            {
              goto Test3;
            }

            export.Ch.Update.DetChCommon.SelectChar = "";
            export.Ch.Update.ChDbOccurrence.Flag = "";
            export.Ch.Update.DetChCsePersonsWorkSet.Assign(
              local.CsePersonsWorkSet);
            export.Ch.Update.ChSsn3.Text3 = local.Ssn3.Text3;
            export.Ch.Update.ChSsn2.Text3 = local.Ssn2.Text3;
            export.Ch.Update.ChSsn4.Text4 = local.Ssn4.Text4;
            ExitState = "ACO_NI0000_SUCCESSFUL_DELETE";
          }
        }

        export.Ch.CheckIndex();

        // ----------------------------------------------
        //                 End Code
        // ----------------------------------------------
        if (IsExitState("ACO_NI0000_SUCCESSFUL_DELETE"))
        {
          export.Ap.Count = 0;
          export.Ar.Count = 0;
          export.Ch.Count = 0;
          export.ApPlus.Text1 = "";
          export.ApMinus.Text1 = "";
          export.ArPlus.Text1 = "";
          export.ArMinus.Text1 = "";
          export.ChPlus.Text1 = "";
          export.ChMinus.Text1 = "";
          local.ApCsePersonsWorkSet.Number = export.ApCsePersonsWorkSet.Number;
          local.ArCsePersonsWorkSet.Number = export.ArCsePersonsWorkSet.Number;
          local.ChCsePersonsWorkSet.Number = export.ChCsePersonsWorkSet.Number;
          UseSiAltsBuildAliasAndSsn1();
          export.ApStandard.PageNumber = 1;
          export.ArStandard.PageNumber = 1;
          export.ChStandard.PageNumber = 1;

          if (export.Ap.IsFull)
          {
            export.ApPlus.Text1 = "+";

            export.PageKeysAp.Index = 0;
            export.PageKeysAp.CheckSize();

            export.Ap.Index = 0;
            export.Ap.CheckSize();

            export.PageKeysAp.Update.PageKeyAp.Number =
              export.ApCsePersonsWorkSet.Number;
            export.PageKeysAp.Update.PageKeyAp.UniqueKey =
              export.Ap.Item.DetApCsePersonsWorkSet.UniqueKey;

            ++export.PageKeysAp.Index;
            export.PageKeysAp.CheckSize();

            export.PageKeysAp.Update.PageKeyAp.UniqueKey =
              export.NextKeyAp.UniqueKey;
            export.PageKeysAp.Update.PageKeyAp.Number =
              export.ApCsePersonsWorkSet.Number;
          }

          if (export.Ar.IsFull)
          {
            export.ArPlus.Text1 = "+";

            export.PageKeysAr.Index = 0;
            export.PageKeysAr.CheckSize();

            export.Ar.Index = 0;
            export.Ar.CheckSize();

            export.PageKeysAr.Update.PageKeyAr.Number =
              export.ArCsePersonsWorkSet.Number;
            export.PageKeysAr.Update.PageKeyAr.UniqueKey =
              export.Ar.Item.DetArCsePersonsWorkSet.UniqueKey;

            ++export.PageKeysAr.Index;
            export.PageKeysAr.CheckSize();

            export.PageKeysAr.Update.PageKeyAr.UniqueKey =
              export.NextKeyAr.UniqueKey;
            export.PageKeysAr.Update.PageKeyAr.Number =
              export.ArCsePersonsWorkSet.Number;
          }

          if (export.Ch.IsFull)
          {
            export.ChPlus.Text1 = "+";

            export.PageKeysCh.Index = 0;
            export.PageKeysCh.CheckSize();

            export.Ch.Index = 0;
            export.Ch.CheckSize();

            export.PageKeysCh.Update.PageKeyCh.Number =
              export.ChCsePersonsWorkSet.Number;
            export.PageKeysCh.Update.PageKeyCh.UniqueKey =
              export.Ch.Item.DetChCsePersonsWorkSet.UniqueKey;

            ++export.PageKeysCh.Index;
            export.PageKeysCh.CheckSize();

            export.PageKeysCh.Update.PageKeyCh.UniqueKey =
              export.NextKeyCh.UniqueKey;
            export.PageKeysCh.Update.PageKeyCh.Number =
              export.ChCsePersonsWorkSet.Number;
          }
        }

        break;
      case "UPDATE":
        if (local.ApCommon.Count == 0 && local.ArCommon.Count == 0 && local
          .ChCommon.Count == 0)
        {
          ExitState = "ACO_NE0000_NO_SELECTION_MADE";

          break;
        }

        export.Ap.Index = 0;

        for(var limit = export.Ap.Count; export.Ap.Index < limit; ++
          export.Ap.Index)
        {
          if (!export.Ap.CheckSize())
          {
            break;
          }

          if (AsChar(export.Ap.Item.DetApCommon.SelectChar) == 'S')
          {
            // -------------------------------------------------------------------------
            // Per WR# 020259, BR#20  the following code is added.
            //                                                        
            // Vithal (05/15/2002)
            // -------------------------------------------------------------------------
            // -----------------------------------------------------------
            //                BR# 20: Start Code
            // -----------------------------------------------------------
            if (IsEmpty(export.ApCsePersonsWorkSet.Sex))
            {
              var field1 =
                GetField(export.ApCsePersonsWorkSet, "formattedName");

              field1.Error = true;

              var field2 = GetField(export.ApCsePersonsWorkSet, "number");

              field2.Error = true;

              ExitState = "SI0000_SEX_MANDATORY";

              goto Test3;
            }

            // -----------------------------------------------------------
            //                BR# 20: End Code
            // -----------------------------------------------------------
            // -------------------------------------------------------------------------
            // Per WR# 020259, BR# 32  the following code is added.
            //                                                        
            // Vithal (08/09/2002)
            // -------------------------------------------------------------------------
            // -----------------------------------------------------------
            // BR# 32: If an alias is showing not being known to CSE, if an 
            // update or delete are attempted a message will display The alias
            // is not known to CSE and can not be deleted or updated.'
            //                        Start Code
            // -----------------------------------------------------------
            if (AsChar(export.Ap.Item.ApCse.Flag) == 'Y')
            {
            }
            else
            {
              // ---------------------------------------------------------------------------------
              // Per PR# 166043:
              // BR# 33 says if the person is inactive in AE/KSC/FACTS/KANPAY if
              // an update is attempted a message will display 'Update
              // successful'.  A system flag will then be created as a Y under
              // CSE.
              // Check if the person is on the system (AE/KSCARES)  AND also 
              // active. If so, can not update (ie.add) the alias for that
              // person  ON ALTS.
              // CSE is above FACTS/KANPAY in Hierarchy. No need to check for 
              // these two systems per SME (Pam Bishop.)
              // ----------------------------------------------------------------------------
              if (AsChar(export.Ap.Item.ApActiveOnAe.Flag) == 'A' && !
                IsEmpty(export.Ap.Item.ApAe.Flag) || AsChar
                (export.Ap.Item.ApActiveOnKscares.Flag) == 'A' && !
                IsEmpty(export.Ap.Item.ApKscares.Flag))
              {
                var field = GetField(export.Ap.Item.DetApCommon, "selectChar");

                field.Error = true;

                ExitState = "SI0000_ALIAS_NOT_KNOWN_TO_CSE";

                goto Test3;
              }
              else
              {
                // -----------------------------------------------------------
                //              OK to process. Proceed.
                // -----------------------------------------------------------
              }
            }

            // -----------------------------------------------------------
            //                BR# 32: End Code
            // -----------------------------------------------------------
            if (AsChar(export.Ap.Item.ApDbOccurrence.Flag) != 'Y')
            {
              var field = GetField(export.Ap.Item.DetApCommon, "selectChar");

              field.Error = true;

              ExitState = "ACO_NE0000_INVALID_ACTION";

              goto Test3;
            }

            if (IsEmpty(export.Ap.Item.DetApCsePersonsWorkSet.FirstName))
            {
              var field =
                GetField(export.Ap.Item.DetApCsePersonsWorkSet, "firstName");

              field.Error = true;

              ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";
            }

            if (IsEmpty(export.Ap.Item.DetApCsePersonsWorkSet.LastName))
            {
              var field =
                GetField(export.Ap.Item.DetApCsePersonsWorkSet, "lastName");

              field.Error = true;

              ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";
            }

            if (!IsEmpty(export.Ap.Item.ApSsn3.Text3) && !
              IsEmpty(Substring(export.Ap.Item.ApSsn2.Text3, 1, 2)) && !
              IsEmpty(export.Ap.Item.ApSsn4.Text4))
            {
              if (Lt(export.Ap.Item.ApSsn3.Text3, "001") || Lt
                ("899", export.Ap.Item.ApSsn3.Text3) || Equal
                (export.Ap.Item.ApSsn3.Text3, "666"))
              {
                var field = GetField(export.Ap.Item.ApSsn3, "text3");

                field.Error = true;

                ExitState = "ACO_NE0000_INVALID_SSN3";

                goto Test3;
              }

              if (Lt(export.Ap.Item.ApSsn2.Text3, "01") || Lt
                ("99", export.Ap.Item.ApSsn2.Text3))
              {
                var field = GetField(export.Ap.Item.ApSsn2, "text3");

                field.Error = true;

                ExitState = "ACO_NE0000_INVALID_SSN2";

                goto Test3;
              }

              if (Lt(export.Ap.Item.ApSsn4.Text4, "0001") || Lt
                ("9999", export.Ap.Item.ApSsn4.Text4))
              {
                var field = GetField(export.Ap.Item.ApSsn4, "text4");

                field.Error = true;

                ExitState = "ACO_NE0000_INVALID_SSN4";

                goto Test3;
              }

              export.Ap.Update.DetApCsePersonsWorkSet.Ssn =
                export.Ap.Item.ApSsn3.Text3 + Substring
                (export.Ap.Item.ApSsn2.Text3, WorkArea.Text3_MaxLength, 1, 2) +
                export.Ap.Item.ApSsn4.Text4;

              // 02/21/08   LSS   Added length check on ssn
              if (Length(TrimEnd(export.Ap.Item.DetApCsePersonsWorkSet.Ssn)) < 9
                )
              {
                var field1 = GetField(export.Ap.Item.ApSsn3, "text3");

                field1.Error = true;

                var field2 = GetField(export.Ap.Item.ApSsn2, "text3");

                field2.Error = true;

                var field3 = GetField(export.Ap.Item.ApSsn4, "text4");

                field3.Error = true;

                ExitState = "SSN_NOT_COMPLETE";

                goto Test3;
              }

              // 02/21/08   LSS   Added verify on ssn
              if (Verify(export.Ap.Item.DetApCsePersonsWorkSet.Ssn, "0123456789")
                != 0 && !IsEmpty(export.Ap.Item.DetApCsePersonsWorkSet.Ssn))
              {
                var field1 = GetField(export.Ap.Item.ApSsn3, "text3");

                field1.Error = true;

                var field2 = GetField(export.Ap.Item.ApSsn2, "text3");

                field2.Error = true;

                var field3 = GetField(export.Ap.Item.ApSsn4, "text4");

                field3.Error = true;

                ExitState = "LE0000_SSN_CONTAINS_NONNUM";

                goto Test3;
              }

              if (!IsEmpty(export.Ap.Item.DetApCsePersonsWorkSet.Ssn) && !
                Equal(export.Ap.Item.DetApCsePersonsWorkSet.Ssn,
                export.Ap.Item.DetApPrev.Ssn))
              {
                local.Check.SsnNum9 =
                  (int)StringToNumber(export.Ap.Item.DetApCsePersonsWorkSet.Ssn);
                  

                if (ReadInvalidSsn1())
                {
                  var field1 = GetField(export.ApCsePersonsWorkSet, "number");

                  field1.Error = true;

                  var field2 = GetField(export.Ap.Item.ApSsn3, "text3");

                  field2.Error = true;

                  var field3 = GetField(export.Ap.Item.ApSsn2, "text3");

                  field3.Error = true;

                  var field4 = GetField(export.Ap.Item.ApSsn4, "text4");

                  field4.Error = true;

                  ExitState = "INVALID_SSN";

                  goto Test3;
                }
                else
                {
                  // this is fine, the cse person number and ssn combination 
                  // being added is not on
                  // the invalid ssn table
                }
              }
            }
            else
            {
              // -------------------------------------------------------------------------
              // Per WR# 020259,  the following code is commented. SSN no longer
              // mandatory.
              //                                                        
              // Vithal (05/15/2002)
              // -------------------------------------------------------------------------
              export.Ap.Update.DetApCsePersonsWorkSet.Ssn = "000000000";
            }

            if (IsExitState("ACO_NE0000_REQUIRED_FIELD_MISSIN"))
            {
              goto Test3;
            }

            // -- Validate name.
            UseSiCheckName1();

            if (IsExitState("SI0000_INVALID_NAME"))
            {
              var field1 =
                GetField(export.Ap.Item.DetApCsePersonsWorkSet, "firstName");

              field1.Error = true;

              var field2 =
                GetField(export.Ap.Item.DetApCsePersonsWorkSet, "lastName");

              field2.Error = true;

              var field3 =
                GetField(export.Ap.Item.DetApCsePersonsWorkSet, "middleInitial");
                

              field3.Error = true;

              ExitState = "SI0001_INVALID_NAME";

              goto Test3;
            }

            MoveCsePersonsWorkSet2(export.Ap.Item.DetApCsePersonsWorkSet,
              local.CsePersonsWorkSet);
            local.CsePersonsWorkSet.Number = export.ApCsePersonsWorkSet.Number;

            if (Equal(export.Ap.Item.DetApCsePersonsWorkSet.Dob, local.Zero.Date))
              
            {
              local.CsePersonsWorkSet.Dob = export.ApCsePersonsWorkSet.Dob;
            }

            if (IsEmpty(export.Ap.Item.DetApCsePersonsWorkSet.Sex))
            {
              local.CsePersonsWorkSet.Sex = export.ApCsePersonsWorkSet.Sex;
            }

            UseSiAltsCabUpdateAlias();

            if (IsExitState("ACO_NN0000_ALL_OK") || IsExitState
              ("ACO_NI0000_SUCCESSFUL_UPDATE"))
            {
            }
            else
            {
              goto Test3;
            }

            export.Ap.Update.DetApCommon.SelectChar = "*";
            ExitState = "ACO_NI0000_SUCCESSFUL_UPDATE";
          }
        }

        export.Ap.CheckIndex();
        export.Ar.Index = 0;

        for(var limit = export.Ar.Count; export.Ar.Index < limit; ++
          export.Ar.Index)
        {
          if (!export.Ar.CheckSize())
          {
            break;
          }

          if (AsChar(export.Ar.Item.DetArCommon.SelectChar) == 'S')
          {
            // -------------------------------------------------------------------------
            // Per WR# 020259, BR#20, the following code is added.
            //                                                        
            // Vithal (05/15/2002)
            // -------------------------------------------------------------------------
            // -----------------------------------------------------------
            //                BR# 20: Start Code
            // -----------------------------------------------------------
            if (IsEmpty(export.ArCsePersonsWorkSet.Sex))
            {
              var field1 =
                GetField(export.ArCsePersonsWorkSet, "formattedName");

              field1.Error = true;

              var field2 = GetField(export.ArCsePersonsWorkSet, "number");

              field2.Error = true;

              ExitState = "SI0000_SEX_MANDATORY";

              goto Test3;
            }

            // -----------------------------------------------------------
            //                BR# 20: End Code
            // -----------------------------------------------------------
            // -------------------------------------------------------------------------
            // Per WR# 020259, BR# 32  the following code is added.
            //                                                        
            // Vithal (08/09/2002)
            // -------------------------------------------------------------------------
            // -----------------------------------------------------------
            // BR# 32: If an alias is showing not being known to CSE, if an 
            // update or delete are attempted a message will display The alias
            // is not known to CSE and can not be deleted or updated.'
            //                        Start Code
            // -----------------------------------------------------------
            if (AsChar(export.Ar.Item.ArCse.Flag) == 'Y')
            {
            }
            else
            {
              // ---------------------------------------------------------------------------------
              // Per PR# 166043:
              // BR# 33 says if the person is inactive in AE/KSC/FACTS/KANPAY if
              // an update is attempted a message will display 'Update
              // successful'.  A system flag will then be created as a Y under
              // CSE.
              // Check if the person is on the system (AE/KSCARES)  AND also 
              // active. If so, can not update (ie.add) the alias for that
              // person  ON ALTS.
              // CSE is above FACTS/KANPAY in Hierarchy. No need to check for 
              // these two systems per SME (Pam Bishop.)
              // ----------------------------------------------------------------------------
              if (AsChar(export.Ar.Item.ArActiveOnAe.Flag) == 'A' && !
                IsEmpty(export.Ar.Item.ArAe.Flag) || AsChar
                (export.Ar.Item.ArActiveOnKscares.Flag) == 'A' && !
                IsEmpty(export.Ar.Item.ArKscares.Flag))
              {
                var field = GetField(export.Ar.Item.DetArCommon, "selectChar");

                field.Error = true;

                ExitState = "SI0000_ALIAS_NOT_KNOWN_TO_CSE";

                goto Test3;
              }
              else
              {
                // -----------------------------------------------------------
                //              OK to process. Proceed.
                // -----------------------------------------------------------
              }
            }

            // -----------------------------------------------------------
            //                BR# 32: End Code
            // -----------------------------------------------------------
            if (AsChar(export.Ar.Item.ArDbOccurrence.Flag) != 'Y')
            {
              var field = GetField(export.Ar.Item.DetArCommon, "selectChar");

              field.Error = true;

              ExitState = "ACO_NE0000_INVALID_ACTION";

              goto Test3;
            }

            if (IsEmpty(export.Ar.Item.DetArCsePersonsWorkSet.FirstName))
            {
              var field =
                GetField(export.Ar.Item.DetArCsePersonsWorkSet, "firstName");

              field.Error = true;

              ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";
            }

            if (IsEmpty(export.Ar.Item.DetArCsePersonsWorkSet.LastName))
            {
              var field =
                GetField(export.Ar.Item.DetArCsePersonsWorkSet, "lastName");

              field.Error = true;

              ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";
            }

            if (!IsEmpty(export.Ar.Item.ArSsn3.Text3) && !
              IsEmpty(Substring(export.Ar.Item.ArSsn2.Text3, 1, 2)) && !
              IsEmpty(export.Ar.Item.ArSsn4.Text4))
            {
              if (Lt(export.Ar.Item.ArSsn3.Text3, "001") || Lt
                ("899", export.Ar.Item.ArSsn3.Text3) || Equal
                (export.Ar.Item.ArSsn3.Text3, "666"))
              {
                var field = GetField(export.Ar.Item.ArSsn3, "text3");

                field.Error = true;

                ExitState = "ACO_NE0000_INVALID_SSN3";

                goto Test3;
              }

              if (Lt(export.Ar.Item.ArSsn2.Text3, "01") || Lt
                ("99", export.Ar.Item.ArSsn2.Text3))
              {
                var field = GetField(export.Ar.Item.ArSsn2, "text3");

                field.Error = true;

                ExitState = "ACO_NE0000_INVALID_SSN2";

                goto Test3;
              }

              if (Lt(export.Ar.Item.ArSsn4.Text4, "0001") || Lt
                ("9999", export.Ar.Item.ArSsn4.Text4))
              {
                var field = GetField(export.Ar.Item.ArSsn4, "text4");

                field.Error = true;

                ExitState = "ACO_NE0000_INVALID_SSN4";

                goto Test3;
              }

              export.Ar.Update.DetArCsePersonsWorkSet.Ssn =
                export.Ar.Item.ArSsn3.Text3 + Substring
                (export.Ar.Item.ArSsn2.Text3, WorkArea.Text3_MaxLength, 1, 2) +
                export.Ar.Item.ArSsn4.Text4;

              // 02/21/08   LSS   Added length check on ssn
              if (Length(TrimEnd(export.Ar.Item.DetArCsePersonsWorkSet.Ssn)) < 9
                )
              {
                var field1 = GetField(export.Ar.Item.ArSsn3, "text3");

                field1.Error = true;

                var field2 = GetField(export.Ar.Item.ArSsn2, "text3");

                field2.Error = true;

                var field3 = GetField(export.Ar.Item.ArSsn4, "text4");

                field3.Error = true;

                ExitState = "SSN_NOT_COMPLETE";

                goto Test3;
              }

              // 02/21/08   LSS   Added verify on ssn
              if (Verify(export.Ar.Item.DetArCsePersonsWorkSet.Ssn, "0123456789")
                != 0 && !IsEmpty(export.Ar.Item.DetArCsePersonsWorkSet.Ssn))
              {
                var field1 = GetField(export.Ar.Item.ArSsn3, "text3");

                field1.Error = true;

                var field2 = GetField(export.Ar.Item.ArSsn2, "text3");

                field2.Error = true;

                var field3 = GetField(export.Ar.Item.ArSsn4, "text4");

                field3.Error = true;

                ExitState = "LE0000_SSN_CONTAINS_NONNUM";

                goto Test3;
              }

              if (!IsEmpty(export.Ar.Item.DetArCsePersonsWorkSet.Ssn) && !
                Equal(export.Ar.Item.DetArCsePersonsWorkSet.Ssn,
                export.Ar.Item.DetArPrev.Ssn))
              {
                local.Check.SsnNum9 =
                  (int)StringToNumber(export.ArCsePersonsWorkSet.Ssn);

                if (ReadInvalidSsn2())
                {
                  var field1 = GetField(export.ArCsePersonsWorkSet, "number");

                  field1.Error = true;

                  var field2 = GetField(export.Ar.Item.ArSsn3, "text3");

                  field2.Error = true;

                  var field3 = GetField(export.Ar.Item.ArSsn2, "text3");

                  field3.Error = true;

                  var field4 = GetField(export.Ar.Item.ArSsn4, "text4");

                  field4.Error = true;

                  ExitState = "INVALID_SSN";

                  goto Test3;
                }
                else
                {
                  // this is fine, the cse person number and ssn combination 
                  // being added is not on
                  // the invalid ssn table
                }
              }
            }
            else
            {
              // -------------------------------------------------------------------------
              // Per WR# 020259,  the following code is commented. SSN no longer
              // mandatory.
              //                                                        
              // Vithal (05/15/2002)
              // -------------------------------------------------------------------------
              export.Ar.Update.DetArCsePersonsWorkSet.Ssn = "000000000";
            }

            if (IsExitState("ACO_NE0000_REQUIRED_FIELD_MISSIN"))
            {
              goto Test3;
            }

            // -- Validate name.
            UseSiCheckName2();

            if (IsExitState("SI0000_INVALID_NAME"))
            {
              var field1 =
                GetField(export.Ar.Item.DetArCsePersonsWorkSet, "firstName");

              field1.Error = true;

              var field2 =
                GetField(export.Ar.Item.DetArCsePersonsWorkSet, "lastName");

              field2.Error = true;

              var field3 =
                GetField(export.Ar.Item.DetArCsePersonsWorkSet, "middleInitial");
                

              field3.Error = true;

              ExitState = "SI0001_INVALID_NAME";

              goto Test3;
            }

            MoveCsePersonsWorkSet2(export.Ar.Item.DetArCsePersonsWorkSet,
              local.CsePersonsWorkSet);
            local.CsePersonsWorkSet.Number = export.ArCsePersonsWorkSet.Number;
            UseSiAltsCabUpdateAlias();

            if (IsExitState("ACO_NN0000_ALL_OK") || IsExitState
              ("ACO_NI0000_SUCCESSFUL_UPDATE"))
            {
            }
            else
            {
              goto Test3;
            }

            export.Ar.Update.DetArCommon.SelectChar = "*";
            ExitState = "ACO_NI0000_SUCCESSFUL_UPDATE";
          }
        }

        export.Ar.CheckIndex();

        // -------------------------------------------------------------------------
        // Per WR# 020259,  the following code is added.
        //                                                        
        // Vithal (05/15/2002)
        // -------------------------------------------------------------------------
        // ---------------------------------------------
        //                     Start Code
        // ---------------------------------------------
        export.Ch.Index = 0;

        for(var limit = export.Ch.Count; export.Ch.Index < limit; ++
          export.Ch.Index)
        {
          if (!export.Ch.CheckSize())
          {
            break;
          }

          if (AsChar(export.Ch.Item.DetChCommon.SelectChar) == 'S')
          {
            // -------------------------------------------------------------------------
            // Per WR# 020259, BR# 32  the following code is added.
            //                                                        
            // Vithal (08/09/2002)
            // -------------------------------------------------------------------------
            // -----------------------------------------------------------
            // BR# 32: If an alias is showing not being known to CSE, if an 
            // update or delete are attempted a message will display The alias
            // is not known to CSE and can not be deleted or updated.'
            //                        Start Code
            // -----------------------------------------------------------
            if (AsChar(export.Ch.Item.ChCse.Flag) == 'Y')
            {
            }
            else
            {
              // ---------------------------------------------------------------------------------
              // Per PR# 166043:
              // BR# 33 says if the person is inactive in AE/KSC/FACTS/KANPAY if
              // an update is attempted a message will display 'Update
              // successful'.  A system flag will then be created as a Y under
              // CSE.
              // Check if the person is on the system (AE/KSCARES)  AND also 
              // active. If so, can not update (ie.add) the alias for that
              // person  ON ALTS.
              // CSE is above FACTS/KANPAY in Hierarchy. No need to check for 
              // these two systems per SME (Pam Bishop.)
              // ----------------------------------------------------------------------------
              if (AsChar(export.Ch.Item.ChActiveOnAe.Flag) == 'A' && !
                IsEmpty(export.Ch.Item.ChAe.Flag) || AsChar
                (export.Ch.Item.ChActiveOnKscares.Flag) == 'A' && !
                IsEmpty(export.Ch.Item.ChKscares.Flag))
              {
                var field = GetField(export.Ch.Item.DetChCommon, "selectChar");

                field.Error = true;

                ExitState = "SI0000_ALIAS_NOT_KNOWN_TO_CSE";

                goto Test3;
              }
              else
              {
                // -----------------------------------------------------------
                //            OK to process. Proceed.
                // -----------------------------------------------------------
              }
            }

            // -----------------------------------------------------------
            //                BR# 32: End Code
            // -----------------------------------------------------------
            if (IsEmpty(export.ChCsePersonsWorkSet.Sex))
            {
              var field1 =
                GetField(export.ChCsePersonsWorkSet, "formattedName");

              field1.Error = true;

              var field2 = GetField(export.ChCsePersonsWorkSet, "number");

              field2.Error = true;

              ExitState = "SI0000_SEX_MANDATORY";

              goto Test3;
            }

            if (AsChar(export.Ch.Item.ChDbOccurrence.Flag) != 'Y')
            {
              var field = GetField(export.Ch.Item.DetChCommon, "selectChar");

              field.Error = true;

              ExitState = "ACO_NE0000_INVALID_ACTION";

              goto Test3;
            }

            if (IsEmpty(export.Ch.Item.DetChCsePersonsWorkSet.FirstName))
            {
              var field =
                GetField(export.Ch.Item.DetChCsePersonsWorkSet, "firstName");

              field.Error = true;

              ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";
            }

            if (IsEmpty(export.Ch.Item.DetChCsePersonsWorkSet.LastName))
            {
              var field =
                GetField(export.Ch.Item.DetChCsePersonsWorkSet, "lastName");

              field.Error = true;

              ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";
            }

            if (!IsEmpty(export.Ch.Item.ChSsn3.Text3) && !
              IsEmpty(Substring(export.Ch.Item.ChSsn2.Text3, 1, 2)) && !
              IsEmpty(export.Ch.Item.ChSsn4.Text4))
            {
              if (Lt(export.Ch.Item.ChSsn3.Text3, "001") || Lt
                ("899", export.Ch.Item.ChSsn3.Text3) || Equal
                (export.Ch.Item.ChSsn3.Text3, "666"))
              {
                var field = GetField(export.Ch.Item.ChSsn3, "text3");

                field.Error = true;

                ExitState = "ACO_NE0000_INVALID_SSN3";

                goto Test3;
              }

              if (Lt(export.Ch.Item.ChSsn2.Text3, "01") || Lt
                ("99", export.Ch.Item.ChSsn2.Text3))
              {
                var field = GetField(export.Ch.Item.ChSsn2, "text3");

                field.Error = true;

                ExitState = "ACO_NE0000_INVALID_SSN2";

                goto Test3;
              }

              if (Lt(export.Ch.Item.ChSsn4.Text4, "0001") || Lt
                ("9999", export.Ch.Item.ChSsn4.Text4))
              {
                var field = GetField(export.Ch.Item.ChSsn4, "text4");

                field.Error = true;

                ExitState = "ACO_NE0000_INVALID_SSN4";

                goto Test3;
              }

              export.Ch.Update.DetChCsePersonsWorkSet.Ssn =
                export.Ch.Item.ChSsn3.Text3 + Substring
                (export.Ch.Item.ChSsn2.Text3, WorkArea.Text3_MaxLength, 1, 2) +
                export.Ch.Item.ChSsn4.Text4;

              // 02/21/08   LSS   Added length check on ssn
              if (Length(TrimEnd(export.Ch.Item.DetChCsePersonsWorkSet.Ssn)) < 9
                )
              {
                var field1 = GetField(export.Ch.Item.ChSsn3, "text3");

                field1.Error = true;

                var field2 = GetField(export.Ch.Item.ChSsn2, "text3");

                field2.Error = true;

                var field3 = GetField(export.Ch.Item.ChSsn4, "text4");

                field3.Error = true;

                ExitState = "SSN_NOT_COMPLETE";

                goto Test3;
              }

              // 02/21/08   LSS   Added verify on ssn
              if (Verify(export.Ch.Item.DetChCsePersonsWorkSet.Ssn, "0123456789")
                != 0 && !IsEmpty(export.Ch.Item.DetChCsePersonsWorkSet.Ssn))
              {
                var field1 = GetField(export.Ch.Item.ChSsn3, "text3");

                field1.Error = true;

                var field2 = GetField(export.Ch.Item.ChSsn2, "text3");

                field2.Error = true;

                var field3 = GetField(export.Ch.Item.ChSsn4, "text4");

                field3.Error = true;

                ExitState = "LE0000_SSN_CONTAINS_NONNUM";

                goto Test3;
              }

              if (!IsEmpty(export.Ch.Item.DetChCsePersonsWorkSet.Ssn) && !
                Equal(export.Ch.Item.DetChCsePersonsWorkSet.Ssn,
                export.Ch.Item.DetChPrev.Ssn))
              {
                local.Check.SsnNum9 =
                  (int)StringToNumber(export.ChCsePersonsWorkSet.Ssn);

                if (ReadInvalidSsn3())
                {
                  var field1 = GetField(export.ChCsePersonsWorkSet, "number");

                  field1.Error = true;

                  var field2 = GetField(export.Ch.Item.ChSsn3, "text3");

                  field2.Error = true;

                  var field3 = GetField(export.Ch.Item.ChSsn2, "text3");

                  field3.Error = true;

                  var field4 = GetField(export.Ch.Item.ChSsn4, "text4");

                  field4.Error = true;

                  ExitState = "INVALID_SSN";

                  goto Test3;
                }
                else
                {
                  // this is fine, the cse person number and ssn combination 
                  // being added is not on
                  // the invalid ssn table
                }
              }
            }
            else
            {
              export.Ch.Update.DetChCsePersonsWorkSet.Ssn = "000000000";
            }

            if (IsExitState("ACO_NE0000_REQUIRED_FIELD_MISSIN"))
            {
              goto Test3;
            }

            // -- Validate name.
            UseSiCheckName3();

            if (IsExitState("SI0000_INVALID_NAME"))
            {
              var field1 =
                GetField(export.Ch.Item.DetChCsePersonsWorkSet, "firstName");

              field1.Error = true;

              var field2 =
                GetField(export.Ch.Item.DetChCsePersonsWorkSet, "lastName");

              field2.Error = true;

              var field3 =
                GetField(export.Ch.Item.DetChCsePersonsWorkSet, "middleInitial");
                

              field3.Error = true;

              ExitState = "SI0001_INVALID_NAME";

              goto Test3;
            }

            MoveCsePersonsWorkSet2(export.Ch.Item.DetChCsePersonsWorkSet,
              local.CsePersonsWorkSet);
            local.CsePersonsWorkSet.Number = export.ChCsePersonsWorkSet.Number;
            UseSiAltsCabUpdateAlias();

            if (IsExitState("ACO_NN0000_ALL_OK") || IsExitState
              ("ACO_NI0000_SUCCESSFUL_UPDATE"))
            {
            }
            else
            {
              goto Test3;
            }

            export.Ch.Update.DetChCommon.SelectChar = "*";
            ExitState = "ACO_NI0000_SUCCESSFUL_UPDATE";
          }
        }

        export.Ch.CheckIndex();

        // ----------------------------------------------
        //                 End Code
        // ----------------------------------------------
        if (IsExitState("ACO_NI0000_SUCCESSFUL_UPDATE"))
        {
          export.Ap.Count = 0;
          export.Ar.Count = 0;
          export.Ch.Count = 0;
          export.ApPlus.Text1 = "";
          export.ApMinus.Text1 = "";
          export.ArPlus.Text1 = "";
          export.ArMinus.Text1 = "";
          export.ChPlus.Text1 = "";
          export.ChMinus.Text1 = "";
          local.ApCsePersonsWorkSet.Number = export.ApCsePersonsWorkSet.Number;
          local.ArCsePersonsWorkSet.Number = export.ArCsePersonsWorkSet.Number;
          local.ChCsePersonsWorkSet.Number = export.ChCsePersonsWorkSet.Number;
          UseSiAltsBuildAliasAndSsn1();
          export.ApStandard.PageNumber = 1;
          export.ArStandard.PageNumber = 1;
          export.ChStandard.PageNumber = 1;

          if (export.Ap.IsFull)
          {
            export.ApPlus.Text1 = "+";

            export.PageKeysAp.Index = 0;
            export.PageKeysAp.CheckSize();

            export.Ap.Index = 0;
            export.Ap.CheckSize();

            export.PageKeysAp.Update.PageKeyAp.Number =
              export.ApCsePersonsWorkSet.Number;
            export.PageKeysAp.Update.PageKeyAp.UniqueKey =
              export.Ap.Item.DetApCsePersonsWorkSet.UniqueKey;

            ++export.PageKeysAp.Index;
            export.PageKeysAp.CheckSize();

            export.PageKeysAp.Update.PageKeyAp.UniqueKey =
              export.NextKeyAp.UniqueKey;
            export.PageKeysAp.Update.PageKeyAp.Number =
              export.ApCsePersonsWorkSet.Number;
          }

          if (export.Ar.IsFull)
          {
            export.ArPlus.Text1 = "+";

            export.PageKeysAr.Index = 0;
            export.PageKeysAr.CheckSize();

            export.Ar.Index = 0;
            export.Ar.CheckSize();

            export.PageKeysAr.Update.PageKeyAr.Number =
              export.ArCsePersonsWorkSet.Number;
            export.PageKeysAr.Update.PageKeyAr.UniqueKey =
              export.Ar.Item.DetArCsePersonsWorkSet.UniqueKey;

            ++export.PageKeysAr.Index;
            export.PageKeysAr.CheckSize();

            export.PageKeysAr.Update.PageKeyAr.UniqueKey =
              export.NextKeyAr.UniqueKey;
            export.PageKeysAr.Update.PageKeyAr.Number =
              export.ArCsePersonsWorkSet.Number;
          }

          if (export.Ch.IsFull)
          {
            export.ChPlus.Text1 = "+";

            export.PageKeysCh.Index = 0;
            export.PageKeysCh.CheckSize();

            export.Ch.Index = 0;
            export.Ch.CheckSize();

            export.PageKeysCh.Update.PageKeyCh.Number =
              export.ChCsePersonsWorkSet.Number;
            export.PageKeysCh.Update.PageKeyCh.UniqueKey =
              export.Ch.Item.DetChCsePersonsWorkSet.UniqueKey;

            ++export.PageKeysCh.Index;
            export.PageKeysCh.CheckSize();

            export.PageKeysCh.Update.PageKeyCh.UniqueKey =
              export.NextKeyCh.UniqueKey;
            export.PageKeysCh.Update.PageKeyCh.Number =
              export.ChCsePersonsWorkSet.Number;
          }
        }

        break;
      case "APDS":
        ExitState = "ECO_LNK_TO_AP_DETAILS";

        break;
      case "ARDS":
        ExitState = "ECO_LNK_TO_AR_DETAILS";

        break;
      case "CHDS":
        ExitState = "ECO_LNK_TO_CHDS";

        break;
      case "LIST":
        local.Selection.Count = 0;

        if (AsChar(export.ApPrompt.Text1) == 'S')
        {
          ++local.Selection.Count;
        }

        if (AsChar(export.ArPrompt.Text1) == 'S')
        {
          ++local.Selection.Count;
        }

        if (AsChar(export.ChPrompt.Text1) == 'S')
        {
          ++local.Selection.Count;
        }

        if (local.Selection.Count > 1)
        {
          if (AsChar(export.ApPrompt.Text1) == 'S')
          {
            var field = GetField(export.ApPrompt, "text1");

            field.Error = true;
          }

          if (AsChar(export.ArPrompt.Text1) == 'S')
          {
            var field = GetField(export.ArPrompt, "text1");

            field.Error = true;
          }

          if (AsChar(export.ChPrompt.Text1) == 'S')
          {
            var field = GetField(export.ChPrompt, "text1");

            field.Error = true;
          }

          ExitState = "ACO_NE0000_ONLY_ONE_SELECTION";

          break;
        }

        if (local.Selection.Count == 0)
        {
          var field1 = GetField(export.ApPrompt, "text1");

          field1.Error = true;

          var field2 = GetField(export.ArPrompt, "text1");

          field2.Error = true;

          var field3 = GetField(export.ChPrompt, "text1");

          field3.Error = true;

          ExitState = "ACO_NE0000_NO_SELECTION_MADE";
        }

        if (AsChar(import.ApPrompt.Text1) == 'S')
        {
          export.ApPrompt.Text1 = "";
          export.HiddenFlowViaPrompt.Flag = "Y";
          export.HiddenFromAlts.Flag = "Y";
          ExitState = "ECO_LNK_TO_CASE_COMPOSITION";
        }

        if (AsChar(import.ArPrompt.Text1) == 'S')
        {
          export.ArPrompt.Text1 = "";
          export.HiddenFromAlts.Flag = "Y";
          ExitState = "ECO_LNK_TO_ROLE";
        }

        if (AsChar(import.ChPrompt.Text1) == 'S')
        {
          export.ChPrompt.Text1 = "";
          export.HiddenFlowViaPrompt.Flag = "Y";
          export.HiddenFromAlts.Flag = "Y";
          ExitState = "ECO_LNK_TO_CASE_COMPOSITION";
        }

        break;
      case "SIGNOFF":
        UseScCabSignoff();

        break;
      default:
        ExitState = "ACO_NE0000_INVALID_COMMAND";

        break;
    }

Test3:

    // ------------------------------------------------------------------------------------
    // Per BR# 32,  if an alias is  on AE or KSCARES and ACTIVE, the record will
    // be protected and no update will be available.
    // --------------------------------------------------------------------------------------
    for(export.Ap.Index = 0; export.Ap.Index < export.Ap.Count; ++
      export.Ap.Index)
    {
      if (!export.Ap.CheckSize())
      {
        break;
      }

      export.Ap.Update.DetApPrev.Ssn =
        export.Ap.Item.DetApCsePersonsWorkSet.Ssn;

      if (AsChar(export.Ap.Item.ApActiveOnAe.Flag) == 'A' && !
        IsEmpty(export.Ap.Item.ApAe.Flag) || AsChar
        (export.Ap.Item.ApActiveOnKscares.Flag) == 'A' && !
        IsEmpty(export.Ap.Item.ApKscares.Flag))
      {
        var field1 = GetField(export.Ap.Item.DetApCommon, "selectChar");

        field1.Color = "cyan";
        field1.Protected = true;

        var field2 =
          GetField(export.Ap.Item.DetApCsePersonsWorkSet, "lastName");

        field2.Color = "cyan";
        field2.Protected = true;

        var field3 =
          GetField(export.Ap.Item.DetApCsePersonsWorkSet, "firstName");

        field3.Color = "cyan";
        field3.Protected = true;

        var field4 =
          GetField(export.Ap.Item.DetApCsePersonsWorkSet, "middleInitial");

        field4.Color = "cyan";
        field4.Protected = true;

        var field5 = GetField(export.Ap.Item.ApSsn3, "text3");

        field5.Color = "cyan";
        field5.Protected = true;

        var field6 = GetField(export.Ap.Item.ApSsn2, "text3");

        field6.Color = "cyan";
        field6.Protected = true;

        var field7 = GetField(export.Ap.Item.ApSsn4, "text4");

        field7.Color = "cyan";
        field7.Protected = true;
      }
    }

    export.Ap.CheckIndex();

    for(export.Ar.Index = 0; export.Ar.Index < export.Ar.Count; ++
      export.Ar.Index)
    {
      if (!export.Ar.CheckSize())
      {
        break;
      }

      export.Ar.Update.DetArPrev.Ssn =
        export.Ar.Item.DetArCsePersonsWorkSet.Ssn;

      if (AsChar(export.Ar.Item.ArActiveOnAe.Flag) == 'A' && !
        IsEmpty(export.Ar.Item.ArAe.Flag) || AsChar
        (export.Ar.Item.ArActiveOnKscares.Flag) == 'A' && !
        IsEmpty(export.Ar.Item.ArKscares.Flag))
      {
        var field1 = GetField(export.Ar.Item.DetArCommon, "selectChar");

        field1.Color = "cyan";
        field1.Protected = true;

        var field2 =
          GetField(export.Ar.Item.DetArCsePersonsWorkSet, "lastName");

        field2.Color = "cyan";
        field2.Protected = true;

        var field3 =
          GetField(export.Ar.Item.DetArCsePersonsWorkSet, "firstName");

        field3.Color = "cyan";
        field3.Protected = true;

        var field4 =
          GetField(export.Ar.Item.DetArCsePersonsWorkSet, "middleInitial");

        field4.Color = "cyan";
        field4.Protected = true;

        var field5 = GetField(export.Ar.Item.ArSsn3, "text3");

        field5.Color = "cyan";
        field5.Protected = true;

        var field6 = GetField(export.Ar.Item.ArSsn2, "text3");

        field6.Color = "cyan";
        field6.Protected = true;

        var field7 = GetField(export.Ar.Item.ArSsn4, "text4");

        field7.Color = "cyan";
        field7.Protected = true;
      }
    }

    export.Ar.CheckIndex();

    for(export.Ch.Index = 0; export.Ch.Index < export.Ch.Count; ++
      export.Ch.Index)
    {
      if (!export.Ch.CheckSize())
      {
        break;
      }

      export.Ch.Update.DetChPrev.Ssn =
        export.Ch.Item.DetChCsePersonsWorkSet.Ssn;

      if (AsChar(export.Ch.Item.ChActiveOnAe.Flag) == 'A' && !
        IsEmpty(export.Ch.Item.ChAe.Flag) || AsChar
        (export.Ch.Item.ChActiveOnKscares.Flag) == 'A' && !
        IsEmpty(export.Ch.Item.ChKscares.Flag))
      {
        var field1 = GetField(export.Ch.Item.DetChCommon, "selectChar");

        field1.Color = "cyan";
        field1.Protected = true;

        var field2 =
          GetField(export.Ch.Item.DetChCsePersonsWorkSet, "lastName");

        field2.Color = "cyan";
        field2.Protected = true;

        var field3 =
          GetField(export.Ch.Item.DetChCsePersonsWorkSet, "firstName");

        field3.Color = "cyan";
        field3.Protected = true;

        var field4 =
          GetField(export.Ch.Item.DetChCsePersonsWorkSet, "middleInitial");

        field4.Color = "cyan";
        field4.Protected = true;

        var field5 = GetField(export.Ch.Item.ChSsn3, "text3");

        field5.Color = "cyan";
        field5.Protected = true;

        var field6 = GetField(export.Ch.Item.ChSsn2, "text3");

        field6.Color = "cyan";
        field6.Protected = true;

        var field7 = GetField(export.Ch.Item.ChSsn4, "text4");

        field7.Color = "cyan";
        field7.Protected = true;
      }
    }

    export.Ch.CheckIndex();
  }

  private static void MoveAp(SiAltsBuildAliasAndSsn2.Export.ApGroup source,
    Export.ApGroup target)
  {
    System.Diagnostics.Trace.TraceWarning(
      "INFO: source and target of the move do not fit perfectly.");
    System.Diagnostics.Trace.TraceWarning(
      "INFO: source and target of the move do not fit perfectly but only head.");
      
    target.DetApCommon.SelectChar = source.GapCommon.SelectChar;
    target.DetApCsePersonsWorkSet.Assign(source.GapCsePersonsWorkSet);
    target.ApSsn3.Text3 = source.GapSsn3.Text3;
    target.ApSsn2.Text3 = source.GapSsn2.Text3;
    target.ApSsn4.Text4 = source.GapSsn4.Text4;
    target.ApKscares.Flag = source.GapKscares.Flag;
    target.ApKanpay.Flag = source.GapKanpay.Flag;
    target.ApCse.Flag = source.GapCse.Flag;
    target.ApAe.Flag = source.GapAe.Flag;
    target.ApDbOccurrence.Flag = source.GapFa.Flag;
  }

  private static void MoveAr(SiAltsBuildAliasAndSsn2.Export.ArGroup source,
    Export.ArGroup target)
  {
    System.Diagnostics.Trace.TraceWarning(
      "INFO: source and target of the move do not fit perfectly.");
    System.Diagnostics.Trace.TraceWarning(
      "INFO: source and target of the move do not fit perfectly but only head.");
      
    target.DetArCommon.SelectChar = source.GarCommon.SelectChar;
    target.DetArCsePersonsWorkSet.Assign(source.GarCsePersonsWorkSet);
    target.ArSsn3.Text3 = source.GarSsn3.Text3;
    target.ArSsn2.Text3 = source.GarSsn2.Text3;
    target.ArSsn4.Text4 = source.GarSsn4.Text4;
    target.ArKscares.Flag = source.GarKscares.Flag;
    target.ArKanpay.Flag = source.GarKanpay.Flag;
    target.ArCse.Flag = source.GarCse.Flag;
    target.ArAe.Flag = source.GarAe.Flag;
    target.ArDbOccurrence.Flag = source.GarFa.Flag;
  }

  private static void MoveCh(SiAltsBuildAliasAndSsn2.Export.ChGroup source,
    Export.ChGroup target)
  {
    System.Diagnostics.Trace.TraceWarning(
      "INFO: move results in empty operation.");
    System.Diagnostics.Trace.TraceWarning(
      "INFO: source and target of the move do not fit perfectly.");
    System.Diagnostics.Trace.TraceWarning(
      "INFO: source and target of the move do not fit perfectly but only head.");
      
  }

  private static void MoveCsePersonsWorkSet1(CsePersonsWorkSet source,
    CsePersonsWorkSet target)
  {
    target.UniqueKey = source.UniqueKey;
    target.Number = source.Number;
  }

  private static void MoveCsePersonsWorkSet2(CsePersonsWorkSet source,
    CsePersonsWorkSet target)
  {
    target.UniqueKey = source.UniqueKey;
    target.Sex = source.Sex;
    target.Dob = source.Dob;
    target.Ssn = source.Ssn;
    target.FirstName = source.FirstName;
    target.MiddleInitial = source.MiddleInitial;
    target.LastName = source.LastName;
  }

  private static void MoveCsePersonsWorkSet3(CsePersonsWorkSet source,
    CsePersonsWorkSet target)
  {
    target.Number = source.Number;
    target.Sex = source.Sex;
    target.Dob = source.Dob;
    target.Ssn = source.Ssn;
    target.FirstName = source.FirstName;
    target.MiddleInitial = source.MiddleInitial;
    target.FormattedName = source.FormattedName;
    target.LastName = source.LastName;
  }

  private static void MoveCsePersonsWorkSet4(CsePersonsWorkSet source,
    CsePersonsWorkSet target)
  {
    target.FirstName = source.FirstName;
    target.MiddleInitial = source.MiddleInitial;
    target.LastName = source.LastName;
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

  private void UseCabZeroFillNumber1()
  {
    var useImport = new CabZeroFillNumber.Import();
    var useExport = new CabZeroFillNumber.Export();

    useImport.CsePersonsWorkSet.Number = export.ApCsePersonsWorkSet.Number;

    Call(CabZeroFillNumber.Execute, useImport, useExport);

    export.ApCsePersonsWorkSet.Number = useImport.CsePersonsWorkSet.Number;
  }

  private void UseCabZeroFillNumber2()
  {
    var useImport = new CabZeroFillNumber.Import();
    var useExport = new CabZeroFillNumber.Export();

    useImport.Case1.Number = export.Next.Number;

    Call(CabZeroFillNumber.Execute, useImport, useExport);

    export.Next.Number = useImport.Case1.Number;
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
    useImport.NextTranInfo.Assign(export.Hidden);

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

    useImport.CsePersonsWorkSet.Number = export.ApCsePersonsWorkSet.Number;
    useImport.Case1.Number = export.Next.Number;

    Call(ScCabTestSecurity.Execute, useImport, useExport);
  }

  private void UseSiAltsBuildAliasAndSsn1()
  {
    var useImport = new SiAltsBuildAliasAndSsn2.Import();
    var useExport = new SiAltsBuildAliasAndSsn2.Export();

    MoveCsePersonsWorkSet1(local.ApCsePersonsWorkSet, useImport.Ap1);
    MoveCsePersonsWorkSet1(local.ArCsePersonsWorkSet, useImport.Ar1);
    MoveCsePersonsWorkSet1(local.ChCsePersonsWorkSet, useImport.Ch1);

    Call(SiAltsBuildAliasAndSsn2.Execute, useImport, useExport);

    export.NextKeyAp.UniqueKey = useExport.NextKeyAp.UniqueKey;
    export.NextKeyAr.UniqueKey = useExport.NextKeyAr.UniqueKey;
    useExport.Ap.CopyTo(export.Ap, MoveAp);
    useExport.Ar.CopyTo(export.Ar, MoveAr);
    useExport.Ch.CopyTo(export.Ch, MoveCh);
    export.NextKeyCh.UniqueKey = useExport.NextKeyCh.UniqueKey;
  }

  private void UseSiAltsBuildAliasAndSsn3()
  {
    var useImport = new SiAltsBuildAliasAndSsn2.Import();
    var useExport = new SiAltsBuildAliasAndSsn2.Export();

    MoveCsePersonsWorkSet1(local.ApCsePersonsWorkSet, useImport.Ap1);

    Call(SiAltsBuildAliasAndSsn2.Execute, useImport, useExport);

    export.NextKeyAp.UniqueKey = useExport.NextKeyAp.UniqueKey;
    useExport.Ap.CopyTo(export.Ap, MoveAp);
  }

  private void UseSiAltsBuildAliasAndSsn4()
  {
    var useImport = new SiAltsBuildAliasAndSsn2.Import();
    var useExport = new SiAltsBuildAliasAndSsn2.Export();

    MoveCsePersonsWorkSet1(local.ApCsePersonsWorkSet, useImport.Ap1);

    Call(SiAltsBuildAliasAndSsn2.Execute, useImport, useExport);

    useExport.Ap.CopyTo(export.Ap, MoveAp);
  }

  private void UseSiAltsBuildAliasAndSsn5()
  {
    var useImport = new SiAltsBuildAliasAndSsn2.Import();
    var useExport = new SiAltsBuildAliasAndSsn2.Export();

    MoveCsePersonsWorkSet1(local.ArCsePersonsWorkSet, useImport.Ar1);

    Call(SiAltsBuildAliasAndSsn2.Execute, useImport, useExport);

    export.NextKeyAr.UniqueKey = useExport.NextKeyAr.UniqueKey;
    useExport.Ar.CopyTo(export.Ar, MoveAr);
  }

  private void UseSiAltsBuildAliasAndSsn6()
  {
    var useImport = new SiAltsBuildAliasAndSsn2.Import();
    var useExport = new SiAltsBuildAliasAndSsn2.Export();

    MoveCsePersonsWorkSet1(local.ArCsePersonsWorkSet, useImport.Ar1);

    Call(SiAltsBuildAliasAndSsn2.Execute, useImport, useExport);

    useExport.Ar.CopyTo(export.Ar, MoveAr);
  }

  private void UseSiAltsBuildAliasAndSsn7()
  {
    var useImport = new SiAltsBuildAliasAndSsn2.Import();
    var useExport = new SiAltsBuildAliasAndSsn2.Export();

    MoveCsePersonsWorkSet1(local.ChCsePersonsWorkSet, useImport.Ch1);

    Call(SiAltsBuildAliasAndSsn2.Execute, useImport, useExport);

    useExport.Ch.CopyTo(export.Ch, MoveCh);
  }

  private void UseSiAltsBuildAliasAndSsn8()
  {
    var useImport = new SiAltsBuildAliasAndSsn2.Import();
    var useExport = new SiAltsBuildAliasAndSsn2.Export();

    MoveCsePersonsWorkSet1(local.ChCsePersonsWorkSet, useImport.Ch1);

    Call(SiAltsBuildAliasAndSsn2.Execute, useImport, useExport);

    useExport.Ch.CopyTo(export.Ch, MoveCh);
    export.NextKeyCh.UniqueKey = useExport.NextKeyCh.UniqueKey;
  }

  private void UseSiAltsCabCreateAlias()
  {
    var useImport = new SiAltsCabCreateAlias.Import();
    var useExport = new SiAltsCabCreateAlias.Export();

    useImport.CsePersonsWorkSet.Assign(local.CsePersonsWorkSet);

    Call(SiAltsCabCreateAlias.Execute, useImport, useExport);

    MoveCsePersonsWorkSet1(useExport.CsePersonsWorkSet, local.CsePersonsWorkSet);
      
  }

  private void UseSiAltsCabDeleteAlias()
  {
    var useImport = new SiAltsCabDeleteAlias.Import();
    var useExport = new SiAltsCabDeleteAlias.Export();

    MoveCsePersonsWorkSet1(local.CsePersonsWorkSet, useImport.CsePersonsWorkSet);
      

    Call(SiAltsCabDeleteAlias.Execute, useImport, useExport);
  }

  private void UseSiAltsCabUpdateAlias()
  {
    var useImport = new SiAltsCabUpdateAlias.Import();
    var useExport = new SiAltsCabUpdateAlias.Export();

    useImport.CsePersonsWorkSet.Assign(local.CsePersonsWorkSet);

    Call(SiAltsCabUpdateAlias.Execute, useImport, useExport);
  }

  private void UseSiAltsReadCaseHeaderInform()
  {
    var useImport = new SiAltsReadCaseHeaderInform.Import();
    var useExport = new SiAltsReadCaseHeaderInform.Export();

    useImport.Ap.Number = export.ApCsePersonsWorkSet.Number;
    useImport.Case1.Number = export.Case1.Number;
    useImport.Ch.Number = export.ChCsePersonsWorkSet.Number;
    useImport.Ar.Number = export.ArCsePersonsWorkSet.Number;

    Call(SiAltsReadCaseHeaderInform.Execute, useImport, useExport);

    export.ApActive.Flag = useExport.ApActive.Flag;
    export.CaseOpen.Flag = useExport.CaseOpen.Flag;
    local.MultipleAps.Flag = useExport.MultipleAps.Flag;
    export.ArCsePersonsWorkSet.Assign(useExport.Ar);
    export.ApCsePersonsWorkSet.Assign(useExport.Ap);
    export.ChCsePersonsWorkSet.Assign(useExport.Ch);
    local.MultipleChs.Flag = useExport.MultipleChs.Flag;
    export.ChActive.Flag = useExport.ChActive.Flag;
  }

  private void UseSiCheckName1()
  {
    var useImport = new SiCheckName.Import();
    var useExport = new SiCheckName.Export();

    MoveCsePersonsWorkSet4(export.Ap.Item.DetApCsePersonsWorkSet,
      useImport.CsePersonsWorkSet);

    Call(SiCheckName.Execute, useImport, useExport);
  }

  private void UseSiCheckName2()
  {
    var useImport = new SiCheckName.Import();
    var useExport = new SiCheckName.Export();

    MoveCsePersonsWorkSet4(export.Ar.Item.DetArCsePersonsWorkSet,
      useImport.CsePersonsWorkSet);

    Call(SiCheckName.Execute, useImport, useExport);
  }

  private void UseSiCheckName3()
  {
    var useImport = new SiCheckName.Import();
    var useExport = new SiCheckName.Export();

    MoveCsePersonsWorkSet4(export.Ch.Item.DetChCsePersonsWorkSet,
      useImport.CsePersonsWorkSet);

    Call(SiCheckName.Execute, useImport, useExport);
  }

  private void UseSiReadOfficeOspHeader()
  {
    var useImport = new SiReadOfficeOspHeader.Import();
    var useExport = new SiReadOfficeOspHeader.Export();

    useImport.Case1.Number = export.Case1.Number;

    Call(SiReadOfficeOspHeader.Execute, useImport, useExport);

    export.HeaderLine.Text35 = useExport.HeaderLine.Text35;
    export.ServiceProvider.LastName = useExport.ServiceProvider.LastName;
    MoveOffice(useExport.Office, export.Office);
  }

  private bool ReadInvalidSsn1()
  {
    entities.InvalidSsn.Populated = false;

    return Read("ReadInvalidSsn1",
      (db, command) =>
      {
        db.SetInt32(command, "ssn", local.Check.SsnNum9);
        db.SetString(command, "cspNumber", export.ApCsePersonsWorkSet.Number);
      },
      (db, reader) =>
      {
        entities.InvalidSsn.CspNumber = db.GetString(reader, 0);
        entities.InvalidSsn.Ssn = db.GetInt32(reader, 1);
        entities.InvalidSsn.Populated = true;
      });
  }

  private bool ReadInvalidSsn2()
  {
    entities.InvalidSsn.Populated = false;

    return Read("ReadInvalidSsn2",
      (db, command) =>
      {
        db.SetInt32(command, "ssn", local.Check.SsnNum9);
        db.SetString(command, "cspNumber", export.ArCsePersonsWorkSet.Number);
      },
      (db, reader) =>
      {
        entities.InvalidSsn.CspNumber = db.GetString(reader, 0);
        entities.InvalidSsn.Ssn = db.GetInt32(reader, 1);
        entities.InvalidSsn.Populated = true;
      });
  }

  private bool ReadInvalidSsn3()
  {
    entities.InvalidSsn.Populated = false;

    return Read("ReadInvalidSsn3",
      (db, command) =>
      {
        db.SetInt32(command, "ssn", local.Check.SsnNum9);
        db.SetString(command, "cspNumber", export.ChCsePersonsWorkSet.Number);
      },
      (db, reader) =>
      {
        entities.InvalidSsn.CspNumber = db.GetString(reader, 0);
        entities.InvalidSsn.Ssn = db.GetInt32(reader, 1);
        entities.InvalidSsn.Populated = true;
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
    /// <summary>A ApGroup group.</summary>
    [Serializable]
    public class ApGroup
    {
      /// <summary>
      /// A value of DetApCommon.
      /// </summary>
      [JsonPropertyName("detApCommon")]
      public Common DetApCommon
      {
        get => detApCommon ??= new();
        set => detApCommon = value;
      }

      /// <summary>
      /// A value of DetApCsePersonsWorkSet.
      /// </summary>
      [JsonPropertyName("detApCsePersonsWorkSet")]
      public CsePersonsWorkSet DetApCsePersonsWorkSet
      {
        get => detApCsePersonsWorkSet ??= new();
        set => detApCsePersonsWorkSet = value;
      }

      /// <summary>
      /// A value of ApSsn3.
      /// </summary>
      [JsonPropertyName("apSsn3")]
      public WorkArea ApSsn3
      {
        get => apSsn3 ??= new();
        set => apSsn3 = value;
      }

      /// <summary>
      /// A value of ApSsn2.
      /// </summary>
      [JsonPropertyName("apSsn2")]
      public WorkArea ApSsn2
      {
        get => apSsn2 ??= new();
        set => apSsn2 = value;
      }

      /// <summary>
      /// A value of ApSsn4.
      /// </summary>
      [JsonPropertyName("apSsn4")]
      public WorkArea ApSsn4
      {
        get => apSsn4 ??= new();
        set => apSsn4 = value;
      }

      /// <summary>
      /// A value of ApKscares.
      /// </summary>
      [JsonPropertyName("apKscares")]
      public Common ApKscares
      {
        get => apKscares ??= new();
        set => apKscares = value;
      }

      /// <summary>
      /// A value of ApKanpay.
      /// </summary>
      [JsonPropertyName("apKanpay")]
      public Common ApKanpay
      {
        get => apKanpay ??= new();
        set => apKanpay = value;
      }

      /// <summary>
      /// A value of ApCse.
      /// </summary>
      [JsonPropertyName("apCse")]
      public Common ApCse
      {
        get => apCse ??= new();
        set => apCse = value;
      }

      /// <summary>
      /// A value of ApAe.
      /// </summary>
      [JsonPropertyName("apAe")]
      public Common ApAe
      {
        get => apAe ??= new();
        set => apAe = value;
      }

      /// <summary>
      /// A value of ApFa.
      /// </summary>
      [JsonPropertyName("apFa")]
      public Common ApFa
      {
        get => apFa ??= new();
        set => apFa = value;
      }

      /// <summary>
      /// A value of ApDbOccurrence.
      /// </summary>
      [JsonPropertyName("apDbOccurrence")]
      public Common ApDbOccurrence
      {
        get => apDbOccurrence ??= new();
        set => apDbOccurrence = value;
      }

      /// <summary>
      /// A value of ApActiveOnKscares.
      /// </summary>
      [JsonPropertyName("apActiveOnKscares")]
      public Common ApActiveOnKscares
      {
        get => apActiveOnKscares ??= new();
        set => apActiveOnKscares = value;
      }

      /// <summary>
      /// A value of ApActiveOnKanpay.
      /// </summary>
      [JsonPropertyName("apActiveOnKanpay")]
      public Common ApActiveOnKanpay
      {
        get => apActiveOnKanpay ??= new();
        set => apActiveOnKanpay = value;
      }

      /// <summary>
      /// A value of ApActiveOnCse.
      /// </summary>
      [JsonPropertyName("apActiveOnCse")]
      public Common ApActiveOnCse
      {
        get => apActiveOnCse ??= new();
        set => apActiveOnCse = value;
      }

      /// <summary>
      /// A value of ApActiveOnAe.
      /// </summary>
      [JsonPropertyName("apActiveOnAe")]
      public Common ApActiveOnAe
      {
        get => apActiveOnAe ??= new();
        set => apActiveOnAe = value;
      }

      /// <summary>
      /// A value of ApActiveOnFacts.
      /// </summary>
      [JsonPropertyName("apActiveOnFacts")]
      public Common ApActiveOnFacts
      {
        get => apActiveOnFacts ??= new();
        set => apActiveOnFacts = value;
      }

      /// <summary>
      /// A value of DetApPrev.
      /// </summary>
      [JsonPropertyName("detApPrev")]
      public CsePersonsWorkSet DetApPrev
      {
        get => detApPrev ??= new();
        set => detApPrev = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 3;

      private Common detApCommon;
      private CsePersonsWorkSet detApCsePersonsWorkSet;
      private WorkArea apSsn3;
      private WorkArea apSsn2;
      private WorkArea apSsn4;
      private Common apKscares;
      private Common apKanpay;
      private Common apCse;
      private Common apAe;
      private Common apFa;
      private Common apDbOccurrence;
      private Common apActiveOnKscares;
      private Common apActiveOnKanpay;
      private Common apActiveOnCse;
      private Common apActiveOnAe;
      private Common apActiveOnFacts;
      private CsePersonsWorkSet detApPrev;
    }

    /// <summary>A ArGroup group.</summary>
    [Serializable]
    public class ArGroup
    {
      /// <summary>
      /// A value of DetArCommon.
      /// </summary>
      [JsonPropertyName("detArCommon")]
      public Common DetArCommon
      {
        get => detArCommon ??= new();
        set => detArCommon = value;
      }

      /// <summary>
      /// A value of DetArCsePersonsWorkSet.
      /// </summary>
      [JsonPropertyName("detArCsePersonsWorkSet")]
      public CsePersonsWorkSet DetArCsePersonsWorkSet
      {
        get => detArCsePersonsWorkSet ??= new();
        set => detArCsePersonsWorkSet = value;
      }

      /// <summary>
      /// A value of ArSsn3.
      /// </summary>
      [JsonPropertyName("arSsn3")]
      public WorkArea ArSsn3
      {
        get => arSsn3 ??= new();
        set => arSsn3 = value;
      }

      /// <summary>
      /// A value of ArSsn2.
      /// </summary>
      [JsonPropertyName("arSsn2")]
      public WorkArea ArSsn2
      {
        get => arSsn2 ??= new();
        set => arSsn2 = value;
      }

      /// <summary>
      /// A value of ArSsn4.
      /// </summary>
      [JsonPropertyName("arSsn4")]
      public WorkArea ArSsn4
      {
        get => arSsn4 ??= new();
        set => arSsn4 = value;
      }

      /// <summary>
      /// A value of ArKscares.
      /// </summary>
      [JsonPropertyName("arKscares")]
      public Common ArKscares
      {
        get => arKscares ??= new();
        set => arKscares = value;
      }

      /// <summary>
      /// A value of ArKanpay.
      /// </summary>
      [JsonPropertyName("arKanpay")]
      public Common ArKanpay
      {
        get => arKanpay ??= new();
        set => arKanpay = value;
      }

      /// <summary>
      /// A value of ArCse.
      /// </summary>
      [JsonPropertyName("arCse")]
      public Common ArCse
      {
        get => arCse ??= new();
        set => arCse = value;
      }

      /// <summary>
      /// A value of ArAe.
      /// </summary>
      [JsonPropertyName("arAe")]
      public Common ArAe
      {
        get => arAe ??= new();
        set => arAe = value;
      }

      /// <summary>
      /// A value of ArFa.
      /// </summary>
      [JsonPropertyName("arFa")]
      public Common ArFa
      {
        get => arFa ??= new();
        set => arFa = value;
      }

      /// <summary>
      /// A value of ArDbOccurrence.
      /// </summary>
      [JsonPropertyName("arDbOccurrence")]
      public Common ArDbOccurrence
      {
        get => arDbOccurrence ??= new();
        set => arDbOccurrence = value;
      }

      /// <summary>
      /// A value of ArActiveOnKscares.
      /// </summary>
      [JsonPropertyName("arActiveOnKscares")]
      public Common ArActiveOnKscares
      {
        get => arActiveOnKscares ??= new();
        set => arActiveOnKscares = value;
      }

      /// <summary>
      /// A value of ArActiveOnKanpay.
      /// </summary>
      [JsonPropertyName("arActiveOnKanpay")]
      public Common ArActiveOnKanpay
      {
        get => arActiveOnKanpay ??= new();
        set => arActiveOnKanpay = value;
      }

      /// <summary>
      /// A value of ArActiveOnCse.
      /// </summary>
      [JsonPropertyName("arActiveOnCse")]
      public Common ArActiveOnCse
      {
        get => arActiveOnCse ??= new();
        set => arActiveOnCse = value;
      }

      /// <summary>
      /// A value of ArActiveOnAe.
      /// </summary>
      [JsonPropertyName("arActiveOnAe")]
      public Common ArActiveOnAe
      {
        get => arActiveOnAe ??= new();
        set => arActiveOnAe = value;
      }

      /// <summary>
      /// A value of ArActiveOnFacts.
      /// </summary>
      [JsonPropertyName("arActiveOnFacts")]
      public Common ArActiveOnFacts
      {
        get => arActiveOnFacts ??= new();
        set => arActiveOnFacts = value;
      }

      /// <summary>
      /// A value of DetArPrev.
      /// </summary>
      [JsonPropertyName("detArPrev")]
      public CsePersonsWorkSet DetArPrev
      {
        get => detArPrev ??= new();
        set => detArPrev = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 3;

      private Common detArCommon;
      private CsePersonsWorkSet detArCsePersonsWorkSet;
      private WorkArea arSsn3;
      private WorkArea arSsn2;
      private WorkArea arSsn4;
      private Common arKscares;
      private Common arKanpay;
      private Common arCse;
      private Common arAe;
      private Common arFa;
      private Common arDbOccurrence;
      private Common arActiveOnKscares;
      private Common arActiveOnKanpay;
      private Common arActiveOnCse;
      private Common arActiveOnAe;
      private Common arActiveOnFacts;
      private CsePersonsWorkSet detArPrev;
    }

    /// <summary>A PageKeysApGroup group.</summary>
    [Serializable]
    public class PageKeysApGroup
    {
      /// <summary>
      /// A value of PageKeyAp.
      /// </summary>
      [JsonPropertyName("pageKeyAp")]
      public CsePersonsWorkSet PageKeyAp
      {
        get => pageKeyAp ??= new();
        set => pageKeyAp = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 15;

      private CsePersonsWorkSet pageKeyAp;
    }

    /// <summary>A PageKeysArGroup group.</summary>
    [Serializable]
    public class PageKeysArGroup
    {
      /// <summary>
      /// A value of PageKeyAr.
      /// </summary>
      [JsonPropertyName("pageKeyAr")]
      public CsePersonsWorkSet PageKeyAr
      {
        get => pageKeyAr ??= new();
        set => pageKeyAr = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 15;

      private CsePersonsWorkSet pageKeyAr;
    }

    /// <summary>A ChGroup group.</summary>
    [Serializable]
    public class ChGroup
    {
      /// <summary>
      /// A value of DetChCommon.
      /// </summary>
      [JsonPropertyName("detChCommon")]
      public Common DetChCommon
      {
        get => detChCommon ??= new();
        set => detChCommon = value;
      }

      /// <summary>
      /// A value of DetChCsePersonsWorkSet.
      /// </summary>
      [JsonPropertyName("detChCsePersonsWorkSet")]
      public CsePersonsWorkSet DetChCsePersonsWorkSet
      {
        get => detChCsePersonsWorkSet ??= new();
        set => detChCsePersonsWorkSet = value;
      }

      /// <summary>
      /// A value of ChSsn3.
      /// </summary>
      [JsonPropertyName("chSsn3")]
      public WorkArea ChSsn3
      {
        get => chSsn3 ??= new();
        set => chSsn3 = value;
      }

      /// <summary>
      /// A value of ChSsn2.
      /// </summary>
      [JsonPropertyName("chSsn2")]
      public WorkArea ChSsn2
      {
        get => chSsn2 ??= new();
        set => chSsn2 = value;
      }

      /// <summary>
      /// A value of ChSsn4.
      /// </summary>
      [JsonPropertyName("chSsn4")]
      public WorkArea ChSsn4
      {
        get => chSsn4 ??= new();
        set => chSsn4 = value;
      }

      /// <summary>
      /// A value of ChKscares.
      /// </summary>
      [JsonPropertyName("chKscares")]
      public Common ChKscares
      {
        get => chKscares ??= new();
        set => chKscares = value;
      }

      /// <summary>
      /// A value of ChKanpay.
      /// </summary>
      [JsonPropertyName("chKanpay")]
      public Common ChKanpay
      {
        get => chKanpay ??= new();
        set => chKanpay = value;
      }

      /// <summary>
      /// A value of ChCse.
      /// </summary>
      [JsonPropertyName("chCse")]
      public Common ChCse
      {
        get => chCse ??= new();
        set => chCse = value;
      }

      /// <summary>
      /// A value of ChFa.
      /// </summary>
      [JsonPropertyName("chFa")]
      public Common ChFa
      {
        get => chFa ??= new();
        set => chFa = value;
      }

      /// <summary>
      /// A value of ChAe.
      /// </summary>
      [JsonPropertyName("chAe")]
      public Common ChAe
      {
        get => chAe ??= new();
        set => chAe = value;
      }

      /// <summary>
      /// A value of ChDbOccurrence.
      /// </summary>
      [JsonPropertyName("chDbOccurrence")]
      public Common ChDbOccurrence
      {
        get => chDbOccurrence ??= new();
        set => chDbOccurrence = value;
      }

      /// <summary>
      /// A value of ChActiveOnKscares.
      /// </summary>
      [JsonPropertyName("chActiveOnKscares")]
      public Common ChActiveOnKscares
      {
        get => chActiveOnKscares ??= new();
        set => chActiveOnKscares = value;
      }

      /// <summary>
      /// A value of ChActiveOnKanpay.
      /// </summary>
      [JsonPropertyName("chActiveOnKanpay")]
      public Common ChActiveOnKanpay
      {
        get => chActiveOnKanpay ??= new();
        set => chActiveOnKanpay = value;
      }

      /// <summary>
      /// A value of ChActiveOnCse.
      /// </summary>
      [JsonPropertyName("chActiveOnCse")]
      public Common ChActiveOnCse
      {
        get => chActiveOnCse ??= new();
        set => chActiveOnCse = value;
      }

      /// <summary>
      /// A value of ChActiveOnAe.
      /// </summary>
      [JsonPropertyName("chActiveOnAe")]
      public Common ChActiveOnAe
      {
        get => chActiveOnAe ??= new();
        set => chActiveOnAe = value;
      }

      /// <summary>
      /// A value of ChActiveOnFacts.
      /// </summary>
      [JsonPropertyName("chActiveOnFacts")]
      public Common ChActiveOnFacts
      {
        get => chActiveOnFacts ??= new();
        set => chActiveOnFacts = value;
      }

      /// <summary>
      /// A value of DetChPrev.
      /// </summary>
      [JsonPropertyName("detChPrev")]
      public CsePersonsWorkSet DetChPrev
      {
        get => detChPrev ??= new();
        set => detChPrev = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 3;

      private Common detChCommon;
      private CsePersonsWorkSet detChCsePersonsWorkSet;
      private WorkArea chSsn3;
      private WorkArea chSsn2;
      private WorkArea chSsn4;
      private Common chKscares;
      private Common chKanpay;
      private Common chCse;
      private Common chFa;
      private Common chAe;
      private Common chDbOccurrence;
      private Common chActiveOnKscares;
      private Common chActiveOnKanpay;
      private Common chActiveOnCse;
      private Common chActiveOnAe;
      private Common chActiveOnFacts;
      private CsePersonsWorkSet detChPrev;
    }

    /// <summary>A PageKeysChGroup group.</summary>
    [Serializable]
    public class PageKeysChGroup
    {
      /// <summary>
      /// A value of PageKeyCh.
      /// </summary>
      [JsonPropertyName("pageKeyCh")]
      public CsePersonsWorkSet PageKeyCh
      {
        get => pageKeyCh ??= new();
        set => pageKeyCh = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 15;

      private CsePersonsWorkSet pageKeyCh;
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
    /// A value of Office.
    /// </summary>
    [JsonPropertyName("office")]
    public Office Office
    {
      get => office ??= new();
      set => office = value;
    }

    /// <summary>
    /// A value of NextKeyAr.
    /// </summary>
    [JsonPropertyName("nextKeyAr")]
    public CsePersonsWorkSet NextKeyAr
    {
      get => nextKeyAr ??= new();
      set => nextKeyAr = value;
    }

    /// <summary>
    /// A value of NextKeyAp.
    /// </summary>
    [JsonPropertyName("nextKeyAp")]
    public CsePersonsWorkSet NextKeyAp
    {
      get => nextKeyAp ??= new();
      set => nextKeyAp = value;
    }

    /// <summary>
    /// A value of Ae.
    /// </summary>
    [JsonPropertyName("ae")]
    public CsePersonsWorkSet Ae
    {
      get => ae ??= new();
      set => ae = value;
    }

    /// <summary>
    /// A value of ApMaint.
    /// </summary>
    [JsonPropertyName("apMaint")]
    public Common ApMaint
    {
      get => apMaint ??= new();
      set => apMaint = value;
    }

    /// <summary>
    /// A value of ArMaint.
    /// </summary>
    [JsonPropertyName("arMaint")]
    public Common ArMaint
    {
      get => arMaint ??= new();
      set => arMaint = value;
    }

    /// <summary>
    /// A value of ApPlus.
    /// </summary>
    [JsonPropertyName("apPlus")]
    public WorkArea ApPlus
    {
      get => apPlus ??= new();
      set => apPlus = value;
    }

    /// <summary>
    /// A value of ApMinus.
    /// </summary>
    [JsonPropertyName("apMinus")]
    public WorkArea ApMinus
    {
      get => apMinus ??= new();
      set => apMinus = value;
    }

    /// <summary>
    /// A value of ArPlus.
    /// </summary>
    [JsonPropertyName("arPlus")]
    public WorkArea ArPlus
    {
      get => arPlus ??= new();
      set => arPlus = value;
    }

    /// <summary>
    /// A value of ArMinus.
    /// </summary>
    [JsonPropertyName("arMinus")]
    public WorkArea ArMinus
    {
      get => arMinus ??= new();
      set => arMinus = value;
    }

    /// <summary>
    /// A value of ApPrompt.
    /// </summary>
    [JsonPropertyName("apPrompt")]
    public WorkArea ApPrompt
    {
      get => apPrompt ??= new();
      set => apPrompt = value;
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
    /// A value of HiddenAp.
    /// </summary>
    [JsonPropertyName("hiddenAp")]
    public CsePersonsWorkSet HiddenAp
    {
      get => hiddenAp ??= new();
      set => hiddenAp = value;
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
    /// A value of Case1.
    /// </summary>
    [JsonPropertyName("case1")]
    public Case1 Case1
    {
      get => case1 ??= new();
      set => case1 = value;
    }

    /// <summary>
    /// A value of ApStandard.
    /// </summary>
    [JsonPropertyName("apStandard")]
    public Standard ApStandard
    {
      get => apStandard ??= new();
      set => apStandard = value;
    }

    /// <summary>
    /// A value of ArStandard.
    /// </summary>
    [JsonPropertyName("arStandard")]
    public Standard ArStandard
    {
      get => arStandard ??= new();
      set => arStandard = value;
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
    /// Gets a value of Ap.
    /// </summary>
    [JsonIgnore]
    public Array<ApGroup> Ap => ap ??= new(ApGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Ap for json serialization.
    /// </summary>
    [JsonPropertyName("ap")]
    [Computed]
    public IList<ApGroup> Ap_Json
    {
      get => ap;
      set => Ap.Assign(value);
    }

    /// <summary>
    /// Gets a value of Ar.
    /// </summary>
    [JsonIgnore]
    public Array<ArGroup> Ar => ar ??= new(ArGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Ar for json serialization.
    /// </summary>
    [JsonPropertyName("ar")]
    [Computed]
    public IList<ArGroup> Ar_Json
    {
      get => ar;
      set => Ar.Assign(value);
    }

    /// <summary>
    /// Gets a value of PageKeysAp.
    /// </summary>
    [JsonIgnore]
    public Array<PageKeysApGroup> PageKeysAp => pageKeysAp ??= new(
      PageKeysApGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of PageKeysAp for json serialization.
    /// </summary>
    [JsonPropertyName("pageKeysAp")]
    [Computed]
    public IList<PageKeysApGroup> PageKeysAp_Json
    {
      get => pageKeysAp;
      set => PageKeysAp.Assign(value);
    }

    /// <summary>
    /// Gets a value of PageKeysAr.
    /// </summary>
    [JsonIgnore]
    public Array<PageKeysArGroup> PageKeysAr => pageKeysAr ??= new(
      PageKeysArGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of PageKeysAr for json serialization.
    /// </summary>
    [JsonPropertyName("pageKeysAr")]
    [Computed]
    public IList<PageKeysArGroup> PageKeysAr_Json
    {
      get => pageKeysAr;
      set => PageKeysAr.Assign(value);
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
    /// A value of ChCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("chCsePersonsWorkSet")]
    public CsePersonsWorkSet ChCsePersonsWorkSet
    {
      get => chCsePersonsWorkSet ??= new();
      set => chCsePersonsWorkSet = value;
    }

    /// <summary>
    /// Gets a value of Ch.
    /// </summary>
    [JsonIgnore]
    public Array<ChGroup> Ch => ch ??= new(ChGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Ch for json serialization.
    /// </summary>
    [JsonPropertyName("ch")]
    [Computed]
    public IList<ChGroup> Ch_Json
    {
      get => ch;
      set => Ch.Assign(value);
    }

    /// <summary>
    /// A value of ChPrompt.
    /// </summary>
    [JsonPropertyName("chPrompt")]
    public WorkArea ChPrompt
    {
      get => chPrompt ??= new();
      set => chPrompt = value;
    }

    /// <summary>
    /// A value of ChPlus.
    /// </summary>
    [JsonPropertyName("chPlus")]
    public WorkArea ChPlus
    {
      get => chPlus ??= new();
      set => chPlus = value;
    }

    /// <summary>
    /// A value of ChMinus.
    /// </summary>
    [JsonPropertyName("chMinus")]
    public WorkArea ChMinus
    {
      get => chMinus ??= new();
      set => chMinus = value;
    }

    /// <summary>
    /// A value of ArPrompt.
    /// </summary>
    [JsonPropertyName("arPrompt")]
    public WorkArea ArPrompt
    {
      get => arPrompt ??= new();
      set => arPrompt = value;
    }

    /// <summary>
    /// A value of NextKeyCh.
    /// </summary>
    [JsonPropertyName("nextKeyCh")]
    public CsePersonsWorkSet NextKeyCh
    {
      get => nextKeyCh ??= new();
      set => nextKeyCh = value;
    }

    /// <summary>
    /// Gets a value of PageKeysCh.
    /// </summary>
    [JsonIgnore]
    public Array<PageKeysChGroup> PageKeysCh => pageKeysCh ??= new(
      PageKeysChGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of PageKeysCh for json serialization.
    /// </summary>
    [JsonPropertyName("pageKeysCh")]
    [Computed]
    public IList<PageKeysChGroup> PageKeysCh_Json
    {
      get => pageKeysCh;
      set => PageKeysCh.Assign(value);
    }

    /// <summary>
    /// A value of ChMaint.
    /// </summary>
    [JsonPropertyName("chMaint")]
    public Common ChMaint
    {
      get => chMaint ??= new();
      set => chMaint = value;
    }

    /// <summary>
    /// A value of ChStandard.
    /// </summary>
    [JsonPropertyName("chStandard")]
    public Standard ChStandard
    {
      get => chStandard ??= new();
      set => chStandard = value;
    }

    /// <summary>
    /// A value of ChActive.
    /// </summary>
    [JsonPropertyName("chActive")]
    public Common ChActive
    {
      get => chActive ??= new();
      set => chActive = value;
    }

    /// <summary>
    /// A value of HiddenFlowViaPrompt.
    /// </summary>
    [JsonPropertyName("hiddenFlowViaPrompt")]
    public Common HiddenFlowViaPrompt
    {
      get => hiddenFlowViaPrompt ??= new();
      set => hiddenFlowViaPrompt = value;
    }

    /// <summary>
    /// A value of HiddenCh.
    /// </summary>
    [JsonPropertyName("hiddenCh")]
    public CsePersonsWorkSet HiddenCh
    {
      get => hiddenCh ??= new();
      set => hiddenCh = value;
    }

    private WorkArea headerLine;
    private ServiceProvider serviceProvider;
    private Office office;
    private CsePersonsWorkSet nextKeyAr;
    private CsePersonsWorkSet nextKeyAp;
    private CsePersonsWorkSet ae;
    private Common apMaint;
    private Common arMaint;
    private WorkArea apPlus;
    private WorkArea apMinus;
    private WorkArea arPlus;
    private WorkArea arMinus;
    private WorkArea apPrompt;
    private CsePersonsWorkSet arCsePersonsWorkSet;
    private CsePersonsWorkSet apCsePersonsWorkSet;
    private CsePersonsWorkSet hiddenAp;
    private Case1 next;
    private Case1 case1;
    private Standard apStandard;
    private Standard arStandard;
    private Case1 hiddenNext;
    private Standard standard;
    private NextTranInfo hidden;
    private Array<ApGroup> ap;
    private Array<ArGroup> ar;
    private Array<PageKeysApGroup> pageKeysAp;
    private Array<PageKeysArGroup> pageKeysAr;
    private Common apActive;
    private Common caseOpen;
    private CsePersonsWorkSet chCsePersonsWorkSet;
    private Array<ChGroup> ch;
    private WorkArea chPrompt;
    private WorkArea chPlus;
    private WorkArea chMinus;
    private WorkArea arPrompt;
    private CsePersonsWorkSet nextKeyCh;
    private Array<PageKeysChGroup> pageKeysCh;
    private Common chMaint;
    private Standard chStandard;
    private Common chActive;
    private Common hiddenFlowViaPrompt;
    private CsePersonsWorkSet hiddenCh;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>A ApGroup group.</summary>
    [Serializable]
    public class ApGroup
    {
      /// <summary>
      /// A value of DetApCommon.
      /// </summary>
      [JsonPropertyName("detApCommon")]
      public Common DetApCommon
      {
        get => detApCommon ??= new();
        set => detApCommon = value;
      }

      /// <summary>
      /// A value of DetApCsePersonsWorkSet.
      /// </summary>
      [JsonPropertyName("detApCsePersonsWorkSet")]
      public CsePersonsWorkSet DetApCsePersonsWorkSet
      {
        get => detApCsePersonsWorkSet ??= new();
        set => detApCsePersonsWorkSet = value;
      }

      /// <summary>
      /// A value of ApSsn3.
      /// </summary>
      [JsonPropertyName("apSsn3")]
      public WorkArea ApSsn3
      {
        get => apSsn3 ??= new();
        set => apSsn3 = value;
      }

      /// <summary>
      /// A value of ApSsn2.
      /// </summary>
      [JsonPropertyName("apSsn2")]
      public WorkArea ApSsn2
      {
        get => apSsn2 ??= new();
        set => apSsn2 = value;
      }

      /// <summary>
      /// A value of ApSsn4.
      /// </summary>
      [JsonPropertyName("apSsn4")]
      public WorkArea ApSsn4
      {
        get => apSsn4 ??= new();
        set => apSsn4 = value;
      }

      /// <summary>
      /// A value of ApKscares.
      /// </summary>
      [JsonPropertyName("apKscares")]
      public Common ApKscares
      {
        get => apKscares ??= new();
        set => apKscares = value;
      }

      /// <summary>
      /// A value of ApKanpay.
      /// </summary>
      [JsonPropertyName("apKanpay")]
      public Common ApKanpay
      {
        get => apKanpay ??= new();
        set => apKanpay = value;
      }

      /// <summary>
      /// A value of ApCse.
      /// </summary>
      [JsonPropertyName("apCse")]
      public Common ApCse
      {
        get => apCse ??= new();
        set => apCse = value;
      }

      /// <summary>
      /// A value of ApAe.
      /// </summary>
      [JsonPropertyName("apAe")]
      public Common ApAe
      {
        get => apAe ??= new();
        set => apAe = value;
      }

      /// <summary>
      /// A value of ApFa.
      /// </summary>
      [JsonPropertyName("apFa")]
      public Common ApFa
      {
        get => apFa ??= new();
        set => apFa = value;
      }

      /// <summary>
      /// A value of ApDbOccurrence.
      /// </summary>
      [JsonPropertyName("apDbOccurrence")]
      public Common ApDbOccurrence
      {
        get => apDbOccurrence ??= new();
        set => apDbOccurrence = value;
      }

      /// <summary>
      /// A value of ApActiveOnKscares.
      /// </summary>
      [JsonPropertyName("apActiveOnKscares")]
      public Common ApActiveOnKscares
      {
        get => apActiveOnKscares ??= new();
        set => apActiveOnKscares = value;
      }

      /// <summary>
      /// A value of ApActiveOnKanpay.
      /// </summary>
      [JsonPropertyName("apActiveOnKanpay")]
      public Common ApActiveOnKanpay
      {
        get => apActiveOnKanpay ??= new();
        set => apActiveOnKanpay = value;
      }

      /// <summary>
      /// A value of ApActiveOnCse.
      /// </summary>
      [JsonPropertyName("apActiveOnCse")]
      public Common ApActiveOnCse
      {
        get => apActiveOnCse ??= new();
        set => apActiveOnCse = value;
      }

      /// <summary>
      /// A value of ApActiveOnAe.
      /// </summary>
      [JsonPropertyName("apActiveOnAe")]
      public Common ApActiveOnAe
      {
        get => apActiveOnAe ??= new();
        set => apActiveOnAe = value;
      }

      /// <summary>
      /// A value of ApActiveOnFacts.
      /// </summary>
      [JsonPropertyName("apActiveOnFacts")]
      public Common ApActiveOnFacts
      {
        get => apActiveOnFacts ??= new();
        set => apActiveOnFacts = value;
      }

      /// <summary>
      /// A value of DetApPrev.
      /// </summary>
      [JsonPropertyName("detApPrev")]
      public CsePersonsWorkSet DetApPrev
      {
        get => detApPrev ??= new();
        set => detApPrev = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 3;

      private Common detApCommon;
      private CsePersonsWorkSet detApCsePersonsWorkSet;
      private WorkArea apSsn3;
      private WorkArea apSsn2;
      private WorkArea apSsn4;
      private Common apKscares;
      private Common apKanpay;
      private Common apCse;
      private Common apAe;
      private Common apFa;
      private Common apDbOccurrence;
      private Common apActiveOnKscares;
      private Common apActiveOnKanpay;
      private Common apActiveOnCse;
      private Common apActiveOnAe;
      private Common apActiveOnFacts;
      private CsePersonsWorkSet detApPrev;
    }

    /// <summary>A ArGroup group.</summary>
    [Serializable]
    public class ArGroup
    {
      /// <summary>
      /// A value of DetArCommon.
      /// </summary>
      [JsonPropertyName("detArCommon")]
      public Common DetArCommon
      {
        get => detArCommon ??= new();
        set => detArCommon = value;
      }

      /// <summary>
      /// A value of DetArCsePersonsWorkSet.
      /// </summary>
      [JsonPropertyName("detArCsePersonsWorkSet")]
      public CsePersonsWorkSet DetArCsePersonsWorkSet
      {
        get => detArCsePersonsWorkSet ??= new();
        set => detArCsePersonsWorkSet = value;
      }

      /// <summary>
      /// A value of ArSsn3.
      /// </summary>
      [JsonPropertyName("arSsn3")]
      public WorkArea ArSsn3
      {
        get => arSsn3 ??= new();
        set => arSsn3 = value;
      }

      /// <summary>
      /// A value of ArSsn2.
      /// </summary>
      [JsonPropertyName("arSsn2")]
      public WorkArea ArSsn2
      {
        get => arSsn2 ??= new();
        set => arSsn2 = value;
      }

      /// <summary>
      /// A value of ArSsn4.
      /// </summary>
      [JsonPropertyName("arSsn4")]
      public WorkArea ArSsn4
      {
        get => arSsn4 ??= new();
        set => arSsn4 = value;
      }

      /// <summary>
      /// A value of ArKscares.
      /// </summary>
      [JsonPropertyName("arKscares")]
      public Common ArKscares
      {
        get => arKscares ??= new();
        set => arKscares = value;
      }

      /// <summary>
      /// A value of ArKanpay.
      /// </summary>
      [JsonPropertyName("arKanpay")]
      public Common ArKanpay
      {
        get => arKanpay ??= new();
        set => arKanpay = value;
      }

      /// <summary>
      /// A value of ArCse.
      /// </summary>
      [JsonPropertyName("arCse")]
      public Common ArCse
      {
        get => arCse ??= new();
        set => arCse = value;
      }

      /// <summary>
      /// A value of ArAe.
      /// </summary>
      [JsonPropertyName("arAe")]
      public Common ArAe
      {
        get => arAe ??= new();
        set => arAe = value;
      }

      /// <summary>
      /// A value of ArFa.
      /// </summary>
      [JsonPropertyName("arFa")]
      public Common ArFa
      {
        get => arFa ??= new();
        set => arFa = value;
      }

      /// <summary>
      /// A value of ArDbOccurrence.
      /// </summary>
      [JsonPropertyName("arDbOccurrence")]
      public Common ArDbOccurrence
      {
        get => arDbOccurrence ??= new();
        set => arDbOccurrence = value;
      }

      /// <summary>
      /// A value of ArActiveOnKscares.
      /// </summary>
      [JsonPropertyName("arActiveOnKscares")]
      public Common ArActiveOnKscares
      {
        get => arActiveOnKscares ??= new();
        set => arActiveOnKscares = value;
      }

      /// <summary>
      /// A value of ArActiveOnKanpay.
      /// </summary>
      [JsonPropertyName("arActiveOnKanpay")]
      public Common ArActiveOnKanpay
      {
        get => arActiveOnKanpay ??= new();
        set => arActiveOnKanpay = value;
      }

      /// <summary>
      /// A value of ArActiveOnCse.
      /// </summary>
      [JsonPropertyName("arActiveOnCse")]
      public Common ArActiveOnCse
      {
        get => arActiveOnCse ??= new();
        set => arActiveOnCse = value;
      }

      /// <summary>
      /// A value of ArActiveOnAe.
      /// </summary>
      [JsonPropertyName("arActiveOnAe")]
      public Common ArActiveOnAe
      {
        get => arActiveOnAe ??= new();
        set => arActiveOnAe = value;
      }

      /// <summary>
      /// A value of ArActiveOnFacts.
      /// </summary>
      [JsonPropertyName("arActiveOnFacts")]
      public Common ArActiveOnFacts
      {
        get => arActiveOnFacts ??= new();
        set => arActiveOnFacts = value;
      }

      /// <summary>
      /// A value of DetArPrev.
      /// </summary>
      [JsonPropertyName("detArPrev")]
      public CsePersonsWorkSet DetArPrev
      {
        get => detArPrev ??= new();
        set => detArPrev = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 3;

      private Common detArCommon;
      private CsePersonsWorkSet detArCsePersonsWorkSet;
      private WorkArea arSsn3;
      private WorkArea arSsn2;
      private WorkArea arSsn4;
      private Common arKscares;
      private Common arKanpay;
      private Common arCse;
      private Common arAe;
      private Common arFa;
      private Common arDbOccurrence;
      private Common arActiveOnKscares;
      private Common arActiveOnKanpay;
      private Common arActiveOnCse;
      private Common arActiveOnAe;
      private Common arActiveOnFacts;
      private CsePersonsWorkSet detArPrev;
    }

    /// <summary>A PageKeysApGroup group.</summary>
    [Serializable]
    public class PageKeysApGroup
    {
      /// <summary>
      /// A value of PageKeyAp.
      /// </summary>
      [JsonPropertyName("pageKeyAp")]
      public CsePersonsWorkSet PageKeyAp
      {
        get => pageKeyAp ??= new();
        set => pageKeyAp = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 15;

      private CsePersonsWorkSet pageKeyAp;
    }

    /// <summary>A PageKeysArGroup group.</summary>
    [Serializable]
    public class PageKeysArGroup
    {
      /// <summary>
      /// A value of PageKeyAr.
      /// </summary>
      [JsonPropertyName("pageKeyAr")]
      public CsePersonsWorkSet PageKeyAr
      {
        get => pageKeyAr ??= new();
        set => pageKeyAr = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 15;

      private CsePersonsWorkSet pageKeyAr;
    }

    /// <summary>A ChGroup group.</summary>
    [Serializable]
    public class ChGroup
    {
      /// <summary>
      /// A value of DetChCommon.
      /// </summary>
      [JsonPropertyName("detChCommon")]
      public Common DetChCommon
      {
        get => detChCommon ??= new();
        set => detChCommon = value;
      }

      /// <summary>
      /// A value of DetChCsePersonsWorkSet.
      /// </summary>
      [JsonPropertyName("detChCsePersonsWorkSet")]
      public CsePersonsWorkSet DetChCsePersonsWorkSet
      {
        get => detChCsePersonsWorkSet ??= new();
        set => detChCsePersonsWorkSet = value;
      }

      /// <summary>
      /// A value of ChSsn3.
      /// </summary>
      [JsonPropertyName("chSsn3")]
      public WorkArea ChSsn3
      {
        get => chSsn3 ??= new();
        set => chSsn3 = value;
      }

      /// <summary>
      /// A value of ChSsn2.
      /// </summary>
      [JsonPropertyName("chSsn2")]
      public WorkArea ChSsn2
      {
        get => chSsn2 ??= new();
        set => chSsn2 = value;
      }

      /// <summary>
      /// A value of ChSsn4.
      /// </summary>
      [JsonPropertyName("chSsn4")]
      public WorkArea ChSsn4
      {
        get => chSsn4 ??= new();
        set => chSsn4 = value;
      }

      /// <summary>
      /// A value of ChKscares.
      /// </summary>
      [JsonPropertyName("chKscares")]
      public Common ChKscares
      {
        get => chKscares ??= new();
        set => chKscares = value;
      }

      /// <summary>
      /// A value of ChKanpay.
      /// </summary>
      [JsonPropertyName("chKanpay")]
      public Common ChKanpay
      {
        get => chKanpay ??= new();
        set => chKanpay = value;
      }

      /// <summary>
      /// A value of ChCse.
      /// </summary>
      [JsonPropertyName("chCse")]
      public Common ChCse
      {
        get => chCse ??= new();
        set => chCse = value;
      }

      /// <summary>
      /// A value of ChAe.
      /// </summary>
      [JsonPropertyName("chAe")]
      public Common ChAe
      {
        get => chAe ??= new();
        set => chAe = value;
      }

      /// <summary>
      /// A value of ChFa.
      /// </summary>
      [JsonPropertyName("chFa")]
      public Common ChFa
      {
        get => chFa ??= new();
        set => chFa = value;
      }

      /// <summary>
      /// A value of ChDbOccurrence.
      /// </summary>
      [JsonPropertyName("chDbOccurrence")]
      public Common ChDbOccurrence
      {
        get => chDbOccurrence ??= new();
        set => chDbOccurrence = value;
      }

      /// <summary>
      /// A value of ChActiveOnKscares.
      /// </summary>
      [JsonPropertyName("chActiveOnKscares")]
      public Common ChActiveOnKscares
      {
        get => chActiveOnKscares ??= new();
        set => chActiveOnKscares = value;
      }

      /// <summary>
      /// A value of ChActiveOnKanpay.
      /// </summary>
      [JsonPropertyName("chActiveOnKanpay")]
      public Common ChActiveOnKanpay
      {
        get => chActiveOnKanpay ??= new();
        set => chActiveOnKanpay = value;
      }

      /// <summary>
      /// A value of ChActiveOnCse.
      /// </summary>
      [JsonPropertyName("chActiveOnCse")]
      public Common ChActiveOnCse
      {
        get => chActiveOnCse ??= new();
        set => chActiveOnCse = value;
      }

      /// <summary>
      /// A value of ChActiveOnAe.
      /// </summary>
      [JsonPropertyName("chActiveOnAe")]
      public Common ChActiveOnAe
      {
        get => chActiveOnAe ??= new();
        set => chActiveOnAe = value;
      }

      /// <summary>
      /// A value of ChActiveOnFacts.
      /// </summary>
      [JsonPropertyName("chActiveOnFacts")]
      public Common ChActiveOnFacts
      {
        get => chActiveOnFacts ??= new();
        set => chActiveOnFacts = value;
      }

      /// <summary>
      /// A value of DetChPrev.
      /// </summary>
      [JsonPropertyName("detChPrev")]
      public CsePersonsWorkSet DetChPrev
      {
        get => detChPrev ??= new();
        set => detChPrev = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 3;

      private Common detChCommon;
      private CsePersonsWorkSet detChCsePersonsWorkSet;
      private WorkArea chSsn3;
      private WorkArea chSsn2;
      private WorkArea chSsn4;
      private Common chKscares;
      private Common chKanpay;
      private Common chCse;
      private Common chAe;
      private Common chFa;
      private Common chDbOccurrence;
      private Common chActiveOnKscares;
      private Common chActiveOnKanpay;
      private Common chActiveOnCse;
      private Common chActiveOnAe;
      private Common chActiveOnFacts;
      private CsePersonsWorkSet detChPrev;
    }

    /// <summary>A PageKeysChGroup group.</summary>
    [Serializable]
    public class PageKeysChGroup
    {
      /// <summary>
      /// A value of PageKeyCh.
      /// </summary>
      [JsonPropertyName("pageKeyCh")]
      public CsePersonsWorkSet PageKeyCh
      {
        get => pageKeyCh ??= new();
        set => pageKeyCh = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 15;

      private CsePersonsWorkSet pageKeyCh;
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
    /// A value of Office.
    /// </summary>
    [JsonPropertyName("office")]
    public Office Office
    {
      get => office ??= new();
      set => office = value;
    }

    /// <summary>
    /// A value of NextKeyAr.
    /// </summary>
    [JsonPropertyName("nextKeyAr")]
    public CsePersonsWorkSet NextKeyAr
    {
      get => nextKeyAr ??= new();
      set => nextKeyAr = value;
    }

    /// <summary>
    /// A value of NextKeyAp.
    /// </summary>
    [JsonPropertyName("nextKeyAp")]
    public CsePersonsWorkSet NextKeyAp
    {
      get => nextKeyAp ??= new();
      set => nextKeyAp = value;
    }

    /// <summary>
    /// A value of ApMaint.
    /// </summary>
    [JsonPropertyName("apMaint")]
    public Common ApMaint
    {
      get => apMaint ??= new();
      set => apMaint = value;
    }

    /// <summary>
    /// A value of ArMaint.
    /// </summary>
    [JsonPropertyName("arMaint")]
    public Common ArMaint
    {
      get => arMaint ??= new();
      set => arMaint = value;
    }

    /// <summary>
    /// A value of ApPlus.
    /// </summary>
    [JsonPropertyName("apPlus")]
    public WorkArea ApPlus
    {
      get => apPlus ??= new();
      set => apPlus = value;
    }

    /// <summary>
    /// A value of ApMinus.
    /// </summary>
    [JsonPropertyName("apMinus")]
    public WorkArea ApMinus
    {
      get => apMinus ??= new();
      set => apMinus = value;
    }

    /// <summary>
    /// A value of ArPlus.
    /// </summary>
    [JsonPropertyName("arPlus")]
    public WorkArea ArPlus
    {
      get => arPlus ??= new();
      set => arPlus = value;
    }

    /// <summary>
    /// A value of ArMinus.
    /// </summary>
    [JsonPropertyName("arMinus")]
    public WorkArea ArMinus
    {
      get => arMinus ??= new();
      set => arMinus = value;
    }

    /// <summary>
    /// A value of ApPrompt.
    /// </summary>
    [JsonPropertyName("apPrompt")]
    public WorkArea ApPrompt
    {
      get => apPrompt ??= new();
      set => apPrompt = value;
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
    /// A value of HiddenAp.
    /// </summary>
    [JsonPropertyName("hiddenAp")]
    public CsePersonsWorkSet HiddenAp
    {
      get => hiddenAp ??= new();
      set => hiddenAp = value;
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
    /// A value of Case1.
    /// </summary>
    [JsonPropertyName("case1")]
    public Case1 Case1
    {
      get => case1 ??= new();
      set => case1 = value;
    }

    /// <summary>
    /// A value of ApStandard.
    /// </summary>
    [JsonPropertyName("apStandard")]
    public Standard ApStandard
    {
      get => apStandard ??= new();
      set => apStandard = value;
    }

    /// <summary>
    /// A value of ArStandard.
    /// </summary>
    [JsonPropertyName("arStandard")]
    public Standard ArStandard
    {
      get => arStandard ??= new();
      set => arStandard = value;
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
    /// Gets a value of Ap.
    /// </summary>
    [JsonIgnore]
    public Array<ApGroup> Ap => ap ??= new(ApGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Ap for json serialization.
    /// </summary>
    [JsonPropertyName("ap")]
    [Computed]
    public IList<ApGroup> Ap_Json
    {
      get => ap;
      set => Ap.Assign(value);
    }

    /// <summary>
    /// Gets a value of Ar.
    /// </summary>
    [JsonIgnore]
    public Array<ArGroup> Ar => ar ??= new(ArGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Ar for json serialization.
    /// </summary>
    [JsonPropertyName("ar")]
    [Computed]
    public IList<ArGroup> Ar_Json
    {
      get => ar;
      set => Ar.Assign(value);
    }

    /// <summary>
    /// Gets a value of PageKeysAp.
    /// </summary>
    [JsonIgnore]
    public Array<PageKeysApGroup> PageKeysAp => pageKeysAp ??= new(
      PageKeysApGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of PageKeysAp for json serialization.
    /// </summary>
    [JsonPropertyName("pageKeysAp")]
    [Computed]
    public IList<PageKeysApGroup> PageKeysAp_Json
    {
      get => pageKeysAp;
      set => PageKeysAp.Assign(value);
    }

    /// <summary>
    /// Gets a value of PageKeysAr.
    /// </summary>
    [JsonIgnore]
    public Array<PageKeysArGroup> PageKeysAr => pageKeysAr ??= new(
      PageKeysArGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of PageKeysAr for json serialization.
    /// </summary>
    [JsonPropertyName("pageKeysAr")]
    [Computed]
    public IList<PageKeysArGroup> PageKeysAr_Json
    {
      get => pageKeysAr;
      set => PageKeysAr.Assign(value);
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
    /// A value of ChCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("chCsePersonsWorkSet")]
    public CsePersonsWorkSet ChCsePersonsWorkSet
    {
      get => chCsePersonsWorkSet ??= new();
      set => chCsePersonsWorkSet = value;
    }

    /// <summary>
    /// Gets a value of Ch.
    /// </summary>
    [JsonIgnore]
    public Array<ChGroup> Ch => ch ??= new(ChGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Ch for json serialization.
    /// </summary>
    [JsonPropertyName("ch")]
    [Computed]
    public IList<ChGroup> Ch_Json
    {
      get => ch;
      set => Ch.Assign(value);
    }

    /// <summary>
    /// A value of ChPrompt.
    /// </summary>
    [JsonPropertyName("chPrompt")]
    public WorkArea ChPrompt
    {
      get => chPrompt ??= new();
      set => chPrompt = value;
    }

    /// <summary>
    /// A value of ChPlus.
    /// </summary>
    [JsonPropertyName("chPlus")]
    public WorkArea ChPlus
    {
      get => chPlus ??= new();
      set => chPlus = value;
    }

    /// <summary>
    /// A value of ChMinus.
    /// </summary>
    [JsonPropertyName("chMinus")]
    public WorkArea ChMinus
    {
      get => chMinus ??= new();
      set => chMinus = value;
    }

    /// <summary>
    /// A value of ArPrompt.
    /// </summary>
    [JsonPropertyName("arPrompt")]
    public WorkArea ArPrompt
    {
      get => arPrompt ??= new();
      set => arPrompt = value;
    }

    /// <summary>
    /// A value of NextKeyCh.
    /// </summary>
    [JsonPropertyName("nextKeyCh")]
    public CsePersonsWorkSet NextKeyCh
    {
      get => nextKeyCh ??= new();
      set => nextKeyCh = value;
    }

    /// <summary>
    /// Gets a value of PageKeysCh.
    /// </summary>
    [JsonIgnore]
    public Array<PageKeysChGroup> PageKeysCh => pageKeysCh ??= new(
      PageKeysChGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of PageKeysCh for json serialization.
    /// </summary>
    [JsonPropertyName("pageKeysCh")]
    [Computed]
    public IList<PageKeysChGroup> PageKeysCh_Json
    {
      get => pageKeysCh;
      set => PageKeysCh.Assign(value);
    }

    /// <summary>
    /// A value of ChMaint.
    /// </summary>
    [JsonPropertyName("chMaint")]
    public Common ChMaint
    {
      get => chMaint ??= new();
      set => chMaint = value;
    }

    /// <summary>
    /// A value of ChStandard.
    /// </summary>
    [JsonPropertyName("chStandard")]
    public Standard ChStandard
    {
      get => chStandard ??= new();
      set => chStandard = value;
    }

    /// <summary>
    /// A value of ChActive.
    /// </summary>
    [JsonPropertyName("chActive")]
    public Common ChActive
    {
      get => chActive ??= new();
      set => chActive = value;
    }

    /// <summary>
    /// A value of HiddenFromAlts.
    /// </summary>
    [JsonPropertyName("hiddenFromAlts")]
    public Common HiddenFromAlts
    {
      get => hiddenFromAlts ??= new();
      set => hiddenFromAlts = value;
    }

    /// <summary>
    /// A value of HiddenFlowViaPrompt.
    /// </summary>
    [JsonPropertyName("hiddenFlowViaPrompt")]
    public Common HiddenFlowViaPrompt
    {
      get => hiddenFlowViaPrompt ??= new();
      set => hiddenFlowViaPrompt = value;
    }

    /// <summary>
    /// A value of HiddenCh.
    /// </summary>
    [JsonPropertyName("hiddenCh")]
    public CsePersonsWorkSet HiddenCh
    {
      get => hiddenCh ??= new();
      set => hiddenCh = value;
    }

    private WorkArea headerLine;
    private ServiceProvider serviceProvider;
    private Office office;
    private CsePersonsWorkSet nextKeyAr;
    private CsePersonsWorkSet nextKeyAp;
    private Common apMaint;
    private Common arMaint;
    private WorkArea apPlus;
    private WorkArea apMinus;
    private WorkArea arPlus;
    private WorkArea arMinus;
    private WorkArea apPrompt;
    private CsePersonsWorkSet arCsePersonsWorkSet;
    private CsePersonsWorkSet apCsePersonsWorkSet;
    private CsePersonsWorkSet hiddenAp;
    private Case1 next;
    private Case1 case1;
    private Standard apStandard;
    private Standard arStandard;
    private Case1 hiddenNext;
    private Standard standard;
    private NextTranInfo hidden;
    private Array<ApGroup> ap;
    private Array<ArGroup> ar;
    private Array<PageKeysApGroup> pageKeysAp;
    private Array<PageKeysArGroup> pageKeysAr;
    private Common apActive;
    private Common caseOpen;
    private CsePersonsWorkSet chCsePersonsWorkSet;
    private Array<ChGroup> ch;
    private WorkArea chPrompt;
    private WorkArea chPlus;
    private WorkArea chMinus;
    private WorkArea arPrompt;
    private CsePersonsWorkSet nextKeyCh;
    private Array<PageKeysChGroup> pageKeysCh;
    private Common chMaint;
    private Standard chStandard;
    private Common chActive;
    private Common hiddenFromAlts;
    private Common hiddenFlowViaPrompt;
    private CsePersonsWorkSet hiddenCh;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of Check.
    /// </summary>
    [JsonPropertyName("check")]
    public SsnWorkArea Check
    {
      get => check ??= new();
      set => check = value;
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
    /// A value of CsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("csePersonsWorkSet")]
    public CsePersonsWorkSet CsePersonsWorkSet
    {
      get => csePersonsWorkSet ??= new();
      set => csePersonsWorkSet = value;
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
    /// A value of MultipleAps.
    /// </summary>
    [JsonPropertyName("multipleAps")]
    public Common MultipleAps
    {
      get => multipleAps ??= new();
      set => multipleAps = value;
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
    /// A value of ArCommon.
    /// </summary>
    [JsonPropertyName("arCommon")]
    public Common ArCommon
    {
      get => arCommon ??= new();
      set => arCommon = value;
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
    /// A value of Selection.
    /// </summary>
    [JsonPropertyName("selection")]
    public Common Selection
    {
      get => selection ??= new();
      set => selection = value;
    }

    /// <summary>
    /// A value of ChCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("chCsePersonsWorkSet")]
    public CsePersonsWorkSet ChCsePersonsWorkSet
    {
      get => chCsePersonsWorkSet ??= new();
      set => chCsePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of ChCommon.
    /// </summary>
    [JsonPropertyName("chCommon")]
    public Common ChCommon
    {
      get => chCommon ??= new();
      set => chCommon = value;
    }

    /// <summary>
    /// A value of MultipleChs.
    /// </summary>
    [JsonPropertyName("multipleChs")]
    public Common MultipleChs
    {
      get => multipleChs ??= new();
      set => multipleChs = value;
    }

    /// <summary>
    /// A value of Ssn3.
    /// </summary>
    [JsonPropertyName("ssn3")]
    public WorkArea Ssn3
    {
      get => ssn3 ??= new();
      set => ssn3 = value;
    }

    /// <summary>
    /// A value of Ssn2.
    /// </summary>
    [JsonPropertyName("ssn2")]
    public WorkArea Ssn2
    {
      get => ssn2 ??= new();
      set => ssn2 = value;
    }

    /// <summary>
    /// A value of Ssn4.
    /// </summary>
    [JsonPropertyName("ssn4")]
    public WorkArea Ssn4
    {
      get => ssn4 ??= new();
      set => ssn4 = value;
    }

    /// <summary>
    /// A value of DbOccurrence.
    /// </summary>
    [JsonPropertyName("dbOccurrence")]
    public Common DbOccurrence
    {
      get => dbOccurrence ??= new();
      set => dbOccurrence = value;
    }

    private SsnWorkArea check;
    private DateWorkArea zero;
    private CsePersonsWorkSet csePersonsWorkSet;
    private CsePersonsWorkSet apCsePersonsWorkSet;
    private CsePersonsWorkSet arCsePersonsWorkSet;
    private Common multipleAps;
    private Common error;
    private Common arCommon;
    private Common apCommon;
    private Common selection;
    private CsePersonsWorkSet chCsePersonsWorkSet;
    private Common chCommon;
    private Common multipleChs;
    private WorkArea ssn3;
    private WorkArea ssn2;
    private WorkArea ssn4;
    private Common dbOccurrence;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of InvalidSsn.
    /// </summary>
    [JsonPropertyName("invalidSsn")]
    public InvalidSsn InvalidSsn
    {
      get => invalidSsn ??= new();
      set => invalidSsn = value;
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

    private InvalidSsn invalidSsn;
    private CsePerson csePerson;
  }
#endregion
}
