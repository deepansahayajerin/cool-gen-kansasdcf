// Program: CAB_FORMAT_DATE_ONLINE, ID: 945074844, model: 746.
// Short name: SWE03700
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: CAB_FORMAT_DATE_ONLINE.
/// </summary>
[Serializable]
public partial class CabFormatDateOnline: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the CAB_FORMAT_DATE_ONLINE program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new CabFormatDateOnline(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of CabFormatDateOnline.
  /// </summary>
  public CabFormatDateOnline(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    export.FormattedDate.Text10 = "";
    local.WorkArea.Text15 = NumberToString(DateToInt(import.Date.Date), 15);

    if (Equal(local.WorkArea.Text15, "000000000000000"))
    {
      export.FormattedDate.Text10 = "";
    }
    else
    {
      export.FormattedDate.Text10 =
        Substring(local.WorkArea.Text15, WorkArea.Text15_MaxLength, 12, 2) + "-"
        + Substring(local.WorkArea.Text15, WorkArea.Text15_MaxLength, 14, 2) + "-"
        + Substring(local.WorkArea.Text15, WorkArea.Text15_MaxLength, 8, 4);
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
    /// A value of Date.
    /// </summary>
    [JsonPropertyName("date")]
    public DateWorkArea Date
    {
      get => date ??= new();
      set => date = value;
    }

    private DateWorkArea date;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of FormattedDate.
    /// </summary>
    [JsonPropertyName("formattedDate")]
    public WorkArea FormattedDate
    {
      get => formattedDate ??= new();
      set => formattedDate = value;
    }

    private WorkArea formattedDate;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
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

    private WorkArea workArea;
  }
#endregion
}
