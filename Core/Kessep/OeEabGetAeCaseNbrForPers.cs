// Program: OE_EAB_GET_AE_CASE_NBR_FOR_PERS, ID: 374473641, model: 746.
// Short name: SWEXOE04
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: OE_EAB_GET_AE_CASE_NBR_FOR_PERS.
/// </para>
/// <para>
/// RESP: OBLGESTB
/// This program will be responsible for retrieving benefit (initial, 
/// supplemental, and adjustment) information. This module will not perform any
/// edit or validation logic on inbound data. It will be up to the calling
/// module to ensure all mandatory and key fields are populated.
/// </para>
/// </summary>
[Serializable]
public partial class OeEabGetAeCaseNbrForPers: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_EAB_GET_AE_CASE_NBR_FOR_PERS program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeEabGetAeCaseNbrForPers(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeEabGetAeCaseNbrForPers.
  /// </summary>
  public OeEabGetAeCaseNbrForPers(IContext context, Import import, Export export)
    :
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
      "SWEXOE04", context, import, export, EabOptions.Hpvp);
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
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    [Member(Index = 1, AccessFields = false, Members = new[] { "Number" })]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
    }

    /// <summary>
    /// A value of OnFcDate.
    /// </summary>
    [JsonPropertyName("onFcDate")]
    [Member(Index = 2, AccessFields = false, Members = new[] { "Date" })]
    public DateWorkArea OnFcDate
    {
      get => onFcDate ??= new();
      set => onFcDate = value;
    }

    /// <summary>
    /// A value of AdabasExternalAction.
    /// </summary>
    [JsonPropertyName("adabasExternalAction")]
    [Member(Index = 3, AccessFields = false, Members = new[] { "Flag" })]
    public Common AdabasExternalAction
    {
      get => adabasExternalAction ??= new();
      set => adabasExternalAction = value;
    }

    private CsePerson csePerson;
    private DateWorkArea onFcDate;
    private Common adabasExternalAction;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of ImHousehold.
    /// </summary>
    [JsonPropertyName("imHousehold")]
    [Member(Index = 1, AccessFields = false, Members
      = new[] { "AeCaseNo", "FirstBenefitDate" })]
    public ImHousehold ImHousehold
    {
      get => imHousehold ??= new();
      set => imHousehold = value;
    }

    /// <summary>
    /// A value of ImHouseholdMbrMnthlySum.
    /// </summary>
    [JsonPropertyName("imHouseholdMbrMnthlySum")]
    [Member(Index = 2, AccessFields = false, Members = new[] { "Relationship" })]
      
    public ImHouseholdMbrMnthlySum ImHouseholdMbrMnthlySum
    {
      get => imHouseholdMbrMnthlySum ??= new();
      set => imHouseholdMbrMnthlySum = value;
    }

    /// <summary>
    /// A value of ExecResults.
    /// </summary>
    [JsonPropertyName("execResults")]
    [Member(Index = 3, AccessFields = false, Members
      = new[] { "Text5", "Text80" })]
    public WorkArea ExecResults
    {
      get => execResults ??= new();
      set => execResults = value;
    }

    private ImHousehold imHousehold;
    private ImHouseholdMbrMnthlySum imHouseholdMbrMnthlySum;
    private WorkArea execResults;
  }
#endregion
}
