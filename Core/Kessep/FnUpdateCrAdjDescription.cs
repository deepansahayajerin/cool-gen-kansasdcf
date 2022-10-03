// Program: FN_UPDATE_CR_ADJ_DESCRIPTION, ID: 372342875, model: 746.
// Short name: SWE02367
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_UPDATE_CR_ADJ_DESCRIPTION.
/// </summary>
[Serializable]
public partial class FnUpdateCrAdjDescription: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_UPDATE_CR_ADJ_DESCRIPTION program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnUpdateCrAdjDescription(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnUpdateCrAdjDescription.
  /// </summary>
  public FnUpdateCrAdjDescription(IContext context, Import import, Export export)
    :
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ---------------------------------------------------------------
    //                          Change Log
    // ---------------------------------------------------------------
    // Date      Developer		Description
    // ---------------------------------------------------------------
    // 06/08/99  J. Katz		Analyzed READ statements and
    // 				changed read property to Select
    // 				Only where appropriate.
    // ---------------------------------------------------------------
    if (ReadCashReceiptBalanceAdjustment())
    {
      try
      {
        UpdateCashReceiptBalanceAdjustment();
        ExitState = "FN0000_UPDATE_SUCCESSFUL";
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "FN0032_CASH_RCPT_BAL_ADJ_NU";

            break;
          case ErrorCode.PermittedValueViolation:
            ExitState = "FN0033_CASH_RCPT_BAL_ADJ_PV";

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
      ExitState = "FN0031_CASH_RCPT_BAL_ADJ_NF";
    }
  }

  private bool ReadCashReceiptBalanceAdjustment()
  {
    entities.CashReceiptBalanceAdjustment.Populated = false;

    return Read("ReadCashReceiptBalanceAdjustment",
      (db, command) =>
      {
        db.SetDateTime(
          command, "createdTimestamp",
          import.CashReceiptBalanceAdjustment.CreatedTimestamp.
            GetValueOrDefault());
        db.SetInt32(
          command, "crrIdentifier",
          import.CashReceiptRlnRsn.SystemGeneratedIdentifier);
        db.
          SetInt32(command, "cashReceiptId1", import.Increase.SequentialNumber);
          
        db.
          SetInt32(command, "cashReceiptId2", import.Decrease.SequentialNumber);
          
      },
      (db, reader) =>
      {
        entities.CashReceiptBalanceAdjustment.CrtIdentifier =
          db.GetInt32(reader, 0);
        entities.CashReceiptBalanceAdjustment.CstIdentifier =
          db.GetInt32(reader, 1);
        entities.CashReceiptBalanceAdjustment.CrvIdentifier =
          db.GetInt32(reader, 2);
        entities.CashReceiptBalanceAdjustment.CrtIIdentifier =
          db.GetInt32(reader, 3);
        entities.CashReceiptBalanceAdjustment.CstIIdentifier =
          db.GetInt32(reader, 4);
        entities.CashReceiptBalanceAdjustment.CrvIIdentifier =
          db.GetInt32(reader, 5);
        entities.CashReceiptBalanceAdjustment.CrrIdentifier =
          db.GetInt32(reader, 6);
        entities.CashReceiptBalanceAdjustment.CreatedTimestamp =
          db.GetDateTime(reader, 7);
        entities.CashReceiptBalanceAdjustment.Description =
          db.GetNullableString(reader, 8);
        entities.CashReceiptBalanceAdjustment.Populated = true;
      });
  }

  private void UpdateCashReceiptBalanceAdjustment()
  {
    System.Diagnostics.Debug.Assert(
      entities.CashReceiptBalanceAdjustment.Populated);

    var description = import.CashReceiptBalanceAdjustment.Description ?? "";

    entities.CashReceiptBalanceAdjustment.Populated = false;
    Update("UpdateCashReceiptBalanceAdjustment",
      (db, command) =>
      {
        db.SetNullableString(command, "description", description);
        db.SetInt32(
          command, "crtIdentifier",
          entities.CashReceiptBalanceAdjustment.CrtIdentifier);
        db.SetInt32(
          command, "cstIdentifier",
          entities.CashReceiptBalanceAdjustment.CstIdentifier);
        db.SetInt32(
          command, "crvIdentifier",
          entities.CashReceiptBalanceAdjustment.CrvIdentifier);
        db.SetInt32(
          command, "crtIIdentifier",
          entities.CashReceiptBalanceAdjustment.CrtIIdentifier);
        db.SetInt32(
          command, "cstIIdentifier",
          entities.CashReceiptBalanceAdjustment.CstIIdentifier);
        db.SetInt32(
          command, "crvIIdentifier",
          entities.CashReceiptBalanceAdjustment.CrvIIdentifier);
        db.SetInt32(
          command, "crrIdentifier",
          entities.CashReceiptBalanceAdjustment.CrrIdentifier);
        db.SetDateTime(
          command, "createdTimestamp",
          entities.CashReceiptBalanceAdjustment.CreatedTimestamp.
            GetValueOrDefault());
      });

    entities.CashReceiptBalanceAdjustment.Description = description;
    entities.CashReceiptBalanceAdjustment.Populated = true;
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
    /// A value of CashReceiptRlnRsn.
    /// </summary>
    [JsonPropertyName("cashReceiptRlnRsn")]
    public CashReceiptRlnRsn CashReceiptRlnRsn
    {
      get => cashReceiptRlnRsn ??= new();
      set => cashReceiptRlnRsn = value;
    }

    /// <summary>
    /// A value of CashReceiptBalanceAdjustment.
    /// </summary>
    [JsonPropertyName("cashReceiptBalanceAdjustment")]
    public CashReceiptBalanceAdjustment CashReceiptBalanceAdjustment
    {
      get => cashReceiptBalanceAdjustment ??= new();
      set => cashReceiptBalanceAdjustment = value;
    }

    /// <summary>
    /// A value of Increase.
    /// </summary>
    [JsonPropertyName("increase")]
    public CashReceipt Increase
    {
      get => increase ??= new();
      set => increase = value;
    }

    /// <summary>
    /// A value of Decrease.
    /// </summary>
    [JsonPropertyName("decrease")]
    public CashReceipt Decrease
    {
      get => decrease ??= new();
      set => decrease = value;
    }

    private CashReceiptRlnRsn cashReceiptRlnRsn;
    private CashReceiptBalanceAdjustment cashReceiptBalanceAdjustment;
    private CashReceipt increase;
    private CashReceipt decrease;
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
    /// A value of CashReceiptBalanceAdjustment.
    /// </summary>
    [JsonPropertyName("cashReceiptBalanceAdjustment")]
    public CashReceiptBalanceAdjustment CashReceiptBalanceAdjustment
    {
      get => cashReceiptBalanceAdjustment ??= new();
      set => cashReceiptBalanceAdjustment = value;
    }

    /// <summary>
    /// A value of CashReceiptRlnRsn.
    /// </summary>
    [JsonPropertyName("cashReceiptRlnRsn")]
    public CashReceiptRlnRsn CashReceiptRlnRsn
    {
      get => cashReceiptRlnRsn ??= new();
      set => cashReceiptRlnRsn = value;
    }

    /// <summary>
    /// A value of Increase.
    /// </summary>
    [JsonPropertyName("increase")]
    public CashReceipt Increase
    {
      get => increase ??= new();
      set => increase = value;
    }

    /// <summary>
    /// A value of Decrease.
    /// </summary>
    [JsonPropertyName("decrease")]
    public CashReceipt Decrease
    {
      get => decrease ??= new();
      set => decrease = value;
    }

    private CashReceiptBalanceAdjustment cashReceiptBalanceAdjustment;
    private CashReceiptRlnRsn cashReceiptRlnRsn;
    private CashReceipt increase;
    private CashReceipt decrease;
  }
#endregion
}
