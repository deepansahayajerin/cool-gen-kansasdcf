// Program: FN_B792_EAB_WRITE_IVR_FILE, ID: 1902416812, model: 746.
// Short name: SWEXEW19
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_B792_EAB_WRITE_IVR_FILE.
/// </summary>
[Serializable]
public partial class FnB792EabWriteIvrFile: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_B792_EAB_WRITE_IVR_FILE program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnB792EabWriteIvrFile(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnB792EabWriteIvrFile.
  /// </summary>
  public FnB792EabWriteIvrFile(IContext context, Import import, Export export):
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
      "SWEXEW19", context, import, export, EabOptions.Hpvp);
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
    /// A value of ArCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("arCsePersonsWorkSet")]
    [Member(Index = 2, AccessFields = false, Members = new[]
    {
      "LastName",
      "FirstName",
      "MiddleInitial",
      "Dob",
      "Ssn"
    })]
    public CsePersonsWorkSet ArCsePersonsWorkSet
    {
      get => arCsePersonsWorkSet ??= new();
      set => arCsePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of ArCsePerson.
    /// </summary>
    [JsonPropertyName("arCsePerson")]
    [Member(Index = 3, AccessFields = false, Members = new[] { "Number" })]
    public CsePerson ArCsePerson
    {
      get => arCsePerson ??= new();
      set => arCsePerson = value;
    }

    /// <summary>
    /// A value of ApCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("apCsePersonsWorkSet")]
    [Member(Index = 4, AccessFields = false, Members = new[]
    {
      "LastName",
      "FirstName",
      "MiddleInitial"
    })]
    public CsePersonsWorkSet ApCsePersonsWorkSet
    {
      get => apCsePersonsWorkSet ??= new();
      set => apCsePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of LegalAction.
    /// </summary>
    [JsonPropertyName("legalAction")]
    [Member(Index = 5, AccessFields = false, Members
      = new[] { "StandardNumber" })]
    public LegalAction LegalAction
    {
      get => legalAction ??= new();
      set => legalAction = value;
    }

    /// <summary>
    /// A value of ApCsePerson.
    /// </summary>
    [JsonPropertyName("apCsePerson")]
    [Member(Index = 6, AccessFields = false, Members = new[] { "Number" })]
    public CsePerson ApCsePerson
    {
      get => apCsePerson ??= new();
      set => apCsePerson = value;
    }

    /// <summary>
    /// A value of Collection.
    /// </summary>
    [JsonPropertyName("collection")]
    [Member(Index = 7, AccessFields = false, Members
      = new[] { "CollectionDt", "Amount" })]
    public Collection Collection
    {
      get => collection ??= new();
      set => collection = value;
    }

    /// <summary>
    /// A value of AppliedAsCurrent.
    /// </summary>
    [JsonPropertyName("appliedAsCurrent")]
    [Member(Index = 8, AccessFields = false, Members = new[] { "Amount" })]
    public Collection AppliedAsCurrent
    {
      get => appliedAsCurrent ??= new();
      set => appliedAsCurrent = value;
    }

    /// <summary>
    /// A value of AppliedAsArrears.
    /// </summary>
    [JsonPropertyName("appliedAsArrears")]
    [Member(Index = 9, AccessFields = false, Members = new[] { "Amount" })]
    public Collection AppliedAsArrears
    {
      get => appliedAsArrears ??= new();
      set => appliedAsArrears = value;
    }

    /// <summary>
    /// A value of TotalForwardedToFamily.
    /// </summary>
    [JsonPropertyName("totalForwardedToFamily")]
    [Member(Index = 10, AccessFields = false, Members = new[] { "Amount" })]
    public Collection TotalForwardedToFamily
    {
      get => totalForwardedToFamily ??= new();
      set => totalForwardedToFamily = value;
    }

    /// <summary>
    /// A value of RecordCount.
    /// </summary>
    [JsonPropertyName("recordCount")]
    [Member(Index = 11, AccessFields = false, Members = new[] { "Text10" })]
    public WorkArea RecordCount
    {
      get => recordCount ??= new();
      set => recordCount = value;
    }

    /// <summary>
    /// A value of CreateDate.
    /// </summary>
    [JsonPropertyName("createDate")]
    [Member(Index = 12, AccessFields = false, Members = new[] { "Text8" })]
    public WorkArea CreateDate
    {
      get => createDate ??= new();
      set => createDate = value;
    }

    /// <summary>
    /// A value of RecordType.
    /// </summary>
    [JsonPropertyName("recordType")]
    [Member(Index = 13, AccessFields = false, Members = new[] { "Text1" })]
    public WorkArea RecordType
    {
      get => recordType ??= new();
      set => recordType = value;
    }

    private EabFileHandling eabFileHandling;
    private CsePersonsWorkSet arCsePersonsWorkSet;
    private CsePerson arCsePerson;
    private CsePersonsWorkSet apCsePersonsWorkSet;
    private LegalAction legalAction;
    private CsePerson apCsePerson;
    private Collection collection;
    private Collection appliedAsCurrent;
    private Collection appliedAsArrears;
    private Collection totalForwardedToFamily;
    private WorkArea recordCount;
    private WorkArea createDate;
    private WorkArea recordType;
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

    /// <summary>
    /// A value of External.
    /// </summary>
    [JsonPropertyName("external")]
    [Member(Index = 2, AccessFields = false, Members = new[]
    {
      "NumericReturnCode",
      "TextReturnCode",
      "TextLine80"
    })]
    public External External
    {
      get => external ??= new();
      set => external = value;
    }

    private EabFileHandling eabFileHandling;
    private External external;
  }
#endregion
}
