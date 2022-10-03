// Program: SI_CREATE_OG_CSENET_COLLECTIONS, ID: 372382220, model: 746.
// Short name: SWE01648
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: SI_CREATE_OG_CSENET_COLLECTIONS.
/// </para>
/// <para>
/// RESP: SRVINIT
/// This CAB creates the entity type that contains Interstate (CSENet) 
/// Collection information.
/// </para>
/// </summary>
[Serializable]
public partial class SiCreateOgCsenetCollections: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_CREATE_OG_CSENET_COLLECTIONS program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiCreateOgCsenetCollections(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiCreateOgCsenetCollections.
  /// </summary>
  public SiCreateOgCsenetCollections(IContext context, Import import,
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
    if (!ReadInterstateCase())
    {
      ExitState = "INTERSTATE_CASE_NF";

      return;
    }

    ReadInterstateCollection();

    try
    {
      CreateInterstateCollection();
      ExitState = "ACO_NI0000_SUCCESSFUL_ADD";
    }
    catch(Exception e)
    {
      switch(GetErrorCode(e))
      {
        case ErrorCode.AlreadyExists:
          break;
        case ErrorCode.PermittedValueViolation:
          break;
        case ErrorCode.DatabaseError:
          break;
        default:
          throw;
      }
    }
  }

  private void CreateInterstateCollection()
  {
    var ccaTransactionDt = entities.InterstateCase.TransactionDate;
    var ccaTransSerNum = entities.InterstateCase.TransSerialNumber;
    var systemGeneratedSequenceNum =
      entities.InterstateCollection.SystemGeneratedSequenceNum + 1;
    var dateOfCollection = import.InterstateCollection.DateOfCollection;
    var dateOfPosting = import.InterstateCollection.DateOfPosting;
    var paymentAmount =
      import.InterstateCollection.PaymentAmount.GetValueOrDefault();
    var paymentSource = import.InterstateCollection.PaymentSource ?? "";
    var interstatePaymentMethod =
      import.InterstateCollection.InterstatePaymentMethod ?? "";
    var rdfiId = import.InterstateCollection.RdfiId ?? "";
    var rdfiAccountNum = import.InterstateCollection.RdfiAccountNum ?? "";

    entities.InterstateCollection.Populated = false;
    Update("CreateInterstateCollection",
      (db, command) =>
      {
        db.SetDate(command, "ccaTransactionDt", ccaTransactionDt);
        db.SetInt64(command, "ccaTransSerNum", ccaTransSerNum);
        db.SetInt32(command, "sysGeneratedId", systemGeneratedSequenceNum);
        db.SetNullableDate(command, "dateOfCollection", dateOfCollection);
        db.SetNullableDate(command, "dateOfPosting", dateOfPosting);
        db.SetNullableDecimal(command, "paymentAmount", paymentAmount);
        db.SetNullableString(command, "paymentSource", paymentSource);
        db.SetNullableString(
          command, "interstatePayment", interstatePaymentMethod);
        db.SetNullableString(command, "rdfiId", rdfiId);
        db.SetNullableString(command, "rdfiAccountNum", rdfiAccountNum);
      });

    entities.InterstateCollection.CcaTransactionDt = ccaTransactionDt;
    entities.InterstateCollection.CcaTransSerNum = ccaTransSerNum;
    entities.InterstateCollection.SystemGeneratedSequenceNum =
      systemGeneratedSequenceNum;
    entities.InterstateCollection.DateOfCollection = dateOfCollection;
    entities.InterstateCollection.DateOfPosting = dateOfPosting;
    entities.InterstateCollection.PaymentAmount = paymentAmount;
    entities.InterstateCollection.PaymentSource = paymentSource;
    entities.InterstateCollection.InterstatePaymentMethod =
      interstatePaymentMethod;
    entities.InterstateCollection.RdfiId = rdfiId;
    entities.InterstateCollection.RdfiAccountNum = rdfiAccountNum;
    entities.InterstateCollection.Populated = true;
  }

  private bool ReadInterstateCase()
  {
    entities.InterstateCase.Populated = false;

    return Read("ReadInterstateCase",
      (db, command) =>
      {
        db.SetInt64(
          command, "transSerialNbr", import.InterstateCase.TransSerialNumber);
        db.SetDate(
          command, "transactionDate",
          import.InterstateCase.TransactionDate.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.InterstateCase.TransSerialNumber = db.GetInt64(reader, 0);
        entities.InterstateCase.TransactionDate = db.GetDate(reader, 1);
        entities.InterstateCase.Populated = true;
      });
  }

  private bool ReadInterstateCollection()
  {
    entities.InterstateCollection.Populated = false;

    return Read("ReadInterstateCollection",
      (db, command) =>
      {
        db.SetDate(
          command, "ccaTransactionDt",
          entities.InterstateCase.TransactionDate.GetValueOrDefault());
        db.SetInt64(
          command, "ccaTransSerNum", entities.InterstateCase.TransSerialNumber);
          
      },
      (db, reader) =>
      {
        entities.InterstateCollection.CcaTransactionDt = db.GetDate(reader, 0);
        entities.InterstateCollection.CcaTransSerNum = db.GetInt64(reader, 1);
        entities.InterstateCollection.SystemGeneratedSequenceNum =
          db.GetInt32(reader, 2);
        entities.InterstateCollection.DateOfCollection =
          db.GetNullableDate(reader, 3);
        entities.InterstateCollection.DateOfPosting =
          db.GetNullableDate(reader, 4);
        entities.InterstateCollection.PaymentAmount =
          db.GetNullableDecimal(reader, 5);
        entities.InterstateCollection.PaymentSource =
          db.GetNullableString(reader, 6);
        entities.InterstateCollection.InterstatePaymentMethod =
          db.GetNullableString(reader, 7);
        entities.InterstateCollection.RdfiId = db.GetNullableString(reader, 8);
        entities.InterstateCollection.RdfiAccountNum =
          db.GetNullableString(reader, 9);
        entities.InterstateCollection.Populated = true;
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
    /// A value of InterstateCollection.
    /// </summary>
    [JsonPropertyName("interstateCollection")]
    public InterstateCollection InterstateCollection
    {
      get => interstateCollection ??= new();
      set => interstateCollection = value;
    }

    /// <summary>
    /// A value of InterstateCase.
    /// </summary>
    [JsonPropertyName("interstateCase")]
    public InterstateCase InterstateCase
    {
      get => interstateCase ??= new();
      set => interstateCase = value;
    }

    private InterstateCollection interstateCollection;
    private InterstateCase interstateCase;
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
    /// A value of InterstateCollection.
    /// </summary>
    [JsonPropertyName("interstateCollection")]
    public InterstateCollection InterstateCollection
    {
      get => interstateCollection ??= new();
      set => interstateCollection = value;
    }

    /// <summary>
    /// A value of InterstateCase.
    /// </summary>
    [JsonPropertyName("interstateCase")]
    public InterstateCase InterstateCase
    {
      get => interstateCase ??= new();
      set => interstateCase = value;
    }

    private InterstateCollection interstateCollection;
    private InterstateCase interstateCase;
  }
#endregion
}
