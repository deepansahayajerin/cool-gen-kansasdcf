// The source file: CSENET_10_DAY_EXTRACT, ID: 372945042, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Worksets;

[Serializable]
public partial class Csenet10DayExtract2: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public Csenet10DayExtract2()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public Csenet10DayExtract2(Csenet10DayExtract2 that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new Csenet10DayExtract2 Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(Csenet10DayExtract2 that)
  {
    base.Assign(that);
    office = that.office;
    serviceProviderName = that.serviceProviderName;
    referralNumber = that.referralNumber;
    startDate = that.startDate;
    nonComplianceDate = that.nonComplianceDate;
  }

  /// <summary>Length of the OFFICE attribute.</summary>
  public const int Office_MaxLength = 30;

  /// <summary>
  /// The value of the OFFICE attribute.
  /// The name of the CSE office.
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

  /// <summary>Length of the SERVICE_PROVIDER_NAME attribute.</summary>
  public const int ServiceProviderName_MaxLength = 30;

  /// <summary>
  /// The value of the SERVICE_PROVIDER_NAME attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 2, Type = MemberType.Char, Length
    = ServiceProviderName_MaxLength)]
  public string ServiceProviderName
  {
    get => serviceProviderName ?? "";
    set => serviceProviderName =
      TrimEnd(Substring(value, 1, ServiceProviderName_MaxLength));
  }

  /// <summary>
  /// The json value of the ServiceProviderName attribute.</summary>
  [JsonPropertyName("serviceProviderName")]
  [Computed]
  public string ServiceProviderName_Json
  {
    get => NullIf(ServiceProviderName, "");
    set => ServiceProviderName = value;
  }

  /// <summary>
  /// The value of the REFERRAL_NUMBER attribute.
  /// This is a unique number assigned to each CSENet transaction.  It has no 
  /// place in the KESSEP system but is required to provide a key used to
  /// process CSENet Referrals.
  /// </summary>
  [JsonPropertyName("referralNumber")]
  [DefaultValue(0L)]
  [Member(Index = 3, Type = MemberType.Number, Length = 12)]
  public long ReferralNumber
  {
    get => referralNumber;
    set => referralNumber = value;
  }

  /// <summary>
  /// The value of the START_DATE attribute.
  /// This is the date on which CSENet transmitted the Referral.
  /// </summary>
  [JsonPropertyName("startDate")]
  [Member(Index = 4, Type = MemberType.Date)]
  public DateTime? StartDate
  {
    get => startDate;
    set => startDate = value;
  }

  /// <summary>
  /// The value of the NON_COMPLIANCE_DATE attribute.
  /// Federal Non Compliance Date is the derived date based on the activity 
  /// create timestamp and the federal non compliance days.
  /// This date represents the date by which the monitored activity is to be 
  /// completed.
  /// </summary>
  [JsonPropertyName("nonComplianceDate")]
  [Member(Index = 5, Type = MemberType.Date)]
  public DateTime? NonComplianceDate
  {
    get => nonComplianceDate;
    set => nonComplianceDate = value;
  }

  private string office;
  private string serviceProviderName;
  private long referralNumber;
  private DateTime? startDate;
  private DateTime? nonComplianceDate;
}
