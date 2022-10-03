// Program: FN_ASSIGN_DISB_SUPP_ID_V2, ID: 372544331, model: 746.
// Short name: SWE02382
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: FN_ASSIGN_DISB_SUPP_ID_V2.
/// </para>
/// <para>
/// Same as fn_assign_disb_supp_id but without the persistent view.
/// </para>
/// </summary>
[Serializable]
public partial class FnAssignDisbSuppIdV2: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_ASSIGN_DISB_SUPP_ID_V2 program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnAssignDisbSuppIdV2(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnAssignDisbSuppIdV2.
  /// </summary>
  public FnAssignDisbSuppIdV2(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ---------------------------------------------
    // Date	By		Description
    // ---------------------------------------------
    // 9/16/99	Fangman		Changed qualification of Read Each to qualify by CSE 
    // Person #  (PR 74316)
    // ---------------------------------------------
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
    entities.DisbSuppressionStatusHistory.Populated = false;

    return Read("ReadDisbSuppressionStatusHistory",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", import.Obligee.Number);
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
    /// A value of Obligee.
    /// </summary>
    [JsonPropertyName("obligee")]
    public CsePerson Obligee
    {
      get => obligee ??= new();
      set => obligee = value;
    }

    private CsePerson obligee;
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
    /// A value of Obligee1.
    /// </summary>
    [JsonPropertyName("obligee1")]
    public CsePerson Obligee1
    {
      get => obligee1 ??= new();
      set => obligee1 = value;
    }

    /// <summary>
    /// A value of Obligee2.
    /// </summary>
    [JsonPropertyName("obligee2")]
    public CsePersonAccount Obligee2
    {
      get => obligee2 ??= new();
      set => obligee2 = value;
    }

    /// <summary>
    /// A value of DisbSuppressionStatusHistory.
    /// </summary>
    [JsonPropertyName("disbSuppressionStatusHistory")]
    public DisbSuppressionStatusHistory DisbSuppressionStatusHistory
    {
      get => disbSuppressionStatusHistory ??= new();
      set => disbSuppressionStatusHistory = value;
    }

    private CsePerson obligee1;
    private CsePersonAccount obligee2;
    private DisbSuppressionStatusHistory disbSuppressionStatusHistory;
  }
#endregion
}
