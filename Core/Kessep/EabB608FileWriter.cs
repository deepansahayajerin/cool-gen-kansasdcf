// Program: EAB_B608_FILE_WRITER, ID: 372956509, model: 746.
// Short name: SWEX608W
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: EAB_B608_FILE_WRITER.
/// </para>
/// <para>
/// This EAB writes a flat file containing data required for the Attorney-
/// Contractor Collection Detail Report.  This file should be the input into a
/// sort step and then into the report writer step.
/// </para>
/// </summary>
[Serializable]
public partial class EabB608FileWriter: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the EAB_B608_FILE_WRITER program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new EabB608FileWriter(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of EabB608FileWriter.
  /// </summary>
  public EabB608FileWriter(IContext context, Import import, Export export):
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
      "SWEX608W", context, import, export, EabOptions.Hpvp);
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
    [Member(Index = 1, AccessFields = false, Members = new[]
    {
      "Parm1",
      "Parm2",
      "SubreportCode"
    })]
    public ReportParms ReportParms
    {
      get => reportParms ??= new();
      set => reportParms = value;
    }

    /// <summary>
    /// A value of Bom.
    /// </summary>
    [JsonPropertyName("bom")]
    [Member(Index = 2, AccessFields = false, Members = new[] { "Date" })]
    public DateWorkArea Bom
    {
      get => bom ??= new();
      set => bom = value;
    }

    /// <summary>
    /// A value of ServiceProvider.
    /// </summary>
    [JsonPropertyName("serviceProvider")]
    [Member(Index = 3, AccessFields = false, Members = new[]
    {
      "SystemGeneratedId",
      "UserId",
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
    /// A value of Ko.
    /// </summary>
    [JsonPropertyName("ko")]
    [Member(Index = 4, AccessFields = false, Members = new[] { "Text1" })]
    public TextWorkArea Ko
    {
      get => ko ??= new();
      set => ko = value;
    }

    /// <summary>
    /// A value of Collection.
    /// </summary>
    [JsonPropertyName("collection")]
    [Member(Index = 5, AccessFields = false, Members = new[]
    {
      "ProgramAppliedTo",
      "Amount",
      "AppliedToCode",
      "CollectionDt",
      "AdjustedInd"
    })]
    public Collection Collection
    {
      get => collection ??= new();
      set => collection = value;
    }

    /// <summary>
    /// A value of ProgGrp.
    /// </summary>
    [JsonPropertyName("progGrp")]
    [Member(Index = 6, AccessFields = false, Members = new[] { "Text1" })]
    public TextWorkArea ProgGrp
    {
      get => progGrp ??= new();
      set => progGrp = value;
    }

    /// <summary>
    /// A value of Ap.
    /// </summary>
    [JsonPropertyName("ap")]
    [Member(Index = 7, AccessFields = false, Members = new[]
    {
      "Number",
      "FirstName",
      "MiddleInitial",
      "FormattedName",
      "LastName"
    })]
    public CsePersonsWorkSet Ap
    {
      get => ap ??= new();
      set => ap = value;
    }

    /// <summary>
    /// A value of Ar.
    /// </summary>
    [JsonPropertyName("ar")]
    [Member(Index = 8, AccessFields = false, Members = new[]
    {
      "Number",
      "FirstName",
      "MiddleInitial",
      "FormattedName",
      "LastName"
    })]
    public CsePersonsWorkSet Ar
    {
      get => ar ??= new();
      set => ar = value;
    }

    /// <summary>
    /// A value of LegalAction.
    /// </summary>
    [JsonPropertyName("legalAction")]
    [Member(Index = 9, AccessFields = false, Members = new[]
    {
      "Identifier",
      "CourtCaseNumber",
      "StandardNumber"
    })]
    public LegalAction LegalAction
    {
      get => legalAction ??= new();
      set => legalAction = value;
    }

    /// <summary>
    /// A value of Case1.
    /// </summary>
    [JsonPropertyName("case1")]
    [Member(Index = 10, AccessFields = false, Members = new[] { "Number" })]
    public Case1 Case1
    {
      get => case1 ??= new();
      set => case1 = value;
    }

    /// <summary>
    /// A value of CollectionType.
    /// </summary>
    [JsonPropertyName("collectionType")]
    [Member(Index = 11, AccessFields = false, Members = new[] { "Code" })]
    public CollectionType CollectionType
    {
      get => collectionType ??= new();
      set => collectionType = value;
    }

    /// <summary>
    /// A value of LegalReferralAssignment.
    /// </summary>
    [JsonPropertyName("legalReferralAssignment")]
    [Member(Index = 12, AccessFields = false, Members
      = new[] { "EffectiveDate" })]
    public LegalReferralAssignment LegalReferralAssignment
    {
      get => legalReferralAssignment ??= new();
      set => legalReferralAssignment = value;
    }

    /// <summary>
    /// A value of Office.
    /// </summary>
    [JsonPropertyName("office")]
    [Member(Index = 13, AccessFields = false, Members = new[] { "Name" })]
    public Office Office
    {
      get => office ??= new();
      set => office = value;
    }

    /// <summary>
    /// A value of Fips.
    /// </summary>
    [JsonPropertyName("fips")]
    [Member(Index = 14, AccessFields = false, Members
      = new[] { "CountyDescription" })]
    public Fips Fips
    {
      get => fips ??= new();
      set => fips = value;
    }

    /// <summary>
    /// A value of Tribunal.
    /// </summary>
    [JsonPropertyName("tribunal")]
    [Member(Index = 15, AccessFields = false, Members
      = new[] { "JudicialDistrict", "Name" })]
    public Tribunal Tribunal
    {
      get => tribunal ??= new();
      set => tribunal = value;
    }

    private ReportParms reportParms;
    private DateWorkArea bom;
    private ServiceProvider serviceProvider;
    private TextWorkArea ko;
    private Collection collection;
    private TextWorkArea progGrp;
    private CsePersonsWorkSet ap;
    private CsePersonsWorkSet ar;
    private LegalAction legalAction;
    private Case1 case1;
    private CollectionType collectionType;
    private LegalReferralAssignment legalReferralAssignment;
    private Office office;
    private Fips fips;
    private Tribunal tribunal;
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
    [Member(Index = 1, AccessFields = false, Members = new[]
    {
      "Parm1",
      "Parm2",
      "SubreportCode"
    })]
    public ReportParms ReportParms
    {
      get => reportParms ??= new();
      set => reportParms = value;
    }

    private ReportParms reportParms;
  }
#endregion
}
