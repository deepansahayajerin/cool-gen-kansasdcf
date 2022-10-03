// Program: FN_CREATE_CR_RLN_RSN_CODE, ID: 373529863, model: 746.
// Short name: SWE00125
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_CREATE_CR_RLN_RSN_CODE.
/// </summary>
[Serializable]
public partial class FnCreateCrRlnRsnCode: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_CREATE_CR_RLN_RSN_CODE program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnCreateCrRlnRsnCode(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnCreateCrRlnRsnCode.
  /// </summary>
  public FnCreateCrRlnRsnCode(IContext context, Import import, Export export):
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

    if (ReadCashReceiptRlnRsn1())
    {
      MoveCashReceiptRlnRsn(import.CashReceiptRlnRsn, export.CashReceiptRlnRsn);
      ExitState = "FN0092_CASH_RCPT_RLN_RSN_AE";

      return;
    }
    else
    {
      // --->  continue
    }

    if (ReadCashReceiptRlnRsn2())
    {
      local.CashReceiptRlnRsn.SystemGeneratedIdentifier =
        entities.New1.SystemGeneratedIdentifier + 1;
    }

    local.CashReceiptRlnRsn.CreatedBy = global.UserId;
    local.CashReceiptRlnRsn.CreatedTimestamp = Now();

    try
    {
      CreateCashReceiptRlnRsn();
      export.CashReceiptRlnRsn.Assign(entities.New1);
    }
    catch(Exception e)
    {
      switch(GetErrorCode(e))
      {
        case ErrorCode.AlreadyExists:
          MoveCashReceiptRlnRsn(import.CashReceiptRlnRsn,
            export.CashReceiptRlnRsn);
          ExitState = "FN0092_CASH_RCPT_RLN_RSN_AE";

          break;
        case ErrorCode.PermittedValueViolation:
          MoveCashReceiptRlnRsn(import.CashReceiptRlnRsn,
            export.CashReceiptRlnRsn);
          ExitState = "FN0094_CASH_RCPT_RLN_RSN_PV";

          break;
        case ErrorCode.DatabaseError:
          break;
        default:
          throw;
      }
    }
  }

  private static void MoveCashReceiptRlnRsn(CashReceiptRlnRsn source,
    CashReceiptRlnRsn target)
  {
    target.Code = source.Code;
    target.Name = source.Name;
    target.Description = source.Description;
    target.EffectiveDate = source.EffectiveDate;
    target.DiscontinueDate = source.DiscontinueDate;
  }

  private void CreateCashReceiptRlnRsn()
  {
    var systemGeneratedIdentifier =
      local.CashReceiptRlnRsn.SystemGeneratedIdentifier;
    var code = import.CashReceiptRlnRsn.Code;
    var name = import.CashReceiptRlnRsn.Name;
    var effectiveDate = import.CashReceiptRlnRsn.EffectiveDate;
    var createdBy = local.CashReceiptRlnRsn.CreatedBy;
    var createdTimestamp = local.CashReceiptRlnRsn.CreatedTimestamp;
    var description = import.CashReceiptRlnRsn.Description ?? "";

    entities.New1.Populated = false;
    Update("CreateCashReceiptRlnRsn",
      (db, command) =>
      {
        db.SetInt32(command, "crRlnRsnId", systemGeneratedIdentifier);
        db.SetString(command, "code", code);
        db.SetString(command, "name", name);
        db.SetDate(command, "effectiveDate", effectiveDate);
        db.SetNullableDate(command, "discontinueDate", null);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetNullableString(command, "lastUpdatedBy", "");
        db.SetNullableDateTime(command, "lastUpdatedTmst", default(DateTime));
        db.SetNullableString(command, "description", description);
      });

    entities.New1.SystemGeneratedIdentifier = systemGeneratedIdentifier;
    entities.New1.Code = code;
    entities.New1.Name = name;
    entities.New1.EffectiveDate = effectiveDate;
    entities.New1.DiscontinueDate = null;
    entities.New1.CreatedBy = createdBy;
    entities.New1.CreatedTimestamp = createdTimestamp;
    entities.New1.Description = description;
    entities.New1.Populated = true;
  }

  private bool ReadCashReceiptRlnRsn1()
  {
    entities.New1.Populated = false;

    return Read("ReadCashReceiptRlnRsn1",
      (db, command) =>
      {
        db.SetString(command, "code", import.CashReceiptRlnRsn.Code);
      },
      (db, reader) =>
      {
        entities.New1.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.New1.Code = db.GetString(reader, 1);
        entities.New1.Name = db.GetString(reader, 2);
        entities.New1.EffectiveDate = db.GetDate(reader, 3);
        entities.New1.DiscontinueDate = db.GetNullableDate(reader, 4);
        entities.New1.CreatedBy = db.GetString(reader, 5);
        entities.New1.CreatedTimestamp = db.GetDateTime(reader, 6);
        entities.New1.Description = db.GetNullableString(reader, 7);
        entities.New1.Populated = true;
      });
  }

  private bool ReadCashReceiptRlnRsn2()
  {
    entities.New1.Populated = false;

    return Read("ReadCashReceiptRlnRsn2",
      null,
      (db, reader) =>
      {
        entities.New1.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.New1.Code = db.GetString(reader, 1);
        entities.New1.Name = db.GetString(reader, 2);
        entities.New1.EffectiveDate = db.GetDate(reader, 3);
        entities.New1.DiscontinueDate = db.GetNullableDate(reader, 4);
        entities.New1.CreatedBy = db.GetString(reader, 5);
        entities.New1.CreatedTimestamp = db.GetDateTime(reader, 6);
        entities.New1.Description = db.GetNullableString(reader, 7);
        entities.New1.Populated = true;
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
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
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
    /// A value of New1.
    /// </summary>
    [JsonPropertyName("new1")]
    public CashReceiptRlnRsn New1
    {
      get => new1 ??= new();
      set => new1 = value;
    }

    private CashReceiptRlnRsn new1;
  }
#endregion
}
