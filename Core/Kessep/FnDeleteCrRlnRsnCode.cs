// Program: FN_DELETE_CR_RLN_RSN_CODE, ID: 373529864, model: 746.
// Short name: SWE00126
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_DELETE_CR_RLN_RSN_CODE.
/// </summary>
[Serializable]
public partial class FnDeleteCrRlnRsnCode: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_DELETE_CR_RLN_RSN_CODE program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnDeleteCrRlnRsnCode(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnDeleteCrRlnRsnCode.
  /// </summary>
  public FnDeleteCrRlnRsnCode(IContext context, Import import, Export export):
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
      if (Lt(Now().Date, entities.ToBeDeleted.EffectiveDate))
      {
        // ---> ok to delete future dated codes
      }
      else
      {
        // ---> ok to delete codes on same day they were created
        if (Lt(Date(entities.ToBeDeleted.CreatedTimestamp), Now().Date))
        {
          ExitState = "CANNOT_DELETE_EFFECTIVE_RECORD";

          return;
        }
      }

      // --->  do these code values need to have a TYPESTAT AUDIT entry?
      export.PassToCab.Code = entities.ToBeDeleted.Code;
      export.PassToCab.CreatedBy = entities.ToBeDeleted.CreatedBy;
      export.PassToCab.CreatedTimestamp = entities.ToBeDeleted.CreatedTimestamp;
      export.PassToCab.Description = entities.ToBeDeleted.Description;
      export.PassToCab.DiscontinueDate = entities.ToBeDeleted.DiscontinueDate;
      export.PassToCab.EffectiveDate = entities.ToBeDeleted.EffectiveDate;
      export.PassToCab.LastUpdatedBy = entities.ToBeDeleted.LastUpdatedBy;
      export.PassToCab.LastUpdatedTmst = entities.ToBeDeleted.LastUpdatedTmst;
      export.PassToCab.Name = entities.ToBeDeleted.Name;
      export.PassToCab.StringOfOthers = "DELETED";
      export.PassToCab.SystemGeneratedIdentifier =
        entities.ToBeDeleted.SystemGeneratedIdentifier;
      export.PassToCab.TableName = "CASH_RECEIPT_RLN_RSN";
      UseCabLogTypeOrStatusAudit();
      DeleteCashReceiptRlnRsn();
    }
    else
    {
      ExitState = "FN0093_CASH_RCPT_RLN_RSN_NF";
    }
  }

  private void UseCabLogTypeOrStatusAudit()
  {
    var useImport = new CabLogTypeOrStatusAudit.Import();
    var useExport = new CabLogTypeOrStatusAudit.Export();

    useImport.TypeStatusAudit.Assign(export.PassToCab);

    Call(CabLogTypeOrStatusAudit.Execute, useImport, useExport);
  }

  private void DeleteCashReceiptRlnRsn()
  {
    Update("DeleteCashReceiptRlnRsn",
      (db, command) =>
      {
        db.SetInt32(
          command, "crRlnRsnId",
          entities.ToBeDeleted.SystemGeneratedIdentifier);
      });
  }

  private bool ReadCashReceiptRlnRsn()
  {
    entities.ToBeDeleted.Populated = false;

    return Read("ReadCashReceiptRlnRsn",
      (db, command) =>
      {
        db.SetInt32(
          command, "crRlnRsnId",
          import.CashReceiptRlnRsn.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.ToBeDeleted.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.ToBeDeleted.Code = db.GetString(reader, 1);
        entities.ToBeDeleted.Name = db.GetString(reader, 2);
        entities.ToBeDeleted.EffectiveDate = db.GetDate(reader, 3);
        entities.ToBeDeleted.DiscontinueDate = db.GetNullableDate(reader, 4);
        entities.ToBeDeleted.CreatedBy = db.GetString(reader, 5);
        entities.ToBeDeleted.CreatedTimestamp = db.GetDateTime(reader, 6);
        entities.ToBeDeleted.LastUpdatedBy = db.GetNullableString(reader, 7);
        entities.ToBeDeleted.LastUpdatedTmst =
          db.GetNullableDateTime(reader, 8);
        entities.ToBeDeleted.Description = db.GetNullableString(reader, 9);
        entities.ToBeDeleted.Populated = true;
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
    /// A value of PassToCab.
    /// </summary>
    [JsonPropertyName("passToCab")]
    public TypeStatusAudit PassToCab
    {
      get => passToCab ??= new();
      set => passToCab = value;
    }

    private TypeStatusAudit passToCab;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of ToBeDeleted.
    /// </summary>
    [JsonPropertyName("toBeDeleted")]
    public CashReceiptRlnRsn ToBeDeleted
    {
      get => toBeDeleted ??= new();
      set => toBeDeleted = value;
    }

    private CashReceiptRlnRsn toBeDeleted;
  }
#endregion
}
