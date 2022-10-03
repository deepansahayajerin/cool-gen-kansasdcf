// Program: OE_EAB_READ_FCR_MATCH_RESPONSES, ID: 373539274, model: 746.
// Short name: SWEXER04
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: OE_EAB_READ_FCR_MATCH_RESPONSES.
/// </para>
/// <para>
/// This EAB returns the PROACTIVE MATCH RESPONSE records one at a time.
/// </para>
/// </summary>
[Serializable]
public partial class OeEabReadFcrMatchResponses: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_EAB_READ_FCR_MATCH_RESPONSES program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeEabReadFcrMatchResponses(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeEabReadFcrMatchResponses.
  /// </summary>
  public OeEabReadFcrMatchResponses(IContext context, Import import,
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
      "SWEXER04", context, import, export, EabOptions.Hpvp);
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

    /// <summary>
    /// A value of Common.
    /// </summary>
    [JsonPropertyName("common")]
    [Member(Index = 2, AccessFields = false, Members = new[] { "Count" })]
    public Common Common
    {
      get => common ??= new();
      set => common = value;
    }

    private External external;
    private Common common;
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
      "TextLine80",
      "TextLine8"
    })]
    public External External
    {
      get => external ??= new();
      set => external = value;
    }

    /// <summary>
    /// A value of FcrProactiveMatchResponse.
    /// </summary>
    [JsonPropertyName("fcrProactiveMatchResponse")]
    [Member(Index = 2, AccessFields = false, Members = new[]
    {
      "ActionTypeCode",
      "TransmitterStateOrTerrCode",
      "UserField",
      "FipsCountyCode",
      "FirstName",
      "MiddleName",
      "SubmittedOrMatchedSsn",
      "StateMemberId",
      "SubmittedCaseId",
      "ResponseCode",
      "MatchedCaseId",
      "MatchedCaseType",
      "MatchFcrFipsCountyCd",
      "MatchedFcrCaseRegDate",
      "MatchedCaseOrderInd",
      "MatchedParticipantType",
      "MatchedMemberId",
      "MatchedPersonDod",
      "MatchedPersonAddFirstName1",
      "MatchedPersonAddMiddleName1",
      "MatchedPersonAddLastName1",
      "MatchedPersonAddFirstName2",
      "MatchedPersonAddMiddleName2",
      "MatchedPersonAddLastName2",
      "MatchedPersonAddFirstName3",
      "MatchedPersonAddMiddleName3",
      "MatchedPersonAddLastName3",
      "MatchedPersonAddFirstName4",
      "MatchedPersonAddMiddleName4",
      "MatchedPersonAddLastName4",
      "AssociatedSsn1",
      "AssociatedFirstName1",
      "AssociatedMiddleName1",
      "AssociatedLastName1",
      "AssociatedPersonSexCode1",
      "AssociatedParticipantType1",
      "AssociatedOthStTerrMembId1",
      "AssociatedDob1",
      "AssociatedDod1",
      "AssociatedSsn2",
      "AssociatedFirstName2",
      "AssociatedMiddleName2",
      "AssociatedLastName2",
      "AssociatedPersonSexCode2",
      "AssociatedParticipantType2",
      "AssociatedOthStTerrMembId2",
      "AssociatedDob2",
      "AssociatedDod2",
      "AssociatedSsn3",
      "AssociatedFirstName3",
      "AssociatedMiddleName3",
      "AssociatedLastName3",
      "AssociatedPersonSexCode3",
      "AssociatedParticipantType3",
      "AssociatedOthStTerrMembId3",
      "AssociatedDob3",
      "AssociatedDod3",
      "AssociatedStateMembId1",
      "AssociatedStateMembId2",
      "AssociatedStateMembId3",
      "Identifier",
      "DateReceived",
      "LastName"
    })]
    public FcrProactiveMatchResponse FcrProactiveMatchResponse
    {
      get => fcrProactiveMatchResponse ??= new();
      set => fcrProactiveMatchResponse = value;
    }

    /// <summary>
    /// A value of DodIndicator.
    /// </summary>
    [JsonPropertyName("dodIndicator")]
    [Member(Index = 3, AccessFields = false, Members = new[] { "Flag" })]
    public Common DodIndicator
    {
      get => dodIndicator ??= new();
      set => dodIndicator = value;
    }

    /// <summary>
    /// A value of LastResidence.
    /// </summary>
    [JsonPropertyName("lastResidence")]
    [Member(Index = 4, AccessFields = false, Members = new[]
    {
      "Text15",
      "Text2",
      "Text5"
    })]
    public WorkArea LastResidence
    {
      get => lastResidence ??= new();
      set => lastResidence = value;
    }

    /// <summary>
    /// A value of LumpSum.
    /// </summary>
    [JsonPropertyName("lumpSum")]
    [Member(Index = 5, AccessFields = false, Members = new[]
    {
      "Text15",
      "Text2",
      "Text5"
    })]
    public WorkArea LumpSum
    {
      get => lumpSum ??= new();
      set => lumpSum = value;
    }

    /// <summary>
    /// A value of PersonDeleteIndicator.
    /// </summary>
    [JsonPropertyName("personDeleteIndicator")]
    [Member(Index = 6, AccessFields = false, Members = new[] { "Flag" })]
    public Common PersonDeleteIndicator
    {
      get => personDeleteIndicator ??= new();
      set => personDeleteIndicator = value;
    }

    /// <summary>
    /// A value of PreviousCaseId.
    /// </summary>
    [JsonPropertyName("previousCaseId")]
    [Member(Index = 7, AccessFields = false, Members = new[] { "Text15" })]
    public WorkArea PreviousCaseId
    {
      get => previousCaseId ??= new();
      set => previousCaseId = value;
    }

    /// <summary>
    /// A value of CaseChangeType.
    /// </summary>
    [JsonPropertyName("caseChangeType")]
    [Member(Index = 8, AccessFields = false, Members = new[] { "Flag" })]
    public Common CaseChangeType
    {
      get => caseChangeType ??= new();
      set => caseChangeType = value;
    }

    private External external;
    private FcrProactiveMatchResponse fcrProactiveMatchResponse;
    private Common dodIndicator;
    private WorkArea lastResidence;
    private WorkArea lumpSum;
    private Common personDeleteIndicator;
    private WorkArea previousCaseId;
    private Common caseChangeType;
  }
#endregion
}
