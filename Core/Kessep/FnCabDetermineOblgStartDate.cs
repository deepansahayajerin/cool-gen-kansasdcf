// Program: FN_CAB_DETERMINE_OBLG_START_DATE, ID: 372754410, model: 746.
// Short name: SWE02460
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_CAB_DETERMINE_OBLG_START_DATE.
/// </summary>
[Serializable]
public partial class FnCabDetermineOblgStartDate: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_CAB_DETERMINE_OBLG_START_DATE program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnCabDetermineOblgStartDate(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnCabDetermineOblgStartDate.
  /// </summary>
  public FnCabDetermineOblgStartDate(IContext context, Import import,
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
    if (ReadObligationType())
    {
      if (AsChar(entities.ObligationType.Classification) == 'A')
      {
        if (ReadObligationPaymentSchedule())
        {
          export.ObligationStartDate.Date =
            entities.ObligationPaymentSchedule.StartDt;
        }
      }
      else
      {
        if (ReadDebtDetail())
        {
          export.ObligationStartDate.Date = entities.DebtDetail.DueDt;
        }
      }
    }
  }

  private bool ReadDebtDetail()
  {
    System.Diagnostics.Debug.Assert(import.Obligation.Populated);
    entities.DebtDetail.Populated = false;

    return Read("ReadDebtDetail",
      (db, command) =>
      {
        db.SetInt32(command, "otyType", import.Obligation.DtyGeneratedId);
        db.SetInt32(
          command, "obgGeneratedId",
          import.Obligation.SystemGeneratedIdentifier);
        db.SetString(command, "cspNumber", import.Obligation.CspNumber);
        db.SetString(command, "cpaType", import.Obligation.CpaType);
        db.SetDateTime(
          command, "createdTmst", import.StmtEnd.Timestamp.GetValueOrDefault());
          
      },
      (db, reader) =>
      {
        entities.DebtDetail.ObgGeneratedId = db.GetInt32(reader, 0);
        entities.DebtDetail.CspNumber = db.GetString(reader, 1);
        entities.DebtDetail.CpaType = db.GetString(reader, 2);
        entities.DebtDetail.OtrGeneratedId = db.GetInt32(reader, 3);
        entities.DebtDetail.OtyType = db.GetInt32(reader, 4);
        entities.DebtDetail.OtrType = db.GetString(reader, 5);
        entities.DebtDetail.DueDt = db.GetDate(reader, 6);
        entities.DebtDetail.CreatedTmst = db.GetDateTime(reader, 7);
        entities.DebtDetail.Populated = true;
        CheckValid<DebtDetail>("CpaType", entities.DebtDetail.CpaType);
        CheckValid<DebtDetail>("OtrType", entities.DebtDetail.OtrType);
      });
  }

  private bool ReadObligationPaymentSchedule()
  {
    System.Diagnostics.Debug.Assert(import.Obligation.Populated);
    entities.ObligationPaymentSchedule.Populated = false;

    return Read("ReadObligationPaymentSchedule",
      (db, command) =>
      {
        db.SetInt32(command, "otyType", import.Obligation.DtyGeneratedId);
        db.SetInt32(
          command, "obgGeneratedId",
          import.Obligation.SystemGeneratedIdentifier);
        db.SetString(command, "obgCspNumber", import.Obligation.CspNumber);
        db.SetString(command, "obgCpaType", import.Obligation.CpaType);
      },
      (db, reader) =>
      {
        entities.ObligationPaymentSchedule.OtyType = db.GetInt32(reader, 0);
        entities.ObligationPaymentSchedule.ObgGeneratedId =
          db.GetInt32(reader, 1);
        entities.ObligationPaymentSchedule.ObgCspNumber =
          db.GetString(reader, 2);
        entities.ObligationPaymentSchedule.ObgCpaType = db.GetString(reader, 3);
        entities.ObligationPaymentSchedule.StartDt = db.GetDate(reader, 4);
        entities.ObligationPaymentSchedule.Populated = true;
        CheckValid<ObligationPaymentSchedule>("ObgCpaType",
          entities.ObligationPaymentSchedule.ObgCpaType);
      });
  }

  private bool ReadObligationType()
  {
    System.Diagnostics.Debug.Assert(import.Obligation.Populated);
    entities.ObligationType.Populated = false;

    return Read("ReadObligationType",
      (db, command) =>
      {
        db.SetInt32(command, "debtTypId", import.Obligation.DtyGeneratedId);
      },
      (db, reader) =>
      {
        entities.ObligationType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.ObligationType.Classification = db.GetString(reader, 1);
        entities.ObligationType.Populated = true;
        CheckValid<ObligationType>("Classification",
          entities.ObligationType.Classification);
      });
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
    /// A value of StmtEnd.
    /// </summary>
    [JsonPropertyName("stmtEnd")]
    public DateWorkArea StmtEnd
    {
      get => stmtEnd ??= new();
      set => stmtEnd = value;
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

    private DateWorkArea stmtEnd;
    private Obligation obligation;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of ObligationStartDate.
    /// </summary>
    [JsonPropertyName("obligationStartDate")]
    public DateWorkArea ObligationStartDate
    {
      get => obligationStartDate ??= new();
      set => obligationStartDate = value;
    }

    private DateWorkArea obligationStartDate;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
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
    /// A value of ObligationPaymentSchedule.
    /// </summary>
    [JsonPropertyName("obligationPaymentSchedule")]
    public ObligationPaymentSchedule ObligationPaymentSchedule
    {
      get => obligationPaymentSchedule ??= new();
      set => obligationPaymentSchedule = value;
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
    /// A value of DebtDetail.
    /// </summary>
    [JsonPropertyName("debtDetail")]
    public DebtDetail DebtDetail
    {
      get => debtDetail ??= new();
      set => debtDetail = value;
    }

    private ObligationType obligationType;
    private ObligationPaymentSchedule obligationPaymentSchedule;
    private ObligationTransaction obligationTransaction;
    private DebtDetail debtDetail;
  }
#endregion
}
