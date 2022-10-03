// Program: DISPLAY_CASH_RECEIPT, ID: 371721896, model: 746.
// Short name: SWE00226
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: DISPLAY_CASH_RECEIPT.
/// </para>
/// <para>
/// RESP: CASHMGMNT
/// This action block will display a cash receipt item.
/// </para>
/// </summary>
[Serializable]
public partial class DisplayCashReceipt: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the DISPLAY_CASH_RECEIPT program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new DisplayCashReceipt(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of DisplayCashReceipt.
  /// </summary>
  public DisplayCashReceipt(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ************************************************************
    // MAINTENANCE LOG
    // AUTHOR            DATE		DESCRIPTION
    // -------  	 -------       ---------------------
    // Ty HIll-MTW     04/28/97   	Change Current_date
    // J Katz		09/22/98	
    // *  Move imports to exports before performing display action
    //    to get rid of duplicate code used on every not found
    //    condition.
    // *  Make import views optional since two different sets of
    //    data may be imported to successfully read.
    // *  Combine the two export cash receipt event views.
    // J Katz		06/03/99	Analyze READ statements and
    // 				change read property to
    // 				Select Only when appropriate.
    // ************************************************************
    UseCabSetMaximumDiscontinueDate();
    export.CashReceipt.SequentialNumber = import.CashReceipt.SequentialNumber;
    export.CashReceiptEvent.SystemGeneratedIdentifier =
      import.CashReceiptEvent.SystemGeneratedIdentifier;
    export.CashReceiptSourceType.SystemGeneratedIdentifier =
      import.CashReceiptSourceType.SystemGeneratedIdentifier;
    export.CashReceiptType.SystemGeneratedIdentifier =
      import.CashReceiptType.SystemGeneratedIdentifier;

    if (import.CashReceiptSourceType.SystemGeneratedIdentifier > 0 && import
      .CashReceiptType.SystemGeneratedIdentifier > 0 && import
      .CashReceiptEvent.SystemGeneratedIdentifier > 0)
    {
      if (!ReadCashReceiptSourceType1())
      {
        ExitState = "FN0000_CR_SOURCE_TYPE_NF_RB";

        return;
      }

      if (!ReadCashReceiptType1())
      {
        ExitState = "FN0113_CASH_RCPT_TYPE_NF";

        return;
      }

      if (!ReadCashReceiptEvent1())
      {
        ExitState = "FN0077_CASH_RCPT_EVENT_NF";

        return;
      }

      if (!ReadCashReceipt1())
      {
        ExitState = "FN0084_CASH_RCPT_NF";

        return;
      }

      if (ReadCashReceiptStatusCashReceiptStatusHistory())
      {
        export.CashReceipt.Assign(entities.CashReceipt);
        export.CashReceiptEvent.Assign(entities.CashReceiptEvent);
        export.CashReceiptSourceType.Assign(entities.CashReceiptSourceType);
        export.CashReceiptType.Assign(entities.CashReceiptType);
        MoveCashReceiptStatus(entities.CashReceiptStatus,
          export.CashReceiptStatus);
      }
      else
      {
        ExitState = "FN0108_CASH_RCPT_STAT_NF";

        return;
      }
    }
    else
    {
      // -------------------------------------------------------------
      // The cash receipt is read using the receipt number.
      // -------------------------------------------------------------
      if (!ReadCashReceipt2())
      {
        export.CashReceipt.SequentialNumber =
          import.CashReceipt.SequentialNumber;
        ExitState = "FN0084_CASH_RCPT_NF";

        return;
      }

      if (!ReadCashReceiptEvent2())
      {
        ExitState = "FN0077_CASH_RCPT_EVENT_NF";

        return;
      }

      if (!ReadCashReceiptSourceType2())
      {
        ExitState = "FN0000_CR_SOURCE_TYPE_NF_RB";

        return;
      }

      if (!ReadCashReceiptType2())
      {
        ExitState = "FN0113_CASH_RCPT_TYPE_NF";

        return;
      }

      if (!ReadCashReceiptStatusHistory())
      {
        ExitState = "FN0102_CASH_RCPT_STAT_HIST_NF";

        return;
      }

      if (ReadCashReceiptStatus())
      {
        export.CashReceipt.Assign(entities.CashReceipt);
        export.CashReceiptEvent.Assign(entities.CashReceiptEvent);
        export.CashReceiptSourceType.Assign(entities.CashReceiptSourceType);
        export.CashReceiptType.Assign(entities.CashReceiptType);
        MoveCashReceiptStatus(entities.CashReceiptStatus,
          export.CashReceiptStatus);
      }
      else
      {
        ExitState = "FN0108_CASH_RCPT_STAT_NF";

        return;
      }
    }

    ReadCashReceiptDetail();
  }

  private static void MoveCashReceiptStatus(CashReceiptStatus source,
    CashReceiptStatus target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Code = source.Code;
  }

  private void UseCabSetMaximumDiscontinueDate()
  {
    var useImport = new CabSetMaximumDiscontinueDate.Import();
    var useExport = new CabSetMaximumDiscontinueDate.Export();

    Call(CabSetMaximumDiscontinueDate.Execute, useImport, useExport);

    local.Maximum.Date = useExport.DateWorkArea.Date;
  }

  private bool ReadCashReceipt1()
  {
    System.Diagnostics.Debug.Assert(entities.CashReceiptEvent.Populated);
    entities.CashReceipt.Populated = false;

    return Read("ReadCashReceipt1",
      (db, command) =>
      {
        db.SetInt32(
          command, "crvIdentifier",
          entities.CashReceiptEvent.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "cstIdentifier", entities.CashReceiptEvent.CstIdentifier);
        db.SetInt32(
          command, "crtIdentifier",
          entities.CashReceiptType.SystemGeneratedIdentifier);
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
        entities.CashReceipt.PayorOrganization =
          db.GetNullableString(reader, 10);
        entities.CashReceipt.PayorFirstName = db.GetNullableString(reader, 11);
        entities.CashReceipt.PayorMiddleName = db.GetNullableString(reader, 12);
        entities.CashReceipt.PayorLastName = db.GetNullableString(reader, 13);
        entities.CashReceipt.TotalCashTransactionAmount =
          db.GetNullableDecimal(reader, 14);
        entities.CashReceipt.CreatedBy = db.GetString(reader, 15);
        entities.CashReceipt.CreatedTimestamp = db.GetDateTime(reader, 16);
        entities.CashReceipt.CashBalanceAmt = db.GetNullableDecimal(reader, 17);
        entities.CashReceipt.CashBalanceReason =
          db.GetNullableString(reader, 18);
        entities.CashReceipt.CashDue = db.GetNullableDecimal(reader, 19);
        entities.CashReceipt.Note = db.GetNullableString(reader, 20);
        entities.CashReceipt.PayorSocialSecurityNumber =
          db.GetNullableString(reader, 21);
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
        entities.CashReceipt.PayorOrganization =
          db.GetNullableString(reader, 10);
        entities.CashReceipt.PayorFirstName = db.GetNullableString(reader, 11);
        entities.CashReceipt.PayorMiddleName = db.GetNullableString(reader, 12);
        entities.CashReceipt.PayorLastName = db.GetNullableString(reader, 13);
        entities.CashReceipt.TotalCashTransactionAmount =
          db.GetNullableDecimal(reader, 14);
        entities.CashReceipt.CreatedBy = db.GetString(reader, 15);
        entities.CashReceipt.CreatedTimestamp = db.GetDateTime(reader, 16);
        entities.CashReceipt.CashBalanceAmt = db.GetNullableDecimal(reader, 17);
        entities.CashReceipt.CashBalanceReason =
          db.GetNullableString(reader, 18);
        entities.CashReceipt.CashDue = db.GetNullableDecimal(reader, 19);
        entities.CashReceipt.Note = db.GetNullableString(reader, 20);
        entities.CashReceipt.PayorSocialSecurityNumber =
          db.GetNullableString(reader, 21);
        entities.CashReceipt.Populated = true;
        CheckValid<CashReceipt>("CashBalanceReason",
          entities.CashReceipt.CashBalanceReason);
      });
  }

  private bool ReadCashReceiptDetail()
  {
    System.Diagnostics.Debug.Assert(entities.CashReceipt.Populated);

    return Read("ReadCashReceiptDetail",
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
        export.NumberOfCrDetails.Count = db.GetInt32(reader, 0);
      });
  }

  private bool ReadCashReceiptEvent1()
  {
    entities.CashReceiptEvent.Populated = false;

    return Read("ReadCashReceiptEvent1",
      (db, command) =>
      {
        db.SetInt32(
          command, "creventId",
          import.CashReceiptEvent.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "cstIdentifier",
          entities.CashReceiptSourceType.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.CashReceiptEvent.CstIdentifier = db.GetInt32(reader, 0);
        entities.CashReceiptEvent.SystemGeneratedIdentifier =
          db.GetInt32(reader, 1);
        entities.CashReceiptEvent.ReceivedDate = db.GetDate(reader, 2);
        entities.CashReceiptEvent.SourceCreationDate =
          db.GetNullableDate(reader, 3);
        entities.CashReceiptEvent.Populated = true;
      });
  }

  private bool ReadCashReceiptEvent2()
  {
    System.Diagnostics.Debug.Assert(entities.CashReceipt.Populated);
    entities.CashReceiptEvent.Populated = false;

    return Read("ReadCashReceiptEvent2",
      (db, command) =>
      {
        db.SetInt32(command, "creventId", entities.CashReceipt.CrvIdentifier);
        db.
          SetInt32(command, "cstIdentifier", entities.CashReceipt.CstIdentifier);
          
      },
      (db, reader) =>
      {
        entities.CashReceiptEvent.CstIdentifier = db.GetInt32(reader, 0);
        entities.CashReceiptEvent.SystemGeneratedIdentifier =
          db.GetInt32(reader, 1);
        entities.CashReceiptEvent.ReceivedDate = db.GetDate(reader, 2);
        entities.CashReceiptEvent.SourceCreationDate =
          db.GetNullableDate(reader, 3);
        entities.CashReceiptEvent.Populated = true;
      });
  }

  private bool ReadCashReceiptSourceType1()
  {
    entities.CashReceiptSourceType.Populated = false;

    return Read("ReadCashReceiptSourceType1",
      (db, command) =>
      {
        db.SetInt32(
          command, "crSrceTypeId",
          import.CashReceiptSourceType.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.CashReceiptSourceType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.CashReceiptSourceType.InterfaceIndicator =
          db.GetString(reader, 1);
        entities.CashReceiptSourceType.Code = db.GetString(reader, 2);
        entities.CashReceiptSourceType.CourtInd =
          db.GetNullableString(reader, 3);
        entities.CashReceiptSourceType.Populated = true;
        CheckValid<CashReceiptSourceType>("InterfaceIndicator",
          entities.CashReceiptSourceType.InterfaceIndicator);
      });
  }

  private bool ReadCashReceiptSourceType2()
  {
    System.Diagnostics.Debug.Assert(entities.CashReceiptEvent.Populated);
    entities.CashReceiptSourceType.Populated = false;

    return Read("ReadCashReceiptSourceType2",
      (db, command) =>
      {
        db.SetInt32(
          command, "crSrceTypeId", entities.CashReceiptEvent.CstIdentifier);
      },
      (db, reader) =>
      {
        entities.CashReceiptSourceType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.CashReceiptSourceType.InterfaceIndicator =
          db.GetString(reader, 1);
        entities.CashReceiptSourceType.Code = db.GetString(reader, 2);
        entities.CashReceiptSourceType.CourtInd =
          db.GetNullableString(reader, 3);
        entities.CashReceiptSourceType.Populated = true;
        CheckValid<CashReceiptSourceType>("InterfaceIndicator",
          entities.CashReceiptSourceType.InterfaceIndicator);
      });
  }

  private bool ReadCashReceiptStatus()
  {
    System.Diagnostics.Debug.
      Assert(entities.CashReceiptStatusHistory.Populated);
    entities.CashReceiptStatus.Populated = false;

    return Read("ReadCashReceiptStatus",
      (db, command) =>
      {
        db.SetInt32(
          command, "crStatusId",
          entities.CashReceiptStatusHistory.CrsIdentifier);
      },
      (db, reader) =>
      {
        entities.CashReceiptStatus.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.CashReceiptStatus.Code = db.GetString(reader, 1);
        entities.CashReceiptStatus.Populated = true;
      });
  }

  private bool ReadCashReceiptStatusCashReceiptStatusHistory()
  {
    System.Diagnostics.Debug.Assert(entities.CashReceipt.Populated);
    entities.CashReceiptStatusHistory.Populated = false;
    entities.CashReceiptStatus.Populated = false;

    return Read("ReadCashReceiptStatusCashReceiptStatusHistory",
      (db, command) =>
      {
        db.
          SetInt32(command, "crtIdentifier", entities.CashReceipt.CrtIdentifier);
          
        db.
          SetInt32(command, "cstIdentifier", entities.CashReceipt.CstIdentifier);
          
        db.
          SetInt32(command, "crvIdentifier", entities.CashReceipt.CrvIdentifier);
          
        db.SetNullableDate(
          command, "discontinueDate", local.Maximum.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CashReceiptStatus.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.CashReceiptStatusHistory.CrsIdentifier =
          db.GetInt32(reader, 0);
        entities.CashReceiptStatus.Code = db.GetString(reader, 1);
        entities.CashReceiptStatusHistory.CrtIdentifier =
          db.GetInt32(reader, 2);
        entities.CashReceiptStatusHistory.CstIdentifier =
          db.GetInt32(reader, 3);
        entities.CashReceiptStatusHistory.CrvIdentifier =
          db.GetInt32(reader, 4);
        entities.CashReceiptStatusHistory.CreatedTimestamp =
          db.GetDateTime(reader, 5);
        entities.CashReceiptStatusHistory.DiscontinueDate =
          db.GetNullableDate(reader, 6);
        entities.CashReceiptStatusHistory.Populated = true;
        entities.CashReceiptStatus.Populated = true;
      });
  }

  private bool ReadCashReceiptStatusHistory()
  {
    System.Diagnostics.Debug.Assert(entities.CashReceipt.Populated);
    entities.CashReceiptStatusHistory.Populated = false;

    return Read("ReadCashReceiptStatusHistory",
      (db, command) =>
      {
        db.
          SetInt32(command, "crtIdentifier", entities.CashReceipt.CrtIdentifier);
          
        db.
          SetInt32(command, "cstIdentifier", entities.CashReceipt.CstIdentifier);
          
        db.
          SetInt32(command, "crvIdentifier", entities.CashReceipt.CrvIdentifier);
          
        db.SetNullableDate(
          command, "discontinueDate", local.Maximum.Date.GetValueOrDefault());
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
        entities.CashReceiptStatusHistory.CreatedTimestamp =
          db.GetDateTime(reader, 4);
        entities.CashReceiptStatusHistory.DiscontinueDate =
          db.GetNullableDate(reader, 5);
        entities.CashReceiptStatusHistory.Populated = true;
      });
  }

  private bool ReadCashReceiptType1()
  {
    entities.CashReceiptType.Populated = false;

    return Read("ReadCashReceiptType1",
      (db, command) =>
      {
        db.SetInt32(
          command, "crtypeId",
          import.CashReceiptType.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.CashReceiptType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.CashReceiptType.Code = db.GetString(reader, 1);
        entities.CashReceiptType.CategoryIndicator = db.GetString(reader, 2);
        entities.CashReceiptType.Populated = true;
        CheckValid<CashReceiptType>("CategoryIndicator",
          entities.CashReceiptType.CategoryIndicator);
      });
  }

  private bool ReadCashReceiptType2()
  {
    System.Diagnostics.Debug.Assert(entities.CashReceipt.Populated);
    entities.CashReceiptType.Populated = false;

    return Read("ReadCashReceiptType2",
      (db, command) =>
      {
        db.SetInt32(command, "crtypeId", entities.CashReceipt.CrtIdentifier);
      },
      (db, reader) =>
      {
        entities.CashReceiptType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.CashReceiptType.Code = db.GetString(reader, 1);
        entities.CashReceiptType.CategoryIndicator = db.GetString(reader, 2);
        entities.CashReceiptType.Populated = true;
        CheckValid<CashReceiptType>("CategoryIndicator",
          entities.CashReceiptType.CategoryIndicator);
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

    private CashReceipt cashReceipt;
    private CashReceiptType cashReceiptType;
    private CashReceiptSourceType cashReceiptSourceType;
    private CashReceiptEvent cashReceiptEvent;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of NumberOfCrDetails.
    /// </summary>
    [JsonPropertyName("numberOfCrDetails")]
    public Common NumberOfCrDetails
    {
      get => numberOfCrDetails ??= new();
      set => numberOfCrDetails = value;
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
    /// A value of CashReceiptStatus.
    /// </summary>
    [JsonPropertyName("cashReceiptStatus")]
    public CashReceiptStatus CashReceiptStatus
    {
      get => cashReceiptStatus ??= new();
      set => cashReceiptStatus = value;
    }

    private Common numberOfCrDetails;
    private CashReceipt cashReceipt;
    private CashReceiptEvent cashReceiptEvent;
    private CashReceiptSourceType cashReceiptSourceType;
    private CashReceiptType cashReceiptType;
    private CashReceiptStatus cashReceiptStatus;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of Maximum.
    /// </summary>
    [JsonPropertyName("maximum")]
    public DateWorkArea Maximum
    {
      get => maximum ??= new();
      set => maximum = value;
    }

    private DateWorkArea maximum;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
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

    private CashReceipt cashReceipt;
    private CashReceiptEvent cashReceiptEvent;
    private CashReceiptSourceType cashReceiptSourceType;
    private CashReceiptType cashReceiptType;
    private CashReceiptStatusHistory cashReceiptStatusHistory;
    private CashReceiptStatus cashReceiptStatus;
    private CashReceiptDetail cashReceiptDetail;
  }
#endregion
}
