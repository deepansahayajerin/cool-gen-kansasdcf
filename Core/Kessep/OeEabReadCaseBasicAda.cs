// Program: OE_EAB_READ_CASE_BASIC_ADA, ID: 374457221, model: 746.
// Short name: SWEXOE02
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: OE_EAB_READ_CASE_BASIC_ADA.
/// </para>
/// <para>
/// RESP: OBLGESTB
/// External Action Block which returns the next AE case where there is a change
/// of household membership
/// </para>
/// </summary>
[Serializable]
public partial class OeEabReadCaseBasicAda: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_EAB_READ_CASE_BASIC_ADA program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeEabReadCaseBasicAda(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeEabReadCaseBasicAda.
  /// </summary>
  public OeEabReadCaseBasicAda(IContext context, Import import, Export export):
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
      "SWEXOE02", context, import, export, EabOptions.Hpvp |
      EabOptions.NoIefParams);
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
    /// A value of ImHousehold.
    /// </summary>
    [JsonPropertyName("imHousehold")]
    [Member(Index = 1, AccessFields = false, Members = new[] { "AeCaseNo" })]
    public ImHousehold ImHousehold
    {
      get => imHousehold ??= new();
      set => imHousehold = value;
    }

    private ImHousehold imHousehold;
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
      = new[] { "CaseStatus", "StatusDate" })]
    public ImHousehold ImHousehold
    {
      get => imHousehold ??= new();
      set => imHousehold = value;
    }

    /// <summary>
    /// A value of ExecResults.
    /// </summary>
    [JsonPropertyName("execResults")]
    [Member(Index = 2, AccessFields = false, Members
      = new[] { "Text5", "Text80" })]
    public WorkArea ExecResults
    {
      get => execResults ??= new();
      set => execResults = value;
    }

    private ImHousehold imHousehold;
    private WorkArea execResults;
  }
#endregion
}
