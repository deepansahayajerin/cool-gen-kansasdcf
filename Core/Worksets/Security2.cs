// The source file: SECURITY, ID: 371424053, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Worksets;

[Serializable]
public partial class Security2: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public Security2()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public Security2(Security2 that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new Security2 Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(Security2 that)
  {
    base.Assign(that);
    linkIndicator = that.linkIndicator;
    command = that.command;
    userid = that.userid;
    indicator = that.indicator;
    tssLastName = that.tssLastName;
    tssFirstName = that.tssFirstName;
    tssMiddleInitial = that.tssMiddleInitial;
  }

  /// <summary>Length of the LINK_INDICATOR attribute.</summary>
  public const int LinkIndicator_MaxLength = 1;

  /// <summary>
  /// The value of the LINK_INDICATOR attribute.
  /// describes if the user is on a link		Y - on a link
  /// 	blank - not on a link
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 1, Type = MemberType.Char, Length = LinkIndicator_MaxLength)]
  public string LinkIndicator
  {
    get => linkIndicator ?? "";
    set => linkIndicator =
      TrimEnd(Substring(value, 1, LinkIndicator_MaxLength));
  }

  /// <summary>
  /// The json value of the LinkIndicator attribute.</summary>
  [JsonPropertyName("linkIndicator")]
  [Computed]
  public string LinkIndicator_Json
  {
    get => NullIf(LinkIndicator, "");
    set => LinkIndicator = value;
  }

  /// <summary>Length of the COMMAND attribute.</summary>
  public const int Command_MaxLength = 8;

  /// <summary>
  /// The value of the COMMAND attribute.
  /// the command which requires some kind of security clearence for the 
  /// transaction.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 2, Type = MemberType.Char, Length = Command_MaxLength)]
  public string Command
  {
    get => command ?? "";
    set => command = TrimEnd(Substring(value, 1, Command_MaxLength));
  }

  /// <summary>
  /// The json value of the Command attribute.</summary>
  [JsonPropertyName("command")]
  [Computed]
  public string Command_Json
  {
    get => NullIf(Command, "");
    set => Command = value;
  }

  /// <summary>Length of the USERID attribute.</summary>
  public const int Userid_MaxLength = 8;

  /// <summary>
  /// The value of the USERID attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 3, Type = MemberType.Char, Length = Userid_MaxLength)]
  public string Userid
  {
    get => userid ?? "";
    set => userid = TrimEnd(Substring(value, 1, Userid_MaxLength));
  }

  /// <summary>
  /// The json value of the Userid attribute.</summary>
  [JsonPropertyName("userid")]
  [Computed]
  public string Userid_Json
  {
    get => NullIf(Userid, "");
    set => Userid = value;
  }

  /// <summary>Length of the INDICATOR attribute.</summary>
  public const int Indicator_MaxLength = 1;

  /// <summary>
  /// The value of the INDICATOR attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 4, Type = MemberType.Char, Length = Indicator_MaxLength)]
  public string Indicator
  {
    get => indicator ?? "";
    set => indicator = TrimEnd(Substring(value, 1, Indicator_MaxLength));
  }

  /// <summary>
  /// The json value of the Indicator attribute.</summary>
  [JsonPropertyName("indicator")]
  [Computed]
  public string Indicator_Json
  {
    get => NullIf(Indicator, "");
    set => Indicator = value;
  }

  /// <summary>Length of the TSS_LAST_NAME attribute.</summary>
  public const int TssLastName_MaxLength = 17;

  /// <summary>
  /// The value of the TSS_LAST_NAME attribute.
  /// service provider's last name
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 5, Type = MemberType.Char, Length = TssLastName_MaxLength)]
  public string TssLastName
  {
    get => tssLastName ?? "";
    set => tssLastName = TrimEnd(Substring(value, 1, TssLastName_MaxLength));
  }

  /// <summary>
  /// The json value of the TssLastName attribute.</summary>
  [JsonPropertyName("tssLastName")]
  [Computed]
  public string TssLastName_Json
  {
    get => NullIf(TssLastName, "");
    set => TssLastName = value;
  }

  /// <summary>Length of the TSS_FIRST_NAME attribute.</summary>
  public const int TssFirstName_MaxLength = 12;

  /// <summary>
  /// The value of the TSS_FIRST_NAME attribute.
  /// service provider's first name
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 6, Type = MemberType.Char, Length = TssFirstName_MaxLength)]
  public string TssFirstName
  {
    get => tssFirstName ?? "";
    set => tssFirstName = TrimEnd(Substring(value, 1, TssFirstName_MaxLength));
  }

  /// <summary>
  /// The json value of the TssFirstName attribute.</summary>
  [JsonPropertyName("tssFirstName")]
  [Computed]
  public string TssFirstName_Json
  {
    get => NullIf(TssFirstName, "");
    set => TssFirstName = value;
  }

  /// <summary>Length of the TSS_MIDDLE_INITIAL attribute.</summary>
  public const int TssMiddleInitial_MaxLength = 1;

  /// <summary>
  /// The value of the TSS_MIDDLE_INITIAL attribute.
  /// service provider's middle initial
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 7, Type = MemberType.Char, Length = TssMiddleInitial_MaxLength)
    ]
  public string TssMiddleInitial
  {
    get => tssMiddleInitial ?? "";
    set => tssMiddleInitial =
      TrimEnd(Substring(value, 1, TssMiddleInitial_MaxLength));
  }

  /// <summary>
  /// The json value of the TssMiddleInitial attribute.</summary>
  [JsonPropertyName("tssMiddleInitial")]
  [Computed]
  public string TssMiddleInitial_Json
  {
    get => NullIf(TssMiddleInitial, "");
    set => TssMiddleInitial = value;
  }

  private string linkIndicator;
  private string command;
  private string userid;
  private string indicator;
  private string tssLastName;
  private string tssFirstName;
  private string tssMiddleInitial;
}
