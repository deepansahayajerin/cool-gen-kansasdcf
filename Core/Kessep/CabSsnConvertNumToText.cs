// Program: CAB_SSN_CONVERT_NUM_TO_TEXT, ID: 371455761, model: 746.
// Short name: SWE01542
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: CAB_SSN_CONVERT_NUM_TO_TEXT.
/// </para>
/// <para>
/// RESP: SRVINIT
/// This action block will convert an imported ssn number to a 9 digit text 
/// field.  An option flag will indicated if the ssn is either (option 1) in a 9
/// digit numeric field or (option 2) it is in 3 seperate numeric fields.
/// </para>
/// </summary>
[Serializable]
public partial class CabSsnConvertNumToText: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the CAB_SSN_CONVERT_NUM_TO_TEXT program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new CabSsnConvertNumToText(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of CabSsnConvertNumToText.
  /// </summary>
  public CabSsnConvertNumToText(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ---------------------------------------------
    //     M A I N T E N A N C E    L O G
    //   Date   Developer	Description
    // 06/09/96 G Lofton	Initial Development
    // ---------------------------------------------
    MoveSsnWorkArea(import.SsnWorkArea, export.SsnWorkArea);

    if (AsChar(import.SsnWorkArea.ConvertOption) == '1')
    {
      if (export.SsnWorkArea.SsnNum9 > 0)
      {
        export.SsnWorkArea.SsnText9 =
          NumberToString(export.SsnWorkArea.SsnNum9, 9);
        export.SsnWorkArea.SsnNumPart1 =
          (int)StringToNumber(Substring(
            export.SsnWorkArea.SsnText9, SsnWorkArea.SsnText9_MaxLength, 1, 3));
          
        export.SsnWorkArea.SsnNumPart2 =
          (int)StringToNumber(Substring(
            export.SsnWorkArea.SsnText9, SsnWorkArea.SsnText9_MaxLength, 4, 2));
          
        export.SsnWorkArea.SsnNumPart3 =
          (int)StringToNumber(Substring(
            export.SsnWorkArea.SsnText9, SsnWorkArea.SsnText9_MaxLength, 6, 4));
          
      }
      else
      {
        export.SsnWorkArea.SsnText9 = "";
        export.SsnWorkArea.SsnNumPart1 = 0;
        export.SsnWorkArea.SsnNumPart2 = 0;
        export.SsnWorkArea.SsnNumPart3 = 0;
        ExitState = "SI0000_SSN_NOT_NUMERIC";
      }
    }
    else if (AsChar(import.SsnWorkArea.ConvertOption) == '2')
    {
      export.SsnWorkArea.SsnNum9 = 0;

      if (export.SsnWorkArea.SsnNumPart1 >= 0)
      {
        if (export.SsnWorkArea.SsnNumPart1 > 0)
        {
          export.SsnWorkArea.SsnNum9 = (int)(export.SsnWorkArea.SsnNum9 + (
            long)export.SsnWorkArea.SsnNumPart1 * 1000000);
        }
      }
      else
      {
        ExitState = "SI0000_SSN_NOT_NUMERIC";
      }

      if (export.SsnWorkArea.SsnNumPart2 >= 0)
      {
        if (export.SsnWorkArea.SsnNumPart2 > 0)
        {
          export.SsnWorkArea.SsnNum9 += export.SsnWorkArea.SsnNumPart2 * 10000;
        }
      }
      else
      {
        ExitState = "SI0000_SSN_NOT_NUMERIC";
      }

      if (export.SsnWorkArea.SsnNumPart3 >= 0)
      {
        if (export.SsnWorkArea.SsnNumPart3 > 0)
        {
          export.SsnWorkArea.SsnNum9 += export.SsnWorkArea.SsnNumPart3;
        }
      }
      else
      {
        ExitState = "SI0000_SSN_NOT_NUMERIC";
      }

      if (export.SsnWorkArea.SsnNum9 == 0)
      {
        export.SsnWorkArea.SsnText9 = "";
      }
      else
      {
        export.SsnWorkArea.SsnText9 =
          NumberToString(export.SsnWorkArea.SsnNum9, 9);
      }
    }
    else
    {
      export.SsnWorkArea.SsnText9 = "";
      ExitState = "SI0000_INVALID_SSN_OPTION_REQD";
    }
  }

  private static void MoveSsnWorkArea(SsnWorkArea source, SsnWorkArea target)
  {
    target.SsnNum9 = source.SsnNum9;
    target.SsnNumPart1 = source.SsnNumPart1;
    target.SsnNumPart2 = source.SsnNumPart2;
    target.SsnNumPart3 = source.SsnNumPart3;
  }
#endregion

#region Parameters.
  protected readonly Import import;
  protected readonly Export export;
#endregion

#region Structures
  /// <summary>
  /// This class defines import view.
  /// </summary>
  [Serializable]
  public class Import
  {
    /// <summary>
    /// A value of SsnWorkArea.
    /// </summary>
    [JsonPropertyName("ssnWorkArea")]
    public SsnWorkArea SsnWorkArea
    {
      get => ssnWorkArea ??= new();
      set => ssnWorkArea = value;
    }

    private SsnWorkArea ssnWorkArea;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of SsnWorkArea.
    /// </summary>
    [JsonPropertyName("ssnWorkArea")]
    public SsnWorkArea SsnWorkArea
    {
      get => ssnWorkArea ??= new();
      set => ssnWorkArea = value;
    }

    private SsnWorkArea ssnWorkArea;
  }
#endregion
}
