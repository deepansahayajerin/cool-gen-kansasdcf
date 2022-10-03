// The source file: PRINTER_OUTPUT_DESTINATION, ID: 371439603, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Entities;

/// <summary>
/// RESP: SRVPLAN
/// Each entry in this queue will point to a printer location for each county or
/// office. This is needed to allow documents to be printed to different
/// locations.
/// </summary>
[Serializable]
public partial class PrinterOutputDestination: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public PrinterOutputDestination()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public PrinterOutputDestination(PrinterOutputDestination that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new PrinterOutputDestination Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(PrinterOutputDestination that)
  {
    base.Assign(that);
    printerId = that.printerId;
    printerDescription = that.printerDescription;
    createdBy = that.createdBy;
    createdTimestamp = that.createdTimestamp;
    lastUpdatedBy = that.lastUpdatedBy;
    lastUpdatdTstamp = that.lastUpdatdTstamp;
    defaultInd = that.defaultInd;
    vtamPrinterId = that.vtamPrinterId;
    offGenerated = that.offGenerated;
  }

  /// <summary>Length of the PRINTER_ID attribute.</summary>
  public const int PrinterId_MaxLength = 8;

  /// <summary>
  /// The value of the PRINTER_ID attribute.
  /// User defined printer id. Unique to the system. Must be the actual printer 
  /// id that a program can use to print to.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 1, Type = MemberType.Char, Length = PrinterId_MaxLength)]
  public string PrinterId
  {
    get => printerId ?? "";
    set => printerId = TrimEnd(Substring(value, 1, PrinterId_MaxLength));
  }

  /// <summary>
  /// The json value of the PrinterId attribute.</summary>
  [JsonPropertyName("printerId")]
  [Computed]
  public string PrinterId_Json
  {
    get => NullIf(PrinterId, "");
    set => PrinterId = value;
  }

  /// <summary>Length of the PRINTER_DESCRIPTION attribute.</summary>
  public const int PrinterDescription_MaxLength = 30;

  /// <summary>
  /// The value of the PRINTER_DESCRIPTION attribute.
  /// Brief description of the printer.
  /// </summary>
  [JsonPropertyName("printerDescription")]
  [Member(Index = 2, Type = MemberType.Char, Length
    = PrinterDescription_MaxLength, Optional = true)]
  public string PrinterDescription
  {
    get => printerDescription;
    set => printerDescription = value != null
      ? TrimEnd(Substring(value, 1, PrinterDescription_MaxLength)) : null;
  }

  /// <summary>Length of the CREATED_BY attribute.</summary>
  public const int CreatedBy_MaxLength = 8;

  /// <summary>
  /// The value of the CREATED_BY attribute.
  /// * Draft *
  /// The user id or program id responsible for the creation of the occurrence.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 3, Type = MemberType.Char, Length = CreatedBy_MaxLength)]
  public string CreatedBy
  {
    get => createdBy ?? "";
    set => createdBy = TrimEnd(Substring(value, 1, CreatedBy_MaxLength));
  }

  /// <summary>
  /// The json value of the CreatedBy attribute.</summary>
  [JsonPropertyName("createdBy")]
  [Computed]
  public string CreatedBy_Json
  {
    get => NullIf(CreatedBy, "");
    set => CreatedBy = value;
  }

  /// <summary>
  /// The value of the CREATED_TIMESTAMP attribute.
  /// * Draft *
  /// The timestamp of when the occurrence of the entity was created.
  /// </summary>
  [JsonPropertyName("createdTimestamp")]
  [Member(Index = 4, Type = MemberType.Timestamp)]
  public DateTime? CreatedTimestamp
  {
    get => createdTimestamp;
    set => createdTimestamp = value;
  }

  /// <summary>Length of the LAST_UPDATED_BY attribute.</summary>
  public const int LastUpdatedBy_MaxLength = 8;

  /// <summary>
  /// The value of the LAST_UPDATED_BY attribute.
  /// * Draft *
  /// The user id or program id responsible for the last update to the 
  /// occurrence.
  /// </summary>
  [JsonPropertyName("lastUpdatedBy")]
  [Member(Index = 5, Type = MemberType.Char, Length = LastUpdatedBy_MaxLength, Optional
    = true)]
  public string LastUpdatedBy
  {
    get => lastUpdatedBy;
    set => lastUpdatedBy = value != null
      ? TrimEnd(Substring(value, 1, LastUpdatedBy_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the LAST_UPDATD_TSTAMP attribute.
  /// * Draft *
  /// The timestamp of the most recent update to the entity occurence.
  /// </summary>
  [JsonPropertyName("lastUpdatdTstamp")]
  [Member(Index = 6, Type = MemberType.Timestamp, Optional = true)]
  public DateTime? LastUpdatdTstamp
  {
    get => lastUpdatdTstamp;
    set => lastUpdatdTstamp = value;
  }

  /// <summary>Length of the DEFAULT_IND attribute.</summary>
  public const int DefaultInd_MaxLength = 1;

  /// <summary>
  /// The value of the DEFAULT_IND attribute.
  /// This attribute defines the default printer
  /// for the related office.    Values are:
  /// Y    -Yes, default printer
  /// 
  /// Space                -No, not the default
  /// printer
  /// </summary>
  [JsonPropertyName("defaultInd")]
  [Member(Index = 7, Type = MemberType.Char, Length = DefaultInd_MaxLength, Optional
    = true)]
  public string DefaultInd
  {
    get => defaultInd;
    set => defaultInd = value != null
      ? TrimEnd(Substring(value, 1, DefaultInd_MaxLength)) : null;
  }

  /// <summary>Length of the VTAM_PRINTER_ID attribute.</summary>
  public const int VtamPrinterId_MaxLength = 8;

  /// <summary>
  /// The value of the VTAM_PRINTER_ID attribute.
  /// Represents the valid VTAM printer ID for a destination printer located in 
  /// a CSE Office.
  /// </summary>
  [JsonPropertyName("vtamPrinterId")]
  [Member(Index = 8, Type = MemberType.Char, Length = VtamPrinterId_MaxLength, Optional
    = true)]
  public string VtamPrinterId
  {
    get => vtamPrinterId;
    set => vtamPrinterId = value != null
      ? TrimEnd(Substring(value, 1, VtamPrinterId_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the SYSTEM_GENERATED_ID attribute.
  /// A unique sequential number (See CONTROL_TABLE) that distinguishes one 
  /// occurrence of the entity type from another.
  /// </summary>
  [JsonPropertyName("offGenerated")]
  [Member(Index = 9, Type = MemberType.Number, Length = 4, Optional = true)]
  public int? OffGenerated
  {
    get => offGenerated;
    set => offGenerated = value;
  }

  private string printerId;
  private string printerDescription;
  private string createdBy;
  private DateTime? createdTimestamp;
  private string lastUpdatedBy;
  private DateTime? lastUpdatdTstamp;
  private string defaultInd;
  private string vtamPrinterId;
  private int? offGenerated;
}
