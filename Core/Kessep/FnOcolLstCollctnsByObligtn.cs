// Program: FN_OCOL_LST_COLLCTNS_BY_OBLIGTN, ID: 371738589, model: 746.
// Short name: SWEOCOLP
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
/// A program: FN_OCOL_LST_COLLCTNS_BY_OBLIGTN.
/// </para>
/// <para>
/// To provide a comprehensive list of collections that have been applied to a 
/// specific obligation.
/// </para>
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class FnOcolLstCollctnsByObligtn: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_OCOL_LST_COLLCTNS_BY_OBLIGTN program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnOcolLstCollctnsByObligtn(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnOcolLstCollctnsByObligtn.
  /// </summary>
  public FnOcolLstCollctnsByObligtn(IContext context, Import import,
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
    // ***************************************************************************
    // CHANGE LOG
    // Date	  Developer		Ref #	Description
    // ---------------------------------------------------------------------------
    // 11/13/96  Holly Kennedy - MTW		Data level security added to the
    // 					procedure.
    // 12/03/96  R. Marchman			Fix security
    // 03/27/97 S.Mahapatra - MTW
    //                   - deleted PF19 to MCOL
    //                   - deleted Reference# column
    //                   - added column status
    //                   - added flow to CRRC (PF19)
    // 03/26/98	Siraj Konkader		ZDEL cleanup
    // 09/30/98  E. Parker			Added edits to CASE  COLA, COLL, and CRRC to check 
    // for selection; changed logic in fn_get_collections_by_obligation to sort
    // descending by collection date; changed return from crrc and cruc to
    // execute and set command retcrrc and retcruc; changed owed amounts to
    // display zero when zero; changed "Amt Due on Oblig" field to "Obligation
    // Amount"; changed logic to retrieve Obligation Amount using
    // fn_cab_get_accrual_or_due_amount; added Obligation Frequency to screen;
    // mapped Obligation Order Type Code to Interstate Ind field; added Accrual/
    // due date to screen; changed Obligation Amount to display zero when zero;
    // added exit state if user attempts to flow directly to this screen;
    // changed logic to keep details from being wiped out on next tran error.
    // 12/03/99 - E. Parker - PR #80974  Changed screen to use Cash Receipt 
    // Event Received Date instead of Cash Receipt Received Date.  Cleaned up
    // commented code.
    // ***************************************************************************
    ExitState = "ACO_NN0000_ALL_OK";

    // **** begin group A ****
    export.Hidden.Assign(import.Hidden);

    if (Equal(global.Command, "CLEAR"))
    {
      return;
    }

    export.AmtPrompt.Text1 = import.AmtPrompt.Text1;
    export.NextTransaction.Command = import.NextTransaction.Command;
    export.ObligationPrompt.Flag = import.ObligationPrompt.Flag;
    export.SearchCsePerson.Number = import.SearchCsePerson.Number;
    MoveLegalAction(import.SearchLegalAction, export.LegalAction);
    export.SearchCsePersonsWorkSet.FormattedName =
      import.SearchCsePersonsWorkSet.FormattedName;
    MoveObligation(import.SearchObligation, export.SearchObligation);
    export.ObligationTransaction.Assign(import.SearchObligationTransaction);
    export.ObligationType.Assign(import.SearchObligationType);
    export.SearchFrom.Date = import.SearchFrom.Date;
    export.SearchTo.Date = import.SearchTo.Date;
    MoveScreenOwedAmounts(import.ScreenOwedAmounts, export.ScreenOwedAmounts);
    export.TotalUndistAmt.TotalCurrency = import.TotalUndistAmt.TotalCurrency;
    export.TotalAmountDue.TotalCurrency = import.TotalAmountDue.TotalCurrency;
    export.ObligationPaymentSchedule.FrequencyCode =
      import.ObligationPaymentSchedule.FrequencyCode;
    export.AccrualOrDue.Date = import.AccrualOrDue.Date;

    // **** begin group B ****
    export.Standard.NextTransaction = import.Standard.NextTransaction;

    if (Equal(global.Command, "XXNEXTXX"))
    {
      UseScCabNextTranGet();

      if (!IsEmpty(export.Hidden.CsePersonNumberAp))
      {
        export.SearchCsePerson.Number = export.Hidden.CsePersonNumberAp ?? Spaces
          (10);
      }
      else if (!IsEmpty(export.Hidden.CsePersonNumber))
      {
        export.SearchCsePerson.Number = export.Hidden.CsePersonNumber ?? Spaces
          (10);
      }
      else if (!IsEmpty(export.Hidden.CsePersonNumberObligor))
      {
        export.SearchCsePerson.Number =
          export.Hidden.CsePersonNumberObligor ?? Spaces(10);
      }
      else if (!IsEmpty(export.Hidden.CsePersonNumberObligee))
      {
        export.SearchCsePerson.Number =
          export.Hidden.CsePersonNumberObligee ?? Spaces(10);
      }
      else
      {
        ExitState = "FN0000_INSUFF_NEXXTRAN_INFO";
      }

      global.Command = "DISPLAY";

      return;
    }

    if (Equal(global.Command, "XXFMMENU"))
    {
      global.Command = "DISPLAY";

      return;
    }

    // *****
    // to validate action level security
    // Logic to validate data level security has been added 11/13/96
    // *****
    if (!Equal(global.Command, "LIST") && !Equal(global.Command, "CRRC") && !
      Equal(global.Command, "RETCRRC") && !Equal(global.Command, "RETCRUC"))
    {
      UseScCabTestSecurity();

      if (IsExitState("ACO_NN0000_ALL_OK"))
      {
      }
      else
      {
        return;
      }
    }

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
      export.Export1.Update.CashReceipt.Assign(import.Import1.Item.CashReceipt);
      export.Export1.Update.CashReceiptDetail.Assign(
        import.Import1.Item.CashReceiptDetail);
      MoveCashReceiptSourceType(import.Import1.Item.CashReceiptSourceType,
        export.Export1.Update.CashReceiptSourceType);
      export.Export1.Update.DistToOblig.TotalCurrency =
        import.Import1.Item.DistToOblig.TotalCurrency;
      MoveCashReceiptEvent(import.Import1.Item.DetailCashReceiptEvent,
        export.Export1.Update.DetailCashReceiptEvent);
      MoveCashReceiptType(import.Import1.Item.DetailCashReceiptType,
        export.Export1.Update.DetailCashReceiptType);
      export.Export1.Update.Status.Code = import.Import1.Item.Status.Code;

      switch(AsChar(import.Import1.Item.Common.SelectChar))
      {
        case ' ':
          break;
        case 'S':
          ++local.GroupViewCounter.Count;

          // Move fields to export views.
          export.SelectedCashReceipt.Assign(export.Export1.Item.CashReceipt);
          export.SelectedCashReceiptDetail.Assign(
            export.Export1.Item.CashReceiptDetail);
          MoveCashReceiptSourceType(export.Export1.Item.CashReceiptSourceType,
            export.SelectedCashReceiptSourceType);
          MoveCashReceiptEvent(export.Export1.Item.DetailCashReceiptEvent,
            export.SelectedCashReceiptEvent);
          MoveCashReceiptType(export.Export1.Item.DetailCashReceiptType,
            export.SelectedCashReceiptType);

          if (local.GroupViewCounter.Count > 1)
          {
            var field1 = GetField(export.Export1.Item.Common, "selectChar");

            field1.Error = true;

            ExitState = "OE0000_MORE_THAN_ONE_RECORD_SELD";
          }

          break;
        default:
          var field = GetField(export.Export1.Item.Common, "selectChar");

          field.Color = "red";
          field.Intensity = Intensity.High;
          field.Protected = false;
          field.Focused = true;

          ExitState = "ACO_NE0000_INVALID_SELECT_CODE";

          break;
      }

      export.Export1.Next();
    }

    // if the next tran info is not equal to spaces, this implies the user 
    // requested a next tran action. now validate
    if (IsEmpty(import.Standard.NextTransaction))
    {
    }
    else
    {
      export.Hidden.CsePersonNumberAp = export.SearchCsePerson.Number;
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

    // *** Display screen as it was before flow to CRRC and CRUC. ***
    if (Equal(global.Command, "RETCRRC") || Equal(global.Command, "RETCRUC"))
    {
      return;
    }

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    switch(TrimEnd(global.Command))
    {
      case "CRRC":
        if (local.GroupViewCounter.Count < 1)
        {
          ExitState = "ACO_NE0000_NO_SELECTION_MADE";

          return;
        }

        for(export.Export1.Index = 0; export.Export1.Index < export
          .Export1.Count; ++export.Export1.Index)
        {
          if (!IsEmpty(export.Export1.Item.Common.SelectChar))
          {
            export.Export1.Update.Common.SelectChar = "";
          }
        }

        ExitState = "ECO_LNK_TO_CRRC_REC_COLL_DTL";

        break;
      case "COLA":
        if (local.GroupViewCounter.Count < 1)
        {
          ExitState = "ACO_NE0000_NO_SELECTION_MADE";

          return;
        }

        for(export.Export1.Index = 0; export.Export1.Index < export
          .Export1.Count; ++export.Export1.Index)
        {
          if (!IsEmpty(export.Export1.Item.Common.SelectChar))
          {
            export.Export1.Update.Common.SelectChar = "";
          }
        }

        ExitState = "ECO_LNK_TO_REC_COLL_ADJMNT";

        break;
      case "COLL":
        if (local.GroupViewCounter.Count < 1)
        {
          ExitState = "ACO_NE0000_NO_SELECTION_MADE";

          return;
        }

        for(export.Export1.Index = 0; export.Export1.Index < export
          .Export1.Count; ++export.Export1.Index)
        {
          if (!IsEmpty(export.Export1.Item.Common.SelectChar))
          {
            export.Export1.Update.Common.SelectChar = "";
          }
        }

        ExitState = "ECO_LNK_TO_LST_COL_ACT_BY_AP_PYR";

        break;
      case "PAYR":
        for(export.Export1.Index = 0; export.Export1.Index < export
          .Export1.Count; ++export.Export1.Index)
        {
          if (!IsEmpty(export.Export1.Item.Common.SelectChar))
          {
            export.Export1.Update.Common.SelectChar = "";
          }
        }

        ExitState = "ECO_LNK_TO_LST_COLL_BY_AP_PYR";

        break;
      case "DISPLAY":
        // The obligation must be passed to this procedure.
        if (IsEmpty(export.SearchCsePerson.Number) || export
          .SearchObligation.SystemGeneratedIdentifier == 0)
        {
          ExitState = "FN0000_INSUFF_NEXXTRAN_INFO";

          return;
        }

        UseFnGetCollectionsByObligation();

        if (!IsEmpty(export.ScreenOwedAmounts.ErrorInformationLine))
        {
          export.MthOblSummaryNfMsg.Msg40 =
            export.ScreenOwedAmounts.ErrorInformationLine;

          var field = GetField(export.MthOblSummaryNfMsg, "msg40");

          field.Color = "red";
          field.Intensity = Intensity.High;
          field.Highlighting = Highlighting.ReverseVideo;
          field.Protected = true;
        }

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          if (!IsEmpty(export.MthOblSummaryNfMsg.Msg40))
          {
            var field1 =
              GetField(export.ScreenOwedAmounts, "arrearsAmountOwed");

            field1.Intensity = Intensity.Dark;
            field1.Protected = true;

            var field2 =
              GetField(export.ScreenOwedAmounts, "currentAmountOwed");

            field2.Intensity = Intensity.Dark;
            field2.Protected = true;

            var field3 =
              GetField(export.ScreenOwedAmounts, "interestAmountOwed");

            field3.Intensity = Intensity.Dark;
            field3.Protected = true;

            var field4 = GetField(export.ScreenOwedAmounts, "totalAmountOwed");

            field4.Intensity = Intensity.Dark;
            field4.Protected = true;

            var field5 = GetField(export.MthOblSummaryNfMsg, "msg40");

            field5.Color = "red";
            field5.Protected = true;
          }

          if (IsExitState("FROM_DATE_GREATER_THAN_TO_DATE"))
          {
            var field = GetField(export.SearchFrom, "date");

            field.Error = true;
          }
          else if (IsExitState("TO_DATE_GREATER_CURRENT_DATE"))
          {
            var field = GetField(export.SearchTo, "date");

            field.Error = true;
          }
          else if (IsExitState("CSE_PERSON_NF"))
          {
            var field = GetField(export.SearchCsePerson, "number");

            field.Error = true;
          }
          else if (IsExitState("LCOB_OBLIGATION_NF"))
          {
          }
          else
          {
          }

          for(export.Export1.Index = 0; export.Export1.Index < export
            .Export1.Count; ++export.Export1.Index)
          {
            if (IsEmpty(export.Export1.Item.Status.Code) && (
              !IsEmpty(export.Export1.Item.CashReceiptSourceType.Code) || export
              .Export1.Item.CashReceiptDetail.SequentialIdentifier > 0 || export
              .Export1.Item.CashReceiptDetail.CollectionAmount > 0))
            {
              var field = GetField(export.Export1.Item.Status, "code");

              field.Error = true;

              ExitState = "FN0000_CASH_RCPT_DTL_STATUS_NF";
            }
          }

          return;
        }

        if (export.Export1.IsEmpty)
        {
          ExitState = "ACO_NI0000_NO_INFO_FOR_LIST";

          return;
        }

        if (export.Export1.IsFull)
        {
          ExitState = "ACO_NI0000_LST_RETURNED_FULL";
        }

        break;
      case "RETURN":
        ExitState = "ACO_NE0000_RETURN";

        break;
      case "LIST":
        switch(AsChar(import.AmtPrompt.Text1))
        {
          case 'S':
            export.AmtPrompt.Text1 = "";
            ExitState = "ECO_LNK_TO_LST_CRUC_UNDSTRBTD_CL";

            break;
          case ' ':
            ExitState = "ACO_NE0000_INVALID_PROMPT_VALUE";

            break;
          case '+':
            ExitState = "ACO_NE0000_INVALID_PROMPT_VALUE";

            break;
          default:
            var field = GetField(export.AmtPrompt, "text1");

            field.Error = true;

            ExitState = "ACO_NE0000_INVALID_SELECT_CODE";

            break;
        }

        break;
      case "PREV":
        ExitState = "ACO_NI0000_TOP_OF_LIST";

        break;
      case "NEXT":
        ExitState = "ACO_NI0000_BOTTOM_OF_LIST";

        break;
      case "SIGNOFF":
        UseScCabSignoff();

        break;
      default:
        ExitState = "ACO_NE0000_INVALID_COMMAND";

        break;
    }
  }

  private static void MoveCashReceiptEvent(CashReceiptEvent source,
    CashReceiptEvent target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.ReceivedDate = source.ReceivedDate;
  }

  private static void MoveCashReceiptSourceType(CashReceiptSourceType source,
    CashReceiptSourceType target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Code = source.Code;
  }

  private static void MoveCashReceiptType(CashReceiptType source,
    CashReceiptType target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Code = source.Code;
  }

  private static void MoveExport1(FnGetCollectionsByObligation.Export.
    ExportGroup source, Export.ExportGroup target)
  {
    target.Status.Code = source.Status.Code;
    MoveCashReceiptEvent(source.DetailCashReceiptEvent,
      target.DetailCashReceiptEvent);
    MoveCashReceiptType(source.DetailCashReceiptType,
      target.DetailCashReceiptType);
    target.Common.SelectChar = source.Common.SelectChar;
    MoveCashReceiptSourceType(source.CashReceiptSourceType,
      target.CashReceiptSourceType);
    target.CashReceipt.Assign(source.CashReceipt);
    target.CashReceiptDetail.Assign(source.CashReceiptDetail);
    target.DistToOblig.TotalCurrency = source.DistToOblig.TotalCurrency;
  }

  private static void MoveLegalAction(LegalAction source, LegalAction target)
  {
    target.CourtCaseNumber = source.CourtCaseNumber;
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

  private static void MoveObligation(Obligation source, Obligation target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.OrderTypeCode = source.OrderTypeCode;
  }

  private static void MoveScreenOwedAmounts(ScreenOwedAmounts source,
    ScreenOwedAmounts target)
  {
    target.CurrentAmountOwed = source.CurrentAmountOwed;
    target.ArrearsAmountOwed = source.ArrearsAmountOwed;
    target.InterestAmountOwed = source.InterestAmountOwed;
    target.TotalAmountOwed = source.TotalAmountOwed;
  }

  private void UseFnGetCollectionsByObligation()
  {
    var useImport = new FnGetCollectionsByObligation.Import();
    var useExport = new FnGetCollectionsByObligation.Export();

    useImport.SearchCsePerson.Number = export.SearchCsePerson.Number;
    useImport.SearchObligation.SystemGeneratedIdentifier =
      export.SearchObligation.SystemGeneratedIdentifier;
    useImport.SearchFrom.Date = export.SearchFrom.Date;
    useImport.SearchTo.Date = export.SearchTo.Date;

    Call(FnGetCollectionsByObligation.Execute, useImport, useExport);

    export.AccrualOrDue.Date = useExport.AccrualOrDue.Date;
    export.ObligationPaymentSchedule.FrequencyCode =
      useExport.ObligationPaymentSchedule.FrequencyCode;
    useExport.Export1.CopyTo(export.Export1, MoveExport1);
    export.ScreenOwedAmounts.Assign(useExport.ScreenOwedAmounts);
    export.TotalAmountDue.TotalCurrency =
      useExport.TotalAmountDue.TotalCurrency;
    MoveLegalAction(useExport.LegalAction, export.LegalAction);
    export.SearchObligation.OrderTypeCode = useExport.Obligation.OrderTypeCode;
    export.ObligationType.Assign(useExport.ObligationType);
    export.SearchFrom.Date = useExport.SearchFrom.Date;
    export.SearchTo.Date = useExport.SearchTo.Date;
    export.TotalUndistAmt.TotalCurrency =
      useExport.TotalUndistAmt.TotalCurrency;
    export.SearchCsePersonsWorkSet.FormattedName =
      useExport.Search.FormattedName;
    export.MthOblSummaryNfMsg.Msg40 = useExport.MthOblSummaryNfMsg.Msg40;
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

    MoveLegalAction(export.LegalAction, useImport.LegalAction);
    useImport.CsePerson.Number = export.SearchCsePerson.Number;

    Call(ScCabTestSecurity.Execute, useImport, useExport);
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
      /// A value of Status.
      /// </summary>
      [JsonPropertyName("status")]
      public CashReceiptDetailStatus Status
      {
        get => status ??= new();
        set => status = value;
      }

      /// <summary>
      /// A value of DetailCashReceiptEvent.
      /// </summary>
      [JsonPropertyName("detailCashReceiptEvent")]
      public CashReceiptEvent DetailCashReceiptEvent
      {
        get => detailCashReceiptEvent ??= new();
        set => detailCashReceiptEvent = value;
      }

      /// <summary>
      /// A value of DetailCashReceiptType.
      /// </summary>
      [JsonPropertyName("detailCashReceiptType")]
      public CashReceiptType DetailCashReceiptType
      {
        get => detailCashReceiptType ??= new();
        set => detailCashReceiptType = value;
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
      /// A value of CashReceiptSourceType.
      /// </summary>
      [JsonPropertyName("cashReceiptSourceType")]
      public CashReceiptSourceType CashReceiptSourceType
      {
        get => cashReceiptSourceType ??= new();
        set => cashReceiptSourceType = value;
      }

      /// <summary>
      /// A value of CashReceipt.
      /// </summary>
      [JsonPropertyName("cashReceipt")]
      public CashReceipt CashReceipt
      {
        get => cashReceipt ??= new();
        set => cashReceipt = value;
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
      /// A value of DistToOblig.
      /// </summary>
      [JsonPropertyName("distToOblig")]
      public Common DistToOblig
      {
        get => distToOblig ??= new();
        set => distToOblig = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 90;

      private CashReceiptDetailStatus status;
      private CashReceiptEvent detailCashReceiptEvent;
      private CashReceiptType detailCashReceiptType;
      private Common common;
      private CashReceiptSourceType cashReceiptSourceType;
      private CashReceipt cashReceipt;
      private CashReceiptDetail cashReceiptDetail;
      private Common distToOblig;
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
    /// A value of ObligationPaymentSchedule.
    /// </summary>
    [JsonPropertyName("obligationPaymentSchedule")]
    public ObligationPaymentSchedule ObligationPaymentSchedule
    {
      get => obligationPaymentSchedule ??= new();
      set => obligationPaymentSchedule = value;
    }

    /// <summary>
    /// A value of AmtPrompt.
    /// </summary>
    [JsonPropertyName("amtPrompt")]
    public TextWorkArea AmtPrompt
    {
      get => amtPrompt ??= new();
      set => amtPrompt = value;
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
    /// A value of SearchCsePerson.
    /// </summary>
    [JsonPropertyName("searchCsePerson")]
    public CsePerson SearchCsePerson
    {
      get => searchCsePerson ??= new();
      set => searchCsePerson = value;
    }

    /// <summary>
    /// A value of SearchObligation.
    /// </summary>
    [JsonPropertyName("searchObligation")]
    public Obligation SearchObligation
    {
      get => searchObligation ??= new();
      set => searchObligation = value;
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
    /// A value of ScreenOwedAmounts.
    /// </summary>
    [JsonPropertyName("screenOwedAmounts")]
    public ScreenOwedAmounts ScreenOwedAmounts
    {
      get => screenOwedAmounts ??= new();
      set => screenOwedAmounts = value;
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
    /// A value of SearchLegalAction.
    /// </summary>
    [JsonPropertyName("searchLegalAction")]
    public LegalAction SearchLegalAction
    {
      get => searchLegalAction ??= new();
      set => searchLegalAction = value;
    }

    /// <summary>
    /// A value of SearchObligationTransaction.
    /// </summary>
    [JsonPropertyName("searchObligationTransaction")]
    public ObligationTransaction SearchObligationTransaction
    {
      get => searchObligationTransaction ??= new();
      set => searchObligationTransaction = value;
    }

    /// <summary>
    /// A value of SearchObligationType.
    /// </summary>
    [JsonPropertyName("searchObligationType")]
    public ObligationType SearchObligationType
    {
      get => searchObligationType ??= new();
      set => searchObligationType = value;
    }

    /// <summary>
    /// A value of SearchCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("searchCsePersonsWorkSet")]
    public CsePersonsWorkSet SearchCsePersonsWorkSet
    {
      get => searchCsePersonsWorkSet ??= new();
      set => searchCsePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of SearchFrom.
    /// </summary>
    [JsonPropertyName("searchFrom")]
    public DateWorkArea SearchFrom
    {
      get => searchFrom ??= new();
      set => searchFrom = value;
    }

    /// <summary>
    /// A value of SearchTo.
    /// </summary>
    [JsonPropertyName("searchTo")]
    public DateWorkArea SearchTo
    {
      get => searchTo ??= new();
      set => searchTo = value;
    }

    /// <summary>
    /// A value of TotalUndistAmt.
    /// </summary>
    [JsonPropertyName("totalUndistAmt")]
    public Common TotalUndistAmt
    {
      get => totalUndistAmt ??= new();
      set => totalUndistAmt = value;
    }

    /// <summary>
    /// A value of NextTransaction.
    /// </summary>
    [JsonPropertyName("nextTransaction")]
    public Common NextTransaction
    {
      get => nextTransaction ??= new();
      set => nextTransaction = value;
    }

    /// <summary>
    /// A value of ObligationPrompt.
    /// </summary>
    [JsonPropertyName("obligationPrompt")]
    public Common ObligationPrompt
    {
      get => obligationPrompt ??= new();
      set => obligationPrompt = value;
    }

    /// <summary>
    /// A value of MthOblSummaryNfMsg.
    /// </summary>
    [JsonPropertyName("mthOblSummaryNfMsg")]
    public ErrorMessageText MthOblSummaryNfMsg
    {
      get => mthOblSummaryNfMsg ??= new();
      set => mthOblSummaryNfMsg = value;
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

    private DateWorkArea accrualOrDue;
    private ObligationPaymentSchedule obligationPaymentSchedule;
    private TextWorkArea amtPrompt;
    private Collection collection;
    private CsePerson searchCsePerson;
    private Obligation searchObligation;
    private Array<ImportGroup> import1;
    private ScreenOwedAmounts screenOwedAmounts;
    private Common totalAmountDue;
    private LegalAction searchLegalAction;
    private ObligationTransaction searchObligationTransaction;
    private ObligationType searchObligationType;
    private CsePersonsWorkSet searchCsePersonsWorkSet;
    private DateWorkArea searchFrom;
    private DateWorkArea searchTo;
    private Common totalUndistAmt;
    private Common nextTransaction;
    private Common obligationPrompt;
    private ErrorMessageText mthOblSummaryNfMsg;
    private NextTranInfo hidden;
    private Standard standard;
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
      /// A value of Status.
      /// </summary>
      [JsonPropertyName("status")]
      public CashReceiptDetailStatus Status
      {
        get => status ??= new();
        set => status = value;
      }

      /// <summary>
      /// A value of DetailCashReceiptEvent.
      /// </summary>
      [JsonPropertyName("detailCashReceiptEvent")]
      public CashReceiptEvent DetailCashReceiptEvent
      {
        get => detailCashReceiptEvent ??= new();
        set => detailCashReceiptEvent = value;
      }

      /// <summary>
      /// A value of DetailCashReceiptType.
      /// </summary>
      [JsonPropertyName("detailCashReceiptType")]
      public CashReceiptType DetailCashReceiptType
      {
        get => detailCashReceiptType ??= new();
        set => detailCashReceiptType = value;
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
      /// A value of CashReceiptSourceType.
      /// </summary>
      [JsonPropertyName("cashReceiptSourceType")]
      public CashReceiptSourceType CashReceiptSourceType
      {
        get => cashReceiptSourceType ??= new();
        set => cashReceiptSourceType = value;
      }

      /// <summary>
      /// A value of CashReceipt.
      /// </summary>
      [JsonPropertyName("cashReceipt")]
      public CashReceipt CashReceipt
      {
        get => cashReceipt ??= new();
        set => cashReceipt = value;
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
      /// A value of DistToOblig.
      /// </summary>
      [JsonPropertyName("distToOblig")]
      public Common DistToOblig
      {
        get => distToOblig ??= new();
        set => distToOblig = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 90;

      private CashReceiptDetailStatus status;
      private CashReceiptEvent detailCashReceiptEvent;
      private CashReceiptType detailCashReceiptType;
      private Common common;
      private CashReceiptSourceType cashReceiptSourceType;
      private CashReceipt cashReceipt;
      private CashReceiptDetail cashReceiptDetail;
      private Common distToOblig;
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
    /// A value of ObligationPaymentSchedule.
    /// </summary>
    [JsonPropertyName("obligationPaymentSchedule")]
    public ObligationPaymentSchedule ObligationPaymentSchedule
    {
      get => obligationPaymentSchedule ??= new();
      set => obligationPaymentSchedule = value;
    }

    /// <summary>
    /// A value of AmtPrompt.
    /// </summary>
    [JsonPropertyName("amtPrompt")]
    public TextWorkArea AmtPrompt
    {
      get => amtPrompt ??= new();
      set => amtPrompt = value;
    }

    /// <summary>
    /// A value of SelectedCashReceiptEvent.
    /// </summary>
    [JsonPropertyName("selectedCashReceiptEvent")]
    public CashReceiptEvent SelectedCashReceiptEvent
    {
      get => selectedCashReceiptEvent ??= new();
      set => selectedCashReceiptEvent = value;
    }

    /// <summary>
    /// A value of SelectedCashReceiptType.
    /// </summary>
    [JsonPropertyName("selectedCashReceiptType")]
    public CashReceiptType SelectedCashReceiptType
    {
      get => selectedCashReceiptType ??= new();
      set => selectedCashReceiptType = value;
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
    /// A value of ScreenOwedAmounts.
    /// </summary>
    [JsonPropertyName("screenOwedAmounts")]
    public ScreenOwedAmounts ScreenOwedAmounts
    {
      get => screenOwedAmounts ??= new();
      set => screenOwedAmounts = value;
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
    /// A value of SelectedCashReceiptSourceType.
    /// </summary>
    [JsonPropertyName("selectedCashReceiptSourceType")]
    public CashReceiptSourceType SelectedCashReceiptSourceType
    {
      get => selectedCashReceiptSourceType ??= new();
      set => selectedCashReceiptSourceType = value;
    }

    /// <summary>
    /// A value of SelectedCashReceipt.
    /// </summary>
    [JsonPropertyName("selectedCashReceipt")]
    public CashReceipt SelectedCashReceipt
    {
      get => selectedCashReceipt ??= new();
      set => selectedCashReceipt = value;
    }

    /// <summary>
    /// A value of SelectedCashReceiptDetail.
    /// </summary>
    [JsonPropertyName("selectedCashReceiptDetail")]
    public CashReceiptDetail SelectedCashReceiptDetail
    {
      get => selectedCashReceiptDetail ??= new();
      set => selectedCashReceiptDetail = value;
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
    /// A value of SearchCsePerson.
    /// </summary>
    [JsonPropertyName("searchCsePerson")]
    public CsePerson SearchCsePerson
    {
      get => searchCsePerson ??= new();
      set => searchCsePerson = value;
    }

    /// <summary>
    /// A value of SearchObligation.
    /// </summary>
    [JsonPropertyName("searchObligation")]
    public Obligation SearchObligation
    {
      get => searchObligation ??= new();
      set => searchObligation = value;
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
    /// A value of SearchFrom.
    /// </summary>
    [JsonPropertyName("searchFrom")]
    public DateWorkArea SearchFrom
    {
      get => searchFrom ??= new();
      set => searchFrom = value;
    }

    /// <summary>
    /// A value of SearchTo.
    /// </summary>
    [JsonPropertyName("searchTo")]
    public DateWorkArea SearchTo
    {
      get => searchTo ??= new();
      set => searchTo = value;
    }

    /// <summary>
    /// A value of TotalUndistAmt.
    /// </summary>
    [JsonPropertyName("totalUndistAmt")]
    public Common TotalUndistAmt
    {
      get => totalUndistAmt ??= new();
      set => totalUndistAmt = value;
    }

    /// <summary>
    /// A value of SearchCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("searchCsePersonsWorkSet")]
    public CsePersonsWorkSet SearchCsePersonsWorkSet
    {
      get => searchCsePersonsWorkSet ??= new();
      set => searchCsePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of NextTransaction.
    /// </summary>
    [JsonPropertyName("nextTransaction")]
    public Common NextTransaction
    {
      get => nextTransaction ??= new();
      set => nextTransaction = value;
    }

    /// <summary>
    /// A value of ObligationPrompt.
    /// </summary>
    [JsonPropertyName("obligationPrompt")]
    public Common ObligationPrompt
    {
      get => obligationPrompt ??= new();
      set => obligationPrompt = value;
    }

    /// <summary>
    /// A value of MthOblSummaryNfMsg.
    /// </summary>
    [JsonPropertyName("mthOblSummaryNfMsg")]
    public ErrorMessageText MthOblSummaryNfMsg
    {
      get => mthOblSummaryNfMsg ??= new();
      set => mthOblSummaryNfMsg = value;
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

    private DateWorkArea accrualOrDue;
    private ObligationPaymentSchedule obligationPaymentSchedule;
    private TextWorkArea amtPrompt;
    private CashReceiptEvent selectedCashReceiptEvent;
    private CashReceiptType selectedCashReceiptType;
    private Collection collection;
    private Array<ExportGroup> export1;
    private ScreenOwedAmounts screenOwedAmounts;
    private Common totalAmountDue;
    private CashReceiptSourceType selectedCashReceiptSourceType;
    private CashReceipt selectedCashReceipt;
    private CashReceiptDetail selectedCashReceiptDetail;
    private LegalAction legalAction;
    private CsePerson searchCsePerson;
    private Obligation searchObligation;
    private ObligationTransaction obligationTransaction;
    private ObligationType obligationType;
    private DateWorkArea searchFrom;
    private DateWorkArea searchTo;
    private Common totalUndistAmt;
    private CsePersonsWorkSet searchCsePersonsWorkSet;
    private Common nextTransaction;
    private Common obligationPrompt;
    private ErrorMessageText mthOblSummaryNfMsg;
    private NextTranInfo hidden;
    private Standard standard;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of GroupViewCounter.
    /// </summary>
    [JsonPropertyName("groupViewCounter")]
    public Common GroupViewCounter
    {
      get => groupViewCounter ??= new();
      set => groupViewCounter = value;
    }

    private Common groupViewCounter;
  }
#endregion
}
