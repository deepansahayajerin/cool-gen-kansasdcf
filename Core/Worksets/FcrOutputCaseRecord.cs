﻿// The source file: FCR_OUTPUT_CASE_RECORD, ID: 374577113, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Worksets;

/// <summary>
/// Record Layout for FCR Extract File and this layout will be used to read the 
/// FCR extract dataset generated by CSE application (SWEEB411).  This workset
/// contains all the case record attribute.
/// </summary>
[Serializable]
public partial class FcrOutputCaseRecord: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public FcrOutputCaseRecord()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public FcrOutputCaseRecord(FcrOutputCaseRecord that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new FcrOutputCaseRecord Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(FcrOutputCaseRecord that)
  {
    base.Assign(that);
    recordId = that.recordId;
    actionTypeCode = that.actionTypeCode;
    caseId = that.caseId;
    caseType = that.caseType;
    orderIndicator = that.orderIndicator;
    fipsCountyCode = that.fipsCountyCode;
    blank1 = that.blank1;
    userField = that.userField;
    previousCaseId = that.previousCaseId;
  }

  /// <summary>Length of the RECORD_ID attribute.</summary>
  public const int RecordId_MaxLength = 2;

  /// <summary>
  /// The value of the RECORD_ID attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 1, Type = MemberType.Char, Length = RecordId_MaxLength)]
  public string RecordId
  {
    get => recordId ?? "";
    set => recordId = TrimEnd(Substring(value, 1, RecordId_MaxLength));
  }

  /// <summary>
  /// The json value of the RecordId attribute.</summary>
  [JsonPropertyName("recordId")]
  [Computed]
  public string RecordId_Json
  {
    get => NullIf(RecordId, "");
    set => RecordId = value;
  }

  /// <summary>Length of the ACTION_TYPE_CODE attribute.</summary>
  public const int ActionTypeCode_MaxLength = 1;

  /// <summary>
  /// The value of the ACTION_TYPE_CODE attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 2, Type = MemberType.Char, Length = ActionTypeCode_MaxLength)]
  public string ActionTypeCode
  {
    get => actionTypeCode ?? "";
    set => actionTypeCode =
      TrimEnd(Substring(value, 1, ActionTypeCode_MaxLength));
  }

  /// <summary>
  /// The json value of the ActionTypeCode attribute.</summary>
  [JsonPropertyName("actionTypeCode")]
  [Computed]
  public string ActionTypeCode_Json
  {
    get => NullIf(ActionTypeCode, "");
    set => ActionTypeCode = value;
  }

  /// <summary>Length of the CASE_ID attribute.</summary>
  public const int CaseId_MaxLength = 10;

  /// <summary>
  /// The value of the CASE_ID attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 3, Type = MemberType.Char, Length = CaseId_MaxLength)]
  public string CaseId
  {
    get => caseId ?? "";
    set => caseId = TrimEnd(Substring(value, 1, CaseId_MaxLength));
  }

  /// <summary>
  /// The json value of the CaseId attribute.</summary>
  [JsonPropertyName("caseId")]
  [Computed]
  public string CaseId_Json
  {
    get => NullIf(CaseId, "");
    set => CaseId = value;
  }

  /// <summary>Length of the CASE_TYPE attribute.</summary>
  public const int CaseType_MaxLength = 1;

  /// <summary>
  /// The value of the CASE_TYPE attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 4, Type = MemberType.Char, Length = CaseType_MaxLength)]
  public string CaseType
  {
    get => caseType ?? "";
    set => caseType = TrimEnd(Substring(value, 1, CaseType_MaxLength));
  }

  /// <summary>
  /// The json value of the CaseType attribute.</summary>
  [JsonPropertyName("caseType")]
  [Computed]
  public string CaseType_Json
  {
    get => NullIf(CaseType, "");
    set => CaseType = value;
  }

  /// <summary>Length of the ORDER_INDICATOR attribute.</summary>
  public const int OrderIndicator_MaxLength = 1;

  /// <summary>
  /// The value of the ORDER_INDICATOR attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 5, Type = MemberType.Char, Length = OrderIndicator_MaxLength)]
  public string OrderIndicator
  {
    get => orderIndicator ?? "";
    set => orderIndicator =
      TrimEnd(Substring(value, 1, OrderIndicator_MaxLength));
  }

  /// <summary>
  /// The json value of the OrderIndicator attribute.</summary>
  [JsonPropertyName("orderIndicator")]
  [Computed]
  public string OrderIndicator_Json
  {
    get => NullIf(OrderIndicator, "");
    set => OrderIndicator = value;
  }

  /// <summary>Length of the FIPS_COUNTY_CODE attribute.</summary>
  public const int FipsCountyCode_MaxLength = 3;

  /// <summary>
  /// The value of the FIPS_COUNTY_CODE attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 6, Type = MemberType.Char, Length = FipsCountyCode_MaxLength)]
  public string FipsCountyCode
  {
    get => fipsCountyCode ?? "";
    set => fipsCountyCode =
      TrimEnd(Substring(value, 1, FipsCountyCode_MaxLength));
  }

  /// <summary>
  /// The json value of the FipsCountyCode attribute.</summary>
  [JsonPropertyName("fipsCountyCode")]
  [Computed]
  public string FipsCountyCode_Json
  {
    get => NullIf(FipsCountyCode, "");
    set => FipsCountyCode = value;
  }

  /// <summary>Length of the BLANK1 attribute.</summary>
  public const int Blank1_MaxLength = 2;

  /// <summary>
  /// The value of the BLANK1 attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 7, Type = MemberType.Char, Length = Blank1_MaxLength)]
  public string Blank1
  {
    get => blank1 ?? "";
    set => blank1 = TrimEnd(Substring(value, 1, Blank1_MaxLength));
  }

  /// <summary>
  /// The json value of the Blank1 attribute.</summary>
  [JsonPropertyName("blank1")]
  [Computed]
  public string Blank1_Json
  {
    get => NullIf(Blank1, "");
    set => Blank1 = value;
  }

  /// <summary>Length of the USER_FIELD attribute.</summary>
  public const int UserField_MaxLength = 15;

  /// <summary>
  /// The value of the USER_FIELD attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 8, Type = MemberType.Char, Length = UserField_MaxLength)]
  public string UserField
  {
    get => userField ?? "";
    set => userField = TrimEnd(Substring(value, 1, UserField_MaxLength));
  }

  /// <summary>
  /// The json value of the UserField attribute.</summary>
  [JsonPropertyName("userField")]
  [Computed]
  public string UserField_Json
  {
    get => NullIf(UserField, "");
    set => UserField = value;
  }

  /// <summary>Length of the PREVIOUS_CASE_ID attribute.</summary>
  public const int PreviousCaseId_MaxLength = 10;

  /// <summary>
  /// The value of the PREVIOUS_CASE_ID attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 9, Type = MemberType.Char, Length = PreviousCaseId_MaxLength)]
  public string PreviousCaseId
  {
    get => previousCaseId ?? "";
    set => previousCaseId =
      TrimEnd(Substring(value, 1, PreviousCaseId_MaxLength));
  }

  /// <summary>
  /// The json value of the PreviousCaseId attribute.</summary>
  [JsonPropertyName("previousCaseId")]
  [Computed]
  public string PreviousCaseId_Json
  {
    get => NullIf(PreviousCaseId, "");
    set => PreviousCaseId = value;
  }

  private string recordId;
  private string actionTypeCode;
  private string caseId;
  private string caseType;
  private string orderIndicator;
  private string fipsCountyCode;
  private string blank1;
  private string userField;
  private string previousCaseId;
}