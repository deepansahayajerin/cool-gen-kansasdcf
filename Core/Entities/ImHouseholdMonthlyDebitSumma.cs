// The source file: IM_HOUSEHOLD_MONTHLY_DEBIT_SUMMA, ID: 372291413, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Entities;

/// <summary>
/// RESP: OBLGESTB
/// IM Household Monthly Debit Summary will store the monthly total grants, 
/// passthrus, and medical grants for the respective IM Household.
/// </summary>
[Serializable]
public partial class ImHouseholdMonthlyDebitSumma: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public ImHouseholdMonthlyDebitSumma()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public ImHouseholdMonthlyDebitSumma(ImHouseholdMonthlyDebitSumma that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new ImHouseholdMonthlyDebitSumma Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(ImHouseholdMonthlyDebitSumma that)
  {
    base.Assign(that);
    sequenceNumber = that.sequenceNumber;
    month = that.month;
    year = that.year;
    grantTotal = that.grantTotal;
    passthruTotal = that.passthruTotal;
    medicalGrantTotal = that.medicalGrantTotal;
    imhAeCaseNo = that.imhAeCaseNo;
  }

  /// <summary>
  /// The value of the SEQUENCE_NUMBER attribute.
  /// System generated identifier (created by adding 1 to the max value of the 
  /// identifier found in the table).
  /// </summary>
  [JsonPropertyName("sequenceNumber")]
  [DefaultValue(0)]
  [Member(Index = 1, Type = MemberType.Number, Length = 9)]
  public int SequenceNumber
  {
    get => sequenceNumber;
    set => sequenceNumber = value;
  }

  /// <summary>
  /// The value of the MONTH attribute.
  /// The month for which the record is created.
  /// </summary>
  [JsonPropertyName("month")]
  [DefaultValue(0)]
  [Member(Index = 2, Type = MemberType.Number, Length = 2)]
  public int Month
  {
    get => month;
    set => month = value;
  }

  /// <summary>
  /// The value of the YEAR attribute.
  /// The year for which the record is created.
  /// </summary>
  [JsonPropertyName("year")]
  [DefaultValue(0)]
  [Member(Index = 3, Type = MemberType.Number, Length = 4)]
  public int Year
  {
    get => year;
    set => year = value;
  }

  /// <summary>
  /// The value of the GRANT_TOTAL attribute.
  /// The total amount of money paid to the household as grants, for the 
  /// particular year and month.
  /// </summary>
  [JsonPropertyName("grantTotal")]
  [DefaultValue(typeof(decimal), "0")]
  [Member(Index = 4, Type = MemberType.Number, Length = 11, Precision = 2)]
  public decimal GrantTotal
  {
    get => grantTotal;
    set => grantTotal = Truncate(value, 2);
  }

  /// <summary>
  /// The value of the PASSTHRU_TOTAL attribute.
  /// The total amount of money paid to the household as passthrus, for the 
  /// particular year and month.
  /// </summary>
  [JsonPropertyName("passthruTotal")]
  [DefaultValue(typeof(decimal), "0")]
  [Member(Index = 5, Type = MemberType.Number, Length = 11, Precision = 2)]
  public decimal PassthruTotal
  {
    get => passthruTotal;
    set => passthruTotal = Truncate(value, 2);
  }

  /// <summary>
  /// The value of the MEDICAL_GRANT_TOTAL attribute.
  /// The total amount of money paid to the household as medical grants, for the
  /// particular year and month.
  /// </summary>
  [JsonPropertyName("medicalGrantTotal")]
  [DefaultValue(typeof(decimal), "0")]
  [Member(Index = 6, Type = MemberType.Number, Length = 11, Precision = 2)]
  public decimal MedicalGrantTotal
  {
    get => medicalGrantTotal;
    set => medicalGrantTotal = Truncate(value, 2);
  }

  /// <summary>Length of the AE_CASE_NO attribute.</summary>
  public const int ImhAeCaseNo_MaxLength = 8;

  /// <summary>
  /// The value of the AE_CASE_NO attribute.
  /// This attribute defines the AE Case No for the IM Household.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 7, Type = MemberType.Char, Length = ImhAeCaseNo_MaxLength)]
  public string ImhAeCaseNo
  {
    get => imhAeCaseNo ?? "";
    set => imhAeCaseNo = TrimEnd(Substring(value, 1, ImhAeCaseNo_MaxLength));
  }

  /// <summary>
  /// The json value of the ImhAeCaseNo attribute.</summary>
  [JsonPropertyName("imhAeCaseNo")]
  [Computed]
  public string ImhAeCaseNo_Json
  {
    get => NullIf(ImhAeCaseNo, "");
    set => ImhAeCaseNo = value;
  }

  private int sequenceNumber;
  private int month;
  private int year;
  private decimal grantTotal;
  private decimal passthruTotal;
  private decimal medicalGrantTotal;
  private string imhAeCaseNo;
}
