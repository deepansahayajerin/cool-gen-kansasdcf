// The source file: EIWO_B587_HEADER_RECORD, ID: 1902488173, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Worksets;

[Serializable]
public partial class EiwoB587HeaderRecord: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public EiwoB587HeaderRecord()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public EiwoB587HeaderRecord(EiwoB587HeaderRecord that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new EiwoB587HeaderRecord Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(EiwoB587HeaderRecord that)
  {
    base.Assign(that);
    documentCode = that.documentCode;
    controlNumber = that.controlNumber;
    stateFipsCode = that.stateFipsCode;
    ein = that.ein;
    primaryEin = that.primaryEin;
    creationDate = that.creationDate;
    creationTime = that.creationTime;
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

  /// <summary>Length of the STATE_FIPS_CODE attribute.</summary>
  public const int StateFipsCode_MaxLength = 5;

  /// <summary>
  /// The value of the STATE_FIPS_CODE attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 3, Type = MemberType.Char, Length = StateFipsCode_MaxLength)]
  public string StateFipsCode
  {
    get => stateFipsCode ?? "";
    set => stateFipsCode =
      TrimEnd(Substring(value, 1, StateFipsCode_MaxLength));
  }

  /// <summary>
  /// The json value of the StateFipsCode attribute.</summary>
  [JsonPropertyName("stateFipsCode")]
  [Computed]
  public string StateFipsCode_Json
  {
    get => NullIf(StateFipsCode, "");
    set => StateFipsCode = value;
  }

  /// <summary>Length of the EIN attribute.</summary>
  public const int Ein_MaxLength = 9;

  /// <summary>
  /// The value of the EIN attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 4, Type = MemberType.Char, Length = Ein_MaxLength)]
  public string Ein
  {
    get => ein ?? "";
    set => ein = TrimEnd(Substring(value, 1, Ein_MaxLength));
  }

  /// <summary>
  /// The json value of the Ein attribute.</summary>
  [JsonPropertyName("ein")]
  [Computed]
  public string Ein_Json
  {
    get => NullIf(Ein, "");
    set => Ein = value;
  }

  /// <summary>Length of the PRIMARY_EIN attribute.</summary>
  public const int PrimaryEin_MaxLength = 9;

  /// <summary>
  /// The value of the PRIMARY_EIN attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 5, Type = MemberType.Char, Length = PrimaryEin_MaxLength)]
  public string PrimaryEin
  {
    get => primaryEin ?? "";
    set => primaryEin = TrimEnd(Substring(value, 1, PrimaryEin_MaxLength));
  }

  /// <summary>
  /// The json value of the PrimaryEin attribute.</summary>
  [JsonPropertyName("primaryEin")]
  [Computed]
  public string PrimaryEin_Json
  {
    get => NullIf(PrimaryEin, "");
    set => PrimaryEin = value;
  }

  /// <summary>Length of the CREATION_DATE attribute.</summary>
  public const int CreationDate_MaxLength = 8;

  /// <summary>
  /// The value of the CREATION_DATE attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 6, Type = MemberType.Char, Length = CreationDate_MaxLength)]
  public string CreationDate
  {
    get => creationDate ?? "";
    set => creationDate = TrimEnd(Substring(value, 1, CreationDate_MaxLength));
  }

  /// <summary>
  /// The json value of the CreationDate attribute.</summary>
  [JsonPropertyName("creationDate")]
  [Computed]
  public string CreationDate_Json
  {
    get => NullIf(CreationDate, "");
    set => CreationDate = value;
  }

  /// <summary>Length of the CREATION_TIME attribute.</summary>
  public const int CreationTime_MaxLength = 6;

  /// <summary>
  /// The value of the CREATION_TIME attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 7, Type = MemberType.Char, Length = CreationTime_MaxLength)]
  public string CreationTime
  {
    get => creationTime ?? "";
    set => creationTime = TrimEnd(Substring(value, 1, CreationTime_MaxLength));
  }

  /// <summary>
  /// The json value of the CreationTime attribute.</summary>
  [JsonPropertyName("creationTime")]
  [Computed]
  public string CreationTime_Json
  {
    get => NullIf(CreationTime, "");
    set => CreationTime = value;
  }

  /// <summary>Length of the ERROR_FIELD_NAME attribute.</summary>
  public const int ErrorFieldName_MaxLength = 18;

  /// <summary>
  /// The value of the ERROR_FIELD_NAME attribute.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 8, Type = MemberType.Char, Length = ErrorFieldName_MaxLength)]
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
  private string stateFipsCode;
  private string ein;
  private string primaryEin;
  private string creationDate;
  private string creationTime;
  private string errorFieldName;
}
