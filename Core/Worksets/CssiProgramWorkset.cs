// The source file: CSSI_PROGRAM_WORKSET, ID: 1902453316, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Worksets;

[Serializable]
public partial class CssiProgramWorkset: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public CssiProgramWorkset()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public CssiProgramWorkset(CssiProgramWorkset that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new CssiProgramWorkset Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(CssiProgramWorkset that)
  {
    base.Assign(that);
    afIndicator = that.afIndicator;
    afiIndicator = that.afiIndicator;
    ccIndicator = that.ccIndicator;
    ciIndicator = that.ciIndicator;
    fcIndicator = that.fcIndicator;
    fciIndicator = that.fciIndicator;
    fsIndicator = that.fsIndicator;
    maIndicator = that.maIndicator;
    maiIndicator = that.maiIndicator;
    mkIndicator = that.mkIndicator;
    mpIndicator = that.mpIndicator;
    msIndicator = that.msIndicator;
    naIndicator = that.naIndicator;
    naiIndicator = that.naiIndicator;
    ncIndicator = that.ncIndicator;
    nfIndicator = that.nfIndicator;
    siIndicator = that.siIndicator;
  }

  /// <summary>Length of the AF_INDICATOR attribute.</summary>
  public const int AfIndicator_MaxLength = 1;

  /// <summary>
  /// The value of the AF_INDICATOR attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 1, Type = MemberType.Char, Length = AfIndicator_MaxLength)]
  public string AfIndicator
  {
    get => afIndicator ?? "";
    set => afIndicator = TrimEnd(Substring(value, 1, AfIndicator_MaxLength));
  }

  /// <summary>
  /// The json value of the AfIndicator attribute.</summary>
  [JsonPropertyName("afIndicator")]
  [Computed]
  public string AfIndicator_Json
  {
    get => NullIf(AfIndicator, "");
    set => AfIndicator = value;
  }

  /// <summary>Length of the AFI_INDICATOR attribute.</summary>
  public const int AfiIndicator_MaxLength = 1;

  /// <summary>
  /// The value of the AFI_INDICATOR attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 2, Type = MemberType.Char, Length = AfiIndicator_MaxLength)]
  public string AfiIndicator
  {
    get => afiIndicator ?? "";
    set => afiIndicator = TrimEnd(Substring(value, 1, AfiIndicator_MaxLength));
  }

  /// <summary>
  /// The json value of the AfiIndicator attribute.</summary>
  [JsonPropertyName("afiIndicator")]
  [Computed]
  public string AfiIndicator_Json
  {
    get => NullIf(AfiIndicator, "");
    set => AfiIndicator = value;
  }

  /// <summary>Length of the CC_INDICATOR attribute.</summary>
  public const int CcIndicator_MaxLength = 1;

  /// <summary>
  /// The value of the CC_INDICATOR attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 3, Type = MemberType.Char, Length = CcIndicator_MaxLength)]
  public string CcIndicator
  {
    get => ccIndicator ?? "";
    set => ccIndicator = TrimEnd(Substring(value, 1, CcIndicator_MaxLength));
  }

  /// <summary>
  /// The json value of the CcIndicator attribute.</summary>
  [JsonPropertyName("ccIndicator")]
  [Computed]
  public string CcIndicator_Json
  {
    get => NullIf(CcIndicator, "");
    set => CcIndicator = value;
  }

  /// <summary>Length of the CI_INDICATOR attribute.</summary>
  public const int CiIndicator_MaxLength = 1;

  /// <summary>
  /// The value of the CI_INDICATOR attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 4, Type = MemberType.Char, Length = CiIndicator_MaxLength)]
  public string CiIndicator
  {
    get => ciIndicator ?? "";
    set => ciIndicator = TrimEnd(Substring(value, 1, CiIndicator_MaxLength));
  }

  /// <summary>
  /// The json value of the CiIndicator attribute.</summary>
  [JsonPropertyName("ciIndicator")]
  [Computed]
  public string CiIndicator_Json
  {
    get => NullIf(CiIndicator, "");
    set => CiIndicator = value;
  }

  /// <summary>Length of the FC_INDICATOR attribute.</summary>
  public const int FcIndicator_MaxLength = 1;

  /// <summary>
  /// The value of the FC_INDICATOR attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 5, Type = MemberType.Char, Length = FcIndicator_MaxLength)]
  public string FcIndicator
  {
    get => fcIndicator ?? "";
    set => fcIndicator = TrimEnd(Substring(value, 1, FcIndicator_MaxLength));
  }

  /// <summary>
  /// The json value of the FcIndicator attribute.</summary>
  [JsonPropertyName("fcIndicator")]
  [Computed]
  public string FcIndicator_Json
  {
    get => NullIf(FcIndicator, "");
    set => FcIndicator = value;
  }

  /// <summary>Length of the FCI_INDICATOR attribute.</summary>
  public const int FciIndicator_MaxLength = 1;

  /// <summary>
  /// The value of the FCI_INDICATOR attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 6, Type = MemberType.Char, Length = FciIndicator_MaxLength)]
  public string FciIndicator
  {
    get => fciIndicator ?? "";
    set => fciIndicator = TrimEnd(Substring(value, 1, FciIndicator_MaxLength));
  }

  /// <summary>
  /// The json value of the FciIndicator attribute.</summary>
  [JsonPropertyName("fciIndicator")]
  [Computed]
  public string FciIndicator_Json
  {
    get => NullIf(FciIndicator, "");
    set => FciIndicator = value;
  }

  /// <summary>Length of the FS_INDICATOR attribute.</summary>
  public const int FsIndicator_MaxLength = 1;

  /// <summary>
  /// The value of the FS_INDICATOR attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 7, Type = MemberType.Char, Length = FsIndicator_MaxLength)]
  public string FsIndicator
  {
    get => fsIndicator ?? "";
    set => fsIndicator = TrimEnd(Substring(value, 1, FsIndicator_MaxLength));
  }

  /// <summary>
  /// The json value of the FsIndicator attribute.</summary>
  [JsonPropertyName("fsIndicator")]
  [Computed]
  public string FsIndicator_Json
  {
    get => NullIf(FsIndicator, "");
    set => FsIndicator = value;
  }

  /// <summary>Length of the MA_INDICATOR attribute.</summary>
  public const int MaIndicator_MaxLength = 1;

  /// <summary>
  /// The value of the MA_INDICATOR attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 8, Type = MemberType.Char, Length = MaIndicator_MaxLength)]
  public string MaIndicator
  {
    get => maIndicator ?? "";
    set => maIndicator = TrimEnd(Substring(value, 1, MaIndicator_MaxLength));
  }

  /// <summary>
  /// The json value of the MaIndicator attribute.</summary>
  [JsonPropertyName("maIndicator")]
  [Computed]
  public string MaIndicator_Json
  {
    get => NullIf(MaIndicator, "");
    set => MaIndicator = value;
  }

  /// <summary>Length of the MAI_INDICATOR attribute.</summary>
  public const int MaiIndicator_MaxLength = 1;

  /// <summary>
  /// The value of the MAI_INDICATOR attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 9, Type = MemberType.Char, Length = MaiIndicator_MaxLength)]
  public string MaiIndicator
  {
    get => maiIndicator ?? "";
    set => maiIndicator = TrimEnd(Substring(value, 1, MaiIndicator_MaxLength));
  }

  /// <summary>
  /// The json value of the MaiIndicator attribute.</summary>
  [JsonPropertyName("maiIndicator")]
  [Computed]
  public string MaiIndicator_Json
  {
    get => NullIf(MaiIndicator, "");
    set => MaiIndicator = value;
  }

  /// <summary>Length of the MK_INDICATOR attribute.</summary>
  public const int MkIndicator_MaxLength = 1;

  /// <summary>
  /// The value of the MK_INDICATOR attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 10, Type = MemberType.Char, Length = MkIndicator_MaxLength)]
  public string MkIndicator
  {
    get => mkIndicator ?? "";
    set => mkIndicator = TrimEnd(Substring(value, 1, MkIndicator_MaxLength));
  }

  /// <summary>
  /// The json value of the MkIndicator attribute.</summary>
  [JsonPropertyName("mkIndicator")]
  [Computed]
  public string MkIndicator_Json
  {
    get => NullIf(MkIndicator, "");
    set => MkIndicator = value;
  }

  /// <summary>Length of the MP_INDICATOR attribute.</summary>
  public const int MpIndicator_MaxLength = 1;

  /// <summary>
  /// The value of the MP_INDICATOR attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 11, Type = MemberType.Char, Length = MpIndicator_MaxLength)]
  public string MpIndicator
  {
    get => mpIndicator ?? "";
    set => mpIndicator = TrimEnd(Substring(value, 1, MpIndicator_MaxLength));
  }

  /// <summary>
  /// The json value of the MpIndicator attribute.</summary>
  [JsonPropertyName("mpIndicator")]
  [Computed]
  public string MpIndicator_Json
  {
    get => NullIf(MpIndicator, "");
    set => MpIndicator = value;
  }

  /// <summary>Length of the MS_INDICATOR attribute.</summary>
  public const int MsIndicator_MaxLength = 1;

  /// <summary>
  /// The value of the MS_INDICATOR attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 12, Type = MemberType.Char, Length = MsIndicator_MaxLength)]
  public string MsIndicator
  {
    get => msIndicator ?? "";
    set => msIndicator = TrimEnd(Substring(value, 1, MsIndicator_MaxLength));
  }

  /// <summary>
  /// The json value of the MsIndicator attribute.</summary>
  [JsonPropertyName("msIndicator")]
  [Computed]
  public string MsIndicator_Json
  {
    get => NullIf(MsIndicator, "");
    set => MsIndicator = value;
  }

  /// <summary>Length of the NA_INDICATOR attribute.</summary>
  public const int NaIndicator_MaxLength = 1;

  /// <summary>
  /// The value of the NA_INDICATOR attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 13, Type = MemberType.Char, Length = NaIndicator_MaxLength)]
  public string NaIndicator
  {
    get => naIndicator ?? "";
    set => naIndicator = TrimEnd(Substring(value, 1, NaIndicator_MaxLength));
  }

  /// <summary>
  /// The json value of the NaIndicator attribute.</summary>
  [JsonPropertyName("naIndicator")]
  [Computed]
  public string NaIndicator_Json
  {
    get => NullIf(NaIndicator, "");
    set => NaIndicator = value;
  }

  /// <summary>Length of the NAI_INDICATOR attribute.</summary>
  public const int NaiIndicator_MaxLength = 1;

  /// <summary>
  /// The value of the NAI_INDICATOR attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 14, Type = MemberType.Char, Length = NaiIndicator_MaxLength)]
  public string NaiIndicator
  {
    get => naiIndicator ?? "";
    set => naiIndicator = TrimEnd(Substring(value, 1, NaiIndicator_MaxLength));
  }

  /// <summary>
  /// The json value of the NaiIndicator attribute.</summary>
  [JsonPropertyName("naiIndicator")]
  [Computed]
  public string NaiIndicator_Json
  {
    get => NullIf(NaiIndicator, "");
    set => NaiIndicator = value;
  }

  /// <summary>Length of the NC_INDICATOR attribute.</summary>
  public const int NcIndicator_MaxLength = 1;

  /// <summary>
  /// The value of the NC_INDICATOR attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 15, Type = MemberType.Char, Length = NcIndicator_MaxLength)]
  public string NcIndicator
  {
    get => ncIndicator ?? "";
    set => ncIndicator = TrimEnd(Substring(value, 1, NcIndicator_MaxLength));
  }

  /// <summary>
  /// The json value of the NcIndicator attribute.</summary>
  [JsonPropertyName("ncIndicator")]
  [Computed]
  public string NcIndicator_Json
  {
    get => NullIf(NcIndicator, "");
    set => NcIndicator = value;
  }

  /// <summary>Length of the NF_INDICATOR attribute.</summary>
  public const int NfIndicator_MaxLength = 1;

  /// <summary>
  /// The value of the NF_INDICATOR attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 16, Type = MemberType.Char, Length = NfIndicator_MaxLength)]
  public string NfIndicator
  {
    get => nfIndicator ?? "";
    set => nfIndicator = TrimEnd(Substring(value, 1, NfIndicator_MaxLength));
  }

  /// <summary>
  /// The json value of the NfIndicator attribute.</summary>
  [JsonPropertyName("nfIndicator")]
  [Computed]
  public string NfIndicator_Json
  {
    get => NullIf(NfIndicator, "");
    set => NfIndicator = value;
  }

  /// <summary>Length of the SI_INDICATOR attribute.</summary>
  public const int SiIndicator_MaxLength = 1;

  /// <summary>
  /// The value of the SI_INDICATOR attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 17, Type = MemberType.Char, Length = SiIndicator_MaxLength)]
  public string SiIndicator
  {
    get => siIndicator ?? "";
    set => siIndicator = TrimEnd(Substring(value, 1, SiIndicator_MaxLength));
  }

  /// <summary>
  /// The json value of the SiIndicator attribute.</summary>
  [JsonPropertyName("siIndicator")]
  [Computed]
  public string SiIndicator_Json
  {
    get => NullIf(SiIndicator, "");
    set => SiIndicator = value;
  }

  private string afIndicator;
  private string afiIndicator;
  private string ccIndicator;
  private string ciIndicator;
  private string fcIndicator;
  private string fciIndicator;
  private string fsIndicator;
  private string maIndicator;
  private string maiIndicator;
  private string mkIndicator;
  private string mpIndicator;
  private string msIndicator;
  private string naIndicator;
  private string naiIndicator;
  private string ncIndicator;
  private string nfIndicator;
  private string siIndicator;
}
