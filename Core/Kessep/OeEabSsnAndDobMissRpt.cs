// Program: OE_EAB_SSN_AND_DOB_MISS_RPT, ID: 371320726, model: 746.
// Short name: SWEXEW04
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Worksets;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: OE_EAB_SSN_AND_DOB_MISS_RPT.
/// </para>
/// <para>
/// Resp:OBLGEST
/// This EXTERNAL procedure carries view that will be used for receiving the 
/// FPLS responses from the Federal Parent Locator Service in reaction to
/// FPLS_LOCATE_REQUEST's.
/// </para>
/// </summary>
[Serializable]
public partial class OeEabSsnAndDobMissRpt: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_EAB_SSN_AND_DOB_MISS_RPT program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeEabSsnAndDobMissRpt(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeEabSsnAndDobMissRpt.
  /// </summary>
  public OeEabSsnAndDobMissRpt(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    GetService<IEabStub>().Execute(
      "SWEXEW04", context, import, export, EabOptions.Hpvp);
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
    /// A value of External.
    /// </summary>
    [JsonPropertyName("external")]
    [Member(Index = 1, Members = new[] { "FileInstruction" })]
    public External External
    {
      get => external ??= new();
      set => external = value;
    }

    /// <summary>
    /// A value of MissingSsnAndDodRpt.
    /// </summary>
    [JsonPropertyName("missingSsnAndDodRpt")]
    [Member(Index = 2, AccessFields = false, Members = new[]
    {
      "CsePersonNumber",
      "LastName",
      "FirstName",
      "MiddleInt",
      "Ssn",
      "Dob",
      "MissingAttribute"
    })]
    public MissingSsnAndDodRpt MissingSsnAndDodRpt
    {
      get => missingSsnAndDodRpt ??= new();
      set => missingSsnAndDodRpt = value;
    }

    private External external;
    private MissingSsnAndDodRpt missingSsnAndDodRpt;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of External.
    /// </summary>
    [JsonPropertyName("external")]
    [Member(Index = 1, Members = new[]
    {
      "FileInstruction",
      "NumericReturnCode",
      "TextReturnCode",
      "TextLine80"
    })]
    public External External
    {
      get => external ??= new();
      set => external = value;
    }

    private External external;
  }
#endregion
}
