// Program: OE_EAB_WRITE_LOCATE_REQUESTS, ID: 374417879, model: 746.
// Short name: SWEXEE40
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Worksets;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: OE_EAB_WRITE_LOCATE_REQUESTS.
/// </para>
/// <para>
/// Write locate requests to a flat file
/// </para>
/// </summary>
[Serializable]
public partial class OeEabWriteLocateRequests: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_EAB_WRITE_LOCATE_REQUESTS program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeEabWriteLocateRequests(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeEabWriteLocateRequests.
  /// </summary>
  public OeEabWriteLocateRequests(IContext context, Import import, Export export)
    :
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
      "SWEXEE40", context, import, export, EabOptions.Hpvp);
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
    /// A value of ExternalLocateRequest.
    /// </summary>
    [JsonPropertyName("externalLocateRequest")]
    [Member(Index = 1, AccessFields = false, Members = new[]
    {
      "Ssn",
      "DateOfBirth",
      "RequestDate",
      "CsePersonNumber",
      "AgencyNumber",
      "SuspensionStatus"
    })]
    public ExternalLocateRequest ExternalLocateRequest
    {
      get => externalLocateRequest ??= new();
      set => externalLocateRequest = value;
    }

    /// <summary>
    /// A value of External.
    /// </summary>
    [JsonPropertyName("external")]
    [Member(Index = 2, AccessFields = false, Members
      = new[] { "FileInstruction" })]
    public External External
    {
      get => external ??= new();
      set => external = value;
    }

    private ExternalLocateRequest externalLocateRequest;
    private External external;
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
    [Member(Index = 1, AccessFields = false, Members = new[]
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
