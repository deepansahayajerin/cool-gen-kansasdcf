// Program: FN_PROCESS_A_DIST_COLL_ONLINE, ID: 373462931, model: 746.
// Short name: SWE02889
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_PROCESS_A_DIST_COLL_ONLINE.
/// </summary>
[Serializable]
public partial class FnProcessADistCollOnline: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_PROCESS_A_DIST_COLL_ONLINE program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnProcessADistCollOnline(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnProcessADistCollOnline.
  /// </summary>
  public FnProcessADistCollOnline(IContext context, Import import, Export export)
    :
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // -----------------------------------------------------------------
    // 11/06/02  Fangman  WR 020323
    //      New AB to process collections that have been erroring out of 650 due
    // to not being able to determine the AR.  This screen will allow a worker
    // to enter an AR for a collection or group of collections for an AP.  The
    // collections will then be disbursed to the AR entered (a disbursement
    // collection will be created) and then 651 will process the disbursement
    // collections in the next run.
    // -----------------------------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";

    if (ReadObligee())
    {
      // Continue
    }
    else if (ReadCsePerson())
    {
      try
      {
        CreateObligee();

        // Continue
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "FN0000_OBLIGEE_AE_RB";

            return;
          case ErrorCode.PermittedValueViolation:
            ExitState = "FN0000_OBLIGEE_PV_RB";

            return;
          case ErrorCode.DatabaseError:
            break;
          default:
            throw;
        }
      }
    }
    else
    {
      ExitState = "FN0000_OBLIGEE_CSE_PERSON_NF_RB";

      return;
    }

    // ---------------------------------------------------------------------------------------
    // For interstate debts, get the currency on Interstate Request
    // ---------------------------------------------------------------------------------------
    local.DisbursementTransaction.InterstateInd = "N";

    if (AsChar(import.PerCollection.AppliedToOrderTypeCode) == 'I')
    {
      if (ReadInterstateRequest())
      {
        local.DisbursementTransaction.InterstateInd = "Y";
      }
      else
      {
        ExitState = "FN0000_INTERSTATE_REQUEST_NF_RB";

        return;
      }
    }

    if (AsChar(import.PerCollection.AdjustedInd) == 'Y')
    {
      // -------------------------------------------------
      // If processing an adjustment then set the amount negative.
      // -------------------------------------------------
      local.Collection.Amount = -import.PerCollection.Amount;
    }
    else
    {
      local.Collection.Amount = import.PerCollection.Amount;
    }

    UseFnAbConcatCrAndCrd();

    if (Equal(import.PerCollection.DistPgmStateAppldTo, "UD") || Equal
      (import.PerCollection.DistPgmStateAppldTo, "UP"))
    {
      local.DisbursementTransaction.ExcessUraInd = "Y";
    }
    else
    {
      local.DisbursementTransaction.ExcessUraInd = "";
    }

    // When archiving goes in code the following stmt to get the id
    // USE ar-get-disb-tran-id
    //      WHICH IMPORTS: Entity View Obligee cse_person
    //      WHICH EXPORTS: Entitiy View loc_for_create disbursement_transaction
    UseFnGetDisbTranId();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    try
    {
      CreateDisbursementTransaction();

      if (AsChar(local.DisbursementTransaction.InterstateInd) == 'Y')
      {
        AssociateDisbursementTransaction();
      }

      // ***************************************************
      // Mark the Collection as having been processed.
      // ***************************************************
      try
      {
        UpdateCollection();

        // Continue
      }
      catch(Exception e1)
      {
        switch(GetErrorCode(e1))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "FN0000_COLLECTION_NU_RB";

            return;
          case ErrorCode.PermittedValueViolation:
            ExitState = "FN0000_COLLECTION_PV_RB";

            return;
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
          ExitState = "FN0000_DISB_TRANS_AE_RB";

          return;
        case ErrorCode.PermittedValueViolation:
          ExitState = "FN0000_DISB_TRANS_PV_RB";

          return;
        case ErrorCode.DatabaseError:
          break;
        default:
          throw;
      }
    }

    if (AsChar(import.PerCollection.AppliedToCode) == 'C' && Equal
      (import.PerCollection.ProgramAppliedTo, "NA") && AsChar
      (import.PerCollection.AppliedToFuture) == 'Y' && AsChar
      (import.PerCollection.AdjustedInd) != 'Y')
    {
      UseFnCheckForAfInvolvement();

      if (AsChar(local.AfInvolvementInd.Flag) == 'Y')
      {
        local.Numeric.NumDate = DateToInt(import.DebtDetail.DueDt);
        local.Text.TextDate = NumberToString(local.Numeric.NumDate, 8);
        local.Text2.TextDate =
          Substring(local.Text.TextDate, DateWorkArea.TextDate_MaxLength, 1, 6) +
          "01";
        local.Numeric.NumDate = (int)StringToNumber(local.Text2.TextDate);
        local.DiscontinueDate.Date = IntToDate(local.Numeric.NumDate);

        if (!Lt(import.ProgramProcessingInfo.ProcessDate,
          local.DiscontinueDate.Date))
        {
          // Do not create suppression rules that have already discontinued.
          return;
        }

        if (ReadDisbSuppressionStatusHistory())
        {
          // This suppression rule already exists - do not need to create 
          // another one.
        }
        else
        {
          try
          {
            CreateDisbSuppressionStatusHistory();
          }
          catch(Exception e)
          {
            switch(GetErrorCode(e))
            {
              case ErrorCode.AlreadyExists:
                ExitState = "FN0000_DISB_SUPP_STAT_AE_RB";

                break;
              case ErrorCode.PermittedValueViolation:
                ExitState = "FN0000_DISB_SUPP_STAT_PV_RB";

                break;
              case ErrorCode.DatabaseError:
                break;
              default:
                throw;
            }
          }
        }
      }
    }
  }

  private void UseFnAbConcatCrAndCrd()
  {
    var useImport = new FnAbConcatCrAndCrd.Import();
    var useExport = new FnAbConcatCrAndCrd.Export();

    useImport.CashReceiptDetail.SequentialIdentifier =
      import.CashReceiptDetail.SequentialIdentifier;
    useImport.CashReceipt.SequentialNumber =
      import.CashReceipt.SequentialNumber;

    Call(FnAbConcatCrAndCrd.Execute, useImport, useExport);

    local.CrdCrComboNo.CrdCrCombo = useExport.CrdCrComboNo.CrdCrCombo;
  }

  private int UseFnAssignDisbSuppIdV2()
  {
    var useImport = new FnAssignDisbSuppIdV2.Import();
    var useExport = new FnAssignDisbSuppIdV2.Export();

    useImport.Obligee.Number = import.Obligee.Number;

    Call(FnAssignDisbSuppIdV2.Execute, useImport, useExport);

    return useExport.DisbSuppressionStatusHistory.SystemGeneratedIdentifier;
  }

  private void UseFnCheckForAfInvolvement()
  {
    var useImport = new FnCheckForAfInvolvement.Import();
    var useExport = new FnCheckForAfInvolvement.Export();

    useImport.ProgramProcessingInfo.ProcessDate =
      import.ProgramProcessingInfo.ProcessDate;
    useImport.Ar.Number = import.Obligee.Number;
    useImport.Child.Number = import.Supported.Number;

    Call(FnCheckForAfInvolvement.Execute, useImport, useExport);

    local.AfInvolvementInd.Flag = useExport.AfInvolvementInd.Flag;
  }

  private void UseFnGetDisbTranId()
  {
    var useImport = new FnGetDisbTranId.Import();
    var useExport = new FnGetDisbTranId.Export();

    useImport.CsePerson.Number = import.Obligee.Number;

    Call(FnGetDisbTranId.Execute, useImport, useExport);

    local.ForCreate.SystemGeneratedIdentifier =
      useExport.DisbursementTransaction.SystemGeneratedIdentifier;
  }

  private void AssociateDisbursementTransaction()
  {
    System.Diagnostics.Debug.Assert(entities.DisbursementTransaction.Populated);

    var intInterId = entities.InterstateRequest.IntHGeneratedId;

    entities.DisbursementTransaction.Populated = false;
    Update("AssociateDisbursementTransaction",
      (db, command) =>
      {
        db.SetNullableInt32(command, "intInterId", intInterId);
        db.SetString(
          command, "cpaType", entities.DisbursementTransaction.CpaType);
        db.SetString(
          command, "cspNumber", entities.DisbursementTransaction.CspNumber);
        db.SetInt32(
          command, "disbTranId",
          entities.DisbursementTransaction.SystemGeneratedIdentifier);
      });

    entities.DisbursementTransaction.IntInterId = intInterId;
    entities.DisbursementTransaction.Populated = true;
  }

  private void CreateDisbSuppressionStatusHistory()
  {
    System.Diagnostics.Debug.Assert(entities.Obligee1.Populated);

    var cpaType = entities.Obligee1.Type1;
    var cspNumber = entities.Obligee1.CspNumber;
    var systemGeneratedIdentifier = UseFnAssignDisbSuppIdV2();
    var effectiveDate = import.ProgramProcessingInfo.ProcessDate;
    var discontinueDate = local.DiscontinueDate.Date;
    var createdBy = global.UserId;
    var createdTimestamp = Now();
    var type1 = "A";
    var reasonText =
      "Current NA Collection has been Automatically suppressed due to AF involvement in last 90 days.";
      

    CheckValid<DisbSuppressionStatusHistory>("CpaType", cpaType);
    CheckValid<DisbSuppressionStatusHistory>("Type1", type1);
    entities.DisbSuppressionStatusHistory.Populated = false;
    Update("CreateDisbSuppressionStatusHistory",
      (db, command) =>
      {
        db.SetString(command, "cpaType", cpaType);
        db.SetString(command, "cspNumber", cspNumber);
        db.SetInt32(command, "dssGeneratedId", systemGeneratedIdentifier);
        db.SetDate(command, "effectiveDate", effectiveDate);
        db.SetDate(command, "discontinueDate", discontinueDate);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetNullableString(command, "lastUpdatedBy", "");
        db.SetNullableDateTime(command, "lastUpdatedTmst", default(DateTime));
        db.SetNullableString(command, "personDisbFiller", "");
        db.SetString(command, "type", type1);
        db.SetNullableString(command, "reasonText", reasonText);
      });

    entities.DisbSuppressionStatusHistory.CpaType = cpaType;
    entities.DisbSuppressionStatusHistory.CspNumber = cspNumber;
    entities.DisbSuppressionStatusHistory.SystemGeneratedIdentifier =
      systemGeneratedIdentifier;
    entities.DisbSuppressionStatusHistory.EffectiveDate = effectiveDate;
    entities.DisbSuppressionStatusHistory.DiscontinueDate = discontinueDate;
    entities.DisbSuppressionStatusHistory.CreatedBy = createdBy;
    entities.DisbSuppressionStatusHistory.CreatedTimestamp = createdTimestamp;
    entities.DisbSuppressionStatusHistory.Type1 = type1;
    entities.DisbSuppressionStatusHistory.ReasonText = reasonText;
    entities.DisbSuppressionStatusHistory.Populated = true;
  }

  private void CreateDisbursementTransaction()
  {
    System.Diagnostics.Debug.Assert(import.PerCollection.Populated);
    System.Diagnostics.Debug.Assert(entities.Obligee1.Populated);

    var cpaType = entities.Obligee1.Type1;
    var cspNumber = entities.Obligee1.CspNumber;
    var systemGeneratedIdentifier = local.ForCreate.SystemGeneratedIdentifier;
    var type1 = "C";
    var amount = local.Collection.Amount;
    var processDate = local.Initialized.Date;
    var createdBy = global.UserId;
    var createdTimestamp = Now();
    var collectionDate = import.PerCollection.CollectionDt;
    var collectionProcessDate = import.ProgramProcessingInfo.ProcessDate;
    var otyId = import.PerCollection.OtyId;
    var otrTypeDisb = import.PerCollection.OtrType;
    var otrId = import.PerCollection.OtrId;
    var cpaTypeDisb = import.PerCollection.CpaType;
    var cspNumberDisb = import.PerCollection.CspNumber;
    var obgId = import.PerCollection.ObgId;
    var crdId = import.PerCollection.CrdId;
    var crvId = import.PerCollection.CrvId;
    var cstId = import.PerCollection.CstId;
    var crtId = import.PerCollection.CrtType;
    var colId = import.PerCollection.SystemGeneratedIdentifier;
    var interstateInd = local.DisbursementTransaction.InterstateInd ?? "";
    var referenceNumber = local.CrdCrComboNo.CrdCrCombo;
    var excessUraInd = local.DisbursementTransaction.ExcessUraInd ?? "";

    CheckValid<DisbursementTransaction>("CpaType", cpaType);
    CheckValid<DisbursementTransaction>("Type1", type1);
    CheckValid<DisbursementTransaction>("OtrTypeDisb", otrTypeDisb);
    CheckValid<DisbursementTransaction>("CpaTypeDisb", cpaTypeDisb);
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
        db.SetNullableDateTime(command, "lastUpdateTmst", default(DateTime));
        db.SetNullableDate(command, "disbursementDate", default(DateTime));
        db.SetString(
          command, "cashNonCashInd", GetImplicitValue<DisbursementTransaction,
          string>("CashNonCashInd"));
        db.SetString(command, "recapturedInd", "");
        db.SetNullableDate(command, "collectionDate", collectionDate);
        db.SetDate(command, "collctnProcessDt", collectionProcessDate);
        db.SetNullableInt32(command, "otyId", otyId);
        db.SetNullableString(command, "otrTypeDisb", otrTypeDisb);
        db.SetNullableInt32(command, "otrId", otrId);
        db.SetNullableString(command, "cpaTypeDisb", cpaTypeDisb);
        db.SetNullableString(command, "cspNumberDisb", cspNumberDisb);
        db.SetNullableInt32(command, "obgId", obgId);
        db.SetNullableInt32(command, "crdId", crdId);
        db.SetNullableInt32(command, "crvId", crvId);
        db.SetNullableInt32(command, "cstId", cstId);
        db.SetNullableInt32(command, "crtId", crtId);
        db.SetNullableInt32(command, "colId", colId);
        db.SetNullableString(command, "interstateInd", interstateInd);
        db.SetNullableString(command, "designatedPayee", "");
        db.SetNullableString(command, "referenceNumber", referenceNumber);
        db.SetInt32(command, "uraExcollSnbr", 0);
        db.SetNullableString(command, "excessUraInd", excessUraInd);
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
    entities.DisbursementTransaction.CollectionDate = collectionDate;
    entities.DisbursementTransaction.CollectionProcessDate =
      collectionProcessDate;
    entities.DisbursementTransaction.OtyId = otyId;
    entities.DisbursementTransaction.OtrTypeDisb = otrTypeDisb;
    entities.DisbursementTransaction.OtrId = otrId;
    entities.DisbursementTransaction.CpaTypeDisb = cpaTypeDisb;
    entities.DisbursementTransaction.CspNumberDisb = cspNumberDisb;
    entities.DisbursementTransaction.ObgId = obgId;
    entities.DisbursementTransaction.CrdId = crdId;
    entities.DisbursementTransaction.CrvId = crvId;
    entities.DisbursementTransaction.CstId = cstId;
    entities.DisbursementTransaction.CrtId = crtId;
    entities.DisbursementTransaction.ColId = colId;
    entities.DisbursementTransaction.InterstateInd = interstateInd;
    entities.DisbursementTransaction.ReferenceNumber = referenceNumber;
    entities.DisbursementTransaction.IntInterId = null;
    entities.DisbursementTransaction.ExcessUraInd = excessUraInd;
    entities.DisbursementTransaction.Populated = true;
  }

  private void CreateObligee()
  {
    var cspNumber = entities.Obligee2.Number;
    var type1 = "E";
    var createdBy = global.UserId;
    var createdTmst = Now();

    CheckValid<CsePersonAccount>("Type1", type1);
    entities.Obligee1.Populated = false;
    Update("CreateObligee",
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

    entities.Obligee1.CspNumber = cspNumber;
    entities.Obligee1.Type1 = type1;
    entities.Obligee1.CreatedBy = createdBy;
    entities.Obligee1.CreatedTmst = createdTmst;
    entities.Obligee1.Populated = true;
  }

  private bool ReadCsePerson()
  {
    entities.Obligee2.Populated = false;

    return Read("ReadCsePerson",
      (db, command) =>
      {
        db.SetString(command, "numb", import.Obligee.Number);
      },
      (db, reader) =>
      {
        entities.Obligee2.Number = db.GetString(reader, 0);
        entities.Obligee2.Populated = true;
      });
  }

  private bool ReadDisbSuppressionStatusHistory()
  {
    System.Diagnostics.Debug.Assert(entities.Obligee1.Populated);
    entities.DisbSuppressionStatusHistory.Populated = false;

    return Read("ReadDisbSuppressionStatusHistory",
      (db, command) =>
      {
        db.SetDate(
          command, "effectiveDate",
          import.ProgramProcessingInfo.ProcessDate.GetValueOrDefault());
        db.SetDate(
          command, "discontinueDate",
          local.DiscontinueDate.Date.GetValueOrDefault());
        db.SetString(command, "cpaType", entities.Obligee1.Type1);
        db.SetString(command, "cspNumber", entities.Obligee1.CspNumber);
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
        entities.DisbSuppressionStatusHistory.CreatedBy =
          db.GetString(reader, 5);
        entities.DisbSuppressionStatusHistory.CreatedTimestamp =
          db.GetDateTime(reader, 6);
        entities.DisbSuppressionStatusHistory.Type1 = db.GetString(reader, 7);
        entities.DisbSuppressionStatusHistory.ReasonText =
          db.GetNullableString(reader, 8);
        entities.DisbSuppressionStatusHistory.Populated = true;
      });
  }

  private bool ReadInterstateRequest()
  {
    System.Diagnostics.Debug.Assert(import.PerObligation.Populated);
    entities.InterstateRequest.Populated = false;

    return Read("ReadInterstateRequest",
      (db, command) =>
      {
        db.SetInt32(command, "otyType", import.PerObligation.DtyGeneratedId);
        db.SetInt32(
          command, "obgGeneratedId",
          import.PerObligation.SystemGeneratedIdentifier);
        db.SetString(command, "cspNumber", import.PerObligation.CspNumber);
        db.SetString(command, "cpaType", import.PerObligation.CpaType);
      },
      (db, reader) =>
      {
        entities.InterstateRequest.IntHGeneratedId = db.GetInt32(reader, 0);
        entities.InterstateRequest.Populated = true;
      });
  }

  private bool ReadObligee()
  {
    entities.Obligee1.Populated = false;

    return Read("ReadObligee",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", import.Obligee.Number);
      },
      (db, reader) =>
      {
        entities.Obligee1.CspNumber = db.GetString(reader, 0);
        entities.Obligee1.Type1 = db.GetString(reader, 1);
        entities.Obligee1.CreatedBy = db.GetString(reader, 2);
        entities.Obligee1.CreatedTmst = db.GetDateTime(reader, 3);
        entities.Obligee1.Populated = true;
      });
  }

  private void UpdateCollection()
  {
    System.Diagnostics.Debug.Assert(import.PerCollection.Populated);

    var disbursementDt = import.CollectionUpdateValues.DisbursementDt;
    var disbursementAdjProcessDate =
      import.CollectionUpdateValues.DisbursementAdjProcessDate;
    var lastUpdatedBy = global.UserId;
    var lastUpdatedTmst = Now();
    var arNumber = import.Obligee.Number;

    import.PerCollection.Populated = false;
    Update("UpdateCollection",
      (db, command) =>
      {
        db.SetNullableDate(command, "disbDt", disbursementDt);
        db.SetDate(command, "disbAdjProcDate", disbursementAdjProcessDate);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTmst);
        db.SetNullableString(command, "arNumber", arNumber);
        db.SetInt32(
          command, "collId", import.PerCollection.SystemGeneratedIdentifier);
        db.SetInt32(command, "crtType", import.PerCollection.CrtType);
        db.SetInt32(command, "cstId", import.PerCollection.CstId);
        db.SetInt32(command, "crvId", import.PerCollection.CrvId);
        db.SetInt32(command, "crdId", import.PerCollection.CrdId);
        db.SetInt32(command, "obgId", import.PerCollection.ObgId);
        db.SetString(command, "cspNumber", import.PerCollection.CspNumber);
        db.SetString(command, "cpaType", import.PerCollection.CpaType);
        db.SetInt32(command, "otrId", import.PerCollection.OtrId);
        db.SetString(command, "otrType", import.PerCollection.OtrType);
        db.SetInt32(command, "otyId", import.PerCollection.OtyId);
      });

    import.PerCollection.DisbursementDt = disbursementDt;
    import.PerCollection.DisbursementAdjProcessDate =
      disbursementAdjProcessDate;
    import.PerCollection.LastUpdatedBy = lastUpdatedBy;
    import.PerCollection.LastUpdatedTmst = lastUpdatedTmst;
    import.PerCollection.ArNumber = arNumber;
    import.PerCollection.Populated = true;
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
    /// A value of PerObligation.
    /// </summary>
    [JsonPropertyName("perObligation")]
    public Obligation PerObligation
    {
      get => perObligation ??= new();
      set => perObligation = value;
    }

    /// <summary>
    /// A value of PerCollection.
    /// </summary>
    [JsonPropertyName("perCollection")]
    public Collection PerCollection
    {
      get => perCollection ??= new();
      set => perCollection = value;
    }

    /// <summary>
    /// A value of CashReceiptDetail.
    /// </summary>
    [JsonPropertyName("cashReceiptDetail")]
    public CashReceiptDetail CashReceiptDetail
    {
      get => cashReceiptDetail ??= new();
      set => cashReceiptDetail = value;
    }

    /// <summary>
    /// A value of CashReceipt.
    /// </summary>
    [JsonPropertyName("cashReceipt")]
    public CashReceipt CashReceipt
    {
      get => cashReceipt ??= new();
      set => cashReceipt = value;
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
    /// A value of CollectionUpdateValues.
    /// </summary>
    [JsonPropertyName("collectionUpdateValues")]
    public Collection CollectionUpdateValues
    {
      get => collectionUpdateValues ??= new();
      set => collectionUpdateValues = value;
    }

    /// <summary>
    /// A value of PerDebt.
    /// </summary>
    [JsonPropertyName("perDebt")]
    public ObligationTransaction PerDebt
    {
      get => perDebt ??= new();
      set => perDebt = value;
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
    /// A value of Supported.
    /// </summary>
    [JsonPropertyName("supported")]
    public CsePerson Supported
    {
      get => supported ??= new();
      set => supported = value;
    }

    /// <summary>
    /// A value of DebtDetail.
    /// </summary>
    [JsonPropertyName("debtDetail")]
    public DebtDetail DebtDetail
    {
      get => debtDetail ??= new();
      set => debtDetail = value;
    }

    private Obligation perObligation;
    private Collection perCollection;
    private CashReceiptDetail cashReceiptDetail;
    private CashReceipt cashReceipt;
    private ProgramProcessingInfo programProcessingInfo;
    private Collection collectionUpdateValues;
    private ObligationTransaction perDebt;
    private CsePerson obligee;
    private CsePerson supported;
    private DebtDetail debtDetail;
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
    /// A value of Collection.
    /// </summary>
    [JsonPropertyName("collection")]
    public Collection Collection
    {
      get => collection ??= new();
      set => collection = value;
    }

    /// <summary>
    /// A value of CrdCrComboNo.
    /// </summary>
    [JsonPropertyName("crdCrComboNo")]
    public CrdCrComboNo CrdCrComboNo
    {
      get => crdCrComboNo ??= new();
      set => crdCrComboNo = value;
    }

    /// <summary>
    /// A value of ForCreate.
    /// </summary>
    [JsonPropertyName("forCreate")]
    public DisbursementTransaction ForCreate
    {
      get => forCreate ??= new();
      set => forCreate = value;
    }

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
    /// A value of AfInvolvementInd.
    /// </summary>
    [JsonPropertyName("afInvolvementInd")]
    public Common AfInvolvementInd
    {
      get => afInvolvementInd ??= new();
      set => afInvolvementInd = value;
    }

    /// <summary>
    /// A value of Numeric.
    /// </summary>
    [JsonPropertyName("numeric")]
    public BatchTimestampWorkArea Numeric
    {
      get => numeric ??= new();
      set => numeric = value;
    }

    /// <summary>
    /// A value of Text.
    /// </summary>
    [JsonPropertyName("text")]
    public DateWorkArea Text
    {
      get => text ??= new();
      set => text = value;
    }

    /// <summary>
    /// A value of Text2.
    /// </summary>
    [JsonPropertyName("text2")]
    public DateWorkArea Text2
    {
      get => text2 ??= new();
      set => text2 = value;
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

    private DisbursementTransaction disbursementTransaction;
    private Collection collection;
    private CrdCrComboNo crdCrComboNo;
    private DisbursementTransaction forCreate;
    private DateWorkArea initialized;
    private Common afInvolvementInd;
    private BatchTimestampWorkArea numeric;
    private DateWorkArea text;
    private DateWorkArea text2;
    private DateWorkArea discontinueDate;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
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
    /// A value of Obligee2.
    /// </summary>
    [JsonPropertyName("obligee2")]
    public CsePerson Obligee2
    {
      get => obligee2 ??= new();
      set => obligee2 = value;
    }

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
    /// A value of InterstateRequestObligation.
    /// </summary>
    [JsonPropertyName("interstateRequestObligation")]
    public InterstateRequestObligation InterstateRequestObligation
    {
      get => interstateRequestObligation ??= new();
      set => interstateRequestObligation = value;
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
    /// A value of DisbSuppressionStatusHistory.
    /// </summary>
    [JsonPropertyName("disbSuppressionStatusHistory")]
    public DisbSuppressionStatusHistory DisbSuppressionStatusHistory
    {
      get => disbSuppressionStatusHistory ??= new();
      set => disbSuppressionStatusHistory = value;
    }

    private CsePersonAccount obligee1;
    private CsePerson obligee2;
    private InterstateRequest interstateRequest;
    private InterstateRequestObligation interstateRequestObligation;
    private DisbursementTransaction disbursementTransaction;
    private DisbSuppressionStatusHistory disbSuppressionStatusHistory;
  }
#endregion
}
