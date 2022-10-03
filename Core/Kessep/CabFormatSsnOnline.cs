// Program: CAB_FORMAT_SSN_ONLINE, ID: 945074858, model: 746.
// Short name: SWE03701
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: CAB_FORMAT_SSN_ONLINE.
/// </summary>
[Serializable]
public partial class CabFormatSsnOnline: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the CAB_FORMAT_SSN_ONLINE program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new CabFormatSsnOnline(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of CabFormatSsnOnline.
  /// </summary>
  public CabFormatSsnOnline(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    export.FormattedSsn.Text11 = "";

    if (IsEmpty(import.Ssn.SsnText9))
    {
      export.FormattedSsn.Text11 = "";
    }
    else
    {
      export.FormattedSsn.Text11 =
        Substring(import.Ssn.SsnText9, SsnWorkArea.SsnText9_MaxLength, 1, 3) + "-"
        ;
      export.FormattedSsn.Text11 = TrimEnd(export.FormattedSsn.Text11) + Substring
        (import.Ssn.SsnText9, SsnWorkArea.SsnText9_MaxLength, 4, 2);
      export.FormattedSsn.Text11 = TrimEnd(export.FormattedSsn.Text11) + "-";
      export.FormattedSsn.Text11 = TrimEnd(export.FormattedSsn.Text11) + Substring
        (import.Ssn.SsnText9, SsnWorkArea.SsnText9_MaxLength, 6, 4);
    }
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
    /// A value of Ssn.
    /// </summary>
    [JsonPropertyName("ssn")]
    public SsnWorkArea Ssn
    {
      get => ssn ??= new();
      set => ssn = value;
    }

    private SsnWorkArea ssn;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of FormattedSsn.
    /// </summary>
    [JsonPropertyName("formattedSsn")]
    public WorkArea FormattedSsn
    {
      get => formattedSsn ??= new();
      set => formattedSsn = value;
    }

    private WorkArea formattedSsn;
  }
#endregion
}
