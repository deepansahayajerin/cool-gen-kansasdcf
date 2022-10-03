// The source file: DASHBOARD_NAME, ID: 1902433855, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Entities;

/// <summary>
/// This table provides a cross reference of User IDs and Contractor IDs to the 
/// appropriate person/organization name.  The dashboard tables store the user
/// id of workers to whom data applies and the contractor id of the contract
/// firm for whom they work.  This table will be used to lookup the names of
/// those workers and contract firms and the names will be displayed on various
/// Dashboard reports instead of the IDs.
/// </summary>
[Serializable]
public partial class DashboardName: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public DashboardName()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public DashboardName(DashboardName that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new DashboardName Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(DashboardName that)
  {
    base.Assign(that);
    providerId = that.providerId;
    providerType = that.providerType;
    orgOrLastName = that.orgOrLastName;
    firstName = that.firstName;
    middleInitial = that.middleInitial;
  }

  /// <summary>Length of the PROVIDER_ID attribute.</summary>
  public const int ProviderId_MaxLength = 8;

  /// <summary>
  /// The value of the PROVIDER_ID attribute.
  /// Identifier of the worker or contract firm to which the name applies.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 1, Type = MemberType.Char, Length = ProviderId_MaxLength)]
  public string ProviderId
  {
    get => providerId ?? "";
    set => providerId = TrimEnd(Substring(value, 1, ProviderId_MaxLength));
  }

  /// <summary>
  /// The json value of the ProviderId attribute.</summary>
  [JsonPropertyName("providerId")]
  [Computed]
  public string ProviderId_Json
  {
    get => NullIf(ProviderId, "");
    set => ProviderId = value;
  }

  /// <summary>Length of the PROVIDER_TYPE attribute.</summary>
  public const int ProviderType_MaxLength = 2;

  /// <summary>
  /// The value of the PROVIDER_TYPE attribute.
  /// The type of entity to which the name relates. For example: SP for Service 
  /// Provider, CT for Contractor firm.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 2, Type = MemberType.Char, Length = ProviderType_MaxLength)]
  public string ProviderType
  {
    get => providerType ?? "";
    set => providerType = TrimEnd(Substring(value, 1, ProviderType_MaxLength));
  }

  /// <summary>
  /// The json value of the ProviderType attribute.</summary>
  [JsonPropertyName("providerType")]
  [Computed]
  public string ProviderType_Json
  {
    get => NullIf(ProviderType, "");
    set => ProviderType = value;
  }

  /// <summary>Length of the ORG_OR_LAST_NAME attribute.</summary>
  public const int OrgOrLastName_MaxLength = 20;

  /// <summary>
  /// The value of the ORG_OR_LAST_NAME attribute.
  /// The last name of an individual or a full organization name.
  /// </summary>
  [JsonPropertyName("orgOrLastName")]
  [Member(Index = 3, Type = MemberType.Char, Length = OrgOrLastName_MaxLength, Optional
    = true)]
  public string OrgOrLastName
  {
    get => orgOrLastName;
    set => orgOrLastName = value != null
      ? TrimEnd(Substring(value, 1, OrgOrLastName_MaxLength)) : null;
  }

  /// <summary>Length of the FIRST_NAME attribute.</summary>
  public const int FirstName_MaxLength = 12;

  /// <summary>
  /// The value of the FIRST_NAME attribute.
  /// The first name of an individual.
  /// </summary>
  [JsonPropertyName("firstName")]
  [Member(Index = 4, Type = MemberType.Char, Length = FirstName_MaxLength, Optional
    = true)]
  public string FirstName
  {
    get => firstName;
    set => firstName = value != null
      ? TrimEnd(Substring(value, 1, FirstName_MaxLength)) : null;
  }

  /// <summary>Length of the MIDDLE_INITIAL attribute.</summary>
  public const int MiddleInitial_MaxLength = 1;

  /// <summary>
  /// The value of the MIDDLE_INITIAL attribute.
  /// The middle initial of an idividual.
  /// </summary>
  [JsonPropertyName("middleInitial")]
  [Member(Index = 5, Type = MemberType.Char, Length = MiddleInitial_MaxLength, Optional
    = true)]
  public string MiddleInitial
  {
    get => middleInitial;
    set => middleInitial = value != null
      ? TrimEnd(Substring(value, 1, MiddleInitial_MaxLength)) : null;
  }

  private string providerId;
  private string providerType;
  private string orgOrLastName;
  private string firstName;
  private string middleInitial;
}
