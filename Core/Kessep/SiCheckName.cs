// Program: SI_CHECK_NAME, ID: 373380284, model: 746.
// Short name: SWE00886
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: SI_CHECK_NAME.
/// </para>
/// <para>
/// SWE00886
/// </para>
/// </summary>
[Serializable]
public partial class SiCheckName: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_CHECK_NAME program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiCheckName(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiCheckName.
  /// </summary>
  public SiCheckName(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // -------------------------------------------------------------
    // 07/15/02 M. Lachowicz - Initial development for PR150856.
    //                         Check if first, last name and middle initial
    //                         contains valid characters only.
    // -------------------------------------------------------------
    // -------------------------------------------------------------
    // 11/01/02 M. Lachowicz - Accept ' in first and last name..
    //                         Work made on PR160147.
    // -------------------------------------------------------------
    if (CharAt(import.CsePersonsWorkSet.FirstName, 1) >= 'A' && CharAt
      (import.CsePersonsWorkSet.FirstName, 1) <= 'Z')
    {
    }
    else
    {
      ExitState = "SI0000_INVALID_NAME";

      return;
    }

    if (CharAt(import.CsePersonsWorkSet.LastName, 1) >= 'A' && CharAt
      (import.CsePersonsWorkSet.LastName, 1) <= 'Z')
    {
    }
    else
    {
      ExitState = "SI0000_INVALID_NAME";

      return;
    }

    if (IsEmpty(import.CsePersonsWorkSet.MiddleInitial) || AsChar
      (import.CsePersonsWorkSet.MiddleInitial) >= 'A' && AsChar
      (import.CsePersonsWorkSet.MiddleInitial) <= 'Z')
    {
    }
    else
    {
      ExitState = "SI0000_INVALID_NAME";

      return;
    }

    local.Common.Subscript = 1;

    for(var limit = Length(TrimEnd(import.CsePersonsWorkSet.FirstName)); local
      .Common.Subscript <= limit; ++local.Common.Subscript)
    {
      if (CharAt(import.CsePersonsWorkSet.FirstName, local.Common.Subscript) >=
        'A' && CharAt
        (import.CsePersonsWorkSet.FirstName, local.Common.Subscript) <= 'Z')
      {
      }
      else if (CharAt(import.CsePersonsWorkSet.FirstName, local.Common.Subscript)
        == '-' || IsEmpty
        (
          Substring(import.CsePersonsWorkSet.FirstName, local.Common.Subscript,
        1)) || CharAt
        (import.CsePersonsWorkSet.FirstName, local.Common.Subscript) == '\'')
      {
        if (Length(TrimEnd(import.CsePersonsWorkSet.FirstName)) == local
          .Common.Subscript)
        {
          break;
        }

        if (CharAt(import.CsePersonsWorkSet.FirstName, local.Common.Subscript +
          1) >= 'A' && CharAt
          (import.CsePersonsWorkSet.FirstName, local.Common.Subscript + 1) <= 'Z'
          )
        {
        }
        else
        {
          ExitState = "SI0000_INVALID_NAME";

          return;
        }
      }
      else
      {
        ExitState = "SI0000_INVALID_NAME";

        return;
      }
    }

    local.Common.Subscript = 1;

    for(var limit = Length(TrimEnd(import.CsePersonsWorkSet.LastName)); local
      .Common.Subscript <= limit; ++local.Common.Subscript)
    {
      if (CharAt(import.CsePersonsWorkSet.LastName, local.Common.Subscript) >= 'A'
        && CharAt
        (import.CsePersonsWorkSet.LastName, local.Common.Subscript) <= 'Z')
      {
      }
      else if (CharAt(import.CsePersonsWorkSet.LastName, local.Common.Subscript) ==
        '-' || IsEmpty
        (Substring(import.CsePersonsWorkSet.LastName, local.Common.Subscript, 1))
        || CharAt
        (import.CsePersonsWorkSet.LastName, local.Common.Subscript) == '\'')
      {
        if (Length(TrimEnd(import.CsePersonsWorkSet.LastName)) == local
          .Common.Subscript)
        {
          return;
        }

        if (CharAt(import.CsePersonsWorkSet.LastName, local.Common.Subscript + 1)
          >= 'A' && CharAt
          (import.CsePersonsWorkSet.LastName, local.Common.Subscript + 1) <= 'Z'
          )
        {
        }
        else
        {
          ExitState = "SI0000_INVALID_NAME";

          return;
        }
      }
      else
      {
        ExitState = "SI0000_INVALID_NAME";

        return;
      }
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
