// Program: FN_GET_OBLIGATION_STATUS, ID: 371739588, model: 746.
// Short name: SWE00474
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
/// A program: FN_GET_OBLIGATION_STATUS.
/// </para>
/// <para>
/// RESP: Finance
/// This action block will get the status of an obligation.
/// </para>
/// </summary>
[Serializable]
public partial class FnGetObligationStatus: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_GET_OBLIGATION_STATUS program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnGetObligationStatus(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnGetObligationStatus.
  /// </summary>
  public FnGetObligationStatus(IContext context, Import import, Export export):
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
    // =================================================
    // 7/6/99 - bud adams  -  Eliminated persistent obligation view;
    //   no need for it, and it added overhead on calling cabs.
    //   Deleted fn_hardcoded_debt_distribution and imported the
    //   one value being used; imported current_date
    // =================================================
    // Oct 6, 1999, mbrown, pr# 75885: Added logic to set status to deactivated
    // if the accrual instructions discontinue date is greater than current 
    // date,
    // and there are no debts with balance greater than zero.
    // ******************************************************************
    // Aug 28 ,2000  , Madhu Kumar pr # 101600 :
    //    Changed the read of the accrual instruction to read each.
    // We can say an obligation is deactivated only if the accrual instruction 
    // is ineffective for all the supported persons.
    // ******************************************************************
    // *************mxk
    // *****************************************
    //    PR 133 368 :System allowing cads closure in error
    //   Obligations which start accruing in the future should
    //  also be considered as active obligations.
    // ***********************************************************
    if (import.ObligationType.SystemGeneratedIdentifier == 0)
    {
      ExitState = "FN0000_OBLIG_TYPE_NF";

      return;
    }

    local.Max.Date = UseCabSetMaximumDiscontinueDate();

    if (ReadObligationPaymentSchedule())
    {
      export.ObligationPaymentSchedule.
        Assign(entities.ObligationPaymentSchedule);
    }

    if (AsChar(import.ObligationType.Classification) == AsChar
      (import.HcOtCAccruing.Classification))
    {
      export.ScreenObligationStatus.ObligationStatus = "A";
      local.AccrualsFound.Text1 = "N";

      foreach(var item in ReadAccrualInstructions())
      {
        local.AccrualsFound.Text1 = "Y";

        // Oct 6, 1999, mbrown, pr# 75885: Added the following code to set the 
        // status
        // to deactivated if there is no amount owing, and accrual has run for 
        // the last
        // time (ie accrual instructions discontinue date could be at the end of
        // the
        // current month, but accrual has run already, and the debts generated 
        // by
        // accrual have been paid).
        if (!Lt(entities.AccrualInstructions.LastAccrualDt,
          entities.AccrualInstructions.DiscontinueDt))
        {
          if (ReadDebtDetail1())
          {
            export.ScreenObligationStatus.ObligationStatus = "A";

            goto Test;

            // --- Obligation Active ---
          }
          else
          {
            // : DEACTIVED OBLIGATION
            export.ScreenObligationStatus.ObligationStatus = "D";
          }
        }
        else
        {
          export.ScreenObligationStatus.ObligationStatus = "A";

          goto Test;

          // --- Obligation Active ---
        }
      }

      if (AsChar(local.AccrualsFound.Text1) == 'N')
      {
        // : INACTIVE OBLIGATION WITH A BALANCE
        export.ScreenObligationStatus.ObligationStatus = "I";

        // --- Check if there is any outstanding balance on this obligation. If 
        // not, obligation is deactivated ---
        if (!ReadDebtDetail1())
        {
          // : DEACTIVED OBLIGATION
          export.ScreenObligationStatus.ObligationStatus = "D";
        }

        return;
      }
    }
    else
    {
      export.ScreenObligationStatus.ObligationStatus = "A";

      if (AsChar(import.ObligationType.Classification) == 'V')
      {
        if (ReadDebtDetail2())
        {
          if (!Lt(import.Current.Date, entities.DebtDetail.CoveredPrdEndDt))
          {
            export.ScreenObligationStatus.ObligationStatus = "D";
          }
        }
        else
        {
          export.ScreenObligationStatus.ObligationStatus = "D";
        }
      }
      else if (ReadDebtDetail1())
      {
        // : ACTIVE OBLIGATION
        if (ReadDebtDetailStatusHistory())
        {
          export.ScreenObligationStatus.ObligationStatus =
            entities.DebtDetailStatusHistory.Code;
        }
        else
        {
          ExitState = "FN0222_DEBT_DETL_STAT_HIST_NF";

          return;
        }
      }
      else
      {
        // : DEACTIVED OBLIGATION
        export.ScreenObligationStatus.ObligationStatus = "D";
      }
    }

Test:

    if (AsChar(export.ScreenObligationStatus.ObligationStatus) == 'A')
    {
      export.ScreenObligationStatus.ObligationStatusTxt = "ACTIVE";
    }
    else
    {
      export.ScreenObligationStatus.ObligationStatusTxt = "DEACTIVED";
    }
  }

  private DateTime? UseCabSetMaximumDiscontinueDate()
  {
    var useImport = new CabSetMaximumDiscontinueDate.Import();
    var useExport = new CabSetMaximumDiscontinueDate.Export();

    Call(CabSetMaximumDiscontinueDate.Execute, useImport, useExport);

    return useExport.DateWorkArea.Date;
  }

  private IEnumerable<bool> ReadAccrualInstructions()
  {
    entities.AccrualInstructions.Populated = false;

    return ReadEach("ReadAccrualInstructions",
      (db, command) =>
      {
        db.SetInt32(
          command, "obgGeneratedId",
          import.Obligation.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "otyId", import.ObligationType.SystemGeneratedIdentifier);
        db.SetString(command, "cpaType", import.CsePersonAccount.Type1);
        db.SetString(command, "cspNumber", import.CsePerson.Number);
        db.SetNullableDate(
          command, "discontinueDt", import.Current.Date.GetValueOrDefault());
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

        return true;
      });
  }

  private bool ReadDebtDetail1()
  {
    entities.DebtDetail.Populated = false;

    return Read("ReadDebtDetail1",
      (db, command) =>
      {
        db.SetInt32(
          command, "obgGeneratedId",
          import.Obligation.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "otyType", import.ObligationType.SystemGeneratedIdentifier);
        db.SetString(command, "cpaType", import.CsePersonAccount.Type1);
        db.SetString(command, "cspNumber", import.CsePerson.Number);
        db.SetNullableDate(
          command, "retiredDt", local.Zero.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.DebtDetail.ObgGeneratedId = db.GetInt32(reader, 0);
        entities.DebtDetail.CspNumber = db.GetString(reader, 1);
        entities.DebtDetail.CpaType = db.GetString(reader, 2);
        entities.DebtDetail.OtrGeneratedId = db.GetInt32(reader, 3);
        entities.DebtDetail.OtyType = db.GetInt32(reader, 4);
        entities.DebtDetail.OtrType = db.GetString(reader, 5);
        entities.DebtDetail.BalanceDueAmt = db.GetDecimal(reader, 6);
        entities.DebtDetail.InterestBalanceDueAmt =
          db.GetNullableDecimal(reader, 7);
        entities.DebtDetail.RetiredDt = db.GetNullableDate(reader, 8);
        entities.DebtDetail.CoveredPrdEndDt = db.GetNullableDate(reader, 9);
        entities.DebtDetail.Populated = true;
        CheckValid<DebtDetail>("CpaType", entities.DebtDetail.CpaType);
        CheckValid<DebtDetail>("OtrType", entities.DebtDetail.OtrType);
      });
  }

  private bool ReadDebtDetail2()
  {
    entities.DebtDetail.Populated = false;

    return Read("ReadDebtDetail2",
      (db, command) =>
      {
        db.SetInt32(
          command, "obgGeneratedId",
          import.Obligation.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "otyType", import.ObligationType.SystemGeneratedIdentifier);
        db.SetString(command, "cpaType", import.CsePersonAccount.Type1);
        db.SetString(command, "cspNumber", import.CsePerson.Number);
      },
      (db, reader) =>
      {
        entities.DebtDetail.ObgGeneratedId = db.GetInt32(reader, 0);
        entities.DebtDetail.CspNumber = db.GetString(reader, 1);
        entities.DebtDetail.CpaType = db.GetString(reader, 2);
        entities.DebtDetail.OtrGeneratedId = db.GetInt32(reader, 3);
        entities.DebtDetail.OtyType = db.GetInt32(reader, 4);
        entities.DebtDetail.OtrType = db.GetString(reader, 5);
        entities.DebtDetail.BalanceDueAmt = db.GetDecimal(reader, 6);
        entities.DebtDetail.InterestBalanceDueAmt =
          db.GetNullableDecimal(reader, 7);
        entities.DebtDetail.RetiredDt = db.GetNullableDate(reader, 8);
        entities.DebtDetail.CoveredPrdEndDt = db.GetNullableDate(reader, 9);
        entities.DebtDetail.Populated = true;
        CheckValid<DebtDetail>("CpaType", entities.DebtDetail.CpaType);
        CheckValid<DebtDetail>("OtrType", entities.DebtDetail.OtrType);
      });
  }

  private bool ReadDebtDetailStatusHistory()
  {
    System.Diagnostics.Debug.Assert(entities.DebtDetail.Populated);
    entities.DebtDetailStatusHistory.Populated = false;

    return Read("ReadDebtDetailStatusHistory",
      (db, command) =>
      {
        db.SetInt32(command, "otyType", entities.DebtDetail.OtyType);
        db.SetInt32(command, "obgId", entities.DebtDetail.ObgGeneratedId);
        db.SetString(command, "cspNumber", entities.DebtDetail.CspNumber);
        db.SetString(command, "cpaType", entities.DebtDetail.CpaType);
        db.SetInt32(command, "otrId", entities.DebtDetail.OtrGeneratedId);
        db.SetString(command, "otrType", entities.DebtDetail.OtrType);
        db.SetNullableDate(
          command, "discontinueDt", local.Max.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.DebtDetailStatusHistory.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.DebtDetailStatusHistory.DiscontinueDt =
          db.GetNullableDate(reader, 1);
        entities.DebtDetailStatusHistory.OtrType = db.GetString(reader, 2);
        entities.DebtDetailStatusHistory.OtrId = db.GetInt32(reader, 3);
        entities.DebtDetailStatusHistory.CpaType = db.GetString(reader, 4);
        entities.DebtDetailStatusHistory.CspNumber = db.GetString(reader, 5);
        entities.DebtDetailStatusHistory.ObgId = db.GetInt32(reader, 6);
        entities.DebtDetailStatusHistory.Code = db.GetString(reader, 7);
        entities.DebtDetailStatusHistory.OtyType = db.GetInt32(reader, 8);
        entities.DebtDetailStatusHistory.Populated = true;
        CheckValid<DebtDetailStatusHistory>("OtrType",
          entities.DebtDetailStatusHistory.OtrType);
        CheckValid<DebtDetailStatusHistory>("CpaType",
          entities.DebtDetailStatusHistory.CpaType);
      });
  }

  private bool ReadObligationPaymentSchedule()
  {
    entities.ObligationPaymentSchedule.Populated = false;

    return Read("ReadObligationPaymentSchedule",
      (db, command) =>
      {
        db.SetInt32(
          command, "obgGeneratedId",
          import.Obligation.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "otyType", import.ObligationType.SystemGeneratedIdentifier);
        db.SetString(command, "obgCpaType", import.CsePersonAccount.Type1);
        db.SetString(command, "obgCspNumber", import.CsePerson.Number);
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
        entities.ObligationPaymentSchedule.Populated = true;
        CheckValid<ObligationPaymentSchedule>("ObgCpaType",
          entities.ObligationPaymentSchedule.ObgCpaType);
        CheckValid<ObligationPaymentSchedule>("FrequencyCode",
          entities.ObligationPaymentSchedule.FrequencyCode);
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
    /// A value of Current.
    /// </summary>
    [JsonPropertyName("current")]
    public DateWorkArea Current
    {
      get => current ??= new();
      set => current = value;
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
    /// A value of ZdelImportP.
    /// </summary>
    [JsonPropertyName("zdelImportP")]
    public Obligation ZdelImportP
    {
      get => zdelImportP ??= new();
      set => zdelImportP = value;
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
    /// A value of Obligation.
    /// </summary>
    [JsonPropertyName("obligation")]
    public Obligation Obligation
    {
      get => obligation ??= new();
      set => obligation = value;
    }

    /// <summary>
    /// A value of HcOtCAccruing.
    /// </summary>
    [JsonPropertyName("hcOtCAccruing")]
    public ObligationType HcOtCAccruing
    {
      get => hcOtCAccruing ??= new();
      set => hcOtCAccruing = value;
    }

    /// <summary>
    /// A value of HcOtCVoluntary.
    /// </summary>
    [JsonPropertyName("hcOtCVoluntary")]
    public ObligationType HcOtCVoluntary
    {
      get => hcOtCVoluntary ??= new();
      set => hcOtCVoluntary = value;
    }

    private DateWorkArea current;
    private ObligationType obligationType;
    private Obligation zdelImportP;
    private CsePersonAccount csePersonAccount;
    private CsePerson csePerson;
    private Obligation obligation;
    private ObligationType hcOtCAccruing;
    private ObligationType hcOtCVoluntary;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
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
    /// A value of ScreenObligationStatus.
    /// </summary>
    [JsonPropertyName("screenObligationStatus")]
    public ScreenObligationStatus ScreenObligationStatus
    {
      get => screenObligationStatus ??= new();
      set => screenObligationStatus = value;
    }

    private ObligationPaymentSchedule obligationPaymentSchedule;
    private ScreenObligationStatus screenObligationStatus;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of AccrualsFound.
    /// </summary>
    [JsonPropertyName("accrualsFound")]
    public TextWorkArea AccrualsFound
    {
      get => accrualsFound ??= new();
      set => accrualsFound = value;
    }

    /// <summary>
    /// A value of ZdelLocalCurrent.
    /// </summary>
    [JsonPropertyName("zdelLocalCurrent")]
    public DateWorkArea ZdelLocalCurrent
    {
      get => zdelLocalCurrent ??= new();
      set => zdelLocalCurrent = value;
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
    /// A value of ZdelLocalAccruingClassificatn.
    /// </summary>
    [JsonPropertyName("zdelLocalAccruingClassificatn")]
    public ObligationType ZdelLocalAccruingClassificatn
    {
      get => zdelLocalAccruingClassificatn ??= new();
      set => zdelLocalAccruingClassificatn = value;
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

    private TextWorkArea accrualsFound;
    private DateWorkArea zdelLocalCurrent;
    private DateWorkArea max;
    private ObligationType zdelLocalAccruingClassificatn;
    private DateWorkArea zero;
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
    /// A value of Obligation.
    /// </summary>
    [JsonPropertyName("obligation")]
    public Obligation Obligation
    {
      get => obligation ??= new();
      set => obligation = value;
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
    /// A value of DebtDetailStatusHistory.
    /// </summary>
    [JsonPropertyName("debtDetailStatusHistory")]
    public DebtDetailStatusHistory DebtDetailStatusHistory
    {
      get => debtDetailStatusHistory ??= new();
      set => debtDetailStatusHistory = value;
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
    /// A value of DebtDetail.
    /// </summary>
    [JsonPropertyName("debtDetail")]
    public DebtDetail DebtDetail
    {
      get => debtDetail ??= new();
      set => debtDetail = value;
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

    private ObligationType obligationType;
    private CsePersonAccount csePersonAccount;
    private CsePerson csePerson;
    private Obligation obligation;
    private AccrualInstructions accrualInstructions;
    private DebtDetailStatusHistory debtDetailStatusHistory;
    private ObligationPaymentSchedule obligationPaymentSchedule;
    private DebtDetail debtDetail;
    private ObligationTransaction obligationTransaction;
  }
#endregion
}
