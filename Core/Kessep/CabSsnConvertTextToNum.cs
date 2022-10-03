// Program: CAB_SSN_CONVERT_TEXT_TO_NUM, ID: 371455760, model: 746.
// Short name: SWE01541
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: CAB_SSN_CONVERT_TEXT_TO_NUM.
/// </para>
/// <para>
/// RESP: SRVINIT
/// This action block will convert a 9 digit ssn to a 9 digit numeric field and 
/// also in a 3 numeric field form (999 99 9999).
/// </para>
/// </summary>
[Serializable]
public partial class CabSsnConvertTextToNum: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the CAB_SSN_CONVERT_TEXT_TO_NUM program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new CabSsnConvertTextToNum(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of CabSsnConvertTextToNum.
  /// </summary>
  public CabSsnConvertTextToNum(IContext context, Import import, Export export):
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
    export.SsnWorkArea.SsnText9 = import.SsnWorkArea.SsnText9;
    local.Common.Flag = "N";

    if (!IsEmpty(import.SsnWorkArea.SsnText9))
    {
      if (StringToNumber(Substring(
        export.SsnWorkArea.SsnText9, SsnWorkArea.SsnText9_MaxLength, 1, 1)) >= 0
        && StringToNumber
        (Substring(
          export.SsnWorkArea.SsnText9, SsnWorkArea.SsnText9_MaxLength, 1, 1)) <=
          9)
      {
        if (StringToNumber(Substring(
          export.SsnWorkArea.SsnText9, SsnWorkArea.SsnText9_MaxLength, 2, 1)) >=
            0 && StringToNumber
          (Substring(
            export.SsnWorkArea.SsnText9, SsnWorkArea.SsnText9_MaxLength, 2,
          1)) <= 9)
        {
          if (StringToNumber(Substring(
            export.SsnWorkArea.SsnText9, SsnWorkArea.SsnText9_MaxLength, 3,
            1)) >= 0 && StringToNumber
            (Substring(
              export.SsnWorkArea.SsnText9, SsnWorkArea.SsnText9_MaxLength, 3,
            1)) <= 9)
          {
            if (StringToNumber(Substring(
              export.SsnWorkArea.SsnText9, SsnWorkArea.SsnText9_MaxLength, 4,
              1)) >= 0 && StringToNumber
              (Substring(
                export.SsnWorkArea.SsnText9, SsnWorkArea.SsnText9_MaxLength, 4,
              1)) <= 9)
            {
              if (StringToNumber(Substring(
                export.SsnWorkArea.SsnText9, SsnWorkArea.SsnText9_MaxLength, 5,
                1)) >= 0 && StringToNumber
                (Substring(
                  export.SsnWorkArea.SsnText9, SsnWorkArea.SsnText9_MaxLength,
                5, 1)) <= 9)
              {
                if (StringToNumber(Substring(
                  export.SsnWorkArea.SsnText9, SsnWorkArea.SsnText9_MaxLength,
                  6, 1)) >= 0 && StringToNumber
                  (Substring(
                    export.SsnWorkArea.SsnText9, SsnWorkArea.SsnText9_MaxLength,
                  6, 1)) <= 9)
                {
                  if (StringToNumber(Substring(
                    export.SsnWorkArea.SsnText9, SsnWorkArea.SsnText9_MaxLength,
                    7, 1)) >= 0 && StringToNumber
                    (Substring(
                      export.SsnWorkArea.SsnText9,
                    SsnWorkArea.SsnText9_MaxLength, 7, 1)) <= 9)
                  {
                    if (StringToNumber(Substring(
                      export.SsnWorkArea.SsnText9,
                      SsnWorkArea.SsnText9_MaxLength, 8, 1)) >= 0 && StringToNumber
                      (Substring(
                        export.SsnWorkArea.SsnText9,
                      SsnWorkArea.SsnText9_MaxLength, 8, 1)) <= 9)
                    {
                      if (StringToNumber(Substring(
                        export.SsnWorkArea.SsnText9,
                        SsnWorkArea.SsnText9_MaxLength, 9, 1)) >= 0 && StringToNumber
                        (Substring(
                          export.SsnWorkArea.SsnText9,
                        SsnWorkArea.SsnText9_MaxLength, 9, 1)) <= 9)
                      {
                        export.SsnWorkArea.SsnNum9 =
                          (int)StringToNumber(export.SsnWorkArea.SsnText9);
                        export.SsnWorkArea.SsnNumPart1 =
                          (int)StringToNumber(Substring(
                            export.SsnWorkArea.SsnText9,
                          SsnWorkArea.SsnText9_MaxLength, 1, 3));
                        export.SsnWorkArea.SsnNumPart2 =
                          (int)StringToNumber(Substring(
                            export.SsnWorkArea.SsnText9,
                          SsnWorkArea.SsnText9_MaxLength, 4, 2));
                        export.SsnWorkArea.SsnNumPart3 =
                          (int)StringToNumber(Substring(
                            export.SsnWorkArea.SsnText9,
                          SsnWorkArea.SsnText9_MaxLength, 6, 4));
                        local.Common.Flag = "Y";
                      }
                    }
                  }
                }
              }
            }
          }
        }
      }

      if (AsChar(local.Common.Flag) != 'Y')
      {
        ExitState = "INVALID_UNIT_MUST_BE_NUMERIC";
      }
    }
    else
    {
      export.SsnWorkArea.SsnNum9 = 0;
      export.SsnWorkArea.SsnNumPart1 = 0;
      export.SsnWorkArea.SsnNumPart2 = 0;
      export.SsnWorkArea.SsnNumPart3 = 0;
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

    private Common common;
  }
#endregion
}
