// Program: FN_B643_OBLIGATION_SUPPRESSION, ID: 372683794, model: 746.
// Short name: SWE02387
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_B643_OBLIGATION_SUPPRESSION.
/// </summary>
[Serializable]
public partial class FnB643ObligationSuppression: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_B643_OBLIGATION_SUPPRESSION program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnB643ObligationSuppression(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnB643ObligationSuppression.
  /// </summary>
  public FnB643ObligationSuppression(IContext context, Import import,
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
    // ********************************************************************
    // * PR NUM     DATE     NAME      DESCRIPTION                        *
    // * ------  ----------  --------  
    // ----------------------------------
    // *
    // * # 414   09-21-1999  Ed Lyman  Do not report as errors, those     *
    // *
    // 
    // obligations without start
    // dates.   *
    // * #77218  11-02-1999  Ed Lyman  Do not print obligations, if       *
    // *
    // 
    // nothing is owed.  Added new
    // actblk *
    // *
    // 
    // fn_determine_active_obligation.
    // *
    // * #78285  11-02-1999  Ed Lyman  Do not print obligations, if       *
    // *
    // 
    // payment history will also
    // print.   *
    // * #78285  11-15-1999  Ed Lyman  Do not print obligations, if       *
    // *
    // 
    // not active before stmt begin
    // date. *
    // ********************************************************************
    export.CpnsSuppressedByDebt.Count = import.CpnsSuppressedByDebt.Count;
    export.StmtSuppressedByDebt.Count = import.StmtSuppressedByDebt.Count;
    export.StmtSuppressedDormant.Count = import.StmtSuppressedDormant.Count;
    export.StmtSuppressedInterstat.Count = import.StmtSuppressedInterstat.Count;
    export.StmtSuppressedSecondary.Count = import.StmtSuppressedSecondary.Count;
    export.SuppressCouponsByDebt.Flag = "";
    export.SuppressStmtAndCpns.Flag = "";
    local.ActiveAndNoPayhistInd.Flag = "N";
    UseFnCabDetermineOblgStartDate();

    if (Equal(local.ObligationStartDate.Date, local.NullDate.Date))
    {
      // **************************************************************
      // Suppress because debt has no start date.
      // **************************************************************
      export.SuppressStmtAndCpns.Flag = "Y";
      ++export.StmtSuppressedByDebt.Count;

      return;
    }
    else if (!Lt(local.ObligationStartDate.Date, import.StmtBeginDate.Date))
    {
      // **************************************************************
      // Suppress because debt is not yet active.
      // **************************************************************
      export.SuppressStmtAndCpns.Flag = "Y";
      ++export.StmtSuppressedByDebt.Count;

      return;
    }

    UseFnB643ActiveObligNoPayhist();

    if (AsChar(local.ActiveAndNoPayhistInd.Flag) == 'N')
    {
      export.SuppressStmtAndCpns.Flag = "Y";
      ++export.StmtSuppressedByDebt.Count;

      return;
    }

    if (AsChar(import.Obligation.DormantInd) == 'Y')
    {
      // **************************************************************
      // Y means that the obligation is dormant.
      // **************************************************************
      export.SuppressStmtAndCpns.Flag = "Y";
      ++export.StmtSuppressedDormant.Count;

      return;
    }

    if (AsChar(import.Obligation.OrderTypeCode) == 'I')
    {
      // **************************************************************
      // I means that the obligation is incoming Interstate.
      // Write to report here.
      // **************************************************************
      export.SuppressStmtAndCpns.Flag = "Y";
      ++export.StmtSuppressedInterstat.Count;

      return;
    }

    if (AsChar(import.Obligation.PrimarySecondaryCode) == 'S')
    {
      // **************************************************************
      // S means that the obligation is secondary.
      // Write to report here.
      // **************************************************************
      export.SuppressStmtAndCpns.Flag = "Y";
      ++export.StmtSuppressedSecondary.Count;

      return;
    }

    // **********************************************************
    // SEE IF STMTS OR COUPONS ARE SUPPRESSED BY OBLIGATION
    // **********************************************************
    if (ReadStmtCpnSuppStatObligation())
    {
      if (AsChar(entities.StmtCpnSuppStatObligation.DocTypeToSuppress) == 'S')
      {
        export.SuppressStmtAndCpns.Flag = "Y";
        ++export.StmtSuppressedByDebt.Count;
        ++export.CpnsSuppressedByDebt.Count;
      }
      else
      {
        export.SuppressCouponsByDebt.Flag = "Y";
        ++export.CpnsSuppressedByDebt.Count;
      }
    }
  }

  private static void MoveDateWorkArea(DateWorkArea source, DateWorkArea target)
  {
    target.Date = source.Date;
    target.Timestamp = source.Timestamp;
  }

  private void UseFnB643ActiveObligNoPayhist()
  {
    var useImport = new FnB643ActiveObligNoPayhist.Import();
    var useExport = new FnB643ActiveObligNoPayhist.Export();

    useImport.FcrtRec.SystemGeneratedIdentifier =
      import.FcrtRec.SystemGeneratedIdentifier;
    useImport.FdirPmt.SystemGeneratedIdentifier =
      import.FdirPmt.SystemGeneratedIdentifier;
    MoveDateWorkArea(import.StmtBeginDate, useImport.StmtBegin);
    useImport.Obligation.Assign(import.Obligation);
    MoveDateWorkArea(import.StmtEndDate, useImport.StmtEnd);

    Call(FnB643ActiveObligNoPayhist.Execute, useImport, useExport);

    local.ActiveAndNoPayhistInd.Flag = useExport.ActiveAndNoPayhistInd.Flag;
  }

  private void UseFnCabDetermineOblgStartDate()
  {
    var useImport = new FnCabDetermineOblgStartDate.Import();
    var useExport = new FnCabDetermineOblgStartDate.Export();

    useImport.Obligation.Assign(import.Obligation);
    MoveDateWorkArea(import.StmtEndDate, useImport.StmtEnd);

    Call(FnCabDetermineOblgStartDate.Execute, useImport, useExport);

    local.ObligationStartDate.Date = useExport.ObligationStartDate.Date;
  }

  private bool ReadStmtCpnSuppStatObligation()
  {
    System.Diagnostics.Debug.Assert(import.Obligation.Populated);
    entities.StmtCpnSuppStatObligation.Populated = false;

    return Read("ReadStmtCpnSuppStatObligation",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "obgId", import.Obligation.SystemGeneratedIdentifier);
        db.SetNullableString(
          command, "cspNumberOblig", import.Obligation.CspNumber);
        db.
          SetNullableString(command, "cpaTypeOblig", import.Obligation.CpaType);
          
        db.SetNullableInt32(command, "otyId", import.Obligation.DtyGeneratedId);
        db.SetDate(
          command, "effectiveDate", import.Process.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.StmtCpnSuppStatObligation.CpaType = db.GetString(reader, 0);
        entities.StmtCpnSuppStatObligation.CspNumber = db.GetString(reader, 1);
        entities.StmtCpnSuppStatObligation.SystemGeneratedIdentifier =
          db.GetInt32(reader, 2);
        entities.StmtCpnSuppStatObligation.Type1 = db.GetString(reader, 3);
        entities.StmtCpnSuppStatObligation.EffectiveDate =
          db.GetDate(reader, 4);
        entities.StmtCpnSuppStatObligation.DiscontinueDate =
          db.GetDate(reader, 5);
        entities.StmtCpnSuppStatObligation.OtyId =
          db.GetNullableInt32(reader, 6);
        entities.StmtCpnSuppStatObligation.CpaTypeOblig =
          db.GetNullableString(reader, 7);
        entities.StmtCpnSuppStatObligation.CspNumberOblig =
          db.GetNullableString(reader, 8);
        entities.StmtCpnSuppStatObligation.ObgId =
          db.GetNullableInt32(reader, 9);
        entities.StmtCpnSuppStatObligation.DocTypeToSuppress =
          db.GetString(reader, 10);
        entities.StmtCpnSuppStatObligation.Populated = true;
        CheckValid<StmtCouponSuppStatusHist>("CpaType",
          entities.StmtCpnSuppStatObligation.CpaType);
        CheckValid<StmtCouponSuppStatusHist>("Type1",
          entities.StmtCpnSuppStatObligation.Type1);
        CheckValid<StmtCouponSuppStatusHist>("CpaTypeOblig",
          entities.StmtCpnSuppStatObligation.CpaTypeOblig);
        CheckValid<StmtCouponSuppStatusHist>("DocTypeToSuppress",
          entities.StmtCpnSuppStatObligation.DocTypeToSuppress);
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
    /// A value of FcrtRec.
    /// </summary>
    [JsonPropertyName("fcrtRec")]
    public CashReceiptType FcrtRec
    {
      get => fcrtRec ??= new();
      set => fcrtRec = value;
    }

    /// <summary>
    /// A value of FdirPmt.
    /// </summary>
    [JsonPropertyName("fdirPmt")]
    public CashReceiptType FdirPmt
    {
      get => fdirPmt ??= new();
      set => fdirPmt = value;
    }

    /// <summary>
    /// A value of StmtBeginDate.
    /// </summary>
    [JsonPropertyName("stmtBeginDate")]
    public DateWorkArea StmtBeginDate
    {
      get => stmtBeginDate ??= new();
      set => stmtBeginDate = value;
    }

    /// <summary>
    /// A value of StmtEndDate.
    /// </summary>
    [JsonPropertyName("stmtEndDate")]
    public DateWorkArea StmtEndDate
    {
      get => stmtEndDate ??= new();
      set => stmtEndDate = value;
    }

    /// <summary>
    /// A value of StmtSuppressedSecondary.
    /// </summary>
    [JsonPropertyName("stmtSuppressedSecondary")]
    public Common StmtSuppressedSecondary
    {
      get => stmtSuppressedSecondary ??= new();
      set => stmtSuppressedSecondary = value;
    }

    /// <summary>
    /// A value of StmtSuppressedDormant.
    /// </summary>
    [JsonPropertyName("stmtSuppressedDormant")]
    public Common StmtSuppressedDormant
    {
      get => stmtSuppressedDormant ??= new();
      set => stmtSuppressedDormant = value;
    }

    /// <summary>
    /// A value of StmtSuppressedInterstat.
    /// </summary>
    [JsonPropertyName("stmtSuppressedInterstat")]
    public Common StmtSuppressedInterstat
    {
      get => stmtSuppressedInterstat ??= new();
      set => stmtSuppressedInterstat = value;
    }

    /// <summary>
    /// A value of StmtSuppressedByDebt.
    /// </summary>
    [JsonPropertyName("stmtSuppressedByDebt")]
    public Common StmtSuppressedByDebt
    {
      get => stmtSuppressedByDebt ??= new();
      set => stmtSuppressedByDebt = value;
    }

    /// <summary>
    /// A value of CpnsSuppressedByDebt.
    /// </summary>
    [JsonPropertyName("cpnsSuppressedByDebt")]
    public Common CpnsSuppressedByDebt
    {
      get => cpnsSuppressedByDebt ??= new();
      set => cpnsSuppressedByDebt = value;
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
    /// A value of Client.
    /// </summary>
    [JsonPropertyName("client")]
    public CsePerson Client
    {
      get => client ??= new();
      set => client = value;
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

    private CashReceiptType fcrtRec;
    private CashReceiptType fdirPmt;
    private DateWorkArea stmtBeginDate;
    private DateWorkArea stmtEndDate;
    private Common stmtSuppressedSecondary;
    private Common stmtSuppressedDormant;
    private Common stmtSuppressedInterstat;
    private Common stmtSuppressedByDebt;
    private Common cpnsSuppressedByDebt;
    private DateWorkArea process;
    private CsePerson client;
    private Obligation obligation;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of StmtSuppressedSecondary.
    /// </summary>
    [JsonPropertyName("stmtSuppressedSecondary")]
    public Common StmtSuppressedSecondary
    {
      get => stmtSuppressedSecondary ??= new();
      set => stmtSuppressedSecondary = value;
    }

    /// <summary>
    /// A value of StmtSuppressedDormant.
    /// </summary>
    [JsonPropertyName("stmtSuppressedDormant")]
    public Common StmtSuppressedDormant
    {
      get => stmtSuppressedDormant ??= new();
      set => stmtSuppressedDormant = value;
    }

    /// <summary>
    /// A value of StmtSuppressedInterstat.
    /// </summary>
    [JsonPropertyName("stmtSuppressedInterstat")]
    public Common StmtSuppressedInterstat
    {
      get => stmtSuppressedInterstat ??= new();
      set => stmtSuppressedInterstat = value;
    }

    /// <summary>
    /// A value of StmtSuppressedByDebt.
    /// </summary>
    [JsonPropertyName("stmtSuppressedByDebt")]
    public Common StmtSuppressedByDebt
    {
      get => stmtSuppressedByDebt ??= new();
      set => stmtSuppressedByDebt = value;
    }

    /// <summary>
    /// A value of CpnsSuppressedByDebt.
    /// </summary>
    [JsonPropertyName("cpnsSuppressedByDebt")]
    public Common CpnsSuppressedByDebt
    {
      get => cpnsSuppressedByDebt ??= new();
      set => cpnsSuppressedByDebt = value;
    }

    /// <summary>
    /// A value of SuppressStmtAndCpns.
    /// </summary>
    [JsonPropertyName("suppressStmtAndCpns")]
    public Common SuppressStmtAndCpns
    {
      get => suppressStmtAndCpns ??= new();
      set => suppressStmtAndCpns = value;
    }

    /// <summary>
    /// A value of SuppressCouponsByDebt.
    /// </summary>
    [JsonPropertyName("suppressCouponsByDebt")]
    public Common SuppressCouponsByDebt
    {
      get => suppressCouponsByDebt ??= new();
      set => suppressCouponsByDebt = value;
    }

    private Common stmtSuppressedSecondary;
    private Common stmtSuppressedDormant;
    private Common stmtSuppressedInterstat;
    private Common stmtSuppressedByDebt;
    private Common cpnsSuppressedByDebt;
    private Common suppressStmtAndCpns;
    private Common suppressCouponsByDebt;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of ActiveAndNoPayhistInd.
    /// </summary>
    [JsonPropertyName("activeAndNoPayhistInd")]
    public Common ActiveAndNoPayhistInd
    {
      get => activeAndNoPayhistInd ??= new();
      set => activeAndNoPayhistInd = value;
    }

    /// <summary>
    /// A value of NullDate.
    /// </summary>
    [JsonPropertyName("nullDate")]
    public DateWorkArea NullDate
    {
      get => nullDate ??= new();
      set => nullDate = value;
    }

    /// <summary>
    /// A value of ObligationStartDate.
    /// </summary>
    [JsonPropertyName("obligationStartDate")]
    public DateWorkArea ObligationStartDate
    {
      get => obligationStartDate ??= new();
      set => obligationStartDate = value;
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

    /// <summary>
    /// A value of EabFileHandling.
    /// </summary>
    [JsonPropertyName("eabFileHandling")]
    public EabFileHandling EabFileHandling
    {
      get => eabFileHandling ??= new();
      set => eabFileHandling = value;
    }

    private Common activeAndNoPayhistInd;
    private DateWorkArea nullDate;
    private DateWorkArea obligationStartDate;
    private EabReportSend eabReportSend;
    private EabFileHandling eabFileHandling;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of StmtCpnSuppStatObligation.
    /// </summary>
    [JsonPropertyName("stmtCpnSuppStatObligation")]
    public StmtCouponSuppStatusHist StmtCpnSuppStatObligation
    {
      get => stmtCpnSuppStatObligation ??= new();
      set => stmtCpnSuppStatObligation = value;
    }

    private StmtCouponSuppStatusHist stmtCpnSuppStatObligation;
  }
#endregion
}
