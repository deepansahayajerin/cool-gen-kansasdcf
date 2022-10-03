// Program: EAB_B609_REPORT_WRITER, ID: 372958144, model: 746.
// Short name: SWEX609R
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: EAB_B609_REPORT_WRITER.
/// </para>
/// <para>
/// This EAB writes a flat file containing data required for the Attorney-
/// Contractor Collection Detail Report.  This file should be the input into a
/// sort step and then into the report writer step.
/// </para>
/// </summary>
[Serializable]
public partial class EabB609ReportWriter: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the EAB_B609_REPORT_WRITER program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new EabB609ReportWriter(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of EabB609ReportWriter.
  /// </summary>
  public EabB609ReportWriter(IContext context, Import import, Export export):
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
      "SWEX609R", context, import, export, EabOptions.Hpvp);
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
    /// A value of Eom.
    /// </summary>
    [JsonPropertyName("eom")]
    [Member(Index = 3, AccessFields = false, Members = new[] { "Date" })]
    public DateWorkArea Eom
    {
      get => eom ??= new();
      set => eom = value;
    }

    /// <summary>
    /// A value of ServiceProvider.
    /// </summary>
    [JsonPropertyName("serviceProvider")]
    [Member(Index = 4, AccessFields = false, Members = new[]
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
    /// A value of Ap.
    /// </summary>
    [JsonPropertyName("ap")]
    [Member(Index = 5, AccessFields = false, Members = new[]
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
    [Member(Index = 6, AccessFields = false, Members = new[]
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
    [Member(Index = 7, AccessFields = false, Members = new[]
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
    /// A value of Collection.
    /// </summary>
    [JsonPropertyName("collection")]
    [Member(Index = 8, AccessFields = false, Members = new[]
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
    /// A value of CollectionType.
    /// </summary>
    [JsonPropertyName("collectionType")]
    [Member(Index = 9, AccessFields = false, Members = new[] { "Code" })]
    public CollectionType CollectionType
    {
      get => collectionType ??= new();
      set => collectionType = value;
    }

    /// <summary>
    /// A value of LegalReferralAssignment.
    /// </summary>
    [JsonPropertyName("legalReferralAssignment")]
    [Member(Index = 10, AccessFields = false, Members
      = new[] { "EffectiveDate" })]
    public LegalReferralAssignment LegalReferralAssignment
    {
      get => legalReferralAssignment ??= new();
      set => legalReferralAssignment = value;
    }

    /// <summary>
    /// A value of Prev.
    /// </summary>
    [JsonPropertyName("prev")]
    [Member(Index = 11, AccessFields = false, Members = new[]
    {
      "SystemGeneratedId",
      "UserId",
      "LastName",
      "FirstName",
      "MiddleInitial"
    })]
    public ServiceProvider Prev
    {
      get => prev ??= new();
      set => prev = value;
    }

    /// <summary>
    /// A value of SpGtotLtParn.
    /// </summary>
    [JsonPropertyName("spGtotLtParn")]
    [Member(Index = 12, AccessFields = false, Members = new[] { "Text1" })]
    public WorkArea SpGtotLtParn
    {
      get => spGtotLtParn ??= new();
      set => spGtotLtParn = value;
    }

    /// <summary>
    /// A value of SpGtotRtParn.
    /// </summary>
    [JsonPropertyName("spGtotRtParn")]
    [Member(Index = 13, AccessFields = false, Members = new[] { "Text1" })]
    public WorkArea SpGtotRtParn
    {
      get => spGtotRtParn ??= new();
      set => spGtotRtParn = value;
    }

    /// <summary>
    /// A value of SpOthTafLtParn.
    /// </summary>
    [JsonPropertyName("spOthTafLtParn")]
    [Member(Index = 14, AccessFields = false, Members = new[] { "Text1" })]
    public WorkArea SpOthTafLtParn
    {
      get => spOthTafLtParn ??= new();
      set => spOthTafLtParn = value;
    }

    /// <summary>
    /// A value of SpOthTafRtParn.
    /// </summary>
    [JsonPropertyName("spOthTafRtParn")]
    [Member(Index = 15, AccessFields = false, Members = new[] { "Text1" })]
    public WorkArea SpOthTafRtParn
    {
      get => spOthTafRtParn ??= new();
      set => spOthTafRtParn = value;
    }

    /// <summary>
    /// A value of SpOthNtafLtParn.
    /// </summary>
    [JsonPropertyName("spOthNtafLtParn")]
    [Member(Index = 16, AccessFields = false, Members = new[] { "Text1" })]
    public WorkArea SpOthNtafLtParn
    {
      get => spOthNtafLtParn ??= new();
      set => spOthNtafLtParn = value;
    }

    /// <summary>
    /// A value of SpOthNtafRtParn.
    /// </summary>
    [JsonPropertyName("spOthNtafRtParn")]
    [Member(Index = 17, AccessFields = false, Members = new[] { "Text1" })]
    public WorkArea SpOthNtafRtParn
    {
      get => spOthNtafRtParn ??= new();
      set => spOthNtafRtParn = value;
    }

    /// <summary>
    /// A value of SpKsSoLtParn.
    /// </summary>
    [JsonPropertyName("spKsSoLtParn")]
    [Member(Index = 18, AccessFields = false, Members = new[] { "Text1" })]
    public WorkArea SpKsSoLtParn
    {
      get => spKsSoLtParn ??= new();
      set => spKsSoLtParn = value;
    }

    /// <summary>
    /// A value of SpKsSoRtParn.
    /// </summary>
    [JsonPropertyName("spKsSoRtParn")]
    [Member(Index = 19, AccessFields = false, Members = new[] { "Text1" })]
    public WorkArea SpKsSoRtParn
    {
      get => spKsSoRtParn ??= new();
      set => spKsSoRtParn = value;
    }

    /// <summary>
    /// A value of SpKsTafLtParn.
    /// </summary>
    [JsonPropertyName("spKsTafLtParn")]
    [Member(Index = 20, AccessFields = false, Members = new[] { "Text1" })]
    public WorkArea SpKsTafLtParn
    {
      get => spKsTafLtParn ??= new();
      set => spKsTafLtParn = value;
    }

    /// <summary>
    /// A value of SpKsTafRtParn.
    /// </summary>
    [JsonPropertyName("spKsTafRtParn")]
    [Member(Index = 21, AccessFields = false, Members = new[] { "Text1" })]
    public WorkArea SpKsTafRtParn
    {
      get => spKsTafRtParn ??= new();
      set => spKsTafRtParn = value;
    }

    /// <summary>
    /// A value of SpKsNtafLtParn.
    /// </summary>
    [JsonPropertyName("spKsNtafLtParn")]
    [Member(Index = 22, AccessFields = false, Members = new[] { "Text1" })]
    public WorkArea SpKsNtafLtParn
    {
      get => spKsNtafLtParn ??= new();
      set => spKsNtafLtParn = value;
    }

    /// <summary>
    /// A value of SpKsNtafRtParn.
    /// </summary>
    [JsonPropertyName("spKsNtafRtParn")]
    [Member(Index = 23, AccessFields = false, Members = new[] { "Text1" })]
    public WorkArea SpKsNtafRtParn
    {
      get => spKsNtafRtParn ??= new();
      set => spKsNtafRtParn = value;
    }

    /// <summary>
    /// A value of PrgTotLtParn.
    /// </summary>
    [JsonPropertyName("prgTotLtParn")]
    [Member(Index = 24, AccessFields = false, Members = new[] { "Text1" })]
    public WorkArea PrgTotLtParn
    {
      get => prgTotLtParn ??= new();
      set => prgTotLtParn = value;
    }

    /// <summary>
    /// A value of PrgTotRtParn.
    /// </summary>
    [JsonPropertyName("prgTotRtParn")]
    [Member(Index = 25, AccessFields = false, Members = new[] { "Text1" })]
    public WorkArea PrgTotRtParn
    {
      get => prgTotRtParn ??= new();
      set => prgTotRtParn = value;
    }

    /// <summary>
    /// A value of DetLtParn.
    /// </summary>
    [JsonPropertyName("detLtParn")]
    [Member(Index = 26, AccessFields = false, Members = new[] { "Text1" })]
    public WorkArea DetLtParn
    {
      get => detLtParn ??= new();
      set => detLtParn = value;
    }

    /// <summary>
    /// A value of DetRtParn.
    /// </summary>
    [JsonPropertyName("detRtParn")]
    [Member(Index = 27, AccessFields = false, Members = new[] { "Text1" })]
    public WorkArea DetRtParn
    {
      get => detRtParn ??= new();
      set => detRtParn = value;
    }

    /// <summary>
    /// A value of SpKsSoCommon.
    /// </summary>
    [JsonPropertyName("spKsSoCommon")]
    [Member(Index = 28, AccessFields = false, Members = new[] { "Count" })]
    public Common SpKsSoCommon
    {
      get => spKsSoCommon ??= new();
      set => spKsSoCommon = value;
    }

    /// <summary>
    /// A value of SpKsSoCollection.
    /// </summary>
    [JsonPropertyName("spKsSoCollection")]
    [Member(Index = 29, AccessFields = false, Members = new[] { "Amount" })]
    public Collection SpKsSoCollection
    {
      get => spKsSoCollection ??= new();
      set => spKsSoCollection = value;
    }

    /// <summary>
    /// A value of SpKsNtafCommon.
    /// </summary>
    [JsonPropertyName("spKsNtafCommon")]
    [Member(Index = 30, AccessFields = false, Members = new[] { "Count" })]
    public Common SpKsNtafCommon
    {
      get => spKsNtafCommon ??= new();
      set => spKsNtafCommon = value;
    }

    /// <summary>
    /// A value of SpKsNtafCollection.
    /// </summary>
    [JsonPropertyName("spKsNtafCollection")]
    [Member(Index = 31, AccessFields = false, Members = new[] { "Amount" })]
    public Collection SpKsNtafCollection
    {
      get => spKsNtafCollection ??= new();
      set => spKsNtafCollection = value;
    }

    /// <summary>
    /// A value of SpKsTafCommon.
    /// </summary>
    [JsonPropertyName("spKsTafCommon")]
    [Member(Index = 32, AccessFields = false, Members = new[] { "Count" })]
    public Common SpKsTafCommon
    {
      get => spKsTafCommon ??= new();
      set => spKsTafCommon = value;
    }

    /// <summary>
    /// A value of SpKsTafCollection.
    /// </summary>
    [JsonPropertyName("spKsTafCollection")]
    [Member(Index = 33, AccessFields = false, Members = new[] { "Amount" })]
    public Collection SpKsTafCollection
    {
      get => spKsTafCollection ??= new();
      set => spKsTafCollection = value;
    }

    /// <summary>
    /// A value of SpGtotCommon.
    /// </summary>
    [JsonPropertyName("spGtotCommon")]
    [Member(Index = 34, AccessFields = false, Members = new[] { "Count" })]
    public Common SpGtotCommon
    {
      get => spGtotCommon ??= new();
      set => spGtotCommon = value;
    }

    /// <summary>
    /// A value of SpGtotCollection.
    /// </summary>
    [JsonPropertyName("spGtotCollection")]
    [Member(Index = 35, AccessFields = false, Members = new[] { "Amount" })]
    public Collection SpGtotCollection
    {
      get => spGtotCollection ??= new();
      set => spGtotCollection = value;
    }

    /// <summary>
    /// A value of SpOthNtafCommon.
    /// </summary>
    [JsonPropertyName("spOthNtafCommon")]
    [Member(Index = 36, AccessFields = false, Members = new[] { "Count" })]
    public Common SpOthNtafCommon
    {
      get => spOthNtafCommon ??= new();
      set => spOthNtafCommon = value;
    }

    /// <summary>
    /// A value of SpOthNtafCollection.
    /// </summary>
    [JsonPropertyName("spOthNtafCollection")]
    [Member(Index = 37, AccessFields = false, Members = new[] { "Amount" })]
    public Collection SpOthNtafCollection
    {
      get => spOthNtafCollection ??= new();
      set => spOthNtafCollection = value;
    }

    /// <summary>
    /// A value of SpOthTafCommon.
    /// </summary>
    [JsonPropertyName("spOthTafCommon")]
    [Member(Index = 38, AccessFields = false, Members = new[] { "Count" })]
    public Common SpOthTafCommon
    {
      get => spOthTafCommon ??= new();
      set => spOthTafCommon = value;
    }

    /// <summary>
    /// A value of SpOthTafCollection.
    /// </summary>
    [JsonPropertyName("spOthTafCollection")]
    [Member(Index = 39, AccessFields = false, Members = new[] { "Amount" })]
    public Collection SpOthTafCollection
    {
      get => spOthTafCollection ??= new();
      set => spOthTafCollection = value;
    }

    /// <summary>
    /// A value of PrgTotCommon.
    /// </summary>
    [JsonPropertyName("prgTotCommon")]
    [Member(Index = 40, AccessFields = false, Members = new[] { "Count" })]
    public Common PrgTotCommon
    {
      get => prgTotCommon ??= new();
      set => prgTotCommon = value;
    }

    /// <summary>
    /// A value of PrgTotCollection.
    /// </summary>
    [JsonPropertyName("prgTotCollection")]
    [Member(Index = 41, AccessFields = false, Members = new[] { "Amount" })]
    public Collection PrgTotCollection
    {
      get => prgTotCollection ??= new();
      set => prgTotCollection = value;
    }

    /// <summary>
    /// A value of CtlBrkKsOth.
    /// </summary>
    [JsonPropertyName("ctlBrkKsOth")]
    [Member(Index = 42, AccessFields = false, Members = new[] { "Text10" })]
    public TextWorkArea CtlBrkKsOth
    {
      get => ctlBrkKsOth ??= new();
      set => ctlBrkKsOth = value;
    }

    /// <summary>
    /// A value of CtlBrkSp.
    /// </summary>
    [JsonPropertyName("ctlBrkSp")]
    [Member(Index = 43, AccessFields = false, Members = new[] { "Text30" })]
    public TextWorkArea CtlBrkSp
    {
      get => ctlBrkSp ??= new();
      set => ctlBrkSp = value;
    }

    /// <summary>
    /// A value of CtlBrkPrgGrp.
    /// </summary>
    [JsonPropertyName("ctlBrkPrgGrp")]
    [Member(Index = 44, AccessFields = false, Members = new[] { "Text10" })]
    public TextWorkArea CtlBrkPrgGrp
    {
      get => ctlBrkPrgGrp ??= new();
      set => ctlBrkPrgGrp = value;
    }

    private ReportParms reportParms;
    private DateWorkArea bom;
    private DateWorkArea eom;
    private ServiceProvider serviceProvider;
    private CsePersonsWorkSet ap;
    private CsePersonsWorkSet ar;
    private LegalAction legalAction;
    private Collection collection;
    private CollectionType collectionType;
    private LegalReferralAssignment legalReferralAssignment;
    private ServiceProvider prev;
    private WorkArea spGtotLtParn;
    private WorkArea spGtotRtParn;
    private WorkArea spOthTafLtParn;
    private WorkArea spOthTafRtParn;
    private WorkArea spOthNtafLtParn;
    private WorkArea spOthNtafRtParn;
    private WorkArea spKsSoLtParn;
    private WorkArea spKsSoRtParn;
    private WorkArea spKsTafLtParn;
    private WorkArea spKsTafRtParn;
    private WorkArea spKsNtafLtParn;
    private WorkArea spKsNtafRtParn;
    private WorkArea prgTotLtParn;
    private WorkArea prgTotRtParn;
    private WorkArea detLtParn;
    private WorkArea detRtParn;
    private Common spKsSoCommon;
    private Collection spKsSoCollection;
    private Common spKsNtafCommon;
    private Collection spKsNtafCollection;
    private Common spKsTafCommon;
    private Collection spKsTafCollection;
    private Common spGtotCommon;
    private Collection spGtotCollection;
    private Common spOthNtafCommon;
    private Collection spOthNtafCollection;
    private Common spOthTafCommon;
    private Collection spOthTafCollection;
    private Common prgTotCommon;
    private Collection prgTotCollection;
    private TextWorkArea ctlBrkKsOth;
    private TextWorkArea ctlBrkSp;
    private TextWorkArea ctlBrkPrgGrp;
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
