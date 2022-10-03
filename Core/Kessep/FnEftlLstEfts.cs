// Program: FN_EFTL_LST_EFTS, ID: 372156897, model: 746.
// Short name: SWEEFTLP
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
/// A program: FN_EFTL_LST_EFTS.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class FnEftlLstEfts: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_EFTL_LST_EFTS program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnEftlLstEfts(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnEftlLstEfts.
  /// </summary>
  public FnEftlLstEfts(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // --------------------------------------------
    // Every initial development and change to that
    // development needs to be documented.
    // --------------------------------------------
    // ---------------------------------------------------------------------------------------
    // Date		Developer Name		Request #	Description
    // 02/27/96         RBM
    // 12/10/96	R. Marchman	Add new security and next tran
    // 04/30/97	G P Kim		Change Current Date
    // 07/01/1997	A Samuels	Completed development
    // 05/10/99        Fangman   Changed process to run by EFT # rather than 
    // Payment Request #.  Made other smaller changes.
    // 07/01/1999   Fangman   Added code to display the name of the state in the
    // Issued To field when the EFT is for an interstate payment.
    // 07/14/99   Fangman   Changed code to display the Designated Payee's name 
    // instead of the Payee's name when a Designated Payee exists.
    // 07/15/99   Fangman   Changed code to display the Designated Payee's CSE 
    // number instead of the Payee's CSE number when a Designated Payee exists.
    // -------------------------------------------------------------------------------------
    if (Equal(global.Command, "CLEAR"))
    {
      ExitState = "ACO_NI0000_CLEAR_SUCCESSFUL";

      return;
    }

    ExitState = "ACO_NN0000_ALL_OK";
    local.Current.Date = Now().Date;

    // *****
    // Move imports to exports
    // *****
    export.Hidden.Assign(import.Hidden);
    export.Standard.NextTransaction = import.Standard.NextTransaction;
    MoveCsePersonsWorkSet(import.CsePersonsWorkSet, export.CsePersonsWorkSet);
    export.Search.TransmissionIdentifier = import.Search.TransmissionIdentifier;
    export.PaymentRequest.SystemGeneratedIdentifier =
      import.PaymentRequest.SystemGeneratedIdentifier;
    export.PaymentStatus.Code = import.PaymentStatus.Code;
    export.CsePerson.PromptField = import.CsePerson.PromptField;
    export.From.Date = import.From.Date;
    export.Status.PromptField = import.Status.PromptField;
    export.To.Date = import.To.Date;

    if (!Equal(global.Command, "DISPLAY"))
    {
      local.Common.Count = 0;

      export.EftPayments.Index = 0;
      export.EftPayments.Clear();

      for(import.EftPayments.Index = 0; import.EftPayments.Index < import
        .EftPayments.Count; ++import.EftPayments.Index)
      {
        if (export.EftPayments.IsFull)
        {
          break;
        }

        export.EftPayments.Update.DetailCommon.SelectChar =
          import.EftPayments.Item.DetailCommon.SelectChar;
        MoveElectronicFundTransmission(import.EftPayments.Item.
          DetailElectronicFundTransmission,
          export.EftPayments.Update.DetailElectronicFundTransmission);
        MoveCsePersonsWorkSet(import.EftPayments.Item.DetailCsePersonsWorkSet,
          export.EftPayments.Update.DetailCsePersonsWorkSet);
        export.EftPayments.Update.DetailPaymentRequest.Assign(
          import.EftPayments.Item.DetailPaymentRequest);
        export.EftPayments.Update.DetailPaymentStatus.Code =
          import.EftPayments.Item.DetailPaymentStatus.Code;

        // *****
        // Determine if a selection has been made
        // *****
        switch(AsChar(export.EftPayments.Item.DetailCommon.SelectChar))
        {
          case ' ':
            break;
          case 'S':
            ++local.Common.Count;
            MoveCsePersonsWorkSet(export.EftPayments.Item.
              DetailCsePersonsWorkSet, export.PassCsePersonsWorkSet);
            export.PassElectronicFundTransmission.TransmissionIdentifier =
              export.EftPayments.Item.DetailElectronicFundTransmission.
                TransmissionIdentifier;
            local.PaymentStatus.Code =
              export.EftPayments.Item.DetailPaymentStatus.Code;
            export.PassPaymentRequest.SystemGeneratedIdentifier =
              export.EftPayments.Item.DetailPaymentRequest.
                SystemGeneratedIdentifier;
            export.PassThruFlow.Number =
              export.EftPayments.Item.DetailCsePersonsWorkSet.Number;

            break;
          default:
            ++local.Common.Count;
            ExitState = "ACO_NE0000_INVALID_SELECT_CODE";

            var field =
              GetField(export.EftPayments.Item.DetailCommon, "selectChar");

            field.Error = true;

            break;
        }

        export.EftPayments.Next();
      }
    }

    if (Equal(global.Command, "RTFRMLNK"))
    {
      return;
    }

    if (local.Common.Count > 1)
    {
      ExitState = "ACO_NE0000_INVALID_MULTIPLE_SEL";

      return;
    }

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    // *** If the control is returned from a selection list - cse_person list or
    // Payment_status list, then Display the screen
    // ***
    if (Equal(global.Command, "CSEPERSO") || Equal(global.Command, "RETPST"))
    {
      export.CsePerson.PromptField = "";
      export.Status.PromptField = "";

      return;
    }

    if (!IsEmpty(export.CsePersonsWorkSet.Number))
    {
      local.LeftPadding.Text10 = export.CsePersonsWorkSet.Number;
      UseEabPadLeftWithZeros();
      export.CsePersonsWorkSet.Number = local.LeftPadding.Text10;
    }

    // *****
    // Next Tran/Security logic
    // *****
    if (!IsEmpty(import.Standard.NextTransaction))
    {
      export.Hidden.CsePersonNumber = export.CsePersonsWorkSet.Number;
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
      export.CsePersonsWorkSet.Number = export.Hidden.CsePersonNumber ?? Spaces
        (10);

      return;
    }

    if (Equal(global.Command, "XXFMMENU"))
    {
      return;
    }

    // to validate action level security
    if (Equal(global.Command, "PACC") || Equal(global.Command, "EDTL") || Equal
      (global.Command, "PPMT") || Equal(global.Command, "WDTL"))
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

    switch(TrimEnd(global.Command))
    {
      case "DISPLAY":
        if (!IsEmpty(export.CsePersonsWorkSet.Number))
        {
          UseSiReadCsePerson();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            var field = GetField(export.CsePersonsWorkSet, "number");

            field.Error = true;

            return;
          }
        }
        else
        {
          export.CsePersonsWorkSet.FormattedName = "";
        }

        if (!IsEmpty(export.PaymentStatus.Code))
        {
          // *****
          // Payment Status entered
          // *****
          if (ReadPaymentStatus())
          {
            // ok. continue processing
          }
          else
          {
            var field = GetField(export.PaymentStatus, "code");

            field.Error = true;

            ExitState = "FN0000_PYMNT_STAT_NF";

            return;
          }
        }

        // *****
        // If the To date is entered set the from date to the current date and 
        // vice versa
        // *****
        if (Equal(export.From.Date, local.Null1.Date) && Equal
          (export.To.Date, local.Null1.Date))
        {
          export.To.Date = local.Current.Date;
          export.From.Date = Now().Date.AddMonths(-1);
        }
        else if (Lt(local.Null1.Date, export.From.Date) && Equal
          (export.To.Date, local.Null1.Date))
        {
          export.To.Date = local.Current.Date;
        }

        // *****
        // Validate that the From date is not greater than the To date.
        // *****
        if (Lt(export.To.Date, export.From.Date))
        {
          var field1 = GetField(export.From, "date");

          field1.Error = true;

          var field2 = GetField(export.To, "date");

          field2.Error = true;

          ExitState = "ACO_NE0000_DATE_RANGE_ERROR";

          return;
        }

        local.Maximum.Date = UseCabSetMaximumDiscontinueDate();

        if (!IsEmpty(export.CsePersonsWorkSet.Number) && !
          IsEmpty(export.PaymentStatus.Code))
        {
          // 1.  Read by person and status
          export.EftPayments.Index = 0;
          export.EftPayments.Clear();

          foreach(var item in ReadElectronicFundTransmissionPaymentRequestPaymentStatusHistory1())
            
          {
            MoveElectronicFundTransmission(entities.ElectronicFundTransmission,
              export.EftPayments.Update.DetailElectronicFundTransmission);
            export.EftPayments.Update.DetailPaymentRequest.Assign(
              entities.PaymentRequest);

            if (!IsEmpty(entities.PaymentRequest.DesignatedPayeeCsePersonNo))
            {
              export.EftPayments.Update.DetailCsePersonsWorkSet.Number =
                entities.PaymentRequest.DesignatedPayeeCsePersonNo ?? Spaces
                (10);
            }
            else
            {
              export.EftPayments.Update.DetailCsePersonsWorkSet.Number =
                entities.PaymentRequest.CsePersonNumber ?? Spaces(10);
            }

            export.EftPayments.Update.DetailCsePersonsWorkSet.FormattedName =
              entities.ElectronicFundTransmission.ReceivingCompanyName ?? Spaces
              (33);
            export.EftPayments.Update.DetailPaymentStatus.Code =
              export.PaymentStatus.Code;
            export.EftPayments.Next();
          }
        }
        else if (!IsEmpty(export.CsePersonsWorkSet.Number))
        {
          // 2.  Read by person
          export.EftPayments.Index = 0;
          export.EftPayments.Clear();

          foreach(var item in ReadElectronicFundTransmissionPaymentRequestPaymentStatusHistory3())
            
          {
            MoveElectronicFundTransmission(entities.ElectronicFundTransmission,
              export.EftPayments.Update.DetailElectronicFundTransmission);
            export.EftPayments.Update.DetailPaymentRequest.Assign(
              entities.PaymentRequest);

            if (!IsEmpty(entities.PaymentRequest.DesignatedPayeeCsePersonNo))
            {
              export.EftPayments.Update.DetailCsePersonsWorkSet.Number =
                entities.PaymentRequest.DesignatedPayeeCsePersonNo ?? Spaces
                (10);
            }
            else
            {
              export.EftPayments.Update.DetailCsePersonsWorkSet.Number =
                entities.PaymentRequest.CsePersonNumber ?? Spaces(10);
            }

            export.EftPayments.Update.DetailCsePersonsWorkSet.FormattedName =
              entities.ElectronicFundTransmission.ReceivingCompanyName ?? Spaces
              (33);
            export.EftPayments.Update.DetailPaymentStatus.Code =
              entities.PaymentStatus.Code;
            export.EftPayments.Next();
          }
        }
        else if (!IsEmpty(export.PaymentStatus.Code))
        {
          // 3.  Read by status
          export.EftPayments.Index = 0;
          export.EftPayments.Clear();

          foreach(var item in ReadElectronicFundTransmissionPaymentRequestPaymentStatusHistory2())
            
          {
            MoveElectronicFundTransmission(entities.ElectronicFundTransmission,
              export.EftPayments.Update.DetailElectronicFundTransmission);
            export.EftPayments.Update.DetailPaymentRequest.Assign(
              entities.PaymentRequest);

            if (!IsEmpty(entities.PaymentRequest.DesignatedPayeeCsePersonNo))
            {
              export.EftPayments.Update.DetailCsePersonsWorkSet.Number =
                entities.PaymentRequest.DesignatedPayeeCsePersonNo ?? Spaces
                (10);
            }
            else
            {
              export.EftPayments.Update.DetailCsePersonsWorkSet.Number =
                entities.PaymentRequest.CsePersonNumber ?? Spaces(10);
            }

            export.EftPayments.Update.DetailCsePersonsWorkSet.FormattedName =
              entities.ElectronicFundTransmission.ReceivingCompanyName ?? Spaces
              (33);
            export.EftPayments.Update.DetailPaymentStatus.Code =
              export.PaymentStatus.Code;
            export.EftPayments.Next();
          }
        }
        else
        {
          // 4.  Generic read
          export.EftPayments.Index = 0;
          export.EftPayments.Clear();

          foreach(var item in ReadElectronicFundTransmissionPaymentRequestPaymentStatusHistory4())
            
          {
            MoveElectronicFundTransmission(entities.ElectronicFundTransmission,
              export.EftPayments.Update.DetailElectronicFundTransmission);
            export.EftPayments.Update.DetailPaymentRequest.Assign(
              entities.PaymentRequest);

            if (!IsEmpty(entities.PaymentRequest.DesignatedPayeeCsePersonNo))
            {
              export.EftPayments.Update.DetailCsePersonsWorkSet.Number =
                entities.PaymentRequest.DesignatedPayeeCsePersonNo ?? Spaces
                (10);
            }
            else
            {
              export.EftPayments.Update.DetailCsePersonsWorkSet.Number =
                entities.PaymentRequest.CsePersonNumber ?? Spaces(10);
            }

            export.EftPayments.Update.DetailCsePersonsWorkSet.FormattedName =
              entities.ElectronicFundTransmission.ReceivingCompanyName ?? Spaces
              (33);
            export.EftPayments.Update.DetailPaymentStatus.Code =
              entities.PaymentStatus.Code;
            export.EftPayments.Next();
          }
        }

        // *****
        // If group view is empty display message.  If group view is full 
        // display message.
        // *****
        if (export.EftPayments.IsEmpty)
        {
          ExitState = "ACO_NI0000_NO_DATA_TO_DISPLAY";

          return;
        }

        if (export.EftPayments.IsFull)
        {
          ExitState = "ACO_NI0000_LIST_IS_FULL";
        }

        break;
      case "EXIT":
        ExitState = "ECO_LNK_RETURN_TO_MENU";

        break;
      case "PREV":
        ExitState = "ACO_NI0000_TOP_OF_LIST";

        break;
      case "NEXT":
        ExitState = "ACO_NI0000_BOTTOM_OF_LIST";

        break;
      case "EDTL":
        ExitState = "ECO_XFR_TO_LST_EFT_DETAIL";

        break;
      case "PACC":
        ExitState = "ECO_XFR_TO_LST_PAYEE_ACCT";

        break;
      case "SIGNOFF":
        UseScCabSignoff();

        break;
      case "LIST":
        if (AsChar(export.CsePerson.PromptField) == 'S' && AsChar
          (export.Status.PromptField) == 'S')
        {
          ExitState = "ACO_NE0000_INVALID_MULT_PROMPT_S";

          return;
        }

        switch(AsChar(export.CsePerson.PromptField))
        {
          case ' ':
            break;
          case '+':
            break;
          case 'S':
            ExitState = "ECO_LNK_TO_CSE_NAME_LIST";

            return;
          default:
            var field = GetField(export.CsePerson, "promptField");

            field.Error = true;

            ExitState = "ACO_NE0000_INVALID_PROMPT_VALUE";

            return;
        }

        switch(AsChar(export.Status.PromptField))
        {
          case ' ':
            break;
          case '+':
            break;
          case 'S':
            ExitState = "ECO_LNK_TO_LST_PAYMENT_STATUSES";

            return;
          default:
            var field = GetField(export.Status, "promptField");

            field.Error = true;

            ExitState = "ACO_NE0000_INVALID_PROMPT_VALUE";

            return;
        }

        ExitState = "ACO_NE0000_MUST_SELECT_4_PROMPT";

        break;
      case "RETURN":
        ExitState = "ACO_NE0000_RETURN";

        break;
      case "PPMT":
        ExitState = "ECO_XFR_TO_MTN_PREF_PMNT_METHOD";

        break;
      case "WDTL":
        if (Equal(local.PaymentStatus.Code, "REIS"))
        {
          if (ReadPaymentRequest())
          {
            export.PassPaymentRequest.SystemGeneratedIdentifier =
              entities.ReissuedTo.SystemGeneratedIdentifier;
            ExitState = "ECO_XFR_TO_LST_WARRANT_DETAIL";
          }
          else
          {
            ExitState = "FN0000_PYMT_REQUEST_NF";
          }
        }
        else
        {
          ExitState = "FN0000_INVALID_STAT_4_REQ_ACTION";
        }

        break;
      default:
        ExitState = "ACO_NE0000_INVALID_COMMAND";

        break;
    }
  }

  private static void MoveCsePersonsWorkSet(CsePersonsWorkSet source,
    CsePersonsWorkSet target)
  {
    target.Number = source.Number;
    target.FormattedName = source.FormattedName;
  }

  private static void MoveElectronicFundTransmission(
    ElectronicFundTransmission source, ElectronicFundTransmission target)
  {
    target.TransmissionIdentifier = source.TransmissionIdentifier;
    target.EffectiveEntryDate = source.EffectiveEntryDate;
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

    useImport.TextWorkArea.Text10 = local.LeftPadding.Text10;
    useExport.TextWorkArea.Text10 = local.LeftPadding.Text10;

    Call(EabPadLeftWithZeros.Execute, useImport, useExport);

    local.LeftPadding.Text10 = useExport.TextWorkArea.Text10;
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
    useImport.CsePerson.Number = export.PassThruFlow.Number;

    Call(ScCabTestSecurity.Execute, useImport, useExport);
  }

  private void UseSiReadCsePerson()
  {
    var useImport = new SiReadCsePerson.Import();
    var useExport = new SiReadCsePerson.Export();

    useImport.CsePersonsWorkSet.Number = export.CsePersonsWorkSet.Number;

    Call(SiReadCsePerson.Execute, useImport, useExport);

    MoveCsePersonsWorkSet(useExport.CsePersonsWorkSet, export.CsePersonsWorkSet);
      
  }

  private IEnumerable<bool>
    ReadElectronicFundTransmissionPaymentRequestPaymentStatusHistory1()
  {
    return ReadEach(
      "ReadElectronicFundTransmissionPaymentRequestPaymentStatusHistory1",
      (db, command) =>
      {
        db.SetInt32(
          command, "transmissionId", export.Search.TransmissionIdentifier);
        db.SetDate(command, "date1", export.From.Date.GetValueOrDefault());
        db.SetDate(command, "date2", export.To.Date.GetValueOrDefault());
        db.SetNullableString(
          command, "csePersonNumber", export.CsePersonsWorkSet.Number);
        db.SetNullableDate(
          command, "discontinueDate", local.Maximum.Date.GetValueOrDefault());
        db.SetInt32(
          command, "pstGeneratedId",
          entities.PaymentStatus.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        if (export.EftPayments.IsFull)
        {
          return false;
        }

        entities.ElectronicFundTransmission.TransmissionType =
          db.GetString(reader, 0);
        entities.ElectronicFundTransmission.TransmissionIdentifier =
          db.GetInt32(reader, 1);
        entities.ElectronicFundTransmission.PrqGeneratedId =
          db.GetNullableInt32(reader, 2);
        entities.PaymentRequest.SystemGeneratedIdentifier =
          db.GetInt32(reader, 2);
        entities.PaymentStatusHistory.PrqGeneratedId = db.GetInt32(reader, 2);
        entities.ElectronicFundTransmission.EffectiveEntryDate =
          db.GetNullableDate(reader, 3);
        entities.ElectronicFundTransmission.ReceivingCompanyName =
          db.GetNullableString(reader, 4);
        entities.PaymentRequest.ProcessDate = db.GetDate(reader, 5);
        entities.PaymentRequest.Amount = db.GetDecimal(reader, 6);
        entities.PaymentRequest.DesignatedPayeeCsePersonNo =
          db.GetNullableString(reader, 7);
        entities.PaymentRequest.CsePersonNumber =
          db.GetNullableString(reader, 8);
        entities.PaymentRequest.Classification = db.GetString(reader, 9);
        entities.PaymentRequest.Type1 = db.GetString(reader, 10);
        entities.PaymentRequest.PrqRGeneratedId =
          db.GetNullableInt32(reader, 11);
        entities.PaymentRequest.InterstateInd =
          db.GetNullableString(reader, 12);
        entities.PaymentStatusHistory.PstGeneratedId = db.GetInt32(reader, 13);
        entities.PaymentStatusHistory.SystemGeneratedIdentifier =
          db.GetInt32(reader, 14);
        entities.PaymentStatusHistory.DiscontinueDate =
          db.GetNullableDate(reader, 15);
        entities.ElectronicFundTransmission.Populated = true;
        entities.PaymentRequest.Populated = true;
        entities.PaymentStatusHistory.Populated = true;
        CheckValid<PaymentRequest>("Type1", entities.PaymentRequest.Type1);

        return true;
      });
  }

  private IEnumerable<bool>
    ReadElectronicFundTransmissionPaymentRequestPaymentStatusHistory2()
  {
    return ReadEach(
      "ReadElectronicFundTransmissionPaymentRequestPaymentStatusHistory2",
      (db, command) =>
      {
        db.SetInt32(
          command, "transmissionId", export.Search.TransmissionIdentifier);
        db.SetDate(command, "date1", export.From.Date.GetValueOrDefault());
        db.SetDate(command, "date2", export.To.Date.GetValueOrDefault());
        db.SetNullableDate(
          command, "discontinueDate", local.Maximum.Date.GetValueOrDefault());
        db.SetInt32(
          command, "pstGeneratedId",
          entities.PaymentStatus.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        if (export.EftPayments.IsFull)
        {
          return false;
        }

        entities.ElectronicFundTransmission.TransmissionType =
          db.GetString(reader, 0);
        entities.ElectronicFundTransmission.TransmissionIdentifier =
          db.GetInt32(reader, 1);
        entities.ElectronicFundTransmission.PrqGeneratedId =
          db.GetNullableInt32(reader, 2);
        entities.PaymentRequest.SystemGeneratedIdentifier =
          db.GetInt32(reader, 2);
        entities.PaymentStatusHistory.PrqGeneratedId = db.GetInt32(reader, 2);
        entities.ElectronicFundTransmission.EffectiveEntryDate =
          db.GetNullableDate(reader, 3);
        entities.ElectronicFundTransmission.ReceivingCompanyName =
          db.GetNullableString(reader, 4);
        entities.PaymentRequest.ProcessDate = db.GetDate(reader, 5);
        entities.PaymentRequest.Amount = db.GetDecimal(reader, 6);
        entities.PaymentRequest.DesignatedPayeeCsePersonNo =
          db.GetNullableString(reader, 7);
        entities.PaymentRequest.CsePersonNumber =
          db.GetNullableString(reader, 8);
        entities.PaymentRequest.Classification = db.GetString(reader, 9);
        entities.PaymentRequest.Type1 = db.GetString(reader, 10);
        entities.PaymentRequest.PrqRGeneratedId =
          db.GetNullableInt32(reader, 11);
        entities.PaymentRequest.InterstateInd =
          db.GetNullableString(reader, 12);
        entities.PaymentStatusHistory.PstGeneratedId = db.GetInt32(reader, 13);
        entities.PaymentStatusHistory.SystemGeneratedIdentifier =
          db.GetInt32(reader, 14);
        entities.PaymentStatusHistory.DiscontinueDate =
          db.GetNullableDate(reader, 15);
        entities.ElectronicFundTransmission.Populated = true;
        entities.PaymentRequest.Populated = true;
        entities.PaymentStatusHistory.Populated = true;
        CheckValid<PaymentRequest>("Type1", entities.PaymentRequest.Type1);

        return true;
      });
  }

  private IEnumerable<bool>
    ReadElectronicFundTransmissionPaymentRequestPaymentStatusHistory3()
  {
    return ReadEach(
      "ReadElectronicFundTransmissionPaymentRequestPaymentStatusHistory3",
      (db, command) =>
      {
        db.SetInt32(
          command, "transmissionId", export.Search.TransmissionIdentifier);
        db.SetDate(command, "date1", export.From.Date.GetValueOrDefault());
        db.SetDate(command, "date2", export.To.Date.GetValueOrDefault());
        db.SetNullableString(
          command, "csePersonNumber", export.CsePersonsWorkSet.Number);
        db.SetNullableDate(
          command, "discontinueDate", local.Maximum.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        if (export.EftPayments.IsFull)
        {
          return false;
        }

        entities.ElectronicFundTransmission.TransmissionType =
          db.GetString(reader, 0);
        entities.ElectronicFundTransmission.TransmissionIdentifier =
          db.GetInt32(reader, 1);
        entities.ElectronicFundTransmission.PrqGeneratedId =
          db.GetNullableInt32(reader, 2);
        entities.PaymentRequest.SystemGeneratedIdentifier =
          db.GetInt32(reader, 2);
        entities.PaymentStatusHistory.PrqGeneratedId = db.GetInt32(reader, 2);
        entities.ElectronicFundTransmission.EffectiveEntryDate =
          db.GetNullableDate(reader, 3);
        entities.ElectronicFundTransmission.ReceivingCompanyName =
          db.GetNullableString(reader, 4);
        entities.PaymentRequest.ProcessDate = db.GetDate(reader, 5);
        entities.PaymentRequest.Amount = db.GetDecimal(reader, 6);
        entities.PaymentRequest.DesignatedPayeeCsePersonNo =
          db.GetNullableString(reader, 7);
        entities.PaymentRequest.CsePersonNumber =
          db.GetNullableString(reader, 8);
        entities.PaymentRequest.Classification = db.GetString(reader, 9);
        entities.PaymentRequest.Type1 = db.GetString(reader, 10);
        entities.PaymentRequest.PrqRGeneratedId =
          db.GetNullableInt32(reader, 11);
        entities.PaymentRequest.InterstateInd =
          db.GetNullableString(reader, 12);
        entities.PaymentStatusHistory.PstGeneratedId = db.GetInt32(reader, 13);
        entities.PaymentStatus.SystemGeneratedIdentifier =
          db.GetInt32(reader, 13);
        entities.PaymentStatusHistory.SystemGeneratedIdentifier =
          db.GetInt32(reader, 14);
        entities.PaymentStatusHistory.DiscontinueDate =
          db.GetNullableDate(reader, 15);
        entities.PaymentStatus.Code = db.GetString(reader, 16);
        entities.ElectronicFundTransmission.Populated = true;
        entities.PaymentRequest.Populated = true;
        entities.PaymentStatusHistory.Populated = true;
        entities.PaymentStatus.Populated = true;
        CheckValid<PaymentRequest>("Type1", entities.PaymentRequest.Type1);

        return true;
      });
  }

  private IEnumerable<bool>
    ReadElectronicFundTransmissionPaymentRequestPaymentStatusHistory4()
  {
    return ReadEach(
      "ReadElectronicFundTransmissionPaymentRequestPaymentStatusHistory4",
      (db, command) =>
      {
        db.SetInt32(
          command, "transmissionId", export.Search.TransmissionIdentifier);
        db.SetDate(command, "date1", export.From.Date.GetValueOrDefault());
        db.SetDate(command, "date2", export.To.Date.GetValueOrDefault());
        db.SetNullableDate(
          command, "discontinueDate", local.Maximum.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        if (export.EftPayments.IsFull)
        {
          return false;
        }

        entities.ElectronicFundTransmission.TransmissionType =
          db.GetString(reader, 0);
        entities.ElectronicFundTransmission.TransmissionIdentifier =
          db.GetInt32(reader, 1);
        entities.ElectronicFundTransmission.PrqGeneratedId =
          db.GetNullableInt32(reader, 2);
        entities.PaymentRequest.SystemGeneratedIdentifier =
          db.GetInt32(reader, 2);
        entities.PaymentStatusHistory.PrqGeneratedId = db.GetInt32(reader, 2);
        entities.ElectronicFundTransmission.EffectiveEntryDate =
          db.GetNullableDate(reader, 3);
        entities.ElectronicFundTransmission.ReceivingCompanyName =
          db.GetNullableString(reader, 4);
        entities.PaymentRequest.ProcessDate = db.GetDate(reader, 5);
        entities.PaymentRequest.Amount = db.GetDecimal(reader, 6);
        entities.PaymentRequest.DesignatedPayeeCsePersonNo =
          db.GetNullableString(reader, 7);
        entities.PaymentRequest.CsePersonNumber =
          db.GetNullableString(reader, 8);
        entities.PaymentRequest.Classification = db.GetString(reader, 9);
        entities.PaymentRequest.Type1 = db.GetString(reader, 10);
        entities.PaymentRequest.PrqRGeneratedId =
          db.GetNullableInt32(reader, 11);
        entities.PaymentRequest.InterstateInd =
          db.GetNullableString(reader, 12);
        entities.PaymentStatusHistory.PstGeneratedId = db.GetInt32(reader, 13);
        entities.PaymentStatus.SystemGeneratedIdentifier =
          db.GetInt32(reader, 13);
        entities.PaymentStatusHistory.SystemGeneratedIdentifier =
          db.GetInt32(reader, 14);
        entities.PaymentStatusHistory.DiscontinueDate =
          db.GetNullableDate(reader, 15);
        entities.PaymentStatus.Code = db.GetString(reader, 16);
        entities.ElectronicFundTransmission.Populated = true;
        entities.PaymentRequest.Populated = true;
        entities.PaymentStatusHistory.Populated = true;
        entities.PaymentStatus.Populated = true;
        CheckValid<PaymentRequest>("Type1", entities.PaymentRequest.Type1);

        return true;
      });
  }

  private bool ReadPaymentRequest()
  {
    entities.ReissuedTo.Populated = false;

    return Read("ReadPaymentRequest",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "prqRGeneratedId",
          export.PassPaymentRequest.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.ReissuedTo.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.ReissuedTo.PrqRGeneratedId = db.GetNullableInt32(reader, 1);
        entities.ReissuedTo.Populated = true;
      });
  }

  private bool ReadPaymentStatus()
  {
    entities.PaymentStatus.Populated = false;

    return Read("ReadPaymentStatus",
      (db, command) =>
      {
        db.SetString(command, "code", export.PaymentStatus.Code);
      },
      (db, reader) =>
      {
        entities.PaymentStatus.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.PaymentStatus.Code = db.GetString(reader, 1);
        entities.PaymentStatus.Populated = true;
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
    /// <summary>A EftPaymentsGroup group.</summary>
    [Serializable]
    public class EftPaymentsGroup
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
      /// A value of DetailElectronicFundTransmission.
      /// </summary>
      [JsonPropertyName("detailElectronicFundTransmission")]
      public ElectronicFundTransmission DetailElectronicFundTransmission
      {
        get => detailElectronicFundTransmission ??= new();
        set => detailElectronicFundTransmission = value;
      }

      /// <summary>
      /// A value of DetailPaymentRequest.
      /// </summary>
      [JsonPropertyName("detailPaymentRequest")]
      public PaymentRequest DetailPaymentRequest
      {
        get => detailPaymentRequest ??= new();
        set => detailPaymentRequest = value;
      }

      /// <summary>
      /// A value of DetailCsePersonsWorkSet.
      /// </summary>
      [JsonPropertyName("detailCsePersonsWorkSet")]
      public CsePersonsWorkSet DetailCsePersonsWorkSet
      {
        get => detailCsePersonsWorkSet ??= new();
        set => detailCsePersonsWorkSet = value;
      }

      /// <summary>
      /// A value of DetailPaymentStatus.
      /// </summary>
      [JsonPropertyName("detailPaymentStatus")]
      public PaymentStatus DetailPaymentStatus
      {
        get => detailPaymentStatus ??= new();
        set => detailPaymentStatus = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 170;

      private Common detailCommon;
      private ElectronicFundTransmission detailElectronicFundTransmission;
      private PaymentRequest detailPaymentRequest;
      private CsePersonsWorkSet detailCsePersonsWorkSet;
      private PaymentStatus detailPaymentStatus;
    }

    /// <summary>
    /// A value of Search.
    /// </summary>
    [JsonPropertyName("search")]
    public ElectronicFundTransmission Search
    {
      get => search ??= new();
      set => search = value;
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
    /// A value of PaymentStatus.
    /// </summary>
    [JsonPropertyName("paymentStatus")]
    public PaymentStatus PaymentStatus
    {
      get => paymentStatus ??= new();
      set => paymentStatus = value;
    }

    /// <summary>
    /// A value of From.
    /// </summary>
    [JsonPropertyName("from")]
    public DateWorkArea From
    {
      get => from ??= new();
      set => from = value;
    }

    /// <summary>
    /// A value of To.
    /// </summary>
    [JsonPropertyName("to")]
    public DateWorkArea To
    {
      get => to ??= new();
      set => to = value;
    }

    /// <summary>
    /// A value of PaymentRequest.
    /// </summary>
    [JsonPropertyName("paymentRequest")]
    public PaymentRequest PaymentRequest
    {
      get => paymentRequest ??= new();
      set => paymentRequest = value;
    }

    /// <summary>
    /// A value of Status.
    /// </summary>
    [JsonPropertyName("status")]
    public Standard Status
    {
      get => status ??= new();
      set => status = value;
    }

    /// <summary>
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public Standard CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
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
    /// Gets a value of EftPayments.
    /// </summary>
    [JsonIgnore]
    public Array<EftPaymentsGroup> EftPayments => eftPayments ??= new(
      EftPaymentsGroup.Capacity);

    /// <summary>
    /// Gets a value of EftPayments for json serialization.
    /// </summary>
    [JsonPropertyName("eftPayments")]
    [Computed]
    public IList<EftPaymentsGroup> EftPayments_Json
    {
      get => eftPayments;
      set => EftPayments.Assign(value);
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

    private ElectronicFundTransmission search;
    private CsePersonsWorkSet csePersonsWorkSet;
    private PaymentStatus paymentStatus;
    private DateWorkArea from;
    private DateWorkArea to;
    private PaymentRequest paymentRequest;
    private Standard status;
    private Standard csePerson;
    private Standard standard;
    private Array<EftPaymentsGroup> eftPayments;
    private NextTranInfo hidden;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>A EftPaymentsGroup group.</summary>
    [Serializable]
    public class EftPaymentsGroup
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
      /// A value of DetailElectronicFundTransmission.
      /// </summary>
      [JsonPropertyName("detailElectronicFundTransmission")]
      public ElectronicFundTransmission DetailElectronicFundTransmission
      {
        get => detailElectronicFundTransmission ??= new();
        set => detailElectronicFundTransmission = value;
      }

      /// <summary>
      /// A value of DetailPaymentRequest.
      /// </summary>
      [JsonPropertyName("detailPaymentRequest")]
      public PaymentRequest DetailPaymentRequest
      {
        get => detailPaymentRequest ??= new();
        set => detailPaymentRequest = value;
      }

      /// <summary>
      /// A value of DetailCsePersonsWorkSet.
      /// </summary>
      [JsonPropertyName("detailCsePersonsWorkSet")]
      public CsePersonsWorkSet DetailCsePersonsWorkSet
      {
        get => detailCsePersonsWorkSet ??= new();
        set => detailCsePersonsWorkSet = value;
      }

      /// <summary>
      /// A value of DetailPaymentStatus.
      /// </summary>
      [JsonPropertyName("detailPaymentStatus")]
      public PaymentStatus DetailPaymentStatus
      {
        get => detailPaymentStatus ??= new();
        set => detailPaymentStatus = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 170;

      private Common detailCommon;
      private ElectronicFundTransmission detailElectronicFundTransmission;
      private PaymentRequest detailPaymentRequest;
      private CsePersonsWorkSet detailCsePersonsWorkSet;
      private PaymentStatus detailPaymentStatus;
    }

    /// <summary>
    /// A value of PassZzzzzzzzzzzzzzzzzzzz.
    /// </summary>
    [JsonPropertyName("passZzzzzzzzzzzzzzzzzzzz")]
    public PaymentRequest PassZzzzzzzzzzzzzzzzzzzz
    {
      get => passZzzzzzzzzzzzzzzzzzzz ??= new();
      set => passZzzzzzzzzzzzzzzzzzzz = value;
    }

    /// <summary>
    /// A value of Search.
    /// </summary>
    [JsonPropertyName("search")]
    public ElectronicFundTransmission Search
    {
      get => search ??= new();
      set => search = value;
    }

    /// <summary>
    /// A value of PassThruFlow.
    /// </summary>
    [JsonPropertyName("passThruFlow")]
    public CsePerson PassThruFlow
    {
      get => passThruFlow ??= new();
      set => passThruFlow = value;
    }

    /// <summary>
    /// A value of PassElectronicFundTransmission.
    /// </summary>
    [JsonPropertyName("passElectronicFundTransmission")]
    public ElectronicFundTransmission PassElectronicFundTransmission
    {
      get => passElectronicFundTransmission ??= new();
      set => passElectronicFundTransmission = value;
    }

    /// <summary>
    /// A value of PassPaymentRequest.
    /// </summary>
    [JsonPropertyName("passPaymentRequest")]
    public PaymentRequest PassPaymentRequest
    {
      get => passPaymentRequest ??= new();
      set => passPaymentRequest = value;
    }

    /// <summary>
    /// A value of PassCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("passCsePersonsWorkSet")]
    public CsePersonsWorkSet PassCsePersonsWorkSet
    {
      get => passCsePersonsWorkSet ??= new();
      set => passCsePersonsWorkSet = value;
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
    /// A value of PaymentStatus.
    /// </summary>
    [JsonPropertyName("paymentStatus")]
    public PaymentStatus PaymentStatus
    {
      get => paymentStatus ??= new();
      set => paymentStatus = value;
    }

    /// <summary>
    /// A value of From.
    /// </summary>
    [JsonPropertyName("from")]
    public DateWorkArea From
    {
      get => from ??= new();
      set => from = value;
    }

    /// <summary>
    /// A value of To.
    /// </summary>
    [JsonPropertyName("to")]
    public DateWorkArea To
    {
      get => to ??= new();
      set => to = value;
    }

    /// <summary>
    /// A value of PaymentRequest.
    /// </summary>
    [JsonPropertyName("paymentRequest")]
    public PaymentRequest PaymentRequest
    {
      get => paymentRequest ??= new();
      set => paymentRequest = value;
    }

    /// <summary>
    /// A value of Status.
    /// </summary>
    [JsonPropertyName("status")]
    public Standard Status
    {
      get => status ??= new();
      set => status = value;
    }

    /// <summary>
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public Standard CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
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
    /// Gets a value of EftPayments.
    /// </summary>
    [JsonIgnore]
    public Array<EftPaymentsGroup> EftPayments => eftPayments ??= new(
      EftPaymentsGroup.Capacity);

    /// <summary>
    /// Gets a value of EftPayments for json serialization.
    /// </summary>
    [JsonPropertyName("eftPayments")]
    [Computed]
    public IList<EftPaymentsGroup> EftPayments_Json
    {
      get => eftPayments;
      set => EftPayments.Assign(value);
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

    private PaymentRequest passZzzzzzzzzzzzzzzzzzzz;
    private ElectronicFundTransmission search;
    private CsePerson passThruFlow;
    private ElectronicFundTransmission passElectronicFundTransmission;
    private PaymentRequest passPaymentRequest;
    private CsePersonsWorkSet passCsePersonsWorkSet;
    private CsePersonsWorkSet csePersonsWorkSet;
    private PaymentStatus paymentStatus;
    private DateWorkArea from;
    private DateWorkArea to;
    private PaymentRequest paymentRequest;
    private Standard status;
    private Standard csePerson;
    private Standard standard;
    private Array<EftPaymentsGroup> eftPayments;
    private NextTranInfo hidden;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of PaymentStatus.
    /// </summary>
    [JsonPropertyName("paymentStatus")]
    public PaymentStatus PaymentStatus
    {
      get => paymentStatus ??= new();
      set => paymentStatus = value;
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
    /// A value of Current.
    /// </summary>
    [JsonPropertyName("current")]
    public DateWorkArea Current
    {
      get => current ??= new();
      set => current = value;
    }

    /// <summary>
    /// A value of LeftPadding.
    /// </summary>
    [JsonPropertyName("leftPadding")]
    public TextWorkArea LeftPadding
    {
      get => leftPadding ??= new();
      set => leftPadding = value;
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
    /// A value of Maximum.
    /// </summary>
    [JsonPropertyName("maximum")]
    public DateWorkArea Maximum
    {
      get => maximum ??= new();
      set => maximum = value;
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

    private PaymentStatus paymentStatus;
    private CsePersonsWorkSet csePersonsWorkSet;
    private DateWorkArea current;
    private TextWorkArea leftPadding;
    private DateWorkArea null1;
    private DateWorkArea maximum;
    private Common common;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of ElectronicFundTransmission.
    /// </summary>
    [JsonPropertyName("electronicFundTransmission")]
    public ElectronicFundTransmission ElectronicFundTransmission
    {
      get => electronicFundTransmission ??= new();
      set => electronicFundTransmission = value;
    }

    /// <summary>
    /// A value of PaymentRequest.
    /// </summary>
    [JsonPropertyName("paymentRequest")]
    public PaymentRequest PaymentRequest
    {
      get => paymentRequest ??= new();
      set => paymentRequest = value;
    }

    /// <summary>
    /// A value of PaymentStatusHistory.
    /// </summary>
    [JsonPropertyName("paymentStatusHistory")]
    public PaymentStatusHistory PaymentStatusHistory
    {
      get => paymentStatusHistory ??= new();
      set => paymentStatusHistory = value;
    }

    /// <summary>
    /// A value of PaymentStatus.
    /// </summary>
    [JsonPropertyName("paymentStatus")]
    public PaymentStatus PaymentStatus
    {
      get => paymentStatus ??= new();
      set => paymentStatus = value;
    }

    /// <summary>
    /// A value of ReissuedTo.
    /// </summary>
    [JsonPropertyName("reissuedTo")]
    public PaymentRequest ReissuedTo
    {
      get => reissuedTo ??= new();
      set => reissuedTo = value;
    }

    private ElectronicFundTransmission electronicFundTransmission;
    private PaymentRequest paymentRequest;
    private PaymentStatusHistory paymentStatusHistory;
    private PaymentStatus paymentStatus;
    private PaymentRequest reissuedTo;
  }
#endregion
}
