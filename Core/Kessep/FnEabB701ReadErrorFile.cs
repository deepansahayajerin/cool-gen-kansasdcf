// Program: FN_EAB_B701_READ_ERROR_FILE, ID: 945110157, model: 746.
// Short name: SWEXER20
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: FN_EAB_B701_READ_ERROR_FILE.
/// </para>
/// <para>
/// Resp:OBLGEST
/// This EXTERNAL procedure carries view that will be used for receiving the 
/// FPLS responses from the Federal Parent Locator Service in reaction to
/// FPLS_LOCATE_REQUEST's.
/// </para>
/// </summary>
[Serializable]
public partial class FnEabB701ReadErrorFile: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_EAB_B701_READ_ERROR_FILE program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnEabB701ReadErrorFile(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnEabB701ReadErrorFile.
  /// </summary>
  public FnEabB701ReadErrorFile(IContext context, Import import, Export export):
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
      "SWEXER20", context, import, export, EabOptions.Hpvp);
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
    /// A value of External.
    /// </summary>
    [JsonPropertyName("external")]
    [Member(Index = 1, Members = new[] { "FileInstruction" })]
    public External External
    {
      get => external ??= new();
      set => external = value;
    }

    private External external;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of Case1.
    /// </summary>
    [JsonPropertyName("case1")]
    [Member(Index = 1, AccessFields = false, Members = new[] { "Number" })]
    public Case1 Case1
    {
      get => case1 ??= new();
      set => case1 = value;
    }

    /// <summary>
    /// A value of ServiceProvider.
    /// </summary>
    [JsonPropertyName("serviceProvider")]
    [Member(Index = 2, AccessFields = false, Members = new[]
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
    [Member(Index = 3, AccessFields = false, Members = new[] { "Name" })]
    public Office Office
    {
      get => office ??= new();
      set => office = value;
    }

    /// <summary>
    /// A value of Region.
    /// </summary>
    [JsonPropertyName("region")]
    [Member(Index = 4, AccessFields = false, Members = new[] { "Name" })]
    public CseOrganization Region
    {
      get => region ??= new();
      set => region = value;
    }

    /// <summary>
    /// A value of EabReportSend.
    /// </summary>
    [JsonPropertyName("eabReportSend")]
    [Member(Index = 5, AccessFields = false, Members = new[] { "RptDetail" })]
    public EabReportSend EabReportSend
    {
      get => eabReportSend ??= new();
      set => eabReportSend = value;
    }

    /// <summary>
    /// A value of External.
    /// </summary>
    [JsonPropertyName("external")]
    [Member(Index = 6, Members = new[]
    {
      "FileInstruction",
      "NumericReturnCode",
      "TextReturnCode",
      "TextLine80"
    })]
    public External External
    {
      get => external ??= new();
      set => external = value;
    }

    private Case1 case1;
    private ServiceProvider serviceProvider;
    private Office office;
    private CseOrganization region;
    private EabReportSend eabReportSend;
    private External external;
  }
#endregion
}
