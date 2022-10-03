// Program: FN_BFX3_FIX_MONTHLY_OLIGEE_SUMM, ID: 372895226, model: 746.
// Short name: SWEFFX3B
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_BFX3_FIX_MONTHLY_OLIGEE_SUMM.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Batch)]
public partial class FnBfx3FixMonthlyOligeeSumm: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_BFX3_FIX_MONTHLY_OLIGEE_SUMM program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnBfx3FixMonthlyOligeeSumm(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnBfx3FixMonthlyOligeeSumm.
  /// </summary>
  public FnBfx3FixMonthlyOligeeSumm(IContext context, Import import,
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
    // -----------------------------------------------------
    // Initial Version :- SWSRKXD 09/16/99
    // This utility will fix converted data on monthly_obligee_summary by:
    // 1. Rolling back passthru amounts by 1 month
    // 2. Setting passthru amount to $40 if higher.
    // -----------------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";

    foreach(var item in ReadMonthlyObligeeSummaryObligeeCsePerson())
    {
      // -----------------------------------------------------
      // Check if commit count has been reached.
      // -----------------------------------------------------
      if (local.CommitCnt.Count > 100)
      {
        local.CommitCnt.Count = 0;
        UseExtToDoACommit();

        if (local.ForCommit.NumericReturnCode != 0)
        {
          ExitState = "FN0000_INVALID_EXT_COMMIT_RTN_CD";

          return;
        }
      }

      ++local.CommitCnt.Count;
      local.MonthlyObligeeSummary.Assign(entities.MonthlyObligeeSummary);

      if (entities.MonthlyObligeeSummary.Month == 1)
      {
        local.MonthlyObligeeSummary.Month = 12;
        --local.MonthlyObligeeSummary.Year;
      }
      else
      {
        --local.MonthlyObligeeSummary.Month;
      }

      // ----------------------------------------------
      // Set PT amount to $40 if higher.
      // ----------------------------------------------
      if (local.MonthlyObligeeSummary.PassthruAmount > 40)
      {
        local.MonthlyObligeeSummary.PassthruAmount = 40;
      }

      if (ReadMonthlyObligeeSummary())
      {
        try
        {
          UpdateMonthlyObligeeSummary2();
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              ExitState = "FN0000_MTH_OBLIGEE_SUM_NU";

              break;
            case ErrorCode.PermittedValueViolation:
              ExitState = "FN0000_MTH_OBLIGEE_SUM_PV";

              break;
            case ErrorCode.DatabaseError:
              break;
            default:
              throw;
          }
        }
      }
      else
      {
        try
        {
          CreateMonthlyObligeeSummary();
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              ExitState = "FN0000_MTH_OBLIGEE_SUM_AE";

              break;
            case ErrorCode.PermittedValueViolation:
              ExitState = "FN0000_MTH_OBLIGEE_SUM_PV";

              break;
            case ErrorCode.DatabaseError:
              break;
            default:
              throw;
          }
        }
      }

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }

      // -----------------------------------------------------
      // If there are no totals other than PT, delete record.
      // Else update PT amount to 0.
      // -----------------------------------------------------
      if (Equal(entities.MonthlyObligeeSummary.AdcReimbursedAmount, 0) && Equal
        (entities.MonthlyObligeeSummary.CollectionsAmount, 0) && Equal
        (entities.MonthlyObligeeSummary.CollectionsDisbursedToAr, 0) && Equal
        (entities.MonthlyObligeeSummary.FeeAmount, 0) && Equal
        (entities.MonthlyObligeeSummary.NaArrearsRecapAmt, 0) && Equal
        (entities.MonthlyObligeeSummary.RecapturedAmt, 0) && Equal
        (entities.MonthlyObligeeSummary.PassthruRecapAmt, 0) && Equal
        (entities.MonthlyObligeeSummary.DisbursementsSuppressed, 0))
      {
        DeleteMonthlyObligeeSummary();
      }
      else
      {
        try
        {
          UpdateMonthlyObligeeSummary1();
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              ExitState = "FN0000_MTH_OBLIGEE_SUM_NU";

              break;
            case ErrorCode.PermittedValueViolation:
              ExitState = "FN0000_MTH_OBLIGEE_SUM_PV";

              break;
            case ErrorCode.DatabaseError:
              break;
            default:
              throw;
          }
        }

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }
      }
    }
  }

  private void UseExtToDoACommit()
  {
    var useImport = new ExtToDoACommit.Import();
    var useExport = new ExtToDoACommit.Export();

    useExport.External.NumericReturnCode = local.ForCommit.NumericReturnCode;

    Call(ExtToDoACommit.Execute, useImport, useExport);

    local.ForCommit.NumericReturnCode = useExport.External.NumericReturnCode;
  }

  private void CreateMonthlyObligeeSummary()
  {
    System.Diagnostics.Debug.Assert(entities.Obligee.Populated);

    var year = local.MonthlyObligeeSummary.Year;
    var month = local.MonthlyObligeeSummary.Month;
    var passthruRecapAmt = 0M;
    var passthruAmount = local.MonthlyObligeeSummary.PassthruAmount;
    var createdBy = global.UserId;
    var createdTimestamp = Now();
    var cpaSType = entities.Obligee.Type1;
    var cspSNumber = entities.Obligee.CspNumber;
    var zdelType = local.MonthlyObligeeSummary.ZdelType ?? "";

    CheckValid<MonthlyObligeeSummary>("CpaSType", cpaSType);
    entities.Prev.Populated = false;
    Update("CreateMonthlyObligeeSummary",
      (db, command) =>
      {
        db.SetInt32(command, "yer", year);
        db.SetInt32(command, "mnth", month);
        db.SetNullableDecimal(command, "ptRecapAmt", passthruRecapAmt);
        db.SetNullableDecimal(command, "disbursementsSupp", passthruRecapAmt);
        db.SetNullableDecimal(command, "recapturedAmt", passthruRecapAmt);
        db.SetNullableDecimal(command, "naArsRecapAmt", passthruRecapAmt);
        db.SetDecimal(command, "passthruAmount", passthruAmount);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetNullableString(command, "lastUpdatedBy", createdBy);
        db.SetNullableDateTime(command, "lastUpdatedTmst", createdTimestamp);
        db.SetNullableDecimal(command, "adcReimbursedAmt", passthruRecapAmt);
        db.SetString(command, "cpaSType", cpaSType);
        db.SetString(command, "cspSNumber", cspSNumber);
        db.SetNullableDecimal(command, "collectionAmt", passthruRecapAmt);
        db.SetNullableDecimal(command, "collctnDsbToAr", passthruRecapAmt);
        db.SetNullableDecimal(command, "feeAmt", passthruRecapAmt);
        db.SetNullableString(command, "zdelType", zdelType);
        db.SetNullableDecimal(command, "totExcessUraAmt", passthruRecapAmt);
        db.SetNullableInt32(command, "nbrOfCollections", 0);
      });

    entities.Prev.Year = year;
    entities.Prev.Month = month;
    entities.Prev.PassthruRecapAmt = passthruRecapAmt;
    entities.Prev.DisbursementsSuppressed = passthruRecapAmt;
    entities.Prev.RecapturedAmt = passthruRecapAmt;
    entities.Prev.NaArrearsRecapAmt = passthruRecapAmt;
    entities.Prev.PassthruAmount = passthruAmount;
    entities.Prev.CreatedBy = createdBy;
    entities.Prev.CreatedTimestamp = createdTimestamp;
    entities.Prev.LastUpdatedBy = createdBy;
    entities.Prev.LastUpdatedTmst = createdTimestamp;
    entities.Prev.AdcReimbursedAmount = passthruRecapAmt;
    entities.Prev.CpaSType = cpaSType;
    entities.Prev.CspSNumber = cspSNumber;
    entities.Prev.CollectionsAmount = passthruRecapAmt;
    entities.Prev.CollectionsDisbursedToAr = passthruRecapAmt;
    entities.Prev.FeeAmount = passthruRecapAmt;
    entities.Prev.ZdelType = zdelType;
    entities.Prev.Populated = true;
  }

  private void DeleteMonthlyObligeeSummary()
  {
    Update("DeleteMonthlyObligeeSummary",
      (db, command) =>
      {
        db.SetInt32(command, "yer", entities.MonthlyObligeeSummary.Year);
        db.SetInt32(command, "mnth", entities.MonthlyObligeeSummary.Month);
        db.SetString(
          command, "cpaSType", entities.MonthlyObligeeSummary.CpaSType);
        db.SetString(
          command, "cspSNumber", entities.MonthlyObligeeSummary.CspSNumber);
      });
  }

  private bool ReadMonthlyObligeeSummary()
  {
    System.Diagnostics.Debug.Assert(entities.Obligee.Populated);
    entities.Prev.Populated = false;

    return Read("ReadMonthlyObligeeSummary",
      (db, command) =>
      {
        db.SetString(command, "cpaSType", entities.Obligee.Type1);
        db.SetString(command, "cspSNumber", entities.Obligee.CspNumber);
        db.SetInt32(command, "mnth", local.MonthlyObligeeSummary.Month);
        db.SetInt32(command, "yer", local.MonthlyObligeeSummary.Year);
      },
      (db, reader) =>
      {
        entities.Prev.Year = db.GetInt32(reader, 0);
        entities.Prev.Month = db.GetInt32(reader, 1);
        entities.Prev.PassthruRecapAmt = db.GetNullableDecimal(reader, 2);
        entities.Prev.DisbursementsSuppressed =
          db.GetNullableDecimal(reader, 3);
        entities.Prev.RecapturedAmt = db.GetNullableDecimal(reader, 4);
        entities.Prev.NaArrearsRecapAmt = db.GetNullableDecimal(reader, 5);
        entities.Prev.PassthruAmount = db.GetDecimal(reader, 6);
        entities.Prev.CreatedBy = db.GetString(reader, 7);
        entities.Prev.CreatedTimestamp = db.GetDateTime(reader, 8);
        entities.Prev.LastUpdatedBy = db.GetNullableString(reader, 9);
        entities.Prev.LastUpdatedTmst = db.GetNullableDateTime(reader, 10);
        entities.Prev.AdcReimbursedAmount = db.GetNullableDecimal(reader, 11);
        entities.Prev.CpaSType = db.GetString(reader, 12);
        entities.Prev.CspSNumber = db.GetString(reader, 13);
        entities.Prev.CollectionsAmount = db.GetNullableDecimal(reader, 14);
        entities.Prev.CollectionsDisbursedToAr =
          db.GetNullableDecimal(reader, 15);
        entities.Prev.FeeAmount = db.GetNullableDecimal(reader, 16);
        entities.Prev.ZdelType = db.GetNullableString(reader, 17);
        entities.Prev.Populated = true;
      });
  }

  private IEnumerable<bool> ReadMonthlyObligeeSummaryObligeeCsePerson()
  {
    entities.Obligee.Populated = false;
    entities.CsePerson.Populated = false;
    entities.MonthlyObligeeSummary.Populated = false;

    return ReadEach("ReadMonthlyObligeeSummaryObligeeCsePerson",
      (db, command) =>
      {
        db.SetNullableString(command, "lastUpdatedBy", global.UserId);
      },
      (db, reader) =>
      {
        entities.MonthlyObligeeSummary.Year = db.GetInt32(reader, 0);
        entities.MonthlyObligeeSummary.Month = db.GetInt32(reader, 1);
        entities.MonthlyObligeeSummary.PassthruRecapAmt =
          db.GetNullableDecimal(reader, 2);
        entities.MonthlyObligeeSummary.DisbursementsSuppressed =
          db.GetNullableDecimal(reader, 3);
        entities.MonthlyObligeeSummary.RecapturedAmt =
          db.GetNullableDecimal(reader, 4);
        entities.MonthlyObligeeSummary.NaArrearsRecapAmt =
          db.GetNullableDecimal(reader, 5);
        entities.MonthlyObligeeSummary.PassthruAmount =
          db.GetDecimal(reader, 6);
        entities.MonthlyObligeeSummary.CreatedBy = db.GetString(reader, 7);
        entities.MonthlyObligeeSummary.CreatedTimestamp =
          db.GetDateTime(reader, 8);
        entities.MonthlyObligeeSummary.LastUpdatedBy =
          db.GetNullableString(reader, 9);
        entities.MonthlyObligeeSummary.LastUpdatedTmst =
          db.GetNullableDateTime(reader, 10);
        entities.MonthlyObligeeSummary.AdcReimbursedAmount =
          db.GetNullableDecimal(reader, 11);
        entities.MonthlyObligeeSummary.CpaSType = db.GetString(reader, 12);
        entities.Obligee.Type1 = db.GetString(reader, 12);
        entities.MonthlyObligeeSummary.CspSNumber = db.GetString(reader, 13);
        entities.Obligee.CspNumber = db.GetString(reader, 13);
        entities.CsePerson.Number = db.GetString(reader, 13);
        entities.CsePerson.Number = db.GetString(reader, 13);
        entities.MonthlyObligeeSummary.CollectionsAmount =
          db.GetNullableDecimal(reader, 14);
        entities.MonthlyObligeeSummary.CollectionsDisbursedToAr =
          db.GetNullableDecimal(reader, 15);
        entities.MonthlyObligeeSummary.FeeAmount =
          db.GetNullableDecimal(reader, 16);
        entities.MonthlyObligeeSummary.ZdelType =
          db.GetNullableString(reader, 17);
        entities.Obligee.Populated = true;
        entities.CsePerson.Populated = true;
        entities.MonthlyObligeeSummary.Populated = true;

        return true;
      });
  }

  private void UpdateMonthlyObligeeSummary1()
  {
    System.Diagnostics.Debug.Assert(entities.MonthlyObligeeSummary.Populated);

    var passthruAmount = 0M;
    var lastUpdatedBy = global.UserId;
    var lastUpdatedTmst = Now();

    entities.MonthlyObligeeSummary.Populated = false;
    Update("UpdateMonthlyObligeeSummary1",
      (db, command) =>
      {
        db.SetDecimal(command, "passthruAmount", passthruAmount);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTmst);
        db.SetInt32(command, "yer", entities.MonthlyObligeeSummary.Year);
        db.SetInt32(command, "mnth", entities.MonthlyObligeeSummary.Month);
        db.SetString(
          command, "cpaSType", entities.MonthlyObligeeSummary.CpaSType);
        db.SetString(
          command, "cspSNumber", entities.MonthlyObligeeSummary.CspSNumber);
      });

    entities.MonthlyObligeeSummary.PassthruAmount = passthruAmount;
    entities.MonthlyObligeeSummary.LastUpdatedBy = lastUpdatedBy;
    entities.MonthlyObligeeSummary.LastUpdatedTmst = lastUpdatedTmst;
    entities.MonthlyObligeeSummary.Populated = true;
  }

  private void UpdateMonthlyObligeeSummary2()
  {
    System.Diagnostics.Debug.Assert(entities.Prev.Populated);

    var passthruAmount = local.MonthlyObligeeSummary.PassthruAmount;
    var lastUpdatedBy = global.UserId;
    var lastUpdatedTmst = Now();

    entities.Prev.Populated = false;
    Update("UpdateMonthlyObligeeSummary2",
      (db, command) =>
      {
        db.SetDecimal(command, "passthruAmount", passthruAmount);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTmst);
        db.SetInt32(command, "yer", entities.Prev.Year);
        db.SetInt32(command, "mnth", entities.Prev.Month);
        db.SetString(command, "cpaSType", entities.Prev.CpaSType);
        db.SetString(command, "cspSNumber", entities.Prev.CspSNumber);
      });

    entities.Prev.PassthruAmount = passthruAmount;
    entities.Prev.LastUpdatedBy = lastUpdatedBy;
    entities.Prev.LastUpdatedTmst = lastUpdatedTmst;
    entities.Prev.Populated = true;
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
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of MonthlyObligeeSummary.
    /// </summary>
    [JsonPropertyName("monthlyObligeeSummary")]
    public MonthlyObligeeSummary MonthlyObligeeSummary
    {
      get => monthlyObligeeSummary ??= new();
      set => monthlyObligeeSummary = value;
    }

    /// <summary>
    /// A value of CommitCnt.
    /// </summary>
    [JsonPropertyName("commitCnt")]
    public Common CommitCnt
    {
      get => commitCnt ??= new();
      set => commitCnt = value;
    }

    /// <summary>
    /// A value of ForCommit.
    /// </summary>
    [JsonPropertyName("forCommit")]
    public External ForCommit
    {
      get => forCommit ??= new();
      set => forCommit = value;
    }

    private MonthlyObligeeSummary monthlyObligeeSummary;
    private Common commitCnt;
    private External forCommit;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
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
    /// A value of Prev.
    /// </summary>
    [JsonPropertyName("prev")]
    public MonthlyObligeeSummary Prev
    {
      get => prev ??= new();
      set => prev = value;
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
    /// A value of MonthlyObligeeSummary.
    /// </summary>
    [JsonPropertyName("monthlyObligeeSummary")]
    public MonthlyObligeeSummary MonthlyObligeeSummary
    {
      get => monthlyObligeeSummary ??= new();
      set => monthlyObligeeSummary = value;
    }

    private CsePersonAccount obligee;
    private MonthlyObligeeSummary prev;
    private CsePerson csePerson;
    private MonthlyObligeeSummary monthlyObligeeSummary;
  }
#endregion
}
