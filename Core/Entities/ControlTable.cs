// The source file: CONTROL_TABLE, ID: 371421544, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Entities;

/// <summary>
/// RESP: FINANCE
/// This table will hold counts for various processes.
/// Example:
/// We will have a row in this table with an identifier of &quot;CASH RECEIPT
/// &quot; and it will contain the next cash receipt number to be used whenever
/// a new cash receipt is created.  There is a CAB called ACCESS_CONTROL_TABLE
/// that will return the next number for any identifier.  It also updates the
/// CONTROL_TABLE with the next number to be used.  The input to this CAB is the
/// Identifier.
/// </summary>
[Serializable]
public partial class ControlTable: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public ControlTable()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public ControlTable(ControlTable that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new ControlTable Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(ControlTable that)
  {
    base.Assign(that);
    identifier = that.identifier;
    dummy1 = that.dummy1;
    dummy2 = that.dummy2;
    dummy3 = that.dummy3;
    dummy4 = that.dummy4;
    dummy5 = that.dummy5;
    dummy6 = that.dummy6;
    dummy7 = that.dummy7;
    dummy8 = that.dummy8;
    lastUsedNumber = that.lastUsedNumber;
  }

  /// <summary>Length of the IDENTIFIER attribute.</summary>
  public const int Identifier_MaxLength = 32;

  /// <summary>
  /// The value of the IDENTIFIER attribute.
  /// A unique identifier for each row in this table.  This identifier may be 
  /// the name of another table.
  /// Example:
  /// We will have a row in this table with an Identifier of &quot;CASH RECEIPT
  /// &quot;.  This row will contain the last cash receipt number used when
  /// creating cash receipts.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 1, Type = MemberType.Char, Length = Identifier_MaxLength)]
  public string Identifier
  {
    get => identifier ?? "";
    set => identifier = TrimEnd(Substring(value, 1, Identifier_MaxLength));
  }

  /// <summary>
  /// The json value of the Identifier attribute.</summary>
  [JsonPropertyName("identifier")]
  [Computed]
  public string Identifier_Json
  {
    get => NullIf(Identifier, "");
    set => Identifier = value;
  }

  /// <summary>Length of the DUMMY_1 attribute.</summary>
  public const int Dummy1_MaxLength = 250;

  /// <summary>
  /// The value of the DUMMY_1 attribute.
  /// By adding dummy fields to CONTROL_TABLE, we can force each record to lie 
  /// on a separate page. This means that during updates, in which DB2 does page
  /// locks, the only page locked would be the one containing the record being
  /// referenced. Per original structure, there was only one DB2 page containing
  /// all the records, so essentially, the whole table was being locked even
  /// though the intent to update was for one row only. This was causing
  /// timeouts due to deadlocks.
  /// </summary>
  [JsonPropertyName("dummy1")]
  [Member(Index = 2, Type = MemberType.Char, Length = Dummy1_MaxLength, Optional
    = true)]
  public string Dummy1
  {
    get => dummy1;
    set => dummy1 = value != null
      ? TrimEnd(Substring(value, 1, Dummy1_MaxLength)) : null;
  }

  /// <summary>Length of the DUMMY_2 attribute.</summary>
  public const int Dummy2_MaxLength = 250;

  /// <summary>
  /// The value of the DUMMY_2 attribute.
  /// </summary>
  [JsonPropertyName("dummy2")]
  [Member(Index = 3, Type = MemberType.Char, Length = Dummy2_MaxLength, Optional
    = true)]
  public string Dummy2
  {
    get => dummy2;
    set => dummy2 = value != null
      ? TrimEnd(Substring(value, 1, Dummy2_MaxLength)) : null;
  }

  /// <summary>Length of the DUMMY_3 attribute.</summary>
  public const int Dummy3_MaxLength = 250;

  /// <summary>
  /// The value of the DUMMY_3 attribute.
  /// </summary>
  [JsonPropertyName("dummy3")]
  [Member(Index = 4, Type = MemberType.Char, Length = Dummy3_MaxLength, Optional
    = true)]
  public string Dummy3
  {
    get => dummy3;
    set => dummy3 = value != null
      ? TrimEnd(Substring(value, 1, Dummy3_MaxLength)) : null;
  }

  /// <summary>Length of the DUMMY_4 attribute.</summary>
  public const int Dummy4_MaxLength = 250;

  /// <summary>
  /// The value of the DUMMY_4 attribute.
  /// </summary>
  [JsonPropertyName("dummy4")]
  [Member(Index = 5, Type = MemberType.Char, Length = Dummy4_MaxLength, Optional
    = true)]
  public string Dummy4
  {
    get => dummy4;
    set => dummy4 = value != null
      ? TrimEnd(Substring(value, 1, Dummy4_MaxLength)) : null;
  }

  /// <summary>Length of the DUMMY_5 attribute.</summary>
  public const int Dummy5_MaxLength = 250;

  /// <summary>
  /// The value of the DUMMY_5 attribute.
  /// </summary>
  [JsonPropertyName("dummy5")]
  [Member(Index = 6, Type = MemberType.Char, Length = Dummy5_MaxLength, Optional
    = true)]
  public string Dummy5
  {
    get => dummy5;
    set => dummy5 = value != null
      ? TrimEnd(Substring(value, 1, Dummy5_MaxLength)) : null;
  }

  /// <summary>Length of the DUMMY_6 attribute.</summary>
  public const int Dummy6_MaxLength = 250;

  /// <summary>
  /// The value of the DUMMY_6 attribute.
  /// </summary>
  [JsonPropertyName("dummy6")]
  [Member(Index = 7, Type = MemberType.Char, Length = Dummy6_MaxLength, Optional
    = true)]
  public string Dummy6
  {
    get => dummy6;
    set => dummy6 = value != null
      ? TrimEnd(Substring(value, 1, Dummy6_MaxLength)) : null;
  }

  /// <summary>Length of the DUMMY_7 attribute.</summary>
  public const int Dummy7_MaxLength = 250;

  /// <summary>
  /// The value of the DUMMY_7 attribute.
  /// </summary>
  [JsonPropertyName("dummy7")]
  [Member(Index = 8, Type = MemberType.Char, Length = Dummy7_MaxLength, Optional
    = true)]
  public string Dummy7
  {
    get => dummy7;
    set => dummy7 = value != null
      ? TrimEnd(Substring(value, 1, Dummy7_MaxLength)) : null;
  }

  /// <summary>Length of the DUMMY_8 attribute.</summary>
  public const int Dummy8_MaxLength = 250;

  /// <summary>
  /// The value of the DUMMY_8 attribute.
  /// </summary>
  [JsonPropertyName("dummy8")]
  [Member(Index = 9, Type = MemberType.Char, Length = Dummy8_MaxLength, Optional
    = true)]
  public string Dummy8
  {
    get => dummy8;
    set => dummy8 = value != null
      ? TrimEnd(Substring(value, 1, Dummy8_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the LAST_USED_NUMBER attribute.
  /// This is a nine digit number used primarly to store the last sequential 
  /// number used for a table.
  /// Example:
  /// For the CASH RECEIP table it will hold the number of the last cash 
  /// receipt.
  /// </summary>
  [JsonPropertyName("lastUsedNumber")]
  [DefaultValue(0)]
  [Member(Index = 10, Type = MemberType.Number, Length = 9)]
  public int LastUsedNumber
  {
    get => lastUsedNumber;
    set => lastUsedNumber = value;
  }

  private string identifier;
  private string dummy1;
  private string dummy2;
  private string dummy3;
  private string dummy4;
  private string dummy5;
  private string dummy6;
  private string dummy7;
  private string dummy8;
  private int lastUsedNumber;
}
