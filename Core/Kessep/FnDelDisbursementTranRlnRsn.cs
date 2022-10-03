// Program: FN_DEL_DISBURSEMENT_TRAN_RLN_RSN, ID: 371834982, model: 746.
// Short name: SWE00391
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: FN_DEL_DISBURSEMENT_TRAN_RLN_RSN.
/// </para>
/// <para>
/// RESP: FINCLMNGMNT
/// </para>
/// </summary>
[Serializable]
public partial class FnDelDisbursementTranRlnRsn: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_DEL_DISBURSEMENT_TRAN_RLN_RSN program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnDelDisbursementTranRlnRsn(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnDelDisbursementTranRlnRsn.
  /// </summary>
  public FnDelDisbursementTranRlnRsn(IContext context, Import import,
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
    ExitState = "ACO_NN0000_ALL_OK";

    // ***** EDIT AREA *****
    if (ReadDisbursementTranRlnRsn())
    {
      // ***** MAIN-LINE AREA *****
      // ***** All changes to a Type or Status entity must be audited.  
      // LOG_TYPE_OR_STATUS_AUDIT will perform this process for any of these
      // entities.
      export.TypeStatusAudit.TableName = "DISBURSEMENT TRAN RLN RSN";
      export.TypeStatusAudit.SystemGeneratedIdentifier =
        entities.DisbursementTranRlnRsn.SystemGeneratedIdentifier;
      export.TypeStatusAudit.Code = entities.DisbursementTranRlnRsn.Code;
      export.TypeStatusAudit.Description =
        entities.DisbursementTranRlnRsn.Description;
      export.TypeStatusAudit.EffectiveDate =
        entities.DisbursementTranRlnRsn.EffectiveDate;
      export.TypeStatusAudit.DiscontinueDate =
        entities.DisbursementTranRlnRsn.DiscontinueDate;
      export.TypeStatusAudit.CreatedBy =
        entities.DisbursementTranRlnRsn.CreatedBy;
      export.TypeStatusAudit.CreatedTimestamp =
        entities.DisbursementTranRlnRsn.CreatedTimestamp;
      export.TypeStatusAudit.LastUpdatedBy =
        entities.DisbursementTranRlnRsn.LastUpdatedBy;
      export.TypeStatusAudit.LastUpdatedTmst =
        entities.DisbursementTranRlnRsn.LastUpdatedTmst;
      UseCabLogTypeOrStatusAudit();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }

      DeleteDisbursementTranRlnRsn();
    }
    else
    {
      ExitState = "FN0000_DISB_TRANS_RLN_RSN_NF";
    }
  }

  private static void MoveTypeStatusAudit(TypeStatusAudit source,
    TypeStatusAudit target)
  {
    target.StringOfOthers = source.StringOfOthers;
    target.TableName = source.TableName;
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Code = source.Code;
    target.Description = source.Description;
    target.EffectiveDate = source.EffectiveDate;
    target.DiscontinueDate = source.DiscontinueDate;
    target.CreatedBy = source.CreatedBy;
    target.CreatedTimestamp = source.CreatedTimestamp;
    target.LastUpdatedBy = source.LastUpdatedBy;
    target.LastUpdatedTmst = source.LastUpdatedTmst;
  }

  private void UseCabLogTypeOrStatusAudit()
  {
    var useImport = new CabLogTypeOrStatusAudit.Import();
    var useExport = new CabLogTypeOrStatusAudit.Export();

    MoveTypeStatusAudit(export.TypeStatusAudit, useImport.TypeStatusAudit);

    Call(CabLogTypeOrStatusAudit.Execute, useImport, useExport);
  }

  private void DeleteDisbursementTranRlnRsn()
  {
    Update("DeleteDisbursementTranRlnRsn",
      (db, command) =>
      {
        db.SetInt32(
          command, "disbTrnRlnRsId",
          entities.DisbursementTranRlnRsn.SystemGeneratedIdentifier);
      });
  }

  private bool ReadDisbursementTranRlnRsn()
  {
    entities.DisbursementTranRlnRsn.Populated = false;

    return Read("ReadDisbursementTranRlnRsn",
      (db, command) =>
      {
        db.SetString(command, "code", import.DisbursementTranRlnRsn.Code);
        db.SetDate(
          command, "effectiveDate",
          import.DisbursementTranRlnRsn.EffectiveDate.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.DisbursementTranRlnRsn.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.DisbursementTranRlnRsn.Code = db.GetString(reader, 1);
        entities.DisbursementTranRlnRsn.EffectiveDate = db.GetDate(reader, 2);
        entities.DisbursementTranRlnRsn.DiscontinueDate =
          db.GetNullableDate(reader, 3);
        entities.DisbursementTranRlnRsn.CreatedBy = db.GetString(reader, 4);
        entities.DisbursementTranRlnRsn.CreatedTimestamp =
          db.GetDateTime(reader, 5);
        entities.DisbursementTranRlnRsn.LastUpdatedBy =
          db.GetNullableString(reader, 6);
        entities.DisbursementTranRlnRsn.LastUpdatedTmst =
          db.GetNullableDateTime(reader, 7);
        entities.DisbursementTranRlnRsn.Description =
          db.GetNullableString(reader, 8);
        entities.DisbursementTranRlnRsn.Populated = true;
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
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of TypeStatusAudit.
    /// </summary>
    [JsonPropertyName("typeStatusAudit")]
    public TypeStatusAudit TypeStatusAudit
    {
      get => typeStatusAudit ??= new();
      set => typeStatusAudit = value;
    }

    private TypeStatusAudit typeStatusAudit;
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
