// Program: FN_COMPUTE_UNDISTRIBUTED_AMOUNT, ID: 371738800, model: 746.
// Short name: SWE00327
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
/// A program: FN_COMPUTE_UNDISTRIBUTED_AMOUNT.
/// </para>
/// <para>
/// RESP: FINANCE
/// This will total all Cash Receipt Detail amounts remaining to be distributed 
/// for a specific CSE Person that can be identified by CSE Person Number.
/// </para>
/// </summary>
[Serializable]
public partial class FnComputeUndistributedAmount: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_COMPUTE_UNDISTRIBUTED_AMOUNT program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnComputeUndistributedAmount(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnComputeUndistributedAmount.
  /// </summary>
  public FnComputeUndistributedAmount(IContext context, Import import,
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
    // ---------------------------------------------
    // Date	By	IDCR#	Description
    // ??????	????????	Initial code
    // 092397	govind		Modified to consider the CRDs with SSN and/or Court Order 
    // Number pertaining to that person. Added check to ignore adjusted CRDs
    // 9/25/98    E. Parker	Changed logic to look for ssn > zeros.  Also changed
    // court order read to avoid ssn <= zeros.
    // 10/14/99   E. Parker 	At some point, all logic was removed to check for 
    // Cash Receipt Details without CSE Person Number.  Added logic to look for
    // Cash Receipt Details without CSE Person Number, but with Social Security
    // Number.
    // ---------------------------------------------
    UseFnHardcodedCashReceipting();
    local.Max.Date = UseCabSetMaximumDiscontinueDate();

    foreach(var item in ReadCashReceiptDetailCashReceiptDetailStatus2())
    {
      if (entities.CashReceiptDetailStatus.SystemGeneratedIdentifier == local
        .HardcodedAdjusted.SystemGeneratedIdentifier)
      {
        continue;
      }

      export.UndistAmount.TotalCurrency += entities.CashReceiptDetail.
        CollectionAmount - entities
        .CashReceiptDetail.RefundedAmount.GetValueOrDefault() - entities
        .CashReceiptDetail.DistributedAmount.GetValueOrDefault();

      foreach(var item1 in ReadCashReceiptDetailBalanceAdjCashReceiptDetail())
      {
        export.UndistAmount.TotalCurrency -= entities.Adjustment.
          CollectionAmount;
      }
    }

    local.CsePersonsWorkSet.Number = import.CsePerson.Number;
    UseSiReadCsePerson();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    foreach(var item in ReadCashReceiptDetailCashReceiptDetailStatus1())
    {
      if (entities.CashReceiptDetailStatus.SystemGeneratedIdentifier == local
        .HardcodedAdjusted.SystemGeneratedIdentifier)
      {
        continue;
      }

      export.UndistAmount.TotalCurrency += entities.CashReceiptDetail.
        CollectionAmount - entities
        .CashReceiptDetail.RefundedAmount.GetValueOrDefault() - entities
        .CashReceiptDetail.DistributedAmount.GetValueOrDefault();

      foreach(var item1 in ReadCashReceiptDetailBalanceAdjCashReceiptDetail())
      {
        export.UndistAmount.TotalCurrency -= entities.Adjustment.
          CollectionAmount;
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

  private void UseFnHardcodedCashReceipting()
  {
    var useImport = new FnHardcodedCashReceipting.Import();
    var useExport = new FnHardcodedCashReceipting.Export();

    Call(FnHardcodedCashReceipting.Execute, useImport, useExport);

    local.HardcodedAdjusted.SystemGeneratedIdentifier =
      useExport.CrdsSystemId.CrdsIdAdjusted.SystemGeneratedIdentifier;
  }

  private void UseSiReadCsePerson()
  {
    var useImport = new SiReadCsePerson.Import();
    var useExport = new SiReadCsePerson.Export();

    useImport.CsePersonsWorkSet.Number = local.CsePersonsWorkSet.Number;

    Call(SiReadCsePerson.Execute, useImport, useExport);

    local.CsePersonsWorkSet.Assign(useExport.CsePersonsWorkSet);
  }

  private IEnumerable<bool> ReadCashReceiptDetailBalanceAdjCashReceiptDetail()
  {
    System.Diagnostics.Debug.Assert(entities.CashReceiptDetail.Populated);
    entities.Adjustment.Populated = false;
    entities.CashReceiptDetailBalanceAdj.Populated = false;

    return ReadEach("ReadCashReceiptDetailBalanceAdjCashReceiptDetail",
      (db, command) =>
      {
        db.SetInt32(
          command, "crdIdentifier",
          entities.CashReceiptDetail.SequentialIdentifier);
        db.SetInt32(
          command, "crvIdentifier", entities.CashReceiptDetail.CrvIdentifier);
        db.SetInt32(
          command, "cstIdentifier", entities.CashReceiptDetail.CstIdentifier);
        db.SetInt32(
          command, "crtIdentifier", entities.CashReceiptDetail.CrtIdentifier);
      },
      (db, reader) =>
      {
        entities.CashReceiptDetailBalanceAdj.CrdIdentifier =
          db.GetInt32(reader, 0);
        entities.CashReceiptDetailBalanceAdj.CrvIdentifier =
          db.GetInt32(reader, 1);
        entities.CashReceiptDetailBalanceAdj.CstIdentifier =
          db.GetInt32(reader, 2);
        entities.CashReceiptDetailBalanceAdj.CrtIdentifier =
          db.GetInt32(reader, 3);
        entities.CashReceiptDetailBalanceAdj.CrdSIdentifier =
          db.GetInt32(reader, 4);
        entities.Adjustment.SequentialIdentifier = db.GetInt32(reader, 4);
        entities.CashReceiptDetailBalanceAdj.CrvSIdentifier =
          db.GetInt32(reader, 5);
        entities.Adjustment.CrvIdentifier = db.GetInt32(reader, 5);
        entities.CashReceiptDetailBalanceAdj.CstSIdentifier =
          db.GetInt32(reader, 6);
        entities.Adjustment.CstIdentifier = db.GetInt32(reader, 6);
        entities.CashReceiptDetailBalanceAdj.CrtSIdentifier =
          db.GetInt32(reader, 7);
        entities.Adjustment.CrtIdentifier = db.GetInt32(reader, 7);
        entities.CashReceiptDetailBalanceAdj.CrnIdentifier =
          db.GetInt32(reader, 8);
        entities.CashReceiptDetailBalanceAdj.CreatedTimestamp =
          db.GetDateTime(reader, 9);
        entities.CashReceiptDetailBalanceAdj.Description =
          db.GetNullableString(reader, 10);
        entities.Adjustment.AdjustmentInd = db.GetNullableString(reader, 11);
        entities.Adjustment.CollectionAmount = db.GetDecimal(reader, 12);
        entities.Adjustment.Populated = true;
        entities.CashReceiptDetailBalanceAdj.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadCashReceiptDetailCashReceiptDetailStatus1()
  {
    entities.CashReceiptDetailStatus.Populated = false;
    entities.CashReceiptDetail.Populated = false;

    return ReadEach("ReadCashReceiptDetailCashReceiptDetailStatus1",
      (db, command) =>
      {
        db.SetNullableString(command, "oblgorSsn", local.CsePersonsWorkSet.Ssn);
        db.SetNullableDate(
          command, "discontinueDate", local.Max.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CashReceiptDetail.CrvIdentifier = db.GetInt32(reader, 0);
        entities.CashReceiptDetail.CstIdentifier = db.GetInt32(reader, 1);
        entities.CashReceiptDetail.CrtIdentifier = db.GetInt32(reader, 2);
        entities.CashReceiptDetail.SequentialIdentifier =
          db.GetInt32(reader, 3);
        entities.CashReceiptDetail.AdjustmentInd =
          db.GetNullableString(reader, 4);
        entities.CashReceiptDetail.CourtOrderNumber =
          db.GetNullableString(reader, 5);
        entities.CashReceiptDetail.CollectionAmount = db.GetDecimal(reader, 6);
        entities.CashReceiptDetail.MultiPayor = db.GetNullableString(reader, 7);
        entities.CashReceiptDetail.ObligorPersonNumber =
          db.GetNullableString(reader, 8);
        entities.CashReceiptDetail.ObligorSocialSecurityNumber =
          db.GetNullableString(reader, 9);
        entities.CashReceiptDetail.RefundedAmount =
          db.GetNullableDecimal(reader, 10);
        entities.CashReceiptDetail.DistributedAmount =
          db.GetNullableDecimal(reader, 11);
        entities.CashReceiptDetail.CollectionAmtFullyAppliedInd =
          db.GetNullableString(reader, 12);
        entities.CashReceiptDetailStatus.SystemGeneratedIdentifier =
          db.GetInt32(reader, 13);
        entities.CashReceiptDetailStatus.Populated = true;
        entities.CashReceiptDetail.Populated = true;
        CheckValid<CashReceiptDetail>("MultiPayor",
          entities.CashReceiptDetail.MultiPayor);
        CheckValid<CashReceiptDetail>("CollectionAmtFullyAppliedInd",
          entities.CashReceiptDetail.CollectionAmtFullyAppliedInd);

        return true;
      });
  }

  private IEnumerable<bool> ReadCashReceiptDetailCashReceiptDetailStatus2()
  {
    entities.CashReceiptDetailStatus.Populated = false;
    entities.CashReceiptDetail.Populated = false;

    return ReadEach("ReadCashReceiptDetailCashReceiptDetailStatus2",
      (db, command) =>
      {
        db.SetNullableString(command, "oblgorPrsnNbr", import.CsePerson.Number);
        db.SetNullableDate(
          command, "discontinueDate", local.Max.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CashReceiptDetail.CrvIdentifier = db.GetInt32(reader, 0);
        entities.CashReceiptDetail.CstIdentifier = db.GetInt32(reader, 1);
        entities.CashReceiptDetail.CrtIdentifier = db.GetInt32(reader, 2);
        entities.CashReceiptDetail.SequentialIdentifier =
          db.GetInt32(reader, 3);
        entities.CashReceiptDetail.AdjustmentInd =
          db.GetNullableString(reader, 4);
        entities.CashReceiptDetail.CourtOrderNumber =
          db.GetNullableString(reader, 5);
        entities.CashReceiptDetail.CollectionAmount = db.GetDecimal(reader, 6);
        entities.CashReceiptDetail.MultiPayor = db.GetNullableString(reader, 7);
        entities.CashReceiptDetail.ObligorPersonNumber =
          db.GetNullableString(reader, 8);
        entities.CashReceiptDetail.ObligorSocialSecurityNumber =
          db.GetNullableString(reader, 9);
        entities.CashReceiptDetail.RefundedAmount =
          db.GetNullableDecimal(reader, 10);
        entities.CashReceiptDetail.DistributedAmount =
          db.GetNullableDecimal(reader, 11);
        entities.CashReceiptDetail.CollectionAmtFullyAppliedInd =
          db.GetNullableString(reader, 12);
        entities.CashReceiptDetailStatus.SystemGeneratedIdentifier =
          db.GetInt32(reader, 13);
        entities.CashReceiptDetailStatus.Populated = true;
        entities.CashReceiptDetail.Populated = true;
        CheckValid<CashReceiptDetail>("MultiPayor",
          entities.CashReceiptDetail.MultiPayor);
        CheckValid<CashReceiptDetail>("CollectionAmtFullyAppliedInd",
          entities.CashReceiptDetail.CollectionAmtFullyAppliedInd);

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
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
    }

    private CsePerson csePerson;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of UndistAmount.
    /// </summary>
    [JsonPropertyName("undistAmount")]
    public Common UndistAmount
    {
      get => undistAmount ??= new();
      set => undistAmount = value;
    }

    private Common undistAmount;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
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
    /// A value of HardcodedAdjusted.
    /// </summary>
    [JsonPropertyName("hardcodedAdjusted")]
    public CashReceiptDetailStatus HardcodedAdjusted
    {
      get => hardcodedAdjusted ??= new();
      set => hardcodedAdjusted = value;
    }

    /// <summary>
    /// A value of Adjustment.
    /// </summary>
    [JsonPropertyName("adjustment")]
    public CashReceiptDetail Adjustment
    {
      get => adjustment ??= new();
      set => adjustment = value;
    }

    /// <summary>
    /// A value of CsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("csePersonsWorkSet")]
    public CsePersonsWorkSet CsePersonsWorkSet
    {
      get => csePersonsWorkSet ??= new();
      set => csePersonsWorkSet = value;
    }

    private DateWorkArea max;
    private CashReceiptDetailStatus hardcodedAdjusted;
    private CashReceiptDetail adjustment;
    private CsePersonsWorkSet csePersonsWorkSet;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of CashReceiptDetailStatHistory.
    /// </summary>
    [JsonPropertyName("cashReceiptDetailStatHistory")]
    public CashReceiptDetailStatHistory CashReceiptDetailStatHistory
    {
      get => cashReceiptDetailStatHistory ??= new();
      set => cashReceiptDetailStatHistory = value;
    }

    /// <summary>
    /// A value of CashReceiptDetailStatus.
    /// </summary>
    [JsonPropertyName("cashReceiptDetailStatus")]
    public CashReceiptDetailStatus CashReceiptDetailStatus
    {
      get => cashReceiptDetailStatus ??= new();
      set => cashReceiptDetailStatus = value;
    }

    /// <summary>
    /// A value of Adjustment.
    /// </summary>
    [JsonPropertyName("adjustment")]
    public CashReceiptDetail Adjustment
    {
      get => adjustment ??= new();
      set => adjustment = value;
    }

    /// <summary>
    /// A value of CashReceiptDetailBalanceAdj.
    /// </summary>
    [JsonPropertyName("cashReceiptDetailBalanceAdj")]
    public CashReceiptDetailBalanceAdj CashReceiptDetailBalanceAdj
    {
      get => cashReceiptDetailBalanceAdj ??= new();
      set => cashReceiptDetailBalanceAdj = value;
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
    /// A value of Obligor.
    /// </summary>
    [JsonPropertyName("obligor")]
    public CsePersonAccount Obligor
    {
      get => obligor ??= new();
      set => obligor = value;
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
    /// A value of CashReceiptDetail.
    /// </summary>
    [JsonPropertyName("cashReceiptDetail")]
    public CashReceiptDetail CashReceiptDetail
    {
      get => cashReceiptDetail ??= new();
      set => cashReceiptDetail = value;
    }

    /// <summary>
    /// A value of DelMe.
    /// </summary>
    [JsonPropertyName("delMe")]
    public LegalAction DelMe
    {
      get => delMe ??= new();
      set => delMe = value;
    }

    private CashReceiptDetailStatHistory cashReceiptDetailStatHistory;
    private CashReceiptDetailStatus cashReceiptDetailStatus;
    private CashReceiptDetail adjustment;
    private CashReceiptDetailBalanceAdj cashReceiptDetailBalanceAdj;
    private CsePerson csePerson;
    private CsePersonAccount obligor;
    private Obligation obligation;
    private CashReceiptDetail cashReceiptDetail;
    private LegalAction delMe;
  }
#endregion
}
