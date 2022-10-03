// Program: SI_CLOSE_ADABAS, ID: 372543906, model: 746.
// Short name: SWE02171
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: SI_CLOSE_ADABAS.
/// </para>
/// <para>
/// RESP: SERVINIT
/// This common action block closes the adabas files explicitly.
/// </para>
/// </summary>
[Serializable]
public partial class SiCloseAdabas: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_CLOSE_ADABAS program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiCloseAdabas(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiCloseAdabas.
  /// </summary>
  public SiCloseAdabas(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ---------------------------------------------------------------------------------------
    // Date	By	IDCR#	Description
    // 121697	govind		initial code
    // ---------------------------------------------------------------------------------------
    // ---------------------------------------------------------------------------------------
    // This action block closes the adabas files.
    // ---------------------------------------------------------------------------------------
    local.Work.Number = "CLOSE";
    UseEabReadCsePersonBatch();

    if (IsEmpty(export.AbendData.Type1))
    {
    }
    else
    {
      ExitState = "SI0000_ERROR_N_CLOSING_ADABAS_FL";
    }
  }

  private void UseEabReadCsePersonBatch()
  {
    var useImport = new EabReadCsePersonBatch.Import();
    var useExport = new EabReadCsePersonBatch.Export();

    useImport.CsePersonsWorkSet.Number = local.Work.Number;
    useExport.AbendData.Assign(export.AbendData);

    Call(EabReadCsePersonBatch.Execute, useImport, useExport);

    export.AbendData.Assign(useExport.AbendData);
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
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of AbendData.
    /// </summary>
    [JsonPropertyName("abendData")]
    public AbendData AbendData
    {
      get => abendData ??= new();
      set => abendData = value;
    }

    private AbendData abendData;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of Work.
    /// </summary>
    [JsonPropertyName("work")]
    public CsePersonsWorkSet Work
    {
      get => work ??= new();
      set => work = value;
    }

    private CsePersonsWorkSet work;
  }
#endregion
}
