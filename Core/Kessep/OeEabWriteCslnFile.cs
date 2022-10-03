// Program: OE_EAB_WRITE_CSLN_FILE, ID: 945243124, model: 746.
// Short name: SWEXEW17
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: OE_EAB_WRITE_CSLN_FILE.
/// </para>
/// <para>
/// This EAB write FIDM data to output file.
/// </para>
/// </summary>
[Serializable]
public partial class OeEabWriteCslnFile: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_EAB_WRITE_CSLN_FILE program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeEabWriteCslnFile(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeEabWriteCslnFile.
  /// </summary>
  public OeEabWriteCslnFile(IContext context, Import import, Export export):
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
      "SWEXEW17", context, import, export, EabOptions.Hpvp);
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
    /// A value of Cp.
    /// </summary>
    [JsonPropertyName("cp")]
    [Member(Index = 1, AccessFields = false, Members = new[]
    {
      "FirstName",
      "MiddleInitial",
      "LastName"
    })]
    public CsePersonsWorkSet Cp
    {
      get => cp ??= new();
      set => cp = value;
    }

    /// <summary>
    /// A value of NcpRecord.
    /// </summary>
    [JsonPropertyName("ncpRecord")]
    [Member(Index = 2, AccessFields = false, Members = new[]
    {
      "Number",
      "Dob",
      "Ssn",
      "FirstName",
      "MiddleInitial",
      "LastName"
    })]
    public CsePersonsWorkSet NcpRecord
    {
      get => ncpRecord ??= new();
      set => ncpRecord = value;
    }

    /// <summary>
    /// A value of ChildRecord.
    /// </summary>
    [JsonPropertyName("childRecord")]
    [Member(Index = 3, AccessFields = false, Members = new[]
    {
      "Dob",
      "FirstName",
      "MiddleInitial",
      "LastName"
    })]
    public CsePersonsWorkSet ChildRecord
    {
      get => childRecord ??= new();
      set => childRecord = value;
    }

    /// <summary>
    /// A value of Ncp.
    /// </summary>
    [JsonPropertyName("ncp")]
    [Member(Index = 4, AccessFields = false, Members = new[]
    {
      "LocationType",
      "LastUpdatedTimestamp",
      "Street1",
      "Street2",
      "City",
      "State",
      "ZipCode"
    })]
    public CsePersonAddress Ncp
    {
      get => ncp ??= new();
      set => ncp = value;
    }

    /// <summary>
    /// A value of StandardCourtOrderNumb.
    /// </summary>
    [JsonPropertyName("standardCourtOrderNumb")]
    [Member(Index = 5, AccessFields = false, Members = new[] { "Text20" })]
    public WorkArea StandardCourtOrderNumb
    {
      get => standardCourtOrderNumb ??= new();
      set => standardCourtOrderNumb = value;
    }

    /// <summary>
    /// A value of CourtOrderState.
    /// </summary>
    [JsonPropertyName("courtOrderState")]
    [Member(Index = 6, AccessFields = false, Members = new[] { "Text2" })]
    public WorkArea CourtOrderState
    {
      get => courtOrderState ??= new();
      set => courtOrderState = value;
    }

    /// <summary>
    /// A value of CourtOrderCity.
    /// </summary>
    [JsonPropertyName("courtOrderCity")]
    [Member(Index = 7, AccessFields = false, Members = new[] { "Text15" })]
    public WorkArea CourtOrderCity
    {
      get => courtOrderCity ??= new();
      set => courtOrderCity = value;
    }

    /// <summary>
    /// A value of NcpModifier.
    /// </summary>
    [JsonPropertyName("ncpModifier")]
    [Member(Index = 8, AccessFields = false, Members = new[] { "Text3" })]
    public WorkArea NcpModifier
    {
      get => ncpModifier ??= new();
      set => ncpModifier = value;
    }

    /// <summary>
    /// A value of ArrsBalanceAsOfDate.
    /// </summary>
    [JsonPropertyName("arrsBalanceAsOfDate")]
    [Member(Index = 9, AccessFields = false, Members = new[] { "Date" })]
    public DateWorkArea ArrsBalanceAsOfDate
    {
      get => arrsBalanceAsOfDate ??= new();
      set => arrsBalanceAsOfDate = value;
    }

    /// <summary>
    /// A value of NcpRecordArrsBalance.
    /// </summary>
    [JsonPropertyName("ncpRecordArrsBalance")]
    [Member(Index = 10, AccessFields = false, Members = new[] { "Text14" })]
    public WorkArea NcpRecordArrsBalance
    {
      get => ncpRecordArrsBalance ??= new();
      set => ncpRecordArrsBalance = value;
    }

    /// <summary>
    /// A value of OrganizationName.
    /// </summary>
    [JsonPropertyName("organizationName")]
    [Member(Index = 11, AccessFields = false, Members = new[] { "Text33" })]
    public WorkArea OrganizationName
    {
      get => organizationName ??= new();
      set => organizationName = value;
    }

    /// <summary>
    /// A value of CaseNumber.
    /// </summary>
    [JsonPropertyName("caseNumber")]
    [Member(Index = 12, AccessFields = false, Members = new[] { "Text10" })]
    public WorkArea CaseNumber
    {
      get => caseNumber ??= new();
      set => caseNumber = value;
    }

    /// <summary>
    /// A value of CountyFipsName.
    /// </summary>
    [JsonPropertyName("countyFipsName")]
    [Member(Index = 13, AccessFields = false, Members = new[] { "Text20" })]
    public WorkArea CountyFipsName
    {
      get => countyFipsName ??= new();
      set => countyFipsName = value;
    }

    /// <summary>
    /// A value of CurrSuppWithholdingAmt.
    /// </summary>
    [JsonPropertyName("currSuppWithholdingAmt")]
    [Member(Index = 14, AccessFields = false, Members = new[] { "Text8" })]
    public WorkArea CurrSuppWithholdingAmt
    {
      get => currSuppWithholdingAmt ??= new();
      set => currSuppWithholdingAmt = value;
    }

    /// <summary>
    /// A value of ArrsSupportWithAmt.
    /// </summary>
    [JsonPropertyName("arrsSupportWithAmt")]
    [Member(Index = 15, AccessFields = false, Members = new[] { "Text8" })]
    public WorkArea ArrsSupportWithAmt
    {
      get => arrsSupportWithAmt ??= new();
      set => arrsSupportWithAmt = value;
    }

    /// <summary>
    /// A value of LegalCapationLine13.
    /// </summary>
    [JsonPropertyName("legalCapationLine13")]
    [Member(Index = 16, AccessFields = false, Members = new[] { "Text40" })]
    public WorkArea LegalCapationLine13
    {
      get => legalCapationLine13 ??= new();
      set => legalCapationLine13 = value;
    }

    /// <summary>
    /// A value of LegalCaptionLine7.
    /// </summary>
    [JsonPropertyName("legalCaptionLine7")]
    [Member(Index = 17, AccessFields = false, Members = new[] { "Text40" })]
    public WorkArea LegalCaptionLine7
    {
      get => legalCaptionLine7 ??= new();
      set => legalCaptionLine7 = value;
    }

    /// <summary>
    /// A value of LegalCaptionLine8.
    /// </summary>
    [JsonPropertyName("legalCaptionLine8")]
    [Member(Index = 18, AccessFields = false, Members = new[] { "Text40" })]
    public WorkArea LegalCaptionLine8
    {
      get => legalCaptionLine8 ??= new();
      set => legalCaptionLine8 = value;
    }

    /// <summary>
    /// A value of LegalCaptionLine9.
    /// </summary>
    [JsonPropertyName("legalCaptionLine9")]
    [Member(Index = 19, AccessFields = false, Members = new[] { "Text40" })]
    public WorkArea LegalCaptionLine9
    {
      get => legalCaptionLine9 ??= new();
      set => legalCaptionLine9 = value;
    }

    /// <summary>
    /// A value of LegalCaptionLine10.
    /// </summary>
    [JsonPropertyName("legalCaptionLine10")]
    [Member(Index = 20, AccessFields = false, Members = new[] { "Text40" })]
    public WorkArea LegalCaptionLine10
    {
      get => legalCaptionLine10 ??= new();
      set => legalCaptionLine10 = value;
    }

    /// <summary>
    /// A value of LegalCaptionLine11.
    /// </summary>
    [JsonPropertyName("legalCaptionLine11")]
    [Member(Index = 21, AccessFields = false, Members = new[] { "Text40" })]
    public WorkArea LegalCaptionLine11
    {
      get => legalCaptionLine11 ??= new();
      set => legalCaptionLine11 = value;
    }

    /// <summary>
    /// A value of LegalCaptionLine12.
    /// </summary>
    [JsonPropertyName("legalCaptionLine12")]
    [Member(Index = 22, AccessFields = false, Members = new[] { "Text40" })]
    public WorkArea LegalCaptionLine12
    {
      get => legalCaptionLine12 ??= new();
      set => legalCaptionLine12 = value;
    }

    /// <summary>
    /// A value of LegalCaptionLine4.
    /// </summary>
    [JsonPropertyName("legalCaptionLine4")]
    [Member(Index = 23, AccessFields = false, Members = new[] { "Text40" })]
    public WorkArea LegalCaptionLine4
    {
      get => legalCaptionLine4 ??= new();
      set => legalCaptionLine4 = value;
    }

    /// <summary>
    /// A value of LegalCaptionLine5.
    /// </summary>
    [JsonPropertyName("legalCaptionLine5")]
    [Member(Index = 24, AccessFields = false, Members = new[] { "Text40" })]
    public WorkArea LegalCaptionLine5
    {
      get => legalCaptionLine5 ??= new();
      set => legalCaptionLine5 = value;
    }

    /// <summary>
    /// A value of LegalCaptionLine6.
    /// </summary>
    [JsonPropertyName("legalCaptionLine6")]
    [Member(Index = 25, AccessFields = false, Members = new[] { "Text40" })]
    public WorkArea LegalCaptionLine6
    {
      get => legalCaptionLine6 ??= new();
      set => legalCaptionLine6 = value;
    }

    /// <summary>
    /// A value of LegalCaptionLine3.
    /// </summary>
    [JsonPropertyName("legalCaptionLine3")]
    [Member(Index = 26, AccessFields = false, Members = new[] { "Text40" })]
    public WorkArea LegalCaptionLine3
    {
      get => legalCaptionLine3 ??= new();
      set => legalCaptionLine3 = value;
    }

    /// <summary>
    /// A value of LegalCaptionLine2.
    /// </summary>
    [JsonPropertyName("legalCaptionLine2")]
    [Member(Index = 27, AccessFields = false, Members = new[] { "Text40" })]
    public WorkArea LegalCaptionLine2
    {
      get => legalCaptionLine2 ??= new();
      set => legalCaptionLine2 = value;
    }

    /// <summary>
    /// A value of LegalCaptionLine1.
    /// </summary>
    [JsonPropertyName("legalCaptionLine1")]
    [Member(Index = 28, AccessFields = false, Members = new[] { "Text40" })]
    public WorkArea LegalCaptionLine1
    {
      get => legalCaptionLine1 ??= new();
      set => legalCaptionLine1 = value;
    }

    /// <summary>
    /// A value of ArrsByStandardNumber.
    /// </summary>
    [JsonPropertyName("arrsByStandardNumber")]
    [Member(Index = 29, AccessFields = false, Members = new[] { "Text12" })]
    public TextWorkArea ArrsByStandardNumber
    {
      get => arrsByStandardNumber ??= new();
      set => arrsByStandardNumber = value;
    }

    /// <summary>
    /// A value of LocalPrepByAttyFirstName.
    /// </summary>
    [JsonPropertyName("localPrepByAttyFirstName")]
    [Member(Index = 30, AccessFields = false, Members = new[] { "Text12" })]
    public WorkArea LocalPrepByAttyFirstName
    {
      get => localPrepByAttyFirstName ??= new();
      set => localPrepByAttyFirstName = value;
    }

    /// <summary>
    /// A value of PrepByAttyMiddleIniti.
    /// </summary>
    [JsonPropertyName("prepByAttyMiddleIniti")]
    [Member(Index = 31, AccessFields = false, Members = new[] { "Text1" })]
    public WorkArea PrepByAttyMiddleIniti
    {
      get => prepByAttyMiddleIniti ??= new();
      set => prepByAttyMiddleIniti = value;
    }

    /// <summary>
    /// A value of PrepByAttyLastName.
    /// </summary>
    [JsonPropertyName("prepByAttyLastName")]
    [Member(Index = 32, AccessFields = false, Members = new[] { "Text17" })]
    public WorkArea PrepByAttyLastName
    {
      get => prepByAttyLastName ??= new();
      set => prepByAttyLastName = value;
    }

    /// <summary>
    /// A value of PrepByAttyCertificate.
    /// </summary>
    [JsonPropertyName("prepByAttyCertificate")]
    [Member(Index = 33, AccessFields = false, Members = new[] { "Text10" })]
    public WorkArea PrepByAttyCertificate
    {
      get => prepByAttyCertificate ??= new();
      set => prepByAttyCertificate = value;
    }

    /// <summary>
    /// A value of PrepByAttyAddress1.
    /// </summary>
    [JsonPropertyName("prepByAttyAddress1")]
    [Member(Index = 34, AccessFields = false, Members = new[] { "Text60" })]
    public WorkArea PrepByAttyAddress1
    {
      get => prepByAttyAddress1 ??= new();
      set => prepByAttyAddress1 = value;
    }

    /// <summary>
    /// A value of PrepByAttyAddress2.
    /// </summary>
    [JsonPropertyName("prepByAttyAddress2")]
    [Member(Index = 35, AccessFields = false, Members = new[] { "Text25" })]
    public WorkArea PrepByAttyAddress2
    {
      get => prepByAttyAddress2 ??= new();
      set => prepByAttyAddress2 = value;
    }

    /// <summary>
    /// A value of PrepByAttyCity.
    /// </summary>
    [JsonPropertyName("prepByAttyCity")]
    [Member(Index = 36, AccessFields = false, Members = new[] { "Text15" })]
    public WorkArea PrepByAttyCity
    {
      get => prepByAttyCity ??= new();
      set => prepByAttyCity = value;
    }

    /// <summary>
    /// A value of PrepByAttyState.
    /// </summary>
    [JsonPropertyName("prepByAttyState")]
    [Member(Index = 37, AccessFields = false, Members = new[] { "Text2" })]
    public WorkArea PrepByAttyState
    {
      get => prepByAttyState ??= new();
      set => prepByAttyState = value;
    }

    /// <summary>
    /// A value of PrepByAttyPhone.
    /// </summary>
    [JsonPropertyName("prepByAttyPhone")]
    [Member(Index = 38, AccessFields = false, Members = new[] { "Text10" })]
    public WorkArea PrepByAttyPhone
    {
      get => prepByAttyPhone ??= new();
      set => prepByAttyPhone = value;
    }

    /// <summary>
    /// A value of PrepByAttyFax.
    /// </summary>
    [JsonPropertyName("prepByAttyFax")]
    [Member(Index = 39, AccessFields = false, Members = new[] { "Text10" })]
    public WorkArea PrepByAttyFax
    {
      get => prepByAttyFax ??= new();
      set => prepByAttyFax = value;
    }

    /// <summary>
    /// A value of PrepByAttyEmail.
    /// </summary>
    [JsonPropertyName("prepByAttyEmail")]
    [Member(Index = 40, AccessFields = false, Members = new[] { "Text50" })]
    public WorkArea PrepByAttyEmail
    {
      get => prepByAttyEmail ??= new();
      set => prepByAttyEmail = value;
    }

    /// <summary>
    /// A value of ArrsStdNmbrArrsAsOf.
    /// </summary>
    [JsonPropertyName("arrsStdNmbrArrsAsOf")]
    [Member(Index = 41, AccessFields = false, Members = new[] { "Date" })]
    public DateWorkArea ArrsStdNmbrArrsAsOf
    {
      get => arrsStdNmbrArrsAsOf ??= new();
      set => arrsStdNmbrArrsAsOf = value;
    }

    /// <summary>
    /// A value of NcpAttyLastName.
    /// </summary>
    [JsonPropertyName("ncpAttyLastName")]
    [Member(Index = 42, AccessFields = false, Members = new[] { "Text17" })]
    public WorkArea NcpAttyLastName
    {
      get => ncpAttyLastName ??= new();
      set => ncpAttyLastName = value;
    }

    /// <summary>
    /// A value of NcpAttyFirstName.
    /// </summary>
    [JsonPropertyName("ncpAttyFirstName")]
    [Member(Index = 43, AccessFields = false, Members = new[] { "Text12" })]
    public WorkArea NcpAttyFirstName
    {
      get => ncpAttyFirstName ??= new();
      set => ncpAttyFirstName = value;
    }

    /// <summary>
    /// A value of NcpAttyMiddleInitial.
    /// </summary>
    [JsonPropertyName("ncpAttyMiddleInitial")]
    [Member(Index = 44, AccessFields = false, Members = new[] { "Text1" })]
    public WorkArea NcpAttyMiddleInitial
    {
      get => ncpAttyMiddleInitial ??= new();
      set => ncpAttyMiddleInitial = value;
    }

    /// <summary>
    /// A value of ChildSuffix.
    /// </summary>
    [JsonPropertyName("childSuffix")]
    [Member(Index = 45, AccessFields = false, Members = new[] { "Text3" })]
    public WorkArea ChildSuffix
    {
      get => childSuffix ??= new();
      set => childSuffix = value;
    }

    /// <summary>
    /// A value of CslnTrailerNcpCount.
    /// </summary>
    [JsonPropertyName("cslnTrailerNcpCount")]
    [Member(Index = 46, AccessFields = false, Members = new[] { "Text6" })]
    public WorkArea CslnTrailerNcpCount
    {
      get => cslnTrailerNcpCount ??= new();
      set => cslnTrailerNcpCount = value;
    }

    /// <summary>
    /// A value of CslnTrailerRecordType.
    /// </summary>
    [JsonPropertyName("cslnTrailerRecordType")]
    [Member(Index = 47, AccessFields = false, Members = new[] { "Text2" })]
    public WorkArea CslnTrailerRecordType
    {
      get => cslnTrailerRecordType ??= new();
      set => cslnTrailerRecordType = value;
    }

    /// <summary>
    /// A value of CslnHdrFileName.
    /// </summary>
    [JsonPropertyName("cslnHdrFileName")]
    [Member(Index = 48, AccessFields = false, Members = new[] { "Text30" })]
    public TextWorkArea CslnHdrFileName
    {
      get => cslnHdrFileName ??= new();
      set => cslnHdrFileName = value;
    }

    /// <summary>
    /// A value of CslnHdrCreatedTmstmp.
    /// </summary>
    [JsonPropertyName("cslnHdrCreatedTmstmp")]
    [Member(Index = 49, AccessFields = false, Members = new[] { "Timestamp" })]
    public DateWorkArea CslnHdrCreatedTmstmp
    {
      get => cslnHdrCreatedTmstmp ??= new();
      set => cslnHdrCreatedTmstmp = value;
    }

    /// <summary>
    /// A value of CslnHdrRecordType.
    /// </summary>
    [JsonPropertyName("cslnHdrRecordType")]
    [Member(Index = 50, AccessFields = false, Members = new[] { "Text2" })]
    public WorkArea CslnHdrRecordType
    {
      get => cslnHdrRecordType ??= new();
      set => cslnHdrRecordType = value;
    }

    /// <summary>
    /// A value of External.
    /// </summary>
    [JsonPropertyName("external")]
    [Member(Index = 51, AccessFields = false, Members
      = new[] { "FileInstruction" })]
    public External External
    {
      get => external ??= new();
      set => external = value;
    }

    /// <summary>
    /// A value of RecordType.
    /// </summary>
    [JsonPropertyName("recordType")]
    [Member(Index = 52, AccessFields = false, Members = new[] { "Text2" })]
    public WorkArea RecordType
    {
      get => recordType ??= new();
      set => recordType = value;
    }

    /// <summary>
    /// A value of NcpAddressVerified.
    /// </summary>
    [JsonPropertyName("ncpAddressVerified")]
    [Member(Index = 53, AccessFields = false, Members = new[] { "Flag" })]
    public Common NcpAddressVerified
    {
      get => ncpAddressVerified ??= new();
      set => ncpAddressVerified = value;
    }

    /// <summary>
    /// A value of CourtOrderCount.
    /// </summary>
    [JsonPropertyName("courtOrderCount")]
    [Member(Index = 54, AccessFields = false, Members = new[] { "Count" })]
    public Common CourtOrderCount
    {
      get => courtOrderCount ??= new();
      set => courtOrderCount = value;
    }

    /// <summary>
    /// A value of ProcessDate.
    /// </summary>
    [JsonPropertyName("processDate")]
    [Member(Index = 55, AccessFields = false, Members = new[] { "Date" })]
    public DateWorkArea ProcessDate
    {
      get => processDate ??= new();
      set => processDate = value;
    }

    /// <summary>
    /// A value of NcpAddressRecordDelete.
    /// </summary>
    [JsonPropertyName("ncpAddressRecordDelete")]
    [Member(Index = 56, AccessFields = false, Members = new[]
    {
      "Street",
      "Street2",
      "City",
      "State",
      "PostalCode"
    })]
    public AddressRecord NcpAddressRecordDelete
    {
      get => ncpAddressRecordDelete ??= new();
      set => ncpAddressRecordDelete = value;
    }

    /// <summary>
    /// A value of PrepByAttyZip.
    /// </summary>
    [JsonPropertyName("prepByAttyZip")]
    [Member(Index = 57, AccessFields = false, Members = new[] { "Text9" })]
    public WorkArea PrepByAttyZip
    {
      get => prepByAttyZip ??= new();
      set => prepByAttyZip = value;
    }

    /// <summary>
    /// A value of PrivateAttorneyAddress.
    /// </summary>
    [JsonPropertyName("privateAttorneyAddress")]
    [Member(Index = 58, AccessFields = false, Members = new[]
    {
      "Street1",
      "Street2",
      "City",
      "State",
      "ZipCode5",
      "ZipCode4"
    })]
    public PrivateAttorneyAddress PrivateAttorneyAddress
    {
      get => privateAttorneyAddress ??= new();
      set => privateAttorneyAddress = value;
    }

    private CsePersonsWorkSet cp;
    private CsePersonsWorkSet ncpRecord;
    private CsePersonsWorkSet childRecord;
    private CsePersonAddress ncp;
    private WorkArea standardCourtOrderNumb;
    private WorkArea courtOrderState;
    private WorkArea courtOrderCity;
    private WorkArea ncpModifier;
    private DateWorkArea arrsBalanceAsOfDate;
    private WorkArea ncpRecordArrsBalance;
    private WorkArea organizationName;
    private WorkArea caseNumber;
    private WorkArea countyFipsName;
    private WorkArea currSuppWithholdingAmt;
    private WorkArea arrsSupportWithAmt;
    private WorkArea legalCapationLine13;
    private WorkArea legalCaptionLine7;
    private WorkArea legalCaptionLine8;
    private WorkArea legalCaptionLine9;
    private WorkArea legalCaptionLine10;
    private WorkArea legalCaptionLine11;
    private WorkArea legalCaptionLine12;
    private WorkArea legalCaptionLine4;
    private WorkArea legalCaptionLine5;
    private WorkArea legalCaptionLine6;
    private WorkArea legalCaptionLine3;
    private WorkArea legalCaptionLine2;
    private WorkArea legalCaptionLine1;
    private TextWorkArea arrsByStandardNumber;
    private WorkArea localPrepByAttyFirstName;
    private WorkArea prepByAttyMiddleIniti;
    private WorkArea prepByAttyLastName;
    private WorkArea prepByAttyCertificate;
    private WorkArea prepByAttyAddress1;
    private WorkArea prepByAttyAddress2;
    private WorkArea prepByAttyCity;
    private WorkArea prepByAttyState;
    private WorkArea prepByAttyPhone;
    private WorkArea prepByAttyFax;
    private WorkArea prepByAttyEmail;
    private DateWorkArea arrsStdNmbrArrsAsOf;
    private WorkArea ncpAttyLastName;
    private WorkArea ncpAttyFirstName;
    private WorkArea ncpAttyMiddleInitial;
    private WorkArea childSuffix;
    private WorkArea cslnTrailerNcpCount;
    private WorkArea cslnTrailerRecordType;
    private TextWorkArea cslnHdrFileName;
    private DateWorkArea cslnHdrCreatedTmstmp;
    private WorkArea cslnHdrRecordType;
    private External external;
    private WorkArea recordType;
    private Common ncpAddressVerified;
    private Common courtOrderCount;
    private DateWorkArea processDate;
    private AddressRecord ncpAddressRecordDelete;
    private WorkArea prepByAttyZip;
    private PrivateAttorneyAddress privateAttorneyAddress;
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
