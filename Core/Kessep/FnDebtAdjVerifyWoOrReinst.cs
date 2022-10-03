// Program: FN_DEBT_ADJ_VERIFY_WO_OR_REINST, ID: 373383705, model: 746.
// Short name: SWE00468
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_DEBT_ADJ_VERIFY_WO_OR_REINST.
/// </summary>
[Serializable]
public partial class FnDebtAdjVerifyWoOrReinst: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_DEBT_ADJ_VERIFY_WO_OR_REINST program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnDebtAdjVerifyWoOrReinst(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnDebtAdjVerifyWoOrReinst.
  /// </summary>
  public FnDebtAdjVerifyWoOrReinst(IContext context, Import import,
    Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
    this.local = context.GetData<Local>();
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // -------------------------------
    // MAINTENANCE LOG
    // -----------------------------
    // M. Brown, PR # 150538, September 2002.
    // Debts may be written off a second time without a reinstate being done 
    // first.
    // Commented out the edit that checks for a writeoff and does not allow a 
    // second writeoff if collections with a distribution method of 'W' are
    // found.
    ExitState = "ACO_NN0000_ALL_OK";
    local.HardcodedAccruing.Classification = "A";

    if (!ReadObligationType())
    {
      ExitState = "OBLIGATION_TYPE_NF";

      return;
    }

    if (!ReadObligationDebtDebtDetail())
    {
      ExitState = "FN0000_OBLIGATION_NF";

      return;
    }

    if (import.ObligationTransactionRlnRsn.SystemGeneratedIdentifier == import
      .HardcodeReinstate.SystemGeneratedIdentifier)
    {
      local.DebtAdjustment.Amount = 0;

      foreach(var item in ReadObligationTransactionRlnRsnDebtAdjustment())
      {
        if (entities.ObligationTransactionRlnRsn.SystemGeneratedIdentifier == import
          .HardcodeReinstate.SystemGeneratedIdentifier)
        {
          // : Debt is reinstated already.
          ExitState = "FN0000_WRITE_OFF_DEBT_ADJ_NF";

          return;
        }

        // : January, 2002 - M Brown - Work Order Number: 020199 -
        //   Added Closed Case to this IF.
        if (entities.ObligationTransactionRlnRsn.SystemGeneratedIdentifier == import
          .HardcodeWriteoffAll.SystemGeneratedIdentifier || entities
          .ObligationTransactionRlnRsn.SystemGeneratedIdentifier == import
          .HardcodeWriteoffNaOnly.SystemGeneratedIdentifier || entities
          .ObligationTransactionRlnRsn.SystemGeneratedIdentifier == import
          .HardcodeCloseCase.SystemGeneratedIdentifier || entities
          .ObligationTransactionRlnRsn.SystemGeneratedIdentifier == import
          .HardcodedWriteoffAf.SystemGeneratedIdentifier)
        {
          goto Test;
        }
      }

      // : If the logic gets here, it means there are no debt adjustments
      //   for this debt, therefore reinstate is invalid.
      ExitState = "FN0000_WRITE_OFF_DEBT_ADJ_NF";

      return;
    }

Test:

    // : IF THIS CAB WAS CALLED TO VERIFY A REINSTATE, WE ARE FINISHED HERE.
    if (import.ObligationTransactionRlnRsn.SystemGeneratedIdentifier == import
      .HardcodeReinstate.SystemGeneratedIdentifier)
    {
      return;
    }

    // ---------------     WRITE-OFF EDITS    ----------------
    // September, 2000, MBrown, Work Order # 197:
    // M. Brown, PR # 150538, September 2002.
    // Debts may be written off a second time without a reinstate being done 
    // first.
    // Commented out the edit.
    // : Check to see if there are any debt adjustments with a reason code of 
    // Writeoff,
    //   for current date. This is not allowed.
    // : January, 2002 - M Brown - Work Order Number: 020199 -
    //   Added Closed Case to this IF.
    if (ReadDebtAdjustment())
    {
      ExitState = "FN0000_WO_FOR_CURRENT_DATE_AE";

      return;
    }
    else
    {
      // :  ok
    }

    if (import.ObligationTransactionRlnRsn.SystemGeneratedIdentifier == import
      .HardcodeWriteoffNaOnly.SystemGeneratedIdentifier)
    {
      // : This is for accruing obligations only, so there will always be a 
      // supported person.
      if (!ReadCsePerson())
      {
        ExitState = "FN0000_SUPPORTED_PERSON_NF";

        return;
      }

      // : Get the program,  to determine whether or not this debt should be
      //   written off.
      UseFnDeterminePgmForDebtDetail();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }

      if (Equal(local.Program.Code, "NA") || Equal(local.Program.Code, "NAI"))
      {
        // : continue
      }
      else
      {
        ExitState = "FN0000_NO_DEBTS_FOUND_FOR_NA_WO";

        return;
      }
    }

    if (import.ObligationTransactionRlnRsn.SystemGeneratedIdentifier == import
      .HardcodedWriteoffAf.SystemGeneratedIdentifier || import
      .ObligationTransactionRlnRsn.SystemGeneratedIdentifier == import
      .HardcodedIncAdj.SystemGeneratedIdentifier)
    {
      // : This is for state arrears obligations only,
      if (!ReadCsePerson())
      {
        ExitState = "FN0000_SUPPORTED_PERSON_NF";

        return;
      }

      // : Get the program,  to determine whether or not this debt should be
      //   written off.
      UseFnDeterminePgmForDebtDetail();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }

      if (Equal(local.Program.Code, "AF") || Equal
        (local.Program.Code, "FC") || Equal(local.Program.Code, "NC") || Equal
        (local.Program.Code, "NF"))
      {
        // : continue
      }
      else
      {
        ExitState = "FN0000_NO_DEBTS_FOUND_FOR_ST_ARR";

        return;
      }
    }

    if (entities.DebtDetail.BalanceDueAmt == 0)
    {
      ExitState = "FN0000_NO_BALANCE_TO_WRITE_OFF";
    }
  }

  private static void MoveDebtDetail(DebtDetail source, DebtDetail target)
  {
    target.DueDt = source.DueDt;
    target.CoveredPrdStartDt = source.CoveredPrdStartDt;
    target.CoveredPrdEndDt = source.CoveredPrdEndDt;
    target.PreconversionProgramCode = source.PreconversionProgramCode;
  }

  private static void MoveObligationType(ObligationType source,
    ObligationType target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Classification = source.Classification;
  }

  private void UseFnDeterminePgmForDebtDetail()
  {
    var useImport = new FnDeterminePgmForDebtDetail.Import();
    var useExport = new FnDeterminePgmForDebtDetail.Export();

    MoveObligationType(entities.ObligationType, useImport.ObligationType);
    useImport.Obligation.OrderTypeCode = entities.Obligation.OrderTypeCode;
    MoveDebtDetail(entities.DebtDetail, useImport.DebtDetail);
    useImport.SupportedPerson.Number = entities.Supported1.Number;
    useImport.Collection.Date = import.Current.Date;
    useImport.HardcodedAccruing.Classification =
      local.HardcodedAccruing.Classification;

    Call(FnDeterminePgmForDebtDetail.Execute, useImport, useExport);

    local.Program.Code = useExport.Program.Code;
  }

  private bool ReadCsePerson()
  {
    System.Diagnostics.Debug.Assert(entities.Debt.Populated);
    entities.Supported1.Populated = false;

    return Read("ReadCsePerson",
      (db, command) =>
      {
        db.SetString(command, "numb", entities.Debt.CspSupNumber ?? "");
      },
      (db, reader) =>
      {
        entities.Supported1.Number = db.GetString(reader, 0);
        entities.Supported1.Type1 = db.GetString(reader, 1);
        entities.Supported1.Populated = true;
        CheckValid<CsePerson>("Type1", entities.Supported1.Type1);
      });
  }

  private bool ReadDebtAdjustment()
  {
    System.Diagnostics.Debug.Assert(entities.Debt.Populated);
    entities.DebtAdjustment.Populated = false;

    return Read("ReadDebtAdjustment",
      (db, command) =>
      {
        db.SetInt32(command, "otyTypePrimary", entities.Debt.OtyType);
        db.SetString(command, "otrPType", entities.Debt.Type1);
        db.SetInt32(
          command, "otrPGeneratedId", entities.Debt.SystemGeneratedIdentifier);
        db.SetString(command, "cpaPType", entities.Debt.CpaType);
        db.SetString(command, "cspPNumber", entities.Debt.CspNumber);
        db.SetInt32(command, "obgPGeneratedId", entities.Debt.ObgGeneratedId);
        db.SetInt32(
          command, "systemGeneratedIdentifier1",
          import.HardcodeWriteoffAll.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "systemGeneratedIdentifier2",
          import.HardcodedWriteoffAf.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "systemGeneratedIdentifier3",
          import.HardcodeWriteoffNaOnly.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "systemGeneratedIdentifier4",
          import.HardcodeCloseCase.SystemGeneratedIdentifier);
        db.
          SetDate(command, "debAdjDt", import.Current.Date.GetValueOrDefault());
          
      },
      (db, reader) =>
      {
        entities.DebtAdjustment.ObgGeneratedId = db.GetInt32(reader, 0);
        entities.DebtAdjustment.CspNumber = db.GetString(reader, 1);
        entities.DebtAdjustment.CpaType = db.GetString(reader, 2);
        entities.DebtAdjustment.SystemGeneratedIdentifier =
          db.GetInt32(reader, 3);
        entities.DebtAdjustment.Type1 = db.GetString(reader, 4);
        entities.DebtAdjustment.Amount = db.GetDecimal(reader, 5);
        entities.DebtAdjustment.DebtAdjustmentType = db.GetString(reader, 6);
        entities.DebtAdjustment.DebtAdjustmentDt = db.GetDate(reader, 7);
        entities.DebtAdjustment.CreatedBy = db.GetString(reader, 8);
        entities.DebtAdjustment.CreatedTmst = db.GetDateTime(reader, 9);
        entities.DebtAdjustment.CspSupNumber = db.GetNullableString(reader, 10);
        entities.DebtAdjustment.CpaSupType = db.GetNullableString(reader, 11);
        entities.DebtAdjustment.OtyType = db.GetInt32(reader, 12);
        entities.DebtAdjustment.DebtAdjustmentProcessDate =
          db.GetDate(reader, 13);
        entities.DebtAdjustment.DebtAdjCollAdjProcReqInd =
          db.GetString(reader, 14);
        entities.DebtAdjustment.ReverseCollectionsInd =
          db.GetNullableString(reader, 15);
        entities.DebtAdjustment.Populated = true;
        CheckValid<ObligationTransaction>("CpaType",
          entities.DebtAdjustment.CpaType);
        CheckValid<ObligationTransaction>("Type1", entities.DebtAdjustment.Type1);
          
        CheckValid<ObligationTransaction>("DebtAdjustmentType",
          entities.DebtAdjustment.DebtAdjustmentType);
        CheckValid<ObligationTransaction>("CpaSupType",
          entities.DebtAdjustment.CpaSupType);
        CheckValid<ObligationTransaction>("DebtAdjCollAdjProcReqInd",
          entities.DebtAdjustment.DebtAdjCollAdjProcReqInd);
      });
  }

  private bool ReadObligationDebtDebtDetail()
  {
    entities.Obligation.Populated = false;
    entities.Debt.Populated = false;
    entities.DebtDetail.Populated = false;

    return Read("ReadObligationDebtDebtDetail",
      (db, command) =>
      {
        db.SetInt32(
          command, "dtyGeneratedId",
          entities.ObligationType.SystemGeneratedIdentifier);
        db.
          SetInt32(command, "obId", import.Obligation.SystemGeneratedIdentifier);
          
        db.SetInt32(command, "obTrnId", import.Debt.SystemGeneratedIdentifier);
        db.SetString(command, "cspNumber", import.CsePerson.Number);
      },
      (db, reader) =>
      {
        entities.Obligation.CpaType = db.GetString(reader, 0);
        entities.Debt.CpaType = db.GetString(reader, 0);
        entities.DebtDetail.CpaType = db.GetString(reader, 0);
        entities.Obligation.CspNumber = db.GetString(reader, 1);
        entities.Debt.CspNumber = db.GetString(reader, 1);
        entities.DebtDetail.CspNumber = db.GetString(reader, 1);
        entities.Obligation.SystemGeneratedIdentifier = db.GetInt32(reader, 2);
        entities.Debt.ObgGeneratedId = db.GetInt32(reader, 2);
        entities.DebtDetail.ObgGeneratedId = db.GetInt32(reader, 2);
        entities.Obligation.DtyGeneratedId = db.GetInt32(reader, 3);
        entities.Debt.OtyType = db.GetInt32(reader, 3);
        entities.DebtDetail.OtyType = db.GetInt32(reader, 3);
        entities.Obligation.PrimarySecondaryCode =
          db.GetNullableString(reader, 4);
        entities.Obligation.OrderTypeCode = db.GetString(reader, 5);
        entities.Debt.SystemGeneratedIdentifier = db.GetInt32(reader, 6);
        entities.DebtDetail.OtrGeneratedId = db.GetInt32(reader, 6);
        entities.Debt.Type1 = db.GetString(reader, 7);
        entities.DebtDetail.OtrType = db.GetString(reader, 7);
        entities.Debt.CspSupNumber = db.GetNullableString(reader, 8);
        entities.Debt.CpaSupType = db.GetNullableString(reader, 9);
        entities.DebtDetail.DueDt = db.GetDate(reader, 10);
        entities.DebtDetail.BalanceDueAmt = db.GetDecimal(reader, 11);
        entities.DebtDetail.InterestBalanceDueAmt =
          db.GetNullableDecimal(reader, 12);
        entities.DebtDetail.AdcDt = db.GetNullableDate(reader, 13);
        entities.DebtDetail.RetiredDt = db.GetNullableDate(reader, 14);
        entities.DebtDetail.CoveredPrdStartDt = db.GetNullableDate(reader, 15);
        entities.DebtDetail.CoveredPrdEndDt = db.GetNullableDate(reader, 16);
        entities.DebtDetail.PreconversionProgramCode =
          db.GetNullableString(reader, 17);
        entities.DebtDetail.LastUpdatedTmst =
          db.GetNullableDateTime(reader, 18);
        entities.DebtDetail.LastUpdatedBy = db.GetNullableString(reader, 19);
        entities.Obligation.Populated = true;
        entities.Debt.Populated = true;
        entities.DebtDetail.Populated = true;
        CheckValid<Obligation>("CpaType", entities.Obligation.CpaType);
        CheckValid<ObligationTransaction>("CpaType", entities.Debt.CpaType);
        CheckValid<DebtDetail>("CpaType", entities.DebtDetail.CpaType);
        CheckValid<Obligation>("PrimarySecondaryCode",
          entities.Obligation.PrimarySecondaryCode);
        CheckValid<Obligation>("OrderTypeCode",
          entities.Obligation.OrderTypeCode);
        CheckValid<ObligationTransaction>("Type1", entities.Debt.Type1);
        CheckValid<DebtDetail>("OtrType", entities.DebtDetail.OtrType);
        CheckValid<ObligationTransaction>("CpaSupType", entities.Debt.CpaSupType);
          
      });
  }

  private IEnumerable<bool> ReadObligationTransactionRlnRsnDebtAdjustment()
  {
    System.Diagnostics.Debug.Assert(entities.Debt.Populated);
    entities.ObligationTransactionRlnRsn.Populated = false;
    entities.DebtAdjustment.Populated = false;

    return ReadEach("ReadObligationTransactionRlnRsnDebtAdjustment",
      (db, command) =>
      {
        db.SetInt32(command, "otyTypePrimary", entities.Debt.OtyType);
        db.SetString(command, "otrPType", entities.Debt.Type1);
        db.SetInt32(
          command, "otrPGeneratedId", entities.Debt.SystemGeneratedIdentifier);
        db.SetString(command, "cpaPType", entities.Debt.CpaType);
        db.SetString(command, "cspPNumber", entities.Debt.CspNumber);
        db.SetInt32(command, "obgPGeneratedId", entities.Debt.ObgGeneratedId);
      },
      (db, reader) =>
      {
        entities.ObligationTransactionRlnRsn.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.DebtAdjustment.ObgGeneratedId = db.GetInt32(reader, 1);
        entities.DebtAdjustment.CspNumber = db.GetString(reader, 2);
        entities.DebtAdjustment.CpaType = db.GetString(reader, 3);
        entities.DebtAdjustment.SystemGeneratedIdentifier =
          db.GetInt32(reader, 4);
        entities.DebtAdjustment.Type1 = db.GetString(reader, 5);
        entities.DebtAdjustment.Amount = db.GetDecimal(reader, 6);
        entities.DebtAdjustment.DebtAdjustmentType = db.GetString(reader, 7);
        entities.DebtAdjustment.DebtAdjustmentDt = db.GetDate(reader, 8);
        entities.DebtAdjustment.CreatedBy = db.GetString(reader, 9);
        entities.DebtAdjustment.CreatedTmst = db.GetDateTime(reader, 10);
        entities.DebtAdjustment.CspSupNumber = db.GetNullableString(reader, 11);
        entities.DebtAdjustment.CpaSupType = db.GetNullableString(reader, 12);
        entities.DebtAdjustment.OtyType = db.GetInt32(reader, 13);
        entities.DebtAdjustment.DebtAdjustmentProcessDate =
          db.GetDate(reader, 14);
        entities.DebtAdjustment.DebtAdjCollAdjProcReqInd =
          db.GetString(reader, 15);
        entities.DebtAdjustment.ReverseCollectionsInd =
          db.GetNullableString(reader, 16);
        entities.ObligationTransactionRlnRsn.Populated = true;
        entities.DebtAdjustment.Populated = true;
        CheckValid<ObligationTransaction>("CpaType",
          entities.DebtAdjustment.CpaType);
        CheckValid<ObligationTransaction>("Type1", entities.DebtAdjustment.Type1);
          
        CheckValid<ObligationTransaction>("DebtAdjustmentType",
          entities.DebtAdjustment.DebtAdjustmentType);
        CheckValid<ObligationTransaction>("CpaSupType",
          entities.DebtAdjustment.CpaSupType);
        CheckValid<ObligationTransaction>("DebtAdjCollAdjProcReqInd",
          entities.DebtAdjustment.DebtAdjCollAdjProcReqInd);

        return true;
      });
  }

  private bool ReadObligationType()
  {
    entities.ObligationType.Populated = false;

    return Read("ReadObligationType",
      (db, command) =>
      {
        db.SetInt32(
          command, "debtTypId",
          import.ObligationType.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.ObligationType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.ObligationType.Classification = db.GetString(reader, 1);
        entities.ObligationType.SupportedPersonReqInd = db.GetString(reader, 2);
        entities.ObligationType.Populated = true;
        CheckValid<ObligationType>("Classification",
          entities.ObligationType.Classification);
        CheckValid<ObligationType>("SupportedPersonReqInd",
          entities.ObligationType.SupportedPersonReqInd);
      });
  }
#endregion

#region Parameters.
  protected readonly Import import;
  protected readonly Export export;
  protected readonly Local local;
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
    /// A value of DebtAdjustment.
    /// </summary>
    [JsonPropertyName("debtAdjustment")]
    public ObligationTransaction DebtAdjustment
    {
      get => debtAdjustment ??= new();
      set => debtAdjustment = value;
    }

    /// <summary>
    /// A value of ObligationType.
    /// </summary>
    [JsonPropertyName("obligationType")]
    public ObligationType ObligationType
    {
      get => obligationType ??= new();
      set => obligationType = value;
    }

    /// <summary>
    /// A value of Obligation.
    /// </summary>
    [JsonPropertyName("obligation")]
    public Obligation Obligation
    {
      get => obligation ??= new();
      set => obligation = value;
    }

    /// <summary>
    /// A value of Debt.
    /// </summary>
    [JsonPropertyName("debt")]
    public ObligationTransaction Debt
    {
      get => debt ??= new();
      set => debt = value;
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
    /// A value of HardcodeReinstate.
    /// </summary>
    [JsonPropertyName("hardcodeReinstate")]
    public ObligationTransactionRlnRsn HardcodeReinstate
    {
      get => hardcodeReinstate ??= new();
      set => hardcodeReinstate = value;
    }

    /// <summary>
    /// A value of HardcodeWriteoffAll.
    /// </summary>
    [JsonPropertyName("hardcodeWriteoffAll")]
    public ObligationTransactionRlnRsn HardcodeWriteoffAll
    {
      get => hardcodeWriteoffAll ??= new();
      set => hardcodeWriteoffAll = value;
    }

    /// <summary>
    /// A value of HardcodeWriteoffNaOnly.
    /// </summary>
    [JsonPropertyName("hardcodeWriteoffNaOnly")]
    public ObligationTransactionRlnRsn HardcodeWriteoffNaOnly
    {
      get => hardcodeWriteoffNaOnly ??= new();
      set => hardcodeWriteoffNaOnly = value;
    }

    /// <summary>
    /// A value of HardcodeCloseCase.
    /// </summary>
    [JsonPropertyName("hardcodeCloseCase")]
    public ObligationTransactionRlnRsn HardcodeCloseCase
    {
      get => hardcodeCloseCase ??= new();
      set => hardcodeCloseCase = value;
    }

    /// <summary>
    /// A value of ObligationTransactionRlnRsn.
    /// </summary>
    [JsonPropertyName("obligationTransactionRlnRsn")]
    public ObligationTransactionRlnRsn ObligationTransactionRlnRsn
    {
      get => obligationTransactionRlnRsn ??= new();
      set => obligationTransactionRlnRsn = value;
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
    /// A value of HardcodedIncAdj.
    /// </summary>
    [JsonPropertyName("hardcodedIncAdj")]
    public ObligationTransactionRlnRsn HardcodedIncAdj
    {
      get => hardcodedIncAdj ??= new();
      set => hardcodedIncAdj = value;
    }

    /// <summary>
    /// A value of HardcodedWriteoffAf.
    /// </summary>
    [JsonPropertyName("hardcodedWriteoffAf")]
    public ObligationTransactionRlnRsn HardcodedWriteoffAf
    {
      get => hardcodedWriteoffAf ??= new();
      set => hardcodedWriteoffAf = value;
    }

    private ObligationTransaction debtAdjustment;
    private ObligationType obligationType;
    private Obligation obligation;
    private ObligationTransaction debt;
    private CsePerson csePerson;
    private ObligationTransactionRlnRsn hardcodeReinstate;
    private ObligationTransactionRlnRsn hardcodeWriteoffAll;
    private ObligationTransactionRlnRsn hardcodeWriteoffNaOnly;
    private ObligationTransactionRlnRsn hardcodeCloseCase;
    private ObligationTransactionRlnRsn obligationTransactionRlnRsn;
    private DateWorkArea current;
    private ObligationTransactionRlnRsn hardcodedIncAdj;
    private ObligationTransactionRlnRsn hardcodedWriteoffAf;
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
  public class Local: IInitializable
  {
    /// <summary>
    /// A value of DebtAdjustment.
    /// </summary>
    [JsonPropertyName("debtAdjustment")]
    public ObligationTransaction DebtAdjustment
    {
      get => debtAdjustment ??= new();
      set => debtAdjustment = value;
    }

    /// <summary>
    /// A value of CollectionAdjustmentReason.
    /// </summary>
    [JsonPropertyName("collectionAdjustmentReason")]
    public CollectionAdjustmentReason CollectionAdjustmentReason
    {
      get => collectionAdjustmentReason ??= new();
      set => collectionAdjustmentReason = value;
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
    /// A value of HardcodedAccruing.
    /// </summary>
    [JsonPropertyName("hardcodedAccruing")]
    public ObligationType HardcodedAccruing
    {
      get => hardcodedAccruing ??= new();
      set => hardcodedAccruing = value;
    }

    /// <summary>
    /// A value of ProtectedCollection.
    /// </summary>
    [JsonPropertyName("protectedCollection")]
    public Common ProtectedCollection
    {
      get => protectedCollection ??= new();
      set => protectedCollection = value;
    }

    /// <summary>
    /// A value of AutoCollTotal.
    /// </summary>
    [JsonPropertyName("autoCollTotal")]
    public Common AutoCollTotal
    {
      get => autoCollTotal ??= new();
      set => autoCollTotal = value;
    }

    /// <summary>
    /// A value of Program.
    /// </summary>
    [JsonPropertyName("program")]
    public Program Program
    {
      get => program ??= new();
      set => program = value;
    }

    /// <para>Resets the state.</para>
    void IInitializable.Initialize()
    {
      debtAdjustment = null;
      hardcodedAccruing = null;
      protectedCollection = null;
      autoCollTotal = null;
      program = null;
    }

    private ObligationTransaction debtAdjustment;
    private CollectionAdjustmentReason collectionAdjustmentReason;
    private Collection collection;
    private ObligationType hardcodedAccruing;
    private Common protectedCollection;
    private Common autoCollTotal;
    private Program program;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of ObligationType.
    /// </summary>
    [JsonPropertyName("obligationType")]
    public ObligationType ObligationType
    {
      get => obligationType ??= new();
      set => obligationType = value;
    }

    /// <summary>
    /// A value of Obligation.
    /// </summary>
    [JsonPropertyName("obligation")]
    public Obligation Obligation
    {
      get => obligation ??= new();
      set => obligation = value;
    }

    /// <summary>
    /// A value of Debt.
    /// </summary>
    [JsonPropertyName("debt")]
    public ObligationTransaction Debt
    {
      get => debt ??= new();
      set => debt = value;
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

    /// <summary>
    /// A value of Obligor1.
    /// </summary>
    [JsonPropertyName("obligor1")]
    public CsePersonAccount Obligor1
    {
      get => obligor1 ??= new();
      set => obligor1 = value;
    }

    /// <summary>
    /// A value of Obligor2.
    /// </summary>
    [JsonPropertyName("obligor2")]
    public CsePerson Obligor2
    {
      get => obligor2 ??= new();
      set => obligor2 = value;
    }

    /// <summary>
    /// A value of Supported1.
    /// </summary>
    [JsonPropertyName("supported1")]
    public CsePerson Supported1
    {
      get => supported1 ??= new();
      set => supported1 = value;
    }

    /// <summary>
    /// A value of Supported2.
    /// </summary>
    [JsonPropertyName("supported2")]
    public CsePersonAccount Supported2
    {
      get => supported2 ??= new();
      set => supported2 = value;
    }

    /// <summary>
    /// A value of ObligationTransactionRlnRsn.
    /// </summary>
    [JsonPropertyName("obligationTransactionRlnRsn")]
    public ObligationTransactionRlnRsn ObligationTransactionRlnRsn
    {
      get => obligationTransactionRlnRsn ??= new();
      set => obligationTransactionRlnRsn = value;
    }

    /// <summary>
    /// A value of DebtAdjustment.
    /// </summary>
    [JsonPropertyName("debtAdjustment")]
    public ObligationTransaction DebtAdjustment
    {
      get => debtAdjustment ??= new();
      set => debtAdjustment = value;
    }

    /// <summary>
    /// A value of ObligationTransactionRln.
    /// </summary>
    [JsonPropertyName("obligationTransactionRln")]
    public ObligationTransactionRln ObligationTransactionRln
    {
      get => obligationTransactionRln ??= new();
      set => obligationTransactionRln = value;
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

    private ObligationType obligationType;
    private Obligation obligation;
    private ObligationTransaction debt;
    private DebtDetail debtDetail;
    private CsePersonAccount obligor1;
    private CsePerson obligor2;
    private CsePerson supported1;
    private CsePersonAccount supported2;
    private ObligationTransactionRlnRsn obligationTransactionRlnRsn;
    private ObligationTransaction debtAdjustment;
    private ObligationTransactionRln obligationTransactionRln;
    private Collection collection;
  }
#endregion
}
