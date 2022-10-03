// Program: FN_CHANGE_DEBT_DETAIL_STAT_HIST, ID: 372257991, model: 746.
// Short name: SWE00312
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: FN_CHANGE_DEBT_DETAIL_STAT_HIST.
/// </para>
/// <para>
/// RESP:  FINANCE
/// DESC:  This action block updates the current status history occurrence with 
/// the discontinue date equal to the imported effective date of the new status.
/// The action block then creates a new status history occurrence.
/// </para>
/// </summary>
[Serializable]
public partial class FnChangeDebtDetailStatHist: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_CHANGE_DEBT_DETAIL_STAT_HIST program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnChangeDebtDetailStatHist(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnChangeDebtDetailStatHist.
  /// </summary>
  public FnChangeDebtDetailStatHist(IContext context, Import import,
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
    if (!import.Persistent.Populated)
    {
      if (ReadDebtDetail())
      {
        ++export.ImportNumberOfReads.Count;
      }
      else
      {
        ExitState = "FN0211_DEBT_DETAIL_NF";

        return;
      }
    }

    if (ReadDebtDetailStatusHistory())
    {
      ++export.ImportNumberOfReads.Count;

      try
      {
        UpdateDebtDetailStatusHistory();
        ++export.ImportNumberOfUpdates.Count;
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "FN0224_DEBT_DETL_STAT_HIST_NU";

            return;
          case ErrorCode.PermittedValueViolation:
            ExitState = "FN0226_DEBT_DETL_STAT_HIST_PV";

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
      ExitState = "FN0222_DEBT_DETL_STAT_HIST_NF";

      return;
    }

    for(local.Loop.Count = 1; local.Loop.Count <= 5; ++local.Loop.Count)
    {
      try
      {
        CreateDebtDetailStatusHistory();
        ++export.ImportNumberOfUpdates.Count;
        export.DebtDetailStatusHistory.Assign(entities.DebtDetailStatusHistory);

        return;
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            if (local.Loop.Count < 5)
            {
              continue;
            }
            else
            {
              ExitState = "FN0220_DEBT_DETL_STAT_HIST_AE";

              return;
            }

            break;
          case ErrorCode.PermittedValueViolation:
            ExitState = "FN0226_DEBT_DETL_STAT_HIST_PV";

            return;
          case ErrorCode.DatabaseError:
            break;
          default:
            throw;
        }
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

  private void CreateDebtDetailStatusHistory()
  {
    System.Diagnostics.Debug.Assert(import.Persistent.Populated);

    var systemGeneratedIdentifier = UseCabGenerate3DigitRandomNum();
    var effectiveDt = import.DebtDetailStatusHistory.EffectiveDt;
    var discontinueDt = import.Max.Date;
    var createdBy = global.UserId;
    var createdTmst = import.Current.Timestamp;
    var otrType = import.Persistent.OtrType;
    var otrId = import.Persistent.OtrGeneratedId;
    var cpaType = import.Persistent.CpaType;
    var cspNumber = import.Persistent.CspNumber;
    var obgId = import.Persistent.ObgGeneratedId;
    var code = import.DebtDetailStatusHistory.Code;
    var otyType = import.Persistent.OtyType;
    var reasonTxt = import.DebtDetailStatusHistory.ReasonTxt ?? "";

    CheckValid<DebtDetailStatusHistory>("OtrType", otrType);
    CheckValid<DebtDetailStatusHistory>("CpaType", cpaType);
    entities.DebtDetailStatusHistory.Populated = false;
    Update("CreateDebtDetailStatusHistory",
      (db, command) =>
      {
        db.SetInt32(command, "obTrnStatHstId", systemGeneratedIdentifier);
        db.SetDate(command, "effectiveDt", effectiveDt);
        db.SetNullableDate(command, "discontinueDt", discontinueDt);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTmst", createdTmst);
        db.SetString(command, "otrType", otrType);
        db.SetInt32(command, "otrId", otrId);
        db.SetString(command, "cpaType", cpaType);
        db.SetString(command, "cspNumber", cspNumber);
        db.SetInt32(command, "obgId", obgId);
        db.SetString(command, "obTrnStCd", code);
        db.SetInt32(command, "otyType", otyType);
        db.SetNullableString(command, "rsnTxt", reasonTxt);
      });

    entities.DebtDetailStatusHistory.SystemGeneratedIdentifier =
      systemGeneratedIdentifier;
    entities.DebtDetailStatusHistory.EffectiveDt = effectiveDt;
    entities.DebtDetailStatusHistory.DiscontinueDt = discontinueDt;
    entities.DebtDetailStatusHistory.CreatedBy = createdBy;
    entities.DebtDetailStatusHistory.CreatedTmst = createdTmst;
    entities.DebtDetailStatusHistory.OtrType = otrType;
    entities.DebtDetailStatusHistory.OtrId = otrId;
    entities.DebtDetailStatusHistory.CpaType = cpaType;
    entities.DebtDetailStatusHistory.CspNumber = cspNumber;
    entities.DebtDetailStatusHistory.ObgId = obgId;
    entities.DebtDetailStatusHistory.Code = code;
    entities.DebtDetailStatusHistory.OtyType = otyType;
    entities.DebtDetailStatusHistory.ReasonTxt = reasonTxt;
    entities.DebtDetailStatusHistory.Populated = true;
  }

  private bool ReadDebtDetail()
  {
    import.Persistent.Populated = false;

    return Read("ReadDebtDetail",
      (db, command) =>
      {
        db.SetInt32(
          command, "otrGeneratedId", import.Debt.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "obgGeneratedId",
          import.Obligation.SystemGeneratedIdentifier);
        db.SetString(command, "cspNumber", import.CsePerson.Number);
        db.SetInt32(
          command, "otyType", import.ObligationType.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        import.Persistent.ObgGeneratedId = db.GetInt32(reader, 0);
        import.Persistent.CspNumber = db.GetString(reader, 1);
        import.Persistent.CpaType = db.GetString(reader, 2);
        import.Persistent.OtrGeneratedId = db.GetInt32(reader, 3);
        import.Persistent.OtyType = db.GetInt32(reader, 4);
        import.Persistent.OtrType = db.GetString(reader, 5);
        import.Persistent.DueDt = db.GetDate(reader, 6);
        import.Persistent.BalanceDueAmt = db.GetDecimal(reader, 7);
        import.Persistent.InterestBalanceDueAmt =
          db.GetNullableDecimal(reader, 8);
        import.Persistent.AdcDt = db.GetNullableDate(reader, 9);
        import.Persistent.RetiredDt = db.GetNullableDate(reader, 10);
        import.Persistent.CoveredPrdStartDt = db.GetNullableDate(reader, 11);
        import.Persistent.CoveredPrdEndDt = db.GetNullableDate(reader, 12);
        import.Persistent.PreconversionProgramCode =
          db.GetNullableString(reader, 13);
        import.Persistent.LastUpdatedTmst = db.GetNullableDateTime(reader, 14);
        import.Persistent.LastUpdatedBy = db.GetNullableString(reader, 15);
        import.Persistent.Populated = true;
        CheckValid<DebtDetail>("CpaType", import.Persistent.CpaType);
        CheckValid<DebtDetail>("OtrType", import.Persistent.OtrType);
      });
  }

  private bool ReadDebtDetailStatusHistory()
  {
    System.Diagnostics.Debug.Assert(import.Persistent.Populated);
    entities.DebtDetailStatusHistory.Populated = false;

    return Read("ReadDebtDetailStatusHistory",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "discontinueDt", import.Max.Date.GetValueOrDefault());
        db.SetInt32(command, "otyType", import.Persistent.OtyType);
        db.SetInt32(command, "obgId", import.Persistent.ObgGeneratedId);
        db.SetString(command, "cspNumber", import.Persistent.CspNumber);
        db.SetString(command, "cpaType", import.Persistent.CpaType);
        db.SetInt32(command, "otrId", import.Persistent.OtrGeneratedId);
        db.SetString(command, "otrType", import.Persistent.OtrType);
      },
      (db, reader) =>
      {
        entities.DebtDetailStatusHistory.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.DebtDetailStatusHistory.EffectiveDt = db.GetDate(reader, 1);
        entities.DebtDetailStatusHistory.DiscontinueDt =
          db.GetNullableDate(reader, 2);
        entities.DebtDetailStatusHistory.CreatedBy = db.GetString(reader, 3);
        entities.DebtDetailStatusHistory.CreatedTmst =
          db.GetDateTime(reader, 4);
        entities.DebtDetailStatusHistory.OtrType = db.GetString(reader, 5);
        entities.DebtDetailStatusHistory.OtrId = db.GetInt32(reader, 6);
        entities.DebtDetailStatusHistory.CpaType = db.GetString(reader, 7);
        entities.DebtDetailStatusHistory.CspNumber = db.GetString(reader, 8);
        entities.DebtDetailStatusHistory.ObgId = db.GetInt32(reader, 9);
        entities.DebtDetailStatusHistory.Code = db.GetString(reader, 10);
        entities.DebtDetailStatusHistory.OtyType = db.GetInt32(reader, 11);
        entities.DebtDetailStatusHistory.ReasonTxt =
          db.GetNullableString(reader, 12);
        entities.DebtDetailStatusHistory.Populated = true;
        CheckValid<DebtDetailStatusHistory>("OtrType",
          entities.DebtDetailStatusHistory.OtrType);
        CheckValid<DebtDetailStatusHistory>("CpaType",
          entities.DebtDetailStatusHistory.CpaType);
      });
  }

  private void UpdateDebtDetailStatusHistory()
  {
    System.Diagnostics.Debug.Assert(entities.DebtDetailStatusHistory.Populated);

    var discontinueDt = import.DebtDetailStatusHistory.EffectiveDt;
    var reasonTxt = import.DebtDetailStatusHistory.ReasonTxt ?? "";

    entities.DebtDetailStatusHistory.Populated = false;
    Update("UpdateDebtDetailStatusHistory",
      (db, command) =>
      {
        db.SetNullableDate(command, "discontinueDt", discontinueDt);
        db.SetNullableString(command, "rsnTxt", reasonTxt);
        db.SetInt32(
          command, "obTrnStatHstId",
          entities.DebtDetailStatusHistory.SystemGeneratedIdentifier);
        db.SetString(
          command, "otrType", entities.DebtDetailStatusHistory.OtrType);
        db.SetInt32(command, "otrId", entities.DebtDetailStatusHistory.OtrId);
        db.SetString(
          command, "cpaType", entities.DebtDetailStatusHistory.CpaType);
        db.SetString(
          command, "cspNumber", entities.DebtDetailStatusHistory.CspNumber);
        db.SetInt32(command, "obgId", entities.DebtDetailStatusHistory.ObgId);
        db.
          SetString(command, "obTrnStCd", entities.DebtDetailStatusHistory.Code);
          
        db.
          SetInt32(command, "otyType", entities.DebtDetailStatusHistory.OtyType);
          
      });

    entities.DebtDetailStatusHistory.DiscontinueDt = discontinueDt;
    entities.DebtDetailStatusHistory.ReasonTxt = reasonTxt;
    entities.DebtDetailStatusHistory.Populated = true;
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
    /// A value of Persistent.
    /// </summary>
    [JsonPropertyName("persistent")]
    public DebtDetail Persistent
    {
      get => persistent ??= new();
      set => persistent = value;
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
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
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

    private DateWorkArea current;
    private DateWorkArea max;
    private DebtDetail persistent;
    private ObligationTransaction debt;
    private Obligation obligation;
    private ObligationType obligationType;
    private CsePerson csePerson;
    private DebtDetailStatusHistory debtDetailStatusHistory;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
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
    /// A value of ImportNumberOfReads.
    /// </summary>
    [JsonPropertyName("importNumberOfReads")]
    public Common ImportNumberOfReads
    {
      get => importNumberOfReads ??= new();
      set => importNumberOfReads = value;
    }

    /// <summary>
    /// A value of ImportNumberOfUpdates.
    /// </summary>
    [JsonPropertyName("importNumberOfUpdates")]
    public Common ImportNumberOfUpdates
    {
      get => importNumberOfUpdates ??= new();
      set => importNumberOfUpdates = value;
    }

    private DebtDetailStatusHistory debtDetailStatusHistory;
    private Common importNumberOfReads;
    private Common importNumberOfUpdates;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local: IInitializable
  {
    /// <summary>
    /// A value of Loop.
    /// </summary>
    [JsonPropertyName("loop")]
    public Common Loop
    {
      get => loop ??= new();
      set => loop = value;
    }

    /// <summary>
    /// A value of ZdelLocalMaximumDiscontinue.
    /// </summary>
    [JsonPropertyName("zdelLocalMaximumDiscontinue")]
    public DateWorkArea ZdelLocalMaximumDiscontinue
    {
      get => zdelLocalMaximumDiscontinue ??= new();
      set => zdelLocalMaximumDiscontinue = value;
    }

    /// <para>Resets the state.</para>
    void IInitializable.Initialize()
    {
      loop = null;
    }

    private Common loop;
    private DateWorkArea zdelLocalMaximumDiscontinue;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
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
    /// A value of Debt.
    /// </summary>
    [JsonPropertyName("debt")]
    public ObligationTransaction Debt
    {
      get => debt ??= new();
      set => debt = value;
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

    private DebtDetailStatusHistory debtDetailStatusHistory;
    private ObligationTransaction debt;
    private Obligation obligation;
    private ObligationType obligationType;
    private CsePersonAccount obligor;
    private CsePerson csePerson;
  }
#endregion
}
