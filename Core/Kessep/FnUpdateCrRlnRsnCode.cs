// Program: FN_UPDATE_CR_RLN_RSN_CODE, ID: 373529865, model: 746.
// Short name: SWE00128
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_UPDATE_CR_RLN_RSN_CODE.
/// </summary>
[Serializable]
public partial class FnUpdateCrRlnRsnCode: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_UPDATE_CR_RLN_RSN_CODE program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnUpdateCrRlnRsnCode(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnUpdateCrRlnRsnCode.
  /// </summary>
  public FnUpdateCrRlnRsnCode(IContext context, Import import, Export export):
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

    local.Current.Timestamp = Now();
    export.CashReceiptRlnRsn.Assign(import.CashReceiptRlnRsn);

    // -------------------------------------------------------------
    // Read the existing Cash Receipt Rln Rsn record.
    // -------------------------------------------------------------
    if (ReadCashReceiptRlnRsn2())
    {
      // ---> continue
    }
    else
    {
      ExitState = "FN0093_CASH_RCPT_RLN_RSN_NF";

      return;
    }

    // -------------------------------------------------------------
    // If the Cash Receipt Rln Rsn CODE has been changed,
    // determine if the new code already exists.
    // -------------------------------------------------------------
    if (!Equal(entities.Existing.Code, import.CashReceiptRlnRsn.Code))
    {
      if (ReadCashReceiptRlnRsn1())
      {
        ExitState = "FN0092_CASH_RCPT_RLN_RSN_AE";

        return;
      }
      else
      {
        // ---> ok to continue
      }
    }

    // -------------------------------------------------------------
    // Update the existing Cash Receipt Rln Rsn record.
    // -------------------------------------------------------------
    try
    {
      UpdateCashReceiptRlnRsn();
      export.CashReceiptRlnRsn.Assign(entities.Existing);

      // --->  do these code values need to have a TYPESTAT AUDIT entry?
      export.PassToCab.Code = entities.Existing.Code;
      export.PassToCab.CreatedBy = entities.Existing.CreatedBy;
      export.PassToCab.CreatedTimestamp = entities.Existing.CreatedTimestamp;
      export.PassToCab.Description = entities.Existing.Description;
      export.PassToCab.DiscontinueDate = entities.Existing.DiscontinueDate;
      export.PassToCab.EffectiveDate = entities.Existing.EffectiveDate;
      export.PassToCab.LastUpdatedBy = entities.Existing.LastUpdatedBy;
      export.PassToCab.LastUpdatedTmst = entities.Existing.LastUpdatedTmst;
      export.PassToCab.Name = entities.Existing.Name;
      export.PassToCab.StringOfOthers = "UPDATED";
      export.PassToCab.SystemGeneratedIdentifier =
        entities.Existing.SystemGeneratedIdentifier;
      export.PassToCab.TableName = "CASH_RECEIPT_RLN_RSN";
      UseCabLogTypeOrStatusAudit();
    }
    catch(Exception e)
    {
      switch(GetErrorCode(e))
      {
        case ErrorCode.AlreadyExists:
          ExitState = "FN0095_CASH_RCPT_RLN_RSN_NU_RB";

          break;
        case ErrorCode.PermittedValueViolation:
          ExitState = "FN0094_CASH_RCPT_RLN_RSN_PV";

          break;
        case ErrorCode.DatabaseError:
          break;
        default:
          throw;
      }
    }
  }

  private void UseCabLogTypeOrStatusAudit()
  {
    var useImport = new CabLogTypeOrStatusAudit.Import();
    var useExport = new CabLogTypeOrStatusAudit.Export();

    useImport.TypeStatusAudit.Assign(export.PassToCab);

    Call(CabLogTypeOrStatusAudit.Execute, useImport, useExport);
  }

  private bool ReadCashReceiptRlnRsn1()
  {
    entities.CheckDuplicate.Populated = false;

    return Read("ReadCashReceiptRlnRsn1",
      (db, command) =>
      {
        db.SetString(command, "code", import.CashReceiptRlnRsn.Code);
      },
      (db, reader) =>
      {
        entities.CheckDuplicate.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.CheckDuplicate.Code = db.GetString(reader, 1);
        entities.CheckDuplicate.Populated = true;
      });
  }

  private bool ReadCashReceiptRlnRsn2()
  {
    entities.Existing.Populated = false;

    return Read("ReadCashReceiptRlnRsn2",
      (db, command) =>
      {
        db.SetInt32(
          command, "crRlnRsnId",
          import.CashReceiptRlnRsn.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.Existing.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.Existing.Code = db.GetString(reader, 1);
        entities.Existing.Name = db.GetString(reader, 2);
        entities.Existing.EffectiveDate = db.GetDate(reader, 3);
        entities.Existing.DiscontinueDate = db.GetNullableDate(reader, 4);
        entities.Existing.CreatedBy = db.GetString(reader, 5);
        entities.Existing.CreatedTimestamp = db.GetDateTime(reader, 6);
        entities.Existing.LastUpdatedBy = db.GetNullableString(reader, 7);
        entities.Existing.LastUpdatedTmst = db.GetNullableDateTime(reader, 8);
        entities.Existing.Description = db.GetNullableString(reader, 9);
        entities.Existing.Populated = true;
      });
  }

  private void UpdateCashReceiptRlnRsn()
  {
    var code = import.CashReceiptRlnRsn.Code;
    var name = import.CashReceiptRlnRsn.Name;
    var effectiveDate = import.CashReceiptRlnRsn.EffectiveDate;
    var discontinueDate = import.CashReceiptRlnRsn.DiscontinueDate;
    var lastUpdatedBy = global.UserId;
    var lastUpdatedTmst = local.Current.Timestamp;
    var description = import.CashReceiptRlnRsn.Description ?? "";

    entities.Existing.Populated = false;
    Update("UpdateCashReceiptRlnRsn",
      (db, command) =>
      {
        db.SetString(command, "code", code);
        db.SetString(command, "name", name);
        db.SetDate(command, "effectiveDate", effectiveDate);
        db.SetNullableDate(command, "discontinueDate", discontinueDate);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTmst);
        db.SetNullableString(command, "description", description);
        db.SetInt32(
          command, "crRlnRsnId", entities.Existing.SystemGeneratedIdentifier);
      });

    entities.Existing.Code = code;
    entities.Existing.Name = name;
    entities.Existing.EffectiveDate = effectiveDate;
    entities.Existing.DiscontinueDate = discontinueDate;
    entities.Existing.LastUpdatedBy = lastUpdatedBy;
    entities.Existing.LastUpdatedTmst = lastUpdatedTmst;
    entities.Existing.Description = description;
    entities.Existing.Populated = true;
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

    /// <summary>
    /// A value of PassToCab.
    /// </summary>
    [JsonPropertyName("passToCab")]
    public TypeStatusAudit PassToCab
    {
      get => passToCab ??= new();
      set => passToCab = value;
    }

    private CashReceiptRlnRsn cashReceiptRlnRsn;
    private TypeStatusAudit passToCab;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of Current.
    /// </summary>
    [JsonPropertyName("current")]
    public DateWorkArea Current
    {
      get => current ??= new();
      set => current = value;
    }

    private DateWorkArea current;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of Existing.
    /// </summary>
    [JsonPropertyName("existing")]
    public CashReceiptRlnRsn Existing
    {
      get => existing ??= new();
      set => existing = value;
    }

    /// <summary>
    /// A value of CheckDuplicate.
    /// </summary>
    [JsonPropertyName("checkDuplicate")]
    public CashReceiptRlnRsn CheckDuplicate
    {
      get => checkDuplicate ??= new();
      set => checkDuplicate = value;
    }

    private CashReceiptRlnRsn existing;
    private CashReceiptRlnRsn checkDuplicate;
  }
#endregion
}
