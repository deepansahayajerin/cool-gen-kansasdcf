// The source file: ABEND_DATA, ID: 371456789, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Worksets;

/// <summary>
/// RESP: SRVINIT		
/// This work set contains attributes required to define all error information 
/// on return from an ADABAS action.
/// </summary>
[Serializable]
public partial class AbendData: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public AbendData()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public AbendData(AbendData that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new AbendData Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(AbendData that)
  {
    base.Assign(that);
    type1 = that.type1;
    adabasFileNumber = that.adabasFileNumber;
    adabasFileAction = that.adabasFileAction;
    adabasResponseCd = that.adabasResponseCd;
    cicsResourceNm = that.cicsResourceNm;
    cicsFunctionCd = that.cicsFunctionCd;
    cicsResponseCd = that.cicsResponseCd;
  }

  /// <summary>Length of the TYPE attribute.</summary>
  public const int Type1_MaxLength = 1;

  /// <summary>
  /// The value of the TYPE attribute.
  /// SPACE	Okay	
  /// A	Adabas
  /// C	CICS
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 1, Type = MemberType.Char, Length = Type1_MaxLength)]
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

  /// <summary>Length of the ADABAS_FILE_NUMBER attribute.</summary>
  public const int AdabasFileNumber_MaxLength = 4;

  /// <summary>
  /// The value of the ADABAS_FILE_NUMBER attribute.
  /// The name of the adabas file being accessed
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 2, Type = MemberType.Char, Length = AdabasFileNumber_MaxLength)
    ]
  public string AdabasFileNumber
  {
    get => adabasFileNumber ?? "";
    set => adabasFileNumber =
      TrimEnd(Substring(value, 1, AdabasFileNumber_MaxLength));
  }

  /// <summary>
  /// The json value of the AdabasFileNumber attribute.</summary>
  [JsonPropertyName("adabasFileNumber")]
  [Computed]
  public string AdabasFileNumber_Json
  {
    get => NullIf(AdabasFileNumber, "");
    set => AdabasFileNumber = value;
  }

  /// <summary>Length of the ADABAS_FILE_ACTION attribute.</summary>
  public const int AdabasFileAction_MaxLength = 3;

  /// <summary>
  /// The value of the ADABAS_FILE_ACTION attribute.
  /// The type of action being performed in ADABAS
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 3, Type = MemberType.Char, Length = AdabasFileAction_MaxLength)
    ]
  public string AdabasFileAction
  {
    get => adabasFileAction ?? "";
    set => adabasFileAction =
      TrimEnd(Substring(value, 1, AdabasFileAction_MaxLength));
  }

  /// <summary>
  /// The json value of the AdabasFileAction attribute.</summary>
  [JsonPropertyName("adabasFileAction")]
  [Computed]
  public string AdabasFileAction_Json
  {
    get => NullIf(AdabasFileAction, "");
    set => AdabasFileAction = value;
  }

  /// <summary>Length of the ADABAS_RESPONSE_CD attribute.</summary>
  public const int AdabasResponseCd_MaxLength = 4;

  /// <summary>
  /// The value of the ADABAS_RESPONSE_CD attribute.
  /// The error code returned
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 4, Type = MemberType.Char, Length = AdabasResponseCd_MaxLength)
    ]
  public string AdabasResponseCd
  {
    get => adabasResponseCd ?? "";
    set => adabasResponseCd =
      TrimEnd(Substring(value, 1, AdabasResponseCd_MaxLength));
  }

  /// <summary>
  /// The json value of the AdabasResponseCd attribute.</summary>
  [JsonPropertyName("adabasResponseCd")]
  [Computed]
  public string AdabasResponseCd_Json
  {
    get => NullIf(AdabasResponseCd, "");
    set => AdabasResponseCd = value;
  }

  /// <summary>Length of the CICS_RESOURCE_NM attribute.</summary>
  public const int CicsResourceNm_MaxLength = 8;

  /// <summary>
  /// The value of the CICS_RESOURCE_NM attribute.
  /// The name of CICS resource being accessed
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 5, Type = MemberType.Char, Length = CicsResourceNm_MaxLength)]
  public string CicsResourceNm
  {
    get => cicsResourceNm ?? "";
    set => cicsResourceNm =
      TrimEnd(Substring(value, 1, CicsResourceNm_MaxLength));
  }

  /// <summary>
  /// The json value of the CicsResourceNm attribute.</summary>
  [JsonPropertyName("cicsResourceNm")]
  [Computed]
  public string CicsResourceNm_Json
  {
    get => NullIf(CicsResourceNm, "");
    set => CicsResourceNm = value;
  }

  /// <summary>Length of the CICS_FUNCTION_CD attribute.</summary>
  public const int CicsFunctionCd_MaxLength = 2;

  /// <summary>
  /// The value of the CICS_FUNCTION_CD attribute.
  /// the type of cics function being performed
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 6, Type = MemberType.Char, Length = CicsFunctionCd_MaxLength)]
  public string CicsFunctionCd
  {
    get => cicsFunctionCd ?? "";
    set => cicsFunctionCd =
      TrimEnd(Substring(value, 1, CicsFunctionCd_MaxLength));
  }

  /// <summary>
  /// The json value of the CicsFunctionCd attribute.</summary>
  [JsonPropertyName("cicsFunctionCd")]
  [Computed]
  public string CicsFunctionCd_Json
  {
    get => NullIf(CicsFunctionCd, "");
    set => CicsFunctionCd = value;
  }

  /// <summary>Length of the CICS_RESPONSE_CD attribute.</summary>
  public const int CicsResponseCd_MaxLength = 6;

  /// <summary>
  /// The value of the CICS_RESPONSE_CD attribute.
  /// the error code returned by CICS
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 7, Type = MemberType.Char, Length = CicsResponseCd_MaxLength)]
  public string CicsResponseCd
  {
    get => cicsResponseCd ?? "";
    set => cicsResponseCd =
      TrimEnd(Substring(value, 1, CicsResponseCd_MaxLength));
  }

  /// <summary>
  /// The json value of the CicsResponseCd attribute.</summary>
  [JsonPropertyName("cicsResponseCd")]
  [Computed]
  public string CicsResponseCd_Json
  {
    get => NullIf(CicsResponseCd, "");
    set => CicsResponseCd = value;
  }

  private string type1;
  private string adabasFileNumber;
  private string adabasFileAction;
  private string adabasResponseCd;
  private string cicsResourceNm;
  private string cicsFunctionCd;
  private string cicsResponseCd;
}
