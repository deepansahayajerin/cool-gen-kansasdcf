// Program: LE_IADA_CREATE_OBLN_ADMIN_ACTION, ID: 372595011, model: 746.
// Short name: SWE00782
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: LE_IADA_CREATE_OBLN_ADMIN_ACTION.
/// </para>
/// <para>
/// RESP: LGLENFAC
/// This action block creates OBLIGATION_ADMINISTRATIVE_ACTION and associates 
/// with all or specified obligations.
/// </para>
/// </summary>
[Serializable]
public partial class LeIadaCreateOblnAdminAction: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the LE_IADA_CREATE_OBLN_ADMIN_ACTION program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new LeIadaCreateOblnAdminAction(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of LeIadaCreateOblnAdminAction.
  /// </summary>
  public LeIadaCreateOblnAdminAction(IContext context, Import import,
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
    // ******************************************************
    // 10/16/98  P. Sharp   Removed redundant reads.
    export.Rollback.Flag = "N";

    if (AsChar(import.CreateForAllObligation.Flag) == 'Y')
    {
      if (IsEmpty(import.LegalAction.CourtCaseNumber))
      {
        ExitState = "OE0000_COURT_CASE_REQD";

        return;
      }
    }
    else if (import.Obligation.SystemGeneratedIdentifier == 0)
    {
      ExitState = "LE0000_OBLIGATION_REQD";

      return;
    }

    if (!ReadCsePersonAccount())
    {
      if (ReadCsePerson())
      {
        ExitState = "LE0000_CSE_PERSON_NOT_AN_OBLIGOR";

        return;
      }
      else
      {
        ExitState = "CSE_PERSON_NF";

        return;
      }
    }

    if (!ReadAdministrativeAction())
    {
      ExitState = "LE0000_INVALID_ADMIN_ACTION_TYPE";

      return;
    }

    if (AsChar(import.CreateForAllObligation.Flag) == 'Y')
    {
      foreach(var item in ReadObligationTribunal())
      {
        if (ReadFips())
        {
          if (!Equal(entities.ExistingFips.StateAbbreviation,
            import.Fips.StateAbbreviation) || !
            Equal(entities.ExistingFips.CountyAbbreviation,
            import.Fips.CountyAbbreviation))
          {
            continue;
          }
        }
        else if (entities.ExistingTribunal.Identifier != import
          .Tribunal.Identifier)
        {
          continue;
        }

        if (ReadObligationType())
        {
          export.ObligationType.Assign(entities.ExistingObligationType);
        }

        try
        {
          CreateObligationAdministrativeAction();
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              export.Rollback.Flag = "Y";
              ExitState = "OBLIGATION_ADMIN_ACTION_AE";

              return;
            case ErrorCode.PermittedValueViolation:
              break;
            case ErrorCode.DatabaseError:
              break;
            default:
              throw;
          }
        }
      }
    }
    else
    {
      // ---------------------------------------------
      // Create and associate with only the given obligation
      // ---------------------------------------------
      if (ReadObligationObligationType())
      {
        export.ObligationType.Assign(entities.ExistingObligationType);

        try
        {
          CreateObligationAdministrativeAction();
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              ExitState = "OBLIGATION_ADMIN_ACTION_AE";

              return;
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
        ExitState = "FN0000_OBLIGATION_NF";

        return;
      }
    }

    if (ReadServiceProvider())
    {
      export.ActionTakenBy.Assign(entities.ExistingActionTakenBy);
    }
  }

  private void CreateObligationAdministrativeAction()
  {
    System.Diagnostics.Debug.Assert(entities.ExistingObligation.Populated);

    var otyType = entities.ExistingObligation.DtyGeneratedId;
    var aatType = entities.ExistingAdministrativeAction.Type1;
    var obgGeneratedId = entities.ExistingObligation.SystemGeneratedIdentifier;
    var cspNumber = entities.ExistingObligation.CspNumber;
    var cpaType = entities.ExistingObligation.CpaType;
    var takenDate = import.ObligationAdministrativeAction.TakenDate;
    var responseDate = import.ObligationAdministrativeAction.ResponseDate;
    var createdBy = global.UserId;
    var createdTstamp = Now();
    var response1 = import.ObligationAdministrativeAction.Response ?? "";

    CheckValid<ObligationAdministrativeAction>("CpaType", cpaType);
    entities.New1.Populated = false;
    Update("CreateObligationAdministrativeAction",
      (db, command) =>
      {
        db.SetInt32(command, "otyType", otyType);
        db.SetString(command, "aatType", aatType);
        db.SetInt32(command, "obgGeneratedId", obgGeneratedId);
        db.SetString(command, "cspNumber", cspNumber);
        db.SetString(command, "cpaType", cpaType);
        db.SetDate(command, "takenDt", takenDate);
        db.SetNullableDate(command, "responseDt", responseDate);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTstamp", createdTstamp);
        db.SetNullableString(command, "lastUpdatedBy", "");
        db.SetNullableDateTime(command, "lastUpdTstamp", default(DateTime));
        db.SetNullableString(command, "response", response1);
      });

    entities.New1.OtyType = otyType;
    entities.New1.AatType = aatType;
    entities.New1.ObgGeneratedId = obgGeneratedId;
    entities.New1.CspNumber = cspNumber;
    entities.New1.CpaType = cpaType;
    entities.New1.TakenDate = takenDate;
    entities.New1.ResponseDate = responseDate;
    entities.New1.CreatedBy = createdBy;
    entities.New1.CreatedTstamp = createdTstamp;
    entities.New1.Response = response1;
    entities.New1.Populated = true;
  }

  private bool ReadAdministrativeAction()
  {
    entities.ExistingAdministrativeAction.Populated = false;

    return Read("ReadAdministrativeAction",
      (db, command) =>
      {
        db.SetString(command, "type", import.AdministrativeAction.Type1);
      },
      (db, reader) =>
      {
        entities.ExistingAdministrativeAction.Type1 = db.GetString(reader, 0);
        entities.ExistingAdministrativeAction.Populated = true;
      });
  }

  private bool ReadCsePerson()
  {
    entities.ExistingCsePerson.Populated = false;

    return Read("ReadCsePerson",
      (db, command) =>
      {
        db.SetString(command, "numb", import.CsePerson.Number);
      },
      (db, reader) =>
      {
        entities.ExistingCsePerson.Number = db.GetString(reader, 0);
        entities.ExistingCsePerson.Populated = true;
      });
  }

  private bool ReadCsePersonAccount()
  {
    entities.ExistingCsePersonAccount.Populated = false;

    return Read("ReadCsePersonAccount",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", import.CsePerson.Number);
      },
      (db, reader) =>
      {
        entities.ExistingCsePersonAccount.CspNumber = db.GetString(reader, 0);
        entities.ExistingCsePersonAccount.Type1 = db.GetString(reader, 1);
        entities.ExistingCsePersonAccount.Populated = true;
        CheckValid<CsePersonAccount>("Type1",
          entities.ExistingCsePersonAccount.Type1);
      });
  }

  private bool ReadFips()
  {
    System.Diagnostics.Debug.Assert(entities.ExistingTribunal.Populated);
    entities.ExistingFips.Populated = false;

    return Read("ReadFips",
      (db, command) =>
      {
        db.SetInt32(
          command, "location",
          entities.ExistingTribunal.FipLocation.GetValueOrDefault());
        db.SetInt32(
          command, "county",
          entities.ExistingTribunal.FipCounty.GetValueOrDefault());
        db.SetInt32(
          command, "state",
          entities.ExistingTribunal.FipState.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ExistingFips.State = db.GetInt32(reader, 0);
        entities.ExistingFips.County = db.GetInt32(reader, 1);
        entities.ExistingFips.Location = db.GetInt32(reader, 2);
        entities.ExistingFips.CountyDescription =
          db.GetNullableString(reader, 3);
        entities.ExistingFips.StateAbbreviation = db.GetString(reader, 4);
        entities.ExistingFips.CountyAbbreviation =
          db.GetNullableString(reader, 5);
        entities.ExistingFips.Populated = true;
      });
  }

  private bool ReadObligationObligationType()
  {
    System.Diagnostics.Debug.
      Assert(entities.ExistingCsePersonAccount.Populated);
    entities.ExistingObligationType.Populated = false;
    entities.ExistingObligation.Populated = false;

    return Read("ReadObligationObligationType",
      (db, command) =>
      {
        db.
          SetString(command, "cpaType", entities.ExistingCsePersonAccount.Type1);
          
        db.SetString(
          command, "cspNumber", entities.ExistingCsePersonAccount.CspNumber);
        db.
          SetInt32(command, "obId", import.Obligation.SystemGeneratedIdentifier);
          
        db.SetInt32(
          command, "debtTypId",
          import.ObligationType.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.ExistingObligation.CpaType = db.GetString(reader, 0);
        entities.ExistingObligation.CspNumber = db.GetString(reader, 1);
        entities.ExistingObligation.SystemGeneratedIdentifier =
          db.GetInt32(reader, 2);
        entities.ExistingObligation.DtyGeneratedId = db.GetInt32(reader, 3);
        entities.ExistingObligationType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 3);
        entities.ExistingObligation.LgaId = db.GetNullableInt32(reader, 4);
        entities.ExistingObligationType.Code = db.GetString(reader, 5);
        entities.ExistingObligationType.Name = db.GetString(reader, 6);
        entities.ExistingObligationType.Populated = true;
        entities.ExistingObligation.Populated = true;
        CheckValid<Obligation>("CpaType", entities.ExistingObligation.CpaType);
      });
  }

  private IEnumerable<bool> ReadObligationTribunal()
  {
    System.Diagnostics.Debug.
      Assert(entities.ExistingCsePersonAccount.Populated);
    entities.ExistingTribunal.Populated = false;
    entities.ExistingObligation.Populated = false;

    return ReadEach("ReadObligationTribunal",
      (db, command) =>
      {
        db.
          SetString(command, "cpaType", entities.ExistingCsePersonAccount.Type1);
          
        db.SetString(
          command, "cspNumber", entities.ExistingCsePersonAccount.CspNumber);
        db.SetNullableString(
          command, "courtCaseNo", import.LegalAction.CourtCaseNumber ?? "");
      },
      (db, reader) =>
      {
        entities.ExistingObligation.CpaType = db.GetString(reader, 0);
        entities.ExistingObligation.CspNumber = db.GetString(reader, 1);
        entities.ExistingObligation.SystemGeneratedIdentifier =
          db.GetInt32(reader, 2);
        entities.ExistingObligation.DtyGeneratedId = db.GetInt32(reader, 3);
        entities.ExistingObligation.LgaId = db.GetNullableInt32(reader, 4);
        entities.ExistingTribunal.JudicialDivision =
          db.GetNullableString(reader, 5);
        entities.ExistingTribunal.Name = db.GetString(reader, 6);
        entities.ExistingTribunal.FipLocation = db.GetNullableInt32(reader, 7);
        entities.ExistingTribunal.JudicialDistrict = db.GetString(reader, 8);
        entities.ExistingTribunal.Identifier = db.GetInt32(reader, 9);
        entities.ExistingTribunal.FipCounty = db.GetNullableInt32(reader, 10);
        entities.ExistingTribunal.FipState = db.GetNullableInt32(reader, 11);
        entities.ExistingTribunal.Populated = true;
        entities.ExistingObligation.Populated = true;
        CheckValid<Obligation>("CpaType", entities.ExistingObligation.CpaType);

        return true;
      });
  }

  private bool ReadObligationType()
  {
    System.Diagnostics.Debug.Assert(entities.ExistingObligation.Populated);
    entities.ExistingObligationType.Populated = false;

    return Read("ReadObligationType",
      (db, command) =>
      {
        db.SetInt32(
          command, "debtTypId", entities.ExistingObligation.DtyGeneratedId);
      },
      (db, reader) =>
      {
        entities.ExistingObligationType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.ExistingObligationType.Code = db.GetString(reader, 1);
        entities.ExistingObligationType.Name = db.GetString(reader, 2);
        entities.ExistingObligationType.Populated = true;
      });
  }

  private bool ReadServiceProvider()
  {
    entities.ExistingActionTakenBy.Populated = false;

    return Read("ReadServiceProvider",
      (db, command) =>
      {
        db.SetString(command, "userId", global.UserId);
      },
      (db, reader) =>
      {
        entities.ExistingActionTakenBy.SystemGeneratedId =
          db.GetInt32(reader, 0);
        entities.ExistingActionTakenBy.UserId = db.GetString(reader, 1);
        entities.ExistingActionTakenBy.LastName = db.GetString(reader, 2);
        entities.ExistingActionTakenBy.FirstName = db.GetString(reader, 3);
        entities.ExistingActionTakenBy.MiddleInitial = db.GetString(reader, 4);
        entities.ExistingActionTakenBy.Populated = true;
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
    /// A value of Fips.
    /// </summary>
    [JsonPropertyName("fips")]
    public Fips Fips
    {
      get => fips ??= new();
      set => fips = value;
    }

    /// <summary>
    /// A value of Tribunal.
    /// </summary>
    [JsonPropertyName("tribunal")]
    public Tribunal Tribunal
    {
      get => tribunal ??= new();
      set => tribunal = value;
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

    /// <summary>
    /// A value of AdministrativeAction.
    /// </summary>
    [JsonPropertyName("administrativeAction")]
    public AdministrativeAction AdministrativeAction
    {
      get => administrativeAction ??= new();
      set => administrativeAction = value;
    }

    /// <summary>
    /// A value of CreateForAllObligation.
    /// </summary>
    [JsonPropertyName("createForAllObligation")]
    public Common CreateForAllObligation
    {
      get => createForAllObligation ??= new();
      set => createForAllObligation = value;
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
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
    }

    /// <summary>
    /// A value of ObligationAdministrativeAction.
    /// </summary>
    [JsonPropertyName("obligationAdministrativeAction")]
    public ObligationAdministrativeAction ObligationAdministrativeAction
    {
      get => obligationAdministrativeAction ??= new();
      set => obligationAdministrativeAction = value;
    }

    private ObligationType obligationType;
    private Fips fips;
    private Tribunal tribunal;
    private LegalAction legalAction;
    private AdministrativeAction administrativeAction;
    private Common createForAllObligation;
    private Obligation obligation;
    private CsePerson csePerson;
    private ObligationAdministrativeAction obligationAdministrativeAction;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of Rollback.
    /// </summary>
    [JsonPropertyName("rollback")]
    public Common Rollback
    {
      get => rollback ??= new();
      set => rollback = value;
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
    /// A value of ActionTakenBy.
    /// </summary>
    [JsonPropertyName("actionTakenBy")]
    public ServiceProvider ActionTakenBy
    {
      get => actionTakenBy ??= new();
      set => actionTakenBy = value;
    }

    /// <summary>
    /// A value of ObligationAdministrativeAction.
    /// </summary>
    [JsonPropertyName("obligationAdministrativeAction")]
    public ObligationAdministrativeAction ObligationAdministrativeAction
    {
      get => obligationAdministrativeAction ??= new();
      set => obligationAdministrativeAction = value;
    }

    private Common rollback;
    private ObligationType obligationType;
    private ServiceProvider actionTakenBy;
    private ObligationAdministrativeAction obligationAdministrativeAction;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of ExistingFips.
    /// </summary>
    [JsonPropertyName("existingFips")]
    public Fips ExistingFips
    {
      get => existingFips ??= new();
      set => existingFips = value;
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
    /// A value of ExistingActionTakenBy.
    /// </summary>
    [JsonPropertyName("existingActionTakenBy")]
    public ServiceProvider ExistingActionTakenBy
    {
      get => existingActionTakenBy ??= new();
      set => existingActionTakenBy = value;
    }

    /// <summary>
    /// A value of ExistingTribunal.
    /// </summary>
    [JsonPropertyName("existingTribunal")]
    public Tribunal ExistingTribunal
    {
      get => existingTribunal ??= new();
      set => existingTribunal = value;
    }

    /// <summary>
    /// A value of ExistingLegalActionDetail.
    /// </summary>
    [JsonPropertyName("existingLegalActionDetail")]
    public LegalActionDetail ExistingLegalActionDetail
    {
      get => existingLegalActionDetail ??= new();
      set => existingLegalActionDetail = value;
    }

    /// <summary>
    /// A value of ExistingLegalAction.
    /// </summary>
    [JsonPropertyName("existingLegalAction")]
    public LegalAction ExistingLegalAction
    {
      get => existingLegalAction ??= new();
      set => existingLegalAction = value;
    }

    /// <summary>
    /// A value of ExistingAdministrativeAction.
    /// </summary>
    [JsonPropertyName("existingAdministrativeAction")]
    public AdministrativeAction ExistingAdministrativeAction
    {
      get => existingAdministrativeAction ??= new();
      set => existingAdministrativeAction = value;
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
    /// A value of ExistingObligation.
    /// </summary>
    [JsonPropertyName("existingObligation")]
    public Obligation ExistingObligation
    {
      get => existingObligation ??= new();
      set => existingObligation = value;
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
    /// A value of New1.
    /// </summary>
    [JsonPropertyName("new1")]
    public ObligationAdministrativeAction New1
    {
      get => new1 ??= new();
      set => new1 = value;
    }

    private Fips existingFips;
    private ObligationType existingObligationType;
    private ServiceProvider existingActionTakenBy;
    private Tribunal existingTribunal;
    private LegalActionDetail existingLegalActionDetail;
    private LegalAction existingLegalAction;
    private AdministrativeAction existingAdministrativeAction;
    private CsePersonAccount existingCsePersonAccount;
    private Obligation existingObligation;
    private CsePerson existingCsePerson;
    private ObligationAdministrativeAction new1;
  }
#endregion
}
