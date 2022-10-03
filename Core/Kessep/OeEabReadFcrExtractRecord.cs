// Program: OE_EAB_READ_FCR_EXTRACT_RECORD, ID: 374569359, model: 746.
// Short name: SWEXER16
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Worksets;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: OE_EAB_READ_FCR_EXTRACT_RECORD.
/// </summary>
[Serializable]
public partial class OeEabReadFcrExtractRecord: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_EAB_READ_FCR_EXTRACT_RECORD program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeEabReadFcrExtractRecord(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeEabReadFcrExtractRecord.
  /// </summary>
  public OeEabReadFcrExtractRecord(IContext context, Import import,
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
      "SWEXER16", context, import, export, EabOptions.Hpvp);
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
    /// A value of ExternalFileStatus.
    /// </summary>
    [JsonPropertyName("externalFileStatus")]
    [Member(Index = 1, AccessFields = false, Members = new[]
    {
      "FileInstruction",
      "NumericReturnCode",
      "TextReturnCode",
      "TextLine130",
      "TextLine80",
      "TextLine8"
    })]
    public External ExternalFileStatus
    {
      get => externalFileStatus ??= new();
      set => externalFileStatus = value;
    }

    private External externalFileStatus;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of ExternalFileStatus.
    /// </summary>
    [JsonPropertyName("externalFileStatus")]
    [Member(Index = 1, AccessFields = false, Members = new[]
    {
      "FileInstruction",
      "NumericReturnCode",
      "TextReturnCode",
      "TextLine130",
      "TextLine80",
      "TextLine8"
    })]
    public External ExternalFileStatus
    {
      get => externalFileStatus ??= new();
      set => externalFileStatus = value;
    }

    /// <summary>
    /// A value of Ext.
    /// </summary>
    [JsonPropertyName("ext")]
    [Member(Index = 2, AccessFields = false, Members = new[]
    {
      "RecordId",
      "ActionTypeCode",
      "CaseId",
      "CaseType",
      "OrderIndicator",
      "FipsCountyCode",
      "Blank1",
      "UserField",
      "PreviousCaseId"
    })]
    public FcrOutputCaseRecord Ext
    {
      get => ext ??= new();
      set => ext = value;
    }

    /// <summary>
    /// A value of FcrOutputExt.
    /// </summary>
    [JsonPropertyName("fcrOutputExt")]
    [Member(Index = 3, AccessFields = false, Members = new[]
    {
      "RecordId",
      "ActionTypeCode",
      "CaseId",
      "FcrProcessingFld1",
      "UserField",
      "FipsCountyCode",
      "FcrProcessingFld2",
      "LocateReqType",
      "BundleResults",
      "ParticipantType",
      "FamilyViolence",
      "MemberId",
      "SexCode",
      "DateOfBirth",
      "Ssn",
      "PreviousSsn",
      "FirstName",
      "MiddleName",
      "LastName",
      "CityOfBirth",
      "StateCntyBirth",
      "FathersFirstName",
      "FathersMidName",
      "FathersLastName",
      "MothersFirstName",
      "MothersMidName",
      "MothersLastName",
      "IrsUSsn",
      "AdditionalSsn1",
      "AdditionalSsn2",
      "AdditionalFnam1",
      "AdditionalMnam1",
      "AdditionalLnam1",
      "AdditionalFnam2",
      "AdditionalMnam2",
      "AdditionalLnam2",
      "AdditionalFnam3",
      "AdditionalMnam3",
      "AdditionalLnam3",
      "AdditionalFnam4",
      "AdditionalMnam4",
      "AdditionalLnam4",
      "NewMemberId",
      "Irs1099",
      "LocateSource1",
      "LocateSource2",
      "LocateSource3",
      "LocateSource4",
      "LocateSource5",
      "LocateSource6",
      "LocateSource7",
      "LocateSource8"
    })]
    public FcrOutputMemberRecord FcrOutputExt
    {
      get => fcrOutputExt ??= new();
      set => fcrOutputExt = value;
    }

    private External externalFileStatus;
    private FcrOutputCaseRecord ext;
    private FcrOutputMemberRecord fcrOutputExt;
  }
#endregion
}
