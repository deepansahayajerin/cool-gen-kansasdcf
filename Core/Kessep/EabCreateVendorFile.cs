// Program: EAB_CREATE_VENDOR_FILE, ID: 372685026, model: 746.
// Short name: SWEXFE43
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: EAB_CREATE_VENDOR_FILE.
/// </summary>
[Serializable]
public partial class EabCreateVendorFile: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the EAB_CREATE_VENDOR_FILE program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new EabCreateVendorFile(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of EabCreateVendorFile.
  /// </summary>
  public EabCreateVendorFile(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    GetService<IEabStub>().Execute(
      "SWEXFE43", context, import, export, EabOptions.Hpvp);
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
    /// A value of EabFileHandling.
    /// </summary>
    [JsonPropertyName("eabFileHandling")]
    [Member(Index = 1, AccessFields = false, Members = new[] { "Action" })]
    public EabFileHandling EabFileHandling
    {
      get => eabFileHandling ??= new();
      set => eabFileHandling = value;
    }

    /// <summary>
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    [Member(Index = 2, AccessFields = false, Members = new[] { "Number" })]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
    }

    /// <summary>
    /// A value of StmtNumber.
    /// </summary>
    [JsonPropertyName("stmtNumber")]
    [Member(Index = 3, AccessFields = false, Members = new[] { "Count" })]
    public Common StmtNumber
    {
      get => stmtNumber ??= new();
      set => stmtNumber = value;
    }

    /// <summary>
    /// A value of RecordNumber.
    /// </summary>
    [JsonPropertyName("recordNumber")]
    [Member(Index = 4, AccessFields = false, Members = new[] { "Count" })]
    public Common RecordNumber
    {
      get => recordNumber ??= new();
      set => recordNumber = value;
    }

    /// <summary>
    /// A value of RecordType.
    /// </summary>
    [JsonPropertyName("recordType")]
    [Member(Index = 5, AccessFields = false, Members = new[] { "ActionEntry" })]
    public Common RecordType
    {
      get => recordType ??= new();
      set => recordType = value;
    }

    /// <summary>
    /// A value of CsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("csePersonsWorkSet")]
    [Member(Index = 6, AccessFields = false, Members = new[]
    {
      "FirstName",
      "MiddleInitial",
      "LastName"
    })]
    public CsePersonsWorkSet CsePersonsWorkSet
    {
      get => csePersonsWorkSet ??= new();
      set => csePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of CaseWorker.
    /// </summary>
    [JsonPropertyName("caseWorker")]
    [Member(Index = 7, AccessFields = false, Members = new[]
    {
      "MainPhoneAreaCode",
      "Name",
      "MainPhoneNumber"
    })]
    public Office CaseWorker
    {
      get => caseWorker ??= new();
      set => caseWorker = value;
    }

    /// <summary>
    /// A value of CsePersonAddress.
    /// </summary>
    [JsonPropertyName("csePersonAddress")]
    [Member(Index = 8, AccessFields = false, Members = new[]
    {
      "LocationType",
      "Street1",
      "Street2",
      "City",
      "State",
      "ZipCode",
      "Zip4",
      "Zip3",
      "Street3",
      "Street4",
      "Province",
      "PostalCode",
      "Country"
    })]
    public CsePersonAddress CsePersonAddress
    {
      get => csePersonAddress ??= new();
      set => csePersonAddress = value;
    }

    /// <summary>
    /// A value of VariableLine1.
    /// </summary>
    [JsonPropertyName("variableLine1")]
    [Member(Index = 9, AccessFields = false, Members = new[] { "RptDetail" })]
    public EabReportSend VariableLine1
    {
      get => variableLine1 ??= new();
      set => variableLine1 = value;
    }

    /// <summary>
    /// A value of VariableLine2.
    /// </summary>
    [JsonPropertyName("variableLine2")]
    [Member(Index = 10, AccessFields = false, Members = new[] { "RptDetail" })]
    public EabReportSend VariableLine2
    {
      get => variableLine2 ??= new();
      set => variableLine2 = value;
    }

    /// <summary>
    /// A value of GlobalStatementMessage.
    /// </summary>
    [JsonPropertyName("globalStatementMessage")]
    [Member(Index = 11, AccessFields = false, Members = new[] { "TextArea" })]
    public GlobalStatementMessage GlobalStatementMessage
    {
      get => globalStatementMessage ??= new();
      set => globalStatementMessage = value;
    }

    /// <summary>
    /// A value of AmountOne.
    /// </summary>
    [JsonPropertyName("amountOne")]
    [Member(Index = 12, AccessFields = false, Members
      = new[] { "AverageCurrency" })]
    public Common AmountOne
    {
      get => amountOne ??= new();
      set => amountOne = value;
    }

    /// <summary>
    /// A value of AmountTwo.
    /// </summary>
    [JsonPropertyName("amountTwo")]
    [Member(Index = 13, AccessFields = false, Members
      = new[] { "AverageCurrency" })]
    public Common AmountTwo
    {
      get => amountTwo ??= new();
      set => amountTwo = value;
    }

    /// <summary>
    /// A value of DateOne.
    /// </summary>
    [JsonPropertyName("dateOne")]
    [Member(Index = 14, AccessFields = false, Members = new[] { "Text10" })]
    public TextWorkArea DateOne
    {
      get => dateOne ??= new();
      set => dateOne = value;
    }

    /// <summary>
    /// A value of DateTwo.
    /// </summary>
    [JsonPropertyName("dateTwo")]
    [Member(Index = 15, AccessFields = false, Members = new[] { "Text10" })]
    public TextWorkArea DateTwo
    {
      get => dateTwo ??= new();
      set => dateTwo = value;
    }

    /// <summary>
    /// A value of SortSequenceNumber.
    /// </summary>
    [JsonPropertyName("sortSequenceNumber")]
    [Member(Index = 16, AccessFields = false, Members = new[] { "Count" })]
    public Common SortSequenceNumber
    {
      get => sortSequenceNumber ??= new();
      set => sortSequenceNumber = value;
    }

    private EabFileHandling eabFileHandling;
    private CsePerson csePerson;
    private Common stmtNumber;
    private Common recordNumber;
    private Common recordType;
    private CsePersonsWorkSet csePersonsWorkSet;
    private Office caseWorker;
    private CsePersonAddress csePersonAddress;
    private EabReportSend variableLine1;
    private EabReportSend variableLine2;
    private GlobalStatementMessage globalStatementMessage;
    private Common amountOne;
    private Common amountTwo;
    private TextWorkArea dateOne;
    private TextWorkArea dateTwo;
    private Common sortSequenceNumber;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of EabFileHandling.
    /// </summary>
    [JsonPropertyName("eabFileHandling")]
    [Member(Index = 1, AccessFields = false, Members = new[] { "Status" })]
    public EabFileHandling EabFileHandling
    {
      get => eabFileHandling ??= new();
      set => eabFileHandling = value;
    }

    private EabFileHandling eabFileHandling;
  }
#endregion
}
