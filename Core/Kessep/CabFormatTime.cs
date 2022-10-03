// Program: CAB_FORMAT_TIME, ID: 372400465, model: 746.
// Short name: CABFORM1
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: CAB_FORMAT_TIME.
/// </summary>
[Serializable]
public partial class CabFormatTime: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the CAB_FORMAT_TIME program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new CabFormatTime(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of CabFormatTime.
  /// </summary>
  public CabFormatTime(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    local.WorkArea.Text15 = NumberToString(TimeToInt(import.Time.Time), 15);
    export.FormattedTime.Text8 =
      Substring(local.WorkArea.Text15, WorkArea.Text15_MaxLength, 10, 2) + ":"
      + Substring(local.WorkArea.Text15, WorkArea.Text15_MaxLength, 12, 2) + ":"
      + Substring(local.WorkArea.Text15, WorkArea.Text15_MaxLength, 14, 2);
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
    /// A value of Time.
    /// </summary>
    [JsonPropertyName("time")]
    public DateWorkArea Time
    {
      get => time ??= new();
      set => time = value;
    }

    private DateWorkArea time;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of FormattedTime.
    /// </summary>
    [JsonPropertyName("formattedTime")]
    public TextWorkArea FormattedTime
    {
      get => formattedTime ??= new();
      set => formattedTime = value;
    }

    private TextWorkArea formattedTime;
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
