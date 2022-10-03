// Program: FN_VALIDATE_OBLIG_STMT_CPN_DATES, ID: 371737114, model: 746.
// Short name: SWE01603
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_VALIDATE_OBLIG_STMT_CPN_DATES.
/// </summary>
[Serializable]
public partial class FnValidateObligStmtCpnDates: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_VALIDATE_OBLIG_STMT_CPN_DATES program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnValidateObligStmtCpnDates(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnValidateObligStmtCpnDates.
  /// </summary>
  public FnValidateObligStmtCpnDates(IContext context, Import import,
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
    // Date	By	IDCR #	Description
    // 050997	govind		Retrofit for CURRENT DATE
    // 			Fixed logic for overlapping date check
    // ---------------------------------------------
    local.Current.Date = Now().Date;

    if (!Lt(import.StmtCouponSuppStatusHist.EffectiveDate,
      import.StmtCouponSuppStatusHist.DiscontinueDate))
    {
      ExitState = "DISC_DATE_NOT_GREATER_EFFECTIVE";

      return;
    }

    foreach(var item in ReadStmtCouponSuppStatusHist())
    {
      if (Equal(import.Common.ActionEntry, "C") && entities
        .StmtCouponSuppStatusHist.SystemGeneratedIdentifier == import
        .StmtCouponSuppStatusHist.SystemGeneratedIdentifier || Lt
        (entities.StmtCouponSuppStatusHist.DiscontinueDate, local.Current.Date))
      {
        continue;
      }

      if (Lt(entities.StmtCouponSuppStatusHist.EffectiveDate,
        import.StmtCouponSuppStatusHist.DiscontinueDate) && Lt
        (import.StmtCouponSuppStatusHist.EffectiveDate,
        entities.StmtCouponSuppStatusHist.DiscontinueDate))
      {
        ExitState = "OVERLAPPING_DATE_RANGE";

        return;
      }
    }
  }

  private IEnumerable<bool> ReadStmtCouponSuppStatusHist()
  {
    entities.StmtCouponSuppStatusHist.Populated = false;

    return ReadEach("ReadStmtCouponSuppStatusHist",
      (db, command) =>
      {
        db.
          SetNullableString(command, "cspNumberOblig", import.CsePerson.Number);
          
        db.SetNullableInt32(
          command, "obgId", import.Obligation.SystemGeneratedIdentifier);
        db.SetNullableInt32(
          command, "otyId", import.ObligationType.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.StmtCouponSuppStatusHist.CpaType = db.GetString(reader, 0);
        entities.StmtCouponSuppStatusHist.CspNumber = db.GetString(reader, 1);
        entities.StmtCouponSuppStatusHist.SystemGeneratedIdentifier =
          db.GetInt32(reader, 2);
        entities.StmtCouponSuppStatusHist.Type1 = db.GetString(reader, 3);
        entities.StmtCouponSuppStatusHist.EffectiveDate = db.GetDate(reader, 4);
        entities.StmtCouponSuppStatusHist.DiscontinueDate =
          db.GetDate(reader, 5);
        entities.StmtCouponSuppStatusHist.CreatedBy = db.GetString(reader, 6);
        entities.StmtCouponSuppStatusHist.OtyId =
          db.GetNullableInt32(reader, 7);
        entities.StmtCouponSuppStatusHist.CpaTypeOblig =
          db.GetNullableString(reader, 8);
        entities.StmtCouponSuppStatusHist.CspNumberOblig =
          db.GetNullableString(reader, 9);
        entities.StmtCouponSuppStatusHist.ObgId =
          db.GetNullableInt32(reader, 10);
        entities.StmtCouponSuppStatusHist.DocTypeToSuppress =
          db.GetString(reader, 11);
        entities.StmtCouponSuppStatusHist.Populated = true;
        CheckValid<StmtCouponSuppStatusHist>("CpaType",
          entities.StmtCouponSuppStatusHist.CpaType);
        CheckValid<StmtCouponSuppStatusHist>("Type1",
          entities.StmtCouponSuppStatusHist.Type1);
        CheckValid<StmtCouponSuppStatusHist>("CpaTypeOblig",
          entities.StmtCouponSuppStatusHist.CpaTypeOblig);
        CheckValid<StmtCouponSuppStatusHist>("DocTypeToSuppress",
          entities.StmtCouponSuppStatusHist.DocTypeToSuppress);

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
    /// A value of Common.
    /// </summary>
    [JsonPropertyName("common")]
    public Common Common
    {
      get => common ??= new();
      set => common = value;
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
    /// A value of Obligation.
    /// </summary>
    [JsonPropertyName("obligation")]
    public Obligation Obligation
    {
      get => obligation ??= new();
      set => obligation = value;
    }

    /// <summary>
    /// A value of StmtCouponSuppStatusHist.
    /// </summary>
    [JsonPropertyName("stmtCouponSuppStatusHist")]
    public StmtCouponSuppStatusHist StmtCouponSuppStatusHist
    {
      get => stmtCouponSuppStatusHist ??= new();
      set => stmtCouponSuppStatusHist = value;
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

    private Common common;
    private ObligationType obligationType;
    private Obligation obligation;
    private StmtCouponSuppStatusHist stmtCouponSuppStatusHist;
    private CsePerson csePerson;
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
    /// A value of Current.
    /// </summary>
    [JsonPropertyName("current")]
    public DateWorkArea Current
    {
      get => current ??= new();
      set => current = value;
    }

    private DateWorkArea current;
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
    /// A value of StmtCouponSuppStatusHist.
    /// </summary>
    [JsonPropertyName("stmtCouponSuppStatusHist")]
    public StmtCouponSuppStatusHist StmtCouponSuppStatusHist
    {
      get => stmtCouponSuppStatusHist ??= new();
      set => stmtCouponSuppStatusHist = value;
    }

    /// <summary>
    /// A value of CsePersonAccount.
    /// </summary>
    [JsonPropertyName("csePersonAccount")]
    public CsePersonAccount CsePersonAccount
    {
      get => csePersonAccount ??= new();
      set => csePersonAccount = value;
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

    private ObligationType obligationType;
    private Obligation obligation;
    private ObligationTransaction obligationTransaction;
    private StmtCouponSuppStatusHist stmtCouponSuppStatusHist;
    private CsePersonAccount csePersonAccount;
    private CsePerson csePerson;
  }
#endregion
}
