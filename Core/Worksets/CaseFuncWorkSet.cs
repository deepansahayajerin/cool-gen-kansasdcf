// The source file: CASE_FUNC_WORK_SET, ID: 371731746, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Worksets;

[Serializable]
public partial class CaseFuncWorkSet: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public CaseFuncWorkSet()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public CaseFuncWorkSet(CaseFuncWorkSet that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new CaseFuncWorkSet Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(CaseFuncWorkSet that)
  {
    base.Assign(that);
    funcText1 = that.funcText1;
    funcText3 = that.funcText3;
    funcDate = that.funcDate;
  }

  /// <summary>Length of the FUNC_TEXT_1 attribute.</summary>
  public const int FuncText1_MaxLength = 1;

  /// <summary>
  /// The value of the FUNC_TEXT_1 attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 1, Type = MemberType.Char, Length = FuncText1_MaxLength)]
  public string FuncText1
  {
    get => funcText1 ?? "";
    set => funcText1 = TrimEnd(Substring(value, 1, FuncText1_MaxLength));
  }

  /// <summary>
  /// The json value of the FuncText1 attribute.</summary>
  [JsonPropertyName("funcText1")]
  [Computed]
  public string FuncText1_Json
  {
    get => NullIf(FuncText1, "");
    set => FuncText1 = value;
  }

  /// <summary>Length of the FUNC_TEXT_3 attribute.</summary>
  public const int FuncText3_MaxLength = 3;

  /// <summary>
  /// The value of the FUNC_TEXT_3 attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 2, Type = MemberType.Char, Length = FuncText3_MaxLength)]
  public string FuncText3
  {
    get => funcText3 ?? "";
    set => funcText3 = TrimEnd(Substring(value, 1, FuncText3_MaxLength));
  }

  /// <summary>
  /// The json value of the FuncText3 attribute.</summary>
  [JsonPropertyName("funcText3")]
  [Computed]
  public string FuncText3_Json
  {
    get => NullIf(FuncText3, "");
    set => FuncText3 = value;
  }

  /// <summary>
  /// The value of the FUNC_DATE attribute.
  /// </summary>
  [JsonPropertyName("funcDate")]
  [Member(Index = 3, Type = MemberType.Date)]
  public DateTime? FuncDate
  {
    get => funcDate;
    set => funcDate = value;
  }

  private string funcText1;
  private string funcText3;
  private DateTime? funcDate;
}
