// Program: FN_UPDATE_CR_DETAIL_VIA_COLL, ID: 372258434, model: 746.
// Short name: SWE00640
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: FN_UPDATE_CR_DETAIL_VIA_COLL.
/// </para>
/// <para>
/// RESP:  FINANCE
/// DESC:  This action block updates the Cash Receipt Detail for a given 
/// collection.  The persistent collection allows the cash receipt detail to be
/// updated without explicitly knowing all of the associated cash receipt
/// identifiers.
/// </para>
/// </summary>
[Serializable]
public partial class FnUpdateCrDetailViaColl: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_UPDATE_CR_DETAIL_VIA_COLL program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnUpdateCrDetailViaColl(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnUpdateCrDetailViaColl.
  /// </summary>
  public FnUpdateCrDetailViaColl(IContext context, Import import, Export export):
    
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ***************************************************
    // Every initial development and change to that
    // development needs to be documented.
    // ***************************************************
    // *****************************************************************************************************
    // DATE      DEVELOPER NAME          REQUEST #  DESCRIPTION
    // --------  ----------------------  ---------  
    // --------------------------------------------------------
    // 02/14/96  Bryan Fristrup - MTW               Initial Development
    // 09/02/97  Govind     Fixed to expect the new amount instead of the +/- 
    // change in Distributed Amount
    // 4/7/99 - Bud Adams  -  The entire CRD entity type was in the
    //   definition of all the views.  Starved them.
    //   Deleted export view of CRD and 'count' views.  No value in
    //   any of them.
    // *****************************************************************************************************
    // =================================================
    // PR# 75768: 9/30/99 - bud adams  -  CURRENT_TIMESTAMP
    //   function has to be used here because (for some reason)
    //   there is a process in KESSEP that is using this attribute as
    //   if it's the unique identifier, even though it is not.
    // =================================================
    if (ReadCashReceiptDetail())
    {
      try
      {
        UpdateCashReceiptDetail();
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "FN0054_CASH_RCPT_DTL_NU";

            break;
          case ErrorCode.PermittedValueViolation:
            ExitState = "FN0056_CASH_RCPT_DTL_PV";

            break;
          case ErrorCode.DatabaseError:
            break;
          default:
            throw;
        }
      }
    }
    else
    {
      ExitState = "FN0052_CASH_RCPT_DTL_NF";
    }
  }

  private bool ReadCashReceiptDetail()
  {
    System.Diagnostics.Debug.Assert(import.Persistent.Populated);
    entities.CashReceiptDetail.Populated = false;

    return Read("ReadCashReceiptDetail",
      (db, command) =>
      {
        db.SetInt32(command, "crdId", import.Persistent.CrdId);
        db.SetInt32(command, "crvIdentifier", import.Persistent.CrvId);
        db.SetInt32(command, "cstIdentifier", import.Persistent.CstId);
        db.SetInt32(command, "crtIdentifier", import.Persistent.CrtType);
      },
      (db, reader) =>
      {
        entities.CashReceiptDetail.CrvIdentifier = db.GetInt32(reader, 0);
        entities.CashReceiptDetail.CstIdentifier = db.GetInt32(reader, 1);
        entities.CashReceiptDetail.CrtIdentifier = db.GetInt32(reader, 2);
        entities.CashReceiptDetail.SequentialIdentifier =
          db.GetInt32(reader, 3);
        entities.CashReceiptDetail.LastUpdatedBy =
          db.GetNullableString(reader, 4);
        entities.CashReceiptDetail.LastUpdatedTmst =
          db.GetNullableDateTime(reader, 5);
        entities.CashReceiptDetail.DistributedAmount =
          db.GetNullableDecimal(reader, 6);
        entities.CashReceiptDetail.CollectionAmtFullyAppliedInd =
          db.GetNullableString(reader, 7);
        entities.CashReceiptDetail.Populated = true;
        CheckValid<CashReceiptDetail>("CollectionAmtFullyAppliedInd",
          entities.CashReceiptDetail.CollectionAmtFullyAppliedInd);
      });
  }

  private void UpdateCashReceiptDetail()
  {
    System.Diagnostics.Debug.Assert(entities.CashReceiptDetail.Populated);

    var lastUpdatedBy = import.CashReceiptDetail.LastUpdatedBy ?? "";
    var lastUpdatedTmst = Now();
    var distributedAmount =
      import.CashReceiptDetail.DistributedAmount.GetValueOrDefault();
    var collectionAmtFullyAppliedInd =
      import.CashReceiptDetail.CollectionAmtFullyAppliedInd ?? "";

    CheckValid<CashReceiptDetail>("CollectionAmtFullyAppliedInd",
      collectionAmtFullyAppliedInd);
    entities.CashReceiptDetail.Populated = false;
    Update("UpdateCashReceiptDetail",
      (db, command) =>
      {
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTmst);
        db.SetNullableDecimal(command, "distributedAmt", distributedAmount);
        db.SetNullableString(
          command, "collamtApplInd", collectionAmtFullyAppliedInd);
        db.SetInt32(
          command, "crvIdentifier", entities.CashReceiptDetail.CrvIdentifier);
        db.SetInt32(
          command, "cstIdentifier", entities.CashReceiptDetail.CstIdentifier);
        db.SetInt32(
          command, "crtIdentifier", entities.CashReceiptDetail.CrtIdentifier);
        db.SetInt32(
          command, "crdId", entities.CashReceiptDetail.SequentialIdentifier);
      });

    entities.CashReceiptDetail.LastUpdatedBy = lastUpdatedBy;
    entities.CashReceiptDetail.LastUpdatedTmst = lastUpdatedTmst;
    entities.CashReceiptDetail.DistributedAmount = distributedAmount;
    entities.CashReceiptDetail.CollectionAmtFullyAppliedInd =
      collectionAmtFullyAppliedInd;
    entities.CashReceiptDetail.Populated = true;
  }
#endregion

#region Parameters.
  protected readonly Import import;
  protected readonly Export export;
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

    /// <summary>
    /// A value of CashReceiptDetail.
    /// </summary>
    [JsonPropertyName("cashReceiptDetail")]
    public CashReceiptDetail CashReceiptDetail
    {
      get => cashReceiptDetail ??= new();
      set => cashReceiptDetail = value;
    }

    private Collection persistent;
    private CashReceiptDetail cashReceiptDetail;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
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
#endregion
}
