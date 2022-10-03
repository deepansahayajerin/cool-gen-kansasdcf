// Program: LE_EAB_READ_IWO_45_DAY_INFO, ID: 373500341, model: 746.
// Short name: SWEXLE06
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: LE_EAB_READ_IWO_45_DAY_INFO.
/// </summary>
[Serializable]
public partial class LeEabReadIwo45DayInfo: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the LE_EAB_READ_IWO_45_DAY_INFO program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new LeEabReadIwo45DayInfo(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of LeEabReadIwo45DayInfo.
  /// </summary>
  public LeEabReadIwo45DayInfo(IContext context, Import import, Export export):
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
      "SWEXLE06", context, import, export, EabOptions.Hpvp);
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
    /// A value of ReportingPeriodStart.
    /// </summary>
    [JsonPropertyName("reportingPeriodStart")]
    [Member(Index = 1, AccessFields = false, Members
      = new[] { "TextDate", "Date" })]
    public DateWorkArea ReportingPeriodStart
    {
      get => reportingPeriodStart ??= new();
      set => reportingPeriodStart = value;
    }

    /// <summary>
    /// A value of IwglCreatedDate.
    /// </summary>
    [JsonPropertyName("iwglCreatedDate")]
    [Member(Index = 2, AccessFields = false, Members
      = new[] { "TextDate", "Date" })]
    public DateWorkArea IwglCreatedDate
    {
      get => iwglCreatedDate ??= new();
      set => iwglCreatedDate = value;
    }

    /// <summary>
    /// A value of CsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("csePersonsWorkSet")]
    [Member(Index = 3, AccessFields = false, Members = new[] { "FormattedName" })
      ]
    public CsePersonsWorkSet CsePersonsWorkSet
    {
      get => csePersonsWorkSet ??= new();
      set => csePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of EmployerAddress.
    /// </summary>
    [JsonPropertyName("employerAddress")]
    [Member(Index = 4, Members = new[]
    {
      "LocationType",
      "City",
      "State"
    })]
    public EmployerAddress EmployerAddress
    {
      get => employerAddress ??= new();
      set => employerAddress = value;
    }

    /// <summary>
    /// A value of Employer.
    /// </summary>
    [JsonPropertyName("employer")]
    [Member(Index = 5, Members = new[] { "Name" })]
    public Employer Employer
    {
      get => employer ??= new();
      set => employer = value;
    }

    /// <summary>
    /// A value of LegalAction.
    /// </summary>
    [JsonPropertyName("legalAction")]
    [Member(Index = 6, Members = new[] { "StandardNumber" })]
    public LegalAction LegalAction
    {
      get => legalAction ??= new();
      set => legalAction = value;
    }

    /// <summary>
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    [Member(Index = 7, Members = new[] { "Number" })]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
    }

    /// <summary>
    /// A value of ServiceProvider.
    /// </summary>
    [JsonPropertyName("serviceProvider")]
    [Member(Index = 8, Members = new[]
    {
      "LastName",
      "FirstName",
      "MiddleInitial"
    })]
    public ServiceProvider ServiceProvider
    {
      get => serviceProvider ??= new();
      set => serviceProvider = value;
    }

    /// <summary>
    /// A value of Office.
    /// </summary>
    [JsonPropertyName("office")]
    [Member(Index = 9, AccessFields = false, Members = new[] { "Name" })]
    public Office Office
    {
      get => office ??= new();
      set => office = value;
    }

    /// <summary>
    /// A value of EabFileHandling.
    /// </summary>
    [JsonPropertyName("eabFileHandling")]
    [Member(Index = 10, AccessFields = false, Members
      = new[] { "Action", "Status" })]
    public EabFileHandling EabFileHandling
    {
      get => eabFileHandling ??= new();
      set => eabFileHandling = value;
    }

    /// <summary>
    /// A value of Area.
    /// </summary>
    [JsonPropertyName("area")]
    [Member(Index = 11, AccessFields = false, Members
      = new[] { "TypeCode", "Name" })]
    public Office Area
    {
      get => area ??= new();
      set => area = value;
    }

    /// <summary>
    /// A value of Caseworker.
    /// </summary>
    [JsonPropertyName("caseworker")]
    [Member(Index = 12, AccessFields = false, Members
      = new[] { "FormattedName" })]
    public CsePersonsWorkSet Caseworker
    {
      get => caseworker ??= new();
      set => caseworker = value;
    }

    private DateWorkArea reportingPeriodStart;
    private DateWorkArea iwglCreatedDate;
    private CsePersonsWorkSet csePersonsWorkSet;
    private EmployerAddress employerAddress;
    private Employer employer;
    private LegalAction legalAction;
    private CsePerson csePerson;
    private ServiceProvider serviceProvider;
    private Office office;
    private EabFileHandling eabFileHandling;
    private Office area;
    private CsePersonsWorkSet caseworker;
  }
#endregion
}
