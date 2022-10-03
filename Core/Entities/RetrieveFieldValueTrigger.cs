// The source file: RETRIEVE_FIELD_VALUE_TRIGGER, ID: 372952404, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Entities;

/// <summary>
/// RESP:  SRVPLAN
/// 
/// Used as input to Field_Value_Retrieval batch Program, to
/// retrieve Field values from Archival file.
/// </summary>
[Serializable]
public partial class RetrieveFieldValueTrigger: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public RetrieveFieldValueTrigger()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public RetrieveFieldValueTrigger(RetrieveFieldValueTrigger that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new RetrieveFieldValueTrigger Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(RetrieveFieldValueTrigger that)
  {
    base.Assign(that);
    archiveDate = that.archiveDate;
    infId = that.infId;
    serviceProviderLogonId = that.serviceProviderLogonId;
    createdBy = that.createdBy;
    createdTimestamp = that.createdTimestamp;
  }

  /// <summary>
  /// The value of the ARCHIVE_DATE attribute.
  /// The date Field_values are archived.
  /// </summary>
  [JsonPropertyName("archiveDate")]
  [Member(Index = 1, Type = MemberType.Date)]
  public DateTime? ArchiveDate
  {
    get => archiveDate;
    set => archiveDate = value;
  }

  /// <summary>
  /// The value of the INF_ID attribute.
  /// Identifier for this table, Infrastructure identifier.
  /// </summary>
  [JsonPropertyName("infId")]
  [DefaultValue(0)]
  [Member(Index = 2, Type = MemberType.Number, Length = 9)]
  public int InfId
  {
    get => infId;
    set => infId = value;
  }

  /// <summary>Length of the SERVICE_PROVIDER_LOGON_ID attribute.</summary>
  public const int ServiceProviderLogonId_MaxLength = 8;

  /// <summary>
  /// The value of the SERVICE_PROVIDER_LOGON_ID attribute.
  /// Service Provider, who created request for retrieval of Field Values.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 3, Type = MemberType.Char, Length
    = ServiceProviderLogonId_MaxLength)]
  public string ServiceProviderLogonId
  {
    get => serviceProviderLogonId ?? "";
    set => serviceProviderLogonId =
      TrimEnd(Substring(value, 1, ServiceProviderLogonId_MaxLength));
  }

  /// <summary>
  /// The json value of the ServiceProviderLogonId attribute.</summary>
  [JsonPropertyName("serviceProviderLogonId")]
  [Computed]
  public string ServiceProviderLogonId_Json
  {
    get => NullIf(ServiceProviderLogonId, "");
    set => ServiceProviderLogonId = value;
  }

  /// <summary>Length of the CREATED_BY attribute.</summary>
  public const int CreatedBy_MaxLength = 8;

  /// <summary>
  /// The value of the CREATED_BY attribute.
  /// The user who requested retrieval of field Values.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 4, Type = MemberType.Char, Length = CreatedBy_MaxLength)]
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
  /// Request Created time stamp.
  /// </summary>
  [JsonPropertyName("createdTimestamp")]
  [Member(Index = 5, Type = MemberType.Timestamp)]
  public DateTime? CreatedTimestamp
  {
    get => createdTimestamp;
    set => createdTimestamp = value;
  }

  private DateTime? archiveDate;
  private int infId;
  private string serviceProviderLogonId;
  private string createdBy;
  private DateTime? createdTimestamp;
}
