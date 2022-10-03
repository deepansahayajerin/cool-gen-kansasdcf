// Program: FN_B652_MARK_DISB_PASSTHRU_PRCSD, ID: 372708319, model: 746.
// Short name: SWE02155
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: FN_B652_MARK_DISB_PASSTHRU_PRCSD.
/// </para>
/// <para>
/// RESP: FINANCE
/// This action block marks the ADC Current Collection Disbursement Transaction 
/// (Debit) as processed.
/// </para>
/// </summary>
[Serializable]
public partial class FnB652MarkDisbPassthruPrcsd: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_B652_MARK_DISB_PASSTHRU_PRCSD program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnB652MarkDisbPassthruPrcsd(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnB652MarkDisbPassthruPrcsd.
  /// </summary>
  public FnB652MarkDisbPassthruPrcsd(IContext context, Import import,
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
    // ---------------------------------------------
    // This action block marks the ADC Current Collection Disbursement (Debit) 
    // Transaction as processed for passthru.
    // ---------------------------------------------
    // ---------------------------------------------
    // Date	By	IDCR#	Description
    // ---------------------------------------------
    // 112097	govind		Initial code
    // 081399  npe             Made it persistent.
    // ---------------------------------------------
    local.Current.Timestamp = Now();

    try
    {
      UpdateDisbursementTransaction();
    }
    catch(Exception e)
    {
      switch(GetErrorCode(e))
      {
        case ErrorCode.AlreadyExists:
          ExitState = "FN0000_DISB_TRANS_NU_RB";

          break;
        case ErrorCode.PermittedValueViolation:
          ExitState = "FN0000_DISB_TRANSACTION_PV";

          break;
        case ErrorCode.DatabaseError:
          break;
        default:
          throw;
      }
    }
  }

  private void UpdateDisbursementTransaction()
  {
    System.Diagnostics.Debug.Assert(import.Persistent.Populated);

    var lastUpdatedBy = global.UserId;
    var lastUpdateTmst = local.Current.Timestamp;
    var passthruProcDate = import.ProgramProcessingInfo.ProcessDate;

    import.Persistent.Populated = false;
    Update("UpdateDisbursementTransaction",
      (db, command) =>
      {
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetNullableDateTime(command, "lastUpdateTmst", lastUpdateTmst);
        db.SetNullableDate(command, "passthruProcDate", passthruProcDate);
        db.SetString(command, "cpaType", import.Persistent.CpaType);
        db.SetString(command, "cspNumber", import.Persistent.CspNumber);
        db.SetInt32(
          command, "disbTranId", import.Persistent.SystemGeneratedIdentifier);
      });

    import.Persistent.LastUpdatedBy = lastUpdatedBy;
    import.Persistent.LastUpdateTmst = lastUpdateTmst;
    import.Persistent.PassthruProcDate = passthruProcDate;
    import.Persistent.Populated = true;
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
    /// A value of Persistent.
    /// </summary>
    [JsonPropertyName("persistent")]
    public DisbursementTransaction Persistent
    {
      get => persistent ??= new();
      set => persistent = value;
    }

    /// <summary>
    /// A value of ProgramProcessingInfo.
    /// </summary>
    [JsonPropertyName("programProcessingInfo")]
    public ProgramProcessingInfo ProgramProcessingInfo
    {
      get => programProcessingInfo ??= new();
      set => programProcessingInfo = value;
    }

    /// <summary>
    /// A value of AdcCurrCollDisbDebit.
    /// </summary>
    [JsonPropertyName("adcCurrCollDisbDebit")]
    public DisbursementTransaction AdcCurrCollDisbDebit
    {
      get => adcCurrCollDisbDebit ??= new();
      set => adcCurrCollDisbDebit = value;
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

    private DisbursementTransaction persistent;
    private ProgramProcessingInfo programProcessingInfo;
    private DisbursementTransaction adcCurrCollDisbDebit;
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
    /// A value of Initialized.
    /// </summary>
    [JsonPropertyName("initialized")]
    public DateWorkArea Initialized
    {
      get => initialized ??= new();
      set => initialized = value;
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

    private DateWorkArea initialized;
    private DateWorkArea current;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of DisbursementTransactionRln.
    /// </summary>
    [JsonPropertyName("disbursementTransactionRln")]
    public DisbursementTransactionRln DisbursementTransactionRln
    {
      get => disbursementTransactionRln ??= new();
      set => disbursementTransactionRln = value;
    }

    /// <summary>
    /// A value of Credit.
    /// </summary>
    [JsonPropertyName("credit")]
    public DisbursementTransaction Credit
    {
      get => credit ??= new();
      set => credit = value;
    }

    /// <summary>
    /// A value of Obligee1.
    /// </summary>
    [JsonPropertyName("obligee1")]
    public CsePersonAccount Obligee1
    {
      get => obligee1 ??= new();
      set => obligee1 = value;
    }

    /// <summary>
    /// A value of AdcCurrCollDisbDebit.
    /// </summary>
    [JsonPropertyName("adcCurrCollDisbDebit")]
    public DisbursementTransaction AdcCurrCollDisbDebit
    {
      get => adcCurrCollDisbDebit ??= new();
      set => adcCurrCollDisbDebit = value;
    }

    /// <summary>
    /// A value of Obligee2.
    /// </summary>
    [JsonPropertyName("obligee2")]
    public CsePerson Obligee2
    {
      get => obligee2 ??= new();
      set => obligee2 = value;
    }

    private DisbursementTransactionRln disbursementTransactionRln;
    private DisbursementTransaction credit;
    private CsePersonAccount obligee1;
    private DisbursementTransaction adcCurrCollDisbDebit;
    private CsePerson obligee2;
  }
#endregion
}
