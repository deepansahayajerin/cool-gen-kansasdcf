// Program: FN_RCAP_CREATE_CASH_RECEIPT, ID: 372674868, model: 746.
// Short name: SWE02112
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: FN_RCAP_CREATE_CASH_RECEIPT.
/// </para>
/// <para/>
/// </summary>
[Serializable]
public partial class FnRcapCreateCashReceipt: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_RCAP_CREATE_CASH_RECEIPT program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnRcapCreateCashReceipt(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnRcapCreateCashReceipt.
  /// </summary>
  public FnRcapCreateCashReceipt(IContext context, Import import, Export export):
    
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ---------------------------------------------
    // This action block creates cash receipt detail for the given
    // obligee and the recaptured amount.
    // ---------------------------------------------
    // ---------------------------------------------
    // Date	By	IDCR #	Description
    // 100297	govind		Initial code
    // 022398	govind		Matched views again for fn_distribute ..
    // 			Renamed to FN_RCAP_...
    // 030798	govind		Added the flag for secondary
    // 			debts to the call to FN
    // 			DISTRIBUTE CASH RECEIPT
    // 			DETAIL.
    // 03/15/99  N.Engoor      Changed the Collection type being read from 'C' 
    // to 'R'.
    // 042299   Fangman        Removed logic to call outdated distribution 
    // routines.  Distribution will happen in the distribution process.
    // 05/22/99  Fangman    Added changes discovered in walkthrus.
    // 06/08/99  Fangman    Added Balance Timestamp, Deposit Release Date and 
    // removed Check Date from Cash Receipt per Judy & Jennifers request.
    // 06/18/99  Fangman Set Cash_Receipt_detail Joint_ind to spaces per Sunya.
    // 06/22/99  Fangman  Added create a cash receipt detail address per Sunya.
    // ---------------------------------------------
    if (!ReadCollectionType())
    {
      ExitState = "FN0000_INV_COLL_TYPE";

      return;
    }

    local.CsePersonsWorkSet.Number = import.Obligee.Number;
    UseSiReadCsePersonBatch();

    if (!IsEmpty(local.AbendData.AdabasResponseCd))
    {
      ExitState = "FN0000_ADABAS_ERRR_PERS_NO_F_SSN";

      return;
    }

    UseCabFnReadCsePersonAddress();

    if (IsExitState("CSE_PERSON_ADDRESS_NF"))
    {
      local.PassCsePersonAddress.Assign(local.InitializedCsePersonAddress);
      local.PassCsePersonAddress.Street1 = "address not found";
      ExitState = "ACO_NN0000_ALL_OK";
    }

    local.PassCashReceipt.ReceiptAmount = import.Recapture.Amount;
    local.PassCashReceipt.ReceiptDate =
      import.ProgramProcessingInfo.ProcessDate;
    local.PassCashReceipt.DepositReleaseDate =
      import.ProgramProcessingInfo.ProcessDate;
    local.PassCashReceipt.CheckType = "CSE";
    local.PassCashReceipt.CheckDate = local.InitializedDateWorkArea.Date;
    local.PassCashReceipt.ReceivedDate =
      import.ProgramProcessingInfo.ProcessDate;
    local.PassCashReceipt.Note =
      "Recapture Amount converted to a Cash Receipt for distribution against the recovery debts";
      
    local.PassCashReceipt.PayorFirstName = local.CsePersonsWorkSet.FirstName;
    local.PassCashReceipt.PayorLastName = local.CsePersonsWorkSet.LastName;
    local.PassCashReceipt.PayorMiddleName =
      local.CsePersonsWorkSet.MiddleInitial;
    local.PassCashReceipt.BalancedTimestamp = Now();

    // *****
    // Set Cash Receipt Detail Views to pass to Create Process.
    // *****
    local.PassCashReceiptDetail.SequentialIdentifier = 1;
    local.PassCashReceiptDetail.ReceivedAmount = import.Recapture.Amount;
    local.PassCashReceiptDetail.CollectionAmount = import.Recapture.Amount;
    local.PassCashReceiptDetail.CollectionDate =
      import.ProgramProcessingInfo.ProcessDate;
    local.PassCashReceiptDetail.ObligorPersonNumber =
      local.CsePersonsWorkSet.Number;
    local.PassCashReceiptDetail.ObligorSocialSecurityNumber =
      local.CsePersonsWorkSet.Ssn;
    local.PassCashReceiptDetail.ObligorFirstName =
      local.CsePersonsWorkSet.FirstName;
    local.PassCashReceiptDetail.ObligorLastName =
      local.CsePersonsWorkSet.LastName;
    local.PassCashReceiptDetail.ObligorMiddleName =
      local.CsePersonsWorkSet.MiddleInitial;
    local.PassCashReceiptDetail.Notes =
      "Recapture Amount converted to a Cash Receipt Detail for distribution";
    local.PassCashReceiptDetail.MultiPayor = "";
    local.HardcodedRecorded.SystemGeneratedIdentifier = 1;
    local.HardcodedReleased.SystemGeneratedIdentifier = 6;

    if (ReadCashReceiptStatus())
    {
      if (ReadCashReceiptDetailStatus())
      {
        if (ReadCashReceiptSourceType())
        {
          // --- Read Cash Receipt Type for Recaptures
          if (ReadCashReceiptType())
          {
            // --- Create Cash Receipt, Cash Receipt Status History, Cash 
            // Receipt Detail and Cash Receipt Receipt Detail Status History
            // The view local_continue is set to "Y" in order to ignore 
            // duplicate cash receipt details on the same date for the same
            // person if any
            local.Continue1.Flag = "Y";
            UseFnCreateCashRcptHistory();
          }
          else
          {
            ExitState = "FN0113_CASH_RCPT_TYPE_NF";
          }
        }
        else
        {
          ExitState = "FN0097_CASH_RCPT_SOURCE_TYPE_NF";
        }
      }
      else
      {
        ExitState = "FN0071_CASH_RCPT_DTL_STAT_NF";
      }
    }
    else
    {
      ExitState = "FN0108_CASH_RCPT_STAT_NF";
    }
  }

  private static void MoveCashReceiptDetail(CashReceiptDetail source,
    CashReceiptDetail target)
  {
    target.SequentialIdentifier = source.SequentialIdentifier;
    target.CourtOrderNumber = source.CourtOrderNumber;
    target.ReceivedAmount = source.ReceivedAmount;
    target.CollectionAmount = source.CollectionAmount;
    target.CollectionDate = source.CollectionDate;
    target.MultiPayor = source.MultiPayor;
    target.ObligorPersonNumber = source.ObligorPersonNumber;
    target.ObligorSocialSecurityNumber = source.ObligorSocialSecurityNumber;
    target.ObligorFirstName = source.ObligorFirstName;
    target.ObligorLastName = source.ObligorLastName;
    target.ObligorMiddleName = source.ObligorMiddleName;
    target.Reference = source.Reference;
    target.Notes = source.Notes;
  }

  private void UseCabFnReadCsePersonAddress()
  {
    var useImport = new CabFnReadCsePersonAddress.Import();
    var useExport = new CabFnReadCsePersonAddress.Export();

    useImport.CsePersonsWorkSet.Number = local.CsePersonsWorkSet.Number;

    Call(CabFnReadCsePersonAddress.Execute, useImport, useExport);

    local.PassCsePersonAddress.Assign(useExport.CsePersonAddress);
  }

  private void UseFnCreateCashRcptHistory()
  {
    var useImport = new FnCreateCashRcptHistory.Import();
    var useExport = new FnCreateCashRcptHistory.Export();

    useImport.CsePerson.Number = import.Obligee.Number;
    useImport.CollectionType.Code = entities.CollectionType.Code;
    useImport.Continue1.Flag = local.Continue1.Flag;
    useImport.CashReceiptDetailStatus.SystemGeneratedIdentifier =
      entities.CashReceiptDetailStatus.SystemGeneratedIdentifier;
    MoveCashReceiptDetail(local.PassCashReceiptDetail,
      useImport.CashReceiptDetail);
    useImport.CashReceiptStatus.SystemGeneratedIdentifier =
      entities.CashReceiptStatus.SystemGeneratedIdentifier;
    useImport.CashReceipt.Assign(local.PassCashReceipt);
    useImport.CashReceiptType.SystemGeneratedIdentifier =
      entities.CashReceiptType.SystemGeneratedIdentifier;
    useImport.CashReceiptSourceType.SystemGeneratedIdentifier =
      entities.CashReceiptSourceType.SystemGeneratedIdentifier;
    useImport.CsePersonAddress.Assign(local.PassCsePersonAddress);

    Call(FnCreateCashRcptHistory.Execute, useImport, useExport);

    local.PassCashReceipt.SequentialNumber =
      useExport.CashReceipt.SequentialNumber;
  }

  private void UseSiReadCsePersonBatch()
  {
    var useImport = new SiReadCsePersonBatch.Import();
    var useExport = new SiReadCsePersonBatch.Export();

    useImport.CsePersonsWorkSet.Number = local.CsePersonsWorkSet.Number;

    Call(SiReadCsePersonBatch.Execute, useImport, useExport);

    local.AbendData.Assign(useExport.AbendData);
    local.CsePersonsWorkSet.Assign(useExport.CsePersonsWorkSet);
  }

  private bool ReadCashReceiptDetailStatus()
  {
    entities.CashReceiptDetailStatus.Populated = false;

    return Read("ReadCashReceiptDetailStatus",
      (db, command) =>
      {
        db.SetInt32(
          command, "crdetailStatId",
          local.HardcodedReleased.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.CashReceiptDetailStatus.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.CashReceiptDetailStatus.Populated = true;
      });
  }

  private bool ReadCashReceiptSourceType()
  {
    entities.CashReceiptSourceType.Populated = false;

    return Read("ReadCashReceiptSourceType",
      (db, command) =>
      {
        db.SetDate(
          command, "effectiveDate",
          import.ProgramProcessingInfo.ProcessDate.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CashReceiptSourceType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.CashReceiptSourceType.Code = db.GetString(reader, 1);
        entities.CashReceiptSourceType.EffectiveDate = db.GetDate(reader, 2);
        entities.CashReceiptSourceType.DiscontinueDate =
          db.GetNullableDate(reader, 3);
        entities.CashReceiptSourceType.CourtInd =
          db.GetNullableString(reader, 4);
        entities.CashReceiptSourceType.Populated = true;
      });
  }

  private bool ReadCashReceiptStatus()
  {
    entities.CashReceiptStatus.Populated = false;

    return Read("ReadCashReceiptStatus",
      (db, command) =>
      {
        db.SetInt32(
          command, "crStatusId",
          local.HardcodedRecorded.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.CashReceiptStatus.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.CashReceiptStatus.Code = db.GetString(reader, 1);
        entities.CashReceiptStatus.Populated = true;
      });
  }

  private bool ReadCashReceiptType()
  {
    entities.CashReceiptType.Populated = false;

    return Read("ReadCashReceiptType",
      (db, command) =>
      {
        db.SetDate(
          command, "effectiveDate",
          import.ProgramProcessingInfo.ProcessDate.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CashReceiptType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.CashReceiptType.Code = db.GetString(reader, 1);
        entities.CashReceiptType.CategoryIndicator = db.GetString(reader, 2);
        entities.CashReceiptType.EffectiveDate = db.GetDate(reader, 3);
        entities.CashReceiptType.DiscontinueDate =
          db.GetNullableDate(reader, 4);
        entities.CashReceiptType.Populated = true;
        CheckValid<CashReceiptType>("CategoryIndicator",
          entities.CashReceiptType.CategoryIndicator);
      });
  }

  private bool ReadCollectionType()
  {
    entities.CollectionType.Populated = false;

    return Read("ReadCollectionType",
      null,
      (db, reader) =>
      {
        entities.CollectionType.SequentialIdentifier = db.GetInt32(reader, 0);
        entities.CollectionType.Code = db.GetString(reader, 1);
        entities.CollectionType.Populated = true;
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
    /// A value of ProgramProcessingInfo.
    /// </summary>
    [JsonPropertyName("programProcessingInfo")]
    public ProgramProcessingInfo ProgramProcessingInfo
    {
      get => programProcessingInfo ??= new();
      set => programProcessingInfo = value;
    }

    /// <summary>
    /// A value of Recapture.
    /// </summary>
    [JsonPropertyName("recapture")]
    public PaymentRequest Recapture
    {
      get => recapture ??= new();
      set => recapture = value;
    }

    /// <summary>
    /// A value of Obligee.
    /// </summary>
    [JsonPropertyName("obligee")]
    public CsePerson Obligee
    {
      get => obligee ??= new();
      set => obligee = value;
    }

    private ProgramProcessingInfo programProcessingInfo;
    private PaymentRequest recapture;
    private CsePerson obligee;
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
    /// <summary>
    /// A value of InitializedCsePersonAddress.
    /// </summary>
    [JsonPropertyName("initializedCsePersonAddress")]
    public CsePersonAddress InitializedCsePersonAddress
    {
      get => initializedCsePersonAddress ??= new();
      set => initializedCsePersonAddress = value;
    }

    /// <summary>
    /// A value of PassCsePersonAddress.
    /// </summary>
    [JsonPropertyName("passCsePersonAddress")]
    public CsePersonAddress PassCsePersonAddress
    {
      get => passCsePersonAddress ??= new();
      set => passCsePersonAddress = value;
    }

    /// <summary>
    /// A value of AbendData.
    /// </summary>
    [JsonPropertyName("abendData")]
    public AbendData AbendData
    {
      get => abendData ??= new();
      set => abendData = value;
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
    public CashReceiptStatus HardcodedRecorded
    {
      get => hardcodedRecorded ??= new();
      set => hardcodedRecorded = value;
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
    /// A value of InitializedDateWorkArea.
    /// </summary>
    [JsonPropertyName("initializedDateWorkArea")]
    public DateWorkArea InitializedDateWorkArea
    {
      get => initializedDateWorkArea ??= new();
      set => initializedDateWorkArea = value;
    }

    private CsePersonAddress initializedCsePersonAddress;
    private CsePersonAddress passCsePersonAddress;
    private AbendData abendData;
    private Common continue1;
    private CashReceiptDetailStatus hardcodedReleased;
    private CashReceiptStatus hardcodedRecorded;
    private CsePersonsWorkSet csePersonsWorkSet;
    private CashReceipt passCashReceipt;
    private CashReceiptDetail passCashReceiptDetail;
    private DateWorkArea initializedDateWorkArea;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of NewRcap.
    /// </summary>
    [JsonPropertyName("newRcap")]
    public CashReceipt NewRcap
    {
      get => newRcap ??= new();
      set => newRcap = value;
    }

    /// <summary>
    /// A value of NewRecap.
    /// </summary>
    [JsonPropertyName("newRecap")]
    public CashReceiptDetail NewRecap
    {
      get => newRecap ??= new();
      set => newRecap = value;
    }

    /// <summary>
    /// A value of CollectionType.
    /// </summary>
    [JsonPropertyName("collectionType")]
    public CollectionType CollectionType
    {
      get => collectionType ??= new();
      set => collectionType = value;
    }

    /// <summary>
    /// A value of CashReceiptStatus.
    /// </summary>
    [JsonPropertyName("cashReceiptStatus")]
    public CashReceiptStatus CashReceiptStatus
    {
      get => cashReceiptStatus ??= new();
      set => cashReceiptStatus = value;
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

    private CashReceipt newRcap;
    private CashReceiptDetail newRecap;
    private CollectionType collectionType;
    private CashReceiptStatus cashReceiptStatus;
    private CashReceiptDetailStatus cashReceiptDetailStatus;
    private CashReceiptSourceType cashReceiptSourceType;
    private CashReceiptType cashReceiptType;
  }
#endregion
}
