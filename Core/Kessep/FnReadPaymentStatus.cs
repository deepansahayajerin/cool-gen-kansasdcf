// Program: FN_READ_PAYMENT_STATUS, ID: 371839447, model: 746.
// Short name: SWE00578
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: FN_READ_PAYMENT_STATUS.
/// </para>
/// <para>
/// RESP: FINANCE
/// This process will read the payment status information for a particular 
/// payment status.
/// </para>
/// </summary>
[Serializable]
public partial class FnReadPaymentStatus: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_READ_PAYMENT_STATUS program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnReadPaymentStatus(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnReadPaymentStatus.
  /// </summary>
  public FnReadPaymentStatus(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    MovePaymentStatus(import.PaymentStatus, export.PaymentStatus);

    if (AsChar(import.Flag.Flag) == 'Y')
    {
      if (ReadPaymentStatus1())
      {
        export.PaymentStatus.Assign(entities.PaymentStatus);
      }
      else
      {
        ExitState = "FN0000_PYMNT_STAT_NF";
      }
    }
    else
    {
      if (ReadPaymentStatus2())
      {
        export.PaymentStatus.Assign(entities.PaymentStatus);

        return;
      }

      ExitState = "FN0000_PYMNT_STAT_NF";
    }
  }

  private static void MovePaymentStatus(PaymentStatus source,
    PaymentStatus target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Code = source.Code;
    target.EffectiveDate = source.EffectiveDate;
  }

  private bool ReadPaymentStatus1()
  {
    entities.PaymentStatus.Populated = false;

    return Read("ReadPaymentStatus1",
      (db, command) =>
      {
        db.SetString(command, "code", import.PaymentStatus.Code);
        db.SetDate(
          command, "effectiveDate",
          import.PaymentStatus.EffectiveDate.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.PaymentStatus.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.PaymentStatus.Code = db.GetString(reader, 1);
        entities.PaymentStatus.Name = db.GetString(reader, 2);
        entities.PaymentStatus.EffectiveDate = db.GetDate(reader, 3);
        entities.PaymentStatus.DiscontinueDate = db.GetNullableDate(reader, 4);
        entities.PaymentStatus.CreatedBy = db.GetString(reader, 5);
        entities.PaymentStatus.CreatedTimestamp = db.GetDateTime(reader, 6);
        entities.PaymentStatus.LastUpdateBy = db.GetNullableString(reader, 7);
        entities.PaymentStatus.LastUpdateTmst =
          db.GetNullableDateTime(reader, 8);
        entities.PaymentStatus.Description = db.GetNullableString(reader, 9);
        entities.PaymentStatus.Populated = true;
      });
  }

  private bool ReadPaymentStatus2()
  {
    entities.PaymentStatus.Populated = false;

    return Read("ReadPaymentStatus2",
      (db, command) =>
      {
        db.SetString(command, "code", import.PaymentStatus.Code);
      },
      (db, reader) =>
      {
        entities.PaymentStatus.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.PaymentStatus.Code = db.GetString(reader, 1);
        entities.PaymentStatus.Name = db.GetString(reader, 2);
        entities.PaymentStatus.EffectiveDate = db.GetDate(reader, 3);
        entities.PaymentStatus.DiscontinueDate = db.GetNullableDate(reader, 4);
        entities.PaymentStatus.CreatedBy = db.GetString(reader, 5);
        entities.PaymentStatus.CreatedTimestamp = db.GetDateTime(reader, 6);
        entities.PaymentStatus.LastUpdateBy = db.GetNullableString(reader, 7);
        entities.PaymentStatus.LastUpdateTmst =
          db.GetNullableDateTime(reader, 8);
        entities.PaymentStatus.Description = db.GetNullableString(reader, 9);
        entities.PaymentStatus.Populated = true;
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
    /// A value of Flag.
    /// </summary>
    [JsonPropertyName("flag")]
    public Common Flag
    {
      get => flag ??= new();
      set => flag = value;
    }

    /// <summary>
    /// A value of PaymentStatus.
    /// </summary>
    [JsonPropertyName("paymentStatus")]
    public PaymentStatus PaymentStatus
    {
      get => paymentStatus ??= new();
      set => paymentStatus = value;
    }

    private Common flag;
    private PaymentStatus paymentStatus;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of PaymentStatus.
    /// </summary>
    [JsonPropertyName("paymentStatus")]
    public PaymentStatus PaymentStatus
    {
      get => paymentStatus ??= new();
      set => paymentStatus = value;
    }

    private PaymentStatus paymentStatus;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of DateWorkArea.
    /// </summary>
    [JsonPropertyName("dateWorkArea")]
    public DateWorkArea DateWorkArea
    {
      get => dateWorkArea ??= new();
      set => dateWorkArea = value;
    }

    /// <summary>
    /// A value of Used.
    /// </summary>
    [JsonPropertyName("used")]
    public DateWorkArea Used
    {
      get => used ??= new();
      set => used = value;
    }

    /// <summary>
    /// A value of Maximum.
    /// </summary>
    [JsonPropertyName("maximum")]
    public DateWorkArea Maximum
    {
      get => maximum ??= new();
      set => maximum = value;
    }

    /// <summary>
    /// A value of Zero.
    /// </summary>
    [JsonPropertyName("zero")]
    public DateWorkArea Zero
    {
      get => zero ??= new();
      set => zero = value;
    }

    private DateWorkArea dateWorkArea;
    private DateWorkArea used;
    private DateWorkArea maximum;
    private DateWorkArea zero;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of PaymentStatus.
    /// </summary>
    [JsonPropertyName("paymentStatus")]
    public PaymentStatus PaymentStatus
    {
      get => paymentStatus ??= new();
      set => paymentStatus = value;
    }

    private PaymentStatus paymentStatus;
  }
#endregion
}
