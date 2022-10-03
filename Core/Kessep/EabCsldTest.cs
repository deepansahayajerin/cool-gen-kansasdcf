// Program: EAB_CSLD_TEST, ID: 372728321, model: 746.
// Short name: EABCSLDT
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: EAB_CSLD_TEST.
/// </summary>
[Serializable]
public partial class EabCsldTest: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the EAB_CSLD_TEST program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new EabCsldTest(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of EabCsldTest.
  /// </summary>
  public EabCsldTest(IContext context, Import import, Export export):
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
      "EABCSLDT", context, import, export, EabOptions.Hpvp);
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
    /// A value of Office.
    /// </summary>
    [JsonPropertyName("office")]
    [Member(Index = 2, AccessFields = false, Members
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
    [Member(Index = 3, AccessFields = false, Members = new[] { "Number" })]
    public Case1 Case1
    {
      get => case1 ??= new();
      set => case1 = value;
    }

    /// <summary>
    /// A value of Program.
    /// </summary>
    [JsonPropertyName("program")]
    [Member(Index = 4, AccessFields = false, Members = new[] { "Code" })]
    public Program Program
    {
      get => program ??= new();
      set => program = value;
    }

    /// <summary>
    /// A value of Tribunal.
    /// </summary>
    [JsonPropertyName("tribunal")]
    [Member(Index = 5, AccessFields = false, Members = new[] { "Text4" })]
    public TextWorkArea Tribunal
    {
      get => tribunal ??= new();
      set => tribunal = value;
    }

    /// <summary>
    /// A value of CaseFuncWorkSet.
    /// </summary>
    [JsonPropertyName("caseFuncWorkSet")]
    [Member(Index = 6, AccessFields = false, Members = new[] { "FuncText3" })]
    public CaseFuncWorkSet CaseFuncWorkSet
    {
      get => caseFuncWorkSet ??= new();
      set => caseFuncWorkSet = value;
    }

    /// <summary>
    /// A value of TextWorkArea.
    /// </summary>
    [JsonPropertyName("textWorkArea")]
    [Member(Index = 7, AccessFields = false, Members = new[] { "Text8" })]
    public TextWorkArea TextWorkArea
    {
      get => textWorkArea ??= new();
      set => textWorkArea = value;
    }

    /// <summary>
    /// A value of OldSp.
    /// </summary>
    [JsonPropertyName("oldSp")]
    [Member(Index = 8, AccessFields = false, Members = new[] { "UserId" })]
    public ServiceProvider OldSp
    {
      get => oldSp ??= new();
      set => oldSp = value;
    }

    /// <summary>
    /// A value of NewSp.
    /// </summary>
    [JsonPropertyName("newSp")]
    [Member(Index = 9, AccessFields = false, Members = new[] { "UserId" })]
    public ServiceProvider NewSp
    {
      get => newSp ??= new();
      set => newSp = value;
    }

    private ReportParms reportParms;
    private Office office;
    private Case1 case1;
    private Program program;
    private TextWorkArea tribunal;
    private CaseFuncWorkSet caseFuncWorkSet;
    private TextWorkArea textWorkArea;
    private ServiceProvider oldSp;
    private ServiceProvider newSp;
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
