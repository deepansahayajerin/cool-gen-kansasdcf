// The source file: OE_WORK_GROUP, ID: 371796188, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Worksets;

/// <summary>
/// Resp - OBLGESTB
/// This work set contains the temp attributes used by Sid in his programs.
/// </summary>
[Serializable]
public partial class OeWorkGroup: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public OeWorkGroup()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public OeWorkGroup(OeWorkGroup that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new OeWorkGroup Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(OeWorkGroup that)
  {
    base.Assign(that);
    locationFips = that.locationFips;
    countyFips = that.countyFips;
    transSerialNumber = that.transSerialNumber;
    formattedNameText = that.formattedNameText;
    percent = that.percent;
  }

  /// <summary>Length of the LOCATION_FIPS attribute.</summary>
  public const int LocationFips_MaxLength = 2;

  /// <summary>
  /// The value of the LOCATION_FIPS attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 1, Type = MemberType.Char, Length = LocationFips_MaxLength)]
  public string LocationFips
  {
    get => locationFips ?? "";
    set => locationFips = TrimEnd(Substring(value, 1, LocationFips_MaxLength));
  }

  /// <summary>
  /// The json value of the LocationFips attribute.</summary>
  [JsonPropertyName("locationFips")]
  [Computed]
  public string LocationFips_Json
  {
    get => NullIf(LocationFips, "");
    set => LocationFips = value;
  }

  /// <summary>Length of the COUNTY_FIPS attribute.</summary>
  public const int CountyFips_MaxLength = 3;

  /// <summary>
  /// The value of the COUNTY_FIPS attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 2, Type = MemberType.Char, Length = CountyFips_MaxLength)]
  public string CountyFips
  {
    get => countyFips ?? "";
    set => countyFips = TrimEnd(Substring(value, 1, CountyFips_MaxLength));
  }

  /// <summary>
  /// The json value of the CountyFips attribute.</summary>
  [JsonPropertyName("countyFips")]
  [Computed]
  public string CountyFips_Json
  {
    get => NullIf(CountyFips, "");
    set => CountyFips = value;
  }

  /// <summary>
  /// The value of the TRANS_SERIAL_NUMBER attribute.
  /// This is a unique number assigned to each CSENet transaction.  It has no 
  /// place in the KESSEP system but is required to provide a key used to
  /// process CSENet Referrals.
  /// </summary>
  [JsonPropertyName("transSerialNumber")]
  [DefaultValue(0L)]
  [Member(Index = 3, Type = MemberType.Number, Length = 12)]
  public long TransSerialNumber
  {
    get => transSerialNumber;
    set => transSerialNumber = value;
  }

  /// <summary>Length of the FORMATTED_NAME_TEXT attribute.</summary>
  public const int FormattedNameText_MaxLength = 33;

  /// <summary>
  /// The value of the FORMATTED_NAME_TEXT attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 4, Type = MemberType.Char, Length
    = FormattedNameText_MaxLength)]
  public string FormattedNameText
  {
    get => formattedNameText ?? "";
    set => formattedNameText =
      TrimEnd(Substring(value, 1, FormattedNameText_MaxLength));
  }

  /// <summary>
  /// The json value of the FormattedNameText attribute.</summary>
  [JsonPropertyName("formattedNameText")]
  [Computed]
  public string FormattedNameText_Json
  {
    get => NullIf(FormattedNameText, "");
    set => FormattedNameText = value;
  }

  /// <summary>
  /// The value of the PERCENT attribute.
  /// This is used to calculate the percentage figure in the Child Support 
  /// Worksheet calculation with the figure rounded to one decimal place.
  /// </summary>
  [JsonPropertyName("percent")]
  [DefaultValue(typeof(decimal), "0")]
  [Member(Index = 5, Type = MemberType.Number, Length = 5, Precision = 1)]
  public decimal Percent
  {
    get => percent;
    set => percent = Truncate(value, 1);
  }

  private string locationFips;
  private string countyFips;
  private long transSerialNumber;
  private string formattedNameText;
  private decimal percent;
}
