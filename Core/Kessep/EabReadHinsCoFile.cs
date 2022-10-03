// Program: EAB_READ_HINS_CO_FILE, ID: 372868987, model: 746.
// Short name: SWEXEE27
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: EAB_READ_HINS_CO_FILE.
/// </summary>
[Serializable]
public partial class EabReadHinsCoFile: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the EAB_READ_HINS_CO_FILE program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new EabReadHinsCoFile(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of EabReadHinsCoFile.
  /// </summary>
  public EabReadHinsCoFile(IContext context, Import import, Export export):
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
      "SWEXEE27", context, import, export, EabOptions.Hpvp);
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
    /// A value of HealthInsuranceCompany.
    /// </summary>
    [JsonPropertyName("healthInsuranceCompany")]
    [Member(Index = 1, AccessFields = false, Members = new[]
    {
      "InsurerPhoneAreaCode",
      "InsurerFaxAreaCode",
      "InsurerFaxExt",
      "InsurerPhoneExt",
      "CarrierCode",
      "InsurancePolicyCarrier",
      "ContactName",
      "InsurerPhone",
      "InsurerFax"
    })]
    public HealthInsuranceCompany HealthInsuranceCompany
    {
      get => healthInsuranceCompany ??= new();
      set => healthInsuranceCompany = value;
    }

    /// <summary>
    /// A value of HealthInsuranceCompanyAddress.
    /// </summary>
    [JsonPropertyName("healthInsuranceCompanyAddress")]
    [Member(Index = 2, AccessFields = false, Members = new[]
    {
      "Street1",
      "Street2",
      "City",
      "State",
      "ZipCode5",
      "ZipCode4",
      "Zip3"
    })]
    public HealthInsuranceCompanyAddress HealthInsuranceCompanyAddress
    {
      get => healthInsuranceCompanyAddress ??= new();
      set => healthInsuranceCompanyAddress = value;
    }

    /// <summary>
    /// A value of FileHeader.
    /// </summary>
    [JsonPropertyName("fileHeader")]
    [Member(Index = 3, AccessFields = false, Members = new[] { "Date" })]
    public DateWorkArea FileHeader
    {
      get => fileHeader ??= new();
      set => fileHeader = value;
    }

    /// <summary>
    /// A value of TotalRecords.
    /// </summary>
    [JsonPropertyName("totalRecords")]
    [Member(Index = 4, AccessFields = false, Members = new[] { "Count" })]
    public Common TotalRecords
    {
      get => totalRecords ??= new();
      set => totalRecords = value;
    }

    /// <summary>
    /// A value of EabFileHandling.
    /// </summary>
    [JsonPropertyName("eabFileHandling")]
    [Member(Index = 5, AccessFields = false, Members = new[] { "Status" })]
    public EabFileHandling EabFileHandling
    {
      get => eabFileHandling ??= new();
      set => eabFileHandling = value;
    }

    private HealthInsuranceCompany healthInsuranceCompany;
    private HealthInsuranceCompanyAddress healthInsuranceCompanyAddress;
    private DateWorkArea fileHeader;
    private Common totalRecords;
    private EabFileHandling eabFileHandling;
  }
#endregion
}
