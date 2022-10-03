// Program: FN_DELETE_DISBURSEMENT_TYPE, ID: 371831995, model: 746.
// Short name: SWE00410
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: FN_DELETE_DISBURSEMENT_TYPE.
/// </para>
/// <para>
/// RESP: FINANCE
/// </para>
/// </summary>
[Serializable]
public partial class FnDeleteDisbursementType: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_DELETE_DISBURSEMENT_TYPE program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnDeleteDisbursementType(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnDeleteDisbursementType.
  /// </summary>
  public FnDeleteDisbursementType(IContext context, Import import, Export export)
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
    // !!!!!!!!!!!
    // May want to add code to handle the logging of Disbursement Type specific 
    // attributes.
    // !!!!!!!!!!!!
    ExitState = "ACO_NN0000_ALL_OK";

    // ***** EDIT AREA *****
    if (ReadDisbursementType())
    {
      // ***** MAIN-LINE AREA *****
      // ***** All changes to a Type or Status entity must be audited.  
      // LOG_TYPE_OR_STATUS_AUDIT will perform this process for any of these
      // entities.
      export.TypeStatusAudit.TableName = "DISBURSEMENT_TYPE";
      export.TypeStatusAudit.SystemGeneratedIdentifier =
        entities.DisbursementType.SystemGeneratedIdentifier;
      export.TypeStatusAudit.Code = entities.DisbursementType.Code;
      export.TypeStatusAudit.Description =
        entities.DisbursementType.Description;
      export.TypeStatusAudit.EffectiveDate =
        entities.DisbursementType.EffectiveDate;
      export.TypeStatusAudit.DiscontinueDate =
        entities.DisbursementType.DiscontinueDate;
      export.TypeStatusAudit.CreatedBy = entities.DisbursementType.CreatedBy;
      export.TypeStatusAudit.CreatedTimestamp =
        entities.DisbursementType.CreatedTimestamp;
      export.TypeStatusAudit.LastUpdatedBy =
        entities.DisbursementType.LastUpdatedBy;
      export.TypeStatusAudit.LastUpdatedTmst =
        entities.DisbursementType.LastUpdatedTmst;
      UseCabLogTypeOrStatusAudit();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }

      DeleteDisbursementType();
    }
    else
    {
      ExitState = "FN0000_DISB_TYP_NF";
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

  private void DeleteDisbursementType()
  {
    Update("DeleteDisbursementType",
      (db, command) =>
      {
        db.SetInt32(
          command, "disbTypeId",
          entities.DisbursementType.SystemGeneratedIdentifier);
      });
  }

  private bool ReadDisbursementType()
  {
    entities.DisbursementType.Populated = false;

    return Read("ReadDisbursementType",
      (db, command) =>
      {
        db.SetString(command, "code", import.DisbursementType.Code);
        db.SetDate(
          command, "effectiveDate",
          import.DisbursementType.EffectiveDate.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.DisbursementType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.DisbursementType.Code = db.GetString(reader, 1);
        entities.DisbursementType.EffectiveDate = db.GetDate(reader, 2);
        entities.DisbursementType.DiscontinueDate =
          db.GetNullableDate(reader, 3);
        entities.DisbursementType.CreatedBy = db.GetString(reader, 4);
        entities.DisbursementType.CreatedTimestamp = db.GetDateTime(reader, 5);
        entities.DisbursementType.LastUpdatedBy =
          db.GetNullableString(reader, 6);
        entities.DisbursementType.LastUpdatedTmst =
          db.GetNullableDateTime(reader, 7);
        entities.DisbursementType.Description = db.GetNullableString(reader, 8);
        entities.DisbursementType.Populated = true;
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
    /// A value of DisbursementType.
    /// </summary>
    [JsonPropertyName("disbursementType")]
    public DisbursementType DisbursementType
    {
      get => disbursementType ??= new();
      set => disbursementType = value;
    }

    private DisbursementType disbursementType;
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
    /// A value of DisbursementType.
    /// </summary>
    [JsonPropertyName("disbursementType")]
    public DisbursementType DisbursementType
    {
      get => disbursementType ??= new();
      set => disbursementType = value;
    }

    private DisbursementType disbursementType;
  }
#endregion
}
