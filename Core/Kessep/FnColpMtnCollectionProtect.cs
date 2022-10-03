// Program: FN_COLP_MTN_COLLECTION_PROTECT, ID: 373388596, model: 746.
// Short name: SWECOLPP
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
/// A program: FN_COLP_MTN_COLLECTION_PROTECT.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class FnColpMtnCollectionProtect: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_COLP_MTN_COLLECTION_PROTECT program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnColpMtnCollectionProtect(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnColpMtnCollectionProtect.
  /// </summary>
  public FnColpMtnCollectionProtect(IContext context, Import import,
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
    // ----------------------------------------------------------------
    // Date     Developer Name   Request#  Description
    // 12/20/01  Mark Ashworth   WR10504   New Development Used MDIS as 
    // template.
    // ----------------------------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";
    export.Hidden.Assign(import.Hidden);
    local.Current.Date = Now().Date;
    local.Current.Timestamp = Now();
    local.MaxDate.Date = new DateTime(2099, 12, 31);
    export.CsePerson.Number = import.CsePerson.Number;
    export.CsePersonAccount.Type1 = import.CsePersonAccount.Type1;
    MoveCsePersonsWorkSet(import.CsePersonsWorkSet, export.CsePersonsWorkSet);
    MoveLegalAction(import.LegalAction, export.LegalAction);
    export.Obligation.Assign(import.Obligation);
    export.ObligationType.Assign(import.ObligationType);
    export.ScreenOwedAmounts.Assign(import.ScreenOwedAmounts);
    export.ShowActiveOnly.Flag = import.ShowActiveOnly.Flag;
    export.First.CollectionDt = import.First.CollectionDt;
    export.Last.CollectionDt = import.Last.CollectionDt;
    export.AccrualOrDue.Date = import.AccrualOrDue.Date;
    export.TotalAmountDue.TotalCurrency = import.TotalAmountDue.TotalCurrency;
    export.Standard.NextTransaction = import.Standard.NextTransaction;

    if (Equal(global.Command, "DISPLAY"))
    {
    }
    else
    {
      export.Ocp.Index = -1;

      for(import.Ocp.Index = 0; import.Ocp.Index < import.Ocp.Count; ++
        import.Ocp.Index)
      {
        ++export.Ocp.Index;
        export.Ocp.CheckSize();

        export.Ocp.Update.Sel.SelectChar = import.Ocp.Item.Sel.SelectChar;
        export.Ocp.Update.ObligCollProtectionHist.Assign(
          import.Ocp.Item.ObligCollProtectionHist);
        export.Ocp.Update.Prompt.Flag = import.Ocp.Item.Prompt.Flag;
      }
    }

    export.Standard.NextTransaction = import.Standard.NextTransaction;

    // if the next tran info is not equal to spaces, this implies the user 
    // requested a next tran action. now validate
    if (IsEmpty(import.Standard.NextTransaction))
    {
    }
    else
    {
      // <<< Set the NEXTTRAN info for the destination procedure .... RBM 03/17/
      // 97 >>>
      export.Hidden.CsePersonNumberObligor = export.CsePerson.Number;

      // *** Added next tran cse_person nunber. GSharp 12/15/98. ***
      export.Hidden.CsePersonNumber = export.CsePerson.Number;
      export.Hidden.ObligationId = export.Obligation.SystemGeneratedIdentifier;
      export.Hidden.LegalActionIdentifier = export.LegalAction.Identifier;
      UseScCabNextTranPut();

      return;
    }

    if (Equal(global.Command, "XXNEXTXX"))
    {
      // ****
      // this is where you set your export value to the export hidden next tran 
      // values if the user is comming into this procedure on a next tran
      // action.
      // ****
      UseScCabNextTranGet();
      export.CsePerson.Number = export.Hidden.CsePersonNumberObligor ?? Spaces
        (10);
      export.LegalAction.Identifier =
        export.Hidden.LegalActionIdentifier.GetValueOrDefault();

      // *** Added next tran legal_action standard_number. GSharp 12/15/98. ***
      export.LegalAction.StandardNumber =
        export.Hidden.StandardCrtOrdNumber ?? "";
      export.Obligation.SystemGeneratedIdentifier =
        export.Hidden.ObligationId.GetValueOrDefault();
      global.Command = "DISPLAY";
    }

    // to validate action level security
    if (Equal(global.Command, "ADD") || Equal(global.Command, "DEACT") || Equal
      (global.Command, "DISPLAY") || Equal(global.Command, "LIST"))
    {
      UseScCabTestSecurity();
    }

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    UseFnHardcodedDebtDistribution();

    switch(TrimEnd(global.Command))
    {
      case "PREV":
        ExitState = "ACO_NI0000_TOP_OF_LIST";

        break;
      case "NEXT":
        ExitState = "CO0000_SCROLLED_BEYOND_LAST_PG";

        break;
      case "RETURN":
        ExitState = "ACO_NE0000_RETURN";

        break;
      case "SIGNOFF":
        UseScCabSignoff();

        break;
      case "EXIT":
        ExitState = "ECO_LNK_RETURN_TO_MENU";

        break;
      case "LIST":
        for(export.Ocp.Index = 0; export.Ocp.Index < export.Ocp.Count; ++
          export.Ocp.Index)
        {
          if (!export.Ocp.CheckSize())
          {
            break;
          }

          switch(AsChar(export.Ocp.Item.Prompt.Flag))
          {
            case ' ':
              continue;
            case 'S':
              export.DlgflwRequired.CodeName = "PROTECTION LEVEL";
              ExitState = "ECO_LNK_TO_LIST_CODE_VALUE";

              goto Test2;
            default:
              ExitState = "ACO_NE0000_INVALID_PROMPT_VALUE";

              var field = GetField(export.Ocp.Item.Prompt, "flag");

              field.Error = true;

              goto Test2;
          }
        }

        export.Ocp.CheckIndex();
        ExitState = "ACO_NE0000_MUST_SELECT_4_PROMPT";

        break;
      case "RETCDVL":
        for(export.Ocp.Index = 0; export.Ocp.Index < export.Ocp.Count; ++
          export.Ocp.Index)
        {
          if (!export.Ocp.CheckSize())
          {
            break;
          }

          switch(AsChar(export.Ocp.Item.Prompt.Flag))
          {
            case ' ':
              continue;
            case 'S':
              export.Ocp.Update.Prompt.Flag = "";
              export.Ocp.Update.ObligCollProtectionHist.ProtectionLevel =
                import.DlgflwSelected.Cdvalue;

              var field = GetField(export.Ocp.Item.Sel, "selectChar");

              field.Protected = false;
              field.Focused = true;

              goto Test2;
            default:
              break;
          }
        }

        export.Ocp.CheckIndex();

        break;
      case "ADD":
        if (AsChar(export.Obligation.PrimarySecondaryCode) == 'S')
        {
          ExitState = "FN0000_COLL_PROT_NOT_VALID";

          break;
        }

        local.TotalRecsSelectedFProc.Count = 0;
        export.Ocp.Index = 0;

        for(var limit = export.Ocp.Count; export.Ocp.Index < limit; ++
          export.Ocp.Index)
        {
          if (!export.Ocp.CheckSize())
          {
            break;
          }

          switch(AsChar(export.Ocp.Item.Sel.SelectChar))
          {
            case ' ':
              continue;
            case 'S':
              ++local.TotalRecsSelectedFProc.Count;

              break;
            default:
              ExitState = "ACO_NE0000_INVALID_SELECT_CODE1";

              var field = GetField(export.Ocp.Item.Sel, "selectChar");

              field.Error = true;

              goto Test2;
          }

          if (Equal(export.First.CollectionDt, local.NullCollection.CollectionDt)
            || Equal
            (import.Last.CollectionDt, local.NullCollection.CollectionDt))
          {
            ExitState = "FN0000_COLL_DATES_MUST_BE_POP";

            var field1 = GetField(export.First, "collectionDt");

            field1.Error = true;

            var field2 = GetField(export.Last, "collectionDt");

            field2.Error = true;

            goto Test2;
          }

          if (Lt(local.NullObligCollProtectionHist.CreatedTmst,
            export.Ocp.Item.ObligCollProtectionHist.CreatedTmst))
          {
            ExitState = "FN0000_COLL_PROT_ALREADY_ADDED";

            goto Test2;
          }

          if (Equal(export.Ocp.Item.ObligCollProtectionHist.CvrdCollStrtDt, null))
            
          {
            var field =
              GetField(export.Ocp.Item.ObligCollProtectionHist, "cvrdCollStrtDt");
              

            field.Error = true;

            ExitState = "ACO_NE0000_REQUIRED_DATA_MISSING";

            goto Test2;
          }

          if (Lt(Now().Date,
            export.Ocp.Item.ObligCollProtectionHist.CvrdCollStrtDt))
          {
            var field =
              GetField(export.Ocp.Item.ObligCollProtectionHist, "cvrdCollStrtDt");
              

            field.Error = true;

            ExitState = "ACO_NE0000_DATE_CANNOT_BE_FUTURE";

            goto Test2;
          }

          if (Lt(export.Ocp.Item.ObligCollProtectionHist.CvrdCollEndDt,
            export.Ocp.Item.ObligCollProtectionHist.CvrdCollStrtDt) && !
            Equal(export.Ocp.Item.ObligCollProtectionHist.CvrdCollEndDt, null))
          {
            var field =
              GetField(export.Ocp.Item.ObligCollProtectionHist, "cvrdCollStrtDt");
              

            field.Error = true;

            ExitState = "ACO_NE0000_END_LESS_THAN_START";

            goto Test2;
          }

          if (Lt(export.Ocp.Item.ObligCollProtectionHist.CvrdCollStrtDt,
            export.First.CollectionDt) || Lt
            (export.Last.CollectionDt,
            export.Ocp.Item.ObligCollProtectionHist.CvrdCollStrtDt))
          {
            var field =
              GetField(export.Ocp.Item.ObligCollProtectionHist, "cvrdCollStrtDt");
              

            field.Error = true;

            ExitState = "FN0000_COLL_PROT_OVERLAP";

            goto Test2;
          }

          if (Equal(export.Ocp.Item.ObligCollProtectionHist.CvrdCollEndDt, null))
            
          {
            var field =
              GetField(export.Ocp.Item.ObligCollProtectionHist, "cvrdCollEndDt");
              

            field.Error = true;

            ExitState = "ACO_NE0000_REQUIRED_DATA_MISSING";

            goto Test2;
          }

          if (Lt(Now().Date,
            export.Ocp.Item.ObligCollProtectionHist.CvrdCollEndDt))
          {
            var field =
              GetField(export.Ocp.Item.ObligCollProtectionHist, "cvrdCollEndDt");
              

            field.Error = true;

            ExitState = "ACO_NE0000_DATE_CANNOT_BE_FUTURE";

            goto Test2;
          }

          if (Lt(export.Ocp.Item.ObligCollProtectionHist.CvrdCollEndDt,
            export.First.CollectionDt) && !
            Equal(export.Ocp.Item.ObligCollProtectionHist.CvrdCollStrtDt, null) ||
            Lt
            (export.Last.CollectionDt,
            export.Ocp.Item.ObligCollProtectionHist.CvrdCollEndDt))
          {
            var field =
              GetField(export.Ocp.Item.ObligCollProtectionHist, "cvrdCollEndDt");
              

            field.Error = true;

            ExitState = "FN0000_COLL_PROT_OVERLAP";

            goto Test2;
          }

          if (IsEmpty(export.Ocp.Item.ObligCollProtectionHist.ReasonText))
          {
            var field =
              GetField(export.Ocp.Item.ObligCollProtectionHist, "reasonText");

            field.Error = true;

            ExitState = "FN0000_REASON_TEXT_REQUIRED";

            goto Test2;
          }

          if (!IsEmpty(export.Ocp.Item.ObligCollProtectionHist.ProtectionLevel))
          {
            local.Code.CodeName = "PROTECTION LEVEL";
            local.CodeValue.Cdvalue =
              export.Ocp.Item.ObligCollProtectionHist.ProtectionLevel;
            UseCabValidateCodeValue();
          }

          if (AsChar(local.ValidateReturnCd.Flag) == 'N')
          {
            var field =
              GetField(export.Ocp.Item.ObligCollProtectionHist,
              "protectionLevel");

            field.Error = true;

            ExitState = "ACO_NE0000_INVALID_CODE";

            goto Test2;
          }

          UseFnCreateObligCollProtection1();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            break;
          }

          // **********************************************************************
          // If the obligation is "Joint and Several", Find the other obligation
          // and create the collection protection for it as well.  It is
          // important that both obligations stay in sync. If the obligation is
          // "S"econdary or "P"rimary, find the concurrent ob and create the
          // time frame for only the primary obligation. Update the secondary
          // collections to "P"rotected.
          // **********************************************************************
          if (AsChar(export.Obligation.PrimarySecondaryCode) == 'J' || AsChar
            (export.Obligation.PrimarySecondaryCode) == 'S' || AsChar
            (export.Obligation.PrimarySecondaryCode) == 'P')
          {
            UseFnFindConcurrentObligation();

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              break;
            }

            if (AsChar(local.FoundConcurrentObl.Flag) == 'Y')
            {
              UseFnCreateObligCollProtection2();
            }
            else
            {
              ExitState = "FN0000_CONCURRENT_OBLIGATN_NF_RB";
            }
          }

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            // **********************************************************************
            // Let the user know that there was a problem with the "Concurrent" 
            // Obligation
            // **********************************************************************
            if (IsExitState("MED_URA_AMT_NEG_TFRAM_NOT_ADDED"))
            {
              ExitState = "CONCURENT_MED_URA_NEG_TFRAM_NOT";
            }
            else if (IsExitState("URA_AMT_NEG_TFRAM_NOT_ADDED"))
            {
              ExitState = "CONCURRENT_URA_AMT_NEG_TFRAM_NOT";
            }
            else if (IsExitState("FN0000_DATE_OVERLAP_RB"))
            {
              ExitState = "CONCURENT_DATE_OVERLAP_TFRAM_NOT";
            }
            else if (IsExitState("FN0000_APPLIED_TO_COLL_NF_RB"))
            {
              ExitState = "FN0000_CONCUR_APPLIED_TO_COLL_RB";
            }
            else
            {
            }

            break;
          }
          else
          {
            // **********************************************************************
            // Create Hist / Alert
            // **********************************************************************
            local.AlrtHistCollProtAction.Text1 = "A";
            UseFnPrcCollProtHistAndAlrts();
          }
        }

        export.Ocp.CheckIndex();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          UseEabRollbackCics();

          break;
        }

        if (local.TotalRecsSelectedFProc.Count == 0)
        {
          ExitState = "FN0000_MUST_SEL_REC_FOR_PROC";

          export.Ocp.Index = 0;
          export.Ocp.CheckSize();

          var field = GetField(export.Ocp.Item.Sel, "selectChar");

          field.Protected = false;
          field.Focused = true;

          break;
        }

        export.Ocp.Update.ObligCollProtectionHist.CreatedTmst = Now();

        // --- If the processing is successful, clear the select chars.
        for(export.Ocp.Index = 0; export.Ocp.Index < export.Ocp.Count; ++
          export.Ocp.Index)
        {
          if (!export.Ocp.CheckSize())
          {
            break;
          }

          if (!IsEmpty(export.Ocp.Item.Sel.SelectChar))
          {
            export.Ocp.Update.Sel.SelectChar = "";

            // **********************************************************************
            // Set the export view so it will give the user and indication it is
            // successfull. Only the date is displayed so the exact TS is not
            // important.
            // **********************************************************************
            export.Ocp.Update.ObligCollProtectionHist.CreatedTmst = Now();
          }
        }

        export.Ocp.CheckIndex();

        // **********************************************************************
        // Re-sort the newly added protection timeframe.
        // **********************************************************************
        UseFnReadCollectionProtection();

        // **********************************************************************
        // Re-Display Collection Dates
        // **********************************************************************
        if (ReadObligation())
        {
          if (IsEmpty(export.Obligation.OrderTypeCode))
          {
            export.Obligation.OrderTypeCode = entities.Obligation.OrderTypeCode;
          }

          export.Obligation.PrimarySecondaryCode =
            entities.Obligation.PrimarySecondaryCode;

          // =================================================
          // Get the First (MIN) collection date
          // =================================================
          ReadCollection2();

          // =================================================
          // Get the Last (MAX) collection date
          // =================================================
          ReadCollection1();
        }
        else
        {
          ExitState = "FN0000_OBLIGATION_NF";

          break;
        }

        if (AsChar(local.FoundConcurrentObl.Flag) == 'Y')
        {
          ExitState = "FN0000_CONCURRENT_COL_PROT_SUCCS";
        }
        else
        {
          ExitState = "ACO_NI0000_SUCCESSFUL_ADD";
        }

        break;
      case "DEACT":
        if (AsChar(export.Obligation.PrimarySecondaryCode) == 'S')
        {
          ExitState = "FN0000_COLL_PROT_NOT_VALID";

          break;
        }

        local.TotalRecsSelectedFProc.Count = 0;
        export.Ocp.Index = 0;

        for(var limit = export.Ocp.Count; export.Ocp.Index < limit; ++
          export.Ocp.Index)
        {
          if (!export.Ocp.CheckSize())
          {
            break;
          }

          switch(AsChar(export.Ocp.Item.Sel.SelectChar))
          {
            case ' ':
              continue;
            case 'S':
              ++local.TotalRecsSelectedFProc.Count;

              break;
            default:
              ExitState = "ACO_NE0000_INVALID_SELECT_CODE1";

              var field = GetField(export.Ocp.Item.Sel, "selectChar");

              field.Error = true;

              goto AfterCycle;
          }

          if (Lt(local.NullCollection.CollectionDt,
            export.Ocp.Item.ObligCollProtectionHist.DeactivationDate) && !
            Equal(export.Ocp.Item.ObligCollProtectionHist.DeactivationDate,
            local.NullObligCollProtectionHist.DeactivationDate))
          {
            ExitState = "FN0000_COLL_PROT_ALREADY_DEACT";

            break;
          }

          if (Equal(export.Ocp.Item.ObligCollProtectionHist.CreatedTmst,
            local.NullObligCollProtectionHist.CreatedTmst))
          {
            var field = GetField(export.Ocp.Item.Sel, "selectChar");

            field.Error = true;

            ExitState = "FN0000_ADD_COLL_PROT_B4_DEACT";

            break;
          }

          UseFnDeactivateCollProtection2();

          // **********************************************************************
          // If the obligation is "Joint and Several", Find the other obligation
          // and Deativate the collection protection for it as well.  It is
          // important that both obligations stay in sync.
          // **********************************************************************
          if (AsChar(export.Obligation.PrimarySecondaryCode) == 'J' || AsChar
            (export.Obligation.PrimarySecondaryCode) == 'S' || AsChar
            (export.Obligation.PrimarySecondaryCode) == 'P')
          {
            UseFnFindConcurrentObligation();

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              goto Test1;
            }

            if (AsChar(local.FoundConcurrentObl.Flag) == 'Y')
            {
              UseFnDeactivateCollProtection1();
            }
            else
            {
              ExitState = "FN0000_CONCURRENT_OBLIGATN_NF_RB";
            }
          }

Test1:

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            break;
          }

          export.Ocp.Update.ObligCollProtectionHist.DeactivationDate =
            Now().Date;

          // **********************************************************************
          // Create Hist / Alert
          // **********************************************************************
          local.AlrtHistCollProtAction.Text1 = "D";
          UseFnPrcCollProtHistAndAlrts();
        }

AfterCycle:

        export.Ocp.CheckIndex();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          UseEabRollbackCics();

          break;
        }

        if (local.TotalRecsSelectedFProc.Count == 0)
        {
          ExitState = "FN0000_MUST_SEL_REC_FOR_PROC";

          export.Ocp.Index = 0;
          export.Ocp.CheckSize();

          var field = GetField(export.Ocp.Item.Sel, "selectChar");

          field.Protected = false;
          field.Focused = true;

          break;
        }

        // --- If the processing is successful, clear the select chars.
        for(export.Ocp.Index = 0; export.Ocp.Index < export.Ocp.Count; ++
          export.Ocp.Index)
        {
          if (!export.Ocp.CheckSize())
          {
            break;
          }

          if (!IsEmpty(export.Ocp.Item.Sel.SelectChar))
          {
            export.Ocp.Update.Sel.SelectChar = "";
          }
        }

        export.Ocp.CheckIndex();

        // **********************************************************************
        // Re-sort the De-activated protection timeframe.
        // **********************************************************************
        UseFnReadCollectionProtection();

        // **********************************************************************
        // Re-Display Collection Dates
        // **********************************************************************
        if (ReadObligation())
        {
          if (IsEmpty(export.Obligation.OrderTypeCode))
          {
            export.Obligation.OrderTypeCode = entities.Obligation.OrderTypeCode;
          }

          export.Obligation.PrimarySecondaryCode =
            entities.Obligation.PrimarySecondaryCode;

          // =================================================
          // Get the First (MIN) collection date
          // =================================================
          ReadCollection2();

          // =================================================
          // Get the Last (MAX) collection date
          // =================================================
          ReadCollection1();
        }
        else
        {
          ExitState = "FN0000_OBLIGATION_NF";

          break;
        }

        if (AsChar(local.FoundConcurrentObl.Flag) == 'Y')
        {
          ExitState = "FN0000_CONCURENT_COL_PROT_DEACT";
        }
        else
        {
          ExitState = "FN0000_COLL_PROT_DEACT_SUCCESS";
        }

        break;
      case "DISPLAY":
        if (IsEmpty(export.ShowActiveOnly.Flag))
        {
          export.ShowActiveOnly.Flag = "Y";
        }

        export.CsePersonsWorkSet.Number = export.CsePerson.Number;
        UseSiReadCsePerson();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          break;
        }

        if (export.Obligation.SystemGeneratedIdentifier == 0 || export
          .ObligationType.SystemGeneratedIdentifier == 0)
        {
          ExitState = "FN0000_MUST_SEL_OBLG";

          break;
        }

        local.OmitCrdInd.Flag = "N";
        UseFnComputeSummaryTotals();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          break;
        }

        UseFnCabSetAccrualOrDueAmount();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          break;
        }

        if (ReadObligation())
        {
          if (IsEmpty(export.Obligation.OrderTypeCode))
          {
            export.Obligation.OrderTypeCode = entities.Obligation.OrderTypeCode;
          }

          export.Obligation.PrimarySecondaryCode =
            entities.Obligation.PrimarySecondaryCode;

          // =================================================
          // Get the First (MIN) collection date
          // =================================================
          ReadCollection2();

          // =================================================
          // Get the Last (MAX) collection date
          // =================================================
          ReadCollection1();
        }
        else
        {
          ExitState = "FN0000_OBLIGATION_NF";

          break;
        }

        UseFnReadCollectionProtection();

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          // --- Reset the max dates back to zeros
          if (export.Ocp.IsFull)
          {
            ExitState = "ACO_NI0000_LIST_IS_FULL";
          }
          else if (export.Ocp.IsEmpty)
          {
            ExitState = "FN0000_OB_COLL_PROT_HIST_NF";
          }
          else
          {
            ExitState = "ACO_NI0000_DISPLAY_SUCCESSFUL";
          }
        }

        if (AsChar(export.Obligation.PrimarySecondaryCode) == 'S')
        {
          ExitState = "FN0000_COLL_PROT_NOT_VALID";
        }
        else
        {
          export.Ocp.Index = 0;
          export.Ocp.CheckSize();

          var field = GetField(export.Ocp.Item.Sel, "selectChar");

          field.Protected = false;
          field.Focused = true;
        }

        break;
      default:
        ExitState = "ACO_NE0000_INVALID_COMMAND";

        break;
    }

Test2:

    if (AsChar(export.Obligation.PrimarySecondaryCode) == 'S')
    {
      for(export.Ocp.Index = 0; export.Ocp.Index < Export.OcpGroup.Capacity; ++
        export.Ocp.Index)
      {
        if (!export.Ocp.CheckSize())
        {
          break;
        }

        var field1 = GetField(export.Ocp.Item.Sel, "selectChar");

        field1.Color = "cyan";
        field1.Protected = true;

        var field2 =
          GetField(export.Ocp.Item.ObligCollProtectionHist, "cvrdCollStrtDt");

        field2.Color = "cyan";
        field2.Protected = true;

        var field3 =
          GetField(export.Ocp.Item.ObligCollProtectionHist, "cvrdCollEndDt");

        field3.Color = "cyan";
        field3.Protected = true;

        var field4 =
          GetField(export.Ocp.Item.ObligCollProtectionHist, "protectionLevel");

        field4.Color = "cyan";
        field4.Protected = true;

        var field5 = GetField(export.Ocp.Item.Prompt, "flag");

        field5.Color = "cyan";
        field5.Protected = true;

        var field6 =
          GetField(export.Ocp.Item.ObligCollProtectionHist, "createdTmst");

        field6.Color = "cyan";
        field6.Protected = true;

        var field7 =
          GetField(export.Ocp.Item.ObligCollProtectionHist, "deactivationDate");
          

        field7.Color = "cyan";
        field7.Protected = true;

        var field8 =
          GetField(export.Ocp.Item.ObligCollProtectionHist, "createdBy");

        field8.Color = "cyan";
        field8.Protected = true;

        var field9 =
          GetField(export.Ocp.Item.ObligCollProtectionHist, "reasonText");

        field9.Color = "cyan";
        field9.Protected = true;
      }

      export.Ocp.CheckIndex();
    }
    else
    {
      for(export.Ocp.Index = 0; export.Ocp.Index < export.Ocp.Count; ++
        export.Ocp.Index)
      {
        if (!export.Ocp.CheckSize())
        {
          break;
        }

        if (Lt(local.NullObligCollProtectionHist.CreatedTmst,
          export.Ocp.Item.ObligCollProtectionHist.CreatedTmst))
        {
          var field1 =
            GetField(export.Ocp.Item.ObligCollProtectionHist, "cvrdCollStrtDt");
            

          field1.Color = "cyan";
          field1.Protected = true;

          var field2 =
            GetField(export.Ocp.Item.ObligCollProtectionHist, "cvrdCollEndDt");

          field2.Color = "cyan";
          field2.Protected = true;

          var field3 =
            GetField(export.Ocp.Item.ObligCollProtectionHist, "protectionLevel");
            

          field3.Color = "cyan";
          field3.Protected = true;

          var field4 = GetField(export.Ocp.Item.Prompt, "flag");

          field4.Color = "cyan";
          field4.Protected = true;

          var field5 =
            GetField(export.Ocp.Item.ObligCollProtectionHist, "createdTmst");

          field5.Color = "cyan";
          field5.Protected = true;

          var field6 =
            GetField(export.Ocp.Item.ObligCollProtectionHist, "deactivationDate");
            

          field6.Color = "cyan";
          field6.Protected = true;

          var field7 =
            GetField(export.Ocp.Item.ObligCollProtectionHist, "createdBy");

          field7.Color = "cyan";
          field7.Protected = true;

          var field8 =
            GetField(export.Ocp.Item.ObligCollProtectionHist, "reasonText");

          field8.Color = "cyan";
          field8.Protected = true;
        }
      }

      export.Ocp.CheckIndex();
    }

    if (!IsEmpty(export.ScreenOwedAmounts.ErrorInformationLine))
    {
      var field = GetField(export.ScreenOwedAmounts, "errorInformationLine");

      field.Color = "red";
      field.Highlighting = Highlighting.ReverseVideo;
      field.Protected = true;
    }
  }

  private static void MoveCsePerson(CsePerson source, CsePerson target)
  {
    target.Type1 = source.Type1;
    target.Number = source.Number;
    target.OrganizationName = source.OrganizationName;
  }

  private static void MoveCsePersonsWorkSet(CsePersonsWorkSet source,
    CsePersonsWorkSet target)
  {
    target.Number = source.Number;
    target.FormattedName = source.FormattedName;
  }

  private static void MoveLegalAction(LegalAction source, LegalAction target)
  {
    target.Identifier = source.Identifier;
    target.StandardNumber = source.StandardNumber;
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

  private static void MoveObligCollProtectionHist1(
    ObligCollProtectionHist source, ObligCollProtectionHist target)
  {
    target.CvrdCollStrtDt = source.CvrdCollStrtDt;
    target.CvrdCollEndDt = source.CvrdCollEndDt;
    target.DeactivationDate = source.DeactivationDate;
    target.CreatedBy = source.CreatedBy;
    target.CreatedTmst = source.CreatedTmst;
    target.ProtectionLevel = source.ProtectionLevel;
    target.ReasonText = source.ReasonText;
  }

  private static void MoveObligCollProtectionHist2(
    ObligCollProtectionHist source, ObligCollProtectionHist target)
  {
    target.CvrdCollStrtDt = source.CvrdCollStrtDt;
    target.CvrdCollEndDt = source.CvrdCollEndDt;
    target.DeactivationDate = source.DeactivationDate;
    target.CreatedBy = source.CreatedBy;
    target.CreatedTmst = source.CreatedTmst;
    target.ReasonText = source.ReasonText;
  }

  private static void MoveObligationType(ObligationType source,
    ObligationType target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Code = source.Code;
  }

  private static void MoveOcp(FnReadCollectionProtection.Export.OcpGroup source,
    Export.OcpGroup target)
  {
    target.Sel.SelectChar = source.Sel.SelectChar;
    target.Prompt.Flag = source.Prompt.Flag;
    target.ObligCollProtectionHist.Assign(source.ObligCollProtectionHist);
  }

  private void UseCabValidateCodeValue()
  {
    var useImport = new CabValidateCodeValue.Import();
    var useExport = new CabValidateCodeValue.Export();

    useImport.CodeValue.Cdvalue = local.CodeValue.Cdvalue;
    useImport.Code.CodeName = local.Code.CodeName;

    Call(CabValidateCodeValue.Execute, useImport, useExport);

    local.ValidateReturnCd.Flag = useExport.ValidCode.Flag;
  }

  private void UseEabRollbackCics()
  {
    var useImport = new EabRollbackCics.Import();
    var useExport = new EabRollbackCics.Export();

    Call(EabRollbackCics.Execute, useImport, useExport);
  }

  private void UseFnCabSetAccrualOrDueAmount()
  {
    var useImport = new FnCabSetAccrualOrDueAmount.Import();
    var useExport = new FnCabSetAccrualOrDueAmount.Export();

    useImport.CsePerson.Number = export.CsePerson.Number;
    useImport.Obligation.SystemGeneratedIdentifier =
      export.Obligation.SystemGeneratedIdentifier;
    MoveObligationType(export.ObligationType, useImport.ObligationType);

    Call(FnCabSetAccrualOrDueAmount.Execute, useImport, useExport);

    export.AccrualOrDue.Date = useExport.StartDte.Date;
    export.TotalAmountDue.TotalCurrency = useExport.Common.TotalCurrency;
  }

  private void UseFnComputeSummaryTotals()
  {
    var useImport = new FnComputeSummaryTotals.Import();
    var useExport = new FnComputeSummaryTotals.Export();

    useImport.OmitCrdInd.Flag = local.OmitCrdInd.Flag;
    useImport.Obligor.Number = export.CsePerson.Number;
    useImport.Filter.SystemGeneratedIdentifier =
      export.Obligation.SystemGeneratedIdentifier;
    useImport.FilterByIdObligationType.SystemGeneratedIdentifier =
      export.ObligationType.SystemGeneratedIdentifier;

    Call(FnComputeSummaryTotals.Execute, useImport, useExport);

    export.ScreenOwedAmounts.Assign(useExport.ScreenOwedAmounts);
  }

  private void UseFnCreateObligCollProtection1()
  {
    var useImport = new FnCreateObligCollProtection.Import();
    var useExport = new FnCreateObligCollProtection.Export();

    useImport.HardcodeMedForCash.SystemGeneratedIdentifier =
      local.HardcodeMedSuppForCash.SystemGeneratedIdentifier;
    useImport.HardcodeMedJudgement.SystemGeneratedIdentifier =
      local.HardcodeMedicalJudgement.SystemGeneratedIdentifier;
    useImport.HardcodeMedicalSupport.SystemGeneratedIdentifier =
      local.HardcodeMedicalSupport.SystemGeneratedIdentifier;
    useImport.CsePersonAccount.Type1 = local.HardcodeObligor.Type1;
    useImport.CsePerson.Number = export.CsePerson.Number;
    useImport.Obligation.SystemGeneratedIdentifier =
      export.Obligation.SystemGeneratedIdentifier;
    useImport.ObligationType.SystemGeneratedIdentifier =
      export.ObligationType.SystemGeneratedIdentifier;
    MoveObligCollProtectionHist1(export.Ocp.Item.ObligCollProtectionHist,
      useImport.ObligCollProtectionHist);

    Call(FnCreateObligCollProtection.Execute, useImport, useExport);
  }

  private void UseFnCreateObligCollProtection2()
  {
    var useImport = new FnCreateObligCollProtection.Import();
    var useExport = new FnCreateObligCollProtection.Export();

    useImport.ObligationType.SystemGeneratedIdentifier =
      local.ConcurrentObligationType.SystemGeneratedIdentifier;
    useImport.CsePerson.Number = local.ConcurrentCsePerson.Number;
    useImport.CsePersonAccount.Type1 = local.ConcurrentCsePersonAccount.Type1;
    useImport.Obligation.SystemGeneratedIdentifier =
      local.ConcurrentObligation.SystemGeneratedIdentifier;
    MoveObligCollProtectionHist1(export.Ocp.Item.ObligCollProtectionHist,
      useImport.ObligCollProtectionHist);

    Call(FnCreateObligCollProtection.Execute, useImport, useExport);
  }

  private void UseFnDeactivateCollProtection1()
  {
    var useImport = new FnDeactivateCollProtection.Import();
    var useExport = new FnDeactivateCollProtection.Export();

    useImport.ObligationType.SystemGeneratedIdentifier =
      local.ConcurrentObligationType.SystemGeneratedIdentifier;
    useImport.CsePerson.Number = local.ConcurrentCsePerson.Number;
    useImport.CsePersonAccount.Type1 = local.ConcurrentCsePersonAccount.Type1;
    useImport.Obligation.SystemGeneratedIdentifier =
      local.ConcurrentObligation.SystemGeneratedIdentifier;
    MoveObligCollProtectionHist2(export.Ocp.Item.ObligCollProtectionHist,
      useImport.ObligCollProtectionHist);

    Call(FnDeactivateCollProtection.Execute, useImport, useExport);
  }

  private void UseFnDeactivateCollProtection2()
  {
    var useImport = new FnDeactivateCollProtection.Import();
    var useExport = new FnDeactivateCollProtection.Export();

    useImport.CsePersonAccount.Type1 = local.HardcodeObligor.Type1;
    useImport.CsePerson.Number = export.CsePerson.Number;
    useImport.Obligation.SystemGeneratedIdentifier =
      export.Obligation.SystemGeneratedIdentifier;
    useImport.ObligationType.SystemGeneratedIdentifier =
      export.ObligationType.SystemGeneratedIdentifier;
    MoveObligCollProtectionHist2(export.Ocp.Item.ObligCollProtectionHist,
      useImport.ObligCollProtectionHist);

    Call(FnDeactivateCollProtection.Execute, useImport, useExport);
  }

  private void UseFnFindConcurrentObligation()
  {
    var useImport = new FnFindConcurrentObligation.Import();
    var useExport = new FnFindConcurrentObligation.Export();

    useImport.CsePersonAccount.Type1 = local.HardcodeObligor.Type1;
    useImport.CsePerson.Number = export.CsePerson.Number;
    useImport.Obligation.SystemGeneratedIdentifier =
      export.Obligation.SystemGeneratedIdentifier;
    useImport.ObligationType.SystemGeneratedIdentifier =
      export.ObligationType.SystemGeneratedIdentifier;

    Call(FnFindConcurrentObligation.Execute, useImport, useExport);

    local.FoundConcurrentObl.Flag = useExport.FoundConcurrentObl.Flag;
    local.ConcurrentObligationType.SystemGeneratedIdentifier =
      useExport.ConcurrentObligationType.SystemGeneratedIdentifier;
    local.ConcurrentCsePerson.Number = useExport.ConcurrentCsePerson.Number;
    local.ConcurrentCsePersonAccount.Type1 =
      useExport.ConcurrentCsePersonAccount.Type1;
    local.ConcurrentObligation.SystemGeneratedIdentifier =
      useExport.ConcurrentObligation.SystemGeneratedIdentifier;
  }

  private void UseFnHardcodedDebtDistribution()
  {
    var useImport = new FnHardcodedDebtDistribution.Import();
    var useExport = new FnHardcodedDebtDistribution.Export();

    Call(FnHardcodedDebtDistribution.Execute, useImport, useExport);

    local.HardcodeMedSuppForCash.SystemGeneratedIdentifier =
      useExport.OtMedicalSupportForCash.SystemGeneratedIdentifier;
    local.HardcodeMedicalJudgement.SystemGeneratedIdentifier =
      useExport.OtMedicalJudgement.SystemGeneratedIdentifier;
    local.HardcodeMedicalSupport.SystemGeneratedIdentifier =
      useExport.OtMedicalSupport.SystemGeneratedIdentifier;
    local.HardcodeObligor.Type1 = useExport.CpaObligor.Type1;
  }

  private void UseFnPrcCollProtHistAndAlrts()
  {
    var useImport = new FnPrcCollProtHistAndAlrts.Import();
    var useExport = new FnPrcCollProtHistAndAlrts.Export();

    useImport.CollProtAction.Text1 = local.AlrtHistCollProtAction.Text1;
    useImport.Obligor.Number = export.CsePerson.Number;
    useImport.Obligation.SystemGeneratedIdentifier =
      export.Obligation.SystemGeneratedIdentifier;
    useImport.LegalAction.StandardNumber = export.LegalAction.StandardNumber;
    useImport.ObligCollProtectionHist.Assign(
      export.Ocp.Item.ObligCollProtectionHist);

    Call(FnPrcCollProtHistAndAlrts.Execute, useImport, useExport);
  }

  private void UseFnReadCollectionProtection()
  {
    var useImport = new FnReadCollectionProtection.Import();
    var useExport = new FnReadCollectionProtection.Export();

    useImport.ShowActiveOnly.Flag = export.ShowActiveOnly.Flag;
    useImport.CsePerson.Number = export.CsePerson.Number;
    useImport.Obligation.SystemGeneratedIdentifier =
      export.Obligation.SystemGeneratedIdentifier;
    useImport.ObligationType.SystemGeneratedIdentifier =
      export.ObligationType.SystemGeneratedIdentifier;

    Call(FnReadCollectionProtection.Execute, useImport, useExport);

    useExport.Ocp.CopyTo(export.Ocp, MoveOcp);
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

    useImport.CsePersonsWorkSet.Number = import.CsePersonsWorkSet.Number;
    MoveLegalAction(import.LegalAction, useImport.LegalAction);
    useImport.CsePerson.Number = import.CsePerson.Number;

    Call(ScCabTestSecurity.Execute, useImport, useExport);
  }

  private void UseSiReadCsePerson()
  {
    var useImport = new SiReadCsePerson.Import();
    var useExport = new SiReadCsePerson.Export();

    useImport.CsePersonsWorkSet.Number = export.CsePersonsWorkSet.Number;

    Call(SiReadCsePerson.Execute, useImport, useExport);

    MoveCsePerson(useExport.CsePerson, export.CsePerson);
    MoveCsePersonsWorkSet(useExport.CsePersonsWorkSet, export.CsePersonsWorkSet);
      
  }

  private bool ReadCollection1()
  {
    System.Diagnostics.Debug.Assert(entities.Obligation.Populated);
    export.Last.Populated = false;

    return Read("ReadCollection1",
      (db, command) =>
      {
        db.SetInt32(command, "otyId", entities.Obligation.DtyGeneratedId);
        db.SetInt32(
          command, "obgId", entities.Obligation.SystemGeneratedIdentifier);
        db.SetString(command, "cspNumber", entities.Obligation.CspNumber);
        db.SetString(command, "cpaType", entities.Obligation.CpaType);
      },
      (db, reader) =>
      {
        export.Last.CollectionDt = db.GetDate(reader, 0);
        export.Last.Populated = true;
      });
  }

  private bool ReadCollection2()
  {
    System.Diagnostics.Debug.Assert(entities.Obligation.Populated);
    export.First.Populated = false;

    return Read("ReadCollection2",
      (db, command) =>
      {
        db.SetInt32(command, "otyId", entities.Obligation.DtyGeneratedId);
        db.SetInt32(
          command, "obgId", entities.Obligation.SystemGeneratedIdentifier);
        db.SetString(command, "cspNumber", entities.Obligation.CspNumber);
        db.SetString(command, "cpaType", entities.Obligation.CpaType);
      },
      (db, reader) =>
      {
        export.First.CollectionDt = db.GetDate(reader, 0);
        export.First.Populated = true;
      });
  }

  private bool ReadObligation()
  {
    entities.Obligation.Populated = false;

    return Read("ReadObligation",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", export.CsePerson.Number);
        db.
          SetInt32(command, "obId", export.Obligation.SystemGeneratedIdentifier);
          
        db.SetInt32(
          command, "dtyGeneratedId",
          export.ObligationType.SystemGeneratedIdentifier);
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
    /// <summary>A OcpGroup group.</summary>
    [Serializable]
    public class OcpGroup
    {
      /// <summary>
      /// A value of Sel.
      /// </summary>
      [JsonPropertyName("sel")]
      public Common Sel
      {
        get => sel ??= new();
        set => sel = value;
      }

      /// <summary>
      /// A value of Prompt.
      /// </summary>
      [JsonPropertyName("prompt")]
      public Common Prompt
      {
        get => prompt ??= new();
        set => prompt = value;
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

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 75;

      private Common sel;
      private Common prompt;
      private ObligCollProtectionHist obligCollProtectionHist;
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
    /// Gets a value of Ocp.
    /// </summary>
    [JsonIgnore]
    public Array<OcpGroup> Ocp => ocp ??= new(OcpGroup.Capacity);

    /// <summary>
    /// Gets a value of Ocp for json serialization.
    /// </summary>
    [JsonPropertyName("ocp")]
    [Computed]
    public IList<OcpGroup> Ocp_Json
    {
      get => ocp;
      set => Ocp.Assign(value);
    }

    /// <summary>
    /// A value of Last.
    /// </summary>
    [JsonPropertyName("last")]
    public Collection Last
    {
      get => last ??= new();
      set => last = value;
    }

    /// <summary>
    /// A value of First.
    /// </summary>
    [JsonPropertyName("first")]
    public Collection First
    {
      get => first ??= new();
      set => first = value;
    }

    /// <summary>
    /// A value of ShowActiveOnly.
    /// </summary>
    [JsonPropertyName("showActiveOnly")]
    public Common ShowActiveOnly
    {
      get => showActiveOnly ??= new();
      set => showActiveOnly = value;
    }

    /// <summary>
    /// A value of LegalActionDetail.
    /// </summary>
    [JsonPropertyName("legalActionDetail")]
    public LegalActionDetail LegalActionDetail
    {
      get => legalActionDetail ??= new();
      set => legalActionDetail = value;
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
    /// A value of LegalAction.
    /// </summary>
    [JsonPropertyName("legalAction")]
    public LegalAction LegalAction
    {
      get => legalAction ??= new();
      set => legalAction = value;
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
    /// A value of Hidden.
    /// </summary>
    [JsonPropertyName("hidden")]
    public NextTranInfo Hidden
    {
      get => hidden ??= new();
      set => hidden = value;
    }

    /// <summary>
    /// A value of AccrualOrDue.
    /// </summary>
    [JsonPropertyName("accrualOrDue")]
    public DateWorkArea AccrualOrDue
    {
      get => accrualOrDue ??= new();
      set => accrualOrDue = value;
    }

    /// <summary>
    /// A value of TotalAmountDue.
    /// </summary>
    [JsonPropertyName("totalAmountDue")]
    public Common TotalAmountDue
    {
      get => totalAmountDue ??= new();
      set => totalAmountDue = value;
    }

    private CodeValue dlgflwSelected;
    private Array<OcpGroup> ocp;
    private Collection last;
    private Collection first;
    private Common showActiveOnly;
    private LegalActionDetail legalActionDetail;
    private Standard standard;
    private CsePersonsWorkSet csePersonsWorkSet;
    private ObligationType obligationType;
    private LegalAction legalAction;
    private ScreenOwedAmounts screenOwedAmounts;
    private CsePersonAccount csePersonAccount;
    private CsePerson csePerson;
    private Obligation obligation;
    private NextTranInfo hidden;
    private DateWorkArea accrualOrDue;
    private Common totalAmountDue;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>A OcpGroup group.</summary>
    [Serializable]
    public class OcpGroup
    {
      /// <summary>
      /// A value of Sel.
      /// </summary>
      [JsonPropertyName("sel")]
      public Common Sel
      {
        get => sel ??= new();
        set => sel = value;
      }

      /// <summary>
      /// A value of Prompt.
      /// </summary>
      [JsonPropertyName("prompt")]
      public Common Prompt
      {
        get => prompt ??= new();
        set => prompt = value;
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

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 75;

      private Common sel;
      private Common prompt;
      private ObligCollProtectionHist obligCollProtectionHist;
    }

    /// <summary>
    /// Gets a value of Ocp.
    /// </summary>
    [JsonIgnore]
    public Array<OcpGroup> Ocp => ocp ??= new(OcpGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Ocp for json serialization.
    /// </summary>
    [JsonPropertyName("ocp")]
    [Computed]
    public IList<OcpGroup> Ocp_Json
    {
      get => ocp;
      set => Ocp.Assign(value);
    }

    /// <summary>
    /// A value of Last.
    /// </summary>
    [JsonPropertyName("last")]
    public Collection Last
    {
      get => last ??= new();
      set => last = value;
    }

    /// <summary>
    /// A value of First.
    /// </summary>
    [JsonPropertyName("first")]
    public Collection First
    {
      get => first ??= new();
      set => first = value;
    }

    /// <summary>
    /// A value of ShowActiveOnly.
    /// </summary>
    [JsonPropertyName("showActiveOnly")]
    public Common ShowActiveOnly
    {
      get => showActiveOnly ??= new();
      set => showActiveOnly = value;
    }

    /// <summary>
    /// A value of LegalActionDetail.
    /// </summary>
    [JsonPropertyName("legalActionDetail")]
    public LegalActionDetail LegalActionDetail
    {
      get => legalActionDetail ??= new();
      set => legalActionDetail = value;
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
    /// A value of Obligation.
    /// </summary>
    [JsonPropertyName("obligation")]
    public Obligation Obligation
    {
      get => obligation ??= new();
      set => obligation = value;
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
    /// A value of LegalAction.
    /// </summary>
    [JsonPropertyName("legalAction")]
    public LegalAction LegalAction
    {
      get => legalAction ??= new();
      set => legalAction = value;
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
    /// A value of Hidden.
    /// </summary>
    [JsonPropertyName("hidden")]
    public NextTranInfo Hidden
    {
      get => hidden ??= new();
      set => hidden = value;
    }

    /// <summary>
    /// A value of AccrualOrDue.
    /// </summary>
    [JsonPropertyName("accrualOrDue")]
    public DateWorkArea AccrualOrDue
    {
      get => accrualOrDue ??= new();
      set => accrualOrDue = value;
    }

    /// <summary>
    /// A value of TotalAmountDue.
    /// </summary>
    [JsonPropertyName("totalAmountDue")]
    public Common TotalAmountDue
    {
      get => totalAmountDue ??= new();
      set => totalAmountDue = value;
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

    private Array<OcpGroup> ocp;
    private Collection last;
    private Collection first;
    private Common showActiveOnly;
    private LegalActionDetail legalActionDetail;
    private CsePersonAccount csePersonAccount;
    private Standard standard;
    private CsePerson csePerson;
    private CsePersonsWorkSet csePersonsWorkSet;
    private Obligation obligation;
    private ObligationType obligationType;
    private LegalAction legalAction;
    private ScreenOwedAmounts screenOwedAmounts;
    private NextTranInfo hidden;
    private DateWorkArea accrualOrDue;
    private Common totalAmountDue;
    private Code dlgflwRequired;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of AlrtHistCollProtAction.
    /// </summary>
    [JsonPropertyName("alrtHistCollProtAction")]
    public TextWorkArea AlrtHistCollProtAction
    {
      get => alrtHistCollProtAction ??= new();
      set => alrtHistCollProtAction = value;
    }

    /// <summary>
    /// A value of ValidateReturnCd.
    /// </summary>
    [JsonPropertyName("validateReturnCd")]
    public Common ValidateReturnCd
    {
      get => validateReturnCd ??= new();
      set => validateReturnCd = value;
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
    /// A value of HardcodeMedSuppForCash.
    /// </summary>
    [JsonPropertyName("hardcodeMedSuppForCash")]
    public ObligationType HardcodeMedSuppForCash
    {
      get => hardcodeMedSuppForCash ??= new();
      set => hardcodeMedSuppForCash = value;
    }

    /// <summary>
    /// A value of HardcodeMedicalJudgement.
    /// </summary>
    [JsonPropertyName("hardcodeMedicalJudgement")]
    public ObligationType HardcodeMedicalJudgement
    {
      get => hardcodeMedicalJudgement ??= new();
      set => hardcodeMedicalJudgement = value;
    }

    /// <summary>
    /// A value of HardcodeMedicalSupport.
    /// </summary>
    [JsonPropertyName("hardcodeMedicalSupport")]
    public ObligationType HardcodeMedicalSupport
    {
      get => hardcodeMedicalSupport ??= new();
      set => hardcodeMedicalSupport = value;
    }

    /// <summary>
    /// A value of NullObligCollProtectionHist.
    /// </summary>
    [JsonPropertyName("nullObligCollProtectionHist")]
    public ObligCollProtectionHist NullObligCollProtectionHist
    {
      get => nullObligCollProtectionHist ??= new();
      set => nullObligCollProtectionHist = value;
    }

    /// <summary>
    /// A value of NullCollection.
    /// </summary>
    [JsonPropertyName("nullCollection")]
    public Collection NullCollection
    {
      get => nullCollection ??= new();
      set => nullCollection = value;
    }

    /// <summary>
    /// A value of FoundConcurrentObl.
    /// </summary>
    [JsonPropertyName("foundConcurrentObl")]
    public Common FoundConcurrentObl
    {
      get => foundConcurrentObl ??= new();
      set => foundConcurrentObl = value;
    }

    /// <summary>
    /// A value of ConcurrentObligationType.
    /// </summary>
    [JsonPropertyName("concurrentObligationType")]
    public ObligationType ConcurrentObligationType
    {
      get => concurrentObligationType ??= new();
      set => concurrentObligationType = value;
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
    /// A value of ConcurrentCsePersonAccount.
    /// </summary>
    [JsonPropertyName("concurrentCsePersonAccount")]
    public CsePersonAccount ConcurrentCsePersonAccount
    {
      get => concurrentCsePersonAccount ??= new();
      set => concurrentCsePersonAccount = value;
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
    /// A value of OmitCrdInd.
    /// </summary>
    [JsonPropertyName("omitCrdInd")]
    public Common OmitCrdInd
    {
      get => omitCrdInd ??= new();
      set => omitCrdInd = value;
    }

    /// <summary>
    /// A value of TotalRecsSelectedFProc.
    /// </summary>
    [JsonPropertyName("totalRecsSelectedFProc")]
    public Common TotalRecsSelectedFProc
    {
      get => totalRecsSelectedFProc ??= new();
      set => totalRecsSelectedFProc = value;
    }

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
    /// A value of InitialisedToZeros.
    /// </summary>
    [JsonPropertyName("initialisedToZeros")]
    public DateWorkArea InitialisedToZeros
    {
      get => initialisedToZeros ??= new();
      set => initialisedToZeros = value;
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
    /// A value of HardcodeObligor.
    /// </summary>
    [JsonPropertyName("hardcodeObligor")]
    public CsePersonAccount HardcodeObligor
    {
      get => hardcodeObligor ??= new();
      set => hardcodeObligor = value;
    }

    private TextWorkArea alrtHistCollProtAction;
    private Common validateReturnCd;
    private CodeValue codeValue;
    private Code code;
    private ObligationType hardcodeMedSuppForCash;
    private ObligationType hardcodeMedicalJudgement;
    private ObligationType hardcodeMedicalSupport;
    private ObligCollProtectionHist nullObligCollProtectionHist;
    private Collection nullCollection;
    private Common foundConcurrentObl;
    private ObligationType concurrentObligationType;
    private CsePerson concurrentCsePerson;
    private CsePersonAccount concurrentCsePersonAccount;
    private Obligation concurrentObligation;
    private Common omitCrdInd;
    private Common totalRecsSelectedFProc;
    private DateWorkArea maxDate;
    private DateWorkArea initialisedToZeros;
    private DateWorkArea current;
    private CsePersonAccount hardcodeObligor;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
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
    /// A value of ObligationTransaction.
    /// </summary>
    [JsonPropertyName("obligationTransaction")]
    public ObligationTransaction ObligationTransaction
    {
      get => obligationTransaction ??= new();
      set => obligationTransaction = value;
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

    private Collection collection;
    private ObligationTransaction obligationTransaction;
    private CsePersonAccount csePersonAccount;
    private ObligationType obligationType;
    private CsePerson csePerson;
    private Obligation obligation;
    private ObligationPaymentSchedule obligationPaymentSchedule;
  }
#endregion
}
