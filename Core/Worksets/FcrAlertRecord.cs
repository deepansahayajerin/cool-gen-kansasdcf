// The source file: FCR_ALERT_RECORD, ID: 371418255, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Worksets;

/// <summary>
/// This work set layout defined to hold information to generate outgoing FCR 
/// alerts.
/// </summary>
[Serializable]
public partial class FcrAlertRecord: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public FcrAlertRecord()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public FcrAlertRecord(FcrAlertRecord that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new FcrAlertRecord Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(FcrAlertRecord that)
  {
    base.Assign(that);
    caseId = that.caseId;
    personId = that.personId;
    cseSsn = that.cseSsn;
    fcrSsn = that.fcrSsn;
    cseAdditionalSsn1 = that.cseAdditionalSsn1;
    cseAdditionalSsn2 = that.cseAdditionalSsn2;
    fcrAdditionalSsn1 = that.fcrAdditionalSsn1;
    fcrAdditionalSsn2 = that.fcrAdditionalSsn2;
  }

  /// <summary>Length of the CASE_ID attribute.</summary>
  public const int CaseId_MaxLength = 10;

  /// <summary>
  /// The value of the CASE_ID attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 1, Type = MemberType.Char, Length = CaseId_MaxLength)]
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

  /// <summary>Length of the PERSON_ID attribute.</summary>
  public const int PersonId_MaxLength = 10;

  /// <summary>
  /// The value of the PERSON_ID attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 2, Type = MemberType.Char, Length = PersonId_MaxLength)]
  public string PersonId
  {
    get => personId ?? "";
    set => personId = TrimEnd(Substring(value, 1, PersonId_MaxLength));
  }

  /// <summary>
  /// The json value of the PersonId attribute.</summary>
  [JsonPropertyName("personId")]
  [Computed]
  public string PersonId_Json
  {
    get => NullIf(PersonId, "");
    set => PersonId = value;
  }

  /// <summary>Length of the CSE_SSN attribute.</summary>
  public const int CseSsn_MaxLength = 9;

  /// <summary>
  /// The value of the CSE_SSN attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 3, Type = MemberType.Char, Length = CseSsn_MaxLength)]
  public string CseSsn
  {
    get => cseSsn ?? "";
    set => cseSsn = TrimEnd(Substring(value, 1, CseSsn_MaxLength));
  }

  /// <summary>
  /// The json value of the CseSsn attribute.</summary>
  [JsonPropertyName("cseSsn")]
  [Computed]
  public string CseSsn_Json
  {
    get => NullIf(CseSsn, "");
    set => CseSsn = value;
  }

  /// <summary>Length of the FCR_SSN attribute.</summary>
  public const int FcrSsn_MaxLength = 9;

  /// <summary>
  /// The value of the FCR_SSN attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 4, Type = MemberType.Char, Length = FcrSsn_MaxLength)]
  public string FcrSsn
  {
    get => fcrSsn ?? "";
    set => fcrSsn = TrimEnd(Substring(value, 1, FcrSsn_MaxLength));
  }

  /// <summary>
  /// The json value of the FcrSsn attribute.</summary>
  [JsonPropertyName("fcrSsn")]
  [Computed]
  public string FcrSsn_Json
  {
    get => NullIf(FcrSsn, "");
    set => FcrSsn = value;
  }

  /// <summary>Length of the CSE_ADDITIONAL_SSN1 attribute.</summary>
  public const int CseAdditionalSsn1_MaxLength = 9;

  /// <summary>
  /// The value of the CSE_ADDITIONAL_SSN1 attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 5, Type = MemberType.Char, Length
    = CseAdditionalSsn1_MaxLength)]
  public string CseAdditionalSsn1
  {
    get => cseAdditionalSsn1 ?? "";
    set => cseAdditionalSsn1 =
      TrimEnd(Substring(value, 1, CseAdditionalSsn1_MaxLength));
  }

  /// <summary>
  /// The json value of the CseAdditionalSsn1 attribute.</summary>
  [JsonPropertyName("cseAdditionalSsn1")]
  [Computed]
  public string CseAdditionalSsn1_Json
  {
    get => NullIf(CseAdditionalSsn1, "");
    set => CseAdditionalSsn1 = value;
  }

  /// <summary>Length of the CSE_ADDITIONAL_SSN2 attribute.</summary>
  public const int CseAdditionalSsn2_MaxLength = 9;

  /// <summary>
  /// The value of the CSE_ADDITIONAL_SSN2 attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 6, Type = MemberType.Char, Length
    = CseAdditionalSsn2_MaxLength)]
  public string CseAdditionalSsn2
  {
    get => cseAdditionalSsn2 ?? "";
    set => cseAdditionalSsn2 =
      TrimEnd(Substring(value, 1, CseAdditionalSsn2_MaxLength));
  }

  /// <summary>
  /// The json value of the CseAdditionalSsn2 attribute.</summary>
  [JsonPropertyName("cseAdditionalSsn2")]
  [Computed]
  public string CseAdditionalSsn2_Json
  {
    get => NullIf(CseAdditionalSsn2, "");
    set => CseAdditionalSsn2 = value;
  }

  /// <summary>Length of the FCR_ADDITIONAL_SSN1 attribute.</summary>
  public const int FcrAdditionalSsn1_MaxLength = 9;

  /// <summary>
  /// The value of the FCR_ADDITIONAL_SSN1 attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 7, Type = MemberType.Char, Length
    = FcrAdditionalSsn1_MaxLength)]
  public string FcrAdditionalSsn1
  {
    get => fcrAdditionalSsn1 ?? "";
    set => fcrAdditionalSsn1 =
      TrimEnd(Substring(value, 1, FcrAdditionalSsn1_MaxLength));
  }

  /// <summary>
  /// The json value of the FcrAdditionalSsn1 attribute.</summary>
  [JsonPropertyName("fcrAdditionalSsn1")]
  [Computed]
  public string FcrAdditionalSsn1_Json
  {
    get => NullIf(FcrAdditionalSsn1, "");
    set => FcrAdditionalSsn1 = value;
  }

  /// <summary>Length of the FCR_ADDITIONAL_SSN2 attribute.</summary>
  public const int FcrAdditionalSsn2_MaxLength = 9;

  /// <summary>
  /// The value of the FCR_ADDITIONAL_SSN2 attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 8, Type = MemberType.Char, Length
    = FcrAdditionalSsn2_MaxLength)]
  public string FcrAdditionalSsn2
  {
    get => fcrAdditionalSsn2 ?? "";
    set => fcrAdditionalSsn2 =
      TrimEnd(Substring(value, 1, FcrAdditionalSsn2_MaxLength));
  }

  /// <summary>
  /// The json value of the FcrAdditionalSsn2 attribute.</summary>
  [JsonPropertyName("fcrAdditionalSsn2")]
  [Computed]
  public string FcrAdditionalSsn2_Json
  {
    get => NullIf(FcrAdditionalSsn2, "");
    set => FcrAdditionalSsn2 = value;
  }

  private string caseId;
  private string personId;
  private string cseSsn;
  private string fcrSsn;
  private string cseAdditionalSsn1;
  private string cseAdditionalSsn2;
  private string fcrAdditionalSsn1;
  private string fcrAdditionalSsn2;
}
