// Program: FN_REIP_REC_INDIVIDUAL_PMNT_HIST, ID: 372418329, model: 746.
// Short name: SWEREIPP
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
/// A program: FN_REIP_REC_INDIVIDUAL_PMNT_HIST.
/// </para>
/// <para>
/// Resp: Finance		
/// This PRAD is designed to list individual payment History.
/// </para>
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class FnReipRecIndividualPmntHist: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_REIP_REC_INDIVIDUAL_PMNT_HIST program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnReipRecIndividualPmntHist(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnReipRecIndividualPmntHist.
  /// </summary>
  public FnReipRecIndividualPmntHist(IContext context, Import import,
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
    // -------------------------------------------------------------------------
    // Every initial development and change to that development needs
    // to be documented.
    // -------------------------------------------------------------------------
    // Date	   Programmer		Reason #	Description
    // -------------------------------------------------------------------------
    // 01/12/95   Holly Kennedy-MTW			Source
    // 02/01/96   Holly Kennedy-MTW			Added logic to populate
    // 						CSE Person field upon
    // 
    // Return from the prompt.
    // 02/08/96   Holly Kennedy-MTW			Retrofit
    // 10/16/96   Holly Kennedy-MTW			Retrofitted data
    //                                                 
    // level security
    // 03/05/97   Siraj Konkader	IDCR 283       	Fixed flow to name.
    // 			                        Made changes to
    //                                                 
    // return from OPAY.
    // 04/22/97   Holly Kennedy                    	Combined REIP and
    //                                                 
    // RERE into one screen
    // 05/16/97   Holly Kennedy	IDCR 313
    // 06-18-97   govind				Cleaned up the nested
    // 						structures; removed
    // 						online distribution;
    // 10-02-97   JeHoward 		PR#26309	Removed all source type
    // 	Parker/Newman				edits
    // 				PR#29128	Fixed flow on return to
    // 						DDMM;
    // 				IDCR#381	show all payments in 'REC'
    // 						status.
    // 						Added PF Key to put pymts
    // 						in 'REL' Status.
    // 12/30/97  Syed Hasan,MTW	PR# 32883	Modified display logic
    // 						to show Pmt Total as
    // 						total for all lines
    // 						selected through filters
    // 						regardless of whether those
    // 						are in the export view or 						not.
    // 01/31/98  Syed Hasan, MTW	PR# 36367	Modified code to correctly
    // 						perform update function as
    // 						required on problem report.
    // -------------------------------------------------------------------------
    // REIP REDESIGN and INTEGRATION TEST MODIFICATIONS
    // -------------------------------------------------------------------------
    // Date	   Programmer		Description
    // -------------------------------------------------------------------------
    // 12/02/98  J. Katz - SRG		Modify logic to support the business
    // 				redesign of REIP including support for
    // 				data migration from KAECSES.
    // 01/22/99  J. Katz		Modified error handling on Display action.
    // 05/14/99  J. Katz		Added validation logic to display error
    // 				message if collection type code 7, 8, or
    // 				9 are selected for an add or update action.
    // 06/08/99  J. Katz		Analyzed READ statements in underlying
    // 				action blocks and changed read property
    // 				to Select Only where appropriate.
    // -------------------------------------------------------------------------
    // 01/07/00  P. Phinney  H00083731  Prevent Dates prior to the
    // Collection Effective Date.
    // 01/07/00  P. Phinney  H00083299  Prevent Future Dates when Updating.
    // 01/31/00  P. Phinney  H00084245  Add PFkey to change Susp to Adj
    // 02/02/00  P. Phinney  H00086708  Changed message that displays when PF17 
    // is pressed
    // 03/28/00  P. Phinney  H00091186  Add code to verify if Person is on ANY
    // Legal Action with supplied STANDARD NUMBER.
    // M. Brown, March 29, 2001; pr# 111813 - Next Tran.
    // 06/12/01  P. Phinney  I00121505  Removed BLOCK of Collection Types 7, 8, 
    // 9 - CSENet
    // -------------------------------------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";

    if (Equal(global.Command, "CLEAR") || Equal(global.Command, "DMEN"))
    {
      return;
    }

    // M. Brown, March 29, 2001; pr# 111813 - Moved these stmts  here, as they 
    // were being executed after the next tran logic, and overwriting court
    // order and person numbers.
    export.CsePerson.Number = import.CsePerson.Number;
    export.CsePersonsWorkSet.Assign(import.CsePersonsWorkSet);
    export.HiddenCsePersonsWorkSet.Number =
      import.HiddenCsePersonsWorkSet.Number;
    export.HiddenNextTranInfo.Assign(import.HiddenNextTranInfo);
    export.Standard.NextTransaction = import.Standard.NextTransaction;
    MoveLegalAction(import.LegalAction, export.LegalAction);
    MoveLegalAction(import.HiddenLegalAction, export.HiddenLegalAction);
    export.CourtOrder.PromptField = import.CourtOrder.PromptField;

    // -------------------------------------------------------------------
    // NEXT TRAN LOGIC
    // If the next tran info is not equal to spaces, the user requested
    // a next tran action.  Now validate.
    // -------------------------------------------------------------------
    if (!IsEmpty(import.Standard.NextTransaction))
    {
      export.HiddenNextTranInfo.CsePersonNumberAp = import.CsePerson.Number;

      // M. Brown, March 29, 2001; pr# 111813 - added set of other next tran 
      // fields.
      export.HiddenNextTranInfo.CsePersonNumber = import.CsePerson.Number;
      export.HiddenNextTranInfo.CsePersonNumberObligor =
        import.CsePerson.Number;
      export.HiddenNextTranInfo.StandardCrtOrdNumber =
        export.LegalAction.StandardNumber ?? "";
      export.HiddenNextTranInfo.LegalActionIdentifier =
        export.LegalAction.Identifier;
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

      // M. Brown, March 29, 2001; pr# 111813 - added further 'IF', in case
      // next tran cse_person_number is the only person # set.
      if (!IsEmpty(export.HiddenNextTranInfo.CsePersonNumberAp))
      {
        export.CsePerson.Number =
          export.HiddenNextTranInfo.CsePersonNumberAp ?? Spaces(10);
      }
      else if (!IsEmpty(export.HiddenNextTranInfo.CsePersonNumberObligor))
      {
        export.CsePerson.Number =
          export.HiddenNextTranInfo.CsePersonNumberObligor ?? Spaces(10);
      }
      else if (!IsEmpty(export.HiddenNextTranInfo.CsePersonNumber))
      {
        export.CsePerson.Number = export.HiddenNextTranInfo.CsePersonNumber ?? Spaces
          (10);
      }

      if (!IsEmpty(export.HiddenNextTranInfo.StandardCrtOrdNumber))
      {
        export.LegalAction.StandardNumber =
          export.HiddenNextTranInfo.StandardCrtOrdNumber ?? "";
      }

      global.Command = "DISPLAY";
    }

    // -----------------------------------------------------------------
    // Security Logic       10/15/96
    // Need to also secure the command RELEASE and MANDIST
    // -----------------------------------------------------------------
    // 01/31/00  P. Phinney  H00084245  Add PFkey to change Susp to Adj
    // HERE * * * *   REMOVED for testing
    if (Equal(global.Command, "DISPLAY") || Equal(global.Command, "ADD") || Equal
      (global.Command, "UPDATE") || Equal(global.Command, "DELETE") || Equal
      (global.Command, "MANDIST") || Equal(global.Command, "MIGRATE") || Equal
      (global.Command, "RELEASE"))
    {
      UseScCabTestSecurity();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }
    }

    // --------------------------------------------------------
    // Set up local work views.
    // --------------------------------------------------------
    local.Current.Date = Now().Date;
    local.Low.Date = new DateTime(1, 1, 1);
    UseFnHardcodedCashReceipting();

    // -------------------------------------
    // Move import to exports
    // -------------------------------------
    export.Payor.PromptField = import.Payor.PromptField;
    export.ListStarting.CollectionDate = import.ListStarting.CollectionDate;
    export.ListEnding.CollectionDate = import.ListEnding.CollectionDate;
    export.TotalItems.Count = import.TotalItems.Count;
    export.TotalPmt.TotalCurrency = import.TotalPmt.TotalCurrency;
    export.TotalDist.TotalCurrency = import.TotalDist.TotalCurrency;
    export.TotalRef.TotalCurrency = import.TotalRef.TotalCurrency;
    export.TotalAdj.TotalCurrency = import.TotalAdj.TotalCurrency;
    export.TotalUndist.TotalCurrency = import.TotalUndist.TotalCurrency;
    export.UndistCruc.PromptField = import.UndistCruc.PromptField;
    export.Continue1.Flag = import.Continue1.Flag;

    export.Export1.Index = 0;
    export.Export1.Clear();

    for(import.Import1.Index = 0; import.Import1.Index < import.Import1.Count; ++
      import.Import1.Index)
    {
      if (export.Export1.IsFull)
      {
        break;
      }

      export.Export1.Update.DetailCommon.SelectChar =
        import.Import1.Item.DetailCommon.SelectChar;
      export.Export1.Update.DetailCashReceiptDetail.Assign(
        import.Import1.Item.DetailCashReceiptDetail);
      export.Export1.Update.RecurringToDate.CollectionDate =
        import.Import1.Item.RecurringToDate.CollectionDate;
      export.Export1.Update.Frequency.Text4 =
        import.Import1.Item.Frequency.Text4;
      export.Export1.Update.DetailCourtOrDp.SelectChar =
        import.Import1.Item.DetailCourtOrDp.SelectChar;
      MoveCashReceiptSourceType(import.Import1.Item.DetailCashReceiptSourceType,
        export.Export1.Update.DetailCashReceiptSourceType);
      export.Export1.Update.DetSourceCode.PromptField =
        import.Import1.Item.DetSourceCode.PromptField;
      MoveCollectionType(import.Import1.Item.DetailCollectionType,
        export.Export1.Update.DetailCollectionType);
      export.Export1.Update.DetCollTypPrompt.PromptField =
        import.Import1.Item.DetCollTypPrompt.PromptField;
      export.Export1.Update.DetailCashReceipt.Assign(
        import.Import1.Item.DetailCashReceipt);
      export.Export1.Update.DetailCashReceiptDetailStatus.Code =
        import.Import1.Item.DetailCashReceiptDetailStatus.Code;
      export.Export1.Update.DetailManDistInd.Flag =
        import.Import1.Item.DetailManDistInd.Flag;
      export.Export1.Update.DetailNoteInd.Flag =
        import.Import1.Item.DetailNoteInd.Flag;
      export.Export1.Update.HiddenCashReceiptEvent.SystemGeneratedIdentifier =
        import.Import1.Item.HiddenCashReceiptEvent.SystemGeneratedIdentifier;
      export.Export1.Update.HiddenCashReceiptType.SystemGeneratedIdentifier =
        import.Import1.Item.HiddenCashReceiptType.SystemGeneratedIdentifier;

      if (export.Export1.Item.DetailCashReceipt.SequentialNumber > 0)
      {
        var field1 =
          GetField(export.Export1.Item.RecurringToDate, "collectionDate");

        field1.Color = "cyan";
        field1.Protected = true;

        var field2 = GetField(export.Export1.Item.Frequency, "text4");

        field2.Color = "cyan";
        field2.Protected = true;

        var field3 =
          GetField(export.Export1.Item.DetailCourtOrDp, "selectChar");

        field3.Color = "cyan";
        field3.Protected = true;

        var field4 =
          GetField(export.Export1.Item.DetailCashReceiptSourceType, "code");

        field4.Color = "cyan";
        field4.Protected = true;

        var field5 = GetField(export.Export1.Item.DetSourceCode, "promptField");

        field5.Color = "cyan";
        field5.Protected = true;

        // ------------------------------------------------------------
        // If the current detail status is ADJ or at least part of
        // the collection amount has been distributed or refunded,
        // the payment history record cannot be changed.
        // JLK  03/23/99
        // ------------------------------------------------------------
        if (Equal(export.Export1.Item.DetailCashReceiptDetailStatus.Code, "ADJ") ||
          export
          .Export1.Item.DetailCashReceiptDetail.DistributedAmount.
            GetValueOrDefault() > 0 || export
          .Export1.Item.DetailCashReceiptDetail.RefundedAmount.
            GetValueOrDefault() > 0)
        {
          var field6 =
            GetField(export.Export1.Item.DetailCashReceiptDetail,
            "collectionDate");

          field6.Color = "cyan";
          field6.Protected = true;

          var field7 =
            GetField(export.Export1.Item.DetailCashReceiptDetail,
            "collectionAmount");

          field7.Color = "cyan";
          field7.Protected = true;

          var field8 =
            GetField(export.Export1.Item.DetailCollectionType, "code");

          field8.Color = "cyan";
          field8.Protected = true;

          var field9 =
            GetField(export.Export1.Item.DetCollTypPrompt, "promptField");

          field9.Color = "cyan";
          field9.Protected = true;
        }
      }

      export.Export1.Next();
    }

    if (AsChar(export.Continue1.Flag) == 'Y' && Equal(global.Command, "ADD"))
    {
      export.Continue1.Flag = "+";
    }
    else if (AsChar(export.Continue1.Flag) == 'Y' && !
      Equal(global.Command, "ADD"))
    {
      export.Continue1.Flag = "";
    }

    // ----------------------------------------------------
    // Validation Edits.
    // ----------------------------------------------------
    if (Equal(global.Command, "DISPLAY"))
    {
      // ----------------------------------------------------------------
      // The AP/Payor Number and Court Order Number are mandatory
      // fields and must have data.
      // ----------------------------------------------------------------
      if (IsEmpty(export.CsePerson.Number))
      {
        var field = GetField(export.CsePerson, "number");

        field.Error = true;

        ExitState = "ACO_NI0000_ENTER_REQUIRED_DATA";
      }

      if (IsEmpty(export.LegalAction.StandardNumber))
      {
        var field = GetField(export.LegalAction, "standardNumber");

        field.Error = true;

        ExitState = "ACO_NI0000_ENTER_REQUIRED_DATA";
      }

      if (IsExitState("ACO_NN0000_ALL_OK"))
      {
        if (!IsEmpty(export.CsePerson.Number))
        {
          // ----------------------------------------------------------------
          // Validate the AP / Payor Number entered.
          // ----------------------------------------------------------------
          if (!Equal(export.HiddenCsePersonsWorkSet.Number,
            export.CsePerson.Number))
          {
            export.HiddenCsePersonsWorkSet.Number = export.CsePerson.Number;
            UseSiReadCsePerson();

            if (IsExitState("ACO_NN0000_ALL_OK"))
            {
              export.HiddenCsePersonsWorkSet.Number = export.CsePerson.Number;
            }
            else
            {
              export.HiddenCsePersonsWorkSet.Number = "";

              var field = GetField(export.CsePerson, "number");

              field.Error = true;
            }
          }
        }
      }

      if (IsExitState("ACO_NN0000_ALL_OK"))
      {
        if (!IsEmpty(export.LegalAction.StandardNumber))
        {
          // --------------------------------------------------------------
          // If a new Court Order was entered, validate court order number.
          // Since the new court order number may have been entered
          // rather than selected from a list, the identifier may not be
          // correct and must be set to 0.
          // --------------------------------------------------------------
          if (!Equal(export.LegalAction.StandardNumber,
            export.HiddenLegalAction.StandardNumber))
          {
            export.LegalAction.Identifier = 0;
            UseFnCabValidateLegalAction();

            if (IsExitState("ACO_NN0000_ALL_OK"))
            {
              MoveLegalAction(export.HiddenLegalAction, export.LegalAction);
            }
            else
            {
              export.HiddenLegalAction.StandardNumber = "";

              var field = GetField(export.LegalAction, "standardNumber");

              field.Error = true;
            }
          }
        }
      }

      if (IsExitState("ACO_NN0000_ALL_OK"))
      {
        // --------------------------------------------------------
        // Validate that the AP/Payor is an obligor for the court
        // order number.
        // --------------------------------------------------------
        // h00091186      03/28/00    PPhinney  -  Add code to verify if Person
        // is on  "ANY"  Legal Action with supplied STANDARD NUMBER.
        export.LegalAction.Identifier = 0;

        // --------------------------------------------------------
        UseFnCabCheckPersOblrInLact();

        if (AsChar(local.PersObligorInLact.Flag) == 'N')
        {
          var field1 = GetField(export.CsePerson, "number");

          field1.Error = true;

          var field2 = GetField(export.LegalAction, "standardNumber");

          field2.Error = true;

          export.HiddenLegalAction.StandardNumber = "xxxxxxxxxx";
          ExitState = "FN0000_PERS_NOT_OBLR_IN_CT_ORD";
        }
      }

      // ----------------------------------------------------------------
      // Blank out group view and escape if any errors were detected for
      // the AP/Payor or Court Order Number.
      // ----------------------------------------------------------------
      if (!IsExitState("ACO_NN0000_ALL_OK"))
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

          // ----- Blank out the group view -----
          export.Export1.Next();
        }

        return;
      }
    }

    // 01/31/00  P. Phinney  H00084245  Add PFkey to change Susp to Adj
    if (Equal(global.Command, "ADD") || Equal(global.Command, "UPDATE") || Equal
      (global.Command, "DELETE") || Equal(global.Command, "MANDIST") || Equal
      (global.Command, "MIGRATE") || Equal(global.Command, "RELEASE") || Equal
      (global.Command, "CRRC") || Equal(global.Command, "CRCN") || Equal
      (global.Command, "ADJUST"))
    {
      // --------------------------------------------------------
      // CSE Person Number and Court Order Number must be entered.
      // --------------------------------------------------------
      if (IsEmpty(export.CsePerson.Number))
      {
        var field = GetField(export.CsePerson, "number");

        field.Error = true;

        ExitState = "ACO_NI0000_ENTER_REQUIRED_DATA";
      }

      if (IsEmpty(export.LegalAction.StandardNumber))
      {
        var field = GetField(export.LegalAction, "standardNumber");

        field.Error = true;

        ExitState = "ACO_NI0000_ENTER_REQUIRED_DATA";
      }

      if (IsExitState("ACO_NN0000_ALL_OK"))
      {
        if (Equal(export.HiddenLegalAction.StandardNumber, "xxxxxxxxxx"))
        {
          var field1 = GetField(export.CsePerson, "number");

          field1.Error = true;

          var field2 = GetField(export.LegalAction, "standardNumber");

          field2.Error = true;

          ExitState = "FN0000_PERS_NOT_OBLR_IN_CT_ORD";
        }
      }

      // --------------------------------------------------------
      // Must display prior to executing these commands.
      // --------------------------------------------------------
      if (IsExitState("ACO_NN0000_ALL_OK"))
      {
        if (!Equal(export.CsePerson.Number,
          export.HiddenCsePersonsWorkSet.Number) || IsEmpty
          (export.HiddenCsePersonsWorkSet.Number))
        {
          var field = GetField(export.CsePerson, "number");

          field.Error = true;

          ExitState = "ACO_NW0000_MUST_DISPLAY_FIRST";
        }

        if (!Equal(export.LegalAction.StandardNumber,
          export.HiddenLegalAction.StandardNumber) || IsEmpty
          (export.HiddenLegalAction.StandardNumber))
        {
          var field = GetField(export.LegalAction, "standardNumber");

          field.Error = true;

          ExitState = "ACO_NW0000_MUST_DISPLAY_FIRST";
        }
      }

      // ----------------------------------------------------------------
      // Blank out group view and escape if a different AP/Payor or
      // Court Order Number was entered since the last display action.
      // ----------------------------------------------------------------
      if (!IsExitState("ACO_NN0000_ALL_OK"))
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

          // ----- Blank out the group view -----
          export.Export1.Next();
        }

        return;
      }
    }

    // 01/31/00  P. Phinney  H00084245  Add PFkey to change Susp to Adj
    if (Equal(global.Command, "UPDATE") || Equal(global.Command, "DELETE") || Equal
      (global.Command, "MANDIST") || Equal(global.Command, "ADJUST"))
    {
      // -------------------------------------------------------------
      // Selected payment history records must already exist on
      // the database.
      // -------------------------------------------------------------
      for(export.Export1.Index = 0; export.Export1.Index < export
        .Export1.Count; ++export.Export1.Index)
      {
        if (AsChar(export.Export1.Item.DetailCommon.SelectChar) == 'S' && export
          .Export1.Item.DetailCashReceipt.SequentialNumber <= 0)
        {
          var field = GetField(export.Export1.Item.DetailCommon, "selectChar");

          field.Error = true;

          ExitState = "FN0000_ADD_B4_UPD_DL_MAN_DIST_RL";

          return;
        }
      }
    }

    // ----------------------------------------------------
    // Case of Command
    // ----------------------------------------------------
    switch(TrimEnd(global.Command))
    {
      case "DISPLAY":
        // ----------------------------------------------------------
        // Validate filter date range.
        // ----------------------------------------------------------
        if (Lt(local.Current.Date, export.ListStarting.CollectionDate))
        {
          ExitState = "FN0000_REIP_INV_LIST_FROM_DATE";

          var field = GetField(export.ListStarting, "collectionDate");

          field.Error = true;

          return;
        }

        if (Lt(local.Current.Date, export.ListEnding.CollectionDate))
        {
          ExitState = "FN0000_REIP_INV_LIST_END_DATE";

          var field = GetField(export.ListEnding, "collectionDate");

          field.Error = true;

          return;
        }

        if (Equal(export.ListStarting.CollectionDate, local.Low.Date))
        {
          export.ListStarting.CollectionDate = local.Null1.Date;
        }

        // 01/07/00  P. Phinney  H00082731  Prevent Invalid Past Dates when 
        // Updating.
        // -------------------------------------------------------------------------
        if (Lt(local.Null1.Date, export.ListStarting.CollectionDate))
        {
          if (Lt(export.ListStarting.CollectionDate, new DateTime(1960, 1, 1)))
          {
            ExitState = "FN0000_COLL_DATE_NOT_VALID";

            var field = GetField(export.ListStarting, "collectionDate");

            field.Error = true;
          }
        }

        if (Lt(local.Null1.Date, export.ListEnding.CollectionDate))
        {
          if (Lt(export.ListEnding.CollectionDate, new DateTime(1960, 1, 1)))
          {
            ExitState = "FN0000_COLL_DATE_NOT_VALID";

            var field = GetField(export.ListEnding, "collectionDate");

            field.Error = true;
          }
        }

        if (IsExitState("FN0000_COLL_DATE_NOT_VALID"))
        {
          return;
        }

        if (Lt(local.Null1.Date, export.ListStarting.CollectionDate) && Lt
          (local.Null1.Date, export.ListEnding.CollectionDate))
        {
          if (Lt(export.ListStarting.CollectionDate,
            export.ListEnding.CollectionDate))
          {
            ExitState = "FN0000_REIP_TO_MUST_BE_LT_FROM";

            var field3 = GetField(export.ListStarting, "collectionDate");

            field3.Error = true;

            var field4 = GetField(export.ListEnding, "collectionDate");

            field4.Error = true;

            return;
          }
        }

        // --------------------------------------------------------------
        // Retrieve list of payment history records.
        // --------------------------------------------------------------
        UseFnReipDisplayPaymentHistory();

        if (Equal(export.ListEnding.CollectionDate, local.Low.Date))
        {
          export.ListEnding.CollectionDate = local.Null1.Date;
        }

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          if (export.Export1.IsFull)
          {
            ExitState = "ACO_NI0000_LIST_IS_FULL";
          }
          else if (export.Export1.IsEmpty)
          {
            ExitState = "FN0000_NO_RECORDS_FOUND";
          }
          else
          {
            ExitState = "ACO_NI0000_DISPLAY_SUCCESSFUL";
          }
        }
        else if (IsExitState("FN0000_LIST_DISPLAYED_WITH_ERROR"))
        {
          // --------------------------------------------------------------
          // Data integrity errors were detected and included in list if
          // they fall within the data range.    JLK  01/22/99
          // --------------------------------------------------------------
          for(export.Export1.Index = 0; export.Export1.Index < export
            .Export1.Count; ++export.Export1.Index)
          {
            if (IsEmpty(export.Export1.Item.DetailCashReceiptSourceType.Code))
            {
              var field =
                GetField(export.Export1.Item.DetailCashReceiptSourceType, "code");
                

              field.Color = "red";
              field.Intensity = Intensity.High;
              field.Highlighting = Highlighting.ReverseVideo;
              field.Protected = true;
            }

            if (IsEmpty(export.Export1.Item.DetailCashReceiptDetailStatus.Code))
            {
              var field =
                GetField(export.Export1.Item.DetailCashReceiptDetailStatus,
                "code");

              field.Color = "red";
              field.Intensity = Intensity.High;
              field.Highlighting = Highlighting.ReverseVideo;
              field.Protected = true;
            }

            if (IsEmpty(export.Export1.Item.DetailCollectionType.Code))
            {
              var field =
                GetField(export.Export1.Item.DetailCollectionType, "code");

              field.Color = "red";
              field.Intensity = Intensity.High;
              field.Highlighting = Highlighting.ReverseVideo;
              field.Protected = false;
            }
          }

          if (export.Export1.IsFull)
          {
            ExitState = "FN0000_LIST_FULL_WITH_ERRORS";
          }
        }
        else
        {
          export.TotalItems.Count = 0;
          export.TotalPmt.TotalCurrency = 0;
          export.TotalDist.TotalCurrency = 0;
          export.TotalRef.TotalCurrency = 0;
          export.TotalAdj.TotalCurrency = 0;
          export.TotalUndist.TotalCurrency = 0;
        }

        for(export.Export1.Index = 0; export.Export1.Index < export
          .Export1.Count; ++export.Export1.Index)
        {
          var field3 =
            GetField(export.Export1.Item.RecurringToDate, "collectionDate");

          field3.Color = "cyan";
          field3.Protected = true;

          var field4 = GetField(export.Export1.Item.Frequency, "text4");

          field4.Color = "cyan";
          field4.Protected = true;

          var field5 =
            GetField(export.Export1.Item.DetailCourtOrDp, "selectChar");

          field5.Color = "cyan";
          field5.Protected = true;

          var field6 =
            GetField(export.Export1.Item.DetailCashReceiptSourceType, "code");

          field6.Color = "cyan";
          field6.Protected = true;

          var field7 =
            GetField(export.Export1.Item.DetSourceCode, "promptField");

          field7.Color = "cyan";
          field7.Protected = true;

          // --------------------------------------------------------
          // If the current detail status is ADJ or at least part of
          // the collection amount has been distributed or refunded,
          // the payment history record cannot be changed.
          // JLK  03/23/99
          // --------------------------------------------------------
          if (Equal(export.Export1.Item.DetailCashReceiptDetailStatus.Code,
            "ADJ") || export
            .Export1.Item.DetailCashReceiptDetail.DistributedAmount.
              GetValueOrDefault() > 0 || export
            .Export1.Item.DetailCashReceiptDetail.RefundedAmount.
              GetValueOrDefault() > 0)
          {
            var field8 =
              GetField(export.Export1.Item.DetailCashReceiptDetail,
              "collectionDate");

            field8.Color = "cyan";
            field8.Protected = true;

            var field9 =
              GetField(export.Export1.Item.DetailCashReceiptDetail,
              "collectionAmount");

            field9.Color = "cyan";
            field9.Protected = true;

            var field10 =
              GetField(export.Export1.Item.DetailCollectionType, "code");

            field10.Color = "cyan";
            field10.Protected = true;

            var field11 =
              GetField(export.Export1.Item.DetCollTypPrompt, "promptField");

            field11.Color = "cyan";
            field11.Protected = true;
          }
        }

        break;
      case "LIST":
        local.Selected.Count = 0;

        switch(AsChar(export.Payor.PromptField))
        {
          case 'S':
            // ---------------------------------------------------------
            // Set the default values for the phonetic search on the
            // Name screen.  JLK  01/16/99
            // ---------------------------------------------------------
            ++local.Selected.Count;
            export.Phonetic.Percentage = 35;
            export.Phonetic.Flag = "Y";

            var field3 = GetField(export.Payor, "promptField");

            field3.Error = true;

            ExitState = "ECO_LNK_TO_CSE_PERSON_NAME_LIST";

            break;
          case ' ':
            break;
          default:
            var field4 = GetField(export.Payor, "promptField");

            field4.Error = true;

            ExitState = "ACO_NE0000_INVALID_SELECT_CODE";

            return;
        }

        switch(AsChar(export.CourtOrder.PromptField))
        {
          case 'S':
            ++local.Selected.Count;

            var field3 = GetField(export.CourtOrder, "promptField");

            field3.Error = true;

            if (IsEmpty(export.CsePersonsWorkSet.Number))
            {
              export.CsePersonsWorkSet.Number = export.CsePerson.Number;
            }

            ExitState = "ECO_XFR_TO_LIST_LEG_ACT_BY_PRSN";

            break;
          case ' ':
            break;
          default:
            var field4 = GetField(export.CourtOrder, "promptField");

            field4.Error = true;

            ExitState = "ACO_NE0000_INVALID_SELECT_CODE";

            return;
        }

        switch(AsChar(export.UndistCruc.PromptField))
        {
          case 'S':
            ++local.Selected.Count;
            export.PassPayHistInd.Flag = "Y";

            var field3 = GetField(export.UndistCruc, "promptField");

            field3.Error = true;

            ExitState = "ECO_LNK_TO_LST_CRUC_UNDSTRBTD_CL";

            break;
          case ' ':
            break;
          default:
            var field4 = GetField(export.UndistCruc, "promptField");

            field4.Error = true;

            ExitState = "ACO_NE0000_INVALID_SELECT_CODE";

            return;
        }

        for(export.Export1.Index = 0; export.Export1.Index < export
          .Export1.Count; ++export.Export1.Index)
        {
          switch(AsChar(export.Export1.Item.DetSourceCode.PromptField))
          {
            case 'S':
              ++local.Selected.Count;

              var field3 =
                GetField(export.Export1.Item.DetSourceCode, "promptField");

              field3.Error = true;

              ExitState = "ECO_LNK_LST_CASH_SOURCES";

              break;
            case ' ':
              break;
            default:
              var field4 =
                GetField(export.Export1.Item.DetSourceCode, "promptField");

              field4.Error = true;

              ExitState = "ACO_NE0000_INVALID_SELECT_CODE";

              return;
          }

          switch(AsChar(export.Export1.Item.DetCollTypPrompt.PromptField))
          {
            case 'S':
              ++local.Selected.Count;

              var field3 =
                GetField(export.Export1.Item.DetCollTypPrompt, "promptField");

              field3.Error = true;

              ExitState = "ECO_LNK_TO_LST_COLLECTION_TYPE";

              break;
            case ' ':
              break;
            default:
              var field4 =
                GetField(export.Export1.Item.DetCollTypPrompt, "promptField");

              field4.Error = true;

              ExitState = "ACO_NE0000_INVALID_SELECT_CODE";

              return;
          }
        }

        if (local.Selected.Count == 0)
        {
          var field = GetField(export.Payor, "promptField");

          field.Error = true;

          ExitState = "ACO_NE0000_MUST_SELECT_4_PROMPT";
        }
        else if (local.Selected.Count > 1)
        {
          ExitState = "ACO_NE0000_INVALID_MULT_PROMPT_S";
        }

        break;
      case "RETNAME":
        export.Payor.PromptField = "";

        if (IsEmpty(import.ReturnedCsePersonsWorkSet.Number))
        {
          if (IsEmpty(export.CsePerson.Number))
          {
            export.CsePersonsWorkSet.Assign(local.Clear);
            export.HiddenCsePersonsWorkSet.Number = local.Clear.Number;
          }
        }
        else
        {
          export.CsePersonsWorkSet.Assign(import.ReturnedCsePersonsWorkSet);
          export.CsePerson.Number = import.ReturnedCsePersonsWorkSet.Number;
          export.HiddenCsePersonsWorkSet.Number =
            import.ReturnedCsePersonsWorkSet.Number;
        }

        var field1 = GetField(export.LegalAction, "standardNumber");

        field1.Protected = false;
        field1.Focused = true;

        break;
      case "RETLAPS":
        export.CourtOrder.PromptField = "";

        if (!IsEmpty(import.ReturnedLegalAction.StandardNumber))
        {
          MoveLegalAction(import.ReturnedLegalAction, export.LegalAction);
        }

        var field2 = GetField(export.ListStarting, "collectionDate");

        field2.Protected = false;
        field2.Focused = true;

        break;
      case "RETCRUC":
        export.UndistCruc.PromptField = "";

        for(export.Export1.Index = 0; export.Export1.Index < export
          .Export1.Count; ++export.Export1.Index)
        {
          var field = GetField(export.Export1.Item.DetailCommon, "selectChar");

          field.Protected = false;
          field.Focused = true;

          return;
        }

        break;
      case "RETCRSL":
        for(export.Export1.Index = 0; export.Export1.Index < export
          .Export1.Count; ++export.Export1.Index)
        {
          if (AsChar(export.Export1.Item.DetSourceCode.PromptField) == 'S')
          {
            export.Export1.Update.DetSourceCode.PromptField = "";

            if (!IsEmpty(import.ReturnedToGroupCrs.Code))
            {
              export.Export1.Update.DetailCashReceiptSourceType.Code =
                import.ReturnedToGroupCrs.Code;
              export.Export1.Update.DetailCashReceiptSourceType.
                SystemGeneratedIdentifier =
                  import.ReturnedToGroupCrs.SystemGeneratedIdentifier;
            }

            var field =
              GetField(export.Export1.Item.DetailCollectionType, "code");

            field.Protected = false;
            field.Focused = true;

            return;
          }
        }

        break;
      case "RETCLCT":
        for(export.Export1.Index = 0; export.Export1.Index < export
          .Export1.Count; ++export.Export1.Index)
        {
          if (AsChar(export.Export1.Item.DetCollTypPrompt.PromptField) == 'S')
          {
            export.Export1.Update.DetCollTypPrompt.PromptField = "";

            if (!IsEmpty(import.ReturnedClctSelected.Code))
            {
              MoveCollectionType(import.ReturnedClctSelected,
                export.Export1.Update.DetailCollectionType);
            }

            var field =
              GetField(export.Export1.Item.DetailCollectionType, "code");

            field.Protected = false;
            field.Focused = true;

            return;
          }
        }

        break;
      case "ADD":
        // -----------------------------------------------------------------
        // Logic changed for IDCR 330.
        // Court and Direct Cash Receipt Types are now handled in the
        // group view to allow entry for both types in one pass.
        // -----------------------------------------------------------------
        if (import.Import1.IsEmpty)
        {
          ExitState = "FN0000_REIP_ATLEAST_ONE_CRD_REQD";

          return;
        }

        // ---------------------------------------------------------------------
        // Validate that at least one row is selected.
        // Also validate that the data entered and determine if similar
        // records already exist.
        // ---------------------------------------------------------------------
        local.Selected.Count = 0;

        for(export.Export1.Index = 0; export.Export1.Index < export
          .Export1.Count; ++export.Export1.Index)
        {
          // ---------------------------------------------------------------
          // Validate the value entered into the selection character field.
          // ---------------------------------------------------------------
          switch(AsChar(export.Export1.Item.DetailCommon.SelectChar))
          {
            case ' ':
              continue;
            case 'S':
              ++local.Selected.Count;

              break;
            default:
              var field =
                GetField(export.Export1.Item.DetailCommon, "selectChar");

              field.Error = true;

              ExitState = "ACO_RE0000_INVALID_SELECT_CODE";

              return;
          }

          // ---------------------------------------------------------------
          // If the Cash Receipt Number is greater than zero, the receipt
          // has already been added.    JLK  03/24/99
          // ---------------------------------------------------------------
          if (export.Export1.Item.DetailCashReceipt.SequentialNumber > 0)
          {
            ExitState = "FN0000_PMT_HIST_RECORD_AE_RB";

            return;
          }

          // ---------------------------------------------------------------
          // Validate that the required fields were entered.
          // ---------------------------------------------------------------
          if (Equal(export.Export1.Item.DetailCashReceiptDetail.CollectionDate,
            null) || Equal
            (export.Export1.Item.DetailCashReceiptDetail.CollectionDate,
            local.Low.Date))
          {
            var field =
              GetField(export.Export1.Item.DetailCashReceiptDetail,
              "collectionDate");

            field.Error = true;

            ExitState = "ACO_NE0000_REQ_DATA_MISSING_W_RB";
          }

          if (export.Export1.Item.DetailCashReceiptDetail.CollectionAmount == 0)
          {
            var field =
              GetField(export.Export1.Item.DetailCashReceiptDetail,
              "collectionAmount");

            field.Error = true;

            ExitState = "ACO_NE0000_REQ_DATA_MISSING_W_RB";
          }

          if (IsEmpty(export.Export1.Item.DetailCashReceiptSourceType.Code))
          {
            var field =
              GetField(export.Export1.Item.DetailCashReceiptSourceType, "code");
              

            field.Error = true;

            ExitState = "ACO_NE0000_REQ_DATA_MISSING_W_RB";
          }

          if (IsEmpty(export.Export1.Item.DetailCollectionType.Code))
          {
            var field =
              GetField(export.Export1.Item.DetailCollectionType, "code");

            field.Error = true;

            ExitState = "ACO_NE0000_REQ_DATA_MISSING_W_RB";
          }

          // ---------------------------------------------------------------
          // Validate Court/ Direct Payment code used to define the Cash
          // Receipt Type.
          // ---------------------------------------------------------------
          switch(AsChar(export.Export1.Item.DetailCourtOrDp.SelectChar))
          {
            case 'C':
              export.Export1.Update.HiddenCashReceiptType.
                SystemGeneratedIdentifier =
                  local.HardcodedCrtFcourtPmt.SystemGeneratedIdentifier;

              break;
            case 'D':
              export.Export1.Update.HiddenCashReceiptType.
                SystemGeneratedIdentifier =
                  local.HardcodedCrtFdirPmt.SystemGeneratedIdentifier;

              break;
            case ' ':
              var field3 =
                GetField(export.Export1.Item.DetailCourtOrDp, "selectChar");

              field3.Error = true;

              ExitState = "ACO_NE0000_REQ_DATA_MISSING_W_RB";

              break;
            default:
              var field4 =
                GetField(export.Export1.Item.DetailCourtOrDp, "selectChar");

              field4.Error = true;

              if (IsExitState("ACO_NN0000_ALL_OK"))
              {
                ExitState = "FN0000_INV_CT_OR_DIR_PMT_IND_RB";
              }

              break;
          }

          if (IsExitState("ACO_NN0000_ALL_OK") || IsExitState
            ("FN0000_CRD_MAY_AE_PRESS_ADD"))
          {
            // ---------------------------------------------------------------
            // Collection Amount must be greater than 0.
            // ---------------------------------------------------------------
            if (export.Export1.Item.DetailCashReceiptDetail.CollectionAmount < 0
              )
            {
              ExitState = "FN0128_CASH_RCPT_AMT_LT_ZERO_RB";

              var field =
                GetField(export.Export1.Item.DetailCashReceiptDetail,
                "collectionAmount");

              field.Error = true;

              return;
            }
            else
            {
              // -->  ok to continue
            }
          }
          else
          {
            return;
          }

          // ---------------------------------------------------------------
          // Collection Date must be less than current date.
          // ---------------------------------------------------------------
          if (!Lt(export.Export1.Item.DetailCashReceiptDetail.CollectionDate,
            local.Current.Date))
          {
            var field =
              GetField(export.Export1.Item.DetailCashReceiptDetail,
              "collectionDate");

            field.Error = true;

            ExitState = "FN0000_INVALID_COLLECTION_DATE";
          }

          if (!Lt(export.Export1.Item.RecurringToDate.CollectionDate,
            local.Current.Date))
          {
            var field =
              GetField(export.Export1.Item.RecurringToDate, "collectionDate");

            field.Error = true;

            ExitState = "FN0000_INVALID_COLLECTION_DATE";
          }

          if (IsExitState("ACO_NN0000_ALL_OK") || IsExitState
            ("FN0000_CRD_MAY_AE_PRESS_ADD"))
          {
            // -->  ok to continue
          }
          else
          {
            return;
          }

          // ---------------------------------------------------------------
          // Both the Frequency and the To-Date must be entered to
          // create recurring payment history records.
          // ---------------------------------------------------------------
          if (IsEmpty(export.Export1.Item.Frequency.Text4) && Lt
            (local.Null1.Date,
            export.Export1.Item.RecurringToDate.CollectionDate))
          {
            var field = GetField(export.Export1.Item.Frequency, "text4");

            field.Error = true;

            ExitState = "FN0000_RECURR_REQD_FIELDS_RB";

            return;
          }

          if (!IsEmpty(export.Export1.Item.Frequency.Text4) && Equal
            (export.Export1.Item.RecurringToDate.CollectionDate,
            local.Null1.Date))
          {
            var field =
              GetField(export.Export1.Item.RecurringToDate, "collectionDate");

            field.Error = true;

            ExitState = "FN0000_RECURR_REQD_FIELDS_RB";

            return;
          }

          // ---------------------------------------------------------------
          // Validate the fields used for recurring payments.
          // ---------------------------------------------------------------
          if (!IsEmpty(export.Export1.Item.Frequency.Text4) && Lt
            (local.Null1.Date,
            export.Export1.Item.RecurringToDate.CollectionDate))
          {
            // ---------------------------------------------------------------
            // The collection date must be less than the recurring to date.
            // ---------------------------------------------------------------
            // 01/07/00  P. Phinney  H00083731  Prevent Dates prior to the
            // Collection Effective Date.
            // Changed local_pass cash_receipt_detail to group_export 
            // cash_receipt
            if (Lt(export.Export1.Item.RecurringToDate.CollectionDate,
              export.Export1.Item.DetailCashReceiptDetail.CollectionDate))
            {
              var field =
                GetField(export.Export1.Item.RecurringToDate, "collectionDate");
                

              field.Error = true;

              ExitState = "FN0000_RECURR_TO_DT_GT_COLL_DT";

              return;
            }

            // ---------------------------------------------------------------
            // Determine if a valid Frequency Code was entered.
            // ---------------------------------------------------------------
            switch(TrimEnd(export.Export1.Item.Frequency.Text4))
            {
              case "M":
                break;
              case "BM":
                break;
              case "SM":
                break;
              case "W":
                local.FrequencyDays.MenuOption = 7;

                break;
              case "BW":
                local.FrequencyDays.MenuOption = 14;

                break;
              default:
                var field = GetField(export.Export1.Item.Frequency, "text4");

                field.Error = true;

                ExitState = "FN0000_INVALID_FREQ_CODE_W_RB";

                return;
            }
          }

          // ---------------------------------------------------------------
          // Validate the Collection Type, if entered.
          // [Collection Type 6 is not valid for payment history.]
          // Collection Types 7, 8, and 9 are only valid for CSENet
          // collections.  Added edit to display error message if these
          // codes are selected for payment history.    JLK 05/14/99
          // ---------------------------------------------------------------
          // 06/12/01  P. Phinney  I00121505  Removed BLOCK of
          // Collection Types 7, 8, 9 - CSENet
          if (Equal(export.Export1.Item.DetailCollectionType.Code, "6"))
          {
            var field =
              GetField(export.Export1.Item.DetailCollectionType, "code");

            field.Error = true;

            ExitState = "FN0000_INVALID_COLL_TYPE";

            return;
          }

          // ----------------------------------------------------------
          // Determine if similar payment history records already exist.
          // ----------------------------------------------------------
          if (AsChar(export.Continue1.Flag) != '+')
          {
            UseFnReipFindSimilarPmtHist();

            if (IsExitState("FN0000_CRD_MAY_AE_PRESS_ADD"))
            {
              var field3 =
                GetField(export.Export1.Item.DetailCashReceiptDetail,
                "collectionDate");

              field3.Error = true;

              var field4 =
                GetField(export.Export1.Item.DetailCashReceiptDetail,
                "collectionAmount");

              field4.Error = true;

              var field5 =
                GetField(export.Export1.Item.DetailCashReceiptSourceType, "code");
                

              field5.Error = true;

              export.Continue1.Flag = "Y";

              continue;
            }
            else if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              return;
            }
          }
        }

        // -----------------------------------------------------------
        // No rows were selected for the Add action.
        // -----------------------------------------------------------
        if (local.Selected.Count == 0)
        {
          for(export.Export1.Index = 0; export.Export1.Index < export
            .Export1.Count; ++export.Export1.Index)
          {
            var field =
              GetField(export.Export1.Item.DetailCommon, "selectChar");

            field.Error = true;

            break;
          }

          ExitState = "ACO_NE0000_INVALID_SELECT_CODE";

          return;
        }

        // -----------------------------------------------------------
        // Similar records exist.  Prompt user to confirm Add action.
        // -----------------------------------------------------------
        if (AsChar(export.Continue1.Flag) == 'Y')
        {
          return;
        }

        // ------------------------------------------------------------------
        // The following logic to set the number of days in each month is
        // only used for the ADD command when the frequency is BM.
        // ------------------------------------------------------------------
        for(local.MaxDaysInMonth.Index = 0; local.MaxDaysInMonth.Index < 12; ++
          local.MaxDaysInMonth.Index)
        {
          if (!local.MaxDaysInMonth.CheckSize())
          {
            break;
          }

          switch(local.MaxDaysInMonth.Index + 1)
          {
            case 1:
              local.MaxDaysInMonth.Update.DetlMaxDaysInMth.Count = 31;

              break;
            case 2:
              local.MaxDaysInMonth.Update.DetlMaxDaysInMth.Count = 28;

              break;
            case 3:
              local.MaxDaysInMonth.Update.DetlMaxDaysInMth.Count = 31;

              break;
            case 4:
              local.MaxDaysInMonth.Update.DetlMaxDaysInMth.Count = 30;

              break;
            case 5:
              local.MaxDaysInMonth.Update.DetlMaxDaysInMth.Count = 31;

              break;
            case 6:
              local.MaxDaysInMonth.Update.DetlMaxDaysInMth.Count = 30;

              break;
            case 7:
              local.MaxDaysInMonth.Update.DetlMaxDaysInMth.Count = 31;

              break;
            case 8:
              local.MaxDaysInMonth.Update.DetlMaxDaysInMth.Count = 31;

              break;
            case 9:
              local.MaxDaysInMonth.Update.DetlMaxDaysInMth.Count = 30;

              break;
            case 10:
              local.MaxDaysInMonth.Update.DetlMaxDaysInMth.Count = 31;

              break;
            case 11:
              local.MaxDaysInMonth.Update.DetlMaxDaysInMth.Count = 30;

              break;
            case 12:
              local.MaxDaysInMonth.Update.DetlMaxDaysInMth.Count = 31;

              break;
            default:
              break;
          }
        }

        local.MaxDaysInMonth.CheckIndex();

        for(export.Export1.Index = 0; export.Export1.Index < export
          .Export1.Count; ++export.Export1.Index)
        {
          if (AsChar(export.Export1.Item.DetailCommon.SelectChar) == 'S')
          {
            // --------------------------------------------------------------
            // Add the selected occurrences.
            // --------------------------------------------------------------
            if (!IsEmpty(export.Export1.Item.Frequency.Text4) && Lt
              (local.Null1.Date,
              export.Export1.Item.RecurringToDate.CollectionDate))
            {
              // -----------------------------------------------------------
              // Set up local view with initial Collection Date to be used
              // for the first recurring payment history record.
              // -----------------------------------------------------------
              MoveCashReceiptDetail(export.Export1.Item.DetailCashReceiptDetail,
                local.Pass);
              local.NumberOfMonths.Count = 0;

              if (Equal(export.Export1.Item.Frequency.Text4, "SM"))
              {
                local.StartingSmCollDateDd.Count =
                  Day(export.Export1.Item.DetailCashReceiptDetail.CollectionDate);
                  

                if (local.StartingSmCollDateDd.Count >= 15)
                {
                  local.NextSmCollDateDd.Count =
                    local.StartingSmCollDateDd.Count;

                  local.MaxDaysInMonth.Index =
                    Month(export.Export1.Item.DetailCashReceiptDetail.
                      CollectionDate) - 1;
                  local.MaxDaysInMonth.CheckSize();

                  if (local.NextSmCollDateDd.Count == local
                    .MaxDaysInMonth.Item.DetlMaxDaysInMth.Count)
                  {
                    local.StartingSmCollDateDd.Count = 15;
                    local.NextSmCollDateDd.Count = 31;
                  }
                  else
                  {
                    local.StartingSmCollDateDd.Count -= 14;
                  }
                }
                else
                {
                  local.NextSmCollDateDd.Count =
                    local.StartingSmCollDateDd.Count + 14;
                }
              }

              // -------------------------------------------------------
              // Create recurring payment history records.
              // -------------------------------------------------------
              while(!Lt(export.Export1.Item.RecurringToDate.CollectionDate,
                local.Pass.CollectionDate))
              {
                UseFnReipCreatePaymentHistory1();

                if (!IsExitState("ACO_NN0000_ALL_OK"))
                {
                  // 01/04/00  P. Phinney  H00082731  Do NOT allow invalid 
                  // collection date
                  if (IsExitState("FN0000_COLL_DATE_NOT_VALID"))
                  {
                    var field =
                      GetField(export.Export1.Item.DetailCashReceiptDetail,
                      "collectionDate");

                    field.Error = true;
                  }

                  return;
                }

                // -------------------------------------------------------
                // Set up local view with next Collection Date.  If the
                // local Collection Date is less than or equal to the
                // Recurring End Collection Date, the WHILE logic is
                // repeated to create the next payment history record.
                // -------------------------------------------------------
                switch(TrimEnd(export.Export1.Item.Frequency.Text4))
                {
                  case "M":
                    // -->  Monthly payment history records to be created.
                    ++local.NumberOfMonths.Count;
                    local.Pass.CollectionDate =
                      AddMonths(export.Export1.Item.DetailCashReceiptDetail.
                        CollectionDate, local.NumberOfMonths.Count);

                    break;
                  case "BM":
                    // -->  Bi-monthly payment history records to be created.
                    local.NumberOfMonths.Count += 2;
                    local.Pass.CollectionDate =
                      AddMonths(export.Export1.Item.DetailCashReceiptDetail.
                        CollectionDate, local.NumberOfMonths.Count);

                    break;
                  case "SM":
                    // --> Semi-monthly payment history records to be created.
                    if (Day(local.Pass.CollectionDate) == local
                      .StartingSmCollDateDd.Count)
                    {
                      local.MaxDaysInMonth.Index =
                        Month(local.Pass.CollectionDate) - 1;
                      local.MaxDaysInMonth.CheckSize();

                      if (local.NextSmCollDateDd.Count <= local
                        .MaxDaysInMonth.Item.DetlMaxDaysInMth.Count)
                      {
                        local.Pass.CollectionDate =
                          IntToDate(Year(local.Pass.CollectionDate) * 10000 + Month
                          (local.Pass.CollectionDate) * 100
                          + local.NextSmCollDateDd.Count);
                      }
                      else
                      {
                        local.Pass.CollectionDate =
                          IntToDate(Year(local.Pass.CollectionDate) * 10000 + Month
                          (local.Pass.CollectionDate) * 100
                          + local.MaxDaysInMonth.Item.DetlMaxDaysInMth.Count);
                      }
                    }
                    else
                    {
                      local.Pass.CollectionDate =
                        AddMonths(local.Pass.CollectionDate, 1);
                      local.Pass.CollectionDate =
                        IntToDate(Year(local.Pass.CollectionDate) * 10000 + Month
                        (local.Pass.CollectionDate) * 100
                        + local.StartingSmCollDateDd.Count);
                    }

                    break;
                  default:
                    local.Pass.CollectionDate =
                      AddDays(local.Pass.CollectionDate,
                      local.FrequencyDays.MenuOption);

                    break;
                }
              }
            }
            else
            {
              // -------------------------------------------------------
              // A single payment history record is created.
              // Then continue with the next selected row to be added.
              // -------------------------------------------------------
              UseFnReipCreatePaymentHistory2();

              if (!IsExitState("ACO_NN0000_ALL_OK"))
              {
                // 01/04/00  P. Phinney  H00082731  Do NOT allow invalid 
                // collection date
                if (IsExitState("FN0000_COLL_DATE_NOT_VALID"))
                {
                  var field =
                    GetField(export.Export1.Item.DetailCashReceiptDetail,
                    "collectionDate");

                  field.Error = true;
                }

                return;
              }
            }
          }
        }

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          export.Continue1.Flag = "";
          ExitState = "ACO_NI0000_SUCCESSFUL_ADD";

          export.Export1.Index = 0;
          export.Export1.Clear();

          for(import.Import1.Index = 0; import.Import1.Index < import
            .Import1.Count; ++import.Import1.Index)
          {
            if (export.Export1.IsFull)
            {
              break;
            }

            // -->  Clear out group view for more data entry.
            export.Export1.Next();
          }
        }

        break;
      case "UPDATE":
        // ----------------------------------------------
        // Logic added for IDCR 313
        // ----------------------------------------------
        local.Selected.Count = 0;

        for(export.Export1.Index = 0; export.Export1.Index < export
          .Export1.Count; ++export.Export1.Index)
        {
          switch(AsChar(export.Export1.Item.DetailCommon.SelectChar))
          {
            case ' ':
              break;
            case 'S':
              ++local.Selected.Count;

              if (IsEmpty(export.Export1.Item.DetailCashReceiptSourceType.Code) ||
                IsEmpty
                (export.Export1.Item.DetailCashReceiptDetailStatus.Code))
              {
                var field3 =
                  GetField(export.Export1.Item.DetailCommon, "selectChar");

                field3.Error = true;

                ExitState = "ACO_NE0000_DATABASE_CORRUPTION";

                return;
              }

              // ---------------------------------------------------------------
              // If the current detail status is ADJ or any portion of the
              // collection amount has been distributed or refunded, the
              // record cannot be updated.  JLK  03/23/99
              // ---------------------------------------------------------------
              if (Equal(export.Export1.Item.DetailCashReceiptDetailStatus.Code,
                "ADJ"))
              {
                ExitState = "FN0000_PMT_HIST_ADJ_CANT_UPD_DEL";
              }

              if (export.Export1.Item.DetailCashReceiptDetail.DistributedAmount.
                GetValueOrDefault() > 0 || export
                .Export1.Item.DetailCashReceiptDetail.RefundedAmount.
                  GetValueOrDefault() > 0)
              {
                ExitState = "FN0000_COLL_OR_REF_EXIST_NO_UD";
              }

              if (!IsExitState("ACO_NN0000_ALL_OK"))
              {
                return;
              }

              // ---------------------------------------------------------------
              // Do not allow user to enter recurring information on an update.
              // ---------------------------------------------------------------
              if (!IsEmpty(export.Export1.Item.Frequency.Text4))
              {
                var field3 = GetField(export.Export1.Item.Frequency, "text4");

                field3.Error = true;

                ExitState = "CANNOT_UPD_HIGHLIGHTED_FIELDS_RB";
              }

              if (!Equal(export.Export1.Item.RecurringToDate.CollectionDate,
                local.Null1.Date))
              {
                var field3 =
                  GetField(export.Export1.Item.RecurringToDate, "collectionDate");
                  

                field3.Error = true;

                ExitState = "CANNOT_UPD_HIGHLIGHTED_FIELDS_RB";
              }

              if (!IsExitState("ACO_NN0000_ALL_OK"))
              {
                return;
              }

              // ---------------------------------------------------
              // Validate that required fields are entered.
              // ---------------------------------------------------
              if (export.Export1.Item.DetailCashReceiptDetail.
                CollectionAmount == 0)
              {
                var field3 =
                  GetField(export.Export1.Item.DetailCashReceiptDetail,
                  "collectionAmount");

                field3.Error = true;

                ExitState = "ACO_NE0000_ENTER_REQUIRD_DATA_RB";
              }

              if (Equal(export.Export1.Item.DetailCashReceiptDetail.
                CollectionDate, null) || Equal
                (export.Export1.Item.DetailCashReceiptDetail.CollectionDate,
                local.Low.Date))
              {
                var field3 =
                  GetField(export.Export1.Item.DetailCashReceiptDetail,
                  "collectionDate");

                field3.Error = true;

                ExitState = "ACO_NE0000_ENTER_REQUIRD_DATA_RB";
              }

              // 01/07/00  P. Phinney  H00083299  Prevent Future Dates when 
              // Updating.
              // -------------------------------------------------------------------------
              if (!Lt(export.Export1.Item.DetailCashReceiptDetail.
                CollectionDate, local.Current.Date))
              {
                var field3 =
                  GetField(export.Export1.Item.DetailCashReceiptDetail,
                  "collectionDate");

                field3.Error = true;

                if (IsExitState("ACO_NN0000_ALL_OK"))
                {
                  ExitState = "FN0000_INVALID_COLLECTION_DATE";
                }
              }

              if (IsEmpty(export.Export1.Item.DetailCollectionType.Code))
              {
                var field3 =
                  GetField(export.Export1.Item.DetailCollectionType, "code");

                field3.Error = true;

                if (IsExitState("ACO_NN0000_ALL_OK"))
                {
                  ExitState = "ACO_NE0000_ENTER_REQUIRD_DATA_RB";
                }
              }

              if (IsExitState("ACO_NN0000_ALL_OK"))
              {
                if (export.Export1.Item.DetailCashReceiptDetail.
                  CollectionAmount <= 0)
                {
                  var field3 =
                    GetField(export.Export1.Item.DetailCashReceiptDetail,
                    "collectionAmount");

                  field3.Error = true;

                  ExitState = "FN0128_CASH_RCPT_AMT_LT_ZERO_RB";

                  return;
                }
              }
              else
              {
                return;
              }

              // ---------------------------------------------------
              // Validate Valid Type code.
              // ---------------------------------------------------
              switch(AsChar(export.Export1.Item.DetailCourtOrDp.SelectChar))
              {
                case 'C':
                  break;
                case 'D':
                  break;
                default:
                  var field3 =
                    GetField(export.Export1.Item.DetailCourtOrDp, "selectChar");
                    

                  field3.Error = true;

                  ExitState = "FN0000_ENTER_VALID_TYPE_CODE_RB";

                  return;
              }

              // ---------------------------------------------------------------
              // Validate the Collection Type, if entered.
              // [Collection Type 6 is not valid for payment history.
              // Collection Types 7, 8, and 9 are only valid for CSENet
              // collections.  Added edit to display error message if these
              // codes are selected for payment history.    JLK 05/14/99
              // ---------------------------------------------------------------
              // 06/12/01  P. Phinney  I00121505  Removed BLOCK of
              // Collection Types 7, 8, 9 - CSENet
              if (Equal(export.Export1.Item.DetailCollectionType.Code, "6"))
              {
                var field3 =
                  GetField(export.Export1.Item.DetailCollectionType, "code");

                field3.Error = true;

                ExitState = "FN0000_INVALID_COLL_TYPE";

                return;
              }

              // ---------------------------------------------------
              // Update payment history record.
              // ---------------------------------------------------
              UseFnReipUpdatePaymentHistory();

              if (!IsExitState("ACO_NN0000_ALL_OK"))
              {
                if (IsExitState("FN0000_SRC_CODE_CHANGED_W_RB"))
                {
                  var field3 =
                    GetField(export.Export1.Item.DetailCashReceiptSourceType,
                    "code");

                  field3.Color = "red";
                  field3.Intensity = Intensity.High;
                  field3.Highlighting = Highlighting.ReverseVideo;
                  field3.Protected = true;
                }
                else if (IsExitState("FN0000_CANNOT_CHG_CR_TYPE_W_RB"))
                {
                  var field3 =
                    GetField(export.Export1.Item.DetailCourtOrDp, "selectChar");
                    

                  field3.Color = "red";
                  field3.Intensity = Intensity.High;
                  field3.Highlighting = Highlighting.ReverseVideo;
                  field3.Protected = true;
                }
                else if (IsExitState("FN0000_COLLECTION_TYPE_NF_RB"))
                {
                  var field3 =
                    GetField(export.Export1.Item.DetailCollectionType, "code");

                  field3.Error = true;
                }
                else if (IsExitState("FN0000_COLL_OR_REF_EXIST_NO_UD"))
                {
                  var field3 =
                    GetField(export.Export1.Item.DetailCommon, "selectChar");

                  field3.Error = true;
                }
                else if (IsExitState("FN0000_PMT_HIST_ADJ_CANT_UPD_DEL"))
                {
                  var field3 =
                    GetField(export.Export1.Item.DetailCommon, "selectChar");

                  field3.Error = true;
                }
                else if (IsExitState("FN0000_COLL_DATE_NOT_VALID"))
                {
                  // 01/04/00  P. Phinney  H00082731  Do NOT allow invalid 
                  // collection date
                  var field3 =
                    GetField(export.Export1.Item.DetailCashReceiptDetail,
                    "collectionDate");

                  field3.Error = true;
                }
                else
                {
                }

                return;
              }

              break;
            default:
              var field =
                GetField(export.Export1.Item.DetailCommon, "selectChar");

              field.Error = true;

              ExitState = "ACO_RE0000_INVALID_SELECT_CODE";

              return;
          }
        }

        if (local.Selected.Count == 0)
        {
          for(export.Export1.Index = 0; export.Export1.Index < export
            .Export1.Count; ++export.Export1.Index)
          {
            var field =
              GetField(export.Export1.Item.DetailCommon, "selectChar");

            field.Error = true;

            break;
          }

          ExitState = "ACO_NE0000_INVALID_SELECT_CODE";

          return;
        }

        // -------------------------------------------------------------
        // Display payment history records after completing the updates.
        // -------------------------------------------------------------
        UseFnReipDisplayPaymentHistory();

        for(export.Export1.Index = 0; export.Export1.Index < export
          .Export1.Count; ++export.Export1.Index)
        {
          if (export.Export1.Item.DetailCashReceipt.SequentialNumber > 0)
          {
            var field3 =
              GetField(export.Export1.Item.RecurringToDate, "collectionDate");

            field3.Color = "cyan";
            field3.Protected = true;

            var field4 = GetField(export.Export1.Item.Frequency, "text4");

            field4.Color = "cyan";
            field4.Protected = true;

            var field5 =
              GetField(export.Export1.Item.DetailCourtOrDp, "selectChar");

            field5.Color = "cyan";
            field5.Protected = true;

            var field6 =
              GetField(export.Export1.Item.DetailCashReceiptSourceType, "code");
              

            field6.Color = "cyan";
            field6.Protected = true;

            var field7 =
              GetField(export.Export1.Item.DetSourceCode, "promptField");

            field7.Color = "cyan";
            field7.Protected = true;

            // --------------------------------------------------------
            // If the current detail status is ADJ or at least part of
            // the collection amount has been distributed or refunded,
            // the payment history record cannot be changed.
            // JLK  03/23/99
            // --------------------------------------------------------
            if (Equal(export.Export1.Item.DetailCashReceiptDetailStatus.Code,
              "ADJ") || export
              .Export1.Item.DetailCashReceiptDetail.DistributedAmount.
                GetValueOrDefault() > 0 || export
              .Export1.Item.DetailCashReceiptDetail.RefundedAmount.
                GetValueOrDefault() > 0)
            {
              var field8 =
                GetField(export.Export1.Item.DetailCashReceiptDetail,
                "collectionDate");

              field8.Color = "cyan";
              field8.Protected = true;

              var field9 =
                GetField(export.Export1.Item.DetailCashReceiptDetail,
                "collectionAmount");

              field9.Color = "cyan";
              field9.Protected = true;

              var field10 =
                GetField(export.Export1.Item.DetailCollectionType, "code");

              field10.Color = "cyan";
              field10.Protected = true;

              var field11 =
                GetField(export.Export1.Item.DetCollTypPrompt, "promptField");

              field11.Color = "cyan";
              field11.Protected = true;
            }
          }
        }

        ExitState = "ACO_NI0000_SUCCESSFUL_UPDATE";

        break;
      case "DELETE":
        local.Selected.Count = 0;

        for(export.Export1.Index = 0; export.Export1.Index < export
          .Export1.Count; ++export.Export1.Index)
        {
          switch(AsChar(export.Export1.Item.DetailCommon.SelectChar))
          {
            case 'S':
              // -----------------------------------------------------------
              // If the current detail status is ADJ or if the detail record
              // has associated collections or receipt refunds, the record
              // cannot be deleted.  JLK  03/23/99
              // -----------------------------------------------------------
              if (Equal(export.Export1.Item.DetailCashReceiptDetailStatus.Code,
                "ADJ"))
              {
                ExitState = "FN0000_PMT_HIST_ADJ_CANT_UPD_DEL";
              }

              if (export.Export1.Item.DetailCashReceiptDetail.DistributedAmount.
                GetValueOrDefault() > 0 || export
                .Export1.Item.DetailCashReceiptDetail.RefundedAmount.
                  GetValueOrDefault() > 0)
              {
                ExitState = "FN0000_COLL_OR_REF_EXIST_NO_UD";
              }

              if (!IsExitState("ACO_NN0000_ALL_OK"))
              {
                return;
              }

              // -----------------------------------------------------------
              // Process the delete action.
              // -----------------------------------------------------------
              ++local.Selected.Count;
              UseFnReipDeletePaymentHistory();

              if (!IsExitState("ACO_NN0000_ALL_OK"))
              {
                var field3 =
                  GetField(export.Export1.Item.DetailCommon, "selectChar");

                field3.Error = true;

                return;
              }

              break;
            case ' ':
              break;
            default:
              ExitState = "ACO_NE0000_INVALID_SELECT_CODE1";

              var field =
                GetField(export.Export1.Item.DetailCommon, "selectChar");

              field.Error = true;

              return;
          }
        }

        if (local.Selected.Count == 0)
        {
          for(export.Export1.Index = 0; export.Export1.Index < export
            .Export1.Count; ++export.Export1.Index)
          {
            var field =
              GetField(export.Export1.Item.DetailCommon, "selectChar");

            field.Error = true;

            break;
          }

          ExitState = "ACO_NE0000_INVALID_SELECT_CODE";

          return;
        }

        UseFnReipDisplayPaymentHistory();

        for(export.Export1.Index = 0; export.Export1.Index < export
          .Export1.Count; ++export.Export1.Index)
        {
          if (export.Export1.Item.DetailCashReceipt.SequentialNumber > 0)
          {
            var field3 =
              GetField(export.Export1.Item.RecurringToDate, "collectionDate");

            field3.Color = "cyan";
            field3.Protected = true;

            var field4 = GetField(export.Export1.Item.Frequency, "text4");

            field4.Color = "cyan";
            field4.Protected = true;

            var field5 =
              GetField(export.Export1.Item.DetailCourtOrDp, "selectChar");

            field5.Color = "cyan";
            field5.Protected = true;

            var field6 =
              GetField(export.Export1.Item.DetailCashReceiptSourceType, "code");
              

            field6.Color = "cyan";
            field6.Protected = true;

            var field7 =
              GetField(export.Export1.Item.DetSourceCode, "promptField");

            field7.Color = "cyan";
            field7.Protected = true;

            // --------------------------------------------------------
            // If the current detail status is ADJ or at least part of
            // the collection amount has been distributed or refunded,
            // the payment history record cannot be changed.
            // JLK  03/23/99
            // --------------------------------------------------------
            if (Equal(export.Export1.Item.DetailCashReceiptDetailStatus.Code,
              "ADJ") || export
              .Export1.Item.DetailCashReceiptDetail.DistributedAmount.
                GetValueOrDefault() > 0 || export
              .Export1.Item.DetailCashReceiptDetail.RefundedAmount.
                GetValueOrDefault() > 0)
            {
              var field8 =
                GetField(export.Export1.Item.DetailCashReceiptDetail,
                "collectionDate");

              field8.Color = "cyan";
              field8.Protected = true;

              var field9 =
                GetField(export.Export1.Item.DetailCashReceiptDetail,
                "collectionAmount");

              field9.Color = "cyan";
              field9.Protected = true;

              var field10 =
                GetField(export.Export1.Item.DetailCollectionType, "code");

              field10.Color = "cyan";
              field10.Protected = true;

              var field11 =
                GetField(export.Export1.Item.DetCollTypPrompt, "promptField");

              field11.Color = "cyan";
              field11.Protected = true;
            }
          }
        }

        ExitState = "ACO_NI0000_SUCCESSFUL_DELETE";

        break;
      case "CRRC":
        for(export.Export1.Index = 0; export.Export1.Index < export
          .Export1.Count; ++export.Export1.Index)
        {
          switch(AsChar(export.Export1.Item.DetailCommon.SelectChar))
          {
            case 'S':
              MoveCashReceiptSourceType(export.Export1.Item.
                DetailCashReceiptSourceType, export.PassCashReceiptSourceType);
              export.PassCashReceiptEvent.SystemGeneratedIdentifier =
                export.Export1.Item.HiddenCashReceiptEvent.
                  SystemGeneratedIdentifier;
              export.PassCashReceiptType.SystemGeneratedIdentifier =
                export.Export1.Item.HiddenCashReceiptType.
                  SystemGeneratedIdentifier;
              export.PassCashReceipt.SequentialNumber =
                export.Export1.Item.DetailCashReceipt.SequentialNumber;
              export.PassCashReceiptDetail.Assign(
                export.Export1.Item.DetailCashReceiptDetail);
              ExitState = "ECO_LNK_TO_CASH_RECEIPT_COLLECTN";

              return;
            case ' ':
              break;
            default:
              var field =
                GetField(export.Export1.Item.DetailCommon, "selectChar");

              field.Error = true;

              ExitState = "ACO_NE0000_INVALID_SELECT_CODE1";

              return;
          }
        }

        for(export.Export1.Index = 0; export.Export1.Index < export
          .Export1.Count; ++export.Export1.Index)
        {
          var field = GetField(export.Export1.Item.DetailCommon, "selectChar");

          field.Error = true;

          break;
        }

        ExitState = "ACO_NE0000_NO_SELECTION_MADE";

        break;
      case "MANDIST":
        // ----------------------------------------------------------------------
        // PF16 Man Dist
        // Place the selected payment history records in SUSP status with a
        // suspense reason code of MANUALDIST.
        // ----------------------------------------------------------------------
        local.Selected.Count = 0;

        for(export.Export1.Index = 0; export.Export1.Index < export
          .Export1.Count; ++export.Export1.Index)
        {
          switch(AsChar(export.Export1.Item.DetailCommon.SelectChar))
          {
            case 'S':
              ++local.Selected.Count;

              if (export.Export1.Item.DetailCashReceipt.SequentialNumber == 0
                || export
                .Export1.Item.DetailCashReceiptDetail.SequentialIdentifier == 0
                )
              {
                var field3 =
                  GetField(export.Export1.Item.DetailCommon, "selectChar");

                field3.Error = true;

                ExitState = "FN0000_ADD_B4_UPD_DL_MAN_DIST_RL";

                return;
              }

              if (IsEmpty(export.Export1.Item.DetailCashReceiptSourceType.Code) ||
                IsEmpty
                (export.Export1.Item.DetailCashReceiptDetailStatus.Code) || export
                .Export1.Item.HiddenCashReceiptType.SystemGeneratedIdentifier ==
                  0)
              {
                var field3 =
                  GetField(export.Export1.Item.DetailCommon, "selectChar");

                field3.Error = true;

                ExitState = "ACO_NE0000_DATABASE_CORRUPTION";

                return;
              }

              if (IsEmpty(export.Export1.Item.DetailCollectionType.Code))
              {
                var field3 =
                  GetField(export.Export1.Item.DetailCollectionType, "code");

                field3.Error = true;

                ExitState = "FN0000_COLL_TYP_NF";

                return;
              }

              UseFnReipMarkForManualDistrib();

              if (!IsExitState("ACO_NN0000_ALL_OK"))
              {
                return;
              }

              break;
            case ' ':
              break;
            default:
              var field =
                GetField(export.Export1.Item.DetailCommon, "selectChar");

              field.Error = true;

              ExitState = "ACO_RE0000_INVALID_SELECT_CODE";

              return;
          }
        }

        for(export.Export1.Index = 0; export.Export1.Index < export
          .Export1.Count; ++export.Export1.Index)
        {
          if (AsChar(export.Export1.Item.DetailCommon.SelectChar) == 'S')
          {
            export.Export1.Update.DetailCommon.SelectChar = "";
            export.Export1.Update.DetailCashReceiptDetailStatus.Code =
              local.New1.Code;
            export.Export1.Update.DetailManDistInd.Flag = "Y";
          }
        }

        if (local.Selected.Count == 0)
        {
          for(export.Export1.Index = 0; export.Export1.Index < export
            .Export1.Count; ++export.Export1.Index)
          {
            var field =
              GetField(export.Export1.Item.DetailCommon, "selectChar");

            field.Error = true;

            break;
          }

          ExitState = "ACO_NE0000_INVALID_SELECT_CODE";
        }
        else
        {
          ExitState = "FN0000_SUCCESSFUL_PROCESSING";
        }

        break;
      case "RELEASE":
        // ----------------------------------------------------------------------
        // PF17 Rel
        // Release all payment history records for distribution.
        // ----------------------------------------------------------------------
        // ----------------------------------------------------------------------
        // Check payment history records displayed for data integrity
        // errors or Suspended records to be manually distributed.
        // Must process manual distributions prior to release.
        // ----------------------------------------------------------------------
        ExitState = "ACO_NN0000_ALL_OK";

        for(export.Export1.Index = 0; export.Export1.Index < export
          .Export1.Count; ++export.Export1.Index)
        {
          if (AsChar(export.Export1.Item.DetailManDistInd.Flag) == 'Y')
          {
            var field =
              GetField(export.Export1.Item.DetailCommon, "selectChar");

            field.Error = true;

            ExitState = "FN0000_PROCESS_MAN_DIST_FIRST";
          }
        }

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }

        for(export.Export1.Index = 0; export.Export1.Index < export
          .Export1.Count; ++export.Export1.Index)
        {
          if (IsEmpty(export.Export1.Item.DetailCashReceiptSourceType.Code) || IsEmpty
            (export.Export1.Item.DetailCashReceiptDetailStatus.Code) || export
            .Export1.Item.HiddenCashReceiptType.SystemGeneratedIdentifier == 0
            || IsEmpty(export.Export1.Item.DetailCashReceiptSourceType.Code))
          {
            // 02/02/00  P. Phinney  H00086708  Changed message that displays 
            // when PF17 is pressed
            var field =
              GetField(export.Export1.Item.DetailCommon, "selectChar");

            field.Error = true;

            ExitState = "FN0000_COLL_RELEASE_INFO_MISSING";
          }
        }

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }

        // -------------------------------------------------------------------
        // Release payment history records for distribution.
        // -------------------------------------------------------------------
        UseFnReipReleasePaymentHistory();

        if (IsExitState("FN0000_COLL_RELEASE_SUCCESSFUL"))
        {
          // -------------------------------------------------------------------
          // Display payment history records after completing the updates.
          // -------------------------------------------------------------------
          ExitState = "ACO_NN0000_ALL_OK";
          UseFnReipDisplayPaymentHistory();

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            for(export.Export1.Index = 0; export.Export1.Index < export
              .Export1.Count; ++export.Export1.Index)
            {
              // -------------------------------------------------------------------
              // Protect fields that cannot be changed.
              // -------------------------------------------------------------------
              if (export.Export1.Item.DetailCashReceipt.SequentialNumber > 0)
              {
                var field3 =
                  GetField(export.Export1.Item.RecurringToDate, "collectionDate");
                  

                field3.Color = "cyan";
                field3.Protected = true;

                var field4 = GetField(export.Export1.Item.Frequency, "text4");

                field4.Color = "cyan";
                field4.Protected = true;

                var field5 =
                  GetField(export.Export1.Item.DetailCourtOrDp, "selectChar");

                field5.Color = "cyan";
                field5.Protected = true;

                var field6 =
                  GetField(export.Export1.Item.DetailCashReceiptSourceType,
                  "code");

                field6.Color = "cyan";
                field6.Protected = true;

                var field7 =
                  GetField(export.Export1.Item.DetSourceCode, "promptField");

                field7.Color = "cyan";
                field7.Protected = true;

                // --------------------------------------------------------
                // If the current detail status is ADJ or at least part of
                // the collection amount has been distributed or refunded,
                // the payment history record cannot be changed.
                // JLK  03/23/99
                // --------------------------------------------------------
                if (Equal(export.Export1.Item.DetailCashReceiptDetailStatus.
                  Code, "ADJ") || export
                  .Export1.Item.DetailCashReceiptDetail.DistributedAmount.
                    GetValueOrDefault() > 0 || export
                  .Export1.Item.DetailCashReceiptDetail.RefundedAmount.
                    GetValueOrDefault() > 0)
                {
                  var field8 =
                    GetField(export.Export1.Item.DetailCashReceiptDetail,
                    "collectionDate");

                  field8.Color = "cyan";
                  field8.Protected = true;

                  var field9 =
                    GetField(export.Export1.Item.DetailCashReceiptDetail,
                    "collectionAmount");

                  field9.Color = "cyan";
                  field9.Protected = true;

                  var field10 =
                    GetField(export.Export1.Item.DetailCollectionType, "code");

                  field10.Color = "cyan";
                  field10.Protected = true;

                  var field11 =
                    GetField(export.Export1.Item.DetCollTypPrompt, "promptField");
                    

                  field11.Color = "cyan";
                  field11.Protected = true;
                }
              }
            }

            ExitState = "FN0000_COLL_RELEASE_SUCCESSFUL";
          }
        }

        break;
      case "MIGRATE":
        // ----------------------------------------------------------------------
        // PF18 KAECSES Hist
        // Record request to migrate KAECSES payment history for AP/Payor
        // and Court Order specified.
        // Validate that the standard number does not exceed the maximum
        // KAECSES length of 12.
        // ----------------------------------------------------------------------
        if (Length(TrimEnd(export.LegalAction.StandardNumber)) > 12)
        {
          var field = GetField(export.LegalAction, "standardNumber");

          field.Error = true;

          ExitState = "LE0000_INVALID_KAECSES_STD_NUM";

          return;
        }

        // ----------------------------------------------------------------------
        // Process request to migrate payment history.
        // ----------------------------------------------------------------------
        local.InterfacePaymentHistoryReques.ObligorCsePersonNumber =
          export.CsePerson.Number;
        local.InterfacePaymentHistoryReques.KaecsesCourtOrderNumber =
          export.LegalAction.StandardNumber ?? Spaces(14);
        UseFnReipMigratePmtHistRequest();

        break;
      case "CRCN":
        for(export.Export1.Index = 0; export.Export1.Index < export
          .Export1.Count; ++export.Export1.Index)
        {
          switch(AsChar(export.Export1.Item.DetailCommon.SelectChar))
          {
            case 'S':
              MoveCashReceiptSourceType(export.Export1.Item.
                DetailCashReceiptSourceType, export.PassCashReceiptSourceType);
              export.PassCashReceiptEvent.SystemGeneratedIdentifier =
                export.Export1.Item.HiddenCashReceiptEvent.
                  SystemGeneratedIdentifier;
              export.PassCashReceiptType.SystemGeneratedIdentifier =
                export.Export1.Item.HiddenCashReceiptType.
                  SystemGeneratedIdentifier;
              export.PassCashReceipt.SequentialNumber =
                export.Export1.Item.DetailCashReceipt.SequentialNumber;
              export.PassCashReceiptDetail.Assign(
                export.Export1.Item.DetailCashReceiptDetail);
              ExitState = "ECO_LNK_TO_CRCN";

              return;
            case ' ':
              break;
            default:
              ExitState = "ACO_NE0000_INVALID_SELECT_CODE1";

              var field =
                GetField(export.Export1.Item.DetailCommon, "selectChar");

              field.Error = true;

              return;
          }
        }

        for(export.Export1.Index = 0; export.Export1.Index < export
          .Export1.Count; ++export.Export1.Index)
        {
          var field = GetField(export.Export1.Item.DetailCommon, "selectChar");

          field.Error = true;

          break;
        }

        ExitState = "ACO_NE0000_NO_SELECTION_MADE";

        break;
      case "EXIT":
        ExitState = "ECO_XFR_TO_DISB_MGMNT_MENU";

        break;
      case "PREV":
        for(export.Export1.Index = 0; export.Export1.Index < export
          .Export1.Count; ++export.Export1.Index)
        {
          if (IsEmpty(export.Export1.Item.DetailCashReceiptSourceType.Code))
          {
            var field =
              GetField(export.Export1.Item.DetailCashReceiptSourceType, "code");
              

            field.Color = "red";
            field.Intensity = Intensity.High;
            field.Highlighting = Highlighting.ReverseVideo;
            field.Protected = true;
          }

          if (IsEmpty(export.Export1.Item.DetailCashReceiptDetailStatus.Code))
          {
            var field =
              GetField(export.Export1.Item.DetailCashReceiptDetailStatus, "code");
              

            field.Color = "red";
            field.Intensity = Intensity.High;
            field.Highlighting = Highlighting.ReverseVideo;
            field.Protected = true;
          }

          if (IsEmpty(export.Export1.Item.DetailCollectionType.Code))
          {
            var field =
              GetField(export.Export1.Item.DetailCollectionType, "code");

            field.Color = "red";
            field.Intensity = Intensity.High;
            field.Highlighting = Highlighting.ReverseVideo;
            field.Protected = false;
          }
        }

        ExitState = "ACO_NI0000_TOP_OF_LIST";

        break;
      case "NEXT":
        for(export.Export1.Index = 0; export.Export1.Index < export
          .Export1.Count; ++export.Export1.Index)
        {
          if (IsEmpty(export.Export1.Item.DetailCashReceiptSourceType.Code))
          {
            var field =
              GetField(export.Export1.Item.DetailCashReceiptSourceType, "code");
              

            field.Color = "red";
            field.Intensity = Intensity.High;
            field.Highlighting = Highlighting.ReverseVideo;
            field.Protected = true;
          }

          if (IsEmpty(export.Export1.Item.DetailCashReceiptDetailStatus.Code))
          {
            var field =
              GetField(export.Export1.Item.DetailCashReceiptDetailStatus, "code");
              

            field.Color = "red";
            field.Intensity = Intensity.High;
            field.Highlighting = Highlighting.ReverseVideo;
            field.Protected = true;
          }

          if (IsEmpty(export.Export1.Item.DetailCollectionType.Code))
          {
            var field =
              GetField(export.Export1.Item.DetailCollectionType, "code");

            field.Color = "red";
            field.Intensity = Intensity.High;
            field.Highlighting = Highlighting.ReverseVideo;
            field.Protected = false;
          }
        }

        ExitState = "ACO_NI0000_BOTTOM_OF_LIST";

        break;
      case "RETURN":
        for(export.Export1.Index = 0; export.Export1.Index < export
          .Export1.Count; ++export.Export1.Index)
        {
          switch(AsChar(export.Export1.Item.DetailCommon.SelectChar))
          {
            case 'S':
              MoveCashReceiptSourceType(export.Export1.Item.
                DetailCashReceiptSourceType, export.PassCashReceiptSourceType);
              export.PassCashReceiptEvent.SystemGeneratedIdentifier =
                export.Export1.Item.HiddenCashReceiptEvent.
                  SystemGeneratedIdentifier;
              export.PassCashReceiptType.SystemGeneratedIdentifier =
                export.Export1.Item.HiddenCashReceiptType.
                  SystemGeneratedIdentifier;
              export.PassCashReceipt.SequentialNumber =
                export.Export1.Item.DetailCashReceipt.SequentialNumber;
              export.PassCashReceiptDetail.Assign(
                export.Export1.Item.DetailCashReceiptDetail);

              goto AfterCycle;
            case ' ':
              break;
            default:
              ExitState = "ACO_NE0000_INVALID_SELECT_CODE1";

              var field =
                GetField(export.Export1.Item.DetailCommon, "selectChar");

              field.Error = true;

              return;
          }
        }

AfterCycle:

        ExitState = "ACO_NE0000_RETURN";

        break;
      case "LCDA":
        for(export.Export1.Index = 0; export.Export1.Index < export
          .Export1.Count; ++export.Export1.Index)
        {
          if (AsChar(export.Export1.Item.DetailCommon.SelectChar) == 'S')
          {
            export.PassCashReceiptDetail.Assign(
              export.Export1.Item.DetailCashReceiptDetail);

            break;
          }
          else
          {
          }
        }

        ExitState = "ECO_LNK_TO_LCDA";

        break;
      case "SIGNOFF":
        UseScCabSignoff();

        break;
      case "ENTER":
        ExitState = "ACO_NE0000_INVALID_COMMAND";

        break;
      case "":
        break;
      case "ADJUST":
        // 01/31/00  P. Phinney  H00084245  Add PFkey to change Susp to Adj
        // ----------------------------------------------------------------------
        // PF20 Adj
        // Change TOTALLY Suspended Collections to Adjusted.
        // ----------------------------------------------------------------------
        local.Selected.Count = 0;

        for(export.Export1.Index = 0; export.Export1.Index < export
          .Export1.Count; ++export.Export1.Index)
        {
          switch(AsChar(export.Export1.Item.DetailCommon.SelectChar))
          {
            case 'S':
              // ----------------------------------------------------------------------
              // PF20 Adj
              // Change TOTALLY Suspended Collections to Adjusted.
              // ----------------------------------------------------------------------
              if (!Equal(export.Export1.Item.DetailCashReceiptDetailStatus.Code,
                "SUSP"))
              {
                ExitState = "FN0000_PAYMENT_HIST_NOT_SUSP";
              }

              if (export.Export1.Item.DetailCashReceiptDetail.DistributedAmount.
                GetValueOrDefault() > 0 || export
                .Export1.Item.DetailCashReceiptDetail.RefundedAmount.
                  GetValueOrDefault() > 0)
              {
                ExitState = "FN0000_COLL_OR_REF_EXIST_NO_UD";
              }

              if (!IsExitState("ACO_NN0000_ALL_OK"))
              {
                var field3 =
                  GetField(export.Export1.Item.DetailCommon, "selectChar");

                field3.Error = true;

                return;
              }

              ++local.Selected.Count;

              break;
            case ' ':
              break;
            default:
              ExitState = "ACO_NE0000_INVALID_SELECT_CODE1";

              var field =
                GetField(export.Export1.Item.DetailCommon, "selectChar");

              field.Error = true;

              return;
          }
        }

        if (local.Selected.Count == 0)
        {
          for(export.Export1.Index = 0; export.Export1.Index < export
            .Export1.Count; ++export.Export1.Index)
          {
            var field =
              GetField(export.Export1.Item.DetailCommon, "selectChar");

            field.Error = true;

            break;
          }

          ExitState = "ACO_NE0000_NO_SELECTION_MADE";

          return;
        }

        // -------------------------------------------------------------------
        // Change Selected payment history records from SUSP to ADJ.
        // -------------------------------------------------------------------
        for(export.Export1.Index = 0; export.Export1.Index < export
          .Export1.Count; ++export.Export1.Index)
        {
          // ----------------------------------------------------------------------
          // PF20 Adj
          // Change TOTALLY Suspended Collections to Adjusted.
          // ----------------------------------------------------------------------
          switch(AsChar(export.Export1.Item.DetailCommon.SelectChar))
          {
            case 'S':
              export.PassCashReceipt.SequentialNumber =
                export.Export1.Item.DetailCashReceipt.SequentialNumber;
              export.PassCashReceiptDetail.Assign(
                export.Export1.Item.DetailCashReceiptDetail);
              UseFnReipChangeSuspToAdj();

              if (!IsExitState("ACO_NN0000_ALL_OK"))
              {
                var field3 =
                  GetField(export.Export1.Item.DetailCommon, "selectChar");

                field3.Error = true;

                return;
              }

              break;
            case ' ':
              break;
            default:
              ExitState = "ACO_RE0000_INVALID_SELECT_CODE";

              var field =
                GetField(export.Export1.Item.DetailCommon, "selectChar");

              field.Error = true;

              return;
          }
        }

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          // -------------------------------------------------------------------
          // Display payment history records after completing the updates.
          // -------------------------------------------------------------------
          ExitState = "ACO_NN0000_ALL_OK";
          UseFnReipDisplayPaymentHistory();

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            for(export.Export1.Index = 0; export.Export1.Index < export
              .Export1.Count; ++export.Export1.Index)
            {
              // -------------------------------------------------------------------
              // Protect fields that cannot be changed.
              // -------------------------------------------------------------------
              if (export.Export1.Item.DetailCashReceipt.SequentialNumber > 0)
              {
                var field3 =
                  GetField(export.Export1.Item.RecurringToDate, "collectionDate");
                  

                field3.Color = "cyan";
                field3.Protected = true;

                var field4 = GetField(export.Export1.Item.Frequency, "text4");

                field4.Color = "cyan";
                field4.Protected = true;

                var field5 =
                  GetField(export.Export1.Item.DetailCourtOrDp, "selectChar");

                field5.Color = "cyan";
                field5.Protected = true;

                var field6 =
                  GetField(export.Export1.Item.DetailCashReceiptSourceType,
                  "code");

                field6.Color = "cyan";
                field6.Protected = true;

                var field7 =
                  GetField(export.Export1.Item.DetSourceCode, "promptField");

                field7.Color = "cyan";
                field7.Protected = true;

                // --------------------------------------------------------
                // If the current detail status is ADJ or at least part of
                // the collection amount has been distributed or refunded,
                // the payment history record cannot be changed.
                // JLK  03/23/99
                // --------------------------------------------------------
                if (Equal(export.Export1.Item.DetailCashReceiptDetailStatus.
                  Code, "ADJ") || export
                  .Export1.Item.DetailCashReceiptDetail.DistributedAmount.
                    GetValueOrDefault() > 0 || export
                  .Export1.Item.DetailCashReceiptDetail.RefundedAmount.
                    GetValueOrDefault() > 0)
                {
                  var field8 =
                    GetField(export.Export1.Item.DetailCashReceiptDetail,
                    "collectionDate");

                  field8.Color = "cyan";
                  field8.Protected = true;

                  var field9 =
                    GetField(export.Export1.Item.DetailCashReceiptDetail,
                    "collectionAmount");

                  field9.Color = "cyan";
                  field9.Protected = true;

                  var field10 =
                    GetField(export.Export1.Item.DetailCollectionType, "code");

                  field10.Color = "cyan";
                  field10.Protected = true;

                  var field11 =
                    GetField(export.Export1.Item.DetCollTypPrompt, "promptField");
                    

                  field11.Color = "cyan";
                  field11.Protected = true;
                }
              }
            }

            ExitState = "FN0000_COLL_ADJUSTMENT_ADDED";
          }
        }

        break;
      default:
        ExitState = "ACO_NE0000_INVALID_PF_KEY";

        break;
    }
  }

  private static void MoveCashReceipt(CashReceipt source, CashReceipt target)
  {
    target.CheckNumber = source.CheckNumber;
    target.ReferenceNumber = source.ReferenceNumber;
  }

  private static void MoveCashReceiptDetail(CashReceiptDetail source,
    CashReceiptDetail target)
  {
    target.CollectionAmount = source.CollectionAmount;
    target.CollectionDate = source.CollectionDate;
  }

  private static void MoveCashReceiptSourceType(CashReceiptSourceType source,
    CashReceiptSourceType target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Code = source.Code;
  }

  private static void MoveCollectionType(CollectionType source,
    CollectionType target)
  {
    target.SequentialIdentifier = source.SequentialIdentifier;
    target.Code = source.Code;
  }

  private static void MoveExport1(FnReipDisplayPaymentHistory.Export.
    ExportGroup source, Export.ExportGroup target)
  {
    target.DetailCommon.SelectChar = source.DetailCommon.SelectChar;
    target.DetailCashReceiptDetail.Assign(source.DetailCashReceiptDetail);
    target.RecurringToDate.CollectionDate =
      source.RecurringToDate.CollectionDate;
    target.Frequency.Text4 = source.Frequency.Text4;
    target.DetailCourtOrDp.SelectChar = source.CourtOrDp.SelectChar;
    MoveCashReceiptSourceType(source.DetailCashReceiptSourceType,
      target.DetailCashReceiptSourceType);
    target.DetSourceCode.PromptField = source.DetSourcePrompt.PromptField;
    MoveCollectionType(source.DetailCollectionType, target.DetailCollectionType);
      
    target.DetCollTypPrompt.PromptField = source.DetCollTypPrompt.PromptField;
    target.DetailCashReceipt.Assign(source.DetailCashReceipt);
    target.DetailCashReceiptDetailStatus.Code =
      source.DetailCashReceiptDetailStatus.Code;
    target.DetailManDistInd.Flag = source.DetManDistInd.Flag;
    target.DetailNoteInd.Flag = source.DetNoteInd.Flag;
    target.HiddenCashReceiptEvent.SystemGeneratedIdentifier =
      source.HiddenCashReceiptEvent.SystemGeneratedIdentifier;
    target.HiddenCashReceiptType.SystemGeneratedIdentifier =
      source.HiddenCashReceiptType.SystemGeneratedIdentifier;
  }

  private static void MoveInterfacePaymentHistoryReques(
    InterfacePaymentHistoryReques source, InterfacePaymentHistoryReques target)
  {
    target.ObligorCsePersonNumber = source.ObligorCsePersonNumber;
    target.KaecsesCourtOrderNumber = source.KaecsesCourtOrderNumber;
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

  private void UseFnCabCheckPersOblrInLact()
  {
    var useImport = new FnCabCheckPersOblrInLact.Import();
    var useExport = new FnCabCheckPersOblrInLact.Export();

    useImport.CsePerson.Number = export.CsePerson.Number;
    MoveLegalAction(export.LegalAction, useImport.LegalAction);

    Call(FnCabCheckPersOblrInLact.Execute, useImport, useExport);

    local.PersObligorInLact.Flag = useExport.PersObligorInLact.Flag;
  }

  private void UseFnCabValidateLegalAction()
  {
    var useImport = new FnCabValidateLegalAction.Import();
    var useExport = new FnCabValidateLegalAction.Export();

    MoveLegalAction(export.LegalAction, useImport.LegalAction);

    Call(FnCabValidateLegalAction.Execute, useImport, useExport);

    MoveLegalAction(useExport.LegalAction, export.HiddenLegalAction);
  }

  private void UseFnHardcodedCashReceipting()
  {
    var useImport = new FnHardcodedCashReceipting.Import();
    var useExport = new FnHardcodedCashReceipting.Export();

    Call(FnHardcodedCashReceipting.Execute, useImport, useExport);

    local.HardcodedCrtFdirPmt.SystemGeneratedIdentifier =
      useExport.CrtSystemId.CrtIdFdirPmt.SystemGeneratedIdentifier;
    local.HardcodedCrtFcourtPmt.SystemGeneratedIdentifier =
      useExport.CrtSystemId.CrtIdFcrtRec.SystemGeneratedIdentifier;
  }

  private void UseFnReipChangeSuspToAdj()
  {
    var useImport = new FnReipChangeSuspToAdj.Import();
    var useExport = new FnReipChangeSuspToAdj.Export();

    useImport.CashReceipt.SequentialNumber =
      export.PassCashReceipt.SequentialNumber;
    useImport.CashReceiptDetail.SequentialIdentifier =
      export.PassCashReceiptDetail.SequentialIdentifier;

    Call(FnReipChangeSuspToAdj.Execute, useImport, useExport);
  }

  private void UseFnReipCreatePaymentHistory1()
  {
    var useImport = new FnReipCreatePaymentHistory.Import();
    var useExport = new FnReipCreatePaymentHistory.Export();

    MoveCashReceiptSourceType(export.Export1.Item.DetailCashReceiptSourceType,
      useImport.CashReceiptSourceType);
    useImport.CashReceiptType.SystemGeneratedIdentifier =
      export.Export1.Item.HiddenCashReceiptType.SystemGeneratedIdentifier;
    useImport.CollectionType.Code =
      export.Export1.Item.DetailCollectionType.Code;
    MoveCashReceipt(export.Export1.Item.DetailCashReceipt, useImport.CashReceipt);
      
    useImport.CsePersonsWorkSet.Assign(export.CsePersonsWorkSet);
    useImport.LegalAction.StandardNumber = export.LegalAction.StandardNumber;
    MoveCashReceiptDetail(local.Pass, useImport.CashReceiptDetail);

    Call(FnReipCreatePaymentHistory.Execute, useImport, useExport);
  }

  private void UseFnReipCreatePaymentHistory2()
  {
    var useImport = new FnReipCreatePaymentHistory.Import();
    var useExport = new FnReipCreatePaymentHistory.Export();

    MoveCashReceiptSourceType(export.Export1.Item.DetailCashReceiptSourceType,
      useImport.CashReceiptSourceType);
    useImport.CashReceiptType.SystemGeneratedIdentifier =
      export.Export1.Item.HiddenCashReceiptType.SystemGeneratedIdentifier;
    useImport.CollectionType.Code =
      export.Export1.Item.DetailCollectionType.Code;
    MoveCashReceipt(export.Export1.Item.DetailCashReceipt, useImport.CashReceipt);
      
    useImport.CsePersonsWorkSet.Assign(export.CsePersonsWorkSet);
    useImport.LegalAction.StandardNumber = export.LegalAction.StandardNumber;
    MoveCashReceiptDetail(export.Export1.Item.DetailCashReceiptDetail,
      useImport.CashReceiptDetail);

    Call(FnReipCreatePaymentHistory.Execute, useImport, useExport);
  }

  private void UseFnReipDeletePaymentHistory()
  {
    var useImport = new FnReipDeletePaymentHistory.Import();
    var useExport = new FnReipDeletePaymentHistory.Export();

    useImport.CashReceiptSourceType.SystemGeneratedIdentifier =
      export.Export1.Item.DetailCashReceiptSourceType.SystemGeneratedIdentifier;
      
    useImport.CashReceiptEvent.SystemGeneratedIdentifier =
      export.Export1.Item.HiddenCashReceiptEvent.SystemGeneratedIdentifier;
    useImport.CashReceiptType.SystemGeneratedIdentifier =
      export.Export1.Item.HiddenCashReceiptType.SystemGeneratedIdentifier;
    useImport.CashReceipt.SequentialNumber =
      export.Export1.Item.DetailCashReceipt.SequentialNumber;
    useImport.CashReceiptDetail.SequentialIdentifier =
      export.Export1.Item.DetailCashReceiptDetail.SequentialIdentifier;

    Call(FnReipDeletePaymentHistory.Execute, useImport, useExport);
  }

  private void UseFnReipDisplayPaymentHistory()
  {
    var useImport = new FnReipDisplayPaymentHistory.Import();
    var useExport = new FnReipDisplayPaymentHistory.Export();

    useImport.LegalAction.StandardNumber = export.LegalAction.StandardNumber;
    useImport.CsePerson.Number = export.CsePerson.Number;
    useImport.ListStarting.CollectionDate = export.ListStarting.CollectionDate;
    useImport.ListEnding.CollectionDate = export.ListEnding.CollectionDate;

    Call(FnReipDisplayPaymentHistory.Execute, useImport, useExport);

    export.TotalItems.Count = useExport.TotalItems.Count;
    export.TotalPmt.TotalCurrency = useExport.TotalPmt.TotalCurrency;
    export.TotalDist.TotalCurrency = useExport.TotalDist.TotalCurrency;
    export.TotalRef.TotalCurrency = useExport.TotalRef.TotalCurrency;
    export.TotalAdj.TotalCurrency = useExport.TotalAdj.TotalCurrency;
    export.TotalUndist.TotalCurrency = useExport.TotalUndist.TotalCurrency;
    useExport.Export1.CopyTo(export.Export1, MoveExport1);
  }

  private void UseFnReipFindSimilarPmtHist()
  {
    var useImport = new FnReipFindSimilarPmtHist.Import();
    var useExport = new FnReipFindSimilarPmtHist.Export();

    MoveCashReceiptSourceType(export.Export1.Item.DetailCashReceiptSourceType,
      useImport.CashReceiptSourceType);
    useImport.CsePersonsWorkSet.Number = export.CsePersonsWorkSet.Number;
    useImport.LegalAction.StandardNumber = export.LegalAction.StandardNumber;
    MoveCashReceiptDetail(export.Export1.Item.DetailCashReceiptDetail,
      useImport.CashReceiptDetail);

    Call(FnReipFindSimilarPmtHist.Execute, useImport, useExport);
  }

  private void UseFnReipMarkForManualDistrib()
  {
    var useImport = new FnReipMarkForManualDistrib.Import();
    var useExport = new FnReipMarkForManualDistrib.Export();

    useImport.CashReceipt.SequentialNumber =
      export.Export1.Item.DetailCashReceipt.SequentialNumber;
    useImport.CashReceiptDetail.SequentialIdentifier =
      export.Export1.Item.DetailCashReceiptDetail.SequentialIdentifier;

    Call(FnReipMarkForManualDistrib.Execute, useImport, useExport);

    local.New1.Code = useExport.New1.Code;
  }

  private void UseFnReipMigratePmtHistRequest()
  {
    var useImport = new FnReipMigratePmtHistRequest.Import();
    var useExport = new FnReipMigratePmtHistRequest.Export();

    MoveInterfacePaymentHistoryReques(local.InterfacePaymentHistoryReques,
      useImport.InterfacePaymentHistoryReques);

    Call(FnReipMigratePmtHistRequest.Execute, useImport, useExport);
  }

  private void UseFnReipReleasePaymentHistory()
  {
    var useImport = new FnReipReleasePaymentHistory.Import();
    var useExport = new FnReipReleasePaymentHistory.Export();

    useImport.CsePerson.Number = export.CsePerson.Number;
    MoveLegalAction(export.LegalAction, useImport.LegalAction);

    Call(FnReipReleasePaymentHistory.Execute, useImport, useExport);
  }

  private void UseFnReipUpdatePaymentHistory()
  {
    var useImport = new FnReipUpdatePaymentHistory.Import();
    var useExport = new FnReipUpdatePaymentHistory.Export();

    MoveCashReceiptSourceType(export.Export1.Item.DetailCashReceiptSourceType,
      useImport.CashReceiptSourceType);
    useImport.CashReceiptType.SystemGeneratedIdentifier =
      export.Export1.Item.HiddenCashReceiptType.SystemGeneratedIdentifier;
    useImport.CashReceipt.SequentialNumber =
      export.Export1.Item.DetailCashReceipt.SequentialNumber;
    useImport.CashReceiptDetail.Assign(
      export.Export1.Item.DetailCashReceiptDetail);
    useImport.New1.Code = export.Export1.Item.DetailCollectionType.Code;
    useImport.CrOrDp.SelectChar =
      export.Export1.Item.DetailCourtOrDp.SelectChar;

    Call(FnReipUpdatePaymentHistory.Execute, useImport, useExport);
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

    useImport.CsePerson.Number = export.CsePerson.Number;
    useImport.CsePersonsWorkSet.Number = export.CsePersonsWorkSet.Number;
    MoveLegalAction(export.LegalAction, useImport.LegalAction);

    Call(ScCabTestSecurity.Execute, useImport, useExport);
  }

  private void UseSiReadCsePerson()
  {
    var useImport = new SiReadCsePerson.Import();
    var useExport = new SiReadCsePerson.Export();

    useImport.CsePersonsWorkSet.Number = export.HiddenCsePersonsWorkSet.Number;

    Call(SiReadCsePerson.Execute, useImport, useExport);

    export.CsePerson.Number = useExport.CsePerson.Number;
    export.CsePersonsWorkSet.Assign(useExport.CsePersonsWorkSet);
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
      /// A value of DetailCommon.
      /// </summary>
      [JsonPropertyName("detailCommon")]
      public Common DetailCommon
      {
        get => detailCommon ??= new();
        set => detailCommon = value;
      }

      /// <summary>
      /// A value of DetailCashReceiptDetail.
      /// </summary>
      [JsonPropertyName("detailCashReceiptDetail")]
      public CashReceiptDetail DetailCashReceiptDetail
      {
        get => detailCashReceiptDetail ??= new();
        set => detailCashReceiptDetail = value;
      }

      /// <summary>
      /// A value of RecurringToDate.
      /// </summary>
      [JsonPropertyName("recurringToDate")]
      public CashReceiptDetail RecurringToDate
      {
        get => recurringToDate ??= new();
        set => recurringToDate = value;
      }

      /// <summary>
      /// A value of Frequency.
      /// </summary>
      [JsonPropertyName("frequency")]
      public TextWorkArea Frequency
      {
        get => frequency ??= new();
        set => frequency = value;
      }

      /// <summary>
      /// A value of DetailCourtOrDp.
      /// </summary>
      [JsonPropertyName("detailCourtOrDp")]
      public Common DetailCourtOrDp
      {
        get => detailCourtOrDp ??= new();
        set => detailCourtOrDp = value;
      }

      /// <summary>
      /// A value of DetailCashReceiptSourceType.
      /// </summary>
      [JsonPropertyName("detailCashReceiptSourceType")]
      public CashReceiptSourceType DetailCashReceiptSourceType
      {
        get => detailCashReceiptSourceType ??= new();
        set => detailCashReceiptSourceType = value;
      }

      /// <summary>
      /// A value of DetSourceCode.
      /// </summary>
      [JsonPropertyName("detSourceCode")]
      public Standard DetSourceCode
      {
        get => detSourceCode ??= new();
        set => detSourceCode = value;
      }

      /// <summary>
      /// A value of DetailCollectionType.
      /// </summary>
      [JsonPropertyName("detailCollectionType")]
      public CollectionType DetailCollectionType
      {
        get => detailCollectionType ??= new();
        set => detailCollectionType = value;
      }

      /// <summary>
      /// A value of DetCollTypPrompt.
      /// </summary>
      [JsonPropertyName("detCollTypPrompt")]
      public Standard DetCollTypPrompt
      {
        get => detCollTypPrompt ??= new();
        set => detCollTypPrompt = value;
      }

      /// <summary>
      /// A value of DetailCashReceipt.
      /// </summary>
      [JsonPropertyName("detailCashReceipt")]
      public CashReceipt DetailCashReceipt
      {
        get => detailCashReceipt ??= new();
        set => detailCashReceipt = value;
      }

      /// <summary>
      /// A value of DetailCashReceiptDetailStatus.
      /// </summary>
      [JsonPropertyName("detailCashReceiptDetailStatus")]
      public CashReceiptDetailStatus DetailCashReceiptDetailStatus
      {
        get => detailCashReceiptDetailStatus ??= new();
        set => detailCashReceiptDetailStatus = value;
      }

      /// <summary>
      /// A value of DetailManDistInd.
      /// </summary>
      [JsonPropertyName("detailManDistInd")]
      public Common DetailManDistInd
      {
        get => detailManDistInd ??= new();
        set => detailManDistInd = value;
      }

      /// <summary>
      /// A value of DetailNoteInd.
      /// </summary>
      [JsonPropertyName("detailNoteInd")]
      public Common DetailNoteInd
      {
        get => detailNoteInd ??= new();
        set => detailNoteInd = value;
      }

      /// <summary>
      /// A value of HiddenCashReceiptEvent.
      /// </summary>
      [JsonPropertyName("hiddenCashReceiptEvent")]
      public CashReceiptEvent HiddenCashReceiptEvent
      {
        get => hiddenCashReceiptEvent ??= new();
        set => hiddenCashReceiptEvent = value;
      }

      /// <summary>
      /// A value of HiddenCashReceiptType.
      /// </summary>
      [JsonPropertyName("hiddenCashReceiptType")]
      public CashReceiptType HiddenCashReceiptType
      {
        get => hiddenCashReceiptType ??= new();
        set => hiddenCashReceiptType = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 50;

      private Common detailCommon;
      private CashReceiptDetail detailCashReceiptDetail;
      private CashReceiptDetail recurringToDate;
      private TextWorkArea frequency;
      private Common detailCourtOrDp;
      private CashReceiptSourceType detailCashReceiptSourceType;
      private Standard detSourceCode;
      private CollectionType detailCollectionType;
      private Standard detCollTypPrompt;
      private CashReceipt detailCashReceipt;
      private CashReceiptDetailStatus detailCashReceiptDetailStatus;
      private Common detailManDistInd;
      private Common detailNoteInd;
      private CashReceiptEvent hiddenCashReceiptEvent;
      private CashReceiptType hiddenCashReceiptType;
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
    /// A value of Payor.
    /// </summary>
    [JsonPropertyName("payor")]
    public Standard Payor
    {
      get => payor ??= new();
      set => payor = value;
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
    /// A value of CourtOrder.
    /// </summary>
    [JsonPropertyName("courtOrder")]
    public Standard CourtOrder
    {
      get => courtOrder ??= new();
      set => courtOrder = value;
    }

    /// <summary>
    /// A value of ListStarting.
    /// </summary>
    [JsonPropertyName("listStarting")]
    public CashReceiptDetail ListStarting
    {
      get => listStarting ??= new();
      set => listStarting = value;
    }

    /// <summary>
    /// A value of ListEnding.
    /// </summary>
    [JsonPropertyName("listEnding")]
    public CashReceiptDetail ListEnding
    {
      get => listEnding ??= new();
      set => listEnding = value;
    }

    /// <summary>
    /// A value of TotalItems.
    /// </summary>
    [JsonPropertyName("totalItems")]
    public Common TotalItems
    {
      get => totalItems ??= new();
      set => totalItems = value;
    }

    /// <summary>
    /// A value of TotalPmt.
    /// </summary>
    [JsonPropertyName("totalPmt")]
    public Common TotalPmt
    {
      get => totalPmt ??= new();
      set => totalPmt = value;
    }

    /// <summary>
    /// A value of TotalDist.
    /// </summary>
    [JsonPropertyName("totalDist")]
    public Common TotalDist
    {
      get => totalDist ??= new();
      set => totalDist = value;
    }

    /// <summary>
    /// A value of TotalRef.
    /// </summary>
    [JsonPropertyName("totalRef")]
    public Common TotalRef
    {
      get => totalRef ??= new();
      set => totalRef = value;
    }

    /// <summary>
    /// A value of TotalAdj.
    /// </summary>
    [JsonPropertyName("totalAdj")]
    public Common TotalAdj
    {
      get => totalAdj ??= new();
      set => totalAdj = value;
    }

    /// <summary>
    /// A value of TotalUndist.
    /// </summary>
    [JsonPropertyName("totalUndist")]
    public Common TotalUndist
    {
      get => totalUndist ??= new();
      set => totalUndist = value;
    }

    /// <summary>
    /// A value of UndistCruc.
    /// </summary>
    [JsonPropertyName("undistCruc")]
    public Standard UndistCruc
    {
      get => undistCruc ??= new();
      set => undistCruc = value;
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
    /// A value of Continue1.
    /// </summary>
    [JsonPropertyName("continue1")]
    public Common Continue1
    {
      get => continue1 ??= new();
      set => continue1 = value;
    }

    /// <summary>
    /// A value of HiddenCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("hiddenCsePersonsWorkSet")]
    public CsePersonsWorkSet HiddenCsePersonsWorkSet
    {
      get => hiddenCsePersonsWorkSet ??= new();
      set => hiddenCsePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of HiddenLegalAction.
    /// </summary>
    [JsonPropertyName("hiddenLegalAction")]
    public LegalAction HiddenLegalAction
    {
      get => hiddenLegalAction ??= new();
      set => hiddenLegalAction = value;
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
    /// A value of Standard.
    /// </summary>
    [JsonPropertyName("standard")]
    public Standard Standard
    {
      get => standard ??= new();
      set => standard = value;
    }

    /// <summary>
    /// A value of ReturnedCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("returnedCsePersonsWorkSet")]
    public CsePersonsWorkSet ReturnedCsePersonsWorkSet
    {
      get => returnedCsePersonsWorkSet ??= new();
      set => returnedCsePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of ReturnedLegalAction.
    /// </summary>
    [JsonPropertyName("returnedLegalAction")]
    public LegalAction ReturnedLegalAction
    {
      get => returnedLegalAction ??= new();
      set => returnedLegalAction = value;
    }

    /// <summary>
    /// A value of ReturnedClctSelected.
    /// </summary>
    [JsonPropertyName("returnedClctSelected")]
    public CollectionType ReturnedClctSelected
    {
      get => returnedClctSelected ??= new();
      set => returnedClctSelected = value;
    }

    /// <summary>
    /// A value of ReturnedToGroupCrs.
    /// </summary>
    [JsonPropertyName("returnedToGroupCrs")]
    public CashReceiptSourceType ReturnedToGroupCrs
    {
      get => returnedToGroupCrs ??= new();
      set => returnedToGroupCrs = value;
    }

    private CsePerson csePerson;
    private Standard payor;
    private CsePersonsWorkSet csePersonsWorkSet;
    private LegalAction legalAction;
    private Standard courtOrder;
    private CashReceiptDetail listStarting;
    private CashReceiptDetail listEnding;
    private Common totalItems;
    private Common totalPmt;
    private Common totalDist;
    private Common totalRef;
    private Common totalAdj;
    private Common totalUndist;
    private Standard undistCruc;
    private Array<ImportGroup> import1;
    private Common continue1;
    private CsePersonsWorkSet hiddenCsePersonsWorkSet;
    private LegalAction hiddenLegalAction;
    private NextTranInfo hiddenNextTranInfo;
    private Standard standard;
    private CsePersonsWorkSet returnedCsePersonsWorkSet;
    private LegalAction returnedLegalAction;
    private CollectionType returnedClctSelected;
    private CashReceiptSourceType returnedToGroupCrs;
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
      /// A value of DetailCommon.
      /// </summary>
      [JsonPropertyName("detailCommon")]
      public Common DetailCommon
      {
        get => detailCommon ??= new();
        set => detailCommon = value;
      }

      /// <summary>
      /// A value of DetailCashReceiptDetail.
      /// </summary>
      [JsonPropertyName("detailCashReceiptDetail")]
      public CashReceiptDetail DetailCashReceiptDetail
      {
        get => detailCashReceiptDetail ??= new();
        set => detailCashReceiptDetail = value;
      }

      /// <summary>
      /// A value of RecurringToDate.
      /// </summary>
      [JsonPropertyName("recurringToDate")]
      public CashReceiptDetail RecurringToDate
      {
        get => recurringToDate ??= new();
        set => recurringToDate = value;
      }

      /// <summary>
      /// A value of Frequency.
      /// </summary>
      [JsonPropertyName("frequency")]
      public TextWorkArea Frequency
      {
        get => frequency ??= new();
        set => frequency = value;
      }

      /// <summary>
      /// A value of DetailCourtOrDp.
      /// </summary>
      [JsonPropertyName("detailCourtOrDp")]
      public Common DetailCourtOrDp
      {
        get => detailCourtOrDp ??= new();
        set => detailCourtOrDp = value;
      }

      /// <summary>
      /// A value of DetailCashReceiptSourceType.
      /// </summary>
      [JsonPropertyName("detailCashReceiptSourceType")]
      public CashReceiptSourceType DetailCashReceiptSourceType
      {
        get => detailCashReceiptSourceType ??= new();
        set => detailCashReceiptSourceType = value;
      }

      /// <summary>
      /// A value of DetSourceCode.
      /// </summary>
      [JsonPropertyName("detSourceCode")]
      public Standard DetSourceCode
      {
        get => detSourceCode ??= new();
        set => detSourceCode = value;
      }

      /// <summary>
      /// A value of DetailCollectionType.
      /// </summary>
      [JsonPropertyName("detailCollectionType")]
      public CollectionType DetailCollectionType
      {
        get => detailCollectionType ??= new();
        set => detailCollectionType = value;
      }

      /// <summary>
      /// A value of DetCollTypPrompt.
      /// </summary>
      [JsonPropertyName("detCollTypPrompt")]
      public Standard DetCollTypPrompt
      {
        get => detCollTypPrompt ??= new();
        set => detCollTypPrompt = value;
      }

      /// <summary>
      /// A value of DetailCashReceipt.
      /// </summary>
      [JsonPropertyName("detailCashReceipt")]
      public CashReceipt DetailCashReceipt
      {
        get => detailCashReceipt ??= new();
        set => detailCashReceipt = value;
      }

      /// <summary>
      /// A value of DetailCashReceiptDetailStatus.
      /// </summary>
      [JsonPropertyName("detailCashReceiptDetailStatus")]
      public CashReceiptDetailStatus DetailCashReceiptDetailStatus
      {
        get => detailCashReceiptDetailStatus ??= new();
        set => detailCashReceiptDetailStatus = value;
      }

      /// <summary>
      /// A value of DetailManDistInd.
      /// </summary>
      [JsonPropertyName("detailManDistInd")]
      public Common DetailManDistInd
      {
        get => detailManDistInd ??= new();
        set => detailManDistInd = value;
      }

      /// <summary>
      /// A value of DetailNoteInd.
      /// </summary>
      [JsonPropertyName("detailNoteInd")]
      public Common DetailNoteInd
      {
        get => detailNoteInd ??= new();
        set => detailNoteInd = value;
      }

      /// <summary>
      /// A value of HiddenCashReceiptEvent.
      /// </summary>
      [JsonPropertyName("hiddenCashReceiptEvent")]
      public CashReceiptEvent HiddenCashReceiptEvent
      {
        get => hiddenCashReceiptEvent ??= new();
        set => hiddenCashReceiptEvent = value;
      }

      /// <summary>
      /// A value of HiddenCashReceiptType.
      /// </summary>
      [JsonPropertyName("hiddenCashReceiptType")]
      public CashReceiptType HiddenCashReceiptType
      {
        get => hiddenCashReceiptType ??= new();
        set => hiddenCashReceiptType = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 50;

      private Common detailCommon;
      private CashReceiptDetail detailCashReceiptDetail;
      private CashReceiptDetail recurringToDate;
      private TextWorkArea frequency;
      private Common detailCourtOrDp;
      private CashReceiptSourceType detailCashReceiptSourceType;
      private Standard detSourceCode;
      private CollectionType detailCollectionType;
      private Standard detCollTypPrompt;
      private CashReceipt detailCashReceipt;
      private CashReceiptDetailStatus detailCashReceiptDetailStatus;
      private Common detailManDistInd;
      private Common detailNoteInd;
      private CashReceiptEvent hiddenCashReceiptEvent;
      private CashReceiptType hiddenCashReceiptType;
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
    /// A value of Payor.
    /// </summary>
    [JsonPropertyName("payor")]
    public Standard Payor
    {
      get => payor ??= new();
      set => payor = value;
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
    /// A value of CourtOrder.
    /// </summary>
    [JsonPropertyName("courtOrder")]
    public Standard CourtOrder
    {
      get => courtOrder ??= new();
      set => courtOrder = value;
    }

    /// <summary>
    /// A value of ListStarting.
    /// </summary>
    [JsonPropertyName("listStarting")]
    public CashReceiptDetail ListStarting
    {
      get => listStarting ??= new();
      set => listStarting = value;
    }

    /// <summary>
    /// A value of ListEnding.
    /// </summary>
    [JsonPropertyName("listEnding")]
    public CashReceiptDetail ListEnding
    {
      get => listEnding ??= new();
      set => listEnding = value;
    }

    /// <summary>
    /// A value of TotalItems.
    /// </summary>
    [JsonPropertyName("totalItems")]
    public Common TotalItems
    {
      get => totalItems ??= new();
      set => totalItems = value;
    }

    /// <summary>
    /// A value of TotalPmt.
    /// </summary>
    [JsonPropertyName("totalPmt")]
    public Common TotalPmt
    {
      get => totalPmt ??= new();
      set => totalPmt = value;
    }

    /// <summary>
    /// A value of TotalDist.
    /// </summary>
    [JsonPropertyName("totalDist")]
    public Common TotalDist
    {
      get => totalDist ??= new();
      set => totalDist = value;
    }

    /// <summary>
    /// A value of TotalRef.
    /// </summary>
    [JsonPropertyName("totalRef")]
    public Common TotalRef
    {
      get => totalRef ??= new();
      set => totalRef = value;
    }

    /// <summary>
    /// A value of TotalAdj.
    /// </summary>
    [JsonPropertyName("totalAdj")]
    public Common TotalAdj
    {
      get => totalAdj ??= new();
      set => totalAdj = value;
    }

    /// <summary>
    /// A value of TotalUndist.
    /// </summary>
    [JsonPropertyName("totalUndist")]
    public Common TotalUndist
    {
      get => totalUndist ??= new();
      set => totalUndist = value;
    }

    /// <summary>
    /// A value of UndistCruc.
    /// </summary>
    [JsonPropertyName("undistCruc")]
    public Standard UndistCruc
    {
      get => undistCruc ??= new();
      set => undistCruc = value;
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
    /// A value of Continue1.
    /// </summary>
    [JsonPropertyName("continue1")]
    public Common Continue1
    {
      get => continue1 ??= new();
      set => continue1 = value;
    }

    /// <summary>
    /// A value of HiddenCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("hiddenCsePersonsWorkSet")]
    public CsePersonsWorkSet HiddenCsePersonsWorkSet
    {
      get => hiddenCsePersonsWorkSet ??= new();
      set => hiddenCsePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of HiddenLegalAction.
    /// </summary>
    [JsonPropertyName("hiddenLegalAction")]
    public LegalAction HiddenLegalAction
    {
      get => hiddenLegalAction ??= new();
      set => hiddenLegalAction = value;
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
    /// A value of Standard.
    /// </summary>
    [JsonPropertyName("standard")]
    public Standard Standard
    {
      get => standard ??= new();
      set => standard = value;
    }

    /// <summary>
    /// A value of PassCashReceiptSourceType.
    /// </summary>
    [JsonPropertyName("passCashReceiptSourceType")]
    public CashReceiptSourceType PassCashReceiptSourceType
    {
      get => passCashReceiptSourceType ??= new();
      set => passCashReceiptSourceType = value;
    }

    /// <summary>
    /// A value of PassCashReceiptEvent.
    /// </summary>
    [JsonPropertyName("passCashReceiptEvent")]
    public CashReceiptEvent PassCashReceiptEvent
    {
      get => passCashReceiptEvent ??= new();
      set => passCashReceiptEvent = value;
    }

    /// <summary>
    /// A value of PassCashReceiptType.
    /// </summary>
    [JsonPropertyName("passCashReceiptType")]
    public CashReceiptType PassCashReceiptType
    {
      get => passCashReceiptType ??= new();
      set => passCashReceiptType = value;
    }

    /// <summary>
    /// A value of PassCashReceipt.
    /// </summary>
    [JsonPropertyName("passCashReceipt")]
    public CashReceipt PassCashReceipt
    {
      get => passCashReceipt ??= new();
      set => passCashReceipt = value;
    }

    /// <summary>
    /// A value of PassCashReceiptDetail.
    /// </summary>
    [JsonPropertyName("passCashReceiptDetail")]
    public CashReceiptDetail PassCashReceiptDetail
    {
      get => passCashReceiptDetail ??= new();
      set => passCashReceiptDetail = value;
    }

    /// <summary>
    /// A value of Phonetic.
    /// </summary>
    [JsonPropertyName("phonetic")]
    public Common Phonetic
    {
      get => phonetic ??= new();
      set => phonetic = value;
    }

    /// <summary>
    /// A value of PassPayHistInd.
    /// </summary>
    [JsonPropertyName("passPayHistInd")]
    public Common PassPayHistInd
    {
      get => passPayHistInd ??= new();
      set => passPayHistInd = value;
    }

    private CsePerson csePerson;
    private Standard payor;
    private CsePersonsWorkSet csePersonsWorkSet;
    private LegalAction legalAction;
    private Standard courtOrder;
    private CashReceiptDetail listStarting;
    private CashReceiptDetail listEnding;
    private Common totalItems;
    private Common totalPmt;
    private Common totalDist;
    private Common totalRef;
    private Common totalAdj;
    private Common totalUndist;
    private Standard undistCruc;
    private Array<ExportGroup> export1;
    private Common continue1;
    private CsePersonsWorkSet hiddenCsePersonsWorkSet;
    private LegalAction hiddenLegalAction;
    private NextTranInfo hiddenNextTranInfo;
    private Standard standard;
    private CashReceiptSourceType passCashReceiptSourceType;
    private CashReceiptEvent passCashReceiptEvent;
    private CashReceiptType passCashReceiptType;
    private CashReceipt passCashReceipt;
    private CashReceiptDetail passCashReceiptDetail;
    private Common phonetic;
    private Common passPayHistInd;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>A MaxDaysInMonthGroup group.</summary>
    [Serializable]
    public class MaxDaysInMonthGroup
    {
      /// <summary>
      /// A value of DetlMaxDaysInMth.
      /// </summary>
      [JsonPropertyName("detlMaxDaysInMth")]
      public Common DetlMaxDaysInMth
      {
        get => detlMaxDaysInMth ??= new();
        set => detlMaxDaysInMth = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 12;

      private Common detlMaxDaysInMth;
    }

    /// <summary>
    /// A value of InterfacePaymentHistoryReques.
    /// </summary>
    [JsonPropertyName("interfacePaymentHistoryReques")]
    public InterfacePaymentHistoryReques InterfacePaymentHistoryReques
    {
      get => interfacePaymentHistoryReques ??= new();
      set => interfacePaymentHistoryReques = value;
    }

    /// <summary>
    /// A value of Low.
    /// </summary>
    [JsonPropertyName("low")]
    public DateWorkArea Low
    {
      get => low ??= new();
      set => low = value;
    }

    /// <summary>
    /// A value of HardcodedCrtFdirPmt.
    /// </summary>
    [JsonPropertyName("hardcodedCrtFdirPmt")]
    public CashReceiptType HardcodedCrtFdirPmt
    {
      get => hardcodedCrtFdirPmt ??= new();
      set => hardcodedCrtFdirPmt = value;
    }

    /// <summary>
    /// A value of HardcodedCrtFcourtPmt.
    /// </summary>
    [JsonPropertyName("hardcodedCrtFcourtPmt")]
    public CashReceiptType HardcodedCrtFcourtPmt
    {
      get => hardcodedCrtFcourtPmt ??= new();
      set => hardcodedCrtFcourtPmt = value;
    }

    /// <summary>
    /// A value of New1.
    /// </summary>
    [JsonPropertyName("new1")]
    public CashReceiptDetailStatus New1
    {
      get => new1 ??= new();
      set => new1 = value;
    }

    /// <summary>
    /// A value of Pass.
    /// </summary>
    [JsonPropertyName("pass")]
    public CashReceiptDetail Pass
    {
      get => pass ??= new();
      set => pass = value;
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
    /// A value of Null1.
    /// </summary>
    [JsonPropertyName("null1")]
    public DateWorkArea Null1
    {
      get => null1 ??= new();
      set => null1 = value;
    }

    /// <summary>
    /// A value of Clear.
    /// </summary>
    [JsonPropertyName("clear")]
    public CsePersonsWorkSet Clear
    {
      get => clear ??= new();
      set => clear = value;
    }

    /// <summary>
    /// A value of PersObligorInLact.
    /// </summary>
    [JsonPropertyName("persObligorInLact")]
    public Common PersObligorInLact
    {
      get => persObligorInLact ??= new();
      set => persObligorInLact = value;
    }

    /// <summary>
    /// A value of Selected.
    /// </summary>
    [JsonPropertyName("selected")]
    public Common Selected
    {
      get => selected ??= new();
      set => selected = value;
    }

    /// <summary>
    /// A value of FrequencyDays.
    /// </summary>
    [JsonPropertyName("frequencyDays")]
    public Standard FrequencyDays
    {
      get => frequencyDays ??= new();
      set => frequencyDays = value;
    }

    /// <summary>
    /// A value of NumberOfMonths.
    /// </summary>
    [JsonPropertyName("numberOfMonths")]
    public Common NumberOfMonths
    {
      get => numberOfMonths ??= new();
      set => numberOfMonths = value;
    }

    /// <summary>
    /// A value of NextSmCollDateDd.
    /// </summary>
    [JsonPropertyName("nextSmCollDateDd")]
    public Common NextSmCollDateDd
    {
      get => nextSmCollDateDd ??= new();
      set => nextSmCollDateDd = value;
    }

    /// <summary>
    /// A value of StartingSmCollDateDd.
    /// </summary>
    [JsonPropertyName("startingSmCollDateDd")]
    public Common StartingSmCollDateDd
    {
      get => startingSmCollDateDd ??= new();
      set => startingSmCollDateDd = value;
    }

    /// <summary>
    /// Gets a value of MaxDaysInMonth.
    /// </summary>
    [JsonIgnore]
    public Array<MaxDaysInMonthGroup> MaxDaysInMonth => maxDaysInMonth ??= new(
      MaxDaysInMonthGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of MaxDaysInMonth for json serialization.
    /// </summary>
    [JsonPropertyName("maxDaysInMonth")]
    [Computed]
    public IList<MaxDaysInMonthGroup> MaxDaysInMonth_Json
    {
      get => maxDaysInMonth;
      set => MaxDaysInMonth.Assign(value);
    }

    private InterfacePaymentHistoryReques interfacePaymentHistoryReques;
    private DateWorkArea low;
    private CashReceiptType hardcodedCrtFdirPmt;
    private CashReceiptType hardcodedCrtFcourtPmt;
    private CashReceiptDetailStatus new1;
    private CashReceiptDetail pass;
    private DateWorkArea current;
    private DateWorkArea null1;
    private CsePersonsWorkSet clear;
    private Common persObligorInLact;
    private Common selected;
    private Standard frequencyDays;
    private Common numberOfMonths;
    private Common nextSmCollDateDd;
    private Common startingSmCollDateDd;
    private Array<MaxDaysInMonthGroup> maxDaysInMonth;
  }
#endregion
}
