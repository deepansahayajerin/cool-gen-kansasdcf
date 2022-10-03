// Program: CAB_SET_MAXIMUM_DISCONTINUE_DATE, ID: 371452062, model: 746.
// Short name: SWE00084
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: CAB_SET_MAXIMUM_DISCONTINUE_DATE.
/// </para>
/// <para>
/// RESP: FINANCE
/// This action block will manipulate the maximum date of 12/31/2099.  Given an 
/// optional input field it will either convert a zero date to the maximum, a
/// maximum to zero, or just return back the imported date unchanged if there is
/// no manipulation required.
/// </para>
/// </summary>
[Serializable]
public partial class CabSetMaximumDiscontinueDate: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the CAB_SET_MAXIMUM_DISCONTINUE_DATE program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new CabSetMaximumDiscontinueDate(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of CabSetMaximumDiscontinueDate.
  /// </summary>
  public CabSetMaximumDiscontinueDate(IContext context, Import import,
    Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    local.Maximum.Date = new DateTime(2099, 12, 31);

    if (Equal(import.DateWorkArea.Date, local.ZeroValueInitialized.Date))
    {
      export.DateWorkArea.Date = local.Maximum.Date;
    }
    else if (Equal(import.DateWorkArea.Date, local.Maximum.Date))
    {
      export.DateWorkArea.Date = local.ZeroValueInitialized.Date;
    }
    else
    {
      export.DateWorkArea.Date = import.DateWorkArea.Date;
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
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of ZeroValueInitialized.
    /// </summary>
    [JsonPropertyName("zeroValueInitialized")]
    public DateWorkArea ZeroValueInitialized
    {
      get => zeroValueInitialized ??= new();
      set => zeroValueInitialized = value;
    }

    /// <summary>
    /// A value of Maximum.
    /// </summary>
    [JsonPropertyName("maximum")]
    public DateWorkArea Maximum
    {
      get => maximum ??= new();
      set => maximum = value;
    }

    private DateWorkArea zeroValueInitialized;
    private DateWorkArea maximum;
  }
#endregion
}
