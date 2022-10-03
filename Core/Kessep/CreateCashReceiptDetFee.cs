// Program: CREATE_CASH_RECEIPT_DET_FEE, ID: 371774293, model: 746.
// Short name: SWE00129
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: CREATE_CASH_RECEIPT_DET_FEE.
/// </para>
/// <para>
/// RESP: CASHMGMT
/// This action block will create cash receipt detail fees and associate them to
/// the cash receipt detail.
/// It expects the key to be passed in since it will more than likely be 
/// implemented in list processing design.
/// </para>
/// </summary>
[Serializable]
public partial class CreateCashReceiptDetFee: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the CREATE_CASH_RECEIPT_DET_FEE program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new CreateCashReceiptDetFee(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of CreateCashReceiptDetFee.
  /// </summary>
  public CreateCashReceiptDetFee(IContext context, Import import, Export export):
    
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    if (ReadCashReceiptDetailFeeType())
    {
      try
      {
        CreateCashReceiptDetailFee();
        ExitState = "ACO_NN0000_ALL_OK";
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
    else
    {
      ExitState = "FN0048_CASH_RCPT_DTL_FEE_TYP_NF";
    }
  }

  private void CreateCashReceiptDetailFee()
  {
    System.Diagnostics.Debug.Assert(import.CashReceiptDetail.Populated);

    var crdIdentifier = import.CashReceiptDetail.SequentialIdentifier;
    var crvIdentifier = import.CashReceiptDetail.CrvIdentifier;
    var cstIdentifier = import.CashReceiptDetail.CstIdentifier;
    var crtIdentifier = import.CashReceiptDetail.CrtIdentifier;
    var systemGeneratedIdentifier =
      import.CashReceiptDetailFee.SystemGeneratedIdentifier;
    var amount = import.CashReceiptDetailFee.Amount;
    var createdBy = global.UserId;
    var createdTimestamp = Now();
    var cdtIdentifier =
      entities.CashReceiptDetailFeeType.SystemGeneratedIdentifier;

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

  private bool ReadCashReceiptDetailFeeType()
  {
    entities.CashReceiptDetailFeeType.Populated = false;

    return Read("ReadCashReceiptDetailFeeType",
      (db, command) =>
      {
        db.SetString(command, "code", import.CashReceiptDetailFeeType.Code);
      },
      (db, reader) =>
      {
        entities.CashReceiptDetailFeeType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.CashReceiptDetailFeeType.Code = db.GetString(reader, 1);
        entities.CashReceiptDetailFeeType.Populated = true;
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
    /// A value of CashReceiptDetail.
    /// </summary>
    [JsonPropertyName("cashReceiptDetail")]
    public CashReceiptDetail CashReceiptDetail
    {
      get => cashReceiptDetail ??= new();
      set => cashReceiptDetail = value;
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

    private CashReceiptDetailFee cashReceiptDetailFee;
    private CashReceiptDetail cashReceiptDetail;
    private CashReceiptDetailFeeType cashReceiptDetailFeeType;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
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
    /// A value of CashReceiptDetailFeeType.
    /// </summary>
    [JsonPropertyName("cashReceiptDetailFeeType")]
    public CashReceiptDetailFeeType CashReceiptDetailFeeType
    {
      get => cashReceiptDetailFeeType ??= new();
      set => cashReceiptDetailFeeType = value;
    }

    private CashReceiptDetailFee cashReceiptDetailFee;
    private CashReceiptDetailFeeType cashReceiptDetailFeeType;
  }
#endregion
}
