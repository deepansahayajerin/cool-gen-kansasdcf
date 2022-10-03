// The source file: DISBURSEMENT_TYPE, ID: 371433807, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Entities;

/// <summary>
/// RESP: FINANCE
/// Describes the type of a disbursement.  Every disbursement must be described 
/// by one and only one type thru its relationship with this entity.  This
/// entity contains all of the possible valid types.
/// Examples:
/// Collection Agency Fee
/// Cost Recovery Fee
/// NADC Current Child Support
/// NADC Arrears Child Support
/// ADC Current Child Support
/// ADC Arrears Child Support
/// Spousal Support
/// Passthru
/// Examples: Pass-thru, Non-ADC child support, spousal support, refunds, 
/// advancements, etc.
/// </summary>
[Serializable]
public partial class DisbursementType: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public DisbursementType()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public DisbursementType(DisbursementType that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new DisbursementType Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(DisbursementType that)
  {
    base.Assign(that);
    systemGeneratedIdentifier = that.systemGeneratedIdentifier;
    code = that.code;
    name = that.name;
    currentArrearsInd = that.currentArrearsInd;
    programCode = that.programCode;
    recaptureInd = that.recaptureInd;
    cashNonCashInd = that.cashNonCashInd;
    effectiveDate = that.effectiveDate;
    discontinueDate = that.discontinueDate;
    createdBy = that.createdBy;
    createdTimestamp = that.createdTimestamp;
    lastUpdatedBy = that.lastUpdatedBy;
    lastUpdatedTmst = that.lastUpdatedTmst;
    description = that.description;
    recoveryType = that.recoveryType;
  }

  /// <summary>
  /// The value of the SYSTEM_GENERATED_IDENTIFIER attribute.
  /// A unique, system generated sequential number that distinguishes one 
  /// occurrence of the entity type from another.
  /// </summary>
  [JsonPropertyName("systemGeneratedIdentifier")]
  [DefaultValue(0)]
  [Member(Index = 1, Type = MemberType.Number, Length = 3)]
  public int SystemGeneratedIdentifier
  {
    get => systemGeneratedIdentifier;
    set => systemGeneratedIdentifier = value;
  }

  /// <summary>Length of the CODE attribute.</summary>
  public const int Code_MaxLength = 8;

  /// <summary>
  /// The value of the CODE attribute.
  /// A short representation of the name for the purpose of quick 
  /// identification.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 2, Type = MemberType.Char, Length = Code_MaxLength)]
  public string Code
  {
    get => code ?? "";
    set => code = TrimEnd(Substring(value, 1, Code_MaxLength));
  }

  /// <summary>
  /// The json value of the Code attribute.</summary>
  [JsonPropertyName("code")]
  [Computed]
  public string Code_Json
  {
    get => NullIf(Code, "");
    set => Code = value;
  }

  /// <summary>Length of the NAME attribute.</summary>
  public const int Name_MaxLength = 40;

  /// <summary>
  /// The value of the NAME attribute.
  /// The name of the code.  This name will be more descriptive than the code 
  /// but will be much less shorter than the description.  The reason for
  /// needing the name is that the code is not descriptive enough and the
  /// description is too long for the list screens.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 3, Type = MemberType.Char, Length = Name_MaxLength)]
  public string Name
  {
    get => name ?? "";
    set => name = TrimEnd(Substring(value, 1, Name_MaxLength));
  }

  /// <summary>
  /// The json value of the Name attribute.</summary>
  [JsonPropertyName("name")]
  [Computed]
  public string Name_Json
  {
    get => NullIf(Name, "");
    set => Name = value;
  }

  /// <summary>Length of the CURRENT_ARREARS_IND attribute.</summary>
  public const int CurrentArrearsInd_MaxLength = 1;

  /// <summary>
  /// The value of the CURRENT_ARREARS_IND attribute.
  /// This indicator determines if the disbursement is for a collection applied 
  /// to a current debt or a debt in arrears.
  /// </summary>
  [JsonPropertyName("currentArrearsInd")]
  [Member(Index = 4, Type = MemberType.Char, Length
    = CurrentArrearsInd_MaxLength, Optional = true)]
  public string CurrentArrearsInd
  {
    get => currentArrearsInd;
    set => currentArrearsInd = value != null
      ? TrimEnd(Substring(value, 1, CurrentArrearsInd_MaxLength)) : null;
  }

  /// <summary>Length of the PROGRAM_CODE attribute.</summary>
  public const int ProgramCode_MaxLength = 2;

  /// <summary>
  /// The value of the PROGRAM_CODE attribute.
  /// The AFDC Reimbursement Inidicator is used to show that the money is due to
  /// AFDC because the  child is on AFDC.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 5, Type = MemberType.Char, Length = ProgramCode_MaxLength)]
  public string ProgramCode
  {
    get => programCode ?? "";
    set => programCode = TrimEnd(Substring(value, 1, ProgramCode_MaxLength));
  }

  /// <summary>
  /// The json value of the ProgramCode attribute.</summary>
  [JsonPropertyName("programCode")]
  [Computed]
  public string ProgramCode_Json
  {
    get => NullIf(ProgramCode, "");
    set => ProgramCode = value;
  }

  /// <summary>Length of the RECAPTURE_IND attribute.</summary>
  public const int RecaptureInd_MaxLength = 1;

  /// <summary>
  /// The value of the RECAPTURE_IND attribute.
  /// This attribute is used to show that a disbursement was recaptured to be 
  /// applied against a recovery debt owed by the Payee.
  /// </summary>
  [JsonPropertyName("recaptureInd")]
  [Member(Index = 6, Type = MemberType.Char, Length = RecaptureInd_MaxLength, Optional
    = true)]
  public string RecaptureInd
  {
    get => recaptureInd;
    set => recaptureInd = value != null
      ? TrimEnd(Substring(value, 1, RecaptureInd_MaxLength)) : null;
  }

  /// <summary>Length of the CASH_NON_CASH_IND attribute.</summary>
  public const int CashNonCashInd_MaxLength = 1;

  /// <summary>
  /// The value of the CASH_NON_CASH_IND attribute.
  /// The cash non cash indicator is used to show if this disbursement will 
  /// actually cause any money to go out or if it is just informational about
  /// money that has already been disbursed.  An example is when the AP sends
  /// the check directly to the AR.  In this case the disbursement would be non
  /// cash because it is informational only.
  /// Some disbursements are always cash such as Collection Agency Fees and AFDC
  /// Cost Recovery Fees.
  /// </summary>
  [JsonPropertyName("cashNonCashInd")]
  [Member(Index = 7, Type = MemberType.Char, Length
    = CashNonCashInd_MaxLength, Optional = true)]
  public string CashNonCashInd
  {
    get => cashNonCashInd;
    set => cashNonCashInd = value != null
      ? TrimEnd(Substring(value, 1, CashNonCashInd_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the EFFECTIVE_DATE attribute.
  /// The date upon which this occurrence is activated by the system.  An 
  /// effective date allows the entity to be entered into the system with a
  /// future date.  The occurrence of the entity will &quot;take effect&quot; on
  /// the effective date.
  /// </summary>
  [JsonPropertyName("effectiveDate")]
  [Member(Index = 8, Type = MemberType.Date)]
  public DateTime? EffectiveDate
  {
    get => effectiveDate;
    set => effectiveDate = value;
  }

  /// <summary>
  /// The value of the DISCONTINUE_DATE attribute.
  /// The date upon which this occurrence of the entity can no longer be used.
  /// </summary>
  [JsonPropertyName("discontinueDate")]
  [Member(Index = 9, Type = MemberType.Date, Optional = true)]
  public DateTime? DiscontinueDate
  {
    get => discontinueDate;
    set => discontinueDate = value;
  }

  /// <summary>Length of the CREATED_BY attribute.</summary>
  public const int CreatedBy_MaxLength = 8;

  /// <summary>
  /// The value of the CREATED_BY attribute.
  /// The User ID or Program ID responsible for the creation of the occurrence.
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
  /// The timestamp the occurrence was created.
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
  /// The User ID or Program ID responsible for the last update to the 
  /// occurrence.
  /// </summary>
  [JsonPropertyName("lastUpdatedBy")]
  [Member(Index = 12, Type = MemberType.Char, Length
    = LastUpdatedBy_MaxLength, Optional = true)]
  public string LastUpdatedBy
  {
    get => lastUpdatedBy;
    set => lastUpdatedBy = value != null
      ? TrimEnd(Substring(value, 1, LastUpdatedBy_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the LAST_UPDATED_TMST attribute.
  /// The timestamp of the most recent update to the occurrence.
  /// </summary>
  [JsonPropertyName("lastUpdatedTmst")]
  [Member(Index = 13, Type = MemberType.Timestamp, Optional = true)]
  public DateTime? LastUpdatedTmst
  {
    get => lastUpdatedTmst;
    set => lastUpdatedTmst = value;
  }

  /// <summary>Length of the DESCRIPTION attribute.</summary>
  public const int Description_MaxLength = 240;

  /// <summary>
  /// The value of the DESCRIPTION attribute.
  /// An explanation of the business function.  The description should be 
  /// specific enough to allow a person to distinguish/understand the business
  /// function.
  /// </summary>
  [JsonPropertyName("description")]
  [Member(Index = 14, Type = MemberType.Varchar, Length
    = Description_MaxLength, Optional = true)]
  public string Description
  {
    get => description;
    set => description = value != null
      ? Substring(value, 1, Description_MaxLength) : null;
  }

  /// <summary>Length of the RECOVERY_TYPE attribute.</summary>
  public const int RecoveryType_MaxLength = 1;

  /// <summary>
  /// The value of the RECOVERY_TYPE attribute.
  /// This attribute specifies the type of the recovery debit if the 
  /// disbursement type is of recovery in nature. We need to be able to identify
  /// the nature of recovery type debits from the debits that get disbursed to
  /// facilitate the processing like recapture.
  /// </summary>
  [JsonPropertyName("recoveryType")]
  [Member(Index = 15, Type = MemberType.Char, Length = RecoveryType_MaxLength, Optional
    = true)]
  public string RecoveryType
  {
    get => recoveryType;
    set => recoveryType = value != null
      ? TrimEnd(Substring(value, 1, RecoveryType_MaxLength)) : null;
  }

  private int systemGeneratedIdentifier;
  private string code;
  private string name;
  private string currentArrearsInd;
  private string programCode;
  private string recaptureInd;
  private string cashNonCashInd;
  private DateTime? effectiveDate;
  private DateTime? discontinueDate;
  private string createdBy;
  private DateTime? createdTimestamp;
  private string lastUpdatedBy;
  private DateTime? lastUpdatedTmst;
  private string description;
  private string recoveryType;
}
