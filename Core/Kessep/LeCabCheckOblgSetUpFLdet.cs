// Program: LE_CAB_CHECK_OBLG_SET_UP_F_LDET, ID: 371993428, model: 746.
// Short name: SWE02027
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: LE_CAB_CHECK_OBLG_SET_UP_F_LDET.
/// </para>
/// <para>
/// RESP: LEGALENF
/// This common action block checks and returns whether or not an obligation has
/// been set up for the given legal detail.
/// </para>
/// </summary>
[Serializable]
public partial class LeCabCheckOblgSetUpFLdet: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the LE_CAB_CHECK_OBLG_SET_UP_F_LDET program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new LeCabCheckOblgSetUpFLdet(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of LeCabCheckOblgSetUpFLdet.
  /// </summary>
  public LeCabCheckOblgSetUpFLdet(IContext context, Import import, Export export)
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
    // --------------------------------------------------------------------------------------------------------------
    // Date	  Developer	Request #	Description
    // --------------------------------------------------------------------------------------------------------------
    // ??/??/??  ??????????			Initial Code
    // 09/30/02  GVandy	WR020120	Re-structured to avoid conflicts with Finance 
    // archive.
    // --------------------------------------------------------------------------------------------------------------
    export.OblgSetUpForLdet.Flag = "N";
    export.OblgActiveForLdet.Flag = "N";
    local.Current.Date = Now().Date;

    // -- Find the legal detail and obligation type.
    if (!ReadLegalActionDetail())
    {
      ExitState = "LEGAL_ACTION_DETAIL_NF";

      return;
    }

    // -- Determine if the legal detail has been obligated.
    if (ReadObligation())
    {
      export.OblgSetUpForLdet.Flag = "Y";
    }
    else
    {
      // -- The detail has never been obligated, no need to check for active 
      // debts.
      return;
    }

    // -- Determine if any debts are still active.
    if (!ReadObligationType())
    {
      ExitState = "FN0000_OBLIGATION_TYPE_NF";

      return;
    }

    if (AsChar(entities.ObligationType.Classification) == 'A')
    {
      // -- Check for active accruing debts.
      if (ReadAccrualInstructions())
      {
        export.OblgActiveForLdet.Flag = "Y";
      }
      else
      {
        // -- Continue
      }
    }
    else
    {
      // -- Check for active non-accruing debts.
      if (ReadDebtDetail())
      {
        export.OblgActiveForLdet.Flag = "Y";
      }
      else
      {
        // -- Continue
      }
    }
  }

  private bool ReadAccrualInstructions()
  {
    System.Diagnostics.Debug.Assert(entities.LegalActionDetail.Populated);
    entities.AccrualInstructions.Populated = false;

    return Read("ReadAccrualInstructions",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "discontinueDt", local.Current.Date.GetValueOrDefault());
        db.SetNullableInt32(
          command, "ladNumber", entities.LegalActionDetail.Number);
        db.SetNullableInt32(
          command, "lgaIdentifier", entities.LegalActionDetail.LgaIdentifier);
      },
      (db, reader) =>
      {
        entities.AccrualInstructions.OtrType = db.GetString(reader, 0);
        entities.AccrualInstructions.OtyId = db.GetInt32(reader, 1);
        entities.AccrualInstructions.ObgGeneratedId = db.GetInt32(reader, 2);
        entities.AccrualInstructions.CspNumber = db.GetString(reader, 3);
        entities.AccrualInstructions.CpaType = db.GetString(reader, 4);
        entities.AccrualInstructions.OtrGeneratedId = db.GetInt32(reader, 5);
        entities.AccrualInstructions.DiscontinueDt =
          db.GetNullableDate(reader, 6);
        entities.AccrualInstructions.Populated = true;
        CheckValid<AccrualInstructions>("OtrType",
          entities.AccrualInstructions.OtrType);
        CheckValid<AccrualInstructions>("CpaType",
          entities.AccrualInstructions.CpaType);
      });
  }

  private bool ReadDebtDetail()
  {
    System.Diagnostics.Debug.Assert(entities.LegalActionDetail.Populated);
    entities.DebtDetail.Populated = false;

    return Read("ReadDebtDetail",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "retiredDt", local.Null1.RetiredDt.GetValueOrDefault());
        db.SetNullableInt32(
          command, "ladNumber", entities.LegalActionDetail.Number);
        db.SetNullableInt32(
          command, "lgaIdentifier", entities.LegalActionDetail.LgaIdentifier);
      },
      (db, reader) =>
      {
        entities.DebtDetail.ObgGeneratedId = db.GetInt32(reader, 0);
        entities.DebtDetail.CspNumber = db.GetString(reader, 1);
        entities.DebtDetail.CpaType = db.GetString(reader, 2);
        entities.DebtDetail.OtrGeneratedId = db.GetInt32(reader, 3);
        entities.DebtDetail.OtyType = db.GetInt32(reader, 4);
        entities.DebtDetail.OtrType = db.GetString(reader, 5);
        entities.DebtDetail.RetiredDt = db.GetNullableDate(reader, 6);
        entities.DebtDetail.Populated = true;
        CheckValid<DebtDetail>("CpaType", entities.DebtDetail.CpaType);
        CheckValid<DebtDetail>("OtrType", entities.DebtDetail.OtrType);
      });
  }

  private bool ReadLegalActionDetail()
  {
    entities.LegalActionDetail.Populated = false;

    return Read("ReadLegalActionDetail",
      (db, command) =>
      {
        db.SetInt32(command, "laDetailNo", import.LegalActionDetail.Number);
        db.SetInt32(command, "lgaIdentifier", import.LegalAction.Identifier);
      },
      (db, reader) =>
      {
        entities.LegalActionDetail.LgaIdentifier = db.GetInt32(reader, 0);
        entities.LegalActionDetail.Number = db.GetInt32(reader, 1);
        entities.LegalActionDetail.OtyId = db.GetNullableInt32(reader, 2);
        entities.LegalActionDetail.Populated = true;
      });
  }

  private bool ReadObligation()
  {
    System.Diagnostics.Debug.Assert(entities.LegalActionDetail.Populated);
    entities.Obligation.Populated = false;

    return Read("ReadObligation",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "ladNumber", entities.LegalActionDetail.Number);
        db.SetNullableInt32(
          command, "lgaIdentifier", entities.LegalActionDetail.LgaIdentifier);
      },
      (db, reader) =>
      {
        entities.Obligation.CpaType = db.GetString(reader, 0);
        entities.Obligation.CspNumber = db.GetString(reader, 1);
        entities.Obligation.SystemGeneratedIdentifier = db.GetInt32(reader, 2);
        entities.Obligation.DtyGeneratedId = db.GetInt32(reader, 3);
        entities.Obligation.LgaIdentifier = db.GetNullableInt32(reader, 4);
        entities.Obligation.LadNumber = db.GetNullableInt32(reader, 5);
        entities.Obligation.Populated = true;
        CheckValid<Obligation>("CpaType", entities.Obligation.CpaType);
      });
  }

  private bool ReadObligationType()
  {
    System.Diagnostics.Debug.Assert(entities.LegalActionDetail.Populated);
    entities.ObligationType.Populated = false;

    return Read("ReadObligationType",
      (db, command) =>
      {
        db.SetInt32(
          command, "debtTypId",
          entities.LegalActionDetail.OtyId.GetValueOrDefault());
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
    /// A value of LegalActionDetail.
    /// </summary>
    [JsonPropertyName("legalActionDetail")]
    public LegalActionDetail LegalActionDetail
    {
      get => legalActionDetail ??= new();
      set => legalActionDetail = value;
    }

    /// <summary>
    /// A value of LegalAction.
    /// </summary>
    [JsonPropertyName("legalAction")]
    public LegalAction LegalAction
    {
      get => legalAction ??= new();
      set => legalAction = value;
    }

    private LegalActionDetail legalActionDetail;
    private LegalAction legalAction;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of OblgActiveForLdet.
    /// </summary>
    [JsonPropertyName("oblgActiveForLdet")]
    public Common OblgActiveForLdet
    {
      get => oblgActiveForLdet ??= new();
      set => oblgActiveForLdet = value;
    }

    /// <summary>
    /// A value of OblgSetUpForLdet.
    /// </summary>
    [JsonPropertyName("oblgSetUpForLdet")]
    public Common OblgSetUpForLdet
    {
      get => oblgSetUpForLdet ??= new();
      set => oblgSetUpForLdet = value;
    }

    private Common oblgActiveForLdet;
    private Common oblgSetUpForLdet;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of Current.
    /// </summary>
    [JsonPropertyName("current")]
    public DateWorkArea Current
    {
      get => current ??= new();
      set => current = value;
    }

    /// <summary>
    /// A value of Null1.
    /// </summary>
    [JsonPropertyName("null1")]
    public DebtDetail Null1
    {
      get => null1 ??= new();
      set => null1 = value;
    }

    private DateWorkArea current;
    private DebtDetail null1;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of AccrualInstructions.
    /// </summary>
    [JsonPropertyName("accrualInstructions")]
    public AccrualInstructions AccrualInstructions
    {
      get => accrualInstructions ??= new();
      set => accrualInstructions = value;
    }

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
    /// A value of DebtDetail.
    /// </summary>
    [JsonPropertyName("debtDetail")]
    public DebtDetail DebtDetail
    {
      get => debtDetail ??= new();
      set => debtDetail = value;
    }

    /// <summary>
    /// A value of LegalActionPerson.
    /// </summary>
    [JsonPropertyName("legalActionPerson")]
    public LegalActionPerson LegalActionPerson
    {
      get => legalActionPerson ??= new();
      set => legalActionPerson = value;
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
    /// A value of Obligation.
    /// </summary>
    [JsonPropertyName("obligation")]
    public Obligation Obligation
    {
      get => obligation ??= new();
      set => obligation = value;
    }

    /// <summary>
    /// A value of LegalActionDetail.
    /// </summary>
    [JsonPropertyName("legalActionDetail")]
    public LegalActionDetail LegalActionDetail
    {
      get => legalActionDetail ??= new();
      set => legalActionDetail = value;
    }

    /// <summary>
    /// A value of LegalAction.
    /// </summary>
    [JsonPropertyName("legalAction")]
    public LegalAction LegalAction
    {
      get => legalAction ??= new();
      set => legalAction = value;
    }

    private AccrualInstructions accrualInstructions;
    private ObligationType obligationType;
    private DebtDetail debtDetail;
    private LegalActionPerson legalActionPerson;
    private ObligationTransaction obligationTransaction;
    private Obligation obligation;
    private LegalActionDetail legalActionDetail;
    private LegalAction legalAction;
  }
#endregion
}
