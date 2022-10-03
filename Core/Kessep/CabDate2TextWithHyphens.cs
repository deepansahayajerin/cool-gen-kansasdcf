// Program: CAB_DATE2TEXT_WITH_HYPHENS, ID: 372055787, model: 746.
// Short name: SWE01760
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: CAB_DATE2TEXT_WITH_HYPHENS.
/// </summary>
[Serializable]
public partial class CabDate2TextWithHyphens: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the CAB_DATE2TEXT_WITH_HYPHENS program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new CabDate2TextWithHyphens(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of CabDate2TextWithHyphens.
  /// </summary>
  public CabDate2TextWithHyphens(IContext context, Import import, Export export):
    
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ----------------------------------------------------------------
    // NAME            DATE       DESCRIPTION
    // Alan Samuels	01061996   Initial Development
    // ----------------------------------------------------------------
    // ----------------------------------------------------------------
    // This CAB will take an imported date and convert it to a text
    // field in the following format: "YYYY-MM-DD"
    // ----------------------------------------------------------------
    local.TextWorkArea.Text8 =
      NumberToString(DateToInt(import.DateWorkArea.Date), 8, 8);
    local.BatchTimestampWorkArea.TextDateYyyy =
      Substring(local.TextWorkArea.Text8, 1, 4);
    local.BatchTimestampWorkArea.TextDateMm =
      Substring(local.TextWorkArea.Text8, 5, 2);
    local.BatchTimestampWorkArea.TestDateDd =
      Substring(local.TextWorkArea.Text8, 7, 2);
    export.TextWorkArea.Text10 = local.BatchTimestampWorkArea.TextDateYyyy + "-"
      + local.BatchTimestampWorkArea.TextDateMm + "-" + local
      .BatchTimestampWorkArea.TestDateDd;
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
    /// A value of DateWorkArea.
    /// </summary>
    [JsonPropertyName("dateWorkArea")]
    public DateWorkArea DateWorkArea
    {
      get => dateWorkArea ??= new();
      set => dateWorkArea = value;
    }

    private DateWorkArea dateWorkArea;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of TextWorkArea.
    /// </summary>
    [JsonPropertyName("textWorkArea")]
    public TextWorkArea TextWorkArea
    {
      get => textWorkArea ??= new();
      set => textWorkArea = value;
    }

    private TextWorkArea textWorkArea;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of BatchTimestampWorkArea.
    /// </summary>
    [JsonPropertyName("batchTimestampWorkArea")]
    public BatchTimestampWorkArea BatchTimestampWorkArea
    {
      get => batchTimestampWorkArea ??= new();
      set => batchTimestampWorkArea = value;
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

    private BatchTimestampWorkArea batchTimestampWorkArea;
    private TextWorkArea textWorkArea;
  }
#endregion
}
