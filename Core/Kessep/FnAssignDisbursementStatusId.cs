// Program: FN_ASSIGN_DISBURSEMENT_STATUS_ID, ID: 371830543, model: 746.
// Short name: SWE00281
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: FN_ASSIGN_DISBURSEMENT_STATUS_ID.
/// </para>
/// <para>
/// RESP: FINANCE
/// </para>
/// </summary>
[Serializable]
public partial class FnAssignDisbursementStatusId: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_ASSIGN_DISBURSEMENT_STATUS_ID program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnAssignDisbursementStatusId(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnAssignDisbursementStatusId.
  /// </summary>
  public FnAssignDisbursementStatusId(IContext context, Import import,
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
    if (ReadDisbursementStatus())
    {
      local.DisbursementStatus.SystemGeneratedIdentifier =
        entities.DisbursementStatus.SystemGeneratedIdentifier;
    }

    export.DisbursementStatus.SystemGeneratedIdentifier =
      local.DisbursementStatus.SystemGeneratedIdentifier + 1;
  }

  private bool ReadDisbursementStatus()
  {
    entities.DisbursementStatus.Populated = false;

    return Read("ReadDisbursementStatus",
      null,
      (db, reader) =>
      {
        entities.DisbursementStatus.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.DisbursementStatus.Populated = true;
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
    /// A value of DisbursementStatus.
    /// </summary>
    [JsonPropertyName("disbursementStatus")]
    public DisbursementStatus DisbursementStatus
    {
      get => disbursementStatus ??= new();
      set => disbursementStatus = value;
    }

    private DisbursementStatus disbursementStatus;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of DisbursementStatus.
    /// </summary>
    [JsonPropertyName("disbursementStatus")]
    public DisbursementStatus DisbursementStatus
    {
      get => disbursementStatus ??= new();
      set => disbursementStatus = value;
    }

    private DisbursementStatus disbursementStatus;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of DisbursementStatus.
    /// </summary>
    [JsonPropertyName("disbursementStatus")]
    public DisbursementStatus DisbursementStatus
    {
      get => disbursementStatus ??= new();
      set => disbursementStatus = value;
    }

    private DisbursementStatus disbursementStatus;
  }
#endregion
}
