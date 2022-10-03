// Program: SP_B316_EAB_READ_INPUT_FILE, ID: 374563321, model: 746.
// Short name: SWEXPR05
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SP_B316_EAB_READ_INPUT_FILE.
/// </summary>
[Serializable]
public partial class SpB316EabReadInputFile: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_B316_EAB_READ_INPUT_FILE program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpB316EabReadInputFile(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpB316EabReadInputFile.
  /// </summary>
  public SpB316EabReadInputFile(IContext context, Import import, Export export):
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
      "SWEXPR05", context, import, export, EabOptions.Hpvp);
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
    /// A value of ToOffice.
    /// </summary>
    [JsonPropertyName("toOffice")]
    [Member(Index = 1, AccessFields = false, Members
      = new[] { "SystemGeneratedId" })]
    public Office ToOffice
    {
      get => toOffice ??= new();
      set => toOffice = value;
    }

    /// <summary>
    /// A value of FromOffice.
    /// </summary>
    [JsonPropertyName("fromOffice")]
    [Member(Index = 2, AccessFields = false, Members
      = new[] { "SystemGeneratedId" })]
    public Office FromOffice
    {
      get => fromOffice ??= new();
      set => fromOffice = value;
    }

    /// <summary>
    /// A value of ToServiceProvider.
    /// </summary>
    [JsonPropertyName("toServiceProvider")]
    [Member(Index = 3, AccessFields = false, Members
      = new[] { "SystemGeneratedId" })]
    public ServiceProvider ToServiceProvider
    {
      get => toServiceProvider ??= new();
      set => toServiceProvider = value;
    }

    /// <summary>
    /// A value of FromServiceProvider.
    /// </summary>
    [JsonPropertyName("fromServiceProvider")]
    [Member(Index = 4, AccessFields = false, Members
      = new[] { "SystemGeneratedId" })]
    public ServiceProvider FromServiceProvider
    {
      get => fromServiceProvider ??= new();
      set => fromServiceProvider = value;
    }

    /// <summary>
    /// A value of EabFileHandling.
    /// </summary>
    [JsonPropertyName("eabFileHandling")]
    [Member(Index = 5, AccessFields = false, Members
      = new[] { "Action", "Status" })]
    public EabFileHandling EabFileHandling
    {
      get => eabFileHandling ??= new();
      set => eabFileHandling = value;
    }

    /// <summary>
    /// A value of FromOfficeServiceProvider.
    /// </summary>
    [JsonPropertyName("fromOfficeServiceProvider")]
    [Member(Index = 6, AccessFields = false, Members = new[] { "RoleCode" })]
    public OfficeServiceProvider FromOfficeServiceProvider
    {
      get => fromOfficeServiceProvider ??= new();
      set => fromOfficeServiceProvider = value;
    }

    /// <summary>
    /// A value of ResultDetail.
    /// </summary>
    [JsonPropertyName("resultDetail")]
    [Member(Index = 7, AccessFields = false, Members = new[] { "TextLine130" })]
    public External ResultDetail
    {
      get => resultDetail ??= new();
      set => resultDetail = value;
    }

    /// <summary>
    /// A value of ToOfficeServiceProvider.
    /// </summary>
    [JsonPropertyName("toOfficeServiceProvider")]
    [Member(Index = 8, AccessFields = false, Members = new[] { "RoleCode" })]
    public OfficeServiceProvider ToOfficeServiceProvider
    {
      get => toOfficeServiceProvider ??= new();
      set => toOfficeServiceProvider = value;
    }

    /// <summary>
    /// A value of Tribunal.
    /// </summary>
    [JsonPropertyName("tribunal")]
    [Member(Index = 9, AccessFields = false, Members = new[] { "Identifier" })]
    public Tribunal Tribunal
    {
      get => tribunal ??= new();
      set => tribunal = value;
    }

    private Office toOffice;
    private Office fromOffice;
    private ServiceProvider toServiceProvider;
    private ServiceProvider fromServiceProvider;
    private EabFileHandling eabFileHandling;
    private OfficeServiceProvider fromOfficeServiceProvider;
    private External resultDetail;
    private OfficeServiceProvider toOfficeServiceProvider;
    private Tribunal tribunal;
  }
#endregion
}
