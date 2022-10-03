// Program: FN_CRE_DISBURSEMENT_TRAN_RLN_RSN, ID: 371834964, model: 746.
// Short name: SWE00334
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_CRE_DISBURSEMENT_TRAN_RLN_RSN.
/// </summary>
[Serializable]
public partial class FnCreDisbursementTranRlnRsn: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_CRE_DISBURSEMENT_TRAN_RLN_RSN program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnCreDisbursementTranRlnRsn(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnCreDisbursementTranRlnRsn.
  /// </summary>
  public FnCreDisbursementTranRlnRsn(IContext context, Import import,
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
    // Check for existing record to prevent adding an existing record.
    export.DisbursementTranRlnRsn.Assign(import.DisbursementTranRlnRsn);

    if (ReadDisbursementTranRlnRsn())
    {
      ExitState = "ACO_NE0000_DATE_OVERLAP";
    }
    else
    {
      try
      {
        CreateDisbursementTranRlnRsn();
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "FN0000_DISB_TRANS_RLN_RSN_AE";

            break;
          case ErrorCode.PermittedValueViolation:
            ExitState = "FN0000_DISB_TRANS_RLN_RSN_PV";

            break;
          case ErrorCode.DatabaseError:
            break;
          default:
            throw;
        }
      }
    }
  }

  private int UseFnAssignDisbTranRlnRsnId()
  {
    var useImport = new FnAssignDisbTranRlnRsnId.Import();
    var useExport = new FnAssignDisbTranRlnRsnId.Export();

    Call(FnAssignDisbTranRlnRsnId.Execute, useImport, useExport);

    return useExport.DisbursementTranRlnRsn.SystemGeneratedIdentifier;
  }

  private void CreateDisbursementTranRlnRsn()
  {
    var systemGeneratedIdentifier = UseFnAssignDisbTranRlnRsnId();
    var code = import.DisbursementTranRlnRsn.Code;
    var name = import.DisbursementTranRlnRsn.Name;
    var effectiveDate = import.DisbursementTranRlnRsn.EffectiveDate;
    var discontinueDate = import.DisbursementTranRlnRsn.DiscontinueDate;
    var createdBy = global.UserId;
    var createdTimestamp = Now();
    var description = import.DisbursementTranRlnRsn.Description ?? "";

    entities.DisbursementTranRlnRsn.Populated = false;
    Update("CreateDisbursementTranRlnRsn",
      (db, command) =>
      {
        db.SetInt32(command, "disbTrnRlnRsId", systemGeneratedIdentifier);
        db.SetString(command, "code", code);
        db.SetString(command, "name", name);
        db.SetDate(command, "effectiveDate", effectiveDate);
        db.SetNullableDate(command, "discontinueDate", discontinueDate);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetNullableString(command, "lastUpdatedBy", "");
        db.SetNullableDateTime(command, "lastUpdatedTmst", default(DateTime));
        db.SetNullableString(command, "description", description);
      });

    entities.DisbursementTranRlnRsn.SystemGeneratedIdentifier =
      systemGeneratedIdentifier;
    entities.DisbursementTranRlnRsn.Code = code;
    entities.DisbursementTranRlnRsn.Name = name;
    entities.DisbursementTranRlnRsn.EffectiveDate = effectiveDate;
    entities.DisbursementTranRlnRsn.DiscontinueDate = discontinueDate;
    entities.DisbursementTranRlnRsn.CreatedBy = createdBy;
    entities.DisbursementTranRlnRsn.CreatedTimestamp = createdTimestamp;
    entities.DisbursementTranRlnRsn.Description = description;
    entities.DisbursementTranRlnRsn.Populated = true;
  }

  private bool ReadDisbursementTranRlnRsn()
  {
    entities.DisbursementTranRlnRsn.Populated = false;

    return Read("ReadDisbursementTranRlnRsn",
      (db, command) =>
      {
        db.SetString(command, "code", import.DisbursementTranRlnRsn.Code);
        db.SetDate(
          command, "effectiveDate1",
          import.DisbursementTranRlnRsn.EffectiveDate.GetValueOrDefault());
        db.SetDate(
          command, "effectiveDate2",
          import.DisbursementTranRlnRsn.DiscontinueDate.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.DisbursementTranRlnRsn.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.DisbursementTranRlnRsn.Code = db.GetString(reader, 1);
        entities.DisbursementTranRlnRsn.Name = db.GetString(reader, 2);
        entities.DisbursementTranRlnRsn.EffectiveDate = db.GetDate(reader, 3);
        entities.DisbursementTranRlnRsn.DiscontinueDate =
          db.GetNullableDate(reader, 4);
        entities.DisbursementTranRlnRsn.CreatedBy = db.GetString(reader, 5);
        entities.DisbursementTranRlnRsn.CreatedTimestamp =
          db.GetDateTime(reader, 6);
        entities.DisbursementTranRlnRsn.Description =
          db.GetNullableString(reader, 7);
        entities.DisbursementTranRlnRsn.Populated = true;
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
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
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
