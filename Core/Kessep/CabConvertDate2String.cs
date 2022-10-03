// Program: CAB_CONVERT_DATE2STRING, ID: 371728351, model: 746.
// Short name: SWE01799
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: CAB_CONVERT_DATE2STRING.
/// </para>
/// <para>
/// RESP: OBLGESTB
/// Written by Raju     : Dec 17 1996
/// Input  : date ( date format )
/// Output : string mmddccyy
/// </para>
/// </summary>
[Serializable]
public partial class CabConvertDate2String: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the CAB_CONVERT_DATE2STRING program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new CabConvertDate2String(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of CabConvertDate2String.
  /// </summary>
  public CabConvertDate2String(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // --------------------------------------------
    // Change log
    // date		author	remarks
    // 12/17/96	raju	initial creation
    // --------------------------------------------
    export.TextWorkArea.Text8 =
      NumberToString(DateToInt(import.DateWorkArea.Date), 8, 8);
    local.DateCcyy.Text4 = Substring(export.TextWorkArea.Text8, 1, 4);
    local.DateMmdd.Text4 = Substring(export.TextWorkArea.Text8, 5, 4);
    export.TextWorkArea.Text8 = local.DateMmdd.Text4 + local.DateCcyy.Text4;
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
    /// A value of DateCcyy.
    /// </summary>
    [JsonPropertyName("dateCcyy")]
    public TextWorkArea DateCcyy
    {
      get => dateCcyy ??= new();
      set => dateCcyy = value;
    }

    /// <summary>
    /// A value of DateMmdd.
    /// </summary>
    [JsonPropertyName("dateMmdd")]
    public TextWorkArea DateMmdd
    {
      get => dateMmdd ??= new();
      set => dateMmdd = value;
    }

    private TextWorkArea dateCcyy;
    private TextWorkArea dateMmdd;
  }
#endregion
}
