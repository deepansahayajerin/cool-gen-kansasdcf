// Program: FN_ASSIGN_DISB_TRAN_TYPE_ID, ID: 371837928, model: 746.
// Short name: SWE00280
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: FN_ASSIGN_DISB_TRAN_TYPE_ID.
/// </para>
/// <para>
/// RESP: FINANCE
/// </para>
/// </summary>
[Serializable]
public partial class FnAssignDisbTranTypeId: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_ASSIGN_DISB_TRAN_TYPE_ID program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnAssignDisbTranTypeId(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnAssignDisbTranTypeId.
  /// </summary>
  public FnAssignDisbTranTypeId(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    if (ReadDisbursementTransactionType())
    {
      local.DisbursementTransactionType.SystemGeneratedIdentifier =
        entities.DisbursementTransactionType.SystemGeneratedIdentifier;
    }

    export.DisbursementTransactionType.SystemGeneratedIdentifier =
      local.DisbursementTransactionType.SystemGeneratedIdentifier + 1;
  }

  private bool ReadDisbursementTransactionType()
  {
    entities.DisbursementTransactionType.Populated = false;

    return Read("ReadDisbursementTransactionType",
      null,
      (db, reader) =>
      {
        entities.DisbursementTransactionType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.DisbursementTransactionType.Populated = true;
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
    /// A value of DisbursementTransactionType.
    /// </summary>
    [JsonPropertyName("disbursementTransactionType")]
    public DisbursementTransactionType DisbursementTransactionType
    {
      get => disbursementTransactionType ??= new();
      set => disbursementTransactionType = value;
    }

    private DisbursementTransactionType disbursementTransactionType;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of DisbursementTransactionType.
    /// </summary>
    [JsonPropertyName("disbursementTransactionType")]
    public DisbursementTransactionType DisbursementTransactionType
    {
      get => disbursementTransactionType ??= new();
      set => disbursementTransactionType = value;
    }

    private DisbursementTransactionType disbursementTransactionType;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of DisbursementTransactionType.
    /// </summary>
    [JsonPropertyName("disbursementTransactionType")]
    public DisbursementTransactionType DisbursementTransactionType
    {
      get => disbursementTransactionType ??= new();
      set => disbursementTransactionType = value;
    }

    private DisbursementTransactionType disbursementTransactionType;
  }
#endregion
}
