// Program: FN_READ_CR_AND_COUNT_CRDTLS, ID: 371770031, model: 746.
// Short name: SWE01946
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_READ_CR_AND_COUNT_CRDTLS.
/// </summary>
[Serializable]
public partial class FnReadCrAndCountCrdtls: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_READ_CR_AND_COUNT_CRDTLS program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnReadCrAndCountCrdtls(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnReadCrAndCountCrdtls.
  /// </summary>
  public FnReadCrAndCountCrdtls(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    export.CashReceipt.SequentialNumber = import.CashReceipt.SequentialNumber;
    export.CashReceiptEvent.SystemGeneratedIdentifier =
      import.CashReceiptEvent.SystemGeneratedIdentifier;
    export.CashReceiptSourceType.SystemGeneratedIdentifier =
      import.CashReceiptSourceType.SystemGeneratedIdentifier;
    export.CashReceiptType.SystemGeneratedIdentifier =
      import.CashReceiptType.SystemGeneratedIdentifier;

    if (import.CashReceiptEvent.SystemGeneratedIdentifier > 0 && import
      .CashReceiptSourceType.SystemGeneratedIdentifier > 0)
    {
      // ---- Control has come from CREC, CREL etc. all identifiers are 
      // populated
      if (!ReadCashReceiptSourceType())
      {
        ExitState = "FN0097_CASH_RCPT_SOURCE_TYPE_NF";

        return;
      }

      if (ReadCashReceipt1())
      {
        MoveCashReceipt(entities.CashReceipt, export.CashReceipt);
        export.CashReceiptSourceType.Assign(entities.CashReceiptSourceType);
      }
      else
      {
        ExitState = "FN0084_CASH_RCPT_NF";

        return;
      }

      export.DeletedCashReceiptFlag.Flag = "N";

      if (ReadCashReceiptStatusHistoryCashReceiptStatus())
      {
        if (Equal(entities.CashReceiptStatus.Code, "DEL"))
        {
          export.DeletedCashReceiptFlag.Flag = "Y";
        }
      }
    }
    else
    {
      // ---- User has entered the receipt number and pressed display
      if (!ReadCashReceipt2())
      {
        ExitState = "FN0084_CASH_RCPT_NF";

        return;
      }

      export.DeletedCashReceiptFlag.Flag = "N";

      if (ReadCashReceiptStatusHistoryCashReceiptStatus())
      {
        if (Equal(entities.CashReceiptStatus.Code, "DEL"))
        {
          export.DeletedCashReceiptFlag.Flag = "Y";
        }
      }

      if (!ReadCashReceiptSourceTypeCashReceiptEvent())
      {
        ExitState = "FN0097_CASH_RCPT_SOURCE_TYPE_NF";

        return;
      }

      if (!ReadCashReceiptType())
      {
        ExitState = "FN0113_CASH_RCPT_TYPE_NF";

        return;
      }

      MoveCashReceipt(entities.CashReceipt, export.CashReceipt);
      export.CashReceiptEvent.SystemGeneratedIdentifier =
        entities.CashReceiptEvent.SystemGeneratedIdentifier;
      export.CashReceiptSourceType.Assign(entities.CashReceiptSourceType);
      export.CashReceiptType.SystemGeneratedIdentifier =
        entities.CashReceiptType.SystemGeneratedIdentifier;
    }

    export.NoOfCrDetails.Count = 0;

    foreach(var item in ReadCashReceiptDetail())
    {
      ++export.NoOfCrDetails.Count;
    }

    if (export.NoOfCrDetails.Count == 1)
    {
      export.CashReceiptDetail.SequentialIdentifier =
        entities.CashReceiptDetail.SequentialIdentifier;
    }
  }

  private static void MoveCashReceipt(CashReceipt source, CashReceipt target)
  {
    target.ReceiptAmount = source.ReceiptAmount;
    target.SequentialNumber = source.SequentialNumber;
    target.CheckNumber = source.CheckNumber;
    target.TotalCashTransactionAmount = source.TotalCashTransactionAmount;
    target.CashDue = source.CashDue;
  }

  private bool ReadCashReceipt1()
  {
    entities.CashReceipt.Populated = false;

    return Read("ReadCashReceipt1",
      (db, command) =>
      {
        db.SetInt32(
          command, "crvIdentifier",
          import.CashReceiptEvent.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "crtIdentifier",
          import.CashReceiptType.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.CashReceipt.CrvIdentifier = db.GetInt32(reader, 0);
        entities.CashReceipt.CstIdentifier = db.GetInt32(reader, 1);
        entities.CashReceipt.CrtIdentifier = db.GetInt32(reader, 2);
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
        entities.CashReceipt.Note = db.GetNullableString(reader, 37);
        entities.CashReceipt.Populated = true;
        CheckValid<CashReceipt>("CashBalanceReason",
          entities.CashReceipt.CashBalanceReason);
      });
  }

  private bool ReadCashReceipt2()
  {
    entities.CashReceipt.Populated = false;

    return Read("ReadCashReceipt2",
      (db, command) =>
      {
        db.SetInt32(
          command, "cashReceiptId", import.CashReceipt.SequentialNumber);
      },
      (db, reader) =>
      {
        entities.CashReceipt.CrvIdentifier = db.GetInt32(reader, 0);
        entities.CashReceipt.CstIdentifier = db.GetInt32(reader, 1);
        entities.CashReceipt.CrtIdentifier = db.GetInt32(reader, 2);
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
        entities.CashReceipt.Note = db.GetNullableString(reader, 37);
        entities.CashReceipt.Populated = true;
        CheckValid<CashReceipt>("CashBalanceReason",
          entities.CashReceipt.CashBalanceReason);
      });
  }

  private IEnumerable<bool> ReadCashReceiptDetail()
  {
    System.Diagnostics.Debug.Assert(entities.CashReceipt.Populated);
    entities.CashReceiptDetail.Populated = false;

    return ReadEach("ReadCashReceiptDetail",
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
        entities.CashReceiptDetail.AdjustmentInd =
          db.GetNullableString(reader, 4);
        entities.CashReceiptDetail.Populated = true;

        return true;
      });
  }

  private bool ReadCashReceiptSourceType()
  {
    entities.CashReceiptSourceType.Populated = false;

    return Read("ReadCashReceiptSourceType",
      (db, command) =>
      {
        db.SetInt32(
          command, "crSrceTypeId",
          import.CashReceiptSourceType.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "creventId",
          import.CashReceiptEvent.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.CashReceiptSourceType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.CashReceiptSourceType.InterfaceIndicator =
          db.GetString(reader, 1);
        entities.CashReceiptSourceType.Code = db.GetString(reader, 2);
        entities.CashReceiptSourceType.Populated = true;
        CheckValid<CashReceiptSourceType>("InterfaceIndicator",
          entities.CashReceiptSourceType.InterfaceIndicator);
      });
  }

  private bool ReadCashReceiptSourceTypeCashReceiptEvent()
  {
    System.Diagnostics.Debug.Assert(entities.CashReceipt.Populated);
    entities.CashReceiptEvent.Populated = false;
    entities.CashReceiptSourceType.Populated = false;

    return Read("ReadCashReceiptSourceTypeCashReceiptEvent",
      (db, command) =>
      {
        db.
          SetInt32(command, "cstIdentifier", entities.CashReceipt.CstIdentifier);
          
        db.
          SetInt32(command, "crvIdentifier", entities.CashReceipt.CrvIdentifier);
          
      },
      (db, reader) =>
      {
        entities.CashReceiptSourceType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.CashReceiptSourceType.InterfaceIndicator =
          db.GetString(reader, 1);
        entities.CashReceiptSourceType.Code = db.GetString(reader, 2);
        entities.CashReceiptEvent.CstIdentifier = db.GetInt32(reader, 3);
        entities.CashReceiptEvent.SystemGeneratedIdentifier =
          db.GetInt32(reader, 4);
        entities.CashReceiptEvent.Populated = true;
        entities.CashReceiptSourceType.Populated = true;
        CheckValid<CashReceiptSourceType>("InterfaceIndicator",
          entities.CashReceiptSourceType.InterfaceIndicator);
      });
  }

  private bool ReadCashReceiptStatusHistoryCashReceiptStatus()
  {
    System.Diagnostics.Debug.Assert(entities.CashReceipt.Populated);
    entities.CashReceiptStatusHistory.Populated = false;
    entities.CashReceiptStatus.Populated = false;

    return Read("ReadCashReceiptStatusHistoryCashReceiptStatus",
      (db, command) =>
      {
        db.
          SetInt32(command, "crtIdentifier", entities.CashReceipt.CrtIdentifier);
          
        db.
          SetInt32(command, "cstIdentifier", entities.CashReceipt.CstIdentifier);
          
        db.
          SetInt32(command, "crvIdentifier", entities.CashReceipt.CrvIdentifier);
          
        db.SetNullableDate(
          command, "discontinueDate", new DateTime(2099, 12, 31));
      },
      (db, reader) =>
      {
        entities.CashReceiptStatusHistory.CrtIdentifier =
          db.GetInt32(reader, 0);
        entities.CashReceiptStatusHistory.CstIdentifier =
          db.GetInt32(reader, 1);
        entities.CashReceiptStatusHistory.CrvIdentifier =
          db.GetInt32(reader, 2);
        entities.CashReceiptStatusHistory.CrsIdentifier =
          db.GetInt32(reader, 3);
        entities.CashReceiptStatus.SystemGeneratedIdentifier =
          db.GetInt32(reader, 3);
        entities.CashReceiptStatusHistory.CreatedTimestamp =
          db.GetDateTime(reader, 4);
        entities.CashReceiptStatusHistory.DiscontinueDate =
          db.GetNullableDate(reader, 5);
        entities.CashReceiptStatus.Code = db.GetString(reader, 6);
        entities.CashReceiptStatusHistory.Populated = true;
        entities.CashReceiptStatus.Populated = true;
      });
  }

  private bool ReadCashReceiptType()
  {
    System.Diagnostics.Debug.Assert(entities.CashReceipt.Populated);
    entities.CashReceiptType.Populated = false;

    return Read("ReadCashReceiptType",
      (db, command) =>
      {
        db.SetInt32(command, "crtypeId", entities.CashReceipt.CrtIdentifier);
      },
      (db, reader) =>
      {
        entities.CashReceiptType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.CashReceiptType.Populated = true;
      });
  }
#endregion

#region Parameters.
  protected readonly Import import;
  protected readonly Export export;
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
    /// A value of CashReceipt.
    /// </summary>
    [JsonPropertyName("cashReceipt")]
    public CashReceipt CashReceipt
    {
      get => cashReceipt ??= new();
      set => cashReceipt = value;
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

    private CashReceiptSourceType cashReceiptSourceType;
    private CashReceiptType cashReceiptType;
    private CashReceipt cashReceipt;
    private CashReceiptEvent cashReceiptEvent;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of DeletedCashReceiptFlag.
    /// </summary>
    [JsonPropertyName("deletedCashReceiptFlag")]
    public Common DeletedCashReceiptFlag
    {
      get => deletedCashReceiptFlag ??= new();
      set => deletedCashReceiptFlag = value;
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
    /// A value of NoOfCrDetails.
    /// </summary>
    [JsonPropertyName("noOfCrDetails")]
    public Common NoOfCrDetails
    {
      get => noOfCrDetails ??= new();
      set => noOfCrDetails = value;
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

    private Common deletedCashReceiptFlag;
    private CashReceiptDetail cashReceiptDetail;
    private Common noOfCrDetails;
    private CashReceiptEvent cashReceiptEvent;
    private CashReceipt cashReceipt;
    private CashReceiptType cashReceiptType;
    private CashReceiptSourceType cashReceiptSourceType;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of CashReceiptStatusHistory.
    /// </summary>
    [JsonPropertyName("cashReceiptStatusHistory")]
    public CashReceiptStatusHistory CashReceiptStatusHistory
    {
      get => cashReceiptStatusHistory ??= new();
      set => cashReceiptStatusHistory = value;
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
    /// A value of CashReceiptDetail.
    /// </summary>
    [JsonPropertyName("cashReceiptDetail")]
    public CashReceiptDetail CashReceiptDetail
    {
      get => cashReceiptDetail ??= new();
      set => cashReceiptDetail = value;
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
    /// A value of CashReceipt.
    /// </summary>
    [JsonPropertyName("cashReceipt")]
    public CashReceipt CashReceipt
    {
      get => cashReceipt ??= new();
      set => cashReceipt = value;
    }

    private CashReceiptStatusHistory cashReceiptStatusHistory;
    private CashReceiptStatus cashReceiptStatus;
    private CashReceiptDetail cashReceiptDetail;
    private CashReceiptType cashReceiptType;
    private CashReceiptEvent cashReceiptEvent;
    private CashReceiptSourceType cashReceiptSourceType;
    private CashReceipt cashReceipt;
  }
#endregion
}
