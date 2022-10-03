// Program: FN_UPDATE_MANUAL_DIST_AUDIT, ID: 372039875, model: 746.
// Short name: SWE00656
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: FN_UPDATE_MANUAL_DIST_AUDIT.
/// </para>
/// <para>
/// RESP: FINANCE
/// </para>
/// </summary>
[Serializable]
public partial class FnUpdateManualDistAudit: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_UPDATE_MANUAL_DIST_AUDIT program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnUpdateManualDistAudit(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnUpdateManualDistAudit.
  /// </summary>
  public FnUpdateManualDistAudit(IContext context, Import import, Export export):
    
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // =================================================
    // 5/11/99 - bud adams  -  imported current-timestamp and added
    //   the 'last-updated' attributes to the UPDATE action; changed
    //   Read properties to select only
    // =================================================
    if (ReadObligation())
    {
      if (ReadManualDistributionAudit2())
      {
        if (ReadManualDistributionAudit1())
        {
          ExitState = "FN0000_OVERLAPPING_MAN_DIST_INST";

          return;
        }

        try
        {
          UpdateManualDistributionAudit();
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              break;
            case ErrorCode.PermittedValueViolation:
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
        ExitState = "FN0000_MANUAL_DIST_AUDIT_NF";
      }
    }
    else
    {
      ExitState = "FN0000_OBLIGATION_NF";
    }
  }

  private bool ReadManualDistributionAudit1()
  {
    System.Diagnostics.Debug.Assert(entities.ExistingObligation.Populated);
    entities.Overlapping.Populated = false;

    return Read("ReadManualDistributionAudit1",
      (db, command) =>
      {
        db.SetInt32(
          command, "otyType", entities.ExistingObligation.DtyGeneratedId);
        db.SetInt32(
          command, "obgGeneratedId",
          entities.ExistingObligation.SystemGeneratedIdentifier);
        db.
          SetString(command, "cspNumber", entities.ExistingObligation.CspNumber);
          
        db.SetString(command, "cpaType", entities.ExistingObligation.CpaType);
        db.SetDate(
          command, "effectiveDt",
          import.ManualDistributionAudit.DiscontinueDt.GetValueOrDefault());
        db.SetNullableDate(
          command, "discontinueDt",
          import.ManualDistributionAudit.EffectiveDt.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Overlapping.OtyType = db.GetInt32(reader, 0);
        entities.Overlapping.ObgGeneratedId = db.GetInt32(reader, 1);
        entities.Overlapping.CspNumber = db.GetString(reader, 2);
        entities.Overlapping.CpaType = db.GetString(reader, 3);
        entities.Overlapping.EffectiveDt = db.GetDate(reader, 4);
        entities.Overlapping.DiscontinueDt = db.GetNullableDate(reader, 5);
        entities.Overlapping.Populated = true;
        CheckValid<ManualDistributionAudit>("CpaType",
          entities.Overlapping.CpaType);
      });
  }

  private bool ReadManualDistributionAudit2()
  {
    System.Diagnostics.Debug.Assert(entities.ExistingObligation.Populated);
    entities.ExistingManualDistributionAudit.Populated = false;

    return Read("ReadManualDistributionAudit2",
      (db, command) =>
      {
        db.SetInt32(
          command, "otyType", entities.ExistingObligation.DtyGeneratedId);
        db.SetInt32(
          command, "obgGeneratedId",
          entities.ExistingObligation.SystemGeneratedIdentifier);
        db.
          SetString(command, "cspNumber", entities.ExistingObligation.CspNumber);
          
        db.SetString(command, "cpaType", entities.ExistingObligation.CpaType);
        db.SetDate(
          command, "effectiveDt",
          import.ManualDistributionAudit.EffectiveDt.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ExistingManualDistributionAudit.OtyType =
          db.GetInt32(reader, 0);
        entities.ExistingManualDistributionAudit.ObgGeneratedId =
          db.GetInt32(reader, 1);
        entities.ExistingManualDistributionAudit.CspNumber =
          db.GetString(reader, 2);
        entities.ExistingManualDistributionAudit.CpaType =
          db.GetString(reader, 3);
        entities.ExistingManualDistributionAudit.EffectiveDt =
          db.GetDate(reader, 4);
        entities.ExistingManualDistributionAudit.DiscontinueDt =
          db.GetNullableDate(reader, 5);
        entities.ExistingManualDistributionAudit.CreatedBy =
          db.GetString(reader, 6);
        entities.ExistingManualDistributionAudit.CreatedTmst =
          db.GetDateTime(reader, 7);
        entities.ExistingManualDistributionAudit.LastUpdatedBy =
          db.GetNullableString(reader, 8);
        entities.ExistingManualDistributionAudit.LastUpdatedTmst =
          db.GetNullableDateTime(reader, 9);
        entities.ExistingManualDistributionAudit.Instructions =
          db.GetNullableString(reader, 10);
        entities.ExistingManualDistributionAudit.Populated = true;
        CheckValid<ManualDistributionAudit>("CpaType",
          entities.ExistingManualDistributionAudit.CpaType);
      });
  }

  private bool ReadObligation()
  {
    entities.ExistingObligation.Populated = false;

    return Read("ReadObligation",
      (db, command) =>
      {
        db.
          SetInt32(command, "obId", import.Obligation.SystemGeneratedIdentifier);
          
        db.SetInt32(
          command, "dtyGeneratedId",
          import.ObligationType.SystemGeneratedIdentifier);
        db.SetString(command, "cpaType", import.CsePersonAccount.Type1);
        db.SetString(command, "cspNumber", import.CsePerson.Number);
      },
      (db, reader) =>
      {
        entities.ExistingObligation.CpaType = db.GetString(reader, 0);
        entities.ExistingObligation.CspNumber = db.GetString(reader, 1);
        entities.ExistingObligation.SystemGeneratedIdentifier =
          db.GetInt32(reader, 2);
        entities.ExistingObligation.DtyGeneratedId = db.GetInt32(reader, 3);
        entities.ExistingObligation.Description =
          db.GetNullableString(reader, 4);
        entities.ExistingObligation.HistoryInd =
          db.GetNullableString(reader, 5);
        entities.ExistingObligation.PrimarySecondaryCode =
          db.GetNullableString(reader, 6);
        entities.ExistingObligation.PreConversionDebtNumber =
          db.GetNullableInt32(reader, 7);
        entities.ExistingObligation.PreConversionCaseNumber =
          db.GetNullableString(reader, 8);
        entities.ExistingObligation.AsOfDtNadArrBal =
          db.GetNullableDecimal(reader, 9);
        entities.ExistingObligation.AsOfDtNadIntBal =
          db.GetNullableDecimal(reader, 10);
        entities.ExistingObligation.AsOfDtAdcArrBal =
          db.GetNullableDecimal(reader, 11);
        entities.ExistingObligation.AsOfDtAdcIntBal =
          db.GetNullableDecimal(reader, 12);
        entities.ExistingObligation.AsOfDtRecBal =
          db.GetNullableDecimal(reader, 13);
        entities.ExistingObligation.AsOdDtRecIntBal =
          db.GetNullableDecimal(reader, 14);
        entities.ExistingObligation.AsOfDtFeeBal =
          db.GetNullableDecimal(reader, 15);
        entities.ExistingObligation.AsOfDtFeeIntBal =
          db.GetNullableDecimal(reader, 16);
        entities.ExistingObligation.AsOfDtTotBalCurrArr =
          db.GetNullableDecimal(reader, 17);
        entities.ExistingObligation.TillDtCsCollCurrArr =
          db.GetNullableDecimal(reader, 18);
        entities.ExistingObligation.TillDtSpCollCurrArr =
          db.GetNullableDecimal(reader, 19);
        entities.ExistingObligation.TillDtMsCollCurrArr =
          db.GetNullableDecimal(reader, 20);
        entities.ExistingObligation.TillDtNadArrColl =
          db.GetNullableDecimal(reader, 21);
        entities.ExistingObligation.TillDtNadIntColl =
          db.GetNullableDecimal(reader, 22);
        entities.ExistingObligation.TillDtAdcArrColl =
          db.GetNullableDecimal(reader, 23);
        entities.ExistingObligation.TillDtAdcIntColl =
          db.GetNullableDecimal(reader, 24);
        entities.ExistingObligation.AsOfDtTotRecColl =
          db.GetNullableDecimal(reader, 25);
        entities.ExistingObligation.AsOfDtTotRecIntColl =
          db.GetNullableDecimal(reader, 26);
        entities.ExistingObligation.AsOfDtTotFeeColl =
          db.GetNullableDecimal(reader, 27);
        entities.ExistingObligation.AsOfDtTotFeeIntColl =
          db.GetNullableDecimal(reader, 28);
        entities.ExistingObligation.AsOfDtTotCollAll =
          db.GetNullableDecimal(reader, 29);
        entities.ExistingObligation.LastCollAmt =
          db.GetNullableDecimal(reader, 30);
        entities.ExistingObligation.LastCollDt = db.GetNullableDate(reader, 31);
        entities.ExistingObligation.CreatedBy = db.GetString(reader, 32);
        entities.ExistingObligation.CreatedTmst = db.GetDateTime(reader, 33);
        entities.ExistingObligation.LastUpdatedBy =
          db.GetNullableString(reader, 34);
        entities.ExistingObligation.LastUpdateTmst =
          db.GetNullableDateTime(reader, 35);
        entities.ExistingObligation.Populated = true;
        CheckValid<Obligation>("CpaType", entities.ExistingObligation.CpaType);
        CheckValid<Obligation>("PrimarySecondaryCode",
          entities.ExistingObligation.PrimarySecondaryCode);
      });
  }

  private void UpdateManualDistributionAudit()
  {
    System.Diagnostics.Debug.Assert(
      entities.ExistingManualDistributionAudit.Populated);

    var discontinueDt = import.ManualDistributionAudit.DiscontinueDt;
    var lastUpdatedBy = global.UserId;
    var lastUpdatedTmst = import.Current.Timestamp;
    var instructions = import.ManualDistributionAudit.Instructions ?? "";

    entities.ExistingManualDistributionAudit.Populated = false;
    Update("UpdateManualDistributionAudit",
      (db, command) =>
      {
        db.SetNullableDate(command, "discontinueDt", discontinueDt);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTmst);
        db.SetNullableString(command, "mnlDistInstr", instructions);
        db.SetInt32(
          command, "otyType", entities.ExistingManualDistributionAudit.OtyType);
          
        db.SetInt32(
          command, "obgGeneratedId",
          entities.ExistingManualDistributionAudit.ObgGeneratedId);
        db.SetString(
          command, "cspNumber",
          entities.ExistingManualDistributionAudit.CspNumber);
        db.SetString(
          command, "cpaType", entities.ExistingManualDistributionAudit.CpaType);
          
        db.SetDate(
          command, "effectiveDt",
          entities.ExistingManualDistributionAudit.EffectiveDt.
            GetValueOrDefault());
      });

    entities.ExistingManualDistributionAudit.DiscontinueDt = discontinueDt;
    entities.ExistingManualDistributionAudit.LastUpdatedBy = lastUpdatedBy;
    entities.ExistingManualDistributionAudit.LastUpdatedTmst = lastUpdatedTmst;
    entities.ExistingManualDistributionAudit.Instructions = instructions;
    entities.ExistingManualDistributionAudit.Populated = true;
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
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
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
    /// A value of Obligation.
    /// </summary>
    [JsonPropertyName("obligation")]
    public Obligation Obligation
    {
      get => obligation ??= new();
      set => obligation = value;
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
    /// A value of ManualDistributionAudit.
    /// </summary>
    [JsonPropertyName("manualDistributionAudit")]
    public ManualDistributionAudit ManualDistributionAudit
    {
      get => manualDistributionAudit ??= new();
      set => manualDistributionAudit = value;
    }

    private DateWorkArea current;
    private CsePerson csePerson;
    private CsePersonAccount csePersonAccount;
    private Obligation obligation;
    private ObligationType obligationType;
    private ManualDistributionAudit manualDistributionAudit;
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
    /// A value of Overlapping.
    /// </summary>
    [JsonPropertyName("overlapping")]
    public ManualDistributionAudit Overlapping
    {
      get => overlapping ??= new();
      set => overlapping = value;
    }

    /// <summary>
    /// A value of ExistingCsePerson.
    /// </summary>
    [JsonPropertyName("existingCsePerson")]
    public CsePerson ExistingCsePerson
    {
      get => existingCsePerson ??= new();
      set => existingCsePerson = value;
    }

    /// <summary>
    /// A value of ExistingCsePersonAccount.
    /// </summary>
    [JsonPropertyName("existingCsePersonAccount")]
    public CsePersonAccount ExistingCsePersonAccount
    {
      get => existingCsePersonAccount ??= new();
      set => existingCsePersonAccount = value;
    }

    /// <summary>
    /// A value of ExistingObligationType.
    /// </summary>
    [JsonPropertyName("existingObligationType")]
    public ObligationType ExistingObligationType
    {
      get => existingObligationType ??= new();
      set => existingObligationType = value;
    }

    /// <summary>
    /// A value of ExistingObligation.
    /// </summary>
    [JsonPropertyName("existingObligation")]
    public Obligation ExistingObligation
    {
      get => existingObligation ??= new();
      set => existingObligation = value;
    }

    /// <summary>
    /// A value of ExistingManualDistributionAudit.
    /// </summary>
    [JsonPropertyName("existingManualDistributionAudit")]
    public ManualDistributionAudit ExistingManualDistributionAudit
    {
      get => existingManualDistributionAudit ??= new();
      set => existingManualDistributionAudit = value;
    }

    private ManualDistributionAudit overlapping;
    private CsePerson existingCsePerson;
    private CsePersonAccount existingCsePersonAccount;
    private ObligationType existingObligationType;
    private Obligation existingObligation;
    private ManualDistributionAudit existingManualDistributionAudit;
  }
#endregion
}
