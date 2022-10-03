// Program: FN_DRA_FEE_MAINTAIN_DISB_SUMMARY, ID: 371344860, model: 746.
// Short name: SWE02007
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_DRA_FEE_MAINTAIN_DISB_SUMMARY.
/// </summary>
[Serializable]
public partial class FnDraFeeMaintainDisbSummary: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_DRA_FEE_MAINTAIN_DISB_SUMMARY program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnDraFeeMaintainDisbSummary(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnDraFeeMaintainDisbSummary.
  /// </summary>
  public FnDraFeeMaintainDisbSummary(IContext context, Import import,
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
    // -----------------------------------------------------------------------------------------------------------------
    // DATE        DEVELOPER   REQUEST         DESCRIPTION
    // ----------  ----------	----------	
    // -------------------------------------------------------------------------
    // 03/17/2008  GVandy	CQ296		Initial Coding
    // -----------------------------------------------------------------------------------------------------------------
    // ------------------------------------------------------------------------------------------------------------
    // Note:  It is assumed that the disbursement has already been determined to
    // be for a non-TAF case prior to this
    // action block being called.
    // If the obligor/obligee combination is non-TAF on the date of the 
    // disbursement then add the disbursement
    // amount to the non_taf_amount in the DISBURSEMENT_SUMMARY record for the 
    // obligor/obligee for the
    // appropriate fiscal year.  If a DISBURSEMENT_SUMMARY record does not 
    // already exists for the obligor/obligee
    // combo for the appropriate fiscal year then one should be created.
    // ------------------------------------------------------------------------------------------------------------
    // -- Determine fiscal year during which the disbursement occurred.
    if (Month(import.DisbursementTransaction.ProcessDate) <= 9)
    {
      local.FiscalYear.Year = Year(import.DisbursementTransaction.ProcessDate);
    }
    else
    {
      local.FiscalYear.Year =
        Year(import.DisbursementTransaction.ProcessDate) + 1;
    }

    if (ReadDisbursementSummary())
    {
      MoveDisbursementSummary(entities.DisbursementSummary,
        local.DisbursementSummary);
    }
    else
    {
      // -- Continue.
    }

    local.DisbursementSummary.NonTafAmount =
      local.DisbursementSummary.NonTafAmount.GetValueOrDefault() + import
      .DisbursementTransaction.Amount;

    // ------------------------------------------------------------------------------------------------------------
    // Don't allow the non_taf_amount to fall below zero.  This could happen due
    // to negative disbursement amounts
    // (i.e. adjustments).
    // ------------------------------------------------------------------------------------------------------------
    if (local.DisbursementSummary.NonTafAmount.GetValueOrDefault() < 0)
    {
      local.DisbursementSummary.NonTafAmount = 0;
    }

    // ------------------------------------------------------------------------------------------------------------
    // If the $500 threshold amount is exceeded and the threshold date has not 
    // previously been set, set the
    // threshold date to the disbursement date.  The threshold date drives 
    // reporting of the number of cases that
    // qualify for the $25 Deficit Reduction Act (DRA) fee.
    // cq61454 - changed the threshold aamount to $550 and the fee to $35 (fee 
    // is effective in swefb726 (srrun150)
    // ------------------------------------------------------------------------------------------------------------
    if (Equal(local.DisbursementSummary.ThresholdDate, local.Null1.Date) && local
      .DisbursementSummary.NonTafAmount.GetValueOrDefault() >= 550)
    {
      local.DisbursementSummary.ThresholdDate =
        import.DisbursementTransaction.ProcessDate;
    }

    if (entities.DisbursementSummary.Populated)
    {
      // -- Update the existing disbursement summary record.
      try
      {
        UpdateDisbursementSummary();
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "DISBURSEMENT_SUMMARY_NU_RB";

            break;
          case ErrorCode.PermittedValueViolation:
            ExitState = "DISBURSEMENT_SUMMARY_PV_RB";

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
      // -- Create a new disbursement summary record.
      if (!ReadObligee())
      {
        ExitState = "FN0000_OBLIGEE_NF_RB";

        return;
      }

      if (!ReadObligor())
      {
        ExitState = "FN0000_OBLIGOR_NF_RB";

        return;
      }

      try
      {
        CreateDisbursementSummary();
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "DISBURSEMENT_SUMMARY_AE_RB";

            break;
          case ErrorCode.PermittedValueViolation:
            ExitState = "DISBURSEMENT_SUMMARY_PV_RB";

            break;
          case ErrorCode.DatabaseError:
            break;
          default:
            throw;
        }
      }
    }
  }

  private static void MoveDisbursementSummary(DisbursementSummary source,
    DisbursementSummary target)
  {
    target.NonTafAmount = source.NonTafAmount;
    target.ThresholdDate = source.ThresholdDate;
  }

  private void CreateDisbursementSummary()
  {
    System.Diagnostics.Debug.Assert(entities.Obligee2.Populated);
    System.Diagnostics.Debug.Assert(entities.Obligor2.Populated);

    var fiscalYear = local.FiscalYear.Year;
    var nonTafAmount =
      local.DisbursementSummary.NonTafAmount.GetValueOrDefault();
    var thresholdDate = local.DisbursementSummary.ThresholdDate;
    var createdBy = global.UserId;
    var createdTstamp = Now();
    var cspNumberOblgee = entities.Obligee2.CspNumber;
    var cpaTypeOblgee = entities.Obligee2.Type1;
    var cspNumberOblgr = entities.Obligor2.CspNumber;
    var cpaTypeOblgr = entities.Obligor2.Type1;

    CheckValid<DisbursementSummary>("CpaTypeOblgee", cpaTypeOblgee);
    CheckValid<DisbursementSummary>("CpaTypeOblgr", cpaTypeOblgr);
    entities.DisbursementSummary.Populated = false;
    Update("CreateDisbursementSummary",
      (db, command) =>
      {
        db.SetInt32(command, "fiscalYear", fiscalYear);
        db.SetNullableDecimal(command, "nonTafAmount", nonTafAmount);
        db.SetNullableDate(command, "thresholdDate", thresholdDate);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTstamp", createdTstamp);
        db.SetNullableString(command, "lastUpdatedBy", "");
        db.SetNullableDateTime(command, "lastUpdTstamp", null);
        db.SetString(command, "cspNumberOblgee", cspNumberOblgee);
        db.SetString(command, "cpaTypeOblgee", cpaTypeOblgee);
        db.SetString(command, "cspNumberOblgr", cspNumberOblgr);
        db.SetString(command, "cpaTypeOblgr", cpaTypeOblgr);
      });

    entities.DisbursementSummary.FiscalYear = fiscalYear;
    entities.DisbursementSummary.NonTafAmount = nonTafAmount;
    entities.DisbursementSummary.ThresholdDate = thresholdDate;
    entities.DisbursementSummary.CreatedBy = createdBy;
    entities.DisbursementSummary.CreatedTstamp = createdTstamp;
    entities.DisbursementSummary.LastUpdatedBy = "";
    entities.DisbursementSummary.LastUpdatedTstamp = null;
    entities.DisbursementSummary.CspNumberOblgee = cspNumberOblgee;
    entities.DisbursementSummary.CpaTypeOblgee = cpaTypeOblgee;
    entities.DisbursementSummary.CspNumberOblgr = cspNumberOblgr;
    entities.DisbursementSummary.CpaTypeOblgr = cpaTypeOblgr;
    entities.DisbursementSummary.Populated = true;
  }

  private bool ReadDisbursementSummary()
  {
    entities.DisbursementSummary.Populated = false;

    return Read("ReadDisbursementSummary",
      (db, command) =>
      {
        db.SetInt32(command, "fiscalYear", local.FiscalYear.Year);
        db.SetString(command, "cspNumberOblgee", import.Obligee.Number);
        db.SetString(command, "cspNumberOblgr", import.Obligor.Number);
      },
      (db, reader) =>
      {
        entities.DisbursementSummary.FiscalYear = db.GetInt32(reader, 0);
        entities.DisbursementSummary.NonTafAmount =
          db.GetNullableDecimal(reader, 1);
        entities.DisbursementSummary.ThresholdDate =
          db.GetNullableDate(reader, 2);
        entities.DisbursementSummary.CreatedBy = db.GetString(reader, 3);
        entities.DisbursementSummary.CreatedTstamp = db.GetDateTime(reader, 4);
        entities.DisbursementSummary.LastUpdatedBy =
          db.GetNullableString(reader, 5);
        entities.DisbursementSummary.LastUpdatedTstamp =
          db.GetNullableDateTime(reader, 6);
        entities.DisbursementSummary.CspNumberOblgee = db.GetString(reader, 7);
        entities.DisbursementSummary.CpaTypeOblgee = db.GetString(reader, 8);
        entities.DisbursementSummary.CspNumberOblgr = db.GetString(reader, 9);
        entities.DisbursementSummary.CpaTypeOblgr = db.GetString(reader, 10);
        entities.DisbursementSummary.Populated = true;
        CheckValid<DisbursementSummary>("CpaTypeOblgee",
          entities.DisbursementSummary.CpaTypeOblgee);
        CheckValid<DisbursementSummary>("CpaTypeOblgr",
          entities.DisbursementSummary.CpaTypeOblgr);
      });
  }

  private bool ReadObligee()
  {
    entities.Obligee2.Populated = false;

    return Read("ReadObligee",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", import.Obligee.Number);
      },
      (db, reader) =>
      {
        entities.Obligee2.CspNumber = db.GetString(reader, 0);
        entities.Obligee2.Type1 = db.GetString(reader, 1);
        entities.Obligee2.Populated = true;
        CheckValid<CsePersonAccount>("Type1", entities.Obligee2.Type1);
      });
  }

  private bool ReadObligor()
  {
    entities.Obligor2.Populated = false;

    return Read("ReadObligor",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", import.Obligor.Number);
      },
      (db, reader) =>
      {
        entities.Obligor2.CspNumber = db.GetString(reader, 0);
        entities.Obligor2.Type1 = db.GetString(reader, 1);
        entities.Obligor2.Populated = true;
        CheckValid<CsePersonAccount>("Type1", entities.Obligor2.Type1);
      });
  }

  private void UpdateDisbursementSummary()
  {
    System.Diagnostics.Debug.Assert(entities.DisbursementSummary.Populated);

    var nonTafAmount =
      local.DisbursementSummary.NonTafAmount.GetValueOrDefault();
    var thresholdDate = local.DisbursementSummary.ThresholdDate;
    var lastUpdatedBy = global.UserId;
    var lastUpdatedTstamp = Now();

    entities.DisbursementSummary.Populated = false;
    Update("UpdateDisbursementSummary",
      (db, command) =>
      {
        db.SetNullableDecimal(command, "nonTafAmount", nonTafAmount);
        db.SetNullableDate(command, "thresholdDate", thresholdDate);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetNullableDateTime(command, "lastUpdTstamp", lastUpdatedTstamp);
        db.SetInt32(
          command, "fiscalYear", entities.DisbursementSummary.FiscalYear);
        db.SetString(
          command, "cspNumberOblgee",
          entities.DisbursementSummary.CspNumberOblgee);
        db.SetString(
          command, "cpaTypeOblgee", entities.DisbursementSummary.CpaTypeOblgee);
          
        db.SetString(
          command, "cspNumberOblgr",
          entities.DisbursementSummary.CspNumberOblgr);
        db.SetString(
          command, "cpaTypeOblgr", entities.DisbursementSummary.CpaTypeOblgr);
      });

    entities.DisbursementSummary.NonTafAmount = nonTafAmount;
    entities.DisbursementSummary.ThresholdDate = thresholdDate;
    entities.DisbursementSummary.LastUpdatedBy = lastUpdatedBy;
    entities.DisbursementSummary.LastUpdatedTstamp = lastUpdatedTstamp;
    entities.DisbursementSummary.Populated = true;
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
    /// A value of DisbursementTransaction.
    /// </summary>
    [JsonPropertyName("disbursementTransaction")]
    public DisbursementTransaction DisbursementTransaction
    {
      get => disbursementTransaction ??= new();
      set => disbursementTransaction = value;
    }

    /// <summary>
    /// A value of Obligor.
    /// </summary>
    [JsonPropertyName("obligor")]
    public CsePerson Obligor
    {
      get => obligor ??= new();
      set => obligor = value;
    }

    /// <summary>
    /// A value of Obligee.
    /// </summary>
    [JsonPropertyName("obligee")]
    public CsePerson Obligee
    {
      get => obligee ??= new();
      set => obligee = value;
    }

    private DisbursementTransaction disbursementTransaction;
    private CsePerson obligor;
    private CsePerson obligee;
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
    /// A value of DisbursementSummary.
    /// </summary>
    [JsonPropertyName("disbursementSummary")]
    public DisbursementSummary DisbursementSummary
    {
      get => disbursementSummary ??= new();
      set => disbursementSummary = value;
    }

    /// <summary>
    /// A value of FiscalYear.
    /// </summary>
    [JsonPropertyName("fiscalYear")]
    public DateWorkArea FiscalYear
    {
      get => fiscalYear ??= new();
      set => fiscalYear = value;
    }

    /// <summary>
    /// A value of Null1.
    /// </summary>
    [JsonPropertyName("null1")]
    public DateWorkArea Null1
    {
      get => null1 ??= new();
      set => null1 = value;
    }

    private DisbursementSummary disbursementSummary;
    private DateWorkArea fiscalYear;
    private DateWorkArea null1;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of Obligor1.
    /// </summary>
    [JsonPropertyName("obligor1")]
    public CsePerson Obligor1
    {
      get => obligor1 ??= new();
      set => obligor1 = value;
    }

    /// <summary>
    /// A value of Obligee1.
    /// </summary>
    [JsonPropertyName("obligee1")]
    public CsePerson Obligee1
    {
      get => obligee1 ??= new();
      set => obligee1 = value;
    }

    /// <summary>
    /// A value of DisbursementSummary.
    /// </summary>
    [JsonPropertyName("disbursementSummary")]
    public DisbursementSummary DisbursementSummary
    {
      get => disbursementSummary ??= new();
      set => disbursementSummary = value;
    }

    /// <summary>
    /// A value of Obligee2.
    /// </summary>
    [JsonPropertyName("obligee2")]
    public CsePersonAccount Obligee2
    {
      get => obligee2 ??= new();
      set => obligee2 = value;
    }

    /// <summary>
    /// A value of Obligor2.
    /// </summary>
    [JsonPropertyName("obligor2")]
    public CsePersonAccount Obligor2
    {
      get => obligor2 ??= new();
      set => obligor2 = value;
    }

    private CsePerson obligor1;
    private CsePerson obligee1;
    private DisbursementSummary disbursementSummary;
    private CsePersonAccount obligee2;
    private CsePersonAccount obligor2;
  }
#endregion
}
