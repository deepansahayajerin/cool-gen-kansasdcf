// The source file: TRANSACTION, ID: 371422781, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Entities;

/// <summary>
/// RESP: SECUR
/// this will contain the screen id and transaction id cross reference.
/// </summary>
[Serializable]
public partial class Transaction: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public Transaction()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public Transaction(Transaction that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new Transaction Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(Transaction that)
  {
    base.Assign(that);
    nextTranAuthorization = that.nextTranAuthorization;
    screenId = that.screenId;
    trancode = that.trancode;
    desc = that.desc;
    menuInd = that.menuInd;
    createdBy = that.createdBy;
    createdTstamp = that.createdTstamp;
    lastUpdatedBy = that.lastUpdatedBy;
    lastUpdatedTstamp = that.lastUpdatedTstamp;
  }

  /// <summary>Length of the NEXT_TRAN_AUTHORIZATION attribute.</summary>
  public const int NextTranAuthorization_MaxLength = 1;

  /// <summary>
  /// The value of the NEXT_TRAN_AUTHORIZATION attribute.
  /// INDICATES WHETHER A USER CAN NEXT TRAN TO THIS TRANSACTION.	
  /// 	Y - CAN NEXT TRAN TO			N - CANNOT NEXT TRAN TO
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 1, Type = MemberType.Char, Length
    = NextTranAuthorization_MaxLength)]
  public string NextTranAuthorization
  {
    get => nextTranAuthorization ?? "";
    set => nextTranAuthorization =
      TrimEnd(Substring(value, 1, NextTranAuthorization_MaxLength));
  }

  /// <summary>
  /// The json value of the NextTranAuthorization attribute.</summary>
  [JsonPropertyName("nextTranAuthorization")]
  [Computed]
  public string NextTranAuthorization_Json
  {
    get => NullIf(NextTranAuthorization, "");
    set => NextTranAuthorization = value;
  }

  /// <summary>Length of the SCREEN_ID attribute.</summary>
  public const int ScreenId_MaxLength = 4;

  /// <summary>
  /// The value of the SCREEN_ID attribute.
  /// the screen id for the transaction(procedure)
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 2, Type = MemberType.Char, Length = ScreenId_MaxLength)]
  public string ScreenId
  {
    get => screenId ?? "";
    set => screenId = TrimEnd(Substring(value, 1, ScreenId_MaxLength));
  }

  /// <summary>
  /// The json value of the ScreenId attribute.</summary>
  [JsonPropertyName("screenId")]
  [Computed]
  public string ScreenId_Json
  {
    get => NullIf(ScreenId, "");
    set => ScreenId = value;
  }

  /// <summary>Length of the TRANCODE attribute.</summary>
  public const int Trancode_MaxLength = 4;

  /// <summary>
  /// The value of the TRANCODE attribute.
  /// the related trancode for the screen id.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 3, Type = MemberType.Char, Length = Trancode_MaxLength)]
  public string Trancode
  {
    get => trancode ?? "";
    set => trancode = TrimEnd(Substring(value, 1, Trancode_MaxLength));
  }

  /// <summary>
  /// The json value of the Trancode attribute.</summary>
  [JsonPropertyName("trancode")]
  [Computed]
  public string Trancode_Json
  {
    get => NullIf(Trancode, "");
    set => Trancode = value;
  }

  /// <summary>Length of the DESC attribute.</summary>
  public const int Desc_MaxLength = 25;

  /// <summary>
  /// The value of the DESC attribute.
  /// the name of the screen(trancode(procedure))
  /// </summary>
  [JsonPropertyName("desc")]
  [Member(Index = 4, Type = MemberType.Char, Length = Desc_MaxLength, Optional
    = true)]
  public string Desc
  {
    get => desc;
    set => desc = value != null
      ? TrimEnd(Substring(value, 1, Desc_MaxLength)) : null;
  }

  /// <summary>Length of the MENU_IND attribute.</summary>
  public const int MenuInd_MaxLength = 1;

  /// <summary>
  /// The value of the MENU_IND attribute.
  /// this indicates if the transaction is a menu or not.	
  /// 	Y - this is a menu		
  /// 	N - this is not a menu
  /// </summary>
  [JsonPropertyName("menuInd")]
  [Member(Index = 5, Type = MemberType.Char, Length = MenuInd_MaxLength, Optional
    = true)]
  public string MenuInd
  {
    get => menuInd;
    set => menuInd = value != null
      ? TrimEnd(Substring(value, 1, MenuInd_MaxLength)) : null;
  }

  /// <summary>Length of the CREATED_BY attribute.</summary>
  public const int CreatedBy_MaxLength = 8;

  /// <summary>
  /// The value of the CREATED_BY attribute.
  /// The signon of the person or program that created the occurrance of the 
  /// entity.
  /// </summary>
  [JsonPropertyName("createdBy")]
  [Member(Index = 6, Type = MemberType.Char, Length = CreatedBy_MaxLength, Optional
    = true)]
  public string CreatedBy
  {
    get => createdBy;
    set => createdBy = value != null
      ? TrimEnd(Substring(value, 1, CreatedBy_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the CREATED_TSTAMP attribute.
  /// The date and time that the occurrance of the entity was created.
  /// </summary>
  [JsonPropertyName("createdTstamp")]
  [Member(Index = 7, Type = MemberType.Timestamp, Optional = true)]
  public DateTime? CreatedTstamp
  {
    get => createdTstamp;
    set => createdTstamp = value;
  }

  /// <summary>Length of the LAST_UPDATED_BY attribute.</summary>
  public const int LastUpdatedBy_MaxLength = 8;

  /// <summary>
  /// The value of the LAST_UPDATED_BY attribute.
  /// The signon of the person or program that last updated the occurrance of 
  /// the entity.
  /// </summary>
  [JsonPropertyName("lastUpdatedBy")]
  [Member(Index = 8, Type = MemberType.Char, Length = LastUpdatedBy_MaxLength, Optional
    = true)]
  public string LastUpdatedBy
  {
    get => lastUpdatedBy;
    set => lastUpdatedBy = value != null
      ? TrimEnd(Substring(value, 1, LastUpdatedBy_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the LAST_UPDATED_TSTAMP attribute.
  /// The date and time that the occurrance of the entity was last updated.
  /// </summary>
  [JsonPropertyName("lastUpdatedTstamp")]
  [Member(Index = 9, Type = MemberType.Timestamp, Optional = true)]
  public DateTime? LastUpdatedTstamp
  {
    get => lastUpdatedTstamp;
    set => lastUpdatedTstamp = value;
  }

  private string nextTranAuthorization;
  private string screenId;
  private string trancode;
  private string desc;
  private string menuInd;
  private string createdBy;
  private DateTime? createdTstamp;
  private string lastUpdatedBy;
  private DateTime? lastUpdatedTstamp;
}
