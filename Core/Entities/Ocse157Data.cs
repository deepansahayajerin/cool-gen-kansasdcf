// The source file: OCSE157_DATA, ID: 371092461, model: 746.
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Entities;

/// <summary>
/// RESP:  FINNANCE
/// 
/// This entity type is used to store data for the ocse157 report. A
/// coolgen program will populate this DB2 table. A Natural program
/// will then read these numbers to create the ocse157 report.
/// </summary>
[Serializable]
public partial class Ocse157Data: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public Ocse157Data()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public Ocse157Data(Ocse157Data that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new Ocse157Data Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(Ocse157Data that)
  {
    base.Assign(that);
    fiscalYear = that.fiscalYear;
    runNumber = that.runNumber;
    lineNumber = that.lineNumber;
    column = that.column;
    createdTimestamp = that.createdTimestamp;
    number = that.number;
  }

  /// <summary>
  /// The value of the FISCAL_YEAR attribute.
  /// Reporting year.
  /// </summary>
  [JsonPropertyName("fiscalYear")]
  [Member(Index = 1, Type = MemberType.Number, Length = 4, Optional = true)]
  public int? FiscalYear
  {
    get => fiscalYear;
    set => fiscalYear = value;
  }

  /// <summary>
  /// The value of the RUN_NUMBER attribute.
  /// Used as additional qualifier incase report needs to be ran multiple times 
  /// for the same FY. Set to 1 for first run, incremented by 1 for subsequent
  /// runs.
  /// </summary>
  [JsonPropertyName("runNumber")]
  [Member(Index = 2, Type = MemberType.Number, Length = 2, Optional = true)]
  public int? RunNumber
  {
    get => runNumber;
    set => runNumber = value;
  }

  /// <summary>Length of the LINE_NUMBER attribute.</summary>
  public const int LineNumber_MaxLength = 3;

  /// <summary>
  /// The value of the LINE_NUMBER attribute.
  /// Line being reported. Eg. 1d, 24, 18a, ..etc.
  /// </summary>
  [JsonPropertyName("lineNumber")]
  [Member(Index = 3, Type = MemberType.Char, Length = LineNumber_MaxLength, Optional
    = true)]
  public string LineNumber
  {
    get => lineNumber;
    set => lineNumber = value != null
      ? TrimEnd(Substring(value, 1, LineNumber_MaxLength)) : null;
  }

  /// <summary>Length of the COLUMN attribute.</summary>
  public const int Column_MaxLength = 1;

  /// <summary>
  /// The value of the COLUMN attribute.
  /// Column being reported. Example a-total, b-current assistance, c-former 
  /// assistance, d-never assistance.
  /// </summary>
  [JsonPropertyName("column")]
  [Member(Index = 4, Type = MemberType.Char, Length = Column_MaxLength, Optional
    = true)]
  public string Column
  {
    get => column;
    set => column = value != null
      ? TrimEnd(Substring(value, 1, Column_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the CREATED_TIMESTAMP attribute.
  /// created_timestamp- also used as primary identifier.
  /// </summary>
  [JsonPropertyName("createdTimestamp")]
  [Member(Index = 5, Type = MemberType.Timestamp)]
  public DateTime? CreatedTimestamp
  {
    get => createdTimestamp;
    set => createdTimestamp = value;
  }

  /// <summary>
  /// The value of the NUMBER attribute.
  /// Number being reported for this line. It could be the number Cases or 
  /// Amount of arrears.
  /// </summary>
  [JsonPropertyName("number")]
  [Member(Index = 6, Type = MemberType.Number, Length = 15, Optional = true)]
  public long? Number
  {
    get => number;
    set => number = value;
  }

  private int? fiscalYear;
  private int? runNumber;
  private string lineNumber;
  private string column;
  private DateTime? createdTimestamp;
  private long? number;
}
