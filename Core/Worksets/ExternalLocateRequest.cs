// The source file: EXTERNAL_LOCATE_REQUEST, ID: 374418424, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Worksets;

[Serializable]
public partial class ExternalLocateRequest: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public ExternalLocateRequest()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public ExternalLocateRequest(ExternalLocateRequest that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new ExternalLocateRequest Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(ExternalLocateRequest that)
  {
    base.Assign(that);
    requestDate = that.requestDate;
    sequenceNumber = that.sequenceNumber;
    agencyNumber = that.agencyNumber;
    csePersonNumber = that.csePersonNumber;
    dateOfBirth = that.dateOfBirth;
    ssn = that.ssn;
    suspensionStatus = that.suspensionStatus;
  }

  /// <summary>
  /// The value of the REQUEST_DATE attribute.
  /// </summary>
  [JsonPropertyName("requestDate")]
  [Member(Index = 1, Type = MemberType.Date)]
  public DateTime? RequestDate
  {
    get => requestDate;
    set => requestDate = value;
  }

  /// <summary>
  /// The value of the SEQUENCE_NUMBER attribute.
  /// </summary>
  [JsonPropertyName("sequenceNumber")]
  [DefaultValue(0)]
  [Member(Index = 2, Type = MemberType.Number, Length = 2)]
  public int SequenceNumber
  {
    get => sequenceNumber;
    set => sequenceNumber = value;
  }

  /// <summary>Length of the AGENCY_NUMBER attribute.</summary>
  public const int AgencyNumber_MaxLength = 5;

  /// <summary>
  /// The value of the AGENCY_NUMBER attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 3, Type = MemberType.Char, Length = AgencyNumber_MaxLength)]
  public string AgencyNumber
  {
    get => agencyNumber ?? "";
    set => agencyNumber = TrimEnd(Substring(value, 1, AgencyNumber_MaxLength));
  }

  /// <summary>
  /// The json value of the AgencyNumber attribute.</summary>
  [JsonPropertyName("agencyNumber")]
  [Computed]
  public string AgencyNumber_Json
  {
    get => NullIf(AgencyNumber, "");
    set => AgencyNumber = value;
  }

  /// <summary>Length of the CSE_PERSON_NUMBER attribute.</summary>
  public const int CsePersonNumber_MaxLength = 10;

  /// <summary>
  /// The value of the CSE_PERSON_NUMBER attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 4, Type = MemberType.Char, Length = CsePersonNumber_MaxLength)]
    
  public string CsePersonNumber
  {
    get => csePersonNumber ?? "";
    set => csePersonNumber =
      TrimEnd(Substring(value, 1, CsePersonNumber_MaxLength));
  }

  /// <summary>
  /// The json value of the CsePersonNumber attribute.</summary>
  [JsonPropertyName("csePersonNumber")]
  [Computed]
  public string CsePersonNumber_Json
  {
    get => NullIf(CsePersonNumber, "");
    set => CsePersonNumber = value;
  }

  /// <summary>
  /// The value of the DATE_OF_BIRTH attribute.
  /// </summary>
  [JsonPropertyName("dateOfBirth")]
  [Member(Index = 5, Type = MemberType.Date)]
  public DateTime? DateOfBirth
  {
    get => dateOfBirth;
    set => dateOfBirth = value;
  }

  /// <summary>Length of the SSN attribute.</summary>
  public const int Ssn_MaxLength = 9;

  /// <summary>
  /// The value of the SSN attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 6, Type = MemberType.Char, Length = Ssn_MaxLength)]
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

  /// <summary>Length of the SUSPENSION_STATUS attribute.</summary>
  public const int SuspensionStatus_MaxLength = 1;

  /// <summary>
  /// The value of the SUSPENSION_STATUS attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 7, Type = MemberType.Char, Length = SuspensionStatus_MaxLength)
    ]
  public string SuspensionStatus
  {
    get => suspensionStatus ?? "";
    set => suspensionStatus =
      TrimEnd(Substring(value, 1, SuspensionStatus_MaxLength));
  }

  /// <summary>
  /// The json value of the SuspensionStatus attribute.</summary>
  [JsonPropertyName("suspensionStatus")]
  [Computed]
  public string SuspensionStatus_Json
  {
    get => NullIf(SuspensionStatus, "");
    set => SuspensionStatus = value;
  }

  private DateTime? requestDate;
  private int sequenceNumber;
  private string agencyNumber;
  private string csePersonNumber;
  private DateTime? dateOfBirth;
  private string ssn;
  private string suspensionStatus;
}
