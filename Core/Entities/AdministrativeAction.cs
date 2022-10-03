// The source file: ADMINISTRATIVE_ACTION, ID: 371430353, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Entities;

/// <summary>
/// RESP: LGLENFAC
/// This entity contains the types of administrative enforcement actions that 
/// can be taken for an obligation.
/// </summary>
[Serializable]
public partial class AdministrativeAction: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public AdministrativeAction()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public AdministrativeAction(AdministrativeAction that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new AdministrativeAction Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(AdministrativeAction that)
  {
    base.Assign(that);
    type1 = that.type1;
    description = that.description;
    indicator = that.indicator;
    createdBy = that.createdBy;
    createdTstamp = that.createdTstamp;
    lastUpdatedBy = that.lastUpdatedBy;
    lastUpdatedTstamp = that.lastUpdatedTstamp;
  }

  /// <summary>Length of the TYPE attribute.</summary>
  public const int Type1_MaxLength = 4;

  /// <summary>
  /// The value of the TYPE attribute.
  /// This identifies the type of enforcement that can be taken.
  /// Examples are:
  ///      SDSO
  ///      FDSO
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 1, Type = MemberType.Char, Length = Type1_MaxLength)]
  public string Type1
  {
    get => type1 ?? "";
    set => type1 = TrimEnd(Substring(value, 1, Type1_MaxLength));
  }

  /// <summary>
  /// The json value of the Type1 attribute.</summary>
  [JsonPropertyName("type1")]
  [Computed]
  public string Type1_Json
  {
    get => NullIf(Type1, "");
    set => Type1 = value;
  }

  /// <summary>Length of the DESCRIPTION attribute.</summary>
  public const int Description_MaxLength = 30;

  /// <summary>
  /// The value of the DESCRIPTION attribute.
  /// This is a description of the type of enforcement action.
  /// Examples include Demand Letter, Phone Call, SDSO, FDSO, Referral to Credit
  /// Agency, Credit Reporting, Military Allotment, Interstate Enforcement
  /// Request, and Work Release.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 2, Type = MemberType.Char, Length = Description_MaxLength)]
  public string Description
  {
    get => description ?? "";
    set => description = TrimEnd(Substring(value, 1, Description_MaxLength));
  }

  /// <summary>
  /// The json value of the Description attribute.</summary>
  [JsonPropertyName("description")]
  [Computed]
  public string Description_Json
  {
    get => NullIf(Description, "");
    set => Description = value;
  }

  /// <summary>Length of the INDICATOR attribute.</summary>
  public const int Indicator_MaxLength = 1;

  /// <summary>
  /// The value of the INDICATOR attribute.
  /// RESP:  LGLENFAC
  /// This is a coded attribute which indicates whether or not the 
  /// Administrative Action is enforced by automatic means (batch processing) or
  /// must be done manually.
  /// Valid codes are:
  ///      A - Automatic
  ///      M - Manual
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 3, Type = MemberType.Char, Length = Indicator_MaxLength)]
  public string Indicator
  {
    get => indicator ?? "";
    set => indicator = TrimEnd(Substring(value, 1, Indicator_MaxLength));
  }

  /// <summary>
  /// The json value of the Indicator attribute.</summary>
  [JsonPropertyName("indicator")]
  [Computed]
  public string Indicator_Json
  {
    get => NullIf(Indicator, "");
    set => Indicator = value;
  }

  /// <summary>Length of the CREATED_BY attribute.</summary>
  public const int CreatedBy_MaxLength = 8;

  /// <summary>
  /// The value of the CREATED_BY attribute.
  /// The signon of the person that created the occurrance of the entity.
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
  /// The value of the CREATED_TSTAMP attribute.
  /// The date and time that the occurrance of the entity was created.
  /// </summary>
  [JsonPropertyName("createdTstamp")]
  [Member(Index = 5, Type = MemberType.Timestamp)]
  public DateTime? CreatedTstamp
  {
    get => createdTstamp;
    set => createdTstamp = value;
  }

  /// <summary>Length of the LAST_UPDATED_BY attribute.</summary>
  public const int LastUpdatedBy_MaxLength = 8;

  /// <summary>
  /// The value of the LAST_UPDATED_BY attribute.
  /// The signon of the person that last updated the occurrance of the entity.
  /// </summary>
  [JsonPropertyName("lastUpdatedBy")]
  [Member(Index = 6, Type = MemberType.Char, Length = LastUpdatedBy_MaxLength, Optional
    = true)]
  public string LastUpdatedBy
  {
    get => lastUpdatedBy;
    set => lastUpdatedBy = value != null
      ? TrimEnd(Substring(value, 1, LastUpdatedBy_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the LAST_UPDATED_TSTAMP attribute.
  /// The date and time that the occurrance of the entity was last updated.
  /// </summary>
  [JsonPropertyName("lastUpdatedTstamp")]
  [Member(Index = 7, Type = MemberType.Timestamp, Optional = true)]
  public DateTime? LastUpdatedTstamp
  {
    get => lastUpdatedTstamp;
    set => lastUpdatedTstamp = value;
  }

  private string type1;
  private string description;
  private string indicator;
  private string createdBy;
  private DateTime? createdTstamp;
  private string lastUpdatedBy;
  private DateTime? lastUpdatedTstamp;
}
