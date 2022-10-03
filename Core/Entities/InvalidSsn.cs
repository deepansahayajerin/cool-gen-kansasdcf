// The source file: INVALID_SSN, ID: 371153559, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Entities;

/// <summary>
/// Stores each combination pair of invalid SSN and CSE Person Number so that 
/// the system will not use them in the future.
/// </summary>
[Serializable]
public partial class InvalidSsn: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public InvalidSsn()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public InvalidSsn(InvalidSsn that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new InvalidSsn Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(InvalidSsn that)
  {
    base.Assign(that);
    ssn = that.ssn;
    fcrSentDate = that.fcrSentDate;
    nextCheckDate = that.nextCheckDate;
    fcrProcessInd = that.fcrProcessInd;
    createdBy = that.createdBy;
    createdTstamp = that.createdTstamp;
    cspNumber = that.cspNumber;
  }

  /// <summary>
  /// The value of the SSN attribute.
  /// The INVALID social security number attributed to this client.
  /// </summary>
  [JsonPropertyName("ssn")]
  [DefaultValue(0)]
  [Member(Index = 1, Type = MemberType.Number, Length = 9)]
  public int Ssn
  {
    get => ssn;
    set => ssn = value;
  }

  /// <summary>
  /// The value of the FCR_SENT_DATE attribute.
  /// The date this SSN-Person Number combination was sent to FCR as invalid.
  /// </summary>
  [JsonPropertyName("fcrSentDate")]
  [Member(Index = 2, Type = MemberType.Date, Optional = true)]
  public DateTime? FcrSentDate
  {
    get => fcrSentDate;
    set => fcrSentDate = value;
  }

  /// <summary>
  /// The value of the NEXT_CHECK_DATE attribute.
  /// Date that purge program will check to determine if this record meets 
  /// deletion criteria.
  /// </summary>
  [JsonPropertyName("nextCheckDate")]
  [Member(Index = 3, Type = MemberType.Date, Optional = true)]
  public DateTime? NextCheckDate
  {
    get => nextCheckDate;
    set => nextCheckDate = value;
  }

  /// <summary>Length of the FCR_PROCESS_IND attribute.</summary>
  public const int FcrProcessInd_MaxLength = 1;

  /// <summary>
  /// The value of the FCR_PROCESS_IND attribute.
  /// Internal indicator for sending FCR records.
  /// </summary>
  [JsonPropertyName("fcrProcessInd")]
  [Member(Index = 4, Type = MemberType.Char, Length = FcrProcessInd_MaxLength, Optional
    = true)]
  public string FcrProcessInd
  {
    get => fcrProcessInd;
    set => fcrProcessInd = value != null
      ? TrimEnd(Substring(value, 1, FcrProcessInd_MaxLength)) : null;
  }

  /// <summary>Length of the CREATED_BY attribute.</summary>
  public const int CreatedBy_MaxLength = 8;

  /// <summary>
  /// The value of the CREATED_BY attribute.
  /// The signon of the person that created the occurrance of the entity.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 5, Type = MemberType.Char, Length = CreatedBy_MaxLength)]
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
  /// The value of the CREATED_TSTAMP attribute.
  /// The date and time that the occurrance of the entity was created.
  /// </summary>
  [JsonPropertyName("createdTstamp")]
  [Member(Index = 6, Type = MemberType.Timestamp)]
  public DateTime? CreatedTstamp
  {
    get => createdTstamp;
    set => createdTstamp = value;
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
  [Member(Index = 7, Type = MemberType.Char, Length = CspNumber_MaxLength)]
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

  private int ssn;
  private DateTime? fcrSentDate;
  private DateTime? nextCheckDate;
  private string fcrProcessInd;
  private string createdBy;
  private DateTime? createdTstamp;
  private string cspNumber;
}
