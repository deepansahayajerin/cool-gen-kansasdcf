// Program: FN_CHK_UNPROC_TRAN_MNL_DIST, ID: 1625336069, model: 746.
// Short name: SWE03106
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_CHK_UNPROC_TRAN_MNL_DIST.
/// </summary>
[Serializable]
public partial class FnChkUnprocTranMnlDist: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_CHK_UNPROC_TRAN_MNL_DIST program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnChkUnprocTranMnlDist(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnChkUnprocTranMnlDist.
  /// </summary>
  public FnChkUnprocTranMnlDist(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ------------------------------------------------------------------------------------
    // 04/26/2019   R Mathews   CQ65311   Copied from 
    // FN_CHECK_FOR_UNPROCESSED_TRANS to
    //                                    
    // shorten error messages for MCOL
    // display.
    // ------------------------------------------------------------------------------------
    if (IsEmpty(import.Obligor.Number))
    {
      ExitState = "FN0000_CSE_PERSON_NOT_PASSED";

      return;
    }

    UseFnHardcodedCashReceipting();
    local.UnprocCashRcptInd.Text1 = "N";
    local.UnprocCollAdjInd.Text1 = "N";
    local.UnprocDebtAdjInd.Text1 = "N";
    local.UnprocDebtInd.Text1 = "N";

    if (import.FilterObligationType.SystemGeneratedIdentifier != 0 && import
      .FilterObligation.SystemGeneratedIdentifier != 0)
    {
      local.LowObligationType.SystemGeneratedIdentifier =
        import.FilterObligationType.SystemGeneratedIdentifier;
      local.LowObligation.SystemGeneratedIdentifier =
        import.FilterObligation.SystemGeneratedIdentifier;
      local.HighObligationType.SystemGeneratedIdentifier =
        import.FilterObligationType.SystemGeneratedIdentifier;
      local.HighObligation.SystemGeneratedIdentifier =
        import.FilterObligation.SystemGeneratedIdentifier;
    }
    else
    {
      local.LowObligationType.SystemGeneratedIdentifier = 0;
      local.LowObligation.SystemGeneratedIdentifier = 0;
      local.HighObligationType.SystemGeneratedIdentifier = 999;
      local.HighObligation.SystemGeneratedIdentifier = 999;
    }

    if (ReadCollection())
    {
      local.UnprocCollAdjInd.Text1 = "Y";
    }
    else
    {
      // : Not a problem - Continue Processing.
    }

    if (ReadDebt())
    {
      local.UnprocDebtInd.Text1 = "Y";
    }
    else
    {
      // : Not a problem - Continue Processing.
    }

    if (ReadDebtAdjustment())
    {
      local.UnprocDebtAdjInd.Text1 = "Y";
    }
    else
    {
      // : Not a problem - Continue Processing.
    }

    if (AsChar(import.OmitCrdInd.Flag) != 'Y')
    {
      foreach(var item in ReadCashReceiptDetailCashReceiptDetailStatus())
      {
        if (entities.ExistingKeyOnlyCashReceiptDetailStatus.
          SystemGeneratedIdentifier == local
          .HardcodedAdjusted.SystemGeneratedIdentifier)
        {
          continue;
        }

        local.UnprocCashRcptInd.Text1 = "Y";

        break;
      }
    }

    local.CompareInd.Text4 = local.UnprocCashRcptInd.Text1 + local
      .UnprocCollAdjInd.Text1 + local.UnprocDebtAdjInd.Text1 + local
      .UnprocDebtInd.Text1;

    switch(TrimEnd(local.CompareInd.Text4))
    {
      case "NNNN":
        export.ScreenOwedAmounts.ErrorInformationLine = "";

        break;
      case "NNNY":
        export.ScreenOwedAmounts.ErrorInformationLine = "UNPROC DE";

        break;
      case "NNYN":
        export.ScreenOwedAmounts.ErrorInformationLine = "UNPROC DA";

        break;
      case "NNYY":
        export.ScreenOwedAmounts.ErrorInformationLine = "UNPROC DE & DA";

        break;
      case "NYNN":
        export.ScreenOwedAmounts.ErrorInformationLine = "UNPROC CA";

        break;
      case "NYNY":
        export.ScreenOwedAmounts.ErrorInformationLine = "UNPROC DE & CA";

        break;
      case "NYYN":
        export.ScreenOwedAmounts.ErrorInformationLine = "UNPROC DA & CA";

        break;
      case "NYYY":
        export.ScreenOwedAmounts.ErrorInformationLine = "UNPROC DE, DA & CA";

        break;
      case "YNNN":
        export.ScreenOwedAmounts.ErrorInformationLine = "UNPROC CRD";

        break;
      case "YNNY":
        export.ScreenOwedAmounts.ErrorInformationLine = "UNPROC DE & CRD";

        break;
      case "YNYN":
        export.ScreenOwedAmounts.ErrorInformationLine = "UNPROC DA & CRD";

        break;
      case "YNYY":
        export.ScreenOwedAmounts.ErrorInformationLine = "UNPROC DE, DA & CRD";

        break;
      case "YYNN":
        export.ScreenOwedAmounts.ErrorInformationLine = "UNPROC CA & CRD";

        break;
      case "YYNY":
        export.ScreenOwedAmounts.ErrorInformationLine = "UNPROC DE, CA & CRD";

        break;
      case "YYYN":
        export.ScreenOwedAmounts.ErrorInformationLine = "UNPROC DA, CA & CRD";

        break;
      case "YYYY":
        export.ScreenOwedAmounts.ErrorInformationLine =
          "UNPROC DE, DA, CA & CRD";

        break;
      default:
        break;
    }
  }

  private void UseFnHardcodedCashReceipting()
  {
    var useImport = new FnHardcodedCashReceipting.Import();
    var useExport = new FnHardcodedCashReceipting.Export();

    Call(FnHardcodedCashReceipting.Execute, useImport, useExport);

    local.HardcodedAdjusted.SystemGeneratedIdentifier =
      useExport.CrdsSystemId.CrdsIdAdjusted.SystemGeneratedIdentifier;
  }

  private IEnumerable<bool> ReadCashReceiptDetailCashReceiptDetailStatus()
  {
    entities.ExistingCashReceiptDetail.Populated = false;
    entities.ExistingKeyOnlyCashReceiptDetailStatus.Populated = false;

    return ReadEach("ReadCashReceiptDetailCashReceiptDetailStatus",
      (db, command) =>
      {
        db.SetNullableString(command, "oblgorPrsnNbr", import.Obligor.Number);
      },
      (db, reader) =>
      {
        entities.ExistingCashReceiptDetail.CrvIdentifier =
          db.GetInt32(reader, 0);
        entities.ExistingCashReceiptDetail.CstIdentifier =
          db.GetInt32(reader, 1);
        entities.ExistingCashReceiptDetail.CrtIdentifier =
          db.GetInt32(reader, 2);
        entities.ExistingCashReceiptDetail.SequentialIdentifier =
          db.GetInt32(reader, 3);
        entities.ExistingCashReceiptDetail.AdjustmentInd =
          db.GetNullableString(reader, 4);
        entities.ExistingCashReceiptDetail.ObligorPersonNumber =
          db.GetNullableString(reader, 5);
        entities.ExistingCashReceiptDetail.CollectionAmtFullyAppliedInd =
          db.GetNullableString(reader, 6);
        entities.ExistingKeyOnlyCashReceiptDetailStatus.
          SystemGeneratedIdentifier = db.GetInt32(reader, 7);
        entities.ExistingCashReceiptDetail.Populated = true;
        entities.ExistingKeyOnlyCashReceiptDetailStatus.Populated = true;
        CheckValid<CashReceiptDetail>("CollectionAmtFullyAppliedInd",
          entities.ExistingCashReceiptDetail.CollectionAmtFullyAppliedInd);

        return true;
      });
  }

  private bool ReadCollection()
  {
    entities.ExistingCollection.Populated = false;

    return Read("ReadCollection",
      (db, command) =>
      {
        db.SetDate(
          command, "collAdjProcDate", local.Null1.Date.GetValueOrDefault());
        db.SetString(command, "cspNumber", import.Obligor.Number);
      },
      (db, reader) =>
      {
        entities.ExistingCollection.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.ExistingCollection.AdjustedInd =
          db.GetNullableString(reader, 1);
        entities.ExistingCollection.CrtType = db.GetInt32(reader, 2);
        entities.ExistingCollection.CstId = db.GetInt32(reader, 3);
        entities.ExistingCollection.CrvId = db.GetInt32(reader, 4);
        entities.ExistingCollection.CrdId = db.GetInt32(reader, 5);
        entities.ExistingCollection.ObgId = db.GetInt32(reader, 6);
        entities.ExistingCollection.CspNumber = db.GetString(reader, 7);
        entities.ExistingCollection.CpaType = db.GetString(reader, 8);
        entities.ExistingCollection.OtrId = db.GetInt32(reader, 9);
        entities.ExistingCollection.OtrType = db.GetString(reader, 10);
        entities.ExistingCollection.OtyId = db.GetInt32(reader, 11);
        entities.ExistingCollection.CollectionAdjProcessDate =
          db.GetDate(reader, 12);
        entities.ExistingCollection.Populated = true;
        CheckValid<Collection>("AdjustedInd",
          entities.ExistingCollection.AdjustedInd);
        CheckValid<Collection>("CpaType", entities.ExistingCollection.CpaType);
        CheckValid<Collection>("OtrType", entities.ExistingCollection.OtrType);
      });
  }

  private bool ReadDebt()
  {
    entities.ExistingDebt.Populated = false;

    return Read("ReadDebt",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "newDebtProcDt", local.Null1.Date.GetValueOrDefault());
        db.SetString(command, "cspNumber", import.Obligor.Number);
        db.SetInt32(
          command, "systemGeneratedIdentifier1",
          local.LowObligationType.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "systemGeneratedIdentifier2",
          local.HighObligationType.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "systemGeneratedIdentifier3",
          local.LowObligation.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "systemGeneratedIdentifier4",
          local.HighObligation.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.ExistingDebt.ObgGeneratedId = db.GetInt32(reader, 0);
        entities.ExistingDebt.CspNumber = db.GetString(reader, 1);
        entities.ExistingDebt.CpaType = db.GetString(reader, 2);
        entities.ExistingDebt.SystemGeneratedIdentifier =
          db.GetInt32(reader, 3);
        entities.ExistingDebt.Type1 = db.GetString(reader, 4);
        entities.ExistingDebt.DebtType = db.GetString(reader, 5);
        entities.ExistingDebt.CspSupNumber = db.GetNullableString(reader, 6);
        entities.ExistingDebt.CpaSupType = db.GetNullableString(reader, 7);
        entities.ExistingDebt.OtyType = db.GetInt32(reader, 8);
        entities.ExistingDebt.NewDebtProcessDate =
          db.GetNullableDate(reader, 9);
        entities.ExistingDebt.Populated = true;
        CheckValid<ObligationTransaction>("CpaType",
          entities.ExistingDebt.CpaType);
        CheckValid<ObligationTransaction>("Type1", entities.ExistingDebt.Type1);
        CheckValid<ObligationTransaction>("DebtType",
          entities.ExistingDebt.DebtType);
        CheckValid<ObligationTransaction>("CpaSupType",
          entities.ExistingDebt.CpaSupType);
      });
  }

  private bool ReadDebtAdjustment()
  {
    entities.ExistingDebtAdjustment.Populated = false;

    return Read("ReadDebtAdjustment",
      (db, command) =>
      {
        db.SetDate(
          command, "debtAdjProcDate", local.Null1.Date.GetValueOrDefault());
        db.SetString(command, "cspNumber", import.Obligor.Number);
        db.SetInt32(
          command, "systemGeneratedIdentifier1",
          local.LowObligationType.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "systemGeneratedIdentifier2",
          local.HighObligationType.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "systemGeneratedIdentifier3",
          local.LowObligation.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "systemGeneratedIdentifier4",
          local.HighObligation.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.ExistingDebtAdjustment.ObgGeneratedId = db.GetInt32(reader, 0);
        entities.ExistingDebtAdjustment.CspNumber = db.GetString(reader, 1);
        entities.ExistingDebtAdjustment.CpaType = db.GetString(reader, 2);
        entities.ExistingDebtAdjustment.SystemGeneratedIdentifier =
          db.GetInt32(reader, 3);
        entities.ExistingDebtAdjustment.Type1 = db.GetString(reader, 4);
        entities.ExistingDebtAdjustment.CspSupNumber =
          db.GetNullableString(reader, 5);
        entities.ExistingDebtAdjustment.CpaSupType =
          db.GetNullableString(reader, 6);
        entities.ExistingDebtAdjustment.OtyType = db.GetInt32(reader, 7);
        entities.ExistingDebtAdjustment.DebtAdjustmentProcessDate =
          db.GetDate(reader, 8);
        entities.ExistingDebtAdjustment.Populated = true;
        CheckValid<ObligationTransaction>("CpaType",
          entities.ExistingDebtAdjustment.CpaType);
        CheckValid<ObligationTransaction>("Type1",
          entities.ExistingDebtAdjustment.Type1);
        CheckValid<ObligationTransaction>("CpaSupType",
          entities.ExistingDebtAdjustment.CpaSupType);
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
    /// A value of Obligor.
    /// </summary>
    [JsonPropertyName("obligor")]
    public CsePerson Obligor
    {
      get => obligor ??= new();
      set => obligor = value;
    }

    /// <summary>
    /// A value of OmitCrdInd.
    /// </summary>
    [JsonPropertyName("omitCrdInd")]
    public Common OmitCrdInd
    {
      get => omitCrdInd ??= new();
      set => omitCrdInd = value;
    }

    /// <summary>
    /// A value of FilterObligationType.
    /// </summary>
    [JsonPropertyName("filterObligationType")]
    public ObligationType FilterObligationType
    {
      get => filterObligationType ??= new();
      set => filterObligationType = value;
    }

    /// <summary>
    /// A value of FilterObligation.
    /// </summary>
    [JsonPropertyName("filterObligation")]
    public Obligation FilterObligation
    {
      get => filterObligation ??= new();
      set => filterObligation = value;
    }

    private CsePerson obligor;
    private Common omitCrdInd;
    private ObligationType filterObligationType;
    private Obligation filterObligation;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of ScreenOwedAmounts.
    /// </summary>
    [JsonPropertyName("screenOwedAmounts")]
    public ScreenOwedAmounts ScreenOwedAmounts
    {
      get => screenOwedAmounts ??= new();
      set => screenOwedAmounts = value;
    }

    private ScreenOwedAmounts screenOwedAmounts;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
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
    /// A value of CompareInd.
    /// </summary>
    [JsonPropertyName("compareInd")]
    public TextWorkArea CompareInd
    {
      get => compareInd ??= new();
      set => compareInd = value;
    }

    /// <summary>
    /// A value of UnprocCollAdjInd.
    /// </summary>
    [JsonPropertyName("unprocCollAdjInd")]
    public TextWorkArea UnprocCollAdjInd
    {
      get => unprocCollAdjInd ??= new();
      set => unprocCollAdjInd = value;
    }

    /// <summary>
    /// A value of UnprocDebtAdjInd.
    /// </summary>
    [JsonPropertyName("unprocDebtAdjInd")]
    public TextWorkArea UnprocDebtAdjInd
    {
      get => unprocDebtAdjInd ??= new();
      set => unprocDebtAdjInd = value;
    }

    /// <summary>
    /// A value of UnprocDebtInd.
    /// </summary>
    [JsonPropertyName("unprocDebtInd")]
    public TextWorkArea UnprocDebtInd
    {
      get => unprocDebtInd ??= new();
      set => unprocDebtInd = value;
    }

    /// <summary>
    /// A value of UnprocCashRcptInd.
    /// </summary>
    [JsonPropertyName("unprocCashRcptInd")]
    public TextWorkArea UnprocCashRcptInd
    {
      get => unprocCashRcptInd ??= new();
      set => unprocCashRcptInd = value;
    }

    /// <summary>
    /// A value of LowObligationType.
    /// </summary>
    [JsonPropertyName("lowObligationType")]
    public ObligationType LowObligationType
    {
      get => lowObligationType ??= new();
      set => lowObligationType = value;
    }

    /// <summary>
    /// A value of LowObligation.
    /// </summary>
    [JsonPropertyName("lowObligation")]
    public Obligation LowObligation
    {
      get => lowObligation ??= new();
      set => lowObligation = value;
    }

    /// <summary>
    /// A value of HighObligationType.
    /// </summary>
    [JsonPropertyName("highObligationType")]
    public ObligationType HighObligationType
    {
      get => highObligationType ??= new();
      set => highObligationType = value;
    }

    /// <summary>
    /// A value of HighObligation.
    /// </summary>
    [JsonPropertyName("highObligation")]
    public Obligation HighObligation
    {
      get => highObligation ??= new();
      set => highObligation = value;
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

    private CashReceiptDetailStatus hardcodedAdjusted;
    private TextWorkArea compareInd;
    private TextWorkArea unprocCollAdjInd;
    private TextWorkArea unprocDebtAdjInd;
    private TextWorkArea unprocDebtInd;
    private TextWorkArea unprocCashRcptInd;
    private ObligationType lowObligationType;
    private Obligation lowObligation;
    private ObligationType highObligationType;
    private Obligation highObligation;
    private DateWorkArea null1;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of ExistingKeyOnlyCsePerson.
    /// </summary>
    [JsonPropertyName("existingKeyOnlyCsePerson")]
    public CsePerson ExistingKeyOnlyCsePerson
    {
      get => existingKeyOnlyCsePerson ??= new();
      set => existingKeyOnlyCsePerson = value;
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
    /// A value of ExistingKeyOnlyObligationType.
    /// </summary>
    [JsonPropertyName("existingKeyOnlyObligationType")]
    public ObligationType ExistingKeyOnlyObligationType
    {
      get => existingKeyOnlyObligationType ??= new();
      set => existingKeyOnlyObligationType = value;
    }

    /// <summary>
    /// A value of ExistingKeyOnlyObligation.
    /// </summary>
    [JsonPropertyName("existingKeyOnlyObligation")]
    public Obligation ExistingKeyOnlyObligation
    {
      get => existingKeyOnlyObligation ??= new();
      set => existingKeyOnlyObligation = value;
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
    /// A value of ExistingDebtAdjustment.
    /// </summary>
    [JsonPropertyName("existingDebtAdjustment")]
    public ObligationTransaction ExistingDebtAdjustment
    {
      get => existingDebtAdjustment ??= new();
      set => existingDebtAdjustment = value;
    }

    /// <summary>
    /// A value of ExistingCollection.
    /// </summary>
    [JsonPropertyName("existingCollection")]
    public Collection ExistingCollection
    {
      get => existingCollection ??= new();
      set => existingCollection = value;
    }

    /// <summary>
    /// A value of ExistingCashReceiptDetail.
    /// </summary>
    [JsonPropertyName("existingCashReceiptDetail")]
    public CashReceiptDetail ExistingCashReceiptDetail
    {
      get => existingCashReceiptDetail ??= new();
      set => existingCashReceiptDetail = value;
    }

    /// <summary>
    /// A value of ExistingKeyOnlyCashReceiptDetailStatHistory.
    /// </summary>
    [JsonPropertyName("existingKeyOnlyCashReceiptDetailStatHistory")]
    public CashReceiptDetailStatHistory ExistingKeyOnlyCashReceiptDetailStatHistory
      
    {
      get => existingKeyOnlyCashReceiptDetailStatHistory ??= new();
      set => existingKeyOnlyCashReceiptDetailStatHistory = value;
    }

    /// <summary>
    /// A value of ExistingKeyOnlyCashReceiptDetailStatus.
    /// </summary>
    [JsonPropertyName("existingKeyOnlyCashReceiptDetailStatus")]
    public CashReceiptDetailStatus ExistingKeyOnlyCashReceiptDetailStatus
    {
      get => existingKeyOnlyCashReceiptDetailStatus ??= new();
      set => existingKeyOnlyCashReceiptDetailStatus = value;
    }

    private CsePerson existingKeyOnlyCsePerson;
    private CsePersonAccount existingKeyOnlyObligor;
    private ObligationType existingKeyOnlyObligationType;
    private Obligation existingKeyOnlyObligation;
    private ObligationTransaction existingDebt;
    private ObligationTransaction existingDebtAdjustment;
    private Collection existingCollection;
    private CashReceiptDetail existingCashReceiptDetail;
    private CashReceiptDetailStatHistory existingKeyOnlyCashReceiptDetailStatHistory;
      
    private CashReceiptDetailStatus existingKeyOnlyCashReceiptDetailStatus;
  }
#endregion
}
