// Program: FN_DELETE_DISBURSEMENT_STATUS, ID: 371830093, model: 746.
// Short name: SWE00408
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_DELETE_DISBURSEMENT_STATUS.
/// </summary>
[Serializable]
public partial class FnDeleteDisbursementStatus: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_DELETE_DISBURSEMENT_STATUS program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnDeleteDisbursementStatus(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnDeleteDisbursementStatus.
  /// </summary>
  public FnDeleteDisbursementStatus(IContext context, Import import,
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
    export.DisbursementStatus.Assign(import.DisbursementStatus);

    if (ReadDisbursementStatus())
    {
      // ***** MAIN-LINE AREA *****
      // ***** All changes to a Type or Status entity must be audited.
      // LOG_TYPE_OR_STATUS_AUDIT will perform this process for any of these 
      // entities.
      export.TypeStatusAudit.TableName = "DISBURSEMENT_STATUS";
      export.TypeStatusAudit.SystemGeneratedIdentifier =
        entities.DisbursementStatus.SystemGeneratedIdentifier;
      export.TypeStatusAudit.Code = entities.DisbursementStatus.Code;
      export.TypeStatusAudit.Description =
        entities.DisbursementStatus.Description;
      export.TypeStatusAudit.EffectiveDate =
        entities.DisbursementStatus.EffectiveDate;
      export.TypeStatusAudit.DiscontinueDate =
        entities.DisbursementStatus.DiscontinueDate;
      export.TypeStatusAudit.CreatedBy = entities.DisbursementStatus.CreatedBy;
      export.TypeStatusAudit.CreatedTimestamp =
        entities.DisbursementStatus.CreatedTimestamp;
      export.TypeStatusAudit.LastUpdatedBy =
        entities.DisbursementStatus.LastUpdateBy;
      export.TypeStatusAudit.LastUpdatedTmst =
        entities.DisbursementStatus.LastUpdateTmst;
      UseCabLogTypeOrStatusAudit();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }

      DeleteDisbursementStatus();
    }
    else
    {
      ExitState = "FN0000_DISB_STAT_NF";
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

  private void DeleteDisbursementStatus()
  {
    Update("DeleteDisbursementStatus",
      (db, command) =>
      {
        db.SetInt32(
          command, "disbStatusId",
          entities.DisbursementStatus.SystemGeneratedIdentifier);
      });
  }

  private bool ReadDisbursementStatus()
  {
    entities.DisbursementStatus.Populated = false;

    return Read("ReadDisbursementStatus",
      (db, command) =>
      {
        db.SetString(command, "code", import.DisbursementStatus.Code);
        db.SetDate(
          command, "effectiveDate",
          import.DisbursementStatus.EffectiveDate.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.DisbursementStatus.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.DisbursementStatus.Code = db.GetString(reader, 1);
        entities.DisbursementStatus.EffectiveDate = db.GetDate(reader, 2);
        entities.DisbursementStatus.DiscontinueDate =
          db.GetNullableDate(reader, 3);
        entities.DisbursementStatus.CreatedBy = db.GetString(reader, 4);
        entities.DisbursementStatus.CreatedTimestamp =
          db.GetDateTime(reader, 5);
        entities.DisbursementStatus.LastUpdateBy =
          db.GetNullableString(reader, 6);
        entities.DisbursementStatus.LastUpdateTmst =
          db.GetNullableDateTime(reader, 7);
        entities.DisbursementStatus.Description =
          db.GetNullableString(reader, 8);
        entities.DisbursementStatus.Populated = true;
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

    /// <summary>
    /// A value of TypeStatusAudit.
    /// </summary>
    [JsonPropertyName("typeStatusAudit")]
    public TypeStatusAudit TypeStatusAudit
    {
      get => typeStatusAudit ??= new();
      set => typeStatusAudit = value;
    }

    private DisbursementStatus disbursementStatus;
    private TypeStatusAudit typeStatusAudit;
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
