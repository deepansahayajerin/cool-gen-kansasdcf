// Program: FN_UPDATE_COLLECTION_FEES, ID: 371774294, model: 746.
// Short name: SWE01953
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_UPDATE_COLLECTION_FEES.
/// </summary>
[Serializable]
public partial class FnUpdateCollectionFees: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_UPDATE_COLLECTION_FEES program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnUpdateCollectionFees(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnUpdateCollectionFees.
  /// </summary>
  public FnUpdateCollectionFees(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    ExitState = "ACO_NN0000_ALL_OK";

    if (!ReadCashReceiptDetailFee())
    {
      ExitState = "FN0130_CASH_RCPT_DTL_FEES_NF";

      return;
    }

    if (!ReadCashReceiptDetailFeeType1())
    {
      ExitState = "FN0048_CASH_RCPT_DTL_FEE_TYP_NF";

      return;
    }

    if (entities.CashReceiptDetailFee.Amount != import
      .CashReceiptDetailFee.Amount || !
      Equal(entities.CashReceiptDetailFeeType.Code,
      import.CashReceiptDetailFeeType.Code))
    {
      if (!ReadCashReceiptDetailFeeType2())
      {
        ExitState = "FN0048_CASH_RCPT_DTL_FEE_TYP_NF";

        return;
      }

      try
      {
        UpdateCashReceiptDetailFee();
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "FN0141_CASH_RCPT_FEE_UPDT_RB";

            break;
          case ErrorCode.PermittedValueViolation:
            ExitState = "FN0141_CASH_RCPT_FEE_UPDT_RB";

            break;
          case ErrorCode.DatabaseError:
            break;
          default:
            throw;
        }
      }
    }
  }

  private bool ReadCashReceiptDetailFee()
  {
    System.Diagnostics.Debug.Assert(import.P.Populated);
    entities.CashReceiptDetailFee.Populated = false;

    return Read("ReadCashReceiptDetailFee",
      (db, command) =>
      {
        db.SetInt32(command, "crdIdentifier", import.P.SequentialIdentifier);
        db.SetInt32(command, "crvIdentifier", import.P.CrvIdentifier);
        db.SetInt32(command, "cstIdentifier", import.P.CstIdentifier);
        db.SetInt32(command, "crtIdentifier", import.P.CrtIdentifier);
        db.SetDateTime(
          command, "crdetailFeeId",
          import.CashReceiptDetailFee.SystemGeneratedIdentifier.
            GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CashReceiptDetailFee.CrdIdentifier = db.GetInt32(reader, 0);
        entities.CashReceiptDetailFee.CrvIdentifier = db.GetInt32(reader, 1);
        entities.CashReceiptDetailFee.CstIdentifier = db.GetInt32(reader, 2);
        entities.CashReceiptDetailFee.CrtIdentifier = db.GetInt32(reader, 3);
        entities.CashReceiptDetailFee.SystemGeneratedIdentifier =
          db.GetDateTime(reader, 4);
        entities.CashReceiptDetailFee.Amount = db.GetDecimal(reader, 5);
        entities.CashReceiptDetailFee.CreatedBy = db.GetString(reader, 6);
        entities.CashReceiptDetailFee.CreatedTimestamp =
          db.GetDateTime(reader, 7);
        entities.CashReceiptDetailFee.LastUpdatedBy =
          db.GetNullableString(reader, 8);
        entities.CashReceiptDetailFee.LastUpdatedTmst =
          db.GetNullableDateTime(reader, 9);
        entities.CashReceiptDetailFee.CdtIdentifier =
          db.GetNullableInt32(reader, 10);
        entities.CashReceiptDetailFee.Populated = true;
      });
  }

  private bool ReadCashReceiptDetailFeeType1()
  {
    System.Diagnostics.Debug.Assert(entities.CashReceiptDetailFee.Populated);
    entities.CashReceiptDetailFeeType.Populated = false;

    return Read("ReadCashReceiptDetailFeeType1",
      (db, command) =>
      {
        db.SetInt32(
          command, "crdtlFeeTypeId",
          entities.CashReceiptDetailFee.CdtIdentifier.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CashReceiptDetailFeeType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.CashReceiptDetailFeeType.Code = db.GetString(reader, 1);
        entities.CashReceiptDetailFeeType.Populated = true;
      });
  }

  private bool ReadCashReceiptDetailFeeType2()
  {
    entities.New1.Populated = false;

    return Read("ReadCashReceiptDetailFeeType2",
      (db, command) =>
      {
        db.SetString(command, "code", import.CashReceiptDetailFeeType.Code);
      },
      (db, reader) =>
      {
        entities.New1.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.New1.Code = db.GetString(reader, 1);
        entities.New1.Populated = true;
      });
  }

  private void UpdateCashReceiptDetailFee()
  {
    System.Diagnostics.Debug.Assert(entities.CashReceiptDetailFee.Populated);

    var amount = import.CashReceiptDetailFee.Amount;
    var lastUpdatedBy = global.UserId;
    var lastUpdatedTmst = Now();
    var cdtIdentifier = entities.New1.SystemGeneratedIdentifier;

    entities.CashReceiptDetailFee.Populated = false;
    Update("UpdateCashReceiptDetailFee",
      (db, command) =>
      {
        db.SetDecimal(command, "amount", amount);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTmst);
        db.SetNullableInt32(command, "cdtIdentifier", cdtIdentifier);
        db.SetInt32(
          command, "crdIdentifier",
          entities.CashReceiptDetailFee.CrdIdentifier);
        db.SetInt32(
          command, "crvIdentifier",
          entities.CashReceiptDetailFee.CrvIdentifier);
        db.SetInt32(
          command, "cstIdentifier",
          entities.CashReceiptDetailFee.CstIdentifier);
        db.SetInt32(
          command, "crtIdentifier",
          entities.CashReceiptDetailFee.CrtIdentifier);
        db.SetDateTime(
          command, "crdetailFeeId",
          entities.CashReceiptDetailFee.SystemGeneratedIdentifier.
            GetValueOrDefault());
      });

    entities.CashReceiptDetailFee.Amount = amount;
    entities.CashReceiptDetailFee.LastUpdatedBy = lastUpdatedBy;
    entities.CashReceiptDetailFee.LastUpdatedTmst = lastUpdatedTmst;
    entities.CashReceiptDetailFee.CdtIdentifier = cdtIdentifier;
    entities.CashReceiptDetailFee.Populated = true;
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
    /// A value of CashReceiptDetailFeeType.
    /// </summary>
    [JsonPropertyName("cashReceiptDetailFeeType")]
    public CashReceiptDetailFeeType CashReceiptDetailFeeType
    {
      get => cashReceiptDetailFeeType ??= new();
      set => cashReceiptDetailFeeType = value;
    }

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
    /// A value of P.
    /// </summary>
    [JsonPropertyName("p")]
    public CashReceiptDetail P
    {
      get => p ??= new();
      set => p = value;
    }

    private CashReceiptDetailFeeType cashReceiptDetailFeeType;
    private CashReceiptDetailFee cashReceiptDetailFee;
    private CashReceiptDetail p;
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
    /// A value of New1.
    /// </summary>
    [JsonPropertyName("new1")]
    public CashReceiptDetailFeeType New1
    {
      get => new1 ??= new();
      set => new1 = value;
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
    /// A value of CashReceiptDetailFee.
    /// </summary>
    [JsonPropertyName("cashReceiptDetailFee")]
    public CashReceiptDetailFee CashReceiptDetailFee
    {
      get => cashReceiptDetailFee ??= new();
      set => cashReceiptDetailFee = value;
    }

    private CashReceiptDetailFeeType new1;
    private CashReceiptDetailFeeType cashReceiptDetailFeeType;
    private CashReceiptDetailFee cashReceiptDetailFee;
  }
#endregion
}
