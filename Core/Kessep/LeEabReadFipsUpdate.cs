// Program: LE_EAB_READ_FIPS_UPDATE, ID: 374349419, model: 746.
// Short name: SWEXLR01
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Worksets;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: LE_EAB_READ_FIPS_UPDATE.
/// </summary>
[Serializable]
public partial class LeEabReadFipsUpdate: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the LE_EAB_READ_FIPS_UPDATE program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new LeEabReadFipsUpdate(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of LeEabReadFipsUpdate.
  /// </summary>
  public LeEabReadFipsUpdate(IContext context, Import import, Export export):
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
      "SWEXLR01", context, import, export, EabOptions.Hpvp);
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
    /// A value of EabReportSend.
    /// </summary>
    [JsonPropertyName("eabReportSend")]
    [Member(Index = 2, AccessFields = false, Members = new[] { "ReportNumber" })]
      
    public EabReportSend EabReportSend
    {
      get => eabReportSend ??= new();
      set => eabReportSend = value;
    }

    private EabFileHandling eabFileHandling;
    private EabReportSend eabReportSend;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of LeAutoFipsUpdate.
    /// </summary>
    [JsonPropertyName("leAutoFipsUpdate")]
    [Member(Index = 1, AccessFields = false, Members = new[]
    {
      "AddressType1",
      "AddressType2",
      "StateCode",
      "LocalCode",
      "SubLocalCode",
      "DepartmentName",
      "Title",
      "Street1",
      "Street2",
      "City",
      "StateOrCountry",
      "ZipCode",
      "AreaCode",
      "PhoneNumber",
      "Extension",
      "ActionCode",
      "FaxAreaCode",
      "FaxNumber",
      "RecordDate"
    })]
    public LeAutoFipsUpdate LeAutoFipsUpdate
    {
      get => leAutoFipsUpdate ??= new();
      set => leAutoFipsUpdate = value;
    }

    /// <summary>
    /// A value of EabFileHandling.
    /// </summary>
    [JsonPropertyName("eabFileHandling")]
    [Member(Index = 2, AccessFields = false, Members = new[] { "Status" })]
    public EabFileHandling EabFileHandling
    {
      get => eabFileHandling ??= new();
      set => eabFileHandling = value;
    }

    private LeAutoFipsUpdate leAutoFipsUpdate;
    private EabFileHandling eabFileHandling;
  }
#endregion
}
