// Program: FN_B681_PROCESS_AR_EXTRACT_DATA, ID: 374558566, model: 746.
// Short name: SWEXER18
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_B681_PROCESS_AR_EXTRACT_DATA.
/// </summary>
[Serializable]
public partial class FnB681ProcessArExtractData: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_B681_PROCESS_AR_EXTRACT_DATA program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnB681ProcessArExtractData(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnB681ProcessArExtractData.
  /// </summary>
  public FnB681ProcessArExtractData(IContext context, Import import,
    Export export):
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
      "SWEXER18", context, import, export, EabOptions.Hpvp);
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

    private EabFileHandling eabFileHandling;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of ArCsePerson.
    /// </summary>
    [JsonPropertyName("arCsePerson")]
    [Member(Index = 1, AccessFields = false, Members = new[] { "Number" })]
    public CsePerson ArCsePerson
    {
      get => arCsePerson ??= new();
      set => arCsePerson = value;
    }

    /// <summary>
    /// A value of ObligorCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("obligorCsePersonsWorkSet")]
    [Member(Index = 2, AccessFields = false, Members = new[] { "LastName" })]
    public CsePersonsWorkSet ObligorCsePersonsWorkSet
    {
      get => obligorCsePersonsWorkSet ??= new();
      set => obligorCsePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of ObligorCsePerson.
    /// </summary>
    [JsonPropertyName("obligorCsePerson")]
    [Member(Index = 3, AccessFields = false, Members = new[] { "Number" })]
    public CsePerson ObligorCsePerson
    {
      get => obligorCsePerson ??= new();
      set => obligorCsePerson = value;
    }

    /// <summary>
    /// A value of Collection.
    /// </summary>
    [JsonPropertyName("collection")]
    [Member(Index = 4, AccessFields = false, Members = new[]
    {
      "SystemGeneratedIdentifier",
      "ProgramAppliedTo",
      "CourtOrderAppliedTo",
      "CollectionDt",
      "AppliedToCode",
      "Amount"
    })]
    public Collection Collection
    {
      get => collection ??= new();
      set => collection = value;
    }

    /// <summary>
    /// A value of Retained.
    /// </summary>
    [JsonPropertyName("retained")]
    [Member(Index = 5, AccessFields = false, Members = new[] { "Amount" })]
    public Collection Retained
    {
      get => retained ??= new();
      set => retained = value;
    }

    /// <summary>
    /// A value of ForwardToFamily.
    /// </summary>
    [JsonPropertyName("forwardToFamily")]
    [Member(Index = 6, AccessFields = false, Members = new[] { "Amount" })]
    public Collection ForwardToFamily
    {
      get => forwardToFamily ??= new();
      set => forwardToFamily = value;
    }

    /// <summary>
    /// A value of Obligor.
    /// </summary>
    [JsonPropertyName("obligor")]
    [Member(Index = 7, AccessFields = false, Members = new[] { "Type1" })]
    public CsePersonAccount Obligor
    {
      get => obligor ??= new();
      set => obligor = value;
    }

    /// <summary>
    /// A value of ObligationType.
    /// </summary>
    [JsonPropertyName("obligationType")]
    [Member(Index = 8, AccessFields = false, Members
      = new[] { "SystemGeneratedIdentifier" })]
    public ObligationType ObligationType
    {
      get => obligationType ??= new();
      set => obligationType = value;
    }

    /// <summary>
    /// A value of Obligation.
    /// </summary>
    [JsonPropertyName("obligation")]
    [Member(Index = 9, AccessFields = false, Members
      = new[] { "SystemGeneratedIdentifier" })]
    public Obligation Obligation
    {
      get => obligation ??= new();
      set => obligation = value;
    }

    /// <summary>
    /// A value of ObligationTransaction.
    /// </summary>
    [JsonPropertyName("obligationTransaction")]
    [Member(Index = 10, AccessFields = false, Members
      = new[] { "SystemGeneratedIdentifier", "Type1" })]
    public ObligationTransaction ObligationTransaction
    {
      get => obligationTransaction ??= new();
      set => obligationTransaction = value;
    }

    /// <summary>
    /// A value of CashReceiptSourceType.
    /// </summary>
    [JsonPropertyName("cashReceiptSourceType")]
    [Member(Index = 11, AccessFields = false, Members
      = new[] { "SystemGeneratedIdentifier" })]
    public CashReceiptSourceType CashReceiptSourceType
    {
      get => cashReceiptSourceType ??= new();
      set => cashReceiptSourceType = value;
    }

    /// <summary>
    /// A value of CashReceiptEvent.
    /// </summary>
    [JsonPropertyName("cashReceiptEvent")]
    [Member(Index = 12, AccessFields = false, Members
      = new[] { "SystemGeneratedIdentifier" })]
    public CashReceiptEvent CashReceiptEvent
    {
      get => cashReceiptEvent ??= new();
      set => cashReceiptEvent = value;
    }

    /// <summary>
    /// A value of CashReceiptType.
    /// </summary>
    [JsonPropertyName("cashReceiptType")]
    [Member(Index = 13, AccessFields = false, Members
      = new[] { "SystemGeneratedIdentifier" })]
    public CashReceiptType CashReceiptType
    {
      get => cashReceiptType ??= new();
      set => cashReceiptType = value;
    }

    /// <summary>
    /// A value of CashReceiptDetail.
    /// </summary>
    [JsonPropertyName("cashReceiptDetail")]
    [Member(Index = 14, AccessFields = false, Members
      = new[] { "SequentialIdentifier" })]
    public CashReceiptDetail CashReceiptDetail
    {
      get => cashReceiptDetail ??= new();
      set => cashReceiptDetail = value;
    }

    /// <summary>
    /// A value of ChPerson.
    /// </summary>
    [JsonPropertyName("chPerson")]
    [Member(Index = 15, AccessFields = false, Members = new[]
    {
      "Number",
      "FirstName",
      "MiddleInitial",
      "LastName"
    })]
    public CsePersonsWorkSet ChPerson
    {
      get => chPerson ??= new();
      set => chPerson = value;
    }

    /// <summary>
    /// A value of ArCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("arCsePersonsWorkSet")]
    [Member(Index = 16, AccessFields = false, Members = new[]
    {
      "Number",
      "FirstName",
      "MiddleInitial",
      "LastName"
    })]
    public CsePersonsWorkSet ArCsePersonsWorkSet
    {
      get => arCsePersonsWorkSet ??= new();
      set => arCsePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of External.
    /// </summary>
    [JsonPropertyName("external")]
    [Member(Index = 17, AccessFields = false, Members = new[]
    {
      "NumericReturnCode",
      "TextReturnCode",
      "TextLine80",
      "FileInstruction"
    })]
    public External External
    {
      get => external ??= new();
      set => external = value;
    }

    /// <summary>
    /// A value of Taf.
    /// </summary>
    [JsonPropertyName("taf")]
    [Member(Index = 18, AccessFields = false, Members
      = new[] { "SystemGeneratedIdentifier" })]
    public ObligationType Taf
    {
      get => taf ??= new();
      set => taf = value;
    }

    private CsePerson arCsePerson;
    private CsePersonsWorkSet obligorCsePersonsWorkSet;
    private CsePerson obligorCsePerson;
    private Collection collection;
    private Collection retained;
    private Collection forwardToFamily;
    private CsePersonAccount obligor;
    private ObligationType obligationType;
    private Obligation obligation;
    private ObligationTransaction obligationTransaction;
    private CashReceiptSourceType cashReceiptSourceType;
    private CashReceiptEvent cashReceiptEvent;
    private CashReceiptType cashReceiptType;
    private CashReceiptDetail cashReceiptDetail;
    private CsePersonsWorkSet chPerson;
    private CsePersonsWorkSet arCsePersonsWorkSet;
    private External external;
    private ObligationType taf;
  }
#endregion
}
