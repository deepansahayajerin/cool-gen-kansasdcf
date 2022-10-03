// Program: FN_ASSIGN_PAYMENT_STATUS_ID, ID: 371839832, model: 746.
// Short name: SWE00289
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: FN_ASSIGN_PAYMENT_STATUS_ID.
/// </para>
/// <para>
/// RESP: FINANCE
/// </para>
/// </summary>
[Serializable]
public partial class FnAssignPaymentStatusId: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_ASSIGN_PAYMENT_STATUS_ID program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnAssignPaymentStatusId(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnAssignPaymentStatusId.
  /// </summary>
  public FnAssignPaymentStatusId(IContext context, Import import, Export export):
    
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    if (ReadPaymentStatus())
    {
      local.PaymentStatus.SystemGeneratedIdentifier =
        entities.PaymentStatus.SystemGeneratedIdentifier;
    }

    export.PaymentStatus.SystemGeneratedIdentifier =
      local.PaymentStatus.SystemGeneratedIdentifier + 1;
  }

  private bool ReadPaymentStatus()
  {
    entities.PaymentStatus.Populated = false;

    return Read("ReadPaymentStatus",
      null,
      (db, reader) =>
      {
        entities.PaymentStatus.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
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
