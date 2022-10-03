// The source file: EFT_TRANSMISSION_FILE_INFO, ID: 371947566, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Entities;

/// <summary>
/// RESP:  FINANCE
/// 
/// This file contains information about tyhe EFT transmission file. This
/// information includes the type of transmission (Inbound or outbound),
/// the date the file was created, the time the file was created, the
/// number of detail records and the total amount of those detail records.
/// </summary>
[Serializable]
public partial class EftTransmissionFileInfo: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public EftTransmissionFileInfo()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public EftTransmissionFileInfo(EftTransmissionFileInfo that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new EftTransmissionFileInfo Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(EftTransmissionFileInfo that)
  {
    base.Assign(that);
    transmissionType = that.transmissionType;
    fileCreationDate = that.fileCreationDate;
    fileCreationTime = that.fileCreationTime;
    recordCount = that.recordCount;
    totalAmount = that.totalAmount;
  }

  /// <summary>Length of the TRANSMISSION_TYPE attribute.</summary>
  public const int TransmissionType_MaxLength = 1;

  /// <summary>
  /// The value of the TRANSMISSION_TYPE attribute.
  /// This field stores information identifying the type of the EFT transmission
  /// file as either inbound or outbound.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 1, Type = MemberType.Char, Length = TransmissionType_MaxLength)
    ]
  [Value("O")]
  [Value("I")]
  public string TransmissionType
  {
    get => transmissionType ?? "";
    set => transmissionType =
      TrimEnd(Substring(value, 1, TransmissionType_MaxLength));
  }

  /// <summary>
  /// The json value of the TransmissionType attribute.</summary>
  [JsonPropertyName("transmissionType")]
  [Computed]
  public string TransmissionType_Json
  {
    get => NullIf(TransmissionType, "");
    set => TransmissionType = value;
  }

  /// <summary>
  /// The value of the FILE_CREATION_DATE attribute.
  /// This field stores the date that the EFT transmission file was created.
  /// </summary>
  [JsonPropertyName("fileCreationDate")]
  [Member(Index = 2, Type = MemberType.Date)]
  public DateTime? FileCreationDate
  {
    get => fileCreationDate;
    set => fileCreationDate = value;
  }

  /// <summary>
  /// The value of the FILE_CREATION_TIME attribute.
  /// This field stores the time that the EFT transmission file was created.
  /// </summary>
  [JsonPropertyName("fileCreationTime")]
  [Member(Index = 3, Type = MemberType.Time)]
  public TimeSpan FileCreationTime
  {
    get => fileCreationTime;
    set => fileCreationTime = value;
  }

  /// <summary>
  /// The value of the RECORD_COUNT attribute.
  /// This field stores the number of detail records that are in the EFT 
  /// transmission file.
  /// </summary>
  [JsonPropertyName("recordCount")]
  [DefaultValue(0)]
  [Member(Index = 4, Type = MemberType.Number, Length = 9)]
  public int RecordCount
  {
    get => recordCount;
    set => recordCount = value;
  }

  /// <summary>
  /// The value of the TOTAL_AMOUNT attribute.
  /// This field stores the total dollar amount of all of the detail records 
  /// contained within the EFT transmission file.
  /// </summary>
  [JsonPropertyName("totalAmount")]
  [DefaultValue(typeof(decimal), "0")]
  [Member(Index = 5, Type = MemberType.Number, Length = 9, Precision = 2)]
  public decimal TotalAmount
  {
    get => totalAmount;
    set => totalAmount = Truncate(value, 2);
  }

  private string transmissionType;
  private DateTime? fileCreationDate;
  private TimeSpan fileCreationTime;
  private int recordCount;
  private decimal totalAmount;
}
