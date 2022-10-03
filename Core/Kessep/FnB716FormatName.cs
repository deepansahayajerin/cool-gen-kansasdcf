// Program: FN_B716_FORMAT_NAME, ID: 945253281, model: 746.
// Short name: SWE02980
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_B716_FORMAT_NAME.
/// </summary>
[Serializable]
public partial class FnB716FormatName: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_B716_FORMAT_NAME program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnB716FormatName(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnB716FormatName.
  /// </summary>
  public FnB716FormatName(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ------------------------------------------------------------------------
    //           M A I N T E N A N C E   L O G
    // Date	   Developer	Request #	Description
    // 10-07-13   LSS	        CQ37588	        Initial Development
    // ------------------------------------------------------------------------
    if (IsEmpty(import.CsePersonsWorkSet.FirstName) && IsEmpty
      (import.CsePersonsWorkSet.LastName) && IsEmpty
      (import.CsePersonsWorkSet.MiddleInitial))
    {
      export.CsePersonsWorkSet.FormattedName = "UNKNOWN";
    }
    else
    {
      export.CsePersonsWorkSet.FormattedName =
        TrimEnd(import.CsePersonsWorkSet.LastName) + " " + TrimEnd
        (import.CsePersonsWorkSet.FirstName) + " " + import
        .CsePersonsWorkSet.MiddleInitial;
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
    /// A value of CsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("csePersonsWorkSet")]
    public CsePersonsWorkSet CsePersonsWorkSet
    {
      get => csePersonsWorkSet ??= new();
      set => csePersonsWorkSet = value;
    }

    private CsePersonsWorkSet csePersonsWorkSet;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of CsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("csePersonsWorkSet")]
    public CsePersonsWorkSet CsePersonsWorkSet
    {
      get => csePersonsWorkSet ??= new();
      set => csePersonsWorkSet = value;
    }

    private CsePersonsWorkSet csePersonsWorkSet;
  }
#endregion
}
