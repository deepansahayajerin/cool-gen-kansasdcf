// Program: FN_DXUA_DISBURSE_EXCESS_URA, ID: 372550513, model: 746.
// Short name: SWEDXUAP
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Security1;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_DXUA_DISBURSE_EXCESS_URA.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class FnDxuaDisburseExcessUra: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_DXUA_DISBURSE_EXCESS_URA program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnDxuaDisburseExcessUra(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnDxuaDisburseExcessUra.
  /// </summary>
  public FnDxuaDisburseExcessUra(IContext context, Import import, Export export):
    
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // **********************************
    // Initial development
    // SWSRKXD - 3/26/99
    // **********************************
    ExitState = "ACO_NN0000_ALL_OK";

    switch(TrimEnd(global.Command))
    {
      case "CLEAR":
        ExitState = "ACO_NI0000_CLEAR_SUCCESSFUL";

        return;
      case "EXIT":
        ExitState = "ECO_LNK_RETURN_TO_MENU";

        return;
      default:
        break;
    }

    // **** begin group A ****
    export.HiddenNextTranInfo.Assign(import.HiddenNextTranInfo);

    // **** end   group A ****
    if (Equal(global.Command, "DISPLAY"))
    {
      export.UraExcessCollection.SequenceNumber =
        import.UraExcessCollection.SequenceNumber;
    }
    else
    {
      // ==>MOVE IMPORTS TO EXPORTS
      MoveCsePersonsWorkSet(import.CsePersonsWorkSet, export.CsePersonsWorkSet);
      export.UraExcessCollection.Assign(import.UraExcessCollection);
      export.CsePerson.Number = import.CsePerson.Number;
      export.CsePersonAddress.Assign(import.CsePersonAddress);
      export.CrdCrComboNo.CrdCrCombo = import.CrdCrComboNo.CrdCrCombo;
      export.Collection.Assign(import.Collection);
      export.Entered.DisbAmount = import.Entered.DisbAmount;
      export.Description.Text30 = import.Description.Text30;

      // ==>MOVE HIDDEN VIEWS
      export.HiddenUraExcessCollection.SequenceNumber =
        import.HiddenUraExcessCollection.SequenceNumber;
      export.HiddenImHousehold.ZdelType = import.HiddenImHousehold.ZdelType;
    }

    // *** If RETURNed from a LIST screen, Escape to simulate DISPLAY FIRST ***
    if (Equal(global.Command, "RETCDVL"))
    {
      var field = GetField(export.UraExcessCollection, "sequenceNumber");

      field.Highlighting = Highlighting.Underscore;
      field.Protected = false;
      field.Focused = true;

      return;
    }

    // **** begin group B ****
    export.Standard.NextTransaction = import.Standard.NextTransaction;

    // if the next tran info is not equal to spaces, this implies the user 
    // requested a next tran action. now validate
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
      // ****
      // this is where you set your export value to the export hidden next tran 
      // values if the user is comming into this procedure on a next tran
      // action.
      // **** Since the nexttran_info does not carry the Warrant_number, no 
      // export view can be set from the nexttran_info.
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
    // -------------------------------------------------------
    // No flows from this screen. Always validate action level
    // security
    // -------------------------------------------------------
    UseScCabTestSecurity();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    // **** end   group C ****
    // ***** Main CASE OF COMMAND structure *****
    switch(TrimEnd(global.Command))
    {
      case "DISPLAY":
        if (export.UraExcessCollection.SequenceNumber == 0)
        {
          var field = GetField(export.UraExcessCollection, "sequenceNumber");

          field.Error = true;

          ExitState = "ACO_NI0000_ENTER_REQUIRED_DATA";

          return;
        }

        if (!ReadUraExcessCollection())
        {
          var field = GetField(export.UraExcessCollection, "sequenceNumber");

          field.Error = true;

          ExitState = "FN0000_URA_EXCESS_COLLECTION_NF";

          return;
        }

        switch(AsChar(entities.UraExcessCollection.Action))
        {
          case 'X':
            export.Description.Text30 = "Household URA";

            break;
          case 'Y':
            export.Description.Text30 = "Current Support";

            break;
          case 'Z':
            export.Description.Text30 = "Current Support";

            break;
          case 'D':
            export.Description.Text30 = "Already Disb";

            break;
          case 'E':
            export.Description.Text30 = "Already Disb";

            break;
          default:
            export.Description.Text30 = "System use only";

            break;
        }

        // --------------------------------------------------------
        // 8/5/99 - SWSRKXD
        // IDCR 524 requested a UNIQUE index for collection on
        // SGI but this index never got implemented! As a result, we
        // may end up with duplicate SGIs in collection. Hence we
        // need to further qualify the read using the supported person
        // (i.e. initiating_cse_person). This may not be 100% accurate
        // but should be very very close! It has been discussed and
        // approved by the URA designer/coordinator.
        // --------------------------------------------------------
        if (!ReadCollection())
        {
          ExitState = "FN0000_COLLECTION_NF";

          return;
        }

        if (!ReadCashReceiptCashReceiptDetail())
        {
          ExitState = "FN0000_CASH_RECEIPT_NF";

          return;
        }

        MoveUraExcessCollection2(entities.UraExcessCollection,
          export.UraExcessCollection);
        MoveCollection(entities.Collection, export.Collection);

        // --------------------------------------------------------
        // As per Disbursement team meeting on 4/28, display
        // Reference Number which is a concatenation of cash_receipt
        // and cash_receipt_detail.
        // --------------------------------------------------------
        UseFnAbConcatCrAndCrd();

        // --------------------------------------------------------
        // Get Payee #
        // --------------------------------------------------------
        // --------------------------------------------------------
        // SWSRKXD - 6/24/99
        // Payee is the AR and is determined via the initiating_cse_person.
        // For Foster Case display a message indicating that excess
        // cannot be disbursed. This follows a discussion with SMEs
        // Kim W and Paula T on 7/24/99, Thursday 2pm.
        // --------------------------------------------------------
        UseOeGetArForUraExcessColl();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          var field = GetField(export.UraExcessCollection, "sequenceNumber");

          field.Error = true;

          return;
        }

        // --------------------------------------------------------
        // Get Payee Name
        // --------------------------------------------------------
        export.CsePersonsWorkSet.Number = export.CsePerson.Number;
        UseSiReadCsePerson();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }

        // ---------------------------------
        // Get Mailing Address
        // ---------------------------------
        UseSiGetCsePersonMailingAddr();

        // --------------------------------------------------------
        // No exit states set by CAB - check if address was returned
        // --------------------------------------------------------
        if (Equal(export.CsePersonAddress.Identifier, local.Null1.Identifier))
        {
          ExitState = "CSE_PERSON_ADDRESS_NF";

          return;
        }

        // --------------------------------------------------------
        // Populate hidden views
        // --------------------------------------------------------
        export.HiddenUraExcessCollection.SequenceNumber =
          export.UraExcessCollection.SequenceNumber;

        // --------------------------------------------------------
        // Display warning message if action is not X, Y or Z
        // --------------------------------------------------------
        if (AsChar(export.UraExcessCollection.Action) != 'X' && AsChar
          (export.UraExcessCollection.Action) != 'Y' && AsChar
          (export.UraExcessCollection.Action) != 'Z')
        {
          ExitState = "FN0000_ACTION_NE_XYZ_DISP_SUCCES";

          return;
        }

        // --------------------------------------------------------
        // SWSRKXD - 6/25/99
        // For Foster Case display a message indicating that excess
        // cannot be disbursed. This follows a discussion with SMEs
        // Kim W and Paula T on 6/24/99, Thursday 2pm.
        // --------------------------------------------------------
        if (Equal(export.HiddenImHousehold.ZdelType, "FC"))
        {
          ExitState = "FN0000_CANNOT_DISB_EXCESS_TO_FC";

          return;
        }

        ExitState = "ACO_NI0000_DISPLAY_SUCCESSFUL";

        break;
      case "DISBURSE":
        // --------------------------------------------------
        // Check to make sure display has been performed before Disburse
        // --------------------------------------------------
        if (export.UraExcessCollection.SequenceNumber != export
          .HiddenUraExcessCollection.SequenceNumber)
        {
          ExitState = "FN0000_REDISPLAY_B4_DISBURSING";

          return;
        }

        if (AsChar(export.UraExcessCollection.Action) != 'X' && AsChar
          (export.UraExcessCollection.Action) != 'Y' && AsChar
          (export.UraExcessCollection.Action) != 'Z')
        {
          ExitState = "FN0000_ACTION_NE_X_Y_OR_Z";

          return;
        }

        if (export.Entered.DisbAmount <= 0)
        {
          var field = GetField(export.Entered, "disbAmount");

          field.Error = true;

          ExitState = "ACO_NI0000_ENTER_REQUIRED_DATA";

          return;
        }

        // --------------------------------------------------------
        // SWSRKXD - 7/2/99
        // Excess cannot be disbursed in a Foster Case,
        // --------------------------------------------------------
        if (Equal(export.HiddenImHousehold.ZdelType, "FC"))
        {
          ExitState = "FN0000_DISB_NOT_ALLOWED_FOR_FC";

          return;
        }

        if (export.Entered.DisbAmount > export.UraExcessCollection.Amount)
        {
          var field = GetField(export.Entered, "disbAmount");

          field.Error = true;

          ExitState = "FN0000_DISB_AMT_GRTR_EXCESS_AMT";

          return;
        }

        if (export.Entered.DisbAmount > export.Collection.Amount)
        {
          var field = GetField(export.Entered, "disbAmount");

          field.Error = true;

          ExitState = "FN0000_DISB_AMT_GRTR_COLL_AMT";

          return;
        }

        if (AsChar(export.Collection.AdjustedInd) != 'N')
        {
          var field = GetField(export.Entered, "disbAmount");

          field.Error = true;

          ExitState = "FN0000_COLL_ALREADY_ADJUSTED";

          return;
        }

        // -------------------------------------------------
        // Passed all validation. Update the DB
        // -------------------------------------------------
        local.UraExcessCollection.SequenceNumber =
          export.UraExcessCollection.SequenceNumber;
        local.UraExcessCollection.Amount = import.Entered.DisbAmount;
        UseFnDisburseExcessUra();

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }

        // -------------------------------------------------
        // Refresh screen fields from database
        // -------------------------------------------------
        if (!ReadUraExcessCollection())
        {
          var field = GetField(export.UraExcessCollection, "sequenceNumber");

          field.Error = true;

          ExitState = "FN0000_URA_EXCESS_COLLECTION_NF";

          return;
        }

        MoveUraExcessCollection2(entities.UraExcessCollection,
          export.UraExcessCollection);

        switch(AsChar(export.UraExcessCollection.Action))
        {
          case 'X':
            export.Description.Text30 = "Household URA";

            break;
          case 'Y':
            export.Description.Text30 = "Current Support";

            break;
          case 'Z':
            export.Description.Text30 = "Current Support";

            break;
          case 'D':
            export.Description.Text30 = "Already Disb";

            break;
          case 'E':
            export.Description.Text30 = "Already Disb";

            break;
          default:
            export.Description.Text30 = "System use only";

            break;
        }

        // ------------------------
        // Clear the entered field
        // ------------------------
        export.Entered.DisbAmount = 0;
        ExitState = "FN0000_DISBURSE_ACTION_SUCCESFUL";

        break;
      case "RETURN":
        ExitState = "ACO_NE0000_RETURN";

        break;
      case "SIGNOFF":
        UseScCabSignoff();

        break;
      case "EXIT":
        break;
      default:
        ExitState = "ZD_ACO_NE0000_INVALID_PF_KEY_3";

        break;
    }
  }

  private static void MoveCollection(Collection source, Collection target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Amount = source.Amount;
    target.AdjustedInd = source.AdjustedInd;
  }

  private static void MoveCsePersonAddress(CsePersonAddress source,
    CsePersonAddress target)
  {
    target.LocationType = source.LocationType;
    target.LastUpdatedTimestamp = source.LastUpdatedTimestamp;
    target.LastUpdatedBy = source.LastUpdatedBy;
    target.CreatedTimestamp = source.CreatedTimestamp;
    target.CreatedBy = source.CreatedBy;
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
    target.County = source.County;
    target.State = source.State;
    target.ZipCode = source.ZipCode;
    target.Zip4 = source.Zip4;
    target.Zip3 = source.Zip3;
    target.Street3 = source.Street3;
    target.Street4 = source.Street4;
    target.Province = source.Province;
    target.PostalCode = source.PostalCode;
    target.Country = source.Country;
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

  private static void MoveUraExcessCollection1(UraExcessCollection source,
    UraExcessCollection target)
  {
    target.SequenceNumber = source.SequenceNumber;
    target.Amount = source.Amount;
  }

  private static void MoveUraExcessCollection2(UraExcessCollection source,
    UraExcessCollection target)
  {
    target.SequenceNumber = source.SequenceNumber;
    target.Amount = source.Amount;
    target.Action = source.Action;
    target.SupplyingCsePerson = source.SupplyingCsePerson;
    target.InitiatingCollection = source.InitiatingCollection;
  }

  private static void MoveUraExcessCollection3(UraExcessCollection source,
    UraExcessCollection target)
  {
    target.SequenceNumber = source.SequenceNumber;
    target.InitiatingCsePerson = source.InitiatingCsePerson;
  }

  private void UseFnAbConcatCrAndCrd()
  {
    var useImport = new FnAbConcatCrAndCrd.Import();
    var useExport = new FnAbConcatCrAndCrd.Export();

    useImport.CashReceipt.SequentialNumber =
      entities.CashReceipt.SequentialNumber;
    useImport.CashReceiptDetail.SequentialIdentifier =
      entities.CashReceiptDetail.SequentialIdentifier;

    Call(FnAbConcatCrAndCrd.Execute, useImport, useExport);

    export.CrdCrComboNo.CrdCrCombo = useExport.CrdCrComboNo.CrdCrCombo;
  }

  private void UseFnDisburseExcessUra()
  {
    var useImport = new FnDisburseExcessUra.Import();
    var useExport = new FnDisburseExcessUra.Export();

    MoveUraExcessCollection1(local.UraExcessCollection,
      useImport.UraExcessCollection);
    useImport.Ar.Number = export.CsePerson.Number;

    Call(FnDisburseExcessUra.Execute, useImport, useExport);
  }

  private void UseOeGetArForUraExcessColl()
  {
    var useImport = new OeGetArForUraExcessColl.Import();
    var useExport = new OeGetArForUraExcessColl.Export();

    MoveUraExcessCollection3(entities.UraExcessCollection,
      useImport.UraExcessCollection);

    Call(OeGetArForUraExcessColl.Execute, useImport, useExport);

    export.HiddenImHousehold.ZdelType = useExport.ImHousehold.ZdelType;
    export.CsePerson.Number = useExport.CsePerson.Number;
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

    useImport.CsePersonsWorkSet.Number = import.PayeeName.Number;

    Call(ScCabTestSecurity.Execute, useImport, useExport);
  }

  private void UseSiGetCsePersonMailingAddr()
  {
    var useImport = new SiGetCsePersonMailingAddr.Import();
    var useExport = new SiGetCsePersonMailingAddr.Export();

    useImport.CsePerson.Number = export.CsePerson.Number;

    Call(SiGetCsePersonMailingAddr.Execute, useImport, useExport);

    MoveCsePersonAddress(useExport.CsePersonAddress, export.CsePersonAddress);
  }

  private void UseSiReadCsePerson()
  {
    var useImport = new SiReadCsePerson.Import();
    var useExport = new SiReadCsePerson.Export();

    useImport.CsePersonsWorkSet.Number = export.CsePersonsWorkSet.Number;

    Call(SiReadCsePerson.Execute, useImport, useExport);

    MoveCsePersonsWorkSet(useExport.CsePersonsWorkSet, export.CsePersonsWorkSet);
      
  }

  private bool ReadCashReceiptCashReceiptDetail()
  {
    System.Diagnostics.Debug.Assert(entities.Collection.Populated);
    entities.CashReceipt.Populated = false;
    entities.CashReceiptDetail.Populated = false;

    return Read("ReadCashReceiptCashReceiptDetail",
      (db, command) =>
      {
        db.SetInt32(command, "crdId", entities.Collection.CrdId);
        db.SetInt32(command, "crvIdentifier", entities.Collection.CrvId);
        db.SetInt32(command, "cstIdentifier", entities.Collection.CstId);
        db.SetInt32(command, "crtIdentifier", entities.Collection.CrtType);
      },
      (db, reader) =>
      {
        entities.CashReceipt.CrvIdentifier = db.GetInt32(reader, 0);
        entities.CashReceiptDetail.CrvIdentifier = db.GetInt32(reader, 0);
        entities.CashReceipt.CstIdentifier = db.GetInt32(reader, 1);
        entities.CashReceiptDetail.CstIdentifier = db.GetInt32(reader, 1);
        entities.CashReceipt.CrtIdentifier = db.GetInt32(reader, 2);
        entities.CashReceiptDetail.CrtIdentifier = db.GetInt32(reader, 2);
        entities.CashReceipt.ReceiptAmount = db.GetDecimal(reader, 3);
        entities.CashReceipt.SequentialNumber = db.GetInt32(reader, 4);
        entities.CashReceipt.ReceiptDate = db.GetDate(reader, 5);
        entities.CashReceipt.CheckType = db.GetNullableString(reader, 6);
        entities.CashReceipt.CheckNumber = db.GetNullableString(reader, 7);
        entities.CashReceipt.CheckDate = db.GetNullableDate(reader, 8);
        entities.CashReceipt.ReceivedDate = db.GetDate(reader, 9);
        entities.CashReceipt.DepositReleaseDate =
          db.GetNullableDate(reader, 10);
        entities.CashReceipt.ReferenceNumber = db.GetNullableString(reader, 11);
        entities.CashReceipt.PayorOrganization =
          db.GetNullableString(reader, 12);
        entities.CashReceipt.PayorFirstName = db.GetNullableString(reader, 13);
        entities.CashReceipt.PayorMiddleName = db.GetNullableString(reader, 14);
        entities.CashReceipt.PayorLastName = db.GetNullableString(reader, 15);
        entities.CashReceipt.ForwardedToName = db.GetNullableString(reader, 16);
        entities.CashReceipt.ForwardedStreet1 =
          db.GetNullableString(reader, 17);
        entities.CashReceipt.ForwardedStreet2 =
          db.GetNullableString(reader, 18);
        entities.CashReceipt.ForwardedCity = db.GetNullableString(reader, 19);
        entities.CashReceipt.ForwardedState = db.GetNullableString(reader, 20);
        entities.CashReceipt.ForwardedZip5 = db.GetNullableString(reader, 21);
        entities.CashReceipt.ForwardedZip4 = db.GetNullableString(reader, 22);
        entities.CashReceipt.ForwardedZip3 = db.GetNullableString(reader, 23);
        entities.CashReceipt.BalancedTimestamp =
          db.GetNullableDateTime(reader, 24);
        entities.CashReceipt.TotalCashTransactionAmount =
          db.GetNullableDecimal(reader, 25);
        entities.CashReceipt.TotalNoncashTransactionAmount =
          db.GetNullableDecimal(reader, 26);
        entities.CashReceipt.TotalCashTransactionCount =
          db.GetNullableInt32(reader, 27);
        entities.CashReceipt.TotalNoncashTransactionCount =
          db.GetNullableInt32(reader, 28);
        entities.CashReceipt.TotalDetailAdjustmentCount =
          db.GetNullableInt32(reader, 29);
        entities.CashReceipt.CreatedBy = db.GetString(reader, 30);
        entities.CashReceipt.CreatedTimestamp = db.GetDateTime(reader, 31);
        entities.CashReceipt.CashBalanceAmt = db.GetNullableDecimal(reader, 32);
        entities.CashReceipt.CashBalanceReason =
          db.GetNullableString(reader, 33);
        entities.CashReceipt.CashDue = db.GetNullableDecimal(reader, 34);
        entities.CashReceipt.TotalNonCashFeeAmount =
          db.GetNullableDecimal(reader, 35);
        entities.CashReceipt.TotalCashFeeAmount =
          db.GetNullableDecimal(reader, 36);
        entities.CashReceipt.LastUpdatedBy = db.GetNullableString(reader, 37);
        entities.CashReceipt.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 38);
        entities.CashReceipt.Note = db.GetNullableString(reader, 39);
        entities.CashReceipt.PayorSocialSecurityNumber =
          db.GetNullableString(reader, 40);
        entities.CashReceiptDetail.SequentialIdentifier =
          db.GetInt32(reader, 41);
        entities.CashReceiptDetail.InterfaceTransId =
          db.GetNullableString(reader, 42);
        entities.CashReceiptDetail.AdjustmentInd =
          db.GetNullableString(reader, 43);
        entities.CashReceiptDetail.CourtOrderNumber =
          db.GetNullableString(reader, 44);
        entities.CashReceiptDetail.CaseNumber =
          db.GetNullableString(reader, 45);
        entities.CashReceiptDetail.OffsetTaxid =
          db.GetNullableInt32(reader, 46);
        entities.CashReceiptDetail.ReceivedAmount = db.GetDecimal(reader, 47);
        entities.CashReceiptDetail.CollectionAmount = db.GetDecimal(reader, 48);
        entities.CashReceiptDetail.CollectionDate = db.GetDate(reader, 49);
        entities.CashReceiptDetail.MultiPayor =
          db.GetNullableString(reader, 50);
        entities.CashReceiptDetail.OffsetTaxYear =
          db.GetNullableInt32(reader, 51);
        entities.CashReceiptDetail.JointReturnInd =
          db.GetNullableString(reader, 52);
        entities.CashReceiptDetail.JointReturnName =
          db.GetNullableString(reader, 53);
        entities.CashReceiptDetail.DefaultedCollectionDateInd =
          db.GetNullableString(reader, 54);
        entities.CashReceiptDetail.ObligorPersonNumber =
          db.GetNullableString(reader, 55);
        entities.CashReceiptDetail.ObligorSocialSecurityNumber =
          db.GetNullableString(reader, 56);
        entities.CashReceiptDetail.ObligorFirstName =
          db.GetNullableString(reader, 57);
        entities.CashReceiptDetail.ObligorLastName =
          db.GetNullableString(reader, 58);
        entities.CashReceiptDetail.ObligorMiddleName =
          db.GetNullableString(reader, 59);
        entities.CashReceiptDetail.ObligorPhoneNumber =
          db.GetNullableString(reader, 60);
        entities.CashReceiptDetail.PayeeFirstName =
          db.GetNullableString(reader, 61);
        entities.CashReceiptDetail.PayeeMiddleName =
          db.GetNullableString(reader, 62);
        entities.CashReceiptDetail.PayeeLastName =
          db.GetNullableString(reader, 63);
        entities.CashReceiptDetail.Attribute1SupportedPersonFirstName =
          db.GetNullableString(reader, 64);
        entities.CashReceiptDetail.Attribute1SupportedPersonMiddleName =
          db.GetNullableString(reader, 65);
        entities.CashReceiptDetail.Attribute1SupportedPersonLastName =
          db.GetNullableString(reader, 66);
        entities.CashReceiptDetail.Attribute2SupportedPersonFirstName =
          db.GetNullableString(reader, 67);
        entities.CashReceiptDetail.Attribute2SupportedPersonLastName =
          db.GetNullableString(reader, 68);
        entities.CashReceiptDetail.Attribute2SupportedPersonMiddleName =
          db.GetNullableString(reader, 69);
        entities.CashReceiptDetail.CreatedBy = db.GetString(reader, 70);
        entities.CashReceiptDetail.CreatedTmst = db.GetDateTime(reader, 71);
        entities.CashReceiptDetail.LastUpdatedBy =
          db.GetNullableString(reader, 72);
        entities.CashReceiptDetail.LastUpdatedTmst =
          db.GetNullableDateTime(reader, 73);
        entities.CashReceiptDetail.RefundedAmount =
          db.GetNullableDecimal(reader, 74);
        entities.CashReceiptDetail.DistributedAmount =
          db.GetNullableDecimal(reader, 75);
        entities.CashReceiptDetail.CollectionAmtFullyAppliedInd =
          db.GetNullableString(reader, 76);
        entities.CashReceiptDetail.SuppPersonNoForVol =
          db.GetNullableString(reader, 77);
        entities.CashReceiptDetail.Reference = db.GetNullableString(reader, 78);
        entities.CashReceiptDetail.Notes = db.GetNullableString(reader, 79);
        entities.CashReceiptDetail.OverrideManualDistInd =
          db.GetNullableString(reader, 80);
        entities.CashReceipt.Populated = true;
        entities.CashReceiptDetail.Populated = true;
        CheckValid<CashReceipt>("CashBalanceReason",
          entities.CashReceipt.CashBalanceReason);
        CheckValid<CashReceiptDetail>("MultiPayor",
          entities.CashReceiptDetail.MultiPayor);
        CheckValid<CashReceiptDetail>("JointReturnInd",
          entities.CashReceiptDetail.JointReturnInd);
        CheckValid<CashReceiptDetail>("DefaultedCollectionDateInd",
          entities.CashReceiptDetail.DefaultedCollectionDateInd);
        CheckValid<CashReceiptDetail>("CollectionAmtFullyAppliedInd",
          entities.CashReceiptDetail.CollectionAmtFullyAppliedInd);
      });
  }

  private bool ReadCollection()
  {
    entities.Collection.Populated = false;

    return Read("ReadCollection",
      (db, command) =>
      {
        db.SetInt32(
          command, "collId", entities.UraExcessCollection.InitiatingCollection);
          
        db.SetNullableString(
          command, "cspSupNumber",
          entities.UraExcessCollection.InitiatingCsePerson);
      },
      (db, reader) =>
      {
        entities.Collection.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.Collection.AdjustedInd = db.GetNullableString(reader, 1);
        entities.Collection.CrtType = db.GetInt32(reader, 2);
        entities.Collection.CstId = db.GetInt32(reader, 3);
        entities.Collection.CrvId = db.GetInt32(reader, 4);
        entities.Collection.CrdId = db.GetInt32(reader, 5);
        entities.Collection.ObgId = db.GetInt32(reader, 6);
        entities.Collection.CspNumber = db.GetString(reader, 7);
        entities.Collection.CpaType = db.GetString(reader, 8);
        entities.Collection.OtrId = db.GetInt32(reader, 9);
        entities.Collection.OtrType = db.GetString(reader, 10);
        entities.Collection.OtyId = db.GetInt32(reader, 11);
        entities.Collection.Amount = db.GetDecimal(reader, 12);
        entities.Collection.Populated = true;
        CheckValid<Collection>("AdjustedInd", entities.Collection.AdjustedInd);
        CheckValid<Collection>("CpaType", entities.Collection.CpaType);
        CheckValid<Collection>("OtrType", entities.Collection.OtrType);
      });
  }

  private bool ReadUraExcessCollection()
  {
    entities.UraExcessCollection.Populated = false;

    return Read("ReadUraExcessCollection",
      (db, command) =>
      {
        db.SetInt32(
          command, "seqNumber", export.UraExcessCollection.SequenceNumber);
      },
      (db, reader) =>
      {
        entities.UraExcessCollection.SequenceNumber = db.GetInt32(reader, 0);
        entities.UraExcessCollection.Amount = db.GetDecimal(reader, 1);
        entities.UraExcessCollection.Action = db.GetString(reader, 2);
        entities.UraExcessCollection.SupplyingCsePerson =
          db.GetString(reader, 3);
        entities.UraExcessCollection.InitiatingCollection =
          db.GetInt32(reader, 4);
        entities.UraExcessCollection.InitiatingCsePerson =
          db.GetString(reader, 5);
        entities.UraExcessCollection.Populated = true;
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
    /// A value of HiddenImHousehold.
    /// </summary>
    [JsonPropertyName("hiddenImHousehold")]
    public ImHousehold HiddenImHousehold
    {
      get => hiddenImHousehold ??= new();
      set => hiddenImHousehold = value;
    }

    /// <summary>
    /// A value of CrdCrComboNo.
    /// </summary>
    [JsonPropertyName("crdCrComboNo")]
    public CrdCrComboNo CrdCrComboNo
    {
      get => crdCrComboNo ??= new();
      set => crdCrComboNo = value;
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
    /// A value of Description.
    /// </summary>
    [JsonPropertyName("description")]
    public TextWorkArea Description
    {
      get => description ??= new();
      set => description = value;
    }

    /// <summary>
    /// A value of HiddenUraExcessCollection.
    /// </summary>
    [JsonPropertyName("hiddenUraExcessCollection")]
    public UraExcessCollection HiddenUraExcessCollection
    {
      get => hiddenUraExcessCollection ??= new();
      set => hiddenUraExcessCollection = value;
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
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
    }

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
    /// A value of Entered.
    /// </summary>
    [JsonPropertyName("entered")]
    public LocalFinanceWorkArea Entered
    {
      get => entered ??= new();
      set => entered = value;
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
    /// A value of UraExcessCollection.
    /// </summary>
    [JsonPropertyName("uraExcessCollection")]
    public UraExcessCollection UraExcessCollection
    {
      get => uraExcessCollection ??= new();
      set => uraExcessCollection = value;
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
    /// A value of DisplayIndicator.
    /// </summary>
    [JsonPropertyName("displayIndicator")]
    public Common DisplayIndicator
    {
      get => displayIndicator ??= new();
      set => displayIndicator = value;
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
    /// A value of PaymentRequest.
    /// </summary>
    [JsonPropertyName("paymentRequest")]
    public PaymentRequest PaymentRequest
    {
      get => paymentRequest ??= new();
      set => paymentRequest = value;
    }

    /// <summary>
    /// A value of PayeeName.
    /// </summary>
    [JsonPropertyName("payeeName")]
    public CsePersonsWorkSet PayeeName
    {
      get => payeeName ??= new();
      set => payeeName = value;
    }

    private ImHousehold hiddenImHousehold;
    private CrdCrComboNo crdCrComboNo;
    private Collection collection;
    private TextWorkArea description;
    private UraExcessCollection hiddenUraExcessCollection;
    private CsePersonsWorkSet csePersonsWorkSet;
    private CsePerson csePerson;
    private CsePersonAddress csePersonAddress;
    private LocalFinanceWorkArea entered;
    private CashReceipt cashReceipt;
    private UraExcessCollection uraExcessCollection;
    private NextTranInfo hiddenNextTranInfo;
    private Common displayIndicator;
    private Standard standard;
    private PaymentRequest paymentRequest;
    private CsePersonsWorkSet payeeName;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of HiddenImHousehold.
    /// </summary>
    [JsonPropertyName("hiddenImHousehold")]
    public ImHousehold HiddenImHousehold
    {
      get => hiddenImHousehold ??= new();
      set => hiddenImHousehold = value;
    }

    /// <summary>
    /// A value of CrdCrComboNo.
    /// </summary>
    [JsonPropertyName("crdCrComboNo")]
    public CrdCrComboNo CrdCrComboNo
    {
      get => crdCrComboNo ??= new();
      set => crdCrComboNo = value;
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
    /// A value of Description.
    /// </summary>
    [JsonPropertyName("description")]
    public TextWorkArea Description
    {
      get => description ??= new();
      set => description = value;
    }

    /// <summary>
    /// A value of HiddenUraExcessCollection.
    /// </summary>
    [JsonPropertyName("hiddenUraExcessCollection")]
    public UraExcessCollection HiddenUraExcessCollection
    {
      get => hiddenUraExcessCollection ??= new();
      set => hiddenUraExcessCollection = value;
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
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
    }

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
    /// A value of Entered.
    /// </summary>
    [JsonPropertyName("entered")]
    public LocalFinanceWorkArea Entered
    {
      get => entered ??= new();
      set => entered = value;
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
    /// A value of UraExcessCollection.
    /// </summary>
    [JsonPropertyName("uraExcessCollection")]
    public UraExcessCollection UraExcessCollection
    {
      get => uraExcessCollection ??= new();
      set => uraExcessCollection = value;
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
    /// A value of DisplayIndicator.
    /// </summary>
    [JsonPropertyName("displayIndicator")]
    public Common DisplayIndicator
    {
      get => displayIndicator ??= new();
      set => displayIndicator = value;
    }

    /// <summary>
    /// A value of ZdelExportNew.
    /// </summary>
    [JsonPropertyName("zdelExportNew")]
    public PaymentStatus ZdelExportNew
    {
      get => zdelExportNew ??= new();
      set => zdelExportNew = value;
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

    private ImHousehold hiddenImHousehold;
    private CrdCrComboNo crdCrComboNo;
    private Collection collection;
    private TextWorkArea description;
    private UraExcessCollection hiddenUraExcessCollection;
    private CsePersonsWorkSet csePersonsWorkSet;
    private CsePerson csePerson;
    private CsePersonAddress csePersonAddress;
    private LocalFinanceWorkArea entered;
    private CashReceipt cashReceipt;
    private UraExcessCollection uraExcessCollection;
    private NextTranInfo hiddenNextTranInfo;
    private Common displayIndicator;
    private PaymentStatus zdelExportNew;
    private Standard standard;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of UraExcessCollection.
    /// </summary>
    [JsonPropertyName("uraExcessCollection")]
    public UraExcessCollection UraExcessCollection
    {
      get => uraExcessCollection ??= new();
      set => uraExcessCollection = value;
    }

    /// <summary>
    /// A value of Null1.
    /// </summary>
    [JsonPropertyName("null1")]
    public CsePersonAddress Null1
    {
      get => null1 ??= new();
      set => null1 = value;
    }

    private UraExcessCollection uraExcessCollection;
    private CsePersonAddress null1;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
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
    /// A value of CsePersonAccount.
    /// </summary>
    [JsonPropertyName("csePersonAccount")]
    public CsePersonAccount CsePersonAccount
    {
      get => csePersonAccount ??= new();
      set => csePersonAccount = value;
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
    /// A value of Collection.
    /// </summary>
    [JsonPropertyName("collection")]
    public Collection Collection
    {
      get => collection ??= new();
      set => collection = value;
    }

    /// <summary>
    /// A value of UraExcessCollection.
    /// </summary>
    [JsonPropertyName("uraExcessCollection")]
    public UraExcessCollection UraExcessCollection
    {
      get => uraExcessCollection ??= new();
      set => uraExcessCollection = value;
    }

    private CsePerson csePerson;
    private CsePersonAccount csePersonAccount;
    private ObligationTransaction obligationTransaction;
    private CashReceipt cashReceipt;
    private CashReceiptDetail cashReceiptDetail;
    private Collection collection;
    private UraExcessCollection uraExcessCollection;
  }
#endregion
}
