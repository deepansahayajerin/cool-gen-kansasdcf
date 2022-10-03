// Program: LE_B591_READ_FILE, ID: 1902621198, model: 746.
// Short name: SWEXER28
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: LE_B591_READ_FILE.
/// </summary>
[Serializable]
public partial class LeB591ReadFile: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the LE_B591_READ_FILE program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new LeB591ReadFile(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of LeB591ReadFile.
  /// </summary>
  public LeB591ReadFile(IContext context, Import import, Export export):
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
      "SWEXER28", context, import, export, EabOptions.Hpvp);
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
    /// A value of Delimeter.
    /// </summary>
    [JsonPropertyName("delimeter")]
    [Member(Index = 2, AccessFields = false, Members = new[] { "Text1" })]
    public WorkArea Delimeter
    {
      get => delimeter ??= new();
      set => delimeter = value;
    }

    private EabFileHandling eabFileHandling;
    private WorkArea delimeter;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of CsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("csePersonsWorkSet")]
    [Member(Index = 1, AccessFields = false, Members = new[]
    {
      "Ssn",
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
    /// A value of Pic.
    /// </summary>
    [JsonPropertyName("pic")]
    [Member(Index = 2, AccessFields = false, Members = new[] { "Text3" })]
    public WorkArea Pic
    {
      get => pic ??= new();
      set => pic = value;
    }

    /// <summary>
    /// A value of CtCaseNumber.
    /// </summary>
    [JsonPropertyName("ctCaseNumber")]
    [Member(Index = 3, AccessFields = false, Members = new[] { "Text60" })]
    public WorkArea CtCaseNumber
    {
      get => ctCaseNumber ??= new();
      set => ctCaseNumber = value;
    }

    /// <summary>
    /// A value of CorrectedCaseNumber.
    /// </summary>
    [JsonPropertyName("correctedCaseNumber")]
    [Member(Index = 4, AccessFields = false, Members = new[] { "Text60" })]
    public WorkArea CorrectedCaseNumber
    {
      get => correctedCaseNumber ??= new();
      set => correctedCaseNumber = value;
    }

    /// <summary>
    /// A value of Suffix.
    /// </summary>
    [JsonPropertyName("suffix")]
    [Member(Index = 5, AccessFields = false, Members = new[] { "Text6" })]
    public WorkArea Suffix
    {
      get => suffix ??= new();
      set => suffix = value;
    }

    /// <summary>
    /// A value of OrderStatus.
    /// </summary>
    [JsonPropertyName("orderStatus")]
    [Member(Index = 6, AccessFields = false, Members = new[] { "Text5" })]
    public WorkArea OrderStatus
    {
      get => orderStatus ??= new();
      set => orderStatus = value;
    }

    /// <summary>
    /// A value of Fips.
    /// </summary>
    [JsonPropertyName("fips")]
    [Member(Index = 7, AccessFields = false, Members = new[] { "Text1" })]
    public WorkArea Fips
    {
      get => fips ??= new();
      set => fips = value;
    }

    /// <summary>
    /// A value of Legend1.
    /// </summary>
    [JsonPropertyName("legend1")]
    [Member(Index = 8, AccessFields = false, Members = new[] { "Text33" })]
    public WorkArea Legend1
    {
      get => legend1 ??= new();
      set => legend1 = value;
    }

    /// <summary>
    /// A value of Legend2.
    /// </summary>
    [JsonPropertyName("legend2")]
    [Member(Index = 9, AccessFields = false, Members = new[] { "Text33" })]
    public WorkArea Legend2
    {
      get => legend2 ??= new();
      set => legend2 = value;
    }

    /// <summary>
    /// A value of EmployerAddress.
    /// </summary>
    [JsonPropertyName("employerAddress")]
    [Member(Index = 10, AccessFields = false, Members = new[]
    {
      "Street1",
      "City",
      "LocationType",
      "Street2",
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
    [Member(Index = 11, AccessFields = false, Members = new[] { "Text1" })]
    public WorkArea Convert
    {
      get => convert ??= new();
      set => convert = value;
    }

    /// <summary>
    /// A value of MiddleName.
    /// </summary>
    [JsonPropertyName("middleName")]
    [Member(Index = 12, AccessFields = false, Members = new[] { "Text12" })]
    public WorkArea MiddleName
    {
      get => middleName ??= new();
      set => middleName = value;
    }

    /// <summary>
    /// A value of AdditionalComments.
    /// </summary>
    [JsonPropertyName("additionalComments")]
    [Member(Index = 13, AccessFields = false, Members = new[] { "Text12" })]
    public WorkArea AdditionalComments
    {
      get => additionalComments ??= new();
      set => additionalComments = value;
    }

    /// <summary>
    /// A value of External.
    /// </summary>
    [JsonPropertyName("external")]
    [Member(Index = 14, AccessFields = false, Members = new[]
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

    private CsePersonsWorkSet csePersonsWorkSet;
    private WorkArea pic;
    private WorkArea ctCaseNumber;
    private WorkArea correctedCaseNumber;
    private WorkArea suffix;
    private WorkArea orderStatus;
    private WorkArea fips;
    private WorkArea legend1;
    private WorkArea legend2;
    private EmployerAddress employerAddress;
    private WorkArea convert;
    private WorkArea middleName;
    private WorkArea additionalComments;
    private External external;
  }
#endregion
}
