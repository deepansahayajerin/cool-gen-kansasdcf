// Program: FN_COMPUTE_TOTALS_FOR_CT_ORDER_3, ID: 371380964, model: 746.
// Short name: SWE03606
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_COMPUTE_TOTALS_FOR_CT_ORDER_3.
/// </summary>
[Serializable]
public partial class FnComputeTotalsForCtOrder3: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_COMPUTE_TOTALS_FOR_CT_ORDER_3 program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnComputeTotalsForCtOrder3(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnComputeTotalsForCtOrder3.
  /// </summary>
  public FnComputeTotalsForCtOrder3(IContext context, Import import,
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
    // **********************************************************************************************
    // Initial Code       Dwayne Dupree        02/29/2007
    // This is determining if debt owed as defined in the driver's license 
    // restriction process.
    // **********************************************************************************************
    ExitState = "ACO_NN0000_ALL_OK";

    if (IsEmpty(import.FilterByStdNo.StandardNumber))
    {
      ExitState = "FN0000_MUST_ENTER_COURT_ORDER";

      return;
    }

    export.ScreenOwedAmountsDtl.IncomingInterstateObExists = "N";
    UseFnHardcodedCashReceipting();
    UseFnHardcodedDebtDistribution();
    local.Process.Date = import.StartDate.Date;
    local.MaxDiscontinue.Date = UseCabSetMaximumDiscontinueDate();
    UseCabFirstAndLastDateOfMonth3();
    local.CurrentFirstOfTheMonth.Date = local.ProcessMonthBegin.Date;

    // DEBT_TYP_ID  DEBT_TYP_CD  
    // DEBT_TYP_NM
    // 
    // DEBT_TYP_CLA
    // ---------+---------+---------+---------+---------+---------+---------+
    // ---------+
    //           1  CS           CHILD SUPPORT                             A
    //           2  SP           SPOUSAL SUPPORT                           A
    //           3  MS           MEDICAL SUPPORT                           A
    //           4  IVD RC       IV-D RECOVERY                             R
    //           5  IRS NEG      IRS NEGATIVE RECOVERY                     R
    //           6  MIS AR       AR MISDIRECTED PAYMENT                    R
    //           7  MIS AP       AP MISDIRECTED PAYMENT                    R
    //           8  MIS NON      NON-CASE PERSON MISDIRECTED PAYMENT       R
    //           9  BDCK RC      BAD 
    // CHECK
    // 
    // R
    //          10  MJ           MEDICAL JUDGEMENT                         M
    //          11  %UME         PERCENT UNINSURED MEDICAL EXP JUDGEMENT   M
    //          12  IJ           INTEREST JUDGEMENT                        N
    //          13  AJ           ARREARS JUDGEMENT                         N
    //          14  CRCH         COST OF RAISING CHILD                     N
    //          15  FEE          GENETIC FEE TEST                          F
    //          16  VOL          
    // VOLUNTARY
    // 
    // V
    //          17  SAJ          SPOUSAL ARREARS JUDGEMENT                 N
    //          18  718B         718B JUDGEMENT                            N
    //          19  MC           MEDICAL COSTS                             A
    //          20  WA           WITHHOLDING ARREARS                       N
    //          21  WC           WITHHOLDING CURRENT                       A
    //          22  GIFT         GIFT
    // 
    // V
    local.CurrentObligationFound.Flag = "";
    local.OmitSecondaryObligInd.Flag = "Y";

    foreach(var item in ReadObligation())
    {
      if (AsChar(entities.ExistingObligation.PrimarySecondaryCode) == 'S')
      {
        continue;
      }

      local.DateWorkArea.Date = Date(entities.ExistingObligation.CreatedTmst);

      if (!Lt(local.DateWorkArea.Date, AddDays(import.StartDate.Date, -7)))
      {
        // next record
        continue;
      }

      if (ReadObligationType())
      {
        if (entities.ExistingObligationType.SystemGeneratedIdentifier == 1 || entities
          .ExistingObligationType.SystemGeneratedIdentifier == 2 || entities
          .ExistingObligationType.SystemGeneratedIdentifier == 3 || entities
          .ExistingObligationType.SystemGeneratedIdentifier == 19)
        {
          if (ReadAccrualInstructions())
          {
            if (Lt(AddDays(import.StartDate.Date, -91),
              entities.AccrualInstructions.AsOfDt))
            {
              continue;

              // next record
            }

            if (Lt(entities.AccrualInstructions.LastAccrualDt,
              import.StartDate.Date))
            {
              // this is a arrears only accural
            }
            else
            {
              local.Process.Date = entities.AccrualInstructions.LastAccrualDt;
              UseCabFirstAndLastDateOfMonth2();
              local.OneFutureMonthAccurals.Date =
                AddMonths(local.CurrentMonthAccurals.Date, 1);
              local.Process.Date = entities.AccrualInstructions.DiscontinueDt;
              UseCabFirstAndLastDateOfMonth1();

              if (Lt(local.OneFutureMonthAccurals.Date,
                local.TestDiscountinedDate.Date))
              {
                // this is a current accural
                local.CurrentObligationFound.Flag = "Y";
              }
              else
              {
                local.AmountOwed.TotalCurrency = 0;

                foreach(var item1 in ReadDebtDetail3())
                {
                  local.AmountOwed.TotalCurrency += entities.ExistingDebtDetail.
                    BalanceDueAmt;
                }

                if (local.AmountOwed.TotalCurrency <= 0)
                {
                  // this is a arrears only accural
                }
                else
                {
                  // this is a current accural
                  local.CurrentObligationFound.Flag = "Y";
                }
              }
            }

            goto Read;
          }

          if (!Lt(local.Null1.Date, entities.AccrualInstructions.AsOfDt))
          {
            // this will only happen if no record is found at all
            ExitState = "CO0000_ACCRUAL_INSTRUCTN_NF";

            return;
          }
        }
        else if (ReadDebtDetail1())
        {
          if (Lt(AddDays(import.StartDate.Date, -91),
            entities.ExistingDebtDetail.DueDt))
          {
            continue;

            // next record
          }
        }
        else
        {
          ExitState = "FN0211_DEBT_DETAIL_NF";

          return;
        }
      }
      else
      {
        continue;

        // next record
      }

Read:

      foreach(var item1 in ReadDebtDetail2())
      {
        if (!Lt(entities.ExistingDebtDetail.DueDt, local.ProcessMonthBegin.Date))
          
        {
          continue;
        }

        export.ScreenOwedAmountsDtl.TotalArrearsOwed += entities.
          ExistingDebtDetail.BalanceDueAmt;
        export.ScreenOwedAmountsDtl.TotalInterestOwed += entities.
          ExistingDebtDetail.InterestBalanceDueAmt.GetValueOrDefault();
        export.ScreenOwedAmountsDtl.TotalCurrArrIntOwed =
          export.ScreenOwedAmountsDtl.TotalCurrArrIntOwed + entities
          .ExistingDebtDetail.BalanceDueAmt + entities
          .ExistingDebtDetail.InterestBalanceDueAmt.GetValueOrDefault();
        export.ScreenOwedAmountsDtl.FeesArrearsOwed += entities.
          ExistingDebtDetail.BalanceDueAmt;
        export.ScreenOwedAmountsDtl.FeesInterestOwed += entities.
          ExistingDebtDetail.InterestBalanceDueAmt.GetValueOrDefault();
      }
    }

    if (AsChar(import.IncludeArrearsOnly.Flag) != 'Y')
    {
      if (AsChar(local.CurrentObligationFound.Flag) != 'Y')
      {
        // this is a arrears only obligations and since the include arrears only
        // flag is not 'Y'
        // than there has to be at least one obligation per court order that has
        // a current
        // accuring obligation.
        export.ScreenOwedAmountsDtl.TotalArrearsOwed = 0;
        export.ScreenOwedAmountsDtl.TotalInterestOwed = 0;
        export.ScreenOwedAmountsDtl.TotalCurrArrIntOwed = 0;
        export.ScreenOwedAmountsDtl.FeesArrearsOwed = 0;
        export.ScreenOwedAmountsDtl.FeesInterestOwed = 0;
      }
    }
  }

  private void UseCabFirstAndLastDateOfMonth1()
  {
    var useImport = new CabFirstAndLastDateOfMonth.Import();
    var useExport = new CabFirstAndLastDateOfMonth.Export();

    useImport.DateWorkArea.Date = local.Process.Date;

    Call(CabFirstAndLastDateOfMonth.Execute, useImport, useExport);

    local.TestDiscountinedDate.Date = useExport.First.Date;
    local.ProcessMonthEnd.Date = useExport.Last.Date;
  }

  private void UseCabFirstAndLastDateOfMonth2()
  {
    var useImport = new CabFirstAndLastDateOfMonth.Import();
    var useExport = new CabFirstAndLastDateOfMonth.Export();

    useImport.DateWorkArea.Date = local.Process.Date;

    Call(CabFirstAndLastDateOfMonth.Execute, useImport, useExport);

    local.CurrentMonthAccurals.Date = useExport.First.Date;
    local.ProcessMonthEnd.Date = useExport.Last.Date;
  }

  private void UseCabFirstAndLastDateOfMonth3()
  {
    var useImport = new CabFirstAndLastDateOfMonth.Import();
    var useExport = new CabFirstAndLastDateOfMonth.Export();

    useImport.DateWorkArea.Date = local.Process.Date;

    Call(CabFirstAndLastDateOfMonth.Execute, useImport, useExport);

    local.ProcessMonthBegin.Date = useExport.First.Date;
    local.ProcessMonthEnd.Date = useExport.Last.Date;
  }

  private DateTime? UseCabSetMaximumDiscontinueDate()
  {
    var useImport = new CabSetMaximumDiscontinueDate.Import();
    var useExport = new CabSetMaximumDiscontinueDate.Export();

    Call(CabSetMaximumDiscontinueDate.Execute, useImport, useExport);

    return useExport.DateWorkArea.Date;
  }

  private void UseFnHardcodedCashReceipting()
  {
    var useImport = new FnHardcodedCashReceipting.Import();
    var useExport = new FnHardcodedCashReceipting.Export();

    Call(FnHardcodedCashReceipting.Execute, useImport, useExport);

    local.HardcodedAdjusted.SystemGeneratedIdentifier =
      useExport.CrdsSystemId.CrdsIdAdjusted.SystemGeneratedIdentifier;
  }

  private void UseFnHardcodedDebtDistribution()
  {
    var useImport = new FnHardcodedDebtDistribution.Import();
    var useExport = new FnHardcodedDebtDistribution.Export();

    Call(FnHardcodedDebtDistribution.Execute, useImport, useExport);

    local.HardcodedVoluntary.SystemGeneratedIdentifier =
      useExport.OtVoluntary.SystemGeneratedIdentifier;
    local.HardcodedMsType.SystemGeneratedIdentifier =
      useExport.OtMedicalSupport.SystemGeneratedIdentifier;
    local.HardcodedMcType.SystemGeneratedIdentifier =
      useExport.OtMedicalSupportForCash.SystemGeneratedIdentifier;
    local.HardcodedCsType.SystemGeneratedIdentifier =
      useExport.OtChildSupport.SystemGeneratedIdentifier;
    local.HardcodedSpType.SystemGeneratedIdentifier =
      useExport.OtSpousalSupport.SystemGeneratedIdentifier;
    local.HardcodedAccruingClass.Classification =
      useExport.OtCAccruingClassification.Classification;
    local.HardcodedSecondary.PrimarySecondaryCode =
      useExport.ObligSecondaryConcurrent.PrimarySecondaryCode;
  }

  private bool ReadAccrualInstructions()
  {
    System.Diagnostics.Debug.Assert(entities.ExistingObligation.Populated);
    entities.AccrualInstructions.Populated = false;

    return Read("ReadAccrualInstructions",
      (db, command) =>
      {
        db.
          SetInt32(command, "otyId", entities.ExistingObligation.DtyGeneratedId);
          
        db.SetInt32(
          command, "obgGeneratedId",
          entities.ExistingObligation.SystemGeneratedIdentifier);
        db.
          SetString(command, "cspNumber", entities.ExistingObligation.CspNumber);
          
        db.SetString(command, "cpaType", entities.ExistingObligation.CpaType);
      },
      (db, reader) =>
      {
        entities.AccrualInstructions.OtrType = db.GetString(reader, 0);
        entities.AccrualInstructions.OtyId = db.GetInt32(reader, 1);
        entities.AccrualInstructions.ObgGeneratedId = db.GetInt32(reader, 2);
        entities.AccrualInstructions.CspNumber = db.GetString(reader, 3);
        entities.AccrualInstructions.CpaType = db.GetString(reader, 4);
        entities.AccrualInstructions.OtrGeneratedId = db.GetInt32(reader, 5);
        entities.AccrualInstructions.AsOfDt = db.GetDate(reader, 6);
        entities.AccrualInstructions.DiscontinueDt =
          db.GetNullableDate(reader, 7);
        entities.AccrualInstructions.LastAccrualDt =
          db.GetNullableDate(reader, 8);
        entities.AccrualInstructions.Populated = true;
        CheckValid<AccrualInstructions>("OtrType",
          entities.AccrualInstructions.OtrType);
        CheckValid<AccrualInstructions>("CpaType",
          entities.AccrualInstructions.CpaType);
      });
  }

  private bool ReadDebtDetail1()
  {
    System.Diagnostics.Debug.Assert(entities.ExistingObligation.Populated);
    entities.ExistingDebtDetail.Populated = false;

    return Read("ReadDebtDetail1",
      (db, command) =>
      {
        db.SetInt32(
          command, "otyType", entities.ExistingObligation.DtyGeneratedId);
        db.SetInt32(
          command, "obgGeneratedId",
          entities.ExistingObligation.SystemGeneratedIdentifier);
        db.
          SetString(command, "cspNumber", entities.ExistingObligation.CspNumber);
          
        db.SetString(command, "cpaType", entities.ExistingObligation.CpaType);
      },
      (db, reader) =>
      {
        entities.ExistingDebtDetail.ObgGeneratedId = db.GetInt32(reader, 0);
        entities.ExistingDebtDetail.CspNumber = db.GetString(reader, 1);
        entities.ExistingDebtDetail.CpaType = db.GetString(reader, 2);
        entities.ExistingDebtDetail.OtrGeneratedId = db.GetInt32(reader, 3);
        entities.ExistingDebtDetail.OtyType = db.GetInt32(reader, 4);
        entities.ExistingDebtDetail.OtrType = db.GetString(reader, 5);
        entities.ExistingDebtDetail.DueDt = db.GetDate(reader, 6);
        entities.ExistingDebtDetail.BalanceDueAmt = db.GetDecimal(reader, 7);
        entities.ExistingDebtDetail.InterestBalanceDueAmt =
          db.GetNullableDecimal(reader, 8);
        entities.ExistingDebtDetail.RetiredDt = db.GetNullableDate(reader, 9);
        entities.ExistingDebtDetail.CreatedTmst = db.GetDateTime(reader, 10);
        entities.ExistingDebtDetail.Populated = true;
        CheckValid<DebtDetail>("CpaType", entities.ExistingDebtDetail.CpaType);
        CheckValid<DebtDetail>("OtrType", entities.ExistingDebtDetail.OtrType);
      });
  }

  private IEnumerable<bool> ReadDebtDetail2()
  {
    System.Diagnostics.Debug.Assert(entities.ExistingObligation.Populated);
    entities.ExistingDebtDetail.Populated = false;

    return ReadEach("ReadDebtDetail2",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "retiredDt", local.Null1.Date.GetValueOrDefault());
        db.SetInt32(
          command, "otyType", entities.ExistingObligation.DtyGeneratedId);
        db.SetInt32(
          command, "obgGeneratedId",
          entities.ExistingObligation.SystemGeneratedIdentifier);
        db.
          SetString(command, "cspNumber", entities.ExistingObligation.CspNumber);
          
        db.SetString(command, "cpaType", entities.ExistingObligation.CpaType);
      },
      (db, reader) =>
      {
        entities.ExistingDebtDetail.ObgGeneratedId = db.GetInt32(reader, 0);
        entities.ExistingDebtDetail.CspNumber = db.GetString(reader, 1);
        entities.ExistingDebtDetail.CpaType = db.GetString(reader, 2);
        entities.ExistingDebtDetail.OtrGeneratedId = db.GetInt32(reader, 3);
        entities.ExistingDebtDetail.OtyType = db.GetInt32(reader, 4);
        entities.ExistingDebtDetail.OtrType = db.GetString(reader, 5);
        entities.ExistingDebtDetail.DueDt = db.GetDate(reader, 6);
        entities.ExistingDebtDetail.BalanceDueAmt = db.GetDecimal(reader, 7);
        entities.ExistingDebtDetail.InterestBalanceDueAmt =
          db.GetNullableDecimal(reader, 8);
        entities.ExistingDebtDetail.RetiredDt = db.GetNullableDate(reader, 9);
        entities.ExistingDebtDetail.CreatedTmst = db.GetDateTime(reader, 10);
        entities.ExistingDebtDetail.Populated = true;
        CheckValid<DebtDetail>("CpaType", entities.ExistingDebtDetail.CpaType);
        CheckValid<DebtDetail>("OtrType", entities.ExistingDebtDetail.OtrType);

        return true;
      });
  }

  private IEnumerable<bool> ReadDebtDetail3()
  {
    System.Diagnostics.Debug.Assert(entities.ExistingObligation.Populated);
    entities.ExistingDebtDetail.Populated = false;

    return ReadEach("ReadDebtDetail3",
      (db, command) =>
      {
        db.SetDate(
          command, "dueDt", local.ProcessMonthBegin.Date.GetValueOrDefault());
        db.SetInt32(
          command, "otyType", entities.ExistingObligation.DtyGeneratedId);
        db.SetInt32(
          command, "obgGeneratedId",
          entities.ExistingObligation.SystemGeneratedIdentifier);
        db.
          SetString(command, "cspNumber", entities.ExistingObligation.CspNumber);
          
        db.SetString(command, "cpaType", entities.ExistingObligation.CpaType);
      },
      (db, reader) =>
      {
        entities.ExistingDebtDetail.ObgGeneratedId = db.GetInt32(reader, 0);
        entities.ExistingDebtDetail.CspNumber = db.GetString(reader, 1);
        entities.ExistingDebtDetail.CpaType = db.GetString(reader, 2);
        entities.ExistingDebtDetail.OtrGeneratedId = db.GetInt32(reader, 3);
        entities.ExistingDebtDetail.OtyType = db.GetInt32(reader, 4);
        entities.ExistingDebtDetail.OtrType = db.GetString(reader, 5);
        entities.ExistingDebtDetail.DueDt = db.GetDate(reader, 6);
        entities.ExistingDebtDetail.BalanceDueAmt = db.GetDecimal(reader, 7);
        entities.ExistingDebtDetail.InterestBalanceDueAmt =
          db.GetNullableDecimal(reader, 8);
        entities.ExistingDebtDetail.RetiredDt = db.GetNullableDate(reader, 9);
        entities.ExistingDebtDetail.CreatedTmst = db.GetDateTime(reader, 10);
        entities.ExistingDebtDetail.Populated = true;
        CheckValid<DebtDetail>("CpaType", entities.ExistingDebtDetail.CpaType);
        CheckValid<DebtDetail>("OtrType", entities.ExistingDebtDetail.OtrType);

        return true;
      });
  }

  private IEnumerable<bool> ReadObligation()
  {
    entities.ExistingObligation.Populated = false;

    return ReadEach("ReadObligation",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", import.Obligor.Number);
        db.SetNullableString(
          command, "standardNo", import.FilterByStdNo.StandardNumber ?? "");
      },
      (db, reader) =>
      {
        entities.ExistingObligation.CpaType = db.GetString(reader, 0);
        entities.ExistingObligation.CspNumber = db.GetString(reader, 1);
        entities.ExistingObligation.SystemGeneratedIdentifier =
          db.GetInt32(reader, 2);
        entities.ExistingObligation.DtyGeneratedId = db.GetInt32(reader, 3);
        entities.ExistingObligation.LgaId = db.GetNullableInt32(reader, 4);
        entities.ExistingObligation.PrimarySecondaryCode =
          db.GetNullableString(reader, 5);
        entities.ExistingObligation.CreatedTmst = db.GetDateTime(reader, 6);
        entities.ExistingObligation.Populated = true;
        CheckValid<Obligation>("CpaType", entities.ExistingObligation.CpaType);
        CheckValid<Obligation>("PrimarySecondaryCode",
          entities.ExistingObligation.PrimarySecondaryCode);

        return true;
      });
  }

  private bool ReadObligationType()
  {
    System.Diagnostics.Debug.Assert(entities.ExistingObligation.Populated);
    entities.ExistingObligationType.Populated = false;

    return Read("ReadObligationType",
      (db, command) =>
      {
        db.SetInt32(
          command, "debtTypId", entities.ExistingObligation.DtyGeneratedId);
      },
      (db, reader) =>
      {
        entities.ExistingObligationType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.ExistingObligationType.Classification =
          db.GetString(reader, 1);
        entities.ExistingObligationType.Populated = true;
        CheckValid<ObligationType>("Classification",
          entities.ExistingObligationType.Classification);
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
    /// A value of IncludeArrearsOnly.
    /// </summary>
    [JsonPropertyName("includeArrearsOnly")]
    public Common IncludeArrearsOnly
    {
      get => includeArrearsOnly ??= new();
      set => includeArrearsOnly = value;
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
    /// A value of FilterByStdNo.
    /// </summary>
    [JsonPropertyName("filterByStdNo")]
    public LegalAction FilterByStdNo
    {
      get => filterByStdNo ??= new();
      set => filterByStdNo = value;
    }

    /// <summary>
    /// A value of StartDate.
    /// </summary>
    [JsonPropertyName("startDate")]
    public DateWorkArea StartDate
    {
      get => startDate ??= new();
      set => startDate = value;
    }

    private Common includeArrearsOnly;
    private CsePerson obligor;
    private LegalAction filterByStdNo;
    private DateWorkArea startDate;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of ScreenOwedAmountsDtl.
    /// </summary>
    [JsonPropertyName("screenOwedAmountsDtl")]
    public ScreenOwedAmountsDtl ScreenOwedAmountsDtl
    {
      get => screenOwedAmountsDtl ??= new();
      set => screenOwedAmountsDtl = value;
    }

    private ScreenOwedAmountsDtl screenOwedAmountsDtl;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of CurrentObligationFound.
    /// </summary>
    [JsonPropertyName("currentObligationFound")]
    public Common CurrentObligationFound
    {
      get => currentObligationFound ??= new();
      set => currentObligationFound = value;
    }

    /// <summary>
    /// A value of AmountOwed.
    /// </summary>
    [JsonPropertyName("amountOwed")]
    public Common AmountOwed
    {
      get => amountOwed ??= new();
      set => amountOwed = value;
    }

    /// <summary>
    /// A value of CurrentFirstOfTheMonth.
    /// </summary>
    [JsonPropertyName("currentFirstOfTheMonth")]
    public DateWorkArea CurrentFirstOfTheMonth
    {
      get => currentFirstOfTheMonth ??= new();
      set => currentFirstOfTheMonth = value;
    }

    /// <summary>
    /// A value of TestDiscountinedDate.
    /// </summary>
    [JsonPropertyName("testDiscountinedDate")]
    public DateWorkArea TestDiscountinedDate
    {
      get => testDiscountinedDate ??= new();
      set => testDiscountinedDate = value;
    }

    /// <summary>
    /// A value of CurrentMonthAccurals.
    /// </summary>
    [JsonPropertyName("currentMonthAccurals")]
    public DateWorkArea CurrentMonthAccurals
    {
      get => currentMonthAccurals ??= new();
      set => currentMonthAccurals = value;
    }

    /// <summary>
    /// A value of OneFutureMonthAccurals.
    /// </summary>
    [JsonPropertyName("oneFutureMonthAccurals")]
    public DateWorkArea OneFutureMonthAccurals
    {
      get => oneFutureMonthAccurals ??= new();
      set => oneFutureMonthAccurals = value;
    }

    /// <summary>
    /// A value of DateWorkArea.
    /// </summary>
    [JsonPropertyName("dateWorkArea")]
    public DateWorkArea DateWorkArea
    {
      get => dateWorkArea ??= new();
      set => dateWorkArea = value;
    }

    /// <summary>
    /// A value of Clear.
    /// </summary>
    [JsonPropertyName("clear")]
    public BatchTimestampWorkArea Clear
    {
      get => clear ??= new();
      set => clear = value;
    }

    /// <summary>
    /// A value of TestTime2.
    /// </summary>
    [JsonPropertyName("testTime2")]
    public BatchTimestampWorkArea TestTime2
    {
      get => testTime2 ??= new();
      set => testTime2 = value;
    }

    /// <summary>
    /// A value of TestObl.
    /// </summary>
    [JsonPropertyName("testObl")]
    public WorkArea TestObl
    {
      get => testObl ??= new();
      set => testObl = value;
    }

    /// <summary>
    /// A value of ConvertMoney.
    /// </summary>
    [JsonPropertyName("convertMoney")]
    public WorkArea ConvertMoney
    {
      get => convertMoney ??= new();
      set => convertMoney = value;
    }

    /// <summary>
    /// A value of TestTime.
    /// </summary>
    [JsonPropertyName("testTime")]
    public BatchTimestampWorkArea TestTime
    {
      get => testTime ??= new();
      set => testTime = value;
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
    /// A value of Process.
    /// </summary>
    [JsonPropertyName("process")]
    public DateWorkArea Process
    {
      get => process ??= new();
      set => process = value;
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

    /// <summary>
    /// A value of ProcessMonthBegin.
    /// </summary>
    [JsonPropertyName("processMonthBegin")]
    public DateWorkArea ProcessMonthBegin
    {
      get => processMonthBegin ??= new();
      set => processMonthBegin = value;
    }

    /// <summary>
    /// A value of ProcessMonthEnd.
    /// </summary>
    [JsonPropertyName("processMonthEnd")]
    public DateWorkArea ProcessMonthEnd
    {
      get => processMonthEnd ??= new();
      set => processMonthEnd = value;
    }

    /// <summary>
    /// A value of OmitSecondaryObligInd.
    /// </summary>
    [JsonPropertyName("omitSecondaryObligInd")]
    public Common OmitSecondaryObligInd
    {
      get => omitSecondaryObligInd ??= new();
      set => omitSecondaryObligInd = value;
    }

    /// <summary>
    /// A value of MaxDiscontinue.
    /// </summary>
    [JsonPropertyName("maxDiscontinue")]
    public DateWorkArea MaxDiscontinue
    {
      get => maxDiscontinue ??= new();
      set => maxDiscontinue = value;
    }

    /// <summary>
    /// A value of HardcodedVoluntary.
    /// </summary>
    [JsonPropertyName("hardcodedVoluntary")]
    public ObligationType HardcodedVoluntary
    {
      get => hardcodedVoluntary ??= new();
      set => hardcodedVoluntary = value;
    }

    /// <summary>
    /// A value of HardcodedMsType.
    /// </summary>
    [JsonPropertyName("hardcodedMsType")]
    public ObligationType HardcodedMsType
    {
      get => hardcodedMsType ??= new();
      set => hardcodedMsType = value;
    }

    /// <summary>
    /// A value of HardcodedMcType.
    /// </summary>
    [JsonPropertyName("hardcodedMcType")]
    public ObligationType HardcodedMcType
    {
      get => hardcodedMcType ??= new();
      set => hardcodedMcType = value;
    }

    /// <summary>
    /// A value of HardcodedCsType.
    /// </summary>
    [JsonPropertyName("hardcodedCsType")]
    public ObligationType HardcodedCsType
    {
      get => hardcodedCsType ??= new();
      set => hardcodedCsType = value;
    }

    /// <summary>
    /// A value of HardcodedSpType.
    /// </summary>
    [JsonPropertyName("hardcodedSpType")]
    public ObligationType HardcodedSpType
    {
      get => hardcodedSpType ??= new();
      set => hardcodedSpType = value;
    }

    /// <summary>
    /// A value of HardcodedAccruingClass.
    /// </summary>
    [JsonPropertyName("hardcodedAccruingClass")]
    public ObligationType HardcodedAccruingClass
    {
      get => hardcodedAccruingClass ??= new();
      set => hardcodedAccruingClass = value;
    }

    /// <summary>
    /// A value of HardcodedSecondary.
    /// </summary>
    [JsonPropertyName("hardcodedSecondary")]
    public Obligation HardcodedSecondary
    {
      get => hardcodedSecondary ??= new();
      set => hardcodedSecondary = value;
    }

    /// <summary>
    /// A value of HardcodedAdjusted.
    /// </summary>
    [JsonPropertyName("hardcodedAdjusted")]
    public CashReceiptDetailStatus HardcodedAdjusted
    {
      get => hardcodedAdjusted ??= new();
      set => hardcodedAdjusted = value;
    }

    /// <summary>
    /// A value of EabFileHandling.
    /// </summary>
    [JsonPropertyName("eabFileHandling")]
    public EabFileHandling EabFileHandling
    {
      get => eabFileHandling ??= new();
      set => eabFileHandling = value;
    }

    /// <summary>
    /// A value of EabReportSend.
    /// </summary>
    [JsonPropertyName("eabReportSend")]
    public EabReportSend EabReportSend
    {
      get => eabReportSend ??= new();
      set => eabReportSend = value;
    }

    private Common currentObligationFound;
    private Common amountOwed;
    private DateWorkArea currentFirstOfTheMonth;
    private DateWorkArea testDiscountinedDate;
    private DateWorkArea currentMonthAccurals;
    private DateWorkArea oneFutureMonthAccurals;
    private DateWorkArea dateWorkArea;
    private BatchTimestampWorkArea clear;
    private BatchTimestampWorkArea testTime2;
    private WorkArea testObl;
    private WorkArea convertMoney;
    private BatchTimestampWorkArea testTime;
    private CsePerson obligor;
    private DateWorkArea process;
    private DateWorkArea null1;
    private DateWorkArea processMonthBegin;
    private DateWorkArea processMonthEnd;
    private Common omitSecondaryObligInd;
    private DateWorkArea maxDiscontinue;
    private ObligationType hardcodedVoluntary;
    private ObligationType hardcodedMsType;
    private ObligationType hardcodedMcType;
    private ObligationType hardcodedCsType;
    private ObligationType hardcodedSpType;
    private ObligationType hardcodedAccruingClass;
    private Obligation hardcodedSecondary;
    private CashReceiptDetailStatus hardcodedAdjusted;
    private EabFileHandling eabFileHandling;
    private EabReportSend eabReportSend;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
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
    /// A value of CsePersonAccount.
    /// </summary>
    [JsonPropertyName("csePersonAccount")]
    public CsePersonAccount CsePersonAccount
    {
      get => csePersonAccount ??= new();
      set => csePersonAccount = value;
    }

    /// <summary>
    /// A value of ExistingDebt.
    /// </summary>
    [JsonPropertyName("existingDebt")]
    public ObligationTransaction ExistingDebt
    {
      get => existingDebt ??= new();
      set => existingDebt = value;
    }

    /// <summary>
    /// A value of ExistingLegalAction.
    /// </summary>
    [JsonPropertyName("existingLegalAction")]
    public LegalAction ExistingLegalAction
    {
      get => existingLegalAction ??= new();
      set => existingLegalAction = value;
    }

    /// <summary>
    /// A value of ExistingObligation.
    /// </summary>
    [JsonPropertyName("existingObligation")]
    public Obligation ExistingObligation
    {
      get => existingObligation ??= new();
      set => existingObligation = value;
    }

    /// <summary>
    /// A value of ExistingObligationType.
    /// </summary>
    [JsonPropertyName("existingObligationType")]
    public ObligationType ExistingObligationType
    {
      get => existingObligationType ??= new();
      set => existingObligationType = value;
    }

    /// <summary>
    /// A value of ExistingDebtDetail.
    /// </summary>
    [JsonPropertyName("existingDebtDetail")]
    public DebtDetail ExistingDebtDetail
    {
      get => existingDebtDetail ??= new();
      set => existingDebtDetail = value;
    }

    /// <summary>
    /// A value of ExistingKeyOnlyDebt.
    /// </summary>
    [JsonPropertyName("existingKeyOnlyDebt")]
    public ObligationTransaction ExistingKeyOnlyDebt
    {
      get => existingKeyOnlyDebt ??= new();
      set => existingKeyOnlyDebt = value;
    }

    /// <summary>
    /// A value of ExistingKeyOnlyObligor.
    /// </summary>
    [JsonPropertyName("existingKeyOnlyObligor")]
    public CsePersonAccount ExistingKeyOnlyObligor
    {
      get => existingKeyOnlyObligor ??= new();
      set => existingKeyOnlyObligor = value;
    }

    /// <summary>
    /// A value of ExistingObligor.
    /// </summary>
    [JsonPropertyName("existingObligor")]
    public CsePerson ExistingObligor
    {
      get => existingObligor ??= new();
      set => existingObligor = value;
    }

    private AccrualInstructions accrualInstructions;
    private CsePersonAccount csePersonAccount;
    private ObligationTransaction existingDebt;
    private LegalAction existingLegalAction;
    private Obligation existingObligation;
    private ObligationType existingObligationType;
    private DebtDetail existingDebtDetail;
    private ObligationTransaction existingKeyOnlyDebt;
    private CsePersonAccount existingKeyOnlyObligor;
    private CsePerson existingObligor;
  }
#endregion
}
