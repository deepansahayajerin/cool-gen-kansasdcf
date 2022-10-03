// The source file: 1099_LOCATE_REQUEST, ID: 371440621, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Entities;

/// <summary>
/// RESP: OBLGESTB
/// This entity contains the requests to IRS 1099 interface.
/// </summary>
[Serializable]
public partial class Data1099LocateRequest: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public Data1099LocateRequest()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public Data1099LocateRequest(Data1099LocateRequest that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new Data1099LocateRequest Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(Data1099LocateRequest that)
  {
    base.Assign(that);
    firstName = that.firstName;
    identifier = that.identifier;
    ssn = that.ssn;
    localCode = that.localCode;
    lastName = that.lastName;
    afdcCode = that.afdcCode;
    caseIdNo = that.caseIdNo;
    courtOrAdminOrdInd = that.courtOrAdminOrdInd;
    noMatchCode = that.noMatchCode;
    createdBy = that.createdBy;
    createdTimestamp = that.createdTimestamp;
    lastUpdatedBy = that.lastUpdatedBy;
    lastUpdatedTimestamp = that.lastUpdatedTimestamp;
    requestSentDate = that.requestSentDate;
    middleInitial = that.middleInitial;
    cspNumber = that.cspNumber;
  }

  /// <summary>Length of the FIRST NAME attribute.</summary>
  public const int FirstName_MaxLength = 15;

  /// <summary>
  /// The value of the FIRST NAME attribute.
  /// This is an input to IRS. It specifies the first name of the AP as sent by 
  /// the state.
  /// </summary>
  [JsonPropertyName("firstName")]
  [Member(Index = 1, Type = MemberType.Char, Length = FirstName_MaxLength, Optional
    = true)]
  public string FirstName
  {
    get => firstName;
    set => firstName = value != null
      ? TrimEnd(Substring(value, 1, FirstName_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the IDENTIFIER attribute.
  /// The attribute, together with the relation to CSE_PERSON, uniquely 
  /// identifies one instance of 1099_TRANSACTION.
  /// </summary>
  [JsonPropertyName("identifier")]
  [DefaultValue(0)]
  [Member(Index = 2, Type = MemberType.Number, Length = 3)]
  public int Identifier
  {
    get => identifier;
    set => identifier = value;
  }

  /// <summary>Length of the SSN attribute.</summary>
  public const int Ssn_MaxLength = 9;

  /// <summary>
  /// The value of the SSN attribute.
  /// Input to IRS.
  /// Mandatory information.
  /// Must be within range, must be 9 position numeric codes where the first 
  /// three positions meet the following conditions:
  ///  - not equal to '000'
  ///  - not falling within range '626' - '700'
  ///  - not greater than '728'
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 3, Type = MemberType.Char, Length = Ssn_MaxLength)]
  public string Ssn
  {
    get => ssn ?? "";
    set => ssn = TrimEnd(Substring(value, 1, Ssn_MaxLength));
  }

  /// <summary>
  /// The json value of the Ssn attribute.</summary>
  [JsonPropertyName("ssn")]
  [Computed]
  public string Ssn_Json
  {
    get => NullIf(Ssn, "");
    set => Ssn = value;
  }

  /// <summary>Length of the LOCAL_CODE attribute.</summary>
  public const int LocalCode_MaxLength = 3;

  /// <summary>
  /// The value of the LOCAL_CODE attribute.
  /// Input to IRS.
  /// Numeric FIPS code of the initiating State.
  /// It can be blank.
  /// </summary>
  [JsonPropertyName("localCode")]
  [Member(Index = 4, Type = MemberType.Char, Length = LocalCode_MaxLength, Optional
    = true)]
  public string LocalCode
  {
    get => localCode;
    set => localCode = value != null
      ? TrimEnd(Substring(value, 1, LocalCode_MaxLength)) : null;
  }

  /// <summary>Length of the LAST_NAME attribute.</summary>
  public const int LastName_MaxLength = 20;

  /// <summary>
  /// The value of the LAST_NAME attribute.
  /// This is an input to IRS. It specifies the last name of the AP as sent by 
  /// the state.
  /// </summary>
  [JsonPropertyName("lastName")]
  [Member(Index = 5, Type = MemberType.Char, Length = LastName_MaxLength, Optional
    = true)]
  public string LastName
  {
    get => lastName;
    set => lastName = value != null
      ? TrimEnd(Substring(value, 1, LastName_MaxLength)) : null;
  }

  /// <summary>Length of the AFDC_CODE attribute.</summary>
  public const int AfdcCode_MaxLength = 1;

  /// <summary>
  /// The value of the AFDC_CODE attribute.
  /// Input to IRS.
  /// Specifies whether the case is AFDC or Non AFDC.
  /// Must be blank or &quot;A&quot; or &quot;N&quot;
  /// A - AFDC
  /// N - Non-AFDC
  /// </summary>
  [JsonPropertyName("afdcCode")]
  [Member(Index = 6, Type = MemberType.Char, Length = AfdcCode_MaxLength, Optional
    = true)]
  public string AfdcCode
  {
    get => afdcCode;
    set => afdcCode = value != null
      ? TrimEnd(Substring(value, 1, AfdcCode_MaxLength)) : null;
  }

  /// <summary>Length of the CASE_ID_NO attribute.</summary>
  public const int CaseIdNo_MaxLength = 15;

  /// <summary>
  /// The value of the CASE_ID_NO attribute.
  /// Input to IRS.
  /// Attribute to match responses to requests.
  /// Suggestion: Move primary identifier of 1099_TRANSACTION (CSE Person 
  /// number, IDENTIFIER of 1099_TRANSACTION).
  /// </summary>
  [JsonPropertyName("caseIdNo")]
  [Member(Index = 7, Type = MemberType.Char, Length = CaseIdNo_MaxLength, Optional
    = true)]
  public string CaseIdNo
  {
    get => caseIdNo;
    set => caseIdNo = value != null
      ? TrimEnd(Substring(value, 1, CaseIdNo_MaxLength)) : null;
  }

  /// <summary>Length of the COURT_OR_ADMIN_ORD_IND attribute.</summary>
  public const int CourtOrAdminOrdInd_MaxLength = 1;

  /// <summary>
  /// The value of the COURT_OR_ADMIN_ORD_IND attribute.
  /// Input to IRS.
  /// Indicates whether or not a court order or administrative order exists.
  /// Must be blank or &quot;Y&quot; or &quot;N&quot;
  /// Y - Court or Administrative order exists
  /// N - No Court or Administrative order exists
  /// </summary>
  [JsonPropertyName("courtOrAdminOrdInd")]
  [Member(Index = 8, Type = MemberType.Char, Length
    = CourtOrAdminOrdInd_MaxLength, Optional = true)]
  public string CourtOrAdminOrdInd
  {
    get => courtOrAdminOrdInd;
    set => courtOrAdminOrdInd = value != null
      ? TrimEnd(Substring(value, 1, CourtOrAdminOrdInd_MaxLength)) : null;
  }

  /// <summary>Length of the NO_MATCH_CODE attribute.</summary>
  public const int NoMatchCode_MaxLength = 2;

  /// <summary>
  /// The value of the NO_MATCH_CODE attribute.
  /// Returned by IRS.
  /// Return code returned by IRS when no match found.
  /// 18 - SSN not on IRS file
  /// 19 - Name submitted by state does not agree
  ///      with IRS name
  /// </summary>
  [JsonPropertyName("noMatchCode")]
  [Member(Index = 9, Type = MemberType.Char, Length = NoMatchCode_MaxLength, Optional
    = true)]
  public string NoMatchCode
  {
    get => noMatchCode;
    set => noMatchCode = value != null
      ? TrimEnd(Substring(value, 1, NoMatchCode_MaxLength)) : null;
  }

  /// <summary>Length of the CREATED_BY attribute.</summary>
  public const int CreatedBy_MaxLength = 8;

  /// <summary>
  /// The value of the CREATED_BY attribute.
  /// User ID or Program ID responsible for creation of the occurrence.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 10, Type = MemberType.Char, Length = CreatedBy_MaxLength)]
  public string CreatedBy
  {
    get => createdBy ?? "";
    set => createdBy = TrimEnd(Substring(value, 1, CreatedBy_MaxLength));
  }

  /// <summary>
  /// The json value of the CreatedBy attribute.</summary>
  [JsonPropertyName("createdBy")]
  [Computed]
  public string CreatedBy_Json
  {
    get => NullIf(CreatedBy, "");
    set => CreatedBy = value;
  }

  /// <summary>
  /// The value of the CREATED_TIMESTAMP attribute.
  /// Timestamp of creation of the occurrence.
  /// </summary>
  [JsonPropertyName("createdTimestamp")]
  [Member(Index = 11, Type = MemberType.Timestamp)]
  public DateTime? CreatedTimestamp
  {
    get => createdTimestamp;
    set => createdTimestamp = value;
  }

  /// <summary>Length of the LAST_UPDATED_BY attribute.</summary>
  public const int LastUpdatedBy_MaxLength = 8;

  /// <summary>
  /// The value of the LAST_UPDATED_BY attribute.
  /// User ID or Program ID responsible for the last update of the occurrence.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 12, Type = MemberType.Char, Length = LastUpdatedBy_MaxLength)]
  public string LastUpdatedBy
  {
    get => lastUpdatedBy ?? "";
    set => lastUpdatedBy =
      TrimEnd(Substring(value, 1, LastUpdatedBy_MaxLength));
  }

  /// <summary>
  /// The json value of the LastUpdatedBy attribute.</summary>
  [JsonPropertyName("lastUpdatedBy")]
  [Computed]
  public string LastUpdatedBy_Json
  {
    get => NullIf(LastUpdatedBy, "");
    set => LastUpdatedBy = value;
  }

  /// <summary>
  /// The value of the LAST_UPDATED_TIMESTAMP attribute.
  /// Timestamp of last update of the occurrence.
  /// </summary>
  [JsonPropertyName("lastUpdatedTimestamp")]
  [Member(Index = 13, Type = MemberType.Timestamp)]
  public DateTime? LastUpdatedTimestamp
  {
    get => lastUpdatedTimestamp;
    set => lastUpdatedTimestamp = value;
  }

  /// <summary>
  /// The value of the REQUEST_SENT_DATE attribute.
  /// This attribute specifies the date on which the 1099 tape was created.
  /// </summary>
  [JsonPropertyName("requestSentDate")]
  [Member(Index = 14, Type = MemberType.Date, Optional = true)]
  public DateTime? RequestSentDate
  {
    get => requestSentDate;
    set => requestSentDate = value;
  }

  /// <summary>Length of the MIDDLE_INITIAL attribute.</summary>
  public const int MiddleInitial_MaxLength = 1;

  /// <summary>
  /// The value of the MIDDLE_INITIAL attribute.
  /// RESP: KESSEP
  /// 
  /// IRS requires middle initial in request file specifications.
  /// </summary>
  [JsonPropertyName("middleInitial")]
  [Member(Index = 15, Type = MemberType.Char, Length
    = MiddleInitial_MaxLength, Optional = true)]
  public string MiddleInitial
  {
    get => middleInitial;
    set => middleInitial = value != null
      ? TrimEnd(Substring(value, 1, MiddleInitial_MaxLength)) : null;
  }

  /// <summary>Length of the NUMBER attribute.</summary>
  public const int CspNumber_MaxLength = 10;

  /// <summary>
  /// The value of the NUMBER attribute.
  /// This is a system generated number which will be used by the users to 
  /// identify a person.  This will have a business meaning, but will be unique
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 16, Type = MemberType.Char, Length = CspNumber_MaxLength)]
  public string CspNumber
  {
    get => cspNumber ?? "";
    set => cspNumber = TrimEnd(Substring(value, 1, CspNumber_MaxLength));
  }

  /// <summary>
  /// The json value of the CspNumber attribute.</summary>
  [JsonPropertyName("cspNumber")]
  [Computed]
  public string CspNumber_Json
  {
    get => NullIf(CspNumber, "");
    set => CspNumber = value;
  }

  private string firstName;
  private int identifier;
  private string ssn;
  private string localCode;
  private string lastName;
  private string afdcCode;
  private string caseIdNo;
  private string courtOrAdminOrdInd;
  private string noMatchCode;
  private string createdBy;
  private DateTime? createdTimestamp;
  private string lastUpdatedBy;
  private DateTime? lastUpdatedTimestamp;
  private DateTime? requestSentDate;
  private string middleInitial;
  private string cspNumber;
}
