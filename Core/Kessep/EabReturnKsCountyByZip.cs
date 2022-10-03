// Program: EAB_RETURN_KS_COUNTY_BY_ZIP, ID: 371727804, model: 746.
// Short name: SWEXIR30
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: EAB_RETURN_KS_COUNTY_BY_ZIP.
/// </summary>
[Serializable]
public partial class EabReturnKsCountyByZip: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the EAB_RETURN_KS_COUNTY_BY_ZIP program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new EabReturnKsCountyByZip(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of EabReturnKsCountyByZip.
  /// </summary>
  public EabReturnKsCountyByZip(IContext context, Import import, Export export):
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
      "SWEXIR30", context, import, export, EabOptions.NoIefParams);
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
    /// A value of CsePersonAddress.
    /// </summary>
    [JsonPropertyName("csePersonAddress")]
    [Member(Index = 1, Members = new[] { "LocationType", "ZipCode" })]
    public CsePersonAddress CsePersonAddress
    {
      get => csePersonAddress ??= new();
      set => csePersonAddress = value;
    }

    private CsePersonAddress csePersonAddress;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of CsePersonAddress.
    /// </summary>
    [JsonPropertyName("csePersonAddress")]
    [Member(Index = 1, Members = new[]
    {
      "LocationType",
      "County",
      "ZipCode"
    })]
    public CsePersonAddress CsePersonAddress
    {
      get => csePersonAddress ??= new();
      set => csePersonAddress = value;
    }

    private CsePersonAddress csePersonAddress;
  }
#endregion
}
