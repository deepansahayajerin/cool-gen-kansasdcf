// Program: OE_EAB_KDMV_RESPONSE_NON_MATCH, ID: 371367335, model: 746.
// Short name: SWEXER10
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Worksets;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: OE_EAB_KDMV_RESPONSE_NON_MATCH.
/// </para>
/// <para>
/// Resp:OBLGEST
/// This EXTERNAL cab carries the view that will be used for receiving the dmv 
/// non-match responses for the request for driver's license information.
/// </para>
/// </summary>
[Serializable]
public partial class OeEabKdmvResponseNonMatch: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_EAB_KDMV_RESPONSE_NON_MATCH program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeEabKdmvResponseNonMatch(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeEabKdmvResponseNonMatch.
  /// </summary>
  public OeEabKdmvResponseNonMatch(IContext context, Import import,
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
    GetService<IEabStub>().Execute(
      "SWEXER10", context, import, export, EabOptions.Hpvp);
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
    [Member(Index = 1, Members = new[] { "FileInstruction", "TextLine8" })]
    public External External
    {
      get => external ??= new();
      set => external = value;
    }

    private External external;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of KdmvFile.
    /// </summary>
    [JsonPropertyName("kdmvFile")]
    [Member(Index = 1, AccessFields = false, Members = new[]
    {
      "CsePersonNumber",
      "LastName",
      "FirstName",
      "Ssn",
      "Dob",
      "FileType",
      "DriverLicenseNumber",
      "RequestStatus",
      "DmvProblemText"
    })]
    public KdmvFile KdmvFile
    {
      get => kdmvFile ??= new();
      set => kdmvFile = value;
    }

    /// <summary>
    /// A value of External.
    /// </summary>
    [JsonPropertyName("external")]
    [Member(Index = 2, Members = new[]
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

    private KdmvFile kdmvFile;
    private External external;
  }
#endregion
}
