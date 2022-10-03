// Program: FN_CONVERT_CRSTC_TO_CO_ENT_DESCR, ID: 372414838, model: 746.
// Short name: SWE02438
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_CONVERT_CRSTC_TO_CO_ENT_DESCR.
/// </summary>
[Serializable]
public partial class FnConvertCrstcToCoEntDescr: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_CONVERT_CRSTC_TO_CO_ENT_DESCR program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnConvertCrstcToCoEntDescr(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnConvertCrstcToCoEntDescr.
  /// </summary>
  public FnConvertCrstcToCoEntDescr(IContext context, Import import,
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
    switch(TrimEnd(import.CashReceiptSourceType.Code))
    {
      case "EMP":
        export.ElectronicFundTransmission.CompanyEntryDescription = "EMPLOYER";

        return;
      case "FDSO":
        export.ElectronicFundTransmission.CompanyEntryDescription = "FDSO";

        return;
      case "MISC":
        export.ElectronicFundTransmission.CompanyEntryDescription =
          "INDIVIDUAL";

        return;
      default:
        break;
    }

    if (import.CashReceiptSourceType.State.GetValueOrDefault() == 20)
    {
      // This is a Kansas court interface FIPS so use the "KS" building the 
      // company entry description.
      local.Common.State = "KS";
    }
    else
    {
      // This is an Interstate FIPS so use the abbreviation part of the code in 
      // building the company entry description.
      local.Common.State = Substring(import.CashReceiptSourceType.Code, 1, 2);
    }

    export.ElectronicFundTransmission.CompanyEntryDescription =
      local.Common.State + NumberToString
      (import.CashReceiptSourceType.State.GetValueOrDefault(), 14, 2) + NumberToString
      (import.CashReceiptSourceType.County.GetValueOrDefault(), 13, 3) + NumberToString
      (import.CashReceiptSourceType.Location.GetValueOrDefault(), 14, 2);
  }
#endregion

#region Parameters.
  protected readonly Import import;
  protected readonly Export export;
  protected readonly Local local = new();
#endregion

#region Structures
  /// <summary>
  /// This class defines import view.
  /// </summary>
  [Serializable]
  public class Import
  {
    /// <summary>
    /// A value of CashReceiptSourceType.
    /// </summary>
    [JsonPropertyName("cashReceiptSourceType")]
    public CashReceiptSourceType CashReceiptSourceType
    {
      get => cashReceiptSourceType ??= new();
      set => cashReceiptSourceType = value;
    }

    private CashReceiptSourceType cashReceiptSourceType;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of ElectronicFundTransmission.
    /// </summary>
    [JsonPropertyName("electronicFundTransmission")]
    public ElectronicFundTransmission ElectronicFundTransmission
    {
      get => electronicFundTransmission ??= new();
      set => electronicFundTransmission = value;
    }

    private ElectronicFundTransmission electronicFundTransmission;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of Common.
    /// </summary>
    [JsonPropertyName("common")]
    public Common Common
    {
      get => common ??= new();
      set => common = value;
    }

    private Common common;
  }
#endregion
}
