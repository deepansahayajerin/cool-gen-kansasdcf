// Program: FN_CAB_RETURN_TEXT_DOLLARS, ID: 372117110, model: 746.
// Short name: SWE02190
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: FN_CAB_RETURN_TEXT_DOLLARS.
/// </para>
/// <para>
/// RESP: FIN
/// This common action block takes a dollar amount in numerical domain
/// and converts to the text domain.
/// </para>
/// </summary>
[Serializable]
public partial class FnCabReturnTextDollars: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_CAB_RETURN_TEXT_DOLLARS program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnCabReturnTextDollars(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnCabReturnTextDollars.
  /// </summary>
  public FnCabReturnTextDollars(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    // Syed Hasan,MTW    01-18-1998    Initial coding
    // ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    local.FinanceWorkAttributes.NumericalCentValue =
      (int)(import.FinanceWorkAttributes.NumericalDollarValue * 100);
    local.FinanceWorkAttributes.TextCentValue =
      NumberToString(local.FinanceWorkAttributes.NumericalCentValue, 7, 9);
    local.FinanceWorkAttributes.TextNonDecimalDollarPart =
      Substring(local.FinanceWorkAttributes.TextCentValue, 1, 7);

    for(local.CharPosition.Subscript = 1; local.CharPosition.Subscript <= 6; ++
      local.CharPosition.Subscript)
    {
      local.FirstCharInTextValue.Text1 =
        Substring(local.FinanceWorkAttributes.TextNonDecimalDollarPart, 1, 1);

      if (AsChar(local.FirstCharInTextValue.Text1) == '0')
      {
        local.TextValueLength.Count = 7 - local.CharPosition.Subscript;
        local.FinanceWorkAttributes.TextNonDecimalDollarPart =
          Substring(local.FinanceWorkAttributes.TextNonDecimalDollarPart, 2,
          local.TextValueLength.Count);
      }
      else
      {
        break;
      }
    }

    local.FinanceWorkAttributes.TextDecimalDollarPart =
      Substring(local.FinanceWorkAttributes.TextCentValue, 8, 2);
    export.FinanceWorkAttributes.TextDollarValue =
      TrimEnd(local.FinanceWorkAttributes.TextNonDecimalDollarPart) + "." + TrimEnd
      (local.FinanceWorkAttributes.TextDecimalDollarPart);
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
    /// A value of FinanceWorkAttributes.
    /// </summary>
    [JsonPropertyName("financeWorkAttributes")]
    public FinanceWorkAttributes FinanceWorkAttributes
    {
      get => financeWorkAttributes ??= new();
      set => financeWorkAttributes = value;
    }

    private FinanceWorkAttributes financeWorkAttributes;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of FinanceWorkAttributes.
    /// </summary>
    [JsonPropertyName("financeWorkAttributes")]
    public FinanceWorkAttributes FinanceWorkAttributes
    {
      get => financeWorkAttributes ??= new();
      set => financeWorkAttributes = value;
    }

    private FinanceWorkAttributes financeWorkAttributes;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of TextValueLength.
    /// </summary>
    [JsonPropertyName("textValueLength")]
    public Common TextValueLength
    {
      get => textValueLength ??= new();
      set => textValueLength = value;
    }

    /// <summary>
    /// A value of FirstCharInTextValue.
    /// </summary>
    [JsonPropertyName("firstCharInTextValue")]
    public TextWorkArea FirstCharInTextValue
    {
      get => firstCharInTextValue ??= new();
      set => firstCharInTextValue = value;
    }

    /// <summary>
    /// A value of CharPosition.
    /// </summary>
    [JsonPropertyName("charPosition")]
    public Common CharPosition
    {
      get => charPosition ??= new();
      set => charPosition = value;
    }

    /// <summary>
    /// A value of FinanceWorkAttributes.
    /// </summary>
    [JsonPropertyName("financeWorkAttributes")]
    public FinanceWorkAttributes FinanceWorkAttributes
    {
      get => financeWorkAttributes ??= new();
      set => financeWorkAttributes = value;
    }

    /// <summary>
    /// A value of EnableCode.
    /// </summary>
    [JsonPropertyName("enableCode")]
    public Common EnableCode
    {
      get => enableCode ??= new();
      set => enableCode = value;
    }

    private Common textValueLength;
    private TextWorkArea firstCharInTextValue;
    private Common charPosition;
    private FinanceWorkAttributes financeWorkAttributes;
    private Common enableCode;
  }
#endregion
}
