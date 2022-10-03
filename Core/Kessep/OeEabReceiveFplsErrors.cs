// Program: OE_EAB_RECEIVE_FPLS_ERRORS, ID: 372366467, model: 746.
// Short name: SWEXEE06
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Worksets;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: OE_EAB_RECEIVE_FPLS_ERRORS.
/// </para>
/// <para>
/// Resp:OBLGEST
/// This EXTERNAL procedure carries view that will be used for receiving the 
/// FPLS responses from the Federal Parent Locator Service in reaction to
/// FPLS_LOCATE_REQUEST's.
/// Note: 7/13/96  The transaction error attribute returned is a 10 byte field 
/// containing up to the 5 most significant error codes, however KESSEP via its
/// editing feels that the only code that could be returned as an error is
/// &quot;Error Code 03&quot; which states that the SSN passed to FPLS is out of
/// range for currently allocated SSA numbers. Kessep was not able to edit for
/// SSN as most SSN's are allocated to States for future use and KESSEP would
/// never receive notification of any changes.
/// </para>
/// </summary>
[Serializable]
public partial class OeEabReceiveFplsErrors: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_EAB_RECEIVE_FPLS_ERRORS program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeEabReceiveFplsErrors(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeEabReceiveFplsErrors.
  /// </summary>
  public OeEabReceiveFplsErrors(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    GetService<IEabStub>().Execute("SWEXEE06", context, import, export, 0);
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

    private External external;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of ExternalFplsRequest.
    /// </summary>
    [JsonPropertyName("externalFplsRequest")]
    [Member(Index = 1, Members = new[]
    {
      "ApCsePersonNumber",
      "FplsLocateRequestIdentifier",
      "TransactionError"
    })]
    public ExternalFplsRequest ExternalFplsRequest
    {
      get => externalFplsRequest ??= new();
      set => externalFplsRequest = value;
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

    private ExternalFplsRequest externalFplsRequest;
    private External external;
  }
#endregion
}
