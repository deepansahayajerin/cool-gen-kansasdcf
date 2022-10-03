// The source file: SP_DOC_KEY, ID: 371924175, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Worksets;

[Serializable]
public partial class SpDocKey: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public SpDocKey()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public SpDocKey(SpDocKey that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new SpDocKey Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(SpDocKey that)
  {
    base.Assign(that);
    keyAdminAction = that.keyAdminAction;
    keyAdminActionCert = that.keyAdminActionCert;
    keyAdminAppeal = that.keyAdminAppeal;
    keyAp = that.keyAp;
    keyAppointment = that.keyAppointment;
    keyAr = that.keyAr;
    keyBankruptcy = that.keyBankruptcy;
    keyCase = that.keyCase;
    keyCashRcptDetail = that.keyCashRcptDetail;
    keyCashRcptEvent = that.keyCashRcptEvent;
    keyCashRcptSource = that.keyCashRcptSource;
    keyCashRcptType = that.keyCashRcptType;
    keyChild = that.keyChild;
    keyContact = that.keyContact;
    keyGeneticTest = that.keyGeneticTest;
    keyHealthInsCoverage = that.keyHealthInsCoverage;
    keyIncarceration = that.keyIncarceration;
    keyIncomeSource = that.keyIncomeSource;
    keyInfoRequest = that.keyInfoRequest;
    keyInterstateRequest = that.keyInterstateRequest;
    keyLegalAction = that.keyLegalAction;
    keyLegalActionDetail = that.keyLegalActionDetail;
    keyLegalReferral = that.keyLegalReferral;
    keyLocateRequestAgency = that.keyLocateRequestAgency;
    keyLocateRequestSource = that.keyLocateRequestSource;
    keyMilitaryService = that.keyMilitaryService;
    keyObligation = that.keyObligation;
    keyObligationAdminAction = that.keyObligationAdminAction;
    keyObligationType = that.keyObligationType;
    keyOffice = that.keyOffice;
    keyPerson = that.keyPerson;
    keyPersonAccount = that.keyPersonAccount;
    keyPersonAddress = that.keyPersonAddress;
    keyRecaptureRule = that.keyRecaptureRule;
    keyResource = that.keyResource;
    keyTribunal = that.keyTribunal;
    keyWorkerComp = that.keyWorkerComp;
    keyWorksheet = that.keyWorksheet;
    keyXferFromDate = that.keyXferFromDate;
    keyXferToDate = that.keyXferToDate;
  }

  /// <summary>Length of the KEY_ADMIN_ACTION attribute.</summary>
  public const int KeyAdminAction_MaxLength = 4;

  /// <summary>
  /// The value of the KEY_ADMIN_ACTION attribute.
  /// This identifies the type of enforcement that can be taken.
  /// Examples are:
  ///      SDSO
  ///      FDSO
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 1, Type = MemberType.Char, Length = KeyAdminAction_MaxLength)]
  public string KeyAdminAction
  {
    get => keyAdminAction ?? "";
    set => keyAdminAction =
      TrimEnd(Substring(value, 1, KeyAdminAction_MaxLength));
  }

  /// <summary>
  /// The json value of the KeyAdminAction attribute.</summary>
  [JsonPropertyName("keyAdminAction")]
  [Computed]
  public string KeyAdminAction_Json
  {
    get => NullIf(KeyAdminAction, "");
    set => KeyAdminAction = value;
  }

  /// <summary>
  /// The value of the KEY_ADMIN_ACTION_CERT attribute.
  /// This is the date the enforcement action is taken for a particular 
  /// Obligation.
  /// </summary>
  [JsonPropertyName("keyAdminActionCert")]
  [Member(Index = 2, Type = MemberType.Date)]
  public DateTime? KeyAdminActionCert
  {
    get => keyAdminActionCert;
    set => keyAdminActionCert = value;
  }

  /// <summary>
  /// The value of the KEY_ADMIN_APPEAL attribute.
  /// This is a system generated number to uniquely identify an Administrative 
  /// Appeal.
  /// </summary>
  [JsonPropertyName("keyAdminAppeal")]
  [DefaultValue(0)]
  [Member(Index = 3, Type = MemberType.Number, Length = 9)]
  public int KeyAdminAppeal
  {
    get => keyAdminAppeal;
    set => keyAdminAppeal = value;
  }

  /// <summary>Length of the KEY_AP attribute.</summary>
  public const int KeyAp_MaxLength = 10;

  /// <summary>
  /// The value of the KEY_AP attribute.
  /// This is a system generated number which will be used by the users to 
  /// identify a person.  This will have a business meaning, but will be unique
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 4, Type = MemberType.Char, Length = KeyAp_MaxLength)]
  public string KeyAp
  {
    get => keyAp ?? "";
    set => keyAp = TrimEnd(Substring(value, 1, KeyAp_MaxLength));
  }

  /// <summary>
  /// The json value of the KeyAp attribute.</summary>
  [JsonPropertyName("keyAp")]
  [Computed]
  public string KeyAp_Json
  {
    get => NullIf(KeyAp, "");
    set => KeyAp = value;
  }

  /// <summary>
  /// The value of the KEY_APPOINTMENT attribute.
  /// Timestamp of creation of the occurrence.
  /// 	
  /// </summary>
  [JsonPropertyName("keyAppointment")]
  [Member(Index = 5, Type = MemberType.Timestamp)]
  public DateTime? KeyAppointment
  {
    get => keyAppointment;
    set => keyAppointment = value;
  }

  /// <summary>Length of the KEY_AR attribute.</summary>
  public const int KeyAr_MaxLength = 10;

  /// <summary>
  /// The value of the KEY_AR attribute.
  /// This is a system generated number which will be used by the users to 
  /// identify a person.  This will have a business meaning, but will be unique
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 6, Type = MemberType.Char, Length = KeyAr_MaxLength)]
  public string KeyAr
  {
    get => keyAr ?? "";
    set => keyAr = TrimEnd(Substring(value, 1, KeyAr_MaxLength));
  }

  /// <summary>
  /// The json value of the KeyAr attribute.</summary>
  [JsonPropertyName("keyAr")]
  [Computed]
  public string KeyAr_Json
  {
    get => NullIf(KeyAr, "");
    set => KeyAr = value;
  }

  /// <summary>
  /// The value of the KEY_BANKRUPTCY attribute.
  /// The attribute, which together with the relation with CSE_PERSON, uniquely 
  /// identifies one occurrence of BANKRUPTCY.
  /// This will be generated by the system, as 1 for the first Bankruptcy 
  /// occurrence for the CSE Person, 2 for the next Bankruptcy occurrence and so
  /// on.
  /// </summary>
  [JsonPropertyName("keyBankruptcy")]
  [DefaultValue(0)]
  [Member(Index = 7, Type = MemberType.Number, Length = 3)]
  public int KeyBankruptcy
  {
    get => keyBankruptcy;
    set => keyBankruptcy = value;
  }

  /// <summary>Length of the KEY_CASE attribute.</summary>
  public const int KeyCase_MaxLength = 10;

  /// <summary>
  /// The value of the KEY_CASE attribute.
  /// A unique number assigned to a request or referral.  Becomes the Case 
  /// number.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 8, Type = MemberType.Char, Length = KeyCase_MaxLength)]
  public string KeyCase
  {
    get => keyCase ?? "";
    set => keyCase = TrimEnd(Substring(value, 1, KeyCase_MaxLength));
  }

  /// <summary>
  /// The json value of the KeyCase attribute.</summary>
  [JsonPropertyName("keyCase")]
  [Computed]
  public string KeyCase_Json
  {
    get => NullIf(KeyCase, "");
    set => KeyCase = value;
  }

  /// <summary>
  /// The value of the KEY_CASH_RCPT_DETAIL attribute.
  /// A unique sequential number within CASH_RECEIPT that distinguishes one 
  /// occurrence of the entity type from another.
  /// </summary>
  [JsonPropertyName("keyCashRcptDetail")]
  [DefaultValue(0)]
  [Member(Index = 9, Type = MemberType.Number, Length = 4)]
  public int KeyCashRcptDetail
  {
    get => keyCashRcptDetail;
    set => keyCashRcptDetail = value;
  }

  /// <summary>
  /// The value of the KEY_CASH_RCPT_EVENT attribute.
  /// A unique sequential number that identifies each negotiable instrument in 
  /// any form of money received by CSE.  Or any information of money received
  /// by CSE.
  /// Examples:
  /// Cash, checks, court transmittals.
  /// </summary>
  [JsonPropertyName("keyCashRcptEvent")]
  [DefaultValue(0)]
  [Member(Index = 10, Type = MemberType.Number, Length = 9)]
  public int KeyCashRcptEvent
  {
    get => keyCashRcptEvent;
    set => keyCashRcptEvent = value;
  }

  /// <summary>
  /// The value of the KEY_CASH_RCPT_SOURCE attribute.
  /// A unique, system generated sequential number that distinguishes one 
  /// occurrence of the entity type from another.
  /// </summary>
  [JsonPropertyName("keyCashRcptSource")]
  [DefaultValue(0)]
  [Member(Index = 11, Type = MemberType.Number, Length = 3)]
  public int KeyCashRcptSource
  {
    get => keyCashRcptSource;
    set => keyCashRcptSource = value;
  }

  /// <summary>
  /// The value of the KEY_CASH_RCPT_TYPE attribute.
  /// A unique, system generated sequential number that distinguishes one 
  /// occurrence of the entity type from another.
  /// </summary>
  [JsonPropertyName("keyCashRcptType")]
  [DefaultValue(0)]
  [Member(Index = 12, Type = MemberType.Number, Length = 3)]
  public int KeyCashRcptType
  {
    get => keyCashRcptType;
    set => keyCashRcptType = value;
  }

  /// <summary>Length of the KEY_CHILD attribute.</summary>
  public const int KeyChild_MaxLength = 10;

  /// <summary>
  /// The value of the KEY_CHILD attribute.
  /// This is a system generated number which will be used by the users to 
  /// identify a person.  This will have a business meaning, but will be unique
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 13, Type = MemberType.Char, Length = KeyChild_MaxLength)]
  public string KeyChild
  {
    get => keyChild ?? "";
    set => keyChild = TrimEnd(Substring(value, 1, KeyChild_MaxLength));
  }

  /// <summary>
  /// The json value of the KeyChild attribute.</summary>
  [JsonPropertyName("keyChild")]
  [Computed]
  public string KeyChild_Json
  {
    get => NullIf(KeyChild, "");
    set => KeyChild = value;
  }

  /// <summary>
  /// The value of the KEY_CONTACT attribute.
  /// Identifier that indicates a particular CSE contact person.
  /// </summary>
  [JsonPropertyName("keyContact")]
  [DefaultValue(0)]
  [Member(Index = 14, Type = MemberType.Number, Length = 2)]
  public int KeyContact
  {
    get => keyContact;
    set => keyContact = value;
  }

  /// <summary>
  /// The value of the KEY_GENETIC_TEST attribute.
  /// A unique identifier of a genetic test.
  /// </summary>
  [JsonPropertyName("keyGeneticTest")]
  [DefaultValue(0)]
  [Member(Index = 15, Type = MemberType.Number, Length = 8)]
  public int KeyGeneticTest
  {
    get => keyGeneticTest;
    set => keyGeneticTest = value;
  }

  /// <summary>
  /// The value of the KEY_HEALTH_INS_COVERAGE attribute.
  /// Artificial attribute to uniquely identify the entity type. This was 
  /// required since Insurance Policy number may not be present sometime. So {
  /// Insurance Company, Policy no} cannot be used as primary identifier.
  /// The identifier will be generated in the form yynnnnnnnn where yy = (2000 
  /// minus year) and nnnnnn is the next number available for the year.
  /// </summary>
  [JsonPropertyName("keyHealthInsCoverage")]
  [DefaultValue(0L)]
  [Member(Index = 16, Type = MemberType.Number, Length = 10)]
  public long KeyHealthInsCoverage
  {
    get => keyHealthInsCoverage;
    set => keyHealthInsCoverage = value;
  }

  /// <summary>
  /// The value of the KEY_INCARCERATION attribute.
  /// The attribute which, together with relation to CSE_PERSON, uniquely 
  /// identifies one instance of INCARCERATION record.
  /// </summary>
  [JsonPropertyName("keyIncarceration")]
  [DefaultValue(0)]
  [Member(Index = 17, Type = MemberType.Number, Length = 2)]
  public int KeyIncarceration
  {
    get => keyIncarceration;
    set => keyIncarceration = value;
  }

  /// <summary>
  /// The value of the KEY_INCOME_SOURCE attribute.
  /// This is a system generated identifier of no business meaning.
  /// </summary>
  [JsonPropertyName("keyIncomeSource")]
  [Member(Index = 18, Type = MemberType.Timestamp)]
  public DateTime? KeyIncomeSource
  {
    get => keyIncomeSource;
    set => keyIncomeSource = value;
  }

  /// <summary>
  /// The value of the KEY_INFO_REQUEST attribute.
  /// A system-generated number used soley to identify inquiries.  Has no innate
  /// significance.
  /// This is also the application number, and is either written on the 
  /// application, or printed on the application in the case of a computer-
  /// generated application.
  /// </summary>
  [JsonPropertyName("keyInfoRequest")]
  [DefaultValue(0L)]
  [Member(Index = 19, Type = MemberType.Number, Length = 10)]
  public long KeyInfoRequest
  {
    get => keyInfoRequest;
    set => keyInfoRequest = value;
  }

  /// <summary>
  /// The value of the KEY_INTERSTATE_REQUEST attribute.
  /// Attribute to uniquely identify an interstate referral associated within a 
  /// case.
  /// This will be a system-generated number.
  /// </summary>
  [JsonPropertyName("keyInterstateRequest")]
  [DefaultValue(0)]
  [Member(Index = 20, Type = MemberType.Number, Length = 8)]
  public int KeyInterstateRequest
  {
    get => keyInterstateRequest;
    set => keyInterstateRequest = value;
  }

  /// <summary>
  /// The value of the KEY_LEGAL_ACTION attribute.
  /// This is a system generated number to uniquely identify a legal action.
  /// </summary>
  [JsonPropertyName("keyLegalAction")]
  [DefaultValue(0)]
  [Member(Index = 21, Type = MemberType.Number, Length = 9)]
  public int KeyLegalAction
  {
    get => keyLegalAction;
    set => keyLegalAction = value;
  }

  /// <summary>
  /// The value of the KEY_LEGAL_ACTION_DETAIL attribute.
  /// A number, unique within the legal action, used to identify each detail of 
  /// the Legal Action.  Starts with one and moves sequentially.
  /// </summary>
  [JsonPropertyName("keyLegalActionDetail")]
  [DefaultValue(0)]
  [Member(Index = 22, Type = MemberType.Number, Length = 2)]
  public int KeyLegalActionDetail
  {
    get => keyLegalActionDetail;
    set => keyLegalActionDetail = value;
  }

  /// <summary>
  /// The value of the KEY_LEGAL_REFERRAL attribute.
  /// Attribute to uniquely identify a legal referral associated within a case.
  /// This will be a system-generated number.
  /// </summary>
  [JsonPropertyName("keyLegalReferral")]
  [DefaultValue(0)]
  [Member(Index = 23, Type = MemberType.Number, Length = 3)]
  public int KeyLegalReferral
  {
    get => keyLegalReferral;
    set => keyLegalReferral = value;
  }

  /// <summary>Length of the KEY_LOCATE_REQUEST_AGENCY attribute.</summary>
  public const int KeyLocateRequestAgency_MaxLength = 5;

  /// <summary>
  /// The value of the KEY_LOCATE_REQUEST_AGENCY attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 24, Type = MemberType.Char, Length
    = KeyLocateRequestAgency_MaxLength)]
  public string KeyLocateRequestAgency
  {
    get => keyLocateRequestAgency ?? "";
    set => keyLocateRequestAgency =
      TrimEnd(Substring(value, 1, KeyLocateRequestAgency_MaxLength));
  }

  /// <summary>
  /// The json value of the KeyLocateRequestAgency attribute.</summary>
  [JsonPropertyName("keyLocateRequestAgency")]
  [Computed]
  public string KeyLocateRequestAgency_Json
  {
    get => NullIf(KeyLocateRequestAgency, "");
    set => KeyLocateRequestAgency = value;
  }

  /// <summary>
  /// The value of the KEY_LOCATE_REQUEST_SOURCE attribute.
  /// </summary>
  [JsonPropertyName("keyLocateRequestSource")]
  [DefaultValue(0)]
  [Member(Index = 25, Type = MemberType.Number, Length = 2)]
  public int KeyLocateRequestSource
  {
    get => keyLocateRequestSource;
    set => keyLocateRequestSource = value;
  }

  /// <summary>
  /// The value of the KEY_MILITARY_SERVICE attribute.
  /// This attribue specifies the date the service in the branch and rank 
  /// started.
  /// </summary>
  [JsonPropertyName("keyMilitaryService")]
  [Member(Index = 26, Type = MemberType.Date)]
  public DateTime? KeyMilitaryService
  {
    get => keyMilitaryService;
    set => keyMilitaryService = value;
  }

  /// <summary>
  /// The value of the KEY_OBLIGATION attribute.
  /// A unique, system generated sequential number (sort descending) that, along
  /// with CSE Person, distinguishes one occurrence of the entity type from
  /// another.
  /// </summary>
  [JsonPropertyName("keyObligation")]
  [DefaultValue(0)]
  [Member(Index = 27, Type = MemberType.Number, Length = 3)]
  public int KeyObligation
  {
    get => keyObligation;
    set => keyObligation = value;
  }

  /// <summary>
  /// The value of the KEY_OBLIGATION_ADMIN_ACTION attribute.
  /// The date that an enforcement action is taken against an Obligor for a 
  /// particular Obligation.
  /// </summary>
  [JsonPropertyName("keyObligationAdminAction")]
  [Member(Index = 28, Type = MemberType.Date)]
  public DateTime? KeyObligationAdminAction
  {
    get => keyObligationAdminAction;
    set => keyObligationAdminAction = value;
  }

  /// <summary>
  /// The value of the KEY_OBLIGATION_TYPE attribute.
  /// A unique, system generated sequential number that distinguishes one 
  /// occurrence of the entity type from another.
  /// </summary>
  [JsonPropertyName("keyObligationType")]
  [DefaultValue(0)]
  [Member(Index = 29, Type = MemberType.Number, Length = 3)]
  public int KeyObligationType
  {
    get => keyObligationType;
    set => keyObligationType = value;
  }

  /// <summary>
  /// The value of the KEY_OFFICE attribute.
  /// </summary>
  [JsonPropertyName("keyOffice")]
  [DefaultValue(0)]
  [Member(Index = 30, Type = MemberType.Number, Length = 4)]
  public int KeyOffice
  {
    get => keyOffice;
    set => keyOffice = value;
  }

  /// <summary>Length of the KEY_PERSON attribute.</summary>
  public const int KeyPerson_MaxLength = 10;

  /// <summary>
  /// The value of the KEY_PERSON attribute.
  /// This is a system generated number which will be used by the users to 
  /// identify a person.  This will have a business meaning, but will be unique
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 31, Type = MemberType.Char, Length = KeyPerson_MaxLength)]
  public string KeyPerson
  {
    get => keyPerson ?? "";
    set => keyPerson = TrimEnd(Substring(value, 1, KeyPerson_MaxLength));
  }

  /// <summary>
  /// The json value of the KeyPerson attribute.</summary>
  [JsonPropertyName("keyPerson")]
  [Computed]
  public string KeyPerson_Json
  {
    get => NullIf(KeyPerson, "");
    set => KeyPerson = value;
  }

  /// <summary>Length of the KEY_PERSON_ACCOUNT attribute.</summary>
  public const int KeyPersonAccount_MaxLength = 1;

  /// <summary>
  /// The value of the KEY_PERSON_ACCOUNT attribute.
  /// Defines the type of account the entity type represents.
  /// Examples:
  /// 	- Obligor
  /// 	- Obligee
  /// 	- Supported Person
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 32, Type = MemberType.Char, Length
    = KeyPersonAccount_MaxLength)]
  [Value("E")]
  [Value("S")]
  [Value("R")]
  [ImplicitValue("S")]
  public string KeyPersonAccount
  {
    get => keyPersonAccount ?? "";
    set => keyPersonAccount =
      TrimEnd(Substring(value, 1, KeyPersonAccount_MaxLength));
  }

  /// <summary>
  /// The json value of the KeyPersonAccount attribute.</summary>
  [JsonPropertyName("keyPersonAccount")]
  [Computed]
  public string KeyPersonAccount_Json
  {
    get => NullIf(KeyPersonAccount, "");
    set => KeyPersonAccount = value;
  }

  /// <summary>
  /// The value of the KEY_PERSON_ADDRESS attribute.
  /// an identifier for an address.  This will be used by the system to ensure 
  /// uniqueness, but will not be displayed or used by the users.  User
  /// inquiries will need to be by relationship to the entity that owns the
  /// address, which will produce a list if more than one address is known for a
  /// given entity.
  /// </summary>
  [JsonPropertyName("keyPersonAddress")]
  [Member(Index = 33, Type = MemberType.Timestamp)]
  public DateTime? KeyPersonAddress
  {
    get => keyPersonAddress;
    set => keyPersonAddress = value;
  }

  /// <summary>
  /// The value of the KEY_RECAPTURE_RULE attribute.
  /// A unique, system generated sequential number that distinguishes one 
  /// occurrence of the entity type from another.
  /// </summary>
  [JsonPropertyName("keyRecaptureRule")]
  [DefaultValue(0)]
  [Member(Index = 34, Type = MemberType.Number, Length = 9)]
  public int KeyRecaptureRule
  {
    get => keyRecaptureRule;
    set => keyRecaptureRule = value;
  }

  /// <summary>
  /// The value of the KEY_RESOURCE attribute.
  /// A running serial number of the resource owned by the CSE Person.
  /// </summary>
  [JsonPropertyName("keyResource")]
  [DefaultValue(0)]
  [Member(Index = 35, Type = MemberType.Number, Length = 3)]
  public int KeyResource
  {
    get => keyResource;
    set => keyResource = value;
  }

  /// <summary>
  /// The value of the KEY_TRIBUNAL attribute.
  /// This attribute uniquely identifies a tribunal record. It is automatically 
  /// generated by the system starting from 1.
  /// </summary>
  [JsonPropertyName("keyTribunal")]
  [DefaultValue(0)]
  [Member(Index = 36, Type = MemberType.Number, Length = 9)]
  public int KeyTribunal
  {
    get => keyTribunal;
    set => keyTribunal = value;
  }

  /// <summary>
  /// The value of the KEY_WORKER_COMP attribute.
  /// This is a system generated identifier of no business meaning.
  /// </summary>
  [JsonPropertyName("keyWorkerComp")]
  [Member(Index = 37, Type = MemberType.Timestamp)]
  public DateTime? KeyWorkerComp
  {
    get => keyWorkerComp;
    set => keyWorkerComp = value;
  }

  /// <summary>
  /// The value of the KEY_WORKSHEET attribute.
  /// Artificial attribute to uniquely identify a record.
  /// </summary>
  [JsonPropertyName("keyWorksheet")]
  [DefaultValue(0L)]
  [Member(Index = 38, Type = MemberType.Number, Length = 10)]
  public long KeyWorksheet
  {
    get => keyWorksheet;
    set => keyWorksheet = value;
  }

  /// <summary>
  /// The value of the KEY_XFER_FROM_DATE attribute.
  /// </summary>
  [JsonPropertyName("keyXferFromDate")]
  [Member(Index = 39, Type = MemberType.Date)]
  public DateTime? KeyXferFromDate
  {
    get => keyXferFromDate;
    set => keyXferFromDate = value;
  }

  /// <summary>
  /// The value of the KEY_XFER_TO_DATE attribute.
  /// </summary>
  [JsonPropertyName("keyXferToDate")]
  [Member(Index = 40, Type = MemberType.Date)]
  public DateTime? KeyXferToDate
  {
    get => keyXferToDate;
    set => keyXferToDate = value;
  }

  private string keyAdminAction;
  private DateTime? keyAdminActionCert;
  private int keyAdminAppeal;
  private string keyAp;
  private DateTime? keyAppointment;
  private string keyAr;
  private int keyBankruptcy;
  private string keyCase;
  private int keyCashRcptDetail;
  private int keyCashRcptEvent;
  private int keyCashRcptSource;
  private int keyCashRcptType;
  private string keyChild;
  private int keyContact;
  private int keyGeneticTest;
  private long keyHealthInsCoverage;
  private int keyIncarceration;
  private DateTime? keyIncomeSource;
  private long keyInfoRequest;
  private int keyInterstateRequest;
  private int keyLegalAction;
  private int keyLegalActionDetail;
  private int keyLegalReferral;
  private string keyLocateRequestAgency;
  private int keyLocateRequestSource;
  private DateTime? keyMilitaryService;
  private int keyObligation;
  private DateTime? keyObligationAdminAction;
  private int keyObligationType;
  private int keyOffice;
  private string keyPerson;
  private string keyPersonAccount;
  private DateTime? keyPersonAddress;
  private int keyRecaptureRule;
  private int keyResource;
  private int keyTribunal;
  private DateTime? keyWorkerComp;
  private long keyWorksheet;
  private DateTime? keyXferFromDate;
  private DateTime? keyXferToDate;
}
