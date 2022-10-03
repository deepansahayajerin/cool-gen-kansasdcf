// Program: FN_CREATE_CASH_RCPT_DTL_FEE, ID: 372566793, model: 746.
// Short name: SWE00341
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: FN_CREATE_CASH_RCPT_DTL_FEE.
/// </para>
/// <para>
/// RESP:  FINANCE
/// DESC:  This action block creates an occurrence of the Cash Receipt Detail 
/// Fee entity and associates it to the imported Cash Receipt and Fee Type.
/// </para>
/// </summary>
[Serializable]
public partial class FnCreateCashRcptDtlFee: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_CREATE_CASH_RCPT_DTL_FEE program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnCreateCashRcptDtlFee(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnCreateCashRcptDtlFee.
  /// </summary>
  public FnCreateCashRcptDtlFee(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    if (!import.PersistentCashReceiptDetailFeeType.Populated)
    {
      if (!ReadCashReceiptDetailFeeType())
      {
        ExitState = "FN0048_CASH_RCPT_DTL_FEE_TYP_NF";

        return;
      }
    }

    if (!import.PersistentCashReceiptDetail.Populated)
    {
      if (!ReadCashReceiptDetail())
      {
        ExitState = "FN0052_CASH_RCPT_DTL_NF";

        return;
      }
    }

    try
    {
      CreateCashReceiptDetailFee();
      export.CashReceiptDetailFee.Assign(entities.CashReceiptDetailFee);
    }
    catch(Exception e)
    {
      switch(GetErrorCode(e))
      {
        case ErrorCode.AlreadyExists:
          ExitState = "FN0046_CASH_RCPT_DTL_FEE_AE";

          break;
        case ErrorCode.PermittedValueViolation:
          ExitState = "FN0047_CASH_RCPT_DTL_FEE_PV";

          break;
        case ErrorCode.DatabaseError:
          break;
        default:
          throw;
      }
    }
  }

  private void CreateCashReceiptDetailFee()
  {
    System.Diagnostics.Debug.
      Assert(import.PersistentCashReceiptDetail.Populated);

    var crdIdentifier = import.PersistentCashReceiptDetail.SequentialIdentifier;
    var crvIdentifier = import.PersistentCashReceiptDetail.CrvIdentifier;
    var cstIdentifier = import.PersistentCashReceiptDetail.CstIdentifier;
    var crtIdentifier = import.PersistentCashReceiptDetail.CrtIdentifier;
    var systemGeneratedIdentifier =
      import.CashReceiptDetailFee.SystemGeneratedIdentifier;
    var amount = import.CashReceiptDetailFee.Amount;
    var createdBy = global.UserId;
    var createdTimestamp = Now();
    var cdtIdentifier =
      import.PersistentCashReceiptDetailFeeType.SystemGeneratedIdentifier;

    entities.CashReceiptDetailFee.Populated = false;
    Update("CreateCashReceiptDetailFee",
      (db, command) =>
      {
        db.SetInt32(command, "crdIdentifier", crdIdentifier);
        db.SetInt32(command, "crvIdentifier", crvIdentifier);
        db.SetInt32(command, "cstIdentifier", cstIdentifier);
        db.SetInt32(command, "crtIdentifier", crtIdentifier);
        db.SetDateTime(command, "crdetailFeeId", systemGeneratedIdentifier);
        db.SetDecimal(command, "amount", amount);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetNullableString(command, "lastUpdatedBy", "");
        db.SetNullableDateTime(command, "lastUpdatedTmst", default(DateTime));
        db.SetNullableInt32(command, "cdtIdentifier", cdtIdentifier);
      });

    entities.CashReceiptDetailFee.CrdIdentifier = crdIdentifier;
    entities.CashReceiptDetailFee.CrvIdentifier = crvIdentifier;
    entities.CashReceiptDetailFee.CstIdentifier = cstIdentifier;
    entities.CashReceiptDetailFee.CrtIdentifier = crtIdentifier;
    entities.CashReceiptDetailFee.SystemGeneratedIdentifier =
      systemGeneratedIdentifier;
    entities.CashReceiptDetailFee.Amount = amount;
    entities.CashReceiptDetailFee.CreatedBy = createdBy;
    entities.CashReceiptDetailFee.CreatedTimestamp = createdTimestamp;
    entities.CashReceiptDetailFee.CdtIdentifier = cdtIdentifier;
    entities.CashReceiptDetailFee.Populated = true;
  }

  private bool ReadCashReceiptDetail()
  {
    import.PersistentCashReceiptDetail.Populated = false;

    return Read("ReadCashReceiptDetail",
      (db, command) =>
      {
        db.SetInt32(
          command, "crdId", import.CashReceiptDetail.SequentialIdentifier);
        db.SetInt32(
          command, "cashReceiptId", import.CashReceipt.SequentialNumber);
      },
      (db, reader) =>
      {
        import.PersistentCashReceiptDetail.CrvIdentifier =
          db.GetInt32(reader, 0);
        import.PersistentCashReceiptDetail.CstIdentifier =
          db.GetInt32(reader, 1);
        import.PersistentCashReceiptDetail.CrtIdentifier =
          db.GetInt32(reader, 2);
        import.PersistentCashReceiptDetail.SequentialIdentifier =
          db.GetInt32(reader, 3);
        import.PersistentCashReceiptDetail.Populated = true;
      });
  }

  private bool ReadCashReceiptDetailFeeType()
  {
    import.PersistentCashReceiptDetailFeeType.Populated = false;

    return Read("ReadCashReceiptDetailFeeType",
      (db, command) =>
      {
        db.SetInt32(
          command, "crdtlFeeTypeId",
          import.CashReceiptDetailFeeType.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        import.PersistentCashReceiptDetailFeeType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        import.PersistentCashReceiptDetailFeeType.Populated = true;
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
    /// A value of CashReceiptDetailFee.
    /// </summary>
    [JsonPropertyName("cashReceiptDetailFee")]
    public CashReceiptDetailFee CashReceiptDetailFee
    {
      get => cashReceiptDetailFee ??= new();
      set => cashReceiptDetailFee = value;
    }

    /// <summary>
    /// A value of PersistentCashReceiptDetailFeeType.
    /// </summary>
    [JsonPropertyName("persistentCashReceiptDetailFeeType")]
    public CashReceiptDetailFeeType PersistentCashReceiptDetailFeeType
    {
      get => persistentCashReceiptDetailFeeType ??= new();
      set => persistentCashReceiptDetailFeeType = value;
    }

    /// <summary>
    /// A value of CashReceiptDetailFeeType.
    /// </summary>
    [JsonPropertyName("cashReceiptDetailFeeType")]
    public CashReceiptDetailFeeType CashReceiptDetailFeeType
    {
      get => cashReceiptDetailFeeType ??= new();
      set => cashReceiptDetailFeeType = value;
    }

    /// <summary>
    /// A value of PersistentCashReceiptDetail.
    /// </summary>
    [JsonPropertyName("persistentCashReceiptDetail")]
    public CashReceiptDetail PersistentCashReceiptDetail
    {
      get => persistentCashReceiptDetail ??= new();
      set => persistentCashReceiptDetail = value;
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

    private CashReceiptDetailFee cashReceiptDetailFee;
    private CashReceiptDetailFeeType persistentCashReceiptDetailFeeType;
    private CashReceiptDetailFeeType cashReceiptDetailFeeType;
    private CashReceiptDetail persistentCashReceiptDetail;
    private CashReceiptDetail cashReceiptDetail;
    private CashReceipt cashReceipt;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of CashReceiptDetailFee.
    /// </summary>
    [JsonPropertyName("cashReceiptDetailFee")]
    public CashReceiptDetailFee CashReceiptDetailFee
    {
      get => cashReceiptDetailFee ??= new();
      set => cashReceiptDetailFee = value;
    }

    /// <summary>
    /// A value of ImportNumberOfReads.
    /// </summary>
    [JsonPropertyName("importNumberOfReads")]
    public Common ImportNumberOfReads
    {
      get => importNumberOfReads ??= new();
      set => importNumberOfReads = value;
    }

    /// <summary>
    /// A value of ImportNumberOfUpdates.
    /// </summary>
    [JsonPropertyName("importNumberOfUpdates")]
    public Common ImportNumberOfUpdates
    {
      get => importNumberOfUpdates ??= new();
      set => importNumberOfUpdates = value;
    }

    private CashReceiptDetailFee cashReceiptDetailFee;
    private Common importNumberOfReads;
    private Common importNumberOfUpdates;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of CashReceiptDetailFee.
    /// </summary>
    [JsonPropertyName("cashReceiptDetailFee")]
    public CashReceiptDetailFee CashReceiptDetailFee
    {
      get => cashReceiptDetailFee ??= new();
      set => cashReceiptDetailFee = value;
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

    private CashReceiptDetailFee cashReceiptDetailFee;
    private CashReceipt cashReceipt;
  }
#endregion
}
