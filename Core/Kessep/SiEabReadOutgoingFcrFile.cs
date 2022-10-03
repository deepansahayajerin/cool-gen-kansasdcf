// Program: SI_EAB_READ_OUTGOING_FCR_FILE, ID: 371419348, model: 746.
// Short name: SWEXER14
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Worksets;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: SI_EAB_READ_OUTGOING_FCR_FILE.
/// </para>
/// <para>
/// Resp:OBLGEST
/// This EXTERNAL procedure carries view that will be used for receiving the 
/// FPLS responses from the Federal Parent Locator Service in reaction to
/// FPLS_LOCATE_REQUEST's.
/// </para>
/// </summary>
[Serializable]
public partial class SiEabReadOutgoingFcrFile: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_EAB_READ_OUTGOING_FCR_FILE program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiEabReadOutgoingFcrFile(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiEabReadOutgoingFcrFile.
  /// </summary>
  public SiEabReadOutgoingFcrFile(IContext context, Import import, Export export)
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
      "SWEXER14", context, import, export, EabOptions.Hpvp);
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
    /// A value of TrailerRecord.
    /// </summary>
    [JsonPropertyName("trailerRecord")]
    [Member(Index = 1, AccessFields = false, Members
      = new[] { "RecordCount", "RecordIdentifier" })]
    public FcrRecord TrailerRecord
    {
      get => trailerRecord ??= new();
      set => trailerRecord = value;
    }

    /// <summary>
    /// A value of QueryRecord.
    /// </summary>
    [JsonPropertyName("queryRecord")]
    [Member(Index = 2, AccessFields = false, Members = new[]
    {
      "Ssn",
      "MemberId",
      "UserField",
      "FipsCountyCode",
      "CaseId",
      "ActionTypeCode",
      "RecordIdentifier"
    })]
    public FcrRecord QueryRecord
    {
      get => queryRecord ??= new();
      set => queryRecord = value;
    }

    /// <summary>
    /// A value of PersonRecord.
    /// </summary>
    [JsonPropertyName("personRecord")]
    [Member(Index = 3, AccessFields = false, Members = new[]
    {
      "LocateSource8",
      "LocateSource7",
      "LocateSource6",
      "LocateSource5",
      "LocateSource4",
      "LocateSource3",
      "LocateSource2",
      "LocateSource1",
      "Irs1099",
      "NewMemberId",
      "AdditionalLastName4",
      "AdditionalMiddleName4",
      "AdditionalFirstName4",
      "AdditionalLastName3",
      "AdditionalMiddleName3",
      "AdditionalFirstName3",
      "AdditionalLastName2",
      "AdditionalMidleName2",
      "AdditionalFirstName2",
      "AdditionalLastName1",
      "AdditionalMiddleName1",
      "AdditionalFirstName1",
      "AdditionalSsn2",
      "AdditionalSsn1",
      "IrsUSsn",
      "MotherMaidenName",
      "MotherMiddleInitial",
      "MotherFirstName",
      "FatherLastName",
      "FatherMiddleInitial",
      "FathersFirstName",
      "StateOfBirth",
      "CityOfBirth",
      "LastName",
      "MiddleName",
      "FirstName",
      "PreviousSsn",
      "Ssn",
      "DateOfBirth",
      "SexCode",
      "MemberId",
      "FamilyViolence",
      "ParticipantType",
      "LocateRequestType",
      "UserField",
      "FipsCountyCode",
      "CaseId",
      "ActionTypeCode",
      "RecordIdentifier",
      "BundleFplsLocateResults"
    })]
    public FcrRecord PersonRecord
    {
      get => personRecord ??= new();
      set => personRecord = value;
    }

    /// <summary>
    /// A value of CaseRecord.
    /// </summary>
    [JsonPropertyName("caseRecord")]
    [Member(Index = 4, AccessFields = false, Members = new[]
    {
      "PreviousCaseId",
      "UserField",
      "FipsCountyCode",
      "OrderIndicator",
      "CaseType",
      "CaseId",
      "ActionTypeCode",
      "RecordIdentifier"
    })]
    public FcrRecord CaseRecord
    {
      get => caseRecord ??= new();
      set => caseRecord = value;
    }

    /// <summary>
    /// A value of Header.
    /// </summary>
    [JsonPropertyName("header")]
    [Member(Index = 5, AccessFields = false, Members = new[]
    {
      "DateStamp",
      "BatchNumber",
      "VersionControlNumber",
      "StateCode",
      "RecordIdentifier"
    })]
    public FcrRecord Header
    {
      get => header ??= new();
      set => header = value;
    }

    /// <summary>
    /// A value of External.
    /// </summary>
    [JsonPropertyName("external")]
    [Member(Index = 6, Members = new[]
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

    private FcrRecord trailerRecord;
    private FcrRecord queryRecord;
    private FcrRecord personRecord;
    private FcrRecord caseRecord;
    private FcrRecord header;
    private External external;
  }
#endregion
}
