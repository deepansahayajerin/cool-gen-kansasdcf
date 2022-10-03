// Program: LE_LDET_CALC_OBLIG_TRN_AMOUNT, ID: 371993426, model: 746.
// Short name: SWE01930
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: LE_LDET_CALC_OBLIG_TRN_AMOUNT.
/// </summary>
[Serializable]
public partial class LeLdetCalcObligTrnAmount: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the LE_LDET_CALC_OBLIG_TRN_AMOUNT program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new LeLdetCalcObligTrnAmount(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of LeLdetCalcObligTrnAmount.
  /// </summary>
  public LeLdetCalcObligTrnAmount(IContext context, Import import, Export export)
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
    // ---------------------------------------------
    // Date	By	IDCR#	Description
    // ??????	Alan Samuels	Initial code
    // 110197	govind		Rewritten to execute only once per legal detail instead of
    // 3 times as it was originally written (once for Current, once for Arrears
    // and once for Judgement amount based on import_amount_indicator).
    // 			Also fixed to look at Accrual Instructions for discontinued accruing 
    // debt.
    // 02/24/98	R Grey	Priority # 19: Add views and code to identify and count 
    // accrual instr discontinued and debt detail retired.
    // 01/07/99 P Sharp Added max date
    // ---------------------------------------------
    // ---------------------------------------------
    // This action block returns the total 'active' obligation transaction 
    // amounts for the given Legal Detail.
    // ---------------------------------------------
    local.Current.Date = Now().Date;
    export.CountAccruingObgDiscon.Count = 0;
    export.CountOnacObgRetired.Count = 0;
    local.MaxDate.Date = UseCabSetMaximumDiscontinueDate();

    if (ReadLegalActionDetail())
    {
      if (!ReadObligationType())
      {
        ExitState = "OBLIGATION_TYPE_NF";

        return;
      }
    }
    else
    {
      ExitState = "LEGAL_ACTION_DETAIL_NF";

      return;
    }

    if (AsChar(entities.ObligationType.Classification) == 'A')
    {
      // .............................................
      // Accruing obligation
      // .............................................
      foreach(var item in ReadObligationTransaction1())
      {
        switch(TrimEnd(entities.ObligationTransaction.Type1))
        {
          case "DA":
            // --- It is a debt adjustment
            if (AsChar(entities.ObligationTransaction.DebtAdjustmentType) == 'I'
              )
            {
              export.NetCurrent.TotalCurrency += entities.ObligationTransaction.
                Amount;
            }
            else
            {
              export.NetCurrent.TotalCurrency -= entities.ObligationTransaction.
                Amount;
            }

            break;
          case "DE":
            // --- It is the template debt record
            if (ReadObligationPaymentSchedule())
            {
              if (ReadAccrualInstructions())
              {
                export.NetCurrent.TotalCurrency += entities.
                  ObligationTransaction.Amount;
              }
              else
              {
                // *********************************************
                // If LOPS person Accrual Instructions discontinued add 1 to 
                // count.
                // RCG 02/17/98
                // *********************************************
                ++export.CountAccruingObgDiscon.Count;

                // --- Possibly expired.
              }
            }
            else
            {
              // --- Possibly expired.
            }

            break;
          default:
            break;
        }
      }
    }
    else
    {
      // --- It is not an accruing obligation. So compute only judgement amount
      foreach(var item in ReadObligationTransaction2())
      {
        switch(TrimEnd(entities.ObligationTransaction.Type1))
        {
          case "DA":
            // --- It is a debt adjustment
            if (AsChar(entities.ObligationTransaction.DebtAdjustmentType) == 'I'
              )
            {
              export.NetJudgement.TotalCurrency += entities.
                ObligationTransaction.Amount;
            }
            else
            {
              export.NetJudgement.TotalCurrency -= entities.
                ObligationTransaction.Amount;
            }

            if (ReadDebtDetail())
            {
              // *********************************************
              // If LOPS person NOAC debt detail retired, add 1 to count
              // RCG 02/25/98
              // *********************************************
              ++export.CountOnacObgRetired.Count;
            }

            break;
          case "DE":
            // --- It is the original debt record
            export.NetJudgement.TotalCurrency += entities.ObligationTransaction.
              Amount;

            if (ReadDebtDetail())
            {
              // *********************************************
              // If LOPS person NOAC debt detail retired, add 1 to count
              // RCG 02/25/98
              // *********************************************
              ++export.CountOnacObgRetired.Count;
            }

            break;
          default:
            break;
        }
      }
    }
  }

  private DateTime? UseCabSetMaximumDiscontinueDate()
  {
    var useImport = new CabSetMaximumDiscontinueDate.Import();
    var useExport = new CabSetMaximumDiscontinueDate.Export();

    Call(CabSetMaximumDiscontinueDate.Execute, useImport, useExport);

    return useExport.DateWorkArea.Date;
  }

  private bool ReadAccrualInstructions()
  {
    System.Diagnostics.Debug.Assert(entities.ObligationTransaction.Populated);
    entities.AccrualInstructions.Populated = false;

    return Read("ReadAccrualInstructions",
      (db, command) =>
      {
        db.SetString(command, "otrType", entities.ObligationTransaction.Type1);
        db.SetInt32(command, "otyId", entities.ObligationTransaction.OtyType);
        db.SetInt32(
          command, "otrGeneratedId",
          entities.ObligationTransaction.SystemGeneratedIdentifier);
        db.
          SetString(command, "cpaType", entities.ObligationTransaction.CpaType);
          
        db.SetString(
          command, "cspNumber", entities.ObligationTransaction.CspNumber);
        db.SetInt32(
          command, "obgGeneratedId",
          entities.ObligationTransaction.ObgGeneratedId);
        db.SetNullableDate(
          command, "discontinueDt", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.AccrualInstructions.OtrType = db.GetString(reader, 0);
        entities.AccrualInstructions.OtyId = db.GetInt32(reader, 1);
        entities.AccrualInstructions.ObgGeneratedId = db.GetInt32(reader, 2);
        entities.AccrualInstructions.CspNumber = db.GetString(reader, 3);
        entities.AccrualInstructions.CpaType = db.GetString(reader, 4);
        entities.AccrualInstructions.OtrGeneratedId = db.GetInt32(reader, 5);
        entities.AccrualInstructions.DiscontinueDt =
          db.GetNullableDate(reader, 6);
        entities.AccrualInstructions.Populated = true;
        CheckValid<AccrualInstructions>("OtrType",
          entities.AccrualInstructions.OtrType);
        CheckValid<AccrualInstructions>("CpaType",
          entities.AccrualInstructions.CpaType);
      });
  }

  private bool ReadDebtDetail()
  {
    System.Diagnostics.Debug.Assert(entities.ObligationTransaction.Populated);
    entities.DebtDetail.Populated = false;

    return Read("ReadDebtDetail",
      (db, command) =>
      {
        db.SetInt32(command, "otyType", entities.ObligationTransaction.OtyType);
        db.SetInt32(
          command, "obgGeneratedId",
          entities.ObligationTransaction.ObgGeneratedId);
        db.SetString(command, "otrType", entities.ObligationTransaction.Type1);
        db.SetInt32(
          command, "otrGeneratedId",
          entities.ObligationTransaction.SystemGeneratedIdentifier);
        db.
          SetString(command, "cpaType", entities.ObligationTransaction.CpaType);
          
        db.SetString(
          command, "cspNumber", entities.ObligationTransaction.CspNumber);
        db.SetNullableDate(
          command, "retiredDt1", local.MaxDate.Date.GetValueOrDefault());
        db.SetNullableDate(
          command, "retiredDt2", local.ZeroDate.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.DebtDetail.ObgGeneratedId = db.GetInt32(reader, 0);
        entities.DebtDetail.CspNumber = db.GetString(reader, 1);
        entities.DebtDetail.CpaType = db.GetString(reader, 2);
        entities.DebtDetail.OtrGeneratedId = db.GetInt32(reader, 3);
        entities.DebtDetail.OtyType = db.GetInt32(reader, 4);
        entities.DebtDetail.OtrType = db.GetString(reader, 5);
        entities.DebtDetail.RetiredDt = db.GetNullableDate(reader, 6);
        entities.DebtDetail.Populated = true;
        CheckValid<DebtDetail>("CpaType", entities.DebtDetail.CpaType);
        CheckValid<DebtDetail>("OtrType", entities.DebtDetail.OtrType);
      });
  }

  private bool ReadLegalActionDetail()
  {
    entities.LegalActionDetail.Populated = false;

    return Read("ReadLegalActionDetail",
      (db, command) =>
      {
        db.SetInt32(command, "laDetailNo", import.LegalActionDetail.Number);
        db.SetInt32(command, "lgaIdentifier", import.LegalAction.Identifier);
      },
      (db, reader) =>
      {
        entities.LegalActionDetail.LgaIdentifier = db.GetInt32(reader, 0);
        entities.LegalActionDetail.Number = db.GetInt32(reader, 1);
        entities.LegalActionDetail.OtyId = db.GetNullableInt32(reader, 2);
        entities.LegalActionDetail.Populated = true;
      });
  }

  private bool ReadObligationPaymentSchedule()
  {
    System.Diagnostics.Debug.Assert(entities.ObligationTransaction.Populated);
    entities.ObligationPaymentSchedule.Populated = false;

    return Read("ReadObligationPaymentSchedule",
      (db, command) =>
      {
        db.SetInt32(command, "otyType", entities.ObligationTransaction.OtyType);
        db.SetInt32(
          command, "obgGeneratedId",
          entities.ObligationTransaction.ObgGeneratedId);
        db.SetString(
          command, "obgCspNumber", entities.ObligationTransaction.CspNumber);
        db.SetString(
          command, "obgCpaType", entities.ObligationTransaction.CpaType);
        db.SetNullableDate(
          command, "endDt", local.Current.Date.GetValueOrDefault());
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
        entities.ObligationPaymentSchedule.Populated = true;
        CheckValid<ObligationPaymentSchedule>("ObgCpaType",
          entities.ObligationPaymentSchedule.ObgCpaType);
      });
  }

  private IEnumerable<bool> ReadObligationTransaction1()
  {
    System.Diagnostics.Debug.Assert(entities.LegalActionDetail.Populated);
    entities.ObligationTransaction.Populated = false;

    return ReadEach("ReadObligationTransaction1",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "ladRNumber", entities.LegalActionDetail.Number);
        db.SetNullableInt32(
          command, "lgaRIdentifier", entities.LegalActionDetail.LgaIdentifier);
      },
      (db, reader) =>
      {
        entities.ObligationTransaction.ObgGeneratedId = db.GetInt32(reader, 0);
        entities.ObligationTransaction.CspNumber = db.GetString(reader, 1);
        entities.ObligationTransaction.CpaType = db.GetString(reader, 2);
        entities.ObligationTransaction.SystemGeneratedIdentifier =
          db.GetInt32(reader, 3);
        entities.ObligationTransaction.Type1 = db.GetString(reader, 4);
        entities.ObligationTransaction.Amount = db.GetDecimal(reader, 5);
        entities.ObligationTransaction.DebtAdjustmentType =
          db.GetString(reader, 6);
        entities.ObligationTransaction.DebtType = db.GetString(reader, 7);
        entities.ObligationTransaction.CspSupNumber =
          db.GetNullableString(reader, 8);
        entities.ObligationTransaction.CpaSupType =
          db.GetNullableString(reader, 9);
        entities.ObligationTransaction.OtyType = db.GetInt32(reader, 10);
        entities.ObligationTransaction.LapId = db.GetNullableInt32(reader, 11);
        entities.ObligationTransaction.Populated = true;
        CheckValid<ObligationTransaction>("CpaType",
          entities.ObligationTransaction.CpaType);
        CheckValid<ObligationTransaction>("Type1",
          entities.ObligationTransaction.Type1);
        CheckValid<ObligationTransaction>("DebtAdjustmentType",
          entities.ObligationTransaction.DebtAdjustmentType);
        CheckValid<ObligationTransaction>("DebtType",
          entities.ObligationTransaction.DebtType);
        CheckValid<ObligationTransaction>("CpaSupType",
          entities.ObligationTransaction.CpaSupType);

        return true;
      });
  }

  private IEnumerable<bool> ReadObligationTransaction2()
  {
    System.Diagnostics.Debug.Assert(entities.LegalActionDetail.Populated);
    entities.ObligationTransaction.Populated = false;

    return ReadEach("ReadObligationTransaction2",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "ladRNumber", entities.LegalActionDetail.Number);
        db.SetNullableInt32(
          command, "lgaRIdentifier", entities.LegalActionDetail.LgaIdentifier);
      },
      (db, reader) =>
      {
        entities.ObligationTransaction.ObgGeneratedId = db.GetInt32(reader, 0);
        entities.ObligationTransaction.CspNumber = db.GetString(reader, 1);
        entities.ObligationTransaction.CpaType = db.GetString(reader, 2);
        entities.ObligationTransaction.SystemGeneratedIdentifier =
          db.GetInt32(reader, 3);
        entities.ObligationTransaction.Type1 = db.GetString(reader, 4);
        entities.ObligationTransaction.Amount = db.GetDecimal(reader, 5);
        entities.ObligationTransaction.DebtAdjustmentType =
          db.GetString(reader, 6);
        entities.ObligationTransaction.DebtType = db.GetString(reader, 7);
        entities.ObligationTransaction.CspSupNumber =
          db.GetNullableString(reader, 8);
        entities.ObligationTransaction.CpaSupType =
          db.GetNullableString(reader, 9);
        entities.ObligationTransaction.OtyType = db.GetInt32(reader, 10);
        entities.ObligationTransaction.LapId = db.GetNullableInt32(reader, 11);
        entities.ObligationTransaction.Populated = true;
        CheckValid<ObligationTransaction>("CpaType",
          entities.ObligationTransaction.CpaType);
        CheckValid<ObligationTransaction>("Type1",
          entities.ObligationTransaction.Type1);
        CheckValid<ObligationTransaction>("DebtAdjustmentType",
          entities.ObligationTransaction.DebtAdjustmentType);
        CheckValid<ObligationTransaction>("DebtType",
          entities.ObligationTransaction.DebtType);
        CheckValid<ObligationTransaction>("CpaSupType",
          entities.ObligationTransaction.CpaSupType);

        return true;
      });
  }

  private bool ReadObligationType()
  {
    System.Diagnostics.Debug.Assert(entities.LegalActionDetail.Populated);
    entities.ObligationType.Populated = false;

    return Read("ReadObligationType",
      (db, command) =>
      {
        db.SetInt32(
          command, "debtTypId",
          entities.LegalActionDetail.OtyId.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ObligationType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.ObligationType.Code = db.GetString(reader, 1);
        entities.ObligationType.Classification = db.GetString(reader, 2);
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
    /// A value of LegalAction.
    /// </summary>
    [JsonPropertyName("legalAction")]
    public LegalAction LegalAction
    {
      get => legalAction ??= new();
      set => legalAction = value;
    }

    /// <summary>
    /// A value of LegalActionDetail.
    /// </summary>
    [JsonPropertyName("legalActionDetail")]
    public LegalActionDetail LegalActionDetail
    {
      get => legalActionDetail ??= new();
      set => legalActionDetail = value;
    }

    private LegalAction legalAction;
    private LegalActionDetail legalActionDetail;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of CountOnacObgRetired.
    /// </summary>
    [JsonPropertyName("countOnacObgRetired")]
    public Common CountOnacObgRetired
    {
      get => countOnacObgRetired ??= new();
      set => countOnacObgRetired = value;
    }

    /// <summary>
    /// A value of CountAccruingObgDiscon.
    /// </summary>
    [JsonPropertyName("countAccruingObgDiscon")]
    public Common CountAccruingObgDiscon
    {
      get => countAccruingObgDiscon ??= new();
      set => countAccruingObgDiscon = value;
    }

    /// <summary>
    /// A value of NetJudgement.
    /// </summary>
    [JsonPropertyName("netJudgement")]
    public Common NetJudgement
    {
      get => netJudgement ??= new();
      set => netJudgement = value;
    }

    /// <summary>
    /// A value of NetCurrent.
    /// </summary>
    [JsonPropertyName("netCurrent")]
    public Common NetCurrent
    {
      get => netCurrent ??= new();
      set => netCurrent = value;
    }

    private Common countOnacObgRetired;
    private Common countAccruingObgDiscon;
    private Common netJudgement;
    private Common netCurrent;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
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
    /// A value of ReturnObligActive.
    /// </summary>
    [JsonPropertyName("returnObligActive")]
    public Common ReturnObligActive
    {
      get => returnObligActive ??= new();
      set => returnObligActive = value;
    }

    /// <summary>
    /// A value of MaxDate.
    /// </summary>
    [JsonPropertyName("maxDate")]
    public DateWorkArea MaxDate
    {
      get => maxDate ??= new();
      set => maxDate = value;
    }

    /// <summary>
    /// A value of ZeroDate.
    /// </summary>
    [JsonPropertyName("zeroDate")]
    public DateWorkArea ZeroDate
    {
      get => zeroDate ??= new();
      set => zeroDate = value;
    }

    private DateWorkArea current;
    private Common returnObligActive;
    private DateWorkArea maxDate;
    private DateWorkArea zeroDate;
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
    /// A value of ObligationPaymentSchedule.
    /// </summary>
    [JsonPropertyName("obligationPaymentSchedule")]
    public ObligationPaymentSchedule ObligationPaymentSchedule
    {
      get => obligationPaymentSchedule ??= new();
      set => obligationPaymentSchedule = value;
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
    /// A value of ObligationTransaction.
    /// </summary>
    [JsonPropertyName("obligationTransaction")]
    public ObligationTransaction ObligationTransaction
    {
      get => obligationTransaction ??= new();
      set => obligationTransaction = value;
    }

    /// <summary>
    /// A value of LegalActionPerson.
    /// </summary>
    [JsonPropertyName("legalActionPerson")]
    public LegalActionPerson LegalActionPerson
    {
      get => legalActionPerson ??= new();
      set => legalActionPerson = value;
    }

    /// <summary>
    /// A value of LegalActionDetail.
    /// </summary>
    [JsonPropertyName("legalActionDetail")]
    public LegalActionDetail LegalActionDetail
    {
      get => legalActionDetail ??= new();
      set => legalActionDetail = value;
    }

    /// <summary>
    /// A value of LegalAction.
    /// </summary>
    [JsonPropertyName("legalAction")]
    public LegalAction LegalAction
    {
      get => legalAction ??= new();
      set => legalAction = value;
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

    private AccrualInstructions accrualInstructions;
    private ObligationPaymentSchedule obligationPaymentSchedule;
    private Obligation obligation;
    private ObligationType obligationType;
    private ObligationTransaction obligationTransaction;
    private LegalActionPerson legalActionPerson;
    private LegalActionDetail legalActionDetail;
    private LegalAction legalAction;
    private DebtDetail debtDetail;
  }
#endregion
}
