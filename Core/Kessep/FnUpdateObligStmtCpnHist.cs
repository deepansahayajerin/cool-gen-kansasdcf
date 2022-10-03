// Program: FN_UPDATE_OBLIG_STMT_CPN_HIST, ID: 371737116, model: 746.
// Short name: SWE01602
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_UPDATE_OBLIG_STMT_CPN_HIST.
/// </summary>
[Serializable]
public partial class FnUpdateObligStmtCpnHist: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_UPDATE_OBLIG_STMT_CPN_HIST program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnUpdateObligStmtCpnHist(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnUpdateObligStmtCpnHist.
  /// </summary>
  public FnUpdateObligStmtCpnHist(IContext context, Import import, Export export)
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
    ExitState = "ACO_NN0000_ALL_OK";
    MoveStmtCouponSuppStatusHist(import.StmtCouponSuppStatusHist,
      export.StmtCouponSuppStatusHist);

    if (ReadStmtCouponSuppStatusHist())
    {
      try
      {
        UpdateStmtCouponSuppStatusHist();
        MoveStmtCouponSuppStatusHist(entities.StmtCouponSuppStatusHist,
          export.StmtCouponSuppStatusHist);
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "FN0000_STMT_CPN_SUPP_STS_HIST_NU";

            break;
          case ErrorCode.PermittedValueViolation:
            ExitState = "FN0000_STMT_CPN_SUPP_S_HST_PV_RB";

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
      ExitState = "FN0000_STMT_CPN_SUPP_STS_HIST_NF";
    }
  }

  private static void MoveStmtCouponSuppStatusHist(
    StmtCouponSuppStatusHist source, StmtCouponSuppStatusHist target)
  {
    target.DocTypeToSuppress = source.DocTypeToSuppress;
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.EffectiveDate = source.EffectiveDate;
    target.DiscontinueDate = source.DiscontinueDate;
    target.ReasonText = source.ReasonText;
  }

  private bool ReadStmtCouponSuppStatusHist()
  {
    entities.StmtCouponSuppStatusHist.Populated = false;

    return Read("ReadStmtCouponSuppStatusHist",
      (db, command) =>
      {
        db.
          SetNullableString(command, "cspNumberOblig", import.CsePerson.Number);
          
        db.SetNullableInt32(
          command, "obgId", import.Obligation.SystemGeneratedIdentifier);
        db.SetNullableInt32(
          command, "otyId", import.ObligationType.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "collId",
          import.StmtCouponSuppStatusHist.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.StmtCouponSuppStatusHist.CpaType = db.GetString(reader, 0);
        entities.StmtCouponSuppStatusHist.CspNumber = db.GetString(reader, 1);
        entities.StmtCouponSuppStatusHist.SystemGeneratedIdentifier =
          db.GetInt32(reader, 2);
        entities.StmtCouponSuppStatusHist.EffectiveDate = db.GetDate(reader, 3);
        entities.StmtCouponSuppStatusHist.DiscontinueDate =
          db.GetDate(reader, 4);
        entities.StmtCouponSuppStatusHist.ReasonText =
          db.GetNullableString(reader, 5);
        entities.StmtCouponSuppStatusHist.LastUpdatedBy =
          db.GetNullableString(reader, 6);
        entities.StmtCouponSuppStatusHist.LastUpdatedTmst =
          db.GetNullableDateTime(reader, 7);
        entities.StmtCouponSuppStatusHist.OtyId =
          db.GetNullableInt32(reader, 8);
        entities.StmtCouponSuppStatusHist.CpaTypeOblig =
          db.GetNullableString(reader, 9);
        entities.StmtCouponSuppStatusHist.CspNumberOblig =
          db.GetNullableString(reader, 10);
        entities.StmtCouponSuppStatusHist.ObgId =
          db.GetNullableInt32(reader, 11);
        entities.StmtCouponSuppStatusHist.DocTypeToSuppress =
          db.GetString(reader, 12);
        entities.StmtCouponSuppStatusHist.Populated = true;
        CheckValid<StmtCouponSuppStatusHist>("CpaType",
          entities.StmtCouponSuppStatusHist.CpaType);
        CheckValid<StmtCouponSuppStatusHist>("CpaTypeOblig",
          entities.StmtCouponSuppStatusHist.CpaTypeOblig);
        CheckValid<StmtCouponSuppStatusHist>("DocTypeToSuppress",
          entities.StmtCouponSuppStatusHist.DocTypeToSuppress);
      });
  }

  private void UpdateStmtCouponSuppStatusHist()
  {
    System.Diagnostics.Debug.
      Assert(entities.StmtCouponSuppStatusHist.Populated);

    var effectiveDate = import.StmtCouponSuppStatusHist.EffectiveDate;
    var discontinueDate = import.StmtCouponSuppStatusHist.DiscontinueDate;
    var reasonText = import.StmtCouponSuppStatusHist.ReasonText ?? "";
    var lastUpdatedBy = global.UserId;
    var lastUpdatedTmst = import.Current.Timestamp;
    var docTypeToSuppress = import.StmtCouponSuppStatusHist.DocTypeToSuppress;

    CheckValid<StmtCouponSuppStatusHist>("DocTypeToSuppress", docTypeToSuppress);
      
    entities.StmtCouponSuppStatusHist.Populated = false;
    Update("UpdateStmtCouponSuppStatusHist",
      (db, command) =>
      {
        db.SetDate(command, "effectiveDate", effectiveDate);
        db.SetDate(command, "discontinueDate", discontinueDate);
        db.SetNullableString(command, "reasonText", reasonText);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTmst);
        db.SetString(command, "docTypeToSupp", docTypeToSuppress);
        db.SetString(
          command, "cpaType", entities.StmtCouponSuppStatusHist.CpaType);
        db.SetString(
          command, "cspNumber", entities.StmtCouponSuppStatusHist.CspNumber);
        db.SetInt32(
          command, "collId",
          entities.StmtCouponSuppStatusHist.SystemGeneratedIdentifier);
      });

    entities.StmtCouponSuppStatusHist.EffectiveDate = effectiveDate;
    entities.StmtCouponSuppStatusHist.DiscontinueDate = discontinueDate;
    entities.StmtCouponSuppStatusHist.ReasonText = reasonText;
    entities.StmtCouponSuppStatusHist.LastUpdatedBy = lastUpdatedBy;
    entities.StmtCouponSuppStatusHist.LastUpdatedTmst = lastUpdatedTmst;
    entities.StmtCouponSuppStatusHist.DocTypeToSuppress = docTypeToSuppress;
    entities.StmtCouponSuppStatusHist.Populated = true;
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
    /// A value of Current.
    /// </summary>
    [JsonPropertyName("current")]
    public DateWorkArea Current
    {
      get => current ??= new();
      set => current = value;
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

    private DateWorkArea current;
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
    /// <summary>
    /// A value of StmtCouponSuppStatusHist.
    /// </summary>
    [JsonPropertyName("stmtCouponSuppStatusHist")]
    public StmtCouponSuppStatusHist StmtCouponSuppStatusHist
    {
      get => stmtCouponSuppStatusHist ??= new();
      set => stmtCouponSuppStatusHist = value;
    }

    private StmtCouponSuppStatusHist stmtCouponSuppStatusHist;
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
