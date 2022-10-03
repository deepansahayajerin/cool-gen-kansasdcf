// Program: SI_FORMAT_CSE_PERSON_NAME, ID: 371455886, model: 746.
// Short name: SWE01164
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: SI_FORMAT_CSE_PERSON_NAME.
/// </para>
/// <para>
/// RESP: SRVINIT
/// This action block takes the last name, first name and middle initial of a 
/// CSE Person and formats it, stripping out trailing blanks.
/// Last Name, First Name Middle Initial
/// </para>
/// </summary>
[Serializable]
public partial class SiFormatCsePersonName: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_FORMAT_CSE_PERSON_NAME program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiFormatCsePersonName(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiFormatCsePersonName.
  /// </summary>
  public SiFormatCsePersonName(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ---------------------------------------------------------
    //           M A I N T E N A N C E   L O G
    //  Date	   Developer	Request #	Description
    // 3-6-95	Helen Sharland	        0	Initial Development
    // ---------------------------------------------------------
    if (IsEmpty(import.CsePersonsWorkSet.FirstName) && IsEmpty
      (import.CsePersonsWorkSet.LastName) && IsEmpty
      (import.CsePersonsWorkSet.MiddleInitial))
    {
      export.CsePersonsWorkSet.FormattedName = "";
    }
    else
    {
      export.CsePersonsWorkSet.FormattedName =
        TrimEnd(import.CsePersonsWorkSet.LastName) + ", " + TrimEnd
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
