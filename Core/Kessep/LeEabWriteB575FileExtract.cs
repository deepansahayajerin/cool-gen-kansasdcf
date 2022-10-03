// Program: LE_EAB_WRITE_B575_FILE_EXTRACT, ID: 371410852, model: 746.
// Short name: SWEXLW02
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: LE_EAB_WRITE_B575_FILE_EXTRACT.
/// </summary>
[Serializable]
public partial class LeEabWriteB575FileExtract: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the LE_EAB_WRITE_B575_FILE_EXTRACT program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new LeEabWriteB575FileExtract(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of LeEabWriteB575FileExtract.
  /// </summary>
  public LeEabWriteB575FileExtract(IContext context, Import import,
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
      "SWEXLW02", context, import, export, EabOptions.Hpvp);
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

    /// <summary>
    /// A value of RegionInfo.
    /// </summary>
    [JsonPropertyName("regionInfo")]
    [Member(Index = 2, AccessFields = false, Members = new[] { "Code", "Name" })]
      
    public CseOrganization RegionInfo
    {
      get => regionInfo ??= new();
      set => regionInfo = value;
    }

    /// <summary>
    /// A value of OfficeInfo.
    /// </summary>
    [JsonPropertyName("officeInfo")]
    [Member(Index = 3, AccessFields = false, Members
      = new[] { "SystemGeneratedId", "Name" })]
    public Office OfficeInfo
    {
      get => officeInfo ??= new();
      set => officeInfo = value;
    }

    /// <summary>
    /// A value of SpInfo.
    /// </summary>
    [JsonPropertyName("spInfo")]
    [Member(Index = 4, AccessFields = false, Members = new[]
    {
      "SystemGeneratedId",
      "LastName",
      "FirstName",
      "MiddleInitial"
    })]
    public ServiceProvider SpInfo
    {
      get => spInfo ??= new();
      set => spInfo = value;
    }

    /// <summary>
    /// A value of CoCaseloadCnt.
    /// </summary>
    [JsonPropertyName("coCaseloadCnt")]
    [Member(Index = 5, AccessFields = false, Members = new[] { "Count" })]
    public Common CoCaseloadCnt
    {
      get => coCaseloadCnt ??= new();
      set => coCaseloadCnt = value;
    }

    /// <summary>
    /// A value of CoReferredCnt.
    /// </summary>
    [JsonPropertyName("coReferredCnt")]
    [Member(Index = 6, AccessFields = false, Members = new[] { "Count" })]
    public Common CoReferredCnt
    {
      get => coReferredCnt ??= new();
      set => coReferredCnt = value;
    }

    private EabFileHandling eabFileHandling;
    private CseOrganization regionInfo;
    private Office officeInfo;
    private ServiceProvider spInfo;
    private Common coCaseloadCnt;
    private Common coReferredCnt;
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
