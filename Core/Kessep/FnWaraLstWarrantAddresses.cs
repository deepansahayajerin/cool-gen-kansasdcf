// Program: FN_WARA_LST_WARRANT_ADDRESSES, ID: 371863889, model: 746.
// Short name: SWEWARAP
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
/// A program: FN_WARA_LST_WARRANT_ADDRESSES.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class FnWaraLstWarrantAddresses: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_WARA_LST_WARRANT_ADDRESSES program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnWaraLstWarrantAddresses(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnWaraLstWarrantAddresses.
  /// </summary>
  public FnWaraLstWarrantAddresses(IContext context, Import import,
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
    // **********************************************************************************
    // 12/16/96 - R. Marchman - Add new security/next tran
    // 12/05/98 - K. Doshi - Amend screen to meet requirements
    // 04/05/00 - C. Scroggins - Added security check for family violence
    // 03/06/01 - K. Doshi - PR# 111410.  Add family violence check for DP.
    // 05/15/01 - K. Doshi - WR# 285.  Cater for Duplicate warrant #s.
    // **********************************************************************************
    if (Equal(global.Command, "CLEAR"))
    {
      ExitState = "ACO_NI0000_CLEAR_SUCCESSFUL";

      return;
    }

    if (Equal(global.Command, "RETLINK"))
    {
      return;
    }

    if (Equal(global.Command, "EXIT"))
    {
      ExitState = "ECO_LNK_RETURN_TO_MENU";

      return;
    }

    ExitState = "ACO_NN0000_ALL_OK";

    // **** begin group A ****
    export.HiddenNextTranInfo.Assign(import.HiddenNextTranInfo);
    MovePaymentRequest2(import.HiddenPaymentRequest, export.HiddenPaymentRequest);
      

    // **** end   group A ****
    if (!Equal(global.Command, "DISPLAY"))
    {
      // ------------------------------------------------------
      // KD - 12/5/98
      // Add move statement for local_fn_work_area
      // -----------------------------------------------------
      export.OrgAddr.Assign(import.OrgAddr);
      MovePaymentRequest1(import.PaymentRequest, export.PaymentRequest);
      MoveCsePersonsWorkSet(import.Payee, export.Payee);
      export.DesigPayee.FormattedName = import.DesigPayee.FormattedName;
      export.WorkAddress.Name = import.WorkAddress.Name;
      export.LocalWorkAddr.Assign(import.LocalWorkAddr);

      export.Group.Index = 0;
      export.Group.Clear();

      for(import.Group.Index = 0; import.Group.Index < import.Group.Count; ++
        import.Group.Index)
      {
        if (export.Group.IsFull)
        {
          break;
        }

        export.Group.Update.GrDetailAddr1.
          Assign(import.Group.Item.GrDetailAddr1);
        export.Group.Update.GrDetAddr2.CityStZip =
          import.Group.Item.GrDetAddr2.CityStZip;
        export.Group.Next();
      }

      export.PassThruFlowPaymentRequest.Assign(import.PaymentRequest);
      export.PassThruFlowCsePerson.Number =
        import.PaymentRequest.CsePersonNumber ?? Spaces(10);
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
      // ****
      export.HiddenNextTranInfo.CsePersonNumberObligee =
        import.PaymentRequest.CsePersonNumber ?? "";
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

    if (Equal(global.Command, "XXNEXTXX"))
    {
      // this is where you set your export value to the export hidden next tran 
      // values if the user is comming into this procedure on a next tran
      // action.
      UseScCabNextTranGet();
      global.Command = "DISPLAY";

      return;
    }

    if (Equal(global.Command, "XXFMMENU"))
    {
      // ****
      // this is where you set your command to do whatever is necessary to do on
      // a flow from the menu, maybe just escape....
      // ****
      // You should get this information from the Dialog Flow Diagram.  It is 
      // the SEND CMD on the propertis for a Transfer from one  procedure to
      // another.
      // *** the statement would read COMMAND IS display   *****
      global.Command = "DISPLAY";

      // *** if the dialog flow property was display first, just add an escape 
      // completely out of the procedure  ****
      return;
    }

    // **** end   group B ****
    // **** begin group C ****
    // to validate action level security
    if (Equal(global.Command, "PACC") || Equal(global.Command, "WARR") || Equal
      (global.Command, "WAST") || Equal(global.Command, "WDTL") || Equal
      (global.Command, "WHST"))
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
        return;
      }
    }

    // **** end   group C ****
    if (!IsEmpty(import.PaymentRequest.Number))
    {
      // ------------------------------------------------------------
      // KD - 12/4/98
      // The statement below was setting the local view to itself. It should be 
      // set to import pay-req number.
      // -----------------------------------------------------------
      local.TextWorkArea.Text10 = import.PaymentRequest.Number ?? Spaces(10);
      UseEabPadLeftWithZeros();
      export.PaymentRequest.Number = Substring(local.TextWorkArea.Text10, 2, 9);
    }

    switch(TrimEnd(global.Command))
    {
      case "DISPLAY":
        if (import.PaymentRequest.SystemGeneratedIdentifier == 0 && IsEmpty
          (import.PaymentRequest.Number))
        {
          ExitState = "FN0000_INVALID_INPUT_MSG";

          var field = GetField(export.PaymentRequest, "number");

          field.Error = true;

          return;
        }

        if (!IsEmpty(import.WarrantNoPrompt.SelectChar))
        {
          ExitState = "ACO_NE0000_PROMPT_INVALID_W_FNCT";

          var field = GetField(export.WarrantNoPromptIn, "selectChar");

          field.Error = true;

          return;
        }

        if (import.PaymentRequest.SystemGeneratedIdentifier == 0 || !
          Equal(import.HiddenPaymentRequest.Number, import.PaymentRequest.Number)
          && !IsEmpty(import.HiddenPaymentRequest.Number))
        {
          // ---------------------------------------------
          // 05/15/01 - K. Doshi - WR# 285
          // Cater for Duplicate warrant #s.
          // ---------------------------------------------
          ReadPaymentRequest3();

          if (local.NbrOfWarrants.Count > 1)
          {
            // ------------------------------------------------------------
            // 05/15/01 - K. Doshi - WR# 285.
            // Duplicate exists. Only send warrant # and duplicate_warrant
            // flag via dialog flow.
            // ------------------------------------------------------------
            export.Payee.Number = "";
            export.Payee.FormattedName = "";
            export.PassThruFlowPaymentRequest.Assign(import.PaymentRequest);
            export.HiddenPaymentRequest.Number = "";
            export.DuplicateWarrants.Flag = "Y";
            ExitState = "ECO_LNK_TO_LST_WARRANTS";

            return;
          }

          if (!ReadPaymentRequest1())
          {
            ExitState = "FN0000_WARRANT_NF";

            // ---------------------------------------------
            // KD - 12/4/98
            // Highlight screen field in error
            // ----------------------------------------------
            var field = GetField(export.PaymentRequest, "number");

            field.Error = true;

            return;
          }
        }
        else if (!ReadPaymentRequest2())
        {
          ExitState = "FN0000_PASSED_PAYMENT_REQ_NF";

          return;
        }

        MovePaymentRequest1(entities.PaymentRequest, export.PaymentRequest);
        MovePaymentRequest2(entities.PaymentRequest, export.HiddenPaymentRequest);
          

        // <<< RBM   02/04/1998  If the Payee and the Designated Payee are the 
        // same, then make the Designated Payee Spaces.
        if (Equal(export.PaymentRequest.DesignatedPayeeCsePersonNo,
          export.PaymentRequest.CsePersonNumber))
        {
          export.PaymentRequest.DesignatedPayeeCsePersonNo = "";
        }

        // ------------------------------------------------------------------
        // 03/06/01 - K. Doshi - PR# 111410.  Add family violence check for DP.
        // ------------------------------------------------------------------
        if (!IsEmpty(export.PaymentRequest.DesignatedPayeeCsePersonNo))
        {
          local.CsePersonsWorkSet.Number =
            export.PaymentRequest.DesignatedPayeeCsePersonNo ?? Spaces(10);
          UseScSecurityCheckForFv();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            var field =
              GetField(export.PaymentRequest, "designatedPayeeCsePersonNo");

            field.Error = true;

            return;
          }
        }

        local.CsePersonsWorkSet.Number =
          export.PaymentRequest.CsePersonNumber ?? Spaces(10);
        UseScSecurityCheckForFv();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          var field = GetField(export.PaymentRequest, "csePersonNumber");

          field.Error = true;

          return;
        }

        if (IsEmpty(import.ReceivedPayee.FormattedName))
        {
          UseSiReadCsePerson();

          if (IsExitState("CSE_PERSON_NF"))
          {
            export.PaymentRequest.CsePersonNumber = "";
            export.PaymentRequest.DesignatedPayeeCsePersonNo = "";
            export.PaymentRequest.SystemGeneratedIdentifier = 0;
            ExitState = "PAYEE_CSE_PERSON_NF";

            return;
          }

          MoveCsePersonsWorkSet(local.CsePersonsWorkSet, export.Payee);
        }
        else
        {
          MoveCsePersonsWorkSet(import.ReceivedPayee, export.Payee);
        }

        local.CsePersonsWorkSet.FormattedName = "";

        if (!IsEmpty(export.PaymentRequest.DesignatedPayeeCsePersonNo))
        {
          local.CsePersonsWorkSet.Number =
            export.PaymentRequest.DesignatedPayeeCsePersonNo ?? Spaces(10);
          UseSiReadCsePerson();

          if (IsExitState("CSE_PERSON_NF"))
          {
            export.PaymentRequest.CsePersonNumber = "";
            export.PaymentRequest.DesignatedPayeeCsePersonNo = "";
            export.PaymentRequest.SystemGeneratedIdentifier = 0;
            ExitState = "DESIGNATED_PAYEE_CSE_PERSON_NF";

            return;
          }

          MoveCsePersonsWorkSet(local.CsePersonsWorkSet, export.DesigPayee);
          export.WorkAddress.Name = export.DesigPayee.FormattedName;
        }
        else
        {
          local.CsePersonsWorkSet.Number =
            export.PaymentRequest.CsePersonNumber ?? Spaces(10);
          export.WorkAddress.Name = export.Payee.FormattedName;
        }

        // ***** Get the Original_Address_Mailed_to *****
        //       The remail address with the lowest remail date is the original 
        // address
        //       to which the Warrant was initially mailed. This address is set 
        // by the
        //       SWEFB656 batch procedure
        if (ReadWarrantRemailAddress1())
        {
          export.OrgAddr.Name = entities.WarrantRemailAddress.Name ?? Spaces
            (33);
          export.OrgAddr.AddressLine1 = entities.WarrantRemailAddress.Street1;
          export.OrgAddr.AddressLine2 =
            entities.WarrantRemailAddress.Street2 ?? Spaces(20);
          export.OrgAddr.City = entities.WarrantRemailAddress.City;
          export.OrgAddr.State = entities.WarrantRemailAddress.State;
          export.OrgAddr.Zip5 = entities.WarrantRemailAddress.ZipCode5;
          export.OrgAddr.Zip4 = entities.WarrantRemailAddress.ZipCode4 ?? Spaces
            (3);
          export.OrgAddr.Zip3 = entities.WarrantRemailAddress.ZipCode3 ?? Spaces
            (3);
        }
        else
        {
          export.OrgAddr.Name = "Original Mailing Address";
          export.OrgAddr.AddressLine1 = "was not recorded";
        }

        // ***** Get the Remail-Addresses for the warrant *****
        // ------------------------------------------------------
        // KD - 12/17/98
        // Sort on Desc created_ts instead of Desc remail_date
        // ------------------------------------------------------
        export.Group.Index = 0;
        export.Group.Clear();

        foreach(var item in ReadWarrantRemailAddress2())
        {
          local.RemlAddressFound.Flag = "Y";
          MoveWarrantRemailAddress(entities.WarrantRemailAddress,
            export.Group.Update.GrDetailAddr1);
          export.Group.Update.GrDetAddr2.CityStZip =
            TrimEnd(entities.WarrantRemailAddress.City) + " , " + entities
            .WarrantRemailAddress.State + " , " + entities
            .WarrantRemailAddress.ZipCode5 + " " + entities
            .WarrantRemailAddress.ZipCode4 + " " + entities
            .WarrantRemailAddress.ZipCode3;
          export.Group.Next();
        }

        if (AsChar(local.RemlAddressFound.Flag) != 'Y')
        {
          ExitState = "FN0000_WARRANT_REMAIL_ADDRESS_NF";
        }

        if (export.Group.IsFull)
        {
          ExitState = "ACO_NI0000_LST_RETURNED_FULL";
        }

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          ExitState = "ACO_NI0000_DISPLAY_SUCCESSFUL";
        }

        break;
      case "EXIT":
        break;
      case "LIST":
        if (AsChar(import.WarrantNoPrompt.SelectChar) != 'S')
        {
          var field = GetField(export.WarrantNoPromptIn, "selectChar");

          field.Error = true;

          ExitState = "ACO_NE0000_INVALID_SELECT_CODE";
        }
        else
        {
          ExitState = "ECO_LNK_TO_LST_WARRANTS";
        }

        break;
      case "PREV":
        ExitState = "ACO_NI0000_TOP_OF_LIST";

        break;
      case "NEXT":
        ExitState = "ACO_NI0000_BOTTOM_OF_LIST";

        break;
      case "RETURN":
        ExitState = "ACO_NE0000_RETURN";

        break;
      case "SIGNOFF":
        // *** Set Exit_State to SIGN-OFF Procedure ***
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
      case "WHST":
        ExitState = "ECO_LNK_TO_WHST";

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

  private static void MovePaymentRequest1(PaymentRequest source,
    PaymentRequest target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Type1 = source.Type1;
    target.CsePersonNumber = source.CsePersonNumber;
    target.DesignatedPayeeCsePersonNo = source.DesignatedPayeeCsePersonNo;
    target.Number = source.Number;
    target.PrintDate = source.PrintDate;
  }

  private static void MovePaymentRequest2(PaymentRequest source,
    PaymentRequest target)
  {
    target.Type1 = source.Type1;
    target.Number = source.Number;
  }

  private static void MoveWarrantRemailAddress(WarrantRemailAddress source,
    WarrantRemailAddress target)
  {
    target.RemailDate = source.RemailDate;
    target.CreatedBy = source.CreatedBy;
    target.Name = source.Name;
    target.Street1 = source.Street1;
    target.Street2 = source.Street2;
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

    useImport.CsePersonsWorkSet.Number = import.Payee.Number;
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
        entities.PaymentRequest.DesignatedPayeeCsePersonNo =
          db.GetNullableString(reader, 1);
        entities.PaymentRequest.CsePersonNumber =
          db.GetNullableString(reader, 2);
        entities.PaymentRequest.Number = db.GetNullableString(reader, 3);
        entities.PaymentRequest.PrintDate = db.GetNullableDate(reader, 4);
        entities.PaymentRequest.Type1 = db.GetString(reader, 5);
        entities.PaymentRequest.PrqRGeneratedId =
          db.GetNullableInt32(reader, 6);
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
          import.PaymentRequest.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.PaymentRequest.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.PaymentRequest.DesignatedPayeeCsePersonNo =
          db.GetNullableString(reader, 1);
        entities.PaymentRequest.CsePersonNumber =
          db.GetNullableString(reader, 2);
        entities.PaymentRequest.Number = db.GetNullableString(reader, 3);
        entities.PaymentRequest.PrintDate = db.GetNullableDate(reader, 4);
        entities.PaymentRequest.Type1 = db.GetString(reader, 5);
        entities.PaymentRequest.PrqRGeneratedId =
          db.GetNullableInt32(reader, 6);
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

  private bool ReadWarrantRemailAddress1()
  {
    entities.WarrantRemailAddress.Populated = false;

    return Read("ReadWarrantRemailAddress1",
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
        entities.WarrantRemailAddress.LastUpdatedTmst =
          db.GetNullableDateTime(reader, 12);
        entities.WarrantRemailAddress.PrqId = db.GetInt32(reader, 13);
        entities.WarrantRemailAddress.Populated = true;
      });
  }

  private IEnumerable<bool> ReadWarrantRemailAddress2()
  {
    return ReadEach("ReadWarrantRemailAddress2",
      (db, command) =>
      {
        db.SetInt32(
          command, "prqId", entities.PaymentRequest.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        if (export.Group.IsFull)
        {
          return false;
        }

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
        entities.WarrantRemailAddress.LastUpdatedTmst =
          db.GetNullableDateTime(reader, 12);
        entities.WarrantRemailAddress.PrqId = db.GetInt32(reader, 13);
        entities.WarrantRemailAddress.Populated = true;

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
    /// <summary>A GroupGroup group.</summary>
    [Serializable]
    public class GroupGroup
    {
      /// <summary>
      /// A value of GrDetailAddr1.
      /// </summary>
      [JsonPropertyName("grDetailAddr1")]
      public WarrantRemailAddress GrDetailAddr1
      {
        get => grDetailAddr1 ??= new();
        set => grDetailAddr1 = value;
      }

      /// <summary>
      /// A value of GrDetAddr2.
      /// </summary>
      [JsonPropertyName("grDetAddr2")]
      public WorkAddress GrDetAddr2
      {
        get => grDetAddr2 ??= new();
        set => grDetAddr2 = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 20;

      private WarrantRemailAddress grDetailAddr1;
      private WorkAddress grDetAddr2;
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

    /// <summary>
    /// A value of OrgAddr.
    /// </summary>
    [JsonPropertyName("orgAddr")]
    public LocalFnWorkArea OrgAddr
    {
      get => orgAddr ??= new();
      set => orgAddr = value;
    }

    /// <summary>
    /// A value of ReceivedPayee.
    /// </summary>
    [JsonPropertyName("receivedPayee")]
    public CsePersonsWorkSet ReceivedPayee
    {
      get => receivedPayee ??= new();
      set => receivedPayee = value;
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
    /// A value of WorkAddress.
    /// </summary>
    [JsonPropertyName("workAddress")]
    public WorkAddress WorkAddress
    {
      get => workAddress ??= new();
      set => workAddress = value;
    }

    /// <summary>
    /// A value of LocalWorkAddr.
    /// </summary>
    [JsonPropertyName("localWorkAddr")]
    public LocalWorkAddr LocalWorkAddr
    {
      get => localWorkAddr ??= new();
      set => localWorkAddr = value;
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
    /// A value of Standard.
    /// </summary>
    [JsonPropertyName("standard")]
    public Standard Standard
    {
      get => standard ??= new();
      set => standard = value;
    }

    private PaymentRequest hiddenPaymentRequest;
    private LocalFnWorkArea orgAddr;
    private CsePersonsWorkSet receivedPayee;
    private Common warrantNoPrompt;
    private CsePersonsWorkSet payee;
    private CsePersonsWorkSet desigPayee;
    private WorkAddress workAddress;
    private LocalWorkAddr localWorkAddr;
    private PaymentRequest paymentRequest;
    private Array<GroupGroup> group;
    private NextTranInfo hiddenNextTranInfo;
    private Standard standard;
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
      /// A value of GrDetailAddr1.
      /// </summary>
      [JsonPropertyName("grDetailAddr1")]
      public WarrantRemailAddress GrDetailAddr1
      {
        get => grDetailAddr1 ??= new();
        set => grDetailAddr1 = value;
      }

      /// <summary>
      /// A value of GrDetAddr2.
      /// </summary>
      [JsonPropertyName("grDetAddr2")]
      public WorkAddress GrDetAddr2
      {
        get => grDetAddr2 ??= new();
        set => grDetAddr2 = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 20;

      private WarrantRemailAddress grDetailAddr1;
      private WorkAddress grDetAddr2;
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

    /// <summary>
    /// A value of DuplicateWarrants.
    /// </summary>
    [JsonPropertyName("duplicateWarrants")]
    public Common DuplicateWarrants
    {
      get => duplicateWarrants ??= new();
      set => duplicateWarrants = value;
    }

    /// <summary>
    /// A value of OrgAddr.
    /// </summary>
    [JsonPropertyName("orgAddr")]
    public LocalFnWorkArea OrgAddr
    {
      get => orgAddr ??= new();
      set => orgAddr = value;
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
    /// A value of WarrantNoPromptIn.
    /// </summary>
    [JsonPropertyName("warrantNoPromptIn")]
    public Common WarrantNoPromptIn
    {
      get => warrantNoPromptIn ??= new();
      set => warrantNoPromptIn = value;
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
    /// A value of WorkAddress.
    /// </summary>
    [JsonPropertyName("workAddress")]
    public WorkAddress WorkAddress
    {
      get => workAddress ??= new();
      set => workAddress = value;
    }

    /// <summary>
    /// A value of LocalWorkAddr.
    /// </summary>
    [JsonPropertyName("localWorkAddr")]
    public LocalWorkAddr LocalWorkAddr
    {
      get => localWorkAddr ??= new();
      set => localWorkAddr = value;
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
    /// A value of Standard.
    /// </summary>
    [JsonPropertyName("standard")]
    public Standard Standard
    {
      get => standard ??= new();
      set => standard = value;
    }

    private PaymentRequest hiddenPaymentRequest;
    private Common duplicateWarrants;
    private LocalFnWorkArea orgAddr;
    private PaymentRequest passThruFlowPaymentRequest;
    private CsePerson passThruFlowCsePerson;
    private Common warrantNoPromptIn;
    private CsePersonsWorkSet payee;
    private CsePersonsWorkSet desigPayee;
    private WorkAddress workAddress;
    private LocalWorkAddr localWorkAddr;
    private PaymentRequest paymentRequest;
    private Array<GroupGroup> group;
    private NextTranInfo hiddenNextTranInfo;
    private Standard standard;
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
    /// A value of OriginalAddrMailingDate.
    /// </summary>
    [JsonPropertyName("originalAddrMailingDate")]
    public WarrantRemailAddress OriginalAddrMailingDate
    {
      get => originalAddrMailingDate ??= new();
      set => originalAddrMailingDate = value;
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
    /// A value of RemlAddressFound.
    /// </summary>
    [JsonPropertyName("remlAddressFound")]
    public Common RemlAddressFound
    {
      get => remlAddressFound ??= new();
      set => remlAddressFound = value;
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
    private WarrantRemailAddress originalAddrMailingDate;
    private CsePerson csePerson;
    private Common remlAddressFound;
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

    private CsePersonAddress csePersonAddress;
    private CsePerson csePerson;
    private WarrantRemailAddress warrantRemailAddress;
    private PaymentRequest paymentRequest;
  }
#endregion
}
