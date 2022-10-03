// Program: FN_READ_PAYMENT_METHOD_TYPE, ID: 371828226, model: 746.
// Short name: SWE00577
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: FN_READ_PAYMENT_METHOD_TYPE.
/// </para>
/// <para>
/// RESP: FINANCE
/// This process will read the payment method type information for a particular 
/// payment method type.
/// </para>
/// </summary>
[Serializable]
public partial class FnReadPaymentMethodType: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_READ_PAYMENT_METHOD_TYPE program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnReadPaymentMethodType(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnReadPaymentMethodType.
  /// </summary>
  public FnReadPaymentMethodType(IContext context, Import import, Export export):
    
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    MovePaymentMethodType(import.PaymentMethodType, export.PaymentMethodType);

    if (AsChar(import.Flag.Flag) == 'Y')
    {
      if (ReadPaymentMethodType1())
      {
        export.PaymentMethodType.Assign(entities.PaymentMethodType);
      }
      else
      {
        ExitState = "FN0000_PYMNT_MTHD_TYPE_NF";
      }
    }
    else
    {
      if (ReadPaymentMethodType2())
      {
        export.PaymentMethodType.Assign(entities.PaymentMethodType);

        return;
      }

      ExitState = "FN0000_PYMNT_MTHD_TYPE_NF";
    }
  }

  private static void MovePaymentMethodType(PaymentMethodType source,
    PaymentMethodType target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Code = source.Code;
    target.EffectiveDate = source.EffectiveDate;
  }

  private bool ReadPaymentMethodType1()
  {
    entities.PaymentMethodType.Populated = false;

    return Read("ReadPaymentMethodType1",
      (db, command) =>
      {
        db.SetString(command, "code", import.PaymentMethodType.Code);
        db.SetDate(
          command, "effectiveDate",
          import.PaymentMethodType.EffectiveDate.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.PaymentMethodType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.PaymentMethodType.Code = db.GetString(reader, 1);
        entities.PaymentMethodType.Name = db.GetString(reader, 2);
        entities.PaymentMethodType.EffectiveDate = db.GetDate(reader, 3);
        entities.PaymentMethodType.DiscontinueDate =
          db.GetNullableDate(reader, 4);
        entities.PaymentMethodType.CreatedBy = db.GetString(reader, 5);
        entities.PaymentMethodType.CreatedTimestamp = db.GetDateTime(reader, 6);
        entities.PaymentMethodType.LastUpdatedBy =
          db.GetNullableString(reader, 7);
        entities.PaymentMethodType.LastUpdatedTmst =
          db.GetNullableDateTime(reader, 8);
        entities.PaymentMethodType.Description =
          db.GetNullableString(reader, 9);
        entities.PaymentMethodType.Populated = true;
      });
  }

  private bool ReadPaymentMethodType2()
  {
    entities.PaymentMethodType.Populated = false;

    return Read("ReadPaymentMethodType2",
      (db, command) =>
      {
        db.SetString(command, "code", import.PaymentMethodType.Code);
      },
      (db, reader) =>
      {
        entities.PaymentMethodType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.PaymentMethodType.Code = db.GetString(reader, 1);
        entities.PaymentMethodType.Name = db.GetString(reader, 2);
        entities.PaymentMethodType.EffectiveDate = db.GetDate(reader, 3);
        entities.PaymentMethodType.DiscontinueDate =
          db.GetNullableDate(reader, 4);
        entities.PaymentMethodType.CreatedBy = db.GetString(reader, 5);
        entities.PaymentMethodType.CreatedTimestamp = db.GetDateTime(reader, 6);
        entities.PaymentMethodType.LastUpdatedBy =
          db.GetNullableString(reader, 7);
        entities.PaymentMethodType.LastUpdatedTmst =
          db.GetNullableDateTime(reader, 8);
        entities.PaymentMethodType.Description =
          db.GetNullableString(reader, 9);
        entities.PaymentMethodType.Populated = true;
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
    /// A value of Flag.
    /// </summary>
    [JsonPropertyName("flag")]
    public Common Flag
    {
      get => flag ??= new();
      set => flag = value;
    }

    /// <summary>
    /// A value of PaymentMethodType.
    /// </summary>
    [JsonPropertyName("paymentMethodType")]
    public PaymentMethodType PaymentMethodType
    {
      get => paymentMethodType ??= new();
      set => paymentMethodType = value;
    }

    private Common flag;
    private PaymentMethodType paymentMethodType;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of PaymentMethodType.
    /// </summary>
    [JsonPropertyName("paymentMethodType")]
    public PaymentMethodType PaymentMethodType
    {
      get => paymentMethodType ??= new();
      set => paymentMethodType = value;
    }

    private PaymentMethodType paymentMethodType;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of PaymentMethodType.
    /// </summary>
    [JsonPropertyName("paymentMethodType")]
    public PaymentMethodType PaymentMethodType
    {
      get => paymentMethodType ??= new();
      set => paymentMethodType = value;
    }

    private PaymentMethodType paymentMethodType;
  }
#endregion
}
