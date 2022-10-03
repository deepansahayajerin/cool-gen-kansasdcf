// Program: SI_LIKE_NAME_SEARCH, ID: 371451434, model: 746.
// Short name: SWE01182
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: SI_LIKE_NAME_SEARCH.
/// </para>
/// <para>
/// RESP: SRVINIT
/// </para>
/// </summary>
[Serializable]
public partial class SiLikeNameSearch: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_LIKE_NAME_SEARCH program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiLikeNameSearch(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiLikeNameSearch.
  /// </summary>
  public SiLikeNameSearch(IContext context, Import import, Export export):
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
    //        M A I N T E N A N C E   L O G
    //  Date   Developer    Description
    // 7-10-95 Ken Evans    Initial Development
    // ---------------------------------------------
    local.Filler.CallerLastName = "%%%%%%%%%%%%%%%%%";

    // *********************************************
    // The following will first determine the length
    // of the entered field, then pad the remaining
    // spaces with percent sign(s).
    // *********************************************
    local.Blank.Count =
      Length(TrimEnd(import.InformationRequest.ApplicantFirstName));

    if (local.Blank.Count > 0)
    {
      export.InformationRequest.ApplicantFirstName =
        Substring(import.InformationRequest.ApplicantFirstName, 12, 1,
        local.Blank.Count) + Substring
        (local.Filler.CallerLastName, 17, local.Blank.Count,
        InformationRequest.ApplicantFirstName_MaxLength - local.Blank.Count);
    }

    local.Blank.Count =
      Length(TrimEnd(import.InformationRequest.ApplicantLastName));

    if (local.Blank.Count > 0)
    {
      export.InformationRequest.ApplicantLastName =
        Substring(import.InformationRequest.ApplicantLastName, 17, 1,
        local.Blank.Count) + Substring
        (local.Filler.CallerLastName, 17, local.Blank.Count,
        InformationRequest.ApplicantLastName_MaxLength - local.Blank.Count);
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
    /// A value of InformationRequest.
    /// </summary>
    [JsonPropertyName("informationRequest")]
    public InformationRequest InformationRequest
    {
      get => informationRequest ??= new();
      set => informationRequest = value;
    }

    private InformationRequest informationRequest;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of InformationRequest.
    /// </summary>
    [JsonPropertyName("informationRequest")]
    public InformationRequest InformationRequest
    {
      get => informationRequest ??= new();
      set => informationRequest = value;
    }

    private InformationRequest informationRequest;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of Filler.
    /// </summary>
    [JsonPropertyName("filler")]
    public InformationRequest Filler
    {
      get => filler ??= new();
      set => filler = value;
    }

    /// <summary>
    /// A value of Blank.
    /// </summary>
    [JsonPropertyName("blank")]
    public Common Blank
    {
      get => blank ??= new();
      set => blank = value;
    }

    private InformationRequest filler;
    private Common blank;
  }
#endregion
}
