// The source file: URA_EXCESS_COLLECTION, ID: 371784965, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Entities;

/// <summary>
/// RESP: OBLGESTB
/// This entity contains exess URA collections, 1 month excess &quot;current 
/// collections&quot;, and closed AFDC cases two months combined excess
/// &quot;current collections&quot; for the IM Household designated by the
/// foreign key AE case number.                     If this amount is carried
/// over from another household (action is R), the amount is considered as a
/// collection in the URA computation algorithm for the household designated
/// by the foreign key AE case number.
/// If the excess amount is
/// from the household designated by the foreign key AE case number, and can
/// be applied to another IM Household (action is S); the action IM household
/// is the AE case number of the household to which the excess URA will be
/// carried.
/// If the
/// excess amount can not be applied to another IM Household (action is X);
/// no action is taken.                             This entity type also
/// contains 1 month excess &quot;current collections&quot;, and for closed
/// AFDC cases, two months combined excess &quot;current collections&quot;
/// for the associated IM Household. The action attribute is set to Y for 1
/// month excess &quot;current collections&quot;, and Z for closed AFDC cases
/// two months combined excess &quot;current collections&quot;. No action is
/// taken.
/// 
/// The action attribute is updated to D when the excess collection of any
/// kind (X,Y,Z) is disbursed.
/// </summary>
[Serializable]
public partial class UraExcessCollection: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public UraExcessCollection()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public UraExcessCollection(UraExcessCollection that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new UraExcessCollection Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(UraExcessCollection that)
  {
    base.Assign(that);
    sequenceNumber = that.sequenceNumber;
    month = that.month;
    year = that.year;
    amount = that.amount;
    type1 = that.type1;
    action = that.action;
    actionImHousehold = that.actionImHousehold;
    supplyingCsePerson = that.supplyingCsePerson;
    initiatingCollection = that.initiatingCollection;
    receivingCsePerson = that.receivingCsePerson;
    initiatingCsePerson = that.initiatingCsePerson;
    initiatingImHousehold = that.initiatingImHousehold;
    imhAeCaseNo = that.imhAeCaseNo;
  }

  /// <summary>
  /// The value of the SEQUENCE_NUMBER attribute.
  /// System Generated Identifier. (Created by adding 1 to the MAX value of the 
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
  /// The value of the AMOUNT attribute.
  /// The amount of URA excess collection, i.e., the amount of collection 
  /// remaining after all URA in the specific IM Household was recovered.
  /// </summary>
  [JsonPropertyName("amount")]
  [DefaultValue(typeof(decimal), "0")]
  [Member(Index = 4, Type = MemberType.Number, Length = 9, Precision = 2)]
  public decimal Amount
  {
    get => amount;
    set => amount = Truncate(value, 2);
  }

  /// <summary>Length of the TYPE attribute.</summary>
  public const int Type1_MaxLength = 1;

  /// <summary>
  /// The value of the TYPE attribute.
  /// The type of URA for which the record is created.
  /// 
  /// C is AFDC Excess Child Support
  /// 
  /// S is AFDC Excess Spousal Support
  /// M is Medical
  /// Excess
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 5, Type = MemberType.Char, Length = Type1_MaxLength)]
  public string Type1
  {
    get => type1 ?? "";
    set => type1 = TrimEnd(Substring(value, 1, Type1_MaxLength));
  }

  /// <summary>
  /// The json value of the Type1 attribute.</summary>
  [JsonPropertyName("type1")]
  [Computed]
  public string Type1_Json
  {
    get => NullIf(Type1, "");
    set => Type1 = value;
  }

  /// <summary>Length of the ACTION attribute.</summary>
  public const int Action_MaxLength = 1;

  /// <summary>
  /// The value of the ACTION attribute.
  /// Describes the nature of the excess amount and the action to be taken.
  /// 
  /// R: The excess URA collection may be carried over from another IM Household
  /// which has children attached to the same Debt Detail court order number (
  /// action is R). The amount is considered as a collection in the URA
  /// computation algorithm for the household designated by the foreign key AE
  /// case number.                                                    S: The
  /// excess amount is from the household designated by the foreign key AE case
  /// number, and can be applied to another IM Household (action is S). The
  /// action IM household is the AE Case Number of the household to which the
  /// excess URA will be posted.
  /// X: The excess amount
  /// is from the household designated by the foreign key AE case number, and
  /// can not be applied to another IM Household (action is X). No action taken.
  /// Y: The excess amount is from the household designated by the
  /// foreign key AE case number, and is for 1 month excess &quot;current
  /// collections&quot;. No action is taken.                                 Z:
  /// The excess amount is from the household designated by the foreign key AE
  /// case number, and is for a closed AFDC case two month combined excess
  /// &quot;current collections&quot;. No action is taken.
  /// 
  /// D: The action attribute is updated to D when the excess collection is
  /// disbursed.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 6, Type = MemberType.Char, Length = Action_MaxLength)]
  public string Action
  {
    get => action ?? "";
    set => action = TrimEnd(Substring(value, 1, Action_MaxLength));
  }

  /// <summary>
  /// The json value of the Action attribute.</summary>
  [JsonPropertyName("action")]
  [Computed]
  public string Action_Json
  {
    get => NullIf(Action, "");
    set => Action = value;
  }

  /// <summary>Length of the ACTION_IM_HOUSEHOLD attribute.</summary>
  public const int ActionImHousehold_MaxLength = 8;

  /// <summary>
  /// The value of the ACTION_IM_HOUSEHOLD attribute.
  /// The AE Case No which is used as the identifier for the IM Household entity
  /// type. It serves to identify the IM Household which supplied the excess
  /// collection amount or to which the excess collection amount is being
  /// transferred.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 7, Type = MemberType.Char, Length
    = ActionImHousehold_MaxLength)]
  public string ActionImHousehold
  {
    get => actionImHousehold ?? "";
    set => actionImHousehold =
      TrimEnd(Substring(value, 1, ActionImHousehold_MaxLength));
  }

  /// <summary>
  /// The json value of the ActionImHousehold attribute.</summary>
  [JsonPropertyName("actionImHousehold")]
  [Computed]
  public string ActionImHousehold_Json
  {
    get => NullIf(ActionImHousehold, "");
    set => ActionImHousehold = value;
  }

  /// <summary>Length of the SUPPLYING_CSE_PERSON attribute.</summary>
  public const int SupplyingCsePerson_MaxLength = 10;

  /// <summary>
  /// The value of the SUPPLYING_CSE_PERSON attribute.
  /// This is a system generated number which is used as the identifier of the 
  /// CSE PERSON entity type. This identifies the CSE Person associated with the
  /// IM Household, from which the excess URA was transferred.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 8, Type = MemberType.Char, Length
    = SupplyingCsePerson_MaxLength)]
  public string SupplyingCsePerson
  {
    get => supplyingCsePerson ?? "";
    set => supplyingCsePerson =
      TrimEnd(Substring(value, 1, SupplyingCsePerson_MaxLength));
  }

  /// <summary>
  /// The json value of the SupplyingCsePerson attribute.</summary>
  [JsonPropertyName("supplyingCsePerson")]
  [Computed]
  public string SupplyingCsePerson_Json
  {
    get => NullIf(SupplyingCsePerson, "");
    set => SupplyingCsePerson = value;
  }

  /// <summary>
  /// The value of the INITIATING_COLLECTION attribute.
  /// This is a system generated number which is used as the identifier of the 
  /// COLLECTION entity type. The identifies the collection record which yielded
  /// the excess URA.
  /// </summary>
  [JsonPropertyName("initiatingCollection")]
  [DefaultValue(0)]
  [Member(Index = 9, Type = MemberType.Number, Length = 9)]
  public int InitiatingCollection
  {
    get => initiatingCollection;
    set => initiatingCollection = value;
  }

  /// <summary>Length of the RECEIVING_CSE_PERSON attribute.</summary>
  public const int ReceivingCsePerson_MaxLength = 10;

  /// <summary>
  /// The value of the RECEIVING_CSE_PERSON attribute.
  /// This is the system generated number which is used as the identifier of the
  /// CSE Person entity type. This identifies the CSE Person associated with
  /// the IM Household to which the excess URA will be transferred.
  /// </summary>
  [JsonPropertyName("receivingCsePerson")]
  [Member(Index = 10, Type = MemberType.Char, Length
    = ReceivingCsePerson_MaxLength, Optional = true)]
  public string ReceivingCsePerson
  {
    get => receivingCsePerson;
    set => receivingCsePerson = value != null
      ? TrimEnd(Substring(value, 1, ReceivingCsePerson_MaxLength)) : null;
  }

  /// <summary>Length of the INITIATING_CSE_PERSON attribute.</summary>
  public const int InitiatingCsePerson_MaxLength = 10;

  /// <summary>
  /// The value of the INITIATING_CSE_PERSON attribute.
  /// This is the system generated number which is used as the identifier of the
  /// CSE Person entity type. This identifies the CSE Person associated with
  /// the collection and IM Household from which the excess URA originated.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 11, Type = MemberType.Char, Length
    = InitiatingCsePerson_MaxLength)]
  public string InitiatingCsePerson
  {
    get => initiatingCsePerson ?? "";
    set => initiatingCsePerson =
      TrimEnd(Substring(value, 1, InitiatingCsePerson_MaxLength));
  }

  /// <summary>
  /// The json value of the InitiatingCsePerson attribute.</summary>
  [JsonPropertyName("initiatingCsePerson")]
  [Computed]
  public string InitiatingCsePerson_Json
  {
    get => NullIf(InitiatingCsePerson, "");
    set => InitiatingCsePerson = value;
  }

  /// <summary>Length of the INITIATING_IM_HOUSEHOLD attribute.</summary>
  public const int InitiatingImHousehold_MaxLength = 8;

  /// <summary>
  /// The value of the INITIATING_IM_HOUSEHOLD attribute.
  /// The AE case number which is used as the identifier for the IM Household 
  /// entity type. This identifies the IM Household associated with the
  /// collection from which the excess URA originated.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 12, Type = MemberType.Char, Length
    = InitiatingImHousehold_MaxLength)]
  public string InitiatingImHousehold
  {
    get => initiatingImHousehold ?? "";
    set => initiatingImHousehold =
      TrimEnd(Substring(value, 1, InitiatingImHousehold_MaxLength));
  }

  /// <summary>
  /// The json value of the InitiatingImHousehold attribute.</summary>
  [JsonPropertyName("initiatingImHousehold")]
  [Computed]
  public string InitiatingImHousehold_Json
  {
    get => NullIf(InitiatingImHousehold, "");
    set => InitiatingImHousehold = value;
  }

  /// <summary>Length of the AE_CASE_NO attribute.</summary>
  public const int ImhAeCaseNo_MaxLength = 8;

  /// <summary>
  /// The value of the AE_CASE_NO attribute.
  /// This attribute defines the AE Case No for the IM Household.
  /// </summary>
  [JsonPropertyName("imhAeCaseNo")]
  [Member(Index = 13, Type = MemberType.Char, Length = ImhAeCaseNo_MaxLength, Optional
    = true)]
  public string ImhAeCaseNo
  {
    get => imhAeCaseNo;
    set => imhAeCaseNo = value != null
      ? TrimEnd(Substring(value, 1, ImhAeCaseNo_MaxLength)) : null;
  }

  private int sequenceNumber;
  private int month;
  private int year;
  private decimal amount;
  private string type1;
  private string action;
  private string actionImHousehold;
  private string supplyingCsePerson;
  private int initiatingCollection;
  private string receivingCsePerson;
  private string initiatingCsePerson;
  private string initiatingImHousehold;
  private string imhAeCaseNo;
}
