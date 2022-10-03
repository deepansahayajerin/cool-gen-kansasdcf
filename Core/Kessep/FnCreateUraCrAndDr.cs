// Program: FN_CREATE_URA_CR_AND_DR, ID: 372551168, model: 746.
// Short name: SWE02011
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_CREATE_URA_CR_AND_DR.
/// </summary>
[Serializable]
public partial class FnCreateUraCrAndDr: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_CREATE_URA_CR_AND_DR program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnCreateUraCrAndDr(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnCreateUraCrAndDr.
  /// </summary>
  public FnCreateUraCrAndDr(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ------------------------------------------
    // Initial Version - SWSRKXD
    // Date - 3/31/99
    // 1999-11-02  PR 77907  Fangman  Set Disbursement Cash_Non_Cash indicator 
    // to 'C'.
    // ------------------------------------------
    if (!ReadCsePersonAccount())
    {
      // ------------------------------------------
      // 6/3/99- SWSRKXD
      // As per instructions from SME(email message),
      // create obligee when nf.
      // ------------------------------------------
      if (!ReadCsePerson())
      {
        ExitState = "CSE_PERSON_NF";

        return;
      }

      try
      {
        CreateCsePersonAccount();
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "FN0000_CSE_PERSON_ACCOUNT_AE";

            return;
          case ErrorCode.PermittedValueViolation:
            ExitState = "FN0000_CSE_PERSON_ACCOUNT_PV";

            return;
          case ErrorCode.DatabaseError:
            break;
          default:
            throw;
        }
      }
    }

    // ---------------------------------------------------
    // KD - 5/13/99
    // As per disbursement team meeting set DP to Payee. (B641 determines DP 
    // when it processes these disbursements)
    // --------------------------------------------------
    for(local.Retry.Count = 1; local.Retry.Count <= 100; ++local.Retry.Count)
    {
      try
      {
        CreateDisbursementTransaction();

        break;
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ++local.Retry.Count;

            continue;
          case ErrorCode.PermittedValueViolation:
            break;
          case ErrorCode.DatabaseError:
            break;
          default:
            throw;
        }
      }

      ExitState = "FN0000_DISB_TRANSACTION_AE";

      return;
    }

    // ----------------------------------------------------------
    // Check if payment needs to be suppressed - only at Person level
    // ----------------------------------------------------------
    if (ReadDisbSuppressionStatusHistory())
    {
      // --------------------------------------
      // Set status to 'SUPPRESS'
      // --------------------------------------
      local.DisbursementStatus.SystemGeneratedIdentifier = 3;
    }
    else
    {
      // --------------------------------------
      // Set status to 'RELEASE'
      // --------------------------------------
      local.DisbursementStatus.SystemGeneratedIdentifier = 1;
    }

    local.DisbursementTranRlnRsn.SystemGeneratedIdentifier = 1;
    local.DisbursementTransaction.Amount =
      import.DisbursementTransaction.Amount;
    local.DisbursementTransaction.DisbursementDate = Now().Date;
    local.DisbursementTransaction.CashNonCashInd = "C";

    // -----------------------------------------------
    // create debit
    // -----------------------------------------------
    UseFnCreateDisbursement();
  }

  private static void MoveDisbursementTransaction1(
    DisbursementTransaction source, DisbursementTransaction target)
  {
    target.ReferenceNumber = source.ReferenceNumber;
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Type1 = source.Type1;
    target.Amount = source.Amount;
    target.ProcessDate = source.ProcessDate;
    target.CreatedTimestamp = source.CreatedTimestamp;
    target.LastUpdatedBy = source.LastUpdatedBy;
    target.LastUpdateTmst = source.LastUpdateTmst;
    target.CollectionDate = source.CollectionDate;
    target.InterstateInd = source.InterstateInd;
  }

  private static void MoveDisbursementTransaction2(
    DisbursementTransaction source, DisbursementTransaction target)
  {
    target.Type1 = source.Type1;
    target.Amount = source.Amount;
    target.ProcessDate = source.ProcessDate;
    target.DesignatedPayee = source.DesignatedPayee;
    target.DisbursementDate = source.DisbursementDate;
    target.CashNonCashInd = source.CashNonCashInd;
  }

  private void UseFnCreateDisbursement()
  {
    var useImport = new FnCreateDisbursement.Import();
    var useExport = new FnCreateDisbursement.Export();

    MoveDisbursementTransaction1(entities.DisbursementTransaction,
      useImport.Credit);
    useImport.DisbursementType.SystemGeneratedIdentifier =
      import.DisbursementType.SystemGeneratedIdentifier;
    useImport.CsePerson.Number = import.CsePerson.Number;
    MoveDisbursementTransaction2(local.DisbursementTransaction, useImport.New1);
    useImport.DisbursementTranRlnRsn.SystemGeneratedIdentifier =
      local.DisbursementTranRlnRsn.SystemGeneratedIdentifier;
    useImport.DisbursementStatus.SystemGeneratedIdentifier =
      local.DisbursementStatus.SystemGeneratedIdentifier;

    Call(FnCreateDisbursement.Execute, useImport, useExport);
  }

  private int UseGenerate9DigitRandomNumber()
  {
    var useImport = new Generate9DigitRandomNumber.Import();
    var useExport = new Generate9DigitRandomNumber.Export();

    Call(Generate9DigitRandomNumber.Execute, useImport, useExport);

    return useExport.SystemGenerated.Attribute9DigitRandomNumber;
  }

  private void CreateCsePersonAccount()
  {
    var cspNumber = entities.CsePerson.Number;
    var type1 = "E";
    var createdBy = global.UserId;
    var createdTmst = Now();

    CheckValid<CsePersonAccount>("Type1", type1);
    entities.CsePersonAccount.Populated = false;
    Update("CreateCsePersonAccount",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", cspNumber);
        db.SetString(command, "type", type1);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTmst", createdTmst);
        db.SetNullableString(command, "lastUpdatedBy", "");
        db.SetNullableDateTime(command, "lastUpdatedTmst", default(DateTime));
        db.SetNullableDate(command, "recompBalFromDt", default(DateTime));
        db.SetNullableDecimal(command, "stdTotGiftColl", 0M);
        db.SetNullableString(command, "triggerType", "");
      });

    entities.CsePersonAccount.CspNumber = cspNumber;
    entities.CsePersonAccount.Type1 = type1;
    entities.CsePersonAccount.CreatedBy = createdBy;
    entities.CsePersonAccount.CreatedTmst = createdTmst;
    entities.CsePersonAccount.Populated = true;
  }

  private void CreateDisbursementTransaction()
  {
    System.Diagnostics.Debug.Assert(entities.CsePersonAccount.Populated);

    var cpaType = entities.CsePersonAccount.Type1;
    var cspNumber = entities.CsePersonAccount.CspNumber;
    var systemGeneratedIdentifier = UseGenerate9DigitRandomNumber();
    var type1 = "X";
    var amount = import.DisbursementTransaction.Amount;
    var processDate = Now().Date;
    var createdBy = global.UserId;
    var createdTimestamp = Now();
    var interstateInd = "N";
    var designatedPayee = import.CsePerson.Number;
    var uraExcessCollSeqNbr = import.UraExcessCollection.SequenceNumber;

    CheckValid<DisbursementTransaction>("CpaType", cpaType);
    CheckValid<DisbursementTransaction>("Type1", type1);
    entities.DisbursementTransaction.Populated = false;
    Update("CreateDisbursementTransaction",
      (db, command) =>
      {
        db.SetString(command, "cpaType", cpaType);
        db.SetString(command, "cspNumber", cspNumber);
        db.SetInt32(command, "disbTranId", systemGeneratedIdentifier);
        db.SetString(command, "type", type1);
        db.SetDecimal(command, "amount", amount);
        db.SetNullableDate(command, "processDate", processDate);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetNullableString(command, "lastUpdatedBy", "");
        db.SetNullableDateTime(command, "lastUpdateTmst", null);
        db.SetNullableDate(command, "disbursementDate", default(DateTime));
        db.SetString(
          command, "cashNonCashInd", GetImplicitValue<DisbursementTransaction,
          string>("CashNonCashInd"));
        db.SetString(command, "recapturedInd", "");
        db.SetNullableDate(command, "collectionDate", processDate);
        db.SetNullableString(
          command, "otrTypeDisb", GetImplicitValue<DisbursementTransaction,
          string>("OtrTypeDisb"));
        db.SetNullableString(
          command, "cpaTypeDisb", GetImplicitValue<DisbursementTransaction,
          string>("CpaTypeDisb"));
        db.SetNullableString(command, "interstateInd", interstateInd);
        db.SetNullableDate(command, "passthruProcDate", null);
        db.SetNullableString(command, "designatedPayee", designatedPayee);
        db.SetNullableString(command, "referenceNumber", "");
        db.SetInt32(command, "uraExcollSnbr", uraExcessCollSeqNbr);
      });

    entities.DisbursementTransaction.CpaType = cpaType;
    entities.DisbursementTransaction.CspNumber = cspNumber;
    entities.DisbursementTransaction.SystemGeneratedIdentifier =
      systemGeneratedIdentifier;
    entities.DisbursementTransaction.Type1 = type1;
    entities.DisbursementTransaction.Amount = amount;
    entities.DisbursementTransaction.ProcessDate = processDate;
    entities.DisbursementTransaction.CreatedBy = createdBy;
    entities.DisbursementTransaction.CreatedTimestamp = createdTimestamp;
    entities.DisbursementTransaction.LastUpdatedBy = "";
    entities.DisbursementTransaction.LastUpdateTmst = null;
    entities.DisbursementTransaction.CollectionDate = processDate;
    entities.DisbursementTransaction.InterstateInd = interstateInd;
    entities.DisbursementTransaction.PassthruProcDate = null;
    entities.DisbursementTransaction.DesignatedPayee = designatedPayee;
    entities.DisbursementTransaction.ReferenceNumber = "";
    entities.DisbursementTransaction.UraExcessCollSeqNbr = uraExcessCollSeqNbr;
    entities.DisbursementTransaction.Populated = true;
  }

  private bool ReadCsePerson()
  {
    entities.CsePerson.Populated = false;

    return Read("ReadCsePerson",
      (db, command) =>
      {
        db.SetString(command, "numb", import.CsePerson.Number);
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CsePerson.Populated = true;
      });
  }

  private bool ReadCsePersonAccount()
  {
    entities.CsePersonAccount.Populated = false;

    return Read("ReadCsePersonAccount",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", import.CsePerson.Number);
      },
      (db, reader) =>
      {
        entities.CsePersonAccount.CspNumber = db.GetString(reader, 0);
        entities.CsePersonAccount.Type1 = db.GetString(reader, 1);
        entities.CsePersonAccount.CreatedBy = db.GetString(reader, 2);
        entities.CsePersonAccount.CreatedTmst = db.GetDateTime(reader, 3);
        entities.CsePersonAccount.Populated = true;
        CheckValid<CsePersonAccount>("Type1", entities.CsePersonAccount.Type1);
      });
  }

  private bool ReadDisbSuppressionStatusHistory()
  {
    System.Diagnostics.Debug.Assert(entities.CsePersonAccount.Populated);
    entities.DisbSuppressionStatusHistory.Populated = false;

    return Read("ReadDisbSuppressionStatusHistory",
      (db, command) =>
      {
        var date = Now().Date;

        db.SetString(command, "cpaType", entities.CsePersonAccount.Type1);
        db.SetString(command, "cspNumber", entities.CsePersonAccount.CspNumber);
        db.SetDate(command, "effectiveDate", date);
      },
      (db, reader) =>
      {
        entities.DisbSuppressionStatusHistory.CpaType = db.GetString(reader, 0);
        entities.DisbSuppressionStatusHistory.CspNumber =
          db.GetString(reader, 1);
        entities.DisbSuppressionStatusHistory.SystemGeneratedIdentifier =
          db.GetInt32(reader, 2);
        entities.DisbSuppressionStatusHistory.EffectiveDate =
          db.GetDate(reader, 3);
        entities.DisbSuppressionStatusHistory.DiscontinueDate =
          db.GetDate(reader, 4);
        entities.DisbSuppressionStatusHistory.Type1 = db.GetString(reader, 5);
        entities.DisbSuppressionStatusHistory.Populated = true;
        CheckValid<DisbSuppressionStatusHistory>("CpaType",
          entities.DisbSuppressionStatusHistory.CpaType);
        CheckValid<DisbSuppressionStatusHistory>("Type1",
          entities.DisbSuppressionStatusHistory.Type1);
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
    /// A value of UraExcessCollection.
    /// </summary>
    [JsonPropertyName("uraExcessCollection")]
    public UraExcessCollection UraExcessCollection
    {
      get => uraExcessCollection ??= new();
      set => uraExcessCollection = value;
    }

    /// <summary>
    /// A value of DisbursementType.
    /// </summary>
    [JsonPropertyName("disbursementType")]
    public DisbursementType DisbursementType
    {
      get => disbursementType ??= new();
      set => disbursementType = value;
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
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
    }

    private UraExcessCollection uraExcessCollection;
    private DisbursementType disbursementType;
    private DisbursementTransaction disbursementTransaction;
    private CsePerson csePerson;
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
    /// A value of DisbursementTransaction.
    /// </summary>
    [JsonPropertyName("disbursementTransaction")]
    public DisbursementTransaction DisbursementTransaction
    {
      get => disbursementTransaction ??= new();
      set => disbursementTransaction = value;
    }

    /// <summary>
    /// A value of DisbursementTranRlnRsn.
    /// </summary>
    [JsonPropertyName("disbursementTranRlnRsn")]
    public DisbursementTranRlnRsn DisbursementTranRlnRsn
    {
      get => disbursementTranRlnRsn ??= new();
      set => disbursementTranRlnRsn = value;
    }

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
    /// A value of Retry.
    /// </summary>
    [JsonPropertyName("retry")]
    public Common Retry
    {
      get => retry ??= new();
      set => retry = value;
    }

    /// <summary>
    /// A value of Dp.
    /// </summary>
    [JsonPropertyName("dp")]
    public CsePerson Dp
    {
      get => dp ??= new();
      set => dp = value;
    }

    private DisbursementTransaction disbursementTransaction;
    private DisbursementTranRlnRsn disbursementTranRlnRsn;
    private DisbursementStatus disbursementStatus;
    private Common retry;
    private CsePerson dp;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of DisbSuppressionStatusHistory.
    /// </summary>
    [JsonPropertyName("disbSuppressionStatusHistory")]
    public DisbSuppressionStatusHistory DisbSuppressionStatusHistory
    {
      get => disbSuppressionStatusHistory ??= new();
      set => disbSuppressionStatusHistory = value;
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
    /// A value of CsePersonAccount.
    /// </summary>
    [JsonPropertyName("csePersonAccount")]
    public CsePersonAccount CsePersonAccount
    {
      get => csePersonAccount ??= new();
      set => csePersonAccount = value;
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
    /// A value of Obligee.
    /// </summary>
    [JsonPropertyName("obligee")]
    public CsePerson Obligee
    {
      get => obligee ??= new();
      set => obligee = value;
    }

    private DisbSuppressionStatusHistory disbSuppressionStatusHistory;
    private DisbursementTransaction disbursementTransaction;
    private CsePersonAccount csePersonAccount;
    private CsePerson csePerson;
    private CsePerson obligee;
  }
#endregion
}
