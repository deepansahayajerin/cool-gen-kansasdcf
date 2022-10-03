// Program: FN_CHECK_FOR_AUTOMATIC_DISB_SUPP, ID: 372544587, model: 746.
// Short name: SWE02425
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: FN_CHECK_FOR_AUTOMATIC_DISB_SUPP.
/// </para>
/// <para>
/// RESP: FINANCE
/// This action block will determine if disbursement suppression is turned on at
/// the automatic level.
/// </para>
/// </summary>
[Serializable]
public partial class FnCheckForAutomaticDisbSupp: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_CHECK_FOR_AUTOMATIC_DISB_SUPP program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnCheckForAutomaticDisbSupp(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnCheckForAutomaticDisbSupp.
  /// </summary>
  public FnCheckForAutomaticDisbSupp(IContext context, Import import,
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
    // R Kessler 01/09/99. Created to look for the new automatic suppression.
    // SWSRKXD 7/15/99 Add import collection and surrounding logic.
    // SWSRKXD 7/20/99 CAB no longer imports Collection. Instead Debt_detail.
    // due_dt is imported.
    // ****************************************************
    // --------------------------------------------------------
    // SWSRKXD - 7/20/99
    // Delete import view of collection and read of debt_detail. Add
    // import debt_detail.due_dt.
    // -------------------------------------------------------
    // --------------------------------------------------------
    // SWSRKXD - 7/15/99
    // B650 sets the discontinue_date of automatic suppression to
    // first day of month of debt_detail due_date. There may be
    // automatic suppressions created for other disbursements
    // but we only read for the record where this criteria is met.
    // -------------------------------------------------------
    // ****************************************************************
    // 2000-02-14  PR 86861  Fangman  Removed unused import view.
    // 2000-03-21  PR 86768  Fangman  Removed export suppression flag & 
    // defaulted the suppression date to initialized values.
    // 2000-09-12  PR 103323  Fangman  Added code to skip suppression if 
    // discontinue date <= process date.
    // 2000-10-05  PR 98039  Fangman  Removed unused entity views.
    // ****************************************************************
    local.DateWorkArea.TextDate =
      NumberToString(DateToInt(import.DebtDetail.DueDt), 8, 6) + "01";
    local.DateWorkArea.Date =
      IntToDate((int)StringToNumber(local.DateWorkArea.TextDate));

    if (!Lt(import.ProgramProcessingInfo.ProcessDate, local.DateWorkArea.Date))
    {
      // Do not suppress disbursement if discontinue date will be equal to or 
      // prior to process date.
      return;
    }

    if (ReadDisbSuppressionStatusHistoryCsePersonAccount())
    {
      export.DisbSuppressionStatusHistory.DiscontinueDate =
        entities.DisbSuppressionStatusHistory.DiscontinueDate;
    }
    else
    {
      export.DisbSuppressionStatusHistory.DiscontinueDate =
        local.Initialize.DiscontinueDate;
    }
  }

  private bool ReadDisbSuppressionStatusHistoryCsePersonAccount()
  {
    entities.CsePersonAccount.Populated = false;
    entities.DisbSuppressionStatusHistory.Populated = false;

    return Read("ReadDisbSuppressionStatusHistoryCsePersonAccount",
      (db, command) =>
      {
        db.SetDate(
          command, "effectiveDate",
          import.ProgramProcessingInfo.ProcessDate.GetValueOrDefault());
        db.SetDate(
          command, "discontinueDate",
          local.DateWorkArea.Date.GetValueOrDefault());
        db.SetString(command, "cspNumber", import.CsePerson.Number);
      },
      (db, reader) =>
      {
        entities.DisbSuppressionStatusHistory.CpaType = db.GetString(reader, 0);
        entities.CsePersonAccount.Type1 = db.GetString(reader, 0);
        entities.DisbSuppressionStatusHistory.CspNumber =
          db.GetString(reader, 1);
        entities.CsePersonAccount.CspNumber = db.GetString(reader, 1);
        entities.DisbSuppressionStatusHistory.SystemGeneratedIdentifier =
          db.GetInt32(reader, 2);
        entities.DisbSuppressionStatusHistory.EffectiveDate =
          db.GetDate(reader, 3);
        entities.DisbSuppressionStatusHistory.DiscontinueDate =
          db.GetDate(reader, 4);
        entities.DisbSuppressionStatusHistory.Type1 = db.GetString(reader, 5);
        entities.CsePersonAccount.Populated = true;
        entities.DisbSuppressionStatusHistory.Populated = true;
        CheckValid<DisbSuppressionStatusHistory>("CpaType",
          entities.DisbSuppressionStatusHistory.CpaType);
        CheckValid<CsePersonAccount>("Type1", entities.CsePersonAccount.Type1);
        CheckValid<DisbSuppressionStatusHistory>("Type1",
          entities.DisbSuppressionStatusHistory.Type1);
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
    /// A value of DebtDetail.
    /// </summary>
    [JsonPropertyName("debtDetail")]
    public DebtDetail DebtDetail
    {
      get => debtDetail ??= new();
      set => debtDetail = value;
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
    /// A value of ProgramProcessingInfo.
    /// </summary>
    [JsonPropertyName("programProcessingInfo")]
    public ProgramProcessingInfo ProgramProcessingInfo
    {
      get => programProcessingInfo ??= new();
      set => programProcessingInfo = value;
    }

    private DebtDetail debtDetail;
    private CsePerson csePerson;
    private ProgramProcessingInfo programProcessingInfo;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of DisbSuppressionStatusHistory.
    /// </summary>
    [JsonPropertyName("disbSuppressionStatusHistory")]
    public DisbSuppressionStatusHistory DisbSuppressionStatusHistory
    {
      get => disbSuppressionStatusHistory ??= new();
      set => disbSuppressionStatusHistory = value;
    }

    private DisbSuppressionStatusHistory disbSuppressionStatusHistory;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of Initialize.
    /// </summary>
    [JsonPropertyName("initialize")]
    public DisbSuppressionStatusHistory Initialize
    {
      get => initialize ??= new();
      set => initialize = value;
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

    private DisbSuppressionStatusHistory initialize;
    private DateWorkArea dateWorkArea;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
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
    /// A value of CsePersonAccount.
    /// </summary>
    [JsonPropertyName("csePersonAccount")]
    public CsePersonAccount CsePersonAccount
    {
      get => csePersonAccount ??= new();
      set => csePersonAccount = value;
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

    private CsePerson csePerson;
    private CsePersonAccount csePersonAccount;
    private DisbSuppressionStatusHistory disbSuppressionStatusHistory;
  }
#endregion
}
