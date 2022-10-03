// The source file: ELECTRONIC_FUND_TRANSMISSION, ID: 371434067, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Entities;

/// <summary>
/// RESP: FINANCE
/// </summary>
[Serializable]
public partial class ElectronicFundTransmission: Heap, ICloneable
{
  /// <summary>Default constructor.</summary>
  public ElectronicFundTransmission()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public ElectronicFundTransmission(ElectronicFundTransmission that):
    base(that)
  {
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new ElectronicFundTransmission Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Length of the CREATED_BY attribute.</summary>
  public const int CreatedBy_MaxLength = 8;

  /// <summary>
  /// The value of the CREATED_BY attribute.
  /// USER ID or Program ID responsible for creation of the occurrence.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 1, Type = MemberType.Char, Length = CreatedBy_MaxLength)]
  public string CreatedBy
  {
    get => Get<string>("createdBy") ?? "";
    set => Set(
      "createdBy", IsEmpty(value) ? null : TrimEnd
      (Substring(value, 1, CreatedBy_MaxLength)));
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
  /// 	
  /// </summary>
  [JsonPropertyName("createdTimestamp")]
  [Member(Index = 2, Type = MemberType.Timestamp)]
  public DateTime? CreatedTimestamp
  {
    get => Get<DateTime?>("createdTimestamp");
    set => Set("createdTimestamp", value);
  }

  /// <summary>Length of the LAST_UPDATED_BY attribute.</summary>
  public const int LastUpdatedBy_MaxLength = 8;

  /// <summary>
  /// The value of the LAST_UPDATED_BY attribute.
  /// User ID or Program ID responsible for the last update of the occurrence.
  /// </summary>
  [JsonPropertyName("lastUpdatedBy")]
  [Member(Index = 3, Type = MemberType.Char, Length = LastUpdatedBy_MaxLength, Optional
    = true)]
  public string LastUpdatedBy
  {
    get => Get<string>("lastUpdatedBy");
    set => Set(
      "lastUpdatedBy", TrimEnd(Substring(value, 1, LastUpdatedBy_MaxLength)));
  }

  /// <summary>
  /// The value of the LAST_UPDATED_TIMESTAMP attribute.
  /// Timestamp of last update of the occurrence.
  /// </summary>
  [JsonPropertyName("lastUpdatedTimestamp")]
  [Member(Index = 4, Type = MemberType.Timestamp, Optional = true)]
  public DateTime? LastUpdatedTimestamp
  {
    get => Get<DateTime?>("lastUpdatedTimestamp");
    set => Set("lastUpdatedTimestamp", value);
  }

  /// <summary>
  /// The value of the PAY_DATE attribute.
  /// The date the funds were withheld by the employer.
  /// </summary>
  [JsonPropertyName("payDate")]
  [Member(Index = 5, Type = MemberType.Date, Optional = true)]
  public DateTime? PayDate
  {
    get => Get<DateTime?>("payDate");
    set => Set("payDate", value);
  }

  /// <summary>
  /// The value of the TRANSMITTAL_AMOUNT attribute.
  /// For inbound EFTs, this is the amount of money actually received/deposited 
  /// by us. This may or may not be the amount of money that was withheld from
  /// the Absent Parent. For outbound EFTs, this is the amount of money we are
  /// actually sending to the Applicant Recipient or other state.
  /// </summary>
  [JsonPropertyName("transmittalAmount")]
  [DefaultValue(typeof(decimal), "0")]
  [Member(Index = 6, Type = MemberType.Number, Length = 10, Precision = 2)]
  public decimal TransmittalAmount
  {
    get => Get<decimal?>("transmittalAmount") ?? 0M;
    set => Set(
      "transmittalAmount", value == 0M ? null : Truncate
      (value, 2) as decimal?);
  }

  /// <summary>
  /// The value of the AP_SSN attribute.
  /// The SOCIAL SECURITY NUMBER of the obligor whose account this transaction 
  /// is to be posted against.
  /// </summary>
  [JsonPropertyName("apSsn")]
  [DefaultValue(0)]
  [Member(Index = 7, Type = MemberType.Number, Length = 9)]
  public int ApSsn
  {
    get => Get<int?>("apSsn") ?? 0;
    set => Set("apSsn", value == 0 ? null : value as int?);
  }

  /// <summary>Length of the MEDICAL_SUPPORT_ID attribute.</summary>
  public const int MedicalSupportId_MaxLength = 1;

  /// <summary>
  /// The value of the MEDICAL_SUPPORT_ID attribute.
  /// Indicates whether the non-custodial parent has family medical insurance 
  /// coverage available through their employer. Y=Yes, N=No,W=N/A.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 8, Type = MemberType.Char, Length = MedicalSupportId_MaxLength)
    ]
  public string MedicalSupportId
  {
    get => Get<string>("medicalSupportId") ?? "";
    set => Set(
      "medicalSupportId", IsEmpty(value) ? null : TrimEnd
      (Substring(value, 1, MedicalSupportId_MaxLength)));
  }

  /// <summary>
  /// The json value of the MedicalSupportId attribute.</summary>
  [JsonPropertyName("medicalSupportId")]
  [Computed]
  public string MedicalSupportId_Json
  {
    get => NullIf(MedicalSupportId, "");
    set => MedicalSupportId = value;
  }

  /// <summary>Length of the AP_NAME attribute.</summary>
  public const int ApName_MaxLength = 10;

  /// <summary>
  /// The value of the AP_NAME attribute.
  /// The first seven letters of the obligor's last name followed by the first 
  /// three letters of the obligor's first name. When last name is less than
  /// seven letters, a comma is used to delimit the two names.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 9, Type = MemberType.Char, Length = ApName_MaxLength)]
  public string ApName
  {
    get => Get<string>("apName") ?? "";
    set => Set(
      "apName", IsEmpty(value) ? null : TrimEnd
      (Substring(value, 1, ApName_MaxLength)));
  }

  /// <summary>
  /// The json value of the ApName attribute.</summary>
  [JsonPropertyName("apName")]
  [Computed]
  public string ApName_Json
  {
    get => NullIf(ApName, "");
    set => ApName = value;
  }

  /// <summary>
  /// The value of the FIPS_CODE attribute.
  /// The Federal information processing system code information which 
  /// identifies geographical areas in the United States. The first two
  /// characters are the state, the next three are county, land the last two are
  /// location.
  /// </summary>
  [JsonPropertyName("fipsCode")]
  [Member(Index = 10, Type = MemberType.Number, Length = 7, Optional = true)]
  public int? FipsCode
  {
    get => Get<int?>("fipsCode");
    set => Set("fipsCode", value);
  }

  /// <summary>Length of the EMPLOYMENT_TERMINATION_ID attribute.</summary>
  public const int EmploymentTerminationId_MaxLength = 1;

  /// <summary>
  /// The value of the EMPLOYMENT_TERMINATION_ID attribute.
  /// Indicator used to notify the receiving child support enforcement agency 
  /// that an individuals' employment has terminated.
  /// </summary>
  [JsonPropertyName("employmentTerminationId")]
  [Member(Index = 11, Type = MemberType.Char, Length
    = EmploymentTerminationId_MaxLength, Optional = true)]
  public string EmploymentTerminationId
  {
    get => Get<string>("employmentTerminationId");
    set => Set(
      "employmentTerminationId", TrimEnd(Substring(value, 1,
      EmploymentTerminationId_MaxLength)));
  }

  /// <summary>
  /// The value of the ZDEL_ADDENDA_SEQUENCE_NUMBER attribute.
  /// The sequence number that is assigned to the addenda record of the ACH file
  /// format.
  /// </summary>
  [JsonPropertyName("zdelAddendaSequenceNumber")]
  [Member(Index = 12, Type = MemberType.Number, Length = 4, Optional = true)]
  public int? ZdelAddendaSequenceNumber
  {
    get => Get<int?>("zdelAddendaSequenceNumber");
    set => Set("zdelAddendaSequenceNumber", value);
  }

  /// <summary>
  /// The value of the SEQUENCE_NUMBER attribute.
  /// The sequential record number within the file (not counting the header 
  /// record).
  /// </summary>
  [JsonPropertyName("sequenceNumber")]
  [Member(Index = 13, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? SequenceNumber
  {
    get => Get<int?>("sequenceNumber");
    set => Set("sequenceNumber", value);
  }

  /// <summary>
  /// The value of the RECEIVING_DFI_IDENTIFICATION attribute.
  /// The standard Routing Number as assigned by Thomson Financial Publishing. 
  /// Is used along with the check digit to identify the Depository Financial
  /// Institution (DFI) in which the Receiver maintains his account or a Routing
  /// Number assigned to a federal government agency by the Federal Reserve.
  /// </summary>
  [JsonPropertyName("receivingDfiIdentification")]
  [Member(Index = 14, Type = MemberType.Number, Length = 8, Optional = true)]
  public int? ReceivingDfiIdentification
  {
    get => Get<int?>("receivingDfiIdentification");
    set => Set("receivingDfiIdentification", value);
  }

  /// <summary>Length of the DFI_ACCOUNT_NUMBER attribute.</summary>
  public const int DfiAccountNumber_MaxLength = 17;

  /// <summary>
  /// The value of the DFI_ACCOUNT_NUMBER attribute.
  /// The account number that the EFT transaction is posted to.
  /// </summary>
  [JsonPropertyName("dfiAccountNumber")]
  [Member(Index = 15, Type = MemberType.Char, Length
    = DfiAccountNumber_MaxLength, Optional = true)]
  public string DfiAccountNumber
  {
    get => Get<string>("dfiAccountNumber");
    set => Set(
      "dfiAccountNumber",
      TrimEnd(Substring(value, 1, DfiAccountNumber_MaxLength)));
  }

  /// <summary>Length of the TRANSACTION_CODE attribute.</summary>
  public const int TransactionCode_MaxLength = 2;

  /// <summary>
  /// The value of the TRANSACTION_CODE attribute.
  /// Signifies the specific type of financial transaction that the transmission
  /// record will post to the targeted RDFI account.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 16, Type = MemberType.Char, Length = TransactionCode_MaxLength)
    ]
  public string TransactionCode
  {
    get => Get<string>("transactionCode") ?? "";
    set => Set(
      "transactionCode", IsEmpty(value) ? null : TrimEnd
      (Substring(value, 1, TransactionCode_MaxLength)));
  }

  /// <summary>
  /// The json value of the TransactionCode attribute.</summary>
  [JsonPropertyName("transactionCode")]
  [Computed]
  public string TransactionCode_Json
  {
    get => NullIf(TransactionCode, "");
    set => TransactionCode = value;
  }

  /// <summary>
  /// The value of the SETTLEMENT_DATE attribute.
  /// The date that funds actually are exchanged between the ODFI and 	RDFI as a
  /// result of the EFT transaction.
  /// </summary>
  [JsonPropertyName("settlementDate")]
  [Member(Index = 17, Type = MemberType.Date, Optional = true)]
  public DateTime? SettlementDate
  {
    get => Get<DateTime?>("settlementDate");
    set => Set("settlementDate", value);
  }

  /// <summary>Length of the CASE_ID attribute.</summary>
  public const int CaseId_MaxLength = 20;

  /// <summary>
  /// The value of the CASE_ID attribute.
  /// The receiving State's IV-D case number or court order number that 
  /// authorizes this EFT funds exchange.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 18, Type = MemberType.Char, Length = CaseId_MaxLength)]
  public string CaseId
  {
    get => Get<string>("caseId") ?? "";
    set => Set(
      "caseId", IsEmpty(value) ? null : TrimEnd
      (Substring(value, 1, CaseId_MaxLength)));
  }

  /// <summary>
  /// The json value of the CaseId attribute.</summary>
  [JsonPropertyName("caseId")]
  [Computed]
  public string CaseId_Json
  {
    get => NullIf(CaseId, "");
    set => CaseId = value;
  }

  /// <summary>Length of the TRANSMISSION_STATUS_CODE attribute.</summary>
  public const int TransmissionStatusCode_MaxLength = 10;

  /// <summary>
  /// The value of the TRANSMISSION_STATUS_CODE attribute.
  /// The code that indicates the immediate status of a transmission record, 
  /// symbolizing a request for the record to be processed or the outcome of
  /// prior processing against the record.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 19, Type = MemberType.Char, Length
    = TransmissionStatusCode_MaxLength)]
  public string TransmissionStatusCode
  {
    get => Get<string>("transmissionStatusCode") ?? "";
    set => Set(
      "transmissionStatusCode", IsEmpty(value) ? null : TrimEnd
      (Substring(value, 1, TransmissionStatusCode_MaxLength)));
  }

  /// <summary>
  /// The json value of the TransmissionStatusCode attribute.</summary>
  [JsonPropertyName("transmissionStatusCode")]
  [Computed]
  public string TransmissionStatusCode_Json
  {
    get => NullIf(TransmissionStatusCode, "");
    set => TransmissionStatusCode = value;
  }

  /// <summary>Length of the COMPANY_NAME attribute.</summary>
  public const int CompanyName_MaxLength = 16;

  /// <summary>
  /// The value of the COMPANY_NAME attribute.
  /// The name of the company sending the EFT transmission. Originating 
  /// Depository Financial Institution (ODFI).
  /// </summary>
  [JsonPropertyName("companyName")]
  [Member(Index = 20, Type = MemberType.Char, Length = CompanyName_MaxLength, Optional
    = true)]
  public string CompanyName
  {
    get => Get<string>("companyName");
    set => Set(
      "companyName", TrimEnd(Substring(value, 1, CompanyName_MaxLength)));
  }

  /// <summary>
  /// The value of the ORIGINATING_DFI_IDENTIFICATION attribute.
  /// The routing number of the Originating Depository Financial Institution (
  /// ODFI).
  /// </summary>
  [JsonPropertyName("originatingDfiIdentification")]
  [Member(Index = 21, Type = MemberType.Number, Length = 8, Optional = true)]
  public int? OriginatingDfiIdentification
  {
    get => Get<int?>("originatingDfiIdentification");
    set => Set("originatingDfiIdentification", value);
  }

  /// <summary>Length of the RECEIVING_ENTITY_NAME attribute.</summary>
  public const int ReceivingEntityName_MaxLength = 22;

  /// <summary>
  /// The value of the RECEIVING_ENTITY_NAME attribute.
  /// The name of the party that receives this electronic fund transmission 
  /// exchange.
  /// </summary>
  [JsonPropertyName("receivingEntityName")]
  [Member(Index = 22, Type = MemberType.Char, Length
    = ReceivingEntityName_MaxLength, Optional = true)]
  public string ReceivingEntityName
  {
    get => Get<string>("receivingEntityName");
    set => Set(
      "receivingEntityName", TrimEnd(Substring(value, 1,
      ReceivingEntityName_MaxLength)));
  }

  /// <summary>Length of the TRANSMISSION_TYPE attribute.</summary>
  public const int TransmissionType_MaxLength = 1;

  /// <summary>
  /// The value of the TRANSMISSION_TYPE attribute.
  /// This attribute identifies whether a record is a result of an inbound or 
  /// outbound file transmission.
  /// Having a repository for electronic payments will facilitate and simplify 
  /// eft processing and online maintenence.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 23, Type = MemberType.Char, Length
    = TransmissionType_MaxLength)]
  public string TransmissionType
  {
    get => Get<string>("transmissionType") ?? "";
    set => Set(
      "transmissionType", IsEmpty(value) ? null : TrimEnd
      (Substring(value, 1, TransmissionType_MaxLength)));
  }

  /// <summary>
  /// The json value of the TransmissionType attribute.</summary>
  [JsonPropertyName("transmissionType")]
  [Computed]
  public string TransmissionType_Json
  {
    get => NullIf(TransmissionType, "");
    set => TransmissionType = value;
  }

  /// <summary>
  /// The value of the TRANSMISSION_IDENTIFIER attribute.
  /// A unique, system generated sequential number that distinguishes one 
  /// occurrence of the entity type from another.
  /// </summary>
  [JsonPropertyName("transmissionIdentifier")]
  [DefaultValue(0)]
  [Member(Index = 24, Type = MemberType.Number, Length = 9)]
  public int TransmissionIdentifier
  {
    get => Get<int?>("transmissionIdentifier") ?? 0;
    set => Set("transmissionIdentifier", value == 0 ? null : value as int?);
  }

  /// <summary>
  /// The value of the TRANSMISSION_PROCESS_DATE attribute.
  /// Date that this transmission record is processed by the KESSEP system 
  /// subsequent to it's insertion onto the database.
  /// </summary>
  [JsonPropertyName("transmissionProcessDate")]
  [Member(Index = 25, Type = MemberType.Date, Optional = true)]
  public DateTime? TransmissionProcessDate
  {
    get => Get<DateTime?>("transmissionProcessDate");
    set => Set("transmissionProcessDate", value);
  }

  /// <summary>
  /// The value of the FILE_CREATION_DATE attribute.
  /// The date that the file was created by the bank(UMB). This date is used to 
  /// prevent duplicate processing and to resolve problems with specific
  /// transactions.
  /// </summary>
  [JsonPropertyName("fileCreationDate")]
  [Member(Index = 26, Type = MemberType.Date, Optional = true)]
  public DateTime? FileCreationDate
  {
    get => Get<DateTime?>("fileCreationDate");
    set => Set("fileCreationDate", value);
  }

  /// <summary>
  /// The value of the FILE_CREATION_TIME attribute.
  /// The time the file was created by the bank (United Missouri Bank-UMB). This
  /// time is used to prevent duplicate processing and to resolve problems with
  /// specific transactions.
  /// </summary>
  [JsonPropertyName("fileCreationTime")]
  [Member(Index = 27, Type = MemberType.Time, Optional = true)]
  public TimeSpan? FileCreationTime
  {
    get => Get<TimeSpan?>("fileCreationTime");
    set => Set("fileCreationTime", value);
  }

  /// <summary>Length of the COMPANY_IDENTIFICATION_ICD attribute.</summary>
  public const int CompanyIdentificationIcd_MaxLength = 1;

  /// <summary>
  /// The value of the COMPANY_IDENTIFICATION_ICD attribute.
  /// The Identification Code Designator (ICD) is a number that is used to 
  /// identify the type of Company Identification Number. A 1 stands for and IRS
  /// Employer Identification Number (EIN). A 3 stands for a Data Universal
  /// Numbering System (DUNS). A 9 stands for a user assigned number.
  /// </summary>
  [JsonPropertyName("companyIdentificationIcd")]
  [Member(Index = 28, Type = MemberType.Char, Length
    = CompanyIdentificationIcd_MaxLength, Optional = true)]
  public string CompanyIdentificationIcd
  {
    get => Get<string>("companyIdentificationIcd");
    set => Set(
      "companyIdentificationIcd", TrimEnd(Substring(value, 1,
      CompanyIdentificationIcd_MaxLength)));
  }

  /// <summary>Length of the COMPANY_IDENTIFICATION_NUMBER attribute.</summary>
  public const int CompanyIdentificationNumber_MaxLength = 9;

  /// <summary>
  /// The value of the COMPANY_IDENTIFICATION_NUMBER attribute.
  /// A number used to identify the Originator. In most cases we expect this 
  /// number to be an IRS Employer Identification Number (EIN).
  /// </summary>
  [JsonPropertyName("companyIdentificationNumber")]
  [Member(Index = 29, Type = MemberType.Char, Length
    = CompanyIdentificationNumber_MaxLength, Optional = true)]
  public string CompanyIdentificationNumber
  {
    get => Get<string>("companyIdentificationNumber");
    set => Set(
      "companyIdentificationNumber", TrimEnd(Substring(value, 1,
      CompanyIdentificationNumber_MaxLength)));
  }

  /// <summary>
  /// The value of the COMPANY_DESCRIPTIVE_DATE attribute.
  /// The date the Originator creates the file. Typically the process date of 
  /// the payment data. This date will be used to match EFT payment data to a
  /// interface transmission from either a court or the federal government.
  /// </summary>
  [JsonPropertyName("companyDescriptiveDate")]
  [Member(Index = 30, Type = MemberType.Date, Optional = true)]
  public DateTime? CompanyDescriptiveDate
  {
    get => Get<DateTime?>("companyDescriptiveDate");
    set => Set("companyDescriptiveDate", value);
  }

  /// <summary>
  /// The value of the EFFECTIVE_ENTRY_DATE attribute.
  /// The date the Originator intends the transaction to be settled.
  /// </summary>
  [JsonPropertyName("effectiveEntryDate")]
  [Member(Index = 31, Type = MemberType.Date, Optional = true)]
  public DateTime? EffectiveEntryDate
  {
    get => Get<DateTime?>("effectiveEntryDate");
    set => Set("effectiveEntryDate", value);
  }

  /// <summary>Length of the RECEIVING_COMPANY_NAME attribute.</summary>
  public const int ReceivingCompanyName_MaxLength = 22;

  /// <summary>
  /// The value of the RECEIVING_COMPANY_NAME attribute.
  /// The name of the receiving company/agency.
  /// </summary>
  [JsonPropertyName("receivingCompanyName")]
  [Member(Index = 32, Type = MemberType.Char, Length
    = ReceivingCompanyName_MaxLength, Optional = true)]
  public string ReceivingCompanyName
  {
    get => Get<string>("receivingCompanyName");
    set => Set(
      "receivingCompanyName", TrimEnd(Substring(value, 1,
      ReceivingCompanyName_MaxLength)));
  }

  /// <summary>
  /// The value of the TRACE_NUMBER attribute.
  /// A sequential number assigned by the Originating Depositor Financial 
  /// Institution (ODFI) on inbound EFTs. On outbound EFTs the last 7 digits of
  /// this number will be assigned by SRS CSE. The first 8 digits will be
  /// assigned by Department of Administration (DOA) after we have sent them the
  /// file.
  /// </summary>
  [JsonPropertyName("traceNumber")]
  [Member(Index = 33, Type = MemberType.Number, Length = 15, Optional = true)]
  public long? TraceNumber
  {
    get => Get<long?>("traceNumber");
    set => Set("traceNumber", value);
  }

  /// <summary>Length of the APPLICATION_IDENTIFIER attribute.</summary>
  public const int ApplicationIdentifier_MaxLength = 2;

  /// <summary>
  /// The value of the APPLICATION_IDENTIFIER attribute.
  /// Identifies the type of collection being transmitted.                 
  /// CS-Income Withholding from Employers                                II
  /// -Interstate Income Withholding
  /// IT-Interstate State Tax
  /// Offset                                                   IO-Interstate
  /// All Others
  /// RI-
  /// Interstate Cost-Recovery Income Withholding                   RT-
  /// Interstate Cost-Recovery State Tax Offset                         RO-
  /// Interstate Cost-Recovery All Others
  /// 
  /// If RI,RT or RO then the two payment amounts will be
  /// different due to a cost recovery fee being taken by the state that
  /// collected the money.
  /// </summary>
  [JsonPropertyName("applicationIdentifier")]
  [Member(Index = 34, Type = MemberType.Char, Length
    = ApplicationIdentifier_MaxLength, Optional = true)]
  public string ApplicationIdentifier
  {
    get => Get<string>("applicationIdentifier");
    set => Set(
      "applicationIdentifier", TrimEnd(Substring(value, 1,
      ApplicationIdentifier_MaxLength)));
  }

  /// <summary>
  /// The value of the COLLECTION_AMOUNT attribute.
  /// For inbound EFT's this is the amount of money witheld by the employer. It 
  /// may or may not match the amount of money we receive. For outbound EFT's
  /// this is the amount of money collected on the behalf of the Applicant
  /// Recipient.
  /// </summary>
  [JsonPropertyName("collectionAmount")]
  [Member(Index = 35, Type = MemberType.Number, Length = 10, Precision = 2, Optional
    = true)]
  public decimal? CollectionAmount
  {
    get => Get<decimal?>("collectionAmount");
    set => Set("collectionAmount", Truncate(value, 2));
  }

  /// <summary>Length of the VENDOR_NUMBER attribute.</summary>
  public const int VendorNumber_MaxLength = 10;

  /// <summary>
  /// The value of the VENDOR_NUMBER attribute.
  /// The SSN or Federal Employer Identification Number (FEIN) of the Applicant 
  /// Recipient or state. This number is used by the STARS system and is not
  /// transmitted with the payment. The number is left justified and the 10th
  /// character is a space.
  /// </summary>
  [JsonPropertyName("vendorNumber")]
  [Member(Index = 36, Type = MemberType.Char, Length = VendorNumber_MaxLength, Optional
    = true)]
  public string VendorNumber
  {
    get => Get<string>("vendorNumber");
    set => Set(
      "vendorNumber", TrimEnd(Substring(value, 1, VendorNumber_MaxLength)));
  }

  /// <summary>
  /// The value of the CHECK_DIGIT attribute.
  /// Used to verify the accuracy of the Receiving_DFI_Identification number it 
  /// makes up the Receiving Routing Number. Depository Financial Institution (
  /// DFI).
  /// </summary>
  [JsonPropertyName("checkDigit")]
  [Member(Index = 37, Type = MemberType.Number, Length = 1, Optional = true)]
  public int? CheckDigit
  {
    get => Get<int?>("checkDigit");
    set => Set("checkDigit", value);
  }

  /// <summary>Length of the RECEIVING_DFI_ACCOUNT_NUMBER attribute.</summary>
  public const int ReceivingDfiAccountNumber_MaxLength = 17;

  /// <summary>
  /// The value of the RECEIVING_DFI_ACCOUNT_NUMBER attribute.
  /// The Receiving Depository Financial Institution's (RDFI) customer 
  /// identification is the Applicant Recipient's or State's bank account
  /// number. If the number is less than 17 characters then left justify and
  /// fill out with spaces.
  /// </summary>
  [JsonPropertyName("receivingDfiAccountNumber")]
  [Member(Index = 38, Type = MemberType.Char, Length
    = ReceivingDfiAccountNumber_MaxLength, Optional = true)]
  public string ReceivingDfiAccountNumber
  {
    get => Get<string>("receivingDfiAccountNumber");
    set => Set(
      "receivingDfiAccountNumber", TrimEnd(Substring(value, 1,
      ReceivingDfiAccountNumber_MaxLength)));
  }

  /// <summary>Length of the COMPANY_ENTRY_DESCRIPTION attribute.</summary>
  public const int CompanyEntryDescription_MaxLength = 10;

  /// <summary>
  /// The value of the COMPANY_ENTRY_DESCRIPTION attribute.
  /// This field is used to describe the state and/or county who is dending the 
  /// transaction. When receiving a transaction from another state the first two
  /// characters of the field specify the state abbreviation and the next five
  /// characters are the first five characters of the Federal Identification
  /// Processing Standards (FIPS) code. When receiving a transaction from an
  /// employer then this field will contain a short description of the type of
  /// transaction being sent.
  /// </summary>
  [JsonPropertyName("companyEntryDescription")]
  [Member(Index = 39, Type = MemberType.Char, Length
    = CompanyEntryDescription_MaxLength, Optional = true)]
  public string CompanyEntryDescription
  {
    get => Get<string>("companyEntryDescription");
    set => Set(
      "companyEntryDescription", TrimEnd(Substring(value, 1,
      CompanyEntryDescription_MaxLength)));
  }

  /// <summary>
  /// The value of the SYSTEM_GENERATED_IDENTIFIER attribute.
  /// A unique, system generated timestamp that distinguishes one occurrence of 
  /// the entity type from another.
  /// </summary>
  [JsonPropertyName("prqGeneratedId")]
  [Member(Index = 40, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? PrqGeneratedId
  {
    get => Get<int?>("prqGeneratedId");
    set => Set("prqGeneratedId", value);
  }
}
