// Program: FIX_DISCONTINUE_DATES, ID: 372895580, model: 746.
// Short name: SWEXBUDP
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FIX_DISCONTINUE_DATES.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Batch)]
public partial class FixDiscontinueDates: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FIX_DISCONTINUE_DATES program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FixDiscontinueDates(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FixDiscontinueDates.
  /// </summary>
  public FixDiscontinueDates(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    foreach(var item in ReadObligationObligationTransactionObligationPaymentSchedule())
      
    {
      if (ReadAccrualInstructions())
      {
        UpdateAccrualInstructions();
      }
    }
  }

  private bool ReadAccrualInstructions()
  {
    System.Diagnostics.Debug.Assert(entities.ObligationTransaction.Populated);
    entities.Update.Populated = false;

    return Read("ReadAccrualInstructions",
      (db, command) =>
      {
        db.SetString(command, "otrType", entities.ObligationTransaction.Type1);
        db.SetInt32(command, "otyId", entities.ObligationTransaction.OtyType);
        db.SetInt32(
          command, "otrGeneratedId",
          entities.ObligationTransaction.SystemGeneratedIdentifier);
        db.
          SetString(command, "cpaType", entities.ObligationTransaction.CpaType);
          
        db.SetString(
          command, "cspNumber", entities.ObligationTransaction.CspNumber);
        db.SetInt32(
          command, "obgGeneratedId",
          entities.ObligationTransaction.ObgGeneratedId);
      },
      (db, reader) =>
      {
        entities.Update.OtrType = db.GetString(reader, 0);
        entities.Update.OtyId = db.GetInt32(reader, 1);
        entities.Update.ObgGeneratedId = db.GetInt32(reader, 2);
        entities.Update.CspNumber = db.GetString(reader, 3);
        entities.Update.CpaType = db.GetString(reader, 4);
        entities.Update.OtrGeneratedId = db.GetInt32(reader, 5);
        entities.Update.DiscontinueDt = db.GetNullableDate(reader, 6);
        entities.Update.Populated = true;
      });
  }

  private IEnumerable<bool>
    ReadObligationObligationTransactionObligationPaymentSchedule()
  {
    entities.Obligation.Populated = false;
    entities.ObligationTransaction.Populated = false;
    entities.ObligationPaymentSchedule.Populated = false;
    entities.AccrualInstructions.Populated = false;

    return ReadEach(
      "ReadObligationObligationTransactionObligationPaymentSchedule",
      (db, command) =>
      {
        db.
          SetNullableDate(command, "discontinueDt", new DateTime(2099, 12, 31));
          
      },
      (db, reader) =>
      {
        entities.Obligation.CpaType = db.GetString(reader, 0);
        entities.ObligationPaymentSchedule.ObgCpaType = db.GetString(reader, 0);
        entities.Obligation.CspNumber = db.GetString(reader, 1);
        entities.ObligationPaymentSchedule.ObgCspNumber =
          db.GetString(reader, 1);
        entities.Obligation.SystemGeneratedIdentifier = db.GetInt32(reader, 2);
        entities.ObligationPaymentSchedule.ObgGeneratedId =
          db.GetInt32(reader, 2);
        entities.Obligation.DtyGeneratedId = db.GetInt32(reader, 3);
        entities.ObligationPaymentSchedule.OtyType = db.GetInt32(reader, 3);
        entities.ObligationTransaction.ObgGeneratedId = db.GetInt32(reader, 4);
        entities.AccrualInstructions.ObgGeneratedId = db.GetInt32(reader, 4);
        entities.ObligationTransaction.CspNumber = db.GetString(reader, 5);
        entities.AccrualInstructions.CspNumber = db.GetString(reader, 5);
        entities.ObligationTransaction.CpaType = db.GetString(reader, 6);
        entities.AccrualInstructions.CpaType = db.GetString(reader, 6);
        entities.ObligationTransaction.SystemGeneratedIdentifier =
          db.GetInt32(reader, 7);
        entities.AccrualInstructions.OtrGeneratedId = db.GetInt32(reader, 7);
        entities.ObligationTransaction.Type1 = db.GetString(reader, 8);
        entities.AccrualInstructions.OtrType = db.GetString(reader, 8);
        entities.ObligationTransaction.CspSupNumber =
          db.GetNullableString(reader, 9);
        entities.ObligationTransaction.CpaSupType =
          db.GetNullableString(reader, 10);
        entities.ObligationTransaction.OtyType = db.GetInt32(reader, 11);
        entities.AccrualInstructions.OtyId = db.GetInt32(reader, 11);
        entities.ObligationPaymentSchedule.StartDt = db.GetDate(reader, 12);
        entities.ObligationPaymentSchedule.EndDt =
          db.GetNullableDate(reader, 13);
        entities.AccrualInstructions.DiscontinueDt =
          db.GetNullableDate(reader, 14);
        entities.Obligation.Populated = true;
        entities.ObligationTransaction.Populated = true;
        entities.ObligationPaymentSchedule.Populated = true;
        entities.AccrualInstructions.Populated = true;

        return true;
      });
  }

  private void UpdateAccrualInstructions()
  {
    System.Diagnostics.Debug.Assert(entities.Update.Populated);

    var discontinueDt = entities.ObligationPaymentSchedule.EndDt;

    entities.Update.Populated = false;
    Update("UpdateAccrualInstructions",
      (db, command) =>
      {
        db.SetNullableDate(command, "discontinueDt", discontinueDt);
        db.SetString(command, "otrType", entities.Update.OtrType);
        db.SetInt32(command, "otyId", entities.Update.OtyId);
        db.SetInt32(command, "obgGeneratedId", entities.Update.ObgGeneratedId);
        db.SetString(command, "cspNumber", entities.Update.CspNumber);
        db.SetString(command, "cpaType", entities.Update.CpaType);
        db.SetInt32(command, "otrGeneratedId", entities.Update.OtrGeneratedId);
      });

    entities.Update.DiscontinueDt = discontinueDt;
    entities.Update.Populated = true;
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
    /// A value of DateWorkArea.
    /// </summary>
    [JsonPropertyName("dateWorkArea")]
    public DateWorkArea DateWorkArea
    {
      get => dateWorkArea ??= new();
      set => dateWorkArea = value;
    }

    private DateWorkArea dateWorkArea;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of Update.
    /// </summary>
    [JsonPropertyName("update")]
    public AccrualInstructions Update
    {
      get => update ??= new();
      set => update = value;
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
    /// A value of ObligationTransaction.
    /// </summary>
    [JsonPropertyName("obligationTransaction")]
    public ObligationTransaction ObligationTransaction
    {
      get => obligationTransaction ??= new();
      set => obligationTransaction = value;
    }

    /// <summary>
    /// A value of ObligationPaymentSchedule.
    /// </summary>
    [JsonPropertyName("obligationPaymentSchedule")]
    public ObligationPaymentSchedule ObligationPaymentSchedule
    {
      get => obligationPaymentSchedule ??= new();
      set => obligationPaymentSchedule = value;
    }

    /// <summary>
    /// A value of AccrualInstructions.
    /// </summary>
    [JsonPropertyName("accrualInstructions")]
    public AccrualInstructions AccrualInstructions
    {
      get => accrualInstructions ??= new();
      set => accrualInstructions = value;
    }

    private AccrualInstructions update;
    private Obligation obligation;
    private ObligationTransaction obligationTransaction;
    private ObligationPaymentSchedule obligationPaymentSchedule;
    private AccrualInstructions accrualInstructions;
  }
#endregion
}
