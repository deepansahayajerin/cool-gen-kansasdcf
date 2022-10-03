// The source file: FCR_SVES_TITLE_II_PENDING_CLAIM, ID: 374521140, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Worksets;

/// <summary>
/// This is file record layout for FCR SVES Title II pending Claim responses 
/// from FCR.
/// </summary>
[Serializable]
public partial class FcrSvesTitleIiPendingClaim: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public FcrSvesTitleIiPendingClaim()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public FcrSvesTitleIiPendingClaim(FcrSvesTitleIiPendingClaim that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new FcrSvesTitleIiPendingClaim Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(FcrSvesTitleIiPendingClaim that)
  {
    base.Assign(that);
    recordIdentifier = that.recordIdentifier;
    matchTypeCode = that.matchTypeCode;
    transmitterStateTerritoryCode = that.transmitterStateTerritoryCode;
    locSrcResponseAgencyCode = that.locSrcResponseAgencyCode;
    nameMatchedCode = that.nameMatchedCode;
    firstName = that.firstName;
    middleName = that.middleName;
    lastName = that.lastName;
    additionalFirstName1 = that.additionalFirstName1;
    additionalMiddleName1 = that.additionalMiddleName1;
    additionalLastName1 = that.additionalLastName1;
    additionalFirstName2 = that.additionalFirstName2;
    additionalMiddleName2 = that.additionalMiddleName2;
    additionalLastName2 = that.additionalLastName2;
    returnedFirstName = that.returnedFirstName;
    returnedMiddleName = that.returnedMiddleName;
    returnedLastName = that.returnedLastName;
    ssn = that.ssn;
    memberIdentifier = that.memberIdentifier;
    fipsCountyCode = that.fipsCountyCode;
    responseDate = that.responseDate;
    locateResponseCode = that.locateResponseCode;
    corrAdditlMultipleSsn = that.corrAdditlMultipleSsn;
    ssnMatchCode = that.ssnMatchCode;
    claimTypeCode = that.claimTypeCode;
    claimantAddress = that.claimantAddress;
    doMailingAddressLine1 = that.doMailingAddressLine1;
    doMailingAddressLine2 = that.doMailingAddressLine2;
    doMailingAddressLine3 = that.doMailingAddressLine3;
    doMailingAddressLine4 = that.doMailingAddressLine4;
    doMailingAddressCity = that.doMailingAddressCity;
    doMailingAddressState = that.doMailingAddressState;
    doMainlingAddressZip = that.doMainlingAddressZip;
    participantTypeCode = that.participantTypeCode;
    stateSortCode = that.stateSortCode;
  }

  /// <summary>Length of the RECORD_IDENTIFIER attribute.</summary>
  public const int RecordIdentifier_MaxLength = 2;

  /// <summary>
  /// The value of the RECORD_IDENTIFIER attribute.
  /// This field value identifies FCR Records types (e.g. FT - Proactive Match 
  /// Record, FD Case Record, FS-Person Record, MC-MSFIDM Record, FN-Newhire
  /// Records ETc.) and for SVES record type this field contains the characters
  /// FK.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 1, Type = MemberType.Char, Length = RecordIdentifier_MaxLength)
    ]
  public string RecordIdentifier
  {
    get => recordIdentifier ?? "";
    set => recordIdentifier =
      TrimEnd(Substring(value, 1, RecordIdentifier_MaxLength));
  }

  /// <summary>
  /// The json value of the RecordIdentifier attribute.</summary>
  [JsonPropertyName("recordIdentifier")]
  [Computed]
  public string RecordIdentifier_Json
  {
    get => NullIf(RecordIdentifier, "");
    set => RecordIdentifier = value;
  }

  /// <summary>Length of the MATCH_TYPE_CODE attribute.</summary>
  public const int MatchTypeCode_MaxLength = 1;

  /// <summary>
  /// The value of the MATCH_TYPE_CODE attribute.
  /// This field contains a value to indicate the action that initiated the 
  /// generation of this record:
  /// N	 Title II Pending Claim-to-FCR proactive response for new information 
  /// added to the Title II Pending Claim File.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 2, Type = MemberType.Char, Length = MatchTypeCode_MaxLength)]
  public string MatchTypeCode
  {
    get => matchTypeCode ?? "";
    set => matchTypeCode =
      TrimEnd(Substring(value, 1, MatchTypeCode_MaxLength));
  }

  /// <summary>
  /// The json value of the MatchTypeCode attribute.</summary>
  [JsonPropertyName("matchTypeCode")]
  [Computed]
  public string MatchTypeCode_Json
  {
    get => NullIf(MatchTypeCode, "");
    set => MatchTypeCode = value;
  }

  /// <summary>Length of the TRANSMITTER_STATE_TERRITORY_CODE attribute.
  /// </summary>
  public const int TransmitterStateTerritoryCode_MaxLength = 2;

  /// <summary>
  /// The value of the TRANSMITTER_STATE_TERRITORY_CODE attribute.
  /// This field contains the two-digit numeric FIPS Code of the State or 
  /// Territory that is stored on the FCR System for a proactive match.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 3, Type = MemberType.Char, Length
    = TransmitterStateTerritoryCode_MaxLength)]
  public string TransmitterStateTerritoryCode
  {
    get => transmitterStateTerritoryCode ?? "";
    set => transmitterStateTerritoryCode =
      TrimEnd(Substring(value, 1, TransmitterStateTerritoryCode_MaxLength));
  }

  /// <summary>
  /// The json value of the TransmitterStateTerritoryCode attribute.</summary>
  [JsonPropertyName("transmitterStateTerritoryCode")]
  [Computed]
  public string TransmitterStateTerritoryCode_Json
  {
    get => NullIf(TransmitterStateTerritoryCode, "");
    set => TransmitterStateTerritoryCode = value;
  }

  /// <summary>Length of the LOC_SRC_RESPONSE_AGENCY_CODE attribute.</summary>
  public const int LocSrcResponseAgencyCode_MaxLength = 3;

  /// <summary>
  /// The value of the LOC_SRC_RESPONSE_AGENCY_CODE attribute.
  /// This field contains the code E04 to identify the Title II Pending Claim 
  /// Locate source.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 4, Type = MemberType.Char, Length
    = LocSrcResponseAgencyCode_MaxLength)]
  public string LocSrcResponseAgencyCode
  {
    get => locSrcResponseAgencyCode ?? "";
    set => locSrcResponseAgencyCode =
      TrimEnd(Substring(value, 1, LocSrcResponseAgencyCode_MaxLength));
  }

  /// <summary>
  /// The json value of the LocSrcResponseAgencyCode attribute.</summary>
  [JsonPropertyName("locSrcResponseAgencyCode")]
  [Computed]
  public string LocSrcResponseAgencyCode_Json
  {
    get => NullIf(LocSrcResponseAgencyCode, "");
    set => LocSrcResponseAgencyCode = value;
  }

  /// <summary>Length of the NAME_MATCHED_CODE attribute.</summary>
  public const int NameMatchedCode_MaxLength = 1;

  /// <summary>
  /// The value of the NAME_MATCHED_CODE attribute.
  /// This field contains a value to indicate which name matched the name on the
  /// Title II Pending Claim record:
  /// 1	 First letter of First Name, first four letters of Last Name
  /// 2	 First letter of Additional First Name 1, first four letters of 
  /// Additional Last Name 1
  /// 3	 First letter of Additional First Name 2, first four letters of 
  /// Additional Last Name 2
  /// If the Name or Additional Names do not match a Title II Pending Claim 
  /// Returned Name, this field contains a space.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 5, Type = MemberType.Char, Length = NameMatchedCode_MaxLength)]
    
  public string NameMatchedCode
  {
    get => nameMatchedCode ?? "";
    set => nameMatchedCode =
      TrimEnd(Substring(value, 1, NameMatchedCode_MaxLength));
  }

  /// <summary>
  /// The json value of the NameMatchedCode attribute.</summary>
  [JsonPropertyName("nameMatchedCode")]
  [Computed]
  public string NameMatchedCode_Json
  {
    get => NullIf(NameMatchedCode, "");
    set => NameMatchedCode = value;
  }

  /// <summary>Length of the FIRST_NAME attribute.</summary>
  public const int FirstName_MaxLength = 16;

  /// <summary>
  /// The value of the FIRST_NAME attribute.
  /// This field contains the First Name that is stored on the FCR Database.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 6, Type = MemberType.Char, Length = FirstName_MaxLength)]
  public string FirstName
  {
    get => firstName ?? "";
    set => firstName = TrimEnd(Substring(value, 1, FirstName_MaxLength));
  }

  /// <summary>
  /// The json value of the FirstName attribute.</summary>
  [JsonPropertyName("firstName")]
  [Computed]
  public string FirstName_Json
  {
    get => NullIf(FirstName, "");
    set => FirstName = value;
  }

  /// <summary>Length of the MIDDLE_NAME attribute.</summary>
  public const int MiddleName_MaxLength = 16;

  /// <summary>
  /// The value of the MIDDLE_NAME attribute.
  /// This field contains the Middle Name that is stored on the FCR Database.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 7, Type = MemberType.Char, Length = MiddleName_MaxLength)]
  public string MiddleName
  {
    get => middleName ?? "";
    set => middleName = TrimEnd(Substring(value, 1, MiddleName_MaxLength));
  }

  /// <summary>
  /// The json value of the MiddleName attribute.</summary>
  [JsonPropertyName("middleName")]
  [Computed]
  public string MiddleName_Json
  {
    get => NullIf(MiddleName, "");
    set => MiddleName = value;
  }

  /// <summary>Length of the LAST_NAME attribute.</summary>
  public const int LastName_MaxLength = 30;

  /// <summary>
  /// The value of the LAST_NAME attribute.
  /// This field contains the Middle Name that is stored on the FCR Database.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 8, Type = MemberType.Char, Length = LastName_MaxLength)]
  public string LastName
  {
    get => lastName ?? "";
    set => lastName = TrimEnd(Substring(value, 1, LastName_MaxLength));
  }

  /// <summary>
  /// The json value of the LastName attribute.</summary>
  [JsonPropertyName("lastName")]
  [Computed]
  public string LastName_Json
  {
    get => NullIf(LastName, "");
    set => LastName = value;
  }

  /// <summary>Length of the ADDITIONAL_FIRST_NAME_1 attribute.</summary>
  public const int AdditionalFirstName1_MaxLength = 16;

  /// <summary>
  /// The value of the ADDITIONAL_FIRST_NAME_1 attribute.
  /// If an Additional First Name 1 on the FCR Database was used in the match, 
  /// this field contains the name that was used.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 9, Type = MemberType.Char, Length
    = AdditionalFirstName1_MaxLength)]
  public string AdditionalFirstName1
  {
    get => additionalFirstName1 ?? "";
    set => additionalFirstName1 =
      TrimEnd(Substring(value, 1, AdditionalFirstName1_MaxLength));
  }

  /// <summary>
  /// The json value of the AdditionalFirstName1 attribute.</summary>
  [JsonPropertyName("additionalFirstName1")]
  [Computed]
  public string AdditionalFirstName1_Json
  {
    get => NullIf(AdditionalFirstName1, "");
    set => AdditionalFirstName1 = value;
  }

  /// <summary>Length of the ADDITIONAL_MIDDLE_NAME_1 attribute.</summary>
  public const int AdditionalMiddleName1_MaxLength = 16;

  /// <summary>
  /// The value of the ADDITIONAL_MIDDLE_NAME_1 attribute.
  /// This field contains the Additional Middle Name 1 that is stored on the FCR
  /// Database.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 10, Type = MemberType.Char, Length
    = AdditionalMiddleName1_MaxLength)]
  public string AdditionalMiddleName1
  {
    get => additionalMiddleName1 ?? "";
    set => additionalMiddleName1 =
      TrimEnd(Substring(value, 1, AdditionalMiddleName1_MaxLength));
  }

  /// <summary>
  /// The json value of the AdditionalMiddleName1 attribute.</summary>
  [JsonPropertyName("additionalMiddleName1")]
  [Computed]
  public string AdditionalMiddleName1_Json
  {
    get => NullIf(AdditionalMiddleName1, "");
    set => AdditionalMiddleName1 = value;
  }

  /// <summary>Length of the ADDITIONAL_LAST_NAME_1 attribute.</summary>
  public const int AdditionalLastName1_MaxLength = 30;

  /// <summary>
  /// The value of the ADDITIONAL_LAST_NAME_1 attribute.
  /// If an Additional Last Name 1 on the FCR Database was used in the match, 
  /// this field contains the name that was used.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 11, Type = MemberType.Char, Length
    = AdditionalLastName1_MaxLength)]
  public string AdditionalLastName1
  {
    get => additionalLastName1 ?? "";
    set => additionalLastName1 =
      TrimEnd(Substring(value, 1, AdditionalLastName1_MaxLength));
  }

  /// <summary>
  /// The json value of the AdditionalLastName1 attribute.</summary>
  [JsonPropertyName("additionalLastName1")]
  [Computed]
  public string AdditionalLastName1_Json
  {
    get => NullIf(AdditionalLastName1, "");
    set => AdditionalLastName1 = value;
  }

  /// <summary>Length of the ADDITIONAL_FIRST_NAME_2 attribute.</summary>
  public const int AdditionalFirstName2_MaxLength = 16;

  /// <summary>
  /// The value of the ADDITIONAL_FIRST_NAME_2 attribute.
  /// If an Additional First Name 2 on the FCR Database was used in the match, 
  /// this field contains the name that was used.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 12, Type = MemberType.Char, Length
    = AdditionalFirstName2_MaxLength)]
  public string AdditionalFirstName2
  {
    get => additionalFirstName2 ?? "";
    set => additionalFirstName2 =
      TrimEnd(Substring(value, 1, AdditionalFirstName2_MaxLength));
  }

  /// <summary>
  /// The json value of the AdditionalFirstName2 attribute.</summary>
  [JsonPropertyName("additionalFirstName2")]
  [Computed]
  public string AdditionalFirstName2_Json
  {
    get => NullIf(AdditionalFirstName2, "");
    set => AdditionalFirstName2 = value;
  }

  /// <summary>Length of the ADDITIONAL_MIDDLE_NAME_2 attribute.</summary>
  public const int AdditionalMiddleName2_MaxLength = 16;

  /// <summary>
  /// The value of the ADDITIONAL_MIDDLE_NAME_2 attribute.
  /// This field contains the Additional Middle Name 2 that is stored on the FCR
  /// Database.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 13, Type = MemberType.Char, Length
    = AdditionalMiddleName2_MaxLength)]
  public string AdditionalMiddleName2
  {
    get => additionalMiddleName2 ?? "";
    set => additionalMiddleName2 =
      TrimEnd(Substring(value, 1, AdditionalMiddleName2_MaxLength));
  }

  /// <summary>
  /// The json value of the AdditionalMiddleName2 attribute.</summary>
  [JsonPropertyName("additionalMiddleName2")]
  [Computed]
  public string AdditionalMiddleName2_Json
  {
    get => NullIf(AdditionalMiddleName2, "");
    set => AdditionalMiddleName2 = value;
  }

  /// <summary>Length of the ADDITIONAL_LAST_NAME_2 attribute.</summary>
  public const int AdditionalLastName2_MaxLength = 30;

  /// <summary>
  /// The value of the ADDITIONAL_LAST_NAME_2 attribute.
  /// If an Additional Last Name 2 on the FCR Database was used in the match, 
  /// this field contains the name that was used.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 14, Type = MemberType.Char, Length
    = AdditionalLastName2_MaxLength)]
  public string AdditionalLastName2
  {
    get => additionalLastName2 ?? "";
    set => additionalLastName2 =
      TrimEnd(Substring(value, 1, AdditionalLastName2_MaxLength));
  }

  /// <summary>
  /// The json value of the AdditionalLastName2 attribute.</summary>
  [JsonPropertyName("additionalLastName2")]
  [Computed]
  public string AdditionalLastName2_Json
  {
    get => NullIf(AdditionalLastName2, "");
    set => AdditionalLastName2 = value;
  }

  /// <summary>Length of the RETURNED_FIRST_NAME attribute.</summary>
  public const int ReturnedFirstName_MaxLength = 16;

  /// <summary>
  /// The value of the RETURNED_FIRST_NAME attribute.
  /// This field contains the first name of the claimant that was returned from 
  /// Title II Pending Claim File.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 15, Type = MemberType.Char, Length
    = ReturnedFirstName_MaxLength)]
  public string ReturnedFirstName
  {
    get => returnedFirstName ?? "";
    set => returnedFirstName =
      TrimEnd(Substring(value, 1, ReturnedFirstName_MaxLength));
  }

  /// <summary>
  /// The json value of the ReturnedFirstName attribute.</summary>
  [JsonPropertyName("returnedFirstName")]
  [Computed]
  public string ReturnedFirstName_Json
  {
    get => NullIf(ReturnedFirstName, "");
    set => ReturnedFirstName = value;
  }

  /// <summary>Length of the RETURNED_MIDDLE_NAME attribute.</summary>
  public const int ReturnedMiddleName_MaxLength = 16;

  /// <summary>
  /// The value of the RETURNED_MIDDLE_NAME attribute.
  /// This field contains the middle name of the claimant that was returned from
  /// Title II Pending Claim File.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 16, Type = MemberType.Char, Length
    = ReturnedMiddleName_MaxLength)]
  public string ReturnedMiddleName
  {
    get => returnedMiddleName ?? "";
    set => returnedMiddleName =
      TrimEnd(Substring(value, 1, ReturnedMiddleName_MaxLength));
  }

  /// <summary>
  /// The json value of the ReturnedMiddleName attribute.</summary>
  [JsonPropertyName("returnedMiddleName")]
  [Computed]
  public string ReturnedMiddleName_Json
  {
    get => NullIf(ReturnedMiddleName, "");
    set => ReturnedMiddleName = value;
  }

  /// <summary>Length of the RETURNED_LAST_NAME attribute.</summary>
  public const int ReturnedLastName_MaxLength = 30;

  /// <summary>
  /// The value of the RETURNED_LAST_NAME attribute.
  /// This field contains the last name of the claimant that was returned from 
  /// Title II Pending Claim File.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 17, Type = MemberType.Char, Length
    = ReturnedLastName_MaxLength)]
  public string ReturnedLastName
  {
    get => returnedLastName ?? "";
    set => returnedLastName =
      TrimEnd(Substring(value, 1, ReturnedLastName_MaxLength));
  }

  /// <summary>
  /// The json value of the ReturnedLastName attribute.</summary>
  [JsonPropertyName("returnedLastName")]
  [Computed]
  public string ReturnedLastName_Json
  {
    get => NullIf(ReturnedLastName, "");
    set => ReturnedLastName = value;
  }

  /// <summary>Length of the SSN attribute.</summary>
  public const int Ssn_MaxLength = 9;

  /// <summary>
  /// The value of the SSN attribute.
  /// This field contains the Primary SSN that is stored on the FCR Database for
  /// the matched person.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 18, Type = MemberType.Char, Length = Ssn_MaxLength)]
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

  /// <summary>Length of the MEMBER_IDENTIFIER attribute.</summary>
  public const int MemberIdentifier_MaxLength = 10;

  /// <summary>
  /// The value of the MEMBER_IDENTIFIER attribute.
  /// This field contains the Member ID that is stored on the FCR Database.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 19, Type = MemberType.Char, Length
    = MemberIdentifier_MaxLength)]
  public string MemberIdentifier
  {
    get => memberIdentifier ?? "";
    set => memberIdentifier =
      TrimEnd(Substring(value, 1, MemberIdentifier_MaxLength));
  }

  /// <summary>
  /// The json value of the MemberIdentifier attribute.</summary>
  [JsonPropertyName("memberIdentifier")]
  [Computed]
  public string MemberIdentifier_Json
  {
    get => NullIf(MemberIdentifier, "");
    set => MemberIdentifier = value;
  }

  /// <summary>Length of the FIPS_COUNTY_CODE attribute.</summary>
  public const int FipsCountyCode_MaxLength = 3;

  /// <summary>
  /// The value of the FIPS_COUNTY_CODE attribute.
  /// This field contains the FIPS County Code that is stored on the FCR 
  /// Database.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 20, Type = MemberType.Char, Length = FipsCountyCode_MaxLength)]
    
  public string FipsCountyCode
  {
    get => fipsCountyCode ?? "";
    set => fipsCountyCode =
      TrimEnd(Substring(value, 1, FipsCountyCode_MaxLength));
  }

  /// <summary>
  /// The json value of the FipsCountyCode attribute.</summary>
  [JsonPropertyName("fipsCountyCode")]
  [Computed]
  public string FipsCountyCode_Json
  {
    get => NullIf(FipsCountyCode, "");
    set => FipsCountyCode = value;
  }

  /// <summary>
  /// The value of the RESPONSE_DATE attribute.
  /// This field contains the date that the Title II Pending Claim Response 
  /// Record was returned to FCR. The date is in CCYYMMDD format.
  /// </summary>
  [JsonPropertyName("responseDate")]
  [Member(Index = 21, Type = MemberType.Date)]
  public DateTime? ResponseDate
  {
    get => responseDate;
    set => responseDate = value;
  }

  /// <summary>Length of the LOCATE_RESPONSE_CODE attribute.</summary>
  public const int LocateResponseCode_MaxLength = 2;

  /// <summary>
  /// The value of the LOCATE_RESPONSE_CODE attribute.
  /// This field contains spaces to indicate that a Title II Pending Claim match
  /// is being returned.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 22, Type = MemberType.Char, Length
    = LocateResponseCode_MaxLength)]
  public string LocateResponseCode
  {
    get => locateResponseCode ?? "";
    set => locateResponseCode =
      TrimEnd(Substring(value, 1, LocateResponseCode_MaxLength));
  }

  /// <summary>
  /// The json value of the LocateResponseCode attribute.</summary>
  [JsonPropertyName("locateResponseCode")]
  [Computed]
  public string LocateResponseCode_Json
  {
    get => NullIf(LocateResponseCode, "");
    set => LocateResponseCode = value;
  }

  /// <summary>Length of the CORR_ADDITL_MULTIPLE_SSN attribute.</summary>
  public const int CorrAdditlMultipleSsn_MaxLength = 9;

  /// <summary>
  /// The value of the CORR_ADDITL_MULTIPLE_SSN attribute.
  /// This field contains the corrected, the additional, or the multiple SSN 
  /// that was used in the match.
  /// 	If the SSN Match Code is an A, this field contains the verified 
  /// additional SSN.
  /// 	If the SSN Match Code is a C, this field contains the corrected SSN or
  /// identified SSN.
  /// 	If the SSN Match Code is an M this field contains the multiple SSN.
  /// 	If the SSN Match Code is a V, this field contains spaces. (The SSN 
  /// that was used in the match is in the SSN field.)
  /// 	If the SSN Match Code is an X this field contains the corrected or 
  /// identified additional SSN.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 23, Type = MemberType.Char, Length
    = CorrAdditlMultipleSsn_MaxLength)]
  public string CorrAdditlMultipleSsn
  {
    get => corrAdditlMultipleSsn ?? "";
    set => corrAdditlMultipleSsn =
      TrimEnd(Substring(value, 1, CorrAdditlMultipleSsn_MaxLength));
  }

  /// <summary>
  /// The json value of the CorrAdditlMultipleSsn attribute.</summary>
  [JsonPropertyName("corrAdditlMultipleSsn")]
  [Computed]
  public string CorrAdditlMultipleSsn_Json
  {
    get => NullIf(CorrAdditlMultipleSsn, "");
    set => CorrAdditlMultipleSsn = value;
  }

  /// <summary>Length of the SSN_MATCH_CODE attribute.</summary>
  public const int SsnMatchCode_MaxLength = 1;

  /// <summary>
  /// The value of the SSN_MATCH_CODE attribute.
  /// This field contains a value that indicates which SSN was used in match.
  /// A	 Verified Additional SSN/Name combination
  /// C	 State-submitted SSN/Name combination corrected or identified by 
  /// processes ESKARI, RMR, IRS-U, Alpha-Search or SVES corrected routine
  /// M	 Multiple SSN that was issued by SSA
  /// V	 State-submitted verified SSN/Name combination
  /// X	 Additional SSN/Name combination corrected or identified by processes 
  /// ESKARI, RMR, IRS-U or Alpha-Search
  /// If this field is A, C, M or X, the SSN that was used in the match 
  /// is in the Corrected/Additional/Multiple SSN field.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 24, Type = MemberType.Char, Length = SsnMatchCode_MaxLength)]
  public string SsnMatchCode
  {
    get => ssnMatchCode ?? "";
    set => ssnMatchCode = TrimEnd(Substring(value, 1, SsnMatchCode_MaxLength));
  }

  /// <summary>
  /// The json value of the SsnMatchCode attribute.</summary>
  [JsonPropertyName("ssnMatchCode")]
  [Computed]
  public string SsnMatchCode_Json
  {
    get => NullIf(SsnMatchCode, "");
    set => SsnMatchCode = value;
  }

  /// <summary>Length of the CLAIM_TYPE_CODE attribute.</summary>
  public const int ClaimTypeCode_MaxLength = 2;

  /// <summary>
  /// The value of the CLAIM_TYPE_CODE attribute.
  /// This field contains a value to indicate the claim type:
  /// AU	 Auxiliary
  /// DI	 Disability
  /// RI	 Retirement
  /// SU	 Survivor Benefits
  /// This field contains spaces if claim type is not available.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 25, Type = MemberType.Char, Length = ClaimTypeCode_MaxLength)]
  public string ClaimTypeCode
  {
    get => claimTypeCode ?? "";
    set => claimTypeCode =
      TrimEnd(Substring(value, 1, ClaimTypeCode_MaxLength));
  }

  /// <summary>
  /// The json value of the ClaimTypeCode attribute.</summary>
  [JsonPropertyName("claimTypeCode")]
  [Computed]
  public string ClaimTypeCode_Json
  {
    get => NullIf(ClaimTypeCode, "");
    set => ClaimTypeCode = value;
  }

  /// <summary>Length of the CLAIMANT_ADDRESS attribute.</summary>
  public const int ClaimantAddress_MaxLength = 240;

  /// <summary>
  /// The value of the CLAIMANT_ADDRESS attribute.
  /// This field is reserved for claimant address information. Currently, this 
  /// field is not available from SSA and will contain spaces. This field will
  /// be defined at a later date by FCR when information becomes available.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 26, Type = MemberType.Varchar, Length
    = ClaimantAddress_MaxLength)]
  public string ClaimantAddress
  {
    get => claimantAddress ?? "";
    set => claimantAddress = Substring(value, 1, ClaimantAddress_MaxLength);
  }

  /// <summary>
  /// The json value of the ClaimantAddress attribute.</summary>
  [JsonPropertyName("claimantAddress")]
  [Computed]
  public string ClaimantAddress_Json
  {
    get => NullIf(ClaimantAddress, "");
    set => ClaimantAddress = value;
  }

  /// <summary>Length of the DO_MAILING_ADDRESS_LINE_1 attribute.</summary>
  public const int DoMailingAddressLine1_MaxLength = 22;

  /// <summary>
  /// The value of the DO_MAILING_ADDRESS_LINE_1 attribute.
  /// This field contains the first line of the default District Office Mailing 
  /// Address.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 27, Type = MemberType.Char, Length
    = DoMailingAddressLine1_MaxLength)]
  public string DoMailingAddressLine1
  {
    get => doMailingAddressLine1 ?? "";
    set => doMailingAddressLine1 =
      TrimEnd(Substring(value, 1, DoMailingAddressLine1_MaxLength));
  }

  /// <summary>
  /// The json value of the DoMailingAddressLine1 attribute.</summary>
  [JsonPropertyName("doMailingAddressLine1")]
  [Computed]
  public string DoMailingAddressLine1_Json
  {
    get => NullIf(DoMailingAddressLine1, "");
    set => DoMailingAddressLine1 = value;
  }

  /// <summary>Length of the DO_MAILING_ADDRESS_LINE_2 attribute.</summary>
  public const int DoMailingAddressLine2_MaxLength = 22;

  /// <summary>
  /// The value of the DO_MAILING_ADDRESS_LINE_2 attribute.
  /// This field contains the second line of the default District Office Mailing
  /// Address.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 28, Type = MemberType.Char, Length
    = DoMailingAddressLine2_MaxLength)]
  public string DoMailingAddressLine2
  {
    get => doMailingAddressLine2 ?? "";
    set => doMailingAddressLine2 =
      TrimEnd(Substring(value, 1, DoMailingAddressLine2_MaxLength));
  }

  /// <summary>
  /// The json value of the DoMailingAddressLine2 attribute.</summary>
  [JsonPropertyName("doMailingAddressLine2")]
  [Computed]
  public string DoMailingAddressLine2_Json
  {
    get => NullIf(DoMailingAddressLine2, "");
    set => DoMailingAddressLine2 = value;
  }

  /// <summary>Length of the DO_MAILING_ADDRESS_LINE_3 attribute.</summary>
  public const int DoMailingAddressLine3_MaxLength = 22;

  /// <summary>
  /// The value of the DO_MAILING_ADDRESS_LINE_3 attribute.
  /// This field contains the third line of the default District Office Mailing 
  /// Address.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 29, Type = MemberType.Char, Length
    = DoMailingAddressLine3_MaxLength)]
  public string DoMailingAddressLine3
  {
    get => doMailingAddressLine3 ?? "";
    set => doMailingAddressLine3 =
      TrimEnd(Substring(value, 1, DoMailingAddressLine3_MaxLength));
  }

  /// <summary>
  /// The json value of the DoMailingAddressLine3 attribute.</summary>
  [JsonPropertyName("doMailingAddressLine3")]
  [Computed]
  public string DoMailingAddressLine3_Json
  {
    get => NullIf(DoMailingAddressLine3, "");
    set => DoMailingAddressLine3 = value;
  }

  /// <summary>Length of the DO_MAILING_ADDRESS_LINE_4 attribute.</summary>
  public const int DoMailingAddressLine4_MaxLength = 22;

  /// <summary>
  /// The value of the DO_MAILING_ADDRESS_LINE_4 attribute.
  /// This field contains the fourth line of the default District Office Mailing
  /// Address.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 30, Type = MemberType.Char, Length
    = DoMailingAddressLine4_MaxLength)]
  public string DoMailingAddressLine4
  {
    get => doMailingAddressLine4 ?? "";
    set => doMailingAddressLine4 =
      TrimEnd(Substring(value, 1, DoMailingAddressLine4_MaxLength));
  }

  /// <summary>
  /// The json value of the DoMailingAddressLine4 attribute.</summary>
  [JsonPropertyName("doMailingAddressLine4")]
  [Computed]
  public string DoMailingAddressLine4_Json
  {
    get => NullIf(DoMailingAddressLine4, "");
    set => DoMailingAddressLine4 = value;
  }

  /// <summary>Length of the DO_MAILING_ADDRESS_CITY attribute.</summary>
  public const int DoMailingAddressCity_MaxLength = 28;

  /// <summary>
  /// The value of the DO_MAILING_ADDRESS_CITY attribute.
  /// This field contains the city of the default District Office Mailing 
  /// Address.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 31, Type = MemberType.Char, Length
    = DoMailingAddressCity_MaxLength)]
  public string DoMailingAddressCity
  {
    get => doMailingAddressCity ?? "";
    set => doMailingAddressCity =
      TrimEnd(Substring(value, 1, DoMailingAddressCity_MaxLength));
  }

  /// <summary>
  /// The json value of the DoMailingAddressCity attribute.</summary>
  [JsonPropertyName("doMailingAddressCity")]
  [Computed]
  public string DoMailingAddressCity_Json
  {
    get => NullIf(DoMailingAddressCity, "");
    set => DoMailingAddressCity = value;
  }

  /// <summary>Length of the DO_MAILING_ADDRESS_STATE attribute.</summary>
  public const int DoMailingAddressState_MaxLength = 2;

  /// <summary>
  /// The value of the DO_MAILING_ADDRESS_STATE attribute.
  /// This field contains the State abbreviation of the default District Office 
  /// Mailing Address.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 32, Type = MemberType.Char, Length
    = DoMailingAddressState_MaxLength)]
  public string DoMailingAddressState
  {
    get => doMailingAddressState ?? "";
    set => doMailingAddressState =
      TrimEnd(Substring(value, 1, DoMailingAddressState_MaxLength));
  }

  /// <summary>
  /// The json value of the DoMailingAddressState attribute.</summary>
  [JsonPropertyName("doMailingAddressState")]
  [Computed]
  public string DoMailingAddressState_Json
  {
    get => NullIf(DoMailingAddressState, "");
    set => DoMailingAddressState = value;
  }

  /// <summary>Length of the DO_MAINLING_ADDRESS_ZIP attribute.</summary>
  public const int DoMainlingAddressZip_MaxLength = 9;

  /// <summary>
  /// The value of the DO_MAINLING_ADDRESS_ZIP attribute.
  /// This field contains the default District Office Mailing Address ZIP Code.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 33, Type = MemberType.Char, Length
    = DoMainlingAddressZip_MaxLength)]
  public string DoMainlingAddressZip
  {
    get => doMainlingAddressZip ?? "";
    set => doMainlingAddressZip =
      TrimEnd(Substring(value, 1, DoMainlingAddressZip_MaxLength));
  }

  /// <summary>
  /// The json value of the DoMainlingAddressZip attribute.</summary>
  [JsonPropertyName("doMainlingAddressZip")]
  [Computed]
  public string DoMainlingAddressZip_Json
  {
    get => NullIf(DoMainlingAddressZip, "");
    set => DoMainlingAddressZip = value;
  }

  /// <summary>Length of the PARTICIPANT_TYPE_CODE attribute.</summary>
  public const int ParticipantTypeCode_MaxLength = 2;

  /// <summary>
  /// The value of the PARTICIPANT_TYPE_CODE attribute.
  /// This field contains a value to define the matched persons Participant 
  /// Type on the case:
  /// CH	 Child
  /// CP	 Custodial Party
  /// NP	 Noncustodial Parent
  /// PF	 Putative Father
  /// If the Match Type is N and the person is on multiple cases, the 
  /// Participant Type that is returned is determined based on the following
  /// hierarchy: NP, PF, CP and CH.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 34, Type = MemberType.Char, Length
    = ParticipantTypeCode_MaxLength)]
  public string ParticipantTypeCode
  {
    get => participantTypeCode ?? "";
    set => participantTypeCode =
      TrimEnd(Substring(value, 1, ParticipantTypeCode_MaxLength));
  }

  /// <summary>
  /// The json value of the ParticipantTypeCode attribute.</summary>
  [JsonPropertyName("participantTypeCode")]
  [Computed]
  public string ParticipantTypeCode_Json
  {
    get => NullIf(ParticipantTypeCode, "");
    set => ParticipantTypeCode = value;
  }

  /// <summary>Length of the STATE_SORT_CODE attribute.</summary>
  public const int StateSortCode_MaxLength = 2;

  /// <summary>
  /// The value of the STATE_SORT_CODE attribute.
  /// This field will contain the two-digit numeric FIPS State Code of the State
  /// that will receive the response.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 35, Type = MemberType.Char, Length = StateSortCode_MaxLength)]
  public string StateSortCode
  {
    get => stateSortCode ?? "";
    set => stateSortCode =
      TrimEnd(Substring(value, 1, StateSortCode_MaxLength));
  }

  /// <summary>
  /// The json value of the StateSortCode attribute.</summary>
  [JsonPropertyName("stateSortCode")]
  [Computed]
  public string StateSortCode_Json
  {
    get => NullIf(StateSortCode, "");
    set => StateSortCode = value;
  }

  private string recordIdentifier;
  private string matchTypeCode;
  private string transmitterStateTerritoryCode;
  private string locSrcResponseAgencyCode;
  private string nameMatchedCode;
  private string firstName;
  private string middleName;
  private string lastName;
  private string additionalFirstName1;
  private string additionalMiddleName1;
  private string additionalLastName1;
  private string additionalFirstName2;
  private string additionalMiddleName2;
  private string additionalLastName2;
  private string returnedFirstName;
  private string returnedMiddleName;
  private string returnedLastName;
  private string ssn;
  private string memberIdentifier;
  private string fipsCountyCode;
  private DateTime? responseDate;
  private string locateResponseCode;
  private string corrAdditlMultipleSsn;
  private string ssnMatchCode;
  private string claimTypeCode;
  private string claimantAddress;
  private string doMailingAddressLine1;
  private string doMailingAddressLine2;
  private string doMailingAddressLine3;
  private string doMailingAddressLine4;
  private string doMailingAddressCity;
  private string doMailingAddressState;
  private string doMainlingAddressZip;
  private string participantTypeCode;
  private string stateSortCode;
}
