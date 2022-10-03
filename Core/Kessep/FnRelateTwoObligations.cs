// Program: FN_RELATE_TWO_OBLIGATIONS, ID: 372086139, model: 746.
// Short name: SWE00596
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: FN_RELATE_TWO_OBLIGATIONS.
/// </para>
/// <para>
/// RESP: FINANCE
/// This action block establishes the association between two Obligations that 
/// are concurrent (ie there are two Obligors).
/// </para>
/// </summary>
[Serializable]
public partial class FnRelateTwoObligations: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_RELATE_TWO_OBLIGATIONS program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnRelateTwoObligations(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnRelateTwoObligations.
  /// </summary>
  public FnRelateTwoObligations(IContext context, Import import, Export export):
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

    // =================================================
    // 2/10/1999 - bud adams  -  deleted fn-set-hardcode-debt-distribution
    //   and imported the 2 attributes.
    //   Deleted Read of obligation-type
    // =================================================
    // ***** MAIN-LINE AREA *****
    // Adjusted by Regan Welborn 3/5/97.
    // This cab now sets obligation relationship reasons for joint and several 
    // for use with OACC.  It also was changed to set the primary secondary
    // relationship indicator to a hard-coded J instead of a P and S.  From now
    // on, P and S will only be set in PREL for concurrent obligations.  All
    // others set here will be for Joint and Several.
    if (ReadObligation1())
    {
      // : The first Obligation successfull retrieved - continue processing.
    }
    else
    {
      ExitState = "FN0000_OBLIGATION_NF_RB";

      return;
    }

    if (ReadObligation2())
    {
      // : The second Obligation successfully retrieved - continue processing.
    }
    else
    {
      ExitState = "FN0000_OBLIGATION_NF_RB";

      return;
    }

    if (ReadObligationRlnRsn())
    {
      try
      {
        CreateObligationRln();

        // : Update the primary/secondary concurrency indicators on Obligations.
        UpdateObligation1();
        UpdateObligation2();
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "FN0000_OBLIG_RLN_AE_RB";

            break;
          case ErrorCode.PermittedValueViolation:
            ExitState = "FN0000_OBLIG_RLN_PV_RB";

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
      ExitState = "FN0000_OBLIG_RLN_RSN_NF_RB";
    }
  }

  private void CreateObligationRln()
  {
    System.Diagnostics.Debug.Assert(entities.FirstObligation.Populated);
    System.Diagnostics.Debug.Assert(entities.SecondObligation.Populated);

    var obgGeneratedId = entities.SecondObligation.SystemGeneratedIdentifier;
    var cspNumber = entities.SecondObligation.CspNumber;
    var cpaType = entities.SecondObligation.CpaType;
    var obgFGeneratedId = entities.FirstObligation.SystemGeneratedIdentifier;
    var cspFNumber = entities.FirstObligation.CspNumber;
    var cpaFType = entities.FirstObligation.CpaType;
    var orrGeneratedId =
      entities.ObligationRlnRsn.SequentialGeneratedIdentifier;
    var createdBy = global.UserId;
    var createdTmst = import.Current.Timestamp;
    var otySecondId = entities.SecondObligation.DtyGeneratedId;
    var otyFirstId = entities.FirstObligation.DtyGeneratedId;
    var description = import.ObligationRln.Description;

    CheckValid<ObligationRln>("CpaType", cpaType);
    CheckValid<ObligationRln>("CpaFType", cpaFType);
    entities.ObligationRln.Populated = false;
    Update("CreateObligationRln",
      (db, command) =>
      {
        db.SetInt32(command, "obgGeneratedId", obgGeneratedId);
        db.SetString(command, "cspNumber", cspNumber);
        db.SetString(command, "cpaType", cpaType);
        db.SetInt32(command, "obgFGeneratedId", obgFGeneratedId);
        db.SetString(command, "cspFNumber", cspFNumber);
        db.SetString(command, "cpaFType", cpaFType);
        db.SetInt32(command, "orrGeneratedId", orrGeneratedId);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTmst", createdTmst);
        db.SetInt32(command, "otySecondId", otySecondId);
        db.SetInt32(command, "otyFirstId", otyFirstId);
        db.SetString(command, "obRlnDsc", description);
      });

    entities.ObligationRln.ObgGeneratedId = obgGeneratedId;
    entities.ObligationRln.CspNumber = cspNumber;
    entities.ObligationRln.CpaType = cpaType;
    entities.ObligationRln.ObgFGeneratedId = obgFGeneratedId;
    entities.ObligationRln.CspFNumber = cspFNumber;
    entities.ObligationRln.CpaFType = cpaFType;
    entities.ObligationRln.OrrGeneratedId = orrGeneratedId;
    entities.ObligationRln.CreatedBy = createdBy;
    entities.ObligationRln.CreatedTmst = createdTmst;
    entities.ObligationRln.OtySecondId = otySecondId;
    entities.ObligationRln.OtyFirstId = otyFirstId;
    entities.ObligationRln.Description = description;
    entities.ObligationRln.Populated = true;
  }

  private bool ReadObligation1()
  {
    entities.FirstObligation.Populated = false;

    return Read("ReadObligation1",
      (db, command) =>
      {
        db.SetInt32(
          command, "obId", import.FirstObligation.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "dtyGeneratedId",
          import.ObligationType.SystemGeneratedIdentifier);
        db.SetString(command, "cpaType", import.HcCpaObligor.Type1);
        db.SetString(command, "cspNumber", import.FirstCsePerson.Number);
      },
      (db, reader) =>
      {
        entities.FirstObligation.CpaType = db.GetString(reader, 0);
        entities.FirstObligation.CspNumber = db.GetString(reader, 1);
        entities.FirstObligation.SystemGeneratedIdentifier =
          db.GetInt32(reader, 2);
        entities.FirstObligation.DtyGeneratedId = db.GetInt32(reader, 3);
        entities.FirstObligation.PrimarySecondaryCode =
          db.GetNullableString(reader, 4);
        entities.FirstObligation.LastUpdatedBy =
          db.GetNullableString(reader, 5);
        entities.FirstObligation.LastUpdateTmst =
          db.GetNullableDateTime(reader, 6);
        entities.FirstObligation.Populated = true;
        CheckValid<Obligation>("CpaType", entities.FirstObligation.CpaType);
        CheckValid<Obligation>("PrimarySecondaryCode",
          entities.FirstObligation.PrimarySecondaryCode);
      });
  }

  private bool ReadObligation2()
  {
    entities.SecondObligation.Populated = false;

    return Read("ReadObligation2",
      (db, command) =>
      {
        db.SetInt32(
          command, "obId", import.SecondObligation.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "dtyGeneratedId",
          import.ObligationType.SystemGeneratedIdentifier);
        db.SetString(command, "cpaType", import.HcCpaObligor.Type1);
        db.SetString(command, "cspNumber", import.SecondCsePerson.Number);
      },
      (db, reader) =>
      {
        entities.SecondObligation.CpaType = db.GetString(reader, 0);
        entities.SecondObligation.CspNumber = db.GetString(reader, 1);
        entities.SecondObligation.SystemGeneratedIdentifier =
          db.GetInt32(reader, 2);
        entities.SecondObligation.DtyGeneratedId = db.GetInt32(reader, 3);
        entities.SecondObligation.PrimarySecondaryCode =
          db.GetNullableString(reader, 4);
        entities.SecondObligation.LastUpdatedBy =
          db.GetNullableString(reader, 5);
        entities.SecondObligation.LastUpdateTmst =
          db.GetNullableDateTime(reader, 6);
        entities.SecondObligation.Populated = true;
        CheckValid<Obligation>("CpaType", entities.SecondObligation.CpaType);
        CheckValid<Obligation>("PrimarySecondaryCode",
          entities.SecondObligation.PrimarySecondaryCode);
      });
  }

  private bool ReadObligationRlnRsn()
  {
    entities.ObligationRlnRsn.Populated = false;

    return Read("ReadObligationRlnRsn",
      (db, command) =>
      {
        db.SetInt32(
          command, "obRlnRsnId",
          import.HcOrrJointSeveral.SequentialGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.ObligationRlnRsn.SequentialGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.ObligationRlnRsn.Populated = true;
      });
  }

  private void UpdateObligation1()
  {
    System.Diagnostics.Debug.Assert(entities.FirstObligation.Populated);

    var primarySecondaryCode =
      import.HcObligJointSevConcur.PrimarySecondaryCode ?? "";
    var lastUpdatedBy = global.UserId;
    var lastUpdateTmst = import.Current.Timestamp;

    CheckValid<Obligation>("PrimarySecondaryCode", primarySecondaryCode);
    entities.FirstObligation.Populated = false;
    Update("UpdateObligation1",
      (db, command) =>
      {
        db.SetNullableString(command, "primSecCd", primarySecondaryCode);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetNullableDateTime(command, "lastUpdateTmst", lastUpdateTmst);
        db.SetString(command, "cpaType", entities.FirstObligation.CpaType);
        db.SetString(command, "cspNumber", entities.FirstObligation.CspNumber);
        db.SetInt32(
          command, "obId", entities.FirstObligation.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "dtyGeneratedId", entities.FirstObligation.DtyGeneratedId);
      });

    entities.FirstObligation.PrimarySecondaryCode = primarySecondaryCode;
    entities.FirstObligation.LastUpdatedBy = lastUpdatedBy;
    entities.FirstObligation.LastUpdateTmst = lastUpdateTmst;
    entities.FirstObligation.Populated = true;
  }

  private void UpdateObligation2()
  {
    System.Diagnostics.Debug.Assert(entities.SecondObligation.Populated);

    var primarySecondaryCode =
      import.HcObligJointSevConcur.PrimarySecondaryCode ?? "";
    var lastUpdatedBy = global.UserId;
    var lastUpdateTmst = import.Current.Timestamp;

    CheckValid<Obligation>("PrimarySecondaryCode", primarySecondaryCode);
    entities.SecondObligation.Populated = false;
    Update("UpdateObligation2",
      (db, command) =>
      {
        db.SetNullableString(command, "primSecCd", primarySecondaryCode);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetNullableDateTime(command, "lastUpdateTmst", lastUpdateTmst);
        db.SetString(command, "cpaType", entities.SecondObligation.CpaType);
        db.SetString(command, "cspNumber", entities.SecondObligation.CspNumber);
        db.SetInt32(
          command, "obId", entities.SecondObligation.SystemGeneratedIdentifier);
          
        db.SetInt32(
          command, "dtyGeneratedId", entities.SecondObligation.DtyGeneratedId);
      });

    entities.SecondObligation.PrimarySecondaryCode = primarySecondaryCode;
    entities.SecondObligation.LastUpdatedBy = lastUpdatedBy;
    entities.SecondObligation.LastUpdateTmst = lastUpdateTmst;
    entities.SecondObligation.Populated = true;
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
    /// A value of HcObligJointSevConcur.
    /// </summary>
    [JsonPropertyName("hcObligJointSevConcur")]
    public Obligation HcObligJointSevConcur
    {
      get => hcObligJointSevConcur ??= new();
      set => hcObligJointSevConcur = value;
    }

    /// <summary>
    /// A value of HcCpaObligor.
    /// </summary>
    [JsonPropertyName("hcCpaObligor")]
    public CsePersonAccount HcCpaObligor
    {
      get => hcCpaObligor ??= new();
      set => hcCpaObligor = value;
    }

    /// <summary>
    /// A value of HcOrrJointSeveral.
    /// </summary>
    [JsonPropertyName("hcOrrJointSeveral")]
    public ObligationRlnRsn HcOrrJointSeveral
    {
      get => hcOrrJointSeveral ??= new();
      set => hcOrrJointSeveral = value;
    }

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
    /// A value of FirstCsePerson.
    /// </summary>
    [JsonPropertyName("firstCsePerson")]
    public CsePerson FirstCsePerson
    {
      get => firstCsePerson ??= new();
      set => firstCsePerson = value;
    }

    /// <summary>
    /// A value of FirstObligation.
    /// </summary>
    [JsonPropertyName("firstObligation")]
    public Obligation FirstObligation
    {
      get => firstObligation ??= new();
      set => firstObligation = value;
    }

    /// <summary>
    /// A value of SecondCsePerson.
    /// </summary>
    [JsonPropertyName("secondCsePerson")]
    public CsePerson SecondCsePerson
    {
      get => secondCsePerson ??= new();
      set => secondCsePerson = value;
    }

    /// <summary>
    /// A value of SecondObligation.
    /// </summary>
    [JsonPropertyName("secondObligation")]
    public Obligation SecondObligation
    {
      get => secondObligation ??= new();
      set => secondObligation = value;
    }

    /// <summary>
    /// A value of ObligationRln.
    /// </summary>
    [JsonPropertyName("obligationRln")]
    public ObligationRln ObligationRln
    {
      get => obligationRln ??= new();
      set => obligationRln = value;
    }

    private Obligation hcObligJointSevConcur;
    private CsePersonAccount hcCpaObligor;
    private ObligationRlnRsn hcOrrJointSeveral;
    private DateWorkArea current;
    private ObligationType obligationType;
    private CsePerson firstCsePerson;
    private Obligation firstObligation;
    private CsePerson secondCsePerson;
    private Obligation secondObligation;
    private ObligationRln obligationRln;
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
    /// A value of ObligationRlnRsn.
    /// </summary>
    [JsonPropertyName("obligationRlnRsn")]
    public ObligationRlnRsn ObligationRlnRsn
    {
      get => obligationRlnRsn ??= new();
      set => obligationRlnRsn = value;
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
    /// A value of FirstCsePerson.
    /// </summary>
    [JsonPropertyName("firstCsePerson")]
    public CsePerson FirstCsePerson
    {
      get => firstCsePerson ??= new();
      set => firstCsePerson = value;
    }

    /// <summary>
    /// A value of FirstCsePersonAccount.
    /// </summary>
    [JsonPropertyName("firstCsePersonAccount")]
    public CsePersonAccount FirstCsePersonAccount
    {
      get => firstCsePersonAccount ??= new();
      set => firstCsePersonAccount = value;
    }

    /// <summary>
    /// A value of FirstObligation.
    /// </summary>
    [JsonPropertyName("firstObligation")]
    public Obligation FirstObligation
    {
      get => firstObligation ??= new();
      set => firstObligation = value;
    }

    /// <summary>
    /// A value of SecondCsePerson.
    /// </summary>
    [JsonPropertyName("secondCsePerson")]
    public CsePerson SecondCsePerson
    {
      get => secondCsePerson ??= new();
      set => secondCsePerson = value;
    }

    /// <summary>
    /// A value of SecondCsePersonAccount.
    /// </summary>
    [JsonPropertyName("secondCsePersonAccount")]
    public CsePersonAccount SecondCsePersonAccount
    {
      get => secondCsePersonAccount ??= new();
      set => secondCsePersonAccount = value;
    }

    /// <summary>
    /// A value of SecondObligation.
    /// </summary>
    [JsonPropertyName("secondObligation")]
    public Obligation SecondObligation
    {
      get => secondObligation ??= new();
      set => secondObligation = value;
    }

    /// <summary>
    /// A value of ObligationRln.
    /// </summary>
    [JsonPropertyName("obligationRln")]
    public ObligationRln ObligationRln
    {
      get => obligationRln ??= new();
      set => obligationRln = value;
    }

    private ObligationRlnRsn obligationRlnRsn;
    private ObligationType obligationType;
    private CsePerson firstCsePerson;
    private CsePersonAccount firstCsePersonAccount;
    private Obligation firstObligation;
    private CsePerson secondCsePerson;
    private CsePersonAccount secondCsePersonAccount;
    private Obligation secondObligation;
    private ObligationRln obligationRln;
  }
#endregion
}
