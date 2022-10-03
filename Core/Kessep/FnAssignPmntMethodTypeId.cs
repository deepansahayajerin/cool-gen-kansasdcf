// Program: FN_ASSIGN_PMNT_METHOD_TYPE_ID, ID: 371828634, model: 746.
// Short name: SWE00290
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: FN_ASSIGN_PMNT_METHOD_TYPE_ID.
/// </para>
/// <para>
/// RESP: FINANCE
/// This action block will get the next payment method type id available for a 
/// creation of a new payment method type.
/// </para>
/// </summary>
[Serializable]
public partial class FnAssignPmntMethodTypeId: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_ASSIGN_PMNT_METHOD_TYPE_ID program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnAssignPmntMethodTypeId(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnAssignPmntMethodTypeId.
  /// </summary>
  public FnAssignPmntMethodTypeId(IContext context, Import import, Export export)
    :
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    if (ReadPaymentMethodType())
    {
      local.PaymentMethodType.SystemGeneratedIdentifier =
        entities.PaymentMethodType.SystemGeneratedIdentifier;
    }

    export.PaymentMethodType.SystemGeneratedIdentifier =
      local.PaymentMethodType.SystemGeneratedIdentifier + 1;
  }

  private bool ReadPaymentMethodType()
  {
    entities.PaymentMethodType.Populated = false;

    return Read("ReadPaymentMethodType",
      null,
      (db, reader) =>
      {
        entities.PaymentMethodType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.PaymentMethodType.Populated = true;
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
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
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
