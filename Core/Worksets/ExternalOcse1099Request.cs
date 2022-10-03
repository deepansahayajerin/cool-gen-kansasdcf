// The source file: EXTERNAL_OCSE_1099_REQUEST, ID: 371802963, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Worksets;

/// <summary>
/// Resp: OBLGEST			
/// This Workset represents the external format for the 1099 submittal to the 
/// OCSE  for Locate information.	
/// All records submitted must be in a specific format as specified by OCSE.	
/// (Records that deviate from the specified layout will NOT be forwarded to 
/// Internal Revenue Service(IRS).
/// Responses will be received in the 1099 response format.	
/// </summary>
[Serializable]
public partial class ExternalOcse1099Request: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public ExternalOcse1099Request()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public ExternalOcse1099Request(ExternalOcse1099Request that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new ExternalOcse1099Request Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(ExternalOcse1099Request that)
  {
    base.Assign(that);
    ssn = that.ssn;
    submittingState = that.submittingState;
    localFipsCode = that.localFipsCode;
    caseIdNumber = that.caseIdNumber;
    lastName = that.lastName;
    firstName = that.firstName;
    caseTypeAfdcNafdc = that.caseTypeAfdcNafdc;
    courtAdminOrderIndicator = that.courtAdminOrderIndicator;
    blanks = that.blanks;
  }

  /// <summary>Length of the SSN attribute.</summary>
  public const int Ssn_MaxLength = 9;

  /// <summary>
  /// The value of the SSN attribute.
  /// This Required OCSE 1099 attribute represents the Social Security Number of
  /// CSE Person for whom the 1099 locate request is being submitted.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 1, Type = MemberType.Char, Length = Ssn_MaxLength)]
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

  /// <summary>Length of the SUBMITTING_STATE attribute.</summary>
  public const int SubmittingState_MaxLength = 2;

  /// <summary>
  /// The value of the SUBMITTING_STATE attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 2, Type = MemberType.Char, Length = SubmittingState_MaxLength)]
    
  public string SubmittingState
  {
    get => submittingState ?? "";
    set => submittingState =
      TrimEnd(Substring(value, 1, SubmittingState_MaxLength));
  }

  /// <summary>
  /// The json value of the SubmittingState attribute.</summary>
  [JsonPropertyName("submittingState")]
  [Computed]
  public string SubmittingState_Json
  {
    get => NullIf(SubmittingState, "");
    set => SubmittingState = value;
  }

  /// <summary>
  /// The value of the LOCAL_FIPS_CODE attribute.
  /// This Optional OCSE 1099 Request attribute, when used should be a three 
  /// digit numeric local code - FIPS code is suggested.
  /// </summary>
  [JsonPropertyName("localFipsCode")]
  [DefaultValue(0)]
  [Member(Index = 3, Type = MemberType.Number, Length = 3)]
  public int LocalFipsCode
  {
    get => localFipsCode;
    set => localFipsCode = value;
  }

  /// <summary>Length of the CASE_ID_NUMBER attribute.</summary>
  public const int CaseIdNumber_MaxLength = 15;

  /// <summary>
  /// The value of the CASE_ID_NUMBER attribute.
  /// This is a system generated number which will be used by the users to 
  /// identify a person.  This will have a business meaning, but will be unique
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 4, Type = MemberType.Char, Length = CaseIdNumber_MaxLength)]
  public string CaseIdNumber
  {
    get => caseIdNumber ?? "";
    set => caseIdNumber = TrimEnd(Substring(value, 1, CaseIdNumber_MaxLength));
  }

  /// <summary>
  /// The json value of the CaseIdNumber attribute.</summary>
  [JsonPropertyName("caseIdNumber")]
  [Computed]
  public string CaseIdNumber_Json
  {
    get => NullIf(CaseIdNumber, "");
    set => CaseIdNumber = value;
  }

  /// <summary>Length of the LAST_NAME attribute.</summary>
  public const int LastName_MaxLength = 20;

  /// <summary>
  /// The value of the LAST_NAME attribute.
  /// CSE_PERSON last name
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 5, Type = MemberType.Char, Length = LastName_MaxLength)]
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

  /// <summary>Length of the FIRST_NAME attribute.</summary>
  public const int FirstName_MaxLength = 15;

  /// <summary>
  /// The value of the FIRST_NAME attribute.
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

  /// <summary>Length of the CASE_TYPE_AFDC_NAFDC attribute.</summary>
  public const int CaseTypeAfdcNafdc_MaxLength = 1;

  /// <summary>
  /// The value of the CASE_TYPE_AFDC_NAFDC attribute.
  /// This OCSE optional attribute represents the CASE type as being either 
  /// &quot;A&quot; for AFDC, &quot;N&quot; for Non_AFDC or Blank.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 7, Type = MemberType.Char, Length
    = CaseTypeAfdcNafdc_MaxLength)]
  public string CaseTypeAfdcNafdc
  {
    get => caseTypeAfdcNafdc ?? "";
    set => caseTypeAfdcNafdc =
      TrimEnd(Substring(value, 1, CaseTypeAfdcNafdc_MaxLength));
  }

  /// <summary>
  /// The json value of the CaseTypeAfdcNafdc attribute.</summary>
  [JsonPropertyName("caseTypeAfdcNafdc")]
  [Computed]
  public string CaseTypeAfdcNafdc_Json
  {
    get => NullIf(CaseTypeAfdcNafdc, "");
    set => CaseTypeAfdcNafdc = value;
  }

  /// <summary>Length of the COURT_ADMIN_ORDER_INDICATOR attribute.</summary>
  public const int CourtAdminOrderIndicator_MaxLength = 1;

  /// <summary>
  /// The value of the COURT_ADMIN_ORDER_INDICATOR attribute.
  /// This OCSE optional attribute represents if the Order was administered by 
  /// the Courts or not.
  /// &quot;Y&quot; = Yes
  /// &quot;N&quot; = No
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 8, Type = MemberType.Char, Length
    = CourtAdminOrderIndicator_MaxLength)]
  public string CourtAdminOrderIndicator
  {
    get => courtAdminOrderIndicator ?? "";
    set => courtAdminOrderIndicator =
      TrimEnd(Substring(value, 1, CourtAdminOrderIndicator_MaxLength));
  }

  /// <summary>
  /// The json value of the CourtAdminOrderIndicator attribute.</summary>
  [JsonPropertyName("courtAdminOrderIndicator")]
  [Computed]
  public string CourtAdminOrderIndicator_Json
  {
    get => NullIf(CourtAdminOrderIndicator, "");
    set => CourtAdminOrderIndicator = value;
  }

  /// <summary>Length of the BLANKS attribute.</summary>
  public const int Blanks_MaxLength = 14;

  /// <summary>
  /// The value of the BLANKS attribute.
  /// This attribute is required by OCSE for 1099 submittal and Must contain 
  /// blanks.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 9, Type = MemberType.Char, Length = Blanks_MaxLength)]
  public string Blanks
  {
    get => blanks ?? "";
    set => blanks = TrimEnd(Substring(value, 1, Blanks_MaxLength));
  }

  /// <summary>
  /// The json value of the Blanks attribute.</summary>
  [JsonPropertyName("blanks")]
  [Computed]
  public string Blanks_Json
  {
    get => NullIf(Blanks, "");
    set => Blanks = value;
  }

  private string ssn;
  private string submittingState;
  private int localFipsCode;
  private string caseIdNumber;
  private string lastName;
  private string firstName;
  private string caseTypeAfdcNafdc;
  private string courtAdminOrderIndicator;
  private string blanks;
}
