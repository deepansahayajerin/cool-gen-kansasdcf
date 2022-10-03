// The source file: GUIDELINE_DEVIATIONS, ID: 1625350650, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Entities;

/// <summary>
/// The answers to the guideline deviation screen.
/// </summary>
[Serializable]
public partial class GuidelineDeviations: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public GuidelineDeviations()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public GuidelineDeviations(GuidelineDeviations that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new GuidelineDeviations Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(GuidelineDeviations that)
  {
    base.Assign(that);
    identifier = that.identifier;
    createdBy = that.createdBy;
    createdTstamp = that.createdTstamp;
    ncpHearing = that.ncpHearing;
    cpHearing = that.cpHearing;
    ncpAttorney = that.ncpAttorney;
    cpAttorney = that.cpAttorney;
    ivDAttorney = that.ivDAttorney;
    ncpIncarcerated = that.ncpIncarcerated;
    ncpIncImputed = that.ncpIncImputed;
    cpIncImputed = that.cpIncImputed;
    csWorksheetSame = that.csWorksheetSame;
    csWorksheetAdjustment = that.csWorksheetAdjustment;
    lowIncomeAdjustment = that.lowIncomeAdjustment;
    longDistanceAdjustment = that.longDistanceAdjustment;
    parentTimeAdjustment = that.parentTimeAdjustment;
    incomeTaxAdjustment = that.incomeTaxAdjustment;
    specialNeedsAdjustment = that.specialNeedsAdjustment;
    minorityAdjustment = that.minorityAdjustment;
    financialConditionAdjustment = that.financialConditionAdjustment;
    extra1 = that.extra1;
    extra2 = that.extra2;
    cseCaseNumber = that.cseCaseNumber;
    csePersonNumber = that.csePersonNumber;
    fkCktLegalAclegalActionId = that.fkCktLegalAclegalActionId;
  }

  /// <summary>
  /// The value of the IDENTIFIER attribute.
  /// The system generated number that identifies a guideline deviation record
  /// </summary>
  [JsonPropertyName("identifier")]
  [DefaultValue(0)]
  [Member(Index = 1, Type = MemberType.Number, Length = 9)]
  public int Identifier
  {
    get => identifier;
    set => identifier = value;
  }

  /// <summary>Length of the CREATED_BY attribute.</summary>
  public const int CreatedBy_MaxLength = 8;

  /// <summary>
  /// The value of the CREATED_BY attribute.
  /// Who created the record
  /// </summary>
  [JsonPropertyName("createdBy")]
  [Member(Index = 2, Type = MemberType.Varchar, Length = CreatedBy_MaxLength, Optional
    = true)]
  public string CreatedBy
  {
    get => createdBy;
    set => createdBy = value != null
      ? Substring(value, 1, CreatedBy_MaxLength) : null;
  }

  /// <summary>
  /// The value of the CREATED_TSTAMP attribute.
  /// This is the date and time when this record was created
  /// </summary>
  [JsonPropertyName("createdTstamp")]
  [Member(Index = 3, Type = MemberType.Timestamp)]
  public DateTime? CreatedTstamp
  {
    get => createdTstamp;
    set => createdTstamp = value;
  }

  /// <summary>Length of the NCP_HEARING attribute.</summary>
  public const int NcpHearing_MaxLength = 1;

  /// <summary>
  /// The value of the NCP_HEARING attribute.
  /// Did the NCP attend hearing
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 4, Type = MemberType.Varchar, Length = NcpHearing_MaxLength)]
  public string NcpHearing
  {
    get => ncpHearing ?? "";
    set => ncpHearing = Substring(value, 1, NcpHearing_MaxLength);
  }

  /// <summary>
  /// The json value of the NcpHearing attribute.</summary>
  [JsonPropertyName("ncpHearing")]
  [Computed]
  public string NcpHearing_Json
  {
    get => NullIf(NcpHearing, "");
    set => NcpHearing = value;
  }

  /// <summary>Length of the CP_HEARING attribute.</summary>
  public const int CpHearing_MaxLength = 1;

  /// <summary>
  /// The value of the CP_HEARING attribute.
  /// Did the CP attend the hearing
  /// </summary>
  [JsonPropertyName("cpHearing")]
  [Member(Index = 5, Type = MemberType.Varchar, Length = CpHearing_MaxLength, Optional
    = true)]
  public string CpHearing
  {
    get => cpHearing;
    set => cpHearing = value != null
      ? Substring(value, 1, CpHearing_MaxLength) : null;
  }

  /// <summary>Length of the NCP_ATTORNEY attribute.</summary>
  public const int NcpAttorney_MaxLength = 1;

  /// <summary>
  /// The value of the NCP_ATTORNEY attribute.
  /// Did the NCPs attorney attend the hearing.
  /// </summary>
  [JsonPropertyName("ncpAttorney")]
  [Member(Index = 6, Type = MemberType.Char, Length = NcpAttorney_MaxLength, Optional
    = true)]
  public string NcpAttorney
  {
    get => ncpAttorney;
    set => ncpAttorney = value != null
      ? TrimEnd(Substring(value, 1, NcpAttorney_MaxLength)) : null;
  }

  /// <summary>Length of the CP_ATTORNEY attribute.</summary>
  public const int CpAttorney_MaxLength = 1;

  /// <summary>
  /// The value of the CP_ATTORNEY attribute.
  /// Did the CPs attorney attend the hearing
  /// </summary>
  [JsonPropertyName("cpAttorney")]
  [Member(Index = 7, Type = MemberType.Char, Length = CpAttorney_MaxLength, Optional
    = true)]
  public string CpAttorney
  {
    get => cpAttorney;
    set => cpAttorney = value != null
      ? TrimEnd(Substring(value, 1, CpAttorney_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the IV_D_ATTORNEY attribute.
  /// The IV-D attorney who attended the hearing
  /// </summary>
  [JsonPropertyName("ivDAttorney")]
  [Member(Index = 8, Type = MemberType.Number, Length = 5, Optional = true)]
  public int? IvDAttorney
  {
    get => ivDAttorney;
    set => ivDAttorney = value;
  }

  /// <summary>Length of the NCP_INCARCERATED attribute.</summary>
  public const int NcpIncarcerated_MaxLength = 1;

  /// <summary>
  /// The value of the NCP_INCARCERATED attribute.
  /// Is the NCP incarcerated
  /// </summary>
  [JsonPropertyName("ncpIncarcerated")]
  [Member(Index = 9, Type = MemberType.Char, Length
    = NcpIncarcerated_MaxLength, Optional = true)]
  public string NcpIncarcerated
  {
    get => ncpIncarcerated;
    set => ncpIncarcerated = value != null
      ? TrimEnd(Substring(value, 1, NcpIncarcerated_MaxLength)) : null;
  }

  /// <summary>Length of the NCP_INC_IMPUTED attribute.</summary>
  public const int NcpIncImputed_MaxLength = 1;

  /// <summary>
  /// The value of the NCP_INC_IMPUTED attribute.
  /// Is the income for the NCP imputed.
  /// </summary>
  [JsonPropertyName("ncpIncImputed")]
  [Member(Index = 10, Type = MemberType.Char, Length
    = NcpIncImputed_MaxLength, Optional = true)]
  public string NcpIncImputed
  {
    get => ncpIncImputed;
    set => ncpIncImputed = value != null
      ? TrimEnd(Substring(value, 1, NcpIncImputed_MaxLength)) : null;
  }

  /// <summary>Length of the CP_INC_IMPUTED attribute.</summary>
  public const int CpIncImputed_MaxLength = 1;

  /// <summary>
  /// The value of the CP_INC_IMPUTED attribute.
  /// Is the income for the CP imputed
  /// </summary>
  [JsonPropertyName("cpIncImputed")]
  [Member(Index = 11, Type = MemberType.Char, Length = CpIncImputed_MaxLength, Optional
    = true)]
  public string CpIncImputed
  {
    get => cpIncImputed;
    set => cpIncImputed = value != null
      ? TrimEnd(Substring(value, 1, CpIncImputed_MaxLength)) : null;
  }

  /// <summary>Length of the CS_WORKSHEET_SAME attribute.</summary>
  public const int CsWorksheetSame_MaxLength = 1;

  /// <summary>
  /// The value of the CS_WORKSHEET_SAME attribute.
  /// Is the CS worksheet the same as filed with the petition
  /// </summary>
  [JsonPropertyName("csWorksheetSame")]
  [Member(Index = 12, Type = MemberType.Char, Length
    = CsWorksheetSame_MaxLength, Optional = true)]
  public string CsWorksheetSame
  {
    get => csWorksheetSame;
    set => csWorksheetSame = value != null
      ? TrimEnd(Substring(value, 1, CsWorksheetSame_MaxLength)) : null;
  }

  /// <summary>Length of the CS_WORKSHEET_ADJUSTMENT attribute.</summary>
  public const int CsWorksheetAdjustment_MaxLength = 1;

  /// <summary>
  /// The value of the CS_WORKSHEET_ADJUSTMENT attribute.
  /// Did the CS worksheet have any adjustments
  /// </summary>
  [JsonPropertyName("csWorksheetAdjustment")]
  [Member(Index = 13, Type = MemberType.Char, Length
    = CsWorksheetAdjustment_MaxLength, Optional = true)]
  public string CsWorksheetAdjustment
  {
    get => csWorksheetAdjustment;
    set => csWorksheetAdjustment = value != null
      ? TrimEnd(Substring(value, 1, CsWorksheetAdjustment_MaxLength)) : null;
  }

  /// <summary>Length of the LOW_INCOME_ADJUSTMENT attribute.</summary>
  public const int LowIncomeAdjustment_MaxLength = 1;

  /// <summary>
  /// The value of the LOW_INCOME_ADJUSTMENT attribute.
  /// Was the low income adjustment used
  /// </summary>
  [JsonPropertyName("lowIncomeAdjustment")]
  [Member(Index = 14, Type = MemberType.Char, Length
    = LowIncomeAdjustment_MaxLength, Optional = true)]
  public string LowIncomeAdjustment
  {
    get => lowIncomeAdjustment;
    set => lowIncomeAdjustment = value != null
      ? TrimEnd(Substring(value, 1, LowIncomeAdjustment_MaxLength)) : null;
  }

  /// <summary>Length of the LONG_DISTANCE_ADJUSTMENT attribute.</summary>
  public const int LongDistanceAdjustment_MaxLength = 1;

  /// <summary>
  /// The value of the LONG_DISTANCE_ADJUSTMENT attribute.
  /// Was the long distance parenting time cost adjustment used
  /// </summary>
  [JsonPropertyName("longDistanceAdjustment")]
  [Member(Index = 15, Type = MemberType.Char, Length
    = LongDistanceAdjustment_MaxLength, Optional = true)]
  public string LongDistanceAdjustment
  {
    get => longDistanceAdjustment;
    set => longDistanceAdjustment = value != null
      ? TrimEnd(Substring(value, 1, LongDistanceAdjustment_MaxLength)) : null;
  }

  /// <summary>Length of the PARENT_TIME_ADJUSTMENT attribute.</summary>
  public const int ParentTimeAdjustment_MaxLength = 1;

  /// <summary>
  /// The value of the PARENT_TIME_ADJUSTMENT attribute.
  /// Was the parenting time adjustment used
  /// </summary>
  [JsonPropertyName("parentTimeAdjustment")]
  [Member(Index = 16, Type = MemberType.Char, Length
    = ParentTimeAdjustment_MaxLength, Optional = true)]
  public string ParentTimeAdjustment
  {
    get => parentTimeAdjustment;
    set => parentTimeAdjustment = value != null
      ? TrimEnd(Substring(value, 1, ParentTimeAdjustment_MaxLength)) : null;
  }

  /// <summary>Length of the INCOME_TAX_ADJUSTMENT attribute.</summary>
  public const int IncomeTaxAdjustment_MaxLength = 1;

  /// <summary>
  /// The value of the INCOME_TAX_ADJUSTMENT attribute.
  /// Was the income tax consideration adjustment used
  /// </summary>
  [JsonPropertyName("incomeTaxAdjustment")]
  [Member(Index = 17, Type = MemberType.Char, Length
    = IncomeTaxAdjustment_MaxLength, Optional = true)]
  public string IncomeTaxAdjustment
  {
    get => incomeTaxAdjustment;
    set => incomeTaxAdjustment = value != null
      ? TrimEnd(Substring(value, 1, IncomeTaxAdjustment_MaxLength)) : null;
  }

  /// <summary>Length of the SPECIAL_NEEDS_ADJUSTMENT attribute.</summary>
  public const int SpecialNeedsAdjustment_MaxLength = 1;

  /// <summary>
  /// The value of the SPECIAL_NEEDS_ADJUSTMENT attribute.
  /// Was the special needs adjustment used
  /// </summary>
  [JsonPropertyName("specialNeedsAdjustment")]
  [Member(Index = 18, Type = MemberType.Char, Length
    = SpecialNeedsAdjustment_MaxLength, Optional = true)]
  public string SpecialNeedsAdjustment
  {
    get => specialNeedsAdjustment;
    set => specialNeedsAdjustment = value != null
      ? TrimEnd(Substring(value, 1, SpecialNeedsAdjustment_MaxLength)) : null;
  }

  /// <summary>Length of the MINORITY_ADJUSTMENT attribute.</summary>
  public const int MinorityAdjustment_MaxLength = 1;

  /// <summary>
  /// The value of the MINORITY_ADJUSTMENT attribute.
  /// Was the support past minority adjustment used
  /// </summary>
  [JsonPropertyName("minorityAdjustment")]
  [Member(Index = 19, Type = MemberType.Char, Length
    = MinorityAdjustment_MaxLength, Optional = true)]
  public string MinorityAdjustment
  {
    get => minorityAdjustment;
    set => minorityAdjustment = value != null
      ? TrimEnd(Substring(value, 1, MinorityAdjustment_MaxLength)) : null;
  }

  /// <summary>Length of the FINANCIAL_CONDITION_ADJUSTMENT attribute.</summary>
  public const int FinancialConditionAdjustment_MaxLength = 1;

  /// <summary>
  /// The value of the FINANCIAL_CONDITION_ADJUSTMENT attribute.
  /// Was the overall financial condition adjustment used
  /// </summary>
  [JsonPropertyName("financialConditionAdjustment")]
  [Member(Index = 20, Type = MemberType.Char, Length
    = FinancialConditionAdjustment_MaxLength, Optional = true)]
  public string FinancialConditionAdjustment
  {
    get => financialConditionAdjustment;
    set => financialConditionAdjustment = value != null
      ? TrimEnd(Substring(value, 1, FinancialConditionAdjustment_MaxLength)) : null
      ;
  }

  /// <summary>Length of the EXTRA_1 attribute.</summary>
  public const int Extra1_MaxLength = 1;

  /// <summary>
  /// The value of the EXTRA_1 attribute.
  /// Future field use
  /// </summary>
  [JsonPropertyName("extra1")]
  [Member(Index = 21, Type = MemberType.Char, Length = Extra1_MaxLength, Optional
    = true)]
  public string Extra1
  {
    get => extra1;
    set => extra1 = value != null
      ? TrimEnd(Substring(value, 1, Extra1_MaxLength)) : null;
  }

  /// <summary>Length of the EXTRA_2 attribute.</summary>
  public const int Extra2_MaxLength = 1;

  /// <summary>
  /// The value of the EXTRA_2 attribute.
  /// Future field use
  /// </summary>
  [JsonPropertyName("extra2")]
  [Member(Index = 22, Type = MemberType.Char, Length = Extra2_MaxLength, Optional
    = true)]
  public string Extra2
  {
    get => extra2;
    set => extra2 = value != null
      ? TrimEnd(Substring(value, 1, Extra2_MaxLength)) : null;
  }

  /// <summary>Length of the CSE_CASE_NUMBER attribute.</summary>
  public const int CseCaseNumber_MaxLength = 10;

  /// <summary>
  /// The value of the CSE_CASE_NUMBER attribute.
  /// The cse case number that is tied to the record.
  /// </summary>
  [JsonPropertyName("cseCaseNumber")]
  [Member(Index = 23, Type = MemberType.Char, Length
    = CseCaseNumber_MaxLength, Optional = true)]
  public string CseCaseNumber
  {
    get => cseCaseNumber;
    set => cseCaseNumber = value != null
      ? TrimEnd(Substring(value, 1, CseCaseNumber_MaxLength)) : null;
  }

  /// <summary>Length of the CSE_PERSON_NUMBER attribute.</summary>
  public const int CsePersonNumber_MaxLength = 10;

  /// <summary>
  /// The value of the CSE_PERSON_NUMBER attribute.
  /// The cse person number that is tied to the record
  /// </summary>
  [JsonPropertyName("csePersonNumber")]
  [Member(Index = 24, Type = MemberType.Char, Length
    = CsePersonNumber_MaxLength, Optional = true)]
  public string CsePersonNumber
  {
    get => csePersonNumber;
    set => csePersonNumber = value != null
      ? TrimEnd(Substring(value, 1, CsePersonNumber_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the IDENTIFIER attribute.
  /// This is a system generated number to uniquely identify a legal action.
  /// </summary>
  [JsonPropertyName("fkCktLegalAclegalActionId")]
  [DefaultValue(0)]
  [Member(Index = 25, Type = MemberType.Number, Length = 9)]
  public int FkCktLegalAclegalActionId
  {
    get => fkCktLegalAclegalActionId;
    set => fkCktLegalAclegalActionId = value;
  }

  private int identifier;
  private string createdBy;
  private DateTime? createdTstamp;
  private string ncpHearing;
  private string cpHearing;
  private string ncpAttorney;
  private string cpAttorney;
  private int? ivDAttorney;
  private string ncpIncarcerated;
  private string ncpIncImputed;
  private string cpIncImputed;
  private string csWorksheetSame;
  private string csWorksheetAdjustment;
  private string lowIncomeAdjustment;
  private string longDistanceAdjustment;
  private string parentTimeAdjustment;
  private string incomeTaxAdjustment;
  private string specialNeedsAdjustment;
  private string minorityAdjustment;
  private string financialConditionAdjustment;
  private string extra1;
  private string extra2;
  private string cseCaseNumber;
  private string csePersonNumber;
  private int fkCktLegalAclegalActionId;
}
