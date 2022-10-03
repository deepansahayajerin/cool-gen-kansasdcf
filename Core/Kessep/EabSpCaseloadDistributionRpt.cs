// Program: EAB_SP_CASELOAD_DISTRIBUTION_RPT, ID: 372571944, model: 746.
// Short name: SWEXPL06
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: EAB_SP_CASELOAD_DISTRIBUTION_RPT.
/// </summary>
[Serializable]
public partial class EabSpCaseloadDistributionRpt: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the EAB_SP_CASELOAD_DISTRIBUTION_RPT program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new EabSpCaseloadDistributionRpt(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of EabSpCaseloadDistributionRpt.
  /// </summary>
  public EabSpCaseloadDistributionRpt(IContext context, Import import,
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
      "SWEXPL06", context, import, export, EabOptions.Hpvp);
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
    /// A value of ReportParms.
    /// </summary>
    [JsonPropertyName("reportParms")]
    [Member(Index = 1, AccessFields = false, Members
      = new[] { "Parm1", "Parm2" })]
    public ReportParms ReportParms
    {
      get => reportParms ??= new();
      set => reportParms = value;
    }

    /// <summary>
    /// A value of RptHeading.
    /// </summary>
    [JsonPropertyName("rptHeading")]
    [Member(Index = 2, AccessFields = false, Members = new[] { "Text30" })]
    public TextWorkArea RptHeading
    {
      get => rptHeading ??= new();
      set => rptHeading = value;
    }

    /// <summary>
    /// A value of Office.
    /// </summary>
    [JsonPropertyName("office")]
    [Member(Index = 3, AccessFields = false, Members
      = new[] { "SystemGeneratedId", "Name" })]
    public Office Office
    {
      get => office ??= new();
      set => office = value;
    }

    /// <summary>
    /// A value of OfficeAddress.
    /// </summary>
    [JsonPropertyName("officeAddress")]
    [Member(Index = 4, AccessFields = false, Members
      = new[] { "Street1", "Street2" })]
    public OfficeAddress OfficeAddress
    {
      get => officeAddress ??= new();
      set => officeAddress = value;
    }

    /// <summary>
    /// A value of RptCityStZip.
    /// </summary>
    [JsonPropertyName("rptCityStZip")]
    [Member(Index = 5, AccessFields = false, Members = new[] { "Text30" })]
    public TextWorkArea RptCityStZip
    {
      get => rptCityStZip ??= new();
      set => rptCityStZip = value;
    }

    /// <summary>
    /// A value of DateWorkArea.
    /// </summary>
    [JsonPropertyName("dateWorkArea")]
    [Member(Index = 6, AccessFields = false, Members = new[] { "Date" })]
    public DateWorkArea DateWorkArea
    {
      get => dateWorkArea ??= new();
      set => dateWorkArea = value;
    }

    /// <summary>
    /// A value of ServiceProvider.
    /// </summary>
    [JsonPropertyName("serviceProvider")]
    [Member(Index = 7, AccessFields = false, Members
      = new[] { "SystemGeneratedId" })]
    public ServiceProvider ServiceProvider
    {
      get => serviceProvider ??= new();
      set => serviceProvider = value;
    }

    /// <summary>
    /// A value of CsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("csePersonsWorkSet")]
    [Member(Index = 8, AccessFields = false, Members = new[] { "FormattedName" })
      ]
    public CsePersonsWorkSet CsePersonsWorkSet
    {
      get => csePersonsWorkSet ??= new();
      set => csePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of OfficeServiceProvider.
    /// </summary>
    [JsonPropertyName("officeServiceProvider")]
    [Member(Index = 9, AccessFields = false, Members = new[] { "RoleCode" })]
    public OfficeServiceProvider OfficeServiceProvider
    {
      get => officeServiceProvider ??= new();
      set => officeServiceProvider = value;
    }

    /// <summary>
    /// A value of Program.
    /// </summary>
    [JsonPropertyName("program")]
    [Member(Index = 10, AccessFields = false, Members = new[] { "Code" })]
    public Program Program
    {
      get => program ??= new();
      set => program = value;
    }

    /// <summary>
    /// A value of Tribunal.
    /// </summary>
    [JsonPropertyName("tribunal")]
    [Member(Index = 11, AccessFields = false, Members = new[] { "Text4" })]
    public TextWorkArea Tribunal
    {
      get => tribunal ??= new();
      set => tribunal = value;
    }

    /// <summary>
    /// A value of OfficeCaseloadAssignment.
    /// </summary>
    [JsonPropertyName("officeCaseloadAssignment")]
    [Member(Index = 12, AccessFields = false, Members = new[]
    {
      "EndingAlpha",
      "BeginingAlpha",
      "Priority",
      "Function"
    })]
    public OfficeCaseloadAssignment OfficeCaseloadAssignment
    {
      get => officeCaseloadAssignment ??= new();
      set => officeCaseloadAssignment = value;
    }

    /// <summary>
    /// A value of Cases.
    /// </summary>
    [JsonPropertyName("cases")]
    [Member(Index = 13, AccessFields = false, Members = new[] { "Count" })]
    public Common Cases
    {
      get => cases ??= new();
      set => cases = value;
    }

    /// <summary>
    /// A value of Ovrd.
    /// </summary>
    [JsonPropertyName("ovrd")]
    [Member(Index = 14, AccessFields = false, Members = new[] { "Count" })]
    public Common Ovrd
    {
      get => ovrd ??= new();
      set => ovrd = value;
    }

    /// <summary>
    /// A value of SpTotCases.
    /// </summary>
    [JsonPropertyName("spTotCases")]
    [Member(Index = 15, AccessFields = false, Members = new[] { "Count" })]
    public Common SpTotCases
    {
      get => spTotCases ??= new();
      set => spTotCases = value;
    }

    /// <summary>
    /// A value of SpTotOvrd.
    /// </summary>
    [JsonPropertyName("spTotOvrd")]
    [Member(Index = 16, AccessFields = false, Members = new[] { "Count" })]
    public Common SpTotOvrd
    {
      get => spTotOvrd ??= new();
      set => spTotOvrd = value;
    }

    /// <summary>
    /// A value of OffcTotCases.
    /// </summary>
    [JsonPropertyName("offcTotCases")]
    [Member(Index = 17, AccessFields = false, Members = new[] { "Count" })]
    public Common OffcTotCases
    {
      get => offcTotCases ??= new();
      set => offcTotCases = value;
    }

    /// <summary>
    /// A value of OffcTotOvrd.
    /// </summary>
    [JsonPropertyName("offcTotOvrd")]
    [Member(Index = 18, AccessFields = false, Members = new[] { "Count" })]
    public Common OffcTotOvrd
    {
      get => offcTotOvrd ??= new();
      set => offcTotOvrd = value;
    }

    private ReportParms reportParms;
    private TextWorkArea rptHeading;
    private Office office;
    private OfficeAddress officeAddress;
    private TextWorkArea rptCityStZip;
    private DateWorkArea dateWorkArea;
    private ServiceProvider serviceProvider;
    private CsePersonsWorkSet csePersonsWorkSet;
    private OfficeServiceProvider officeServiceProvider;
    private Program program;
    private TextWorkArea tribunal;
    private OfficeCaseloadAssignment officeCaseloadAssignment;
    private Common cases;
    private Common ovrd;
    private Common spTotCases;
    private Common spTotOvrd;
    private Common offcTotCases;
    private Common offcTotOvrd;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of ReportParms.
    /// </summary>
    [JsonPropertyName("reportParms")]
    [Member(Index = 1, AccessFields = false, Members
      = new[] { "Parm1", "Parm2" })]
    public ReportParms ReportParms
    {
      get => reportParms ??= new();
      set => reportParms = value;
    }

    private ReportParms reportParms;
  }
#endregion
}
