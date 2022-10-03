// The source file: SP_DOC_LITERAL, ID: 371822181, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Worksets;

[Serializable]
public partial class SpDocLiteral: Heap, ICloneable
{
  /// <summary>Default constructor.</summary>
  public SpDocLiteral()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public SpDocLiteral(SpDocLiteral that):
    base(that)
  {
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new SpDocLiteral Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Length of the ID_ADMIN_ACT_CERT attribute.</summary>
  public const int IdAdminActCert_MaxLength = 10;

  /// <summary>
  /// The value of the ID_ADMIN_ACT_CERT attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 1, Type = MemberType.Char, Length = IdAdminActCert_MaxLength)]
  public string IdAdminActCert
  {
    get => Get<string>("idAdminActCert") ?? "";
    set => Set(
      "idAdminActCert", IsEmpty(value) ? null : TrimEnd
      (Substring(value, 1, IdAdminActCert_MaxLength)));
  }

  /// <summary>
  /// The json value of the IdAdminActCert attribute.</summary>
  [JsonPropertyName("idAdminActCert")]
  [Computed]
  public string IdAdminActCert_Json
  {
    get => NullIf(IdAdminActCert, "");
    set => IdAdminActCert = value;
  }

  /// <summary>Length of the ID_ADMIN_ACTION attribute.</summary>
  public const int IdAdminAction_MaxLength = 10;

  /// <summary>
  /// The value of the ID_ADMIN_ACTION attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 2, Type = MemberType.Char, Length = IdAdminAction_MaxLength)]
  public string IdAdminAction
  {
    get => Get<string>("idAdminAction") ?? "";
    set => Set(
      "idAdminAction", IsEmpty(value) ? null : TrimEnd
      (Substring(value, 1, IdAdminAction_MaxLength)));
  }

  /// <summary>
  /// The json value of the IdAdminAction attribute.</summary>
  [JsonPropertyName("idAdminAction")]
  [Computed]
  public string IdAdminAction_Json
  {
    get => NullIf(IdAdminAction, "");
    set => IdAdminAction = value;
  }

  /// <summary>Length of the ID_APPOINTMENT attribute.</summary>
  public const int IdAppointment_MaxLength = 10;

  /// <summary>
  /// The value of the ID_APPOINTMENT attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 3, Type = MemberType.Char, Length = IdAppointment_MaxLength)]
  public string IdAppointment
  {
    get => Get<string>("idAppointment") ?? "";
    set => Set(
      "idAppointment", IsEmpty(value) ? null : TrimEnd
      (Substring(value, 1, IdAppointment_MaxLength)));
  }

  /// <summary>
  /// The json value of the IdAppointment attribute.</summary>
  [JsonPropertyName("idAppointment")]
  [Computed]
  public string IdAppointment_Json
  {
    get => NullIf(IdAppointment, "");
    set => IdAppointment = value;
  }

  /// <summary>Length of the ID_BANKRUPTCY attribute.</summary>
  public const int IdBankruptcy_MaxLength = 10;

  /// <summary>
  /// The value of the ID_BANKRUPTCY attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 4, Type = MemberType.Char, Length = IdBankruptcy_MaxLength)]
  public string IdBankruptcy
  {
    get => Get<string>("idBankruptcy") ?? "";
    set => Set(
      "idBankruptcy", IsEmpty(value) ? null : TrimEnd
      (Substring(value, 1, IdBankruptcy_MaxLength)));
  }

  /// <summary>
  /// The json value of the IdBankruptcy attribute.</summary>
  [JsonPropertyName("idBankruptcy")]
  [Computed]
  public string IdBankruptcy_Json
  {
    get => NullIf(IdBankruptcy, "");
    set => IdBankruptcy = value;
  }

  /// <summary>Length of the ID_CASH_RCPT_DETAIL attribute.</summary>
  public const int IdCashRcptDetail_MaxLength = 10;

  /// <summary>
  /// The value of the ID_CASH_RCPT_DETAIL attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 5, Type = MemberType.Char, Length = IdCashRcptDetail_MaxLength)
    ]
  public string IdCashRcptDetail
  {
    get => Get<string>("idCashRcptDetail") ?? "";
    set => Set(
      "idCashRcptDetail", IsEmpty(value) ? null : TrimEnd
      (Substring(value, 1, IdCashRcptDetail_MaxLength)));
  }

  /// <summary>
  /// The json value of the IdCashRcptDetail attribute.</summary>
  [JsonPropertyName("idCashRcptDetail")]
  [Computed]
  public string IdCashRcptDetail_Json
  {
    get => NullIf(IdCashRcptDetail, "");
    set => IdCashRcptDetail = value;
  }

  /// <summary>Length of the ID_CASH_RCPT_EVENT attribute.</summary>
  public const int IdCashRcptEvent_MaxLength = 10;

  /// <summary>
  /// The value of the ID_CASH_RCPT_EVENT attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 6, Type = MemberType.Char, Length = IdCashRcptEvent_MaxLength)]
    
  public string IdCashRcptEvent
  {
    get => Get<string>("idCashRcptEvent") ?? "";
    set => Set(
      "idCashRcptEvent", IsEmpty(value) ? null : TrimEnd
      (Substring(value, 1, IdCashRcptEvent_MaxLength)));
  }

  /// <summary>
  /// The json value of the IdCashRcptEvent attribute.</summary>
  [JsonPropertyName("idCashRcptEvent")]
  [Computed]
  public string IdCashRcptEvent_Json
  {
    get => NullIf(IdCashRcptEvent, "");
    set => IdCashRcptEvent = value;
  }

  /// <summary>Length of the ID_CASH_RCPT_SOURCE attribute.</summary>
  public const int IdCashRcptSource_MaxLength = 10;

  /// <summary>
  /// The value of the ID_CASH_RCPT_SOURCE attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 7, Type = MemberType.Char, Length = IdCashRcptSource_MaxLength)
    ]
  public string IdCashRcptSource
  {
    get => Get<string>("idCashRcptSource") ?? "";
    set => Set(
      "idCashRcptSource", IsEmpty(value) ? null : TrimEnd
      (Substring(value, 1, IdCashRcptSource_MaxLength)));
  }

  /// <summary>
  /// The json value of the IdCashRcptSource attribute.</summary>
  [JsonPropertyName("idCashRcptSource")]
  [Computed]
  public string IdCashRcptSource_Json
  {
    get => NullIf(IdCashRcptSource, "");
    set => IdCashRcptSource = value;
  }

  /// <summary>Length of the ID_CASH_RCPT_TYPE attribute.</summary>
  public const int IdCashRcptType_MaxLength = 10;

  /// <summary>
  /// The value of the ID_CASH_RCPT_TYPE attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 8, Type = MemberType.Char, Length = IdCashRcptType_MaxLength)]
  public string IdCashRcptType
  {
    get => Get<string>("idCashRcptType") ?? "";
    set => Set(
      "idCashRcptType", IsEmpty(value) ? null : TrimEnd
      (Substring(value, 1, IdCashRcptType_MaxLength)));
  }

  /// <summary>
  /// The json value of the IdCashRcptType attribute.</summary>
  [JsonPropertyName("idCashRcptType")]
  [Computed]
  public string IdCashRcptType_Json
  {
    get => NullIf(IdCashRcptType, "");
    set => IdCashRcptType = value;
  }

  /// <summary>Length of the ID_CONTACT attribute.</summary>
  public const int IdContact_MaxLength = 10;

  /// <summary>
  /// The value of the ID_CONTACT attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 9, Type = MemberType.Char, Length = IdContact_MaxLength)]
  public string IdContact
  {
    get => Get<string>("idContact") ?? "";
    set => Set(
      "idContact", IsEmpty(value) ? null : TrimEnd
      (Substring(value, 1, IdContact_MaxLength)));
  }

  /// <summary>
  /// The json value of the IdContact attribute.</summary>
  [JsonPropertyName("idContact")]
  [Computed]
  public string IdContact_Json
  {
    get => NullIf(IdContact, "");
    set => IdContact = value;
  }

  /// <summary>Length of the ID_CH_NUMBER attribute.</summary>
  public const int IdChNumber_MaxLength = 10;

  /// <summary>
  /// The value of the ID_CH_NUMBER attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 10, Type = MemberType.Char, Length = IdChNumber_MaxLength)]
  public string IdChNumber
  {
    get => Get<string>("idChNumber") ?? "";
    set => Set(
      "idChNumber", IsEmpty(value) ? null : TrimEnd
      (Substring(value, 1, IdChNumber_MaxLength)));
  }

  /// <summary>
  /// The json value of the IdChNumber attribute.</summary>
  [JsonPropertyName("idChNumber")]
  [Computed]
  public string IdChNumber_Json
  {
    get => NullIf(IdChNumber, "");
    set => IdChNumber = value;
  }

  /// <summary>Length of the ID_DOCUMENT attribute.</summary>
  public const int IdDocument_MaxLength = 10;

  /// <summary>
  /// The value of the ID_DOCUMENT attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 11, Type = MemberType.Char, Length = IdDocument_MaxLength)]
  public string IdDocument
  {
    get => Get<string>("idDocument") ?? "";
    set => Set(
      "idDocument", IsEmpty(value) ? null : TrimEnd
      (Substring(value, 1, IdDocument_MaxLength)));
  }

  /// <summary>
  /// The json value of the IdDocument attribute.</summary>
  [JsonPropertyName("idDocument")]
  [Computed]
  public string IdDocument_Json
  {
    get => NullIf(IdDocument, "");
    set => IdDocument = value;
  }

  /// <summary>Length of the ID_GENETIC attribute.</summary>
  public const int IdGenetic_MaxLength = 10;

  /// <summary>
  /// The value of the ID_GENETIC attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 12, Type = MemberType.Char, Length = IdGenetic_MaxLength)]
  public string IdGenetic
  {
    get => Get<string>("idGenetic") ?? "";
    set => Set(
      "idGenetic", IsEmpty(value) ? null : TrimEnd
      (Substring(value, 1, IdGenetic_MaxLength)));
  }

  /// <summary>
  /// The json value of the IdGenetic attribute.</summary>
  [JsonPropertyName("idGenetic")]
  [Computed]
  public string IdGenetic_Json
  {
    get => NullIf(IdGenetic, "");
    set => IdGenetic = value;
  }

  /// <summary>Length of the ID_HEALTH_INS_COVERAGE attribute.</summary>
  public const int IdHealthInsCoverage_MaxLength = 10;

  /// <summary>
  /// The value of the ID_HEALTH_INS_COVERAGE attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 13, Type = MemberType.Char, Length
    = IdHealthInsCoverage_MaxLength)]
  public string IdHealthInsCoverage
  {
    get => Get<string>("idHealthInsCoverage") ?? "";
    set => Set(
      "idHealthInsCoverage", IsEmpty(value) ? null : TrimEnd
      (Substring(value, 1, IdHealthInsCoverage_MaxLength)));
  }

  /// <summary>
  /// The json value of the IdHealthInsCoverage attribute.</summary>
  [JsonPropertyName("idHealthInsCoverage")]
  [Computed]
  public string IdHealthInsCoverage_Json
  {
    get => NullIf(IdHealthInsCoverage, "");
    set => IdHealthInsCoverage = value;
  }

  /// <summary>Length of the ID_INCOME_SOURCE attribute.</summary>
  public const int IdIncomeSource_MaxLength = 10;

  /// <summary>
  /// The value of the ID_INCOME_SOURCE attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 14, Type = MemberType.Char, Length = IdIncomeSource_MaxLength)]
    
  public string IdIncomeSource
  {
    get => Get<string>("idIncomeSource") ?? "";
    set => Set(
      "idIncomeSource", IsEmpty(value) ? null : TrimEnd
      (Substring(value, 1, IdIncomeSource_MaxLength)));
  }

  /// <summary>
  /// The json value of the IdIncomeSource attribute.</summary>
  [JsonPropertyName("idIncomeSource")]
  [Computed]
  public string IdIncomeSource_Json
  {
    get => NullIf(IdIncomeSource, "");
    set => IdIncomeSource = value;
  }

  /// <summary>Length of the ID_INFO_REQUEST attribute.</summary>
  public const int IdInfoRequest_MaxLength = 10;

  /// <summary>
  /// The value of the ID_INFO_REQUEST attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 15, Type = MemberType.Char, Length = IdInfoRequest_MaxLength)]
  public string IdInfoRequest
  {
    get => Get<string>("idInfoRequest") ?? "";
    set => Set(
      "idInfoRequest", IsEmpty(value) ? null : TrimEnd
      (Substring(value, 1, IdInfoRequest_MaxLength)));
  }

  /// <summary>
  /// The json value of the IdInfoRequest attribute.</summary>
  [JsonPropertyName("idInfoRequest")]
  [Computed]
  public string IdInfoRequest_Json
  {
    get => NullIf(IdInfoRequest, "");
    set => IdInfoRequest = value;
  }

  /// <summary>Length of the ID_INTERSTATE_REQUEST attribute.</summary>
  public const int IdInterstateRequest_MaxLength = 10;

  /// <summary>
  /// The value of the ID_INTERSTATE_REQUEST attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 16, Type = MemberType.Char, Length
    = IdInterstateRequest_MaxLength)]
  public string IdInterstateRequest
  {
    get => Get<string>("idInterstateRequest") ?? "";
    set => Set(
      "idInterstateRequest", IsEmpty(value) ? null : TrimEnd
      (Substring(value, 1, IdInterstateRequest_MaxLength)));
  }

  /// <summary>
  /// The json value of the IdInterstateRequest attribute.</summary>
  [JsonPropertyName("idInterstateRequest")]
  [Computed]
  public string IdInterstateRequest_Json
  {
    get => NullIf(IdInterstateRequest, "");
    set => IdInterstateRequest = value;
  }

  /// <summary>Length of the ID_JAIL attribute.</summary>
  public const int IdJail_MaxLength = 10;

  /// <summary>
  /// The value of the ID_JAIL attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 17, Type = MemberType.Char, Length = IdJail_MaxLength)]
  public string IdJail
  {
    get => Get<string>("idJail") ?? "";
    set => Set(
      "idJail", IsEmpty(value) ? null : TrimEnd
      (Substring(value, 1, IdJail_MaxLength)));
  }

  /// <summary>
  /// The json value of the IdJail attribute.</summary>
  [JsonPropertyName("idJail")]
  [Computed]
  public string IdJail_Json
  {
    get => NullIf(IdJail, "");
    set => IdJail = value;
  }

  /// <summary>Length of the ID_LEGAL_ACTION_DETAIL attribute.</summary>
  public const int IdLegalActionDetail_MaxLength = 10;

  /// <summary>
  /// The value of the ID_LEGAL_ACTION_DETAIL attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 18, Type = MemberType.Char, Length
    = IdLegalActionDetail_MaxLength)]
  public string IdLegalActionDetail
  {
    get => Get<string>("idLegalActionDetail") ?? "";
    set => Set(
      "idLegalActionDetail", IsEmpty(value) ? null : TrimEnd
      (Substring(value, 1, IdLegalActionDetail_MaxLength)));
  }

  /// <summary>
  /// The json value of the IdLegalActionDetail attribute.</summary>
  [JsonPropertyName("idLegalActionDetail")]
  [Computed]
  public string IdLegalActionDetail_Json
  {
    get => NullIf(IdLegalActionDetail, "");
    set => IdLegalActionDetail = value;
  }

  /// <summary>Length of the ID_LEGAL_REFERRAL attribute.</summary>
  public const int IdLegalReferral_MaxLength = 10;

  /// <summary>
  /// The value of the ID_LEGAL_REFERRAL attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 19, Type = MemberType.Char, Length = IdLegalReferral_MaxLength)
    ]
  public string IdLegalReferral
  {
    get => Get<string>("idLegalReferral") ?? "";
    set => Set(
      "idLegalReferral", IsEmpty(value) ? null : TrimEnd
      (Substring(value, 1, IdLegalReferral_MaxLength)));
  }

  /// <summary>
  /// The json value of the IdLegalReferral attribute.</summary>
  [JsonPropertyName("idLegalReferral")]
  [Computed]
  public string IdLegalReferral_Json
  {
    get => NullIf(IdLegalReferral, "");
    set => IdLegalReferral = value;
  }

  /// <summary>Length of the ID_LOCATE_REQUEST_AGENCY attribute.</summary>
  public const int IdLocateRequestAgency_MaxLength = 10;

  /// <summary>
  /// The value of the ID_LOCATE_REQUEST_AGENCY attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 20, Type = MemberType.Char, Length
    = IdLocateRequestAgency_MaxLength)]
  public string IdLocateRequestAgency
  {
    get => Get<string>("idLocateRequestAgency") ?? "";
    set => Set(
      "idLocateRequestAgency", IsEmpty(value) ? null : TrimEnd
      (Substring(value, 1, IdLocateRequestAgency_MaxLength)));
  }

  /// <summary>
  /// The json value of the IdLocateRequestAgency attribute.</summary>
  [JsonPropertyName("idLocateRequestAgency")]
  [Computed]
  public string IdLocateRequestAgency_Json
  {
    get => NullIf(IdLocateRequestAgency, "");
    set => IdLocateRequestAgency = value;
  }

  /// <summary>Length of the ID_LOCATE_REQUEST_SOURCE attribute.</summary>
  public const int IdLocateRequestSource_MaxLength = 10;

  /// <summary>
  /// The value of the ID_LOCATE_REQUEST_SOURCE attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 21, Type = MemberType.Char, Length
    = IdLocateRequestSource_MaxLength)]
  public string IdLocateRequestSource
  {
    get => Get<string>("idLocateRequestSource") ?? "";
    set => Set(
      "idLocateRequestSource", IsEmpty(value) ? null : TrimEnd
      (Substring(value, 1, IdLocateRequestSource_MaxLength)));
  }

  /// <summary>
  /// The json value of the IdLocateRequestSource attribute.</summary>
  [JsonPropertyName("idLocateRequestSource")]
  [Computed]
  public string IdLocateRequestSource_Json
  {
    get => NullIf(IdLocateRequestSource, "");
    set => IdLocateRequestSource = value;
  }

  /// <summary>Length of the ID_MILITARY attribute.</summary>
  public const int IdMilitary_MaxLength = 10;

  /// <summary>
  /// The value of the ID_MILITARY attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 22, Type = MemberType.Char, Length = IdMilitary_MaxLength)]
  public string IdMilitary
  {
    get => Get<string>("idMilitary") ?? "";
    set => Set(
      "idMilitary", IsEmpty(value) ? null : TrimEnd
      (Substring(value, 1, IdMilitary_MaxLength)));
  }

  /// <summary>
  /// The json value of the IdMilitary attribute.</summary>
  [JsonPropertyName("idMilitary")]
  [Computed]
  public string IdMilitary_Json
  {
    get => NullIf(IdMilitary, "");
    set => IdMilitary = value;
  }

  /// <summary>Length of the ID_OBLIGATION_ADMIN_ACTION attribute.</summary>
  public const int IdObligationAdminAction_MaxLength = 10;

  /// <summary>
  /// The value of the ID_OBLIGATION_ADMIN_ACTION attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 23, Type = MemberType.Char, Length
    = IdObligationAdminAction_MaxLength)]
  public string IdObligationAdminAction
  {
    get => Get<string>("idObligationAdminAction") ?? "";
    set => Set(
      "idObligationAdminAction", IsEmpty(value) ? null : TrimEnd
      (Substring(value, 1, IdObligationAdminAction_MaxLength)));
  }

  /// <summary>
  /// The json value of the IdObligationAdminAction attribute.</summary>
  [JsonPropertyName("idObligationAdminAction")]
  [Computed]
  public string IdObligationAdminAction_Json
  {
    get => NullIf(IdObligationAdminAction, "");
    set => IdObligationAdminAction = value;
  }

  /// <summary>Length of the ID_OBLIGATION_TYPE attribute.</summary>
  public const int IdObligationType_MaxLength = 10;

  /// <summary>
  /// The value of the ID_OBLIGATION_TYPE attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 24, Type = MemberType.Char, Length
    = IdObligationType_MaxLength)]
  public string IdObligationType
  {
    get => Get<string>("idObligationType") ?? "";
    set => Set(
      "idObligationType", IsEmpty(value) ? null : TrimEnd
      (Substring(value, 1, IdObligationType_MaxLength)));
  }

  /// <summary>
  /// The json value of the IdObligationType attribute.</summary>
  [JsonPropertyName("idObligationType")]
  [Computed]
  public string IdObligationType_Json
  {
    get => NullIf(IdObligationType, "");
    set => IdObligationType = value;
  }

  /// <summary>Length of the ID_PERSON_ACCT attribute.</summary>
  public const int IdPersonAcct_MaxLength = 10;

  /// <summary>
  /// The value of the ID_PERSON_ACCT attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 25, Type = MemberType.Char, Length = IdPersonAcct_MaxLength)]
  public string IdPersonAcct
  {
    get => Get<string>("idPersonAcct") ?? "";
    set => Set(
      "idPersonAcct", IsEmpty(value) ? null : TrimEnd
      (Substring(value, 1, IdPersonAcct_MaxLength)));
  }

  /// <summary>
  /// The json value of the IdPersonAcct attribute.</summary>
  [JsonPropertyName("idPersonAcct")]
  [Computed]
  public string IdPersonAcct_Json
  {
    get => NullIf(IdPersonAcct, "");
    set => IdPersonAcct = value;
  }

  /// <summary>Length of the ID_PERSON_ADDRESS attribute.</summary>
  public const int IdPersonAddress_MaxLength = 10;

  /// <summary>
  /// The value of the ID_PERSON_ADDRESS attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 26, Type = MemberType.Char, Length = IdPersonAddress_MaxLength)
    ]
  public string IdPersonAddress
  {
    get => Get<string>("idPersonAddress") ?? "";
    set => Set(
      "idPersonAddress", IsEmpty(value) ? null : TrimEnd
      (Substring(value, 1, IdPersonAddress_MaxLength)));
  }

  /// <summary>
  /// The json value of the IdPersonAddress attribute.</summary>
  [JsonPropertyName("idPersonAddress")]
  [Computed]
  public string IdPersonAddress_Json
  {
    get => NullIf(IdPersonAddress, "");
    set => IdPersonAddress = value;
  }

  /// <summary>Length of the ID_PR_NUMBER attribute.</summary>
  public const int IdPrNumber_MaxLength = 10;

  /// <summary>
  /// The value of the ID_PR_NUMBER attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 27, Type = MemberType.Char, Length = IdPrNumber_MaxLength)]
  public string IdPrNumber
  {
    get => Get<string>("idPrNumber") ?? "";
    set => Set(
      "idPrNumber", IsEmpty(value) ? null : TrimEnd
      (Substring(value, 1, IdPrNumber_MaxLength)));
  }

  /// <summary>
  /// The json value of the IdPrNumber attribute.</summary>
  [JsonPropertyName("idPrNumber")]
  [Computed]
  public string IdPrNumber_Json
  {
    get => NullIf(IdPrNumber, "");
    set => IdPrNumber = value;
  }

  /// <summary>Length of the ID_RECAPTURE_RULE attribute.</summary>
  public const int IdRecaptureRule_MaxLength = 10;

  /// <summary>
  /// The value of the ID_RECAPTURE_RULE attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 28, Type = MemberType.Char, Length = IdRecaptureRule_MaxLength)
    ]
  public string IdRecaptureRule
  {
    get => Get<string>("idRecaptureRule") ?? "";
    set => Set(
      "idRecaptureRule", IsEmpty(value) ? null : TrimEnd
      (Substring(value, 1, IdRecaptureRule_MaxLength)));
  }

  /// <summary>
  /// The json value of the IdRecaptureRule attribute.</summary>
  [JsonPropertyName("idRecaptureRule")]
  [Computed]
  public string IdRecaptureRule_Json
  {
    get => NullIf(IdRecaptureRule, "");
    set => IdRecaptureRule = value;
  }

  /// <summary>Length of the ID_RESOURCE attribute.</summary>
  public const int IdResource_MaxLength = 10;

  /// <summary>
  /// The value of the ID_RESOURCE attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 29, Type = MemberType.Char, Length = IdResource_MaxLength)]
  public string IdResource
  {
    get => Get<string>("idResource") ?? "";
    set => Set(
      "idResource", IsEmpty(value) ? null : TrimEnd
      (Substring(value, 1, IdResource_MaxLength)));
  }

  /// <summary>
  /// The json value of the IdResource attribute.</summary>
  [JsonPropertyName("idResource")]
  [Computed]
  public string IdResource_Json
  {
    get => NullIf(IdResource, "");
    set => IdResource = value;
  }

  /// <summary>Length of the ID_TRIBUNAL attribute.</summary>
  public const int IdTribunal_MaxLength = 10;

  /// <summary>
  /// The value of the ID_TRIBUNAL attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 30, Type = MemberType.Char, Length = IdTribunal_MaxLength)]
  public string IdTribunal
  {
    get => Get<string>("idTribunal") ?? "";
    set => Set(
      "idTribunal", IsEmpty(value) ? null : TrimEnd
      (Substring(value, 1, IdTribunal_MaxLength)));
  }

  /// <summary>
  /// The json value of the IdTribunal attribute.</summary>
  [JsonPropertyName("idTribunal")]
  [Computed]
  public string IdTribunal_Json
  {
    get => NullIf(IdTribunal, "");
    set => IdTribunal = value;
  }

  /// <summary>Length of the ID_WORKER_COMP attribute.</summary>
  public const int IdWorkerComp_MaxLength = 10;

  /// <summary>
  /// The value of the ID_WORKER_COMP attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 31, Type = MemberType.Char, Length = IdWorkerComp_MaxLength)]
  public string IdWorkerComp
  {
    get => Get<string>("idWorkerComp") ?? "";
    set => Set(
      "idWorkerComp", IsEmpty(value) ? null : TrimEnd
      (Substring(value, 1, IdWorkerComp_MaxLength)));
  }

  /// <summary>
  /// The json value of the IdWorkerComp attribute.</summary>
  [JsonPropertyName("idWorkerComp")]
  [Computed]
  public string IdWorkerComp_Json
  {
    get => NullIf(IdWorkerComp, "");
    set => IdWorkerComp = value;
  }

  /// <summary>Length of the ID_WORKSHEET attribute.</summary>
  public const int IdWorksheet_MaxLength = 10;

  /// <summary>
  /// The value of the ID_WORKSHEET attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 32, Type = MemberType.Char, Length = IdWorksheet_MaxLength)]
  public string IdWorksheet
  {
    get => Get<string>("idWorksheet") ?? "";
    set => Set(
      "idWorksheet", IsEmpty(value) ? null : TrimEnd
      (Substring(value, 1, IdWorksheet_MaxLength)));
  }

  /// <summary>
  /// The json value of the IdWorksheet attribute.</summary>
  [JsonPropertyName("idWorksheet")]
  [Computed]
  public string IdWorksheet_Json
  {
    get => NullIf(IdWorksheet, "");
    set => IdWorksheet = value;
  }

  /// <summary>Length of the SCREEN_ADMIN_ACT_CERT attribute.</summary>
  public const int ScreenAdminActCert_MaxLength = 33;

  /// <summary>
  /// The value of the SCREEN_ADMIN_ACT_CERT attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 33, Type = MemberType.Char, Length
    = ScreenAdminActCert_MaxLength)]
  public string ScreenAdminActCert
  {
    get => Get<string>("screenAdminActCert") ?? "";
    set => Set(
      "screenAdminActCert", IsEmpty(value) ? null : TrimEnd
      (Substring(value, 1, ScreenAdminActCert_MaxLength)));
  }

  /// <summary>
  /// The json value of the ScreenAdminActCert attribute.</summary>
  [JsonPropertyName("screenAdminActCert")]
  [Computed]
  public string ScreenAdminActCert_Json
  {
    get => NullIf(ScreenAdminActCert, "");
    set => ScreenAdminActCert = value;
  }

  /// <summary>Length of the SCREEN_ADMIN_ACTION attribute.</summary>
  public const int ScreenAdminAction_MaxLength = 33;

  /// <summary>
  /// The value of the SCREEN_ADMIN_ACTION attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 34, Type = MemberType.Char, Length
    = ScreenAdminAction_MaxLength)]
  public string ScreenAdminAction
  {
    get => Get<string>("screenAdminAction") ?? "";
    set => Set(
      "screenAdminAction", IsEmpty(value) ? null : TrimEnd
      (Substring(value, 1, ScreenAdminAction_MaxLength)));
  }

  /// <summary>
  /// The json value of the ScreenAdminAction attribute.</summary>
  [JsonPropertyName("screenAdminAction")]
  [Computed]
  public string ScreenAdminAction_Json
  {
    get => NullIf(ScreenAdminAction, "");
    set => ScreenAdminAction = value;
  }

  /// <summary>Length of the SCREEN_ADMIN_APPEAL attribute.</summary>
  public const int ScreenAdminAppeal_MaxLength = 33;

  /// <summary>
  /// The value of the SCREEN_ADMIN_APPEAL attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 35, Type = MemberType.Char, Length
    = ScreenAdminAppeal_MaxLength)]
  public string ScreenAdminAppeal
  {
    get => Get<string>("screenAdminAppeal") ?? "";
    set => Set(
      "screenAdminAppeal", IsEmpty(value) ? null : TrimEnd
      (Substring(value, 1, ScreenAdminAppeal_MaxLength)));
  }

  /// <summary>
  /// The json value of the ScreenAdminAppeal attribute.</summary>
  [JsonPropertyName("screenAdminAppeal")]
  [Computed]
  public string ScreenAdminAppeal_Json
  {
    get => NullIf(ScreenAdminAppeal, "");
    set => ScreenAdminAppeal = value;
  }

  /// <summary>Length of the SCREEN_AP_NUMBER attribute.</summary>
  public const int ScreenApNumber_MaxLength = 33;

  /// <summary>
  /// The value of the SCREEN_AP_NUMBER attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 36, Type = MemberType.Char, Length = ScreenApNumber_MaxLength)]
    
  public string ScreenApNumber
  {
    get => Get<string>("screenApNumber") ?? "";
    set => Set(
      "screenApNumber", IsEmpty(value) ? null : TrimEnd
      (Substring(value, 1, ScreenApNumber_MaxLength)));
  }

  /// <summary>
  /// The json value of the ScreenApNumber attribute.</summary>
  [JsonPropertyName("screenApNumber")]
  [Computed]
  public string ScreenApNumber_Json
  {
    get => NullIf(ScreenApNumber, "");
    set => ScreenApNumber = value;
  }

  /// <summary>Length of the SCREEN_APPOINTMENT attribute.</summary>
  public const int ScreenAppointment_MaxLength = 33;

  /// <summary>
  /// The value of the SCREEN_APPOINTMENT attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 37, Type = MemberType.Char, Length
    = ScreenAppointment_MaxLength)]
  public string ScreenAppointment
  {
    get => Get<string>("screenAppointment") ?? "";
    set => Set(
      "screenAppointment", IsEmpty(value) ? null : TrimEnd
      (Substring(value, 1, ScreenAppointment_MaxLength)));
  }

  /// <summary>
  /// The json value of the ScreenAppointment attribute.</summary>
  [JsonPropertyName("screenAppointment")]
  [Computed]
  public string ScreenAppointment_Json
  {
    get => NullIf(ScreenAppointment, "");
    set => ScreenAppointment = value;
  }

  /// <summary>Length of the SCREEN_AR_NUMBER attribute.</summary>
  public const int ScreenArNumber_MaxLength = 33;

  /// <summary>
  /// The value of the SCREEN_AR_NUMBER attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 38, Type = MemberType.Char, Length = ScreenArNumber_MaxLength)]
    
  public string ScreenArNumber
  {
    get => Get<string>("screenArNumber") ?? "";
    set => Set(
      "screenArNumber", IsEmpty(value) ? null : TrimEnd
      (Substring(value, 1, ScreenArNumber_MaxLength)));
  }

  /// <summary>
  /// The json value of the ScreenArNumber attribute.</summary>
  [JsonPropertyName("screenArNumber")]
  [Computed]
  public string ScreenArNumber_Json
  {
    get => NullIf(ScreenArNumber, "");
    set => ScreenArNumber = value;
  }

  /// <summary>Length of the SCREEN_BANKRUPTCY attribute.</summary>
  public const int ScreenBankruptcy_MaxLength = 33;

  /// <summary>
  /// The value of the SCREEN_BANKRUPTCY attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 39, Type = MemberType.Char, Length
    = ScreenBankruptcy_MaxLength)]
  public string ScreenBankruptcy
  {
    get => Get<string>("screenBankruptcy") ?? "";
    set => Set(
      "screenBankruptcy", IsEmpty(value) ? null : TrimEnd
      (Substring(value, 1, ScreenBankruptcy_MaxLength)));
  }

  /// <summary>
  /// The json value of the ScreenBankruptcy attribute.</summary>
  [JsonPropertyName("screenBankruptcy")]
  [Computed]
  public string ScreenBankruptcy_Json
  {
    get => NullIf(ScreenBankruptcy, "");
    set => ScreenBankruptcy = value;
  }

  /// <summary>Length of the SCREEN_CASE_NUMBER attribute.</summary>
  public const int ScreenCaseNumber_MaxLength = 33;

  /// <summary>
  /// The value of the SCREEN_CASE_NUMBER attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 40, Type = MemberType.Char, Length
    = ScreenCaseNumber_MaxLength)]
  public string ScreenCaseNumber
  {
    get => Get<string>("screenCaseNumber") ?? "";
    set => Set(
      "screenCaseNumber", IsEmpty(value) ? null : TrimEnd
      (Substring(value, 1, ScreenCaseNumber_MaxLength)));
  }

  /// <summary>
  /// The json value of the ScreenCaseNumber attribute.</summary>
  [JsonPropertyName("screenCaseNumber")]
  [Computed]
  public string ScreenCaseNumber_Json
  {
    get => NullIf(ScreenCaseNumber, "");
    set => ScreenCaseNumber = value;
  }

  /// <summary>Length of the SCREEN_CASH_RCPT_DETAIL attribute.</summary>
  public const int ScreenCashRcptDetail_MaxLength = 33;

  /// <summary>
  /// The value of the SCREEN_CASH_RCPT_DETAIL attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 41, Type = MemberType.Char, Length
    = ScreenCashRcptDetail_MaxLength)]
  public string ScreenCashRcptDetail
  {
    get => Get<string>("screenCashRcptDetail") ?? "";
    set => Set(
      "screenCashRcptDetail", IsEmpty(value) ? null : TrimEnd
      (Substring(value, 1, ScreenCashRcptDetail_MaxLength)));
  }

  /// <summary>
  /// The json value of the ScreenCashRcptDetail attribute.</summary>
  [JsonPropertyName("screenCashRcptDetail")]
  [Computed]
  public string ScreenCashRcptDetail_Json
  {
    get => NullIf(ScreenCashRcptDetail, "");
    set => ScreenCashRcptDetail = value;
  }

  /// <summary>Length of the SCREEN_CASH_RCPT_EVENT attribute.</summary>
  public const int ScreenCashRcptEvent_MaxLength = 33;

  /// <summary>
  /// The value of the SCREEN_CASH_RCPT_EVENT attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 42, Type = MemberType.Char, Length
    = ScreenCashRcptEvent_MaxLength)]
  public string ScreenCashRcptEvent
  {
    get => Get<string>("screenCashRcptEvent") ?? "";
    set => Set(
      "screenCashRcptEvent", IsEmpty(value) ? null : TrimEnd
      (Substring(value, 1, ScreenCashRcptEvent_MaxLength)));
  }

  /// <summary>
  /// The json value of the ScreenCashRcptEvent attribute.</summary>
  [JsonPropertyName("screenCashRcptEvent")]
  [Computed]
  public string ScreenCashRcptEvent_Json
  {
    get => NullIf(ScreenCashRcptEvent, "");
    set => ScreenCashRcptEvent = value;
  }

  /// <summary>Length of the SCREEN_CASH_RCPT_SOURCE attribute.</summary>
  public const int ScreenCashRcptSource_MaxLength = 33;

  /// <summary>
  /// The value of the SCREEN_CASH_RCPT_SOURCE attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 43, Type = MemberType.Char, Length
    = ScreenCashRcptSource_MaxLength)]
  public string ScreenCashRcptSource
  {
    get => Get<string>("screenCashRcptSource") ?? "";
    set => Set(
      "screenCashRcptSource", IsEmpty(value) ? null : TrimEnd
      (Substring(value, 1, ScreenCashRcptSource_MaxLength)));
  }

  /// <summary>
  /// The json value of the ScreenCashRcptSource attribute.</summary>
  [JsonPropertyName("screenCashRcptSource")]
  [Computed]
  public string ScreenCashRcptSource_Json
  {
    get => NullIf(ScreenCashRcptSource, "");
    set => ScreenCashRcptSource = value;
  }

  /// <summary>Length of the SCREEN_CASH_RCPT_TYPE attribute.</summary>
  public const int ScreenCashRcptType_MaxLength = 33;

  /// <summary>
  /// The value of the SCREEN_CASH_RCPT_TYPE attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 44, Type = MemberType.Char, Length
    = ScreenCashRcptType_MaxLength)]
  public string ScreenCashRcptType
  {
    get => Get<string>("screenCashRcptType") ?? "";
    set => Set(
      "screenCashRcptType", IsEmpty(value) ? null : TrimEnd
      (Substring(value, 1, ScreenCashRcptType_MaxLength)));
  }

  /// <summary>
  /// The json value of the ScreenCashRcptType attribute.</summary>
  [JsonPropertyName("screenCashRcptType")]
  [Computed]
  public string ScreenCashRcptType_Json
  {
    get => NullIf(ScreenCashRcptType, "");
    set => ScreenCashRcptType = value;
  }

  /// <summary>Length of the SCREEN_CONTACT attribute.</summary>
  public const int ScreenContact_MaxLength = 33;

  /// <summary>
  /// The value of the SCREEN_CONTACT attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 45, Type = MemberType.Char, Length = ScreenContact_MaxLength)]
  public string ScreenContact
  {
    get => Get<string>("screenContact") ?? "";
    set => Set(
      "screenContact", IsEmpty(value) ? null : TrimEnd
      (Substring(value, 1, ScreenContact_MaxLength)));
  }

  /// <summary>
  /// The json value of the ScreenContact attribute.</summary>
  [JsonPropertyName("screenContact")]
  [Computed]
  public string ScreenContact_Json
  {
    get => NullIf(ScreenContact, "");
    set => ScreenContact = value;
  }

  /// <summary>Length of the SCREEN_CH_NUMBER attribute.</summary>
  public const int ScreenChNumber_MaxLength = 33;

  /// <summary>
  /// The value of the SCREEN_CH_NUMBER attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 46, Type = MemberType.Char, Length = ScreenChNumber_MaxLength)]
    
  public string ScreenChNumber
  {
    get => Get<string>("screenChNumber") ?? "";
    set => Set(
      "screenChNumber", IsEmpty(value) ? null : TrimEnd
      (Substring(value, 1, ScreenChNumber_MaxLength)));
  }

  /// <summary>
  /// The json value of the ScreenChNumber attribute.</summary>
  [JsonPropertyName("screenChNumber")]
  [Computed]
  public string ScreenChNumber_Json
  {
    get => NullIf(ScreenChNumber, "");
    set => ScreenChNumber = value;
  }

  /// <summary>Length of the SCREEN_GENETIC attribute.</summary>
  public const int ScreenGenetic_MaxLength = 33;

  /// <summary>
  /// The value of the SCREEN_GENETIC attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 47, Type = MemberType.Char, Length = ScreenGenetic_MaxLength)]
  public string ScreenGenetic
  {
    get => Get<string>("screenGenetic") ?? "";
    set => Set(
      "screenGenetic", IsEmpty(value) ? null : TrimEnd
      (Substring(value, 1, ScreenGenetic_MaxLength)));
  }

  /// <summary>
  /// The json value of the ScreenGenetic attribute.</summary>
  [JsonPropertyName("screenGenetic")]
  [Computed]
  public string ScreenGenetic_Json
  {
    get => NullIf(ScreenGenetic, "");
    set => ScreenGenetic = value;
  }

  /// <summary>Length of the SCREEN_HEALTH_INS_COVERAGE attribute.</summary>
  public const int ScreenHealthInsCoverage_MaxLength = 33;

  /// <summary>
  /// The value of the SCREEN_HEALTH_INS_COVERAGE attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 48, Type = MemberType.Char, Length
    = ScreenHealthInsCoverage_MaxLength)]
  public string ScreenHealthInsCoverage
  {
    get => Get<string>("screenHealthInsCoverage") ?? "";
    set => Set(
      "screenHealthInsCoverage", IsEmpty(value) ? null : TrimEnd
      (Substring(value, 1, ScreenHealthInsCoverage_MaxLength)));
  }

  /// <summary>
  /// The json value of the ScreenHealthInsCoverage attribute.</summary>
  [JsonPropertyName("screenHealthInsCoverage")]
  [Computed]
  public string ScreenHealthInsCoverage_Json
  {
    get => NullIf(ScreenHealthInsCoverage, "");
    set => ScreenHealthInsCoverage = value;
  }

  /// <summary>Length of the SCREEN_INCOME_SOURCE attribute.</summary>
  public const int ScreenIncomeSource_MaxLength = 33;

  /// <summary>
  /// The value of the SCREEN_INCOME_SOURCE attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 49, Type = MemberType.Char, Length
    = ScreenIncomeSource_MaxLength)]
  public string ScreenIncomeSource
  {
    get => Get<string>("screenIncomeSource") ?? "";
    set => Set(
      "screenIncomeSource", IsEmpty(value) ? null : TrimEnd
      (Substring(value, 1, ScreenIncomeSource_MaxLength)));
  }

  /// <summary>
  /// The json value of the ScreenIncomeSource attribute.</summary>
  [JsonPropertyName("screenIncomeSource")]
  [Computed]
  public string ScreenIncomeSource_Json
  {
    get => NullIf(ScreenIncomeSource, "");
    set => ScreenIncomeSource = value;
  }

  /// <summary>Length of the SCREEN_INFO_REQUEST attribute.</summary>
  public const int ScreenInfoRequest_MaxLength = 33;

  /// <summary>
  /// The value of the SCREEN_INFO_REQUEST attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 50, Type = MemberType.Char, Length
    = ScreenInfoRequest_MaxLength)]
  public string ScreenInfoRequest
  {
    get => Get<string>("screenInfoRequest") ?? "";
    set => Set(
      "screenInfoRequest", IsEmpty(value) ? null : TrimEnd
      (Substring(value, 1, ScreenInfoRequest_MaxLength)));
  }

  /// <summary>
  /// The json value of the ScreenInfoRequest attribute.</summary>
  [JsonPropertyName("screenInfoRequest")]
  [Computed]
  public string ScreenInfoRequest_Json
  {
    get => NullIf(ScreenInfoRequest, "");
    set => ScreenInfoRequest = value;
  }

  /// <summary>Length of the SCREEN_INTERSTATE_REQUEST attribute.</summary>
  public const int ScreenInterstateRequest_MaxLength = 33;

  /// <summary>
  /// The value of the SCREEN_INTERSTATE_REQUEST attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 51, Type = MemberType.Char, Length
    = ScreenInterstateRequest_MaxLength)]
  public string ScreenInterstateRequest
  {
    get => Get<string>("screenInterstateRequest") ?? "";
    set => Set(
      "screenInterstateRequest", IsEmpty(value) ? null : TrimEnd
      (Substring(value, 1, ScreenInterstateRequest_MaxLength)));
  }

  /// <summary>
  /// The json value of the ScreenInterstateRequest attribute.</summary>
  [JsonPropertyName("screenInterstateRequest")]
  [Computed]
  public string ScreenInterstateRequest_Json
  {
    get => NullIf(ScreenInterstateRequest, "");
    set => ScreenInterstateRequest = value;
  }

  /// <summary>Length of the SCREEN_JAIL attribute.</summary>
  public const int ScreenJail_MaxLength = 33;

  /// <summary>
  /// The value of the SCREEN_JAIL attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 52, Type = MemberType.Char, Length = ScreenJail_MaxLength)]
  public string ScreenJail
  {
    get => Get<string>("screenJail") ?? "";
    set => Set(
      "screenJail", IsEmpty(value) ? null : TrimEnd
      (Substring(value, 1, ScreenJail_MaxLength)));
  }

  /// <summary>
  /// The json value of the ScreenJail attribute.</summary>
  [JsonPropertyName("screenJail")]
  [Computed]
  public string ScreenJail_Json
  {
    get => NullIf(ScreenJail, "");
    set => ScreenJail = value;
  }

  /// <summary>Length of the SCREEN_LEGAL_ACTION attribute.</summary>
  public const int ScreenLegalAction_MaxLength = 33;

  /// <summary>
  /// The value of the SCREEN_LEGAL_ACTION attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 53, Type = MemberType.Char, Length
    = ScreenLegalAction_MaxLength)]
  public string ScreenLegalAction
  {
    get => Get<string>("screenLegalAction") ?? "";
    set => Set(
      "screenLegalAction", IsEmpty(value) ? null : TrimEnd
      (Substring(value, 1, ScreenLegalAction_MaxLength)));
  }

  /// <summary>
  /// The json value of the ScreenLegalAction attribute.</summary>
  [JsonPropertyName("screenLegalAction")]
  [Computed]
  public string ScreenLegalAction_Json
  {
    get => NullIf(ScreenLegalAction, "");
    set => ScreenLegalAction = value;
  }

  /// <summary>Length of the SCREEN_LEGAL_ACTION_DETAIL attribute.</summary>
  public const int ScreenLegalActionDetail_MaxLength = 33;

  /// <summary>
  /// The value of the SCREEN_LEGAL_ACTION_DETAIL attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 54, Type = MemberType.Char, Length
    = ScreenLegalActionDetail_MaxLength)]
  public string ScreenLegalActionDetail
  {
    get => Get<string>("screenLegalActionDetail") ?? "";
    set => Set(
      "screenLegalActionDetail", IsEmpty(value) ? null : TrimEnd
      (Substring(value, 1, ScreenLegalActionDetail_MaxLength)));
  }

  /// <summary>
  /// The json value of the ScreenLegalActionDetail attribute.</summary>
  [JsonPropertyName("screenLegalActionDetail")]
  [Computed]
  public string ScreenLegalActionDetail_Json
  {
    get => NullIf(ScreenLegalActionDetail, "");
    set => ScreenLegalActionDetail = value;
  }

  /// <summary>Length of the SCREEN_LEGAL_REFERRAL attribute.</summary>
  public const int ScreenLegalReferral_MaxLength = 33;

  /// <summary>
  /// The value of the SCREEN_LEGAL_REFERRAL attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 55, Type = MemberType.Char, Length
    = ScreenLegalReferral_MaxLength)]
  public string ScreenLegalReferral
  {
    get => Get<string>("screenLegalReferral") ?? "";
    set => Set(
      "screenLegalReferral", IsEmpty(value) ? null : TrimEnd
      (Substring(value, 1, ScreenLegalReferral_MaxLength)));
  }

  /// <summary>
  /// The json value of the ScreenLegalReferral attribute.</summary>
  [JsonPropertyName("screenLegalReferral")]
  [Computed]
  public string ScreenLegalReferral_Json
  {
    get => NullIf(ScreenLegalReferral, "");
    set => ScreenLegalReferral = value;
  }

  /// <summary>Length of the SCREEN_LOCATE_REQUEST_AGENCY attribute.</summary>
  public const int ScreenLocateRequestAgency_MaxLength = 33;

  /// <summary>
  /// The value of the SCREEN_LOCATE_REQUEST_AGENCY attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 56, Type = MemberType.Char, Length
    = ScreenLocateRequestAgency_MaxLength)]
  public string ScreenLocateRequestAgency
  {
    get => Get<string>("screenLocateRequestAgency") ?? "";
    set => Set(
      "screenLocateRequestAgency", IsEmpty(value) ? null : TrimEnd
      (Substring(value, 1, ScreenLocateRequestAgency_MaxLength)));
  }

  /// <summary>
  /// The json value of the ScreenLocateRequestAgency attribute.</summary>
  [JsonPropertyName("screenLocateRequestAgency")]
  [Computed]
  public string ScreenLocateRequestAgency_Json
  {
    get => NullIf(ScreenLocateRequestAgency, "");
    set => ScreenLocateRequestAgency = value;
  }

  /// <summary>Length of the SCREEN_LOCATE_REQUEST_SOURCE attribute.</summary>
  public const int ScreenLocateRequestSource_MaxLength = 33;

  /// <summary>
  /// The value of the SCREEN_LOCATE_REQUEST_SOURCE attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 57, Type = MemberType.Char, Length
    = ScreenLocateRequestSource_MaxLength)]
  public string ScreenLocateRequestSource
  {
    get => Get<string>("screenLocateRequestSource") ?? "";
    set => Set(
      "screenLocateRequestSource", IsEmpty(value) ? null : TrimEnd
      (Substring(value, 1, ScreenLocateRequestSource_MaxLength)));
  }

  /// <summary>
  /// The json value of the ScreenLocateRequestSource attribute.</summary>
  [JsonPropertyName("screenLocateRequestSource")]
  [Computed]
  public string ScreenLocateRequestSource_Json
  {
    get => NullIf(ScreenLocateRequestSource, "");
    set => ScreenLocateRequestSource = value;
  }

  /// <summary>Length of the SCREEN_MILITARY attribute.</summary>
  public const int ScreenMilitary_MaxLength = 33;

  /// <summary>
  /// The value of the SCREEN_MILITARY attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 58, Type = MemberType.Char, Length = ScreenMilitary_MaxLength)]
    
  public string ScreenMilitary
  {
    get => Get<string>("screenMilitary") ?? "";
    set => Set(
      "screenMilitary", IsEmpty(value) ? null : TrimEnd
      (Substring(value, 1, ScreenMilitary_MaxLength)));
  }

  /// <summary>
  /// The json value of the ScreenMilitary attribute.</summary>
  [JsonPropertyName("screenMilitary")]
  [Computed]
  public string ScreenMilitary_Json
  {
    get => NullIf(ScreenMilitary, "");
    set => ScreenMilitary = value;
  }

  /// <summary>Length of the SCREEN_OBLIGATION attribute.</summary>
  public const int ScreenObligation_MaxLength = 33;

  /// <summary>
  /// The value of the SCREEN_OBLIGATION attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 59, Type = MemberType.Char, Length
    = ScreenObligation_MaxLength)]
  public string ScreenObligation
  {
    get => Get<string>("screenObligation") ?? "";
    set => Set(
      "screenObligation", IsEmpty(value) ? null : TrimEnd
      (Substring(value, 1, ScreenObligation_MaxLength)));
  }

  /// <summary>
  /// The json value of the ScreenObligation attribute.</summary>
  [JsonPropertyName("screenObligation")]
  [Computed]
  public string ScreenObligation_Json
  {
    get => NullIf(ScreenObligation, "");
    set => ScreenObligation = value;
  }

  /// <summary>Length of the SCREEN_OBLIGATION_ADMIN_ACTION attribute.</summary>
  public const int ScreenObligationAdminAction_MaxLength = 33;

  /// <summary>
  /// The value of the SCREEN_OBLIGATION_ADMIN_ACTION attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 60, Type = MemberType.Char, Length
    = ScreenObligationAdminAction_MaxLength)]
  public string ScreenObligationAdminAction
  {
    get => Get<string>("screenObligationAdminAction") ?? "";
    set => Set(
      "screenObligationAdminAction", IsEmpty(value) ? null : TrimEnd
      (Substring(value, 1, ScreenObligationAdminAction_MaxLength)));
  }

  /// <summary>
  /// The json value of the ScreenObligationAdminAction attribute.</summary>
  [JsonPropertyName("screenObligationAdminAction")]
  [Computed]
  public string ScreenObligationAdminAction_Json
  {
    get => NullIf(ScreenObligationAdminAction, "");
    set => ScreenObligationAdminAction = value;
  }

  /// <summary>Length of the SCREEN_OBLIGATION_TYPE attribute.</summary>
  public const int ScreenObligationType_MaxLength = 33;

  /// <summary>
  /// The value of the SCREEN_OBLIGATION_TYPE attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 61, Type = MemberType.Char, Length
    = ScreenObligationType_MaxLength)]
  public string ScreenObligationType
  {
    get => Get<string>("screenObligationType") ?? "";
    set => Set(
      "screenObligationType", IsEmpty(value) ? null : TrimEnd
      (Substring(value, 1, ScreenObligationType_MaxLength)));
  }

  /// <summary>
  /// The json value of the ScreenObligationType attribute.</summary>
  [JsonPropertyName("screenObligationType")]
  [Computed]
  public string ScreenObligationType_Json
  {
    get => NullIf(ScreenObligationType, "");
    set => ScreenObligationType = value;
  }

  /// <summary>Length of the SCREEN_OBLIGOR attribute.</summary>
  public const int ScreenObligor_MaxLength = 33;

  /// <summary>
  /// The value of the SCREEN_OBLIGOR attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 62, Type = MemberType.Char, Length = ScreenObligor_MaxLength)]
  public string ScreenObligor
  {
    get => Get<string>("screenObligor") ?? "";
    set => Set(
      "screenObligor", IsEmpty(value) ? null : TrimEnd
      (Substring(value, 1, ScreenObligor_MaxLength)));
  }

  /// <summary>
  /// The json value of the ScreenObligor attribute.</summary>
  [JsonPropertyName("screenObligor")]
  [Computed]
  public string ScreenObligor_Json
  {
    get => NullIf(ScreenObligor, "");
    set => ScreenObligor = value;
  }

  /// <summary>Length of the SCREEN_PERSON_ACCT attribute.</summary>
  public const int ScreenPersonAcct_MaxLength = 33;

  /// <summary>
  /// The value of the SCREEN_PERSON_ACCT attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 63, Type = MemberType.Char, Length
    = ScreenPersonAcct_MaxLength)]
  public string ScreenPersonAcct
  {
    get => Get<string>("screenPersonAcct") ?? "";
    set => Set(
      "screenPersonAcct", IsEmpty(value) ? null : TrimEnd
      (Substring(value, 1, ScreenPersonAcct_MaxLength)));
  }

  /// <summary>
  /// The json value of the ScreenPersonAcct attribute.</summary>
  [JsonPropertyName("screenPersonAcct")]
  [Computed]
  public string ScreenPersonAcct_Json
  {
    get => NullIf(ScreenPersonAcct, "");
    set => ScreenPersonAcct = value;
  }

  /// <summary>Length of the SCREEN_PERSON_ADDRESS attribute.</summary>
  public const int ScreenPersonAddress_MaxLength = 33;

  /// <summary>
  /// The value of the SCREEN_PERSON_ADDRESS attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 64, Type = MemberType.Char, Length
    = ScreenPersonAddress_MaxLength)]
  public string ScreenPersonAddress
  {
    get => Get<string>("screenPersonAddress") ?? "";
    set => Set(
      "screenPersonAddress", IsEmpty(value) ? null : TrimEnd
      (Substring(value, 1, ScreenPersonAddress_MaxLength)));
  }

  /// <summary>
  /// The json value of the ScreenPersonAddress attribute.</summary>
  [JsonPropertyName("screenPersonAddress")]
  [Computed]
  public string ScreenPersonAddress_Json
  {
    get => NullIf(ScreenPersonAddress, "");
    set => ScreenPersonAddress = value;
  }

  /// <summary>Length of the SCREEN_PR_NUMBER attribute.</summary>
  public const int ScreenPrNumber_MaxLength = 33;

  /// <summary>
  /// The value of the SCREEN_PR_NUMBER attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 65, Type = MemberType.Char, Length = ScreenPrNumber_MaxLength)]
    
  public string ScreenPrNumber
  {
    get => Get<string>("screenPrNumber") ?? "";
    set => Set(
      "screenPrNumber", IsEmpty(value) ? null : TrimEnd
      (Substring(value, 1, ScreenPrNumber_MaxLength)));
  }

  /// <summary>
  /// The json value of the ScreenPrNumber attribute.</summary>
  [JsonPropertyName("screenPrNumber")]
  [Computed]
  public string ScreenPrNumber_Json
  {
    get => NullIf(ScreenPrNumber, "");
    set => ScreenPrNumber = value;
  }

  /// <summary>Length of the SCREEN_RECAPTURE_RULE attribute.</summary>
  public const int ScreenRecaptureRule_MaxLength = 33;

  /// <summary>
  /// The value of the SCREEN_RECAPTURE_RULE attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 66, Type = MemberType.Char, Length
    = ScreenRecaptureRule_MaxLength)]
  public string ScreenRecaptureRule
  {
    get => Get<string>("screenRecaptureRule") ?? "";
    set => Set(
      "screenRecaptureRule", IsEmpty(value) ? null : TrimEnd
      (Substring(value, 1, ScreenRecaptureRule_MaxLength)));
  }

  /// <summary>
  /// The json value of the ScreenRecaptureRule attribute.</summary>
  [JsonPropertyName("screenRecaptureRule")]
  [Computed]
  public string ScreenRecaptureRule_Json
  {
    get => NullIf(ScreenRecaptureRule, "");
    set => ScreenRecaptureRule = value;
  }

  /// <summary>Length of the SCREEN_RESOURCE attribute.</summary>
  public const int ScreenResource_MaxLength = 33;

  /// <summary>
  /// The value of the SCREEN_RESOURCE attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 67, Type = MemberType.Char, Length = ScreenResource_MaxLength)]
    
  public string ScreenResource
  {
    get => Get<string>("screenResource") ?? "";
    set => Set(
      "screenResource", IsEmpty(value) ? null : TrimEnd
      (Substring(value, 1, ScreenResource_MaxLength)));
  }

  /// <summary>
  /// The json value of the ScreenResource attribute.</summary>
  [JsonPropertyName("screenResource")]
  [Computed]
  public string ScreenResource_Json
  {
    get => NullIf(ScreenResource, "");
    set => ScreenResource = value;
  }

  /// <summary>Length of the SCREEN_TRIBUNAL attribute.</summary>
  public const int ScreenTribunal_MaxLength = 33;

  /// <summary>
  /// The value of the SCREEN_TRIBUNAL attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 68, Type = MemberType.Char, Length = ScreenTribunal_MaxLength)]
    
  public string ScreenTribunal
  {
    get => Get<string>("screenTribunal") ?? "";
    set => Set(
      "screenTribunal", IsEmpty(value) ? null : TrimEnd
      (Substring(value, 1, ScreenTribunal_MaxLength)));
  }

  /// <summary>
  /// The json value of the ScreenTribunal attribute.</summary>
  [JsonPropertyName("screenTribunal")]
  [Computed]
  public string ScreenTribunal_Json
  {
    get => NullIf(ScreenTribunal, "");
    set => ScreenTribunal = value;
  }

  /// <summary>Length of the SCREEN_WORKER_COMP attribute.</summary>
  public const int ScreenWorkerComp_MaxLength = 33;

  /// <summary>
  /// The value of the SCREEN_WORKER_COMP attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 69, Type = MemberType.Char, Length
    = ScreenWorkerComp_MaxLength)]
  public string ScreenWorkerComp
  {
    get => Get<string>("screenWorkerComp") ?? "";
    set => Set(
      "screenWorkerComp", IsEmpty(value) ? null : TrimEnd
      (Substring(value, 1, ScreenWorkerComp_MaxLength)));
  }

  /// <summary>
  /// The json value of the ScreenWorkerComp attribute.</summary>
  [JsonPropertyName("screenWorkerComp")]
  [Computed]
  public string ScreenWorkerComp_Json
  {
    get => NullIf(ScreenWorkerComp, "");
    set => ScreenWorkerComp = value;
  }

  /// <summary>Length of the SCREEN_WORKSHEET attribute.</summary>
  public const int ScreenWorksheet_MaxLength = 33;

  /// <summary>
  /// The value of the SCREEN_WORKSHEET attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 70, Type = MemberType.Char, Length = ScreenWorksheet_MaxLength)
    ]
  public string ScreenWorksheet
  {
    get => Get<string>("screenWorksheet") ?? "";
    set => Set(
      "screenWorksheet", IsEmpty(value) ? null : TrimEnd
      (Substring(value, 1, ScreenWorksheet_MaxLength)));
  }

  /// <summary>
  /// The json value of the ScreenWorksheet attribute.</summary>
  [JsonPropertyName("screenWorksheet")]
  [Computed]
  public string ScreenWorksheet_Json
  {
    get => NullIf(ScreenWorksheet, "");
    set => ScreenWorksheet = value;
  }
}
