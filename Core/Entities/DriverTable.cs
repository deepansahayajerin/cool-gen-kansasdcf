// The source file: DRIVER_TABLE, ID: 371434040, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Entities;

/// <summary>
/// RESP: FINANCE
/// This is a designer added entity type.  This table will be loaded during the 
/// disbursement process each night with the CSE Person Number for all Obligees
/// who had Collection Disbursement Transactions created for them.  This table
/// will drive the disbursement process that actually creates a disbursement,
/// fee or recapture for each collection.  This table will (may) also drive the
/// nightly passthru process.
/// </summary>
[Serializable]
public partial class DriverTable: Heap, ICloneable
{
  /// <summary>Default constructor.</summary>
  public DriverTable()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public DriverTable(DriverTable that):
    base(that)
  {
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new DriverTable Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>
  /// The value of the YEAR_MONTH attribute.
  /// Contains the year and month that a table should be processed for.  This 
  /// attribute will be used by the monthly processes so that a select can be
  /// limited to just those rows that need to be processed in a particular
  /// monthly run.
  /// </summary>
  [JsonPropertyName("yearMonth")]
  [Member(Index = 1, Type = MemberType.Number, Length = 6, Optional = true)]
  public int? YearMonth
  {
    get => Get<int?>("yearMonth");
    set => Set("yearMonth", value);
  }

  /// <summary>
  /// The value of the PROCESSED_DATE attribute.
  /// This attribute will contain the date that the driver table row was 
  /// processed by the batch process that it was placed on the table for.  This
  /// provides an audit of when a driver table row was processed.  It also
  /// provides restarted processes a way to select only the unprocessed rows.
  /// </summary>
  [JsonPropertyName("processedDate")]
  [Member(Index = 2, Type = MemberType.Date, Optional = true)]
  public DateTime? ProcessedDate
  {
    get => Get<DateTime?>("processedDate");
    set => Set("processedDate", value);
  }

  /// <summary>Length of the PROCESS_ID attribute.</summary>
  public const int ProcessId_MaxLength = 8;

  /// <summary>
  /// The value of the PROCESS_ID attribute.
  /// The name of the process that will use (be driven by) this row.  This will 
  /// allow a process that uses the driver table to select rows for just its own
  /// processing.  This will improve the efficiency of the table driven batch
  /// processes.	
  /// EX:
  /// MOPASST - The name of the process (load module name) that creates monthly 
  /// passthrus.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 3, Type = MemberType.Char, Length = ProcessId_MaxLength)]
  public string ProcessId
  {
    get => Get<string>("processId") ?? "";
    set => Set(
      "processId", IsEmpty(value) ? null : TrimEnd
      (Substring(value, 1, ProcessId_MaxLength)));
  }

  /// <summary>
  /// The json value of the ProcessId attribute.</summary>
  [JsonPropertyName("processId")]
  [Computed]
  public string ProcessId_Json
  {
    get => NullIf(ProcessId, "");
    set => ProcessId = value;
  }

  /// <summary>Length of the CSE_PERSON_NUMBER attribute.</summary>
  public const int CsePersonNumber_MaxLength = 10;

  /// <summary>
  /// The value of the CSE_PERSON_NUMBER attribute.
  /// The CSE Person Number of the person receiving the disbursement.
  /// </summary>
  [JsonPropertyName("csePersonNumber")]
  [Member(Index = 4, Type = MemberType.Char, Length
    = CsePersonNumber_MaxLength, Optional = true)]
  public string CsePersonNumber
  {
    get => Get<string>("csePersonNumber");
    set => Set(
      "csePersonNumber",
      TrimEnd(Substring(value, 1, CsePersonNumber_MaxLength)));
  }

  /// <summary>
  /// The value of the COLL_OBLI_TRAN_SYS_GEN_ID attribute.
  /// A unique, system generated random number to provide a unique identifier 
  /// for each transaction.
  /// </summary>
  [JsonPropertyName("collObliTranSysGenId")]
  [Member(Index = 5, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? CollObliTranSysGenId
  {
    get => Get<int?>("collObliTranSysGenId");
    set => Set("collObliTranSysGenId", value);
  }

  /// <summary>Length of the COURT_ORDER_NUMBER attribute.</summary>
  public const int CourtOrderNumber_MaxLength = 20;

  /// <summary>
  /// The value of the COURT_ORDER_NUMBER attribute.
  /// The unique identifier assigned to the court order by the court.
  /// </summary>
  [JsonPropertyName("courtOrderNumber")]
  [Member(Index = 6, Type = MemberType.Char, Length
    = CourtOrderNumber_MaxLength, Optional = true)]
  public string CourtOrderNumber
  {
    get => Get<string>("courtOrderNumber");
    set => Set(
      "courtOrderNumber",
      TrimEnd(Substring(value, 1, CourtOrderNumber_MaxLength)));
  }

  /// <summary>
  /// The value of the AMOUNT attribute.
  /// Amount of the obligation transaction.
  /// Examples: Amount of a debt, refund, collection or adjustment.
  /// </summary>
  [JsonPropertyName("amount")]
  [Member(Index = 7, Type = MemberType.Number, Length = 9, Precision = 2, Optional
    = true)]
  public decimal? Amount
  {
    get => Get<decimal?>("amount");
    set => Set("amount", Truncate(value, 2));
  }

  /// <summary>
  /// The value of the COLLECTION_DATE attribute.
  /// The date of Collection.
  /// </summary>
  [JsonPropertyName("collectionDate")]
  [Member(Index = 8, Type = MemberType.Date, Optional = true)]
  public DateTime? CollectionDate
  {
    get => Get<DateTime?>("collectionDate");
    set => Set("collectionDate", value);
  }

  /// <summary>
  /// The value of the DEBT_OBLI_TRAN_SYS_GEN_ID attribute.
  /// A unique, system generated random number to provide a unique identifier 
  /// for each transaction.
  /// </summary>
  [JsonPropertyName("debtObliTranSysGenId")]
  [Member(Index = 9, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? DebtObliTranSysGenId
  {
    get => Get<int?>("debtObliTranSysGenId");
    set => Set("debtObliTranSysGenId", value);
  }

  /// <summary>
  /// The value of the ADC_DATE attribute.
  /// The date the debt program type was changed to ADC.  At this time this debt
  /// would have been assigned to ADC and would be owed to the state from this
  /// point on regardless of other program changes that may occurr in the
  /// future.
  /// </summary>
  [JsonPropertyName("adcDate")]
  [Member(Index = 10, Type = MemberType.Date, Optional = true)]
  public DateTime? AdcDate
  {
    get => Get<DateTime?>("adcDate");
    set => Set("adcDate", value);
  }

  /// <summary>Length of the DEBT_CLASSIFICATION attribute.</summary>
  public const int DebtClassification_MaxLength = 20;

  /// <summary>
  /// The value of the DEBT_CLASSIFICATION attribute.
  /// Defines a class of debt type to aid in determining how processes will 
  /// enteract with the various debt types.
  /// Examples: Current Support, Arrearage, or Recovery.
  /// </summary>
  [JsonPropertyName("debtClassification")]
  [Member(Index = 11, Type = MemberType.Char, Length
    = DebtClassification_MaxLength, Optional = true)]
  [Value(null)]
  [Value("A")]
  [Value("R")]
  [Value("J")]
  [Value("S")]
  [ImplicitValue("S")]
  public string DebtClassification
  {
    get => Get<string>("debtClassification");
    set => Set(
      "debtClassification", TrimEnd(Substring(value, 1,
      DebtClassification_MaxLength)));
  }

  /// <summary>Length of the PROGRAM_TYPE attribute.</summary>
  public const int ProgramType_MaxLength = 3;

  /// <summary>
  /// The value of the PROGRAM_TYPE attribute.
  /// This is to indicate whether there is federal funding participation in the 
  /// program.  The field will indicate either ADC, GA or will be blank to
  /// indicate Non-ADC.
  /// </summary>
  [JsonPropertyName("programType")]
  [Member(Index = 12, Type = MemberType.Char, Length = ProgramType_MaxLength, Optional
    = true)]
  [Value(null)]
  [Value("GA")]
  [Value("ADC")]
  public string ProgramType
  {
    get => Get<string>("programType");
    set => Set(
      "programType", TrimEnd(Substring(value, 1, ProgramType_MaxLength)));
  }

  /// <summary>
  /// The value of the CREATED_TMST attribute.
  /// The timestamp for the date the pay record was created.
  /// </summary>
  [JsonPropertyName("createdTmst")]
  [Member(Index = 13, Type = MemberType.Timestamp)]
  public DateTime? CreatedTmst
  {
    get => Get<DateTime?>("createdTmst");
    set => Set("createdTmst", value);
  }
}
