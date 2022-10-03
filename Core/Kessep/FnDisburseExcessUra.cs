// Program: FN_DISBURSE_EXCESS_URA, ID: 372550699, model: 746.
// Short name: SWE02013
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_DISBURSE_EXCESS_URA.
/// </summary>
[Serializable]
public partial class FnDisburseExcessUra: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_DISBURSE_EXCESS_URA program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnDisburseExcessUra(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnDisburseExcessUra.
  /// </summary>
  public FnDisburseExcessUra(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // -------------------------------
    // Initial Version - SWSRKXD - 3/31/99
    // -------------------------------
    // -------------------------------
    // SWSRKXD - 6/25/99
    // Use import AR instead of supplying cse_person.
    // -------------------------------
    if (!ReadUraExcessCollection())
    {
      ExitState = "FN0000_URA_EXCESS_COLLECTION_NF";

      return;
    }

    // ------------------------------
    // Update amount to reflect disb.
    // ------------------------------
    local.Update.Assign(entities.UraExcessCollection);
    local.Update.Amount = entities.UraExcessCollection.Amount - import
      .UraExcessCollection.Amount;
    UseFnUpdateUraExcessCollection();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    // ------------------------------------------------------------------
    // Read imhh. This is then passed to fn_create_ura_excess_coll
    // ------------------------------------------------------------------
    if (!ReadImHousehold())
    {
      // -------------------------------------------
      // Should never happen!
      // -------------------------------------------
      ExitState = "IM_HOUSEHOLD_NF";

      return;
    }

    // -------------------------------------------
    // Create D/E record for the disbursed amount.
    // -------------------------------------------
    local.Create.Assign(entities.UraExcessCollection);
    local.Create.Amount = import.UraExcessCollection.Amount;

    if (AsChar(entities.UraExcessCollection.Action) == 'X')
    {
      local.Create.Action = "D";
    }
    else
    {
      local.Create.Action = "E";
    }

    UseFnCreateUraExcessCollection();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    // ------------------------------------------------------
    // Get currency on AR's IMHH member. (This may not
    // necessarily be an open record)
    // ------------------------------------------------------
    // ------------------------
    // Read optimized for 1
    // ------------------------
    ReadImHouseholdMember();

    if (entities.ImHouseholdMember.Populated)
    {
      // ------------------------------------------------------
      // 8/24/99 - SWSRKXD.
      // AR member found.
      // ------------------------------------------------------
      local.CsePerson.Number = import.Ar.Number;
    }
    else
    {
      // ------------------------------------------------------
      // 8/24/99 - SWSRKXD.
      // AR member nf, get most recent PI member.(This PI record
      // may not be open but that's okay)
      // ------------------------------------------------------
      if (ReadImHouseholdMemberCsePerson())
      {
        local.CsePerson.Number = entities.CsePerson.Number;

        goto Test;
      }

      ExitState = "OE0000_AR_NOR_PI_MEMBER_FOUND";

      return;
    }

Test:

    // -------------------------------------------
    // Create Balancing URA Adjustment transaction
    // -------------------------------------------
    if (AsChar(entities.UraExcessCollection.Type1) == 'C' || AsChar
      (entities.UraExcessCollection.Type1) == 'S')
    {
      local.UraAdjustment.AdjHouseholdUra = import.UraExcessCollection.Amount;
    }
    else if (AsChar(entities.UraExcessCollection.Type1) == 'M')
    {
      local.UraAdjustment.AdjHholdMedicalUra =
        import.UraExcessCollection.Amount;
    }

    local.UraAdjustment.AdjReason = "Excess URA";
    local.UraAdjustment.AdjMonth = Now().Date.Month;
    local.UraAdjustment.AdjYear = Now().Date.Year;

    // ------------------------------------------------------
    // 8/24/99 - SWSRKXD.
    // Pass local view of cse_person instead of import_ar
    // ------------------------------------------------------
    UseOeUraaAddUraAdj();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    // --------------------------------------------------
    // Hardcode disb_type values.
    // If action = X set it to 'Current' else 'Arrears'.
    // --------------------------------------------------
    if (AsChar(entities.UraExcessCollection.Action) == 'X')
    {
      local.DisbursementType.SystemGeneratedIdentifier = 348;
    }
    else
    {
      local.DisbursementType.SystemGeneratedIdentifier = 349;
    }

    // --------------------------------------------------
    // Always create Disb_tran Credits and Debits for 'AR'.
    // --------------------------------------------------
    local.DisbursementTransaction.Amount = import.UraExcessCollection.Amount;
    UseFnCreateUraCrAndDr();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    // -----------------------------------
    // AE notification for AR.
    // ----------------------------------
    for(local.RetryCount.Count = 1; local.RetryCount.Count <= 10; ++
      local.RetryCount.Count)
    {
      try
      {
        CreateInterfaceIncomeNotification();

        return;
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            continue;
          case ErrorCode.PermittedValueViolation:
            ExitState = "FN0000_INCOME_INTF_NOTF_PV_RB";

            return;
          case ErrorCode.DatabaseError:
            break;
          default:
            throw;
        }
      }

      ExitState = "LE0000_RETRY_ADD_4_AVAIL_RANDOM";

      return;
    }
  }

  private void UseFnCreateUraCrAndDr()
  {
    var useImport = new FnCreateUraCrAndDr.Import();
    var useExport = new FnCreateUraCrAndDr.Export();

    useImport.UraExcessCollection.SequenceNumber =
      import.UraExcessCollection.SequenceNumber;
    useImport.DisbursementType.SystemGeneratedIdentifier =
      local.DisbursementType.SystemGeneratedIdentifier;
    useImport.DisbursementTransaction.Amount =
      local.DisbursementTransaction.Amount;
    useImport.CsePerson.Number = import.Ar.Number;

    Call(FnCreateUraCrAndDr.Execute, useImport, useExport);
  }

  private void UseFnCreateUraExcessCollection()
  {
    var useImport = new FnCreateUraExcessCollection.Import();
    var useExport = new FnCreateUraExcessCollection.Export();

    useImport.ImHousehold.AeCaseNo = entities.ImHousehold.AeCaseNo;
    useImport.UraExcessCollection.Assign(local.Create);

    Call(FnCreateUraExcessCollection.Execute, useImport, useExport);
  }

  private void UseFnUpdateUraExcessCollection()
  {
    var useImport = new FnUpdateUraExcessCollection.Import();
    var useExport = new FnUpdateUraExcessCollection.Export();

    useImport.UraExcessCollection.Assign(local.Update);

    Call(FnUpdateUraExcessCollection.Execute, useImport, useExport);
  }

  private int UseGenerate9DigitRandomNumber()
  {
    var useImport = new Generate9DigitRandomNumber.Import();
    var useExport = new Generate9DigitRandomNumber.Export();

    Call(Generate9DigitRandomNumber.Execute, useImport, useExport);

    return useExport.SystemGenerated.Attribute9DigitRandomNumber;
  }

  private void UseOeUraaAddUraAdj()
  {
    var useImport = new OeUraaAddUraAdj.Import();
    var useExport = new OeUraaAddUraAdj.Export();

    useImport.UraAdjustment.Assign(local.UraAdjustment);
    useImport.ImHouseholdMember.StartDate =
      entities.ImHouseholdMember.StartDate;
    useImport.ImHousehold.AeCaseNo = entities.ImHousehold.AeCaseNo;
    useImport.CsePerson.Number = local.CsePerson.Number;

    Call(OeUraaAddUraAdj.Execute, useImport, useExport);
  }

  private void CreateInterfaceIncomeNotification()
  {
    var systemGeneratedIdentifier = UseGenerate9DigitRandomNumber();
    var supportedCsePersonNumber = import.Ar.Number;
    var collectionDate = local.Null1.Date;
    var collectionAmount = import.UraExcessCollection.Amount;
    var appliedToCode = "X";
    var distributionDate = Now().Date;
    var createdTimestamp = Now();
    var createdBy = global.UserId;

    entities.InterfaceIncomeNotification.Populated = false;
    Update("CreateInterfaceIncomeNotification",
      (db, command) =>
      {
        db.SetInt32(command, "intrfcIncNtfId", systemGeneratedIdentifier);
        db.SetString(command, "suppCspNumber", supportedCsePersonNumber);
        db.SetString(command, "obligorCspNumber", "");
        db.SetString(command, "caseNumb", "");
        db.SetDate(command, "collectionDate", collectionDate);
        db.SetDecimal(command, "collectionAmount", collectionAmount);
        db.SetString(command, "personProgram", "");
        db.SetString(command, "programAppliedTo", "");
        db.SetString(command, "appliedToCode", appliedToCode);
        db.SetDate(command, "distributionDate", distributionDate);
        db.SetDateTime(command, "createdTmst", createdTimestamp);
        db.SetString(command, "createdBy", createdBy);
        db.SetDate(command, "processDt", null);
      });

    entities.InterfaceIncomeNotification.SystemGeneratedIdentifier =
      systemGeneratedIdentifier;
    entities.InterfaceIncomeNotification.SupportedCsePersonNumber =
      supportedCsePersonNumber;
    entities.InterfaceIncomeNotification.ObligorCsePersonNumber = "";
    entities.InterfaceIncomeNotification.CaseNumber = "";
    entities.InterfaceIncomeNotification.CollectionDate = collectionDate;
    entities.InterfaceIncomeNotification.CollectionAmount = collectionAmount;
    entities.InterfaceIncomeNotification.PersonProgram = "";
    entities.InterfaceIncomeNotification.ProgramAppliedTo = "";
    entities.InterfaceIncomeNotification.AppliedToCode = appliedToCode;
    entities.InterfaceIncomeNotification.DistributionDate = distributionDate;
    entities.InterfaceIncomeNotification.CreatedTimestamp = createdTimestamp;
    entities.InterfaceIncomeNotification.CreatedBy = createdBy;
    entities.InterfaceIncomeNotification.ProcessDate = null;
    entities.InterfaceIncomeNotification.Populated = true;
  }

  private bool ReadImHousehold()
  {
    System.Diagnostics.Debug.Assert(entities.UraExcessCollection.Populated);
    entities.ImHousehold.Populated = false;

    return Read("ReadImHousehold",
      (db, command) =>
      {
        db.SetString(
          command, "aeCaseNo", entities.UraExcessCollection.ImhAeCaseNo ?? "");
      },
      (db, reader) =>
      {
        entities.ImHousehold.AeCaseNo = db.GetString(reader, 0);
        entities.ImHousehold.Populated = true;
      });
  }

  private bool ReadImHouseholdMember()
  {
    entities.ImHouseholdMember.Populated = false;

    return Read("ReadImHouseholdMember",
      (db, command) =>
      {
        db.SetString(command, "imhAeCaseNo", entities.ImHousehold.AeCaseNo);
        db.SetString(command, "cspNumber", import.Ar.Number);
      },
      (db, reader) =>
      {
        entities.ImHouseholdMember.ImhAeCaseNo = db.GetString(reader, 0);
        entities.ImHouseholdMember.CspNumber = db.GetString(reader, 1);
        entities.ImHouseholdMember.StartDate = db.GetDate(reader, 2);
        entities.ImHouseholdMember.EndDate = db.GetDate(reader, 3);
        entities.ImHouseholdMember.CreatedTimestamp = db.GetDateTime(reader, 4);
        entities.ImHouseholdMember.Relationship = db.GetString(reader, 5);
        entities.ImHouseholdMember.Populated = true;
      });
  }

  private bool ReadImHouseholdMemberCsePerson()
  {
    entities.CsePerson.Populated = false;
    entities.ImHouseholdMember.Populated = false;

    return Read("ReadImHouseholdMemberCsePerson",
      (db, command) =>
      {
        db.SetString(command, "imhAeCaseNo", entities.ImHousehold.AeCaseNo);
      },
      (db, reader) =>
      {
        entities.ImHouseholdMember.ImhAeCaseNo = db.GetString(reader, 0);
        entities.ImHouseholdMember.CspNumber = db.GetString(reader, 1);
        entities.CsePerson.Number = db.GetString(reader, 1);
        entities.ImHouseholdMember.StartDate = db.GetDate(reader, 2);
        entities.ImHouseholdMember.EndDate = db.GetDate(reader, 3);
        entities.ImHouseholdMember.CreatedTimestamp = db.GetDateTime(reader, 4);
        entities.ImHouseholdMember.Relationship = db.GetString(reader, 5);
        entities.CsePerson.Populated = true;
        entities.ImHouseholdMember.Populated = true;
      });
  }

  private bool ReadUraExcessCollection()
  {
    entities.UraExcessCollection.Populated = false;

    return Read("ReadUraExcessCollection",
      (db, command) =>
      {
        db.SetInt32(
          command, "seqNumber", import.UraExcessCollection.SequenceNumber);
      },
      (db, reader) =>
      {
        entities.UraExcessCollection.SequenceNumber = db.GetInt32(reader, 0);
        entities.UraExcessCollection.Month = db.GetInt32(reader, 1);
        entities.UraExcessCollection.Year = db.GetInt32(reader, 2);
        entities.UraExcessCollection.Amount = db.GetDecimal(reader, 3);
        entities.UraExcessCollection.Type1 = db.GetString(reader, 4);
        entities.UraExcessCollection.Action = db.GetString(reader, 5);
        entities.UraExcessCollection.ActionImHousehold =
          db.GetString(reader, 6);
        entities.UraExcessCollection.SupplyingCsePerson =
          db.GetString(reader, 7);
        entities.UraExcessCollection.InitiatingCollection =
          db.GetInt32(reader, 8);
        entities.UraExcessCollection.ImhAeCaseNo =
          db.GetNullableString(reader, 9);
        entities.UraExcessCollection.ReceivingCsePerson =
          db.GetNullableString(reader, 10);
        entities.UraExcessCollection.InitiatingCsePerson =
          db.GetString(reader, 11);
        entities.UraExcessCollection.InitiatingImHousehold =
          db.GetString(reader, 12);
        entities.UraExcessCollection.Populated = true;
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
    /// A value of Ar.
    /// </summary>
    [JsonPropertyName("ar")]
    public CsePerson Ar
    {
      get => ar ??= new();
      set => ar = value;
    }

    /// <summary>
    /// A value of UraExcessCollection.
    /// </summary>
    [JsonPropertyName("uraExcessCollection")]
    public UraExcessCollection UraExcessCollection
    {
      get => uraExcessCollection ??= new();
      set => uraExcessCollection = value;
    }

    private CsePerson ar;
    private UraExcessCollection uraExcessCollection;
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
    /// A value of DisbursementType.
    /// </summary>
    [JsonPropertyName("disbursementType")]
    public DisbursementType DisbursementType
    {
      get => disbursementType ??= new();
      set => disbursementType = value;
    }

    /// <summary>
    /// A value of UraAdjustment.
    /// </summary>
    [JsonPropertyName("uraAdjustment")]
    public UraAdjustment UraAdjustment
    {
      get => uraAdjustment ??= new();
      set => uraAdjustment = value;
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

    /// <summary>
    /// A value of Create.
    /// </summary>
    [JsonPropertyName("create")]
    public UraExcessCollection Create
    {
      get => create ??= new();
      set => create = value;
    }

    /// <summary>
    /// A value of Update.
    /// </summary>
    [JsonPropertyName("update")]
    public UraExcessCollection Update
    {
      get => update ??= new();
      set => update = value;
    }

    /// <summary>
    /// A value of RetryCount.
    /// </summary>
    [JsonPropertyName("retryCount")]
    public Common RetryCount
    {
      get => retryCount ??= new();
      set => retryCount = value;
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

    private DisbursementType disbursementType;
    private UraAdjustment uraAdjustment;
    private DisbursementTransaction disbursementTransaction;
    private CsePerson csePerson;
    private UraExcessCollection create;
    private UraExcessCollection update;
    private Common retryCount;
    private DateWorkArea null1;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of InterfaceIncomeNotification.
    /// </summary>
    [JsonPropertyName("interfaceIncomeNotification")]
    public InterfaceIncomeNotification InterfaceIncomeNotification
    {
      get => interfaceIncomeNotification ??= new();
      set => interfaceIncomeNotification = value;
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
    /// A value of ImHouseholdMember.
    /// </summary>
    [JsonPropertyName("imHouseholdMember")]
    public ImHouseholdMember ImHouseholdMember
    {
      get => imHouseholdMember ??= new();
      set => imHouseholdMember = value;
    }

    /// <summary>
    /// A value of ImHousehold.
    /// </summary>
    [JsonPropertyName("imHousehold")]
    public ImHousehold ImHousehold
    {
      get => imHousehold ??= new();
      set => imHousehold = value;
    }

    /// <summary>
    /// A value of UraExcessCollection.
    /// </summary>
    [JsonPropertyName("uraExcessCollection")]
    public UraExcessCollection UraExcessCollection
    {
      get => uraExcessCollection ??= new();
      set => uraExcessCollection = value;
    }

    private InterfaceIncomeNotification interfaceIncomeNotification;
    private CsePerson csePerson;
    private ImHouseholdMember imHouseholdMember;
    private ImHousehold imHousehold;
    private UraExcessCollection uraExcessCollection;
  }
#endregion
}
