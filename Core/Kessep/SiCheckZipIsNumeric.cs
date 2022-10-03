// Program: SI_CHECK_ZIP_IS_NUMERIC, ID: 371429474, model: 746.
// Short name: SWE01928
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SI_CHECK_ZIP_IS_NUMERIC.
/// </summary>
[Serializable]
public partial class SiCheckZipIsNumeric: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_CHECK_ZIP_IS_NUMERIC program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiCheckZipIsNumeric(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiCheckZipIsNumeric.
  /// </summary>
  public SiCheckZipIsNumeric(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    do
    {
      ++local.CheckZip.Count;
      local.CheckZip.Flag =
        Substring(import.CsePersonAddress.ZipCode, local.CheckZip.Count, 1);

      if (AsChar(local.CheckZip.Flag) < '0' || AsChar(local.CheckZip.Flag) > '9'
        )
      {
        export.NumericZip.Flag = "N";
        ExitState = "ZD_CO0000_ZIP_CODE_NOT_NUMERIC_2";

        return;
      }
    }
    while(local.CheckZip.Count != 5);
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
    /// A value of CsePersonAddress.
    /// </summary>
    [JsonPropertyName("csePersonAddress")]
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
    /// A value of NumericZip.
    /// </summary>
    [JsonPropertyName("numericZip")]
    public Common NumericZip
    {
      get => numericZip ??= new();
      set => numericZip = value;
    }

    private Common numericZip;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of CheckZip.
    /// </summary>
    [JsonPropertyName("checkZip")]
    public Common CheckZip
    {
      get => checkZip ??= new();
      set => checkZip = value;
    }

    private Common checkZip;
  }
#endregion
}
