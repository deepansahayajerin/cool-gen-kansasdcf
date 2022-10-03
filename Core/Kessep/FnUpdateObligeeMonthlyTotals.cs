// Program: FN_UPDATE_OBLIGEE_MONTHLY_TOTALS, ID: 372544589, model: 746.
// Short name: SWE00666
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: FN_UPDATE_OBLIGEE_MONTHLY_TOTALS.
/// </para>
/// <para>
/// RESP: FINANCE
/// This action block will update the monthly totals for an Obligee.
/// </para>
/// </summary>
[Serializable]
public partial class FnUpdateObligeeMonthlyTotals: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_UPDATE_OBLIGEE_MONTHLY_TOTALS program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnUpdateObligeeMonthlyTotals(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnUpdateObligeeMonthlyTotals.
  /// </summary>
  public FnUpdateObligeeMonthlyTotals(IContext context, Import import,
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
    // ----------------------------------------------------
    // Initial version - ????
    // K. Doshi - 05/24/2000 - Work Order # 164
    // Add tot_excess_ura_amount views and set statements.
    // 01/09/02  Fangman  WR 000235 PSUM Redesign.  Added new attributes & 
    // removed attributes no longer used.  Changed code to support new
    // definition of table.
    // ----------------------------------------------------
    if (ReadMonthlyObligeeSummary())
    {
      local.Previous.CollectionsDisbursedToAr =
        entities.MonthlyObligeeSummary.CollectionsDisbursedToAr;

      try
      {
        UpdateMonthlyObligeeSummary();
        export.MonthlyObligeeSummary.Assign(entities.MonthlyObligeeSummary);

        if (Equal(global.UserId, "SWEFB651") || Equal
          (global.UserId, "SWEFB653") || Equal(global.UserId, "SWEFB641") || Equal
          (global.UserId, "SWEFB666"))
        {
          if (!Lt(entities.MonthlyObligeeSummary.CollectionsDisbursedToAr, 1000) &&
            local.Previous.CollectionsDisbursedToAr.GetValueOrDefault() < 1000)
          {
            if (ReadCsePerson())
            {
              local.CsePerson.Number = entities.CsePerson.Number;
            }

            UseSp1000ExternalAlert();
          }
        }
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "FN0000_MTH_OBLIGEE_SUMMARY_NU_RB";

            break;
          case ErrorCode.PermittedValueViolation:
            ExitState = "FN0000_MTH_OBLIGEE_SUMMARY_PV_RB";

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
        export.MonthlyObligeeSummary.Assign(entities.MonthlyObligeeSummary);

        if (Equal(global.UserId, "SWEFB651") || Equal
          (global.UserId, "SWEFB653") || Equal(global.UserId, "SWEFB641") || Equal
          (global.UserId, "SWEFB666"))
        {
          if (!Lt(entities.MonthlyObligeeSummary.CollectionsDisbursedToAr, 1000))
            
          {
            if (ReadCsePerson())
            {
              local.CsePerson.Number = entities.CsePerson.Number;
            }

            UseSp1000ExternalAlert();
          }
        }
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "FN0000_MTH_OBLIGEE_SUM_AE_RB";

            break;
          case ErrorCode.PermittedValueViolation:
            ExitState = "FN0000_MTH_OBLIGEE_SUMMARY_PV_RB";

            break;
          case ErrorCode.DatabaseError:
            break;
          default:
            throw;
        }
      }
    }
  }

  private static void MoveMonthlyObligeeSummary(MonthlyObligeeSummary source,
    MonthlyObligeeSummary target)
  {
    target.Year = source.Year;
    target.Month = source.Month;
    target.CollectionsDisbursedToAr = source.CollectionsDisbursedToAr;
  }

  private void UseSp1000ExternalAlert()
  {
    var useImport = new Sp1000ExternalAlert.Import();
    var useExport = new Sp1000ExternalAlert.Export();

    MoveMonthlyObligeeSummary(entities.MonthlyObligeeSummary,
      useImport.MonthlyObligeeSummary);
    useImport.CsePerson.Number = local.CsePerson.Number;

    Call(Sp1000ExternalAlert.Execute, useImport, useExport);
  }

  private void CreateMonthlyObligeeSummary()
  {
    System.Diagnostics.Debug.Assert(import.Per.Populated);

    var year = import.MonthlyObligeeSummary.Year;
    var month = import.MonthlyObligeeSummary.Month;
    var passthruRecapAmt =
      import.MonthlyObligeeSummary.PassthruRecapAmt.GetValueOrDefault();
    var disbursementsSuppressed =
      import.MonthlyObligeeSummary.DisbursementsSuppressed.GetValueOrDefault();
    var recapturedAmt =
      import.MonthlyObligeeSummary.RecapturedAmt.GetValueOrDefault();
    var naArrearsRecapAmt =
      import.MonthlyObligeeSummary.NaArrearsRecapAmt.GetValueOrDefault();
    var passthruAmount = import.MonthlyObligeeSummary.PassthruAmount;
    var createdBy = global.UserId;
    var createdTimestamp = Now();
    var adcReimbursedAmount =
      import.MonthlyObligeeSummary.AdcReimbursedAmount.GetValueOrDefault();
    var cpaSType = import.Per.Type1;
    var cspSNumber = import.Per.CspNumber;
    var collectionsAmount =
      import.MonthlyObligeeSummary.CollectionsAmount.GetValueOrDefault();
    var collectionsDisbursedToAr =
      import.MonthlyObligeeSummary.CollectionsDisbursedToAr.GetValueOrDefault();
      
    var feeAmount = import.MonthlyObligeeSummary.FeeAmount.GetValueOrDefault();
    var totExcessUraAmt =
      import.MonthlyObligeeSummary.TotExcessUraAmt.GetValueOrDefault();
    var numberOfCollections =
      import.MonthlyObligeeSummary.NumberOfCollections.GetValueOrDefault();
    var naCurrRecapAmt =
      import.MonthlyObligeeSummary.NaCurrRecapAmt.GetValueOrDefault();

    CheckValid<MonthlyObligeeSummary>("CpaSType", cpaSType);
    entities.MonthlyObligeeSummary.Populated = false;
    Update("CreateMonthlyObligeeSummary",
      (db, command) =>
      {
        db.SetInt32(command, "yer", year);
        db.SetInt32(command, "mnth", month);
        db.SetNullableDecimal(command, "ptRecapAmt", passthruRecapAmt);
        db.SetNullableDecimal(
          command, "disbursementsSupp", disbursementsSuppressed);
        db.SetNullableDecimal(command, "recapturedAmt", recapturedAmt);
        db.SetNullableDecimal(command, "naArsRecapAmt", naArrearsRecapAmt);
        db.SetDecimal(command, "passthruAmount", passthruAmount);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetNullableString(command, "lastUpdatedBy", "");
        db.SetNullableDateTime(command, "lastUpdatedTmst", null);
        db.SetNullableDecimal(command, "adcReimbursedAmt", adcReimbursedAmount);
        db.SetString(command, "cpaSType", cpaSType);
        db.SetString(command, "cspSNumber", cspSNumber);
        db.SetNullableDecimal(command, "collectionAmt", collectionsAmount);
        db.SetNullableDecimal(
          command, "collctnDsbToAr", collectionsDisbursedToAr);
        db.SetNullableDecimal(command, "feeAmt", feeAmount);
        db.SetNullableString(command, "zdelType", "");
        db.SetNullableDecimal(command, "totExcessUraAmt", totExcessUraAmt);
        db.SetNullableInt32(command, "nbrOfCollections", numberOfCollections);
        db.SetNullableDecimal(command, "naCurrRecapAmt", naCurrRecapAmt);
      });

    entities.MonthlyObligeeSummary.Year = year;
    entities.MonthlyObligeeSummary.Month = month;
    entities.MonthlyObligeeSummary.PassthruRecapAmt = passthruRecapAmt;
    entities.MonthlyObligeeSummary.DisbursementsSuppressed =
      disbursementsSuppressed;
    entities.MonthlyObligeeSummary.RecapturedAmt = recapturedAmt;
    entities.MonthlyObligeeSummary.NaArrearsRecapAmt = naArrearsRecapAmt;
    entities.MonthlyObligeeSummary.PassthruAmount = passthruAmount;
    entities.MonthlyObligeeSummary.CreatedBy = createdBy;
    entities.MonthlyObligeeSummary.CreatedTimestamp = createdTimestamp;
    entities.MonthlyObligeeSummary.LastUpdatedBy = "";
    entities.MonthlyObligeeSummary.LastUpdatedTmst = null;
    entities.MonthlyObligeeSummary.AdcReimbursedAmount = adcReimbursedAmount;
    entities.MonthlyObligeeSummary.CpaSType = cpaSType;
    entities.MonthlyObligeeSummary.CspSNumber = cspSNumber;
    entities.MonthlyObligeeSummary.CollectionsAmount = collectionsAmount;
    entities.MonthlyObligeeSummary.CollectionsDisbursedToAr =
      collectionsDisbursedToAr;
    entities.MonthlyObligeeSummary.FeeAmount = feeAmount;
    entities.MonthlyObligeeSummary.TotExcessUraAmt = totExcessUraAmt;
    entities.MonthlyObligeeSummary.NumberOfCollections = numberOfCollections;
    entities.MonthlyObligeeSummary.NaCurrRecapAmt = naCurrRecapAmt;
    entities.MonthlyObligeeSummary.Populated = true;
  }

  private bool ReadCsePerson()
  {
    System.Diagnostics.Debug.Assert(import.Per.Populated);
    entities.CsePerson.Populated = false;

    return Read("ReadCsePerson",
      (db, command) =>
      {
        db.SetString(command, "numb", import.Per.CspNumber);
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CsePerson.Populated = true;
      });
  }

  private bool ReadMonthlyObligeeSummary()
  {
    System.Diagnostics.Debug.Assert(import.Per.Populated);
    entities.MonthlyObligeeSummary.Populated = false;

    return Read("ReadMonthlyObligeeSummary",
      (db, command) =>
      {
        db.SetInt32(command, "yer", import.MonthlyObligeeSummary.Year);
        db.SetInt32(command, "mnth", import.MonthlyObligeeSummary.Month);
        db.SetString(command, "cpaSType", import.Per.Type1);
        db.SetString(command, "cspSNumber", import.Per.CspNumber);
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
        entities.MonthlyObligeeSummary.CspSNumber = db.GetString(reader, 13);
        entities.MonthlyObligeeSummary.CollectionsAmount =
          db.GetNullableDecimal(reader, 14);
        entities.MonthlyObligeeSummary.CollectionsDisbursedToAr =
          db.GetNullableDecimal(reader, 15);
        entities.MonthlyObligeeSummary.FeeAmount =
          db.GetNullableDecimal(reader, 16);
        entities.MonthlyObligeeSummary.TotExcessUraAmt =
          db.GetNullableDecimal(reader, 17);
        entities.MonthlyObligeeSummary.NumberOfCollections =
          db.GetNullableInt32(reader, 18);
        entities.MonthlyObligeeSummary.NaCurrRecapAmt =
          db.GetNullableDecimal(reader, 19);
        entities.MonthlyObligeeSummary.Populated = true;
        CheckValid<MonthlyObligeeSummary>("CpaSType",
          entities.MonthlyObligeeSummary.CpaSType);
      });
  }

  private void UpdateMonthlyObligeeSummary()
  {
    System.Diagnostics.Debug.Assert(entities.MonthlyObligeeSummary.Populated);

    var passthruRecapAmt =
      entities.MonthlyObligeeSummary.PassthruRecapAmt.GetValueOrDefault() +
      import.MonthlyObligeeSummary.PassthruRecapAmt.GetValueOrDefault();
    var disbursementsSuppressed =
      entities.MonthlyObligeeSummary.DisbursementsSuppressed.
        GetValueOrDefault() +
      import.MonthlyObligeeSummary.DisbursementsSuppressed.GetValueOrDefault();
    var recapturedAmt =
      entities.MonthlyObligeeSummary.RecapturedAmt.GetValueOrDefault() +
      import.MonthlyObligeeSummary.RecapturedAmt.GetValueOrDefault();
    var naArrearsRecapAmt =
      entities.MonthlyObligeeSummary.NaArrearsRecapAmt.GetValueOrDefault() +
      import.MonthlyObligeeSummary.NaArrearsRecapAmt.GetValueOrDefault();
    var passthruAmount =
      entities.MonthlyObligeeSummary.PassthruAmount +
      import.MonthlyObligeeSummary.PassthruAmount;
    var lastUpdatedBy = global.UserId;
    var lastUpdatedTmst = Now();
    var adcReimbursedAmount =
      entities.MonthlyObligeeSummary.AdcReimbursedAmount.GetValueOrDefault() +
      import.MonthlyObligeeSummary.AdcReimbursedAmount.GetValueOrDefault();
    var collectionsAmount =
      entities.MonthlyObligeeSummary.CollectionsAmount.GetValueOrDefault() +
      import.MonthlyObligeeSummary.CollectionsAmount.GetValueOrDefault();
    var collectionsDisbursedToAr =
      entities.MonthlyObligeeSummary.CollectionsDisbursedToAr.
        GetValueOrDefault() +
      import.MonthlyObligeeSummary.CollectionsDisbursedToAr.GetValueOrDefault();
      
    var feeAmount =
      entities.MonthlyObligeeSummary.FeeAmount.GetValueOrDefault() +
      import.MonthlyObligeeSummary.FeeAmount.GetValueOrDefault();
    var totExcessUraAmt =
      entities.MonthlyObligeeSummary.TotExcessUraAmt.GetValueOrDefault() +
      import.MonthlyObligeeSummary.TotExcessUraAmt.GetValueOrDefault();
    var numberOfCollections =
      entities.MonthlyObligeeSummary.NumberOfCollections.GetValueOrDefault() +
      import.MonthlyObligeeSummary.NumberOfCollections.GetValueOrDefault();
    var naCurrRecapAmt =
      entities.MonthlyObligeeSummary.NaCurrRecapAmt.GetValueOrDefault() +
      import.MonthlyObligeeSummary.NaCurrRecapAmt.GetValueOrDefault();

    entities.MonthlyObligeeSummary.Populated = false;
    Update("UpdateMonthlyObligeeSummary",
      (db, command) =>
      {
        db.SetNullableDecimal(command, "ptRecapAmt", passthruRecapAmt);
        db.SetNullableDecimal(
          command, "disbursementsSupp", disbursementsSuppressed);
        db.SetNullableDecimal(command, "recapturedAmt", recapturedAmt);
        db.SetNullableDecimal(command, "naArsRecapAmt", naArrearsRecapAmt);
        db.SetDecimal(command, "passthruAmount", passthruAmount);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTmst);
        db.SetNullableDecimal(command, "adcReimbursedAmt", adcReimbursedAmount);
        db.SetNullableDecimal(command, "collectionAmt", collectionsAmount);
        db.SetNullableDecimal(
          command, "collctnDsbToAr", collectionsDisbursedToAr);
        db.SetNullableDecimal(command, "feeAmt", feeAmount);
        db.SetNullableDecimal(command, "totExcessUraAmt", totExcessUraAmt);
        db.SetNullableInt32(command, "nbrOfCollections", numberOfCollections);
        db.SetNullableDecimal(command, "naCurrRecapAmt", naCurrRecapAmt);
        db.SetInt32(command, "yer", entities.MonthlyObligeeSummary.Year);
        db.SetInt32(command, "mnth", entities.MonthlyObligeeSummary.Month);
        db.SetString(
          command, "cpaSType", entities.MonthlyObligeeSummary.CpaSType);
        db.SetString(
          command, "cspSNumber", entities.MonthlyObligeeSummary.CspSNumber);
      });

    entities.MonthlyObligeeSummary.PassthruRecapAmt = passthruRecapAmt;
    entities.MonthlyObligeeSummary.DisbursementsSuppressed =
      disbursementsSuppressed;
    entities.MonthlyObligeeSummary.RecapturedAmt = recapturedAmt;
    entities.MonthlyObligeeSummary.NaArrearsRecapAmt = naArrearsRecapAmt;
    entities.MonthlyObligeeSummary.PassthruAmount = passthruAmount;
    entities.MonthlyObligeeSummary.LastUpdatedBy = lastUpdatedBy;
    entities.MonthlyObligeeSummary.LastUpdatedTmst = lastUpdatedTmst;
    entities.MonthlyObligeeSummary.AdcReimbursedAmount = adcReimbursedAmount;
    entities.MonthlyObligeeSummary.CollectionsAmount = collectionsAmount;
    entities.MonthlyObligeeSummary.CollectionsDisbursedToAr =
      collectionsDisbursedToAr;
    entities.MonthlyObligeeSummary.FeeAmount = feeAmount;
    entities.MonthlyObligeeSummary.TotExcessUraAmt = totExcessUraAmt;
    entities.MonthlyObligeeSummary.NumberOfCollections = numberOfCollections;
    entities.MonthlyObligeeSummary.NaCurrRecapAmt = naCurrRecapAmt;
    entities.MonthlyObligeeSummary.Populated = true;
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
    /// A value of Per.
    /// </summary>
    [JsonPropertyName("per")]
    public CsePersonAccount Per
    {
      get => per ??= new();
      set => per = value;
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

    private CsePersonAccount per;
    private MonthlyObligeeSummary monthlyObligeeSummary;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
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

    private MonthlyObligeeSummary monthlyObligeeSummary;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of Previous.
    /// </summary>
    [JsonPropertyName("previous")]
    public MonthlyObligeeSummary Previous
    {
      get => previous ??= new();
      set => previous = value;
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

    private MonthlyObligeeSummary previous;
    private CsePerson csePerson;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
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

    private CsePerson csePerson;
    private MonthlyObligeeSummary monthlyObligeeSummary;
  }
#endregion
}
