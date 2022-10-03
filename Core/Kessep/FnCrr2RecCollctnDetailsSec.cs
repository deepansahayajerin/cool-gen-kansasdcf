// Program: FN_CRR2_REC_COLLCTN_DETAILS_SEC, ID: 371769984, model: 746.
// Short name: SWECRR2P
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_CRR2_REC_COLLCTN_DETAILS_SEC.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class FnCrr2RecCollctnDetailsSec: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_CRR2_REC_COLLCTN_DETAILS_SEC program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnCrr2RecCollctnDetailsSec(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnCrr2RecCollctnDetailsSec.
  /// </summary>
  public FnCrr2RecCollctnDetailsSec(IContext context, Import import,
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
    // --------------------------------------------
    // Every initial development and change to that
    // development needs to be documented.
    // --------------------------------------------
    // ---------------------------------------------------------------------------------------
    // Date 	  	Developer Name		Request #	Description
    // 02/06/96	Holly Kennedy-MTW			Retrofits
    // 01/02/97	R. Marchman				Add new security/next tran.
    // 10/01/98	Sunya Sharp				Make changes per screen assessment signed 9/22/
    // 98.  Removed clear logic.  It is not on the screen.  On screen PF key
    // properties made PF12 invalid.  Changed FDSO information to be display
    // only.
    // ---------------------------------------------------------------------------------------
    // --------------------------------------------
    // 11/17/99	Paul Phinney   H00080494  Added display of
    // TANF/NON-TANF flag on FDSO Details.
    // ---------------------------------------------
    // --------------------------------------------
    // 11/17/99	Paul Phinney   H00080495  Added display of
    // FEDS NOTIFIED flag on FDSO Details.
    // ---------------------------------------------
    // --------------------------------------------
    // 06/15/00	PPhinney   H00093019  Added display of Joint Return Name (pos 26
    // -60 of Joint Return Name Field).
    // ---------------------------------------------------------
    // 09/05/02	  KDoshi   PR149011 Fix screen Help
    // ---------------------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";
    export.HiddenNextTranInfo.Assign(import.HiddenNextTranInfo);
    export.CashReceipt.Assign(import.CashReceipt);
    export.CashReceiptDetail.Assign(import.CashReceiptDetail);
    MoveCashReceiptSourceType(import.CashReceiptSourceType,
      export.CashReceiptSourceType);
    MoveCollectionType(import.CollectionType, export.CollectionType);
    export.HiddenCashReceiptEvent.SystemGeneratedIdentifier =
      import.HiddenCashReceiptEvent.SystemGeneratedIdentifier;
    export.HiddenCashReceiptType.SystemGeneratedIdentifier =
      import.HiddenCashReceiptType.SystemGeneratedIdentifier;
    export.Standard.NextTransaction = import.Standard.NextTransaction;

    // 11/17/99	Paul Phinney   H00080495
    export.FedsNotified.Flag = import.FedsNotified.Flag;

    // 11/17/99	Paul Phinney   H00080494
    export.TanfFlag.OneChar = import.TanfFlag.OneChar;

    // 06/15/00	      PPhinney     H00093019
    export.DisplayJointName.JointReturnName =
      import.DisplayJointName.JointReturnName;
    export.Display.Assign(import.Display);

    // if the next tran info is not equal to spaces, this implies the user 
    // requested a next tran action. now validate
    if (IsEmpty(import.Standard.NextTransaction))
    {
    }
    else
    {
      // ****
      // No values to set
      // ****
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
      // ****
      // No values to set
      // ****
      UseScCabNextTranGet();
      global.Command = "DISPLAY";

      return;
    }

    if (Equal(global.Command, "XXFMMENU"))
    {
      // --------------------------------------------
      // 11/17/99	Paul Phinney   H00080494 and H00080495
      export.TanfFlag.OneChar = "";
      export.FedsNotified.Flag = "";

      // 06/15/00	      PPhinney     H00093019
      export.DisplayJointName.JointReturnName = "";

      if (Equal(import.CashReceiptSourceType.Code, "FDSO"))
      {
        // --------------------------------------------
        // 11/17/99	Paul Phinney   H00080494  Added display of
        // TANF/NON-TANF flag on FDSO Details.
        // ---------------------------------------------
        export.TanfFlag.OneChar =
          Substring(import.CashReceiptDetail.JointReturnName, 13, 1);

        // --------------------------------------------
        // 06/15/00	PPhinney   H00093019  Added display of Joint Return Name (
        // pos 26-60 of Joint Return Name Field).
        if (AsChar(export.CashReceiptDetail.JointReturnInd) == 'Y')
        {
          export.DisplayJointName.JointReturnName =
            Substring(import.CashReceiptDetail.JointReturnName, 26, 35);
          local.AuthorizedForAddress.Flag = "N";

          if (ReadServiceProviderProfileServiceProviderProfile())
          {
            local.AuthorizedForAddress.Flag = "Y";
          }

          if (AsChar(local.AuthorizedForAddress.Flag) == 'Y')
          {
            if (ReadCashReceiptDetailAddress())
            {
              export.Display.Assign(entities.CashReceiptDetailAddress);
            }
            else
            {
              export.Display.Assign(local.Blank);
              export.Display.Street1 = "Address NOT Availiable";
            }
          }
          else
          {
            export.Display.Assign(local.Blank);
            export.Display.Street1 = "Address Blocked";
          }
        }

        // --------------------------------------------
        // 11/17/99	Paul Phinney   H00080495  Added display of
        // FEDS NOTIFIED flag on FDSO Details.
        // ---------------------------------------------
        export.FedsNotified.Flag = "N";

        if (ReadReceiptRefund())
        {
          export.FedsNotified.Flag = "Y";
        }
      }

      global.Command = "DISPLAY";

      return;
    }

    // to validate action level security
    UseScCabTestSecurity();

    if (IsExitState("ACO_NN0000_ALL_OK"))
    {
    }
    else
    {
      return;
    }

    switch(TrimEnd(global.Command))
    {
      case "":
        break;
      case "UPDATE":
        if (Lt(local.NullDate.DateTransmitted,
          export.CashReceiptDetail.CruProcessedDate) && !
          Lt(local.NullDate.DateTransmitted,
          export.CashReceiptDetail.JfaReceivedDate))
        {
          var field = GetField(export.CashReceiptDetail, "jfaReceivedDate");

          field.Error = true;

          ExitState = "JFA_DATE_MUST_BE_POPULATED_ALSO";

          return;
        }

        if (Lt(local.NullDate.DateTransmitted,
          export.CashReceiptDetail.JfaReceivedDate) && Lt
          (Now().Date, export.CashReceiptDetail.JfaReceivedDate))
        {
          var field = GetField(export.CashReceiptDetail, "jfaReceivedDate");

          field.Error = true;

          ExitState = "ACO_NE0000_DATE_CANNOT_BE_FUTURE";

          return;
        }

        if (Lt(local.NullDate.DateTransmitted,
          export.CashReceiptDetail.CruProcessedDate) && Lt
          (Now().Date, export.CashReceiptDetail.CruProcessedDate))
        {
          var field = GetField(export.CashReceiptDetail, "cruProcessedDate");

          field.Error = true;

          ExitState = "ACO_NE0000_DATE_CANNOT_BE_FUTURE";

          return;
        }

        if (Lt(local.NullDate.DateTransmitted,
          export.CashReceiptDetail.JfaReceivedDate) && Lt
          (local.NullDate.DateTransmitted,
          export.CashReceiptDetail.CruProcessedDate) && Lt
          (export.CashReceiptDetail.CruProcessedDate,
          export.CashReceiptDetail.JfaReceivedDate))
        {
          var field = GetField(export.CashReceiptDetail, "cruProcessedDate");

          field.Error = true;

          ExitState = "CRU_DATE_LESS_THAN_JFA_DATE";

          return;
        }

        UseFnUpdateCrDetail();

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          ExitState = "ACO_NI0000_SUCCESSFUL_UPDATE";
        }
        else
        {
          UseEabRollbackCics();

          var field =
            GetField(export.CashReceiptDetail, "sequentialIdentifier");

          field.Error = true;
        }

        break;
      case "PREV":
        ExitState = "ECO_XFR_TO_REC_CRRC";

        break;
      case "EXIT":
        ExitState = "ECO_LNK_RETURN_TO_MENU";

        break;
      default:
        ExitState = "ACO_NE0000_INVALID_COMMAND";

        break;
    }
  }

  private static void MoveCashReceiptDetail(CashReceiptDetail source,
    CashReceiptDetail target)
  {
    target.SequentialIdentifier = source.SequentialIdentifier;
    target.PayeeFirstName = source.PayeeFirstName;
    target.PayeeMiddleName = source.PayeeMiddleName;
    target.PayeeLastName = source.PayeeLastName;
    target.Attribute1SupportedPersonFirstName =
      source.Attribute1SupportedPersonFirstName;
    target.Attribute1SupportedPersonMiddleName =
      source.Attribute1SupportedPersonMiddleName;
    target.Attribute1SupportedPersonLastName =
      source.Attribute1SupportedPersonLastName;
    target.Attribute2SupportedPersonFirstName =
      source.Attribute2SupportedPersonFirstName;
    target.Attribute2SupportedPersonLastName =
      source.Attribute2SupportedPersonLastName;
    target.Attribute2SupportedPersonMiddleName =
      source.Attribute2SupportedPersonMiddleName;
    target.JfaReceivedDate = source.JfaReceivedDate;
    target.CruProcessedDate = source.CruProcessedDate;
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

  private void UseEabRollbackCics()
  {
    var useImport = new EabRollbackCics.Import();
    var useExport = new EabRollbackCics.Export();

    Call(EabRollbackCics.Execute, useImport, useExport);
  }

  private void UseFnUpdateCrDetail()
  {
    var useImport = new FnUpdateCrDetail.Import();
    var useExport = new FnUpdateCrDetail.Export();

    useImport.CashReceipt.SequentialNumber =
      export.CashReceipt.SequentialNumber;
    MoveCashReceiptDetail(export.CashReceiptDetail, useImport.CashReceiptDetail);
      
    useImport.CashReceiptSourceType.SystemGeneratedIdentifier =
      export.CashReceiptSourceType.SystemGeneratedIdentifier;
    useImport.CollectionType.SequentialIdentifier =
      export.CollectionType.SequentialIdentifier;
    useImport.CashReceiptEvent.SystemGeneratedIdentifier =
      export.HiddenCashReceiptEvent.SystemGeneratedIdentifier;
    useImport.CashReceiptType.SystemGeneratedIdentifier =
      export.HiddenCashReceiptType.SystemGeneratedIdentifier;

    Call(FnUpdateCrDetail.Execute, useImport, useExport);
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

  private void UseScCabTestSecurity()
  {
    var useImport = new ScCabTestSecurity.Import();
    var useExport = new ScCabTestSecurity.Export();

    Call(ScCabTestSecurity.Execute, useImport, useExport);
  }

  private bool ReadCashReceiptDetailAddress()
  {
    entities.CashReceiptDetailAddress.Populated = false;

    return Read("ReadCashReceiptDetailAddress",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "crdIdentifier",
          import.CashReceiptDetail.SequentialIdentifier);
        db.SetInt32(
          command, "cashReceiptId", export.CashReceipt.SequentialNumber);
      },
      (db, reader) =>
      {
        entities.CashReceiptDetailAddress.SystemGeneratedIdentifier =
          db.GetDateTime(reader, 0);
        entities.CashReceiptDetailAddress.Street1 = db.GetString(reader, 1);
        entities.CashReceiptDetailAddress.Street2 =
          db.GetNullableString(reader, 2);
        entities.CashReceiptDetailAddress.City = db.GetString(reader, 3);
        entities.CashReceiptDetailAddress.State = db.GetString(reader, 4);
        entities.CashReceiptDetailAddress.ZipCode5 = db.GetString(reader, 5);
        entities.CashReceiptDetailAddress.ZipCode4 =
          db.GetNullableString(reader, 6);
        entities.CashReceiptDetailAddress.CrtIdentifier =
          db.GetNullableInt32(reader, 7);
        entities.CashReceiptDetailAddress.CstIdentifier =
          db.GetNullableInt32(reader, 8);
        entities.CashReceiptDetailAddress.CrvIdentifier =
          db.GetNullableInt32(reader, 9);
        entities.CashReceiptDetailAddress.CrdIdentifier =
          db.GetNullableInt32(reader, 10);
        entities.CashReceiptDetailAddress.Populated = true;
      });
  }

  private bool ReadReceiptRefund()
  {
    entities.ReceiptRefund.Populated = false;

    return Read("ReadReceiptRefund",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "crdIdentifier",
          import.CashReceiptDetail.SequentialIdentifier);
        db.SetInt32(
          command, "cashReceiptId", import.CashReceipt.SequentialNumber);
        db.SetNullableDate(
          command, "dateTransmitted",
          local.NullDate.DateTransmitted.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ReceiptRefund.CreatedTimestamp = db.GetDateTime(reader, 0);
        entities.ReceiptRefund.CrvIdentifier = db.GetNullableInt32(reader, 1);
        entities.ReceiptRefund.CrdIdentifier = db.GetNullableInt32(reader, 2);
        entities.ReceiptRefund.DateTransmitted = db.GetNullableDate(reader, 3);
        entities.ReceiptRefund.CrtIdentifier = db.GetNullableInt32(reader, 4);
        entities.ReceiptRefund.CstIdentifier = db.GetNullableInt32(reader, 5);
        entities.ReceiptRefund.Populated = true;
      });
  }

  private bool ReadServiceProviderProfileServiceProviderProfile()
  {
    entities.ProfileAuthorization.Populated = false;
    entities.ServiceProviderProfile.Populated = false;
    entities.ServiceProvider.Populated = false;
    entities.Profile.Populated = false;

    return Read("ReadServiceProviderProfileServiceProviderProfile",
      (db, command) =>
      {
        var date = Now().Date;

        db.SetString(command, "userId", global.UserId);
        db.SetNullableDate(command, "discontinueDate", date);
      },
      (db, reader) =>
      {
        entities.ServiceProviderProfile.CreatedTimestamp =
          db.GetDateTime(reader, 0);
        entities.ServiceProviderProfile.EffectiveDate = db.GetDate(reader, 1);
        entities.ServiceProviderProfile.DiscontinueDate =
          db.GetNullableDate(reader, 2);
        entities.ServiceProviderProfile.ProName = db.GetString(reader, 3);
        entities.Profile.Name = db.GetString(reader, 3);
        entities.ServiceProviderProfile.SpdGenId = db.GetInt32(reader, 4);
        entities.ServiceProvider.SystemGeneratedId = db.GetInt32(reader, 4);
        entities.ServiceProvider.UserId = db.GetString(reader, 5);
        entities.ProfileAuthorization.CreatedTimestamp =
          db.GetDateTime(reader, 6);
        entities.ProfileAuthorization.ActiveInd =
          db.GetNullableString(reader, 7);
        entities.ProfileAuthorization.FkProName = db.GetString(reader, 8);
        entities.ProfileAuthorization.FkTrnTrancode = db.GetString(reader, 9);
        entities.ProfileAuthorization.FkTrnScreenid = db.GetString(reader, 10);
        entities.ProfileAuthorization.FkCmdValue = db.GetString(reader, 11);
        entities.ProfileAuthorization.Populated = true;
        entities.ServiceProviderProfile.Populated = true;
        entities.ServiceProvider.Populated = true;
        entities.Profile.Populated = true;
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
    /// A value of CashReceiptSourceType.
    /// </summary>
    [JsonPropertyName("cashReceiptSourceType")]
    public CashReceiptSourceType CashReceiptSourceType
    {
      get => cashReceiptSourceType ??= new();
      set => cashReceiptSourceType = value;
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
    /// A value of FedsNotified.
    /// </summary>
    [JsonPropertyName("fedsNotified")]
    public Common FedsNotified
    {
      get => fedsNotified ??= new();
      set => fedsNotified = value;
    }

    /// <summary>
    /// A value of TanfFlag.
    /// </summary>
    [JsonPropertyName("tanfFlag")]
    public Standard TanfFlag
    {
      get => tanfFlag ??= new();
      set => tanfFlag = value;
    }

    /// <summary>
    /// A value of DisplayJointName.
    /// </summary>
    [JsonPropertyName("displayJointName")]
    public CashReceiptDetail DisplayJointName
    {
      get => displayJointName ??= new();
      set => displayJointName = value;
    }

    /// <summary>
    /// A value of Display.
    /// </summary>
    [JsonPropertyName("display")]
    public CashReceiptDetailAddress Display
    {
      get => display ??= new();
      set => display = value;
    }

    /// <summary>
    /// A value of JfaProcessed.
    /// </summary>
    [JsonPropertyName("jfaProcessed")]
    public DateWorkArea JfaProcessed
    {
      get => jfaProcessed ??= new();
      set => jfaProcessed = value;
    }

    /// <summary>
    /// A value of JfaReceivedDateWorkArea.
    /// </summary>
    [JsonPropertyName("jfaReceivedDateWorkArea")]
    public DateWorkArea JfaReceivedDateWorkArea
    {
      get => jfaReceivedDateWorkArea ??= new();
      set => jfaReceivedDateWorkArea = value;
    }

    /// <summary>
    /// A value of InjuredSpouseTaxesFile.
    /// </summary>
    [JsonPropertyName("injuredSpouseTaxesFile")]
    public WorkArea InjuredSpouseTaxesFile
    {
      get => injuredSpouseTaxesFile ??= new();
      set => injuredSpouseTaxesFile = value;
    }

    /// <summary>
    /// A value of InjuredSpouse.
    /// </summary>
    [JsonPropertyName("injuredSpouse")]
    public WorkArea InjuredSpouse
    {
      get => injuredSpouse ??= new();
      set => injuredSpouse = value;
    }

    /// <summary>
    /// A value of JfaReceivedWorkArea.
    /// </summary>
    [JsonPropertyName("jfaReceivedWorkArea")]
    public WorkArea JfaReceivedWorkArea
    {
      get => jfaReceivedWorkArea ??= new();
      set => jfaReceivedWorkArea = value;
    }

    private CashReceipt cashReceipt;
    private CashReceiptDetail cashReceiptDetail;
    private CashReceiptSourceType cashReceiptSourceType;
    private CollectionType collectionType;
    private CashReceiptEvent hiddenCashReceiptEvent;
    private CashReceiptType hiddenCashReceiptType;
    private NextTranInfo hiddenNextTranInfo;
    private Standard standard;
    private Common fedsNotified;
    private Standard tanfFlag;
    private CashReceiptDetail displayJointName;
    private CashReceiptDetailAddress display;
    private DateWorkArea jfaProcessed;
    private DateWorkArea jfaReceivedDateWorkArea;
    private WorkArea injuredSpouseTaxesFile;
    private WorkArea injuredSpouse;
    private WorkArea jfaReceivedWorkArea;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
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
    /// A value of CashReceiptSourceType.
    /// </summary>
    [JsonPropertyName("cashReceiptSourceType")]
    public CashReceiptSourceType CashReceiptSourceType
    {
      get => cashReceiptSourceType ??= new();
      set => cashReceiptSourceType = value;
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
    /// A value of FedsNotified.
    /// </summary>
    [JsonPropertyName("fedsNotified")]
    public Common FedsNotified
    {
      get => fedsNotified ??= new();
      set => fedsNotified = value;
    }

    /// <summary>
    /// A value of TanfFlag.
    /// </summary>
    [JsonPropertyName("tanfFlag")]
    public Standard TanfFlag
    {
      get => tanfFlag ??= new();
      set => tanfFlag = value;
    }

    /// <summary>
    /// A value of DisplayJointName.
    /// </summary>
    [JsonPropertyName("displayJointName")]
    public CashReceiptDetail DisplayJointName
    {
      get => displayJointName ??= new();
      set => displayJointName = value;
    }

    /// <summary>
    /// A value of Display.
    /// </summary>
    [JsonPropertyName("display")]
    public CashReceiptDetailAddress Display
    {
      get => display ??= new();
      set => display = value;
    }

    /// <summary>
    /// A value of JfaProcessed.
    /// </summary>
    [JsonPropertyName("jfaProcessed")]
    public DateWorkArea JfaProcessed
    {
      get => jfaProcessed ??= new();
      set => jfaProcessed = value;
    }

    /// <summary>
    /// A value of JfaReceivedDateWorkArea.
    /// </summary>
    [JsonPropertyName("jfaReceivedDateWorkArea")]
    public DateWorkArea JfaReceivedDateWorkArea
    {
      get => jfaReceivedDateWorkArea ??= new();
      set => jfaReceivedDateWorkArea = value;
    }

    /// <summary>
    /// A value of InjuredSpouseTaxesFile.
    /// </summary>
    [JsonPropertyName("injuredSpouseTaxesFile")]
    public WorkArea InjuredSpouseTaxesFile
    {
      get => injuredSpouseTaxesFile ??= new();
      set => injuredSpouseTaxesFile = value;
    }

    /// <summary>
    /// A value of InjuredSpouse.
    /// </summary>
    [JsonPropertyName("injuredSpouse")]
    public WorkArea InjuredSpouse
    {
      get => injuredSpouse ??= new();
      set => injuredSpouse = value;
    }

    /// <summary>
    /// A value of JfaReceivedWorkArea.
    /// </summary>
    [JsonPropertyName("jfaReceivedWorkArea")]
    public WorkArea JfaReceivedWorkArea
    {
      get => jfaReceivedWorkArea ??= new();
      set => jfaReceivedWorkArea = value;
    }

    private CashReceipt cashReceipt;
    private CashReceiptDetail cashReceiptDetail;
    private CashReceiptSourceType cashReceiptSourceType;
    private CollectionType collectionType;
    private CashReceiptEvent hiddenCashReceiptEvent;
    private CashReceiptType hiddenCashReceiptType;
    private NextTranInfo hiddenNextTranInfo;
    private Standard standard;
    private Common fedsNotified;
    private Standard tanfFlag;
    private CashReceiptDetail displayJointName;
    private CashReceiptDetailAddress display;
    private DateWorkArea jfaProcessed;
    private DateWorkArea jfaReceivedDateWorkArea;
    private WorkArea injuredSpouseTaxesFile;
    private WorkArea injuredSpouse;
    private WorkArea jfaReceivedWorkArea;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of AuthorizedForAddress.
    /// </summary>
    [JsonPropertyName("authorizedForAddress")]
    public Common AuthorizedForAddress
    {
      get => authorizedForAddress ??= new();
      set => authorizedForAddress = value;
    }

    /// <summary>
    /// A value of Blank.
    /// </summary>
    [JsonPropertyName("blank")]
    public CashReceiptDetailAddress Blank
    {
      get => blank ??= new();
      set => blank = value;
    }

    /// <summary>
    /// A value of NullDate.
    /// </summary>
    [JsonPropertyName("nullDate")]
    public ReceiptRefund NullDate
    {
      get => nullDate ??= new();
      set => nullDate = value;
    }

    private Common authorizedForAddress;
    private CashReceiptDetailAddress blank;
    private ReceiptRefund nullDate;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of ProfileAuthorization.
    /// </summary>
    [JsonPropertyName("profileAuthorization")]
    public ProfileAuthorization ProfileAuthorization
    {
      get => profileAuthorization ??= new();
      set => profileAuthorization = value;
    }

    /// <summary>
    /// A value of ServiceProviderProfile.
    /// </summary>
    [JsonPropertyName("serviceProviderProfile")]
    public ServiceProviderProfile ServiceProviderProfile
    {
      get => serviceProviderProfile ??= new();
      set => serviceProviderProfile = value;
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
    /// A value of Profile.
    /// </summary>
    [JsonPropertyName("profile")]
    public Profile Profile
    {
      get => profile ??= new();
      set => profile = value;
    }

    /// <summary>
    /// A value of CashReceiptDetailAddress.
    /// </summary>
    [JsonPropertyName("cashReceiptDetailAddress")]
    public CashReceiptDetailAddress CashReceiptDetailAddress
    {
      get => cashReceiptDetailAddress ??= new();
      set => cashReceiptDetailAddress = value;
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
    /// A value of CashReceipt.
    /// </summary>
    [JsonPropertyName("cashReceipt")]
    public CashReceipt CashReceipt
    {
      get => cashReceipt ??= new();
      set => cashReceipt = value;
    }

    /// <summary>
    /// A value of ReceiptRefund.
    /// </summary>
    [JsonPropertyName("receiptRefund")]
    public ReceiptRefund ReceiptRefund
    {
      get => receiptRefund ??= new();
      set => receiptRefund = value;
    }

    private ProfileAuthorization profileAuthorization;
    private ServiceProviderProfile serviceProviderProfile;
    private ServiceProvider serviceProvider;
    private Profile profile;
    private CashReceiptDetailAddress cashReceiptDetailAddress;
    private CashReceiptDetail cashReceiptDetail;
    private CashReceipt cashReceipt;
    private ReceiptRefund receiptRefund;
  }
#endregion
}
