// Program: FN_VERIFY_DUE_DATE, ID: 371969654, model: 746.
// Short name: SWE00686
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: FN_VERIFY_DUE_DATE.
/// </para>
/// <para>
/// RESP: FINANCE
/// This AB will verify a due against the starting and ending dates for an 
/// accrual instructions and the frequency suspensions.
/// </para>
/// </summary>
[Serializable]
public partial class FnVerifyDueDate: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_VERIFY_DUE_DATE program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnVerifyDueDate(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnVerifyDueDate.
  /// </summary>
  public FnVerifyDueDate(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // =================================================
    // 4/17/99 - bud adams  -  got rid of 'number-of-reads' counter
    //   and zdel the view.
    // 4/20/99 - bud adams  -  exported Accrual_Suspension
    //   Suspend_Dt; this is needed by B630 to appropriately set
    //   the value of Accrual_Instructions Last_Accrual_Date
    // 6/3/99 - bud adams  -  added Resume_Dt; SET valid-date-ind
    //   to S when totally suspended.
    // =================================================
    export.ProcessCurrent.Date = import.ProcessCurrent.Date;
    export.ValidDateInd.Flag = "N";

    if (!Lt(import.Due.Date, import.ProcessFrom.Date) && !
      Lt(import.ProcessThru.Date, import.Due.Date) && !
      Lt(import.Persistent.DiscontinueDt, import.Due.Date))
    {
      if (ReadAccrualSuspension())
      {
        export.AccrualSuspension.SuspendDt =
          entities.AccrualSuspension.SuspendDt;
        export.ValidDateInd.Flag = "S";

        if (Lt(0, entities.AccrualSuspension.ReductionAmount))
        {
          if (import.AccrualInstructions.Amount - entities
            .AccrualSuspension.ReductionAmount.GetValueOrDefault() > 0)
          {
            export.ValidDateInd.Flag = "Y";
            export.DebtDetail.DueDt = import.Due.Date;
            export.Debt.Amount = import.AccrualInstructions.Amount - entities
              .AccrualSuspension.ReductionAmount.GetValueOrDefault();
          }
        }
        else if (import.AccrualInstructions.Amount - import
          .AccrualInstructions.Amount * entities
          .AccrualSuspension.ReductionPercentage.GetValueOrDefault() * 0.01M > 0
          )
        {
          export.ValidDateInd.Flag = "Y";
          export.DebtDetail.DueDt = import.Due.Date;
          export.Debt.Amount = import.AccrualInstructions.Amount - import
            .AccrualInstructions.Amount * entities
            .AccrualSuspension.ReductionPercentage.GetValueOrDefault() * 0.01M;
        }

        if (AsChar(export.ValidDateInd.Flag) == 'S')
        {
          export.ProcessCurrent.Date = entities.AccrualSuspension.ResumeDt;
        }
      }
      else
      {
        export.ValidDateInd.Flag = "Y";
        export.DebtDetail.DueDt = import.Due.Date;
        export.Debt.Amount = import.AccrualInstructions.Amount;
      }
    }
  }

  private bool ReadAccrualSuspension()
  {
    System.Diagnostics.Debug.Assert(import.Persistent.Populated);
    entities.AccrualSuspension.Populated = false;

    return Read("ReadAccrualSuspension",
      (db, command) =>
      {
        db.SetInt32(command, "otrId", import.Persistent.OtrGeneratedId);
        db.SetString(command, "cpaType", import.Persistent.CpaType);
        db.SetString(command, "cspNumber", import.Persistent.CspNumber);
        db.SetInt32(command, "obgId", import.Persistent.ObgGeneratedId);
        db.SetInt32(command, "otyId", import.Persistent.OtyId);
        db.SetString(command, "otrType", import.Persistent.OtrType);
        db.SetDate(command, "suspendDt", import.Due.Date.GetValueOrDefault());
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
        entities.AccrualSuspension.ReductionAmount =
          db.GetNullableDecimal(reader, 10);
        entities.AccrualSuspension.Populated = true;
        CheckValid<AccrualSuspension>("OtrType",
          entities.AccrualSuspension.OtrType);
        CheckValid<AccrualSuspension>("CpaType",
          entities.AccrualSuspension.CpaType);
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
    /// A value of Persistent.
    /// </summary>
    [JsonPropertyName("persistent")]
    public AccrualInstructions Persistent
    {
      get => persistent ??= new();
      set => persistent = value;
    }

    /// <summary>
    /// A value of AccrualInstructions.
    /// </summary>
    [JsonPropertyName("accrualInstructions")]
    public ObligationTransaction AccrualInstructions
    {
      get => accrualInstructions ??= new();
      set => accrualInstructions = value;
    }

    /// <summary>
    /// A value of Due.
    /// </summary>
    [JsonPropertyName("due")]
    public DateWorkArea Due
    {
      get => due ??= new();
      set => due = value;
    }

    /// <summary>
    /// A value of ProcessFrom.
    /// </summary>
    [JsonPropertyName("processFrom")]
    public DateWorkArea ProcessFrom
    {
      get => processFrom ??= new();
      set => processFrom = value;
    }

    /// <summary>
    /// A value of ProcessThru.
    /// </summary>
    [JsonPropertyName("processThru")]
    public DateWorkArea ProcessThru
    {
      get => processThru ??= new();
      set => processThru = value;
    }

    /// <summary>
    /// A value of XxxObligationTransaction.
    /// </summary>
    [JsonPropertyName("xxxObligationTransaction")]
    public ObligationTransaction XxxObligationTransaction
    {
      get => xxxObligationTransaction ??= new();
      set => xxxObligationTransaction = value;
    }

    /// <summary>
    /// A value of XxxAccrualWorkArea.
    /// </summary>
    [JsonPropertyName("xxxAccrualWorkArea")]
    public AccrualWorkArea XxxAccrualWorkArea
    {
      get => xxxAccrualWorkArea ??= new();
      set => xxxAccrualWorkArea = value;
    }

    /// <summary>
    /// A value of ProcessCurrent.
    /// </summary>
    [JsonPropertyName("processCurrent")]
    public DateWorkArea ProcessCurrent
    {
      get => processCurrent ??= new();
      set => processCurrent = value;
    }

    private AccrualInstructions persistent;
    private ObligationTransaction accrualInstructions;
    private DateWorkArea due;
    private DateWorkArea processFrom;
    private DateWorkArea processThru;
    private ObligationTransaction xxxObligationTransaction;
    private AccrualWorkArea xxxAccrualWorkArea;
    private DateWorkArea processCurrent;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
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
    /// A value of ValidDateInd.
    /// </summary>
    [JsonPropertyName("validDateInd")]
    public Common ValidDateInd
    {
      get => validDateInd ??= new();
      set => validDateInd = value;
    }

    /// <summary>
    /// A value of ZdelExpImpNumberOfReads.
    /// </summary>
    [JsonPropertyName("zdelExpImpNumberOfReads")]
    public Common ZdelExpImpNumberOfReads
    {
      get => zdelExpImpNumberOfReads ??= new();
      set => zdelExpImpNumberOfReads = value;
    }

    /// <summary>
    /// A value of Xxx.
    /// </summary>
    [JsonPropertyName("xxx")]
    public ObligationTransaction Xxx
    {
      get => xxx ??= new();
      set => xxx = value;
    }

    /// <summary>
    /// A value of ProcessCurrent.
    /// </summary>
    [JsonPropertyName("processCurrent")]
    public DateWorkArea ProcessCurrent
    {
      get => processCurrent ??= new();
      set => processCurrent = value;
    }

    private AccrualSuspension accrualSuspension;
    private ObligationTransaction debt;
    private DebtDetail debtDetail;
    private Common validDateInd;
    private Common zdelExpImpNumberOfReads;
    private ObligationTransaction xxx;
    private DateWorkArea processCurrent;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of XxxVerifyDate.
    /// </summary>
    [JsonPropertyName("xxxVerifyDate")]
    public DateWorkArea XxxVerifyDate
    {
      get => xxxVerifyDate ??= new();
      set => xxxVerifyDate = value;
    }

    private DateWorkArea xxxVerifyDate;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of AccrualSuspension.
    /// </summary>
    [JsonPropertyName("accrualSuspension")]
    public AccrualSuspension AccrualSuspension
    {
      get => accrualSuspension ??= new();
      set => accrualSuspension = value;
    }

    private AccrualSuspension accrualSuspension;
  }
#endregion
}
