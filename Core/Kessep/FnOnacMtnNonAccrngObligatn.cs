// Program: FN_ONAC_MTN_NON_ACCRNG_OBLIGATN, ID: 372094941, model: 746.
// Short name: SWEONACP
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
/// A program: FN_ONAC_MTN_NON_ACCRNG_OBLIGATN.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class FnOnacMtnNonAccrngObligatn: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_ONAC_MTN_NON_ACCRNG_OBLIGATN program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnOnacMtnNonAccrngObligatn(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnOnacMtnNonAccrngObligatn.
  /// </summary>
  public FnOnacMtnNonAccrngObligatn(IContext context, Import import,
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
    // *************************************************************
    //  DATE	  PROGRAMMER		DESCRIPTION
    // 10/15/96  Holly Kennedy-MTW	Retrofitted Matt's security and data level
    //                                 
    // security.  added the Primary/
    // Secondary
    //                                 
    // indicator  to the screen.
    // 01/13/97  HOOKS		        Raise events
    // 01/23/97  HOOKS		        Add logic for HIST/MONA automatic NEXTTRAN.
    // 05/01/97  Sumanta - MTW         Changed the DB2 current_date to IEF 
    // current
    //                                 
    // date in the reads.  Added logic
    // for
    //                                 
    // Alternate billing address Added
    //                                 
    // designated payee and flow to
    // DPAY
    // 05/13/97  Sumanta - MTW		Added flow to ASIN and related logic. Check
    //                                 
    // Obligation Type edits to insure
    // that all
    //                                 
    // non accruing types are covered.
    // 09/19/97 Alan Samuels		Problem Report 26361
    // 12/29/97 Venkatesh Kamaraj      Changed to set situation # to 0
    //                                 
    // instead of calling
    // get_next_situation_no
    //                                 
    // because of changes to
    // infrastucture
    // 03/26/98 Siraj Konkader         ZDEL cleanup
    //  8/26/98 Bud Adams              Added 'select-character' to Group_Export
    //                                 
    // so group view match would work.
    //  9-25-98   B Adams              Deleted assigned-to from screen;
    //                                 
    // deleted logic  that finds out
    // who is
    //                                 
    // the Responsible Person.
    // ************************************************************************
    // =================================================
    // 10-15-98  bud adams  -  removed all 'designated payee' code.  The dialog 
    // flow to and from DPAY still needs to be deleted.
    // 12-18-98  B Adams  -  Legal decided they don't care about interstate 
    // stuff and so are not going to enter that information.  It has to be
    // entered here, now.
    // 3/31/99 - B Adams  -  Program code associated with supported
    //   persons removed from screen and code.  Brings no value
    //   to this screen and is only confusing as to what it represents.
    //   This evening is a Blue Moon
    // =================================================
    // ================================================
    // 1/3/00 - Kent Price  -  Add warning message If obligation
    // exist but are retired.
    // ================================================
    // =================================================
    // 3/1/00  K. Price - process country or state code for ADD
    // and UPDATE
    // =================================================
    // =================================================
    // Oct, 2000 M. Brown, pr# 106234 - NEXT TRAN updates.
    // =================================================
    // ----------------------------------------------------------------------------------
    // 01/09/2001                   Vithal Madhira               WR# 000257
    // Unprotect the 'Program' code field on ONAC (allow for workers to enter AF
    // or NA only) if field is blank. Field should remain protected if 'AF' or
    // 'NA' code is already populated. The Prog field is mandatory for
    // obligation type 'MJ'. Prog field should be protected after adding the
    // obligation. User can not update the fields.
    // -----------------------------------------------------------------------------------
    // ----------------------------------------------------------------------------------
    // 12/14/2000                    Vithal Madhira               WR# 000253
    // 1. Display child's DOB on financial screens ( ONAC, OACC).
    // 2. An automatic flow to PEPR when adding 'OACC & ONAC'.
    // (Not exactly automatic, user needs to press PF22 to flow to PEPR after 
    // adding the record on the screen. A message will be displayed after
    // successful addition.
    // -----------------------------------------------------------------------------------
    // 02/28/2001            Vithal Madhira                  PR# 114682
    // Obligation type '718B' can not be added/updated  if case is interstate.
    // -------------------------------------------------------------------------------------
    // =================================================
    // Jan, 2002, M. Brown, PR# 128555
    // Message that incorrect end date was entered, but end date is protected, 
    // so user cannot enter a correct date.
    // Jan, 2002, M. Brown, WO# 020144
    // Make Debt Detail pre-conversion code updateable.  This was changed to be 
    // 3 digits,
    // in order to allow the user to enter the interstate program codes.
    // =================================================
    // =================================================================================
    // 06/22/2006               GVandy              WR# 230751
    // Add capability to select tribal interstate request.
    // ===================================================================================
    ExitState = "ACO_NN0000_ALL_OK";

    // ***** MOVE IMPORTS TO EXPORTS *****
    export.ObCollProtAct.Flag = import.ObCollProtAct.Flag;
    export.HiddenNextTranInfo.Assign(import.HiddenNextTranInfo);
    export.Case1.Number = import.Case1.Number;
    export.AltAddrPrompt.SelectChar = import.AltAddrPrompt.SelectChar;
    export.HiddenInterstateRequest.Assign(import.HiddenInterstateRequest);
    export.Standard.NextTransaction = import.Standard.NextTransaction;
    MoveLegalAction1(import.LegalAction, export.LegalAction);
    MoveLegalActionDetail(import.LegalActionDetail, export.LegalActionDetail);
    export.ObligationPaymentSchedule.Assign(import.ObligationPaymentSchedule);
    export.ObligationAmt.TotalCurrency = import.ObligationAmt.TotalCurrency;
    export.ObligationType.Assign(import.ObligationType);
    export.ObligorCsePerson.Number = import.ObligorCsePerson.Number;
    export.HiddenObligor.Number = import.HiddenObligor.Number;
    MoveCsePersonsWorkSet3(import.ObligorCsePersonsWorkSet,
      export.ObligorCsePersonsWorkSet);
    export.ConcurrentObligation.SystemGeneratedIdentifier =
      import.ConcurrentObligation.SystemGeneratedIdentifier;
    export.HiddenConcurrentObligationType.SystemGeneratedIdentifier =
      import.HiddenConcurrentObligationType.SystemGeneratedIdentifier;
    export.BalanceOwed.TotalCurrency = import.BalanceOwed.TotalCurrency;
    export.InterestOwed.TotalCurrency = import.InterestOwed.TotalCurrency;
    export.ConcurrentCsePerson.Number = import.ConcurrentCsePerson.Number;
    export.ConcurrentCsePersonsWorkSet.FormattedName =
      import.ConcurrentCsePersonsWorkSet.FormattedName;
    export.PaymentScheduleInd.Flag = import.PaymentScheduleInd.Flag;
    export.SuspendInterestInd.Flag = import.SuspendInterestInd.Flag;
    export.TotalOwed.TotalCurrency = import.TotalOwed.TotalCurrency;
    export.Previous.DueDt = import.Previous.DueDt;
    export.HiddenDisplayed.Assign(import.HiddenDisplayed);
    export.DisplayedObligCreateDt.Date = import.DisplayedObligCreateDt.Date;
    export.ManualDistributionInd.Flag = import.ManualDistributionInd.Flag;
    export.Code.CodeName = import.Code.CodeName;
    export.Header.Assign(import.Header);

    export.Export1.Index = 0;
    export.Export1.Clear();

    for(import.Import1.Index = 0; import.Import1.Index < import.Import1.Count; ++
      import.Import1.Index)
    {
      if (export.Export1.IsFull)
      {
        break;
      }

      export.Export1.Update.Common.SelectChar =
        import.Import1.Item.Common.SelectChar;
      export.Export1.Update.SupportedCsePerson.Number =
        import.Import1.Item.SupportedCsePerson.Number;
      export.Export1.Update.SupportedCsePersonsWorkSet.Assign(
        import.Import1.Item.SupportedCsePersonsWorkSet);
      export.Export1.Update.ObligationTransaction.Assign(
        import.Import1.Item.ObligationTransaction);
      export.Export1.Update.Concurrent.SystemGeneratedIdentifier =
        import.Import1.Item.Concurrent.SystemGeneratedIdentifier;
      export.Export1.Update.ObligationPaymentSchedule.EndDt =
        import.Import1.Item.ObligationPaymentSchedule.EndDt;
      MoveServiceProvider(import.Import1.Item.ServiceProvider,
        export.Export1.Update.ServiceProvider);
      export.Export1.Update.Case1.Number = import.Import1.Item.Case1.Number;
      export.Export1.Update.DebtDetail.Assign(import.Import1.Item.DebtDetail);
      export.Export1.Update.Hidden.PreconversionProgramCode =
        import.Import1.Item.Hidden.PreconversionProgramCode;
      export.Export1.Update.Prev.Amount = import.Import1.Item.Prev.Amount;
      export.Export1.Update.DebtDetail.DueDt = export.Header.DueDt;
      export.Export1.Update.DebtDetail.CoveredPrdStartDt =
        import.Header.CoveredPrdStartDt;
      export.Export1.Update.DebtDetail.CoveredPrdEndDt =
        import.Header.CoveredPrdEndDt;
      export.Export1.Next();
    }

    MoveCodeValue(import.Country, export.Country);
    export.InterstateRequest.Assign(import.InterstateRequest);

    if (IsEmpty(export.InterstateRequest.Country) && IsEmpty
      (export.InterstateRequest.TribalAgency) && !
      IsEmpty(export.Country.Cdvalue))
    {
      export.InterstateRequest.Country =
        Substring(export.Country.Cdvalue, 1, 2);
    }

    export.AltAddress.Assign(import.AltAddress);
    export.HiddenAltAddress.Assign(import.HiddenAltAddress);
    MoveObligation3(import.Obligation, export.Obligation);
    MoveObligation12(import.Obligation, local.Obligation);
    export.ObligationActive.Flag = import.ObligationActive.Flag;
    MoveDebtDetail4(import.HiddenStoredDebtDetail, export.HiddenStoredDebtDetail);
      
    export.HiddenStoredObligation.Assign(import.HiddenStoredObligation);
    export.HiddenFlowToPeprCase.Number = import.HiddenFlowToPeprCase.Number;
    export.HiddenFlowToPeprCsePersonsWorkSet.Number =
      import.HiddenFlowToPeprCsePersonsWorkSet.Number;

    if (IsEmpty(export.ObCollProtAct.Flag))
    {
      export.ObCollProtAct.Flag = "N";
    }

    // M Brown, April 2002, Retro processing.
    // These fields are used in logic to have the user enter Y or N when an 
    // obligation discontinue date is updated or when a new obligation is added,
    // and AF collections exist on the court order.
    export.ProtectQuestionLiteral.Text80 = "";
    export.CollProtAnswer.SelectChar = "";

    var field = GetField(export.CollProtAnswer, "selectChar");

    field.Intensity = Intensity.Dark;
    field.Protected = true;

    if (Equal(global.Command, "CLEAR"))
    {
      if (!Equal(export.AltAddress.Number, export.HiddenAltAddress.Number))
      {
        MoveCsePersonsWorkSet5(import.HiddenAltAddress, export.AltAddress);
      }

      global.Command = "DISPLAY";
    }
    else
    {
      export.AltAddress.Assign(import.AltAddress);
    }

    local.Current.Date = Now().Date;
    local.Current.Timestamp = Now();
    UseCabSetMaximumDiscontinueDate1();

    // =================================================
    // 10/14/98 - b adams  -  This was added as a quick fix, since checking this
    // in and then trying to pick up REIP in modify seemed unlikely at the
    // time.  Return from REIP sets command to SPACES.
    // =================================================
    if (Equal(import.ReturnFlowFrom.Text4, "REIP"))
    {
      global.Command = "DISPLAY";
    }

    if (Equal(global.Command, "FROMLDET"))
    {
      export.ReturnFlowFrom.Text4 = "LDET";
    }

    // ***** HARDCODE AREA *****
    UseFnHardcodedDebtDistribution();
    UseFnHardcodeLegal();

    if (AsChar(import.ObligationActive.Flag) == 'Y')
    {
      local.ProtectAll.Flag = "Y";
    }
    else
    {
      local.ProtectAll.Flag = "N";
    }

    local.Max.Date = UseCabSetMaximumDiscontinueDate2();

    if (AsChar(import.Obligation.OrderTypeCode) != 'I')
    {
      export.Obligation.OrderTypeCode = "";
    }

    if (Equal(global.Command, "ADD"))
    {
      export.DisplayedObligCreateDt.Date = local.Current.Date;
    }

    // -----------------------------------------------------------------------------------
    // The following fields need to be protected if they are read from the 
    // database.  First time when we display/add screen the values are moved to
    // hidden_stored views. So check the hidden_stored views and protect them .
    // ---------------------------------------------------------------------------------
    if (!IsEmpty(export.HiddenStoredObligation.OrderTypeCode))
    {
      var field1 = GetField(export.Obligation, "orderTypeCode");

      field1.Color = "cyan";
      field1.Protected = true;
    }

    if (!IsEmpty(export.HiddenStoredObligation.OtherStateAbbr))
    {
      var field1 = GetField(export.Obligation, "otherStateAbbr");

      field1.Color = "cyan";
      field1.Protected = true;

      var field2 = GetField(export.InterstateRequest, "country");

      field2.Color = "cyan";
      field2.Protected = true;

      var field3 = GetField(export.InterstateRequest, "tribalAgency");

      field3.Color = "cyan";
      field3.Protected = true;
    }

    if (!IsEmpty(export.HiddenInterstateRequest.OtherStateCaseId))
    {
      var field1 = GetField(export.InterstateRequest, "otherStateCaseId");

      field1.Color = "cyan";
      field1.Protected = true;
    }

    if (!IsEmpty(export.HiddenInterstateRequest.Country))
    {
      var field1 = GetField(export.InterstateRequest, "country");

      field1.Color = "cyan";
      field1.Protected = true;

      var field2 = GetField(export.InterstateRequest, "tribalAgency");

      field2.Color = "cyan";
      field2.Protected = true;

      var field3 = GetField(export.Obligation, "otherStateAbbr");

      field3.Color = "cyan";
      field3.Protected = true;
    }

    if (!IsEmpty(export.HiddenInterstateRequest.TribalAgency))
    {
      var field1 = GetField(export.InterstateRequest, "country");

      field1.Color = "cyan";
      field1.Protected = true;

      var field2 = GetField(export.InterstateRequest, "tribalAgency");

      field2.Color = "cyan";
      field2.Protected = true;

      var field3 = GetField(export.Obligation, "otherStateAbbr");

      field3.Color = "cyan";
      field3.Protected = true;
    }

    if (!Equal(export.HiddenStoredDebtDetail.CoveredPrdStartDt,
      local.BlankDateWorkArea.Date) && !
      Equal(export.HiddenStoredDebtDetail.CoveredPrdEndDt,
      local.BlankDateWorkArea.Date))
    {
      var field1 = GetField(export.Header, "coveredPrdStartDt");

      field1.Color = "cyan";
      field1.Protected = true;

      var field2 = GetField(export.Header, "coveredPrdEndDt");

      field2.Color = "cyan";
      field2.Protected = true;
    }

    // if the next tran info is not equal to spaces, this implies the user 
    // requested a next tran action. now validate
    if (!IsEmpty(import.Standard.NextTransaction))
    {
      // : pr# 106234 M. Brown, Oct, 2000 - ensure person, case and legal action
      // std no are
      //   populated in the next tran view.
      if (!IsEmpty(export.ObligorCsePerson.Number) && !
        Equal(export.ObligorCsePerson.Number,
        export.HiddenNextTranInfo.CsePersonNumber))
      {
        // : Obligor number has been changed since this screen was entered,
        //  so reset next tran fields before flowing out.
        export.HiddenNextTranInfo.CsePersonNumberAp =
          export.ObligorCsePerson.Number;
        export.HiddenNextTranInfo.CsePersonNumber =
          export.ObligorCsePerson.Number;
        export.HiddenNextTranInfo.CsePersonNumberObligor =
          export.ObligorCsePerson.Number;
        export.HiddenNextTranInfo.StandardCrtOrdNumber =
          export.LegalAction.StandardNumber ?? "";
        export.HiddenNextTranInfo.CourtCaseNumber =
          export.LegalAction.CourtCaseNumber ?? "";
        export.HiddenNextTranInfo.LegalActionIdentifier =
          export.LegalAction.Identifier;
        export.HiddenNextTranInfo.ObligationId =
          export.Obligation.SystemGeneratedIdentifier;
        export.HiddenNextTranInfo.MiscNum2 =
          export.ObligationType.SystemGeneratedIdentifier;
        export.HiddenNextTranInfo.CourtOrderNumber = "";
        export.HiddenNextTranInfo.CsePersonNumberObligee = "";
        export.HiddenNextTranInfo.CaseNumber = "";
        export.HiddenNextTranInfo.InfrastructureId = 0;
      }

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

    switch(TrimEnd(global.Command))
    {
      case "XXNEXTXX":
        // ************************************************
        // *This is where you set your export value to the*
        // *export hidden next tran values if the user is *
        // *coming into this procedure on a next tran     *
        // *action.
        // 
        // *
        // ************************************************
        UseScCabNextTranGet();
        export.ObligorCsePerson.Number =
          export.HiddenNextTranInfo.CsePersonNumber ?? Spaces(10);
        export.LegalAction.Identifier =
          export.HiddenNextTranInfo.LegalActionIdentifier.GetValueOrDefault();
        export.LegalAction.StandardNumber =
          export.HiddenNextTranInfo.StandardCrtOrdNumber ?? "";

        if (Equal(export.HiddenNextTranInfo.LastTran, "SRPT") || Equal
          (export.HiddenNextTranInfo.LastTran, "SRPU"))
        {
          local.FromHistMonaNxttran.Flag = "Y";
          export.ObligorCsePerson.Number =
            export.HiddenNextTranInfo.CsePersonNumber ?? Spaces(10);
          global.Command = "DISPLAY";

          break;
        }
        else
        {
          local.FromHistMonaNxttran.Flag = "N";
        }

        ExitState = "FN_NO_NEXT_TO_ONAC";

        return;
      case "XXFMMENU":
        return;
      case "RETNAME":
        if (!IsEmpty(import.FromFlow.Number))
        {
          MoveCsePersonsWorkSet2(import.FromFlow, export.AltAddress);
          MoveCsePersonsWorkSet3(import.FromFlow, export.HiddenAltAddress);

          var field1 = GetField(export.AltAddress, "number");

          field1.Protected = false;
          field1.Focused = false;
        }

        if (IsEmpty(export.Obligation.OrderTypeCode))
        {
          export.Obligation.OrderTypeCode = "K";
        }

        if (!IsEmpty(export.HiddenStoredObligation.OtherStateAbbr))
        {
          var field1 = GetField(export.Obligation, "otherStateAbbr");

          field1.Color = "cyan";
          field1.Protected = true;

          var field2 = GetField(export.InterstateRequest, "country");

          field2.Color = "cyan";
          field2.Protected = true;

          var field3 = GetField(export.InterstateRequest, "tribalAgency");

          field3.Color = "cyan";
          field3.Protected = true;
        }

        if (!IsEmpty(export.HiddenInterstateRequest.OtherStateCaseId))
        {
          var field1 = GetField(export.InterstateRequest, "otherStateCaseId");

          field1.Color = "cyan";
          field1.Protected = true;

          var field2 = GetField(export.Obligation, "orderTypeCode");

          field2.Color = "cyan";
          field2.Protected = true;
        }

        if (!IsEmpty(export.HiddenInterstateRequest.Country))
        {
          var field1 = GetField(export.InterstateRequest, "country");

          field1.Color = "cyan";
          field1.Protected = true;

          var field2 = GetField(export.Obligation, "otherStateAbbr");

          field2.Color = "cyan";
          field2.Protected = true;

          var field3 = GetField(export.InterstateRequest, "tribalAgency");

          field3.Color = "cyan";
          field3.Protected = true;
        }

        if (!IsEmpty(export.HiddenInterstateRequest.TribalAgency))
        {
          var field1 = GetField(export.InterstateRequest, "country");

          field1.Color = "cyan";
          field1.Protected = true;

          var field2 = GetField(export.Obligation, "otherStateAbbr");

          field2.Color = "cyan";
          field2.Protected = true;

          var field3 = GetField(export.InterstateRequest, "tribalAgency");

          field3.Color = "cyan";
          field3.Protected = true;
        }

        if (AsChar(export.HiddenStoredObligation.OrderTypeCode) == 'K')
        {
          var field1 = GetField(export.Obligation, "orderTypeCode");

          field1.Color = "cyan";
          field1.Protected = true;

          var field2 = GetField(export.InterstateRequest, "otherStateCaseId");

          field2.Color = "cyan";
          field2.Protected = true;

          var field3 = GetField(export.Obligation, "otherStateAbbr");

          field3.Color = "cyan";
          field3.Protected = true;

          var field4 = GetField(export.InterstateRequest, "country");

          field4.Color = "cyan";
          field4.Protected = true;

          var field5 = GetField(export.InterstateRequest, "tribalAgency");

          field5.Color = "cyan";
          field5.Protected = true;
        }

        if (!Equal(export.HiddenStoredDebtDetail.CoveredPrdStartDt,
          local.BlankDateWorkArea.Date) && !
          Equal(export.HiddenStoredDebtDetail.CoveredPrdEndDt,
          local.BlankDateWorkArea.Date))
        {
          var field1 = GetField(export.Header, "coveredPrdStartDt");

          field1.Color = "cyan";
          field1.Protected = true;

          var field2 = GetField(export.Header, "coveredPrdEndDt");

          field2.Color = "cyan";
          field2.Protected = true;
        }

        return;
      case "ENTER":
        ExitState = "ACO_NE0000_INVALID_COMMAND";

        break;
      default:
        break;
    }

    // =================================================
    // 12/16/98 - B Adams  -  Next idea is that Legal doesn't want
    //   to have to enter this information so it has to be done here.
    // =================================================
    if (Equal(export.DisplayedObligCreateDt.Date, local.BlankDateWorkArea.Date))
    {
    }
    else if (Equal(export.DisplayedObligCreateDt.Date, local.Current.Date))
    {
    }
    else
    {
      var field1 = GetField(export.InterstateRequest, "otherStateCaseId");

      field1.Color = "cyan";
      field1.Protected = true;

      var field2 = GetField(export.InterstateRequest, "country");

      field2.Color = "cyan";
      field2.Protected = true;

      var field3 = GetField(export.InterstateRequest, "tribalAgency");

      field3.Color = "cyan";
      field3.Protected = true;

      var field4 = GetField(export.Country, "description");

      field4.Color = "cyan";
      field4.Protected = true;

      var field5 = GetField(export.Obligation, "otherStateAbbr");

      field5.Color = "cyan";
      field5.Protected = true;

      var field6 = GetField(export.Obligation, "orderTypeCode");

      field6.Color = "cyan";
      field6.Protected = true;
    }

    // ================================================
    // Moved this up from before general "main line" processing - B Adams - 10/
    // 14/98
    // ================================================
    if (Equal(global.Command, "FROMLDET") || Equal
      (global.Command, "FROMOPAY") || Equal(global.Command, "FROMOCTO"))
    {
      global.Command = "DISPLAY";
    }

    if (export.LegalAction.Identifier != 0 && export
      .Obligation.SystemGeneratedIdentifier == 0 && Equal
      (global.Command, "DISPLAY"))
    {
      export.ReturnFlowFrom.Text4 = "LDET";
      export.ObligorCsePerson.Number = "";
      export.ObligationType.SystemGeneratedIdentifier = 0;
    }

    // ************************************************
    // *Validate Data Level Security                  *
    // ************************************************
    if (Equal(global.Command, "ADD") || Equal(global.Command, "DISPLAY") || Equal
      (global.Command, "DELETE") || Equal(global.Command, "UPDATE"))
    {
      UseScCabTestSecurity();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }
    }

    // ***** EDIT AREA *****
    if (Equal(global.Command, "DISPLAY") && !
      Equal(export.ReturnFlowFrom.Text4, "LDET") || Equal
      (global.Command, "ADD") || Equal(global.Command, "DELETE") || Equal
      (global.Command, "UPDATE") || Equal(global.Command, "RTLIST"))
    {
      if (export.LegalAction.Identifier == 0 && IsEmpty
        (export.ObligationType.Code) && IsEmpty
        (export.ObligorCsePerson.Number))
      {
        ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";

        var field1 = GetField(export.LegalAction, "standardNumber");

        field1.Error = true;

        goto Test2;
      }

      if (export.LegalActionDetail.Number == 0 && IsEmpty
        (export.ObligationType.Code) && IsEmpty
        (export.ObligorCsePerson.Number))
      {
        ExitState = "LEGAL_ACTION_DETAIL_NF";

        goto Test2;
      }

      // : Read Obligor and Concurrent Obligor.
      // FYI: Obligor is imported as a persistent view in process ABs.
      local.TextWorkArea.Text10 = export.ObligorCsePerson.Number;
      UseEabPadLeftWithZeros();
      export.ObligorCsePerson.Number = local.TextWorkArea.Text10;

      if (Equal(global.Command, "ADD") || Equal(global.Command, "DELETE") || Equal
        (global.Command, "UPDATE"))
      {
        if (IsEmpty(export.ObligorCsePerson.Number))
        {
          var field1 = GetField(export.ObligorCsePerson, "number");

          field1.Error = true;

          ExitState = "ACO_NE0000_REQUIRED_DATA_MISSING";
        }

        if (export.ObligationType.SystemGeneratedIdentifier == 0 || IsEmpty
          (export.ObligationType.Code))
        {
          var field1 = GetField(export.ObligationType, "code");

          field1.Error = true;

          ExitState = "ACO_NE0000_REQUIRED_DATA_MISSING";
        }

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          local.ProgramFieldError.Flag = "Y";

          goto Test1;
        }

        // ***---  Sumanta - MTW - 05/05/1997
        // ***---  Check to see if an address exists for the person..
        // ***---
        if ((Equal(global.Command, "ADD") || Equal(global.Command, "UPDATE")) &&
          !IsEmpty(export.AltAddress.Number))
        {
          // ************************************************
          // We are Reading this to check if the Alternate Billing
          // Location has a Valid address or not. If not, we can not
          // add it as an alternate billing location for the amount
          // owed against this Obligation by the Payor.
          // ************************************************
          UseFnCabCheckAltAddr();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            var field1 = GetField(export.AltAddress, "number");

            field1.Error = true;
          }
        }
      }

Test1:

      // : If the Obligation System Generated ID is zero, Update and Delete 
      // functions are invalid.
      if (Equal(global.Command, "UPDATE") || Equal(global.Command, "DELETE"))
      {
        if (export.Obligation.SystemGeneratedIdentifier == 0)
        {
          ExitState = "ACO_NE0000_DISPLAY_BEFORE_UPDATE";
        }
      }
    }

Test2:

    if (Equal(global.Command, "DELETE"))
    {
      for(export.Export1.Index = 0; export.Export1.Index < export
        .Export1.Count; ++export.Export1.Index)
      {
        if (!IsEmpty(export.Export1.Item.Common.SelectChar))
        {
          var field1 = GetField(export.Export1.Item.Common, "selectChar");

          field1.Error = true;

          ExitState = "ACO_NE0000_PROMPT_INVALID_W_FNCT";
        }
      }
    }

    if (Equal(global.Command, "UPDATE") || Equal(global.Command, "ADD"))
    {
      if (AsChar(export.Obligation.OrderTypeCode) == 'I')
      {
        if (IsEmpty(export.InterstateRequest.OtherStateCaseId))
        {
          var field1 = GetField(export.InterstateRequest, "otherStateCaseId");

          field1.Error = true;

          ExitState = "FN0000_MANDATORY_FIELDS";
        }

        if (IsEmpty(export.Obligation.OtherStateAbbr) && IsEmpty
          (export.InterstateRequest.Country) && IsEmpty
          (export.InterstateRequest.TribalAgency))
        {
          var field1 = GetField(export.Obligation, "otherStateAbbr");

          field1.Error = true;

          var field2 = GetField(export.InterstateRequest, "country");

          field2.Error = true;

          var field3 = GetField(export.InterstateRequest, "tribalAgency");

          field3.Error = true;

          ExitState = "FN0000_IVD_AGENCY_REQUIRED";
        }

        local.IvdAgency.Count = 0;

        if (!IsEmpty(export.Obligation.OtherStateAbbr))
        {
          ++local.IvdAgency.Count;
        }

        if (!IsEmpty(export.InterstateRequest.Country))
        {
          ++local.IvdAgency.Count;
        }

        if (!IsEmpty(export.InterstateRequest.TribalAgency))
        {
          ++local.IvdAgency.Count;
        }

        if (local.IvdAgency.Count > 1)
        {
          var field1 = GetField(export.Obligation, "otherStateAbbr");

          field1.Error = true;

          var field2 = GetField(export.InterstateRequest, "country");

          field2.Error = true;

          var field3 = GetField(export.InterstateRequest, "tribalAgency");

          field3.Error = true;

          ExitState = "FN0000_IVD_AGENCY_REQUIRED";
        }
      }

      if (Equal(global.Command, "ADD") && AsChar
        (export.Obligation.OrderTypeCode) == 'I' && Equal
        (export.ObligationType.Code, "718B"))
      {
        // -------------------------------------------------------------------------
        // Per PR# 114682, obligation type '718B' can not be added/updated  if 
        // case is interstate.
        //                                                            
        // Vithal (02/28/2001)
        // ------------------------------------------------------------------------
        if (Equal(export.ObligationType.Code, "718B"))
        {
          var field1 = GetField(export.ObligationType, "code");

          field1.Error = true;

          ExitState = "ACO_NE0000_INTERSTATE_718B_ERROR";
        }
      }

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        goto Test3;
      }

      // =================================================
      // 3/1/00  K. Price - process country or state code for ADD
      // and UPDATE
      // =================================================
      switch(AsChar(export.Obligation.OrderTypeCode))
      {
        case 'I':
          UseFnRetrieveInterstateRequest();

          if (IsExitState("FN0000_FIPS_FOR_THE_STATE_NF"))
          {
            var field2 = GetField(export.Obligation, "otherStateAbbr");

            field2.Error = true;

            var field3 = GetField(export.InterstateRequest, "otherStateCaseId");

            field3.Error = true;
          }
          else if (IsExitState("INTERSTATE_REQUEST_NF"))
          {
            var field2 = GetField(export.InterstateRequest, "otherStateCaseId");

            field2.Error = true;
          }
          else if (IsExitState("LE0000_INVALID_COUNTRY_CODE"))
          {
            var field2 = GetField(export.InterstateRequest, "country");

            field2.Error = true;
          }
          else if (IsExitState("ACO_NE0000_INVALID_STATE_CODE"))
          {
            var field2 = GetField(export.Obligation, "otherStateAbbr");

            field2.Error = true;
          }
          else if (IsExitState("FN0000_INVALID_COUNTRY_INTERSTAT"))
          {
            var field2 = GetField(export.InterstateRequest, "country");

            field2.Error = true;
          }
          else if (IsExitState("FN0000_INVALID_STATE_INTERSTATE"))
          {
            var field2 = GetField(export.Obligation, "otherStateAbbr");

            field2.Error = true;
          }
          else if (IsExitState("FN0000_INTERSTATE_AP_MISMATCH"))
          {
            var field2 = GetField(export.InterstateRequest, "otherStateCaseId");

            field2.Error = true;
          }
          else if (IsExitState("FN0000_INVALID_TRIBAL_AGENCY"))
          {
            var field2 = GetField(export.InterstateRequest, "tribalAgency");

            field2.Error = true;
          }
          else if (IsExitState("FN0000_INVALID_TRIBAL_INTERSTAT"))
          {
            var field2 = GetField(export.InterstateRequest, "tribalAgency");

            field2.Error = true;
          }
          else
          {
          }

          break;
        case 'K':
          if (!IsEmpty(export.Obligation.OtherStateAbbr))
          {
            var field2 = GetField(export.Obligation, "otherStateAbbr");

            field2.Error = true;

            ExitState = "INVALID_VALUE";
          }

          if (!IsEmpty(export.InterstateRequest.Country))
          {
            var field2 = GetField(export.InterstateRequest, "country");

            field2.Error = true;

            ExitState = "INVALID_VALUE";
          }

          if (!IsEmpty(export.InterstateRequest.TribalAgency))
          {
            var field2 = GetField(export.InterstateRequest, "tribalAgency");

            field2.Error = true;

            ExitState = "INVALID_VALUE";
          }

          if (!IsEmpty(export.InterstateRequest.OtherStateCaseId))
          {
            var field2 = GetField(export.InterstateRequest, "otherStateCaseId");

            field2.Error = true;

            ExitState = "INVALID_VALUE";
          }

          break;
        case ' ':
          export.Obligation.OrderTypeCode = "K";
          export.InterstateRequest.OtherStateCaseId = "";
          export.InterstateRequest.Country = "";
          export.InterstateRequest.TribalAgency = "";
          export.Obligation.OtherStateAbbr = "";

          break;
        default:
          var field1 = GetField(export.Obligation, "orderTypeCode");

          field1.Error = true;

          ExitState = "FN0000_INVALID_INTERSTATE_IND";

          break;
      }

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        goto Test3;
      }

      // : Obligation Type entered must exist and must have classification of "
      // non-accruing".
      // FYI: Obligation Type is imported as a persistent view in the process 
      // ABs.
      if (ReadObligationType())
      {
        if (AsChar(entities.ObligationType.Classification) != AsChar
          (local.HardcodeOtCMedicalClassifica.Classification) && AsChar
          (entities.ObligationType.Classification) != AsChar
          (local.HardcodeOtCNonAccruingClass.Classification))
        {
          ExitState = "FN0000_OBLIG_TYPE_INVALID";

          var field1 = GetField(export.ObligationType, "code");

          field1.Error = true;

          goto Test3;
        }

        // ** OK ** Obligation Type Classification is valid
        export.ObligationType.Assign(entities.ObligationType);
      }
      else
      {
        ExitState = "FN0000_OBLIG_TYPE_NF";

        var field1 = GetField(export.ObligationType, "code");

        field1.Error = true;

        goto Test3;
      }

      // : DATE EDITS for ADD and UPDATE commands.
      // ************************************************
      // *Due Date is mandatory.                        *
      // ************************************************
      if (Equal(export.Header.DueDt, local.BlankDateWorkArea.Date))
      {
        ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";

        var field1 = GetField(export.Header, "dueDt");

        field1.Error = true;
      }

      // ================================================
      // 10/14/98 - B Adams  -   Debts coming into KESSEP via conversion
      //   will not have Covered Period Dates - and this is OK.
      // ================================================
      if (Equal(export.Obligation.CreatedBy, "CONVERSN"))
      {
      }
      else
      {
        if (Equal(export.Header.CoveredPrdStartDt, local.BlankDateWorkArea.Date))
          
        {
          ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";

          var field1 = GetField(export.Header, "coveredPrdStartDt");

          field1.Color = "red";
          field1.Highlighting = Highlighting.ReverseVideo;
          field1.Protected = false;
          field1.Focused = true;

          return;
        }

        if (Equal(export.Header.CoveredPrdEndDt, local.BlankDateWorkArea.Date))
        {
          ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";

          var field1 = GetField(export.Header, "coveredPrdEndDt");

          field1.Color = "red";
          field1.Highlighting = Highlighting.ReverseVideo;
          field1.Protected = false;
          field1.Focused = true;

          return;
        }
      }

      if (!Lt(export.Header.CoveredPrdStartDt, export.Header.CoveredPrdEndDt) &&
        !Equal(export.Header.CoveredPrdStartDt, local.BlankDateWorkArea.Date))
      {
        // : Jan, 2002, M. Brown, PR# 128555
        //   End date was protected when this error occurred.  Added unprotect 
        // of end date.
        var field1 = GetField(export.Header, "coveredPrdStartDt");

        field1.Error = true;

        var field2 = GetField(export.Header, "coveredPrdEndDt");

        field2.Error = true;

        ExitState = "FN0000_END_DATE_ERROR";
      }

      if (Lt(export.Header.DueDt, export.Header.CoveredPrdStartDt))
      {
        ExitState = "FN0000_COV_PRD_DAT_GTE_DUE_DATE";

        var field1 = GetField(export.Header, "coveredPrdStartDt");

        field1.Color = "red";
        field1.Highlighting = Highlighting.ReverseVideo;
        field1.Protected = false;
        field1.Focused = true;

        var field2 = GetField(export.Header, "dueDt");

        field2.Color = "yellow";
        field2.Highlighting = Highlighting.ReverseVideo;
        field2.Protected = true;
        field2.Focused = false;
      }

      if (Lt(export.Header.DueDt, export.Header.CoveredPrdEndDt))
      {
        ExitState = "FN0000_COV_PRD_DAT_GTE_DUE_DATE";

        var field1 = GetField(export.Header, "coveredPrdEndDt");

        field1.Color = "red";
        field1.Highlighting = Highlighting.ReverseVideo;
        field1.Protected = false;
        field1.Focused = true;

        var field2 = GetField(export.Header, "dueDt");

        field2.Color = "yellow";
        field2.Highlighting = Highlighting.ReverseVideo;
        field2.Protected = true;
      }

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }

      if (!Lt(AddMonths(local.Current.Date, -1), export.Header.DueDt))
      {
        if (Lt(export.Header.DueDt, AddYears(local.Current.Date, -20)))
        {
          ExitState = "FN0000_DATE_CANT_BE_OVER_20_YRS";

          var field1 = GetField(export.Header, "dueDt");

          field1.Error = true;

          return;
        }
      }

      // : EDIT LIST PORTION OF SCREEN for ADD and UPDATE commands.
      local.SelectionCount.Count = 0;

      for(export.Export1.Index = 0; export.Export1.Index < export
        .Export1.Count; ++export.Export1.Index)
      {
        MoveDebtDetail2(export.Header, export.Export1.Update.DebtDetail);

        // ************************************************
        // *Check the Obligation Type supported person    *
        // *indicator.  If "N", no supported persons are  *
        // *allowed.
        // 
        // *
        // ************************************************
        if (AsChar(entities.ObligationType.SupportedPersonReqInd) == 'N')
        {
          ExitState = "FN0000_OB_TYP_CANT_HAVE_SUP_PRSN";

          var field1 = GetField(export.ObligationType, "code");

          field1.Error = true;

          goto Test3;
        }

        // : Jan, 2002, M. Brown, Work Order # 020144
        //   Make the pre-conversion code updateable.
        // : 718B Obligations are always AF.
        if (export.ObligationType.SystemGeneratedIdentifier == local
          .HardcodeOt718BUraJudgement.SystemGeneratedIdentifier)
        {
          export.Export1.Update.DebtDetail.PreconversionProgramCode = "AF";
        }

        if (IsEmpty(export.Export1.Item.DebtDetail.PreconversionProgramCode))
        {
          // : Jan, 2002, M. Brown, Work Order # 020144
          //   Make the pre-conversion code updateable.
          if (Equal(global.Command, "ADD") || Equal
            (global.Command, "UPDATE") && !
            Equal(export.Export1.Item.DebtDetail.PreconversionProgramCode,
            export.Export1.Item.Hidden.PreconversionProgramCode))
          {
            if (AsChar(export.Obligation.OrderTypeCode) == 'I')
            {
              ExitState = "FN_INVALID_PRECONV_CODE_FOR_I_OB";
            }
            else
            {
              ExitState = "FN0000_INVALID_PRECONV_PGM_CODE";
            }

            var field1 =
              GetField(export.Export1.Item.DebtDetail,
              "preconversionProgramCode");

            field1.Error = true;

            local.ProgramFieldError.Flag = "Y";
          }
        }
        else if (AsChar(export.Obligation.OrderTypeCode) == 'I')
        {
          // : Jan, 2002, M. Brown, Work Order # 020144
          //  Interstate obligations can only have preconversion program codes 
          // of
          //  NAI, AFI or FCI.
          if (Equal(export.Export1.Item.DebtDetail.PreconversionProgramCode,
            "AFI") || Equal
            (export.Export1.Item.DebtDetail.PreconversionProgramCode, "FCI") ||
            Equal
            (export.Export1.Item.DebtDetail.PreconversionProgramCode, "NAI"))
          {
          }
          else
          {
            var field1 =
              GetField(export.Export1.Item.DebtDetail,
              "preconversionProgramCode");

            field1.Error = true;

            ExitState = "FN_INVALID_PRECONV_CODE_FOR_I_OB";
            local.ProgramFieldError.Flag = "Y";
          }
        }
        else if (Equal(export.Export1.Item.DebtDetail.PreconversionProgramCode,
          "AF") || Equal
          (export.Export1.Item.DebtDetail.PreconversionProgramCode, "FC") || Equal
          (export.Export1.Item.DebtDetail.PreconversionProgramCode, "NA") || Equal
          (export.Export1.Item.DebtDetail.PreconversionProgramCode, "NC") || Equal
          (export.Export1.Item.DebtDetail.PreconversionProgramCode, "NF"))
        {
        }
        else
        {
          var field1 =
            GetField(export.Export1.Item.DebtDetail, "preconversionProgramCode");
            

          field1.Error = true;

          ExitState = "FN0000_INVALID_PRECONV_PGM_CODE";
          local.ProgramFieldError.Flag = "Y";
        }

        // If edit errors found on current line, exit FOR EACH loop
        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          goto Test3;
        }
      }

      // : Jan, 2002, M. Brown, Work Order # 020144
      // Check for a blank time frame for the covered period start date.  If it 
      // falls
      // in a time frame that is blank, user must fill in time frames on PEPR 
      // before the
      // add/update may be done.
      if (Equal(global.Command, "ADD") || Equal(global.Command, "UPDATE") && !
        Equal(export.Export1.Item.DebtDetail.PreconversionProgramCode,
        export.Export1.Item.Hidden.PreconversionProgramCode))
      {
        for(export.Export1.Index = 0; export.Export1.Index < export
          .Export1.Count; ++export.Export1.Index)
        {
          if (AsChar(export.Export1.Item.Common.SelectChar) == 'S')
          {
            if (!ReadPersonProgram())
            {
              // : Blank time frame.
              ExitState = "FN0000_FILL_IN_TIMEFRAME_ON_PEPR";
            }
          }
        }
      }
    }

Test3:

    // ***** MAIN-LINE *****
    switch(TrimEnd(global.Command))
    {
      case "REIP":
        export.ReturnFlowFrom.Text4 = "REIP";
        ExitState = "ECO_LNK_TO_REC_IND_PYMNT_HIST";

        break;
      case "DISPLAY":
        if (IsEmpty(export.ObligorCsePerson.Number) || export
          .ObligationType.SystemGeneratedIdentifier == 0)
        {
          // <<< RBM   12/08/97  When it is a flow from either LDET or or a flow
          // without any selection from another screen dealing with
          // Obligations. >>>
          if (AsChar(local.FromHistMonaNxttran.Flag) == 'Y')
          {
            UseFnGetOblFromHistMonaNxtran();

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              break;
            }
          }

          UseFnAbDetIfObExistsForLa2();
        }
        else
        {
          local.ObExists.Flag = "Y";
        }

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          break;
        }

        if (AsChar(local.ObExists.Flag) == 'Y')
        {
          UseFnReadSupportObligation();

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            ExitState = "OBLIGATION_SUCCESSFULLY_DISPLAY";
          }
          else
          {
            break;
          }

          if (export.ObligationType.SystemGeneratedIdentifier == local
            .HardcodeOtInterestJudgment.SystemGeneratedIdentifier)
          {
            export.SuspendInterestInd.Flag = "Y";
          }
        }
        else
        {
          UseFnReadLegalActionInfo();

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            ExitState = "OBLIGATION_NOT_ACTIVE";
          }
          else
          {
            break;
          }

          if (export.ObligationType.SystemGeneratedIdentifier == local
            .HardcodeOtInterestJudgment.SystemGeneratedIdentifier)
          {
            export.SuspendInterestInd.Flag = "Y";
          }

          if (IsEmpty(export.InterstateRequest.Country) && !
            IsEmpty(export.Country.Cdvalue))
          {
            export.InterstateRequest.Country =
              Substring(export.Country.Cdvalue, 1, 2);
          }
        }

        // 03/14/2002       Maureen Brown      WO# 10504   Retro processing.
        if (ReadObligCollProtectionHist())
        {
          export.ObCollProtAct.Flag = "Y";
        }
        else
        {
          export.ObCollProtAct.Flag = "N";
        }

        export.DisplayedObligCreateDt.Date =
          Date(export.Obligation.CreatedTmst);
        MoveDebtDetail4(export.Header, export.HiddenStoredDebtDetail);
        export.HiddenInterstateRequest.Assign(export.InterstateRequest);
        MoveObligation6(export.Obligation, export.HiddenStoredObligation);

        // -----------------------------------------------------------------------------------
        // The following fields need to be protected if they are read /added/
        // updated  to/from the database.  First time when we display/add/update
        // screen,  the values are moved to hidden_stored views. So check the
        // hidden_stored views  and protect them .
        // ---------------------------------------------------------------------------------
        if (!IsEmpty(export.HiddenStoredObligation.OtherStateAbbr))
        {
          var field1 = GetField(export.Obligation, "otherStateAbbr");

          field1.Color = "cyan";
          field1.Protected = true;

          var field2 = GetField(export.InterstateRequest, "country");

          field2.Color = "cyan";
          field2.Protected = true;

          var field3 = GetField(export.InterstateRequest, "tribalAgency");

          field3.Color = "cyan";
          field3.Protected = true;
        }

        if (!IsEmpty(export.HiddenInterstateRequest.OtherStateCaseId))
        {
          var field1 = GetField(export.InterstateRequest, "otherStateCaseId");

          field1.Color = "cyan";
          field1.Protected = true;

          var field2 = GetField(export.Obligation, "orderTypeCode");

          field2.Color = "cyan";
          field2.Protected = true;
        }

        if (!IsEmpty(export.HiddenInterstateRequest.Country))
        {
          var field1 = GetField(export.InterstateRequest, "country");

          field1.Color = "cyan";
          field1.Protected = true;

          var field2 = GetField(export.InterstateRequest, "tribalAgency");

          field2.Color = "cyan";
          field2.Protected = true;

          var field3 = GetField(export.Obligation, "otherStateAbbr");

          field3.Color = "cyan";
          field3.Protected = true;
        }

        if (!IsEmpty(export.HiddenInterstateRequest.TribalAgency))
        {
          var field1 = GetField(export.InterstateRequest, "country");

          field1.Color = "cyan";
          field1.Protected = true;

          var field2 = GetField(export.InterstateRequest, "tribalAgency");

          field2.Color = "cyan";
          field2.Protected = true;

          var field3 = GetField(export.Obligation, "otherStateAbbr");

          field3.Color = "cyan";
          field3.Protected = true;
        }

        if (!Equal(export.HiddenStoredDebtDetail.CoveredPrdStartDt,
          local.BlankDateWorkArea.Date) && !
          Equal(export.HiddenStoredDebtDetail.CoveredPrdEndDt,
          local.BlankDateWorkArea.Date))
        {
          var field1 = GetField(export.Header, "coveredPrdStartDt");

          field1.Color = "cyan";
          field1.Protected = true;

          var field2 = GetField(export.Header, "coveredPrdEndDt");

          field2.Color = "cyan";
          field2.Protected = true;
        }

        // =================================================
        // 6/16/99 - bud adams  -  If the dates are blank and the create
        //   date has passed and the balance owed is zero, it's a
        //   conversion debt that has been adjusted to zero and needs
        //   to be treated as if it's a new obligation.
        // =================================================
        if (Lt(export.DisplayedObligCreateDt.Date, local.Current.Date) && Lt
          (local.BlankDateWorkArea.Date, export.DisplayedObligCreateDt.Date) &&
          !
          Equal(export.Header.CoveredPrdStartDt, local.BlankDateWorkArea.Date) &&
          !
          Equal(export.Header.CoveredPrdEndDt, local.BlankDateWorkArea.Date) &&
          export.BalanceOwed.TotalCurrency != 0)
        {
          var field1 = GetField(export.Header, "coveredPrdStartDt");

          field1.Color = "cyan";
          field1.Protected = true;

          var field2 = GetField(export.Header, "coveredPrdEndDt");

          field2.Color = "cyan";
          field2.Protected = true;
        }

        if (IsExitState("OBLIGATION_NOT_ACTIVE") || IsExitState
          ("OBLIGATION_SUCCESSFULLY_DISPLAY"))
        {
          if (IsExitState("OBLIGATION_SUCCESSFULLY_DISPLAY"))
          {
            export.Previous.DueDt = export.Header.DueDt;
            export.HiddenDisplayed.Assign(export.Header);
          }

          if (AsChar(export.ObligationActive.Flag) == 'Y')
          {
            local.ProtectAll.Flag = "Y";
          }

          for(export.Export1.Index = 0; export.Export1.Index < export
            .Export1.Count; ++export.Export1.Index)
          {
            export.Export1.Update.Prev.Amount =
              export.Export1.Item.ObligationTransaction.Amount;
          }

          if (IsExitState("OBLIGATION_NOT_ACTIVE"))
          {
            // ================================================
            // 1/3/00 - Kent Price  -
            // Check local_ob_exists
            //        If 'R' - exit state 'confirm before add'
            //             else  exit state 'PF5 to add'
            // ================================================
            if (AsChar(local.ObExists.Flag) == 'R')
            {
              ExitState = "FN0000_OB_NF_FOR_LA_RETIRED";
              export.Obligation.Assign(local.Null1);
            }
            else
            {
              ExitState = "FN0000_OB_NF_FOR_LA_PF5_TO_ADD";
            }
          }

          // =================================================
          // 5/12/99 - bud adams  -  These must be protected
          // =================================================
          // =================================================
          // 05/04/2000  Vithal Madhira    Commented as requested by SME ( 
          // Marilyn Gasperich). SME is going to update the Business rules
          // accordingly.
          // =================================================
          for(export.Export1.Index = 0; export.Export1.Index < export
            .Export1.Count; ++export.Export1.Index)
          {
            export.Export1.Update.Hidden.PreconversionProgramCode =
              export.Export1.Item.DebtDetail.PreconversionProgramCode;
          }

          switch(AsChar(export.Obligation.OrderTypeCode))
          {
            case 'I':
              UseFnRetrieveInterstateRequest();

              break;
            case 'K':
              export.Obligation.OtherStateAbbr = "";
              export.InterstateRequest.Country = "";
              export.InterstateRequest.TribalAgency = "";
              export.InterstateRequest.OtherStateCaseId = "";

              var field1 = GetField(export.Obligation, "otherStateAbbr");

              field1.Color = "cyan";
              field1.Protected = true;

              var field2 = GetField(export.Obligation, "orderTypeCode");

              field2.Color = "cyan";
              field2.Protected = true;

              var field3 = GetField(export.InterstateRequest, "country");

              field3.Color = "cyan";
              field3.Protected = true;

              var field4 = GetField(export.InterstateRequest, "tribalAgency");

              field4.Color = "cyan";
              field4.Protected = true;

              var field5 =
                GetField(export.InterstateRequest, "otherStateCaseId");

              field5.Color = "cyan";
              field5.Protected = true;

              break;
            default:
              export.Obligation.OrderTypeCode = "K";
              export.Obligation.OtherStateAbbr = "";

              break;
          }
        }

        // ****---- end of CASE DISPLAY
        break;
      case "ADD":
        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          break;
        }

        if (AsChar(export.Obligation.OrderTypeCode) != 'I')
        {
          export.Obligation.OrderTypeCode = "K";
        }

        if (Equal(export.ObligorCsePerson.Number,
          export.ConcurrentCsePerson.Number))
        {
          ExitState = "FN0000_OBLIGOR_TWICE_ON_LOPS";

          break;
        }

        for(export.Export1.Index = 0; export.Export1.Index < export
          .Export1.Count; ++export.Export1.Index)
        {
          local.TestDup.Count = 0;

          for(import.Import1.Index = 0; import.Import1.Index < import
            .Import1.Count; ++import.Import1.Index)
          {
            if (Equal(export.Export1.Item.SupportedCsePerson.Number,
              import.Import1.Item.SupportedCsePerson.Number))
            {
              ++local.TestDup.Count;
            }
          }

          if (local.TestDup.Count > 1)
          {
            ExitState = "FN0000_SUPPORTED_TWICE_ON_LOPS";

            goto Test5;
          }
        }

        // ************************************************
        // *              A D D   Logic                   *
        // ************************************************
        // : Check to see if the obligation being added is potentially a 
        // duplicate.
        UseFnAbDetIfObExistsForLa1();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          break;
        }

        if (AsChar(local.ObExists.Flag) == 'Y')
        {
          ExitState = "FN0000_SIMILAR_OBLG_FOUND";

          for(export.Export1.Index = 0; export.Export1.Index < export
            .Export1.Count; ++export.Export1.Index)
          {
            export.Export1.Update.Hidden.PreconversionProgramCode =
              export.Export1.Item.DebtDetail.PreconversionProgramCode;
          }

          // -----------------------------------------------------------------------------------
          // The following fields need to be protected if they are read /added/
          // updated  to/from the database.  First time when we display/add/
          // update  screen,  the values are moved to hidden_stored views. So
          // check the hidden_stored views  and protect them .
          // ---------------------------------------------------------------------------------
          if (!IsEmpty(export.HiddenStoredObligation.OrderTypeCode))
          {
            var field1 = GetField(export.Obligation, "orderTypeCode");

            field1.Color = "cyan";
            field1.Protected = true;
          }

          if (!IsEmpty(export.HiddenStoredObligation.OtherStateAbbr))
          {
            var field1 = GetField(export.Obligation, "otherStateAbbr");

            field1.Color = "cyan";
            field1.Protected = true;

            var field2 = GetField(export.InterstateRequest, "country");

            field2.Color = "cyan";
            field2.Protected = true;

            var field3 = GetField(export.InterstateRequest, "tribalAgency");

            field3.Color = "cyan";
            field3.Protected = true;
          }

          if (!IsEmpty(export.HiddenInterstateRequest.OtherStateCaseId))
          {
            var field1 = GetField(export.InterstateRequest, "otherStateCaseId");

            field1.Color = "cyan";
            field1.Protected = true;
          }

          if (!IsEmpty(export.HiddenInterstateRequest.Country))
          {
            var field1 = GetField(export.InterstateRequest, "country");

            field1.Color = "cyan";
            field1.Protected = true;

            var field2 = GetField(export.Obligation, "otherStateAbbr");

            field2.Color = "cyan";
            field2.Protected = true;

            var field3 = GetField(export.InterstateRequest, "tribalAgency");

            field3.Color = "cyan";
            field3.Protected = true;
          }

          if (!IsEmpty(export.HiddenInterstateRequest.TribalAgency))
          {
            var field1 = GetField(export.InterstateRequest, "country");

            field1.Color = "cyan";
            field1.Protected = true;

            var field2 = GetField(export.Obligation, "otherStateAbbr");

            field2.Color = "cyan";
            field2.Protected = true;

            var field3 = GetField(export.InterstateRequest, "tribalAgency");

            field3.Color = "cyan";
            field3.Protected = true;
          }

          if (!Equal(export.HiddenStoredDebtDetail.CoveredPrdStartDt,
            local.BlankDateWorkArea.Date) && !
            Equal(export.HiddenStoredDebtDetail.CoveredPrdEndDt,
            local.BlankDateWorkArea.Date))
          {
            var field1 = GetField(export.Header, "coveredPrdStartDt");

            field1.Color = "cyan";
            field1.Protected = true;

            var field2 = GetField(export.Header, "coveredPrdEndDt");

            field2.Color = "cyan";
            field2.Protected = true;
          }

          if (AsChar(export.HiddenStoredObligation.OrderTypeCode) == 'K')
          {
            var field1 = GetField(export.Obligation, "otherStateAbbr");

            field1.Color = "cyan";
            field1.Protected = true;

            var field2 = GetField(export.Obligation, "orderTypeCode");

            field2.Color = "cyan";
            field2.Protected = true;

            var field3 = GetField(export.InterstateRequest, "country");

            field3.Color = "cyan";
            field3.Protected = true;

            var field4 = GetField(export.InterstateRequest, "tribalAgency");

            field4.Color = "cyan";
            field4.Protected = true;

            var field5 = GetField(export.InterstateRequest, "otherStateCaseId");

            field5.Color = "cyan";
            field5.Protected = true;
          }

          break;
        }

        // ------------------------------------------------------------------------
        // Sometimes user can hit PF5 ( ADD) key mistakenly when an obligation 
        // was already displayed on the screen. In this case we must not ADD the
        // already displayed obligation again.  FYI: OB_ID was  protected on
        // screen.
        // --------------------------------------------------------------------------
        if (export.Obligation.SystemGeneratedIdentifier > 0)
        {
          ExitState = "FN0000_SIMILAR_OBLG_FOUND";

          for(export.Export1.Index = 0; export.Export1.Index < export
            .Export1.Count; ++export.Export1.Index)
          {
            export.Export1.Update.Hidden.PreconversionProgramCode =
              export.Export1.Item.DebtDetail.PreconversionProgramCode;

            // -------------------------------------------------------------------
            // WR# 000257 : If the Obligation type is 'MJ', the 'PROG'  and the
            // 'SEL' fields must be unprotected. The PROG field can be added
            // with 'AF' or 'NA' , if the obligation type is 'MJ'.
            // ------------------------------------------------------------------------------
            // -------------------------------------------------------------------
            // WR# 020144, Jan, 2002, M. Brown
            // The above rule has changed.  MJ obligations can have any of the 
            // valid program types.
            // ------------------------------------------------------------------------------
          }

          // -----------------------------------------------------------------------------------
          // The following fields need to be protected if they are read /added/
          // updated  to/from the database.  First time when we display/add/
          // update  screen,  the values are moved to hidden_stored views. So
          // check the hidden_stored views  and protect them .
          // ---------------------------------------------------------------------------------
          if (!IsEmpty(export.HiddenStoredObligation.OrderTypeCode))
          {
            var field1 = GetField(export.Obligation, "orderTypeCode");

            field1.Color = "cyan";
            field1.Protected = true;
          }

          if (!IsEmpty(export.HiddenStoredObligation.OtherStateAbbr))
          {
            var field1 = GetField(export.Obligation, "otherStateAbbr");

            field1.Color = "cyan";
            field1.Protected = true;

            var field2 = GetField(export.InterstateRequest, "country");

            field2.Color = "cyan";
            field2.Protected = true;

            var field3 = GetField(export.InterstateRequest, "tribalAgency");

            field3.Color = "cyan";
            field3.Protected = true;
          }

          if (!IsEmpty(export.HiddenInterstateRequest.OtherStateCaseId))
          {
            var field1 = GetField(export.InterstateRequest, "otherStateCaseId");

            field1.Color = "cyan";
            field1.Protected = true;
          }

          if (!IsEmpty(export.HiddenInterstateRequest.Country))
          {
            var field1 = GetField(export.InterstateRequest, "country");

            field1.Color = "cyan";
            field1.Protected = true;

            var field2 = GetField(export.Obligation, "otherStateAbbr");

            field2.Color = "cyan";
            field2.Protected = true;

            var field3 = GetField(export.InterstateRequest, "tribalAgency");

            field3.Color = "cyan";
            field3.Protected = true;
          }

          if (!IsEmpty(export.HiddenInterstateRequest.TribalAgency))
          {
            var field1 = GetField(export.InterstateRequest, "country");

            field1.Color = "cyan";
            field1.Protected = true;

            var field2 = GetField(export.Obligation, "otherStateAbbr");

            field2.Color = "cyan";
            field2.Protected = true;

            var field3 = GetField(export.InterstateRequest, "tribalAgency");

            field3.Color = "cyan";
            field3.Protected = true;
          }

          if (!Equal(export.HiddenStoredDebtDetail.CoveredPrdStartDt,
            local.BlankDateWorkArea.Date) && !
            Equal(export.HiddenStoredDebtDetail.CoveredPrdEndDt,
            local.BlankDateWorkArea.Date))
          {
            var field1 = GetField(export.Header, "coveredPrdStartDt");

            field1.Color = "cyan";
            field1.Protected = true;

            var field2 = GetField(export.Header, "coveredPrdEndDt");

            field2.Color = "cyan";
            field2.Protected = true;
          }

          if (AsChar(export.HiddenStoredObligation.OrderTypeCode) == 'K')
          {
            var field1 = GetField(export.Obligation, "otherStateAbbr");

            field1.Color = "cyan";
            field1.Protected = true;

            var field2 = GetField(export.Obligation, "orderTypeCode");

            field2.Color = "cyan";
            field2.Protected = true;

            var field3 = GetField(export.InterstateRequest, "country");

            field3.Color = "cyan";
            field3.Protected = true;

            var field4 = GetField(export.InterstateRequest, "tribalAgency");

            field4.Color = "cyan";
            field4.Protected = true;

            var field5 = GetField(export.InterstateRequest, "otherStateCaseId");

            field5.Color = "cyan";
            field5.Protected = true;
          }

          break;
        }

        if (!IsEmpty(export.ConcurrentCsePerson.Number))
        {
          export.Obligation.PrimarySecondaryCode =
            local.HardcodeObligJointSeveralCon.PrimarySecondaryCode ?? "";
        }

        // ================================================
        // 1/31/00 - Kent Price  -
        // If all the flags are Z, then no case was found for
        // support persons
        // ================================================
        ExitState = "NO_CASE_RL_FOUND_FOR_SUPP_PERSON";

        for(export.Export1.Index = 0; export.Export1.Index < export
          .Export1.Count; ++export.Export1.Index)
        {
          if (AsChar(export.Export1.Item.SupportedCsePersonsWorkSet.Flag) != 'Z'
            )
          {
            ExitState = "ACO_NN0000_ALL_OK";

            break;
          }
        }

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          break;
        }

        if (IsEmpty(export.Obligation.OtherStateAbbr) && AsChar
          (export.Obligation.OrderTypeCode) == 'K')
        {
          export.Obligation.OtherStateAbbr = "KS";
        }

        // : WR 010504 - M Brown
        // To support Collection Protection, a warning message is to be 
        // displayed when the worker changes the discontinue date for the
        // obligation or any of the supported persons.
        if (IsEmpty(import.CollProtAnswer.SelectChar))
        {
          local.ObligCollProtectionHist.CvrdCollStrtDt =
            local.BlankDateWorkArea.Date;

          for(export.Export1.Index = 0; export.Export1.Index < export
            .Export1.Count; ++export.Export1.Index)
          {
            if (Equal(export.Export1.Item.DebtDetail.PreconversionProgramCode,
              "AF"))
            {
            }
            else
            {
              local.ObligCollProtectionHist.CvrdCollStrtDt =
                export.Header.DueDt;

              break;
            }
          }

          if (Equal(local.ObligCollProtectionHist.CvrdCollStrtDt,
            local.BlankDateWorkArea.Date))
          {
            goto Test4;
          }

          // : Only go further if a debt with preconversion code NOT 'AF' is 
          // found.
          if (ReadCollection())
          {
            var field1 = GetField(export.CollProtAnswer, "selectChar");

            field1.Highlighting = Highlighting.Underscore;
            field1.Protected = false;
            field1.Focused = true;

            export.ProtectQuestionLiteral.Text80 =
              "State retained collections exist: protect prior payments?";
            ExitState = "FN0000_Y_OR_N_AND_PF5_TO_ADD_OB";

            return;
          }
          else
          {
            // : OK
          }
        }
        else if (AsChar(import.CollProtAnswer.SelectChar) == 'N')
        {
          // : OK, continue.
        }
        else if (AsChar(import.CollProtAnswer.SelectChar) == 'Y')
        {
          // : Protect collections.
          local.ObligCollProtectionHist.CvrdCollEndDt = local.Current.Date;
          local.ObligCollProtectionHist.CvrdCollStrtDt = local.Current.Date;

          for(export.Export1.Index = 0; export.Export1.Index < export
            .Export1.Count; ++export.Export1.Index)
          {
            if (Lt(export.Export1.Item.DebtDetail.DueDt,
              local.ObligCollProtectionHist.CvrdCollStrtDt) && !
              Equal(export.Export1.Item.DebtDetail.PreconversionProgramCode,
              "AF"))
            {
              local.ObligCollProtectionHist.CvrdCollStrtDt =
                export.Export1.Item.DebtDetail.DueDt;
            }
          }

          foreach(var item in ReadObligation3())
          {
            UseFnProtectCollectionsForOblig();

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              return;
            }
          }
        }
        else
        {
          // : Something other than 'Y' or 'N' was entered in the answer.
          var field1 = GetField(export.CollProtAnswer, "selectChar");

          field1.Highlighting = Highlighting.Underscore;
          field1.Protected = false;
          field1.Focused = true;

          export.ProtectQuestionLiteral.Text80 =
            "State retained collections exist: protect prior payments?";
          ExitState = "FN0000_Y_OR_N_AND_PF5_TO_ADD_OB";

          return;
        }

Test4:

        UseFnCreateObligation1();

        if (Equal(export.Obligation.OtherStateAbbr, "KS"))
        {
          export.Obligation.OtherStateAbbr = "";
          export.Obligation.OrderTypeCode = "K";
        }

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          break;
        }

        if (Equal(import.ObligationType.Code, "MANUAL") || export
          .ObligationType.SystemGeneratedIdentifier == local
          .HardcodeOtInterestJudgment.SystemGeneratedIdentifier)
        {
          export.InterestOwed.TotalCurrency =
            export.ObligationAmt.TotalCurrency;
          export.SuspendInterestInd.Flag = "Y";
        }
        else
        {
          export.BalanceOwed.TotalCurrency = export.ObligationAmt.TotalCurrency;
        }

        export.DisplayedObligCreateDt.Date =
          Date(export.Obligation.CreatedTmst);
        export.Obligation.CreatedBy = global.UserId;
        export.Obligation.CreatedTmst = local.Current.Timestamp;
        export.TotalOwed.TotalCurrency = export.ObligationAmt.TotalCurrency;

        // ---------------------------------------------
        // 02/18/97  R.B.Mohapatra
        //           Set alternate address FIPS for the
        //           Primary Obligation.
        // 05/05/97  Sumanta Mahapatra - MTW
        //         *-Removed the current FIPS related code.
        //         *-Added the association between Obligation and alt add
        // ---------------------------------------------
        // 2/23/1999 - B Adams  -  Restructured the IF constructs and
        //   logic.  Replaced the interstate request CRUD and replaced
        //   it with the CAB.
        // =================================================
        if (!IsEmpty(export.AltAddress.Number))
        {
          if (ReadObligation2())
          {
            if (ReadCsePerson())
            {
              AssociateObligation();
            }
            else
            {
              ExitState = "CSE_PERSON_NF";
            }
          }
          else
          {
            ExitState = "FN0000_OBLIGATION_NF";
          }

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            break;
          }
        }

        if (!IsEmpty(export.InterstateRequest.OtherStateCaseId))
        {
          UseFnCreateInterstateRqstOblign1();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            MoveObligation11(local.BlankObligation, export.Obligation);
            export.InterestOwed.TotalCurrency = 0;
            export.BalanceOwed.TotalCurrency = 0;
            export.TotalOwed.TotalCurrency = 0;

            var field1 = GetField(export.InterstateRequest, "otherStateCaseId");

            field1.Error = true;

            for(export.Export1.Index = 0; export.Export1.Index < export
              .Export1.Count; ++export.Export1.Index)
            {
              export.Export1.Update.Hidden.PreconversionProgramCode =
                export.Export1.Item.DebtDetail.PreconversionProgramCode;

              // : Jan, 2002, M. Brown, Wrok ORder # 020144
              //   Preconversion program code is updateable.  Removed protection
              // of it,
              //   and the 'sel' field.
            }

            // -----------------------------------------------------------------------------------
            // The following fields need to be protected if they are read /added
            // /updated  to/from the database.  First time when we display/add/
            // update  screen,  the values are moved to hidden_stored views. So
            // check the hidden_stored views  and protect them .
            // ---------------------------------------------------------------------------------
            if (!IsEmpty(export.HiddenStoredObligation.OrderTypeCode))
            {
              var field2 = GetField(export.Obligation, "orderTypeCode");

              field2.Color = "cyan";
              field2.Protected = true;
            }

            if (!IsEmpty(export.HiddenStoredObligation.OtherStateAbbr))
            {
              var field2 = GetField(export.Obligation, "otherStateAbbr");

              field2.Color = "cyan";
              field2.Protected = true;

              var field3 = GetField(export.InterstateRequest, "country");

              field3.Color = "cyan";
              field3.Protected = true;

              var field4 = GetField(export.InterstateRequest, "tribalAgency");

              field4.Color = "cyan";
              field4.Protected = true;
            }

            if (!IsEmpty(export.HiddenInterstateRequest.OtherStateCaseId))
            {
              var field2 =
                GetField(export.InterstateRequest, "otherStateCaseId");

              field2.Color = "cyan";
              field2.Protected = true;
            }

            if (!IsEmpty(export.HiddenInterstateRequest.Country))
            {
              var field2 = GetField(export.InterstateRequest, "country");

              field2.Color = "cyan";
              field2.Protected = true;

              var field3 = GetField(export.Obligation, "otherStateAbbr");

              field3.Color = "cyan";
              field3.Protected = true;

              var field4 = GetField(export.InterstateRequest, "tribalAgency");

              field4.Color = "cyan";
              field4.Protected = true;
            }

            if (!IsEmpty(export.HiddenInterstateRequest.TribalAgency))
            {
              var field2 = GetField(export.InterstateRequest, "country");

              field2.Color = "cyan";
              field2.Protected = true;

              var field3 = GetField(export.Obligation, "otherStateAbbr");

              field3.Color = "cyan";
              field3.Protected = true;

              var field4 = GetField(export.InterstateRequest, "tribalAgency");

              field4.Color = "cyan";
              field4.Protected = true;
            }

            if (!Equal(export.HiddenStoredDebtDetail.CoveredPrdStartDt,
              local.BlankDateWorkArea.Date) && !
              Equal(export.HiddenStoredDebtDetail.CoveredPrdEndDt,
              local.BlankDateWorkArea.Date))
            {
              var field2 = GetField(export.Header, "coveredPrdStartDt");

              field2.Color = "cyan";
              field2.Protected = true;

              var field3 = GetField(export.Header, "coveredPrdEndDt");

              field3.Color = "cyan";
              field3.Protected = true;
            }

            if (AsChar(export.Obligation.OrderTypeCode) == 'K')
            {
              var field2 = GetField(export.Obligation, "otherStateAbbr");

              field2.Color = "cyan";
              field2.Protected = true;

              var field3 = GetField(export.Obligation, "orderTypeCode");

              field3.Color = "cyan";
              field3.Protected = true;

              var field4 = GetField(export.InterstateRequest, "country");

              field4.Color = "cyan";
              field4.Protected = true;

              var field5 = GetField(export.InterstateRequest, "tribalAgency");

              field5.Color = "cyan";
              field5.Protected = true;

              var field6 =
                GetField(export.InterstateRequest, "otherStateCaseId");

              field6.Color = "cyan";
              field6.Protected = true;
            }

            break;
          }
        }

        if (!IsEmpty(export.ConcurrentCsePerson.Number))
        {
          export.Obligation.PrimarySecondaryCode =
            local.HardcodeObligJointSeveralCon.PrimarySecondaryCode ?? "";
          UseFnCreateObligation2();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            break;
          }

          // ---------------------------------------------
          // 02/18/97  R.B.Mohapatra
          //           Set alternate address FIPS for the
          //           Concurrent Obligation.
          // 05/05/97  Sumanta Mahapatra - MTW
          //         *-Removed the current FIPS related code.
          //         *-Added the association between Obligation and alt add
          // ---------------------------------------------
          // 2/23/1999 - b adams  -  Again restructured IF constructs and
          //   logic; replaced interstate request CRUD with CAB
          // =================================================
          if (!IsEmpty(export.AltAddress.Number))
          {
            if (ReadObligation1())
            {
              if (ReadCsePerson())
              {
                AssociateObligation();
              }
              else
              {
                ExitState = "CSE_PERSON_NF";
              }
            }
            else
            {
              ExitState = "FN0000_OBLIGATION_NF";
            }

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              break;
            }
          }

          if (!IsEmpty(export.InterstateRequest.OtherStateCaseId))
          {
            UseFnCreateInterstateRqstOblign2();

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              MoveObligation11(local.BlankObligation, export.Obligation);
              export.InterestOwed.TotalCurrency = 0;
              export.BalanceOwed.TotalCurrency = 0;
              export.TotalOwed.TotalCurrency = 0;

              var field1 =
                GetField(export.InterstateRequest, "otherStateCaseId");

              field1.Error = true;

              break;
            }
          }

          UseFnRelateTwoObligations();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            break;
          }
        }

        // : Now handle the Obligation Details.
        for(export.Export1.Index = 0; export.Export1.Index < export
          .Export1.Count; ++export.Export1.Index)
        {
          if (!IsEmpty(export.Export1.Item.SupportedCsePerson.Number))
          {
            // =================================================
            // 5/26/99 - bud adams  -  If a supported is non-case related
            //   then do not create an ob_tran record.
            // =================================================
            if (AsChar(export.Export1.Item.SupportedCsePersonsWorkSet.Flag) == 'Z'
              )
            {
              continue;
            }

            export.Export1.Update.DebtDetail.CoveredPrdStartDt =
              import.Header.CoveredPrdStartDt;
            export.Export1.Update.DebtDetail.BalanceDueAmt =
              export.Export1.Item.ObligationTransaction.Amount;
            export.Export1.Update.DebtDetail.DueDt = import.Header.DueDt;

            // <<< RBM  02/25/1998 Removed the code which was explicitly setting
            // the Interest_Balance_Due_Amt in debt_dtl for "IJ" >>>
            export.Export1.Update.DebtDetail.BalanceDueAmt =
              export.Export1.Item.ObligationTransaction.Amount;
            local.Infrastructure.SituationNumber = 0;
            local.Infrastructure.ReferenceDate = local.Current.Date;
            UseFnCreateObligationTransaction1();

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              break;
            }

            // : Aug 10, 1999 mfb.
            local.Infrastructure.EventId = 45;
            local.Infrastructure.UserId = "ONAC";
            local.Infrastructure.CsePersonNumber =
              import.ObligorCsePerson.Number;

            if (import.LegalAction.Identifier == 0)
            {
              local.Infrastructure.BusinessObjectCd = "OBL";
              local.Infrastructure.DenormNumeric12 =
                export.Export1.Item.ObligationTransaction.
                  SystemGeneratedIdentifier;
              local.Infrastructure.DenormText12 = local.HardcodeOtrnTDebt.Type1;
              local.Infrastructure.ReferenceDate = local.Current.Date;
            }
            else
            {
              local.Infrastructure.BusinessObjectCd = "LEA";
              local.Infrastructure.DenormNumeric12 =
                import.LegalAction.Identifier;
              local.Infrastructure.DenormText12 =
                import.LegalAction.CourtCaseNumber ?? "";
              local.Infrastructure.ReferenceDate = import.LegalAction.FiledDate;
            }

            UseFnCabRaiseEvent1();

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              goto Test5;
            }

            if (!IsEmpty(export.ConcurrentCsePerson.Number))
            {
              // =================================================
              // 3/26/00 -  Create Joint and Several Obligation Transactions
              // =================================================
              UseFnCreateObligationTransaction2();

              if (!IsExitState("ACO_NN0000_ALL_OK"))
              {
                break;
              }

              // : Aug 10, 1999 mfb.
              local.Infrastructure.CsePersonNumber =
                import.ConcurrentCsePerson.Number;

              if (export.LegalAction.Identifier == 0)
              {
                local.Infrastructure.BusinessObjectCd = "OBL";
                local.Infrastructure.DenormNumeric12 =
                  export.Export1.Item.Concurrent.SystemGeneratedIdentifier;
                local.Infrastructure.DenormText12 =
                  local.HardcodeOtrnTDebt.Type1;
              }
              else
              {
                local.Infrastructure.BusinessObjectCd = "LEA";
                local.Infrastructure.DenormNumeric12 =
                  export.LegalAction.Identifier;
                local.Infrastructure.DenormText12 =
                  export.LegalAction.CourtCaseNumber ?? "";
                local.Infrastructure.ReferenceDate =
                  export.LegalAction.FiledDate;
              }

              UseFnCabRaiseEvent2();

              if (!IsExitState("ACO_NN0000_ALL_OK"))
              {
                goto Test5;
              }

              local.ConcurrentCommon.Flag = "Y";
              local.ConcurrentObligationTransaction.SystemGeneratedIdentifier =
                export.Export1.Item.ObligationTransaction.
                  SystemGeneratedIdentifier;
              local.ConcurrentObligationTransaction.Amount =
                export.Export1.Item.ObligationTransaction.Amount;
              UseFnCreateObligationTranRln();
            }
          }

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            goto Test5;
          }
        }

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          export.Previous.DueDt = export.Header.DueDt;

          if (IsEmpty(export.ConcurrentCsePerson.Number))
          {
            var field1 = GetField(export.ConcurrentCsePerson, "number");

            field1.Color = "cyan";
            field1.Protected = true;

            var field2 =
              GetField(export.ConcurrentCsePersonsWorkSet, "formattedName");

            field2.Color = "cyan";
            field2.Protected = true;
          }

          for(export.Export1.Index = 0; export.Export1.Index < export
            .Export1.Count; ++export.Export1.Index)
          {
            export.Export1.Update.Prev.Amount =
              export.Export1.Item.ObligationTransaction.Amount;
          }

          if (export.ObligationType.SystemGeneratedIdentifier == local
            .HardcodeOtInterestJudgment.SystemGeneratedIdentifier)
          {
            export.SuspendInterestInd.Flag = "Y";
          }

          // =================================================
          // 2/13/1999 - bud adams  -  After an obligation has been Added
          //   but no activities have been posted against it, it can be updated.
          // =================================================
          export.ObligationActive.Flag = "N";
          ExitState = "ACO_NI0000_CREATED_OK";
          MoveObligation6(export.Obligation, export.HiddenStoredObligation);
          export.HiddenInterstateRequest.Assign(export.InterstateRequest);
          MoveDebtDetail4(export.Header, export.HiddenStoredDebtDetail);

          // : Jan, 2002, M. Brown, Work Order # 020144
          //   Removed pre-conversion code protection.
          for(export.Export1.Index = 0; export.Export1.Index < export
            .Export1.Count; ++export.Export1.Index)
          {
            export.Export1.Update.Hidden.PreconversionProgramCode =
              export.Export1.Item.DebtDetail.PreconversionProgramCode;
            export.Export1.Update.Common.SelectChar = "";
          }

          // -----------------------------------------------------------------------------------
          // The following fields need to be protected if they are read /added/
          // updated  to/from the database.  First time when we display/add/
          // update  screen,  the values are moved to hidden_stored views. So
          // check the hidden_stored views  and protect them .
          // ---------------------------------------------------------------------------------
          if (!IsEmpty(export.HiddenStoredObligation.OrderTypeCode))
          {
            var field1 = GetField(export.Obligation, "orderTypeCode");

            field1.Color = "cyan";
            field1.Protected = true;
          }

          if (!IsEmpty(export.HiddenStoredObligation.OtherStateAbbr))
          {
            var field1 = GetField(export.Obligation, "otherStateAbbr");

            field1.Color = "cyan";
            field1.Protected = true;

            var field2 = GetField(export.InterstateRequest, "country");

            field2.Color = "cyan";
            field2.Protected = true;

            var field3 = GetField(export.InterstateRequest, "tribalAgency");

            field3.Color = "cyan";
            field3.Protected = true;
          }

          if (!IsEmpty(export.HiddenInterstateRequest.OtherStateCaseId))
          {
            var field1 = GetField(export.InterstateRequest, "otherStateCaseId");

            field1.Color = "cyan";
            field1.Protected = true;
          }

          if (!IsEmpty(export.HiddenInterstateRequest.Country))
          {
            var field1 = GetField(export.InterstateRequest, "country");

            field1.Color = "cyan";
            field1.Protected = true;

            var field2 = GetField(export.Obligation, "otherStateAbbr");

            field2.Color = "cyan";
            field2.Protected = true;

            var field3 = GetField(export.InterstateRequest, "tribalAgency");

            field3.Color = "cyan";
            field3.Protected = true;
          }

          if (!IsEmpty(export.HiddenInterstateRequest.TribalAgency))
          {
            var field1 = GetField(export.InterstateRequest, "country");

            field1.Color = "cyan";
            field1.Protected = true;

            var field2 = GetField(export.Obligation, "otherStateAbbr");

            field2.Color = "cyan";
            field2.Protected = true;

            var field3 = GetField(export.InterstateRequest, "tribalAgency");

            field3.Color = "cyan";
            field3.Protected = true;
          }

          if (!Equal(export.HiddenStoredDebtDetail.CoveredPrdStartDt,
            local.BlankDateWorkArea.Date) && !
            Equal(export.HiddenStoredDebtDetail.CoveredPrdEndDt,
            local.BlankDateWorkArea.Date))
          {
            var field1 = GetField(export.Header, "coveredPrdStartDt");

            field1.Color = "cyan";
            field1.Protected = true;

            var field2 = GetField(export.Header, "coveredPrdEndDt");

            field2.Color = "cyan";
            field2.Protected = true;
          }

          if (AsChar(export.HiddenStoredObligation.OrderTypeCode) == 'K')
          {
            var field1 = GetField(export.Obligation, "otherStateAbbr");

            field1.Color = "cyan";
            field1.Protected = true;

            var field2 = GetField(export.Obligation, "orderTypeCode");

            field2.Color = "cyan";
            field2.Protected = true;

            var field3 = GetField(export.InterstateRequest, "country");

            field3.Color = "cyan";
            field3.Protected = true;

            var field4 = GetField(export.InterstateRequest, "tribalAgency");

            field4.Color = "cyan";
            field4.Protected = true;

            var field5 = GetField(export.InterstateRequest, "otherStateCaseId");

            field5.Color = "cyan";
            field5.Protected = true;
          }
        }
        else
        {
        }

        // ****----  end of CASE ADD
        break;
      case "UPDATE":
        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }

        if (IsEmpty(export.Obligation.OrderTypeCode))
        {
          export.Obligation.OrderTypeCode = "K";
        }

        if (IsEmpty(export.Obligation.OtherStateAbbr) && AsChar
          (export.Obligation.OrderTypeCode) == 'K')
        {
          export.Obligation.OtherStateAbbr = "KS";
        }

        MoveObligation8(export.Obligation, local.PriorToUpdate);
        UseFnUpdateNonAccruingOblig2();

        if (Equal(export.Obligation.OtherStateAbbr, "KS"))
        {
          export.Obligation.OtherStateAbbr = "";
        }

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          break;
        }

        // ---------------------------------------------------------------
        // 02/18/97  R.B.Mohapatra
        //           Update the Alternate billing address FIPS.
        //           The same cab SET_ALTERNATE_ADDRESS accomplices the task.
        // 05/05/97  Sumanta Mahapatra - MTW
        //       *- Removed the FIPS related codes..
        //       *- Added the association of obligation to alt add
        // ---------------------------------------------------------------
        // =================================================
        // 1/6/1999 - B Adams  -  The alternate address handling was
        //   totally incorrect.  Created a CAB and all debt PrADs use it.
        // =================================================
        if (!IsEmpty(export.AltAddress.Number))
        {
          UseFnUpdateAlternateAddress1();

          if (IsExitState("FN0000_CSE_PERSON_ACCOUNT_NF_RB"))
          {
            var field1 = GetField(export.AltAddress, "number");

            field1.Error = true;
          }
          else if (IsExitState("FN0000_ALTERNATE_ADDR_NF_RB"))
          {
            var field1 = GetField(export.AltAddress, "number");

            field1.Error = true;
          }
          else
          {
          }

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            break;
          }
        }

        // =================================================
        // 12/18/98 - b adams  -  This can only happen on the day the
        //   obligation is created, and then only to correct a mistake.
        //   Changed the IF logic; changed the Update to Delete.
        // =================================================
        if (!Equal(export.InterstateRequest.OtherStateCaseId,
          export.HiddenInterstateRequest.OtherStateCaseId))
        {
          // =================================================
          // 2/23/1999 - b adams  -  IF construct was wrong.  Logic had
          //   been fixed, but replaced it all with this new CAB.
          // =================================================
          UseFnUpdateInterstateRqstOblign2();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            break;
          }
        }

        if (AsChar(export.ObligationActive.Flag) == 'Y')
        {
          local.ProtectAll.Flag = "Y";
        }

        if (!IsEmpty(export.ConcurrentCsePerson.Number))
        {
          MoveObligation8(local.PriorToUpdate, export.ConcurrentObligation);
          local.Concurr.Flag = "Y";
          UseFnUpdateNonAccruingOblig1();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            break;
          }

          // =================================================
          // 1/6/1999 - B Adams  -  The alternate address handling was
          //   totally incorrect.  Created a CAB and all debt PrADs use it.
          // =================================================
          if (!IsEmpty(export.AltAddress.Number))
          {
            UseFnUpdateAlternateAddress2();

            if (IsExitState("FN0000_CSE_PERSON_ACCOUNT_NF_RB"))
            {
              var field1 = GetField(export.AltAddress, "number");

              field1.Error = true;
            }
            else if (IsExitState("FN0000_ALTERNATE_ADDR_NF_RB"))
            {
              var field1 = GetField(export.AltAddress, "number");

              field1.Error = true;
            }
            else
            {
            }

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              break;
            }
          }

          if (!Equal(export.InterstateRequest.OtherStateCaseId,
            export.HiddenInterstateRequest.OtherStateCaseId))
          {
            // =================================================
            // 2/23/1999 - b adams  -  IF construct was wrong, logic was
            //   totally wrong.  This is a new CAB to fix this problem in all
            //   of trade.  Each did it differently, etc....
            // =================================================
            UseFnUpdateInterstateRqstOblign1();

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              break;
            }
          }
        }

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          export.Previous.DueDt = export.Header.DueDt;
          export.HiddenInterstateRequest.Assign(export.InterstateRequest);
          export.HiddenAltAddress.Assign(export.AltAddress);

          if (export.ObligationType.SystemGeneratedIdentifier == local
            .HardcodeOtInterestJudgment.SystemGeneratedIdentifier)
          {
            export.SuspendInterestInd.Flag = "Y";
          }

          ExitState = "ACO_NI0000_SUCCESSFUL_UPDATE";
          MoveObligation6(export.Obligation, export.HiddenStoredObligation);
          export.HiddenInterstateRequest.Assign(export.InterstateRequest);
          MoveDebtDetail4(export.Header, export.HiddenStoredDebtDetail);

          for(export.Export1.Index = 0; export.Export1.Index < export
            .Export1.Count; ++export.Export1.Index)
          {
            export.Export1.Update.Hidden.PreconversionProgramCode =
              export.Export1.Item.DebtDetail.PreconversionProgramCode;
          }

          // -----------------------------------------------------------------------------------
          // The following fields need to be protected if they are read /added/
          // updated  to/from the database.  First time when we display/add/
          // update  screen,  the values are moved to hidden_stored views. So
          // check the hidden_stored views  and protect them .
          // ---------------------------------------------------------------------------------
          if (!IsEmpty(export.HiddenStoredObligation.OrderTypeCode))
          {
            var field1 = GetField(export.Obligation, "orderTypeCode");

            field1.Color = "cyan";
            field1.Protected = true;
          }

          if (!IsEmpty(export.HiddenStoredObligation.OtherStateAbbr))
          {
            var field1 = GetField(export.Obligation, "otherStateAbbr");

            field1.Color = "cyan";
            field1.Protected = true;

            var field2 = GetField(export.InterstateRequest, "country");

            field2.Color = "cyan";
            field2.Protected = true;

            var field3 = GetField(export.InterstateRequest, "tribalAgency");

            field3.Color = "cyan";
            field3.Protected = true;
          }

          if (!IsEmpty(export.HiddenInterstateRequest.OtherStateCaseId))
          {
            var field1 = GetField(export.InterstateRequest, "otherStateCaseId");

            field1.Color = "cyan";
            field1.Protected = true;
          }

          if (!IsEmpty(export.HiddenInterstateRequest.Country))
          {
            var field1 = GetField(export.InterstateRequest, "country");

            field1.Color = "cyan";
            field1.Protected = true;

            var field2 = GetField(export.Obligation, "otherStateAbbr");

            field2.Color = "cyan";
            field2.Protected = true;

            var field3 = GetField(export.InterstateRequest, "tribalAgency");

            field3.Color = "cyan";
            field3.Protected = true;
          }

          if (!IsEmpty(export.HiddenInterstateRequest.TribalAgency))
          {
            var field1 = GetField(export.InterstateRequest, "country");

            field1.Color = "cyan";
            field1.Protected = true;

            var field2 = GetField(export.Obligation, "otherStateAbbr");

            field2.Color = "cyan";
            field2.Protected = true;

            var field3 = GetField(export.InterstateRequest, "tribalAgency");

            field3.Color = "cyan";
            field3.Protected = true;
          }

          if (AsChar(export.HiddenStoredObligation.OrderTypeCode) == 'K')
          {
            var field1 = GetField(export.Obligation, "otherStateAbbr");

            field1.Color = "cyan";
            field1.Protected = true;

            var field2 = GetField(export.Obligation, "orderTypeCode");

            field2.Color = "cyan";
            field2.Protected = true;

            var field3 = GetField(export.InterstateRequest, "country");

            field3.Color = "cyan";
            field3.Protected = true;

            var field4 = GetField(export.InterstateRequest, "tribalAgency");

            field4.Color = "cyan";
            field4.Protected = true;

            var field5 = GetField(export.InterstateRequest, "otherStateCaseId");

            field5.Color = "cyan";
            field5.Protected = true;
          }

          if (!Equal(export.HiddenStoredDebtDetail.CoveredPrdStartDt,
            local.BlankDateWorkArea.Date) && !
            Equal(export.HiddenStoredDebtDetail.CoveredPrdEndDt,
            local.BlankDateWorkArea.Date))
          {
            var field1 = GetField(export.Header, "coveredPrdStartDt");

            field1.Color = "cyan";
            field1.Protected = true;

            var field2 = GetField(export.Header, "coveredPrdEndDt");

            field2.Color = "cyan";
            field2.Protected = true;
          }
        }

        // ****----  end of CASE UPDATE
        break;
      case "DELETE":
        // -----------------------------------------------------------------------------------
        // The following fields need to be protected.
        // ---------------------------------------------------------------------------------
        if (!IsEmpty(export.HiddenStoredObligation.OrderTypeCode))
        {
          var field1 = GetField(export.Obligation, "orderTypeCode");

          field1.Color = "cyan";
          field1.Protected = true;
        }

        if (!IsEmpty(export.HiddenStoredObligation.OtherStateAbbr))
        {
          var field1 = GetField(export.Obligation, "otherStateAbbr");

          field1.Color = "cyan";
          field1.Protected = true;

          var field2 = GetField(export.InterstateRequest, "country");

          field2.Color = "cyan";
          field2.Protected = true;

          var field3 = GetField(export.InterstateRequest, "tribalAgency");

          field3.Color = "cyan";
          field3.Protected = true;
        }

        if (!IsEmpty(export.HiddenInterstateRequest.OtherStateCaseId))
        {
          var field1 = GetField(export.InterstateRequest, "otherStateCaseId");

          field1.Color = "cyan";
          field1.Protected = true;
        }

        if (!IsEmpty(export.HiddenInterstateRequest.Country))
        {
          var field1 = GetField(export.InterstateRequest, "country");

          field1.Color = "cyan";
          field1.Protected = true;

          var field2 = GetField(export.Obligation, "otherStateAbbr");

          field2.Color = "cyan";
          field2.Protected = true;

          var field3 = GetField(export.InterstateRequest, "tribalAgency");

          field3.Color = "cyan";
          field3.Protected = true;
        }

        if (!IsEmpty(export.HiddenInterstateRequest.TribalAgency))
        {
          var field1 = GetField(export.InterstateRequest, "country");

          field1.Color = "cyan";
          field1.Protected = true;

          var field2 = GetField(export.Obligation, "otherStateAbbr");

          field2.Color = "cyan";
          field2.Protected = true;

          var field3 = GetField(export.InterstateRequest, "tribalAgency");

          field3.Color = "cyan";
          field3.Protected = true;
        }

        if (!Equal(export.HiddenStoredDebtDetail.CoveredPrdStartDt,
          local.BlankDateWorkArea.Date) && !
          Equal(export.HiddenStoredDebtDetail.CoveredPrdEndDt,
          local.BlankDateWorkArea.Date))
        {
          var field1 = GetField(export.Header, "coveredPrdStartDt");

          field1.Color = "cyan";
          field1.Protected = true;

          var field2 = GetField(export.Header, "coveredPrdEndDt");

          field2.Color = "cyan";
          field2.Protected = true;
        }

        if (AsChar(export.Obligation.OrderTypeCode) == 'K')
        {
          var field1 = GetField(export.Obligation, "otherStateAbbr");

          field1.Color = "cyan";
          field1.Protected = true;

          var field2 = GetField(export.Obligation, "orderTypeCode");

          field2.Color = "cyan";
          field2.Protected = true;

          var field3 = GetField(export.InterstateRequest, "country");

          field3.Color = "cyan";
          field3.Protected = true;

          var field4 = GetField(export.InterstateRequest, "tribalAgency");

          field4.Color = "cyan";
          field4.Protected = true;

          var field5 = GetField(export.InterstateRequest, "otherStateCaseId");

          field5.Color = "cyan";
          field5.Protected = true;
        }

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }

        if (Lt(export.DisplayedObligCreateDt.Date, local.Current.Date) && !
          Equal(export.DisplayedObligCreateDt.Date, local.BlankDateWorkArea.Date))
          
        {
          ExitState = "FN0000_CANT_DEL_AFTER_CREAT_DATE";

          return;
        }

        UseFnCheckObligationForActivity();

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          if (AsChar(export.ObligationActive.Flag) == 'Y')
          {
            ExitState = "CANNOT_DELETE_DUE_TO_ACTIVITY";

            break;
          }
        }
        else
        {
          break;
        }

        UseFnRemoveObligation1();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          break;
        }
        else
        {
          ExitState = "ACO_NI0000_SUCCESSFUL_DELETE";
        }

        if (!IsEmpty(export.ConcurrentCsePerson.Number))
        {
          UseFnRemoveObligation2();

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            ExitState = "ACO_NI0000_SUCCESSFUL_DELETE";
          }
          else
          {
          }
        }

        break;
      case "BYPASS":
        // : Jan, 2002, M. Brown, Work Order # 020144
        //   Removed pre-conversion code protection.
        // -----------------------------------------------------------------------------------
        // The following fields need to be protected if they are read /added/
        // updated  to/from the database.  First time when we display/add/update
        // screen,  the values are moved to hidden_stored views. So check the
        // hidden_stored views  and protect them .
        // ---------------------------------------------------------------------------------
        if (!IsEmpty(export.HiddenStoredObligation.OrderTypeCode))
        {
          var field1 = GetField(export.Obligation, "orderTypeCode");

          field1.Color = "cyan";
          field1.Protected = true;
        }

        if (!IsEmpty(export.HiddenStoredObligation.OtherStateAbbr))
        {
          var field1 = GetField(export.Obligation, "otherStateAbbr");

          field1.Color = "cyan";
          field1.Protected = true;

          var field2 = GetField(export.InterstateRequest, "country");

          field2.Color = "cyan";
          field2.Protected = true;

          var field3 = GetField(export.InterstateRequest, "tribalAgency");

          field3.Color = "cyan";
          field3.Protected = true;
        }

        if (!IsEmpty(export.HiddenInterstateRequest.OtherStateCaseId))
        {
          var field1 = GetField(export.InterstateRequest, "otherStateCaseId");

          field1.Color = "cyan";
          field1.Protected = true;
        }

        if (!IsEmpty(export.HiddenInterstateRequest.Country))
        {
          var field1 = GetField(export.InterstateRequest, "country");

          field1.Color = "cyan";
          field1.Protected = true;

          var field2 = GetField(export.Obligation, "otherStateAbbr");

          field2.Color = "cyan";
          field2.Protected = true;

          var field3 = GetField(export.InterstateRequest, "tribalAgency");

          field3.Color = "cyan";
          field3.Protected = true;
        }

        if (!IsEmpty(export.HiddenInterstateRequest.TribalAgency))
        {
          var field1 = GetField(export.InterstateRequest, "country");

          field1.Color = "cyan";
          field1.Protected = true;

          var field2 = GetField(export.Obligation, "otherStateAbbr");

          field2.Color = "cyan";
          field2.Protected = true;

          var field3 = GetField(export.InterstateRequest, "tribalAgency");

          field3.Color = "cyan";
          field3.Protected = true;
        }

        if (!Equal(export.HiddenStoredDebtDetail.CoveredPrdStartDt,
          local.BlankDateWorkArea.Date) && !
          Equal(export.HiddenStoredDebtDetail.CoveredPrdEndDt,
          local.BlankDateWorkArea.Date))
        {
          var field1 = GetField(export.Header, "coveredPrdStartDt");

          field1.Color = "cyan";
          field1.Protected = true;

          var field2 = GetField(export.Header, "coveredPrdEndDt");

          field2.Color = "cyan";
          field2.Protected = true;
        }

        if (AsChar(export.HiddenStoredObligation.OrderTypeCode) == 'K')
        {
          var field1 = GetField(export.Obligation, "otherStateAbbr");

          field1.Color = "cyan";
          field1.Protected = true;

          var field2 = GetField(export.Obligation, "orderTypeCode");

          field2.Color = "cyan";
          field2.Protected = true;

          var field3 = GetField(export.InterstateRequest, "country");

          field3.Color = "cyan";
          field3.Protected = true;

          var field4 = GetField(export.InterstateRequest, "tribalAgency");

          field4.Color = "cyan";
          field4.Protected = true;

          var field5 = GetField(export.InterstateRequest, "otherStateCaseId");

          field5.Color = "cyan";
          field5.Protected = true;
        }

        break;
      case "MDIS":
        ExitState = "ECO_LNK_TO_MTN_MANUAL_DIST_INST";

        break;
      case "INMS":
        ExitState = "ACO_NE0000_OPTION_UNAVAILABLE";

        // : Jan, 2002, M. Brown, Work Order # 020144
        //   Removed pre-conversion code protection.
        if (!IsEmpty(export.HiddenStoredObligation.OrderTypeCode))
        {
          var field1 = GetField(export.Obligation, "orderTypeCode");

          field1.Color = "cyan";
          field1.Protected = true;
        }

        if (!IsEmpty(export.HiddenStoredObligation.OtherStateAbbr))
        {
          var field1 = GetField(export.Obligation, "otherStateAbbr");

          field1.Color = "cyan";
          field1.Protected = true;

          var field2 = GetField(export.InterstateRequest, "country");

          field2.Color = "cyan";
          field2.Protected = true;

          var field3 = GetField(export.InterstateRequest, "tribalAgency");

          field3.Color = "cyan";
          field3.Protected = true;
        }

        if (!IsEmpty(export.HiddenInterstateRequest.OtherStateCaseId))
        {
          var field1 = GetField(export.InterstateRequest, "otherStateCaseId");

          field1.Color = "cyan";
          field1.Protected = true;
        }

        if (!IsEmpty(export.HiddenInterstateRequest.Country))
        {
          var field1 = GetField(export.InterstateRequest, "country");

          field1.Color = "cyan";
          field1.Protected = true;

          var field2 = GetField(export.Obligation, "otherStateAbbr");

          field2.Color = "cyan";
          field2.Protected = true;

          var field3 = GetField(export.InterstateRequest, "tribalAgency");

          field3.Color = "cyan";
          field3.Protected = true;
        }

        if (!IsEmpty(export.HiddenInterstateRequest.TribalAgency))
        {
          var field1 = GetField(export.InterstateRequest, "country");

          field1.Color = "cyan";
          field1.Protected = true;

          var field2 = GetField(export.Obligation, "otherStateAbbr");

          field2.Color = "cyan";
          field2.Protected = true;

          var field3 = GetField(export.InterstateRequest, "tribalAgency");

          field3.Color = "cyan";
          field3.Protected = true;
        }

        if (!Equal(export.HiddenStoredDebtDetail.CoveredPrdStartDt,
          local.BlankDateWorkArea.Date) && !
          Equal(export.HiddenStoredDebtDetail.CoveredPrdEndDt,
          local.BlankDateWorkArea.Date))
        {
          var field1 = GetField(export.Header, "coveredPrdStartDt");

          field1.Color = "cyan";
          field1.Protected = true;

          var field2 = GetField(export.Header, "coveredPrdEndDt");

          field2.Color = "cyan";
          field2.Protected = true;
        }

        if (AsChar(export.HiddenStoredObligation.OrderTypeCode) == 'K')
        {
          var field1 = GetField(export.Obligation, "otherStateAbbr");

          field1.Color = "cyan";
          field1.Protected = true;

          var field2 = GetField(export.Obligation, "orderTypeCode");

          field2.Color = "cyan";
          field2.Protected = true;

          var field3 = GetField(export.InterstateRequest, "country");

          field3.Color = "cyan";
          field3.Protected = true;

          var field4 = GetField(export.InterstateRequest, "tribalAgency");

          field4.Color = "cyan";
          field4.Protected = true;

          var field5 = GetField(export.InterstateRequest, "otherStateCaseId");

          field5.Color = "cyan";
          field5.Protected = true;
        }

        break;
      case "OPAY":
        ExitState = "ECO_LNK_LST_OBLIG_BY_AP_PAYOR";

        break;
      case "OCTO":
        ExitState = "ECO_LNK_TO_LST_OBLIG_BY_CRT_ORDR";

        break;
      case "OPSC":
        // : Link to Maintain Payment Schedules.
        ExitState = "ECO_LNK_TO_LST_MTN_PYMNT_SCH";

        break;
      case "CSPM":
        // : Link to List/Maintain Statement/Coupon Suppression by Obligation.
        ExitState = "ECO_LNK_LST_MTN_OB_S_C_SUPP";

        break;
      case "NEXT":
        ExitState = "ACO_NI0000_BOTTOM_OF_LIST";

        break;
      case "PREV":
        ExitState = "ACO_NI0000_TOP_OF_LIST";

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
      case "LIST":
        if (AsChar(export.AltAddrPrompt.SelectChar) == 'S')
        {
          export.AltAddrPrompt.SelectChar = "+";
          ExitState = "ECO_LNK_TO_CSE_NAME_LIST";

          return;
        }
        else
        {
          var field1 = GetField(export.AltAddrPrompt, "selectChar");

          field1.Error = true;

          ExitState = "ACO_NE0000_INVALID_SELECT_CODE1";
        }

        break;
      case "PEPR":
        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          break;
        }

        // --------------------------------------------------------------------------
        // WR# 000253 :  This new flow is added as part of the business rule.
        //                                                     
        // Vithal (12/14/2000)
        // --------------------------------------------------------------------------
        local.CountSelected.Count = 0;

        for(export.Export1.Index = 0; export.Export1.Index < export
          .Export1.Count; ++export.Export1.Index)
        {
          // -----------------------------------------------------------------------
          // Count the no. of supported persons selected. If more than one 
          // selected, display an error message.
          // ---------------------------------------------------------------------
          if (AsChar(export.Export1.Item.Common.SelectChar) == 'S')
          {
            ++local.CountSelected.Count;
          }
          else if (!IsEmpty(export.Export1.Item.Common.SelectChar))
          {
            var field1 = GetField(export.Export1.Item.Common, "selectChar");

            field1.Error = true;

            ExitState = "ACO_NE0000_INVALID_SELECT_CODE";
          }
        }

        switch(local.CountSelected.Count)
        {
          case 0:
            if (IsExitState("ACO_NN0000_ALL_OK"))
            {
              local.CountSelected.Count = 0;

              for(export.Export1.Index = 0; export.Export1.Index < export
                .Export1.Count; ++export.Export1.Index)
              {
                // ---------------------------------------------------------------------------------
                // Per WR# 253, user can flow to PEPR without selecting 
                // supported person, if only one supported person exists.
                // -------------------------------------------------------------------------------
                if (!IsEmpty(export.Export1.Item.SupportedCsePerson.Number))
                {
                  ++local.CountSelected.Count;
                  export.HiddenFlowToPeprCsePersonsWorkSet.Number =
                    export.Export1.Item.SupportedCsePerson.Number;
                }
              }

              if (local.CountSelected.Count > 1)
              {
                for(export.Export1.Index = 0; export.Export1.Index < export
                  .Export1.Count; ++export.Export1.Index)
                {
                  var field1 =
                    GetField(export.Export1.Item.Common, "selectChar");

                  field1.Error = true;

                  ExitState = "ACO_NE0000_NO_SELECTION_MADE";
                }

                export.HiddenFlowToPeprCsePersonsWorkSet.Number = "";
              }
              else
              {
                local.FlowToPepr.Flag = "Y";
              }
            }

            break;
          case 1:
            local.FlowToPepr.Flag = "Y";

            for(export.Export1.Index = 0; export.Export1.Index < export
              .Export1.Count; ++export.Export1.Index)
            {
              if (AsChar(export.Export1.Item.Common.SelectChar) == 'S')
              {
                export.Export1.Update.Common.SelectChar = "";
                export.HiddenFlowToPeprCsePersonsWorkSet.Number =
                  export.Export1.Item.SupportedCsePerson.Number;
              }
            }

            break;
          default:
            for(export.Export1.Index = 0; export.Export1.Index < export
              .Export1.Count; ++export.Export1.Index)
            {
              if (AsChar(export.Export1.Item.Common.SelectChar) == 'S')
              {
                var field1 = GetField(export.Export1.Item.Common, "selectChar");

                field1.Error = true;
              }
            }

            ExitState = "ACO_NE0000_INVALID_MULTIPLE_SEL";

            break;
        }

        if (AsChar(local.FlowToPepr.Flag) == 'Y')
        {
          // ---------------------------------------------------------------------------------
          // Case# needs to be passed to PEPR. It is mandatory. Find out the 
          // Case# to which the selected supported person associated and pass
          // the value to PEPR.
          // -------------------------------------------------------------------------------
          if (ReadCase())
          {
            export.HiddenFlowToPeprCase.Number = entities.Case1.Number;
            ExitState = "ECO_LNK_TO_PEPR";

            return;
          }
          else
          {
            ExitState = "CASE_NF";
          }
        }

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          // -----------------------------------------------------------------------------------
          // The following fields need to be protected if they are read /added/
          // updated  to/from the database.  First time when we display/add/
          // update  screen,  the values are moved to hidden_stored views. So
          // check the hidden_stored views  and protect them .
          // ---------------------------------------------------------------------------------
          if (!IsEmpty(export.HiddenStoredObligation.OrderTypeCode))
          {
            var field1 = GetField(export.Obligation, "orderTypeCode");

            field1.Color = "cyan";
            field1.Protected = true;
          }

          if (!IsEmpty(export.HiddenStoredObligation.OtherStateAbbr))
          {
            var field1 = GetField(export.Obligation, "otherStateAbbr");

            field1.Color = "cyan";
            field1.Protected = true;

            var field2 = GetField(export.InterstateRequest, "country");

            field2.Color = "cyan";
            field2.Protected = true;

            var field3 = GetField(export.InterstateRequest, "tribalAgency");

            field3.Color = "cyan";
            field3.Protected = true;
          }

          if (!IsEmpty(export.HiddenInterstateRequest.OtherStateCaseId))
          {
            var field1 = GetField(export.InterstateRequest, "otherStateCaseId");

            field1.Color = "cyan";
            field1.Protected = true;
          }

          if (!IsEmpty(export.HiddenInterstateRequest.Country))
          {
            var field1 = GetField(export.InterstateRequest, "country");

            field1.Color = "cyan";
            field1.Protected = true;

            var field2 = GetField(export.Obligation, "otherStateAbbr");

            field2.Color = "cyan";
            field2.Protected = true;

            var field3 = GetField(export.InterstateRequest, "tribalAgency");

            field3.Color = "cyan";
            field3.Protected = true;
          }

          if (!IsEmpty(export.HiddenInterstateRequest.TribalAgency))
          {
            var field1 = GetField(export.InterstateRequest, "country");

            field1.Color = "cyan";
            field1.Protected = true;

            var field2 = GetField(export.Obligation, "otherStateAbbr");

            field2.Color = "cyan";
            field2.Protected = true;

            var field3 = GetField(export.InterstateRequest, "tribalAgency");

            field3.Color = "cyan";
            field3.Protected = true;
          }

          if (!Equal(export.HiddenStoredDebtDetail.CoveredPrdStartDt,
            local.BlankDateWorkArea.Date) && !
            Equal(export.HiddenStoredDebtDetail.CoveredPrdEndDt,
            local.BlankDateWorkArea.Date))
          {
            var field1 = GetField(export.Header, "coveredPrdStartDt");

            field1.Color = "cyan";
            field1.Protected = true;

            var field2 = GetField(export.Header, "coveredPrdEndDt");

            field2.Color = "cyan";
            field2.Protected = true;
          }

          if (AsChar(export.HiddenStoredObligation.OrderTypeCode) == 'K')
          {
            var field1 = GetField(export.Obligation, "otherStateAbbr");

            field1.Color = "cyan";
            field1.Protected = true;

            var field2 = GetField(export.Obligation, "orderTypeCode");

            field2.Color = "cyan";
            field2.Protected = true;

            var field3 = GetField(export.InterstateRequest, "country");

            field3.Color = "cyan";
            field3.Protected = true;

            var field4 = GetField(export.InterstateRequest, "tribalAgency");

            field4.Color = "cyan";
            field4.Protected = true;

            var field5 = GetField(export.InterstateRequest, "otherStateCaseId");

            field5.Color = "cyan";
            field5.Protected = true;
          }
        }

        break;
      case "COLP":
        ExitState = "ECO_LNK_TO_COLP";

        break;
      default:
        // : If hidden command is CONFIRM, the user was asked to confirm
        //   an add action. Any key may be pressed to cancel the add.
        if (AsChar(local.ProgramFieldError.Flag) == 'Y')
        {
        }
        else
        {
          ExitState = "ACO_NE0000_INVALID_COMMAND";
        }

        // : Jan, 2002, M. Brown, Work Order # 020144
        //   Removed pre-conversion code protection.
        // -----------------------------------------------------------------------------------
        // The following fields need to be protected if they are read /added/
        // updated  to/from the database.  First time when we display/add/update
        // screen,  the values are moved to hidden_stored views. So check the
        // hidden_stored views  and protect them .
        // ---------------------------------------------------------------------------------
        if (!IsEmpty(export.HiddenStoredObligation.OrderTypeCode))
        {
          var field1 = GetField(export.Obligation, "orderTypeCode");

          field1.Color = "cyan";
          field1.Protected = true;
        }

        if (!IsEmpty(export.HiddenStoredObligation.OtherStateAbbr))
        {
          var field1 = GetField(export.Obligation, "otherStateAbbr");

          field1.Color = "cyan";
          field1.Protected = true;

          var field2 = GetField(export.InterstateRequest, "country");

          field2.Color = "cyan";
          field2.Protected = true;

          var field3 = GetField(export.InterstateRequest, "tribalAgency");

          field3.Color = "cyan";
          field3.Protected = true;
        }

        if (!IsEmpty(export.HiddenInterstateRequest.OtherStateCaseId))
        {
          var field1 = GetField(export.InterstateRequest, "otherStateCaseId");

          field1.Color = "cyan";
          field1.Protected = true;
        }

        if (!IsEmpty(export.HiddenInterstateRequest.Country))
        {
          var field1 = GetField(export.InterstateRequest, "country");

          field1.Color = "cyan";
          field1.Protected = true;

          var field2 = GetField(export.Obligation, "otherStateAbbr");

          field2.Color = "cyan";
          field2.Protected = true;

          var field3 = GetField(export.InterstateRequest, "tribalAgency");

          field3.Color = "cyan";
          field3.Protected = true;
        }

        if (!IsEmpty(export.HiddenInterstateRequest.TribalAgency))
        {
          var field1 = GetField(export.InterstateRequest, "country");

          field1.Color = "cyan";
          field1.Protected = true;

          var field2 = GetField(export.Obligation, "otherStateAbbr");

          field2.Color = "cyan";
          field2.Protected = true;

          var field3 = GetField(export.InterstateRequest, "tribalAgency");

          field3.Color = "cyan";
          field3.Protected = true;
        }

        if (!Equal(export.HiddenStoredDebtDetail.CoveredPrdStartDt,
          local.BlankDateWorkArea.Date) && !
          Equal(export.HiddenStoredDebtDetail.CoveredPrdEndDt,
          local.BlankDateWorkArea.Date))
        {
          var field1 = GetField(export.Header, "coveredPrdStartDt");

          field1.Color = "cyan";
          field1.Protected = true;

          var field2 = GetField(export.Header, "coveredPrdEndDt");

          field2.Color = "cyan";
          field2.Protected = true;
        }

        if (AsChar(export.HiddenStoredObligation.OrderTypeCode) == 'K')
        {
          var field1 = GetField(export.Obligation, "otherStateAbbr");

          field1.Color = "cyan";
          field1.Protected = true;

          var field2 = GetField(export.Obligation, "orderTypeCode");

          field2.Color = "cyan";
          field2.Protected = true;

          var field3 = GetField(export.InterstateRequest, "country");

          field3.Color = "cyan";
          field3.Protected = true;

          var field4 = GetField(export.InterstateRequest, "tribalAgency");

          field4.Color = "cyan";
          field4.Protected = true;

          var field5 = GetField(export.InterstateRequest, "otherStateCaseId");

          field5.Color = "cyan";
          field5.Protected = true;
        }

        break;
    }

Test5:

    if (AsChar(local.ProtectAll.Flag) == 'Y')
    {
      var field1 = GetField(export.Header, "coveredPrdStartDt");

      field1.Color = "cyan";
      field1.Protected = true;

      var field2 = GetField(export.Header, "coveredPrdEndDt");

      field2.Color = "cyan";
      field2.Protected = true;
    }

    if (IsEmpty(export.Obligation.OrderTypeCode))
    {
      export.Obligation.OrderTypeCode = "K";
    }

    // <<< RBM   12/30/1997 >>>
    //    If the ADD, UPDATE or DELETE operations were not successful, perform a
    // CICS ROLLBACK
    if (Equal(global.Command, "ADD") && !
      IsExitState("ACO_NI0000_CREATED_OK") || Equal
      (global.Command, "UPDATE") && !
      IsExitState("ACO_NI0000_SUCCESSFUL_UPDATE") || Equal
      (global.Command, "DELETE") && !
      IsExitState("ACO_NI0000_SUCCESSFUL_DELETE"))
    {
      // <<< Call to Fn_Eab_Rollback_CICS >>>
      UseEabRollbackCics();
    }

    // =================================================
    // 2/17/1999 - bud adams  -  If an alternate address has been
    //   set via LACT, then it cannot be changed by debt screens.
    // =================================================
    if (Equal(export.AltAddress.Char2, "LE"))
    {
      export.AltAddrPrompt.SelectChar = "";

      var field1 = GetField(export.AltAddress, "number");

      field1.Color = "cyan";
      field1.Protected = true;

      var field2 = GetField(export.AltAddrPrompt, "selectChar");

      field2.Color = "cyan";
      field2.Protected = true;
    }

    if (!Equal(export.Header.CoveredPrdStartDt, local.BlankDateWorkArea.Date) &&
      !Equal(export.Header.CoveredPrdEndDt, local.BlankDateWorkArea.Date))
    {
      // --------------------------------------------------------------
      // COMMAND will be set to spaces if the data entered in Sel and Prog 
      // fields is wrong while ADDing .
      // -----------------------------------------------------------------
      if (!IsEmpty(global.Command))
      {
        var field1 = GetField(export.Header, "coveredPrdStartDt");

        field1.Color = "cyan";
        field1.Protected = true;

        var field2 = GetField(export.Header, "coveredPrdEndDt");

        field2.Color = "cyan";
        field2.Protected = true;
      }
    }

    // ==============================================
    // 04/15/2000 - K. Price
    // SI_READ_CSE_PERSON calls ADABAS to get the first,
    // middle initial, and last name for a person.  When debugging
    // in trace mode, it will abend when calling this action block.
    // All calls to SI_READ_CSE_PERSON have been moved here
    // to facilitate disabling the calls quickly, and safely.
    // ===============================================
    if (!IsEmpty(export.ObligorCsePerson.Number) && !
      Equal(export.ObligorCsePerson.Number, export.HiddenObligor.Number))
    {
      export.ObligorCsePersonsWorkSet.Number = export.ObligorCsePerson.Number;
      UseSiReadCsePerson2();

      if (IsEmpty(export.ObligorCsePersonsWorkSet.FormattedName))
      {
        export.ObligorCsePersonsWorkSet.FormattedName =
          TrimEnd(export.ObligorCsePersonsWorkSet.LastName) + ", " + TrimEnd
          (export.ObligorCsePersonsWorkSet.FirstName) + " " + export
          .ObligorCsePersonsWorkSet.MiddleInitial;
      }

      if (!IsEmpty(local.Eab.Type1))
      {
        var field1 = GetField(export.ObligorCsePerson, "number");

        field1.Error = true;

        ExitState = "CSE_PERSON_NF_ON_EXTERNAL_RB";
      }
    }

    export.HiddenObligor.Number = export.ObligorCsePerson.Number;

    if (!IsEmpty(export.ConcurrentCsePerson.Number) && !
      Equal(export.ConcurrentCsePerson.Number,
      export.HiddenConcurrentCsePerson.Number))
    {
      export.ConcurrentCsePersonsWorkSet.Number =
        export.ConcurrentCsePerson.Number;
      UseSiReadCsePerson3();

      if (IsEmpty(export.ConcurrentCsePersonsWorkSet.FormattedName))
      {
        export.ConcurrentCsePersonsWorkSet.FormattedName =
          TrimEnd(export.ConcurrentCsePersonsWorkSet.LastName) + ", " + TrimEnd
          (export.ConcurrentCsePersonsWorkSet.FirstName) + " " + export
          .ConcurrentCsePersonsWorkSet.MiddleInitial;
      }

      if (!IsEmpty(local.Eab.Type1))
      {
        var field1 = GetField(export.ConcurrentCsePerson, "number");

        field1.Error = true;

        ExitState = "CSE_PERSON_NF_ON_EXTERNAL_RB";
      }
    }

    export.HiddenConcurrentCsePerson.Number = export.ConcurrentCsePerson.Number;

    if (!IsEmpty(export.AltAddress.Number) && !
      Equal(export.AltAddress.Number, export.HiddenAltAddress.Number))
    {
      UseSiReadCsePerson1();

      if (IsEmpty(export.AltAddress.FormattedName))
      {
        export.AltAddress.FormattedName =
          TrimEnd(export.AltAddress.LastName) + ", " + TrimEnd
          (export.AltAddress.FirstName) + " " + export
          .AltAddress.MiddleInitial;
      }

      if (!IsEmpty(local.Eab.Type1))
      {
        var field1 = GetField(export.AltAddress, "number");

        field1.Error = true;

        ExitState = "CSE_PERSON_NF_ON_EXTERNAL_RB";
      }
    }

    // -------------------------------------------------------------------------------------
    // After executing the 'FN_READ_SUPPORT_OBLIGATION' and '
    // FN_READ_LEGAL_ACTION_INFO'  in CASE of DISPLAY,  the 'Export_Alt_Address
    // cse_person_work_set  Formatted_ Name' will be overwritten.  So reset the
    // name with the hidden_value.
    //                                                             
    // Vithal(05/09/2000)
    // -----------------------------------------------------------------------------------
    if (Equal(export.HiddenAltAddress.Number, export.AltAddress.Number))
    {
      export.AltAddress.FormattedName = export.HiddenAltAddress.FormattedName;
    }

    export.HiddenAltAddress.Assign(export.AltAddress);

    // : Jan, 2002, M. Brown, Work Order # 020144
    //   Removed pre-conversion code protection.
    for(export.Export1.Index = 0; export.Export1.Index < export.Export1.Count; ++
      export.Export1.Index)
    {
      // =================================================
      // 5/28/99 - bud adams  -  Don't display max dates.
      // =================================================
      if (Equal(export.Export1.Item.DebtDetail.RetiredDt, local.Max.Date))
      {
        export.Export1.Update.DebtDetail.RetiredDt = local.Current.Date;
      }

      if (!IsEmpty(export.Export1.Item.SupportedCsePerson.Number) && !
        Equal(export.Export1.Item.SupportedCsePerson.Number,
        export.Export1.Item.SupportedCsePersonsWorkSet.Number))
      {
        local.Supported.Number = export.Export1.Item.SupportedCsePerson.Number;
        UseSiReadCsePerson4();
        MoveCsePersonsWorkSet1(local.Supported,
          export.Export1.Update.SupportedCsePersonsWorkSet);

        if (IsEmpty(local.Supported.FormattedName))
        {
          export.Export1.Update.SupportedCsePersonsWorkSet.FormattedName =
            TrimEnd(local.Supported.FirstName) + " " + TrimEnd
            (local.Supported.MiddleInitial) + " " + local.Supported.LastName;
        }

        if (!IsEmpty(local.Eab.Type1))
        {
          var field1 =
            GetField(export.Export1.Item.SupportedCsePerson, "number");

          field1.Error = true;

          ExitState = "CSE_PERSON_NF_ON_EXTERNAL_RB";
        }
      }
    }
  }

  private static void MoveCodeValue(CodeValue source, CodeValue target)
  {
    target.Cdvalue = source.Cdvalue;
    target.Description = source.Description;
  }

  private static void MoveCsePersonsWorkSet1(CsePersonsWorkSet source,
    CsePersonsWorkSet target)
  {
    target.Number = source.Number;
    target.Dob = source.Dob;
    target.FormattedName = source.FormattedName;
  }

  private static void MoveCsePersonsWorkSet2(CsePersonsWorkSet source,
    CsePersonsWorkSet target)
  {
    target.Number = source.Number;
    target.FirstName = source.FirstName;
    target.MiddleInitial = source.MiddleInitial;
    target.FormattedName = source.FormattedName;
    target.LastName = source.LastName;
  }

  private static void MoveCsePersonsWorkSet3(CsePersonsWorkSet source,
    CsePersonsWorkSet target)
  {
    target.Number = source.Number;
    target.FormattedName = source.FormattedName;
  }

  private static void MoveCsePersonsWorkSet4(CsePersonsWorkSet source,
    CsePersonsWorkSet target)
  {
    target.Number = source.Number;
    target.FormattedName = source.FormattedName;
    target.Flag = source.Flag;
  }

  private static void MoveCsePersonsWorkSet5(CsePersonsWorkSet source,
    CsePersonsWorkSet target)
  {
    target.Number = source.Number;
    target.FormattedName = source.FormattedName;
    target.Char2 = source.Char2;
  }

  private static void MoveDateWorkArea(DateWorkArea source, DateWorkArea target)
  {
    target.Date = source.Date;
    target.Timestamp = source.Timestamp;
  }

  private static void MoveDebtDetail1(DebtDetail source, DebtDetail target)
  {
    target.DueDt = source.DueDt;
    target.BalanceDueAmt = source.BalanceDueAmt;
    target.InterestBalanceDueAmt = source.InterestBalanceDueAmt;
    target.AdcDt = source.AdcDt;
    target.RetiredDt = source.RetiredDt;
    target.CoveredPrdStartDt = source.CoveredPrdStartDt;
    target.CoveredPrdEndDt = source.CoveredPrdEndDt;
  }

  private static void MoveDebtDetail2(DebtDetail source, DebtDetail target)
  {
    target.DueDt = source.DueDt;
    target.RetiredDt = source.RetiredDt;
    target.CoveredPrdStartDt = source.CoveredPrdStartDt;
    target.CoveredPrdEndDt = source.CoveredPrdEndDt;
  }

  private static void MoveDebtDetail3(DebtDetail source, DebtDetail target)
  {
    target.DueDt = source.DueDt;
    target.CoveredPrdStartDt = source.CoveredPrdStartDt;
    target.CoveredPrdEndDt = source.CoveredPrdEndDt;
  }

  private static void MoveDebtDetail4(DebtDetail source, DebtDetail target)
  {
    target.CoveredPrdStartDt = source.CoveredPrdStartDt;
    target.CoveredPrdEndDt = source.CoveredPrdEndDt;
  }

  private static void MoveExport2(FnReadSupportObligation.Export.
    ExportGroup source, Export.ExportGroup target)
  {
    System.Diagnostics.Trace.TraceWarning(
      "INFO: source and target of the move do not fit perfectly.");
    target.ZdelExportGrpDetail.Code = source.ZdelExportGrpDetail.Code;
    target.SupportedCsePerson.Number = source.SupportedCsePerson.Number;
    MoveCsePersonsWorkSet4(source.SupportedCsePersonsWorkSet,
      target.SupportedCsePersonsWorkSet);
    target.Case1.Number = source.Case1.Number;
    MoveServiceProvider(source.ServiceProvider, target.ServiceProvider);
    target.ObligationPaymentSchedule.EndDt =
      source.ObligationPaymentSchedule.EndDt;
  }

  private static void MoveExport3(FnReadLegalActionInfo.Export.
    ExportGroup source, Export.ExportGroup target)
  {
    target.ZdelExportGrpDetail.Code = source.Zdel.Code;
    target.SupportedCsePerson.Number = source.SupportedCsePerson.Number;
    target.SupportedCsePersonsWorkSet.Assign(source.SupportedCsePersonsWorkSet);
    target.Case1.Number = source.Case1.Number;
    target.ObligationTransaction.Assign(source.ObligationTransaction);
    MoveDebtDetail1(source.DebtDetail, target.DebtDetail);
    MoveServiceProvider(source.ServiceProvider, target.ServiceProvider);
    target.ObligationPaymentSchedule.EndDt =
      source.ObligationPaymentSchedule.EndDt;
    target.Concurrent.SystemGeneratedIdentifier =
      source.Concurrent.SystemGeneratedIdentifier;
    target.Prev.Amount = source.Prev.Amount;
    target.Common.SelectChar = source.Common.SelectChar;
    target.Hidden.PreconversionProgramCode =
      source.Hidden.PreconversionProgramCode;
  }

  private static void MoveExport1ToImport1(Export.ExportGroup source,
    FnUpdateNonAccruingOblig.Import.ImportGroup target)
  {
    target.ZdelImportGrpDetail.Code = source.ZdelExportGrpDetail.Code;
    target.SupportedCsePerson.Number = source.SupportedCsePerson.Number;
    target.SupportedCsePersonsWorkSet.Assign(source.SupportedCsePersonsWorkSet);
    target.Case1.Number = source.Case1.Number;
    target.ObligationTransaction.Assign(source.ObligationTransaction);
    target.DebtDetail.Assign(source.DebtDetail);
    MoveServiceProvider(source.ServiceProvider, target.ServiceProvider);
    target.ObligationPaymentSchedule.EndDt =
      source.ObligationPaymentSchedule.EndDt;
    target.Concurrent.SystemGeneratedIdentifier =
      source.Concurrent.SystemGeneratedIdentifier;
    target.Prev.Amount = source.Prev.Amount;
    target.Common.SelectChar = source.Common.SelectChar;
    target.Hidden.PreconversionProgramCode =
      source.Hidden.PreconversionProgramCode;
  }

  private static void MoveLegalAction1(LegalAction source, LegalAction target)
  {
    target.Identifier = source.Identifier;
    target.Classification = source.Classification;
    target.Type1 = source.Type1;
    target.FiledDate = source.FiledDate;
    target.CourtCaseNumber = source.CourtCaseNumber;
    target.StandardNumber = source.StandardNumber;
  }

  private static void MoveLegalAction2(LegalAction source, LegalAction target)
  {
    target.Identifier = source.Identifier;
    target.Classification = source.Classification;
    target.CourtCaseNumber = source.CourtCaseNumber;
    target.StandardNumber = source.StandardNumber;
  }

  private static void MoveLegalAction3(LegalAction source, LegalAction target)
  {
    target.Identifier = source.Identifier;
    target.Type1 = source.Type1;
    target.FiledDate = source.FiledDate;
    target.CourtCaseNumber = source.CourtCaseNumber;
    target.StandardNumber = source.StandardNumber;
  }

  private static void MoveLegalAction4(LegalAction source, LegalAction target)
  {
    target.Identifier = source.Identifier;
    target.StandardNumber = source.StandardNumber;
  }

  private static void MoveLegalActionDetail(LegalActionDetail source,
    LegalActionDetail target)
  {
    target.FreqPeriodCode = source.FreqPeriodCode;
    target.DayOfWeek = source.DayOfWeek;
    target.DayOfMonth1 = source.DayOfMonth1;
    target.DayOfMonth2 = source.DayOfMonth2;
    target.PeriodInd = source.PeriodInd;
    target.Number = source.Number;
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
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Description = source.Description;
  }

  private static void MoveObligation2(Obligation source, Obligation target)
  {
    target.OtherStateAbbr = source.OtherStateAbbr;
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Description = source.Description;
    target.HistoryInd = source.HistoryInd;
    target.PrimarySecondaryCode = source.PrimarySecondaryCode;
    target.CreatedBy = source.CreatedBy;
    target.CreatedTmst = source.CreatedTmst;
  }

  private static void MoveObligation3(Obligation source, Obligation target)
  {
    target.OtherStateAbbr = source.OtherStateAbbr;
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
    target.OtherStateAbbr = source.OtherStateAbbr;
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Description = source.Description;
    target.HistoryInd = source.HistoryInd;
    target.OrderTypeCode = source.OrderTypeCode;
  }

  private static void MoveObligation5(Obligation source, Obligation target)
  {
    target.OtherStateAbbr = source.OtherStateAbbr;
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Description = source.Description;
    target.OrderTypeCode = source.OrderTypeCode;
  }

  private static void MoveObligation6(Obligation source, Obligation target)
  {
    target.OtherStateAbbr = source.OtherStateAbbr;
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.OrderTypeCode = source.OrderTypeCode;
  }

  private static void MoveObligation7(Obligation source, Obligation target)
  {
    target.OtherStateAbbr = source.OtherStateAbbr;
    target.Description = source.Description;
    target.HistoryInd = source.HistoryInd;
    target.PrimarySecondaryCode = source.PrimarySecondaryCode;
    target.OrderTypeCode = source.OrderTypeCode;
  }

  private static void MoveObligation8(Obligation source, Obligation target)
  {
    target.OtherStateAbbr = source.OtherStateAbbr;
    target.Description = source.Description;
    target.OrderTypeCode = source.OrderTypeCode;
  }

  private static void MoveObligation9(Obligation source, Obligation target)
  {
    target.OtherStateAbbr = source.OtherStateAbbr;
    target.OrderTypeCode = source.OrderTypeCode;
  }

  private static void MoveObligation10(Obligation source, Obligation target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Description = source.Description;
  }

  private static void MoveObligation11(Obligation source, Obligation target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Description = source.Description;
    target.HistoryInd = source.HistoryInd;
    target.PrimarySecondaryCode = source.PrimarySecondaryCode;
    target.AsOfDtNadArrBal = source.AsOfDtNadArrBal;
    target.AsOfDtNadIntBal = source.AsOfDtNadIntBal;
    target.AsOfDtAdcArrBal = source.AsOfDtAdcArrBal;
    target.AsOfDtAdcIntBal = source.AsOfDtAdcIntBal;
    target.AsOfDtRecBal = source.AsOfDtRecBal;
    target.AsOdDtRecIntBal = source.AsOdDtRecIntBal;
    target.AsOfDtFeeBal = source.AsOfDtFeeBal;
    target.AsOfDtFeeIntBal = source.AsOfDtFeeIntBal;
    target.AsOfDtTotBalCurrArr = source.AsOfDtTotBalCurrArr;
    target.CreatedBy = source.CreatedBy;
    target.CreatedTmst = source.CreatedTmst;
    target.LastUpdatedBy = source.LastUpdatedBy;
    target.LastUpdateTmst = source.LastUpdateTmst;
  }

  private static void MoveObligation12(Obligation source, Obligation target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Description = source.Description;
    target.HistoryInd = source.HistoryInd;
    target.PrimarySecondaryCode = source.PrimarySecondaryCode;
    target.CreatedTmst = source.CreatedTmst;
  }

  private static void MoveObligation13(Obligation source, Obligation target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.CreatedTmst = source.CreatedTmst;
  }

  private static void MoveObligation14(Obligation source, Obligation target)
  {
    target.LastUpdatedBy = source.LastUpdatedBy;
    target.LastUpdateTmst = source.LastUpdateTmst;
  }

  private static void MoveObligationPaymentSchedule(
    ObligationPaymentSchedule source, ObligationPaymentSchedule target)
  {
    target.FrequencyCode = source.FrequencyCode;
    target.Amount = source.Amount;
  }

  private static void MoveObligationTransaction1(ObligationTransaction source,
    ObligationTransaction target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Type1 = source.Type1;
  }

  private static void MoveObligationTransaction2(ObligationTransaction source,
    ObligationTransaction target)
  {
    target.Type1 = source.Type1;
    target.DebtType = source.DebtType;
  }

  private static void MoveObligationType1(ObligationType source,
    ObligationType target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Code = source.Code;
  }

  private static void MoveObligationType2(ObligationType source,
    ObligationType target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Classification = source.Classification;
  }

  private static void MoveServiceProvider(ServiceProvider source,
    ServiceProvider target)
  {
    target.SystemGeneratedId = source.SystemGeneratedId;
    target.UserId = source.UserId;
  }

  private void UseCabSetMaximumDiscontinueDate1()
  {
    var useImport = new CabSetMaximumDiscontinueDate.Import();
    var useExport = new CabSetMaximumDiscontinueDate.Export();

    Call(CabSetMaximumDiscontinueDate.Execute, useImport, useExport);

    local.Max.Date = useExport.DateWorkArea.Date;
  }

  private DateTime? UseCabSetMaximumDiscontinueDate2()
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

  private void UseFnAbDetIfObExistsForLa1()
  {
    var useImport = new FnAbDetIfObExistsForLa.Import();
    var useExport = new FnAbDetIfObExistsForLa.Export();

    useImport.HcLapSupportedAcctTyp.AccountType =
      local.HardcodeSupported.AccountType;
    useImport.HcLapObligorAcctType.AccountType =
      local.HardcodeObligorLegalActionPerson.AccountType;
    useImport.HcOtrnTDebt.Type1 = local.HardcodeOtrnTDebt.Type1;
    MoveObligationTransaction2(local.HardcodeOtrnDtDebtDetail,
      useImport.HcOtrnDtDebtDetail);
    useImport.LegalAction.Identifier = import.LegalAction.Identifier;
    useImport.LegalActionDetail.Number = import.LegalActionDetail.Number;
    useImport.CsePerson.Number = import.ObligorCsePerson.Number;
    useImport.DebtDetail.Assign(import.Header);
    MoveObligationType1(import.ObligationType, useImport.ObligationType);

    Call(FnAbDetIfObExistsForLa.Execute, useImport, useExport);

    local.ObExists.Flag = useExport.ActiveObligationFound.Flag;
    export.ObligationType.Assign(useExport.ObligationType);
  }

  private void UseFnAbDetIfObExistsForLa2()
  {
    var useImport = new FnAbDetIfObExistsForLa.Import();
    var useExport = new FnAbDetIfObExistsForLa.Export();

    useImport.HcLapSupportedAcctTyp.AccountType =
      local.HardcodeSupported.AccountType;
    useImport.HcLapObligorAcctType.AccountType =
      local.HardcodeObligorLegalActionPerson.AccountType;
    useImport.HcOtrnTDebt.Type1 = local.HardcodeOtrnTDebt.Type1;
    MoveObligationTransaction2(local.HardcodeOtrnDtDebtDetail,
      useImport.HcOtrnDtDebtDetail);
    useImport.DebtDetail.Assign(export.Header);
    useImport.LegalActionDetail.Number = export.LegalActionDetail.Number;
    useImport.LegalAction.Identifier = export.LegalAction.Identifier;
    MoveObligationType1(export.ObligationType, useImport.ObligationType);
    useImport.CsePerson.Number = export.ObligorCsePerson.Number;
    useImport.Obligation.SystemGeneratedIdentifier =
      export.Obligation.SystemGeneratedIdentifier;

    Call(FnAbDetIfObExistsForLa.Execute, useImport, useExport);

    export.LegalActionDetail.Assign(useExport.LegalActionDetail);
    local.ObExists.Flag = useExport.ActiveObligationFound.Flag;
    export.ObligationType.Assign(useExport.ObligationType);
    MoveObligation13(useExport.Obligation, export.Obligation);
    export.ObligorCsePerson.Number = useExport.Obligor.Number;
  }

  private void UseFnCabCheckAltAddr()
  {
    var useImport = new FnCabCheckAltAddr.Import();
    var useExport = new FnCabCheckAltAddr.Export();

    useImport.Current.Date = local.Current.Date;
    useImport.Alternate.Number = export.AltAddress.Number;

    Call(FnCabCheckAltAddr.Execute, useImport, useExport);
  }

  private void UseFnCabRaiseEvent1()
  {
    var useImport = new FnCabRaiseEvent.Import();
    var useExport = new FnCabRaiseEvent.Export();

    useImport.Infrastructure.Assign(local.Infrastructure);
    useImport.Obligation.SystemGeneratedIdentifier =
      export.Obligation.SystemGeneratedIdentifier;
    useImport.Supported.Number = export.Export1.Item.SupportedCsePerson.Number;
    MoveObligationTransaction1(export.Export1.Item.ObligationTransaction,
      useImport.ObligationTransaction);
    useImport.Current.Timestamp = local.Current.Timestamp;
    useImport.ObligationType.Assign(export.ObligationType);
    useImport.LegalAction.Identifier = export.LegalAction.Identifier;

    Call(FnCabRaiseEvent.Execute, useImport, useExport);
  }

  private void UseFnCabRaiseEvent2()
  {
    var useImport = new FnCabRaiseEvent.Import();
    var useExport = new FnCabRaiseEvent.Export();

    useImport.Infrastructure.Assign(local.Infrastructure);
    useImport.Obligation.SystemGeneratedIdentifier =
      export.ConcurrentObligation.SystemGeneratedIdentifier;
    useImport.ObligationType.Assign(export.ObligationType);
    useImport.Supported.Number = export.Export1.Item.SupportedCsePerson.Number;
    useImport.ObligationTransaction.SystemGeneratedIdentifier =
      export.Export1.Item.Concurrent.SystemGeneratedIdentifier;
    useImport.Current.Timestamp = local.Current.Timestamp;
    useImport.LegalAction.Identifier = export.LegalAction.Identifier;

    Call(FnCabRaiseEvent.Execute, useImport, useExport);
  }

  private void UseFnCheckObligationForActivity()
  {
    var useImport = new FnCheckObligationForActivity.Import();
    var useExport = new FnCheckObligationForActivity.Export();

    useImport.HcOtrrConcurrentObliga.SystemGeneratedIdentifier =
      local.HardcodeOtrrConcurrentObliga.SystemGeneratedIdentifier;
    useImport.HcCpaObligor.Type1 = local.HardcodeObligorCsePersonAccount.Type1;
    useImport.Obligor.Number = export.ObligorCsePerson.Number;
    useImport.Obligation.SystemGeneratedIdentifier =
      export.Obligation.SystemGeneratedIdentifier;
    useImport.ObligationType.SystemGeneratedIdentifier =
      export.ObligationType.SystemGeneratedIdentifier;

    Call(FnCheckObligationForActivity.Execute, useImport, useExport);

    export.ObligationActive.Flag = useExport.ActiveObligation.Flag;
  }

  private void UseFnCreateInterstateRqstOblign1()
  {
    var useImport = new FnCreateInterstateRqstOblign.Import();
    var useExport = new FnCreateInterstateRqstOblign.Export();

    useImport.Current.Date = local.Current.Date;
    useImport.CsePersonAccount.Type1 =
      local.HardcodeObligorCsePersonAccount.Type1;
    useImport.Max.Date = local.Max.Date;
    useImport.CsePerson.Number = export.ObligorCsePerson.Number;
    useImport.Obligation.SystemGeneratedIdentifier =
      export.Obligation.SystemGeneratedIdentifier;
    useImport.ObligationType.SystemGeneratedIdentifier =
      export.ObligationType.SystemGeneratedIdentifier;
    useImport.InterstateRequest.IntHGeneratedId =
      export.InterstateRequest.IntHGeneratedId;

    Call(FnCreateInterstateRqstOblign.Execute, useImport, useExport);
  }

  private void UseFnCreateInterstateRqstOblign2()
  {
    var useImport = new FnCreateInterstateRqstOblign.Import();
    var useExport = new FnCreateInterstateRqstOblign.Export();

    useImport.Current.Date = local.Current.Date;
    useImport.CsePersonAccount.Type1 =
      local.HardcodeObligorCsePersonAccount.Type1;
    useImport.Max.Date = local.Max.Date;
    useImport.ObligationType.SystemGeneratedIdentifier =
      export.ObligationType.SystemGeneratedIdentifier;
    useImport.InterstateRequest.IntHGeneratedId =
      export.InterstateRequest.IntHGeneratedId;
    useImport.CsePerson.Number = export.ConcurrentCsePerson.Number;
    useImport.Obligation.SystemGeneratedIdentifier =
      export.ConcurrentObligation.SystemGeneratedIdentifier;

    Call(FnCreateInterstateRqstOblign.Execute, useImport, useExport);
  }

  private void UseFnCreateObligation1()
  {
    var useImport = new FnCreateObligation.Import();
    var useExport = new FnCreateObligation.Export();

    useImport.HcOtCFeesClassificati.Classification =
      local.HardcodeOtCFeesClassificatio.Classification;
    MoveDateWorkArea(local.Current, useImport.Current);
    useImport.HcOtCAccruingClassifi.Classification =
      local.HardcodeAccruing.Classification;
    useImport.HcOtCVoluntaryClassif.Classification =
      local.HardcodedVoluntary.Classification;
    useImport.HcOtCRecoverClassific.Classification =
      local.HardcodedRecovery.Classification;
    useImport.HardcodeCpaObligor.Type1 =
      local.HardcodeObligorCsePersonAccount.Type1;
    useImport.Max.Date = local.Max.Date;
    MoveObligation7(export.Obligation, useImport.Obligation);
    useImport.ObligationPaymentSchedule.
      Assign(export.ObligationPaymentSchedule);
    useImport.ObligationType.Assign(export.ObligationType);
    useImport.LegalAction.Identifier = import.LegalAction.Identifier;
    useImport.LegalActionDetail.Number = import.LegalActionDetail.Number;
    useImport.CsePerson.Number = import.ObligorCsePerson.Number;

    Call(FnCreateObligation.Execute, useImport, useExport);

    MoveObligation2(useExport.Obligation, export.Obligation);
  }

  private void UseFnCreateObligation2()
  {
    var useImport = new FnCreateObligation.Import();
    var useExport = new FnCreateObligation.Export();

    useImport.HcOtCFeesClassificati.Classification =
      local.HardcodeOtCFeesClassificatio.Classification;
    MoveDateWorkArea(local.Current, useImport.Current);
    useImport.HcOtCAccruingClassifi.Classification =
      local.HardcodeAccruing.Classification;
    useImport.HcOtCVoluntaryClassif.Classification =
      local.HardcodedVoluntary.Classification;
    useImport.HcOtCRecoverClassific.Classification =
      local.HardcodedRecovery.Classification;
    useImport.HardcodeCpaObligor.Type1 =
      local.HardcodeObligorCsePersonAccount.Type1;
    useImport.Max.Date = local.Max.Date;
    useImport.ObligationPaymentSchedule.
      Assign(export.ObligationPaymentSchedule);
    useImport.ObligationType.Assign(export.ObligationType);
    useImport.LegalActionDetail.Number = export.LegalActionDetail.Number;
    useImport.CsePerson.Number = export.ConcurrentCsePerson.Number;
    MoveObligation7(export.Obligation, useImport.Obligation);
    useImport.LegalAction.Identifier = export.LegalAction.Identifier;

    Call(FnCreateObligation.Execute, useImport, useExport);

    MoveObligation1(useExport.Obligation, export.ConcurrentObligation);
  }

  private void UseFnCreateObligationTranRln()
  {
    var useImport = new FnCreateObligationTranRln.Import();
    var useExport = new FnCreateObligationTranRln.Export();

    useImport.OtrrConcurrentObligatio.SystemGeneratedIdentifier =
      local.HardcodeOtrrConcurrentObliga.SystemGeneratedIdentifier;
    useImport.Current.Timestamp = local.Current.Timestamp;
    useImport.CpaObligor.Type1 = local.HardcodeObligorCsePersonAccount.Type1;
    useImport.Obligation.SystemGeneratedIdentifier =
      export.Obligation.SystemGeneratedIdentifier;
    useImport.ConcurrentObligation.SystemGeneratedIdentifier =
      export.ConcurrentObligation.SystemGeneratedIdentifier;
    useImport.ObligationTransaction.SystemGeneratedIdentifier =
      export.Export1.Item.ObligationTransaction.SystemGeneratedIdentifier;
    useImport.ConcurrentObligationTransaction.SystemGeneratedIdentifier =
      export.Export1.Item.Concurrent.SystemGeneratedIdentifier;
    useImport.CsePerson.Number = export.ObligorCsePerson.Number;
    useImport.ConcurrentCsePerson.Number = export.ConcurrentCsePerson.Number;

    Call(FnCreateObligationTranRln.Execute, useImport, useExport);
  }

  private void UseFnCreateObligationTransaction1()
  {
    var useImport = new FnCreateObligationTransaction.Import();
    var useExport = new FnCreateObligationTransaction.Export();

    useImport.HardcodeObligorLap.AccountType =
      local.HardcodeObligorLegalActionPerson.AccountType;
    useImport.HcOt718BUraJudgement.SystemGeneratedIdentifier =
      local.HardcodeOt718BUraJudgement.SystemGeneratedIdentifier;
    MoveObligationTransaction2(local.HardcodeOtrnDtVoluntary,
      useImport.HcOtrnDtVoluntary);
    MoveObligationTransaction2(local.HardcodeOtrnDtAccrual,
      useImport.HcOtrnDtAccrual);
    useImport.HcOtCFeesClassificati.Classification =
      local.HardcodeOtCFeesClassificatio.Classification;
    MoveObligationTransaction2(local.HardcodeOtrnDtDebtDetail,
      useImport.HcOtrnDtDebtDetail);
    MoveObligationTransaction2(local.HardcodeOtrnDtDebtDetail,
      useImport.Hardcoded);
    useImport.HcCpaSupportedPerson.Type1 =
      local.HardcodeCpaSupportedPerson.Type1;
    useImport.HcDdshActiveStatus.Code = local.HardcodeDdshActiveStatus.Code;
    MoveDateWorkArea(local.Current, useImport.Current);
    useImport.HcOtCVoluntaryClassif.Classification =
      local.HardcodedVoluntary.Classification;
    useImport.HcOtCRecoverClassific.Classification =
      local.HardcodedRecovery.Classification;
    useImport.HcOtrnTDebt.Type1 = local.HardcodeOtrnTDebt.Type1;
    useImport.HcCpaObligor.Type1 = local.HardcodeObligorCsePersonAccount.Type1;
    useImport.Max.Date = local.Max.Date;
    useImport.Supported.Number = export.Export1.Item.SupportedCsePerson.Number;
    useImport.ObligationTransaction.Assign(
      export.Export1.Item.ObligationTransaction);
    useImport.DebtDetail.Assign(export.Export1.Item.DebtDetail);
    useImport.ObligationPaymentSchedule.EndDt =
      export.Export1.Item.ObligationPaymentSchedule.EndDt;
    useImport.LegalActionDetail.Number = export.LegalActionDetail.Number;
    useImport.Obligor.Number = export.ObligorCsePerson.Number;
    useImport.Obligation.SystemGeneratedIdentifier =
      export.Obligation.SystemGeneratedIdentifier;
    MoveObligationType2(export.ObligationType, useImport.ObligationType);
    MoveLegalAction4(export.LegalAction, useImport.LegalAction);

    Call(FnCreateObligationTransaction.Execute, useImport, useExport);

    export.Export1.Update.ObligationTransaction.SystemGeneratedIdentifier =
      useExport.ObligationTransaction.SystemGeneratedIdentifier;
  }

  private void UseFnCreateObligationTransaction2()
  {
    var useImport = new FnCreateObligationTransaction.Import();
    var useExport = new FnCreateObligationTransaction.Export();

    useImport.HardcodeObligorLap.AccountType =
      local.HardcodeObligorLegalActionPerson.AccountType;
    useImport.HcOt718BUraJudgement.SystemGeneratedIdentifier =
      local.HardcodeOt718BUraJudgement.SystemGeneratedIdentifier;
    MoveObligationTransaction2(local.HardcodeOtrnDtVoluntary,
      useImport.HcOtrnDtVoluntary);
    MoveObligationTransaction2(local.HardcodeOtrnDtAccrual,
      useImport.HcOtrnDtAccrual);
    useImport.HcOtCFeesClassificati.Classification =
      local.HardcodeOtCFeesClassificatio.Classification;
    MoveObligationTransaction2(local.HardcodeOtrnDtDebtDetail,
      useImport.HcOtrnDtDebtDetail);
    MoveObligationTransaction2(local.HardcodeOtrnDtDebtDetail,
      useImport.Hardcoded);
    useImport.HcCpaSupportedPerson.Type1 =
      local.HardcodeCpaSupportedPerson.Type1;
    useImport.HcDdshActiveStatus.Code = local.HardcodeDdshActiveStatus.Code;
    MoveDateWorkArea(local.Current, useImport.Current);
    useImport.HcOtCVoluntaryClassif.Classification =
      local.HardcodedVoluntary.Classification;
    useImport.HcOtCRecoverClassific.Classification =
      local.HardcodedRecovery.Classification;
    useImport.HcOtrnTDebt.Type1 = local.HardcodeOtrnTDebt.Type1;
    useImport.HcCpaObligor.Type1 = local.HardcodeObligorCsePersonAccount.Type1;
    useImport.Max.Date = local.Max.Date;
    useImport.DebtDetail.Assign(export.Export1.Item.DebtDetail);
    useImport.ObligationPaymentSchedule.EndDt =
      export.Export1.Item.ObligationPaymentSchedule.EndDt;
    useImport.LegalActionDetail.Number = export.LegalActionDetail.Number;
    useImport.Obligor.Number = export.ConcurrentCsePerson.Number;
    useImport.Obligation.SystemGeneratedIdentifier =
      export.ConcurrentObligation.SystemGeneratedIdentifier;
    MoveObligationType2(export.ObligationType, useImport.ObligationType);
    MoveLegalAction4(export.LegalAction, useImport.LegalAction);
    useImport.Supported.Number = export.Export1.Item.SupportedCsePerson.Number;
    useImport.ObligationTransaction.Assign(
      export.Export1.Item.ObligationTransaction);

    Call(FnCreateObligationTransaction.Execute, useImport, useExport);

    export.Export1.Update.Concurrent.SystemGeneratedIdentifier =
      useExport.ObligationTransaction.SystemGeneratedIdentifier;
  }

  private void UseFnGetOblFromHistMonaNxtran()
  {
    var useImport = new FnGetOblFromHistMonaNxtran.Import();
    var useExport = new FnGetOblFromHistMonaNxtran.Export();

    useImport.NextTranInfo.InfrastructureId =
      export.HiddenNextTranInfo.InfrastructureId;

    Call(FnGetOblFromHistMonaNxtran.Execute, useImport, useExport);

    export.LegalActionDetail.Number = useExport.LegalActionDetail.Number;
    export.Obligation.SystemGeneratedIdentifier =
      useExport.Obligation.SystemGeneratedIdentifier;
    MoveObligationType1(useExport.ObligationType, export.ObligationType);
    MoveLegalAction2(useExport.LegalAction, export.LegalAction);
  }

  private void UseFnHardcodeLegal()
  {
    var useImport = new FnHardcodeLegal.Import();
    var useExport = new FnHardcodeLegal.Export();

    Call(FnHardcodeLegal.Execute, useImport, useExport);

    local.HardcodeObligorLegalActionPerson.AccountType =
      useExport.Obligor.AccountType;
    local.HardcodeSupported.AccountType = useExport.Supported.AccountType;
  }

  private void UseFnHardcodedDebtDistribution()
  {
    var useImport = new FnHardcodedDebtDistribution.Import();
    var useExport = new FnHardcodedDebtDistribution.Export();

    Call(FnHardcodedDebtDistribution.Execute, useImport, useExport);

    local.HardcodeOtCMedicalClassifica.Classification =
      useExport.OtCMedicalClassification.Classification;
    local.HardcodeObligJointSeveralCon.PrimarySecondaryCode =
      useExport.ObligJointSeveralConcurrent.PrimarySecondaryCode;
    local.HardcodePgmAdcFosterCare.ProgramTypeInd =
      useExport.PgmAdcFosterCare.ProgramTypeInd;
    local.HardcodePgmAdc.ProgramTypeInd = useExport.PgmAdc.ProgramTypeInd;
    local.HardcodePgmNonAdcFosterCare.ProgramTypeInd =
      useExport.PgmNonAdcFosterCare.ProgramTypeInd;
    local.HardcodeOt718BUraJudgement.SystemGeneratedIdentifier =
      useExport.Ot718BUraJudgement.SystemGeneratedIdentifier;
    MoveObligationTransaction2(useExport.OtrnDtVoluntary,
      local.HardcodeOtrnDtVoluntary);
    MoveObligationTransaction2(useExport.OtrnDtAccrualInstructions,
      local.HardcodeOtrnDtAccrual);
    local.HardcodeOtCFeesClassificatio.Classification =
      useExport.OtCFeesClassification.Classification;
    MoveObligationTransaction2(useExport.OtrnDtDebtDetail,
      local.HardcodeOtrnDtDebtDetail);
    local.HardcodeCpaSupportedPerson.Type1 = useExport.CpaSupportedPerson.Type1;
    local.HardcodeDdshActiveStatus.Code = useExport.DdshActiveStatus.Code;
    local.HardcodeOtrrConcurrentObliga.SystemGeneratedIdentifier =
      useExport.OtrrConcurrentObligation.SystemGeneratedIdentifier;
    local.HardcodeAccruing.Classification =
      useExport.OtCAccruingClassification.Classification;
    local.HardcodedVoluntary.Classification =
      useExport.OtCVoluntaryClassification.Classification;
    local.HardcodedRecovery.Classification =
      useExport.OtCRecoverClassification.Classification;
    local.HardcodeOtrnTDebt.Type1 = useExport.OtrnTDebt.Type1;
    local.HardcodeObligorCsePersonAccount.Type1 = useExport.CpaObligor.Type1;
    local.HardcodeOtCNonAccruingClass.Classification =
      useExport.OtCNonAccruingClassification.Classification;
    local.HardcodeSpousalArrears.SystemGeneratedIdentifier =
      useExport.OtSpousalArrearsJudgement.SystemGeneratedIdentifier;
    local.HardcodeOtInterestJudgment.SystemGeneratedIdentifier =
      useExport.OtInterestJudgement.SystemGeneratedIdentifier;
    local.HardcodeOrrJointSeveral.SequentialGeneratedIdentifier =
      useExport.OrrJointSeveral.SequentialGeneratedIdentifier;
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

  private void UseFnReadLegalActionInfo()
  {
    var useImport = new FnReadLegalActionInfo.Import();
    var useExport = new FnReadLegalActionInfo.Export();

    useImport.HcSupported.AccountType = local.HardcodeSupported.AccountType;
    useImport.HcObligor.AccountType =
      local.HardcodeObligorLegalActionPerson.AccountType;
    useImport.HcPgmNonAdcFosterCar.ProgramTypeInd =
      local.HardcodePgmNonAdcFosterCare.ProgramTypeInd;
    useImport.HcPgmAdc.ProgramTypeInd = local.HardcodePgmAdc.ProgramTypeInd;
    useImport.HcPgmAdcFosterCare.ProgramTypeInd =
      local.HardcodePgmAdcFosterCare.ProgramTypeInd;
    useImport.Max.Date = local.Max.Date;
    useImport.Current.Date = local.Current.Date;
    useImport.LegalActionDetail.Number = export.LegalActionDetail.Number;
    useImport.LegalAction.Identifier = export.LegalAction.Identifier;

    Call(FnReadLegalActionInfo.Execute, useImport, useExport);

    MoveCsePersonsWorkSet3(useExport.ConcurrentCsePersonsWorkSet,
      export.ConcurrentCsePersonsWorkSet);
    export.ObligationPaymentSchedule.
      Assign(useExport.ObligationPaymentSchedule);
    MoveObligation9(useExport.Obligation, export.Obligation);
    export.ObligationType.Assign(useExport.ObligationType);
    MoveDebtDetail3(useExport.DebtDetail, export.Header);
    export.ObligationAmt.TotalCurrency =
      useExport.ObligationAmount.TotalCurrency;
    MoveLegalAction3(useExport.LegalAction, export.LegalAction);
    export.ConcurrentCsePerson.Number = useExport.ConcurrentCsePerson.Number;
    export.ObligorCsePerson.Number = useExport.ObligorCsePerson.Number;
    MoveCsePersonsWorkSet5(useExport.Alternate, export.AltAddress);
    MoveCsePersonsWorkSet3(useExport.Supported, local.Supported);
    useExport.Export1.CopyTo(export.Export1, MoveExport3);
    export.Country.Description = useExport.Country.Description;
    export.InterstateRequest.Assign(useExport.InterstateRequest);
  }

  private void UseFnReadSupportObligation()
  {
    var useImport = new FnReadSupportObligation.Import();
    var useExport = new FnReadSupportObligation.Export();

    useImport.Max.Date = local.Max.Date;
    useImport.Current.Date = local.Current.Date;
    useImport.HcOtrnTDebt.Type1 = local.HardcodeOtrnTDebt.Type1;
    useImport.HcCpaObligor.Type1 = local.HardcodeObligorCsePersonAccount.Type1;
    useImport.HcDdshActiveStatus.Code = local.HardcodeDdshActiveStatus.Code;
    useImport.HcCpaSupportedPerson.Type1 =
      local.HardcodeCpaSupportedPerson.Type1;
    useImport.HcOrrJointSeveral.SequentialGeneratedIdentifier =
      local.HardcodeOrrJointSeveral.SequentialGeneratedIdentifier;
    useImport.HcOtrrConcurrentObliga.SystemGeneratedIdentifier =
      local.HardcodeOtrrConcurrentObliga.SystemGeneratedIdentifier;
    useImport.LegalActionDetail.Number = export.LegalActionDetail.Number;
    useImport.LegalAction.Identifier = export.LegalAction.Identifier;
    useImport.Obligor.Number = export.ObligorCsePerson.Number;
    useImport.Obligation.SystemGeneratedIdentifier =
      export.Obligation.SystemGeneratedIdentifier;
    useImport.HcObJointSeveralConcu.PrimarySecondaryCode =
      local.HardcodeObligJointSeveralCon.PrimarySecondaryCode;

    Call(FnReadSupportObligation.Execute, useImport, useExport);

    export.HiddenConcurrentObligationType.SystemGeneratedIdentifier =
      useExport.ConcurrentObligationType.SystemGeneratedIdentifier;
    export.LegalActionDetail.Number = useExport.LegalActionDetail.Number;
    MoveObligationPaymentSchedule(useExport.ObligationPaymentSchedule,
      export.ObligationPaymentSchedule);
    export.ObligorCsePerson.Number = useExport.ObligorCsePerson.Number;
    export.Obligation.Assign(useExport.Obligation);
    export.ConcurrentObligation.SystemGeneratedIdentifier =
      useExport.ConcurrentObligation.SystemGeneratedIdentifier;
    export.Header.Assign(useExport.Header);
    export.ConcurrentCsePersonsWorkSet.Assign(
      useExport.ConcurrentCsePersonsWorkSet);
    export.ConcurrentCsePerson.Number = useExport.ConcurrentCsePerson.Number;
    export.ObligationType.Assign(useExport.ObligationType);
    MoveLegalAction2(useExport.LegalAction, export.LegalAction);
    export.PaymentScheduleInd.Flag = useExport.PaymentScheduleInd.Flag;
    export.ManualDistributionInd.Flag = useExport.ManualDistributionInd.Flag;
    export.ObligationActive.Flag = useExport.ObligationActive.Flag;
    export.ObligationAmt.TotalCurrency = useExport.ObligationAmt.TotalCurrency;
    export.BalanceOwed.TotalCurrency = useExport.BalanceOwed.TotalCurrency;
    export.InterestOwed.TotalCurrency = useExport.InterestOwed.TotalCurrency;
    export.TotalOwed.TotalCurrency = useExport.TotalOwed.TotalCurrency;
    export.SuspendInterestInd.Flag = useExport.InterestSuspendedInd.Flag;
    MoveCsePersonsWorkSet5(useExport.Alternate, export.AltAddress);
    export.Case1.Number = useExport.Case1.Number;
    useExport.Export1.CopyTo(export.Export1, MoveExport2);
    export.Country.Cdvalue = useExport.Country.Cdvalue;
    export.InterstateRequest.Assign(useExport.InterstateRequest);
  }

  private void UseFnRelateTwoObligations()
  {
    var useImport = new FnRelateTwoObligations.Import();
    var useExport = new FnRelateTwoObligations.Export();

    useImport.HcObligJointSevConcur.PrimarySecondaryCode =
      local.HardcodeObligJointSeveralCon.PrimarySecondaryCode;
    useImport.HcOrrJointSeveral.SequentialGeneratedIdentifier =
      local.HardcodeOrrJointSeveral.SequentialGeneratedIdentifier;
    useImport.Current.Timestamp = local.Current.Timestamp;
    useImport.HcCpaObligor.Type1 = local.HardcodeObligorCsePersonAccount.Type1;
    useImport.FirstObligation.SystemGeneratedIdentifier =
      export.Obligation.SystemGeneratedIdentifier;
    useImport.SecondObligation.SystemGeneratedIdentifier =
      export.ConcurrentObligation.SystemGeneratedIdentifier;
    useImport.ObligationType.SystemGeneratedIdentifier =
      export.ObligationType.SystemGeneratedIdentifier;
    useImport.FirstCsePerson.Number = export.ObligorCsePerson.Number;
    useImport.SecondCsePerson.Number = export.ConcurrentCsePerson.Number;

    Call(FnRelateTwoObligations.Execute, useImport, useExport);
  }

  private void UseFnRemoveObligation1()
  {
    var useImport = new FnRemoveObligation.Import();
    var useExport = new FnRemoveObligation.Export();

    useImport.HcCpaObligor.Type1 = local.HardcodeObligorCsePersonAccount.Type1;
    MoveObligationTransaction2(local.HardcodeOtrnDtDebtDetail,
      useImport.HcOtrnDtDebtDetail);
    useImport.HcOtrnTDebt.Type1 = local.HardcodeOtrnTDebt.Type1;
    useImport.HcOtCRecoveryClassifi.Classification =
      local.HardcodedRecovery.Classification;
    useImport.Max.Date = local.Max.Date;
    MoveDateWorkArea(local.Current, useImport.Current);
    useImport.ObligationType.Assign(export.ObligationType);
    useImport.Obligor.Number = export.ObligorCsePerson.Number;
    useImport.CsePerson.Number = export.ObligorCsePerson.Number;
    MoveObligation10(export.Obligation, useImport.Obligation);

    Call(FnRemoveObligation.Execute, useImport, useExport);
  }

  private void UseFnRemoveObligation2()
  {
    var useImport = new FnRemoveObligation.Import();
    var useExport = new FnRemoveObligation.Export();

    useImport.HcCpaObligor.Type1 = local.HardcodeObligorCsePersonAccount.Type1;
    MoveObligationTransaction2(local.HardcodeOtrnDtDebtDetail,
      useImport.HcOtrnDtDebtDetail);
    useImport.HcOtrnTDebt.Type1 = local.HardcodeOtrnTDebt.Type1;
    useImport.HcOtCRecoveryClassifi.Classification =
      local.HardcodedRecovery.Classification;
    useImport.Max.Date = local.Max.Date;
    MoveDateWorkArea(local.Current, useImport.Current);
    useImport.ObligationType.Assign(export.ObligationType);
    useImport.Obligor.Number = export.ObligorCsePerson.Number;
    useImport.CsePerson.Number = export.ConcurrentCsePerson.Number;
    MoveObligation10(export.ConcurrentObligation, useImport.Obligation);

    Call(FnRemoveObligation.Execute, useImport, useExport);
  }

  private void UseFnRetrieveInterstateRequest()
  {
    var useImport = new FnRetrieveInterstateRequest.Import();
    var useExport = new FnRetrieveInterstateRequest.Export();

    useImport.Current.Date = local.Current.Date;
    useImport.LegalActionDetail.Number = export.LegalActionDetail.Number;
    useImport.CsePerson.Number = export.ObligorCsePerson.Number;
    MoveObligation6(export.Obligation, useImport.Obligor);
    useImport.LegalAction.Identifier = export.LegalAction.Identifier;
    useImport.InterstateRequest.Assign(export.InterstateRequest);

    Call(FnRetrieveInterstateRequest.Execute, useImport, useExport);

    export.Country.Description = useExport.Country.Description;
    export.InterstateRequest.Assign(useExport.InterstateRequest);
  }

  private void UseFnUpdateAlternateAddress1()
  {
    var useImport = new FnUpdateAlternateAddress.Import();
    var useExport = new FnUpdateAlternateAddress.Export();

    useImport.AlternateBillingAddress.Number = export.AltAddress.Number;
    useImport.HcCpaObligor.Type1 = local.HardcodeObligorCsePersonAccount.Type1;
    useImport.ObligationType.SystemGeneratedIdentifier =
      export.ObligationType.SystemGeneratedIdentifier;
    useImport.Obligation.SystemGeneratedIdentifier =
      export.Obligation.SystemGeneratedIdentifier;
    useImport.Obligor.Number = export.ObligorCsePerson.Number;

    Call(FnUpdateAlternateAddress.Execute, useImport, useExport);
  }

  private void UseFnUpdateAlternateAddress2()
  {
    var useImport = new FnUpdateAlternateAddress.Import();
    var useExport = new FnUpdateAlternateAddress.Export();

    useImport.AlternateBillingAddress.Number = export.AltAddress.Number;
    useImport.HcCpaObligor.Type1 = local.HardcodeObligorCsePersonAccount.Type1;
    useImport.ObligationType.SystemGeneratedIdentifier =
      export.ObligationType.SystemGeneratedIdentifier;
    useImport.Obligation.SystemGeneratedIdentifier =
      export.ConcurrentObligation.SystemGeneratedIdentifier;
    useImport.Obligor.Number = export.ConcurrentCsePerson.Number;

    Call(FnUpdateAlternateAddress.Execute, useImport, useExport);
  }

  private void UseFnUpdateInterstateRqstOblign1()
  {
    var useImport = new FnUpdateInterstateRqstOblign.Import();
    var useExport = new FnUpdateInterstateRqstOblign.Export();

    useImport.CsePerson.Number = entities.ConcurrentObligor.Number;
    useImport.Current.Date = local.Current.Date;
    useImport.CsePersonAccount.Type1 =
      local.HardcodeObligorCsePersonAccount.Type1;
    useImport.Max.Date = local.Max.Date;
    useImport.Old.IntHGeneratedId =
      export.HiddenInterstateRequest.IntHGeneratedId;
    useImport.Obligation.SystemGeneratedIdentifier =
      export.ConcurrentObligation.SystemGeneratedIdentifier;
    useImport.ObligationType.SystemGeneratedIdentifier =
      export.ObligationType.SystemGeneratedIdentifier;
    useImport.New1.IntHGeneratedId = export.InterstateRequest.IntHGeneratedId;

    Call(FnUpdateInterstateRqstOblign.Execute, useImport, useExport);
  }

  private void UseFnUpdateInterstateRqstOblign2()
  {
    var useImport = new FnUpdateInterstateRqstOblign.Import();
    var useExport = new FnUpdateInterstateRqstOblign.Export();

    useImport.Current.Date = local.Current.Date;
    useImport.CsePersonAccount.Type1 =
      local.HardcodeObligorCsePersonAccount.Type1;
    useImport.Max.Date = local.Max.Date;
    useImport.Old.IntHGeneratedId =
      export.HiddenInterstateRequest.IntHGeneratedId;
    useImport.CsePerson.Number = export.ObligorCsePerson.Number;
    useImport.Obligation.SystemGeneratedIdentifier =
      export.Obligation.SystemGeneratedIdentifier;
    useImport.ObligationType.SystemGeneratedIdentifier =
      export.ObligationType.SystemGeneratedIdentifier;
    useImport.New1.IntHGeneratedId = export.InterstateRequest.IntHGeneratedId;

    Call(FnUpdateInterstateRqstOblign.Execute, useImport, useExport);
  }

  private void UseFnUpdateNonAccruingOblig1()
  {
    var useImport = new FnUpdateNonAccruingOblig.Import();
    var useExport = new FnUpdateNonAccruingOblig.Export();

    useImport.ObligationType.SystemGeneratedIdentifier =
      export.ObligationType.SystemGeneratedIdentifier;
    useImport.Current.Timestamp = local.Current.Timestamp;
    useImport.HcOtrrConcurrentObliga.SystemGeneratedIdentifier =
      local.HardcodeOtrrConcurrentObliga.SystemGeneratedIdentifier;
    useImport.Concurrent.Flag = local.Concurr.Flag;
    useImport.Header.Assign(export.Header);
    useImport.Prev.DueDt = export.Previous.DueDt;
    useImport.ActiveObligation.Flag = export.ObligationActive.Flag;
    useImport.HardcodedObligor.Type1 =
      local.HardcodeObligorCsePersonAccount.Type1;
    MoveObligation5(export.ConcurrentObligation, useImport.Obligation);
    useImport.CsePerson.Number = export.ConcurrentCsePerson.Number;

    Call(FnUpdateNonAccruingOblig.Execute, useImport, useExport);

    export.ObligationActive.Flag = useExport.ActiveObligation.Flag;
  }

  private void UseFnUpdateNonAccruingOblig2()
  {
    var useImport = new FnUpdateNonAccruingOblig.Import();
    var useExport = new FnUpdateNonAccruingOblig.Export();

    export.Export1.CopyTo(useImport.Import1, MoveExport1ToImport1);
    useImport.ObligationType.SystemGeneratedIdentifier =
      export.ObligationType.SystemGeneratedIdentifier;
    useImport.Current.Timestamp = local.Current.Timestamp;
    useImport.HcOtrrConcurrentObliga.SystemGeneratedIdentifier =
      local.HardcodeOtrrConcurrentObliga.SystemGeneratedIdentifier;
    useImport.Header.Assign(export.Header);
    useImport.Prev.DueDt = export.Previous.DueDt;
    useImport.ActiveObligation.Flag = export.ObligationActive.Flag;
    useImport.HardcodedObligor.Type1 =
      local.HardcodeObligorCsePersonAccount.Type1;
    MoveObligation4(export.Obligation, useImport.Obligation);
    useImport.CsePerson.Number = export.ObligorCsePerson.Number;

    Call(FnUpdateNonAccruingOblig.Execute, useImport, useExport);

    MoveObligation14(useExport.Obligation, export.Obligation);
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

    MoveNextTranInfo(export.HiddenNextTranInfo, useImport.NextTranInfo);
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

    useImport.CsePerson.Number = export.ObligorCsePerson.Number;
    useImport.CsePersonsWorkSet.Number = export.ObligorCsePersonsWorkSet.Number;
    useImport.LegalAction.Assign(export.LegalAction);
    useImport.Case1.Number = export.Export1.Item.Case1.Number;

    Call(ScCabTestSecurity.Execute, useImport, useExport);
  }

  private void UseSiReadCsePerson1()
  {
    var useImport = new SiReadCsePerson.Import();
    var useExport = new SiReadCsePerson.Export();

    useImport.CsePersonsWorkSet.Number = export.AltAddress.Number;

    Call(SiReadCsePerson.Execute, useImport, useExport);

    local.Eab.Assign(useExport.AbendData);
    MoveCsePersonsWorkSet2(useExport.CsePersonsWorkSet, export.AltAddress);
  }

  private void UseSiReadCsePerson2()
  {
    var useImport = new SiReadCsePerson.Import();
    var useExport = new SiReadCsePerson.Export();

    useImport.CsePersonsWorkSet.Number = export.ObligorCsePersonsWorkSet.Number;

    Call(SiReadCsePerson.Execute, useImport, useExport);

    local.Eab.Assign(useExport.AbendData);
    export.ObligorCsePersonsWorkSet.Assign(useExport.CsePersonsWorkSet);
  }

  private void UseSiReadCsePerson3()
  {
    var useImport = new SiReadCsePerson.Import();
    var useExport = new SiReadCsePerson.Export();

    useImport.CsePersonsWorkSet.Number =
      export.ConcurrentCsePersonsWorkSet.Number;

    Call(SiReadCsePerson.Execute, useImport, useExport);

    local.Eab.Assign(useExport.AbendData);
    export.ConcurrentCsePersonsWorkSet.Assign(useExport.CsePersonsWorkSet);
  }

  private void UseSiReadCsePerson4()
  {
    var useImport = new SiReadCsePerson.Import();
    var useExport = new SiReadCsePerson.Export();

    useImport.CsePersonsWorkSet.Number = local.Supported.Number;

    Call(SiReadCsePerson.Execute, useImport, useExport);

    local.Supported.Assign(useExport.CsePersonsWorkSet);
    local.Eab.Assign(useExport.AbendData);
  }

  private void AssociateObligation()
  {
    System.Diagnostics.Debug.Assert(entities.Obligation.Populated);

    var cspPNumber = entities.Alternate.Number;

    entities.Obligation.Populated = false;
    Update("AssociateObligation",
      (db, command) =>
      {
        db.SetNullableString(command, "cspPNumber", cspPNumber);
        db.SetString(command, "cpaType", entities.Obligation.CpaType);
        db.SetString(command, "cspNumber", entities.Obligation.CspNumber);
        db.SetInt32(
          command, "obId", entities.Obligation.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "dtyGeneratedId", entities.Obligation.DtyGeneratedId);
      });

    entities.Obligation.CspPNumber = cspPNumber;
    entities.Obligation.Populated = true;
  }

  private bool ReadCase()
  {
    entities.Case1.Populated = false;

    return Read("ReadCase",
      (db, command) =>
      {
        db.SetString(command, "cspNumber1", export.ObligorCsePerson.Number);
        db.SetString(
          command, "cspNumber2",
          export.HiddenFlowToPeprCsePersonsWorkSet.Number);
      },
      (db, reader) =>
      {
        entities.Case1.Number = db.GetString(reader, 0);
        entities.Case1.Status = db.GetNullableString(reader, 1);
        entities.Case1.Populated = true;
      });
  }

  private bool ReadCollection()
  {
    entities.Collection.Populated = false;

    return Read("ReadCollection",
      (db, command) =>
      {
        db.SetDate(
          command, "cvrdCollStrtDt",
          local.ObligCollProtectionHist.CvrdCollStrtDt.GetValueOrDefault());
        db.SetDate(command, "date", local.Current.Date.GetValueOrDefault());
        db.SetNullableString(
          command, "standardNo", export.LegalAction.StandardNumber ?? "");
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

  private bool ReadCsePerson()
  {
    entities.Alternate.Populated = false;

    return Read("ReadCsePerson",
      (db, command) =>
      {
        db.SetString(command, "numb", export.AltAddress.Number);
      },
      (db, reader) =>
      {
        entities.Alternate.Number = db.GetString(reader, 0);
        entities.Alternate.Type1 = db.GetString(reader, 1);
        entities.Alternate.Populated = true;
        CheckValid<CsePerson>("Type1", entities.Alternate.Type1);
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
        db.SetInt32(
          command, "otyIdentifier",
          export.ObligationType.SystemGeneratedIdentifier);
        db.SetString(command, "cspNumber", export.ObligorCsePerson.Number);
        db.SetDate(
          command, "deactivationDate",
          local.BlankDateWorkArea.Date.GetValueOrDefault());
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

  private bool ReadObligation1()
  {
    entities.Obligation.Populated = false;

    return Read("ReadObligation1",
      (db, command) =>
      {
        db.SetInt32(
          command, "dtyGeneratedId",
          export.ObligationType.SystemGeneratedIdentifier);
        db.SetString(
          command, "cpaType", local.HardcodeObligorCsePersonAccount.Type1);
        db.SetString(command, "cspNumber", export.ConcurrentCsePerson.Number);
        db.SetInt32(
          command, "obId",
          export.ConcurrentObligation.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.Obligation.CpaType = db.GetString(reader, 0);
        entities.Obligation.CspNumber = db.GetString(reader, 1);
        entities.Obligation.SystemGeneratedIdentifier = db.GetInt32(reader, 2);
        entities.Obligation.DtyGeneratedId = db.GetInt32(reader, 3);
        entities.Obligation.CspPNumber = db.GetNullableString(reader, 4);
        entities.Obligation.OtherStateAbbr = db.GetNullableString(reader, 5);
        entities.Obligation.Description = db.GetNullableString(reader, 6);
        entities.Obligation.HistoryInd = db.GetNullableString(reader, 7);
        entities.Obligation.PrimarySecondaryCode =
          db.GetNullableString(reader, 8);
        entities.Obligation.AsOfDtNadArrBal = db.GetNullableDecimal(reader, 9);
        entities.Obligation.AsOfDtNadIntBal = db.GetNullableDecimal(reader, 10);
        entities.Obligation.AsOfDtAdcArrBal = db.GetNullableDecimal(reader, 11);
        entities.Obligation.AsOfDtAdcIntBal = db.GetNullableDecimal(reader, 12);
        entities.Obligation.AsOfDtRecBal = db.GetNullableDecimal(reader, 13);
        entities.Obligation.AsOdDtRecIntBal = db.GetNullableDecimal(reader, 14);
        entities.Obligation.AsOfDtFeeBal = db.GetNullableDecimal(reader, 15);
        entities.Obligation.AsOfDtFeeIntBal = db.GetNullableDecimal(reader, 16);
        entities.Obligation.AsOfDtTotBalCurrArr =
          db.GetNullableDecimal(reader, 17);
        entities.Obligation.CreatedBy = db.GetString(reader, 18);
        entities.Obligation.CreatedTmst = db.GetDateTime(reader, 19);
        entities.Obligation.LastUpdatedBy = db.GetNullableString(reader, 20);
        entities.Obligation.LastUpdateTmst = db.GetNullableDateTime(reader, 21);
        entities.Obligation.OrderTypeCode = db.GetString(reader, 22);
        entities.Obligation.Populated = true;
        CheckValid<Obligation>("CpaType", entities.Obligation.CpaType);
        CheckValid<Obligation>("PrimarySecondaryCode",
          entities.Obligation.PrimarySecondaryCode);
        CheckValid<Obligation>("OrderTypeCode",
          entities.Obligation.OrderTypeCode);
      });
  }

  private bool ReadObligation2()
  {
    entities.Obligation.Populated = false;

    return Read("ReadObligation2",
      (db, command) =>
      {
        db.SetInt32(
          command, "dtyGeneratedId",
          export.ObligationType.SystemGeneratedIdentifier);
        db.SetString(
          command, "cpaType", local.HardcodeObligorCsePersonAccount.Type1);
        db.SetString(command, "cspNumber", export.ObligorCsePerson.Number);
        db.
          SetInt32(command, "obId", export.Obligation.SystemGeneratedIdentifier);
          
      },
      (db, reader) =>
      {
        entities.Obligation.CpaType = db.GetString(reader, 0);
        entities.Obligation.CspNumber = db.GetString(reader, 1);
        entities.Obligation.SystemGeneratedIdentifier = db.GetInt32(reader, 2);
        entities.Obligation.DtyGeneratedId = db.GetInt32(reader, 3);
        entities.Obligation.CspPNumber = db.GetNullableString(reader, 4);
        entities.Obligation.OtherStateAbbr = db.GetNullableString(reader, 5);
        entities.Obligation.Description = db.GetNullableString(reader, 6);
        entities.Obligation.HistoryInd = db.GetNullableString(reader, 7);
        entities.Obligation.PrimarySecondaryCode =
          db.GetNullableString(reader, 8);
        entities.Obligation.AsOfDtNadArrBal = db.GetNullableDecimal(reader, 9);
        entities.Obligation.AsOfDtNadIntBal = db.GetNullableDecimal(reader, 10);
        entities.Obligation.AsOfDtAdcArrBal = db.GetNullableDecimal(reader, 11);
        entities.Obligation.AsOfDtAdcIntBal = db.GetNullableDecimal(reader, 12);
        entities.Obligation.AsOfDtRecBal = db.GetNullableDecimal(reader, 13);
        entities.Obligation.AsOdDtRecIntBal = db.GetNullableDecimal(reader, 14);
        entities.Obligation.AsOfDtFeeBal = db.GetNullableDecimal(reader, 15);
        entities.Obligation.AsOfDtFeeIntBal = db.GetNullableDecimal(reader, 16);
        entities.Obligation.AsOfDtTotBalCurrArr =
          db.GetNullableDecimal(reader, 17);
        entities.Obligation.CreatedBy = db.GetString(reader, 18);
        entities.Obligation.CreatedTmst = db.GetDateTime(reader, 19);
        entities.Obligation.LastUpdatedBy = db.GetNullableString(reader, 20);
        entities.Obligation.LastUpdateTmst = db.GetNullableDateTime(reader, 21);
        entities.Obligation.OrderTypeCode = db.GetString(reader, 22);
        entities.Obligation.Populated = true;
        CheckValid<Obligation>("CpaType", entities.Obligation.CpaType);
        CheckValid<Obligation>("PrimarySecondaryCode",
          entities.Obligation.PrimarySecondaryCode);
        CheckValid<Obligation>("OrderTypeCode",
          entities.Obligation.OrderTypeCode);
      });
  }

  private IEnumerable<bool> ReadObligation3()
  {
    entities.OtherView.Populated = false;

    return ReadEach("ReadObligation3",
      (db, command) =>
      {
        db.SetNullableString(
          command, "standardNo", export.LegalAction.StandardNumber ?? "");
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

  private bool ReadObligationType()
  {
    entities.ObligationType.Populated = false;

    return Read("ReadObligationType",
      (db, command) =>
      {
        db.SetString(command, "debtTypCd", import.ObligationType.Code);
        db.SetNullableDate(
          command, "discontinueDt", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ObligationType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.ObligationType.Code = db.GetString(reader, 1);
        entities.ObligationType.Name = db.GetString(reader, 2);
        entities.ObligationType.Classification = db.GetString(reader, 3);
        entities.ObligationType.EffectiveDt = db.GetDate(reader, 4);
        entities.ObligationType.DiscontinueDt = db.GetNullableDate(reader, 5);
        entities.ObligationType.SupportedPersonReqInd = db.GetString(reader, 6);
        entities.ObligationType.Populated = true;
        CheckValid<ObligationType>("Classification",
          entities.ObligationType.Classification);
        CheckValid<ObligationType>("SupportedPersonReqInd",
          entities.ObligationType.SupportedPersonReqInd);
      });
  }

  private bool ReadPersonProgram()
  {
    entities.PersonProgram.Populated = false;

    return Read("ReadPersonProgram",
      (db, command) =>
      {
        db.SetString(
          command, "cspNumber", export.Export1.Item.SupportedCsePerson.Number);
        db.SetDate(
          command, "effectiveDate",
          export.Header.CoveredPrdStartDt.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.PersonProgram.CspNumber = db.GetString(reader, 0);
        entities.PersonProgram.EffectiveDate = db.GetDate(reader, 1);
        entities.PersonProgram.DiscontinueDate = db.GetNullableDate(reader, 2);
        entities.PersonProgram.CreatedTimestamp = db.GetDateTime(reader, 3);
        entities.PersonProgram.PrgGeneratedId = db.GetInt32(reader, 4);
        entities.PersonProgram.Populated = true;
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
    /// <summary>A ImportGroup group.</summary>
    [Serializable]
    public class ImportGroup
    {
      /// <summary>
      /// A value of ZdelImportGrpDetail.
      /// </summary>
      [JsonPropertyName("zdelImportGrpDetail")]
      public Program ZdelImportGrpDetail
      {
        get => zdelImportGrpDetail ??= new();
        set => zdelImportGrpDetail = value;
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
      /// A value of DebtDetail.
      /// </summary>
      [JsonPropertyName("debtDetail")]
      public DebtDetail DebtDetail
      {
        get => debtDetail ??= new();
        set => debtDetail = value;
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
      /// A value of ObligationPaymentSchedule.
      /// </summary>
      [JsonPropertyName("obligationPaymentSchedule")]
      public ObligationPaymentSchedule ObligationPaymentSchedule
      {
        get => obligationPaymentSchedule ??= new();
        set => obligationPaymentSchedule = value;
      }

      /// <summary>
      /// A value of Concurrent.
      /// </summary>
      [JsonPropertyName("concurrent")]
      public ObligationTransaction Concurrent
      {
        get => concurrent ??= new();
        set => concurrent = value;
      }

      /// <summary>
      /// A value of Prev.
      /// </summary>
      [JsonPropertyName("prev")]
      public ObligationTransaction Prev
      {
        get => prev ??= new();
        set => prev = value;
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
      /// A value of Hidden.
      /// </summary>
      [JsonPropertyName("hidden")]
      public DebtDetail Hidden
      {
        get => hidden ??= new();
        set => hidden = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 20;

      private Program zdelImportGrpDetail;
      private CsePerson supportedCsePerson;
      private CsePersonsWorkSet supportedCsePersonsWorkSet;
      private Case1 case1;
      private ObligationTransaction obligationTransaction;
      private DebtDetail debtDetail;
      private ServiceProvider serviceProvider;
      private ObligationPaymentSchedule obligationPaymentSchedule;
      private ObligationTransaction concurrent;
      private ObligationTransaction prev;
      private Common common;
      private DebtDetail hidden;
    }

    /// <summary>
    /// A value of HiddenConcurrentCsePerson.
    /// </summary>
    [JsonPropertyName("hiddenConcurrentCsePerson")]
    public CsePerson HiddenConcurrentCsePerson
    {
      get => hiddenConcurrentCsePerson ??= new();
      set => hiddenConcurrentCsePerson = value;
    }

    /// <summary>
    /// A value of HiddenObligor.
    /// </summary>
    [JsonPropertyName("hiddenObligor")]
    public CsePerson HiddenObligor
    {
      get => hiddenObligor ??= new();
      set => hiddenObligor = value;
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
    /// A value of CountryPrompt.
    /// </summary>
    [JsonPropertyName("countryPrompt")]
    public Common CountryPrompt
    {
      get => countryPrompt ??= new();
      set => countryPrompt = value;
    }

    /// <summary>
    /// A value of Country.
    /// </summary>
    [JsonPropertyName("country")]
    public CodeValue Country
    {
      get => country ??= new();
      set => country = value;
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
    /// A value of LegalActionDetail.
    /// </summary>
    [JsonPropertyName("legalActionDetail")]
    public LegalActionDetail LegalActionDetail
    {
      get => legalActionDetail ??= new();
      set => legalActionDetail = value;
    }

    /// <summary>
    /// A value of HiddenAltAddress.
    /// </summary>
    [JsonPropertyName("hiddenAltAddress")]
    public CsePersonsWorkSet HiddenAltAddress
    {
      get => hiddenAltAddress ??= new();
      set => hiddenAltAddress = value;
    }

    /// <summary>
    /// A value of HiddenInterstateRequest.
    /// </summary>
    [JsonPropertyName("hiddenInterstateRequest")]
    public InterstateRequest HiddenInterstateRequest
    {
      get => hiddenInterstateRequest ??= new();
      set => hiddenInterstateRequest = value;
    }

    /// <summary>
    /// A value of HiddenConcurrentObligationType.
    /// </summary>
    [JsonPropertyName("hiddenConcurrentObligationType")]
    public ObligationType HiddenConcurrentObligationType
    {
      get => hiddenConcurrentObligationType ??= new();
      set => hiddenConcurrentObligationType = value;
    }

    /// <summary>
    /// A value of DisplayedObligCreateDt.
    /// </summary>
    [JsonPropertyName("displayedObligCreateDt")]
    public DateWorkArea DisplayedObligCreateDt
    {
      get => displayedObligCreateDt ??= new();
      set => displayedObligCreateDt = value;
    }

    /// <summary>
    /// A value of ReturnFlowFrom.
    /// </summary>
    [JsonPropertyName("returnFlowFrom")]
    public TextWorkArea ReturnFlowFrom
    {
      get => returnFlowFrom ??= new();
      set => returnFlowFrom = value;
    }

    /// <summary>
    /// A value of HiddenDisplayed.
    /// </summary>
    [JsonPropertyName("hiddenDisplayed")]
    public DebtDetail HiddenDisplayed
    {
      get => hiddenDisplayed ??= new();
      set => hiddenDisplayed = value;
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
    /// A value of FromFlow.
    /// </summary>
    [JsonPropertyName("fromFlow")]
    public CsePersonsWorkSet FromFlow
    {
      get => fromFlow ??= new();
      set => fromFlow = value;
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
    /// A value of PayeePrompt.
    /// </summary>
    [JsonPropertyName("payeePrompt")]
    public TextWorkArea PayeePrompt
    {
      get => payeePrompt ??= new();
      set => payeePrompt = value;
    }

    /// <summary>
    /// A value of ZdelImportDesigPayee.
    /// </summary>
    [JsonPropertyName("zdelImportDesigPayee")]
    public CsePersonsWorkSet ZdelImportDesigPayee
    {
      get => zdelImportDesigPayee ??= new();
      set => zdelImportDesigPayee = value;
    }

    /// <summary>
    /// A value of AltAddress.
    /// </summary>
    [JsonPropertyName("altAddress")]
    public CsePersonsWorkSet AltAddress
    {
      get => altAddress ??= new();
      set => altAddress = value;
    }

    /// <summary>
    /// A value of AltAddrPrompt.
    /// </summary>
    [JsonPropertyName("altAddrPrompt")]
    public Common AltAddrPrompt
    {
      get => altAddrPrompt ??= new();
      set => altAddrPrompt = value;
    }

    /// <summary>
    /// A value of FormattedAlternateAddre.
    /// </summary>
    [JsonPropertyName("formattedAlternateAddre")]
    public TextWorkArea FormattedAlternateAddre
    {
      get => formattedAlternateAddre ??= new();
      set => formattedAlternateAddre = value;
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
    /// A value of ObligorCsePerson.
    /// </summary>
    [JsonPropertyName("obligorCsePerson")]
    public CsePerson ObligorCsePerson
    {
      get => obligorCsePerson ??= new();
      set => obligorCsePerson = value;
    }

    /// <summary>
    /// A value of ObligorCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("obligorCsePersonsWorkSet")]
    public CsePersonsWorkSet ObligorCsePersonsWorkSet
    {
      get => obligorCsePersonsWorkSet ??= new();
      set => obligorCsePersonsWorkSet = value;
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
    /// A value of ConcurrentCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("concurrentCsePersonsWorkSet")]
    public CsePersonsWorkSet ConcurrentCsePersonsWorkSet
    {
      get => concurrentCsePersonsWorkSet ??= new();
      set => concurrentCsePersonsWorkSet = value;
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
    /// A value of ConcurrentObligation.
    /// </summary>
    [JsonPropertyName("concurrentObligation")]
    public Obligation ConcurrentObligation
    {
      get => concurrentObligation ??= new();
      set => concurrentObligation = value;
    }

    /// <summary>
    /// A value of Header.
    /// </summary>
    [JsonPropertyName("header")]
    public DebtDetail Header
    {
      get => header ??= new();
      set => header = value;
    }

    /// <summary>
    /// A value of Previous.
    /// </summary>
    [JsonPropertyName("previous")]
    public DebtDetail Previous
    {
      get => previous ??= new();
      set => previous = value;
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
    /// A value of ManualDistributionInd.
    /// </summary>
    [JsonPropertyName("manualDistributionInd")]
    public Common ManualDistributionInd
    {
      get => manualDistributionInd ??= new();
      set => manualDistributionInd = value;
    }

    /// <summary>
    /// A value of PaymentScheduleInd.
    /// </summary>
    [JsonPropertyName("paymentScheduleInd")]
    public Common PaymentScheduleInd
    {
      get => paymentScheduleInd ??= new();
      set => paymentScheduleInd = value;
    }

    /// <summary>
    /// A value of SuspendInterestInd.
    /// </summary>
    [JsonPropertyName("suspendInterestInd")]
    public Common SuspendInterestInd
    {
      get => suspendInterestInd ??= new();
      set => suspendInterestInd = value;
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
    /// A value of BalanceOwed.
    /// </summary>
    [JsonPropertyName("balanceOwed")]
    public Common BalanceOwed
    {
      get => balanceOwed ??= new();
      set => balanceOwed = value;
    }

    /// <summary>
    /// A value of InterestOwed.
    /// </summary>
    [JsonPropertyName("interestOwed")]
    public Common InterestOwed
    {
      get => interestOwed ??= new();
      set => interestOwed = value;
    }

    /// <summary>
    /// A value of TotalOwed.
    /// </summary>
    [JsonPropertyName("totalOwed")]
    public Common TotalOwed
    {
      get => totalOwed ??= new();
      set => totalOwed = value;
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
    /// A value of Passed.
    /// </summary>
    [JsonPropertyName("passed")]
    public LegalActionDetail Passed
    {
      get => passed ??= new();
      set => passed = value;
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
    /// A value of HiddenStoredDebtDetail.
    /// </summary>
    [JsonPropertyName("hiddenStoredDebtDetail")]
    public DebtDetail HiddenStoredDebtDetail
    {
      get => hiddenStoredDebtDetail ??= new();
      set => hiddenStoredDebtDetail = value;
    }

    /// <summary>
    /// A value of HiddenStoredObligation.
    /// </summary>
    [JsonPropertyName("hiddenStoredObligation")]
    public Obligation HiddenStoredObligation
    {
      get => hiddenStoredObligation ??= new();
      set => hiddenStoredObligation = value;
    }

    /// <summary>
    /// A value of HiddenFlowToPeprCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("hiddenFlowToPeprCsePersonsWorkSet")]
    public CsePersonsWorkSet HiddenFlowToPeprCsePersonsWorkSet
    {
      get => hiddenFlowToPeprCsePersonsWorkSet ??= new();
      set => hiddenFlowToPeprCsePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of HiddenFlowToPeprCase.
    /// </summary>
    [JsonPropertyName("hiddenFlowToPeprCase")]
    public Case1 HiddenFlowToPeprCase
    {
      get => hiddenFlowToPeprCase ??= new();
      set => hiddenFlowToPeprCase = value;
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
    /// A value of ObCollProtAct.
    /// </summary>
    [JsonPropertyName("obCollProtAct")]
    public Common ObCollProtAct
    {
      get => obCollProtAct ??= new();
      set => obCollProtAct = value;
    }

    private CsePerson hiddenConcurrentCsePerson;
    private CsePerson hiddenObligor;
    private Code code;
    private Common countryPrompt;
    private CodeValue country;
    private LegalAction legalAction;
    private LegalActionDetail legalActionDetail;
    private CsePersonsWorkSet hiddenAltAddress;
    private InterstateRequest hiddenInterstateRequest;
    private ObligationType hiddenConcurrentObligationType;
    private DateWorkArea displayedObligCreateDt;
    private TextWorkArea returnFlowFrom;
    private DebtDetail hiddenDisplayed;
    private InterstateRequest interstateRequest;
    private CsePersonsWorkSet fromFlow;
    private Case1 case1;
    private TextWorkArea payeePrompt;
    private CsePersonsWorkSet zdelImportDesigPayee;
    private CsePersonsWorkSet altAddress;
    private Common altAddrPrompt;
    private TextWorkArea formattedAlternateAddre;
    private Standard standard;
    private CsePerson obligorCsePerson;
    private CsePersonsWorkSet obligorCsePersonsWorkSet;
    private CsePerson concurrentCsePerson;
    private CsePersonsWorkSet concurrentCsePersonsWorkSet;
    private Obligation obligation;
    private ObligationPaymentSchedule obligationPaymentSchedule;
    private Obligation concurrentObligation;
    private DebtDetail header;
    private DebtDetail previous;
    private ObligationType obligationType;
    private Common manualDistributionInd;
    private Common paymentScheduleInd;
    private Common suspendInterestInd;
    private Common obligationAmt;
    private Common balanceOwed;
    private Common interestOwed;
    private Common totalOwed;
    private Common obligationActive;
    private LegalActionDetail passed;
    private NextTranInfo hiddenNextTranInfo;
    private Array<ImportGroup> import1;
    private DebtDetail hiddenStoredDebtDetail;
    private Obligation hiddenStoredObligation;
    private CsePersonsWorkSet hiddenFlowToPeprCsePersonsWorkSet;
    private Case1 hiddenFlowToPeprCase;
    private Common collProtAnswer;
    private Common obCollProtAct;
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
      /// A value of ZdelExportGrpDetail.
      /// </summary>
      [JsonPropertyName("zdelExportGrpDetail")]
      public Program ZdelExportGrpDetail
      {
        get => zdelExportGrpDetail ??= new();
        set => zdelExportGrpDetail = value;
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
      /// A value of DebtDetail.
      /// </summary>
      [JsonPropertyName("debtDetail")]
      public DebtDetail DebtDetail
      {
        get => debtDetail ??= new();
        set => debtDetail = value;
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
      /// A value of ObligationPaymentSchedule.
      /// </summary>
      [JsonPropertyName("obligationPaymentSchedule")]
      public ObligationPaymentSchedule ObligationPaymentSchedule
      {
        get => obligationPaymentSchedule ??= new();
        set => obligationPaymentSchedule = value;
      }

      /// <summary>
      /// A value of Concurrent.
      /// </summary>
      [JsonPropertyName("concurrent")]
      public ObligationTransaction Concurrent
      {
        get => concurrent ??= new();
        set => concurrent = value;
      }

      /// <summary>
      /// A value of Prev.
      /// </summary>
      [JsonPropertyName("prev")]
      public ObligationTransaction Prev
      {
        get => prev ??= new();
        set => prev = value;
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
      /// A value of Hidden.
      /// </summary>
      [JsonPropertyName("hidden")]
      public DebtDetail Hidden
      {
        get => hidden ??= new();
        set => hidden = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 20;

      private Program zdelExportGrpDetail;
      private CsePerson supportedCsePerson;
      private CsePersonsWorkSet supportedCsePersonsWorkSet;
      private Case1 case1;
      private ObligationTransaction obligationTransaction;
      private DebtDetail debtDetail;
      private ServiceProvider serviceProvider;
      private ObligationPaymentSchedule obligationPaymentSchedule;
      private ObligationTransaction concurrent;
      private ObligationTransaction prev;
      private Common common;
      private DebtDetail hidden;
    }

    /// <summary>
    /// A value of HiddenConcurrentCsePerson.
    /// </summary>
    [JsonPropertyName("hiddenConcurrentCsePerson")]
    public CsePerson HiddenConcurrentCsePerson
    {
      get => hiddenConcurrentCsePerson ??= new();
      set => hiddenConcurrentCsePerson = value;
    }

    /// <summary>
    /// A value of HiddenObligor.
    /// </summary>
    [JsonPropertyName("hiddenObligor")]
    public CsePerson HiddenObligor
    {
      get => hiddenObligor ??= new();
      set => hiddenObligor = value;
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
    /// A value of CountryPrompt.
    /// </summary>
    [JsonPropertyName("countryPrompt")]
    public Common CountryPrompt
    {
      get => countryPrompt ??= new();
      set => countryPrompt = value;
    }

    /// <summary>
    /// A value of Country.
    /// </summary>
    [JsonPropertyName("country")]
    public CodeValue Country
    {
      get => country ??= new();
      set => country = value;
    }

    /// <summary>
    /// A value of HiddenAltAddress.
    /// </summary>
    [JsonPropertyName("hiddenAltAddress")]
    public CsePersonsWorkSet HiddenAltAddress
    {
      get => hiddenAltAddress ??= new();
      set => hiddenAltAddress = value;
    }

    /// <summary>
    /// A value of HiddenInterstateRequest.
    /// </summary>
    [JsonPropertyName("hiddenInterstateRequest")]
    public InterstateRequest HiddenInterstateRequest
    {
      get => hiddenInterstateRequest ??= new();
      set => hiddenInterstateRequest = value;
    }

    /// <summary>
    /// A value of HiddenConcurrentObligationType.
    /// </summary>
    [JsonPropertyName("hiddenConcurrentObligationType")]
    public ObligationType HiddenConcurrentObligationType
    {
      get => hiddenConcurrentObligationType ??= new();
      set => hiddenConcurrentObligationType = value;
    }

    /// <summary>
    /// A value of DisplayedObligCreateDt.
    /// </summary>
    [JsonPropertyName("displayedObligCreateDt")]
    public DateWorkArea DisplayedObligCreateDt
    {
      get => displayedObligCreateDt ??= new();
      set => displayedObligCreateDt = value;
    }

    /// <summary>
    /// A value of HiddenStoredDebtDetail.
    /// </summary>
    [JsonPropertyName("hiddenStoredDebtDetail")]
    public DebtDetail HiddenStoredDebtDetail
    {
      get => hiddenStoredDebtDetail ??= new();
      set => hiddenStoredDebtDetail = value;
    }

    /// <summary>
    /// A value of ReturnFlowFrom.
    /// </summary>
    [JsonPropertyName("returnFlowFrom")]
    public TextWorkArea ReturnFlowFrom
    {
      get => returnFlowFrom ??= new();
      set => returnFlowFrom = value;
    }

    /// <summary>
    /// A value of HiddenDisplayed.
    /// </summary>
    [JsonPropertyName("hiddenDisplayed")]
    public DebtDetail HiddenDisplayed
    {
      get => hiddenDisplayed ??= new();
      set => hiddenDisplayed = value;
    }

    /// <summary>
    /// A value of PassAr.
    /// </summary>
    [JsonPropertyName("passAr")]
    public CsePerson PassAr
    {
      get => passAr ??= new();
      set => passAr = value;
    }

    /// <summary>
    /// A value of PassSupported.
    /// </summary>
    [JsonPropertyName("passSupported")]
    public CsePersonsWorkSet PassSupported
    {
      get => passSupported ??= new();
      set => passSupported = value;
    }

    /// <summary>
    /// A value of Pass.
    /// </summary>
    [JsonPropertyName("pass")]
    public ObligationTransaction Pass
    {
      get => pass ??= new();
      set => pass = value;
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
    /// A value of Case1.
    /// </summary>
    [JsonPropertyName("case1")]
    public Case1 Case1
    {
      get => case1 ??= new();
      set => case1 = value;
    }

    /// <summary>
    /// A value of PayeePrompt.
    /// </summary>
    [JsonPropertyName("payeePrompt")]
    public TextWorkArea PayeePrompt
    {
      get => payeePrompt ??= new();
      set => payeePrompt = value;
    }

    /// <summary>
    /// A value of ZdelExportDesigPayee.
    /// </summary>
    [JsonPropertyName("zdelExportDesigPayee")]
    public CsePersonsWorkSet ZdelExportDesigPayee
    {
      get => zdelExportDesigPayee ??= new();
      set => zdelExportDesigPayee = value;
    }

    /// <summary>
    /// A value of AltAddress.
    /// </summary>
    [JsonPropertyName("altAddress")]
    public CsePersonsWorkSet AltAddress
    {
      get => altAddress ??= new();
      set => altAddress = value;
    }

    /// <summary>
    /// A value of AltAddrPrompt.
    /// </summary>
    [JsonPropertyName("altAddrPrompt")]
    public Common AltAddrPrompt
    {
      get => altAddrPrompt ??= new();
      set => altAddrPrompt = value;
    }

    /// <summary>
    /// A value of FormattedAlternateAddre.
    /// </summary>
    [JsonPropertyName("formattedAlternateAddre")]
    public TextWorkArea FormattedAlternateAddre
    {
      get => formattedAlternateAddre ??= new();
      set => formattedAlternateAddre = value;
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
    /// A value of ObligorCsePerson.
    /// </summary>
    [JsonPropertyName("obligorCsePerson")]
    public CsePerson ObligorCsePerson
    {
      get => obligorCsePerson ??= new();
      set => obligorCsePerson = value;
    }

    /// <summary>
    /// A value of ObligorCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("obligorCsePersonsWorkSet")]
    public CsePersonsWorkSet ObligorCsePersonsWorkSet
    {
      get => obligorCsePersonsWorkSet ??= new();
      set => obligorCsePersonsWorkSet = value;
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
    /// A value of ConcurrentCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("concurrentCsePersonsWorkSet")]
    public CsePersonsWorkSet ConcurrentCsePersonsWorkSet
    {
      get => concurrentCsePersonsWorkSet ??= new();
      set => concurrentCsePersonsWorkSet = value;
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
    /// A value of ConcurrentObligation.
    /// </summary>
    [JsonPropertyName("concurrentObligation")]
    public Obligation ConcurrentObligation
    {
      get => concurrentObligation ??= new();
      set => concurrentObligation = value;
    }

    /// <summary>
    /// A value of Header.
    /// </summary>
    [JsonPropertyName("header")]
    public DebtDetail Header
    {
      get => header ??= new();
      set => header = value;
    }

    /// <summary>
    /// A value of Previous.
    /// </summary>
    [JsonPropertyName("previous")]
    public DebtDetail Previous
    {
      get => previous ??= new();
      set => previous = value;
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
    /// A value of ObligationActive.
    /// </summary>
    [JsonPropertyName("obligationActive")]
    public Common ObligationActive
    {
      get => obligationActive ??= new();
      set => obligationActive = value;
    }

    /// <summary>
    /// A value of ManualDistributionInd.
    /// </summary>
    [JsonPropertyName("manualDistributionInd")]
    public Common ManualDistributionInd
    {
      get => manualDistributionInd ??= new();
      set => manualDistributionInd = value;
    }

    /// <summary>
    /// A value of PaymentScheduleInd.
    /// </summary>
    [JsonPropertyName("paymentScheduleInd")]
    public Common PaymentScheduleInd
    {
      get => paymentScheduleInd ??= new();
      set => paymentScheduleInd = value;
    }

    /// <summary>
    /// A value of SuspendInterestInd.
    /// </summary>
    [JsonPropertyName("suspendInterestInd")]
    public Common SuspendInterestInd
    {
      get => suspendInterestInd ??= new();
      set => suspendInterestInd = value;
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
    /// A value of BalanceOwed.
    /// </summary>
    [JsonPropertyName("balanceOwed")]
    public Common BalanceOwed
    {
      get => balanceOwed ??= new();
      set => balanceOwed = value;
    }

    /// <summary>
    /// A value of InterestOwed.
    /// </summary>
    [JsonPropertyName("interestOwed")]
    public Common InterestOwed
    {
      get => interestOwed ??= new();
      set => interestOwed = value;
    }

    /// <summary>
    /// A value of TotalOwed.
    /// </summary>
    [JsonPropertyName("totalOwed")]
    public Common TotalOwed
    {
      get => totalOwed ??= new();
      set => totalOwed = value;
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
    /// A value of DlgflwAr.
    /// </summary>
    [JsonPropertyName("dlgflwAr")]
    public CsePersonsWorkSet DlgflwAr
    {
      get => dlgflwAr ??= new();
      set => dlgflwAr = value;
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
    /// A value of HiddenStoredObligation.
    /// </summary>
    [JsonPropertyName("hiddenStoredObligation")]
    public Obligation HiddenStoredObligation
    {
      get => hiddenStoredObligation ??= new();
      set => hiddenStoredObligation = value;
    }

    /// <summary>
    /// A value of HiddenFlowToPeprCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("hiddenFlowToPeprCsePersonsWorkSet")]
    public CsePersonsWorkSet HiddenFlowToPeprCsePersonsWorkSet
    {
      get => hiddenFlowToPeprCsePersonsWorkSet ??= new();
      set => hiddenFlowToPeprCsePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of HiddenFlowToPeprCase.
    /// </summary>
    [JsonPropertyName("hiddenFlowToPeprCase")]
    public Case1 HiddenFlowToPeprCase
    {
      get => hiddenFlowToPeprCase ??= new();
      set => hiddenFlowToPeprCase = value;
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
    /// A value of ObCollProtAct.
    /// </summary>
    [JsonPropertyName("obCollProtAct")]
    public Common ObCollProtAct
    {
      get => obCollProtAct ??= new();
      set => obCollProtAct = value;
    }

    private CsePerson hiddenConcurrentCsePerson;
    private CsePerson hiddenObligor;
    private Code code;
    private Common countryPrompt;
    private CodeValue country;
    private CsePersonsWorkSet hiddenAltAddress;
    private InterstateRequest hiddenInterstateRequest;
    private ObligationType hiddenConcurrentObligationType;
    private DateWorkArea displayedObligCreateDt;
    private DebtDetail hiddenStoredDebtDetail;
    private TextWorkArea returnFlowFrom;
    private DebtDetail hiddenDisplayed;
    private CsePerson passAr;
    private CsePersonsWorkSet passSupported;
    private ObligationTransaction pass;
    private SpTextWorkArea flowSpTextWorkArea;
    private CsePersonAccount flowCsePersonAccount;
    private Case1 case1;
    private TextWorkArea payeePrompt;
    private CsePersonsWorkSet zdelExportDesigPayee;
    private CsePersonsWorkSet altAddress;
    private Common altAddrPrompt;
    private TextWorkArea formattedAlternateAddre;
    private LegalActionDetail legalActionDetail;
    private Standard standard;
    private CsePerson obligorCsePerson;
    private CsePersonsWorkSet obligorCsePersonsWorkSet;
    private CsePerson concurrentCsePerson;
    private CsePersonsWorkSet concurrentCsePersonsWorkSet;
    private Obligation obligation;
    private ObligationPaymentSchedule obligationPaymentSchedule;
    private Obligation concurrentObligation;
    private DebtDetail header;
    private DebtDetail previous;
    private ObligationType obligationType;
    private LegalAction legalAction;
    private Common obligationActive;
    private Common manualDistributionInd;
    private Common paymentScheduleInd;
    private Common suspendInterestInd;
    private Common obligationAmt;
    private Common balanceOwed;
    private Common interestOwed;
    private Common totalOwed;
    private NextTranInfo hiddenNextTranInfo;
    private CsePersonsWorkSet dlgflwAr;
    private InterstateRequest interstateRequest;
    private Array<ExportGroup> export1;
    private Obligation hiddenStoredObligation;
    private CsePersonsWorkSet hiddenFlowToPeprCsePersonsWorkSet;
    private Case1 hiddenFlowToPeprCase;
    private SpTextWorkArea protectQuestionLiteral;
    private Common collProtAnswer;
    private Common obCollProtAct;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local: IInitializable
  {
    /// <summary>
    /// A value of ObligationRln.
    /// </summary>
    [JsonPropertyName("obligationRln")]
    public ObligationRln ObligationRln
    {
      get => obligationRln ??= new();
      set => obligationRln = value;
    }

    /// <summary>
    /// A value of Supported.
    /// </summary>
    [JsonPropertyName("supported")]
    public CsePersonsWorkSet Supported
    {
      get => supported ??= new();
      set => supported = value;
    }

    /// <summary>
    /// A value of Null1.
    /// </summary>
    [JsonPropertyName("null1")]
    public Obligation Null1
    {
      get => null1 ??= new();
      set => null1 = value;
    }

    /// <summary>
    /// A value of TestDup.
    /// </summary>
    [JsonPropertyName("testDup")]
    public Common TestDup
    {
      get => testDup ??= new();
      set => testDup = value;
    }

    /// <summary>
    /// A value of HardcodeOtInterestJudgment.
    /// </summary>
    [JsonPropertyName("hardcodeOtInterestJudgment")]
    public ObligationType HardcodeOtInterestJudgment
    {
      get => hardcodeOtInterestJudgment ??= new();
      set => hardcodeOtInterestJudgment = value;
    }

    /// <summary>
    /// A value of HardcodeOtCMedicalClassifica.
    /// </summary>
    [JsonPropertyName("hardcodeOtCMedicalClassifica")]
    public ObligationType HardcodeOtCMedicalClassifica
    {
      get => hardcodeOtCMedicalClassifica ??= new();
      set => hardcodeOtCMedicalClassifica = value;
    }

    /// <summary>
    /// A value of HardcodeObligJointSeveralCon.
    /// </summary>
    [JsonPropertyName("hardcodeObligJointSeveralCon")]
    public Obligation HardcodeObligJointSeveralCon
    {
      get => hardcodeObligJointSeveralCon ??= new();
      set => hardcodeObligJointSeveralCon = value;
    }

    /// <summary>
    /// A value of HardcodeObligorLegalActionPerson.
    /// </summary>
    [JsonPropertyName("hardcodeObligorLegalActionPerson")]
    public LegalActionPerson HardcodeObligorLegalActionPerson
    {
      get => hardcodeObligorLegalActionPerson ??= new();
      set => hardcodeObligorLegalActionPerson = value;
    }

    /// <summary>
    /// A value of HardcodePgmAdcFosterCare.
    /// </summary>
    [JsonPropertyName("hardcodePgmAdcFosterCare")]
    public ProgramScreenAttributes HardcodePgmAdcFosterCare
    {
      get => hardcodePgmAdcFosterCare ??= new();
      set => hardcodePgmAdcFosterCare = value;
    }

    /// <summary>
    /// A value of HardcodePgmAdc.
    /// </summary>
    [JsonPropertyName("hardcodePgmAdc")]
    public ProgramScreenAttributes HardcodePgmAdc
    {
      get => hardcodePgmAdc ??= new();
      set => hardcodePgmAdc = value;
    }

    /// <summary>
    /// A value of HardcodePgmNonAdcFosterCare.
    /// </summary>
    [JsonPropertyName("hardcodePgmNonAdcFosterCare")]
    public ProgramScreenAttributes HardcodePgmNonAdcFosterCare
    {
      get => hardcodePgmNonAdcFosterCare ??= new();
      set => hardcodePgmNonAdcFosterCare = value;
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
    /// A value of HardcodeOtrnDtVoluntary.
    /// </summary>
    [JsonPropertyName("hardcodeOtrnDtVoluntary")]
    public ObligationTransaction HardcodeOtrnDtVoluntary
    {
      get => hardcodeOtrnDtVoluntary ??= new();
      set => hardcodeOtrnDtVoluntary = value;
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
    /// A value of HardcodeOtCFeesClassificatio.
    /// </summary>
    [JsonPropertyName("hardcodeOtCFeesClassificatio")]
    public ObligationType HardcodeOtCFeesClassificatio
    {
      get => hardcodeOtCFeesClassificatio ??= new();
      set => hardcodeOtCFeesClassificatio = value;
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
    /// A value of HardcodeOrrJointSeveral.
    /// </summary>
    [JsonPropertyName("hardcodeOrrJointSeveral")]
    public ObligationRlnRsn HardcodeOrrJointSeveral
    {
      get => hardcodeOrrJointSeveral ??= new();
      set => hardcodeOrrJointSeveral = value;
    }

    /// <summary>
    /// A value of HardcodeCpaSupportedPerson.
    /// </summary>
    [JsonPropertyName("hardcodeCpaSupportedPerson")]
    public CsePersonAccount HardcodeCpaSupportedPerson
    {
      get => hardcodeCpaSupportedPerson ??= new();
      set => hardcodeCpaSupportedPerson = value;
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
    /// A value of HardcodeOtrrConcurrentObliga.
    /// </summary>
    [JsonPropertyName("hardcodeOtrrConcurrentObliga")]
    public ObligationTransactionRlnRsn HardcodeOtrrConcurrentObliga
    {
      get => hardcodeOtrrConcurrentObliga ??= new();
      set => hardcodeOtrrConcurrentObliga = value;
    }

    /// <summary>
    /// A value of Bypass.
    /// </summary>
    [JsonPropertyName("bypass")]
    public Common Bypass
    {
      get => bypass ??= new();
      set => bypass = value;
    }

    /// <summary>
    /// A value of PriorToUpdate.
    /// </summary>
    [JsonPropertyName("priorToUpdate")]
    public Obligation PriorToUpdate
    {
      get => priorToUpdate ??= new();
      set => priorToUpdate = value;
    }

    /// <summary>
    /// A value of NoOfInvDsgPayeePrompt.
    /// </summary>
    [JsonPropertyName("noOfInvDsgPayeePrompt")]
    public Common NoOfInvDsgPayeePrompt
    {
      get => noOfInvDsgPayeePrompt ??= new();
      set => noOfInvDsgPayeePrompt = value;
    }

    /// <summary>
    /// A value of NoOfValDsgPayeePrompt.
    /// </summary>
    [JsonPropertyName("noOfValDsgPayeePrompt")]
    public Common NoOfValDsgPayeePrompt
    {
      get => noOfValDsgPayeePrompt ??= new();
      set => noOfValDsgPayeePrompt = value;
    }

    /// <summary>
    /// A value of ForRefreshPgm.
    /// </summary>
    [JsonPropertyName("forRefreshPgm")]
    public CsePersonAccount ForRefreshPgm
    {
      get => forRefreshPgm ??= new();
      set => forRefreshPgm = value;
    }

    /// <summary>
    /// A value of Found.
    /// </summary>
    [JsonPropertyName("found")]
    public Common Found
    {
      get => found ??= new();
      set => found = value;
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
    /// A value of Current.
    /// </summary>
    [JsonPropertyName("current")]
    public DateWorkArea Current
    {
      get => current ??= new();
      set => current = value;
    }

    /// <summary>
    /// A value of HardcodeAccruing.
    /// </summary>
    [JsonPropertyName("hardcodeAccruing")]
    public ObligationType HardcodeAccruing
    {
      get => hardcodeAccruing ??= new();
      set => hardcodeAccruing = value;
    }

    /// <summary>
    /// A value of HardcodedVoluntary.
    /// </summary>
    [JsonPropertyName("hardcodedVoluntary")]
    public ObligationType HardcodedVoluntary
    {
      get => hardcodedVoluntary ??= new();
      set => hardcodedVoluntary = value;
    }

    /// <summary>
    /// A value of HardcodedRecovery.
    /// </summary>
    [JsonPropertyName("hardcodedRecovery")]
    public ObligationType HardcodedRecovery
    {
      get => hardcodedRecovery ??= new();
      set => hardcodedRecovery = value;
    }

    /// <summary>
    /// A value of Concurr.
    /// </summary>
    [JsonPropertyName("concurr")]
    public Common Concurr
    {
      get => concurr ??= new();
      set => concurr = value;
    }

    /// <summary>
    /// A value of MaxAssigned.
    /// </summary>
    [JsonPropertyName("maxAssigned")]
    public DateWorkArea MaxAssigned
    {
      get => maxAssigned ??= new();
      set => maxAssigned = value;
    }

    /// <summary>
    /// A value of ObExists.
    /// </summary>
    [JsonPropertyName("obExists")]
    public Common ObExists
    {
      get => obExists ??= new();
      set => obExists = value;
    }

    /// <summary>
    /// A value of SelectionCount.
    /// </summary>
    [JsonPropertyName("selectionCount")]
    public Common SelectionCount
    {
      get => selectionCount ??= new();
      set => selectionCount = value;
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
    /// A value of ConcurrentObligationTransaction.
    /// </summary>
    [JsonPropertyName("concurrentObligationTransaction")]
    public ObligationTransaction ConcurrentObligationTransaction
    {
      get => concurrentObligationTransaction ??= new();
      set => concurrentObligationTransaction = value;
    }

    /// <summary>
    /// A value of ConcurrentCommon.
    /// </summary>
    [JsonPropertyName("concurrentCommon")]
    public Common ConcurrentCommon
    {
      get => concurrentCommon ??= new();
      set => concurrentCommon = value;
    }

    /// <summary>
    /// A value of Action.
    /// </summary>
    [JsonPropertyName("action")]
    public Common Action
    {
      get => action ??= new();
      set => action = value;
    }

    /// <summary>
    /// A value of HardcodeOtrnTDebt.
    /// </summary>
    [JsonPropertyName("hardcodeOtrnTDebt")]
    public ObligationTransaction HardcodeOtrnTDebt
    {
      get => hardcodeOtrnTDebt ??= new();
      set => hardcodeOtrnTDebt = value;
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
    /// A value of ProtectAll.
    /// </summary>
    [JsonPropertyName("protectAll")]
    public Common ProtectAll
    {
      get => protectAll ??= new();
      set => protectAll = value;
    }

    /// <summary>
    /// A value of HardcodeObligorCsePersonAccount.
    /// </summary>
    [JsonPropertyName("hardcodeObligorCsePersonAccount")]
    public CsePersonAccount HardcodeObligorCsePersonAccount
    {
      get => hardcodeObligorCsePersonAccount ??= new();
      set => hardcodeObligorCsePersonAccount = value;
    }

    /// <summary>
    /// A value of HardcodeOtCNonAccruingClass.
    /// </summary>
    [JsonPropertyName("hardcodeOtCNonAccruingClass")]
    public ObligationType HardcodeOtCNonAccruingClass
    {
      get => hardcodeOtCNonAccruingClass ??= new();
      set => hardcodeOtCNonAccruingClass = value;
    }

    /// <summary>
    /// A value of HardcodeSupported.
    /// </summary>
    [JsonPropertyName("hardcodeSupported")]
    public LegalActionPerson HardcodeSupported
    {
      get => hardcodeSupported ??= new();
      set => hardcodeSupported = value;
    }

    /// <summary>
    /// A value of HardcodeSpousalArrears.
    /// </summary>
    [JsonPropertyName("hardcodeSpousalArrears")]
    public ObligationType HardcodeSpousalArrears
    {
      get => hardcodeSpousalArrears ??= new();
      set => hardcodeSpousalArrears = value;
    }

    /// <summary>
    /// A value of BlankDateWorkArea.
    /// </summary>
    [JsonPropertyName("blankDateWorkArea")]
    public DateWorkArea BlankDateWorkArea
    {
      get => blankDateWorkArea ??= new();
      set => blankDateWorkArea = value;
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
    /// A value of Accumulate.
    /// </summary>
    [JsonPropertyName("accumulate")]
    public Common Accumulate
    {
      get => accumulate ??= new();
      set => accumulate = value;
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
    /// A value of NoOfPromptsSelected.
    /// </summary>
    [JsonPropertyName("noOfPromptsSelected")]
    public Common NoOfPromptsSelected
    {
      get => noOfPromptsSelected ??= new();
      set => noOfPromptsSelected = value;
    }

    /// <summary>
    /// A value of ToRead.
    /// </summary>
    [JsonPropertyName("toRead")]
    public CaseRole ToRead
    {
      get => toRead ??= new();
      set => toRead = value;
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
    /// A value of CursorPosition.
    /// </summary>
    [JsonPropertyName("cursorPosition")]
    public CursorPosition CursorPosition
    {
      get => cursorPosition ??= new();
      set => cursorPosition = value;
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
    /// A value of BlankObligation.
    /// </summary>
    [JsonPropertyName("blankObligation")]
    public Obligation BlankObligation
    {
      get => blankObligation ??= new();
      set => blankObligation = value;
    }

    /// <summary>
    /// A value of ProgramFieldError.
    /// </summary>
    [JsonPropertyName("programFieldError")]
    public Common ProgramFieldError
    {
      get => programFieldError ??= new();
      set => programFieldError = value;
    }

    /// <summary>
    /// A value of CountSelected.
    /// </summary>
    [JsonPropertyName("countSelected")]
    public Common CountSelected
    {
      get => countSelected ??= new();
      set => countSelected = value;
    }

    /// <summary>
    /// A value of FlowToPepr.
    /// </summary>
    [JsonPropertyName("flowToPepr")]
    public Common FlowToPepr
    {
      get => flowToPepr ??= new();
      set => flowToPepr = value;
    }

    /// <summary>
    /// A value of SelectedSupported.
    /// </summary>
    [JsonPropertyName("selectedSupported")]
    public CsePersonsWorkSet SelectedSupported
    {
      get => selectedSupported ??= new();
      set => selectedSupported = value;
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
    /// A value of IvdAgency.
    /// </summary>
    [JsonPropertyName("ivdAgency")]
    public Common IvdAgency
    {
      get => ivdAgency ??= new();
      set => ivdAgency = value;
    }

    /// <para>Resets the state.</para>
    void IInitializable.Initialize()
    {
      obligationRln = null;
      supported = null;
      null1 = null;
      testDup = null;
      hardcodeOtInterestJudgment = null;
      hardcodeOtCMedicalClassifica = null;
      hardcodeObligJointSeveralCon = null;
      hardcodeObligorLegalActionPerson = null;
      hardcodePgmAdcFosterCare = null;
      hardcodePgmAdc = null;
      hardcodePgmNonAdcFosterCare = null;
      hardcodeOt718BUraJudgement = null;
      hardcodeOtrnDtVoluntary = null;
      hardcodeOtrnDtAccrual = null;
      hardcodeOtCFeesClassificatio = null;
      hardcodeOtrnDtDebtDetail = null;
      bypass = null;
      priorToUpdate = null;
      noOfInvDsgPayeePrompt = null;
      noOfValDsgPayeePrompt = null;
      forRefreshPgm = null;
      found = null;
      fips = null;
      current = null;
      concurr = null;
      obExists = null;
      selectionCount = null;
      textWorkArea = null;
      concurrentObligationTransaction = null;
      concurrentCommon = null;
      action = null;
      obligation = null;
      protectAll = null;
      blankDateWorkArea = null;
      accumulate = null;
      infrastructure = null;
      fromHistMonaNxttran = null;
      noOfPromptsSelected = null;
      toRead = null;
      interstateRequest = null;
      cursorPosition = null;
      eab = null;
      blankObligation = null;
      programFieldError = null;
      countSelected = null;
      flowToPepr = null;
      selectedSupported = null;
      obligCollProtectionHist = null;
      obCollProtHistCreated = null;
      collsFndToProtect = null;
      common = null;
      ivdAgency = null;
    }

    private ObligationRln obligationRln;
    private CsePersonsWorkSet supported;
    private Obligation null1;
    private Common testDup;
    private ObligationType hardcodeOtInterestJudgment;
    private ObligationType hardcodeOtCMedicalClassifica;
    private Obligation hardcodeObligJointSeveralCon;
    private LegalActionPerson hardcodeObligorLegalActionPerson;
    private ProgramScreenAttributes hardcodePgmAdcFosterCare;
    private ProgramScreenAttributes hardcodePgmAdc;
    private ProgramScreenAttributes hardcodePgmNonAdcFosterCare;
    private ObligationType hardcodeOt718BUraJudgement;
    private ObligationTransaction hardcodeOtrnDtVoluntary;
    private ObligationTransaction hardcodeOtrnDtAccrual;
    private ObligationType hardcodeOtCFeesClassificatio;
    private ObligationTransaction hardcodeOtrnDtDebtDetail;
    private ObligationRlnRsn hardcodeOrrJointSeveral;
    private CsePersonAccount hardcodeCpaSupportedPerson;
    private DebtDetailStatusHistory hardcodeDdshActiveStatus;
    private ObligationTransactionRlnRsn hardcodeOtrrConcurrentObliga;
    private Common bypass;
    private Obligation priorToUpdate;
    private Common noOfInvDsgPayeePrompt;
    private Common noOfValDsgPayeePrompt;
    private CsePersonAccount forRefreshPgm;
    private Common found;
    private Fips fips;
    private DateWorkArea current;
    private ObligationType hardcodeAccruing;
    private ObligationType hardcodedVoluntary;
    private ObligationType hardcodedRecovery;
    private Common concurr;
    private DateWorkArea maxAssigned;
    private Common obExists;
    private Common selectionCount;
    private TextWorkArea textWorkArea;
    private ObligationTransaction concurrentObligationTransaction;
    private Common concurrentCommon;
    private Common action;
    private ObligationTransaction hardcodeOtrnTDebt;
    private Obligation obligation;
    private Common protectAll;
    private CsePersonAccount hardcodeObligorCsePersonAccount;
    private ObligationType hardcodeOtCNonAccruingClass;
    private LegalActionPerson hardcodeSupported;
    private ObligationType hardcodeSpousalArrears;
    private DateWorkArea blankDateWorkArea;
    private DateWorkArea max;
    private Common accumulate;
    private Infrastructure infrastructure;
    private Common fromHistMonaNxttran;
    private Common noOfPromptsSelected;
    private CaseRole toRead;
    private InterstateRequest interstateRequest;
    private CursorPosition cursorPosition;
    private AbendData eab;
    private Obligation blankObligation;
    private Common programFieldError;
    private Common countSelected;
    private Common flowToPepr;
    private CsePersonsWorkSet selectedSupported;
    private ObligCollProtectionHist obligCollProtectionHist;
    private Common obCollProtHistCreated;
    private Common collsFndToProtect;
    private Common common;
    private Common ivdAgency;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of PersonProgram.
    /// </summary>
    [JsonPropertyName("personProgram")]
    public PersonProgram PersonProgram
    {
      get => personProgram ??= new();
      set => personProgram = value;
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
    /// A value of ObligorCsePerson.
    /// </summary>
    [JsonPropertyName("obligorCsePerson")]
    public CsePerson ObligorCsePerson
    {
      get => obligorCsePerson ??= new();
      set => obligorCsePerson = value;
    }

    /// <summary>
    /// A value of ConcurrentObligor.
    /// </summary>
    [JsonPropertyName("concurrentObligor")]
    public CsePerson ConcurrentObligor
    {
      get => concurrentObligor ??= new();
      set => concurrentObligor = value;
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
    /// A value of CsePersonAccount.
    /// </summary>
    [JsonPropertyName("csePersonAccount")]
    public CsePersonAccount CsePersonAccount
    {
      get => csePersonAccount ??= new();
      set => csePersonAccount = value;
    }

    /// <summary>
    /// A value of Alternate.
    /// </summary>
    [JsonPropertyName("alternate")]
    public CsePerson Alternate
    {
      get => alternate ??= new();
      set => alternate = value;
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
    /// A value of ObligationTransaction.
    /// </summary>
    [JsonPropertyName("obligationTransaction")]
    public ObligationTransaction ObligationTransaction
    {
      get => obligationTransaction ??= new();
      set => obligationTransaction = value;
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
    /// A value of SupportedPersonCaseUnit.
    /// </summary>
    [JsonPropertyName("supportedPersonCaseUnit")]
    public CaseUnit SupportedPersonCaseUnit
    {
      get => supportedPersonCaseUnit ??= new();
      set => supportedPersonCaseUnit = value;
    }

    /// <summary>
    /// A value of Supported.
    /// </summary>
    [JsonPropertyName("supported")]
    public CsePerson Supported
    {
      get => supported ??= new();
      set => supported = value;
    }

    /// <summary>
    /// A value of Ap.
    /// </summary>
    [JsonPropertyName("ap")]
    public CaseRole Ap
    {
      get => ap ??= new();
      set => ap = value;
    }

    /// <summary>
    /// A value of SupportedPersonCaseRole.
    /// </summary>
    [JsonPropertyName("supportedPersonCaseRole")]
    public CaseRole SupportedPersonCaseRole
    {
      get => supportedPersonCaseRole ??= new();
      set => supportedPersonCaseRole = value;
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
    /// A value of ObligCollProtectionHist.
    /// </summary>
    [JsonPropertyName("obligCollProtectionHist")]
    public ObligCollProtectionHist ObligCollProtectionHist
    {
      get => obligCollProtectionHist ??= new();
      set => obligCollProtectionHist = value;
    }

    private PersonProgram personProgram;
    private ObligationType obligationType;
    private CsePerson obligorCsePerson;
    private CsePerson concurrentObligor;
    private Obligation obligation;
    private CsePersonAccount csePersonAccount;
    private CsePerson alternate;
    private DebtDetail debtDetail;
    private ObligationTransaction obligationTransaction;
    private CsePersonAccount obligorCsePersonAccount;
    private Case1 case1;
    private CaseUnit supportedPersonCaseUnit;
    private CsePerson supported;
    private CaseRole ap;
    private CaseRole supportedPersonCaseRole;
    private Collection collection;
    private ObligationTransaction debt;
    private Obligation otherView;
    private LegalAction legalAction;
    private ObligCollProtectionHist obligCollProtectionHist;
  }
#endregion
}
