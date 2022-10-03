// Program: FN_ASSIGN_DISB_SUPPR_ID, ID: 371752084, model: 746.
// Short name: SWE00278
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: FN_ASSIGN_DISB_SUPPR_ID.
/// </para>
/// <para>
/// RESP: FINANCE
/// </para>
/// </summary>
[Serializable]
public partial class FnAssignDisbSupprId: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_ASSIGN_DISB_SUPPR_ID program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnAssignDisbSupprId(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnAssignDisbSupprId.
  /// </summary>
  public FnAssignDisbSupprId(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    if (ReadDisbSuppressionStatusHistory())
    {
      local.DisbSuppressionStatusHistory.SystemGeneratedIdentifier =
        entities.DisbSuppressionStatusHistory.SystemGeneratedIdentifier;
    }

    export.DisbSuppressionStatusHistory.SystemGeneratedIdentifier =
      local.DisbSuppressionStatusHistory.SystemGeneratedIdentifier + 1;
  }

  private bool ReadDisbSuppressionStatusHistory()
  {
    System.Diagnostics.Debug.Assert(import.Persistent.Populated);
    entities.DisbSuppressionStatusHistory.Populated = false;

    return Read("ReadDisbSuppressionStatusHistory",
      (db, command) =>
      {
        db.SetString(command, "cpaType", import.Persistent.Type1);
        db.SetString(command, "cspNumber", import.Persistent.CspNumber);
      },
      (db, reader) =>
      {
        entities.DisbSuppressionStatusHistory.CpaType = db.GetString(reader, 0);
        entities.DisbSuppressionStatusHistory.CspNumber =
          db.GetString(reader, 1);
        entities.DisbSuppressionStatusHistory.SystemGeneratedIdentifier =
          db.GetInt32(reader, 2);
        entities.DisbSuppressionStatusHistory.Populated = true;
        CheckValid<DisbSuppressionStatusHistory>("CpaType",
          entities.DisbSuppressionStatusHistory.CpaType);
      });
  }
#endregion

#region Parameters.
  protected readonly Import import;
  protected readonly Export export;
  protected readonly Local local = new();
  protected readonly Entities entities = new();
#endregion

#region Structures
  /// <summary>
  /// This class defines import view.
  /// </summary>
  [Serializable]
  public class Import
  {
    /// <summary>
    /// A value of Persistent.
    /// </summary>
    [JsonPropertyName("persistent")]
    public CsePersonAccount Persistent
    {
      get => persistent ??= new();
      set => persistent = value;
    }

    private CsePersonAccount persistent;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of DisbSuppressionStatusHistory.
    /// </summary>
    [JsonPropertyName("disbSuppressionStatusHistory")]
    public DisbSuppressionStatusHistory DisbSuppressionStatusHistory
    {
      get => disbSuppressionStatusHistory ??= new();
      set => disbSuppressionStatusHistory = value;
    }

    private DisbSuppressionStatusHistory disbSuppressionStatusHistory;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of DisbSuppressionStatusHistory.
    /// </summary>
    [JsonPropertyName("disbSuppressionStatusHistory")]
    public DisbSuppressionStatusHistory DisbSuppressionStatusHistory
    {
      get => disbSuppressionStatusHistory ??= new();
      set => disbSuppressionStatusHistory = value;
    }

    private DisbSuppressionStatusHistory disbSuppressionStatusHistory;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of DisbSuppressionStatusHistory.
    /// </summary>
    [JsonPropertyName("disbSuppressionStatusHistory")]
    public DisbSuppressionStatusHistory DisbSuppressionStatusHistory
    {
      get => disbSuppressionStatusHistory ??= new();
      set => disbSuppressionStatusHistory = value;
    }

    private DisbSuppressionStatusHistory disbSuppressionStatusHistory;
  }
#endregion
}
