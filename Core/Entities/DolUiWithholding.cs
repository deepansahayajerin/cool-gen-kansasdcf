// The source file: DOL_UI_WITHHOLDING, ID: 945091598, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Entities;

/// <summary>
/// This entity will contain information about Income Withholding requested from
/// Unemployment Compensation through the Department of Labor.
/// </summary>
[Serializable]
public partial class DolUiWithholding: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public DolUiWithholding()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public DolUiWithholding(DolUiWithholding that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new DolUiWithholding Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(DolUiWithholding that)
  {
    base.Assign(that);
    csePersonNumber = that.csePersonNumber;
    withholdingCertificationDate = that.withholdingCertificationDate;
    standardNumber = that.standardNumber;
    socialSecurityNumber = that.socialSecurityNumber;
    waAmount = that.waAmount;
    wcAmount = that.wcAmount;
    maxWithholdingPercent = that.maxWithholdingPercent;
    firstName = that.firstName;
    lastName = that.lastName;
    middleInitial = that.middleInitial;
  }

  /// <summary>Length of the CSE_PERSON_NUMBER attribute.</summary>
  public const int CsePersonNumber_MaxLength = 10;

  /// <summary>
  /// The value of the CSE_PERSON_NUMBER attribute.
  /// Person number for which Income Withholding is requested.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 1, Type = MemberType.Char, Length = CsePersonNumber_MaxLength)]
    
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
  /// The value of the WITHHOLDING_CERTIFICATION_DATE attribute.
  /// Date that the person and court order combination was certified for IWO 
  /// withholding at DOL.
  /// </summary>
  [JsonPropertyName("withholdingCertificationDate")]
  [Member(Index = 2, Type = MemberType.Date)]
  public DateTime? WithholdingCertificationDate
  {
    get => withholdingCertificationDate;
    set => withholdingCertificationDate = value;
  }

  /// <summary>Length of the STANDARD_NUMBER attribute.</summary>
  public const int StandardNumber_MaxLength = 12;

  /// <summary>
  /// The value of the STANDARD_NUMBER attribute.
  /// Court Order Standard Number authorizing Income Withholding.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 3, Type = MemberType.Char, Length = StandardNumber_MaxLength)]
  public string StandardNumber
  {
    get => standardNumber ?? "";
    set => standardNumber =
      TrimEnd(Substring(value, 1, StandardNumber_MaxLength));
  }

  /// <summary>
  /// The json value of the StandardNumber attribute.</summary>
  [JsonPropertyName("standardNumber")]
  [Computed]
  public string StandardNumber_Json
  {
    get => NullIf(StandardNumber, "");
    set => StandardNumber = value;
  }

  /// <summary>Length of the SOCIAL_SECURITY_NUMBER attribute.</summary>
  public const int SocialSecurityNumber_MaxLength = 9;

  /// <summary>
  /// The value of the SOCIAL_SECURITY_NUMBER attribute.
  /// Social Security number for which Income Withholding is requested.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 4, Type = MemberType.Char, Length
    = SocialSecurityNumber_MaxLength)]
  public string SocialSecurityNumber
  {
    get => socialSecurityNumber ?? "";
    set => socialSecurityNumber =
      TrimEnd(Substring(value, 1, SocialSecurityNumber_MaxLength));
  }

  /// <summary>
  /// The json value of the SocialSecurityNumber attribute.</summary>
  [JsonPropertyName("socialSecurityNumber")]
  [Computed]
  public string SocialSecurityNumber_Json
  {
    get => NullIf(SocialSecurityNumber, "");
    set => SocialSecurityNumber = value;
  }

  /// <summary>
  /// The value of the WA_AMOUNT attribute.
  /// Amount ordered withheld to satisfy arrears debts.
  /// </summary>
  [JsonPropertyName("waAmount")]
  [Member(Index = 5, Type = MemberType.Number, Length = 8, Precision = 2, Optional
    = true)]
  public decimal? WaAmount
  {
    get => waAmount;
    set => waAmount = value != null ? Truncate(value, 2) : null;
  }

  /// <summary>
  /// The value of the WC_AMOUNT attribute.
  /// Amount ordered withheld to satisfy current support.
  /// </summary>
  [JsonPropertyName("wcAmount")]
  [Member(Index = 6, Type = MemberType.Number, Length = 8, Precision = 2, Optional
    = true)]
  public decimal? WcAmount
  {
    get => wcAmount;
    set => wcAmount = value != null ? Truncate(value, 2) : null;
  }

  /// <summary>
  /// The value of the MAX_WITHHOLDING_PERCENT attribute.
  /// The maximum percentage to be withheld from NCPs Unemployment Benefit.
  /// </summary>
  [JsonPropertyName("maxWithholdingPercent")]
  [Member(Index = 7, Type = MemberType.Number, Length = 3, Optional = true)]
  public int? MaxWithholdingPercent
  {
    get => maxWithholdingPercent;
    set => maxWithholdingPercent = value;
  }

  /// <summary>Length of the FIRST_NAME attribute.</summary>
  public const int FirstName_MaxLength = 12;

  /// <summary>
  /// The value of the FIRST_NAME attribute.
  /// First name of person for whom Income Withholding is requested.
  /// </summary>
  [JsonPropertyName("firstName")]
  [Member(Index = 8, Type = MemberType.Char, Length = FirstName_MaxLength, Optional
    = true)]
  public string FirstName
  {
    get => firstName;
    set => firstName = value != null
      ? TrimEnd(Substring(value, 1, FirstName_MaxLength)) : null;
  }

  /// <summary>Length of the LAST_NAME attribute.</summary>
  public const int LastName_MaxLength = 25;

  /// <summary>
  /// The value of the LAST_NAME attribute.
  /// Last name of person for whom Income Withholding is requested.
  /// </summary>
  [JsonPropertyName("lastName")]
  [Member(Index = 9, Type = MemberType.Char, Length = LastName_MaxLength, Optional
    = true)]
  public string LastName
  {
    get => lastName;
    set => lastName = value != null
      ? TrimEnd(Substring(value, 1, LastName_MaxLength)) : null;
  }

  /// <summary>Length of the MIDDLE_INITIAL attribute.</summary>
  public const int MiddleInitial_MaxLength = 1;

  /// <summary>
  /// The value of the MIDDLE_INITIAL attribute.
  /// Middle initial of person for whom Income Withholding is requested.
  /// </summary>
  [JsonPropertyName("middleInitial")]
  [Member(Index = 10, Type = MemberType.Char, Length
    = MiddleInitial_MaxLength, Optional = true)]
  public string MiddleInitial
  {
    get => middleInitial;
    set => middleInitial = value != null
      ? TrimEnd(Substring(value, 1, MiddleInitial_MaxLength)) : null;
  }

  private string csePersonNumber;
  private DateTime? withholdingCertificationDate;
  private string standardNumber;
  private string socialSecurityNumber;
  private decimal? waAmount;
  private decimal? wcAmount;
  private int? maxWithholdingPercent;
  private string firstName;
  private string lastName;
  private string middleInitial;
}
