// Program: FN_CONVERT_CURRENCY_TO_TEXT, ID: 372134165, model: 746.
// Short name: SWE00328
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: FN_CONVERT_CURRENCY_TO_TEXT.
/// </para>
/// <para>
/// When IEF converts a number to text, the decimal value is truncated and the 
/// sign is ignored.
/// Example:  Textnum(-25.75) = 000000000000025
/// This action block converts a number with two decimal places to text with two
/// decimal places without truncating the decimal value.  The leading zeros
/// will also be removed.  If the absolute value is less than one, a zero is
/// placed in front of the decimal.  If the number is negative, a &quot;-&quot;
/// will be placed in front of the number.  If the number is positive, a &quot;+
/// &quot; will NOT be added.
/// Example:  -.25 ==>  -0.25
/// </para>
/// </summary>
[Serializable]
public partial class FnConvertCurrencyToText: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_CONVERT_CURRENCY_TO_TEXT program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnConvertCurrencyToText(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnConvertCurrencyToText.
  /// </summary>
  public FnConvertCurrencyToText(IContext context, Import import, Export export):
    
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
    // When IEF converts a number to text, the decimal value is
    // truncated and the sign is ignored.
    // Example:  textnum(-25.75) = 000000000000025
    // This action block converts a number with two decimal places
    // to text with two decimal places and a decimal point without
    // truncating the decimal value.  The leading zeros are also
    // removed.  If the absolute value is less than one, a zero is
    // placed in front of the decimal.  If the number is negative,
    // a "-" will be placed in front of the number.  If the number
    // is positive, a "+" will NOT be added.
    // Example:  -.25  ==>  -0.25
    // ************************************************************
    if (import.BatchConvertNumToText.Currency == 0)
    {
      export.BatchConvertNumToText.TextNumber16 = "0.00";

      return;
    }

    // *****  Remove the decimal.
    local.BatchConvertNumToText.Number15 =
      (long)(import.BatchConvertNumToText.Currency * 100);

    // *****  Convert number to text.
    local.BatchConvertNumToText.TextNumber15 =
      NumberToString(local.BatchConvertNumToText.Number15, 15);

    // *****  Insert decimal point.
    local.BatchConvertNumToText.TextNumber16 =
      Substring(local.BatchConvertNumToText.TextNumber15,
      BatchConvertNumToText.TextNumber15_MaxLength, 1, 13) + "." + Substring
      (local.BatchConvertNumToText.TextNumber15,
      BatchConvertNumToText.TextNumber15_MaxLength, 14, 2);

    // *****  Remove leading zeros.
    export.BatchConvertNumToText.TextNumber16 =
      Substring(local.BatchConvertNumToText.TextNumber16,
      Verify(local.BatchConvertNumToText.TextNumber16, "0"), 17 -
      Verify(local.BatchConvertNumToText.TextNumber16, "0"));

    // *****  If the absolute value is less than 1, place a zero in front of the
    // decimal point.
    if (import.BatchConvertNumToText.Currency < 1 && import
      .BatchConvertNumToText.Currency > -1)
    {
      export.BatchConvertNumToText.TextNumber16 = "0" + export
        .BatchConvertNumToText.TextNumber16;
    }

    // *****  If the value is negative, place a negative sign in front of the 
    // value.
    if (import.BatchConvertNumToText.Currency < 0)
    {
      export.BatchConvertNumToText.TextNumber16 = "-" + export
        .BatchConvertNumToText.TextNumber16;
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
    /// A value of BatchConvertNumToText.
    /// </summary>
    [JsonPropertyName("batchConvertNumToText")]
    public BatchConvertNumToText BatchConvertNumToText
    {
      get => batchConvertNumToText ??= new();
      set => batchConvertNumToText = value;
    }

    private BatchConvertNumToText batchConvertNumToText;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of BatchConvertNumToText.
    /// </summary>
    [JsonPropertyName("batchConvertNumToText")]
    public BatchConvertNumToText BatchConvertNumToText
    {
      get => batchConvertNumToText ??= new();
      set => batchConvertNumToText = value;
    }

    private BatchConvertNumToText batchConvertNumToText;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of BatchConvertNumToText.
    /// </summary>
    [JsonPropertyName("batchConvertNumToText")]
    public BatchConvertNumToText BatchConvertNumToText
    {
      get => batchConvertNumToText ??= new();
      set => batchConvertNumToText = value;
    }

    private BatchConvertNumToText batchConvertNumToText;
  }
#endregion
}
