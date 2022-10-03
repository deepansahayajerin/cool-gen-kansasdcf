// Program: LE_B591_WRITE_FILE, ID: 1902621242, model: 746.
// Short name: SWEXEW32
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: LE_B591_WRITE_FILE.
/// </summary>
[Serializable]
public partial class LeB591WriteFile: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the LE_B591_WRITE_FILE program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new LeB591WriteFile(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of LeB591WriteFile.
  /// </summary>
  public LeB591WriteFile(IContext context, Import import, Export export):
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
      "SWEXEW32", context, import, export, EabOptions.Hpvp);
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
    /// A value of Export1.
    /// </summary>
    [JsonPropertyName("export1")]
    [Member(Index = 2, AccessFields = false, Members = new[]
    {
      "Ssn",
      "FirstName",
      "MiddleInitial",
      "LastName"
    })]
    public CsePersonsWorkSet Export1
    {
      get => export1 ??= new();
      set => export1 = value;
    }

    /// <summary>
    /// A value of Pic.
    /// </summary>
    [JsonPropertyName("pic")]
    [Member(Index = 3, AccessFields = false, Members = new[] { "Text3" })]
    public WorkArea Pic
    {
      get => pic ??= new();
      set => pic = value;
    }

    /// <summary>
    /// A value of CtCaseNumber.
    /// </summary>
    [JsonPropertyName("ctCaseNumber")]
    [Member(Index = 4, AccessFields = false, Members = new[] { "Text60" })]
    public WorkArea CtCaseNumber
    {
      get => ctCaseNumber ??= new();
      set => ctCaseNumber = value;
    }

    /// <summary>
    /// A value of CorrectedCtCaseNum.
    /// </summary>
    [JsonPropertyName("correctedCtCaseNum")]
    [Member(Index = 5, AccessFields = false, Members = new[] { "Text60" })]
    public WorkArea CorrectedCtCaseNum
    {
      get => correctedCtCaseNum ??= new();
      set => correctedCtCaseNum = value;
    }

    /// <summary>
    /// A value of Suffix.
    /// </summary>
    [JsonPropertyName("suffix")]
    [Member(Index = 6, AccessFields = false, Members = new[] { "Text6" })]
    public WorkArea Suffix
    {
      get => suffix ??= new();
      set => suffix = value;
    }

    /// <summary>
    /// A value of OrderStatus.
    /// </summary>
    [JsonPropertyName("orderStatus")]
    [Member(Index = 7, AccessFields = false, Members = new[] { "Text5" })]
    public WorkArea OrderStatus
    {
      get => orderStatus ??= new();
      set => orderStatus = value;
    }

    /// <summary>
    /// A value of Fips.
    /// </summary>
    [JsonPropertyName("fips")]
    [Member(Index = 8, AccessFields = false, Members = new[] { "Text1" })]
    public WorkArea Fips
    {
      get => fips ??= new();
      set => fips = value;
    }

    /// <summary>
    /// A value of Legends1.
    /// </summary>
    [JsonPropertyName("legends1")]
    [Member(Index = 9, AccessFields = false, Members = new[] { "Text33" })]
    public WorkArea Legends1
    {
      get => legends1 ??= new();
      set => legends1 = value;
    }

    /// <summary>
    /// A value of Legends2.
    /// </summary>
    [JsonPropertyName("legends2")]
    [Member(Index = 10, AccessFields = false, Members = new[] { "Text33" })]
    public WorkArea Legends2
    {
      get => legends2 ??= new();
      set => legends2 = value;
    }

    /// <summary>
    /// A value of EmployerAddress.
    /// </summary>
    [JsonPropertyName("employerAddress")]
    [Member(Index = 11, AccessFields = false, Members = new[]
    {
      "LocationType",
      "Street1",
      "Street2",
      "City",
      "Street3",
      "State",
      "ZipCode"
    })]
    public EmployerAddress EmployerAddress
    {
      get => employerAddress ??= new();
      set => employerAddress = value;
    }

    /// <summary>
    /// A value of Convert.
    /// </summary>
    [JsonPropertyName("convert")]
    [Member(Index = 12, AccessFields = false, Members = new[] { "Text1" })]
    public WorkArea Convert
    {
      get => convert ??= new();
      set => convert = value;
    }

    /// <summary>
    /// A value of MiddleName.
    /// </summary>
    [JsonPropertyName("middleName")]
    [Member(Index = 13, AccessFields = false, Members = new[] { "Text12" })]
    public WorkArea MiddleName
    {
      get => middleName ??= new();
      set => middleName = value;
    }

    /// <summary>
    /// A value of Remarks.
    /// </summary>
    [JsonPropertyName("remarks")]
    [Member(Index = 14, AccessFields = false, Members = new[] { "Text12" })]
    public WorkArea Remarks
    {
      get => remarks ??= new();
      set => remarks = value;
    }

    /// <summary>
    /// A value of MultiCtOrderNumbers.
    /// </summary>
    [JsonPropertyName("multiCtOrderNumbers")]
    [Member(Index = 15, AccessFields = false, Members = new[] { "Text200" })]
    public WorkArea MultiCtOrderNumbers
    {
      get => multiCtOrderNumbers ??= new();
      set => multiCtOrderNumbers = value;
    }

    /// <summary>
    /// A value of MultiCtOrderNumber2.
    /// </summary>
    [JsonPropertyName("multiCtOrderNumber2")]
    [Member(Index = 16, AccessFields = false, Members = new[] { "Text200" })]
    public WorkArea MultiCtOrderNumber2
    {
      get => multiCtOrderNumber2 ??= new();
      set => multiCtOrderNumber2 = value;
    }

    private EabFileHandling eabFileHandling;
    private CsePersonsWorkSet export1;
    private WorkArea pic;
    private WorkArea ctCaseNumber;
    private WorkArea correctedCtCaseNum;
    private WorkArea suffix;
    private WorkArea orderStatus;
    private WorkArea fips;
    private WorkArea legends1;
    private WorkArea legends2;
    private EmployerAddress employerAddress;
    private WorkArea convert;
    private WorkArea middleName;
    private WorkArea remarks;
    private WorkArea multiCtOrderNumbers;
    private WorkArea multiCtOrderNumber2;
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
    [Member(Index = 1, AccessFields = false, Members = new[]
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

    private External external;
  }
#endregion
}
