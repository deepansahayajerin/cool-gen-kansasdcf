// Program: FN_CREATE_DISBURSEMENT, ID: 372259150, model: 746.
// Short name: SWE01798
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: FN_CREATE_DISBURSEMENT.
/// </para>
/// <para>
/// RESP: Finance
/// This action block will create a disbursement transaction and a Disbursement 
/// Status History.  Associate it to a Disb Collection, Disbursement Type,
/// Disbursement Status.
/// </para>
/// </summary>
[Serializable]
public partial class FnCreateDisbursement: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_CREATE_DISBURSEMENT program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnCreateDisbursement(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnCreateDisbursement.
  /// </summary>
  public FnCreateDisbursement(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ---------------------------------------------------------------------------------------
    // Date	By	IDCR#	Description
    // ---------------------------------------------------------------------------------------
    // ??????	??????		Initial code
    // 112097	govind		Fixed to use local currentdate.
    // 			Fixed to use local max date instead of datenum(20991231)
    // 120197	govind		Fixed to set the designated payee and passthru proc date.
    // 120397	govind		Fixed to export the created Disb Tran identifier
    // 120697	govind		Removed the usage of persistent views
    // 121197	govind		Set Desig Payee to that in the credit transaction.
    // 020598	govind		Fixed bug in sequence number generation for Disb Status 
    // History
    // 022698	govind		Associate with Interstate Request
    // 17dec98	lxj		Deleted AB with hardcoded
    // 			values. This AB nedds just
    // 			one value.
    // 1/14/98    RK           New flag added to set the discontinue date of 
    // FDSO's to 6 months from coll date.
    // 5/25/99  rk     Import in the end date of the highest suppression that 
    // applies to this disbursement.
    // 99/11/01  PR 77907  Fangman  Set Cash_Non_Cash indicator based on import 
    // Cash_Non_Cash indicator instead of the indicator from the Disbursement
    // Type.
    // 00/03/21  PR 86768  Fangman  Change suppression logic to ensure FDSO 
    // collection type is suppressed for 6 mo.
    // 00/05/04  PRWORA  Fangman  Add code to populate the URA ind on the new DB
    // disbursement tran based on the URA ind on the CR disbursement tran.
    // ---------------------------------------------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";
    local.Max.Date = UseCabSetMaximumDiscontinueDate();
    local.Current.Timestamp = Now();
    local.HardcodeDisbursement.Type1 = "D";
    local.HardcodeObligee.Type1 = "E";

    if (ReadObligee())
    {
      if (!ReadDisbursementTransaction())
      {
        ExitState = "FN0000_DISB_TRANSACTION_NF";

        return;
      }
    }
    else
    {
      ExitState = "FN0000_OBLIGEE_NF";

      return;
    }

    if (AsChar(entities.Credit.InterstateInd) == 'Y')
    {
      if (!ReadInterstateRequest())
      {
        ExitState = "INTERSTATE_REQUEST_NF";

        return;
      }
    }

    if (ReadDisbursementTranRlnRsn())
    {
      if (ReadDisbursementStatus())
      {
        // Read disbursement type to get consistency before the create.
        if (!ReadDisbursementType())
        {
          ExitState = "FN0000_DISB_TYPE_NF";
        }
      }
      else
      {
        ExitState = "FN0000_DISB_STATUS_NF";
      }
    }
    else
    {
      ExitState = "FN0000_DISB_TRANS_RLN_RSN_NF";
    }

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    // Create the disbursement disbursement transaction and
    // associate it to the Obligee and the disbursement type.
    local.Dummy.Flag = "Y";

    if (import.DisbursementType.SystemGeneratedIdentifier == 73)
    {
      local.ForUpdate.ExcessUraInd = "";
    }
    else
    {
      local.ForUpdate.ExcessUraInd = entities.Credit.ExcessUraInd;
    }

    if (AsChar(local.Dummy.Flag) == 'Y')
    {
      local.TranCreated.Flag = "N";

      for(local.CreateAttempts.Count = 1; local.CreateAttempts.Count <= 10; ++
        local.CreateAttempts.Count)
      {
        try
        {
          CreateDisbursementTransaction();
          local.TranCreated.Flag = "Y";

          if (entities.InterstateRequest.Populated)
          {
            AssociateDisbursementTransaction();
          }

          export.New1.Assign(entities.New1);

          goto Test;
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              break;
            case ErrorCode.PermittedValueViolation:
              ExitState = "FN0000_DISB_TRANS_PV";

              return;
            case ErrorCode.DatabaseError:
              break;
            default:
              throw;
          }
        }
      }

      ExitState = "LE0000_RETRY_ADD_4_AVAIL_RANDOM";

      return;
    }

Test:

    if (AsChar(local.TranCreated.Flag) == 'N')
    {
      ExitState = "FN0000_DISB_TRANSACTION_AE";

      return;
    }

    // Create the disbursement transaction relation and assoicate it to the disb
    // collection disbursement transaction and the disbursement tran rln rsn
    // and to the new disbursement.
    try
    {
      CreateDisbursementTransactionRln();

      // Create the disbursement status history and associate it to
      // the disbursement status and the new disbursement
      // disbursement transaction.
      // *****************************************************************
      // If this disbursement is to be suppressed(3) then give it the highest 
      // found suppression end date.
      // *****************************************************************
      if (entities.DisbursementStatus.SystemGeneratedIdentifier == 3 && Lt
        (local.Zero.Date, import.HighestSuppressionDate.Date))
      {
        local.DiscontinueDate.Date = import.HighestSuppressionDate.Date;
      }
      else
      {
        local.DiscontinueDate.Date = local.Max.Date;
      }

      try
      {
        CreateDisbursementStatusHistory();
      }
      catch(Exception e1)
      {
        switch(GetErrorCode(e1))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "FN0000_DISB_STATUS_HISTORY_AE";

            break;
          case ErrorCode.PermittedValueViolation:
            ExitState = "FN0000_DISB_STAT_HIST_PV";

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
          ExitState = "FN0000_DISB_TRANS_RLN_AE";

          break;
        case ErrorCode.PermittedValueViolation:
          ExitState = "FN0000_DISB_TRAND_RLN_PV";

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

  private void AssociateDisbursementTransaction()
  {
    System.Diagnostics.Debug.Assert(entities.New1.Populated);

    var intInterId = entities.InterstateRequest.IntHGeneratedId;

    entities.New1.Populated = false;
    Update("AssociateDisbursementTransaction",
      (db, command) =>
      {
        db.SetNullableInt32(command, "intInterId", intInterId);
        db.SetString(command, "cpaType", entities.New1.CpaType);
        db.SetString(command, "cspNumber", entities.New1.CspNumber);
        db.SetInt32(
          command, "disbTranId", entities.New1.SystemGeneratedIdentifier);
      });

    entities.New1.IntInterId = intInterId;
    entities.New1.Populated = true;
  }

  private void CreateDisbursementStatusHistory()
  {
    System.Diagnostics.Debug.Assert(entities.New1.Populated);

    var dbsGeneratedId = entities.DisbursementStatus.SystemGeneratedIdentifier;
    var dtrGeneratedId = entities.New1.SystemGeneratedIdentifier;
    var cspNumber = entities.New1.CspNumber;
    var cpaType = entities.New1.CpaType;
    var systemGeneratedIdentifier = 1;
    var effectiveDate = import.New1.DisbursementDate;
    var discontinueDate = local.DiscontinueDate.Date;
    var createdBy = global.UserId;
    var createdTimestamp = local.Current.Timestamp;

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
        db.SetNullableString(command, "reasonText", "");
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
    entities.DisbursementStatusHistory.Populated = true;
  }

  private void CreateDisbursementTransaction()
  {
    System.Diagnostics.Debug.Assert(entities.Obligee.Populated);

    var cpaType = entities.Obligee.Type1;
    var cspNumber = entities.Obligee.CspNumber;
    var systemGeneratedIdentifier = UseGenerate9DigitRandomNumber();
    var type1 = local.HardcodeDisbursement.Type1;
    var amount = import.New1.Amount;
    var processDate = import.New1.ProcessDate;
    var createdBy = global.UserId;
    var createdTimestamp = local.Current.Timestamp;
    var disbursementDate = import.New1.DisbursementDate;
    var cashNonCashInd = import.New1.CashNonCashInd;
    var recapturedInd = entities.DisbursementType.RecaptureInd ?? Spaces(1);
    var collectionDate = entities.Credit.CollectionDate;
    var dbtGeneratedId = entities.DisbursementType.SystemGeneratedIdentifier;
    var interstateInd = entities.Credit.InterstateInd;
    var passthruProcDate = import.New1.PassthruProcDate;
    var designatedPayee = entities.Credit.DesignatedPayee;
    var referenceNumber = entities.Credit.ReferenceNumber;
    var excessUraInd = local.ForUpdate.ExcessUraInd ?? "";

    CheckValid<DisbursementTransaction>("CpaType", cpaType);
    CheckValid<DisbursementTransaction>("Type1", type1);
    CheckValid<DisbursementTransaction>("CashNonCashInd", cashNonCashInd);
    entities.New1.Populated = false;
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
        db.SetNullableDate(command, "disbursementDate", disbursementDate);
        db.SetString(command, "cashNonCashInd", cashNonCashInd);
        db.SetString(command, "recapturedInd", recapturedInd);
        db.SetNullableDate(command, "collectionDate", collectionDate);
        db.SetDate(command, "collctnProcessDt", default(DateTime));
        db.SetNullableInt32(command, "dbtGeneratedId", dbtGeneratedId);
        db.SetNullableString(
          command, "otrTypeDisb", GetImplicitValue<DisbursementTransaction,
          string>("OtrTypeDisb"));
        db.SetNullableString(
          command, "cpaTypeDisb", GetImplicitValue<DisbursementTransaction,
          string>("CpaTypeDisb"));
        db.SetNullableString(command, "interstateInd", interstateInd);
        db.SetNullableDate(command, "passthruProcDate", passthruProcDate);
        db.SetNullableString(command, "designatedPayee", designatedPayee);
        db.SetNullableString(command, "referenceNumber", referenceNumber);
        db.SetInt32(command, "uraExcollSnbr", 0);
        db.SetNullableString(command, "excessUraInd", excessUraInd);
      });

    entities.New1.CpaType = cpaType;
    entities.New1.CspNumber = cspNumber;
    entities.New1.SystemGeneratedIdentifier = systemGeneratedIdentifier;
    entities.New1.Type1 = type1;
    entities.New1.Amount = amount;
    entities.New1.ProcessDate = processDate;
    entities.New1.CreatedBy = createdBy;
    entities.New1.CreatedTimestamp = createdTimestamp;
    entities.New1.LastUpdatedBy = "";
    entities.New1.LastUpdateTmst = null;
    entities.New1.DisbursementDate = disbursementDate;
    entities.New1.CashNonCashInd = cashNonCashInd;
    entities.New1.RecapturedInd = recapturedInd;
    entities.New1.CollectionDate = collectionDate;
    entities.New1.DbtGeneratedId = dbtGeneratedId;
    entities.New1.InterstateInd = interstateInd;
    entities.New1.PassthruProcDate = passthruProcDate;
    entities.New1.DesignatedPayee = designatedPayee;
    entities.New1.ReferenceNumber = referenceNumber;
    entities.New1.IntInterId = null;
    entities.New1.ExcessUraInd = excessUraInd;
    entities.New1.Populated = true;
  }

  private void CreateDisbursementTransactionRln()
  {
    System.Diagnostics.Debug.Assert(entities.Credit.Populated);
    System.Diagnostics.Debug.Assert(entities.New1.Populated);

    var systemGeneratedIdentifier = UseCabGenerate3DigitRandomNum();
    var createdBy = global.UserId;
    var createdTimestamp = local.Current.Timestamp;
    var dnrGeneratedId =
      entities.DisbursementTranRlnRsn.SystemGeneratedIdentifier;
    var cspNumber = entities.New1.CspNumber;
    var cpaType = entities.New1.CpaType;
    var dtrGeneratedId = entities.New1.SystemGeneratedIdentifier;
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

  private bool ReadDisbursementStatus()
  {
    entities.DisbursementStatus.Populated = false;

    return Read("ReadDisbursementStatus",
      (db, command) =>
      {
        db.SetInt32(
          command, "disbStatusId",
          import.DisbursementStatus.SystemGeneratedIdentifier);
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
    entities.DisbursementTranRlnRsn.Populated = false;

    return Read("ReadDisbursementTranRlnRsn",
      (db, command) =>
      {
        db.SetInt32(
          command, "disbTrnRlnRsId",
          import.DisbursementTranRlnRsn.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.DisbursementTranRlnRsn.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.DisbursementTranRlnRsn.Populated = true;
      });
  }

  private bool ReadDisbursementTransaction()
  {
    System.Diagnostics.Debug.Assert(entities.Obligee.Populated);
    entities.Credit.Populated = false;

    return Read("ReadDisbursementTransaction",
      (db, command) =>
      {
        db.SetString(command, "cpaType", entities.Obligee.Type1);
        db.SetString(command, "cspNumber", entities.Obligee.CspNumber);
        db.SetInt32(
          command, "disbTranId", import.Credit.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.Credit.CpaType = db.GetString(reader, 0);
        entities.Credit.CspNumber = db.GetString(reader, 1);
        entities.Credit.SystemGeneratedIdentifier = db.GetInt32(reader, 2);
        entities.Credit.Type1 = db.GetString(reader, 3);
        entities.Credit.Amount = db.GetDecimal(reader, 4);
        entities.Credit.ProcessDate = db.GetNullableDate(reader, 5);
        entities.Credit.CreatedTimestamp = db.GetDateTime(reader, 6);
        entities.Credit.LastUpdatedBy = db.GetNullableString(reader, 7);
        entities.Credit.LastUpdateTmst = db.GetNullableDateTime(reader, 8);
        entities.Credit.CollectionDate = db.GetNullableDate(reader, 9);
        entities.Credit.CollectionProcessDate = db.GetDate(reader, 10);
        entities.Credit.InterstateInd = db.GetNullableString(reader, 11);
        entities.Credit.DesignatedPayee = db.GetNullableString(reader, 12);
        entities.Credit.ReferenceNumber = db.GetNullableString(reader, 13);
        entities.Credit.IntInterId = db.GetNullableInt32(reader, 14);
        entities.Credit.ExcessUraInd = db.GetNullableString(reader, 15);
        entities.Credit.Populated = true;
        CheckValid<DisbursementTransaction>("CpaType", entities.Credit.CpaType);
        CheckValid<DisbursementTransaction>("Type1", entities.Credit.Type1);
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
          import.DisbursementType.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.DisbursementType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.DisbursementType.RecaptureInd =
          db.GetNullableString(reader, 1);
        entities.DisbursementType.Populated = true;
      });
  }

  private bool ReadInterstateRequest()
  {
    System.Diagnostics.Debug.Assert(entities.Credit.Populated);
    entities.InterstateRequest.Populated = false;

    return Read("ReadInterstateRequest",
      (db, command) =>
      {
        db.SetInt32(
          command, "identifier",
          entities.Credit.IntInterId.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.InterstateRequest.IntHGeneratedId = db.GetInt32(reader, 0);
        entities.InterstateRequest.Populated = true;
      });
  }

  private bool ReadObligee()
  {
    entities.Obligee.Populated = false;

    return Read("ReadObligee",
      (db, command) =>
      {
        db.SetString(command, "type", local.HardcodeObligee.Type1);
        db.SetString(command, "cspNumber", import.CsePerson.Number);
      },
      (db, reader) =>
      {
        entities.Obligee.CspNumber = db.GetString(reader, 0);
        entities.Obligee.Type1 = db.GetString(reader, 1);
        entities.Obligee.Populated = true;
        CheckValid<CsePersonAccount>("Type1", entities.Obligee.Type1);
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
    /// A value of HighestSuppressionDate.
    /// </summary>
    [JsonPropertyName("highestSuppressionDate")]
    public DateWorkArea HighestSuppressionDate
    {
      get => highestSuppressionDate ??= new();
      set => highestSuppressionDate = value;
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
    /// A value of FdsoZzzzzzzzzzzzzzzzzzzz.
    /// </summary>
    [JsonPropertyName("fdsoZzzzzzzzzzzzzzzzzzzz")]
    public Common FdsoZzzzzzzzzzzzzzzzzzzz
    {
      get => fdsoZzzzzzzzzzzzzzzzzzzz ??= new();
      set => fdsoZzzzzzzzzzzzzzzzzzzz = value;
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
    /// A value of DisbursementStatus.
    /// </summary>
    [JsonPropertyName("disbursementStatus")]
    public DisbursementStatus DisbursementStatus
    {
      get => disbursementStatus ??= new();
      set => disbursementStatus = value;
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
    /// A value of New1.
    /// </summary>
    [JsonPropertyName("new1")]
    public DisbursementTransaction New1
    {
      get => new1 ??= new();
      set => new1 = value;
    }

    private DateWorkArea highestSuppressionDate;
    private ProgramProcessingInfo programProcessingInfo;
    private Common fdsoZzzzzzzzzzzzzzzzzzzz;
    private CsePerson csePerson;
    private DisbursementType disbursementType;
    private DisbursementTransaction credit;
    private DisbursementStatus disbursementStatus;
    private DisbursementTranRlnRsn disbursementTranRlnRsn;
    private DisbursementTransaction new1;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of New1.
    /// </summary>
    [JsonPropertyName("new1")]
    public DisbursementTransaction New1
    {
      get => new1 ??= new();
      set => new1 = value;
    }

    /// <summary>
    /// A value of ProgramError.
    /// </summary>
    [JsonPropertyName("programError")]
    public ProgramError ProgramError
    {
      get => programError ??= new();
      set => programError = value;
    }

    private DisbursementTransaction new1;
    private ProgramError programError;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of ForUpdate.
    /// </summary>
    [JsonPropertyName("forUpdate")]
    public DisbursementTransaction ForUpdate
    {
      get => forUpdate ??= new();
      set => forUpdate = value;
    }

    /// <summary>
    /// A value of DiscontinueDate.
    /// </summary>
    [JsonPropertyName("discontinueDate")]
    public DateWorkArea DiscontinueDate
    {
      get => discontinueDate ??= new();
      set => discontinueDate = value;
    }

    /// <summary>
    /// A value of TranCreated.
    /// </summary>
    [JsonPropertyName("tranCreated")]
    public Common TranCreated
    {
      get => tranCreated ??= new();
      set => tranCreated = value;
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
    /// A value of Dummy.
    /// </summary>
    [JsonPropertyName("dummy")]
    public Common Dummy
    {
      get => dummy ??= new();
      set => dummy = value;
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
    /// A value of Current.
    /// </summary>
    [JsonPropertyName("current")]
    public DateWorkArea Current
    {
      get => current ??= new();
      set => current = value;
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
    /// A value of HardcodeObligee.
    /// </summary>
    [JsonPropertyName("hardcodeObligee")]
    public CsePersonAccount HardcodeObligee
    {
      get => hardcodeObligee ??= new();
      set => hardcodeObligee = value;
    }

    /// <summary>
    /// A value of HardcodeDisbursement.
    /// </summary>
    [JsonPropertyName("hardcodeDisbursement")]
    public DisbursementTransaction HardcodeDisbursement
    {
      get => hardcodeDisbursement ??= new();
      set => hardcodeDisbursement = value;
    }

    /// <summary>
    /// A value of CreateAttempts.
    /// </summary>
    [JsonPropertyName("createAttempts")]
    public Common CreateAttempts
    {
      get => createAttempts ??= new();
      set => createAttempts = value;
    }

    private DisbursementTransaction forUpdate;
    private DateWorkArea discontinueDate;
    private Common tranCreated;
    private DisbursementStatusHistory lastGenerated;
    private Common dummy;
    private DateWorkArea zero;
    private DateWorkArea current;
    private DateWorkArea max;
    private CsePersonAccount hardcodeObligee;
    private DisbursementTransaction hardcodeDisbursement;
    private Common createAttempts;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of InterstateRequest.
    /// </summary>
    [JsonPropertyName("interstateRequest")]
    public InterstateRequest InterstateRequest
    {
      get => interstateRequest ??= new();
      set => interstateRequest = value;
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
    public CsePersonAccount Obligee
    {
      get => obligee ??= new();
      set => obligee = value;
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
    /// A value of New1.
    /// </summary>
    [JsonPropertyName("new1")]
    public DisbursementTransaction New1
    {
      get => new1 ??= new();
      set => new1 = value;
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

    private InterstateRequest interstateRequest;
    private DisbursementTransaction credit;
    private DisbursementTranRlnRsn disbursementTranRlnRsn;
    private DisbursementStatus disbursementStatus;
    private CsePerson csePerson;
    private CsePersonAccount obligee;
    private DisbursementType disbursementType;
    private DisbursementTransaction new1;
    private DisbursementTransactionRln disbursementTransactionRln;
    private DisbursementStatusHistory disbursementStatusHistory;
  }
#endregion
}
