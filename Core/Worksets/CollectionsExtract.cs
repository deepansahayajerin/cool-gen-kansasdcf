// The source file: COLLECTIONS_EXTRACT, ID: 372819730, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Worksets;

[Serializable]
public partial class CollectionsExtract: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public CollectionsExtract()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public CollectionsExtract(CollectionsExtract that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new CollectionsExtract Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(CollectionsExtract that)
  {
    base.Assign(that);
    office = that.office;
    sectionSupervisor = that.sectionSupervisor;
    collectionOfficer = that.collectionOfficer;
    caseNumber = that.caseNumber;
    obligationCode = that.obligationCode;
    appliedTo = that.appliedTo;
    amount1 = that.amount1;
    amount3 = that.amount3;
    amount2 = that.amount2;
    amount4 = that.amount4;
    amount5 = that.amount5;
    amount6 = that.amount6;
    amount7 = that.amount7;
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

  /// <summary>Length of the SECTION_SUPERVISOR attribute.</summary>
  public const int SectionSupervisor_MaxLength = 30;

  /// <summary>
  /// The value of the SECTION_SUPERVISOR attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 2, Type = MemberType.Char, Length
    = SectionSupervisor_MaxLength)]
  public string SectionSupervisor
  {
    get => sectionSupervisor ?? "";
    set => sectionSupervisor =
      TrimEnd(Substring(value, 1, SectionSupervisor_MaxLength));
  }

  /// <summary>
  /// The json value of the SectionSupervisor attribute.</summary>
  [JsonPropertyName("sectionSupervisor")]
  [Computed]
  public string SectionSupervisor_Json
  {
    get => NullIf(SectionSupervisor, "");
    set => SectionSupervisor = value;
  }

  /// <summary>Length of the COLLECTION_OFFICER attribute.</summary>
  public const int CollectionOfficer_MaxLength = 30;

  /// <summary>
  /// The value of the COLLECTION_OFFICER attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 3, Type = MemberType.Char, Length
    = CollectionOfficer_MaxLength)]
  public string CollectionOfficer
  {
    get => collectionOfficer ?? "";
    set => collectionOfficer =
      TrimEnd(Substring(value, 1, CollectionOfficer_MaxLength));
  }

  /// <summary>
  /// The json value of the CollectionOfficer attribute.</summary>
  [JsonPropertyName("collectionOfficer")]
  [Computed]
  public string CollectionOfficer_Json
  {
    get => NullIf(CollectionOfficer, "");
    set => CollectionOfficer = value;
  }

  /// <summary>Length of the CASE_NUMBER attribute.</summary>
  public const int CaseNumber_MaxLength = 10;

  /// <summary>
  /// The value of the CASE_NUMBER attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 4, Type = MemberType.Char, Length = CaseNumber_MaxLength)]
  public string CaseNumber
  {
    get => caseNumber ?? "";
    set => caseNumber = TrimEnd(Substring(value, 1, CaseNumber_MaxLength));
  }

  /// <summary>
  /// The json value of the CaseNumber attribute.</summary>
  [JsonPropertyName("caseNumber")]
  [Computed]
  public string CaseNumber_Json
  {
    get => NullIf(CaseNumber, "");
    set => CaseNumber = value;
  }

  /// <summary>Length of the OBLIGATION_CODE attribute.</summary>
  public const int ObligationCode_MaxLength = 7;

  /// <summary>
  /// The value of the OBLIGATION_CODE attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 5, Type = MemberType.Char, Length = ObligationCode_MaxLength)]
  public string ObligationCode
  {
    get => obligationCode ?? "";
    set => obligationCode =
      TrimEnd(Substring(value, 1, ObligationCode_MaxLength));
  }

  /// <summary>
  /// The json value of the ObligationCode attribute.</summary>
  [JsonPropertyName("obligationCode")]
  [Computed]
  public string ObligationCode_Json
  {
    get => NullIf(ObligationCode, "");
    set => ObligationCode = value;
  }

  /// <summary>Length of the APPLIED_TO attribute.</summary>
  public const int AppliedTo_MaxLength = 1;

  /// <summary>
  /// The value of the APPLIED_TO attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 6, Type = MemberType.Char, Length = AppliedTo_MaxLength)]
  public string AppliedTo
  {
    get => appliedTo ?? "";
    set => appliedTo = TrimEnd(Substring(value, 1, AppliedTo_MaxLength));
  }

  /// <summary>
  /// The json value of the AppliedTo attribute.</summary>
  [JsonPropertyName("appliedTo")]
  [Computed]
  public string AppliedTo_Json
  {
    get => NullIf(AppliedTo, "");
    set => AppliedTo = value;
  }

  /// <summary>
  /// The value of the AMOUNT1 attribute.
  /// </summary>
  [JsonPropertyName("amount1")]
  [DefaultValue(typeof(decimal), "0")]
  [Member(Index = 7, Type = MemberType.Number, Length = 11, Precision = 2)]
  public decimal Amount1
  {
    get => amount1;
    set => amount1 = Truncate(value, 2);
  }

  /// <summary>
  /// The value of the AMOUNT3 attribute.
  /// </summary>
  [JsonPropertyName("amount3")]
  [DefaultValue(typeof(decimal), "0")]
  [Member(Index = 8, Type = MemberType.Number, Length = 11, Precision = 2)]
  public decimal Amount3
  {
    get => amount3;
    set => amount3 = Truncate(value, 2);
  }

  /// <summary>
  /// The value of the AMOUNT2 attribute.
  /// </summary>
  [JsonPropertyName("amount2")]
  [DefaultValue(typeof(decimal), "0")]
  [Member(Index = 9, Type = MemberType.Number, Length = 11, Precision = 2)]
  public decimal Amount2
  {
    get => amount2;
    set => amount2 = Truncate(value, 2);
  }

  /// <summary>
  /// The value of the AMOUNT4 attribute.
  /// </summary>
  [JsonPropertyName("amount4")]
  [DefaultValue(typeof(decimal), "0")]
  [Member(Index = 10, Type = MemberType.Number, Length = 11, Precision = 2)]
  public decimal Amount4
  {
    get => amount4;
    set => amount4 = Truncate(value, 2);
  }

  /// <summary>
  /// The value of the AMOUNT5 attribute.
  /// </summary>
  [JsonPropertyName("amount5")]
  [DefaultValue(typeof(decimal), "0")]
  [Member(Index = 11, Type = MemberType.Number, Length = 11, Precision = 2)]
  public decimal Amount5
  {
    get => amount5;
    set => amount5 = Truncate(value, 2);
  }

  /// <summary>
  /// The value of the AMOUNT6 attribute.
  /// </summary>
  [JsonPropertyName("amount6")]
  [DefaultValue(typeof(decimal), "0")]
  [Member(Index = 12, Type = MemberType.Number, Length = 11, Precision = 2)]
  public decimal Amount6
  {
    get => amount6;
    set => amount6 = Truncate(value, 2);
  }

  /// <summary>
  /// The value of the AMOUNT7 attribute.
  /// </summary>
  [JsonPropertyName("amount7")]
  [DefaultValue(typeof(decimal), "0")]
  [Member(Index = 13, Type = MemberType.Number, Length = 11, Precision = 2)]
  public decimal Amount7
  {
    get => amount7;
    set => amount7 = Truncate(value, 2);
  }

  private string office;
  private string sectionSupervisor;
  private string collectionOfficer;
  private string caseNumber;
  private string obligationCode;
  private string appliedTo;
  private decimal amount1;
  private decimal amount3;
  private decimal amount2;
  private decimal amount4;
  private decimal amount5;
  private decimal amount6;
  private decimal amount7;
}
