// Program: FN_CREATE_DISBURSEMENT_STATUS, ID: 371830095, model: 746.
// Short name: SWE00363
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: FN_CREATE_DISBURSEMENT_STATUS.
/// </para>
/// <para>
/// RESP: FINANCE
/// </para>
/// </summary>
[Serializable]
public partial class FnCreateDisbursementStatus: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_CREATE_DISBURSEMENT_STATUS program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnCreateDisbursementStatus(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnCreateDisbursementStatus.
  /// </summary>
  public FnCreateDisbursementStatus(IContext context, Import import,
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
    MoveDisbursementStatus(import.DisbursementStatus, export.DisbursementStatus);
      

    // If Discontinue date is blank, then default it to max date
    if (Equal(import.DisbursementStatus.DiscontinueDate, null))
    {
      local.DisbursementStatus.DiscontinueDate =
        UseCabSetMaximumDiscontinueDate();
    }
    else
    {
      local.DisbursementStatus.DiscontinueDate =
        import.DisbursementStatus.DiscontinueDate;
    }

    // ***********************
    //     Validate input
    // ***********************
    // ----------------------------------------------------------
    // Check for if there is the same record with the same date range or 
    // overlapping date ranges existing.
    // ----------------------------------------------------------
    if (ReadDisbursementStatus())
    {
      ExitState = "ACO_NE0000_DATE_OVERLAP";
    }
    else
    {
      // ***** MAIN-LINE AREA *****
      try
      {
        CreateDisbursementStatus();
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "FN0000_DISB_STAT_AE";

            break;
          case ErrorCode.PermittedValueViolation:
            ExitState = "FN0000_DISB_STAT_PV";

            break;
          case ErrorCode.DatabaseError:
            break;
          default:
            throw;
        }
      }
    }
  }

  private static void MoveDisbursementStatus(DisbursementStatus source,
    DisbursementStatus target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Code = source.Code;
    target.Name = source.Name;
    target.Description = source.Description;
    target.EffectiveDate = source.EffectiveDate;
    target.DiscontinueDate = source.DiscontinueDate;
  }

  private DateTime? UseCabSetMaximumDiscontinueDate()
  {
    var useImport = new CabSetMaximumDiscontinueDate.Import();
    var useExport = new CabSetMaximumDiscontinueDate.Export();

    Call(CabSetMaximumDiscontinueDate.Execute, useImport, useExport);

    return useExport.DateWorkArea.Date;
  }

  private int UseFnAssignDisbursementStatusId()
  {
    var useImport = new FnAssignDisbursementStatusId.Import();
    var useExport = new FnAssignDisbursementStatusId.Export();

    Call(FnAssignDisbursementStatusId.Execute, useImport, useExport);

    return useExport.DisbursementStatus.SystemGeneratedIdentifier;
  }

  private void CreateDisbursementStatus()
  {
    var systemGeneratedIdentifier = UseFnAssignDisbursementStatusId();
    var code = import.DisbursementStatus.Code;
    var name = import.DisbursementStatus.Name;
    var effectiveDate = import.DisbursementStatus.EffectiveDate;
    var discontinueDate = local.DisbursementStatus.DiscontinueDate;
    var createdBy = global.UserId;
    var createdTimestamp = Now();
    var description = import.DisbursementStatus.Description ?? "";

    entities.DisbursementStatus.Populated = false;
    Update("CreateDisbursementStatus",
      (db, command) =>
      {
        db.SetInt32(command, "disbStatusId", systemGeneratedIdentifier);
        db.SetString(command, "code", code);
        db.SetString(command, "name", name);
        db.SetDate(command, "effectiveDate", effectiveDate);
        db.SetNullableDate(command, "discontinueDate", discontinueDate);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetNullableString(command, "lastUpdateBy", "");
        db.SetNullableDateTime(command, "lastUpdateTmst", default(DateTime));
        db.SetNullableString(command, "description", description);
      });

    entities.DisbursementStatus.SystemGeneratedIdentifier =
      systemGeneratedIdentifier;
    entities.DisbursementStatus.Code = code;
    entities.DisbursementStatus.Name = name;
    entities.DisbursementStatus.EffectiveDate = effectiveDate;
    entities.DisbursementStatus.DiscontinueDate = discontinueDate;
    entities.DisbursementStatus.CreatedBy = createdBy;
    entities.DisbursementStatus.CreatedTimestamp = createdTimestamp;
    entities.DisbursementStatus.Description = description;
    entities.DisbursementStatus.Populated = true;
  }

  private bool ReadDisbursementStatus()
  {
    entities.DisbursementStatus.Populated = false;

    return Read("ReadDisbursementStatus",
      (db, command) =>
      {
        db.SetString(command, "code", import.DisbursementStatus.Code);
        db.SetDate(
          command, "effectiveDate1",
          import.DisbursementStatus.EffectiveDate.GetValueOrDefault());
        db.SetDate(
          command, "effectiveDate2",
          import.DisbursementStatus.DiscontinueDate.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.DisbursementStatus.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.DisbursementStatus.Code = db.GetString(reader, 1);
        entities.DisbursementStatus.Name = db.GetString(reader, 2);
        entities.DisbursementStatus.EffectiveDate = db.GetDate(reader, 3);
        entities.DisbursementStatus.DiscontinueDate =
          db.GetNullableDate(reader, 4);
        entities.DisbursementStatus.CreatedBy = db.GetString(reader, 5);
        entities.DisbursementStatus.CreatedTimestamp =
          db.GetDateTime(reader, 6);
        entities.DisbursementStatus.Description =
          db.GetNullableString(reader, 7);
        entities.DisbursementStatus.Populated = true;
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

    private DisbursementStatus disbursementStatus;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
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
