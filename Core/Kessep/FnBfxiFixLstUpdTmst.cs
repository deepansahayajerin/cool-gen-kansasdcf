// Program: FN_BFXI_FIX_LST_UPD_TMST, ID: 373027904, model: 746.
// Short name: SWEFFXIB
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_BFXI_FIX_LST_UPD_TMST.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Batch)]
public partial class FnBfxiFixLstUpdTmst: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_BFXI_FIX_LST_UPD_TMST program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnBfxiFixLstUpdTmst(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnBfxiFixLstUpdTmst.
  /// </summary>
  public FnBfxiFixLstUpdTmst(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    local.Tmp.LastUpdatedTmst = Now();

    foreach(var item in ReadCashReceiptDetail())
    {
      try
      {
        UpdateCashReceiptDetail();
        local.Tmp.LastUpdatedTmst =
          AddMicroseconds(local.Tmp.LastUpdatedTmst, 1);
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "FN0000_CASH_RCPT_DTL_NU_RB";

            return;
          case ErrorCode.PermittedValueViolation:
            ExitState = "FN0000_CASH_RCPT_DTL_PV_RB";

            return;
          case ErrorCode.DatabaseError:
            break;
          default:
            throw;
        }
      }
    }
  }

  private IEnumerable<bool> ReadCashReceiptDetail()
  {
    entities.CashReceiptDetail.Populated = false;

    return ReadEach("ReadCashReceiptDetail",
      (db, command) =>
      {
        db.SetNullableDateTime(
          command, "lastUpdatedTmst", AddMicroseconds(new DateTime(2001, 12, 15,
          19, 19, 38), 353382));
      },
      (db, reader) =>
      {
        entities.CashReceiptDetail.CrvIdentifier = db.GetInt32(reader, 0);
        entities.CashReceiptDetail.CstIdentifier = db.GetInt32(reader, 1);
        entities.CashReceiptDetail.CrtIdentifier = db.GetInt32(reader, 2);
        entities.CashReceiptDetail.SequentialIdentifier =
          db.GetInt32(reader, 3);
        entities.CashReceiptDetail.LastUpdatedTmst =
          db.GetNullableDateTime(reader, 4);
        entities.CashReceiptDetail.Populated = true;

        return true;
      });
  }

  private void UpdateCashReceiptDetail()
  {
    System.Diagnostics.Debug.Assert(entities.CashReceiptDetail.Populated);

    var lastUpdatedTmst = local.Tmp.LastUpdatedTmst;

    entities.CashReceiptDetail.Populated = false;
    Update("UpdateCashReceiptDetail",
      (db, command) =>
      {
        db.SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTmst);
        db.SetInt32(
          command, "crvIdentifier", entities.CashReceiptDetail.CrvIdentifier);
        db.SetInt32(
          command, "cstIdentifier", entities.CashReceiptDetail.CstIdentifier);
        db.SetInt32(
          command, "crtIdentifier", entities.CashReceiptDetail.CrtIdentifier);
        db.SetInt32(
          command, "crdId", entities.CashReceiptDetail.SequentialIdentifier);
      });

    entities.CashReceiptDetail.LastUpdatedTmst = lastUpdatedTmst;
    entities.CashReceiptDetail.Populated = true;
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
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
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
    public CashReceiptDetail Tmp
    {
      get => tmp ??= new();
      set => tmp = value;
    }

    private CashReceiptDetail tmp;
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
