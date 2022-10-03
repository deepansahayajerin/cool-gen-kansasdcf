// Program: CAB_TEXTNUM_9, ID: 372400445, model: 746.
// Short name: SWE02414
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: CAB_TEXTNUM_9.
/// </para>
/// <para>
/// This action block will convert a number to a text field.
/// </para>
/// </summary>
[Serializable]
public partial class CabTextnum9: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the CAB_TEXTNUM_9 program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new CabTextnum9(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of CabTextnum9.
  /// </summary>
  public CabTextnum9(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // This action block will convert a number to a text string.  The number 
    // being converted is limitted to 999,999,999.  An import number greater
    // than this limit will result in "*********" being returned back in the
    // export view.
    if (import.ElectronicFundTransmission.TransmissionIdentifier > 0)
    {
      local.Number.Count =
        import.ElectronicFundTransmission.TransmissionIdentifier;
    }
    else if (import.PaymentRequest.SystemGeneratedIdentifier > 0)
    {
      local.Number.Count = import.PaymentRequest.SystemGeneratedIdentifier;
    }
    else
    {
      local.Number.Count = import.Number.Count;
    }

    if (import.Number.Count > 999999999)
    {
      export.TextNumber.Text9 = "*********";
    }
    else
    {
      export.TextNumber.Text9 = NumberToString(local.Number.Count, 7, 9);
    }
  }
#endregion

#region Parameters.
  protected readonly Import import;
  protected readonly Export export;
  protected readonly Local local = new();
#endregion

#region Structures
  /// <summary>
  /// This class defines import view.
  /// </summary>
  [Serializable]
  public class Import
  {
    /// <summary>
    /// A value of Number.
    /// </summary>
    [JsonPropertyName("number")]
    public Common Number
    {
      get => number ??= new();
      set => number = value;
    }

    /// <summary>
    /// A value of ElectronicFundTransmission.
    /// </summary>
    [JsonPropertyName("electronicFundTransmission")]
    public ElectronicFundTransmission ElectronicFundTransmission
    {
      get => electronicFundTransmission ??= new();
      set => electronicFundTransmission = value;
    }

    /// <summary>
    /// A value of PaymentRequest.
    /// </summary>
    [JsonPropertyName("paymentRequest")]
    public PaymentRequest PaymentRequest
    {
      get => paymentRequest ??= new();
      set => paymentRequest = value;
    }

    private Common number;
    private ElectronicFundTransmission electronicFundTransmission;
    private PaymentRequest paymentRequest;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of TextNumber.
    /// </summary>
    [JsonPropertyName("textNumber")]
    public WorkArea TextNumber
    {
      get => textNumber ??= new();
      set => textNumber = value;
    }

    private WorkArea textNumber;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of Number.
    /// </summary>
    [JsonPropertyName("number")]
    public Common Number
    {
      get => number ??= new();
      set => number = value;
    }

    private Common number;
  }
#endregion
}
