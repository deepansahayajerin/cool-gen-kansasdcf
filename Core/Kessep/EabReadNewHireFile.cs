// Program: EAB_READ_NEW_HIRE_FILE, ID: 372746190, model: 746.
// Short name: SWEXIC02
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: EAB_READ_NEW_HIRE_FILE.
/// </summary>
[Serializable]
public partial class EabReadNewHireFile: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the EAB_READ_NEW_HIRE_FILE program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new EabReadNewHireFile(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of EabReadNewHireFile.
  /// </summary>
  public EabReadNewHireFile(IContext context, Import import, Export export):
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
      "SWEXIC02", context, import, export, EabOptions.Hpvp);
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
    /// A value of Office.
    /// </summary>
    [JsonPropertyName("office")]
    [Member(Index = 1, AccessFields = false, Members
      = new[] { "SystemGeneratedId" })]
    public Office Office
    {
      get => office ??= new();
      set => office = value;
    }

    /// <summary>
    /// A value of Case1.
    /// </summary>
    [JsonPropertyName("case1")]
    [Member(Index = 2, AccessFields = false, Members = new[] { "Number" })]
    public Case1 Case1
    {
      get => case1 ??= new();
      set => case1 = value;
    }

    /// <summary>
    /// A value of CsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("csePersonsWorkSet")]
    [Member(Index = 3, AccessFields = false, Members = new[]
    {
      "Number",
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
    /// A value of ObligorStreetAddr.
    /// </summary>
    [JsonPropertyName("obligorStreetAddr")]
    [Member(Index = 4, AccessFields = false, Members = new[] { "TextLine80" })]
    public External ObligorStreetAddr
    {
      get => obligorStreetAddr ??= new();
      set => obligorStreetAddr = value;
    }

    /// <summary>
    /// A value of ObligorCity.
    /// </summary>
    [JsonPropertyName("obligorCity")]
    [Member(Index = 5, AccessFields = false, Members = new[] { "Text30" })]
    public TextWorkArea ObligorCity
    {
      get => obligorCity ??= new();
      set => obligorCity = value;
    }

    /// <summary>
    /// A value of ObligorAddress.
    /// </summary>
    [JsonPropertyName("obligorAddress")]
    [Member(Index = 6, AccessFields = false, Members = new[]
    {
      "LocationType",
      "State",
      "ZipCode",
      "Zip4"
    })]
    public CsePersonAddress ObligorAddress
    {
      get => obligorAddress ??= new();
      set => obligorAddress = value;
    }

    /// <summary>
    /// A value of Business.
    /// </summary>
    [JsonPropertyName("business")]
    [Member(Index = 7, AccessFields = false, Members
      = new[] { "LocationType", "State" })]
    public CsePersonAddress Business
    {
      get => business ??= new();
      set => business = value;
    }

    /// <summary>
    /// A value of Start.
    /// </summary>
    [JsonPropertyName("start")]
    [Member(Index = 8, AccessFields = false, Members = new[] { "Date" })]
    public DateWorkArea Start
    {
      get => start ??= new();
      set => start = value;
    }

    /// <summary>
    /// A value of CompanyName.
    /// </summary>
    [JsonPropertyName("companyName")]
    [Member(Index = 9, AccessFields = false, Members = new[] { "TextLine80" })]
    public External CompanyName
    {
      get => companyName ??= new();
      set => companyName = value;
    }

    /// <summary>
    /// A value of CompanyStreetAddr.
    /// </summary>
    [JsonPropertyName("companyStreetAddr")]
    [Member(Index = 10, AccessFields = false, Members = new[] { "TextLine80" })]
    public External CompanyStreetAddr
    {
      get => companyStreetAddr ??= new();
      set => companyStreetAddr = value;
    }

    /// <summary>
    /// A value of CompanyCity.
    /// </summary>
    [JsonPropertyName("companyCity")]
    [Member(Index = 11, AccessFields = false, Members = new[] { "Text30" })]
    public TextWorkArea CompanyCity
    {
      get => companyCity ??= new();
      set => companyCity = value;
    }

    /// <summary>
    /// A value of CompanyAddress.
    /// </summary>
    [JsonPropertyName("companyAddress")]
    [Member(Index = 12, AccessFields = false, Members = new[]
    {
      "LocationType",
      "State",
      "ZipCode",
      "Zip4"
    })]
    public CsePersonAddress CompanyAddress
    {
      get => companyAddress ??= new();
      set => companyAddress = value;
    }

    /// <summary>
    /// A value of StateHiredIn.
    /// </summary>
    [JsonPropertyName("stateHiredIn")]
    [Member(Index = 13, AccessFields = false, Members
      = new[] { "LocationType", "State" })]
    public CsePersonAddress StateHiredIn
    {
      get => stateHiredIn ??= new();
      set => stateHiredIn = value;
    }

    /// <summary>
    /// A value of Employer.
    /// </summary>
    [JsonPropertyName("employer")]
    [Member(Index = 14, AccessFields = false, Members
      = new[] { "Ein", "KansasId" })]
    public Employer Employer
    {
      get => employer ??= new();
      set => employer = value;
    }

    /// <summary>
    /// A value of EabFileHandling.
    /// </summary>
    [JsonPropertyName("eabFileHandling")]
    [Member(Index = 15, AccessFields = false, Members = new[] { "Status" })]
    public EabFileHandling EabFileHandling
    {
      get => eabFileHandling ??= new();
      set => eabFileHandling = value;
    }

    private Office office;
    private Case1 case1;
    private CsePersonsWorkSet csePersonsWorkSet;
    private External obligorStreetAddr;
    private TextWorkArea obligorCity;
    private CsePersonAddress obligorAddress;
    private CsePersonAddress business;
    private DateWorkArea start;
    private External companyName;
    private External companyStreetAddr;
    private TextWorkArea companyCity;
    private CsePersonAddress companyAddress;
    private CsePersonAddress stateHiredIn;
    private Employer employer;
    private EabFileHandling eabFileHandling;
  }
#endregion
}
