// The source file: FN_KPC_COURT_NOTC, ID: 374444393, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Worksets;

[Serializable]
public partial class FnKpcCourtNotc: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public FnKpcCourtNotc()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public FnKpcCourtNotc(FnKpcCourtNotc that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new FnKpcCourtNotc Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(FnKpcCourtNotc that)
  {
    base.Assign(that);
    obligorSsn = that.obligorSsn;
    legalActionStandardNumber = that.legalActionStandardNumber;
    distributionDate = that.distributionDate;
    amount = that.amount;
    collectionType = that.collectionType;
  }

  /// <summary>Length of the OBLIGOR_SSN attribute.</summary>
  public const int ObligorSsn_MaxLength = 9;

  /// <summary>
  /// The value of the OBLIGOR_SSN attribute.
  /// This field replaces the CSE_Person SSN field.
  /// This information will be retrieved from the ADABAS files and displayed on 
  /// KESSEP screens using this field.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 1, Type = MemberType.Char, Length = ObligorSsn_MaxLength)]
  public string ObligorSsn
  {
    get => obligorSsn ?? "";
    set => obligorSsn = TrimEnd(Substring(value, 1, ObligorSsn_MaxLength));
  }

  /// <summary>
  /// The json value of the ObligorSsn attribute.</summary>
  [JsonPropertyName("obligorSsn")]
  [Computed]
  public string ObligorSsn_Json
  {
    get => NullIf(ObligorSsn, "");
    set => ObligorSsn = value;
  }

  /// <summary>Length of the LEGAL_ACTION_STANDARD_NUMBER attribute.</summary>
  public const int LegalActionStandardNumber_MaxLength = 20;

  /// <summary>
  /// The value of the LEGAL_ACTION_STANDARD_NUMBER attribute.
  /// This is the converted Court Case number which is used by the courts to 
  /// make support payments. The existing SRS court order number consists of the
  /// court case number with a two digit county designator. The court order
  /// number, a SRS attribute, is created by the court when the payment extract
  /// is created. KAESCES adds the &quot;*&quot; in the second digit of the type
  /// if it is blank. The following is a description of the court case number
  /// in which  YR=Year, TT=Type of Court Case, and #=Number.
  /// Example:
  /// CMASS, Douglas, &amp; Sedgwick Co.:
  /// YRTT ##### (ex. 85D 12345)
  /// When the payment extract is made, the two digit county is placed at the 
  /// front of the court case number and a 6th digit (0) is added to the front
  /// of the number (0#####) thus making a twelve digit number.
  /// 	Standardized Number - SG85D*012345
  /// Wyandotte Co. Court:
  /// YRTT ##### (ex. 85D 12345) Screen display
  /// YRYRTT ##### (ex. 1985D 12345) Database storage.
  /// When the payment extract is made, the two digit county is placed at the 
  /// front of the court case number, the first two digits of the year are
  /// stripped off, and a 6th digit (0) is added to the front of the number (0
  /// #####) thus making a twelve digit number.
  /// 	Standardized Number - WY85D*012345
  /// Shawnee &amp; Johnson Co. Court:
  /// YRTT ##### (ex. 85D 123456)
  /// When the payment extract is made, the two digit county is placed at the 
  /// front of the court case number. Johnson County does suppress the 2nd digit
  /// of type on the database if it is blank; however, it is a two digit field.
  /// Also Johnson county asdds an &quot;a&quot; or &quot;b&quot; at the end of
  /// the number for Court Trustee cases if it is a multiple payor situation.
  /// 	Standardized Number - SN85D*123456
  /// 	Multi-payor	    - JO85D*12345A
  /// 			    - JO85D*12345B
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 2, Type = MemberType.Char, Length
    = LegalActionStandardNumber_MaxLength)]
  public string LegalActionStandardNumber
  {
    get => legalActionStandardNumber ?? "";
    set => legalActionStandardNumber =
      TrimEnd(Substring(value, 1, LegalActionStandardNumber_MaxLength));
  }

  /// <summary>
  /// The json value of the LegalActionStandardNumber attribute.</summary>
  [JsonPropertyName("legalActionStandardNumber")]
  [Computed]
  public string LegalActionStandardNumber_Json
  {
    get => NullIf(LegalActionStandardNumber, "");
    set => LegalActionStandardNumber = value;
  }

  /// <summary>
  /// The value of the DISTRIBUTION_DATE attribute.
  /// </summary>
  [JsonPropertyName("distributionDate")]
  [Member(Index = 3, Type = MemberType.Date)]
  public DateTime? DistributionDate
  {
    get => distributionDate;
    set => distributionDate = value;
  }

  /// <summary>
  /// The value of the AMOUNT attribute.
  /// Amount of the Cash Receipt Detail that was applied to the Debt as a 
  /// COLLECTION.
  /// </summary>
  [JsonPropertyName("amount")]
  [DefaultValue(typeof(decimal), "0")]
  [Member(Index = 4, Type = MemberType.Number, Length = 9, Precision = 2)]
  public decimal Amount
  {
    get => amount;
    set => amount = Truncate(value, 2);
  }

  /// <summary>Length of the COLLECTION_TYPE attribute.</summary>
  public const int CollectionType_MaxLength = 40;

  /// <summary>
  /// The value of the COLLECTION_TYPE attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 5, Type = MemberType.Char, Length = CollectionType_MaxLength)]
  public string CollectionType
  {
    get => collectionType ?? "";
    set => collectionType =
      TrimEnd(Substring(value, 1, CollectionType_MaxLength));
  }

  /// <summary>
  /// The json value of the CollectionType attribute.</summary>
  [JsonPropertyName("collectionType")]
  [Computed]
  public string CollectionType_Json
  {
    get => NullIf(CollectionType, "");
    set => CollectionType = value;
  }

  private string obligorSsn;
  private string legalActionStandardNumber;
  private DateTime? distributionDate;
  private decimal amount;
  private string collectionType;
}
