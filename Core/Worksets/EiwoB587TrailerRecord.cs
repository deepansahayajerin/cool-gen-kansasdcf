// The source file: EIWO_B587_TRAILER_RECORD, ID: 1902488182, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Worksets;

[Serializable]
public partial class EiwoB587TrailerRecord: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public EiwoB587TrailerRecord()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public EiwoB587TrailerRecord(EiwoB587TrailerRecord that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new EiwoB587TrailerRecord Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(EiwoB587TrailerRecord that)
  {
    base.Assign(that);
    documentCode = that.documentCode;
    controlNumber = that.controlNumber;
    batchCount = that.batchCount;
    recordCount = that.recordCount;
    employerSentCount = that.employerSentCount;
    stateSentCount = that.stateSentCount;
    errorFieldName = that.errorFieldName;
  }

  /// <summary>Length of the DOCUMENT_CODE attribute.</summary>
  public const int DocumentCode_MaxLength = 3;

  /// <summary>
  /// The value of the DOCUMENT_CODE attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 1, Type = MemberType.Char, Length = DocumentCode_MaxLength)]
  public string DocumentCode
  {
    get => documentCode ?? "";
    set => documentCode = TrimEnd(Substring(value, 1, DocumentCode_MaxLength));
  }

  /// <summary>
  /// The json value of the DocumentCode attribute.</summary>
  [JsonPropertyName("documentCode")]
  [Computed]
  public string DocumentCode_Json
  {
    get => NullIf(DocumentCode, "");
    set => DocumentCode = value;
  }

  /// <summary>Length of the CONTROL_NUMBER attribute.</summary>
  public const int ControlNumber_MaxLength = 22;

  /// <summary>
  /// The value of the CONTROL_NUMBER attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 2, Type = MemberType.Char, Length = ControlNumber_MaxLength)]
  public string ControlNumber
  {
    get => controlNumber ?? "";
    set => controlNumber =
      TrimEnd(Substring(value, 1, ControlNumber_MaxLength));
  }

  /// <summary>
  /// The json value of the ControlNumber attribute.</summary>
  [JsonPropertyName("controlNumber")]
  [Computed]
  public string ControlNumber_Json
  {
    get => NullIf(ControlNumber, "");
    set => ControlNumber = value;
  }

  /// <summary>Length of the BATCH_COUNT attribute.</summary>
  public const int BatchCount_MaxLength = 5;

  /// <summary>
  /// The value of the BATCH_COUNT attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 3, Type = MemberType.Char, Length = BatchCount_MaxLength)]
  public string BatchCount
  {
    get => batchCount ?? "";
    set => batchCount = TrimEnd(Substring(value, 1, BatchCount_MaxLength));
  }

  /// <summary>
  /// The json value of the BatchCount attribute.</summary>
  [JsonPropertyName("batchCount")]
  [Computed]
  public string BatchCount_Json
  {
    get => NullIf(BatchCount, "");
    set => BatchCount = value;
  }

  /// <summary>Length of the RECORD_COUNT attribute.</summary>
  public const int RecordCount_MaxLength = 5;

  /// <summary>
  /// The value of the RECORD_COUNT attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 4, Type = MemberType.Char, Length = RecordCount_MaxLength)]
  public string RecordCount
  {
    get => recordCount ?? "";
    set => recordCount = TrimEnd(Substring(value, 1, RecordCount_MaxLength));
  }

  /// <summary>
  /// The json value of the RecordCount attribute.</summary>
  [JsonPropertyName("recordCount")]
  [Computed]
  public string RecordCount_Json
  {
    get => NullIf(RecordCount, "");
    set => RecordCount = value;
  }

  /// <summary>Length of the EMPLOYER_SENT_COUNT attribute.</summary>
  public const int EmployerSentCount_MaxLength = 5;

  /// <summary>
  /// The value of the EMPLOYER_SENT_COUNT attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 5, Type = MemberType.Char, Length
    = EmployerSentCount_MaxLength)]
  public string EmployerSentCount
  {
    get => employerSentCount ?? "";
    set => employerSentCount =
      TrimEnd(Substring(value, 1, EmployerSentCount_MaxLength));
  }

  /// <summary>
  /// The json value of the EmployerSentCount attribute.</summary>
  [JsonPropertyName("employerSentCount")]
  [Computed]
  public string EmployerSentCount_Json
  {
    get => NullIf(EmployerSentCount, "");
    set => EmployerSentCount = value;
  }

  /// <summary>Length of the STATE_SENT_COUNT attribute.</summary>
  public const int StateSentCount_MaxLength = 5;

  /// <summary>
  /// The value of the STATE_SENT_COUNT attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 6, Type = MemberType.Char, Length = StateSentCount_MaxLength)]
  public string StateSentCount
  {
    get => stateSentCount ?? "";
    set => stateSentCount =
      TrimEnd(Substring(value, 1, StateSentCount_MaxLength));
  }

  /// <summary>
  /// The json value of the StateSentCount attribute.</summary>
  [JsonPropertyName("stateSentCount")]
  [Computed]
  public string StateSentCount_Json
  {
    get => NullIf(StateSentCount, "");
    set => StateSentCount = value;
  }

  /// <summary>Length of the ERROR_FIELD_NAME attribute.</summary>
  public const int ErrorFieldName_MaxLength = 18;

  /// <summary>
  /// The value of the ERROR_FIELD_NAME attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 7, Type = MemberType.Char, Length = ErrorFieldName_MaxLength)]
  public string ErrorFieldName
  {
    get => errorFieldName ?? "";
    set => errorFieldName =
      TrimEnd(Substring(value, 1, ErrorFieldName_MaxLength));
  }

  /// <summary>
  /// The json value of the ErrorFieldName attribute.</summary>
  [JsonPropertyName("errorFieldName")]
  [Computed]
  public string ErrorFieldName_Json
  {
    get => NullIf(ErrorFieldName, "");
    set => ErrorFieldName = value;
  }

  private string documentCode;
  private string controlNumber;
  private string batchCount;
  private string recordCount;
  private string employerSentCount;
  private string stateSentCount;
  private string errorFieldName;
}
