// Program: FN_CAB_CURRENCY_TO_TEXT, ID: 371127185, model: 746.
// Short name: SWE02220
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: FN_CAB_CURRENCY_TO_TEXT.
/// </para>
/// <para>
/// : This cab takes a currency field and converts it to a zero suppressed,
///   right justified field.  A field that is zero will be returned as 0.00.
///   The biggest field that can be converted is 10.2 - 9999999999V99
///   Commas will be included in the formatted output if the input  commas 
/// required
///   flag is 'Y'.
///   The cab populates export text work attributes of length 10, 11, 12,13,14 
/// and
///   15.  However, it will not truncate in populating an export attribute - 
/// those   attributes that are smaller than the length of the formatted output
/// are left   blank..
///   This cab does not provide a negative sign in the text output for negative
/// numbers.
/// </para>
/// </summary>
[Serializable]
public partial class FnCabCurrencyToText: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_CAB_CURRENCY_TO_TEXT program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnCabCurrencyToText(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnCabCurrencyToText.
  /// </summary>
  public FnCabCurrencyToText(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // : This cab takes a currency field and converts it to a zero suppressed,
    //   right justified field.  A field that is zero will be returned as 0.00.
    //   The biggest field that can be converted is 10.2 - 9999999999V99
    //   Commas will be included in the formatted output if the input  commas 
    // required
    //   flag is 'Y'.
    // : If imported sign required flag is set, the number will be returned with
    // a +/- sign.
    //   A negative number will always be returned with a - sign.
    // : If imported no cents flag is set to 'Y', the field will be returned
    //   without the cents portion.
    // : Export return code will be spaces if all ok.  It can also have these 
    // values:
    // - OVERFLOW
    // : If an integer field was passed in, move it to a currency field.
    if (import.Common.Count > 0)
    {
      local.Common.TotalCurrency = import.Common.Count;
    }
    else
    {
      MoveCommon(import.Common, local.Common);
    }

    if (local.Common.TotalCurrency < 0)
    {
      local.Sign.Text1 = "-";
    }
    else if (AsChar(import.SignRequired.Flag) == 'Y')
    {
      local.Sign.Text1 = "+";
    }

    // : If we are sending back an integer,  round the number to an integer,
    //   then put it back into the currency fld.
    if (AsChar(import.ReturnAnInteger.Flag) == 'Y')
    {
      local.Common.TotalCurrency += 0.5M;
      local.Common.Count = (int)local.Common.TotalCurrency;
      local.Common.TotalCurrency = local.Common.Count;
    }

    if (local.Common.TotalCurrency == 0)
    {
      if (AsChar(import.ReturnAnInteger.Flag) == 'Y')
      {
        local.FormattedTextField.Text15 = "              0";
        local.SizeOfFormattedNumber.Count = 1;
      }
      else
      {
        local.FormattedTextField.Text15 = "          0.00";
        local.SizeOfFormattedNumber.Count = 4;
      }
    }
    else
    {
      local.TextWorkArea.Text2 =
        NumberToString((long)(local.Common.TotalCurrency * 100), 14, 2);
      local.FormattedTextField.Text15 =
        NumberToString((long)local.Common.TotalCurrency, 15);
      local.NumberOfLeadingSpaces.Count = 1;

      do
      {
        if (CharAt(local.FormattedTextField.Text15,
          local.NumberOfLeadingSpaces.Count) == '0')
        {
          ++local.NumberOfLeadingSpaces.Count;
        }
        else
        {
          --local.NumberOfLeadingSpaces.Count;
          local.SizeOfFormattedNumber.Count = 15 - local
            .NumberOfLeadingSpaces.Count;

          if (IsEmpty(local.Sign.Text1))
          {
            local.FormattedTextField.Text15 =
              Substring(local.Spaces.Text15, WorkArea.Text15_MaxLength, 1,
              local.NumberOfLeadingSpaces.Count) + Substring
              (local.FormattedTextField.Text15, WorkArea.Text15_MaxLength,
              local.NumberOfLeadingSpaces.Count +
              1, local.SizeOfFormattedNumber.Count + 0);
          }
          else
          {
            local.FormattedTextField.Text15 =
              Substring(local.Spaces.Text15, WorkArea.Text15_MaxLength, 1,
              local.NumberOfLeadingSpaces.Count - 1) + local.Sign.Text1 + Substring
              (local.FormattedTextField.Text15, WorkArea.Text15_MaxLength,
              local.NumberOfLeadingSpaces.Count +
              1, local.SizeOfFormattedNumber.Count + 0);
          }

          break;
        }
      }
      while(local.NumberOfLeadingSpaces.Count <= 15);

      if (local.NumberOfLeadingSpaces.Count > 15)
      {
        // The dollar portion of the number is zero.
        if (IsEmpty(local.Sign.Text1))
        {
          local.SizeOfFormattedNumber.Count = 0;
          local.FormattedTextField.Text15 = "";
        }
        else
        {
          local.SizeOfFormattedNumber.Count = 1;
          local.FormattedTextField.Text15 =
            Substring(local.Spaces.Text15, WorkArea.Text15_MaxLength, 1, 14) + local
            .Sign.Text1;
        }
      }

      if (AsChar(import.ReturnAnInteger.Flag) == 'Y')
      {
      }
      else
      {
        // : Add decimal portion of number to size.
        local.SizeOfFormattedNumber.Count += 3;
      }

      // : Check for overflow situation.
      if (local.SizeOfFormattedNumber.Count > 15)
      {
        export.TextWorkArea.Text10 = "OVERFLOW";

        return;
      }

      if (AsChar(import.CommasRequired.Flag) == 'Y')
      {
        if (local.SizeOfFormattedNumber.Count > 12)
        {
          // : Overflow.
          export.TextWorkArea.Text10 = "OVERFLOW";

          return;
        }

        // : There are only 2 possible commas.
        local.WorkSeg1.Text3 =
          Substring(local.FormattedTextField.Text15, 10, 3);
        local.WorkSeg2.Text3 =
          Substring(local.FormattedTextField.Text15, 13, 3);
        local.TextWorkArea.Text1 =
          Substring(local.FormattedTextField.Text15, 9, 1);

        if (IsEmpty(local.TextWorkArea.Text1) || AsChar
          (local.TextWorkArea.Text1) == AsChar(local.Sign.Text1))
        {
          local.TextWorkArea.Text1 =
            Substring(local.FormattedTextField.Text15, 12, 1);

          if (IsEmpty(local.TextWorkArea.Text1) || AsChar
            (local.TextWorkArea.Text1) == AsChar(local.Sign.Text1))
          {
          }
          else
          {
            local.FormattedTextField.Text15 =
              Substring(local.FormattedTextField.Text15,
              WorkArea.Text15_MaxLength, 2, 11) + "," + local.WorkSeg2.Text3;
            ++local.SizeOfFormattedNumber.Count;
          }
        }
        else
        {
          local.FormattedTextField.Text15 =
            Substring(local.FormattedTextField.Text15,
            WorkArea.Text15_MaxLength, 3, 7) + "," + local.WorkSeg1.Text3 + ","
            + local.WorkSeg2.Text3;
          local.SizeOfFormattedNumber.Count += 2;
        }
      }

      if (AsChar(import.ReturnAnInteger.Flag) == 'Y')
      {
      }
      else
      {
        local.FormattedTextField.Text15 =
          Substring(local.FormattedTextField.Text15, WorkArea.Text15_MaxLength,
          5, 11) + "." + local.TextWorkArea.Text2;
      }
    }

    // : Check for overflow situation
    if (local.SizeOfFormattedNumber.Count > 15)
    {
      export.TextWorkArea.Text10 = "OVERFLOW";
      export.TextWorkArea.Text10 = "**********";

      return;
    }

    // : Set the export fields.
    if (local.SizeOfFormattedNumber.Count <= 10)
    {
      export.TextWorkArea.Text10 =
        Substring(local.FormattedTextField.Text15, 6, 10);
      export.WorkArea.Text10 =
        Substring(local.FormattedTextField.Text15, 6, 10);
      export.WorkArea.Text11 =
        Substring(local.FormattedTextField.Text15, 5, 11);
      export.WorkArea.Text12 =
        Substring(local.FormattedTextField.Text15, 4, 12);
      export.WorkArea.Text13 =
        Substring(local.FormattedTextField.Text15, 3, 13);
      export.WorkArea.Text14 =
        Substring(local.FormattedTextField.Text15, 2, 14);
      export.WorkArea.Text15 = local.FormattedTextField.Text15;
    }
    else
    {
      export.TextWorkArea.Text10 = "**********";

      switch(local.SizeOfFormattedNumber.Count)
      {
        case 11:
          export.WorkArea.Text11 =
            Substring(local.FormattedTextField.Text15, 5, 11);
          export.WorkArea.Text12 =
            Substring(local.FormattedTextField.Text15, 4, 12);
          export.WorkArea.Text13 =
            Substring(local.FormattedTextField.Text15, 3, 13);
          export.WorkArea.Text14 =
            Substring(local.FormattedTextField.Text15, 2, 14);
          export.WorkArea.Text15 = local.FormattedTextField.Text15;

          break;
        case 12:
          export.WorkArea.Text12 =
            Substring(local.FormattedTextField.Text15, 4, 12);
          export.WorkArea.Text13 =
            Substring(local.FormattedTextField.Text15, 3, 13);
          export.WorkArea.Text14 =
            Substring(local.FormattedTextField.Text15, 2, 14);
          export.WorkArea.Text15 = local.FormattedTextField.Text15;

          break;
        case 13:
          export.WorkArea.Text13 =
            Substring(local.FormattedTextField.Text15, 3, 13);
          export.WorkArea.Text14 =
            Substring(local.FormattedTextField.Text15, 2, 14);
          export.WorkArea.Text15 = local.FormattedTextField.Text15;

          break;
        case 14:
          export.WorkArea.Text14 =
            Substring(local.FormattedTextField.Text15, 2, 14);
          export.WorkArea.Text15 = local.FormattedTextField.Text15;

          break;
        case 15:
          export.WorkArea.Text15 = local.FormattedTextField.Text15;

          break;
        default:
          break;
      }
    }
  }

  private static void MoveCommon(Common source, Common target)
  {
    target.Count = source.Count;
    target.TotalCurrency = source.TotalCurrency;
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
    /// A value of SignRequired.
    /// </summary>
    [JsonPropertyName("signRequired")]
    public Common SignRequired
    {
      get => signRequired ??= new();
      set => signRequired = value;
    }

    /// <summary>
    /// A value of CommasRequired.
    /// </summary>
    [JsonPropertyName("commasRequired")]
    public Common CommasRequired
    {
      get => commasRequired ??= new();
      set => commasRequired = value;
    }

    /// <summary>
    /// A value of Common.
    /// </summary>
    [JsonPropertyName("common")]
    public Common Common
    {
      get => common ??= new();
      set => common = value;
    }

    /// <summary>
    /// A value of ReturnAnInteger.
    /// </summary>
    [JsonPropertyName("returnAnInteger")]
    public Common ReturnAnInteger
    {
      get => returnAnInteger ??= new();
      set => returnAnInteger = value;
    }

    private Common signRequired;
    private Common commasRequired;
    private Common common;
    private Common returnAnInteger;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of WorkArea.
    /// </summary>
    [JsonPropertyName("workArea")]
    public WorkArea WorkArea
    {
      get => workArea ??= new();
      set => workArea = value;
    }

    /// <summary>
    /// A value of TextWorkArea.
    /// </summary>
    [JsonPropertyName("textWorkArea")]
    public TextWorkArea TextWorkArea
    {
      get => textWorkArea ??= new();
      set => textWorkArea = value;
    }

    /// <summary>
    /// A value of ReturnCode.
    /// </summary>
    [JsonPropertyName("returnCode")]
    public TextWorkArea ReturnCode
    {
      get => returnCode ??= new();
      set => returnCode = value;
    }

    private WorkArea workArea;
    private TextWorkArea textWorkArea;
    private TextWorkArea returnCode;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of Common.
    /// </summary>
    [JsonPropertyName("common")]
    public Common Common
    {
      get => common ??= new();
      set => common = value;
    }

    /// <summary>
    /// A value of SubstrCalcField.
    /// </summary>
    [JsonPropertyName("substrCalcField")]
    public Common SubstrCalcField
    {
      get => substrCalcField ??= new();
      set => substrCalcField = value;
    }

    /// <summary>
    /// A value of Sign.
    /// </summary>
    [JsonPropertyName("sign")]
    public TextWorkArea Sign
    {
      get => sign ??= new();
      set => sign = value;
    }

    /// <summary>
    /// A value of WorkSeg1.
    /// </summary>
    [JsonPropertyName("workSeg1")]
    public WorkArea WorkSeg1
    {
      get => workSeg1 ??= new();
      set => workSeg1 = value;
    }

    /// <summary>
    /// A value of WorkSeg2.
    /// </summary>
    [JsonPropertyName("workSeg2")]
    public WorkArea WorkSeg2
    {
      get => workSeg2 ??= new();
      set => workSeg2 = value;
    }

    /// <summary>
    /// A value of Spaces.
    /// </summary>
    [JsonPropertyName("spaces")]
    public WorkArea Spaces
    {
      get => spaces ??= new();
      set => spaces = value;
    }

    /// <summary>
    /// A value of FormattedTextField.
    /// </summary>
    [JsonPropertyName("formattedTextField")]
    public WorkArea FormattedTextField
    {
      get => formattedTextField ??= new();
      set => formattedTextField = value;
    }

    /// <summary>
    /// A value of SizeOfFormattedNumber.
    /// </summary>
    [JsonPropertyName("sizeOfFormattedNumber")]
    public Common SizeOfFormattedNumber
    {
      get => sizeOfFormattedNumber ??= new();
      set => sizeOfFormattedNumber = value;
    }

    /// <summary>
    /// A value of NumberOfLeadingSpaces.
    /// </summary>
    [JsonPropertyName("numberOfLeadingSpaces")]
    public Common NumberOfLeadingSpaces
    {
      get => numberOfLeadingSpaces ??= new();
      set => numberOfLeadingSpaces = value;
    }

    /// <summary>
    /// A value of TextWorkArea.
    /// </summary>
    [JsonPropertyName("textWorkArea")]
    public TextWorkArea TextWorkArea
    {
      get => textWorkArea ??= new();
      set => textWorkArea = value;
    }

    private Common common;
    private Common substrCalcField;
    private TextWorkArea sign;
    private WorkArea workSeg1;
    private WorkArea workSeg2;
    private WorkArea spaces;
    private WorkArea formattedTextField;
    private Common sizeOfFormattedNumber;
    private Common numberOfLeadingSpaces;
    private TextWorkArea textWorkArea;
  }
#endregion
}
