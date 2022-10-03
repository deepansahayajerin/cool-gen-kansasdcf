﻿// Program: FN_DELETE_OBLIG_STMT_CPN_HIST, ID: 371737113, model: 746.
// Short name: SWE01604
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_DELETE_OBLIG_STMT_CPN_HIST.
/// </summary>
[Serializable]
public partial class FnDeleteObligStmtCpnHist: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_DELETE_OBLIG_STMT_CPN_HIST program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnDeleteObligStmtCpnHist(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnDeleteObligStmtCpnHist.
  /// </summary>
  public FnDeleteObligStmtCpnHist(IContext context, Import import, Export export)
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
    if (ReadCsePersonAccount())
    {
      if (ReadStmtCouponSuppStatusHist())
      {
        DeleteStmtCouponSuppStatusHist();
      }
      else
      {
        ExitState = "FN0000_STMT_CPN_SUPP_STS_HIST_NF";
      }
    }
    else
    {
      ExitState = "FN0000_CSE_PERSON_ACCOUNT_NF";
    }
  }

  private void DeleteStmtCouponSuppStatusHist()
  {
    Update("DeleteStmtCouponSuppStatusHist",
      (db, command) =>
      {
        db.SetString(
          command, "cpaType", entities.StmtCouponSuppStatusHist.CpaType);
        db.SetString(
          command, "cspNumber", entities.StmtCouponSuppStatusHist.CspNumber);
        db.SetInt32(
          command, "collId",
          entities.StmtCouponSuppStatusHist.SystemGeneratedIdentifier);
      });
  }

  private bool ReadCsePersonAccount()
  {
    entities.CsePersonAccount.Populated = false;

    return Read("ReadCsePersonAccount",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", import.CsePerson.Number);
      },
      (db, reader) =>
      {
        entities.CsePersonAccount.CspNumber = db.GetString(reader, 0);
        entities.CsePersonAccount.Type1 = db.GetString(reader, 1);
        entities.CsePersonAccount.Populated = true;
        CheckValid<CsePersonAccount>("Type1", entities.CsePersonAccount.Type1);
      });
  }

  private bool ReadStmtCouponSuppStatusHist()
  {
    System.Diagnostics.Debug.Assert(entities.CsePersonAccount.Populated);
    entities.StmtCouponSuppStatusHist.Populated = false;

    return Read("ReadStmtCouponSuppStatusHist",
      (db, command) =>
      {
        db.SetInt32(
          command, "collId",
          import.StmtCouponSuppStatusHist.SystemGeneratedIdentifier);
        db.SetString(command, "cpaType", entities.CsePersonAccount.Type1);
        db.SetString(command, "cspNumber", entities.CsePersonAccount.CspNumber);
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
        entities.StmtCouponSuppStatusHist.OtyId =
          db.GetNullableInt32(reader, 6);
        entities.StmtCouponSuppStatusHist.CpaTypeOblig =
          db.GetNullableString(reader, 7);
        entities.StmtCouponSuppStatusHist.CspNumberOblig =
          db.GetNullableString(reader, 8);
        entities.StmtCouponSuppStatusHist.ObgId =
          db.GetNullableInt32(reader, 9);
        entities.StmtCouponSuppStatusHist.Populated = true;
        CheckValid<StmtCouponSuppStatusHist>("CpaType",
          entities.StmtCouponSuppStatusHist.CpaType);
        CheckValid<StmtCouponSuppStatusHist>("Type1",
          entities.StmtCouponSuppStatusHist.Type1);
        CheckValid<StmtCouponSuppStatusHist>("CpaTypeOblig",
          entities.StmtCouponSuppStatusHist.CpaTypeOblig);
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
    /// A value of CsePersonAccount.
    /// </summary>
    [JsonPropertyName("csePersonAccount")]
    public CsePersonAccount CsePersonAccount
    {
      get => csePersonAccount ??= new();
      set => csePersonAccount = value;
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

    private ObligationType obligationType;
    private Obligation obligation;
    private CsePersonAccount csePersonAccount;
    private StmtCouponSuppStatusHist stmtCouponSuppStatusHist;
    private CsePerson csePerson;
  }
#endregion
}
