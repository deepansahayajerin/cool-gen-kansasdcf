// Program: FN_CAB_SET_ACCRUAL_SUSPENSION_ID, ID: 372083175, model: 746.
// Short name: SWE00298
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_CAB_SET_ACCRUAL_SUSPENSION_ID.
/// </summary>
[Serializable]
public partial class FnCabSetAccrualSuspensionId: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_CAB_SET_ACCRUAL_SUSPENSION_ID program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnCabSetAccrualSuspensionId(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnCabSetAccrualSuspensionId.
  /// </summary>
  public FnCabSetAccrualSuspensionId(IContext context, Import import,
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
    if (ReadAccrualSuspension())
    {
      local.AccrualSuspension.SystemGeneratedIdentifier =
        entities.AccrualSuspension.SystemGeneratedIdentifier;
    }

    export.AccrualSuspension.SystemGeneratedIdentifier =
      local.AccrualSuspension.SystemGeneratedIdentifier + 1;
  }

  private bool ReadAccrualSuspension()
  {
    entities.AccrualSuspension.Populated = false;

    return Read("ReadAccrualSuspension",
      null,
      (db, reader) =>
      {
        entities.AccrualSuspension.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.AccrualSuspension.OtrType = db.GetString(reader, 1);
        entities.AccrualSuspension.OtyId = db.GetInt32(reader, 2);
        entities.AccrualSuspension.ObgId = db.GetInt32(reader, 3);
        entities.AccrualSuspension.CspNumber = db.GetString(reader, 4);
        entities.AccrualSuspension.CpaType = db.GetString(reader, 5);
        entities.AccrualSuspension.OtrId = db.GetInt32(reader, 6);
        entities.AccrualSuspension.Populated = true;
        CheckValid<AccrualSuspension>("OtrType",
          entities.AccrualSuspension.OtrType);
        CheckValid<AccrualSuspension>("CpaType",
          entities.AccrualSuspension.CpaType);
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
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of AccrualSuspension.
    /// </summary>
    [JsonPropertyName("accrualSuspension")]
    public AccrualSuspension AccrualSuspension
    {
      get => accrualSuspension ??= new();
      set => accrualSuspension = value;
    }

    private AccrualSuspension accrualSuspension;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of AccrualSuspension.
    /// </summary>
    [JsonPropertyName("accrualSuspension")]
    public AccrualSuspension AccrualSuspension
    {
      get => accrualSuspension ??= new();
      set => accrualSuspension = value;
    }

    private AccrualSuspension accrualSuspension;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of AccrualSuspension.
    /// </summary>
    [JsonPropertyName("accrualSuspension")]
    public AccrualSuspension AccrualSuspension
    {
      get => accrualSuspension ??= new();
      set => accrualSuspension = value;
    }

    private AccrualSuspension accrualSuspension;
  }
#endregion
}
