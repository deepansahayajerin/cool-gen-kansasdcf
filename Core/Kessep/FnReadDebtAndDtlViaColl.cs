// Program: FN_READ_DEBT_AND_DTL_VIA_COLL, ID: 372258433, model: 746.
// Short name: SWE00551
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: FN_READ_DEBT_AND_DTL_VIA_COLL.
/// </para>
/// <para>
/// RESP:  FINANCE
/// DESC:  This action block reads the Debt and the Debt Detail for a given 
/// collection.  The persistent collection allows the debt information to be
/// retrieved for the current collection without explicitly knowing all of the
/// associated obligation identifiers.
/// </para>
/// </summary>
[Serializable]
public partial class FnReadDebtAndDtlViaColl: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_READ_DEBT_AND_DTL_VIA_COLL program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnReadDebtAndDtlViaColl(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnReadDebtAndDtlViaColl.
  /// </summary>
  public FnReadDebtAndDtlViaColl(IContext context, Import import, Export export):
    
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
    // 4/7/99 - bud adams  -  Set Read properties; deleted statements
    //   that counted the number of reads(?)
    // =================================================
    // *****  Retrieve the maximum discontinue date.  This date is used to set 
    // the discontinue date of any currently active entity occurrences.  This
    // date is used instead of a blank discontinue date.
    local.MaximumDiscontinue.Date = UseCabSetMaximumDiscontinueDate();

    if (ReadDebt())
    {
      MoveObligationTransaction(entities.Debt, export.Debt);

      if (ReadDebtDetail())
      {
        MoveDebtDetail(entities.DebtDetail, export.DebtDetail);

        if (ReadDebtDetailStatusHistory())
        {
          MoveDebtDetailStatusHistory(entities.DebtDetailStatusHistory,
            export.DebtDetailStatusHistory);
        }
        else
        {
          ExitState = "FN0222_DEBT_DETL_STAT_HIST_NF";
        }
      }
      else
      {
        ExitState = "FN0211_DEBT_DETAIL_NF";
      }
    }
    else
    {
      ExitState = "FN0229_DEBT_NF";
    }
  }

  private static void MoveDebtDetail(DebtDetail source, DebtDetail target)
  {
    target.DueDt = source.DueDt;
    target.BalanceDueAmt = source.BalanceDueAmt;
    target.InterestBalanceDueAmt = source.InterestBalanceDueAmt;
    target.AdcDt = source.AdcDt;
    target.RetiredDt = source.RetiredDt;
  }

  private static void MoveDebtDetailStatusHistory(
    DebtDetailStatusHistory source, DebtDetailStatusHistory target)
  {
    target.Code = source.Code;
    target.EffectiveDt = source.EffectiveDt;
    target.DiscontinueDt = source.DiscontinueDt;
    target.ReasonTxt = source.ReasonTxt;
  }

  private static void MoveObligationTransaction(ObligationTransaction source,
    ObligationTransaction target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Type1 = source.Type1;
    target.Amount = source.Amount;
  }

  private DateTime? UseCabSetMaximumDiscontinueDate()
  {
    var useImport = new CabSetMaximumDiscontinueDate.Import();
    var useExport = new CabSetMaximumDiscontinueDate.Export();

    Call(CabSetMaximumDiscontinueDate.Execute, useImport, useExport);

    return useExport.DateWorkArea.Date;
  }

  private bool ReadDebt()
  {
    System.Diagnostics.Debug.Assert(import.Persistent.Populated);
    entities.Debt.Populated = false;

    return Read("ReadDebt",
      (db, command) =>
      {
        db.SetInt32(command, "otyType", import.Persistent.OtyId);
        db.SetString(command, "obTrnTyp", import.Persistent.OtrType);
        db.SetInt32(command, "obTrnId", import.Persistent.OtrId);
        db.SetString(command, "cpaType", import.Persistent.CpaType);
        db.SetString(command, "cspNumber", import.Persistent.CspNumber);
        db.SetInt32(command, "obgGeneratedId", import.Persistent.ObgId);
      },
      (db, reader) =>
      {
        entities.Debt.ObgGeneratedId = db.GetInt32(reader, 0);
        entities.Debt.CspNumber = db.GetString(reader, 1);
        entities.Debt.CpaType = db.GetString(reader, 2);
        entities.Debt.SystemGeneratedIdentifier = db.GetInt32(reader, 3);
        entities.Debt.Type1 = db.GetString(reader, 4);
        entities.Debt.Amount = db.GetDecimal(reader, 5);
        entities.Debt.CspSupNumber = db.GetNullableString(reader, 6);
        entities.Debt.CpaSupType = db.GetNullableString(reader, 7);
        entities.Debt.OtyType = db.GetInt32(reader, 8);
        entities.Debt.Populated = true;
        CheckValid<ObligationTransaction>("CpaType", entities.Debt.CpaType);
        CheckValid<ObligationTransaction>("Type1", entities.Debt.Type1);
        CheckValid<ObligationTransaction>("CpaSupType", entities.Debt.CpaSupType);
          
      });
  }

  private bool ReadDebtDetail()
  {
    System.Diagnostics.Debug.Assert(entities.Debt.Populated);
    entities.DebtDetail.Populated = false;

    return Read("ReadDebtDetail",
      (db, command) =>
      {
        db.SetInt32(command, "otyType", entities.Debt.OtyType);
        db.SetInt32(command, "obgGeneratedId", entities.Debt.ObgGeneratedId);
        db.SetString(command, "otrType", entities.Debt.Type1);
        db.SetInt32(
          command, "otrGeneratedId", entities.Debt.SystemGeneratedIdentifier);
        db.SetString(command, "cpaType", entities.Debt.CpaType);
        db.SetString(command, "cspNumber", entities.Debt.CspNumber);
      },
      (db, reader) =>
      {
        entities.DebtDetail.ObgGeneratedId = db.GetInt32(reader, 0);
        entities.DebtDetail.CspNumber = db.GetString(reader, 1);
        entities.DebtDetail.CpaType = db.GetString(reader, 2);
        entities.DebtDetail.OtrGeneratedId = db.GetInt32(reader, 3);
        entities.DebtDetail.OtyType = db.GetInt32(reader, 4);
        entities.DebtDetail.OtrType = db.GetString(reader, 5);
        entities.DebtDetail.DueDt = db.GetDate(reader, 6);
        entities.DebtDetail.BalanceDueAmt = db.GetDecimal(reader, 7);
        entities.DebtDetail.InterestBalanceDueAmt =
          db.GetNullableDecimal(reader, 8);
        entities.DebtDetail.AdcDt = db.GetNullableDate(reader, 9);
        entities.DebtDetail.RetiredDt = db.GetNullableDate(reader, 10);
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
        db.SetNullableDate(
          command, "discontinueDt",
          local.MaximumDiscontinue.Date.GetValueOrDefault());
        db.SetInt32(command, "otyType", entities.DebtDetail.OtyType);
        db.SetInt32(command, "obgId", entities.DebtDetail.ObgGeneratedId);
        db.SetString(command, "cspNumber", entities.DebtDetail.CspNumber);
        db.SetString(command, "cpaType", entities.DebtDetail.CpaType);
        db.SetInt32(command, "otrId", entities.DebtDetail.OtrGeneratedId);
        db.SetString(command, "otrType", entities.DebtDetail.OtrType);
      },
      (db, reader) =>
      {
        entities.DebtDetailStatusHistory.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.DebtDetailStatusHistory.EffectiveDt = db.GetDate(reader, 1);
        entities.DebtDetailStatusHistory.DiscontinueDt =
          db.GetNullableDate(reader, 2);
        entities.DebtDetailStatusHistory.OtrType = db.GetString(reader, 3);
        entities.DebtDetailStatusHistory.OtrId = db.GetInt32(reader, 4);
        entities.DebtDetailStatusHistory.CpaType = db.GetString(reader, 5);
        entities.DebtDetailStatusHistory.CspNumber = db.GetString(reader, 6);
        entities.DebtDetailStatusHistory.ObgId = db.GetInt32(reader, 7);
        entities.DebtDetailStatusHistory.Code = db.GetString(reader, 8);
        entities.DebtDetailStatusHistory.OtyType = db.GetInt32(reader, 9);
        entities.DebtDetailStatusHistory.ReasonTxt =
          db.GetNullableString(reader, 10);
        entities.DebtDetailStatusHistory.Populated = true;
        CheckValid<DebtDetailStatusHistory>("OtrType",
          entities.DebtDetailStatusHistory.OtrType);
        CheckValid<DebtDetailStatusHistory>("CpaType",
          entities.DebtDetailStatusHistory.CpaType);
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
    public Collection Persistent
    {
      get => persistent ??= new();
      set => persistent = value;
    }

    private Collection persistent;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
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
    /// A value of DebtDetailStatusHistory.
    /// </summary>
    [JsonPropertyName("debtDetailStatusHistory")]
    public DebtDetailStatusHistory DebtDetailStatusHistory
    {
      get => debtDetailStatusHistory ??= new();
      set => debtDetailStatusHistory = value;
    }

    private ObligationTransaction debt;
    private DebtDetail debtDetail;
    private DebtDetailStatusHistory debtDetailStatusHistory;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of MaximumDiscontinue.
    /// </summary>
    [JsonPropertyName("maximumDiscontinue")]
    public DateWorkArea MaximumDiscontinue
    {
      get => maximumDiscontinue ??= new();
      set => maximumDiscontinue = value;
    }

    private DateWorkArea maximumDiscontinue;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
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
    /// A value of DebtDetailStatusHistory.
    /// </summary>
    [JsonPropertyName("debtDetailStatusHistory")]
    public DebtDetailStatusHistory DebtDetailStatusHistory
    {
      get => debtDetailStatusHistory ??= new();
      set => debtDetailStatusHistory = value;
    }

    private ObligationTransaction debt;
    private DebtDetail debtDetail;
    private DebtDetailStatusHistory debtDetailStatusHistory;
  }
#endregion
}
