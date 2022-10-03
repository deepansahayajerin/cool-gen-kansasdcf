// Program: LE_EXMP_UPDATE_OBLN_ADMIN_EXMPN, ID: 372589936, model: 746.
// Short name: SWE00772
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
/// A program: LE_EXMP_UPDATE_OBLN_ADMIN_EXMPN.
/// </para>
/// <para>
/// RESP: LGLENFAC
/// This action block creates OBLIGATION_ADMINISTRATIVE_ACTION and associates 
/// with all or specified obligations.
/// </para>
/// </summary>
[Serializable]
public partial class LeExmpUpdateOblnAdminExmpn: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the LE_EXMP_UPDATE_OBLN_ADMIN_EXMPN program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new LeExmpUpdateOblnAdminExmpn(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of LeExmpUpdateOblnAdminExmpn.
  /// </summary>
  public LeExmpUpdateOblnAdminExmpn(IContext context, Import import,
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
    // 10/20/98 P. Sharp    Remove redundant reads of legal action, fips, and 
    // trib.  Added rollback flag. Removed used views.
    // ********************************************
    local.MaxDate.EndDate = UseCabSetMaximumDiscontinueDate();
    export.Rollback.Flag = "N";

    if (AsChar(import.UpdateForAllObligation.Flag) == 'Y')
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

      ExitState = "LE0000_CSE_PERSON_NOT_AN_OBLIGOR";

      return;
    }

    if (!ReadAdministrativeAction())
    {
      ExitState = "LE0000_INVALID_ADMIN_ACTION_TYPE";

      return;
    }

    if (!Lt(local.InitialisedToZeros.EndDate,
      import.ObligationAdmActionExemption.EndDate))
    {
      local.EndDate.EndDate = UseCabSetMaximumDiscontinueDate();
    }
    else
    {
      local.EndDate.EndDate = import.ObligationAdmActionExemption.EndDate;
    }

    local.Infrastructure.SituationNumber = 0;

    if (AsChar(import.UpdateForAllObligation.Flag) == 'Y')
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

        if (ReadObligationAdmActionExemption())
        {
          if (Lt(local.EndDate.EndDate, local.MaxDate.EndDate) && Equal
            (entities.ExistingObligationAdmActionExemption.EndDate,
            local.MaxDate.EndDate))
          {
            local.ExemptionRemoved.Flag = "Y";
          }
          else
          {
            local.ExemptionRemoved.Flag = "N";
          }

          try
          {
            UpdateObligationAdmActionExemption();
            MoveObligationAdmActionExemption2(entities.
              ExistingObligationAdmActionExemption,
              local.ObligationAdmActionExemption);

            if (AsChar(local.ExemptionRemoved.Flag) == 'Y')
            {
              // --- Exemption has been removed. Raise Infrastructure event.
              UseLeExmpRaiseInfrastrucEvents();

              if (!IsExitState("ACO_NN0000_ALL_OK"))
              {
                UseEabRollbackCics();

                return;
              }
            }
          }
          catch(Exception e)
          {
            switch(GetErrorCode(e))
            {
              case ErrorCode.AlreadyExists:
                export.Rollback.Flag = "Y";
                ExitState = "LE0000_OBLIG_ADM_ACTN_EXEMPT_NU";

                return;
              case ErrorCode.PermittedValueViolation:
                export.Rollback.Flag = "Y";
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
          // ---------------------------------------------
          // This is not an error. The worker is trying to update for all 
          // exemptions of all the obligations for the court order. Some legal
          // actions and their obligations may not have any exemptions defined
          // ---------------------------------------------
        }
      }
    }
    else
    {
      // ---------------------------------------------
      // Update only the given obligation
      // ---------------------------------------------
      if (ReadObligationObligationType())
      {
        if (ReadObligationAdmActionExemption())
        {
          if (Lt(local.EndDate.EndDate, local.MaxDate.EndDate) && Equal
            (entities.ExistingObligationAdmActionExemption.EndDate,
            local.MaxDate.EndDate))
          {
            local.ExemptionRemoved.Flag = "Y";
          }
          else
          {
            local.ExemptionRemoved.Flag = "N";
          }

          try
          {
            UpdateObligationAdmActionExemption();
            MoveObligationAdmActionExemption2(entities.
              ExistingObligationAdmActionExemption,
              local.ObligationAdmActionExemption);

            if (AsChar(local.ExemptionRemoved.Flag) == 'Y')
            {
              // --- Exemption has been removed. Raise Infrastructure event.
              UseLeExmpRaiseInfrastrucEvents();

              if (!IsExitState("ACO_NN0000_ALL_OK"))
              {
                UseEabRollbackCics();

                return;
              }
            }
          }
          catch(Exception e)
          {
            switch(GetErrorCode(e))
            {
              case ErrorCode.AlreadyExists:
                ExitState = "LE0000_OBLIG_ADM_ACTN_EXEMPT_NU";

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
          // ---------------------------------------------
          // This is an error. The worker is trying to update exemption for a 
          // specific obligation. So the exemption for that obligation must
          // exist
          // ---------------------------------------------
          ExitState = "LE0000_OBLIG_ADM_ACTN_EXEMPT_NF";

          return;
        }
      }
      else
      {
        ExitState = "FN0000_OBLIGATION_NF";
      }
    }

    if (!IsEmpty(local.ObligationAdmActionExemption.LastUpdatedBy))
    {
      if (ReadServiceProvider())
      {
        export.UpdatedBy.Assign(entities.ExistingUpdatedBy);
      }
    }
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
    target.LastUpdatedBy = source.LastUpdatedBy;
    target.LastUpdatedTstamp = source.LastUpdatedTstamp;
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
    MoveObligationAdmActionExemption1(entities.
      ExistingObligationAdmActionExemption,
      useImport.ObligationAdmActionExemption);
    useImport.Obligation.SystemGeneratedIdentifier =
      entities.ExistingObligation.SystemGeneratedIdentifier;
    useImport.ObligationType.Assign(entities.ExistingObligationType);

    Call(LeExmpRaiseInfrastrucEvents.Execute, useImport, useExport);

    local.Infrastructure.SituationNumber =
      useExport.Infrastructure.SituationNumber;
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

  private bool ReadObligationAdmActionExemption()
  {
    System.Diagnostics.Debug.Assert(entities.ExistingObligation.Populated);
    entities.ExistingObligationAdmActionExemption.Populated = false;

    return Read("ReadObligationAdmActionExemption",
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
        db.SetString(
          command, "aatType", entities.ExistingAdministrativeAction.Type1);
        db.SetDate(
          command, "effectiveDt",
          import.ObligationAdmActionExemption.EffectiveDate.
            GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ExistingObligationAdmActionExemption.OtyType =
          db.GetInt32(reader, 0);
        entities.ExistingObligationAdmActionExemption.AatType =
          db.GetString(reader, 1);
        entities.ExistingObligationAdmActionExemption.ObgGeneratedId =
          db.GetInt32(reader, 2);
        entities.ExistingObligationAdmActionExemption.CspNumber =
          db.GetString(reader, 3);
        entities.ExistingObligationAdmActionExemption.CpaType =
          db.GetString(reader, 4);
        entities.ExistingObligationAdmActionExemption.EffectiveDate =
          db.GetDate(reader, 5);
        entities.ExistingObligationAdmActionExemption.EndDate =
          db.GetDate(reader, 6);
        entities.ExistingObligationAdmActionExemption.LastName =
          db.GetString(reader, 7);
        entities.ExistingObligationAdmActionExemption.FirstName =
          db.GetString(reader, 8);
        entities.ExistingObligationAdmActionExemption.MiddleInitial =
          db.GetNullableString(reader, 9);
        entities.ExistingObligationAdmActionExemption.Suffix =
          db.GetNullableString(reader, 10);
        entities.ExistingObligationAdmActionExemption.Reason =
          db.GetString(reader, 11);
        entities.ExistingObligationAdmActionExemption.LastUpdatedBy =
          db.GetNullableString(reader, 12);
        entities.ExistingObligationAdmActionExemption.LastUpdatedTstamp =
          db.GetNullableDateTime(reader, 13);
        entities.ExistingObligationAdmActionExemption.Description =
          db.GetNullableString(reader, 14);
        entities.ExistingObligationAdmActionExemption.Populated = true;
        CheckValid<ObligationAdmActionExemption>("CpaType",
          entities.ExistingObligationAdmActionExemption.CpaType);
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

  private bool ReadServiceProvider()
  {
    entities.ExistingUpdatedBy.Populated = false;

    return Read("ReadServiceProvider",
      (db, command) =>
      {
        db.SetString(
          command, "userId",
          local.ObligationAdmActionExemption.LastUpdatedBy ?? "");
      },
      (db, reader) =>
      {
        entities.ExistingUpdatedBy.SystemGeneratedId = db.GetInt32(reader, 0);
        entities.ExistingUpdatedBy.UserId = db.GetString(reader, 1);
        entities.ExistingUpdatedBy.LastName = db.GetString(reader, 2);
        entities.ExistingUpdatedBy.FirstName = db.GetString(reader, 3);
        entities.ExistingUpdatedBy.MiddleInitial = db.GetString(reader, 4);
        entities.ExistingUpdatedBy.Populated = true;
      });
  }

  private void UpdateObligationAdmActionExemption()
  {
    System.Diagnostics.Debug.Assert(
      entities.ExistingObligationAdmActionExemption.Populated);

    var endDate = local.EndDate.EndDate;
    var lastName = import.ObligationAdmActionExemption.LastName;
    var firstName = import.ObligationAdmActionExemption.FirstName;
    var middleInitial = import.ObligationAdmActionExemption.MiddleInitial ?? "";
    var suffix = import.ObligationAdmActionExemption.Suffix ?? "";
    var reason = import.ObligationAdmActionExemption.Reason;
    var lastUpdatedBy = global.UserId;
    var lastUpdatedTstamp = Now();
    var description = import.ObligationAdmActionExemption.Description ?? "";

    entities.ExistingObligationAdmActionExemption.Populated = false;
    Update("UpdateObligationAdmActionExemption",
      (db, command) =>
      {
        db.SetDate(command, "endDt", endDate);
        db.SetString(command, "lastNm", lastName);
        db.SetString(command, "firstNm", firstName);
        db.SetNullableString(command, "middleInitial", middleInitial);
        db.SetNullableString(command, "suffix", suffix);
        db.SetString(command, "reason", reason);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetNullableDateTime(command, "lastUpdTstamp", lastUpdatedTstamp);
        db.SetNullableString(command, "description", description);
        db.SetInt32(
          command, "otyType",
          entities.ExistingObligationAdmActionExemption.OtyType);
        db.SetString(
          command, "aatType",
          entities.ExistingObligationAdmActionExemption.AatType);
        db.SetInt32(
          command, "obgGeneratedId",
          entities.ExistingObligationAdmActionExemption.ObgGeneratedId);
        db.SetString(
          command, "cspNumber",
          entities.ExistingObligationAdmActionExemption.CspNumber);
        db.SetString(
          command, "cpaType",
          entities.ExistingObligationAdmActionExemption.CpaType);
        db.SetDate(
          command, "effectiveDt",
          entities.ExistingObligationAdmActionExemption.EffectiveDate.
            GetValueOrDefault());
      });

    entities.ExistingObligationAdmActionExemption.EndDate = endDate;
    entities.ExistingObligationAdmActionExemption.LastName = lastName;
    entities.ExistingObligationAdmActionExemption.FirstName = firstName;
    entities.ExistingObligationAdmActionExemption.MiddleInitial = middleInitial;
    entities.ExistingObligationAdmActionExemption.Suffix = suffix;
    entities.ExistingObligationAdmActionExemption.Reason = reason;
    entities.ExistingObligationAdmActionExemption.LastUpdatedBy = lastUpdatedBy;
    entities.ExistingObligationAdmActionExemption.LastUpdatedTstamp =
      lastUpdatedTstamp;
    entities.ExistingObligationAdmActionExemption.Description = description;
    entities.ExistingObligationAdmActionExemption.Populated = true;
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
    /// A value of AdministrativeAction.
    /// </summary>
    [JsonPropertyName("administrativeAction")]
    public AdministrativeAction AdministrativeAction
    {
      get => administrativeAction ??= new();
      set => administrativeAction = value;
    }

    /// <summary>
    /// A value of UpdateForAllObligation.
    /// </summary>
    [JsonPropertyName("updateForAllObligation")]
    public Common UpdateForAllObligation
    {
      get => updateForAllObligation ??= new();
      set => updateForAllObligation = value;
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
    /// A value of LegalAction.
    /// </summary>
    [JsonPropertyName("legalAction")]
    public LegalAction LegalAction
    {
      get => legalAction ??= new();
      set => legalAction = value;
    }

    private ObligationType obligationType;
    private Tribunal foreign;
    private Fips fips;
    private ObligationAdmActionExemption obligationAdmActionExemption;
    private AdministrativeAction administrativeAction;
    private Common updateForAllObligation;
    private Obligation obligation;
    private CsePerson csePerson;
    private LegalAction legalAction;
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
    /// A value of UpdatedBy.
    /// </summary>
    [JsonPropertyName("updatedBy")]
    public ServiceProvider UpdatedBy
    {
      get => updatedBy ??= new();
      set => updatedBy = value;
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

    private Common rollback;
    private ServiceProvider updatedBy;
    private ObligationAdmActionExemption obligationAdmActionExemption;
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
    /// A value of ZeroDate.
    /// </summary>
    [JsonPropertyName("zeroDate")]
    public DateWorkArea ZeroDate
    {
      get => zeroDate ??= new();
      set => zeroDate = value;
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
    /// A value of ExemptionRemoved.
    /// </summary>
    [JsonPropertyName("exemptionRemoved")]
    public Common ExemptionRemoved
    {
      get => exemptionRemoved ??= new();
      set => exemptionRemoved = value;
    }

    /// <summary>
    /// A value of MaxDate.
    /// </summary>
    [JsonPropertyName("maxDate")]
    public ObligationAdmActionExemption MaxDate
    {
      get => maxDate ??= new();
      set => maxDate = value;
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
    /// A value of InitialisedToZeros.
    /// </summary>
    [JsonPropertyName("initialisedToZeros")]
    public ObligationAdmActionExemption InitialisedToZeros
    {
      get => initialisedToZeros ??= new();
      set => initialisedToZeros = value;
    }

    private ObligationAdmActionExemption obligationAdmActionExemption;
    private DateWorkArea zeroDate;
    private Infrastructure infrastructure;
    private Common exemptionRemoved;
    private ObligationAdmActionExemption maxDate;
    private ObligationAdmActionExemption endDate;
    private ObligationAdmActionExemption initialisedToZeros;
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
    /// A value of ExistingUpdatedBy.
    /// </summary>
    [JsonPropertyName("existingUpdatedBy")]
    public ServiceProvider ExistingUpdatedBy
    {
      get => existingUpdatedBy ??= new();
      set => existingUpdatedBy = value;
    }

    /// <summary>
    /// A value of ExistingObligationAdmActionExemption.
    /// </summary>
    [JsonPropertyName("existingObligationAdmActionExemption")]
    public ObligationAdmActionExemption ExistingObligationAdmActionExemption
    {
      get => existingObligationAdmActionExemption ??= new();
      set => existingObligationAdmActionExemption = value;
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
    /// A value of ExistingLegalAction.
    /// </summary>
    [JsonPropertyName("existingLegalAction")]
    public LegalAction ExistingLegalAction
    {
      get => existingLegalAction ??= new();
      set => existingLegalAction = value;
    }

    private ObligationType existingObligationType;
    private Tribunal existingTribunal;
    private Fips existingFips;
    private ServiceProvider existingUpdatedBy;
    private ObligationAdmActionExemption existingObligationAdmActionExemption;
    private AdministrativeAction existingAdministrativeAction;
    private CsePersonAccount existingCsePersonAccount;
    private Obligation existingObligation;
    private CsePerson existingCsePerson;
    private LegalAction existingLegalAction;
  }
#endregion
}
