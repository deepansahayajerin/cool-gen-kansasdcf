// Program: FN_DETERMINE_MNL_DIST_EXIST, ID: 372279910, model: 746.
// Short name: SWE02272
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_DETERMINE_MNL_DIST_EXIST.
/// </summary>
[Serializable]
public partial class FnDetermineMnlDistExist: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_DETERMINE_MNL_DIST_EXIST program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnDetermineMnlDistExist(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnDetermineMnlDistExist.
  /// </summary>
  public FnDetermineMnlDistExist(IContext context, Import import, Export export):
    
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    export.SuspendForMnlDistInst.Flag = "N";

    if (!IsEmpty(import.CashReceiptDetail.CourtOrderNumber))
    {
      // now we are looking at payments that will be suspended by court order 
      // number instead
      // of just person number.
      foreach(var item in ReadManualDistributionAuditObligation1())
      {
        foreach(var item1 in ReadDebtDetail())
        {
          local.Tmp.TotalCurrency += entities.ExistingDebtDetail.BalanceDueAmt +
            entities
            .ExistingDebtDetail.InterestBalanceDueAmt.GetValueOrDefault();
        }

        if (local.Tmp.TotalCurrency > 0)
        {
          export.SuspendForMnlDistInst.Flag = "Y";

          return;
        }

        if (ReadAccrualInstructions())
        {
          export.SuspendForMnlDistInst.Flag = "Y";

          return;
        }
      }
    }
    else
    {
      // this is the old way of checking by justr person number, it is still 
      // used for payments
      // that are posted by person number (FDSO, SDSO, etc)
      foreach(var item in ReadManualDistributionAuditObligation2())
      {
        foreach(var item1 in ReadDebtDetail())
        {
          local.Tmp.TotalCurrency += entities.ExistingDebtDetail.BalanceDueAmt +
            entities
            .ExistingDebtDetail.InterestBalanceDueAmt.GetValueOrDefault();
        }

        if (local.Tmp.TotalCurrency > 0)
        {
          export.SuspendForMnlDistInst.Flag = "Y";

          return;
        }

        if (ReadAccrualInstructions())
        {
          export.SuspendForMnlDistInst.Flag = "Y";

          return;
        }
      }
    }
  }

  private bool ReadAccrualInstructions()
  {
    System.Diagnostics.Debug.
      Assert(entities.ExistingKeyOnlyObligation.Populated);
    entities.ExistingAccrualInstructions.Populated = false;

    return Read("ReadAccrualInstructions",
      (db, command) =>
      {
        var date = Now().Date;

        db.SetInt32(
          command, "otyId", entities.ExistingKeyOnlyObligation.DtyGeneratedId);
        db.SetInt32(
          command, "obgGeneratedId",
          entities.ExistingKeyOnlyObligation.SystemGeneratedIdentifier);
        db.SetString(
          command, "cspNumber", entities.ExistingKeyOnlyObligation.CspNumber);
        db.SetString(
          command, "cpaType", entities.ExistingKeyOnlyObligation.CpaType);
        db.SetDate(command, "asOfDt", date);
      },
      (db, reader) =>
      {
        entities.ExistingAccrualInstructions.OtrType = db.GetString(reader, 0);
        entities.ExistingAccrualInstructions.OtyId = db.GetInt32(reader, 1);
        entities.ExistingAccrualInstructions.ObgGeneratedId =
          db.GetInt32(reader, 2);
        entities.ExistingAccrualInstructions.CspNumber =
          db.GetString(reader, 3);
        entities.ExistingAccrualInstructions.CpaType = db.GetString(reader, 4);
        entities.ExistingAccrualInstructions.OtrGeneratedId =
          db.GetInt32(reader, 5);
        entities.ExistingAccrualInstructions.AsOfDt = db.GetDate(reader, 6);
        entities.ExistingAccrualInstructions.DiscontinueDt =
          db.GetNullableDate(reader, 7);
        entities.ExistingAccrualInstructions.Populated = true;
        CheckValid<AccrualInstructions>("OtrType",
          entities.ExistingAccrualInstructions.OtrType);
        CheckValid<AccrualInstructions>("CpaType",
          entities.ExistingAccrualInstructions.CpaType);
      });
  }

  private IEnumerable<bool> ReadDebtDetail()
  {
    System.Diagnostics.Debug.
      Assert(entities.ExistingKeyOnlyObligation.Populated);
    entities.ExistingDebtDetail.Populated = false;

    return ReadEach("ReadDebtDetail",
      (db, command) =>
      {
        db.SetInt32(
          command, "otyType",
          entities.ExistingKeyOnlyObligation.DtyGeneratedId);
        db.SetInt32(
          command, "obgGeneratedId",
          entities.ExistingKeyOnlyObligation.SystemGeneratedIdentifier);
        db.SetString(
          command, "cspNumber", entities.ExistingKeyOnlyObligation.CspNumber);
        db.SetString(
          command, "cpaType", entities.ExistingKeyOnlyObligation.CpaType);
        db.SetNullableDate(
          command, "retiredDt", local.Null1.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ExistingDebtDetail.ObgGeneratedId = db.GetInt32(reader, 0);
        entities.ExistingDebtDetail.CspNumber = db.GetString(reader, 1);
        entities.ExistingDebtDetail.CpaType = db.GetString(reader, 2);
        entities.ExistingDebtDetail.OtrGeneratedId = db.GetInt32(reader, 3);
        entities.ExistingDebtDetail.OtyType = db.GetInt32(reader, 4);
        entities.ExistingDebtDetail.OtrType = db.GetString(reader, 5);
        entities.ExistingDebtDetail.BalanceDueAmt = db.GetDecimal(reader, 6);
        entities.ExistingDebtDetail.InterestBalanceDueAmt =
          db.GetNullableDecimal(reader, 7);
        entities.ExistingDebtDetail.RetiredDt = db.GetNullableDate(reader, 8);
        entities.ExistingDebtDetail.Populated = true;
        CheckValid<DebtDetail>("CpaType", entities.ExistingDebtDetail.CpaType);
        CheckValid<DebtDetail>("OtrType", entities.ExistingDebtDetail.OtrType);

        return true;
      });
  }

  private IEnumerable<bool> ReadManualDistributionAuditObligation1()
  {
    entities.ExistingKeyOnlyObligation.Populated = false;
    entities.ExistingManualDistributionAudit.Populated = false;

    return ReadEach("ReadManualDistributionAuditObligation1",
      (db, command) =>
      {
        var date = Now().Date;

        db.SetString(
          command, "cspNumber",
          import.CashReceiptDetail.ObligorPersonNumber ?? "");
        db.SetDate(command, "effectiveDt", date);
        db.SetNullableString(
          command, "standardNo", import.CashReceiptDetail.CourtOrderNumber ?? ""
          );
      },
      (db, reader) =>
      {
        entities.ExistingManualDistributionAudit.OtyType =
          db.GetInt32(reader, 0);
        entities.ExistingKeyOnlyObligation.DtyGeneratedId =
          db.GetInt32(reader, 0);
        entities.ExistingManualDistributionAudit.ObgGeneratedId =
          db.GetInt32(reader, 1);
        entities.ExistingKeyOnlyObligation.SystemGeneratedIdentifier =
          db.GetInt32(reader, 1);
        entities.ExistingManualDistributionAudit.CspNumber =
          db.GetString(reader, 2);
        entities.ExistingKeyOnlyObligation.CspNumber = db.GetString(reader, 2);
        entities.ExistingManualDistributionAudit.CpaType =
          db.GetString(reader, 3);
        entities.ExistingKeyOnlyObligation.CpaType = db.GetString(reader, 3);
        entities.ExistingManualDistributionAudit.EffectiveDt =
          db.GetDate(reader, 4);
        entities.ExistingManualDistributionAudit.DiscontinueDt =
          db.GetNullableDate(reader, 5);
        entities.ExistingKeyOnlyObligation.LgaId =
          db.GetNullableInt32(reader, 6);
        entities.ExistingKeyOnlyObligation.Populated = true;
        entities.ExistingManualDistributionAudit.Populated = true;
        CheckValid<ManualDistributionAudit>("CpaType",
          entities.ExistingManualDistributionAudit.CpaType);
        CheckValid<Obligation>("CpaType",
          entities.ExistingKeyOnlyObligation.CpaType);

        return true;
      });
  }

  private IEnumerable<bool> ReadManualDistributionAuditObligation2()
  {
    entities.ExistingKeyOnlyObligation.Populated = false;
    entities.ExistingManualDistributionAudit.Populated = false;

    return ReadEach("ReadManualDistributionAuditObligation2",
      (db, command) =>
      {
        var date = Now().Date;

        db.SetString(
          command, "cspNumber",
          import.CashReceiptDetail.ObligorPersonNumber ?? "");
        db.SetDate(command, "effectiveDt", date);
      },
      (db, reader) =>
      {
        entities.ExistingManualDistributionAudit.OtyType =
          db.GetInt32(reader, 0);
        entities.ExistingKeyOnlyObligation.DtyGeneratedId =
          db.GetInt32(reader, 0);
        entities.ExistingManualDistributionAudit.ObgGeneratedId =
          db.GetInt32(reader, 1);
        entities.ExistingKeyOnlyObligation.SystemGeneratedIdentifier =
          db.GetInt32(reader, 1);
        entities.ExistingManualDistributionAudit.CspNumber =
          db.GetString(reader, 2);
        entities.ExistingKeyOnlyObligation.CspNumber = db.GetString(reader, 2);
        entities.ExistingManualDistributionAudit.CpaType =
          db.GetString(reader, 3);
        entities.ExistingKeyOnlyObligation.CpaType = db.GetString(reader, 3);
        entities.ExistingManualDistributionAudit.EffectiveDt =
          db.GetDate(reader, 4);
        entities.ExistingManualDistributionAudit.DiscontinueDt =
          db.GetNullableDate(reader, 5);
        entities.ExistingKeyOnlyObligation.LgaId =
          db.GetNullableInt32(reader, 6);
        entities.ExistingKeyOnlyObligation.Populated = true;
        entities.ExistingManualDistributionAudit.Populated = true;
        CheckValid<ManualDistributionAudit>("CpaType",
          entities.ExistingManualDistributionAudit.CpaType);
        CheckValid<Obligation>("CpaType",
          entities.ExistingKeyOnlyObligation.CpaType);

        return true;
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
    /// A value of CashReceiptDetail.
    /// </summary>
    [JsonPropertyName("cashReceiptDetail")]
    public CashReceiptDetail CashReceiptDetail
    {
      get => cashReceiptDetail ??= new();
      set => cashReceiptDetail = value;
    }

    private CashReceiptDetail cashReceiptDetail;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of SuspendForMnlDistInst.
    /// </summary>
    [JsonPropertyName("suspendForMnlDistInst")]
    public Common SuspendForMnlDistInst
    {
      get => suspendForMnlDistInst ??= new();
      set => suspendForMnlDistInst = value;
    }

    private Common suspendForMnlDistInst;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of Tmp.
    /// </summary>
    [JsonPropertyName("tmp")]
    public Common Tmp
    {
      get => tmp ??= new();
      set => tmp = value;
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

    private Common tmp;
    private DateWorkArea null1;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
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
    /// A value of ExistingObligorKeyOnly.
    /// </summary>
    [JsonPropertyName("existingObligorKeyOnly")]
    public CsePerson ExistingObligorKeyOnly
    {
      get => existingObligorKeyOnly ??= new();
      set => existingObligorKeyOnly = value;
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
    /// A value of ExistingKeyOnlyObligation.
    /// </summary>
    [JsonPropertyName("existingKeyOnlyObligation")]
    public Obligation ExistingKeyOnlyObligation
    {
      get => existingKeyOnlyObligation ??= new();
      set => existingKeyOnlyObligation = value;
    }

    /// <summary>
    /// A value of ExistingManualDistributionAudit.
    /// </summary>
    [JsonPropertyName("existingManualDistributionAudit")]
    public ManualDistributionAudit ExistingManualDistributionAudit
    {
      get => existingManualDistributionAudit ??= new();
      set => existingManualDistributionAudit = value;
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
    /// A value of ExistingAccrualInstructions.
    /// </summary>
    [JsonPropertyName("existingAccrualInstructions")]
    public AccrualInstructions ExistingAccrualInstructions
    {
      get => existingAccrualInstructions ??= new();
      set => existingAccrualInstructions = value;
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

    private LegalAction legalAction;
    private CsePerson existingObligorKeyOnly;
    private CsePersonAccount existingKeyOnlyObligor;
    private Obligation existingKeyOnlyObligation;
    private ManualDistributionAudit existingManualDistributionAudit;
    private ObligationTransaction existingDebt;
    private AccrualInstructions existingAccrualInstructions;
    private DebtDetail existingDebtDetail;
  }
#endregion
}
