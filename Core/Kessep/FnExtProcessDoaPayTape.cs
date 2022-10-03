// Program: FN_EXT_PROCESS_DOA_PAY_TAPE, ID: 372720556, model: 746.
// Short name: SWEXF656
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_EXT_PROCESS_DOA_PAY_TAPE.
/// </summary>
[Serializable]
public partial class FnExtProcessDoaPayTape: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_EXT_PROCESS_DOA_PAY_TAPE program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnExtProcessDoaPayTape(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnExtProcessDoaPayTape.
  /// </summary>
  public FnExtProcessDoaPayTape(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    GetService<IEabStub>().Execute("SWEXF656", context, import, export, 0);
  }
#endregion

#region Parameters.
  protected readonly Import import;
  protected readonly Export export;
#endregion

#region Structures
  /// <summary>
  /// This class defines import view.
  /// </summary>
  [Serializable]
  public class Import
  {
    /// <summary>
    /// A value of External.
    /// </summary>
    [JsonPropertyName("external")]
    [Member(Index = 1, AccessFields = false, Members
      = new[] { "FileInstruction" })]
    public External External
    {
      get => external ??= new();
      set => external = value;
    }

    /// <summary>
    /// A value of RecordType.
    /// </summary>
    [JsonPropertyName("recordType")]
    [Member(Index = 2, AccessFields = false, Members = new[] { "Text4" })]
    public TextWorkArea RecordType
    {
      get => recordType ??= new();
      set => recordType = value;
    }

    /// <summary>
    /// A value of FileId.
    /// </summary>
    [JsonPropertyName("fileId")]
    [Member(Index = 3, AccessFields = false, Members = new[] { "Text4" })]
    public TextWorkArea FileId
    {
      get => fileId ??= new();
      set => fileId = value;
    }

    /// <summary>
    /// A value of PayTape.
    /// </summary>
    [JsonPropertyName("payTape")]
    [Member(Index = 4, AccessFields = false, Members
      = new[] { "ProcessDate", "VoucherNumber" })]
    public PayTape PayTape
    {
      get => payTape ??= new();
      set => payTape = value;
    }

    /// <summary>
    /// A value of Warrant.
    /// </summary>
    [JsonPropertyName("warrant")]
    [Member(Index = 5, AccessFields = false, Members = new[]
    {
      "SystemGeneratedIdentifier",
      "Type1",
      "Classification",
      "Amount",
      "ProcessDate",
      "CsePersonNumber",
      "DesignatedPayeeCsePersonNo",
      "InterstateInd"
    })]
    public PaymentRequest Warrant
    {
      get => warrant ??= new();
      set => warrant = value;
    }

    /// <summary>
    /// A value of WarrantPayee.
    /// </summary>
    [JsonPropertyName("warrantPayee")]
    [Member(Index = 6, AccessFields = false, Members = new[]
    {
      "Number",
      "Ssn",
      "FormattedName"
    })]
    public CsePersonsWorkSet WarrantPayee
    {
      get => warrantPayee ??= new();
      set => warrantPayee = value;
    }

    /// <summary>
    /// A value of WarrantPayeePrint.
    /// </summary>
    [JsonPropertyName("warrantPayeePrint")]
    [Member(Index = 7, AccessFields = false, Members = new[]
    {
      "Number",
      "Ssn",
      "FormattedName"
    })]
    public CsePersonsWorkSet WarrantPayeePrint
    {
      get => warrantPayeePrint ??= new();
      set => warrantPayeePrint = value;
    }

    /// <summary>
    /// A value of WarrantStubFor.
    /// </summary>
    [JsonPropertyName("warrantStubFor")]
    [Member(Index = 8, AccessFields = false, Members = new[]
    {
      "Number",
      "Ssn",
      "FormattedName"
    })]
    public CsePersonsWorkSet WarrantStubFor
    {
      get => warrantStubFor ??= new();
      set => warrantStubFor = value;
    }

    /// <summary>
    /// A value of WarrantMailingAddr.
    /// </summary>
    [JsonPropertyName("warrantMailingAddr")]
    [Member(Index = 9, AccessFields = false, Members = new[]
    {
      "LocationType",
      "Street1",
      "Street2",
      "City",
      "County",
      "State",
      "ZipCode",
      "Zip4",
      "Street3",
      "Street4",
      "Province",
      "PostalCode",
      "Country",
      "EndDate"
    })]
    public CsePersonAddress WarrantMailingAddr
    {
      get => warrantMailingAddr ??= new();
      set => warrantMailingAddr = value;
    }

    /// <summary>
    /// A value of InterstatWarrant.
    /// </summary>
    [JsonPropertyName("interstatWarrant")]
    [Member(Index = 10, AccessFields = false, Members = new[]
    {
      "InterstateInd",
      "ShortArName",
      "Fips",
      "MedicalInd",
      "ShortApName",
      "ApSsn",
      "OtherStateCaseNo",
      "CourtOrderNo"
    })]
    public InterstatWarrant InterstatWarrant
    {
      get => interstatWarrant ??= new();
      set => interstatWarrant = value;
    }

    /// <summary>
    /// A value of ReceiptRefund.
    /// </summary>
    [JsonPropertyName("receiptRefund")]
    [Member(Index = 11, AccessFields = false, Members = new[]
    {
      "ReasonCode",
      "PayeeName",
      "Amount",
      "RequestDate",
      "ReasonText"
    })]
    public ReceiptRefund ReceiptRefund
    {
      get => receiptRefund ??= new();
      set => receiptRefund = value;
    }

    /// <summary>
    /// A value of DisbursementTransaction.
    /// </summary>
    [JsonPropertyName("disbursementTransaction")]
    [Member(Index = 12, AccessFields = false, Members = new[]
    {
      "ReferenceNumber",
      "SystemGeneratedIdentifier",
      "Type1",
      "Amount",
      "ProcessDate",
      "CollectionDate",
      "DisbursementDate",
      "PassthruDate"
    })]
    public DisbursementTransaction DisbursementTransaction
    {
      get => disbursementTransaction ??= new();
      set => disbursementTransaction = value;
    }

    /// <summary>
    /// A value of DisbursementType.
    /// </summary>
    [JsonPropertyName("disbursementType")]
    [Member(Index = 13, AccessFields = false, Members = new[]
    {
      "SystemGeneratedIdentifier",
      "Code",
      "Name"
    })]
    public DisbursementType DisbursementType
    {
      get => disbursementType ??= new();
      set => disbursementType = value;
    }

    /// <summary>
    /// A value of Payor.
    /// </summary>
    [JsonPropertyName("payor")]
    [Member(Index = 14, AccessFields = false, Members = new[]
    {
      "Number",
      "Ssn",
      "FormattedName"
    })]
    public CsePersonsWorkSet Payor
    {
      get => payor ??= new();
      set => payor = value;
    }

    /// <summary>
    /// A value of Collection.
    /// </summary>
    [JsonPropertyName("collection")]
    [Member(Index = 15, AccessFields = false, Members
      = new[] { "CollectionDt", "CourtOrderAppliedTo" })]
    public Collection Collection
    {
      get => collection ??= new();
      set => collection = value;
    }

    /// <summary>
    /// A value of CollectionType.
    /// </summary>
    [JsonPropertyName("collectionType")]
    [Member(Index = 16, AccessFields = false, Members = new[] { "PrintName" })]
    public CollectionType CollectionType
    {
      get => collectionType ??= new();
      set => collectionType = value;
    }

    /// <summary>
    /// A value of PrintMsg.
    /// </summary>
    [JsonPropertyName("printMsg")]
    [Member(Index = 17, AccessFields = false, Members = new[] { "Text80" })]
    public MessageTextArea PrintMsg
    {
      get => printMsg ??= new();
      set => printMsg = value;
    }

    private External external;
    private TextWorkArea recordType;
    private TextWorkArea fileId;
    private PayTape payTape;
    private PaymentRequest warrant;
    private CsePersonsWorkSet warrantPayee;
    private CsePersonsWorkSet warrantPayeePrint;
    private CsePersonsWorkSet warrantStubFor;
    private CsePersonAddress warrantMailingAddr;
    private InterstatWarrant interstatWarrant;
    private ReceiptRefund receiptRefund;
    private DisbursementTransaction disbursementTransaction;
    private DisbursementType disbursementType;
    private CsePersonsWorkSet payor;
    private Collection collection;
    private CollectionType collectionType;
    private MessageTextArea printMsg;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of External.
    /// </summary>
    [JsonPropertyName("external")]
    [Member(Index = 1, AccessFields = false, Members
      = new[] { "NumericReturnCode" })]
    public External External
    {
      get => external ??= new();
      set => external = value;
    }

    private External external;
  }
#endregion
}
