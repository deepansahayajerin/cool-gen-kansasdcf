// Program: FN_WHST_LST_WARRANT_STATUS_HIST, ID: 371871534, model: 746.
// Short name: SWEWHSTP
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
/// A program: FN_WHST_LST_WARRANT_STATUS_HIST.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class FnWhstLstWarrantStatusHist: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_WHST_LST_WARRANT_STATUS_HIST program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnWhstLstWarrantStatusHist(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnWhstLstWarrantStatusHist.
  /// </summary>
  public FnWhstLstWarrantStatusHist(IContext context, Import import,
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
    // Procedure : List Warrant Status History
    // Developed by : R.B.Mohapatra, MTW
    // Change Log :
    // 03/11/96    Incorporated Security and Nexttran retrofits
    // 12/16/96    R. Marchman	     Add new security/next tran
    // 12/05/98 - K Doshi - Various phase 2 changes as suggested by SMEs
    // 12/03/98 - K Doshi - Amend return flow from WARA.
    // 04/06/00 - C. Scroggins - Added security check for family violence on the
    // payee.
    // 02/19/01 - K.Doshi - PR#111410 - Only block address for family violence 
    // protection.
    // 03/02/01 - K.Doshi - PR#111410 - Also perform family violence check for 
    // DP.
    // 05/23/01 - K.Doshi - WR#285 - Cater for duplicate warrant #s..
    // 03/08/19 - GVandy CQ65422  Correct logic so that warrants without warrant
    // numbers
    // 			   (e.g. in REQ or KPC status) will display.
    // ***************************************************************************
    ExitState = "ACO_NN0000_ALL_OK";

    if (Equal(global.Command, "EXIT"))
    {
      ExitState = "ECO_LNK_RETURN_TO_MENU";

      return;
    }

    if (Equal(global.Command, "CLEAR"))
    {
      ExitState = "ACO_NI0000_CLEAR_SUCCESSFUL";

      return;
    }

    if (Equal(global.Command, "RETLINK"))
    {
      return;
    }

    export.HiddenNextTranInfo.Assign(import.HiddenNextTranInfo);
    export.HiddenPaymentRequest.Assign(import.HiddenPaymentRequest);
    export.Standard.NextTransaction = import.Standard.NextTransaction;
    export.PaymentRequest.Number = import.PaymentRequest.Number ?? "";

    if (Equal(global.Command, "DISPLAY"))
    {
      // *** Continue
    }
    else
    {
      MovePaymentRequest(import.PaymentRequest, export.PaymentRequest);
      export.LocalFnWorkArea.Assign(import.LocalFnWorkArea);
      export.Payee.FormattedName = import.Payee.FormattedName;
      export.Payee.Number = import.PaymentRequest.CsePersonNumber ?? Spaces(10);
      export.DesignatedPayee.Number =
        import.PaymentRequest.DesignatedPayeeCsePersonNo ?? Spaces(10);
      export.DesignatedPayee.FormattedName = import.DesigPayee.FormattedName;

      export.Export1.Index = 0;
      export.Export1.Clear();

      for(import.Import1.Index = 0; import.Import1.Index < import
        .Import1.Count; ++import.Import1.Index)
      {
        if (export.Export1.IsFull)
        {
          break;
        }

        export.Export1.Update.DetailPaymentStatus.Code =
          import.Import1.Item.DetailPaymentStatus.Code;
        export.Export1.Update.DetailPaymentStatusHistory.Assign(
          import.Import1.Item.DetailPaymentStatusHistory);
        export.Export1.Next();
      }

      export.PassThruFlowCsePerson.Number =
        import.PaymentRequest.CsePersonNumber ?? Spaces(10);
      export.PassThruFlowPaymentRequest.Assign(import.PaymentRequest);
    }

    if (!IsEmpty(import.Standard.NextTransaction))
    {
      export.HiddenNextTranInfo.CsePersonNumberObligee =
        import.PaymentRequest.CsePersonNumber ?? "";
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

      return;
    }

    if (Equal(global.Command, "XXFMMENU"))
    {
      return;
    }

    if (Equal(global.Command, "PACC") || Equal(global.Command, "WARA") || Equal
      (global.Command, "WARR") || Equal(global.Command, "WAST") || Equal
      (global.Command, "WDTL"))
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

    if (!IsEmpty(export.PaymentRequest.Number))
    {
      local.TextWorkArea.Text10 = export.PaymentRequest.Number ?? Spaces(10);
      UseEabPadLeftWithZeros();
      export.PaymentRequest.Number = Substring(local.TextWorkArea.Text10, 2, 9);
    }

    switch(TrimEnd(global.Command))
    {
      case "DISPLAY":
        if (!IsEmpty(import.WarrantNoPrompt.SelectChar))
        {
          ExitState = "ACO_NE0000_PROMPT_INVALID_W_FNCT";

          var field = GetField(export.WarrantNumberPromptIn, "selectChar");

          field.Error = true;

          return;
        }

        // @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
        // @@@ New code below
        // @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
        // 03/08/2019  GVandy CQ65422  Correct logic so that warrants without 
        // warrant numbers
        // (e.g. in REQ or KPC status) will display.
        if (!Equal(import.PaymentRequest.Number,
          import.HiddenPaymentRequest.Number) && import
          .PaymentRequest.SystemGeneratedIdentifier == import
          .HiddenPaymentRequest.SystemGeneratedIdentifier)
        {
          export.PaymentRequest.SystemGeneratedIdentifier = 0;
        }
        else
        {
          export.PaymentRequest.SystemGeneratedIdentifier =
            import.PaymentRequest.SystemGeneratedIdentifier;
        }

        if (export.PaymentRequest.SystemGeneratedIdentifier == 0 && IsEmpty
          (export.PaymentRequest.Number))
        {
          ExitState = "FN0000_INVALID_INPUT";

          var field = GetField(export.PaymentRequest, "number");

          field.Error = true;

          return;
        }

        if (export.PaymentRequest.SystemGeneratedIdentifier != 0)
        {
          if (!ReadPaymentRequest2())
          {
            ExitState = "FN0000_PASSED_PAYMENT_REQ_NF";

            return;
          }
        }
        else
        {
          // ---------------------------------------------
          // 05/23/01 - K. Doshi - WR# 285
          // Cater for Duplicate warrant #s.
          // ---------------------------------------------
          ReadPaymentRequest3();

          if (local.NbrOfWarrants.Count > 1)
          {
            // ------------------------------------------------------------
            // Duplicate exists. Only send warrant # and duplicate_warrant
            // flag via dialog flow.
            // ------------------------------------------------------------
            export.Payee.Number = "";
            export.Payee.FormattedName = "";
            export.PassThruFlowPaymentRequest.Assign(export.PaymentRequest);
            export.HiddenPaymentRequest.Number = "";
            export.DuplicateWarrantsExist.Flag = "Y";
            ExitState = "ECO_LNK_TO_LST_WARRANTS";

            return;
          }

          if (!ReadPaymentRequest1())
          {
            var field = GetField(export.PaymentRequest, "number");

            field.Error = true;

            ExitState = "FN0000_WARRANT_NF";

            return;
          }
        }

        // 03/08/2019  GVandy CQ65422  End of changes for this ticket.
        // @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
        // @@@ New code above
        // @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
        MovePaymentRequest(entities.PaymentRequest, export.PaymentRequest);
        export.HiddenPaymentRequest.Assign(entities.PaymentRequest);

        // <<< RBM   02/04/1998  If Payee and DP are same, then set DP to 
        // Spaces.
        if (Equal(export.PaymentRequest.DesignatedPayeeCsePersonNo,
          export.PaymentRequest.CsePersonNumber))
        {
          export.PaymentRequest.DesignatedPayeeCsePersonNo = "";
        }

        // *** Get Payee Name from ADABAS ***
        if (IsEmpty(import.Received.FormattedName))
        {
          local.CsePersonsWorkSet.Number =
            export.PaymentRequest.CsePersonNumber ?? Spaces(10);
          UseSiReadCsePerson();

          if (IsExitState("CSE_PERSON_NF"))
          {
            ExitState = "FN0000_PAYEE_CSE_PERSON_NF";

            return;
          }

          export.Payee.FormattedName = local.CsePersonsWorkSet.FormattedName;
        }
        else
        {
          export.Payee.FormattedName = import.Received.FormattedName;
        }

        // *** Finding the Designated Payee Cse_Person Name ***
        export.DesignatedPayee.FormattedName = "";

        if (!IsEmpty(export.PaymentRequest.DesignatedPayeeCsePersonNo))
        {
          local.CsePersonsWorkSet.Number =
            entities.PaymentRequest.DesignatedPayeeCsePersonNo ?? Spaces(10);
          UseSiReadCsePerson();

          if (IsExitState("CSE_PERSON_NF"))
          {
            ExitState = "FN0000_DESIG_PAYEE_CSE_PERSON_NF";

            return;
          }

          export.DesignatedPayee.FormattedName =
            local.CsePersonsWorkSet.FormattedName;
        }

        // *** Getting the Detail Lines  ***
        export.Export1.Index = 0;
        export.Export1.Clear();

        foreach(var item in ReadPaymentStatusHistory())
        {
          local.FoundFlagIn.Flag = "Y";
          export.Export1.Update.DetailPaymentStatusHistory.Assign(
            entities.PaymentStatusHistory);

          // *** The following READ must cause an ABORT if it is Unsuccessful 
          // due to the Mandatory Relationship between the operand entity types
          // ***
          if (ReadPaymentStatus())
          {
            export.Export1.Update.DetailPaymentStatus.Code =
              entities.PaymentStatus.Code;
          }

          export.Export1.Next();
        }

        // ----------------------------------------------
        // 03/05/2001 - K.Doshi - PR#111410
        // Add FV check for DP.
        // ----------------------------------------------
        if (!IsEmpty(export.PaymentRequest.DesignatedPayeeCsePersonNo))
        {
          local.CsePersonsWorkSet.Number =
            entities.PaymentRequest.DesignatedPayeeCsePersonNo ?? Spaces(10);
          UseScSecurityCheckForFv();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            var field =
              GetField(export.PaymentRequest, "designatedPayeeCsePersonNo");

            field.Error = true;

            return;
          }
        }

        // ---------------------------------------------------------------------------------------
        // 02/19/2001 - K.Doshi - PR#111410
        // Moved call here so rest of screen fields will still display 
        // irrespective of FV protection.
        // ---------------------------------------------------------------------------------------
        local.CsePersonsWorkSet.Number =
          entities.PaymentRequest.CsePersonNumber ?? Spaces(10);
        UseScSecurityCheckForFv();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          var field = GetField(export.PaymentRequest, "csePersonNumber");

          field.Error = true;

          return;
        }

        // -------------------------------------------------------
        // KD - 11/19/98
        // Display the most recent address instead of the original
        // mailing address.
        // -------------------------------------------------------
        if (ReadWarrantRemailAddress())
        {
          export.LocalFnWorkArea.Name = entities.WarrantRemailAddress.Name ?? Spaces
            (33);
          export.LocalFnWorkArea.AddressLine1 =
            entities.WarrantRemailAddress.Street1;
          export.LocalFnWorkArea.AddressLine2 =
            entities.WarrantRemailAddress.Street2 ?? Spaces(20);
          export.LocalFnWorkArea.City = entities.WarrantRemailAddress.City;
          export.LocalFnWorkArea.State = entities.WarrantRemailAddress.State;
          export.LocalFnWorkArea.ZipCode =
            TrimEnd(entities.WarrantRemailAddress.ZipCode5) + TrimEnd
            (entities.WarrantRemailAddress.ZipCode4) + TrimEnd
            (entities.WarrantRemailAddress.ZipCode3);
        }

        if (!entities.WarrantRemailAddress.Populated)
        {
          export.LocalFnWorkArea.Name = "No Mailing Address";
          export.LocalFnWorkArea.AddressLine1 = "was recorded";
        }

        if (export.Export1.IsFull)
        {
          ExitState = "ACO_NI0000_LST_RETURNED_FULL";

          return;
        }

        ExitState = "ACO_NI0000_DISPLAY_SUCCESSFUL";

        break;
      case "LIST":
        // *** Processing for PROMPT ***
        // Set Exit_State to flow to 'List Warrant Number' Procedure
        if (AsChar(import.WarrantNoPrompt.SelectChar) != 'S')
        {
          var field = GetField(export.WarrantNumberPromptIn, "selectChar");

          field.Error = true;

          ExitState = "ACO_NE0000_INVALID_SELECT_CODE";
        }
        else
        {
          ExitState = "ECO_LNK_TO_LST_WARRANTS";
        }

        break;
      case "RETURN":
        ExitState = "ACO_NE0000_RETURN";

        break;
      case "NEXT":
        ExitState = "ACO_NI0000_BOTTOM_OF_LIST";

        break;
      case "PREV":
        ExitState = "ACO_NI0000_TOP_OF_LIST";

        break;
      case "EXIT":
        break;
      case "SIGNOFF":
        // *** Set Exit_State to flow to Signoff Procedure ***
        UseScCabSignoff();

        break;
      case "PACC":
        ExitState = "ECO_XFR_TO_LST_PAYEE_ACCT";

        break;
      case "WARR":
        ExitState = "ECO_LNK_TO_LST_WARRANTS";

        break;
      case "WDTL":
        ExitState = "ECO_LNK_TO_WDTL";

        break;
      case "WAST":
        ExitState = "ECO_LNK_TO_WAST";

        break;
      case "WARA":
        ExitState = "ECO_LNK_TO_WARA";

        break;
      default:
        ExitState = "ZD_ACO_NE0000_INVALID_PF_KEY_3";

        break;
    }
  }

  private static void MoveCsePersonsWorkSet(CsePersonsWorkSet source,
    CsePersonsWorkSet target)
  {
    target.Number = source.Number;
    target.FormattedName = source.FormattedName;
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

  private static void MovePaymentRequest(PaymentRequest source,
    PaymentRequest target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Type1 = source.Type1;
    target.Amount = source.Amount;
    target.ProcessDate = source.ProcessDate;
    target.CsePersonNumber = source.CsePersonNumber;
    target.DesignatedPayeeCsePersonNo = source.DesignatedPayeeCsePersonNo;
    target.Number = source.Number;
    target.PrintDate = source.PrintDate;
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

    useImport.CsePerson.Number = export.PassThruFlowCsePerson.Number;

    Call(ScCabTestSecurity.Execute, useImport, useExport);
  }

  private void UseScSecurityCheckForFv()
  {
    var useImport = new ScSecurityCheckForFv.Import();
    var useExport = new ScSecurityCheckForFv.Export();

    useImport.CsePersonsWorkSet.Number = local.CsePersonsWorkSet.Number;

    Call(ScSecurityCheckForFv.Execute, useImport, useExport);
  }

  private void UseSiReadCsePerson()
  {
    var useImport = new SiReadCsePerson.Import();
    var useExport = new SiReadCsePerson.Export();

    useImport.CsePersonsWorkSet.Number = local.CsePersonsWorkSet.Number;

    Call(SiReadCsePerson.Execute, useImport, useExport);

    MoveCsePersonsWorkSet(useExport.CsePersonsWorkSet, local.CsePersonsWorkSet);
  }

  private bool ReadPaymentRequest1()
  {
    entities.PaymentRequest.Populated = false;

    return Read("ReadPaymentRequest1",
      (db, command) =>
      {
        db.SetNullableString(
          command, "number", export.PaymentRequest.Number ?? "");
      },
      (db, reader) =>
      {
        entities.PaymentRequest.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.PaymentRequest.ProcessDate = db.GetDate(reader, 1);
        entities.PaymentRequest.Amount = db.GetDecimal(reader, 2);
        entities.PaymentRequest.DesignatedPayeeCsePersonNo =
          db.GetNullableString(reader, 3);
        entities.PaymentRequest.CsePersonNumber =
          db.GetNullableString(reader, 4);
        entities.PaymentRequest.Number = db.GetNullableString(reader, 5);
        entities.PaymentRequest.PrintDate = db.GetNullableDate(reader, 6);
        entities.PaymentRequest.Type1 = db.GetString(reader, 7);
        entities.PaymentRequest.PrqRGeneratedId =
          db.GetNullableInt32(reader, 8);
        entities.PaymentRequest.Populated = true;
        CheckValid<PaymentRequest>("Type1", entities.PaymentRequest.Type1);
      });
  }

  private bool ReadPaymentRequest2()
  {
    entities.PaymentRequest.Populated = false;

    return Read("ReadPaymentRequest2",
      (db, command) =>
      {
        db.SetInt32(
          command, "paymentRequestId",
          export.PaymentRequest.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.PaymentRequest.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.PaymentRequest.ProcessDate = db.GetDate(reader, 1);
        entities.PaymentRequest.Amount = db.GetDecimal(reader, 2);
        entities.PaymentRequest.DesignatedPayeeCsePersonNo =
          db.GetNullableString(reader, 3);
        entities.PaymentRequest.CsePersonNumber =
          db.GetNullableString(reader, 4);
        entities.PaymentRequest.Number = db.GetNullableString(reader, 5);
        entities.PaymentRequest.PrintDate = db.GetNullableDate(reader, 6);
        entities.PaymentRequest.Type1 = db.GetString(reader, 7);
        entities.PaymentRequest.PrqRGeneratedId =
          db.GetNullableInt32(reader, 8);
        entities.PaymentRequest.Populated = true;
        CheckValid<PaymentRequest>("Type1", entities.PaymentRequest.Type1);
      });
  }

  private bool ReadPaymentRequest3()
  {
    return Read("ReadPaymentRequest3",
      (db, command) =>
      {
        db.SetNullableString(
          command, "number", export.PaymentRequest.Number ?? "");
      },
      (db, reader) =>
      {
        local.NbrOfWarrants.Count = db.GetInt32(reader, 0);
      });
  }

  private bool ReadPaymentStatus()
  {
    System.Diagnostics.Debug.Assert(entities.PaymentStatusHistory.Populated);
    entities.PaymentStatus.Populated = false;

    return Read("ReadPaymentStatus",
      (db, command) =>
      {
        db.SetInt32(
          command, "paymentStatusId",
          entities.PaymentStatusHistory.PstGeneratedId);
      },
      (db, reader) =>
      {
        entities.PaymentStatus.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.PaymentStatus.Code = db.GetString(reader, 1);
        entities.PaymentStatus.Populated = true;
      });
  }

  private IEnumerable<bool> ReadPaymentStatusHistory()
  {
    return ReadEach("ReadPaymentStatusHistory",
      (db, command) =>
      {
        db.SetInt32(
          command, "prqGeneratedId",
          entities.PaymentRequest.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        if (export.Export1.IsFull)
        {
          return false;
        }

        entities.PaymentStatusHistory.PstGeneratedId = db.GetInt32(reader, 0);
        entities.PaymentStatusHistory.PrqGeneratedId = db.GetInt32(reader, 1);
        entities.PaymentStatusHistory.SystemGeneratedIdentifier =
          db.GetInt32(reader, 2);
        entities.PaymentStatusHistory.EffectiveDate = db.GetDate(reader, 3);
        entities.PaymentStatusHistory.CreatedBy = db.GetString(reader, 4);
        entities.PaymentStatusHistory.CreatedTimestamp =
          db.GetDateTime(reader, 5);
        entities.PaymentStatusHistory.ReasonText =
          db.GetNullableString(reader, 6);
        entities.PaymentStatusHistory.Populated = true;

        return true;
      });
  }

  private bool ReadWarrantRemailAddress()
  {
    entities.WarrantRemailAddress.Populated = false;

    return Read("ReadWarrantRemailAddress",
      (db, command) =>
      {
        db.SetInt32(
          command, "prqId", entities.PaymentRequest.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.WarrantRemailAddress.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.WarrantRemailAddress.Street1 = db.GetString(reader, 1);
        entities.WarrantRemailAddress.Street2 = db.GetNullableString(reader, 2);
        entities.WarrantRemailAddress.City = db.GetString(reader, 3);
        entities.WarrantRemailAddress.State = db.GetString(reader, 4);
        entities.WarrantRemailAddress.ZipCode4 =
          db.GetNullableString(reader, 5);
        entities.WarrantRemailAddress.ZipCode5 = db.GetString(reader, 6);
        entities.WarrantRemailAddress.ZipCode3 =
          db.GetNullableString(reader, 7);
        entities.WarrantRemailAddress.Name = db.GetNullableString(reader, 8);
        entities.WarrantRemailAddress.RemailDate = db.GetDate(reader, 9);
        entities.WarrantRemailAddress.CreatedBy = db.GetString(reader, 10);
        entities.WarrantRemailAddress.CreatedTimestamp =
          db.GetDateTime(reader, 11);
        entities.WarrantRemailAddress.PrqId = db.GetInt32(reader, 12);
        entities.WarrantRemailAddress.Populated = true;
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
      /// A value of DetailPaymentStatus.
      /// </summary>
      [JsonPropertyName("detailPaymentStatus")]
      public PaymentStatus DetailPaymentStatus
      {
        get => detailPaymentStatus ??= new();
        set => detailPaymentStatus = value;
      }

      /// <summary>
      /// A value of DetailPaymentStatusHistory.
      /// </summary>
      [JsonPropertyName("detailPaymentStatusHistory")]
      public PaymentStatusHistory DetailPaymentStatusHistory
      {
        get => detailPaymentStatusHistory ??= new();
        set => detailPaymentStatusHistory = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 20;

      private PaymentStatus detailPaymentStatus;
      private PaymentStatusHistory detailPaymentStatusHistory;
    }

    /// <summary>
    /// A value of Received.
    /// </summary>
    [JsonPropertyName("received")]
    public CsePersonsWorkSet Received
    {
      get => received ??= new();
      set => received = value;
    }

    /// <summary>
    /// A value of Payee.
    /// </summary>
    [JsonPropertyName("payee")]
    public CsePersonsWorkSet Payee
    {
      get => payee ??= new();
      set => payee = value;
    }

    /// <summary>
    /// A value of DesigPayee.
    /// </summary>
    [JsonPropertyName("desigPayee")]
    public CsePersonsWorkSet DesigPayee
    {
      get => desigPayee ??= new();
      set => desigPayee = value;
    }

    /// <summary>
    /// A value of WarrantNoPrompt.
    /// </summary>
    [JsonPropertyName("warrantNoPrompt")]
    public Common WarrantNoPrompt
    {
      get => warrantNoPrompt ??= new();
      set => warrantNoPrompt = value;
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
    /// A value of LocalFnWorkArea.
    /// </summary>
    [JsonPropertyName("localFnWorkArea")]
    public LocalFnWorkArea LocalFnWorkArea
    {
      get => localFnWorkArea ??= new();
      set => localFnWorkArea = value;
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
    /// A value of Selected.
    /// </summary>
    [JsonPropertyName("selected")]
    public PaymentRequest Selected
    {
      get => selected ??= new();
      set => selected = value;
    }

    /// <summary>
    /// A value of HiddenPaymentRequest.
    /// </summary>
    [JsonPropertyName("hiddenPaymentRequest")]
    public PaymentRequest HiddenPaymentRequest
    {
      get => hiddenPaymentRequest ??= new();
      set => hiddenPaymentRequest = value;
    }

    private CsePersonsWorkSet received;
    private CsePersonsWorkSet payee;
    private CsePersonsWorkSet desigPayee;
    private Common warrantNoPrompt;
    private PaymentRequest paymentRequest;
    private LocalFnWorkArea localFnWorkArea;
    private Array<ImportGroup> import1;
    private NextTranInfo hiddenNextTranInfo;
    private Standard standard;
    private PaymentRequest selected;
    private PaymentRequest hiddenPaymentRequest;
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
      /// A value of DetailPaymentStatus.
      /// </summary>
      [JsonPropertyName("detailPaymentStatus")]
      public PaymentStatus DetailPaymentStatus
      {
        get => detailPaymentStatus ??= new();
        set => detailPaymentStatus = value;
      }

      /// <summary>
      /// A value of DetailPaymentStatusHistory.
      /// </summary>
      [JsonPropertyName("detailPaymentStatusHistory")]
      public PaymentStatusHistory DetailPaymentStatusHistory
      {
        get => detailPaymentStatusHistory ??= new();
        set => detailPaymentStatusHistory = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 20;

      private PaymentStatus detailPaymentStatus;
      private PaymentStatusHistory detailPaymentStatusHistory;
    }

    /// <summary>
    /// A value of PassThruFlowPaymentRequest.
    /// </summary>
    [JsonPropertyName("passThruFlowPaymentRequest")]
    public PaymentRequest PassThruFlowPaymentRequest
    {
      get => passThruFlowPaymentRequest ??= new();
      set => passThruFlowPaymentRequest = value;
    }

    /// <summary>
    /// A value of PassThruFlowCsePerson.
    /// </summary>
    [JsonPropertyName("passThruFlowCsePerson")]
    public CsePerson PassThruFlowCsePerson
    {
      get => passThruFlowCsePerson ??= new();
      set => passThruFlowCsePerson = value;
    }

    /// <summary>
    /// A value of Payee.
    /// </summary>
    [JsonPropertyName("payee")]
    public CsePersonsWorkSet Payee
    {
      get => payee ??= new();
      set => payee = value;
    }

    /// <summary>
    /// A value of DesignatedPayee.
    /// </summary>
    [JsonPropertyName("designatedPayee")]
    public CsePersonsWorkSet DesignatedPayee
    {
      get => designatedPayee ??= new();
      set => designatedPayee = value;
    }

    /// <summary>
    /// A value of WarrantNumberPromptIn.
    /// </summary>
    [JsonPropertyName("warrantNumberPromptIn")]
    public Common WarrantNumberPromptIn
    {
      get => warrantNumberPromptIn ??= new();
      set => warrantNumberPromptIn = value;
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
    /// A value of LocalFnWorkArea.
    /// </summary>
    [JsonPropertyName("localFnWorkArea")]
    public LocalFnWorkArea LocalFnWorkArea
    {
      get => localFnWorkArea ??= new();
      set => localFnWorkArea = value;
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
    /// A value of DuplicateWarrantsExist.
    /// </summary>
    [JsonPropertyName("duplicateWarrantsExist")]
    public Common DuplicateWarrantsExist
    {
      get => duplicateWarrantsExist ??= new();
      set => duplicateWarrantsExist = value;
    }

    /// <summary>
    /// A value of HiddenPaymentRequest.
    /// </summary>
    [JsonPropertyName("hiddenPaymentRequest")]
    public PaymentRequest HiddenPaymentRequest
    {
      get => hiddenPaymentRequest ??= new();
      set => hiddenPaymentRequest = value;
    }

    private PaymentRequest passThruFlowPaymentRequest;
    private CsePerson passThruFlowCsePerson;
    private CsePersonsWorkSet payee;
    private CsePersonsWorkSet designatedPayee;
    private Common warrantNumberPromptIn;
    private PaymentRequest paymentRequest;
    private LocalFnWorkArea localFnWorkArea;
    private Array<ExportGroup> export1;
    private NextTranInfo hiddenNextTranInfo;
    private Standard standard;
    private Common duplicateWarrantsExist;
    private PaymentRequest hiddenPaymentRequest;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of NbrOfWarrants.
    /// </summary>
    [JsonPropertyName("nbrOfWarrants")]
    public Common NbrOfWarrants
    {
      get => nbrOfWarrants ??= new();
      set => nbrOfWarrants = value;
    }

    /// <summary>
    /// A value of FoundFlagIn.
    /// </summary>
    [JsonPropertyName("foundFlagIn")]
    public Common FoundFlagIn
    {
      get => foundFlagIn ??= new();
      set => foundFlagIn = value;
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
    /// A value of TextWorkArea.
    /// </summary>
    [JsonPropertyName("textWorkArea")]
    public TextWorkArea TextWorkArea
    {
      get => textWorkArea ??= new();
      set => textWorkArea = value;
    }

    private Common nbrOfWarrants;
    private Common foundFlagIn;
    private CsePersonsWorkSet csePersonsWorkSet;
    private TextWorkArea textWorkArea;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of CsePersonAddress.
    /// </summary>
    [JsonPropertyName("csePersonAddress")]
    public CsePersonAddress CsePersonAddress
    {
      get => csePersonAddress ??= new();
      set => csePersonAddress = value;
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
    /// A value of WarrantRemailAddress.
    /// </summary>
    [JsonPropertyName("warrantRemailAddress")]
    public WarrantRemailAddress WarrantRemailAddress
    {
      get => warrantRemailAddress ??= new();
      set => warrantRemailAddress = value;
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
    /// A value of PaymentStatus.
    /// </summary>
    [JsonPropertyName("paymentStatus")]
    public PaymentStatus PaymentStatus
    {
      get => paymentStatus ??= new();
      set => paymentStatus = value;
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

    private CsePersonAddress csePersonAddress;
    private CsePerson csePerson;
    private WarrantRemailAddress warrantRemailAddress;
    private PaymentRequest paymentRequest;
    private PaymentStatus paymentStatus;
    private PaymentStatusHistory paymentStatusHistory;
  }
#endregion
}
