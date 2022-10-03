// The source file: PROGRAM_INDICATOR, ID: 371439835, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Entities;

/// <summary>
/// RESP: SRVPLAN
/// This entity is created to retain  the history of retained child support and 
/// IV-D  fee.
/// </summary>
[Serializable]
public partial class ProgramIndicator: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public ProgramIndicator()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public ProgramIndicator(ProgramIndicator that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new ProgramIndicator Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(ProgramIndicator that)
  {
    base.Assign(that);
    childSupportRetentionCode = that.childSupportRetentionCode;
    ivDFeeIndicator = that.ivDFeeIndicator;
    effectiveDate = that.effectiveDate;
    discontinueDate = that.discontinueDate;
    prgGeneratedId = that.prgGeneratedId;
  }

  /// <summary>Length of the CHILD_SUPPORT_RETENTION_CODE attribute.</summary>
  public const int ChildSupportRetentionCode_MaxLength = 5;

  /// <summary>
  /// The value of the CHILD_SUPPORT_RETENTION_CODE attribute.
  /// A code describing the agency or person twho is th payee for collected 
  /// child support. Examples: IV-A agency, IV-E agency.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 1, Type = MemberType.Char, Length
    = ChildSupportRetentionCode_MaxLength)]
  public string ChildSupportRetentionCode
  {
    get => childSupportRetentionCode ?? "";
    set => childSupportRetentionCode =
      TrimEnd(Substring(value, 1, ChildSupportRetentionCode_MaxLength));
  }

  /// <summary>
  /// The json value of the ChildSupportRetentionCode attribute.</summary>
  [JsonPropertyName("childSupportRetentionCode")]
  [Computed]
  public string ChildSupportRetentionCode_Json
  {
    get => NullIf(ChildSupportRetentionCode, "");
    set => ChildSupportRetentionCode = value;
  }

  /// <summary>Length of the IV_D_FEE_INDICATOR attribute.</summary>
  public const int IvDFeeIndicator_MaxLength = 1;

  /// <summary>
  /// The value of the IV_D_FEE_INDICATOR attribute.
  /// An indicator that specifies if a IV-D cost recovery fee is to be assesed.
  /// Y = Assess fee
  /// N = Waive fee
  /// indicate if program is a IV D Fee? (Y or N)
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 2, Type = MemberType.Char, Length = IvDFeeIndicator_MaxLength)]
    
  public string IvDFeeIndicator
  {
    get => ivDFeeIndicator ?? "";
    set => ivDFeeIndicator =
      TrimEnd(Substring(value, 1, IvDFeeIndicator_MaxLength));
  }

  /// <summary>
  /// The json value of the IvDFeeIndicator attribute.</summary>
  [JsonPropertyName("ivDFeeIndicator")]
  [Computed]
  public string IvDFeeIndicator_Json
  {
    get => NullIf(IvDFeeIndicator, "");
    set => IvDFeeIndicator = value;
  }

  /// <summary>
  /// The value of the EFFECTIVE_DATE attribute.
  /// date program begins
  /// </summary>
  [JsonPropertyName("effectiveDate")]
  [Member(Index = 3, Type = MemberType.Date)]
  public DateTime? EffectiveDate
  {
    get => effectiveDate;
    set => effectiveDate = value;
  }

  /// <summary>
  /// The value of the DISCONTINUE_DATE attribute.
  /// date program ends.
  /// </summary>
  [JsonPropertyName("discontinueDate")]
  [Member(Index = 4, Type = MemberType.Date)]
  public DateTime? DiscontinueDate
  {
    get => discontinueDate;
    set => discontinueDate = value;
  }

  /// <summary>
  /// The value of the SYSTEM_GENERATED_IDENTIFIER attribute.
  /// identifies the program
  /// </summary>
  [JsonPropertyName("prgGeneratedId")]
  [DefaultValue(0)]
  [Member(Index = 5, Type = MemberType.Number, Length = 3)]
  public int PrgGeneratedId
  {
    get => prgGeneratedId;
    set => prgGeneratedId = value;
  }

  private string childSupportRetentionCode;
  private string ivDFeeIndicator;
  private DateTime? effectiveDate;
  private DateTime? discontinueDate;
  private int prgGeneratedId;
}
