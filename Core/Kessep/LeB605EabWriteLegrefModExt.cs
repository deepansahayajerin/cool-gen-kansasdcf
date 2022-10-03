// Program: LE_B605_EAB_WRITE_LEGREF_MOD_EXT, ID: 1625411583, model: 746.
// Short name: SWEXLE09
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: LE_B605_EAB_WRITE_LEGREF_MOD_EXT.
/// </para>
/// <para>
/// This External Action Block created to write the Legal Request Modifiction 
/// reason information(only modification legal referral reason codes will be
/// selected) on to a file.
/// </para>
/// </summary>
[Serializable]
public partial class LeB605EabWriteLegrefModExt: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the LE_B605_EAB_WRITE_LEGREF_MOD_EXT program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new LeB605EabWriteLegrefModExt(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of LeB605EabWriteLegrefModExt.
  /// </summary>
  public LeB605EabWriteLegrefModExt(IContext context, Import import,
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
      "SWEXLE09", context, import, export, EabOptions.Hpvp);
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
    /// A value of CsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("csePersonsWorkSet")]
    [Member(Index = 1, AccessFields = false, Members = new[] { "FormattedName" })
      ]
    public CsePersonsWorkSet CsePersonsWorkSet
    {
      get => csePersonsWorkSet ??= new();
      set => csePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of LegalReferral.
    /// </summary>
    [JsonPropertyName("legalReferral")]
    [Member(Index = 2, Members = new[]
    {
      "ReferralDate",
      "ReferralReason1",
      "ReferralReason2",
      "ReferralReason3",
      "ReferralReason5",
      "ReferralReason4",
      "Status",
      "Identifier",
      "StatusDate",
      "ReferredByUserId",
      "CourtCaseNumber",
      "TribunalId"
    })]
    public LegalReferral LegalReferral
    {
      get => legalReferral ??= new();
      set => legalReferral = value;
    }

    /// <summary>
    /// A value of Case1.
    /// </summary>
    [JsonPropertyName("case1")]
    [Member(Index = 3, Members = new[] { "Number" })]
    public Case1 Case1
    {
      get => case1 ??= new();
      set => case1 = value;
    }

    /// <summary>
    /// A value of Legal.
    /// </summary>
    [JsonPropertyName("legal")]
    [Member(Index = 4, Members = new[]
    {
      "LastName",
      "FirstName",
      "MiddleInitial"
    })]
    public ServiceProvider Legal
    {
      get => legal ??= new();
      set => legal = value;
    }

    /// <summary>
    /// A value of CoServiceProvider.
    /// </summary>
    [JsonPropertyName("coServiceProvider")]
    [Member(Index = 5, Members = new[]
    {
      "LastName",
      "FirstName",
      "MiddleInitial"
    })]
    public ServiceProvider CoServiceProvider
    {
      get => coServiceProvider ??= new();
      set => coServiceProvider = value;
    }

    /// <summary>
    /// A value of Supervisor.
    /// </summary>
    [JsonPropertyName("supervisor")]
    [Member(Index = 6, Members = new[]
    {
      "LastName",
      "FirstName",
      "MiddleInitial"
    })]
    public ServiceProvider Supervisor
    {
      get => supervisor ??= new();
      set => supervisor = value;
    }

    /// <summary>
    /// A value of CoOffice.
    /// </summary>
    [JsonPropertyName("coOffice")]
    [Member(Index = 7, Members = new[] { "SystemGeneratedId", "Name" })]
    public Office CoOffice
    {
      get => coOffice ??= new();
      set => coOffice = value;
    }

    /// <summary>
    /// A value of Area.
    /// </summary>
    [JsonPropertyName("area")]
    [Member(Index = 8, AccessFields = false, Members = new[] { "Name" })]
    public Office Area
    {
      get => area ??= new();
      set => area = value;
    }

    /// <summary>
    /// A value of EabFileHandling.
    /// </summary>
    [JsonPropertyName("eabFileHandling")]
    [Member(Index = 9, AccessFields = false, Members = new[] { "Action" })]
    public EabFileHandling EabFileHandling
    {
      get => eabFileHandling ??= new();
      set => eabFileHandling = value;
    }

    /// <summary>
    /// A value of Fips.
    /// </summary>
    [JsonPropertyName("fips")]
    [Member(Index = 10, AccessFields = false, Members = new[]
    {
      "StateAbbreviation",
      "State",
      "CountyAbbreviation",
      "County",
      "Location",
      "StateDescription",
      "CountyDescription",
      "LocationDescription"
    })]
    public Fips Fips
    {
      get => fips ??= new();
      set => fips = value;
    }

    /// <summary>
    /// A value of LegalAction.
    /// </summary>
    [JsonPropertyName("legalAction")]
    [Member(Index = 11, AccessFields = false, Members
      = new[] { "CourtCaseNumber", "StandardNumber" })]
    public LegalAction LegalAction
    {
      get => legalAction ??= new();
      set => legalAction = value;
    }

    private CsePersonsWorkSet csePersonsWorkSet;
    private LegalReferral legalReferral;
    private Case1 case1;
    private ServiceProvider legal;
    private ServiceProvider coServiceProvider;
    private ServiceProvider supervisor;
    private Office coOffice;
    private Office area;
    private EabFileHandling eabFileHandling;
    private Fips fips;
    private LegalAction legalAction;
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
