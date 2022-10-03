// Program: OE_FPLS_EAB_SEND_FPLS_REQUEST, ID: 372364885, model: 746.
// Short name: SWEXEE04
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Worksets;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: OE_FPLS_EAB_SEND_FPLS_REQUEST.
/// </para>
/// <para>
/// Resp: OBLmgmt		
/// This procedure is designed to accept the commands Open, Write, and Close. 
/// Output from this External Action Block is a tape for FPLS_LOCATE_REQUEST's
/// to be sent to the Federal Person Locator Service(FPLS).
/// </para>
/// </summary>
[Serializable]
public partial class OeFplsEabSendFplsRequest: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_FPLS_EAB_SEND_FPLS_REQUEST program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeFplsEabSendFplsRequest(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeFplsEabSendFplsRequest.
  /// </summary>
  public OeFplsEabSendFplsRequest(IContext context, Import import, Export export)
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
    GetService<IEabStub>().Execute("SWEXEE04", context, import, export, 0);
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
    /// A value of ExternalFplsHeader.
    /// </summary>
    [JsonPropertyName("externalFplsHeader")]
    [Member(Index = 1, Members = new[]
    {
      "Spaces1",
      "StateCode",
      "StationNumber",
      "Spaces2",
      "FplsHeaderConstant",
      "TotalResponses",
      "FplsSortCode",
      "MmDateGenerated",
      "DdDateGenerated",
      "YyDateGenerated",
      "Spaces3",
      "Spaces4"
    })]
    public ExternalFplsHeader ExternalFplsHeader
    {
      get => externalFplsHeader ??= new();
      set => externalFplsHeader = value;
    }

    /// <summary>
    /// A value of ExternalFplsRequest.
    /// </summary>
    [JsonPropertyName("externalFplsRequest")]
    [Member(Index = 2, Members = new[]
    {
      "Spaces1",
      "StateAbbr",
      "StationNumber",
      "Spaces2",
      "TransactionType",
      "Ssn",
      "ApCsePersonNumber",
      "FplsLocateRequestIdentifier",
      "LocalCode",
      "UsersField",
      "TypeOfCase",
      "ApFirstName",
      "ApMiddleName",
      "ApFirstLastName",
      "ApSecondLastName",
      "ApThirdLastName",
      "ApDateOfBirth",
      "ApDateOfBirthMonth",
      "ApDateOfBirthDay",
      "ApDateOfBirthYear",
      "Sex",
      "CollectAllResponsesTogether",
      "Spaces3",
      "SendRequestTo",
      "TransactionError",
      "Spaces4",
      "ApCityOfBirth",
      "ApStateOrCountryOfBirth",
      "ApsFathersFirstName",
      "ApsFathersMi",
      "ApsFathersLastName",
      "ApsMothersFirstName",
      "ApsMothersMi",
      "ApsMothersMaidenName",
      "CpSsn"
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
    [Member(Index = 3, Members = new[] { "FileInstruction" })]
    public External External
    {
      get => external ??= new();
      set => external = value;
    }

    private ExternalFplsHeader externalFplsHeader;
    private ExternalFplsRequest externalFplsRequest;
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
      "TextLine80",
      "FileInstruction"
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
