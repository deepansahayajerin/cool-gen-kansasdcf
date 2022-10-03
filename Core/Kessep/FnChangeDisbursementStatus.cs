// Program: FN_CHANGE_DISBURSEMENT_STATUS, ID: 371753806, model: 746.
// Short name: SWE00313
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: FN_CHANGE_DISBURSEMENT_STATUS.
/// </para>
/// <para>
/// RESP: FINANCE
/// This cab will be used to change the status of a disbursement from PEND to 
/// SUPP or SUPP to PEND.
/// </para>
/// </summary>
[Serializable]
public partial class FnChangeDisbursementStatus: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_CHANGE_DISBURSEMENT_STATUS program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnChangeDisbursementStatus(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnChangeDisbursementStatus.
  /// </summary>
  public FnChangeDisbursementStatus(IContext context, Import import,
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
    // ****************************************************************
    // Initial Version ????
    // ***************************************************************
    // ****************************************************************
    // SWSRKXD, PR# 80198, 11/16/99
    // SGI on newly created disb_stat_hist should never be set to
    // 0. CAB retrieves highest SGI and then adds 1 to obtain new
    // SGI. The read each to get highest SGI was coded to look for
    // a specific status. It should be irrespective of status.
    // Fangman, PR 98039, 11/1/00
    // Changed exit states to similar exit states with roll back.
    // ***************************************************************
    // read the current disbursement and its status
    local.Maximum.Date = UseCabSetMaximumDiscontinueDate();
    local.Current.Date = Now().Date;
    MoveDisbursementStatusHistory(import.DisbursementStatusHistory,
      export.DisbursementStatusHistory);

    // ****************************************************************
    // Took out read for discontinue date of 12-31-2099, because we could be 
    // changing the dates on a suppression that had an end date less than that.
    // rk 6/15/99
    // ***************************************************************
    if (!ReadDisbursementStatusHistoryDisbursementTransaction())
    {
      ExitState = "FN0000_DISB_STAT_HIST_NF_RB";

      return;
    }

    try
    {
      UpdateDisbursementStatusHistory();

      // -----------------------------------------------------
      // SWSRKXD, PR# 80198, 11/16/99
      // Read for highest SGI within a disb. Remove the clause
      // 'AND DESIRED disb_stat_hist is_defined_by
      // import_per_new disb_status'.
      // ----------------------------------------------------
      if (ReadDisbursementStatusHistory())
      {
        local.DisbursementStatusHistory.SystemGeneratedIdentifier =
          entities.DisbursementStatusHistory.SystemGeneratedIdentifier + 1;
      }

      try
      {
        CreateDisbursementStatusHistory();
        MoveDisbursementStatusHistory(entities.DisbursementStatusHistory,
          export.DisbursementStatusHistory);
      }
      catch(Exception e1)
      {
        switch(GetErrorCode(e1))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "FN0000_DISB_STAT_HIST_AE_RB";

            break;
          case ErrorCode.PermittedValueViolation:
            ExitState = "FN0000_DISB_STAT_HIST_PV_RB";

            break;
          case ErrorCode.DatabaseError:
            break;
          default:
            throw;
        }
      }
    }
    catch(Exception e)
    {
      switch(GetErrorCode(e))
      {
        case ErrorCode.AlreadyExists:
          ExitState = "FN0000_DISB_STAT_HIST_NU_RB";

          break;
        case ErrorCode.PermittedValueViolation:
          ExitState = "FN0000_DISB_STAT_HIST_PV_RB";

          break;
        case ErrorCode.DatabaseError:
          break;
        default:
          throw;
      }
    }
  }

  private static void MoveDisbursementStatusHistory(
    DisbursementStatusHistory source, DisbursementStatusHistory target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.ReasonText = source.ReasonText;
  }

  private DateTime? UseCabSetMaximumDiscontinueDate()
  {
    var useImport = new CabSetMaximumDiscontinueDate.Import();
    var useExport = new CabSetMaximumDiscontinueDate.Export();

    Call(CabSetMaximumDiscontinueDate.Execute, useImport, useExport);

    return useExport.DateWorkArea.Date;
  }

  private void CreateDisbursementStatusHistory()
  {
    System.Diagnostics.Debug.Assert(entities.DisbursementTransaction.Populated);

    var dbsGeneratedId = import.PerNew.SystemGeneratedIdentifier;
    var dtrGeneratedId =
      entities.DisbursementTransaction.SystemGeneratedIdentifier;
    var cspNumber = entities.DisbursementTransaction.CspNumber;
    var cpaType = entities.DisbursementTransaction.CpaType;
    var systemGeneratedIdentifier =
      local.DisbursementStatusHistory.SystemGeneratedIdentifier;
    var effectiveDate = local.Current.Date;
    var discontinueDate = local.Maximum.Date;
    var createdBy = global.UserId;
    var createdTimestamp = Now();
    var reasonText = import.DisbursementStatusHistory.ReasonText ?? "";

    CheckValid<DisbursementStatusHistory>("CpaType", cpaType);
    entities.DisbursementStatusHistory.Populated = false;
    Update("CreateDisbursementStatusHistory",
      (db, command) =>
      {
        db.SetInt32(command, "dbsGeneratedId", dbsGeneratedId);
        db.SetInt32(command, "dtrGeneratedId", dtrGeneratedId);
        db.SetString(command, "cspNumber", cspNumber);
        db.SetString(command, "cpaType", cpaType);
        db.SetInt32(command, "disbStatHistId", systemGeneratedIdentifier);
        db.SetDate(command, "effectiveDate", effectiveDate);
        db.SetNullableDate(command, "discontinueDate", discontinueDate);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetNullableString(command, "reasonText", reasonText);
        db.SetNullableString(command, "suppressionReason", "");
      });

    entities.DisbursementStatusHistory.DbsGeneratedId = dbsGeneratedId;
    entities.DisbursementStatusHistory.DtrGeneratedId = dtrGeneratedId;
    entities.DisbursementStatusHistory.CspNumber = cspNumber;
    entities.DisbursementStatusHistory.CpaType = cpaType;
    entities.DisbursementStatusHistory.SystemGeneratedIdentifier =
      systemGeneratedIdentifier;
    entities.DisbursementStatusHistory.EffectiveDate = effectiveDate;
    entities.DisbursementStatusHistory.DiscontinueDate = discontinueDate;
    entities.DisbursementStatusHistory.CreatedBy = createdBy;
    entities.DisbursementStatusHistory.CreatedTimestamp = createdTimestamp;
    entities.DisbursementStatusHistory.ReasonText = reasonText;
    entities.DisbursementStatusHistory.Populated = true;
  }

  private bool ReadDisbursementStatusHistory()
  {
    System.Diagnostics.Debug.Assert(entities.DisbursementTransaction.Populated);
    entities.DisbursementStatusHistory.Populated = false;

    return Read("ReadDisbursementStatusHistory",
      (db, command) =>
      {
        db.SetInt32(
          command, "dtrGeneratedId",
          entities.DisbursementTransaction.SystemGeneratedIdentifier);
        db.SetString(
          command, "cspNumber", entities.DisbursementTransaction.CspNumber);
        db.SetString(
          command, "cpaType", entities.DisbursementTransaction.CpaType);
      },
      (db, reader) =>
      {
        entities.DisbursementStatusHistory.DbsGeneratedId =
          db.GetInt32(reader, 0);
        entities.DisbursementStatusHistory.DtrGeneratedId =
          db.GetInt32(reader, 1);
        entities.DisbursementStatusHistory.CspNumber = db.GetString(reader, 2);
        entities.DisbursementStatusHistory.CpaType = db.GetString(reader, 3);
        entities.DisbursementStatusHistory.SystemGeneratedIdentifier =
          db.GetInt32(reader, 4);
        entities.DisbursementStatusHistory.EffectiveDate =
          db.GetDate(reader, 5);
        entities.DisbursementStatusHistory.DiscontinueDate =
          db.GetNullableDate(reader, 6);
        entities.DisbursementStatusHistory.CreatedBy = db.GetString(reader, 7);
        entities.DisbursementStatusHistory.CreatedTimestamp =
          db.GetDateTime(reader, 8);
        entities.DisbursementStatusHistory.ReasonText =
          db.GetNullableString(reader, 9);
        entities.DisbursementStatusHistory.Populated = true;
        CheckValid<DisbursementStatusHistory>("CpaType",
          entities.DisbursementStatusHistory.CpaType);
      });
  }

  private bool ReadDisbursementStatusHistoryDisbursementTransaction()
  {
    entities.DisbursementStatusHistory.Populated = false;
    entities.DisbursementTransaction.Populated = false;

    return Read("ReadDisbursementStatusHistoryDisbursementTransaction",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", import.CsePerson.Number);
        db.SetInt32(
          command, "disbTranId",
          import.DisbursementTransaction.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "disbStatHistId",
          import.DisbursementStatusHistory.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.DisbursementStatusHistory.DbsGeneratedId =
          db.GetInt32(reader, 0);
        entities.DisbursementStatusHistory.DtrGeneratedId =
          db.GetInt32(reader, 1);
        entities.DisbursementTransaction.SystemGeneratedIdentifier =
          db.GetInt32(reader, 1);
        entities.DisbursementStatusHistory.CspNumber = db.GetString(reader, 2);
        entities.DisbursementTransaction.CspNumber = db.GetString(reader, 2);
        entities.DisbursementStatusHistory.CpaType = db.GetString(reader, 3);
        entities.DisbursementTransaction.CpaType = db.GetString(reader, 3);
        entities.DisbursementStatusHistory.SystemGeneratedIdentifier =
          db.GetInt32(reader, 4);
        entities.DisbursementStatusHistory.EffectiveDate =
          db.GetDate(reader, 5);
        entities.DisbursementStatusHistory.DiscontinueDate =
          db.GetNullableDate(reader, 6);
        entities.DisbursementStatusHistory.CreatedBy = db.GetString(reader, 7);
        entities.DisbursementStatusHistory.CreatedTimestamp =
          db.GetDateTime(reader, 8);
        entities.DisbursementStatusHistory.ReasonText =
          db.GetNullableString(reader, 9);
        entities.DisbursementTransaction.Type1 = db.GetString(reader, 10);
        entities.DisbursementStatusHistory.Populated = true;
        entities.DisbursementTransaction.Populated = true;
        CheckValid<DisbursementStatusHistory>("CpaType",
          entities.DisbursementStatusHistory.CpaType);
        CheckValid<DisbursementTransaction>("CpaType",
          entities.DisbursementTransaction.CpaType);
        CheckValid<DisbursementTransaction>("Type1",
          entities.DisbursementTransaction.Type1);
      });
  }

  private void UpdateDisbursementStatusHistory()
  {
    System.Diagnostics.Debug.
      Assert(entities.DisbursementStatusHistory.Populated);

    var discontinueDate = Now().Date;
    var reasonText = "PROCESSED";

    entities.DisbursementStatusHistory.Populated = false;
    Update("UpdateDisbursementStatusHistory",
      (db, command) =>
      {
        db.SetNullableDate(command, "discontinueDate", discontinueDate);
        db.SetNullableString(command, "reasonText", reasonText);
        db.SetInt32(
          command, "dbsGeneratedId",
          entities.DisbursementStatusHistory.DbsGeneratedId);
        db.SetInt32(
          command, "dtrGeneratedId",
          entities.DisbursementStatusHistory.DtrGeneratedId);
        db.SetString(
          command, "cspNumber", entities.DisbursementStatusHistory.CspNumber);
        db.SetString(
          command, "cpaType", entities.DisbursementStatusHistory.CpaType);
        db.SetInt32(
          command, "disbStatHistId",
          entities.DisbursementStatusHistory.SystemGeneratedIdentifier);
      });

    entities.DisbursementStatusHistory.DiscontinueDate = discontinueDate;
    entities.DisbursementStatusHistory.ReasonText = reasonText;
    entities.DisbursementStatusHistory.Populated = true;
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
    /// A value of PerNew.
    /// </summary>
    [JsonPropertyName("perNew")]
    public DisbursementStatus PerNew
    {
      get => perNew ??= new();
      set => perNew = value;
    }

    /// <summary>
    /// A value of DisbursementStatusHistory.
    /// </summary>
    [JsonPropertyName("disbursementStatusHistory")]
    public DisbursementStatusHistory DisbursementStatusHistory
    {
      get => disbursementStatusHistory ??= new();
      set => disbursementStatusHistory = value;
    }

    /// <summary>
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
    }

    /// <summary>
    /// A value of DisbursementTransaction.
    /// </summary>
    [JsonPropertyName("disbursementTransaction")]
    public DisbursementTransaction DisbursementTransaction
    {
      get => disbursementTransaction ??= new();
      set => disbursementTransaction = value;
    }

    private DisbursementStatus perNew;
    private DisbursementStatusHistory disbursementStatusHistory;
    private CsePerson csePerson;
    private DisbursementTransaction disbursementTransaction;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of DisbursementStatusHistory.
    /// </summary>
    [JsonPropertyName("disbursementStatusHistory")]
    public DisbursementStatusHistory DisbursementStatusHistory
    {
      get => disbursementStatusHistory ??= new();
      set => disbursementStatusHistory = value;
    }

    /// <summary>
    /// A value of New1.
    /// </summary>
    [JsonPropertyName("new1")]
    public DisbursementStatus New1
    {
      get => new1 ??= new();
      set => new1 = value;
    }

    private DisbursementStatusHistory disbursementStatusHistory;
    private DisbursementStatus new1;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of DisbursementStatusHistory.
    /// </summary>
    [JsonPropertyName("disbursementStatusHistory")]
    public DisbursementStatusHistory DisbursementStatusHistory
    {
      get => disbursementStatusHistory ??= new();
      set => disbursementStatusHistory = value;
    }

    /// <summary>
    /// A value of Current.
    /// </summary>
    [JsonPropertyName("current")]
    public DateWorkArea Current
    {
      get => current ??= new();
      set => current = value;
    }

    /// <summary>
    /// A value of Maximum.
    /// </summary>
    [JsonPropertyName("maximum")]
    public DateWorkArea Maximum
    {
      get => maximum ??= new();
      set => maximum = value;
    }

    private DisbursementStatusHistory disbursementStatusHistory;
    private DateWorkArea current;
    private DateWorkArea maximum;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of DisbursementStatusHistory.
    /// </summary>
    [JsonPropertyName("disbursementStatusHistory")]
    public DisbursementStatusHistory DisbursementStatusHistory
    {
      get => disbursementStatusHistory ??= new();
      set => disbursementStatusHistory = value;
    }

    /// <summary>
    /// A value of DisbursementTransaction.
    /// </summary>
    [JsonPropertyName("disbursementTransaction")]
    public DisbursementTransaction DisbursementTransaction
    {
      get => disbursementTransaction ??= new();
      set => disbursementTransaction = value;
    }

    /// <summary>
    /// A value of Obligee.
    /// </summary>
    [JsonPropertyName("obligee")]
    public CsePersonAccount Obligee
    {
      get => obligee ??= new();
      set => obligee = value;
    }

    /// <summary>
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
    }

    private DisbursementStatusHistory disbursementStatusHistory;
    private DisbursementTransaction disbursementTransaction;
    private CsePersonAccount obligee;
    private CsePerson csePerson;
  }
#endregion
}
