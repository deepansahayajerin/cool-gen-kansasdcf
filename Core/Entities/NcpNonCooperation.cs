// The source file: NCP_NON_COOPERATION, ID: 1902537242, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Entities;

/// <summary>
/// This table will store both active and inactive 	non cooperation records for 
/// an NCP.To manage an NCP who is receiving food assistance.
/// </summary>
[Serializable]
public partial class NcpNonCooperation: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public NcpNonCooperation()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public NcpNonCooperation(NcpNonCooperation that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new NcpNonCooperation Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(NcpNonCooperation that)
  {
    base.Assign(that);
    ncpStatusCode = that.ncpStatusCode;
    effectiveDate = that.effectiveDate;
    reasonCode = that.reasonCode;
    letter1Date = that.letter1Date;
    letter1Code = that.letter1Code;
    letter2Date = that.letter2Date;
    letter2Code = that.letter2Code;
    phone1Date = that.phone1Date;
    phone1Code = that.phone1Code;
    phone2Date = that.phone2Date;
    phone2Code = that.phone2Code;
    endDate = that.endDate;
    endStatusCode = that.endStatusCode;
    createdBy = that.createdBy;
    createdTimestamp = that.createdTimestamp;
    lastUpdatedBy = that.lastUpdatedBy;
    lastUpdatedTimestamp = that.lastUpdatedTimestamp;
    note = that.note;
    casNumber = that.casNumber;
    cspNumber = that.cspNumber;
  }

  /// <summary>Length of the NCP_STATUS_CODE attribute.</summary>
  public const int NcpStatusCode_MaxLength = 1;

  /// <summary>
  /// The value of the NCP_STATUS_CODE attribute.
  /// The status the non-cooperation is currently in.
  /// </summary>
  [JsonPropertyName("ncpStatusCode")]
  [Member(Index = 1, Type = MemberType.Char, Length = NcpStatusCode_MaxLength, Optional
    = true)]
  public string NcpStatusCode
  {
    get => ncpStatusCode;
    set => ncpStatusCode = value != null
      ? TrimEnd(Substring(value, 1, NcpStatusCode_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the EFFECTIVE_DATE attribute.
  /// The date that this record became non-cooperation.
  /// </summary>
  [JsonPropertyName("effectiveDate")]
  [Member(Index = 2, Type = MemberType.Date, Optional = true)]
  public DateTime? EffectiveDate
  {
    get => effectiveDate;
    set => effectiveDate = value;
  }

  /// <summary>Length of the REASON_CODE attribute.</summary>
  public const int ReasonCode_MaxLength = 2;

  /// <summary>
  /// The value of the REASON_CODE attribute.
  /// The reason that this record became non-cooperation.
  /// </summary>
  [JsonPropertyName("reasonCode")]
  [Member(Index = 3, Type = MemberType.Char, Length = ReasonCode_MaxLength, Optional
    = true)]
  public string ReasonCode
  {
    get => reasonCode;
    set => reasonCode = value != null
      ? TrimEnd(Substring(value, 1, ReasonCode_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the LETTER_1_DATE attribute.
  /// The date that the first letter was sent to the NCP.
  /// </summary>
  [JsonPropertyName("letter1Date")]
  [Member(Index = 4, Type = MemberType.Date, Optional = true)]
  public DateTime? Letter1Date
  {
    get => letter1Date;
    set => letter1Date = value;
  }

  /// <summary>Length of the LETTER_1_CODE attribute.</summary>
  public const int Letter1Code_MaxLength = 2;

  /// <summary>
  /// The value of the LETTER_1_CODE attribute.
  /// The result from the letter being sent.
  /// </summary>
  [JsonPropertyName("letter1Code")]
  [Member(Index = 5, Type = MemberType.Char, Length = Letter1Code_MaxLength, Optional
    = true)]
  public string Letter1Code
  {
    get => letter1Code;
    set => letter1Code = value != null
      ? TrimEnd(Substring(value, 1, Letter1Code_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the LETTER_2_DATE attribute.
  /// The date that the second letter was sent to the NCP.
  /// </summary>
  [JsonPropertyName("letter2Date")]
  [Member(Index = 6, Type = MemberType.Date, Optional = true)]
  public DateTime? Letter2Date
  {
    get => letter2Date;
    set => letter2Date = value;
  }

  /// <summary>Length of the LETTER_2_CODE attribute.</summary>
  public const int Letter2Code_MaxLength = 2;

  /// <summary>
  /// The value of the LETTER_2_CODE attribute.
  /// The result from the letter being sent.
  /// </summary>
  [JsonPropertyName("letter2Code")]
  [Member(Index = 7, Type = MemberType.Char, Length = Letter2Code_MaxLength, Optional
    = true)]
  public string Letter2Code
  {
    get => letter2Code;
    set => letter2Code = value != null
      ? TrimEnd(Substring(value, 1, Letter2Code_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the PHONE_1_DATE attribute.
  /// The date that the first phone call was made to
  /// </summary>
  [JsonPropertyName("phone1Date")]
  [Member(Index = 8, Type = MemberType.Date, Optional = true)]
  public DateTime? Phone1Date
  {
    get => phone1Date;
    set => phone1Date = value;
  }

  /// <summary>Length of the PHONE_1_CODE attribute.</summary>
  public const int Phone1Code_MaxLength = 2;

  /// <summary>
  /// The value of the PHONE_1_CODE attribute.
  /// The result from the phone call.
  /// </summary>
  [JsonPropertyName("phone1Code")]
  [Member(Index = 9, Type = MemberType.Char, Length = Phone1Code_MaxLength, Optional
    = true)]
  public string Phone1Code
  {
    get => phone1Code;
    set => phone1Code = value != null
      ? TrimEnd(Substring(value, 1, Phone1Code_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the PHONE_2_DATE attribute.
  /// The date that the second phone call was made to the NCP.
  /// </summary>
  [JsonPropertyName("phone2Date")]
  [Member(Index = 10, Type = MemberType.Date, Optional = true)]
  public DateTime? Phone2Date
  {
    get => phone2Date;
    set => phone2Date = value;
  }

  /// <summary>Length of the PHONE_2_CODE attribute.</summary>
  public const int Phone2Code_MaxLength = 2;

  /// <summary>
  /// The value of the PHONE_2_CODE attribute.
  /// The result from the phone call.
  /// </summary>
  [JsonPropertyName("phone2Code")]
  [Member(Index = 11, Type = MemberType.Char, Length = Phone2Code_MaxLength, Optional
    = true)]
  public string Phone2Code
  {
    get => phone2Code;
    set => phone2Code = value != null
      ? TrimEnd(Substring(value, 1, Phone2Code_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the END_DATE attribute.
  /// The date that this record stop being in non-cooperation status.
  /// </summary>
  [JsonPropertyName("endDate")]
  [Member(Index = 12, Type = MemberType.Date, Optional = true)]
  public DateTime? EndDate
  {
    get => endDate;
    set => endDate = value;
  }

  /// <summary>Length of the END_STATUS_CODE attribute.</summary>
  public const int EndStatusCode_MaxLength = 2;

  /// <summary>
  /// The value of the END_STATUS_CODE attribute.
  /// Reason that this record was able to be removed from non-cooperaton status.
  /// </summary>
  [JsonPropertyName("endStatusCode")]
  [Member(Index = 13, Type = MemberType.Char, Length
    = EndStatusCode_MaxLength, Optional = true)]
  public string EndStatusCode
  {
    get => endStatusCode;
    set => endStatusCode = value != null
      ? TrimEnd(Substring(value, 1, EndStatusCode_MaxLength)) : null;
  }

  /// <summary>Length of the CREATED_BY attribute.</summary>
  public const int CreatedBy_MaxLength = 8;

  /// <summary>
  /// The value of the CREATED_BY attribute.
  /// USER ID or Program ID responsible for creation of the occurrence.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 14, Type = MemberType.Char, Length = CreatedBy_MaxLength)]
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
  /// 	
  /// </summary>
  [JsonPropertyName("createdTimestamp")]
  [Member(Index = 15, Type = MemberType.Timestamp)]
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
  [JsonPropertyName("lastUpdatedBy")]
  [Member(Index = 16, Type = MemberType.Char, Length
    = LastUpdatedBy_MaxLength, Optional = true)]
  public string LastUpdatedBy
  {
    get => lastUpdatedBy;
    set => lastUpdatedBy = value != null
      ? TrimEnd(Substring(value, 1, LastUpdatedBy_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the LAST_UPDATED_TIMESTAMP attribute.
  /// Timestamp of last update of the occurrence.
  /// </summary>
  [JsonPropertyName("lastUpdatedTimestamp")]
  [Member(Index = 17, Type = MemberType.Timestamp, Optional = true)]
  public DateTime? LastUpdatedTimestamp
  {
    get => lastUpdatedTimestamp;
    set => lastUpdatedTimestamp = value;
  }

  /// <summary>Length of the NOTE attribute.</summary>
  public const int Note_MaxLength = 72;

  /// <summary>
  /// The value of the NOTE attribute.
  /// The result of the phone call
  /// </summary>
  [JsonPropertyName("note")]
  [Member(Index = 18, Type = MemberType.Varchar, Length = Note_MaxLength, Optional
    = true)]
  public string Note
  {
    get => note;
    set => note = value != null ? Substring(value, 1, Note_MaxLength) : null;
  }

  /// <summary>Length of the NUMBER attribute.</summary>
  public const int CasNumber_MaxLength = 10;

  /// <summary>
  /// The value of the NUMBER attribute.
  /// A unique number assigned to a request or referral.  Becomes the Case 
  /// number.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 19, Type = MemberType.Char, Length = CasNumber_MaxLength)]
  public string CasNumber
  {
    get => casNumber ?? "";
    set => casNumber = TrimEnd(Substring(value, 1, CasNumber_MaxLength));
  }

  /// <summary>
  /// The json value of the CasNumber attribute.</summary>
  [JsonPropertyName("casNumber")]
  [Computed]
  public string CasNumber_Json
  {
    get => NullIf(CasNumber, "");
    set => CasNumber = value;
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
  [Member(Index = 20, Type = MemberType.Char, Length = CspNumber_MaxLength)]
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

  private string ncpStatusCode;
  private DateTime? effectiveDate;
  private string reasonCode;
  private DateTime? letter1Date;
  private string letter1Code;
  private DateTime? letter2Date;
  private string letter2Code;
  private DateTime? phone1Date;
  private string phone1Code;
  private DateTime? phone2Date;
  private string phone2Code;
  private DateTime? endDate;
  private string endStatusCode;
  private string createdBy;
  private DateTime? createdTimestamp;
  private string lastUpdatedBy;
  private DateTime? lastUpdatedTimestamp;
  private string note;
  private string casNumber;
  private string cspNumber;
}
