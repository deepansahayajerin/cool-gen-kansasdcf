// Program: SI_CREATE_AUTO_CSENET_TRANS, ID: 372629389, model: 746.
// Short name: SWE01196
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: SI_CREATE_AUTO_CSENET_TRANS.
/// </para>
/// <para>
/// AB will determine under what situations a particular RS Code will be created
/// thus setting off its repective transaction event and associated data blocks
/// for OUTGOING CSENet interstate cases.
/// After a check for required fields, a determination is made whether or not 
/// the case is an outgoing interstate csenet case (originated in KS) or is a
/// responding interstate csenet case (not originated in KS).  At that point,
/// processing will enter the needed AB.
/// Reason for reads:
/// Case		Case, Participant data blocks
/// Cse Person	AP Identification, Participant data blocks
/// Legal Action	Order data block
/// </para>
/// </summary>
[Serializable]
public partial class SiCreateAutoCsenetTrans: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_CREATE_AUTO_CSENET_TRANS program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiCreateAutoCsenetTrans(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiCreateAutoCsenetTrans.
  /// </summary>
  public SiCreateAutoCsenetTrans(IContext context, Import import, Export export):
    
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
    // Change Log:
    // 05/15/1999	PMcElderry
    // Original coding.   The complementing incoming (IC) interstate
    // case (IC) AB is SI_PROCESS_EVENT_TRANS_FOR_IC_IC
    // 12/09/1999	PMcElderry
    // PR #'s 82003, 82006
    // 05/03/2000	PMcElderry
    // PR# 93888 =>  Currency not established for GTSC
    // 07/24/2000	PMcElderry
    // PR# 91860 =>  Special update conditions from ADDR/FADS
    // 03/12/01 C Fairley
    // 00104264  Moved the CASE status to INTERSTATE_CASE
    // case_status
    // 05/06/01 C Fairley  I00118986  View matched
    // export_order_db_created to a local and incremented the
    // local_order_indicator using local view.
    // 05/21/01 C Fairley
    // WR 010357  Case level program for "NC" should be "NC" -
    // not "NA".  (Business rule change per Ralph Malott)
    // 07/13/2001	PMcElderry
    // PR #123170 => When closing from IIMC, CSEnet
    // transaction is not being sent to initiating state.
    // Corrected problem with fix made on WR # 10357.
    // 07/16/2001	MLachowicz
    // PR # 122926 => Transaction GSPAD erroring out when
    // address changed for AR
    // 09/28/2001	SWSRPRM
    // PR 128006 => Read logic for NADR needs to be better
    // qualified due to -811 errors.
    // 04/23/2002	M Ramirez
    // Misc changes to improve performance and results
    // 04/23/2002	M Ramirez
    // Renamed AB from SI_PROCESS_EVENT_TRANS_FOR_OG_IC
    // to SI_CREATE_AUTO_CSENET_TRANS
    // 02-07-2011	T. Pierce	CQ24439
    // Incorporate changes for new closure codes and equate with appropriate GSC
    // transaction types.
    // ----------------------------------------------------------------
    // ------------------------------------------------------------------
    // KS_CASE_IND values:
    // <Y>	Outgoing Interstate Involvement
    // <N>	Incoming Interstate Involvement
    // <space>	Neither (used for LO1, CSI and MSC correspondence)
    // ------------------------------------------------------------------
    // ------------------------------------------------------
    // All CSENet actions performed in this AB are provisions
    // (Action Code = P)
    // ------------------------------------------------------
    // ------------------------------------------------------
    // All CSENet actions performed in this AB use
    // Function Type Code = ENF, EST, MSC and PAT.
    // No LO1, CSI or COL transactions are currently sent
    // from this AB
    // ------------------------------------------------------
    // lss--09-24-2014
    // ----------------------------------------------------
    // GTSC screen - changed the READ EACH for interestate_request case
    // to read for the case equal to the import case. It was reading for
    // any interstate case for the person causing a KS only case to send
    // transactions on the interstate case for that person if one existed.
    // -------------------------------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";

    // mjr
    // ----------------------------------------------
    // 04/19/2002
    // Added to determine whether it was called in batch
    // -----------------------------------------------------------
    if (!IsEmpty(Substring(global.TranCode, 5, 4)))
    {
      local.Batch.Flag = "Y";
    }

    local.ScreenAction.Command = global.Command;
    local.Current.Date = Now().Date;
    local.Screen.Command = Substring(import.ScreenIdentification.Command, 1, 4);

    if (!IsEmpty(import.Specific.ActionReasonCode))
    {
      local.Specific.ActionReasonCode = import.Specific.ActionReasonCode;
    }
    else
    {
      local.Specific.ActionReasonCode =
        Substring(import.ScreenIdentification.Command, 6, 5);
    }

    local.Transactions.Index = -1;

    if (Equal(local.Screen.Command, "ASIN") || Equal
      (local.Screen.Command, "IIOI") || Equal(local.Screen.Command, "ROLE"))
    {
      foreach(var item in ReadInterstateRequest3())
      {
        if (AsChar(entities.InterstateRequest.KsCaseInd) == 'Y')
        {
          if (Equal(local.Screen.Command, "IIOI"))
          {
            continue;
          }
        }

        if (local.Transactions.Index + 1 >= Local.TransactionsGroup.Capacity)
        {
          break;
        }

        ++local.Transactions.Index;
        local.Transactions.CheckSize();

        local.Transactions.Update.GlocalTxnCase.Number = import.Case1.Number;
        local.Transactions.Update.GlocalTxnInterstateRequest.Assign(
          entities.InterstateRequest);

        switch(TrimEnd(local.Screen.Command))
        {
          case "ASIN":
            local.Transactions.Update.GlocalTxnInterstateRequestHistory.
              ActionReasonCode = "GSWKR";

            break;
          case "IIOI":
            local.Transactions.Update.GlocalTxnInterstateRequestHistory.
              ActionReasonCode = "AADIN";

            break;
          case "ROLE":
            switch(TrimEnd(local.ScreenAction.Command))
            {
              case "CREATE":
                local.Transactions.Update.GlocalTxnInterstateRequestHistory.
                  ActionReasonCode = "GSADD";

                break;
              case "UPDATE":
                local.Transactions.Update.GlocalTxnInterstateRequestHistory.
                  ActionReasonCode = "GSDEL";

                break;
              default:
                break;
            }

            break;
          default:
            break;
        }
      }
    }
    else if (Equal(local.Screen.Command, "IIMC"))
    {
      if (ReadInterstateRequest2())
      {
        if (AsChar(entities.InterstateRequest.KsCaseInd) == 'Y')
        {
          goto Test1;
        }

        ++local.Transactions.Index;
        local.Transactions.CheckSize();

        local.Transactions.Update.GlocalTxnCase.Number = import.Case1.Number;
        local.Transactions.Update.GlocalTxnInterstateRequest.Assign(
          entities.InterstateRequest);

        // -----------------------------------------------------------
        // CQ 24439 2/2011  T. Pierce
        // Added conditional statements for Case closure codes
        // "IC", "IN", "IS", "IW".
        // Case "CC" was incorrectly specified twice in this structure.
        // Changed the incorrect occurrence to "LO", which had been
        // incorrectly omitted.
        // Case "MJ" was corrected to equate with "GSC02"
        // -----------------------------------------------------------
        if (Equal(import.Case1.ClosureReason, "MJ"))
        {
          local.Transactions.Update.GlocalTxnInterstateRequestHistory.
            ActionReasonCode = "GSC02";
        }
        else if (Equal(import.Case1.ClosureReason, "CC"))
        {
          local.Transactions.Update.GlocalTxnInterstateRequestHistory.
            ActionReasonCode = "GSC02";
        }
        else if (Equal(import.Case1.ClosureReason, "DC"))
        {
          local.Transactions.Update.GlocalTxnInterstateRequestHistory.
            ActionReasonCode = "GSC03";
        }
        else if (Equal(import.Case1.ClosureReason, "NP"))
        {
          local.Transactions.Update.GlocalTxnInterstateRequestHistory.
            ActionReasonCode = "GSC4B";
        }
        else if (Equal(import.Case1.ClosureReason, "EM"))
        {
          local.Transactions.Update.GlocalTxnInterstateRequestHistory.
            ActionReasonCode = "GSC4A";
        }
        else if (Equal(import.Case1.ClosureReason, "4D"))
        {
          local.Transactions.Update.GlocalTxnInterstateRequestHistory.
            ActionReasonCode = "GSC4C";
        }
        else if (Equal(import.Case1.ClosureReason, "NL"))
        {
          local.Transactions.Update.GlocalTxnInterstateRequestHistory.
            ActionReasonCode = "GSC05";
        }
        else if (Equal(import.Case1.ClosureReason, "AB"))
        {
          local.Transactions.Update.GlocalTxnInterstateRequestHistory.
            ActionReasonCode = "GSC06";
        }
        else if (Equal(import.Case1.ClosureReason, "FO"))
        {
          local.Transactions.Update.GlocalTxnInterstateRequestHistory.
            ActionReasonCode = "GSC07";
        }
        else if (Equal(import.Case1.ClosureReason, "LO"))
        {
          local.Transactions.Update.GlocalTxnInterstateRequestHistory.
            ActionReasonCode = "GSC08";
        }
        else if (Equal(import.Case1.ClosureReason, "AR"))
        {
          local.Transactions.Update.GlocalTxnInterstateRequestHistory.
            ActionReasonCode = "GSC09";
        }
        else if (Equal(import.Case1.ClosureReason, "GC"))
        {
          local.Transactions.Update.GlocalTxnInterstateRequestHistory.
            ActionReasonCode = "GSC10";
        }
        else if (Equal(import.Case1.ClosureReason, "LC"))
        {
          local.Transactions.Update.GlocalTxnInterstateRequestHistory.
            ActionReasonCode = "GSC11";
        }
        else if (Equal(import.Case1.ClosureReason, "FC"))
        {
          local.Transactions.Update.GlocalTxnInterstateRequestHistory.
            ActionReasonCode = "GSC12";
        }
        else if (Equal(import.Case1.ClosureReason, "IC"))
        {
          local.Transactions.Update.GlocalTxnInterstateRequestHistory.
            ActionReasonCode = "GSC16";
        }
        else if (Equal(import.Case1.ClosureReason, "IN"))
        {
          local.Transactions.Update.GlocalTxnInterstateRequestHistory.
            ActionReasonCode = "GSC15";
        }
        else if (Equal(import.Case1.ClosureReason, "IS"))
        {
          local.Transactions.Update.GlocalTxnInterstateRequestHistory.
            ActionReasonCode = "GSC13";
        }
        else if (Equal(import.Case1.ClosureReason, "IW"))
        {
          local.Transactions.Update.GlocalTxnInterstateRequestHistory.
            ActionReasonCode = "GSC18";
        }
        else
        {
        }
      }
    }
    else if (Equal(local.Screen.Command, "GTSC"))
    {
      // lss--09-24-2014
      // ----------------------------------------------------
      // GTSC screen - changed the READ EACH for interestate_request case
      // to read for the case equal to the import case. It was reading for
      // any interstate case for the person causing a KS only case to send
      // transactions on the interstate case for that person if one existed.
      // -------------------------------------------------------------------
      foreach(var item in ReadInterstateRequestCase2())
      {
        if (local.Transactions.Index + 1 >= Local.TransactionsGroup.Capacity)
        {
          break;
        }

        ++local.Transactions.Index;
        local.Transactions.CheckSize();

        local.Transactions.Update.GlocalTxnCase.Number = entities.Case1.Number;
        local.Transactions.Update.GlocalTxnInterstateRequest.Assign(
          entities.InterstateRequest);
        local.Transactions.Update.GlocalTxnInterstateRequestHistory.
          ActionReasonCode = local.Specific.ActionReasonCode ?? "";
      }
    }
    else if (Equal(local.Screen.Command, "ADDR") || Equal
      (local.Screen.Command, "INCS") || Equal(local.Screen.Command, "IADA"))
    {
      if (Equal(local.Screen.Command, "ADDR"))
      {
        if (Equal(local.Specific.ActionReasonCode, "GSPAD") || Equal
          (local.Specific.ActionReasonCode, "GSPAY"))
        {
          return;
        }
      }

      local.CsePerson.Number = import.CsePerson.Number;

      foreach(var item in ReadInterstateRequestCase1())
      {
        if (AsChar(entities.InterstateRequest.KsCaseInd) == 'Y')
        {
          if (Equal(local.Screen.Command, "IADA"))
          {
            continue;
          }
        }

        if (local.Transactions.Index + 1 >= Local.TransactionsGroup.Capacity)
        {
          break;
        }

        ++local.Transactions.Index;
        local.Transactions.CheckSize();

        local.Transactions.Update.GlocalTxnCase.Number = entities.Case1.Number;
        local.Transactions.Update.GlocalTxnInterstateRequest.Assign(
          entities.InterstateRequest);

        switch(TrimEnd(local.Screen.Command))
        {
          case "ADDR":
            local.Transactions.Update.GlocalTxnInterstateRequestHistory.
              ActionReasonCode = local.Specific.ActionReasonCode ?? "";

            break;
          case "INCS":
            local.Transactions.Update.GlocalTxnInterstateRequestHistory.
              ActionReasonCode = "LSEMP";

            break;
          case "IADA":
            switch(TrimEnd(local.Specific.ActionReasonCode ?? ""))
            {
              case "OCASE":
                local.Transactions.Update.GlocalTxnInterstateRequestHistory.
                  ActionReasonCode = "EILRE";

                break;
              case "OPERS":
                local.Transactions.Update.GlocalTxnInterstateRequestHistory.
                  ActionReasonCode = "EILRE";

                break;
              case "LCASE":
                local.Transactions.Update.GlocalTxnInterstateRequestHistory.
                  ActionReasonCode = "EILMV";

                break;
              case "LPERS":
                local.Transactions.Update.GlocalTxnInterstateRequestHistory.
                  ActionReasonCode = "EILMV";

                break;
              default:
                break;
            }

            break;
          default:
            break;
        }
      }

      // -----------------------------------------------------------
      // PR# 91860 - If a mistake was made during entry,
      // delete any transactions related to the same action
      // -----------------------------------------------------------
      if (Equal(local.Screen.Command, "ADDR"))
      {
        for(local.Transactions.Index = 0; local.Transactions.Index < local
          .Transactions.Count; ++local.Transactions.Index)
        {
          if (!local.Transactions.CheckSize())
          {
            break;
          }

          if (ReadInterstateRequest1())
          {
            foreach(var item in ReadInterstateRequestHistory())
            {
              if (ReadInterstateCase())
              {
                if (ReadCsenetTransactionEnvelop())
                {
                  DeleteCsenetTransactionEnvelop();
                  DeleteInterstateCase();
                  DeleteInterstateRequestHistory();
                }
              }
            }
          }
        }

        local.Transactions.CheckIndex();
      }
    }
    else if (Equal(local.Screen.Command, "LACT") || Equal
      (local.Screen.Command, "HEAR"))
    {
      foreach(var item in ReadCaseRole())
      {
        foreach(var item1 in ReadInterstateRequestCase3())
        {
          if (AsChar(entities.InterstateRequest.KsCaseInd) == 'Y')
          {
            if (Equal(local.Screen.Command, "LACT"))
            {
              continue;
            }
          }

          if (local.Transactions.Index + 1 >= Local.TransactionsGroup.Capacity)
          {
            goto ReadEach;
          }

          ++local.Transactions.Index;
          local.Transactions.CheckSize();

          local.Transactions.Update.GlocalTxnCase.Number =
            entities.Case1.Number;
          local.Transactions.Update.GlocalTxnInterstateRequest.Assign(
            entities.InterstateRequest);

          switch(TrimEnd(local.Screen.Command))
          {
            case "HEAR":
              if (!IsEmpty(local.Specific.ActionReasonCode))
              {
                local.Transactions.Update.GlocalTxnInterstateRequestHistory.
                  ActionReasonCode = local.Specific.ActionReasonCode ?? "";
              }
              else
              {
                local.Transactions.Update.GlocalTxnInterstateRequestHistory.
                  ActionReasonCode = "GIHER";
              }

              break;
            case "LACT":
              local.Transactions.Update.GlocalTxnInterstateRequestHistory.
                ActionReasonCode = local.Specific.ActionReasonCode ?? "";

              break;
            default:
              break;
          }
        }
      }

ReadEach:
      ;
    }
    else
    {
      return;
    }

Test1:

    if (local.Transactions.Count <= 0)
    {
      return;
    }

    local.Transactions.Index = 0;

    for(var limit = local.Transactions.Count; local.Transactions.Index < limit; ++
      local.Transactions.Index)
    {
      if (!local.Transactions.CheckSize())
      {
        break;
      }

      local.InterstateCase.Assign(local.NullInterstateCase);
      local.InterstateApIdentification.Assign(
        local.NullInterstateApIdentification);
      local.InterstateApLocate.Assign(local.NullInterstateApLocate);
      local.InterstateMiscellaneous.Assign(local.NullInterstateMiscellaneous);
      local.InterstateRequestHistory.Assign(local.NullInterstateRequestHistory);

      for(local.Participants.Index = 0; local.Participants.Index < local
        .Participants.Count; ++local.Participants.Index)
      {
        if (!local.Participants.CheckSize())
        {
          break;
        }

        local.Participants.Update.G.Assign(local.NullInterstateParticipant);
      }

      local.Participants.CheckIndex();
      local.Participants.Count = 0;

      for(local.Orders.Index = 0; local.Orders.Index < local.Orders.Count; ++
        local.Orders.Index)
      {
        if (!local.Orders.CheckSize())
        {
          break;
        }

        local.Orders.Update.G.Assign(local.NullInterstateSupportOrder);
      }

      local.Orders.CheckIndex();
      local.Orders.Count = 0;

      // ------------------------------------------------------
      // Verify this state accepts CSENet transactions
      // ------------------------------------------------------
      if (ReadFips())
      {
        local.CsenetStateTable.StateCode = entities.Fips.StateAbbreviation;
        UseSiReadCsenetStateTable();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          ExitState = "ACO_NN0000_ALL_OK";

          continue;
        }

        if (AsChar(local.CsenetStateTable.CsenetReadyInd) == 'N')
        {
          continue;
        }

        if (AsChar(local.CsenetStateTable.RecStateInd) == 'N')
        {
          continue;
        }
      }
      else
      {
        continue;
      }

      local.InterstateCase.OtherFipsState =
        local.Transactions.Item.GlocalTxnInterstateRequest.OtherStateFips;
      local.InterstateCase.OtherFipsCounty = 0;
      local.InterstateCase.OtherFipsLocation = 0;
      local.InterstateCase.InterstateCaseId =
        local.Transactions.Item.GlocalTxnInterstateRequest.OtherStateCaseId ?? ""
        ;
      local.InterstateCase.ActionCode = "P";

      if (IsEmpty(local.Transactions.Item.GlocalTxnInterstateRequestHistory.
        ActionReasonCode))
      {
        ExitState = "SI0000_INTERSTATE_TRANS_ERROR_RB";

        break;
      }

      local.InterstateCase.ActionReasonCode =
        local.Transactions.Item.GlocalTxnInterstateRequestHistory.
          ActionReasonCode ?? "";

      // ------------------------------------------------------
      // Determine the Functional Type Code
      // ------------------------------------------------------
      switch(TrimEnd(Substring(local.InterstateCase.ActionReasonCode, 1, 1)))
      {
        case "E":
          local.InterstateCase.FunctionalTypeCode = "ENF";

          break;
        case "G":
          local.InterstateCase.FunctionalTypeCode = "MSC";

          break;
        case "L":
          local.InterstateCase.FunctionalTypeCode = "MSC";

          break;
        case "P":
          local.InterstateCase.FunctionalTypeCode = "PAT";

          break;
        case "S":
          local.InterstateCase.FunctionalTypeCode = "EST";

          break;
        default:
          continue;
      }

      // ------------------------------------------------------
      // ENF, EST and PAT transactions cannot be sent if the
      // interstate involvement is limited to LO1s, CSIs or MSCs.
      // In this action block we don't create LO1 nor CSI
      // transactions, so there is no need to check for them.
      // ------------------------------------------------------
      if (!Equal(local.InterstateCase.FunctionalTypeCode, "MSC"))
      {
        if (IsEmpty(local.Transactions.Item.GlocalTxnInterstateRequest.KsCaseInd))
          
        {
          continue;
        }
      }

      if (import.LegalAction.Identifier > 0)
      {
        local.LegalActions.Index = 0;
        local.LegalActions.CheckSize();

        local.LegalActions.Update.GlocalLegalAction.Identifier =
          import.LegalAction.Identifier;
      }

      UseSiGetDataInterstateCaseDb();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        ExitState = "ACO_NN0000_ALL_OK";

        continue;
      }

      // ------------------------------------------------------
      // Determine primary AP
      // ------------------------------------------------------
      if (ReadCsePerson())
      {
        local.PrimaryAp.Number = entities.CsePerson.Number;
      }
      else
      {
        continue;
      }

      UseSiGetDataInterstateApIdDb();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        ExitState = "ACO_NN0000_ALL_OK";

        if (!Equal(local.InterstateCase.FunctionalTypeCode, "MSC"))
        {
          continue;
        }
      }

      if (local.InterstateCase.ApIdentificationInd.GetValueOrDefault() > 0)
      {
        UseSiGetDataInterstateApLocDb();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          ExitState = "ACO_NN0000_ALL_OK";

          continue;
        }
      }

      if (Equal(local.Screen.Command, "GTSC") && !
        IsEmpty(import.GeneticTestInformation.ChildPersonNo))
      {
        local.Children.Index = 0;
        local.Children.CheckSize();

        local.Children.Update.GlocalChild.Number =
          import.GeneticTestInformation.ChildPersonNo;
      }

      UseSiGetDataInterstatePartDbs();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        ExitState = "ACO_NN0000_ALL_OK";

        continue;
      }

      // mjr--->  Set attributes for Interstate Order
      if (local.InterstateCase.ApIdentificationInd.GetValueOrDefault() > 0)
      {
        if (Equal(local.Screen.Command, "HEAR") || Equal
          (local.Screen.Command, "GTSC"))
        {
          goto Test2;
        }

        UseSiGetDataInterstateOrderDb();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          ExitState = "ACO_NN0000_ALL_OK";
        }
      }

Test2:

      // mjr--->  Set attributes for Interstate Miscellaneous
      if (Equal(local.Screen.Command, "HEAR") || !
        IsEmpty(local.FamilyViolence.Flag))
      {
        if (Equal(local.Screen.Command, "HEAR"))
        {
          // -----------------------------------------------------------
          // The date of the hearing needs to appear on the text line on
          // the miscellaneous DB
          // -----------------------------------------------------------
          local.DateWorkArea.Date = import.Misc.Date;
          UseSpDocFormatDate();
          local.InterstateMiscellaneous.InformationTextLine1 =
            "HEARING DATE IS " + (local.FieldValue.Value ?? "");
        }

        if (!IsEmpty(local.FamilyViolence.Flag))
        {
          if (!IsEmpty(local.InterstateMiscellaneous.InformationTextLine1))
          {
            local.InterstateMiscellaneous.InformationTextLine2 =
              ";  Protected due to family violence";
          }
          else
          {
            local.InterstateMiscellaneous.InformationTextLine1 =
              "Protected due to family violence";
          }
        }

        local.InterstateMiscellaneous.StatusChangeCode = "O";
        local.InterstateCase.InformationInd = 1;
      }

      // mjr--->  Set attributes for Interstate Request History
      local.InterstateRequestHistory.TransactionDirectionInd = "O";
      local.InterstateRequestHistory.TransactionSerialNum =
        local.InterstateCase.TransSerialNumber;
      local.InterstateRequestHistory.TransactionDate =
        local.InterstateCase.TransactionDate;
      local.InterstateRequestHistory.FunctionalTypeCode =
        local.InterstateCase.FunctionalTypeCode;
      local.InterstateRequestHistory.ActionCode =
        local.InterstateCase.ActionCode;
      local.InterstateRequestHistory.ActionReasonCode =
        local.InterstateCase.ActionReasonCode ?? "";
      local.InterstateRequestHistory.ActionResolutionDate =
        local.InterstateCase.ActionResolutionDate;
      local.InterstateRequestHistory.CreatedBy = local.Screen.Command;
      local.InterstateRequestHistory.AttachmentIndicator =
        local.InterstateCase.AttachmentsInd;
      local.InterstateRequestHistory.Note =
        local.InterstateMiscellaneous.InformationTextLine1 ?? "";

      if (!IsEmpty(local.InterstateMiscellaneous.InformationTextLine2))
      {
        local.InterstateRequestHistory.Note =
          TrimEnd(local.InterstateRequestHistory.Note) + (
            local.InterstateMiscellaneous.InformationTextLine2 ?? "");
      }

      // mjr--->  Create datablocks
      UseSiCreateInterstateCase();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        break;
      }

      UseSiCreateOgCsenetEnvelop();

      if (!IsExitState("ACO_NI0000_SUCCESSFUL_ADD"))
      {
        break;
      }
      else
      {
        ExitState = "ACO_NN0000_ALL_OK";
      }

      if (local.InterstateCase.ApIdentificationInd.GetValueOrDefault() > 0)
      {
        UseSiCreateInterstateApId();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          break;
        }
      }

      if (local.InterstateCase.ApLocateDataInd.GetValueOrDefault() > 0)
      {
        UseSiCreateInterstateApLocate();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          break;
        }
      }

      if (local.InterstateCase.ParticipantDataInd.GetValueOrDefault() > 0)
      {
        for(local.Participants.Index = 0; local.Participants.Index < local
          .Participants.Count; ++local.Participants.Index)
        {
          if (!local.Participants.CheckSize())
          {
            break;
          }

          UseSiCreateInterstateParticipant();

          if (!IsExitState("ACO_NI0000_SUCCESSFUL_ADD"))
          {
            goto AfterCycle;
          }
          else
          {
            ExitState = "ACO_NN0000_ALL_OK";
          }
        }

        local.Participants.CheckIndex();
      }

      if (local.InterstateCase.OrderDataInd.GetValueOrDefault() > 0)
      {
        for(local.Orders.Index = 0; local.Orders.Index < local.Orders.Count; ++
          local.Orders.Index)
        {
          if (!local.Orders.CheckSize())
          {
            break;
          }

          UseSiCreateInterstateOrder();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            goto AfterCycle;
          }
        }

        local.Orders.CheckIndex();
      }

      if (local.InterstateCase.InformationInd.GetValueOrDefault() > 0)
      {
        UseSiCreateInterstateMisc();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          break;
        }
      }

      // mjr--->  Create Interstate Request History
      UseSiCabCreateIsRequestHistory();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        break;
      }
    }

AfterCycle:

    local.Transactions.CheckIndex();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      if (AsChar(local.Batch.Flag) != 'Y')
      {
        UseEabRollbackCics();
      }
    }
  }

  private static void MoveChildren(Local.ChildrenGroup source,
    SiGetDataInterstatePartDbs.Import.ChildrenGroup target)
  {
    target.GimportChild.Number = source.GlocalChild.Number;
  }

  private static void MoveGroupToOrders(SiGetDataInterstateOrderDb.Export.
    GroupGroup source, Local.OrdersGroup target)
  {
    target.G.Assign(source.G);
  }

  private static void MoveGroupToParticipants(SiGetDataInterstatePartDbs.Export.
    GroupGroup source, Local.ParticipantsGroup target)
  {
    target.GlocalParticipant.Number = source.GexportParticipant.Number;
    target.G.Assign(source.G);
  }

  private static void MoveInterstateCase1(InterstateCase source,
    InterstateCase target)
  {
    target.OtherFipsState = source.OtherFipsState;
    target.OtherFipsCounty = source.OtherFipsCounty;
    target.OtherFipsLocation = source.OtherFipsLocation;
    target.ActionCode = source.ActionCode;
    target.FunctionalTypeCode = source.FunctionalTypeCode;
    target.InterstateCaseId = source.InterstateCaseId;
    target.ActionReasonCode = source.ActionReasonCode;
    target.AttachmentsInd = source.AttachmentsInd;
  }

  private static void MoveInterstateCase2(InterstateCase source,
    InterstateCase target)
  {
    target.TransSerialNumber = source.TransSerialNumber;
    target.TransactionDate = source.TransactionDate;
  }

  private static void MoveInterstateCase3(InterstateCase source,
    InterstateCase target)
  {
    target.ActionCode = source.ActionCode;
    target.FunctionalTypeCode = source.FunctionalTypeCode;
  }

  private static void MoveLegalActions1(Local.LegalActionsGroup source,
    SiGetDataInterstateOrderDb.Import.LegalActionsGroup target)
  {
    target.G.Identifier = source.GlocalLegalAction.Identifier;
  }

  private static void MoveLegalActions2(Local.LegalActionsGroup source,
    SiGetDataInterstateCaseDb.Import.LegalActionsGroup target)
  {
    target.G.Identifier = source.GlocalLegalAction.Identifier;
  }

  private static void MoveParticipants(Local.ParticipantsGroup source,
    SiGetDataInterstateOrderDb.Import.ParticipantsGroup target)
  {
    target.GimportParticipant.Number = source.GlocalParticipant.Number;
    target.G.Assign(source.G);
  }

  private void UseEabRollbackCics()
  {
    var useImport = new EabRollbackCics.Import();
    var useExport = new EabRollbackCics.Export();

    Call(EabRollbackCics.Execute, useImport, useExport);
  }

  private void UseSiCabCreateIsRequestHistory()
  {
    var useImport = new SiCabCreateIsRequestHistory.Import();
    var useExport = new SiCabCreateIsRequestHistory.Export();

    useImport.InterstateRequest.IntHGeneratedId =
      local.Transactions.Item.GlocalTxnInterstateRequest.IntHGeneratedId;
    useImport.InterstateRequestHistory.Assign(local.InterstateRequestHistory);

    Call(SiCabCreateIsRequestHistory.Execute, useImport, useExport);
  }

  private void UseSiCreateInterstateApId()
  {
    var useImport = new SiCreateInterstateApId.Import();
    var useExport = new SiCreateInterstateApId.Export();

    MoveInterstateCase2(local.InterstateCase, useImport.InterstateCase);
    useImport.InterstateApIdentification.
      Assign(local.InterstateApIdentification);

    Call(SiCreateInterstateApId.Execute, useImport, useExport);
  }

  private void UseSiCreateInterstateApLocate()
  {
    var useImport = new SiCreateInterstateApLocate.Import();
    var useExport = new SiCreateInterstateApLocate.Export();

    MoveInterstateCase2(local.InterstateCase, useImport.InterstateCase);
    useImport.InterstateApLocate.Assign(local.InterstateApLocate);

    Call(SiCreateInterstateApLocate.Execute, useImport, useExport);
  }

  private void UseSiCreateInterstateCase()
  {
    var useImport = new SiCreateInterstateCase.Import();
    var useExport = new SiCreateInterstateCase.Export();

    useImport.InterstateCase.Assign(local.InterstateCase);

    Call(SiCreateInterstateCase.Execute, useImport, useExport);
  }

  private void UseSiCreateInterstateMisc()
  {
    var useImport = new SiCreateInterstateMisc.Import();
    var useExport = new SiCreateInterstateMisc.Export();

    MoveInterstateCase2(local.InterstateCase, useImport.InterstateCase);
    useImport.InterstateMiscellaneous.Assign(local.InterstateMiscellaneous);

    Call(SiCreateInterstateMisc.Execute, useImport, useExport);
  }

  private void UseSiCreateInterstateOrder()
  {
    var useImport = new SiCreateInterstateOrder.Import();
    var useExport = new SiCreateInterstateOrder.Export();

    MoveInterstateCase2(local.InterstateCase, useImport.InterstateCase);
    useImport.InterstateSupportOrder.Assign(local.Orders.Item.G);

    Call(SiCreateInterstateOrder.Execute, useImport, useExport);
  }

  private void UseSiCreateInterstateParticipant()
  {
    var useImport = new SiCreateInterstateParticipant.Import();
    var useExport = new SiCreateInterstateParticipant.Export();

    MoveInterstateCase2(local.InterstateCase, useImport.InterstateCase);
    useImport.InterstateParticipant.Assign(local.Participants.Item.G);

    Call(SiCreateInterstateParticipant.Execute, useImport, useExport);
  }

  private void UseSiCreateOgCsenetEnvelop()
  {
    var useImport = new SiCreateOgCsenetEnvelop.Import();
    var useExport = new SiCreateOgCsenetEnvelop.Export();

    MoveInterstateCase2(local.InterstateCase, useImport.InterstateCase);

    Call(SiCreateOgCsenetEnvelop.Execute, useImport, useExport);
  }

  private void UseSiGetDataInterstateApIdDb()
  {
    var useImport = new SiGetDataInterstateApIdDb.Import();
    var useExport = new SiGetDataInterstateApIdDb.Export();

    useImport.Batch.Flag = local.Batch.Flag;
    useImport.Ap.Number = local.PrimaryAp.Number;

    Call(SiGetDataInterstateApIdDb.Execute, useImport, useExport);

    local.InterstateCase.ApIdentificationInd =
      useExport.InterstateCase.ApIdentificationInd;
    local.InterstateApIdentification.
      Assign(useExport.InterstateApIdentification);
  }

  private void UseSiGetDataInterstateApLocDb()
  {
    var useImport = new SiGetDataInterstateApLocDb.Import();
    var useExport = new SiGetDataInterstateApLocDb.Export();

    useImport.Batch.Flag = local.Batch.Flag;
    useImport.Ap.Number = local.PrimaryAp.Number;
    useImport.Current.Date = local.Current.Date;

    Call(SiGetDataInterstateApLocDb.Execute, useImport, useExport);

    local.FamilyViolence.Flag = useExport.FamilyViolence.Flag;
    local.InterstateCase.ApLocateDataInd =
      useExport.InterstateCase.ApLocateDataInd;
    local.InterstateApLocate.Assign(useExport.InterstateApLocate);
  }

  private void UseSiGetDataInterstateCaseDb()
  {
    var useImport = new SiGetDataInterstateCaseDb.Import();
    var useExport = new SiGetDataInterstateCaseDb.Export();

    useImport.Case1.Number = local.Transactions.Item.GlocalTxnCase.Number;
    MoveInterstateCase1(local.InterstateCase, useImport.InterstateCase);
    useImport.Current.Date = local.Current.Date;
    local.LegalActions.CopyTo(useImport.LegalActions, MoveLegalActions2);

    Call(SiGetDataInterstateCaseDb.Execute, useImport, useExport);

    local.InterstateCase.Assign(useExport.InterstateCase);
  }

  private void UseSiGetDataInterstateOrderDb()
  {
    var useImport = new SiGetDataInterstateOrderDb.Import();
    var useExport = new SiGetDataInterstateOrderDb.Export();

    useImport.Case1.Number = local.Transactions.Item.GlocalTxnCase.Number;
    local.LegalActions.CopyTo(useImport.LegalActions, MoveLegalActions1);
    local.Participants.CopyTo(useImport.Participants, MoveParticipants);
    useImport.Current.Date = local.Current.Date;
    useImport.PrimaryAp.Number = local.PrimaryAp.Number;

    Call(SiGetDataInterstateOrderDb.Execute, useImport, useExport);

    local.InterstateCase.OrderDataInd = useExport.InterstateCase.OrderDataInd;
    useExport.Group.CopyTo(local.Orders, MoveGroupToOrders);
  }

  private void UseSiGetDataInterstatePartDbs()
  {
    var useImport = new SiGetDataInterstatePartDbs.Import();
    var useExport = new SiGetDataInterstatePartDbs.Export();

    useImport.Case1.Number = local.Transactions.Item.GlocalTxnCase.Number;
    useImport.PrimaryAp.Number = local.PrimaryAp.Number;
    useImport.Batch.Flag = local.Batch.Flag;
    useImport.Current.Date = local.Current.Date;
    local.Children.CopyTo(useImport.Children, MoveChildren);
    MoveInterstateCase3(local.InterstateCase, useImport.InterstateCase);

    Call(SiGetDataInterstatePartDbs.Execute, useImport, useExport);

    useExport.Group.CopyTo(local.Participants, MoveGroupToParticipants);
    local.InterstateCase.ParticipantDataInd =
      useExport.InterstateCase.ParticipantDataInd;
  }

  private void UseSiReadCsenetStateTable()
  {
    var useImport = new SiReadCsenetStateTable.Import();
    var useExport = new SiReadCsenetStateTable.Export();

    useImport.CsenetStateTable.StateCode = local.CsenetStateTable.StateCode;

    Call(SiReadCsenetStateTable.Execute, useImport, useExport);

    local.CsenetStateTable.Assign(useExport.CsenetStateTable);
  }

  private void UseSpDocFormatDate()
  {
    var useImport = new SpDocFormatDate.Import();
    var useExport = new SpDocFormatDate.Export();

    useImport.DateWorkArea.Date = local.DateWorkArea.Date;

    Call(SpDocFormatDate.Execute, useImport, useExport);

    local.FieldValue.Value = useExport.FieldValue.Value;
  }

  private void DeleteCsenetTransactionEnvelop()
  {
    var ccaTransactionDt =
      entities.CsenetTransactionEnvelop.CcaTransactionDt.GetValueOrDefault();
    var ccaTransSerNum = entities.CsenetTransactionEnvelop.CcaTransSerNum;

    Update("DeleteCsenetTransactionEnvelop#1",
      (db, command) =>
      {
        db.SetDate(command, "ccaTransactionDt", ccaTransactionDt);
        db.SetInt64(command, "ccaTransSerNum", ccaTransSerNum);
      });

    Update("DeleteCsenetTransactionEnvelop#2",
      (db, command) =>
      {
        db.SetDate(command, "transactionDate", ccaTransactionDt);
        db.SetInt64(command, "transSerialNbr", ccaTransSerNum);
      });
  }

  private void DeleteInterstateCase()
  {
    Update("DeleteInterstateCase#1",
      (db, command) =>
      {
        db.
          SetInt64(command, "icsNo", entities.InterstateCase.TransSerialNumber);
          
        db.SetDate(
          command, "icsDate",
          entities.InterstateCase.TransactionDate.GetValueOrDefault());
      });

    Update("DeleteInterstateCase#2",
      (db, command) =>
      {
        db.
          SetInt64(command, "icsNo", entities.InterstateCase.TransSerialNumber);
          
        db.SetDate(
          command, "icsDate",
          entities.InterstateCase.TransactionDate.GetValueOrDefault());
      });
  }

  private void DeleteInterstateRequestHistory()
  {
    Update("DeleteInterstateRequestHistory",
      (db, command) =>
      {
        db.SetInt32(
          command, "intGeneratedId",
          entities.InterstateRequestHistory.IntGeneratedId);
        db.SetDateTime(
          command, "createdTstamp",
          entities.InterstateRequestHistory.CreatedTimestamp.
            GetValueOrDefault());
      });
  }

  private IEnumerable<bool> ReadCaseRole()
  {
    entities.CaseRole.Populated = false;

    return ReadEach("ReadCaseRole",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "startDate", local.Current.Date.GetValueOrDefault());
        db.SetNullableInt32(
          command, "lgaIdentifier", import.LegalAction.Identifier);
      },
      (db, reader) =>
      {
        entities.CaseRole.CasNumber = db.GetString(reader, 0);
        entities.CaseRole.CspNumber = db.GetString(reader, 1);
        entities.CaseRole.Type1 = db.GetString(reader, 2);
        entities.CaseRole.Identifier = db.GetInt32(reader, 3);
        entities.CaseRole.StartDate = db.GetNullableDate(reader, 4);
        entities.CaseRole.EndDate = db.GetNullableDate(reader, 5);
        entities.CaseRole.Populated = true;
        CheckValid<CaseRole>("Type1", entities.CaseRole.Type1);

        return true;
      });
  }

  private bool ReadCsePerson()
  {
    entities.CsePerson.Populated = false;

    return Read("ReadCsePerson",
      (db, command) =>
      {
        db.SetInt32(
          command, "identifier",
          local.Transactions.Item.GlocalTxnInterstateRequest.IntHGeneratedId);
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CsePerson.Populated = true;
      });
  }

  private bool ReadCsenetTransactionEnvelop()
  {
    entities.CsenetTransactionEnvelop.Populated = false;

    return Read("ReadCsenetTransactionEnvelop",
      (db, command) =>
      {
        db.SetDate(
          command, "ccaTransactionDt",
          entities.InterstateCase.TransactionDate.GetValueOrDefault());
        db.SetInt64(
          command, "ccaTransSerNum", entities.InterstateCase.TransSerialNumber);
          
      },
      (db, reader) =>
      {
        entities.CsenetTransactionEnvelop.CcaTransactionDt =
          db.GetDate(reader, 0);
        entities.CsenetTransactionEnvelop.CcaTransSerNum =
          db.GetInt64(reader, 1);
        entities.CsenetTransactionEnvelop.DirectionInd =
          db.GetString(reader, 2);
        entities.CsenetTransactionEnvelop.ProcessingStatusCode =
          db.GetString(reader, 3);
        entities.CsenetTransactionEnvelop.Populated = true;
      });
  }

  private bool ReadFips()
  {
    entities.Fips.Populated = false;

    return Read("ReadFips",
      (db, command) =>
      {
        db.SetInt32(
          command, "state",
          local.Transactions.Item.GlocalTxnInterstateRequest.OtherStateFips);
      },
      (db, reader) =>
      {
        entities.Fips.State = db.GetInt32(reader, 0);
        entities.Fips.County = db.GetInt32(reader, 1);
        entities.Fips.Location = db.GetInt32(reader, 2);
        entities.Fips.StateAbbreviation = db.GetString(reader, 3);
        entities.Fips.Populated = true;
      });
  }

  private bool ReadInterstateCase()
  {
    entities.InterstateCase.Populated = false;

    return Read("ReadInterstateCase",
      (db, command) =>
      {
        db.SetInt64(
          command, "transSerialNbr",
          entities.InterstateRequestHistory.TransactionSerialNum);
        db.SetDate(
          command, "transactionDate",
          entities.InterstateRequestHistory.TransactionDate.
            GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.InterstateCase.TransSerialNumber = db.GetInt64(reader, 0);
        entities.InterstateCase.TransactionDate = db.GetDate(reader, 1);
        entities.InterstateCase.Populated = true;
      });
  }

  private bool ReadInterstateRequest1()
  {
    entities.InterstateRequest.Populated = false;

    return Read("ReadInterstateRequest1",
      (db, command) =>
      {
        db.SetInt32(
          command, "identifier",
          local.Transactions.Item.GlocalTxnInterstateRequest.IntHGeneratedId);
      },
      (db, reader) =>
      {
        entities.InterstateRequest.IntHGeneratedId = db.GetInt32(reader, 0);
        entities.InterstateRequest.OtherStateCaseId =
          db.GetNullableString(reader, 1);
        entities.InterstateRequest.OtherStateFips = db.GetInt32(reader, 2);
        entities.InterstateRequest.OtherStateCaseStatus =
          db.GetString(reader, 3);
        entities.InterstateRequest.KsCaseInd = db.GetString(reader, 4);
        entities.InterstateRequest.CasINumber = db.GetNullableString(reader, 5);
        entities.InterstateRequest.CasNumber = db.GetNullableString(reader, 6);
        entities.InterstateRequest.CspNumber = db.GetNullableString(reader, 7);
        entities.InterstateRequest.CroType = db.GetNullableString(reader, 8);
        entities.InterstateRequest.CroId = db.GetNullableInt32(reader, 9);
        entities.InterstateRequest.Populated = true;
        CheckValid<InterstateRequest>("CroType",
          entities.InterstateRequest.CroType);
      });
  }

  private bool ReadInterstateRequest2()
  {
    entities.InterstateRequest.Populated = false;

    return Read("ReadInterstateRequest2",
      (db, command) =>
      {
        db.SetInt32(
          command, "identifier", import.InterstateRequest.IntHGeneratedId);
      },
      (db, reader) =>
      {
        entities.InterstateRequest.IntHGeneratedId = db.GetInt32(reader, 0);
        entities.InterstateRequest.OtherStateCaseId =
          db.GetNullableString(reader, 1);
        entities.InterstateRequest.OtherStateFips = db.GetInt32(reader, 2);
        entities.InterstateRequest.OtherStateCaseStatus =
          db.GetString(reader, 3);
        entities.InterstateRequest.KsCaseInd = db.GetString(reader, 4);
        entities.InterstateRequest.CasINumber = db.GetNullableString(reader, 5);
        entities.InterstateRequest.CasNumber = db.GetNullableString(reader, 6);
        entities.InterstateRequest.CspNumber = db.GetNullableString(reader, 7);
        entities.InterstateRequest.CroType = db.GetNullableString(reader, 8);
        entities.InterstateRequest.CroId = db.GetNullableInt32(reader, 9);
        entities.InterstateRequest.Populated = true;
        CheckValid<InterstateRequest>("CroType",
          entities.InterstateRequest.CroType);
      });
  }

  private IEnumerable<bool> ReadInterstateRequest3()
  {
    entities.InterstateRequest.Populated = false;

    return ReadEach("ReadInterstateRequest3",
      (db, command) =>
      {
        db.SetNullableString(command, "casINumber", import.Case1.Number);
      },
      (db, reader) =>
      {
        entities.InterstateRequest.IntHGeneratedId = db.GetInt32(reader, 0);
        entities.InterstateRequest.OtherStateCaseId =
          db.GetNullableString(reader, 1);
        entities.InterstateRequest.OtherStateFips = db.GetInt32(reader, 2);
        entities.InterstateRequest.OtherStateCaseStatus =
          db.GetString(reader, 3);
        entities.InterstateRequest.KsCaseInd = db.GetString(reader, 4);
        entities.InterstateRequest.CasINumber = db.GetNullableString(reader, 5);
        entities.InterstateRequest.CasNumber = db.GetNullableString(reader, 6);
        entities.InterstateRequest.CspNumber = db.GetNullableString(reader, 7);
        entities.InterstateRequest.CroType = db.GetNullableString(reader, 8);
        entities.InterstateRequest.CroId = db.GetNullableInt32(reader, 9);
        entities.InterstateRequest.Populated = true;
        CheckValid<InterstateRequest>("CroType",
          entities.InterstateRequest.CroType);

        return true;
      });
  }

  private IEnumerable<bool> ReadInterstateRequestCase1()
  {
    entities.Case1.Populated = false;
    entities.InterstateRequest.Populated = false;

    return ReadEach("ReadInterstateRequestCase1",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "startDate", local.Current.Date.GetValueOrDefault());
        db.SetString(command, "cspNumber", local.CsePerson.Number);
      },
      (db, reader) =>
      {
        entities.InterstateRequest.IntHGeneratedId = db.GetInt32(reader, 0);
        entities.InterstateRequest.OtherStateCaseId =
          db.GetNullableString(reader, 1);
        entities.InterstateRequest.OtherStateFips = db.GetInt32(reader, 2);
        entities.InterstateRequest.OtherStateCaseStatus =
          db.GetString(reader, 3);
        entities.InterstateRequest.KsCaseInd = db.GetString(reader, 4);
        entities.InterstateRequest.CasINumber = db.GetNullableString(reader, 5);
        entities.Case1.Number = db.GetString(reader, 5);
        entities.InterstateRequest.CasNumber = db.GetNullableString(reader, 6);
        entities.InterstateRequest.CspNumber = db.GetNullableString(reader, 7);
        entities.InterstateRequest.CroType = db.GetNullableString(reader, 8);
        entities.InterstateRequest.CroId = db.GetNullableInt32(reader, 9);
        entities.Case1.Populated = true;
        entities.InterstateRequest.Populated = true;
        CheckValid<InterstateRequest>("CroType",
          entities.InterstateRequest.CroType);

        return true;
      });
  }

  private IEnumerable<bool> ReadInterstateRequestCase2()
  {
    entities.Case1.Populated = false;
    entities.InterstateRequest.Populated = false;

    return ReadEach("ReadInterstateRequestCase2",
      (db, command) =>
      {
        db.SetNullableString(command, "casINumber", import.Case1.Number);
        db.SetNullableString(
          command, "cspNumber", import.GeneticTestInformation.FatherPersonNo);
      },
      (db, reader) =>
      {
        entities.InterstateRequest.IntHGeneratedId = db.GetInt32(reader, 0);
        entities.InterstateRequest.OtherStateCaseId =
          db.GetNullableString(reader, 1);
        entities.InterstateRequest.OtherStateFips = db.GetInt32(reader, 2);
        entities.InterstateRequest.OtherStateCaseStatus =
          db.GetString(reader, 3);
        entities.InterstateRequest.KsCaseInd = db.GetString(reader, 4);
        entities.InterstateRequest.CasINumber = db.GetNullableString(reader, 5);
        entities.Case1.Number = db.GetString(reader, 5);
        entities.InterstateRequest.CasNumber = db.GetNullableString(reader, 6);
        entities.InterstateRequest.CspNumber = db.GetNullableString(reader, 7);
        entities.InterstateRequest.CroType = db.GetNullableString(reader, 8);
        entities.InterstateRequest.CroId = db.GetNullableInt32(reader, 9);
        entities.Case1.Populated = true;
        entities.InterstateRequest.Populated = true;
        CheckValid<InterstateRequest>("CroType",
          entities.InterstateRequest.CroType);

        return true;
      });
  }

  private IEnumerable<bool> ReadInterstateRequestCase3()
  {
    System.Diagnostics.Debug.Assert(entities.CaseRole.Populated);
    entities.Case1.Populated = false;
    entities.InterstateRequest.Populated = false;

    return ReadEach("ReadInterstateRequestCase3",
      (db, command) =>
      {
        db.SetNullableInt32(command, "croId", entities.CaseRole.Identifier);
        db.SetNullableString(command, "croType", entities.CaseRole.Type1);
        db.SetNullableString(command, "cspNumber", entities.CaseRole.CspNumber);
        db.SetNullableString(command, "casNumber", entities.CaseRole.CasNumber);
      },
      (db, reader) =>
      {
        entities.InterstateRequest.IntHGeneratedId = db.GetInt32(reader, 0);
        entities.InterstateRequest.OtherStateCaseId =
          db.GetNullableString(reader, 1);
        entities.InterstateRequest.OtherStateFips = db.GetInt32(reader, 2);
        entities.InterstateRequest.OtherStateCaseStatus =
          db.GetString(reader, 3);
        entities.InterstateRequest.KsCaseInd = db.GetString(reader, 4);
        entities.InterstateRequest.CasINumber = db.GetNullableString(reader, 5);
        entities.Case1.Number = db.GetString(reader, 5);
        entities.InterstateRequest.CasNumber = db.GetNullableString(reader, 6);
        entities.InterstateRequest.CspNumber = db.GetNullableString(reader, 7);
        entities.InterstateRequest.CroType = db.GetNullableString(reader, 8);
        entities.InterstateRequest.CroId = db.GetNullableInt32(reader, 9);
        entities.Case1.Populated = true;
        entities.InterstateRequest.Populated = true;
        CheckValid<InterstateRequest>("CroType",
          entities.InterstateRequest.CroType);

        return true;
      });
  }

  private IEnumerable<bool> ReadInterstateRequestHistory()
  {
    entities.InterstateRequestHistory.Populated = false;

    return ReadEach("ReadInterstateRequestHistory",
      (db, command) =>
      {
        db.SetInt32(
          command, "intGeneratedId",
          entities.InterstateRequest.IntHGeneratedId);
        db.SetDate(
          command, "transactionDate", local.Current.Date.GetValueOrDefault());
        db.SetNullableString(
          command, "actionReasonCode",
          local.Transactions.Item.GlocalTxnInterstateRequestHistory.
            ActionReasonCode ?? "");
      },
      (db, reader) =>
      {
        entities.InterstateRequestHistory.IntGeneratedId =
          db.GetInt32(reader, 0);
        entities.InterstateRequestHistory.CreatedTimestamp =
          db.GetDateTime(reader, 1);
        entities.InterstateRequestHistory.TransactionSerialNum =
          db.GetInt64(reader, 2);
        entities.InterstateRequestHistory.TransactionDate =
          db.GetDate(reader, 3);
        entities.InterstateRequestHistory.ActionReasonCode =
          db.GetNullableString(reader, 4);
        entities.InterstateRequestHistory.Populated = true;

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
    /// A value of Specific.
    /// </summary>
    [JsonPropertyName("specific")]
    public InterstateRequestHistory Specific
    {
      get => specific ??= new();
      set => specific = value;
    }

    /// <summary>
    /// A value of Zdel.
    /// </summary>
    [JsonPropertyName("zdel")]
    public CsePerson Zdel
    {
      get => zdel ??= new();
      set => zdel = value;
    }

    /// <summary>
    /// A value of InterstateRequest.
    /// </summary>
    [JsonPropertyName("interstateRequest")]
    public InterstateRequest InterstateRequest
    {
      get => interstateRequest ??= new();
      set => interstateRequest = value;
    }

    /// <summary>
    /// A value of GeneticTestInformation.
    /// </summary>
    [JsonPropertyName("geneticTestInformation")]
    public GeneticTestInformation GeneticTestInformation
    {
      get => geneticTestInformation ??= new();
      set => geneticTestInformation = value;
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
    /// A value of Case1.
    /// </summary>
    [JsonPropertyName("case1")]
    public Case1 Case1
    {
      get => case1 ??= new();
      set => case1 = value;
    }

    /// <summary>
    /// A value of Misc.
    /// </summary>
    [JsonPropertyName("misc")]
    public DateWorkArea Misc
    {
      get => misc ??= new();
      set => misc = value;
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

    private Common screenIdentification;
    private InterstateRequestHistory specific;
    private CsePerson zdel;
    private InterstateRequest interstateRequest;
    private GeneticTestInformation geneticTestInformation;
    private LegalAction legalAction;
    private Case1 case1;
    private DateWorkArea misc;
    private CsePerson csePerson;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of InterstateCase.
    /// </summary>
    [JsonPropertyName("interstateCase")]
    public InterstateCase InterstateCase
    {
      get => interstateCase ??= new();
      set => interstateCase = value;
    }

    private InterstateCase interstateCase;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>A ChildrenGroup group.</summary>
    [Serializable]
    public class ChildrenGroup
    {
      /// <summary>
      /// A value of GlocalChild.
      /// </summary>
      [JsonPropertyName("glocalChild")]
      public CsePerson GlocalChild
      {
        get => glocalChild ??= new();
        set => glocalChild = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 8;

      private CsePerson glocalChild;
    }

    /// <summary>A TransactionsGroup group.</summary>
    [Serializable]
    public class TransactionsGroup
    {
      /// <summary>
      /// A value of GlocalTxnInterstateRequest.
      /// </summary>
      [JsonPropertyName("glocalTxnInterstateRequest")]
      public InterstateRequest GlocalTxnInterstateRequest
      {
        get => glocalTxnInterstateRequest ??= new();
        set => glocalTxnInterstateRequest = value;
      }

      /// <summary>
      /// A value of GlocalTxnCase.
      /// </summary>
      [JsonPropertyName("glocalTxnCase")]
      public Case1 GlocalTxnCase
      {
        get => glocalTxnCase ??= new();
        set => glocalTxnCase = value;
      }

      /// <summary>
      /// A value of GlocalTxnInterstateRequestHistory.
      /// </summary>
      [JsonPropertyName("glocalTxnInterstateRequestHistory")]
      public InterstateRequestHistory GlocalTxnInterstateRequestHistory
      {
        get => glocalTxnInterstateRequestHistory ??= new();
        set => glocalTxnInterstateRequestHistory = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 50;

      private InterstateRequest glocalTxnInterstateRequest;
      private Case1 glocalTxnCase;
      private InterstateRequestHistory glocalTxnInterstateRequestHistory;
    }

    /// <summary>A ParticipantsGroup group.</summary>
    [Serializable]
    public class ParticipantsGroup
    {
      /// <summary>
      /// A value of GlocalParticipant.
      /// </summary>
      [JsonPropertyName("glocalParticipant")]
      public CsePerson GlocalParticipant
      {
        get => glocalParticipant ??= new();
        set => glocalParticipant = value;
      }

      /// <summary>
      /// A value of G.
      /// </summary>
      [JsonPropertyName("g")]
      public InterstateParticipant G
      {
        get => g ??= new();
        set => g = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 9;

      private CsePerson glocalParticipant;
      private InterstateParticipant g;
    }

    /// <summary>A OrdersGroup group.</summary>
    [Serializable]
    public class OrdersGroup
    {
      /// <summary>
      /// A value of G.
      /// </summary>
      [JsonPropertyName("g")]
      public InterstateSupportOrder G
      {
        get => g ??= new();
        set => g = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 9;

      private InterstateSupportOrder g;
    }

    /// <summary>A LegalActionsGroup group.</summary>
    [Serializable]
    public class LegalActionsGroup
    {
      /// <summary>
      /// A value of GlocalLegalAction.
      /// </summary>
      [JsonPropertyName("glocalLegalAction")]
      public LegalAction GlocalLegalAction
      {
        get => glocalLegalAction ??= new();
        set => glocalLegalAction = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 9;

      private LegalAction glocalLegalAction;
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
    /// Gets a value of Children.
    /// </summary>
    [JsonIgnore]
    public Array<ChildrenGroup> Children => children ??= new(
      ChildrenGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Children for json serialization.
    /// </summary>
    [JsonPropertyName("children")]
    [Computed]
    public IList<ChildrenGroup> Children_Json
    {
      get => children;
      set => Children.Assign(value);
    }

    /// <summary>
    /// A value of Screen.
    /// </summary>
    [JsonPropertyName("screen")]
    public Common Screen
    {
      get => screen ??= new();
      set => screen = value;
    }

    /// <summary>
    /// A value of ScreenAction.
    /// </summary>
    [JsonPropertyName("screenAction")]
    public Common ScreenAction
    {
      get => screenAction ??= new();
      set => screenAction = value;
    }

    /// <summary>
    /// A value of Specific.
    /// </summary>
    [JsonPropertyName("specific")]
    public InterstateRequestHistory Specific
    {
      get => specific ??= new();
      set => specific = value;
    }

    /// <summary>
    /// A value of Batch.
    /// </summary>
    [JsonPropertyName("batch")]
    public Common Batch
    {
      get => batch ??= new();
      set => batch = value;
    }

    /// <summary>
    /// A value of PrimaryAp.
    /// </summary>
    [JsonPropertyName("primaryAp")]
    public CsePersonsWorkSet PrimaryAp
    {
      get => primaryAp ??= new();
      set => primaryAp = value;
    }

    /// <summary>
    /// A value of FamilyViolence.
    /// </summary>
    [JsonPropertyName("familyViolence")]
    public Common FamilyViolence
    {
      get => familyViolence ??= new();
      set => familyViolence = value;
    }

    /// <summary>
    /// A value of CsenetStateTable.
    /// </summary>
    [JsonPropertyName("csenetStateTable")]
    public CsenetStateTable CsenetStateTable
    {
      get => csenetStateTable ??= new();
      set => csenetStateTable = value;
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
    /// A value of DateWorkArea.
    /// </summary>
    [JsonPropertyName("dateWorkArea")]
    public DateWorkArea DateWorkArea
    {
      get => dateWorkArea ??= new();
      set => dateWorkArea = value;
    }

    /// <summary>
    /// A value of FieldValue.
    /// </summary>
    [JsonPropertyName("fieldValue")]
    public FieldValue FieldValue
    {
      get => fieldValue ??= new();
      set => fieldValue = value;
    }

    /// <summary>
    /// Gets a value of Transactions.
    /// </summary>
    [JsonIgnore]
    public Array<TransactionsGroup> Transactions => transactions ??= new(
      TransactionsGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Transactions for json serialization.
    /// </summary>
    [JsonPropertyName("transactions")]
    [Computed]
    public IList<TransactionsGroup> Transactions_Json
    {
      get => transactions;
      set => Transactions.Assign(value);
    }

    /// <summary>
    /// A value of InterstateCase.
    /// </summary>
    [JsonPropertyName("interstateCase")]
    public InterstateCase InterstateCase
    {
      get => interstateCase ??= new();
      set => interstateCase = value;
    }

    /// <summary>
    /// A value of InterstateApIdentification.
    /// </summary>
    [JsonPropertyName("interstateApIdentification")]
    public InterstateApIdentification InterstateApIdentification
    {
      get => interstateApIdentification ??= new();
      set => interstateApIdentification = value;
    }

    /// <summary>
    /// A value of InterstateApLocate.
    /// </summary>
    [JsonPropertyName("interstateApLocate")]
    public InterstateApLocate InterstateApLocate
    {
      get => interstateApLocate ??= new();
      set => interstateApLocate = value;
    }

    /// <summary>
    /// Gets a value of Participants.
    /// </summary>
    [JsonIgnore]
    public Array<ParticipantsGroup> Participants => participants ??= new(
      ParticipantsGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Participants for json serialization.
    /// </summary>
    [JsonPropertyName("participants")]
    [Computed]
    public IList<ParticipantsGroup> Participants_Json
    {
      get => participants;
      set => Participants.Assign(value);
    }

    /// <summary>
    /// Gets a value of Orders.
    /// </summary>
    [JsonIgnore]
    public Array<OrdersGroup> Orders => orders ??= new(OrdersGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Orders for json serialization.
    /// </summary>
    [JsonPropertyName("orders")]
    [Computed]
    public IList<OrdersGroup> Orders_Json
    {
      get => orders;
      set => Orders.Assign(value);
    }

    /// <summary>
    /// A value of InterstateMiscellaneous.
    /// </summary>
    [JsonPropertyName("interstateMiscellaneous")]
    public InterstateMiscellaneous InterstateMiscellaneous
    {
      get => interstateMiscellaneous ??= new();
      set => interstateMiscellaneous = value;
    }

    /// <summary>
    /// A value of InterstateRequestHistory.
    /// </summary>
    [JsonPropertyName("interstateRequestHistory")]
    public InterstateRequestHistory InterstateRequestHistory
    {
      get => interstateRequestHistory ??= new();
      set => interstateRequestHistory = value;
    }

    /// <summary>
    /// Gets a value of LegalActions.
    /// </summary>
    [JsonIgnore]
    public Array<LegalActionsGroup> LegalActions => legalActions ??= new(
      LegalActionsGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of LegalActions for json serialization.
    /// </summary>
    [JsonPropertyName("legalActions")]
    [Computed]
    public IList<LegalActionsGroup> LegalActions_Json
    {
      get => legalActions;
      set => LegalActions.Assign(value);
    }

    /// <summary>
    /// A value of NullInterstateRequestHistory.
    /// </summary>
    [JsonPropertyName("nullInterstateRequestHistory")]
    public InterstateRequestHistory NullInterstateRequestHistory
    {
      get => nullInterstateRequestHistory ??= new();
      set => nullInterstateRequestHistory = value;
    }

    /// <summary>
    /// A value of NullInterstateMiscellaneous.
    /// </summary>
    [JsonPropertyName("nullInterstateMiscellaneous")]
    public InterstateMiscellaneous NullInterstateMiscellaneous
    {
      get => nullInterstateMiscellaneous ??= new();
      set => nullInterstateMiscellaneous = value;
    }

    /// <summary>
    /// A value of NullInterstateSupportOrder.
    /// </summary>
    [JsonPropertyName("nullInterstateSupportOrder")]
    public InterstateSupportOrder NullInterstateSupportOrder
    {
      get => nullInterstateSupportOrder ??= new();
      set => nullInterstateSupportOrder = value;
    }

    /// <summary>
    /// A value of NullInterstateApLocate.
    /// </summary>
    [JsonPropertyName("nullInterstateApLocate")]
    public InterstateApLocate NullInterstateApLocate
    {
      get => nullInterstateApLocate ??= new();
      set => nullInterstateApLocate = value;
    }

    /// <summary>
    /// A value of NullInterstateCase.
    /// </summary>
    [JsonPropertyName("nullInterstateCase")]
    public InterstateCase NullInterstateCase
    {
      get => nullInterstateCase ??= new();
      set => nullInterstateCase = value;
    }

    /// <summary>
    /// A value of NullInterstateApIdentification.
    /// </summary>
    [JsonPropertyName("nullInterstateApIdentification")]
    public InterstateApIdentification NullInterstateApIdentification
    {
      get => nullInterstateApIdentification ??= new();
      set => nullInterstateApIdentification = value;
    }

    /// <summary>
    /// A value of NullInterstateParticipant.
    /// </summary>
    [JsonPropertyName("nullInterstateParticipant")]
    public InterstateParticipant NullInterstateParticipant
    {
      get => nullInterstateParticipant ??= new();
      set => nullInterstateParticipant = value;
    }

    private CsePerson csePerson;
    private Array<ChildrenGroup> children;
    private Common screen;
    private Common screenAction;
    private InterstateRequestHistory specific;
    private Common batch;
    private CsePersonsWorkSet primaryAp;
    private Common familyViolence;
    private CsenetStateTable csenetStateTable;
    private DateWorkArea current;
    private DateWorkArea dateWorkArea;
    private FieldValue fieldValue;
    private Array<TransactionsGroup> transactions;
    private InterstateCase interstateCase;
    private InterstateApIdentification interstateApIdentification;
    private InterstateApLocate interstateApLocate;
    private Array<ParticipantsGroup> participants;
    private Array<OrdersGroup> orders;
    private InterstateMiscellaneous interstateMiscellaneous;
    private InterstateRequestHistory interstateRequestHistory;
    private Array<LegalActionsGroup> legalActions;
    private InterstateRequestHistory nullInterstateRequestHistory;
    private InterstateMiscellaneous nullInterstateMiscellaneous;
    private InterstateSupportOrder nullInterstateSupportOrder;
    private InterstateApLocate nullInterstateApLocate;
    private InterstateCase nullInterstateCase;
    private InterstateApIdentification nullInterstateApIdentification;
    private InterstateParticipant nullInterstateParticipant;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
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
    /// A value of Case1.
    /// </summary>
    [JsonPropertyName("case1")]
    public Case1 Case1
    {
      get => case1 ??= new();
      set => case1 = value;
    }

    /// <summary>
    /// A value of CsenetTransactionEnvelop.
    /// </summary>
    [JsonPropertyName("csenetTransactionEnvelop")]
    public CsenetTransactionEnvelop CsenetTransactionEnvelop
    {
      get => csenetTransactionEnvelop ??= new();
      set => csenetTransactionEnvelop = value;
    }

    /// <summary>
    /// A value of InterstateCase.
    /// </summary>
    [JsonPropertyName("interstateCase")]
    public InterstateCase InterstateCase
    {
      get => interstateCase ??= new();
      set => interstateCase = value;
    }

    /// <summary>
    /// A value of InterstateRequestHistory.
    /// </summary>
    [JsonPropertyName("interstateRequestHistory")]
    public InterstateRequestHistory InterstateRequestHistory
    {
      get => interstateRequestHistory ??= new();
      set => interstateRequestHistory = value;
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
    /// A value of CaseRole.
    /// </summary>
    [JsonPropertyName("caseRole")]
    public CaseRole CaseRole
    {
      get => caseRole ??= new();
      set => caseRole = value;
    }

    /// <summary>
    /// A value of InterstateRequest.
    /// </summary>
    [JsonPropertyName("interstateRequest")]
    public InterstateRequest InterstateRequest
    {
      get => interstateRequest ??= new();
      set => interstateRequest = value;
    }

    /// <summary>
    /// A value of Fips.
    /// </summary>
    [JsonPropertyName("fips")]
    public Fips Fips
    {
      get => fips ??= new();
      set => fips = value;
    }

    /// <summary>
    /// A value of LegalActionCaseRole.
    /// </summary>
    [JsonPropertyName("legalActionCaseRole")]
    public LegalActionCaseRole LegalActionCaseRole
    {
      get => legalActionCaseRole ??= new();
      set => legalActionCaseRole = value;
    }

    /// <summary>
    /// A value of LaPersonLaCaseRole.
    /// </summary>
    [JsonPropertyName("laPersonLaCaseRole")]
    public LaPersonLaCaseRole LaPersonLaCaseRole
    {
      get => laPersonLaCaseRole ??= new();
      set => laPersonLaCaseRole = value;
    }

    /// <summary>
    /// A value of LegalActionPerson.
    /// </summary>
    [JsonPropertyName("legalActionPerson")]
    public LegalActionPerson LegalActionPerson
    {
      get => legalActionPerson ??= new();
      set => legalActionPerson = value;
    }

    private LegalAction legalAction;
    private Case1 case1;
    private CsenetTransactionEnvelop csenetTransactionEnvelop;
    private InterstateCase interstateCase;
    private InterstateRequestHistory interstateRequestHistory;
    private CsePerson csePerson;
    private CaseRole caseRole;
    private InterstateRequest interstateRequest;
    private Fips fips;
    private LegalActionCaseRole legalActionCaseRole;
    private LaPersonLaCaseRole laPersonLaCaseRole;
    private LegalActionPerson legalActionPerson;
  }
#endregion
}
