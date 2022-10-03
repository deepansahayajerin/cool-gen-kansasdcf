// The source file: CSENET_HOST_ERROR_FILE, ID: 372895150, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Worksets;

[Serializable]
public partial class CsenetHostErrorFile: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public CsenetHostErrorFile()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public CsenetHostErrorFile(CsenetHostErrorFile that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new CsenetHostErrorFile Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(CsenetHostErrorFile that)
  {
    base.Assign(that);
    txnCounter = that.txnCounter;
    filler1 = that.filler1;
    localFipsCd = that.localFipsCd;
    filler2 = that.filler2;
    otherFipsCd = that.otherFipsCd;
    filler3 = that.filler3;
    localCaseId = that.localCaseId;
    filler4 = that.filler4;
    otherCaseId = that.otherCaseId;
    filler5 = that.filler5;
    actionCd = that.actionCd;
    filler6 = that.filler6;
    functionalTypeCd = that.functionalTypeCd;
    filler7 = that.filler7;
    actionReasonCd = that.actionReasonCd;
    filler8 = that.filler8;
    actionResolutionDt = that.actionResolutionDt;
    filler9 = that.filler9;
    transactionDt = that.transactionDt;
    filler10 = that.filler10;
    errorCd = that.errorCd;
    filler11 = that.filler11;
    errorMsg = that.errorMsg;
    transactionId = that.transactionId;
  }

  /// <summary>
  /// The value of the TXN_COUNTER attribute.
  /// </summary>
  [JsonPropertyName("txnCounter")]
  [DefaultValue(0)]
  [Member(Index = 1, Type = MemberType.Number, Length = 6)]
  public int TxnCounter
  {
    get => txnCounter;
    set => txnCounter = value;
  }

  /// <summary>Length of the FILLER_1 attribute.</summary>
  public const int Filler1_MaxLength = 1;

  /// <summary>
  /// The value of the FILLER_1 attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 2, Type = MemberType.Char, Length = Filler1_MaxLength)]
  public string Filler1
  {
    get => filler1 ?? "";
    set => filler1 = TrimEnd(Substring(value, 1, Filler1_MaxLength));
  }

  /// <summary>
  /// The json value of the Filler1 attribute.</summary>
  [JsonPropertyName("filler1")]
  [Computed]
  public string Filler1_Json
  {
    get => NullIf(Filler1, "");
    set => Filler1 = value;
  }

  /// <summary>Length of the LOCAL_FIPS_CD attribute.</summary>
  public const int LocalFipsCd_MaxLength = 7;

  /// <summary>
  /// The value of the LOCAL_FIPS_CD attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 3, Type = MemberType.Char, Length = LocalFipsCd_MaxLength)]
  public string LocalFipsCd
  {
    get => localFipsCd ?? "";
    set => localFipsCd = TrimEnd(Substring(value, 1, LocalFipsCd_MaxLength));
  }

  /// <summary>
  /// The json value of the LocalFipsCd attribute.</summary>
  [JsonPropertyName("localFipsCd")]
  [Computed]
  public string LocalFipsCd_Json
  {
    get => NullIf(LocalFipsCd, "");
    set => LocalFipsCd = value;
  }

  /// <summary>Length of the FILLER_2 attribute.</summary>
  public const int Filler2_MaxLength = 1;

  /// <summary>
  /// The value of the FILLER_2 attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 4, Type = MemberType.Char, Length = Filler2_MaxLength)]
  public string Filler2
  {
    get => filler2 ?? "";
    set => filler2 = TrimEnd(Substring(value, 1, Filler2_MaxLength));
  }

  /// <summary>
  /// The json value of the Filler2 attribute.</summary>
  [JsonPropertyName("filler2")]
  [Computed]
  public string Filler2_Json
  {
    get => NullIf(Filler2, "");
    set => Filler2 = value;
  }

  /// <summary>Length of the OTHER_FIPS_CD attribute.</summary>
  public const int OtherFipsCd_MaxLength = 7;

  /// <summary>
  /// The value of the OTHER_FIPS_CD attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 5, Type = MemberType.Char, Length = OtherFipsCd_MaxLength)]
  public string OtherFipsCd
  {
    get => otherFipsCd ?? "";
    set => otherFipsCd = TrimEnd(Substring(value, 1, OtherFipsCd_MaxLength));
  }

  /// <summary>
  /// The json value of the OtherFipsCd attribute.</summary>
  [JsonPropertyName("otherFipsCd")]
  [Computed]
  public string OtherFipsCd_Json
  {
    get => NullIf(OtherFipsCd, "");
    set => OtherFipsCd = value;
  }

  /// <summary>Length of the FILLER_3 attribute.</summary>
  public const int Filler3_MaxLength = 1;

  /// <summary>
  /// The value of the FILLER_3 attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 6, Type = MemberType.Char, Length = Filler3_MaxLength)]
  public string Filler3
  {
    get => filler3 ?? "";
    set => filler3 = TrimEnd(Substring(value, 1, Filler3_MaxLength));
  }

  /// <summary>
  /// The json value of the Filler3 attribute.</summary>
  [JsonPropertyName("filler3")]
  [Computed]
  public string Filler3_Json
  {
    get => NullIf(Filler3, "");
    set => Filler3 = value;
  }

  /// <summary>Length of the LOCAL_CASE_ID attribute.</summary>
  public const int LocalCaseId_MaxLength = 15;

  /// <summary>
  /// The value of the LOCAL_CASE_ID attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 7, Type = MemberType.Char, Length = LocalCaseId_MaxLength)]
  public string LocalCaseId
  {
    get => localCaseId ?? "";
    set => localCaseId = TrimEnd(Substring(value, 1, LocalCaseId_MaxLength));
  }

  /// <summary>
  /// The json value of the LocalCaseId attribute.</summary>
  [JsonPropertyName("localCaseId")]
  [Computed]
  public string LocalCaseId_Json
  {
    get => NullIf(LocalCaseId, "");
    set => LocalCaseId = value;
  }

  /// <summary>Length of the FILLER_4 attribute.</summary>
  public const int Filler4_MaxLength = 1;

  /// <summary>
  /// The value of the FILLER_4 attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 8, Type = MemberType.Char, Length = Filler4_MaxLength)]
  public string Filler4
  {
    get => filler4 ?? "";
    set => filler4 = TrimEnd(Substring(value, 1, Filler4_MaxLength));
  }

  /// <summary>
  /// The json value of the Filler4 attribute.</summary>
  [JsonPropertyName("filler4")]
  [Computed]
  public string Filler4_Json
  {
    get => NullIf(Filler4, "");
    set => Filler4 = value;
  }

  /// <summary>Length of the OTHER_CASE_ID attribute.</summary>
  public const int OtherCaseId_MaxLength = 15;

  /// <summary>
  /// The value of the OTHER_CASE_ID attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 9, Type = MemberType.Char, Length = OtherCaseId_MaxLength)]
  public string OtherCaseId
  {
    get => otherCaseId ?? "";
    set => otherCaseId = TrimEnd(Substring(value, 1, OtherCaseId_MaxLength));
  }

  /// <summary>
  /// The json value of the OtherCaseId attribute.</summary>
  [JsonPropertyName("otherCaseId")]
  [Computed]
  public string OtherCaseId_Json
  {
    get => NullIf(OtherCaseId, "");
    set => OtherCaseId = value;
  }

  /// <summary>Length of the FILLER_5 attribute.</summary>
  public const int Filler5_MaxLength = 1;

  /// <summary>
  /// The value of the FILLER_5 attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 10, Type = MemberType.Char, Length = Filler5_MaxLength)]
  public string Filler5
  {
    get => filler5 ?? "";
    set => filler5 = TrimEnd(Substring(value, 1, Filler5_MaxLength));
  }

  /// <summary>
  /// The json value of the Filler5 attribute.</summary>
  [JsonPropertyName("filler5")]
  [Computed]
  public string Filler5_Json
  {
    get => NullIf(Filler5, "");
    set => Filler5 = value;
  }

  /// <summary>Length of the ACTION_CD attribute.</summary>
  public const int ActionCd_MaxLength = 1;

  /// <summary>
  /// The value of the ACTION_CD attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 11, Type = MemberType.Char, Length = ActionCd_MaxLength)]
  public string ActionCd
  {
    get => actionCd ?? "";
    set => actionCd = TrimEnd(Substring(value, 1, ActionCd_MaxLength));
  }

  /// <summary>
  /// The json value of the ActionCd attribute.</summary>
  [JsonPropertyName("actionCd")]
  [Computed]
  public string ActionCd_Json
  {
    get => NullIf(ActionCd, "");
    set => ActionCd = value;
  }

  /// <summary>Length of the FILLER_6 attribute.</summary>
  public const int Filler6_MaxLength = 1;

  /// <summary>
  /// The value of the FILLER_6 attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 12, Type = MemberType.Char, Length = Filler6_MaxLength)]
  public string Filler6
  {
    get => filler6 ?? "";
    set => filler6 = TrimEnd(Substring(value, 1, Filler6_MaxLength));
  }

  /// <summary>
  /// The json value of the Filler6 attribute.</summary>
  [JsonPropertyName("filler6")]
  [Computed]
  public string Filler6_Json
  {
    get => NullIf(Filler6, "");
    set => Filler6 = value;
  }

  /// <summary>Length of the FUNCTIONAL_TYPE_CD attribute.</summary>
  public const int FunctionalTypeCd_MaxLength = 3;

  /// <summary>
  /// The value of the FUNCTIONAL_TYPE_CD attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 13, Type = MemberType.Char, Length
    = FunctionalTypeCd_MaxLength)]
  public string FunctionalTypeCd
  {
    get => functionalTypeCd ?? "";
    set => functionalTypeCd =
      TrimEnd(Substring(value, 1, FunctionalTypeCd_MaxLength));
  }

  /// <summary>
  /// The json value of the FunctionalTypeCd attribute.</summary>
  [JsonPropertyName("functionalTypeCd")]
  [Computed]
  public string FunctionalTypeCd_Json
  {
    get => NullIf(FunctionalTypeCd, "");
    set => FunctionalTypeCd = value;
  }

  /// <summary>Length of the FILLER_7 attribute.</summary>
  public const int Filler7_MaxLength = 1;

  /// <summary>
  /// The value of the FILLER_7 attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 14, Type = MemberType.Char, Length = Filler7_MaxLength)]
  public string Filler7
  {
    get => filler7 ?? "";
    set => filler7 = TrimEnd(Substring(value, 1, Filler7_MaxLength));
  }

  /// <summary>
  /// The json value of the Filler7 attribute.</summary>
  [JsonPropertyName("filler7")]
  [Computed]
  public string Filler7_Json
  {
    get => NullIf(Filler7, "");
    set => Filler7 = value;
  }

  /// <summary>Length of the ACTION_REASON_CD attribute.</summary>
  public const int ActionReasonCd_MaxLength = 5;

  /// <summary>
  /// The value of the ACTION_REASON_CD attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 15, Type = MemberType.Char, Length = ActionReasonCd_MaxLength)]
    
  public string ActionReasonCd
  {
    get => actionReasonCd ?? "";
    set => actionReasonCd =
      TrimEnd(Substring(value, 1, ActionReasonCd_MaxLength));
  }

  /// <summary>
  /// The json value of the ActionReasonCd attribute.</summary>
  [JsonPropertyName("actionReasonCd")]
  [Computed]
  public string ActionReasonCd_Json
  {
    get => NullIf(ActionReasonCd, "");
    set => ActionReasonCd = value;
  }

  /// <summary>Length of the FILLER_8 attribute.</summary>
  public const int Filler8_MaxLength = 1;

  /// <summary>
  /// The value of the FILLER_8 attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 16, Type = MemberType.Char, Length = Filler8_MaxLength)]
  public string Filler8
  {
    get => filler8 ?? "";
    set => filler8 = TrimEnd(Substring(value, 1, Filler8_MaxLength));
  }

  /// <summary>
  /// The json value of the Filler8 attribute.</summary>
  [JsonPropertyName("filler8")]
  [Computed]
  public string Filler8_Json
  {
    get => NullIf(Filler8, "");
    set => Filler8 = value;
  }

  /// <summary>
  /// The value of the ACTION_RESOLUTION_DT attribute.
  /// </summary>
  [JsonPropertyName("actionResolutionDt")]
  [Member(Index = 17, Type = MemberType.Date)]
  public DateTime? ActionResolutionDt
  {
    get => actionResolutionDt;
    set => actionResolutionDt = value;
  }

  /// <summary>Length of the FILLER_9 attribute.</summary>
  public const int Filler9_MaxLength = 1;

  /// <summary>
  /// The value of the FILLER_9 attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 18, Type = MemberType.Char, Length = Filler9_MaxLength)]
  public string Filler9
  {
    get => filler9 ?? "";
    set => filler9 = TrimEnd(Substring(value, 1, Filler9_MaxLength));
  }

  /// <summary>
  /// The json value of the Filler9 attribute.</summary>
  [JsonPropertyName("filler9")]
  [Computed]
  public string Filler9_Json
  {
    get => NullIf(Filler9, "");
    set => Filler9 = value;
  }

  /// <summary>
  /// The value of the TRANSACTION_DT attribute.
  /// </summary>
  [JsonPropertyName("transactionDt")]
  [Member(Index = 19, Type = MemberType.Date)]
  public DateTime? TransactionDt
  {
    get => transactionDt;
    set => transactionDt = value;
  }

  /// <summary>Length of the FILLER_10 attribute.</summary>
  public const int Filler10_MaxLength = 1;

  /// <summary>
  /// The value of the FILLER_10 attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 20, Type = MemberType.Char, Length = Filler10_MaxLength)]
  public string Filler10
  {
    get => filler10 ?? "";
    set => filler10 = TrimEnd(Substring(value, 1, Filler10_MaxLength));
  }

  /// <summary>
  /// The json value of the Filler10 attribute.</summary>
  [JsonPropertyName("filler10")]
  [Computed]
  public string Filler10_Json
  {
    get => NullIf(Filler10, "");
    set => Filler10 = value;
  }

  /// <summary>Length of the ERROR_CD attribute.</summary>
  public const int ErrorCd_MaxLength = 4;

  /// <summary>
  /// The value of the ERROR_CD attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 21, Type = MemberType.Char, Length = ErrorCd_MaxLength)]
  public string ErrorCd
  {
    get => errorCd ?? "";
    set => errorCd = TrimEnd(Substring(value, 1, ErrorCd_MaxLength));
  }

  /// <summary>
  /// The json value of the ErrorCd attribute.</summary>
  [JsonPropertyName("errorCd")]
  [Computed]
  public string ErrorCd_Json
  {
    get => NullIf(ErrorCd, "");
    set => ErrorCd = value;
  }

  /// <summary>Length of the FILLER_11 attribute.</summary>
  public const int Filler11_MaxLength = 1;

  /// <summary>
  /// The value of the FILLER_11 attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 22, Type = MemberType.Char, Length = Filler11_MaxLength)]
  public string Filler11
  {
    get => filler11 ?? "";
    set => filler11 = TrimEnd(Substring(value, 1, Filler11_MaxLength));
  }

  /// <summary>
  /// The json value of the Filler11 attribute.</summary>
  [JsonPropertyName("filler11")]
  [Computed]
  public string Filler11_Json
  {
    get => NullIf(Filler11, "");
    set => Filler11 = value;
  }

  /// <summary>Length of the ERROR_MSG attribute.</summary>
  public const int ErrorMsg_MaxLength = 55;

  /// <summary>
  /// The value of the ERROR_MSG attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 23, Type = MemberType.Char, Length = ErrorMsg_MaxLength)]
  public string ErrorMsg
  {
    get => errorMsg ?? "";
    set => errorMsg = TrimEnd(Substring(value, 1, ErrorMsg_MaxLength));
  }

  /// <summary>
  /// The json value of the ErrorMsg attribute.</summary>
  [JsonPropertyName("errorMsg")]
  [Computed]
  public string ErrorMsg_Json
  {
    get => NullIf(ErrorMsg, "");
    set => ErrorMsg = value;
  }

  /// <summary>
  /// The value of the TRANSACTION_ID attribute.
  /// </summary>
  [JsonPropertyName("transactionId")]
  [DefaultValue(0L)]
  [Member(Index = 24, Type = MemberType.Number, Length = 12)]
  public long TransactionId
  {
    get => transactionId;
    set => transactionId = value;
  }

  private int txnCounter;
  private string filler1;
  private string localFipsCd;
  private string filler2;
  private string otherFipsCd;
  private string filler3;
  private string localCaseId;
  private string filler4;
  private string otherCaseId;
  private string filler5;
  private string actionCd;
  private string filler6;
  private string functionalTypeCd;
  private string filler7;
  private string actionReasonCd;
  private string filler8;
  private DateTime? actionResolutionDt;
  private string filler9;
  private DateTime? transactionDt;
  private string filler10;
  private string errorCd;
  private string filler11;
  private string errorMsg;
  private long transactionId;
}
