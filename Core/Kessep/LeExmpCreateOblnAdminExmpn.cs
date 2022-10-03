// Program: LE_EXMP_CREATE_OBLN_ADMIN_EXMPN, ID: 372589937, model: 746.
// Short name: SWE00769
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
/// A program: LE_EXMP_CREATE_OBLN_ADMIN_EXMPN.
/// </para>
/// <para>
/// RESP: LGLENFAC
/// This action block creates OBLIGATION_ADMINISTRATIVE_ACTION and associates 
/// with all or specified obligations.
/// </para>
/// </summary>
[Serializable]
public partial class LeExmpCreateOblnAdminExmpn: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the LE_EXMP_CREATE_OBLN_ADMIN_EXMPN program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new LeExmpCreateOblnAdminExmpn(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of LeExmpCreateOblnAdminExmpn.
  /// </summary>
  public LeExmpCreateOblnAdminExmpn(IContext context, Import import,
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
    // ********************************************
    // 12/15/97 R Grey	   SP remove USE Get_Next_Sit_no / ADD Exit States
    // 10/20/98 P. Sharp    Removed redundant reads of legal action, fips and 
    // trib. Cleaned up views. Added rollback flag.
    // ********************************************
    export.LegalAction.CourtCaseNumber = import.LegalAction.CourtCaseNumber;
    MoveFips(import.Fips, export.Fips);
    export.ObligationType.Assign(import.ObligationType);
    export.RollbackFlag.Flag = "N";

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

    if (!Lt(local.Initialised.EndDate,
      import.ObligationAdmActionExemption.EndDate))
    {
      local.EndDate.EndDate = UseCabSetMaximumDiscontinueDate();
    }
    else
    {
      local.EndDate.EndDate = import.ObligationAdmActionExemption.EndDate;
    }

    local.Infrastructure.SituationNumber = 0;

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
          .Foreign.Identifier)
        {
          continue;
        }

        export.Obligation.SystemGeneratedIdentifier =
          entities.ExistingObligation.SystemGeneratedIdentifier;

        if (ReadObligationType())
        {
          export.ObligationType.Assign(entities.ExistingObligationType);
        }

        try
        {
          CreateObligationAdmActionExemption();
          MoveObligationAdmActionExemption2(entities.New1,
            local.ObligationAdmActionExemption);
          UseLeExmpRaiseInfrastrucEvents();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            UseEabRollbackCics();

            return;
          }
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              ExitState = "LE0000_OBLIG_ADM_ACT_EXEMPTN_AE";
              export.RollbackFlag.Flag = "Y";

              return;
            case ErrorCode.PermittedValueViolation:
              ExitState = "LE0000_OBLIG_ADM_ACTN_EXEMPT_PV";
              export.RollbackFlag.Flag = "Y";

              return;
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
        export.Obligation.SystemGeneratedIdentifier =
          entities.ExistingObligation.SystemGeneratedIdentifier;
        export.ObligationType.Assign(entities.ExistingObligationType);

        try
        {
          CreateObligationAdmActionExemption();
          MoveObligationAdmActionExemption2(entities.New1,
            local.ObligationAdmActionExemption);
          UseLeExmpRaiseInfrastrucEvents();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            UseEabRollbackCics();

            return;
          }
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              ExitState = "LE0000_OBLIG_ADM_ACT_EXEMPTN_AE";

              return;
            case ErrorCode.PermittedValueViolation:
              ExitState = "LE0000_OBLIG_ADM_ACTN_EXEMPT_PV";

              return;
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

    if (!IsEmpty(local.ObligationAdmActionExemption.CreatedBy))
    {
      if (ReadServiceProvider())
      {
        export.CreatedBy.Assign(entities.ExistingCreatedBy);
      }
    }
  }

  private static void MoveFips(Fips source, Fips target)
  {
    target.StateAbbreviation = source.StateAbbreviation;
    target.CountyAbbreviation = source.CountyAbbreviation;
  }

  private static void MoveObligationAdmActionExemption1(
    ObligationAdmActionExemption source, ObligationAdmActionExemption target)
  {
    target.EffectiveDate = source.EffectiveDate;
    target.EndDate = source.EndDate;
  }

  private static void MoveObligationAdmActionExemption2(
    ObligationAdmActionExemption source, ObligationAdmActionExemption target)
  {
    target.CreatedBy = source.CreatedBy;
    target.CreatedTstamp = source.CreatedTstamp;
  }

  private DateTime? UseCabSetMaximumDiscontinueDate()
  {
    var useImport = new CabSetMaximumDiscontinueDate.Import();
    var useExport = new CabSetMaximumDiscontinueDate.Export();

    Call(CabSetMaximumDiscontinueDate.Execute, useImport, useExport);

    return useExport.DateWorkArea.Date;
  }

  private void UseEabRollbackCics()
  {
    var useImport = new EabRollbackCics.Import();
    var useExport = new EabRollbackCics.Export();

    Call(EabRollbackCics.Execute, useImport, useExport);
  }

  private void UseLeExmpRaiseInfrastrucEvents()
  {
    var useImport = new LeExmpRaiseInfrastrucEvents.Import();
    var useExport = new LeExmpRaiseInfrastrucEvents.Export();

    useImport.Infrastructure.SituationNumber =
      local.Infrastructure.SituationNumber;
    useImport.Obligor.Number = import.CsePerson.Number;
    useImport.AdministrativeAction.Type1 = import.AdministrativeAction.Type1;
    MoveObligationAdmActionExemption1(entities.New1,
      useImport.ObligationAdmActionExemption);
    useImport.Obligation.SystemGeneratedIdentifier =
      entities.ExistingObligation.SystemGeneratedIdentifier;
    useImport.ObligationType.Assign(entities.ExistingObligationType);

    Call(LeExmpRaiseInfrastrucEvents.Execute, useImport, useExport);

    local.Infrastructure.SituationNumber =
      useExport.Infrastructure.SituationNumber;
  }

  private void CreateObligationAdmActionExemption()
  {
    System.Diagnostics.Debug.Assert(entities.ExistingObligation.Populated);

    var otyType = entities.ExistingObligation.DtyGeneratedId;
    var aatType = entities.ExistingAdministrativeAction.Type1;
    var obgGeneratedId = entities.ExistingObligation.SystemGeneratedIdentifier;
    var cspNumber = entities.ExistingObligation.CspNumber;
    var cpaType = entities.ExistingObligation.CpaType;
    var effectiveDate = import.ObligationAdmActionExemption.EffectiveDate;
    var endDate = local.EndDate.EndDate;
    var lastName = import.ObligationAdmActionExemption.LastName;
    var firstName = import.ObligationAdmActionExemption.FirstName;
    var middleInitial = import.ObligationAdmActionExemption.MiddleInitial ?? "";
    var suffix = import.ObligationAdmActionExemption.Suffix ?? "";
    var reason = import.ObligationAdmActionExemption.Reason;
    var createdBy = global.UserId;
    var createdTstamp = Now();
    var description = import.ObligationAdmActionExemption.Description ?? "";

    CheckValid<ObligationAdmActionExemption>("CpaType", cpaType);
    entities.New1.Populated = false;
    Update("CreateObligationAdmActionExemption",
      (db, command) =>
      {
        db.SetInt32(command, "otyType", otyType);
        db.SetString(command, "aatType", aatType);
        db.SetInt32(command, "obgGeneratedId", obgGeneratedId);
        db.SetString(command, "cspNumber", cspNumber);
        db.SetString(command, "cpaType", cpaType);
        db.SetDate(command, "effectiveDt", effectiveDate);
        db.SetDate(command, "endDt", endDate);
        db.SetString(command, "lastNm", lastName);
        db.SetString(command, "firstNm", firstName);
        db.SetNullableString(command, "middleInitial", middleInitial);
        db.SetNullableString(command, "suffix", suffix);
        db.SetString(command, "reason", reason);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTstamp", createdTstamp);
        db.SetNullableString(command, "lastUpdatedBy", "");
        db.SetNullableDateTime(command, "lastUpdTstamp", default(DateTime));
        db.SetNullableString(command, "description", description);
      });

    entities.New1.OtyType = otyType;
    entities.New1.AatType = aatType;
    entities.New1.ObgGeneratedId = obgGeneratedId;
    entities.New1.CspNumber = cspNumber;
    entities.New1.CpaType = cpaType;
    entities.New1.EffectiveDate = effectiveDate;
    entities.New1.EndDate = endDate;
    entities.New1.LastName = lastName;
    entities.New1.FirstName = firstName;
    entities.New1.MiddleInitial = middleInitial;
    entities.New1.Suffix = suffix;
    entities.New1.Reason = reason;
    entities.New1.CreatedBy = createdBy;
    entities.New1.CreatedTstamp = createdTstamp;
    entities.New1.Description = description;
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
    entities.ExistingCreatedBy.Populated = false;

    return Read("ReadServiceProvider",
      (db, command) =>
      {
        db.SetString(
          command, "userId", local.ObligationAdmActionExemption.CreatedBy);
      },
      (db, reader) =>
      {
        entities.ExistingCreatedBy.SystemGeneratedId = db.GetInt32(reader, 0);
        entities.ExistingCreatedBy.UserId = db.GetString(reader, 1);
        entities.ExistingCreatedBy.LastName = db.GetString(reader, 2);
        entities.ExistingCreatedBy.FirstName = db.GetString(reader, 3);
        entities.ExistingCreatedBy.MiddleInitial = db.GetString(reader, 4);
        entities.ExistingCreatedBy.Populated = true;
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
    /// A value of ObligationType.
    /// </summary>
    [JsonPropertyName("obligationType")]
    public ObligationType ObligationType
    {
      get => obligationType ??= new();
      set => obligationType = value;
    }

    /// <summary>
    /// A value of Foreign.
    /// </summary>
    [JsonPropertyName("foreign")]
    public Tribunal Foreign
    {
      get => foreign ??= new();
      set => foreign = value;
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
    /// A value of ObligationAdmActionExemption.
    /// </summary>
    [JsonPropertyName("obligationAdmActionExemption")]
    public ObligationAdmActionExemption ObligationAdmActionExemption
    {
      get => obligationAdmActionExemption ??= new();
      set => obligationAdmActionExemption = value;
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

    private ObligationType obligationType;
    private Tribunal foreign;
    private Fips fips;
    private ObligationAdmActionExemption obligationAdmActionExemption;
    private LegalAction legalAction;
    private AdministrativeAction administrativeAction;
    private Common createForAllObligation;
    private Obligation obligation;
    private CsePerson csePerson;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of RollbackFlag.
    /// </summary>
    [JsonPropertyName("rollbackFlag")]
    public Common RollbackFlag
    {
      get => rollbackFlag ??= new();
      set => rollbackFlag = value;
    }

    /// <summary>
    /// A value of Foreign.
    /// </summary>
    [JsonPropertyName("foreign")]
    public FipsTribAddress Foreign
    {
      get => foreign ??= new();
      set => foreign = value;
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
    /// A value of LegalAction.
    /// </summary>
    [JsonPropertyName("legalAction")]
    public LegalAction LegalAction
    {
      get => legalAction ??= new();
      set => legalAction = value;
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
    /// A value of Fips.
    /// </summary>
    [JsonPropertyName("fips")]
    public Fips Fips
    {
      get => fips ??= new();
      set => fips = value;
    }

    /// <summary>
    /// A value of CreatedBy.
    /// </summary>
    [JsonPropertyName("createdBy")]
    public ServiceProvider CreatedBy
    {
      get => createdBy ??= new();
      set => createdBy = value;
    }

    private Common rollbackFlag;
    private FipsTribAddress foreign;
    private Obligation obligation;
    private ObligationType obligationType;
    private LegalAction legalAction;
    private Tribunal tribunal;
    private Fips fips;
    private ServiceProvider createdBy;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of ObligationAdmActionExemption.
    /// </summary>
    [JsonPropertyName("obligationAdmActionExemption")]
    public ObligationAdmActionExemption ObligationAdmActionExemption
    {
      get => obligationAdmActionExemption ??= new();
      set => obligationAdmActionExemption = value;
    }

    /// <summary>
    /// A value of Infrastructure.
    /// </summary>
    [JsonPropertyName("infrastructure")]
    public Infrastructure Infrastructure
    {
      get => infrastructure ??= new();
      set => infrastructure = value;
    }

    /// <summary>
    /// A value of EndDate.
    /// </summary>
    [JsonPropertyName("endDate")]
    public ObligationAdmActionExemption EndDate
    {
      get => endDate ??= new();
      set => endDate = value;
    }

    /// <summary>
    /// A value of Initialised.
    /// </summary>
    [JsonPropertyName("initialised")]
    public ObligationAdmActionExemption Initialised
    {
      get => initialised ??= new();
      set => initialised = value;
    }

    private ObligationAdmActionExemption obligationAdmActionExemption;
    private Infrastructure infrastructure;
    private ObligationAdmActionExemption endDate;
    private ObligationAdmActionExemption initialised;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
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
    /// A value of ExistingTribunal.
    /// </summary>
    [JsonPropertyName("existingTribunal")]
    public Tribunal ExistingTribunal
    {
      get => existingTribunal ??= new();
      set => existingTribunal = value;
    }

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
    /// A value of ExistingCreatedBy.
    /// </summary>
    [JsonPropertyName("existingCreatedBy")]
    public ServiceProvider ExistingCreatedBy
    {
      get => existingCreatedBy ??= new();
      set => existingCreatedBy = value;
    }

    /// <summary>
    /// A value of New1.
    /// </summary>
    [JsonPropertyName("new1")]
    public ObligationAdmActionExemption New1
    {
      get => new1 ??= new();
      set => new1 = value;
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

    private ObligationType existingObligationType;
    private Tribunal existingTribunal;
    private Fips existingFips;
    private ServiceProvider existingCreatedBy;
    private ObligationAdmActionExemption new1;
    private LegalAction existingLegalAction;
    private AdministrativeAction existingAdministrativeAction;
    private CsePersonAccount existingCsePersonAccount;
    private Obligation existingObligation;
    private CsePerson existingCsePerson;
  }
#endregion
}
