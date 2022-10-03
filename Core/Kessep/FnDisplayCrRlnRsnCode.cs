// Program: FN_DISPLAY_CR_RLN_RSN_CODE, ID: 373529861, model: 746.
// Short name: SWE00127
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_DISPLAY_CR_RLN_RSN_CODE.
/// </summary>
[Serializable]
public partial class FnDisplayCrRlnRsnCode: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_DISPLAY_CR_RLN_RSN_CODE program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnDisplayCrRlnRsnCode(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnDisplayCrRlnRsnCode.
  /// </summary>
  public FnDisplayCrRlnRsnCode(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    if (ReadCashReceiptRlnRsn())
    {
      export.CashReceiptRlnRsn.Assign(entities.Eav);
    }
    else
    {
      export.CashReceiptRlnRsn.Code = import.CashReceiptRlnRsn.Code;
      ExitState = "FN0093_CASH_RCPT_RLN_RSN_NF";
    }
  }

  private bool ReadCashReceiptRlnRsn()
  {
    entities.Eav.Populated = false;

    return Read("ReadCashReceiptRlnRsn",
      (db, command) =>
      {
        db.SetString(command, "code", import.CashReceiptRlnRsn.Code);
      },
      (db, reader) =>
      {
        entities.Eav.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.Eav.Code = db.GetString(reader, 1);
        entities.Eav.Name = db.GetString(reader, 2);
        entities.Eav.EffectiveDate = db.GetDate(reader, 3);
        entities.Eav.DiscontinueDate = db.GetNullableDate(reader, 4);
        entities.Eav.CreatedBy = db.GetString(reader, 5);
        entities.Eav.CreatedTimestamp = db.GetDateTime(reader, 6);
        entities.Eav.LastUpdatedBy = db.GetNullableString(reader, 7);
        entities.Eav.LastUpdatedTmst = db.GetNullableDateTime(reader, 8);
        entities.Eav.Description = db.GetNullableString(reader, 9);
        entities.Eav.Populated = true;
      });
  }
#endregion

#region Parameters.
  protected readonly Import import;
  protected readonly Export export;
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
    /// A value of CashReceiptRlnRsn.
    /// </summary>
    [JsonPropertyName("cashReceiptRlnRsn")]
    public CashReceiptRlnRsn CashReceiptRlnRsn
    {
      get => cashReceiptRlnRsn ??= new();
      set => cashReceiptRlnRsn = value;
    }

    private CashReceiptRlnRsn cashReceiptRlnRsn;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of CashReceiptRlnRsn.
    /// </summary>
    [JsonPropertyName("cashReceiptRlnRsn")]
    public CashReceiptRlnRsn CashReceiptRlnRsn
    {
      get => cashReceiptRlnRsn ??= new();
      set => cashReceiptRlnRsn = value;
    }

    private CashReceiptRlnRsn cashReceiptRlnRsn;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of Eav.
    /// </summary>
    [JsonPropertyName("eav")]
    public CashReceiptRlnRsn Eav
    {
      get => eav ??= new();
      set => eav = value;
    }

    private CashReceiptRlnRsn eav;
  }
#endregion
}
