// Program: SI_B283_EAB_WRITE_INITIATIVE_FIL, ID: 1902519469, model: 746.
// Short name: SWEXIE03
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Worksets;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SI_B283_EAB_WRITE_INITIATIVE_FIL.
/// </summary>
[Serializable]
public partial class SiB283EabWriteInitiativeFil: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_B283_EAB_WRITE_INITIATIVE_FIL program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiB283EabWriteInitiativeFil(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiB283EabWriteInitiativeFil.
  /// </summary>
  public SiB283EabWriteInitiativeFil(IContext context, Import import,
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
      "SWEXIE03", context, import, export, EabOptions.Hpvp);
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
    /// A value of NewHireInitiativeRecord.
    /// </summary>
    [JsonPropertyName("newHireInitiativeRecord")]
    [Member(Index = 2, AccessFields = false, Members = new[] { "OutputRecord" })]
      
    public NewHireInitiativeRecord NewHireInitiativeRecord
    {
      get => newHireInitiativeRecord ??= new();
      set => newHireInitiativeRecord = value;
    }

    private EabFileHandling eabFileHandling;
    private NewHireInitiativeRecord newHireInitiativeRecord;
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
