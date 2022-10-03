// Program: LE_B578_PROCESS_UI_CR_DETAIL, ID: 945096284, model: 746.
// Short name: SWE03073
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: LE_B578_PROCESS_UI_CR_DETAIL.
/// </summary>
[Serializable]
public partial class LeB578ProcessUiCrDetail: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the LE_B578_PROCESS_UI_CR_DETAIL program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new LeB578ProcessUiCrDetail(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of LeB578ProcessUiCrDetail.
  /// </summary>
  public LeB578ProcessUiCrDetail(IContext context, Import import, Export export):
    
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // -------------------------------------------------------------------------------------
    // Date      Developer  Request #	Description
    // --------  ---------  ---------	
    // -----------------------------------------------------
    // 06/16/12  GVandy     CQ33628	Initial Development - This is based upon how
    // SDSO
    // 				is receipted.
    // -------------------------------------------------------------------------------------
    MoveCashReceiptDetail2(import.CashReceiptDetail, local.CashReceiptDetail);
    local.U.Code = "U";
    local.U.SequentialIdentifier = 5;
    local.HardcodedInvalidPersonNumber.ReasonCodeId = "INVPERSNBR";
    UseFnHardcodedCashReceipting();

    // -----------------------------------------------------------------------------------
    // -- Read cash receipt and cash receipt event under which the cash receipt 
    // detail will be added.
    // -----------------------------------------------------------------------------------
    if (ReadCashReceiptEventCashReceipt())
    {
      // -- Continue
    }
    else
    {
      ExitState = "FN0084_CASH_RCPT_NF";

      return;
    }

    // -----------------------------------------------------------------------------------
    // -- Set the appropriate values and record the new cash receipt detail.
    // -----------------------------------------------------------------------------------
    // -- Determine the next available cash receipt detail number for this cash 
    // receipt.
    local.CashReceiptDetail.SequentialIdentifier = 1;

    if (ReadCashReceiptDetail2())
    {
      local.CashReceiptDetail.SequentialIdentifier =
        entities.CashReceiptDetail.SequentialIdentifier + 1;
    }

    // -- Get phone number for cash receipt detail.
    local.CashReceiptDetail.ObligorPhoneNumber = "";
    local.CsePersonsWorkSet.Number =
      import.CashReceiptDetail.ObligorPersonNumber ?? Spaces(10);
    UseSiReadCsePersonBatch();

    if (IsExitState("ACO_NN0000_ALL_OK"))
    {
      // -- Format the Obligor Home-Phone for the Cash_Receipt_Detail
      if (local.CsePerson.HomePhone.GetValueOrDefault() > 0)
      {
        if (local.CsePerson.HomePhoneAreaCode.GetValueOrDefault() > 0)
        {
          local.CashReceiptDetail.ObligorPhoneNumber =
            NumberToString(local.CsePerson.HomePhoneAreaCode.
              GetValueOrDefault(), 13, 3);
          local.CashReceiptDetail.ObligorPhoneNumber =
            Substring(local.CashReceiptDetail.ObligorPhoneNumber, 12, 1, 3) + NumberToString
            (local.CsePerson.HomePhone.GetValueOrDefault(), 9, 7);
        }
        else
        {
          local.CashReceiptDetail.ObligorPhoneNumber =
            NumberToString(local.CsePerson.HomePhone.GetValueOrDefault(), 9, 7);
            
        }
      }

      // -- After the cash receipt detail is created it will be placed in to "
      // RELEASED" status.
      local.CashReceiptDetailStatus.SystemGeneratedIdentifier =
        local.HardcodedViews.HardcodedReleased.SystemGeneratedIdentifier;
    }
    else if (IsExitState("CSE_PERSON_NF"))
    {
      // -- Continue.  After the cash receipt detail is created it will be 
      // placed in
      //    "SUSPENDED" status with reason code "INVPERSNBR".
      local.CashReceiptDetailStatus.SystemGeneratedIdentifier =
        local.HardcodedViews.HardcodedSuspended.SystemGeneratedIdentifier;
      local.CashReceiptDetailStatHistory.ReasonCodeId =
        local.HardcodedInvalidPersonNumber.ReasonCodeId ?? "";
      ExitState = "ACO_NN0000_ALL_OK";
    }
    else
    {
      return;
    }

    local.CashReceiptDetail.ReceivedAmount =
      import.CashReceiptDetail.CollectionAmount;
    local.CashReceiptDetail.CollectionAmount =
      import.CashReceiptDetail.CollectionAmount;
    local.CashReceiptDetail.ObligorSocialSecurityNumber =
      local.CsePersonsWorkSet.Ssn;
    local.CashReceiptDetail.ObligorFirstName =
      local.CsePersonsWorkSet.FirstName;
    local.CashReceiptDetail.ObligorLastName = local.CsePersonsWorkSet.LastName;
    local.CashReceiptDetail.ObligorMiddleName =
      local.CsePersonsWorkSet.MiddleInitial;
    local.CashReceiptDetail.CollectionDate =
      entities.CashReceiptEvent.SourceCreationDate;
    UseRecordCollection();

    if (IsExitState("ACO_NN0000_ALL_OK"))
    {
    }
    else
    {
      return;
    }

    // -----------------------------------------------------------------------------------
    // -- Set the appropriate status for the new cash receipt detail.
    // -----------------------------------------------------------------------------------
    // -- Get currency on the cash receipt detail for the action block below.
    if (!ReadCashReceiptDetail1())
    {
      ExitState = "FN0052_CASH_RCPT_DTL_NF";

      return;
    }

    // -- Local status and status_history views were set above based on whether 
    // person was found or not.
    UseFnChangeCashRcptDtlStatHis();

    if (IsExitState("ACO_NN0000_ALL_OK"))
    {
    }
    else
    {
      return;
    }

    // -----------------------------------------------------------------------------------
    // -- Increment counts and amounts on the Cash Receipt and Cash Receipt 
    // Event.
    // -----------------------------------------------------------------------------------
    try
    {
      UpdateCashReceiptEvent();
    }
    catch(Exception e)
    {
      switch(GetErrorCode(e))
      {
        case ErrorCode.AlreadyExists:
          ExitState = "FN0079_CASH_RCPT_EVENT_NU";

          return;
        case ErrorCode.PermittedValueViolation:
          ExitState = "FN0080_CASH_RCPT_EVENT_PV";

          return;
        case ErrorCode.DatabaseError:
          break;
        default:
          throw;
      }
    }

    try
    {
      UpdateCashReceipt();
    }
    catch(Exception e)
    {
      switch(GetErrorCode(e))
      {
        case ErrorCode.AlreadyExists:
          ExitState = "FN0088_CASH_RCPT_NU";

          return;
        case ErrorCode.PermittedValueViolation:
          ExitState = "FN0090_CASH_RCPT_PV";

          return;
        case ErrorCode.DatabaseError:
          break;
        default:
          throw;
      }
    }

    // -----------------------------------------------------------------------------------
    // -- Add mailing address to cash receipt detail.
    // -----------------------------------------------------------------------------------
    if (!Equal(local.CashReceiptDetailStatHistory.ReasonCodeId,
      local.HardcodedInvalidPersonNumber.ReasonCodeId))
    {
      UseSiGetCsePersonMailingAddr();

      if (!IsEmpty(local.Returned.Street1))
      {
        local.Create.City = local.Returned.City ?? Spaces(30);
        local.Create.State = local.Returned.State ?? Spaces(2);
        local.Create.Street1 = local.Returned.Street1 ?? Spaces(25);
        local.Create.Street2 = local.Returned.Street2 ?? "";
        local.Create.ZipCode3 = local.Returned.Zip3 ?? "";
        local.Create.ZipCode4 = local.Returned.Zip4 ?? "";
        local.Create.ZipCode5 = local.Returned.ZipCode ?? Spaces(5);
        UseCreateCrDetailAddress();
      }

      ExitState = "ACO_NN0000_ALL_OK";
    }

    // -----------------------------------------------------------------------------------
    // -- Keep a running total of the number and amount of CRDs in each status.
    // -- These values are displayed on the control report.
    // -----------------------------------------------------------------------------------
    ++import.ExportRecCollections.Count;
    import.ExportRecCollections.TotalCurrency += import.CashReceiptDetail.
      CollectionAmount;

    if (local.CashReceiptDetailStatus.SystemGeneratedIdentifier == local
      .HardcodedViews.HardcodedSuspended.SystemGeneratedIdentifier)
    {
      ++import.ExportSusCollections.Count;
      import.ExportSusCollections.TotalCurrency += import.CashReceiptDetail.
        CollectionAmount;
    }
    else if (local.CashReceiptDetailStatus.SystemGeneratedIdentifier == local
      .HardcodedViews.HardcodedReleased.SystemGeneratedIdentifier)
    {
      ++import.ExportRelCollections.Count;
      import.ExportRelCollections.TotalCurrency += import.CashReceiptDetail.
        CollectionAmount;
    }
    else
    {
    }
  }

  private static void MoveCashReceiptDetail1(CashReceiptDetail source,
    CashReceiptDetail target)
  {
    target.SequentialIdentifier = source.SequentialIdentifier;
    target.CourtOrderNumber = source.CourtOrderNumber;
    target.CaseNumber = source.CaseNumber;
    target.OffsetTaxid = source.OffsetTaxid;
    target.ReceivedAmount = source.ReceivedAmount;
    target.CollectionAmount = source.CollectionAmount;
    target.CollectionDate = source.CollectionDate;
    target.MultiPayor = source.MultiPayor;
    target.OffsetTaxYear = source.OffsetTaxYear;
    target.JointReturnInd = source.JointReturnInd;
    target.JointReturnName = source.JointReturnName;
    target.DefaultedCollectionDateInd = source.DefaultedCollectionDateInd;
    target.ObligorPersonNumber = source.ObligorPersonNumber;
    target.ObligorSocialSecurityNumber = source.ObligorSocialSecurityNumber;
    target.ObligorFirstName = source.ObligorFirstName;
    target.ObligorLastName = source.ObligorLastName;
    target.ObligorMiddleName = source.ObligorMiddleName;
    target.ObligorPhoneNumber = source.ObligorPhoneNumber;
  }

  private static void MoveCashReceiptDetail2(CashReceiptDetail source,
    CashReceiptDetail target)
  {
    target.CourtOrderNumber = source.CourtOrderNumber;
    target.CollectionAmount = source.CollectionAmount;
    target.ObligorPersonNumber = source.ObligorPersonNumber;
  }

  private static void MoveCollectionType(CollectionType source,
    CollectionType target)
  {
    target.SequentialIdentifier = source.SequentialIdentifier;
    target.Code = source.Code;
  }

  private static void MoveCsePerson(CsePerson source, CsePerson target)
  {
    target.Type1 = source.Type1;
    target.Number = source.Number;
    target.HomePhone = source.HomePhone;
    target.HomePhoneAreaCode = source.HomePhoneAreaCode;
  }

  private static void MoveCsePersonAddress(CsePersonAddress source,
    CsePersonAddress target)
  {
    target.LocationType = source.LocationType;
    target.Street1 = source.Street1;
    target.Street2 = source.Street2;
    target.City = source.City;
    target.Type1 = source.Type1;
    target.State = source.State;
    target.ZipCode = source.ZipCode;
    target.Zip4 = source.Zip4;
    target.Zip3 = source.Zip3;
  }

  private void UseCreateCrDetailAddress()
  {
    var useImport = new CreateCrDetailAddress.Import();
    var useExport = new CreateCrDetailAddress.Export();

    useImport.CashReceiptEvent.SystemGeneratedIdentifier =
      entities.CashReceiptEvent.SystemGeneratedIdentifier;
    useImport.CashReceiptSourceType.SystemGeneratedIdentifier =
      import.CashReceiptSourceType.SystemGeneratedIdentifier;
    useImport.CashReceiptType.SystemGeneratedIdentifier =
      import.CashReceiptType.SystemGeneratedIdentifier;
    useImport.CashReceiptDetailAddress.Assign(local.Create);
    useImport.CashReceiptDetail.SequentialIdentifier =
      local.CashReceiptDetail.SequentialIdentifier;

    Call(CreateCrDetailAddress.Execute, useImport, useExport);

    local.Create.SystemGeneratedIdentifier =
      useExport.CashReceiptDetailAddress.SystemGeneratedIdentifier;
  }

  private void UseFnChangeCashRcptDtlStatHis()
  {
    var useImport = new FnChangeCashRcptDtlStatHis.Import();
    var useExport = new FnChangeCashRcptDtlStatHis.Export();

    useImport.Persistent.Assign(entities.CashReceiptDetail);
    useImport.CashReceiptEvent.SystemGeneratedIdentifier =
      import.CashReceiptEvent.SystemGeneratedIdentifier;
    useImport.CashReceiptSourceType.SystemGeneratedIdentifier =
      import.CashReceiptSourceType.SystemGeneratedIdentifier;
    useImport.CashReceiptType.SystemGeneratedIdentifier =
      import.CashReceiptType.SystemGeneratedIdentifier;
    useImport.CashReceiptDetailStatus.SystemGeneratedIdentifier =
      local.CashReceiptDetailStatus.SystemGeneratedIdentifier;
    useImport.New1.ReasonCodeId =
      local.CashReceiptDetailStatHistory.ReasonCodeId;

    Call(FnChangeCashRcptDtlStatHis.Execute, useImport, useExport);

    entities.CashReceiptDetail.Assign(useImport.Persistent);
  }

  private void UseFnHardcodedCashReceipting()
  {
    var useImport = new FnHardcodedCashReceipting.Import();
    var useExport = new FnHardcodedCashReceipting.Export();

    Call(FnHardcodedCashReceipting.Execute, useImport, useExport);

    local.HardcodedViews.HardcodedReleased.SystemGeneratedIdentifier =
      useExport.CrdsSystemId.CrdsIdReleased.SystemGeneratedIdentifier;
    local.HardcodedViews.HardcodedRecorded.SystemGeneratedIdentifier =
      useExport.CrdsSystemId.CrdsIdRecorded.SystemGeneratedIdentifier;
    local.HardcodedViews.HardcodedSuspended.SystemGeneratedIdentifier =
      useExport.CrdsSystemId.CrdsIdSuspended.SystemGeneratedIdentifier;
    local.HardcodedViews.HardcodedRefunded.SystemGeneratedIdentifier =
      useExport.CrdsSystemId.CrdsIdRefunded.SystemGeneratedIdentifier;
    local.HardcodedViews.HardcodedPended.SystemGeneratedIdentifier =
      useExport.CrdsSystemId.CrdsIdPended.SystemGeneratedIdentifier;
  }

  private void UseRecordCollection()
  {
    var useImport = new RecordCollection.Import();
    var useExport = new RecordCollection.Export();

    useImport.CashReceipt.SequentialNumber =
      entities.CashReceipt.SequentialNumber;
    MoveCashReceiptDetail1(local.CashReceiptDetail, useImport.CashReceiptDetail);
      
    MoveCollectionType(local.U, useImport.CollectionType);
    useImport.CashReceiptDetailStatus.SystemGeneratedIdentifier =
      local.HardcodedViews.HardcodedRecorded.SystemGeneratedIdentifier;

    Call(RecordCollection.Execute, useImport, useExport);

    MoveCashReceiptDetail1(useExport.CashReceiptDetail, local.CashReceiptDetail);
      
  }

  private void UseSiGetCsePersonMailingAddr()
  {
    var useImport = new SiGetCsePersonMailingAddr.Import();
    var useExport = new SiGetCsePersonMailingAddr.Export();

    useImport.CsePerson.Number = local.CsePerson.Number;

    Call(SiGetCsePersonMailingAddr.Execute, useImport, useExport);

    MoveCsePersonAddress(useExport.CsePersonAddress, local.Returned);
  }

  private void UseSiReadCsePersonBatch()
  {
    var useImport = new SiReadCsePersonBatch.Import();
    var useExport = new SiReadCsePersonBatch.Export();

    useImport.CsePersonsWorkSet.Number = local.CsePersonsWorkSet.Number;

    Call(SiReadCsePersonBatch.Execute, useImport, useExport);

    MoveCsePerson(useExport.CsePerson, local.CsePerson);
    local.CsePersonsWorkSet.Assign(useExport.CsePersonsWorkSet);
  }

  private bool ReadCashReceiptDetail1()
  {
    System.Diagnostics.Debug.Assert(entities.CashReceipt.Populated);
    entities.CashReceiptDetail.Populated = false;

    return Read("ReadCashReceiptDetail1",
      (db, command) =>
      {
        db.
          SetInt32(command, "crtIdentifier", entities.CashReceipt.CrtIdentifier);
          
        db.
          SetInt32(command, "cstIdentifier", entities.CashReceipt.CstIdentifier);
          
        db.
          SetInt32(command, "crvIdentifier", entities.CashReceipt.CrvIdentifier);
          
        db.SetInt32(
          command, "crdId", local.CashReceiptDetail.SequentialIdentifier);
      },
      (db, reader) =>
      {
        entities.CashReceiptDetail.CrvIdentifier = db.GetInt32(reader, 0);
        entities.CashReceiptDetail.CstIdentifier = db.GetInt32(reader, 1);
        entities.CashReceiptDetail.CrtIdentifier = db.GetInt32(reader, 2);
        entities.CashReceiptDetail.SequentialIdentifier =
          db.GetInt32(reader, 3);
        entities.CashReceiptDetail.Populated = true;
      });
  }

  private bool ReadCashReceiptDetail2()
  {
    System.Diagnostics.Debug.Assert(entities.CashReceipt.Populated);
    entities.CashReceiptDetail.Populated = false;

    return Read("ReadCashReceiptDetail2",
      (db, command) =>
      {
        db.
          SetInt32(command, "crtIdentifier", entities.CashReceipt.CrtIdentifier);
          
        db.
          SetInt32(command, "cstIdentifier", entities.CashReceipt.CstIdentifier);
          
        db.
          SetInt32(command, "crvIdentifier", entities.CashReceipt.CrvIdentifier);
          
      },
      (db, reader) =>
      {
        entities.CashReceiptDetail.CrvIdentifier = db.GetInt32(reader, 0);
        entities.CashReceiptDetail.CstIdentifier = db.GetInt32(reader, 1);
        entities.CashReceiptDetail.CrtIdentifier = db.GetInt32(reader, 2);
        entities.CashReceiptDetail.SequentialIdentifier =
          db.GetInt32(reader, 3);
        entities.CashReceiptDetail.Populated = true;
      });
  }

  private bool ReadCashReceiptEventCashReceipt()
  {
    entities.CashReceiptEvent.Populated = false;
    entities.CashReceipt.Populated = false;

    return Read("ReadCashReceiptEventCashReceipt",
      (db, command) =>
      {
        db.SetInt32(
          command, "creventId",
          import.CashReceiptEvent.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "cstIdentifier",
          import.CashReceiptSourceType.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "crtIdentifier",
          import.CashReceiptType.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.CashReceiptEvent.CstIdentifier = db.GetInt32(reader, 0);
        entities.CashReceipt.CstIdentifier = db.GetInt32(reader, 0);
        entities.CashReceiptEvent.SystemGeneratedIdentifier =
          db.GetInt32(reader, 1);
        entities.CashReceipt.CrvIdentifier = db.GetInt32(reader, 1);
        entities.CashReceiptEvent.SourceCreationDate =
          db.GetNullableDate(reader, 2);
        entities.CashReceiptEvent.TotalNonCashTransactionCount =
          db.GetNullableInt32(reader, 3);
        entities.CashReceiptEvent.AnticipatedCheckAmt =
          db.GetNullableDecimal(reader, 4);
        entities.CashReceiptEvent.TotalCashAmt =
          db.GetNullableDecimal(reader, 5);
        entities.CashReceiptEvent.TotalCashTransactionCount =
          db.GetNullableInt32(reader, 6);
        entities.CashReceiptEvent.TotalNonCashAmt =
          db.GetNullableDecimal(reader, 7);
        entities.CashReceipt.CrtIdentifier = db.GetInt32(reader, 8);
        entities.CashReceipt.ReceiptAmount = db.GetDecimal(reader, 9);
        entities.CashReceipt.SequentialNumber = db.GetInt32(reader, 10);
        entities.CashReceipt.TotalCashTransactionAmount =
          db.GetNullableDecimal(reader, 11);
        entities.CashReceipt.TotalNoncashTransactionAmount =
          db.GetNullableDecimal(reader, 12);
        entities.CashReceipt.TotalCashTransactionCount =
          db.GetNullableInt32(reader, 13);
        entities.CashReceipt.TotalNoncashTransactionCount =
          db.GetNullableInt32(reader, 14);
        entities.CashReceipt.CashBalanceAmt = db.GetNullableDecimal(reader, 15);
        entities.CashReceipt.CashBalanceReason =
          db.GetNullableString(reader, 16);
        entities.CashReceipt.CashDue = db.GetNullableDecimal(reader, 17);
        entities.CashReceiptEvent.Populated = true;
        entities.CashReceipt.Populated = true;
        CheckValid<CashReceipt>("CashBalanceReason",
          entities.CashReceipt.CashBalanceReason);
      });
  }

  private void UpdateCashReceipt()
  {
    System.Diagnostics.Debug.Assert(entities.CashReceipt.Populated);

    var totalCashTransactionAmount =
      entities.CashReceipt.TotalCashTransactionAmount.GetValueOrDefault() +
      import.CashReceiptDetail.CollectionAmount;
    var totalCashTransactionCount =
      entities.CashReceipt.TotalCashTransactionCount.GetValueOrDefault() + 1;
    var cashBalanceAmt =
      entities.CashReceipt.CashBalanceAmt.GetValueOrDefault() +
      import.CashReceiptDetail.CollectionAmount;
    var cashBalanceReason = "UNDER";
    var cashDue =
      entities.CashReceipt.CashDue.GetValueOrDefault() +
      import.CashReceiptDetail.CollectionAmount;

    CheckValid<CashReceipt>("CashBalanceReason", cashBalanceReason);
    entities.CashReceipt.Populated = false;
    Update("UpdateCashReceipt",
      (db, command) =>
      {
        db.SetNullableDecimal(
          command, "totalCashTransac", totalCashTransactionAmount);
        db.
          SetNullableInt32(command, "totCashTranCnt", totalCashTransactionCount);
          
        db.SetNullableDecimal(command, "cashBalAmt", cashBalanceAmt);
        db.SetNullableString(command, "cashBalRsn", cashBalanceReason);
        db.SetNullableDecimal(command, "cashDue", cashDue);
        db.
          SetInt32(command, "crvIdentifier", entities.CashReceipt.CrvIdentifier);
          
        db.
          SetInt32(command, "cstIdentifier", entities.CashReceipt.CstIdentifier);
          
        db.
          SetInt32(command, "crtIdentifier", entities.CashReceipt.CrtIdentifier);
          
      });

    entities.CashReceipt.TotalCashTransactionAmount =
      totalCashTransactionAmount;
    entities.CashReceipt.TotalCashTransactionCount = totalCashTransactionCount;
    entities.CashReceipt.CashBalanceAmt = cashBalanceAmt;
    entities.CashReceipt.CashBalanceReason = cashBalanceReason;
    entities.CashReceipt.CashDue = cashDue;
    entities.CashReceipt.Populated = true;
  }

  private void UpdateCashReceiptEvent()
  {
    System.Diagnostics.Debug.Assert(entities.CashReceiptEvent.Populated);

    var anticipatedCheckAmt =
      entities.CashReceiptEvent.AnticipatedCheckAmt.GetValueOrDefault() +
      import.CashReceiptDetail.CollectionAmount;
    var totalCashAmt =
      entities.CashReceiptEvent.TotalCashAmt.GetValueOrDefault() +
      import.CashReceiptDetail.CollectionAmount;
    var totalCashTransactionCount =
      entities.CashReceiptEvent.TotalCashTransactionCount.GetValueOrDefault() +
      1;

    entities.CashReceiptEvent.Populated = false;
    Update("UpdateCashReceiptEvent",
      (db, command) =>
      {
        db.SetNullableDecimal(command, "anticCheckAmt", anticipatedCheckAmt);
        db.SetNullableDecimal(command, "totalCashAmt", totalCashAmt);
        db.
          SetNullableInt32(command, "totCashTranCnt", totalCashTransactionCount);
          
        db.SetInt32(
          command, "cstIdentifier", entities.CashReceiptEvent.CstIdentifier);
        db.SetInt32(
          command, "creventId",
          entities.CashReceiptEvent.SystemGeneratedIdentifier);
      });

    entities.CashReceiptEvent.AnticipatedCheckAmt = anticipatedCheckAmt;
    entities.CashReceiptEvent.TotalCashAmt = totalCashAmt;
    entities.CashReceiptEvent.TotalCashTransactionCount =
      totalCashTransactionCount;
    entities.CashReceiptEvent.Populated = true;
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
    /// A value of CashReceiptDetail.
    /// </summary>
    [JsonPropertyName("cashReceiptDetail")]
    public CashReceiptDetail CashReceiptDetail
    {
      get => cashReceiptDetail ??= new();
      set => cashReceiptDetail = value;
    }

    /// <summary>
    /// A value of CashReceiptEvent.
    /// </summary>
    [JsonPropertyName("cashReceiptEvent")]
    public CashReceiptEvent CashReceiptEvent
    {
      get => cashReceiptEvent ??= new();
      set => cashReceiptEvent = value;
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
    /// A value of CashReceiptType.
    /// </summary>
    [JsonPropertyName("cashReceiptType")]
    public CashReceiptType CashReceiptType
    {
      get => cashReceiptType ??= new();
      set => cashReceiptType = value;
    }

    /// <summary>
    /// A value of ExportRecCollections.
    /// </summary>
    [JsonPropertyName("exportRecCollections")]
    public Common ExportRecCollections
    {
      get => exportRecCollections ??= new();
      set => exportRecCollections = value;
    }

    /// <summary>
    /// A value of ExportRelCollections.
    /// </summary>
    [JsonPropertyName("exportRelCollections")]
    public Common ExportRelCollections
    {
      get => exportRelCollections ??= new();
      set => exportRelCollections = value;
    }

    /// <summary>
    /// A value of ExportSusCollections.
    /// </summary>
    [JsonPropertyName("exportSusCollections")]
    public Common ExportSusCollections
    {
      get => exportSusCollections ??= new();
      set => exportSusCollections = value;
    }

    private CashReceiptDetail cashReceiptDetail;
    private CashReceiptEvent cashReceiptEvent;
    private CashReceiptSourceType cashReceiptSourceType;
    private CashReceiptType cashReceiptType;
    private Common exportRecCollections;
    private Common exportRelCollections;
    private Common exportSusCollections;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>A HardcodedViewsGroup group.</summary>
    [Serializable]
    public class HardcodedViewsGroup
    {
      /// <summary>
      /// A value of HardcodedReleased.
      /// </summary>
      [JsonPropertyName("hardcodedReleased")]
      public CashReceiptDetailStatus HardcodedReleased
      {
        get => hardcodedReleased ??= new();
        set => hardcodedReleased = value;
      }

      /// <summary>
      /// A value of HardcodedRecorded.
      /// </summary>
      [JsonPropertyName("hardcodedRecorded")]
      public CashReceiptDetailStatus HardcodedRecorded
      {
        get => hardcodedRecorded ??= new();
        set => hardcodedRecorded = value;
      }

      /// <summary>
      /// A value of HardcodedSuspended.
      /// </summary>
      [JsonPropertyName("hardcodedSuspended")]
      public CashReceiptDetailStatus HardcodedSuspended
      {
        get => hardcodedSuspended ??= new();
        set => hardcodedSuspended = value;
      }

      /// <summary>
      /// A value of HardcodedRefunded.
      /// </summary>
      [JsonPropertyName("hardcodedRefunded")]
      public CashReceiptDetailStatus HardcodedRefunded
      {
        get => hardcodedRefunded ??= new();
        set => hardcodedRefunded = value;
      }

      /// <summary>
      /// A value of HardcodedPended.
      /// </summary>
      [JsonPropertyName("hardcodedPended")]
      public CashReceiptDetailStatus HardcodedPended
      {
        get => hardcodedPended ??= new();
        set => hardcodedPended = value;
      }

      private CashReceiptDetailStatus hardcodedReleased;
      private CashReceiptDetailStatus hardcodedRecorded;
      private CashReceiptDetailStatus hardcodedSuspended;
      private CashReceiptDetailStatus hardcodedRefunded;
      private CashReceiptDetailStatus hardcodedPended;
    }

    /// <summary>
    /// A value of Create.
    /// </summary>
    [JsonPropertyName("create")]
    public CashReceiptDetailAddress Create
    {
      get => create ??= new();
      set => create = value;
    }

    /// <summary>
    /// A value of Returned.
    /// </summary>
    [JsonPropertyName("returned")]
    public CsePersonAddress Returned
    {
      get => returned ??= new();
      set => returned = value;
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
    /// Gets a value of HardcodedViews.
    /// </summary>
    [JsonPropertyName("hardcodedViews")]
    public HardcodedViewsGroup HardcodedViews
    {
      get => hardcodedViews ?? (hardcodedViews = new());
      set => hardcodedViews = value;
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
    /// A value of U.
    /// </summary>
    [JsonPropertyName("u")]
    public CollectionType U
    {
      get => u ??= new();
      set => u = value;
    }

    /// <summary>
    /// A value of CashReceiptDetailStatus.
    /// </summary>
    [JsonPropertyName("cashReceiptDetailStatus")]
    public CashReceiptDetailStatus CashReceiptDetailStatus
    {
      get => cashReceiptDetailStatus ??= new();
      set => cashReceiptDetailStatus = value;
    }

    /// <summary>
    /// A value of CashReceiptDetailStatHistory.
    /// </summary>
    [JsonPropertyName("cashReceiptDetailStatHistory")]
    public CashReceiptDetailStatHistory CashReceiptDetailStatHistory
    {
      get => cashReceiptDetailStatHistory ??= new();
      set => cashReceiptDetailStatHistory = value;
    }

    /// <summary>
    /// A value of HardcodedInvalidPersonNumber.
    /// </summary>
    [JsonPropertyName("hardcodedInvalidPersonNumber")]
    public CashReceiptDetailStatHistory HardcodedInvalidPersonNumber
    {
      get => hardcodedInvalidPersonNumber ??= new();
      set => hardcodedInvalidPersonNumber = value;
    }

    private CashReceiptDetailAddress create;
    private CsePersonAddress returned;
    private CsePerson csePerson;
    private CsePersonsWorkSet csePersonsWorkSet;
    private HardcodedViewsGroup hardcodedViews;
    private CashReceiptDetail cashReceiptDetail;
    private CollectionType u;
    private CashReceiptDetailStatus cashReceiptDetailStatus;
    private CashReceiptDetailStatHistory cashReceiptDetailStatHistory;
    private CashReceiptDetailStatHistory hardcodedInvalidPersonNumber;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of CashReceiptType.
    /// </summary>
    [JsonPropertyName("cashReceiptType")]
    public CashReceiptType CashReceiptType
    {
      get => cashReceiptType ??= new();
      set => cashReceiptType = value;
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
    /// A value of CashReceiptEvent.
    /// </summary>
    [JsonPropertyName("cashReceiptEvent")]
    public CashReceiptEvent CashReceiptEvent
    {
      get => cashReceiptEvent ??= new();
      set => cashReceiptEvent = value;
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

    private CashReceiptType cashReceiptType;
    private CashReceiptSourceType cashReceiptSourceType;
    private CashReceiptEvent cashReceiptEvent;
    private CashReceipt cashReceipt;
    private CashReceiptDetail cashReceiptDetail;
  }
#endregion
}
