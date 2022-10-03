// The source file: KDOL_FILE, ID: 945102640, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Worksets;

/// <summary>
/// UI match file to be sent to KDOL
/// </summary>
[Serializable]
public partial class KdolFile: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public KdolFile()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public KdolFile(KdolFile that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new KdolFile Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(KdolFile that)
  {
    base.Assign(that);
    ssn = that.ssn;
    firstName = that.firstName;
    middleInitial = that.middleInitial;
    lastName = that.lastName;
    clientNumber = that.clientNumber;
    extractDate = that.extractDate;
    amount = that.amount;
    maxPercent = that.maxPercent;
  }

  /// <summary>Length of the SSN attribute.</summary>
  public const int Ssn_MaxLength = 9;

  /// <summary>
  /// The value of the SSN attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 1, Type = MemberType.Char, Length = Ssn_MaxLength)]
  public string Ssn
  {
    get => ssn ?? "";
    set => ssn = TrimEnd(Substring(value, 1, Ssn_MaxLength));
  }

  /// <summary>
  /// The json value of the Ssn attribute.</summary>
  [JsonPropertyName("ssn")]
  [Computed]
  public string Ssn_Json
  {
    get => NullIf(Ssn, "");
    set => Ssn = value;
  }

  /// <summary>Length of the FIRST_NAME attribute.</summary>
  public const int FirstName_MaxLength = 12;

  /// <summary>
  /// The value of the FIRST_NAME attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 2, Type = MemberType.Char, Length = FirstName_MaxLength)]
  public string FirstName
  {
    get => firstName ?? "";
    set => firstName = TrimEnd(Substring(value, 1, FirstName_MaxLength));
  }

  /// <summary>
  /// The json value of the FirstName attribute.</summary>
  [JsonPropertyName("firstName")]
  [Computed]
  public string FirstName_Json
  {
    get => NullIf(FirstName, "");
    set => FirstName = value;
  }

  /// <summary>Length of the MIDDLE_INITIAL attribute.</summary>
  public const int MiddleInitial_MaxLength = 1;

  /// <summary>
  /// The value of the MIDDLE_INITIAL attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 3, Type = MemberType.Char, Length = MiddleInitial_MaxLength)]
  public string MiddleInitial
  {
    get => middleInitial ?? "";
    set => middleInitial =
      TrimEnd(Substring(value, 1, MiddleInitial_MaxLength));
  }

  /// <summary>
  /// The json value of the MiddleInitial attribute.</summary>
  [JsonPropertyName("middleInitial")]
  [Computed]
  public string MiddleInitial_Json
  {
    get => NullIf(MiddleInitial, "");
    set => MiddleInitial = value;
  }

  /// <summary>Length of the LAST_NAME attribute.</summary>
  public const int LastName_MaxLength = 25;

  /// <summary>
  /// The value of the LAST_NAME attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 4, Type = MemberType.Char, Length = LastName_MaxLength)]
  public string LastName
  {
    get => lastName ?? "";
    set => lastName = TrimEnd(Substring(value, 1, LastName_MaxLength));
  }

  /// <summary>
  /// The json value of the LastName attribute.</summary>
  [JsonPropertyName("lastName")]
  [Computed]
  public string LastName_Json
  {
    get => NullIf(LastName, "");
    set => LastName = value;
  }

  /// <summary>Length of the CLIENT_NUMBER attribute.</summary>
  public const int ClientNumber_MaxLength = 14;

  /// <summary>
  /// The value of the CLIENT_NUMBER attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 5, Type = MemberType.Char, Length = ClientNumber_MaxLength)]
  public string ClientNumber
  {
    get => clientNumber ?? "";
    set => clientNumber = TrimEnd(Substring(value, 1, ClientNumber_MaxLength));
  }

  /// <summary>
  /// The json value of the ClientNumber attribute.</summary>
  [JsonPropertyName("clientNumber")]
  [Computed]
  public string ClientNumber_Json
  {
    get => NullIf(ClientNumber, "");
    set => ClientNumber = value;
  }

  /// <summary>Length of the EXTRACT_DATE attribute.</summary>
  public const int ExtractDate_MaxLength = 8;

  /// <summary>
  /// The value of the EXTRACT_DATE attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 6, Type = MemberType.Char, Length = ExtractDate_MaxLength)]
  public string ExtractDate
  {
    get => extractDate ?? "";
    set => extractDate = TrimEnd(Substring(value, 1, ExtractDate_MaxLength));
  }

  /// <summary>
  /// The json value of the ExtractDate attribute.</summary>
  [JsonPropertyName("extractDate")]
  [Computed]
  public string ExtractDate_Json
  {
    get => NullIf(ExtractDate, "");
    set => ExtractDate = value;
  }

  /// <summary>
  /// The value of the AMOUNT attribute.
  /// </summary>
  [JsonPropertyName("amount")]
  [DefaultValue(typeof(decimal), "0")]
  [Member(Index = 7, Type = MemberType.Number, Length = 8, Precision = 2)]
  public decimal Amount
  {
    get => amount;
    set => amount = Truncate(value, 2);
  }

  /// <summary>Length of the MAX_PERCENT attribute.</summary>
  public const int MaxPercent_MaxLength = 3;

  /// <summary>
  /// The value of the MAX_PERCENT attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 8, Type = MemberType.Char, Length = MaxPercent_MaxLength)]
  public string MaxPercent
  {
    get => maxPercent ?? "";
    set => maxPercent = TrimEnd(Substring(value, 1, MaxPercent_MaxLength));
  }

  /// <summary>
  /// The json value of the MaxPercent attribute.</summary>
  [JsonPropertyName("maxPercent")]
  [Computed]
  public string MaxPercent_Json
  {
    get => NullIf(MaxPercent, "");
    set => MaxPercent = value;
  }

  private string ssn;
  private string firstName;
  private string middleInitial;
  private string lastName;
  private string clientNumber;
  private string extractDate;
  private decimal amount;
  private string maxPercent;
}
