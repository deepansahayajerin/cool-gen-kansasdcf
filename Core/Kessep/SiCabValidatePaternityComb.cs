// Program: SI_CAB_VALIDATE_PATERNITY_COMB, ID: 374375743, model: 746.
// Short name: SWE02865
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SI_CAB_VALIDATE_PATERNITY_COMB.
/// </summary>
[Serializable]
public partial class SiCabValidatePaternityComb: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_CAB_VALIDATE_PATERNITY_COMB program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiCabValidatePaternityComb(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiCabValidatePaternityComb.
  /// </summary>
  public SiCabValidatePaternityComb(IContext context, Import import,
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
    // ****************************************************************
    // 02/24/2000  C. Ott  Action block used to validate combination of CSE 
    // Person Paternity indicators.  The imported indicators are concatenated
    // into a text string in this order:
    // 1. Born out of wedlock indicator
    // 2. CSE to establish paternity indicator
    // 3. Paternity established indicator
    // 05/10/2017  GVandy  CQ48108  IV-D PEP changes.  Combinations UYN, UYY & 
    // NYY are now valid.
    // ***************************************************************
    // The following chart details whether certain indicator combinations are 
    // valid.
    // BOW	CSE Est Pat     Pat Est      Valid combination
    // Y	Y	        Y	       yes
    // Y	Y	        N	       yes
    // Y	N	        Y	       yes
    // Y	U	        Y	       no
    // Y	N	        N	       no
    // Y	U	        N	       yes
    // N	Y	        Y	       yes
    // N	Y	        N	       no
    // N	N	        Y	       yes
    // N	N	        N	       no
    // N	U	        Y	       no
    // N	U	        N	       no
    // U	Y	        Y	       yes
    // U	Y	        N	       yes
    // U	N	        Y	       yes
    // U	N	        N	       no
    // U	U	        Y	       no
    // U	U	        N	       yes
    // ****************************************************************
    local.CodeCombinationConcat.Text3 =
      (import.CsePerson.BornOutOfWedlock ?? "") + (
        import.CsePerson.CseToEstblPaternity ?? "") + (
        import.CsePerson.PaternityEstablishedIndicator ?? "");

    switch(TrimEnd(local.CodeCombinationConcat.Text3))
    {
      case "YYY":
        break;
      case "YYN":
        break;
      case "YNY":
        break;
      case "YUN":
        break;
      case "NNY":
        break;
      case "NYY":
        break;
      case "UNY":
        break;
      case "UYN":
        break;
      case "UYY":
        break;
      case "UUN":
        break;
      default:
        ExitState = "SI0000_INV_PATERNITY_IND_COMB";

        break;
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
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
    }

    private CsePerson csePerson;
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
    /// A value of CodeCombinationConcat.
    /// </summary>
    [JsonPropertyName("codeCombinationConcat")]
    public WorkArea CodeCombinationConcat
    {
      get => codeCombinationConcat ??= new();
      set => codeCombinationConcat = value;
    }

    private WorkArea codeCombinationConcat;
  }
#endregion
}
