// Program: FN_B652_CREATE_PASSTHRU_DISB_DEB, ID: 372709029, model: 746.
// Short name: SWE02152
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: FN_B652_CREATE_PASSTHRU_DISB_DEB.
/// </para>
/// <para>
/// RESP: FINANCE
/// This action block will create the debit side of the passthru (disbursement 
/// disbursement_transaction).  It will be related to the credit side (passthru
/// disbursement_transaction).
/// </para>
/// </summary>
[Serializable]
public partial class FnB652CreatePassthruDisbDeb: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_B652_CREATE_PASSTHRU_DISB_DEB program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnB652CreatePassthruDisbDeb(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnB652CreatePassthruDisbDeb.
  /// </summary>
  public FnB652CreatePassthruDisbDeb(IContext context, Import import,
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
    // Date	By	IDCR#	Description
    // ??????	??????		Initial code
    // 111797	govind		Changed acblk name.
    // 			Removed persistent views and cleaned up code.
    // 020598	govind		Fixed bug in sequence number for Disb Status History.
    // 072099  N.Engoor        Made changes as per new specifications.
    // ---------------------------------------------
    local.Current.Date = Now().Date;
    local.Current.Timestamp = Now();
    local.Max.Date = UseCabSetMaximumDiscontinueDate();
    local.HardcodedDebit.Type1 = "D";
    local.HardcodeDisbursed.SystemGeneratedIdentifier = 1;
    local.HardcodedReleased.SystemGeneratedIdentifier = 1;
    local.HardcodedSuppressed.SystemGeneratedIdentifier = 3;
    local.HardcodedPassthru.SystemGeneratedIdentifier = 71;

    // -----------------------
    // Removed the hardcoded CAB since only a few views were being matched.
    // -----------------------
    switch(AsChar(import.ReleaseOrSuppressedInd.Flag))
    {
      case 'S':
        if (!ReadDisbursementStatus2())
        {
          ExitState = "FN0000_DISB_STATUS_NF";

          return;
        }

        break;
      case 'R':
        if (ReadDisbursementStatus1())
        {
          local.RelOrSupp.ReasonText = "PROCESSED";
        }
        else
        {
          ExitState = "FN0000_DISB_STATUS_NF";

          return;
        }

        break;
      default:
        break;
    }

    if (!ReadDisbursementTranRlnRsn())
    {
      ExitState = "FN0000_DISB_TRANS_RLN_RSN_NF";

      return;
    }

    if (!ReadDisbursementType())
    {
      ExitState = "FN0000_DISB_TYPE_NF";

      return;
    }

    if (!ReadPassthru())
    {
      ExitState = "FN0000_DISB_TRAN_PASSTHRU_NF";

      return;
    }

    // *****  Create the passthru disbursement_transaction.
    for(local.NumberOfCreateAttempts.Count = 1; local
      .NumberOfCreateAttempts.Count <= 10; ++
      local.NumberOfCreateAttempts.Count)
    {
      // ---------------
      // Reference Number is not being set ???
      // ---------------
      local.Current.Timestamp = Now();

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
            break;
          case ErrorCode.PermittedValueViolation:
            ExitState = "FN0000_DISB_TRANS_PV_RB";

            return;
          case ErrorCode.DatabaseError:
            break;
          default:
            throw;
        }
      }

      ++local.NumberOfCreateAttempts.Count;

      if (local.NumberOfCreateAttempts.Count > 10)
      {
        ExitState = "LE0000_RETRY_ADD_4_AVAIL_RANDOM";

        return;
      }
    }

    // -----------------------
    // Create the disbursement_transaction_rln and relate it to the passthru 
    // credit disbursement_transaction and the passthru debit
    // disbursement_transaction and the
    // disbursement_transaction_relation_reason.
    // -----------------------
    for(local.NumberOfCreateAttempts.Count = 1; local
      .NumberOfCreateAttempts.Count <= 10; ++
      local.NumberOfCreateAttempts.Count)
    {
      try
      {
        CreateDisbursementTransactionRln();

        break;
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            break;
          case ErrorCode.PermittedValueViolation:
            ExitState = "FN0000_DISB_TRANS_RLN_PV_RB";

            return;
          case ErrorCode.DatabaseError:
            break;
          default:
            throw;
        }
      }

      ++local.NumberOfCreateAttempts.Count;

      if (local.NumberOfCreateAttempts.Count > 10)
      {
        ExitState = "LE0000_RETRY_ADD_4_AVAIL_RANDOM";

        return;
      }
    }

    // ----------------
    // Create the disbursement_transaction_status_history and relate it to the 
    // passthru disbursement_transaction and the Disbursement Status.
    // ----------------
    local.LastGenerated.SystemGeneratedIdentifier = 0;
    local.Current.Timestamp = Now();

    try
    {
      CreateDisbursementStatusHistory();
    }
    catch(Exception e)
    {
      switch(GetErrorCode(e))
      {
        case ErrorCode.AlreadyExists:
          ExitState = "FN0000_DISB_STATUS_HISTORY_AE";

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

  private int UseCabGenerate3DigitRandomNum()
  {
    var useImport = new CabGenerate3DigitRandomNum.Import();
    var useExport = new CabGenerate3DigitRandomNum.Export();

    Call(CabGenerate3DigitRandomNum.Execute, useImport, useExport);

    return useExport.SystemGenerated.Attribute3DigitRandomNumber;
  }

  private DateTime? UseCabSetMaximumDiscontinueDate()
  {
    var useImport = new CabSetMaximumDiscontinueDate.Import();
    var useExport = new CabSetMaximumDiscontinueDate.Export();

    Call(CabSetMaximumDiscontinueDate.Execute, useImport, useExport);

    return useExport.DateWorkArea.Date;
  }

  private int UseGenerate9DigitRandomNumber()
  {
    var useImport = new Generate9DigitRandomNumber.Import();
    var useExport = new Generate9DigitRandomNumber.Export();

    Call(Generate9DigitRandomNumber.Execute, useImport, useExport);

    return useExport.SystemGenerated.Attribute9DigitRandomNumber;
  }

  private void CreateDisbursementStatusHistory()
  {
    System.Diagnostics.Debug.Assert(entities.PassthruDebit.Populated);

    var dbsGeneratedId = entities.DisbursementStatus.SystemGeneratedIdentifier;
    var dtrGeneratedId = entities.PassthruDebit.SystemGeneratedIdentifier;
    var cspNumber = entities.PassthruDebit.CspNumber;
    var cpaType = entities.PassthruDebit.CpaType;
    var systemGeneratedIdentifier =
      local.LastGenerated.SystemGeneratedIdentifier + 1;
    var effectiveDate = import.ProgramProcessingInfo.ProcessDate;
    var discontinueDate = local.Max.Date;
    var createdBy = global.UserId;
    var createdTimestamp = local.Current.Timestamp;
    var reasonText = local.RelOrSupp.ReasonText ?? "";

    CheckValid<DisbursementStatusHistory>("CpaType", cpaType);
    entities.New1.Populated = false;
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

    entities.New1.DbsGeneratedId = dbsGeneratedId;
    entities.New1.DtrGeneratedId = dtrGeneratedId;
    entities.New1.CspNumber = cspNumber;
    entities.New1.CpaType = cpaType;
    entities.New1.SystemGeneratedIdentifier = systemGeneratedIdentifier;
    entities.New1.EffectiveDate = effectiveDate;
    entities.New1.DiscontinueDate = discontinueDate;
    entities.New1.CreatedBy = createdBy;
    entities.New1.CreatedTimestamp = createdTimestamp;
    entities.New1.ReasonText = reasonText;
    entities.New1.Populated = true;
  }

  private void CreateDisbursementTransaction()
  {
    System.Diagnostics.Debug.
      Assert(import.PersistentCsePersonAccount.Populated);

    var cpaType = import.PersistentCsePersonAccount.Type1;
    var cspNumber = import.PersistentCsePersonAccount.CspNumber;
    var systemGeneratedIdentifier = UseGenerate9DigitRandomNumber();
    var type1 = local.HardcodedDebit.Type1;
    var amount = import.NewDebitPassthru.Amount;
    var processDate = local.Zero.Date;
    var createdBy = global.UserId;
    var createdTimestamp = local.Current.Timestamp;
    var disbursementDate = import.NewDebitPassthru.DisbursementDate;
    var cashNonCashInd = import.NewDebitPassthru.CashNonCashInd;
    var recapturedInd = import.NewDebitPassthru.RecapturedInd;
    var collectionDate = import.Credit.PassthruDate;
    var dbtGeneratedId = entities.DisbursementType.SystemGeneratedIdentifier;
    var interstateInd = import.NewDebitPassthru.InterstateInd ?? "";

    CheckValid<DisbursementTransaction>("CpaType", cpaType);
    CheckValid<DisbursementTransaction>("Type1", type1);
    CheckValid<DisbursementTransaction>("CashNonCashInd", cashNonCashInd);
    entities.PassthruDebit.Populated = false;
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
        db.SetNullableDateTime(command, "lastUpdateTmst", default(DateTime));
        db.SetNullableDate(command, "disbursementDate", disbursementDate);
        db.SetString(command, "cashNonCashInd", cashNonCashInd);
        db.SetString(command, "recapturedInd", recapturedInd);
        db.SetNullableDate(command, "collectionDate", collectionDate);
        db.SetDate(command, "collctnProcessDt", default(DateTime));
        db.SetDate(command, "passthruDate", null);
        db.SetNullableInt32(command, "dbtGeneratedId", dbtGeneratedId);
        db.SetNullableString(
          command, "otrTypeDisb", GetImplicitValue<DisbursementTransaction,
          string>("OtrTypeDisb"));
        db.SetNullableString(
          command, "cpaTypeDisb", GetImplicitValue<DisbursementTransaction,
          string>("CpaTypeDisb"));
        db.SetNullableString(command, "interstateInd", interstateInd);
        db.SetNullableString(command, "designatedPayee", "");
        db.SetNullableString(command, "referenceNumber", "");
        db.SetInt32(command, "uraExcollSnbr", 0);
        db.SetNullableString(command, "excessUraInd", "");
      });

    entities.PassthruDebit.CpaType = cpaType;
    entities.PassthruDebit.CspNumber = cspNumber;
    entities.PassthruDebit.SystemGeneratedIdentifier =
      systemGeneratedIdentifier;
    entities.PassthruDebit.Type1 = type1;
    entities.PassthruDebit.Amount = amount;
    entities.PassthruDebit.ProcessDate = processDate;
    entities.PassthruDebit.CreatedBy = createdBy;
    entities.PassthruDebit.CreatedTimestamp = createdTimestamp;
    entities.PassthruDebit.DisbursementDate = disbursementDate;
    entities.PassthruDebit.CashNonCashInd = cashNonCashInd;
    entities.PassthruDebit.RecapturedInd = recapturedInd;
    entities.PassthruDebit.CollectionDate = collectionDate;
    entities.PassthruDebit.PassthruDate = null;
    entities.PassthruDebit.DbtGeneratedId = dbtGeneratedId;
    entities.PassthruDebit.InterstateInd = interstateInd;
    entities.PassthruDebit.ReferenceNumber = "";
    entities.PassthruDebit.Populated = true;
  }

  private void CreateDisbursementTransactionRln()
  {
    System.Diagnostics.Debug.Assert(entities.Credit.Populated);
    System.Diagnostics.Debug.Assert(entities.PassthruDebit.Populated);

    var systemGeneratedIdentifier = UseCabGenerate3DigitRandomNum();
    var createdBy = global.UserId;
    var createdTimestamp = Now();
    var dnrGeneratedId = entities.Disbursed.SystemGeneratedIdentifier;
    var cspNumber = entities.PassthruDebit.CspNumber;
    var cpaType = entities.PassthruDebit.CpaType;
    var dtrGeneratedId = entities.PassthruDebit.SystemGeneratedIdentifier;
    var cspPNumber = entities.Credit.CspNumber;
    var cpaPType = entities.Credit.CpaType;
    var dtrPGeneratedId = entities.Credit.SystemGeneratedIdentifier;

    CheckValid<DisbursementTransactionRln>("CpaType", cpaType);
    CheckValid<DisbursementTransactionRln>("CpaPType", cpaPType);
    entities.DisbursementTransactionRln.Populated = false;
    Update("CreateDisbursementTransactionRln",
      (db, command) =>
      {
        db.SetInt32(command, "disbTranRlnId", systemGeneratedIdentifier);
        db.SetNullableString(command, "description", "");
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetNullableString(command, "lastUpdatedBy", "");
        db.SetNullableDateTime(command, "lastUpdatedTmst", default(DateTime));
        db.SetInt32(command, "dnrGeneratedId", dnrGeneratedId);
        db.SetString(command, "cspNumber", cspNumber);
        db.SetString(command, "cpaType", cpaType);
        db.SetInt32(command, "dtrGeneratedId", dtrGeneratedId);
        db.SetString(command, "cspPNumber", cspPNumber);
        db.SetString(command, "cpaPType", cpaPType);
        db.SetInt32(command, "dtrPGeneratedId", dtrPGeneratedId);
      });

    entities.DisbursementTransactionRln.SystemGeneratedIdentifier =
      systemGeneratedIdentifier;
    entities.DisbursementTransactionRln.CreatedBy = createdBy;
    entities.DisbursementTransactionRln.CreatedTimestamp = createdTimestamp;
    entities.DisbursementTransactionRln.DnrGeneratedId = dnrGeneratedId;
    entities.DisbursementTransactionRln.CspNumber = cspNumber;
    entities.DisbursementTransactionRln.CpaType = cpaType;
    entities.DisbursementTransactionRln.DtrGeneratedId = dtrGeneratedId;
    entities.DisbursementTransactionRln.CspPNumber = cspPNumber;
    entities.DisbursementTransactionRln.CpaPType = cpaPType;
    entities.DisbursementTransactionRln.DtrPGeneratedId = dtrPGeneratedId;
    entities.DisbursementTransactionRln.Populated = true;
  }

  private bool ReadDisbursementStatus1()
  {
    entities.DisbursementStatus.Populated = false;

    return Read("ReadDisbursementStatus1",
      (db, command) =>
      {
        db.SetInt32(
          command, "disbStatusId",
          local.HardcodedReleased.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.DisbursementStatus.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.DisbursementStatus.Populated = true;
      });
  }

  private bool ReadDisbursementStatus2()
  {
    entities.DisbursementStatus.Populated = false;

    return Read("ReadDisbursementStatus2",
      (db, command) =>
      {
        db.SetInt32(
          command, "disbStatusId",
          local.HardcodedSuppressed.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.DisbursementStatus.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.DisbursementStatus.Populated = true;
      });
  }

  private bool ReadDisbursementTranRlnRsn()
  {
    entities.Disbursed.Populated = false;

    return Read("ReadDisbursementTranRlnRsn",
      (db, command) =>
      {
        db.SetInt32(
          command, "disbTrnRlnRsId",
          local.HardcodeDisbursed.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.Disbursed.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.Disbursed.Populated = true;
      });
  }

  private bool ReadDisbursementType()
  {
    entities.DisbursementType.Populated = false;

    return Read("ReadDisbursementType",
      (db, command) =>
      {
        db.SetInt32(
          command, "disbTypeId",
          local.HardcodedPassthru.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.DisbursementType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.DisbursementType.Populated = true;
      });
  }

  private bool ReadPassthru()
  {
    System.Diagnostics.Debug.
      Assert(import.PersistentCsePersonAccount.Populated);
    entities.Credit.Populated = false;

    return Read("ReadPassthru",
      (db, command) =>
      {
        db.
          SetString(command, "cpaType", import.PersistentCsePersonAccount.Type1);
          
        db.SetString(
          command, "cspNumber", import.PersistentCsePersonAccount.CspNumber);
        db.SetInt32(
          command, "disbTranId", import.Credit.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.Credit.CpaType = db.GetString(reader, 0);
        entities.Credit.CspNumber = db.GetString(reader, 1);
        entities.Credit.SystemGeneratedIdentifier = db.GetInt32(reader, 2);
        entities.Credit.Type1 = db.GetString(reader, 3);
        entities.Credit.Populated = true;
        CheckValid<DisbursementTransaction>("CpaType", entities.Credit.CpaType);
        CheckValid<DisbursementTransaction>("Type1", entities.Credit.Type1);
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
    /// A value of PersistentCsePersonAccount.
    /// </summary>
    [JsonPropertyName("persistentCsePersonAccount")]
    public CsePersonAccount PersistentCsePersonAccount
    {
      get => persistentCsePersonAccount ??= new();
      set => persistentCsePersonAccount = value;
    }

    /// <summary>
    /// A value of PersistentCsePerson.
    /// </summary>
    [JsonPropertyName("persistentCsePerson")]
    public CsePerson PersistentCsePerson
    {
      get => persistentCsePerson ??= new();
      set => persistentCsePerson = value;
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
    /// A value of Credit.
    /// </summary>
    [JsonPropertyName("credit")]
    public DisbursementTransaction Credit
    {
      get => credit ??= new();
      set => credit = value;
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
    /// A value of ReleaseOrSuppressedInd.
    /// </summary>
    [JsonPropertyName("releaseOrSuppressedInd")]
    public Common ReleaseOrSuppressedInd
    {
      get => releaseOrSuppressedInd ??= new();
      set => releaseOrSuppressedInd = value;
    }

    /// <summary>
    /// A value of NewDebitPassthru.
    /// </summary>
    [JsonPropertyName("newDebitPassthru")]
    public DisbursementTransaction NewDebitPassthru
    {
      get => newDebitPassthru ??= new();
      set => newDebitPassthru = value;
    }

    private CsePersonAccount persistentCsePersonAccount;
    private CsePerson persistentCsePerson;
    private DisbursementStatusHistory disbursementStatusHistory;
    private DisbursementTransaction credit;
    private CsePerson obligee;
    private ProgramProcessingInfo programProcessingInfo;
    private Common releaseOrSuppressedInd;
    private DisbursementTransaction newDebitPassthru;
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
    /// A value of RelOrSupp.
    /// </summary>
    [JsonPropertyName("relOrSupp")]
    public DisbursementStatusHistory RelOrSupp
    {
      get => relOrSupp ??= new();
      set => relOrSupp = value;
    }

    /// <summary>
    /// A value of LastGenerated.
    /// </summary>
    [JsonPropertyName("lastGenerated")]
    public DisbursementStatusHistory LastGenerated
    {
      get => lastGenerated ??= new();
      set => lastGenerated = value;
    }

    /// <summary>
    /// A value of Max.
    /// </summary>
    [JsonPropertyName("max")]
    public DateWorkArea Max
    {
      get => max ??= new();
      set => max = value;
    }

    /// <summary>
    /// A value of Zero.
    /// </summary>
    [JsonPropertyName("zero")]
    public DateWorkArea Zero
    {
      get => zero ??= new();
      set => zero = value;
    }

    /// <summary>
    /// A value of HardcodedPassthru.
    /// </summary>
    [JsonPropertyName("hardcodedPassthru")]
    public DisbursementType HardcodedPassthru
    {
      get => hardcodedPassthru ??= new();
      set => hardcodedPassthru = value;
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
    /// A value of HardcodedDebit.
    /// </summary>
    [JsonPropertyName("hardcodedDebit")]
    public DisbursementTransaction HardcodedDebit
    {
      get => hardcodedDebit ??= new();
      set => hardcodedDebit = value;
    }

    /// <summary>
    /// A value of Dummy.
    /// </summary>
    [JsonPropertyName("dummy")]
    public Common Dummy
    {
      get => dummy ??= new();
      set => dummy = value;
    }

    /// <summary>
    /// A value of HardcodedReleased.
    /// </summary>
    [JsonPropertyName("hardcodedReleased")]
    public DisbursementStatus HardcodedReleased
    {
      get => hardcodedReleased ??= new();
      set => hardcodedReleased = value;
    }

    /// <summary>
    /// A value of HardcodedSuppressed.
    /// </summary>
    [JsonPropertyName("hardcodedSuppressed")]
    public DisbursementStatus HardcodedSuppressed
    {
      get => hardcodedSuppressed ??= new();
      set => hardcodedSuppressed = value;
    }

    /// <summary>
    /// A value of HardcodeDisbursed.
    /// </summary>
    [JsonPropertyName("hardcodeDisbursed")]
    public DisbursementTranRlnRsn HardcodeDisbursed
    {
      get => hardcodeDisbursed ??= new();
      set => hardcodeDisbursed = value;
    }

    /// <summary>
    /// A value of NumberOfCreateAttempts.
    /// </summary>
    [JsonPropertyName("numberOfCreateAttempts")]
    public Common NumberOfCreateAttempts
    {
      get => numberOfCreateAttempts ??= new();
      set => numberOfCreateAttempts = value;
    }

    private DisbursementStatusHistory relOrSupp;
    private DisbursementStatusHistory lastGenerated;
    private DateWorkArea max;
    private DateWorkArea zero;
    private DisbursementType hardcodedPassthru;
    private DateWorkArea current;
    private DisbursementTransaction hardcodedDebit;
    private Common dummy;
    private DisbursementStatus hardcodedReleased;
    private DisbursementStatus hardcodedSuppressed;
    private DisbursementTranRlnRsn hardcodeDisbursed;
    private Common numberOfCreateAttempts;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of New1.
    /// </summary>
    [JsonPropertyName("new1")]
    public DisbursementStatusHistory New1
    {
      get => new1 ??= new();
      set => new1 = value;
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
    /// A value of Credit.
    /// </summary>
    [JsonPropertyName("credit")]
    public DisbursementTransaction Credit
    {
      get => credit ??= new();
      set => credit = value;
    }

    /// <summary>
    /// A value of ObligeeCsePerson.
    /// </summary>
    [JsonPropertyName("obligeeCsePerson")]
    public CsePerson ObligeeCsePerson
    {
      get => obligeeCsePerson ??= new();
      set => obligeeCsePerson = value;
    }

    /// <summary>
    /// A value of ObligeeCsePersonAccount.
    /// </summary>
    [JsonPropertyName("obligeeCsePersonAccount")]
    public CsePersonAccount ObligeeCsePersonAccount
    {
      get => obligeeCsePersonAccount ??= new();
      set => obligeeCsePersonAccount = value;
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
    /// A value of Disbursed.
    /// </summary>
    [JsonPropertyName("disbursed")]
    public DisbursementTranRlnRsn Disbursed
    {
      get => disbursed ??= new();
      set => disbursed = value;
    }

    /// <summary>
    /// A value of PassthruDebit.
    /// </summary>
    [JsonPropertyName("passthruDebit")]
    public DisbursementTransaction PassthruDebit
    {
      get => passthruDebit ??= new();
      set => passthruDebit = value;
    }

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
    /// A value of DisbursementStatusHistory.
    /// </summary>
    [JsonPropertyName("disbursementStatusHistory")]
    public DisbursementStatusHistory DisbursementStatusHistory
    {
      get => disbursementStatusHistory ??= new();
      set => disbursementStatusHistory = value;
    }

    private DisbursementStatusHistory new1;
    private DisbursementType disbursementType;
    private DisbursementTransaction credit;
    private CsePerson obligeeCsePerson;
    private CsePersonAccount obligeeCsePersonAccount;
    private DisbursementStatus disbursementStatus;
    private DisbursementTranRlnRsn disbursed;
    private DisbursementTransaction passthruDebit;
    private DisbursementTransactionRln disbursementTransactionRln;
    private DisbursementStatusHistory disbursementStatusHistory;
  }
#endregion
}
