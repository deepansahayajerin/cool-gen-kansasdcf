// Program: OE_EAB_SEND_1099_REQUEST, ID: 371802993, model: 746.
// Short name: SWEXEE02
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Worksets;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: OE_EAB_SEND_1099_REQUEST.
/// </para>
/// <para>
/// Resp: OBLGMGT		
/// This procedure is designed to accept the commands Open, Write, and Close. 
/// Output from this External Action Block is a tape for 1099 request
/// transmission to OCSE.	
/// </para>
/// </summary>
[Serializable]
public partial class OeEabSend1099Request: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_EAB_SEND_1099_REQUEST program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeEabSend1099Request(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeEabSend1099Request.
  /// </summary>
  public OeEabSend1099Request(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    GetService<IEabStub>().Execute("SWEXEE02", context, import, export, 0);
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
    /// A value of ExternalOcse1099Request.
    /// </summary>
    [JsonPropertyName("externalOcse1099Request")]
    [Member(Index = 1, Members = new[]
    {
      "Ssn",
      "SubmittingState",
      "LocalFipsCode",
      "CaseIdNumber",
      "LastName",
      "FirstName",
      "CaseTypeAfdcNafdc",
      "CourtAdminOrderIndicator",
      "Blanks"
    })]
    public ExternalOcse1099Request ExternalOcse1099Request
    {
      get => externalOcse1099Request ??= new();
      set => externalOcse1099Request = value;
    }

    /// <summary>
    /// A value of External.
    /// </summary>
    [JsonPropertyName("external")]
    [Member(Index = 2, Members = new[] { "FileInstruction" })]
    public External External
    {
      get => external ??= new();
      set => external = value;
    }

    private ExternalOcse1099Request externalOcse1099Request;
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
    [Member(Index = 1, Members = new[]
    {
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
