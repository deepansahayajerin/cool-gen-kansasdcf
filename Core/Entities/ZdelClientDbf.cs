// The source file: ZDEL_CLIENT_DBF, ID: 374600616, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Entities;

/// <summary>
/// RESP: OBLGESTB
/// This is a temporary standalone entity to facilitate unit testing on PCs. 
/// This contains the CSE person details which are kept in ADABAS file client.
/// dbf.
/// </summary>
[Serializable]
public partial class ZdelClientDbf: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public ZdelClientDbf()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public ZdelClientDbf(ZdelClientDbf that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new ZdelClientDbf Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(ZdelClientDbf that)
  {
    base.Assign(that);
    clientNumber = that.clientNumber;
    sex = that.sex;
    dob = that.dob;
    ssn = that.ssn;
    firstName = that.firstName;
    middleInitial = that.middleInitial;
    lastName = that.lastName;
  }

  /// <summary>Length of the CLIENT_NUMBER attribute.</summary>
  public const int ClientNumber_MaxLength = 10;

  /// <summary>
  /// The value of the CLIENT_NUMBER attribute.
  /// This replaces the CSE_PERSON Number.		
  /// This information will be retrieved from the ADABAS files and displayed on 
  /// KESSEP screens using this field.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 1, Type = MemberType.Char, Length = ClientNumber_MaxLength)]
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

  /// <summary>Length of the SEX attribute.</summary>
  public const int Sex_MaxLength = 1;

  /// <summary>
  /// The value of the SEX attribute.
  /// This field replaces the CSE_Person SEX.
  /// The information will be retrieved from the ADABAS files and displayed on 
  /// KESSEP screens using this field.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 2, Type = MemberType.Char, Length = Sex_MaxLength)]
  public string Sex
  {
    get => sex ?? "";
    set => sex = TrimEnd(Substring(value, 1, Sex_MaxLength));
  }

  /// <summary>
  /// The json value of the Sex attribute.</summary>
  [JsonPropertyName("sex")]
  [Computed]
  public string Sex_Json
  {
    get => NullIf(Sex, "");
    set => Sex = value;
  }

  /// <summary>
  /// The value of the DOB attribute.
  /// This field replaces the CSE_Person Date_Of_Birth.
  /// The information will be retrieved from the ADABAS files and displayed on 
  /// KESSEP screens using this field.
  /// </summary>
  [JsonPropertyName("dob")]
  [Member(Index = 3, Type = MemberType.Date)]
  public DateTime? Dob
  {
    get => dob;
    set => dob = value;
  }

  /// <summary>Length of the SSN attribute.</summary>
  public const int Ssn_MaxLength = 9;

  /// <summary>
  /// The value of the SSN attribute.
  /// This field replaces the CSE_Person SSN field.
  /// This information will be retrieved from the ADABAS files and displayed on 
  /// KESSEP screens using this field.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 4, Type = MemberType.Char, Length = Ssn_MaxLength)]
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
  [Member(Index = 5, Type = MemberType.Char, Length = FirstName_MaxLength)]
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
  [Member(Index = 6, Type = MemberType.Char, Length = MiddleInitial_MaxLength)]
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
  public const int LastName_MaxLength = 17;

  /// <summary>
  /// The value of the LAST_NAME attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 7, Type = MemberType.Char, Length = LastName_MaxLength)]
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

  private string clientNumber;
  private string sex;
  private DateTime? dob;
  private string ssn;
  private string firstName;
  private string middleInitial;
  private string lastName;
}
