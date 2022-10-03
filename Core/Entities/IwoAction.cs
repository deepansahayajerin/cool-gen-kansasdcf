// The source file: IWO_ACTION, ID: 1902467050, model: 746.
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Bphx.Cool;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Entities;

/// <summary>
/// Defines the action taken for an IWO_TRANSACTION.  Example, the IWO 
/// information was printed, submitted to the OCSE portal, or re-submitted to
/// the OCSE portal.
/// </summary>
[Serializable]
public partial class IwoAction: View, ICloneable
{
  /// <summary>Default constructor.</summary>
  public IwoAction()
  {
  }

  /// <summary>Copy constructor.</summary>
  /// <param name="that">An instance to copy.</param>
  public IwoAction(IwoAction that)
  {
    Init(that);
    Assign(that);
  }

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  public new IwoAction Clone() => new(this);

  /// <summary>Creates a copy of this object.</summary>
  /// <returns>A copy of this instance.</returns>
  object ICloneable.Clone() => Clone();

  /// <summary>Assigns value from another instance.</summary>
  /// <param name="that">Another instance.</param>
  public void Assign(IwoAction that)
  {
    base.Assign(that);
    identifier = that.identifier;
    actionType = that.actionType;
    statusCd = that.statusCd;
    statusDate = that.statusDate;
    statusReasonCode = that.statusReasonCode;
    documentTrackingIdentifier = that.documentTrackingIdentifier;
    fileControlId = that.fileControlId;
    batchControlId = that.batchControlId;
    severityClearedInd = that.severityClearedInd;
    errorRecordType = that.errorRecordType;
    errorField1 = that.errorField1;
    errorField2 = that.errorField2;
    moreErrorsInd = that.moreErrorsInd;
    createdBy = that.createdBy;
    createdTimestamp = that.createdTimestamp;
    lastUpdatedBy = that.lastUpdatedBy;
    lastUpdatedTimestamp = that.lastUpdatedTimestamp;
    infId = that.infId;
    cspNumber = that.cspNumber;
    lgaIdentifier = that.lgaIdentifier;
    iwtIdentifier = that.iwtIdentifier;
  }

  /// <summary>
  /// The value of the IDENTIFIER attribute.
  /// Sequential number used to identify unique occurrences for the same 
  /// IWO_TRANSACTION
  /// </summary>
  [JsonPropertyName("identifier")]
  [DefaultValue(0)]
  [Member(Index = 1, Type = MemberType.Number, Length = 3)]
  public int Identifier
  {
    get => identifier;
    set => identifier = value;
  }

  /// <summary>Length of the ACTION_TYPE attribute.</summary>
  public const int ActionType_MaxLength = 5;

  /// <summary>
  /// The value of the ACTION_TYPE attribute.
  /// Identifies the action to be taken for an IWO_TRANSACTION.  Example, the 
  /// IWO information is to be printed (PRINT), eIWO is to be submitted to the
  /// OCSE portal (E-IWO), or eIWO is to be re-submitted to the OCSE portal (
  /// RESUB).
  /// </summary>
  [JsonPropertyName("actionType")]
  [Member(Index = 2, Type = MemberType.Char, Length = ActionType_MaxLength, Optional
    = true)]
  public string ActionType
  {
    get => actionType;
    set => actionType = value != null
      ? TrimEnd(Substring(value, 1, ActionType_MaxLength)) : null;
  }

  /// <summary>Length of the STATUS_CD attribute.</summary>
  public const int StatusCd_MaxLength = 1;

  /// <summary>
  /// The value of the STATUS_CD attribute.
  /// One character code indicating the status of the IWO_ACTION.  Example, 
  /// ready to send to portal (S), sent to portal (N), receipted by portal (R),
  /// cancelled (C), error (E), employer accepted (A), employer rejected (J),
  /// queued for Printing (Q), Printed (P).
  /// </summary>
  [JsonPropertyName("statusCd")]
  [Member(Index = 3, Type = MemberType.Char, Length = StatusCd_MaxLength, Optional
    = true)]
  public string StatusCd
  {
    get => statusCd;
    set => statusCd = value != null
      ? TrimEnd(Substring(value, 1, StatusCd_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the STATUS_DATE attribute.
  /// The date the status_cd became effective.
  /// </summary>
  [JsonPropertyName("statusDate")]
  [Member(Index = 4, Type = MemberType.Date, Optional = true)]
  public DateTime? StatusDate
  {
    get => statusDate;
    set => statusDate = value;
  }

  /// <summary>Length of the STATUS_REASON_CODE attribute.</summary>
  public const int StatusReasonCode_MaxLength = 3;

  /// <summary>
  /// The value of the STATUS_REASON_CODE attribute.
  /// Code indicating why a transaction was rejected by an employer.  Also, may 
  /// be populated even if the transaction is accepted by the employer in which
  /// case it denotes something did not accurately match the employer records.
  /// </summary>
  [JsonPropertyName("statusReasonCode")]
  [Member(Index = 5, Type = MemberType.Char, Length
    = StatusReasonCode_MaxLength, Optional = true)]
  public string StatusReasonCode
  {
    get => statusReasonCode;
    set => statusReasonCode = value != null
      ? TrimEnd(Substring(value, 1, StatusReasonCode_MaxLength)) : null;
  }

  /// <summary>Length of the DOCUMENT_TRACKING_IDENTIFIER attribute.</summary>
  public const int DocumentTrackingIdentifier_MaxLength = 12;

  /// <summary>
  /// The value of the DOCUMENT_TRACKING_IDENTIFIER attribute.
  /// Tracking identifier assigned to the IWO document when submitted to the 
  /// Portal.
  /// </summary>
  [JsonPropertyName("documentTrackingIdentifier")]
  [Member(Index = 6, Type = MemberType.Char, Length
    = DocumentTrackingIdentifier_MaxLength, Optional = true)]
  public string DocumentTrackingIdentifier
  {
    get => documentTrackingIdentifier;
    set => documentTrackingIdentifier = value != null
      ? TrimEnd(Substring(value, 1, DocumentTrackingIdentifier_MaxLength)) : null
      ;
  }

  /// <summary>Length of the FILE_CONTROL_ID attribute.</summary>
  public const int FileControlId_MaxLength = 22;

  /// <summary>
  /// The value of the FILE_CONTROL_ID attribute.
  /// Control identifier of the portal interface file in which the IWO document 
  /// was submitted.
  /// </summary>
  [JsonPropertyName("fileControlId")]
  [Member(Index = 7, Type = MemberType.Char, Length = FileControlId_MaxLength, Optional
    = true)]
  public string FileControlId
  {
    get => fileControlId;
    set => fileControlId = value != null
      ? TrimEnd(Substring(value, 1, FileControlId_MaxLength)) : null;
  }

  /// <summary>Length of the BATCH_CONTROL_ID attribute.</summary>
  public const int BatchControlId_MaxLength = 22;

  /// <summary>
  /// The value of the BATCH_CONTROL_ID attribute.
  /// Batch identifier within the portal interface file in which IWO document 
  /// was submitted.
  /// </summary>
  [JsonPropertyName("batchControlId")]
  [Member(Index = 8, Type = MemberType.Char, Length
    = BatchControlId_MaxLength, Optional = true)]
  public string BatchControlId
  {
    get => batchControlId;
    set => batchControlId = value != null
      ? TrimEnd(Substring(value, 1, BatchControlId_MaxLength)) : null;
  }

  /// <summary>Length of the SEVERITY_CLEARED_IND attribute.</summary>
  public const int SeverityClearedInd_MaxLength = 1;

  /// <summary>
  /// The value of the SEVERITY_CLEARED_IND attribute.
  /// Yes/No value indicating if a user has manually cleared the severity of an 
  /// IWO_ACTION.  The severity determines the display characteristics of the
  /// IWO information.  For example, overdue items or items that require
  /// immediate user action are displayed in red, items that are pending but not
  /// overdue are displayed in yellow.
  /// </summary>
  [JsonPropertyName("severityClearedInd")]
  [Member(Index = 9, Type = MemberType.Char, Length
    = SeverityClearedInd_MaxLength, Optional = true)]
  public string SeverityClearedInd
  {
    get => severityClearedInd;
    set => severityClearedInd = value != null
      ? TrimEnd(Substring(value, 1, SeverityClearedInd_MaxLength)) : null;
  }

  /// <summary>Length of the ERROR_RECORD_TYPE attribute.</summary>
  public const int ErrorRecordType_MaxLength = 3;

  /// <summary>
  /// The value of the ERROR_RECORD_TYPE attribute.
  /// The record type returned by the portal in which an error was identified.
  /// </summary>
  [JsonPropertyName("errorRecordType")]
  [Member(Index = 10, Type = MemberType.Char, Length
    = ErrorRecordType_MaxLength, Optional = true)]
  public string ErrorRecordType
  {
    get => errorRecordType;
    set => errorRecordType = value != null
      ? TrimEnd(Substring(value, 1, ErrorRecordType_MaxLength)) : null;
  }

  /// <summary>Length of the ERROR_FIELD_1 attribute.</summary>
  public const int ErrorField1_MaxLength = 32;

  /// <summary>
  /// The value of the ERROR_FIELD_1 attribute.
  /// The first file field identified by the portal as being in error.
  /// </summary>
  [JsonPropertyName("errorField1")]
  [Member(Index = 11, Type = MemberType.Varchar, Length
    = ErrorField1_MaxLength, Optional = true)]
  public string ErrorField1
  {
    get => errorField1;
    set => errorField1 = value != null
      ? Substring(value, 1, ErrorField1_MaxLength) : null;
  }

  /// <summary>Length of the ERROR_FIELD_2 attribute.</summary>
  public const int ErrorField2_MaxLength = 32;

  /// <summary>
  /// The value of the ERROR_FIELD_2 attribute.
  /// The second file field identified by the portal as being in error.
  /// </summary>
  [JsonPropertyName("errorField2")]
  [Member(Index = 12, Type = MemberType.Varchar, Length
    = ErrorField2_MaxLength, Optional = true)]
  public string ErrorField2
  {
    get => errorField2;
    set => errorField2 = value != null
      ? Substring(value, 1, ErrorField2_MaxLength) : null;
  }

  /// <summary>Length of the MORE_ERRORS_IND attribute.</summary>
  public const int MoreErrorsInd_MaxLength = 1;

  /// <summary>
  /// The value of the MORE_ERRORS_IND attribute.
  /// A Y/N value returned from the portal indicating if more than two errors 
  /// exist in the record.
  /// </summary>
  [JsonPropertyName("moreErrorsInd")]
  [Member(Index = 13, Type = MemberType.Char, Length
    = MoreErrorsInd_MaxLength, Optional = true)]
  public string MoreErrorsInd
  {
    get => moreErrorsInd;
    set => moreErrorsInd = value != null
      ? TrimEnd(Substring(value, 1, MoreErrorsInd_MaxLength)) : null;
  }

  /// <summary>Length of the CREATED_BY attribute.</summary>
  public const int CreatedBy_MaxLength = 8;

  /// <summary>
  /// The value of the CREATED_BY attribute.
  /// USER ID or Program ID responsible for creation of the occurrence.
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 14, Type = MemberType.Char, Length = CreatedBy_MaxLength)]
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
  /// Timestamp of creation of the occurrence.
  /// 	
  /// </summary>
  [JsonPropertyName("createdTimestamp")]
  [Member(Index = 15, Type = MemberType.Timestamp)]
  public DateTime? CreatedTimestamp
  {
    get => createdTimestamp;
    set => createdTimestamp = value;
  }

  /// <summary>Length of the LAST_UPDATED_BY attribute.</summary>
  public const int LastUpdatedBy_MaxLength = 8;

  /// <summary>
  /// The value of the LAST_UPDATED_BY attribute.
  /// User ID or Program ID responsible for the last update of the occurrence.
  /// </summary>
  [JsonPropertyName("lastUpdatedBy")]
  [Member(Index = 16, Type = MemberType.Char, Length
    = LastUpdatedBy_MaxLength, Optional = true)]
  public string LastUpdatedBy
  {
    get => lastUpdatedBy;
    set => lastUpdatedBy = value != null
      ? TrimEnd(Substring(value, 1, LastUpdatedBy_MaxLength)) : null;
  }

  /// <summary>
  /// The value of the LAST_UPDATED_TIMESTAMP attribute.
  /// Timestamp of last update of the occurrence.
  /// </summary>
  [JsonPropertyName("lastUpdatedTimestamp")]
  [Member(Index = 17, Type = MemberType.Timestamp, Optional = true)]
  public DateTime? LastUpdatedTimestamp
  {
    get => lastUpdatedTimestamp;
    set => lastUpdatedTimestamp = value;
  }

  /// <summary>
  /// The value of the SYSTEM_GENERATED_IDENTIFIER attribute.
  /// A unique, system generated sequential number that distinguishes one 
  /// occurrence of the entity type from another.
  /// </summary>
  [JsonPropertyName("infId")]
  [Member(Index = 18, Type = MemberType.Number, Length = 9, Optional = true)]
  public int? InfId
  {
    get => infId;
    set => infId = value;
  }

  /// <summary>Length of the NUMBER attribute.</summary>
  public const int CspNumber_MaxLength = 10;

  /// <summary>
  /// The value of the NUMBER attribute.
  /// This is a system generated number which will be used by the users to 
  /// identify a person.  This will have a business meaning, but will be unique
  /// </summary>
  [JsonIgnore]
  [DefaultValue("")]
  [Member(Index = 19, Type = MemberType.Char, Length = CspNumber_MaxLength)]
  public string CspNumber
  {
    get => cspNumber ?? "";
    set => cspNumber = TrimEnd(Substring(value, 1, CspNumber_MaxLength));
  }

  /// <summary>
  /// The json value of the CspNumber attribute.</summary>
  [JsonPropertyName("cspNumber")]
  [Computed]
  public string CspNumber_Json
  {
    get => NullIf(CspNumber, "");
    set => CspNumber = value;
  }

  /// <summary>
  /// The value of the IDENTIFIER attribute.
  /// This is a system generated number to uniquely identify a legal action.
  /// </summary>
  [JsonPropertyName("lgaIdentifier")]
  [DefaultValue(0)]
  [Member(Index = 20, Type = MemberType.Number, Length = 9)]
  public int LgaIdentifier
  {
    get => lgaIdentifier;
    set => lgaIdentifier = value;
  }

  /// <summary>
  /// The value of the IDENTIFIER attribute.
  /// Sequential number used to identify unique occurrences for the same 
  /// cse_person and legal_action.
  /// </summary>
  [JsonPropertyName("iwtIdentifier")]
  [DefaultValue(0)]
  [Member(Index = 21, Type = MemberType.Number, Length = 3)]
  public int IwtIdentifier
  {
    get => iwtIdentifier;
    set => iwtIdentifier = value;
  }

  private int identifier;
  private string actionType;
  private string statusCd;
  private DateTime? statusDate;
  private string statusReasonCode;
  private string documentTrackingIdentifier;
  private string fileControlId;
  private string batchControlId;
  private string severityClearedInd;
  private string errorRecordType;
  private string errorField1;
  private string errorField2;
  private string moreErrorsInd;
  private string createdBy;
  private DateTime? createdTimestamp;
  private string lastUpdatedBy;
  private DateTime? lastUpdatedTimestamp;
  private int? infId;
  private string cspNumber;
  private int lgaIdentifier;
  private int iwtIdentifier;
}
