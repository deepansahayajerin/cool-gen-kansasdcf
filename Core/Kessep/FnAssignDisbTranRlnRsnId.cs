// Program: FN_ASSIGN_DISB_TRAN_RLN_RSN_ID, ID: 371835396, model: 746.
// Short name: SWE00279
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: FN_ASSIGN_DISB_TRAN_RLN_RSN_ID.
/// </para>
/// <para>
/// RESP: FINANCE
/// </para>
/// </summary>
[Serializable]
public partial class FnAssignDisbTranRlnRsnId: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_ASSIGN_DISB_TRAN_RLN_RSN_ID program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnAssignDisbTranRlnRsnId(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnAssignDisbTranRlnRsnId.
  /// </summary>
  public FnAssignDisbTranRlnRsnId(IContext context, Import import, Export export)
    :
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    if (ReadDisbursementTranRlnRsn())
    {
      local.DisbursementTranRlnRsn.SystemGeneratedIdentifier =
        entities.DisbursementTranRlnRsn.SystemGeneratedIdentifier;
    }

    export.DisbursementTranRlnRsn.SystemGeneratedIdentifier =
      local.DisbursementTranRlnRsn.SystemGeneratedIdentifier + 1;
  }

  private bool ReadDisbursementTranRlnRsn()
  {
    entities.DisbursementTranRlnRsn.Populated = false;

    return Read("ReadDisbursementTranRlnRsn",
      null,
      (db, reader) =>
      {
        entities.DisbursementTranRlnRsn.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.DisbursementTranRlnRsn.Populated = true;
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
    /// A value of DisbursementTranRlnRsn.
    /// </summary>
    [JsonPropertyName("disbursementTranRlnRsn")]
    public DisbursementTranRlnRsn DisbursementTranRlnRsn
    {
      get => disbursementTranRlnRsn ??= new();
      set => disbursementTranRlnRsn = value;
    }

    private DisbursementTranRlnRsn disbursementTranRlnRsn;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of DisbursementTranRlnRsn.
    /// </summary>
    [JsonPropertyName("disbursementTranRlnRsn")]
    public DisbursementTranRlnRsn DisbursementTranRlnRsn
    {
      get => disbursementTranRlnRsn ??= new();
      set => disbursementTranRlnRsn = value;
    }

    private DisbursementTranRlnRsn disbursementTranRlnRsn;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of DisbursementTranRlnRsn.
    /// </summary>
    [JsonPropertyName("disbursementTranRlnRsn")]
    public DisbursementTranRlnRsn DisbursementTranRlnRsn
    {
      get => disbursementTranRlnRsn ??= new();
      set => disbursementTranRlnRsn = value;
    }

    private DisbursementTranRlnRsn disbursementTranRlnRsn;
  }
#endregion
}
