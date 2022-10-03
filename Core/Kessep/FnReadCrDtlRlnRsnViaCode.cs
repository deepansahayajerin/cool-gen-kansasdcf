// Program: FN_READ_CR_DTL_RLN_RSN_VIA_CODE, ID: 372566724, model: 746.
// Short name: SWE00549
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: FN_READ_CR_DTL_RLN_RSN_VIA_CODE.
/// </para>
/// <para>
/// RESP:  FINANCE
/// DESC:  This action block reads the Cash Receipt Detail Relationship Reason 
/// via the Code.
/// </para>
/// </summary>
[Serializable]
public partial class FnReadCrDtlRlnRsnViaCode: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_READ_CR_DTL_RLN_RSN_VIA_CODE program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnReadCrDtlRlnRsnViaCode(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnReadCrDtlRlnRsnViaCode.
  /// </summary>
  public FnReadCrDtlRlnRsnViaCode(IContext context, Import import, Export export)
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
    // *****  If an "AS_OF" date is not imported, assume the current date.
    if (Equal(import.AsOf.Date, local.Initialized.Date))
    {
      local.AsOf.Date = Now().Date;
    }
    else
    {
      local.AsOf.Date = import.AsOf.Date;
    }

    // *****  Read the relationship reason.
    if (ReadCashReceiptDetailRlnRsn())
    {
      ++export.ImportNumberOfReads.Count;
      export.CashReceiptDetailRlnRsn.Assign(entities.CashReceiptDetailRlnRsn);
    }
    else
    {
      ExitState = "FN0059_CASH_RCPT_DTL_RLN_RSN_NF";
    }
  }

  private bool ReadCashReceiptDetailRlnRsn()
  {
    entities.CashReceiptDetailRlnRsn.Populated = false;

    return Read("ReadCashReceiptDetailRlnRsn",
      (db, command) =>
      {
        db.SetString(command, "code", import.CashReceiptDetailRlnRsn.Code);
        db.
          SetDate(command, "effectiveDate", local.AsOf.Date.GetValueOrDefault());
          
      },
      (db, reader) =>
      {
        entities.CashReceiptDetailRlnRsn.SequentialIdentifier =
          db.GetInt32(reader, 0);
        entities.CashReceiptDetailRlnRsn.Code = db.GetString(reader, 1);
        entities.CashReceiptDetailRlnRsn.Name = db.GetString(reader, 2);
        entities.CashReceiptDetailRlnRsn.EffectiveDate = db.GetDate(reader, 3);
        entities.CashReceiptDetailRlnRsn.DiscontinueDate =
          db.GetNullableDate(reader, 4);
        entities.CashReceiptDetailRlnRsn.CreatedBy = db.GetString(reader, 5);
        entities.CashReceiptDetailRlnRsn.CreatedTimestamp =
          db.GetDateTime(reader, 6);
        entities.CashReceiptDetailRlnRsn.LastUpdatedBy =
          db.GetNullableString(reader, 7);
        entities.CashReceiptDetailRlnRsn.LastUpdatedTmst =
          db.GetNullableDateTime(reader, 8);
        entities.CashReceiptDetailRlnRsn.Description =
          db.GetNullableString(reader, 9);
        entities.CashReceiptDetailRlnRsn.Populated = true;
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
    /// A value of AsOf.
    /// </summary>
    [JsonPropertyName("asOf")]
    public DateWorkArea AsOf
    {
      get => asOf ??= new();
      set => asOf = value;
    }

    /// <summary>
    /// A value of CashReceiptDetailRlnRsn.
    /// </summary>
    [JsonPropertyName("cashReceiptDetailRlnRsn")]
    public CashReceiptDetailRlnRsn CashReceiptDetailRlnRsn
    {
      get => cashReceiptDetailRlnRsn ??= new();
      set => cashReceiptDetailRlnRsn = value;
    }

    private DateWorkArea asOf;
    private CashReceiptDetailRlnRsn cashReceiptDetailRlnRsn;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of CashReceiptDetailRlnRsn.
    /// </summary>
    [JsonPropertyName("cashReceiptDetailRlnRsn")]
    public CashReceiptDetailRlnRsn CashReceiptDetailRlnRsn
    {
      get => cashReceiptDetailRlnRsn ??= new();
      set => cashReceiptDetailRlnRsn = value;
    }

    /// <summary>
    /// A value of ImportNumberOfReads.
    /// </summary>
    [JsonPropertyName("importNumberOfReads")]
    public Common ImportNumberOfReads
    {
      get => importNumberOfReads ??= new();
      set => importNumberOfReads = value;
    }

    private CashReceiptDetailRlnRsn cashReceiptDetailRlnRsn;
    private Common importNumberOfReads;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of Initialized.
    /// </summary>
    [JsonPropertyName("initialized")]
    public DateWorkArea Initialized
    {
      get => initialized ??= new();
      set => initialized = value;
    }

    /// <summary>
    /// A value of AsOf.
    /// </summary>
    [JsonPropertyName("asOf")]
    public DateWorkArea AsOf
    {
      get => asOf ??= new();
      set => asOf = value;
    }

    private DateWorkArea initialized;
    private DateWorkArea asOf;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of CashReceiptDetailRlnRsn.
    /// </summary>
    [JsonPropertyName("cashReceiptDetailRlnRsn")]
    public CashReceiptDetailRlnRsn CashReceiptDetailRlnRsn
    {
      get => cashReceiptDetailRlnRsn ??= new();
      set => cashReceiptDetailRlnRsn = value;
    }

    private CashReceiptDetailRlnRsn cashReceiptDetailRlnRsn;
  }
#endregion
}
