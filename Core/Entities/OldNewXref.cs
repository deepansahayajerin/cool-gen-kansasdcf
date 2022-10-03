// The source file: OLD_NEW_XREF, ID: 372611026, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Entities;

/// <summary>
/// RESP: CSENET
/// Crossreference table between KESSEP CASE NBR and the CAECSES CASE NBR.
/// </summary>
[Serializable]
public partial class OldNewXref: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public OldNewXref()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public OldNewXref(OldNewXref that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new OldNewXref Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(OldNewXref that)
  {
    base.Assign(that);
    kessepCaseNbr = that.kessepCaseNbr;
    caecsesCaseNbr = that.caecsesCaseNbr;
    clientType = that.clientType;
    clientNbr = that.clientNbr;
    clientName = that.clientName;
    startDate = that.startDate;
    endDate = that.endDate;
    relToAr = that.relToAr;
    paternityInd = that.paternityInd;
    regionNbr = that.regionNbr;
    teamLtr = that.teamLtr;
  }

  /// <summary>Length of the KESSEP_CASE_NBR attribute.</summary>
  public const int KessepCaseNbr_MaxLength = 10;

  /// <summary>
  /// The value of the KESSEP_CASE_NBR attribute.
  /// This all the Kessep Case Nbrs used to send to other states as needed.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 1, Type = MemberType.Char, Length = KessepCaseNbr_MaxLength)]
  public string KessepCaseNbr
  {
    get => kessepCaseNbr ?? "";
    set => kessepCaseNbr =
      TrimEnd(Substring(value, 1, KessepCaseNbr_MaxLength));
  }

  /// <summary>
  /// The json value of the KessepCaseNbr attribute.</summary>
  [JsonPropertyName("kessepCaseNbr")]
  [Computed]
  public string KessepCaseNbr_Json
  {
    get => NullIf(KessepCaseNbr, "");
    set => KessepCaseNbr = value;
  }

  /// <summary>Length of the CAECSES_CASE_NBR attribute.</summary>
  public const int CaecsesCaseNbr_MaxLength = 12;

  /// <summary>
  /// The value of the CAECSES_CASE_NBR attribute.
  /// This is the Caecses Case Nbrs which are needed to send to other states if 
  /// needed.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 2, Type = MemberType.Char, Length = CaecsesCaseNbr_MaxLength)]
  public string CaecsesCaseNbr
  {
    get => caecsesCaseNbr ?? "";
    set => caecsesCaseNbr =
      TrimEnd(Substring(value, 1, CaecsesCaseNbr_MaxLength));
  }

  /// <summary>
  /// The json value of the CaecsesCaseNbr attribute.</summary>
  [JsonPropertyName("caecsesCaseNbr")]
  [Computed]
  public string CaecsesCaseNbr_Json
  {
    get => NullIf(CaecsesCaseNbr, "");
    set => CaecsesCaseNbr = value;
  }

  /// <summary>Length of the CLIENT_TYPE attribute.</summary>
  public const int ClientType_MaxLength = 2;

  /// <summary>
  /// The value of the CLIENT_TYPE attribute.
  /// This is the type of client that is a definition of his status.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 3, Type = MemberType.Char, Length = ClientType_MaxLength)]
  public string ClientType
  {
    get => clientType ?? "";
    set => clientType = TrimEnd(Substring(value, 1, ClientType_MaxLength));
  }

  /// <summary>
  /// The json value of the ClientType attribute.</summary>
  [JsonPropertyName("clientType")]
  [Computed]
  public string ClientType_Json
  {
    get => NullIf(ClientType, "");
    set => ClientType = value;
  }

  /// <summary>
  /// The value of the CLIENT_NBR attribute.
  /// This is the client number which will stay with this person while using 
  /// benifits.
  /// </summary>
  [JsonPropertyName("clientNbr")]
  [DefaultValue(0L)]
  [Member(Index = 4, Type = MemberType.Number, Length = 10)]
  public long ClientNbr
  {
    get => clientNbr;
    set => clientNbr = value;
  }

  /// <summary>Length of the CLIENT_NAME attribute.</summary>
  public const int ClientName_MaxLength = 30;

  /// <summary>
  /// The value of the CLIENT_NAME attribute.
  /// This is the client name that is used while using the benifits.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 5, Type = MemberType.Char, Length = ClientName_MaxLength)]
  public string ClientName
  {
    get => clientName ?? "";
    set => clientName = TrimEnd(Substring(value, 1, ClientName_MaxLength));
  }

  /// <summary>
  /// The json value of the ClientName attribute.</summary>
  [JsonPropertyName("clientName")]
  [Computed]
  public string ClientName_Json
  {
    get => NullIf(ClientName, "");
    set => ClientName = value;
  }

  /// <summary>
  /// The value of the START_DATE attribute.
  /// This is the date the clients will enter the system.
  /// </summary>
  [JsonPropertyName("startDate")]
  [Member(Index = 6, Type = MemberType.Date)]
  public DateTime? StartDate
  {
    get => startDate;
    set => startDate = value;
  }

  /// <summary>
  /// The value of the END_DATE attribute.
  /// This is the date the clients will be removed from the system.
  /// </summary>
  [JsonPropertyName("endDate")]
  [Member(Index = 7, Type = MemberType.Date)]
  public DateTime? EndDate
  {
    get => endDate;
    set => endDate = value;
  }

  /// <summary>Length of the REL_TO_AR attribute.</summary>
  public const int RelToAr_MaxLength = 2;

  /// <summary>
  /// The value of the REL_TO_AR attribute.
  /// This is the relationship to the absent receipient.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 8, Type = MemberType.Char, Length = RelToAr_MaxLength)]
  public string RelToAr
  {
    get => relToAr ?? "";
    set => relToAr = TrimEnd(Substring(value, 1, RelToAr_MaxLength));
  }

  /// <summary>
  /// The json value of the RelToAr attribute.</summary>
  [JsonPropertyName("relToAr")]
  [Computed]
  public string RelToAr_Json
  {
    get => NullIf(RelToAr, "");
    set => RelToAr = value;
  }

  /// <summary>Length of the PATERNITY_IND attribute.</summary>
  public const int PaternityInd_MaxLength = 1;

  /// <summary>
  /// The value of the PATERNITY_IND attribute.
  /// This is the paternity ind for each client in the system.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 9, Type = MemberType.Char, Length = PaternityInd_MaxLength)]
  public string PaternityInd
  {
    get => paternityInd ?? "";
    set => paternityInd = TrimEnd(Substring(value, 1, PaternityInd_MaxLength));
  }

  /// <summary>
  /// The json value of the PaternityInd attribute.</summary>
  [JsonPropertyName("paternityInd")]
  [Computed]
  public string PaternityInd_Json
  {
    get => NullIf(PaternityInd, "");
    set => PaternityInd = value;
  }

  /// <summary>Length of the REGION_NBR attribute.</summary>
  public const int RegionNbr_MaxLength = 2;

  /// <summary>
  /// The value of the REGION_NBR attribute.
  /// This will be the regions number that will specify which area the client is
  /// in.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 10, Type = MemberType.Char, Length = RegionNbr_MaxLength)]
  public string RegionNbr
  {
    get => regionNbr ?? "";
    set => regionNbr = TrimEnd(Substring(value, 1, RegionNbr_MaxLength));
  }

  /// <summary>
  /// The json value of the RegionNbr attribute.</summary>
  [JsonPropertyName("regionNbr")]
  [Computed]
  public string RegionNbr_Json
  {
    get => NullIf(RegionNbr, "");
    set => RegionNbr = value;
  }

  /// <summary>Length of the TEAM_LTR attribute.</summary>
  public const int TeamLtr_MaxLength = 1;

  /// <summary>
  /// The value of the TEAM_LTR attribute.
  /// This is the team leader in the region which is assigned to the client.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 11, Type = MemberType.Char, Length = TeamLtr_MaxLength)]
  public string TeamLtr
  {
    get => teamLtr ?? "";
    set => teamLtr = TrimEnd(Substring(value, 1, TeamLtr_MaxLength));
  }

  /// <summary>
  /// The json value of the TeamLtr attribute.</summary>
  [JsonPropertyName("teamLtr")]
  [Computed]
  public string TeamLtr_Json
  {
    get => NullIf(TeamLtr, "");
    set => TeamLtr = value;
  }

  private string kessepCaseNbr;
  private string caecsesCaseNbr;
  private string clientType;
  private long clientNbr;
  private string clientName;
  private DateTime? startDate;
  private DateTime? endDate;
  private string relToAr;
  private string paternityInd;
  private string regionNbr;
  private string teamLtr;
}
