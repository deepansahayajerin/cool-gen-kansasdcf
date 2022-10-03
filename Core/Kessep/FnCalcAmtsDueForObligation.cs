// Program: FN_CALC_AMTS_DUE_FOR_OBLIGATION, ID: 371739589, model: 746.
// Short name: SWE00303
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: FN_CALC_AMTS_DUE_FOR_OBLIGATION.
/// </para>
/// <para>
/// RESP: FINANCE
/// This CAB will calulate the amounts due for an obligation.
/// Determine the status for the obligation.
/// Get the service provider for the obligation.
/// </para>
/// </summary>
[Serializable]
public partial class FnCalcAmtsDueForObligation: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_CALC_AMTS_DUE_FOR_OBLIGATION program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnCalcAmtsDueForObligation(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnCalcAmtsDueForObligation.
  /// </summary>
  public FnCalcAmtsDueForObligation(IContext context, Import import,
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
    // ****************************************************
    // A.Kinney  05/01/97	Changed Current_Date
    // ****************************************************
    // Sept 29, 1999, pr#75508, mbrown: Amount due calculation is including 
    // inactive
    // supported persons (accrual instructions have been discontinued).  Added 
    // qualifier
    // to the read of accrual instructions to check discontinue date.
    local.Current.Date = Now().Date;

    if (IsEmpty(import.CsePerson.Number))
    {
      ExitState = "CSE_PERSON_NF";

      return;
    }

    if (import.ObligationType.SystemGeneratedIdentifier == 0)
    {
      ExitState = "FN0000_OBLIG_TYPE_NF";

      return;
    }

    if (import.Obligation.SystemGeneratedIdentifier == 0)
    {
      ExitState = "FN0000_OBLIGATION_NF";

      return;
    }

    // : Initialize Hardcoded constants
    UseFnHardcodedDebtDistribution();

    if (IsEmpty(import.ObligationType.Classification))
    {
      if (ReadObligationType())
      {
        MoveObligationType(entities.ObligationType, local.ObligationType);
      }
      else
      {
        ExitState = "FN0000_OBLIG_TYPE_NF";

        return;
      }
    }
    else
    {
      MoveObligationType(import.ObligationType, local.ObligationType);
    }

    if (!ReadObligation())
    {
      ExitState = "FN0000_OBLIGATION_NF";

      return;
    }

    // : Process Accruing Obligations.
    if (AsChar(local.ObligationType.Classification) == 'A')
    {
      // : Check to see if accrual has been discontinued.
      if (!ReadObligationPaymentSchedule())
      {
        return;
      }

      // : Read each set of accrual instructions.
      //   One for each supported person.
      // Sept 29, 1999, pr#75508, mbrown: Added qualifier to check accrual 
      // instructions
      // discontinue date.
      foreach(var item in ReadDebtAccrualInstructions())
      {
        if (ReadAccrualSuspension())
        {
          // : Suspension found - Add reduced amount to current due.
          local.TmpAmount.TotalCurrency += entities.Debt.Amount;
        }
        else
        {
          // : No suspension found - Add amount to current due.
          local.TmpAmount.TotalCurrency += entities.Debt.Amount;
        }
      }

      // : Must convert amount to a monthly amount.
      UseFnCalculateMonthlyAmountDue();
      export.ScreenDueAmounts.CurrentAmountDue = local.TmpAmount.TotalCurrency;
    }
    else
    {
      // *****
      // Determine the Periodic amount for non accruing debts.
      // *****
      // : Obligation is a non accruing type.
      //   The obligation payment schedule defines the periodic amount due.
      if (ReadObligationPaymentSchedule())
      {
        local.TmpAmount.TotalCurrency =
          entities.ObligationPaymentSchedule.Amount.GetValueOrDefault();
        UseFnCalculateMonthlyAmountDue();
        export.ScreenDueAmounts.PeriodicAmountDue =
          local.TmpAmount.TotalCurrency;
      }
      else
      {
        // : No periodic amount due for this obligation.
      }
    }

    export.ScreenDueAmounts.TotalAmountDue =
      export.ScreenDueAmounts.CurrentAmountDue + export
      .ScreenDueAmounts.PeriodicAmountDue;
  }

  private static void MoveObligationTransaction(ObligationTransaction source,
    ObligationTransaction target)
  {
    target.Type1 = source.Type1;
    target.DebtType = source.DebtType;
  }

  private static void MoveObligationType(ObligationType source,
    ObligationType target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Classification = source.Classification;
  }

  private void UseFnCalculateMonthlyAmountDue()
  {
    var useImport = new FnCalculateMonthlyAmountDue.Import();
    var useExport = new FnCalculateMonthlyAmountDue.Export();

    useImport.ObligationPaymentSchedule.Assign(
      entities.ObligationPaymentSchedule);
    useImport.PeriodAmountDue.TotalCurrency = local.TmpAmount.TotalCurrency;
    useImport.Period.Date = local.Current.Date;

    Call(FnCalculateMonthlyAmountDue.Execute, useImport, useExport);

    local.TmpAmount.TotalCurrency = useExport.MonthlyDue.TotalCurrency;
  }

  private void UseFnHardcodedDebtDistribution()
  {
    var useImport = new FnHardcodedDebtDistribution.Import();
    var useExport = new FnHardcodedDebtDistribution.Export();

    Call(FnHardcodedDebtDistribution.Execute, useImport, useExport);

    local.HardcodeObligorType.Type1 = useExport.CpaObligor.Type1;
    local.HardcodedDebtType.Type1 = useExport.OtrnTDebt.Type1;
    MoveObligationTransaction(useExport.OtrnDtAccrualInstructions,
      local.HardcodedAccuralInstrType);
  }

  private bool ReadAccrualSuspension()
  {
    System.Diagnostics.Debug.Assert(entities.AccrualInstructions.Populated);
    entities.AccrualSuspension.Populated = false;

    return Read("ReadAccrualSuspension",
      (db, command) =>
      {
        db.SetInt32(
          command, "otrId", entities.AccrualInstructions.OtrGeneratedId);
        db.SetString(command, "cpaType", entities.AccrualInstructions.CpaType);
        db.SetString(
          command, "cspNumber", entities.AccrualInstructions.CspNumber);
        db.SetInt32(
          command, "obgId", entities.AccrualInstructions.ObgGeneratedId);
        db.SetInt32(command, "otyId", entities.AccrualInstructions.OtyId);
        db.SetString(command, "otrType", entities.AccrualInstructions.OtrType);
        db.
          SetDate(command, "suspendDt", local.Current.Date.GetValueOrDefault());
          
      },
      (db, reader) =>
      {
        entities.AccrualSuspension.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.AccrualSuspension.SuspendDt = db.GetDate(reader, 1);
        entities.AccrualSuspension.ResumeDt = db.GetNullableDate(reader, 2);
        entities.AccrualSuspension.ReductionPercentage =
          db.GetNullableDecimal(reader, 3);
        entities.AccrualSuspension.OtrType = db.GetString(reader, 4);
        entities.AccrualSuspension.OtyId = db.GetInt32(reader, 5);
        entities.AccrualSuspension.ObgId = db.GetInt32(reader, 6);
        entities.AccrualSuspension.CspNumber = db.GetString(reader, 7);
        entities.AccrualSuspension.CpaType = db.GetString(reader, 8);
        entities.AccrualSuspension.OtrId = db.GetInt32(reader, 9);
        entities.AccrualSuspension.Populated = true;
        CheckValid<AccrualSuspension>("OtrType",
          entities.AccrualSuspension.OtrType);
        CheckValid<AccrualSuspension>("CpaType",
          entities.AccrualSuspension.CpaType);
      });
  }

  private IEnumerable<bool> ReadDebtAccrualInstructions()
  {
    System.Diagnostics.Debug.Assert(entities.Obligation.Populated);
    entities.Debt.Populated = false;
    entities.AccrualInstructions.Populated = false;

    return ReadEach("ReadDebtAccrualInstructions",
      (db, command) =>
      {
        db.SetInt32(command, "otyType", entities.Obligation.DtyGeneratedId);
        db.SetInt32(
          command, "obgGeneratedId",
          entities.Obligation.SystemGeneratedIdentifier);
        db.SetString(command, "cspNumber", entities.Obligation.CspNumber);
        db.SetString(command, "cpaType", entities.Obligation.CpaType);
        db.SetNullableDate(
          command, "discontinueDt", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Debt.ObgGeneratedId = db.GetInt32(reader, 0);
        entities.AccrualInstructions.ObgGeneratedId = db.GetInt32(reader, 0);
        entities.Debt.CspNumber = db.GetString(reader, 1);
        entities.AccrualInstructions.CspNumber = db.GetString(reader, 1);
        entities.Debt.CpaType = db.GetString(reader, 2);
        entities.AccrualInstructions.CpaType = db.GetString(reader, 2);
        entities.Debt.SystemGeneratedIdentifier = db.GetInt32(reader, 3);
        entities.AccrualInstructions.OtrGeneratedId = db.GetInt32(reader, 3);
        entities.Debt.Type1 = db.GetString(reader, 4);
        entities.AccrualInstructions.OtrType = db.GetString(reader, 4);
        entities.Debt.Amount = db.GetDecimal(reader, 5);
        entities.Debt.CspSupNumber = db.GetNullableString(reader, 6);
        entities.Debt.CpaSupType = db.GetNullableString(reader, 7);
        entities.Debt.OtyType = db.GetInt32(reader, 8);
        entities.AccrualInstructions.OtyId = db.GetInt32(reader, 8);
        entities.AccrualInstructions.AsOfDt = db.GetDate(reader, 9);
        entities.AccrualInstructions.DiscontinueDt =
          db.GetNullableDate(reader, 10);
        entities.Debt.Populated = true;
        entities.AccrualInstructions.Populated = true;
        CheckValid<ObligationTransaction>("CpaType", entities.Debt.CpaType);
        CheckValid<AccrualInstructions>("CpaType",
          entities.AccrualInstructions.CpaType);
        CheckValid<ObligationTransaction>("Type1", entities.Debt.Type1);
        CheckValid<AccrualInstructions>("OtrType",
          entities.AccrualInstructions.OtrType);
        CheckValid<ObligationTransaction>("CpaSupType", entities.Debt.CpaSupType);
          

        return true;
      });
  }

  private bool ReadObligation()
  {
    entities.Obligation.Populated = false;

    return Read("ReadObligation",
      (db, command) =>
      {
        db.
          SetInt32(command, "obId", import.Obligation.SystemGeneratedIdentifier);
          
        db.SetInt32(
          command, "dtyGeneratedId",
          import.ObligationType.SystemGeneratedIdentifier);
        db.SetString(command, "cspNumber", import.CsePerson.Number);
      },
      (db, reader) =>
      {
        entities.Obligation.CpaType = db.GetString(reader, 0);
        entities.Obligation.CspNumber = db.GetString(reader, 1);
        entities.Obligation.SystemGeneratedIdentifier = db.GetInt32(reader, 2);
        entities.Obligation.DtyGeneratedId = db.GetInt32(reader, 3);
        entities.Obligation.PrimarySecondaryCode =
          db.GetNullableString(reader, 4);
        entities.Obligation.Populated = true;
        CheckValid<Obligation>("CpaType", entities.Obligation.CpaType);
        CheckValid<Obligation>("PrimarySecondaryCode",
          entities.Obligation.PrimarySecondaryCode);
      });
  }

  private bool ReadObligationPaymentSchedule()
  {
    System.Diagnostics.Debug.Assert(entities.Obligation.Populated);
    entities.ObligationPaymentSchedule.Populated = false;

    return Read("ReadObligationPaymentSchedule",
      (db, command) =>
      {
        db.SetInt32(command, "otyType", entities.Obligation.DtyGeneratedId);
        db.SetInt32(
          command, "obgGeneratedId",
          entities.Obligation.SystemGeneratedIdentifier);
        db.SetString(command, "obgCspNumber", entities.Obligation.CspNumber);
        db.SetString(command, "obgCpaType", entities.Obligation.CpaType);
        db.SetDate(command, "startDt", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ObligationPaymentSchedule.OtyType = db.GetInt32(reader, 0);
        entities.ObligationPaymentSchedule.ObgGeneratedId =
          db.GetInt32(reader, 1);
        entities.ObligationPaymentSchedule.ObgCspNumber =
          db.GetString(reader, 2);
        entities.ObligationPaymentSchedule.ObgCpaType = db.GetString(reader, 3);
        entities.ObligationPaymentSchedule.StartDt = db.GetDate(reader, 4);
        entities.ObligationPaymentSchedule.Amount =
          db.GetNullableDecimal(reader, 5);
        entities.ObligationPaymentSchedule.EndDt =
          db.GetNullableDate(reader, 6);
        entities.ObligationPaymentSchedule.FrequencyCode =
          db.GetString(reader, 7);
        entities.ObligationPaymentSchedule.DayOfWeek =
          db.GetNullableInt32(reader, 8);
        entities.ObligationPaymentSchedule.DayOfMonth1 =
          db.GetNullableInt32(reader, 9);
        entities.ObligationPaymentSchedule.DayOfMonth2 =
          db.GetNullableInt32(reader, 10);
        entities.ObligationPaymentSchedule.PeriodInd =
          db.GetNullableString(reader, 11);
        entities.ObligationPaymentSchedule.Populated = true;
        CheckValid<ObligationPaymentSchedule>("ObgCpaType",
          entities.ObligationPaymentSchedule.ObgCpaType);
        CheckValid<ObligationPaymentSchedule>("FrequencyCode",
          entities.ObligationPaymentSchedule.FrequencyCode);
        CheckValid<ObligationPaymentSchedule>("PeriodInd",
          entities.ObligationPaymentSchedule.PeriodInd);
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
        entities.ObligationType.Populated = true;
        CheckValid<ObligationType>("Classification",
          entities.ObligationType.Classification);
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
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
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

    private CsePerson csePerson;
    private ObligationType obligationType;
    private Obligation obligation;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of ScreenDueAmounts.
    /// </summary>
    [JsonPropertyName("screenDueAmounts")]
    public ScreenDueAmounts ScreenDueAmounts
    {
      get => screenDueAmounts ??= new();
      set => screenDueAmounts = value;
    }

    private ScreenDueAmounts screenDueAmounts;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
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
    /// A value of Current.
    /// </summary>
    [JsonPropertyName("current")]
    public DateWorkArea Current
    {
      get => current ??= new();
      set => current = value;
    }

    /// <summary>
    /// A value of Ops.
    /// </summary>
    [JsonPropertyName("ops")]
    public Common Ops
    {
      get => ops ??= new();
      set => ops = value;
    }

    /// <summary>
    /// A value of HardcodeObligorType.
    /// </summary>
    [JsonPropertyName("hardcodeObligorType")]
    public CsePersonAccount HardcodeObligorType
    {
      get => hardcodeObligorType ??= new();
      set => hardcodeObligorType = value;
    }

    /// <summary>
    /// A value of TmpAmount.
    /// </summary>
    [JsonPropertyName("tmpAmount")]
    public Common TmpAmount
    {
      get => tmpAmount ??= new();
      set => tmpAmount = value;
    }

    /// <summary>
    /// A value of TmpDate.
    /// </summary>
    [JsonPropertyName("tmpDate")]
    public DateWorkArea TmpDate
    {
      get => tmpDate ??= new();
      set => tmpDate = value;
    }

    /// <summary>
    /// A value of HardcodedDebtType.
    /// </summary>
    [JsonPropertyName("hardcodedDebtType")]
    public ObligationTransaction HardcodedDebtType
    {
      get => hardcodedDebtType ??= new();
      set => hardcodedDebtType = value;
    }

    /// <summary>
    /// A value of HardcodedAccuralInstrType.
    /// </summary>
    [JsonPropertyName("hardcodedAccuralInstrType")]
    public ObligationTransaction HardcodedAccuralInstrType
    {
      get => hardcodedAccuralInstrType ??= new();
      set => hardcodedAccuralInstrType = value;
    }

    private ObligationType obligationType;
    private DateWorkArea current;
    private Common ops;
    private CsePersonAccount hardcodeObligorType;
    private Common tmpAmount;
    private DateWorkArea tmpDate;
    private ObligationTransaction hardcodedDebtType;
    private ObligationTransaction hardcodedAccuralInstrType;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of KeyOnly.
    /// </summary>
    [JsonPropertyName("keyOnly")]
    public ObligationType KeyOnly
    {
      get => keyOnly ??= new();
      set => keyOnly = value;
    }

    /// <summary>
    /// A value of Obligor.
    /// </summary>
    [JsonPropertyName("obligor")]
    public CsePersonAccount Obligor
    {
      get => obligor ??= new();
      set => obligor = value;
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
    /// A value of DelMeCsePersonAccount.
    /// </summary>
    [JsonPropertyName("delMeCsePersonAccount")]
    public CsePersonAccount DelMeCsePersonAccount
    {
      get => delMeCsePersonAccount ??= new();
      set => delMeCsePersonAccount = value;
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
    /// A value of ObligationType.
    /// </summary>
    [JsonPropertyName("obligationType")]
    public ObligationType ObligationType
    {
      get => obligationType ??= new();
      set => obligationType = value;
    }

    /// <summary>
    /// A value of ObligationPaymentSchedule.
    /// </summary>
    [JsonPropertyName("obligationPaymentSchedule")]
    public ObligationPaymentSchedule ObligationPaymentSchedule
    {
      get => obligationPaymentSchedule ??= new();
      set => obligationPaymentSchedule = value;
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
    /// A value of AccrualInstructions.
    /// </summary>
    [JsonPropertyName("accrualInstructions")]
    public AccrualInstructions AccrualInstructions
    {
      get => accrualInstructions ??= new();
      set => accrualInstructions = value;
    }

    /// <summary>
    /// A value of AccrualSuspension.
    /// </summary>
    [JsonPropertyName("accrualSuspension")]
    public AccrualSuspension AccrualSuspension
    {
      get => accrualSuspension ??= new();
      set => accrualSuspension = value;
    }

    /// <summary>
    /// A value of DelMeObligationTransaction.
    /// </summary>
    [JsonPropertyName("delMeObligationTransaction")]
    public ObligationTransaction DelMeObligationTransaction
    {
      get => delMeObligationTransaction ??= new();
      set => delMeObligationTransaction = value;
    }

    private ObligationType keyOnly;
    private CsePersonAccount obligor;
    private CsePerson csePerson;
    private CsePersonAccount delMeCsePersonAccount;
    private Obligation obligation;
    private ObligationType obligationType;
    private ObligationPaymentSchedule obligationPaymentSchedule;
    private ObligationTransaction debt;
    private AccrualInstructions accrualInstructions;
    private AccrualSuspension accrualSuspension;
    private ObligationTransaction delMeObligationTransaction;
  }
#endregion
}
