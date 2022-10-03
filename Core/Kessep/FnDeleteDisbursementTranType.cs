// Program: FN_DELETE_DISBURSEMENT_TRAN_TYPE, ID: 371837537, model: 746.
// Short name: SWE00409
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: FN_DELETE_DISBURSEMENT_TRAN_TYPE.
/// </para>
/// <para>
/// RESP: FINANCE
/// </para>
/// </summary>
[Serializable]
public partial class FnDeleteDisbursementTranType: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_DELETE_DISBURSEMENT_TRAN_TYPE program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnDeleteDisbursementTranType(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnDeleteDisbursementTranType.
  /// </summary>
  public FnDeleteDisbursementTranType(IContext context, Import import,
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
    if (ReadDisbursementTransactionType())
    {
      // ***** MAIN-LINE AREA *****
      // ***** All changes to a Type or Status entity must be audited.  
      // LOG_TYPE_OR_STATUS_AUDIT will perform this process for any of these
      // entities.
      export.TypeStatusAudit.TableName = "disbursement tran type";
      export.TypeStatusAudit.SystemGeneratedIdentifier =
        entities.DisbursementTransactionType.SystemGeneratedIdentifier;
      export.TypeStatusAudit.Code = entities.DisbursementTransactionType.Code;
      export.TypeStatusAudit.Description =
        entities.DisbursementTransactionType.Description;
      export.TypeStatusAudit.EffectiveDate =
        entities.DisbursementTransactionType.EffectiveDate;
      export.TypeStatusAudit.DiscontinueDate =
        entities.DisbursementTransactionType.DiscontinueDate;
      export.TypeStatusAudit.CreatedBy =
        entities.DisbursementTransactionType.CreatedBy;
      export.TypeStatusAudit.CreatedTimestamp =
        entities.DisbursementTransactionType.CreatedTmst;
      export.TypeStatusAudit.LastUpdatedBy =
        entities.DisbursementTransactionType.LastUpdatedBy;
      export.TypeStatusAudit.LastUpdatedTmst =
        entities.DisbursementTransactionType.LastUpdatedTmst;
      UseCabLogTypeOrStatusAudit();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }

      DeleteDisbursementTransactionType();
    }
    else
    {
      ExitState = "FN0000_DISB_TRANS_TYP_NF";
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

  private void DeleteDisbursementTransactionType()
  {
    Update("DeleteDisbursementTransactionType",
      (db, command) =>
      {
        db.SetInt32(
          command, "disbTranTypeId",
          entities.DisbursementTransactionType.SystemGeneratedIdentifier);
      });
  }

  private bool ReadDisbursementTransactionType()
  {
    entities.DisbursementTransactionType.Populated = false;

    return Read("ReadDisbursementTransactionType",
      (db, command) =>
      {
        db.SetString(command, "code", import.DisbursementTransactionType.Code);
        db.SetDate(
          command, "effectiveDate",
          import.DisbursementTransactionType.EffectiveDate.GetValueOrDefault());
          
      },
      (db, reader) =>
      {
        entities.DisbursementTransactionType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.DisbursementTransactionType.Code = db.GetString(reader, 1);
        entities.DisbursementTransactionType.EffectiveDate =
          db.GetDate(reader, 2);
        entities.DisbursementTransactionType.DiscontinueDate =
          db.GetNullableDate(reader, 3);
        entities.DisbursementTransactionType.CreatedBy =
          db.GetString(reader, 4);
        entities.DisbursementTransactionType.CreatedTmst =
          db.GetDateTime(reader, 5);
        entities.DisbursementTransactionType.LastUpdatedBy =
          db.GetNullableString(reader, 6);
        entities.DisbursementTransactionType.LastUpdatedTmst =
          db.GetNullableDateTime(reader, 7);
        entities.DisbursementTransactionType.Description =
          db.GetNullableString(reader, 8);
        entities.DisbursementTransactionType.Populated = true;
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
