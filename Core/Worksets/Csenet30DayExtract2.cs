// The source file: CSENET_30_DAY_EXTRACT, ID: 372945048, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Worksets;

[Serializable]
public partial class Csenet30DayExtract2: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public Csenet30DayExtract2()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public Csenet30DayExtract2(Csenet30DayExtract2 that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new Csenet30DayExtract2 Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(Csenet30DayExtract2 that)
  {
    base.Assign(that);
    office = that.office;
    serviceProvider = that.serviceProvider;
    receivedDate = that.receivedDate;
    referralNumber = that.referralNumber;
    referralType = that.referralType;
  }

  /// <summary>Length of the OFFICE attribute.</summary>
  public const int Office_MaxLength = 30;

  /// <summary>
  /// The value of the OFFICE attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 1, Type = MemberType.Char, Length = Office_MaxLength)]
  public string Office
  {
    get => office ?? "";
    set => office = TrimEnd(Substring(value, 1, Office_MaxLength));
  }

  /// <summary>
  /// The json value of the Office attribute.</summary>
  [JsonPropertyName("office")]
  [Computed]
  public string Office_Json
  {
    get => NullIf(Office, "");
    set => Office = value;
  }

  /// <summary>Length of the SERVICE_PROVIDER attribute.</summary>
  public const int ServiceProvider_MaxLength = 30;

  /// <summary>
  /// The value of the SERVICE_PROVIDER attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 2, Type = MemberType.Char, Length = ServiceProvider_MaxLength)]
    
  public string ServiceProvider
  {
    get => serviceProvider ?? "";
    set => serviceProvider =
      TrimEnd(Substring(value, 1, ServiceProvider_MaxLength));
  }

  /// <summary>
  /// The json value of the ServiceProvider attribute.</summary>
  [JsonPropertyName("serviceProvider")]
  [Computed]
  public string ServiceProvider_Json
  {
    get => NullIf(ServiceProvider, "");
    set => ServiceProvider = value;
  }

  /// <summary>
  /// The value of the RECEIVED_DATE attribute.
  /// </summary>
  [JsonPropertyName("receivedDate")]
  [Member(Index = 3, Type = MemberType.Date)]
  public DateTime? ReceivedDate
  {
    get => receivedDate;
    set => receivedDate = value;
  }

  /// <summary>
  /// The value of the REFERRAL_NUMBER attribute.
  /// </summary>
  [JsonPropertyName("referralNumber")]
  [DefaultValue(0L)]
  [Member(Index = 4, Type = MemberType.Number, Length = 12)]
  public long ReferralNumber
  {
    get => referralNumber;
    set => referralNumber = value;
  }

  /// <summary>Length of the REFERRAL_TYPE attribute.</summary>
  public const int ReferralType_MaxLength = 8;

  /// <summary>
  /// The value of the REFERRAL_TYPE attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 5, Type = MemberType.Char, Length = ReferralType_MaxLength)]
  public string ReferralType
  {
    get => referralType ?? "";
    set => referralType = TrimEnd(Substring(value, 1, ReferralType_MaxLength));
  }

  /// <summary>
  /// The json value of the ReferralType attribute.</summary>
  [JsonPropertyName("referralType")]
  [Computed]
  public string ReferralType_Json
  {
    get => NullIf(ReferralType, "");
    set => ReferralType = value;
  }

  private string office;
  private string serviceProvider;
  private DateTime? receivedDate;
  private long referralNumber;
  private string referralType;
}
