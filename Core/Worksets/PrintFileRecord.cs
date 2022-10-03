// The source file: PRINT_FILE_RECORD, ID: 374396787, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Worksets;

[Serializable]
public partial class PrintFileRecord: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public PrintFileRecord()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public PrintFileRecord(PrintFileRecord that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new PrintFileRecord Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(PrintFileRecord that)
  {
    base.Assign(that);
    courtOrderLine = that.courtOrderLine;
    employerLine = that.employerLine;
  }

  /// <summary>Length of the COURT_ORDER_LINE attribute.</summary>
  public const int CourtOrderLine_MaxLength = 196;

  /// <summary>
  /// The value of the COURT_ORDER_LINE attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 1, Type = MemberType.Char, Length = CourtOrderLine_MaxLength)]
  public string CourtOrderLine
  {
    get => courtOrderLine ?? "";
    set => courtOrderLine =
      TrimEnd(Substring(value, 1, CourtOrderLine_MaxLength));
  }

  /// <summary>
  /// The json value of the CourtOrderLine attribute.</summary>
  [JsonPropertyName("courtOrderLine")]
  [Computed]
  public string CourtOrderLine_Json
  {
    get => NullIf(CourtOrderLine, "");
    set => CourtOrderLine = value;
  }

  /// <summary>Length of the EMPLOYER_LINE attribute.</summary>
  public const int EmployerLine_MaxLength = 252;

  /// <summary>
  /// The value of the EMPLOYER_LINE attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 2, Type = MemberType.Char, Length = EmployerLine_MaxLength)]
  public string EmployerLine
  {
    get => employerLine ?? "";
    set => employerLine = TrimEnd(Substring(value, 1, EmployerLine_MaxLength));
  }

  /// <summary>
  /// The json value of the EmployerLine attribute.</summary>
  [JsonPropertyName("employerLine")]
  [Computed]
  public string EmployerLine_Json
  {
    get => NullIf(EmployerLine, "");
    set => EmployerLine = value;
  }

  private string courtOrderLine;
  private string employerLine;
}
