// Program: LE_EXMP_DISPLAY_OBLN_ADMIN_EXMPN, ID: 372589940, model: 746.
// Short name: SWE00771
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
/// A program: LE_EXMP_DISPLAY_OBLN_ADMIN_EXMPN.
/// </para>
/// <para>
/// RESP: LGLENFAC
/// This action block creates OBLIGATION_ADMINISTRATIVE_ACTION and associates 
/// with all or specified obligations.
/// </para>
/// </summary>
[Serializable]
public partial class LeExmpDisplayOblnAdminExmpn: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the LE_EXMP_DISPLAY_OBLN_ADMIN_EXMPN program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new LeExmpDisplayOblnAdminExmpn(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of LeExmpDisplayOblnAdminExmpn.
  /// </summary>
  public LeExmpDisplayOblnAdminExmpn(IContext context, Import import,
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
    // 10/13/98    P. Sharp    Cleaned up duplicated code and removed used 
    // views.
    export.Obligation.SystemGeneratedIdentifier =
      import.Obligation.SystemGeneratedIdentifier;
    export.ObligationType.Assign(import.ObligationType);
    MoveFips(import.Fips, export.Fips);
    MoveLegalAction(import.LegalAction, export.LegalAction);
    export.AdministrativeAction.Type1 = import.AdministrativeAction.Type1;
    export.ObligationAdmActionExemption.EffectiveDate =
      import.ObligationAdmActionExemption.EffectiveDate;

    if (!IsEmpty(import.AdministrativeAction.Type1))
    {
      if (ReadAdministrativeAction())
      {
        MoveAdministrativeAction(entities.ExistingAdministrativeAction,
          export.AdministrativeAction);
      }
      else
      {
        ExitState = "LE0000_INVALID_ADMIN_ACT_TYPE";

        return;
      }
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

    if (AsChar(import.DisplayAnyObligation.Flag) == 'N')
    {
      if (import.Obligation.SystemGeneratedIdentifier == 0)
      {
        ExitState = "LE0000_OBLIGATION_REQD";

        return;
      }
    }

    local.ObligationFound.Flag = "N";
    local.AdminActExemptionFound.Flag = "N";

    if (AsChar(import.DisplayAnyObligation.Flag) == 'Y')
    {
      if (!IsEmpty(import.LegalAction.CourtCaseNumber))
      {
        foreach(var item in ReadObligationLegalActionTribunal())
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

            export.Fips.Assign(entities.ExistingFips);
          }
          else
          {
            if (entities.ExistingTribunal.Identifier != import
              .Tribunal.Identifier)
            {
              continue;
            }

            if (ReadFipsTribAddress())
            {
              export.Foreign.Country = entities.ExistingForeign.Country;
            }
          }

          local.ObligationFound.Flag = "Y";
          MoveLegalAction(entities.ExistingLegalAction, export.LegalAction);
          export.Tribunal.Assign(entities.ExistingTribunal);
          MoveObligation(entities.ExistingObligation, export.Obligation);

          if (ReadObligationType())
          {
            export.ObligationType.Assign(entities.ExistingObligationType);
          }

          break;
        }
      }
      else if (ReadObligation())
      {
        local.ObligationFound.Flag = "Y";
        MoveObligation(entities.ExistingObligation, export.Obligation);

        if (ReadObligationType())
        {
          export.ObligationType.Assign(entities.ExistingObligationType);
        }

        if (ReadLegalAction())
        {
          MoveLegalAction(entities.ExistingLegalAction, export.LegalAction);

          if (ReadTribunal())
          {
            export.Tribunal.Assign(entities.ExistingTribunal);

            if (ReadFips())
            {
              export.Fips.Assign(entities.ExistingFips);
            }
            else if (ReadFipsTribAddress())
            {
              export.Foreign.Country = entities.ExistingForeign.Country;
            }
          }
        }
      }
    }
    else if (ReadObligationObligationType())
    {
      local.ObligationFound.Flag = "Y";
      MoveObligation(entities.ExistingObligation, export.Obligation);
      export.ObligationType.Assign(entities.ExistingObligationType);

      if (ReadLegalAction())
      {
        MoveLegalAction(entities.ExistingLegalAction, export.LegalAction);

        if (ReadTribunal())
        {
          export.Tribunal.Assign(entities.ExistingTribunal);

          if (ReadFips())
          {
            export.Fips.Assign(entities.ExistingFips);
          }
          else if (ReadFipsTribAddress())
          {
            export.Foreign.Country = entities.ExistingForeign.Country;
          }
        }
      }
    }
    else
    {
      ExitState = "FN0000_OBLIGATION_NF";

      return;
    }

    if (AsChar(local.ObligationFound.Flag) == 'N')
    {
      ExitState = "LE0000_NO_OBLIGATION_FOUND";

      return;
    }

    if (export.LegalAction.Identifier > 0)
    {
      UseLeGetPetitionerRespondent();
    }

    if (AsChar(local.ObligationFound.Flag) == 'Y')
    {
      foreach(var item in ReadObligationAdmActionExemptionAdministrativeAction())
        
      {
        if (Lt(local.InitialisedObligationAdmActionExemption.EffectiveDate,
          import.ObligationAdmActionExemption.EffectiveDate))
        {
          if (Equal(entities.ExistingObligationAdmActionExemption.EffectiveDate,
            import.ObligationAdmActionExemption.EffectiveDate))
          {
          }
          else
          {
            continue;
          }
        }

        if (!IsEmpty(import.AdministrativeAction.Type1))
        {
          if (Equal(entities.ExistingAdministrativeAction.Type1,
            import.AdministrativeAction.Type1))
          {
          }
          else
          {
            continue;
          }
        }

        export.ObligationAdmActionExemption.Assign(
          entities.ExistingObligationAdmActionExemption);
        MoveAdministrativeAction(entities.ExistingAdministrativeAction,
          export.AdministrativeAction);
        local.AdminActExemptionFound.Flag = "Y";

        break;
      }
    }

    local.DateWorkArea.Date = export.ObligationAdmActionExemption.EndDate;
    export.ObligationAdmActionExemption.EndDate =
      UseCabSetMaximumDiscontinueDate();

    if (AsChar(local.AdminActExemptionFound.Flag) == 'N')
    {
      ExitState = "LE0000_ADMIN_ACT_EXMP_NF";
    }
    else if (!IsEmpty(export.ObligationAdmActionExemption.CreatedBy))
    {
      if (ReadServiceProvider1())
      {
        export.CreatedBy.Assign(entities.ExistingServiceProvider);
      }

      if (ReadServiceProvider2())
      {
        export.UpdatedBy.Assign(entities.ExistingServiceProvider);
      }
    }
  }

  private static void MoveAdministrativeAction(AdministrativeAction source,
    AdministrativeAction target)
  {
    target.Type1 = source.Type1;
    target.Description = source.Description;
  }

  private static void MoveFips(Fips source, Fips target)
  {
    target.StateAbbreviation = source.StateAbbreviation;
    target.CountyAbbreviation = source.CountyAbbreviation;
  }

  private static void MoveLegalAction(LegalAction source, LegalAction target)
  {
    target.Identifier = source.Identifier;
    target.CourtCaseNumber = source.CourtCaseNumber;
  }

  private static void MoveObligation(Obligation source, Obligation target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Description = source.Description;
  }

  private DateTime? UseCabSetMaximumDiscontinueDate()
  {
    var useImport = new CabSetMaximumDiscontinueDate.Import();
    var useExport = new CabSetMaximumDiscontinueDate.Export();

    useImport.DateWorkArea.Date = local.DateWorkArea.Date;

    Call(CabSetMaximumDiscontinueDate.Execute, useImport, useExport);

    return useExport.DateWorkArea.Date;
  }

  private void UseLeGetPetitionerRespondent()
  {
    var useImport = new LeGetPetitionerRespondent.Import();
    var useExport = new LeGetPetitionerRespondent.Export();

    useImport.LegalAction.Identifier = export.LegalAction.Identifier;

    Call(LeGetPetitionerRespondent.Execute, useImport, useExport);

    export.PetitionerRespondentDetails.Assign(
      useExport.PetitionerRespondentDetails);
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
        entities.ExistingAdministrativeAction.Description =
          db.GetString(reader, 1);
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

  private bool ReadFipsTribAddress()
  {
    entities.ExistingForeign.Populated = false;

    return Read("ReadFipsTribAddress",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "trbId", entities.ExistingTribunal.Identifier);
      },
      (db, reader) =>
      {
        entities.ExistingForeign.Identifier = db.GetInt32(reader, 0);
        entities.ExistingForeign.Country = db.GetNullableString(reader, 1);
        entities.ExistingForeign.TrbId = db.GetNullableInt32(reader, 2);
        entities.ExistingForeign.Populated = true;
      });
  }

  private bool ReadLegalAction()
  {
    System.Diagnostics.Debug.Assert(entities.ExistingObligation.Populated);
    entities.ExistingLegalAction.Populated = false;

    return Read("ReadLegalAction",
      (db, command) =>
      {
        db.SetInt32(
          command, "legalActionId",
          entities.ExistingObligation.LgaId.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ExistingLegalAction.Identifier = db.GetInt32(reader, 0);
        entities.ExistingLegalAction.CourtCaseNumber =
          db.GetNullableString(reader, 1);
        entities.ExistingLegalAction.TrbId = db.GetNullableInt32(reader, 2);
        entities.ExistingLegalAction.Populated = true;
      });
  }

  private bool ReadObligation()
  {
    System.Diagnostics.Debug.
      Assert(entities.ExistingCsePersonAccount.Populated);
    entities.ExistingObligation.Populated = false;

    return Read("ReadObligation",
      (db, command) =>
      {
        db.
          SetString(command, "cpaType", entities.ExistingCsePersonAccount.Type1);
          
        db.SetString(
          command, "cspNumber", entities.ExistingCsePersonAccount.CspNumber);
      },
      (db, reader) =>
      {
        entities.ExistingObligation.CpaType = db.GetString(reader, 0);
        entities.ExistingObligation.CspNumber = db.GetString(reader, 1);
        entities.ExistingObligation.SystemGeneratedIdentifier =
          db.GetInt32(reader, 2);
        entities.ExistingObligation.DtyGeneratedId = db.GetInt32(reader, 3);
        entities.ExistingObligation.LgaId = db.GetNullableInt32(reader, 4);
        entities.ExistingObligation.Description =
          db.GetNullableString(reader, 5);
        entities.ExistingObligation.Populated = true;
        CheckValid<Obligation>("CpaType", entities.ExistingObligation.CpaType);
      });
  }

  private IEnumerable<bool>
    ReadObligationAdmActionExemptionAdministrativeAction()
  {
    System.Diagnostics.Debug.Assert(entities.ExistingObligation.Populated);
    entities.ExistingObligationAdmActionExemption.Populated = false;
    entities.ExistingAdministrativeAction.Populated = false;

    return ReadEach("ReadObligationAdmActionExemptionAdministrativeAction",
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
      },
      (db, reader) =>
      {
        entities.ExistingObligationAdmActionExemption.OtyType =
          db.GetInt32(reader, 0);
        entities.ExistingObligationAdmActionExemption.AatType =
          db.GetString(reader, 1);
        entities.ExistingAdministrativeAction.Type1 = db.GetString(reader, 1);
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
        entities.ExistingObligationAdmActionExemption.CreatedBy =
          db.GetString(reader, 12);
        entities.ExistingObligationAdmActionExemption.CreatedTstamp =
          db.GetDateTime(reader, 13);
        entities.ExistingObligationAdmActionExemption.LastUpdatedBy =
          db.GetNullableString(reader, 14);
        entities.ExistingObligationAdmActionExemption.LastUpdatedTstamp =
          db.GetNullableDateTime(reader, 15);
        entities.ExistingObligationAdmActionExemption.Description =
          db.GetNullableString(reader, 16);
        entities.ExistingAdministrativeAction.Description =
          db.GetString(reader, 17);
        entities.ExistingObligationAdmActionExemption.Populated = true;
        entities.ExistingAdministrativeAction.Populated = true;
        CheckValid<ObligationAdmActionExemption>("CpaType",
          entities.ExistingObligationAdmActionExemption.CpaType);

        return true;
      });
  }

  private IEnumerable<bool> ReadObligationLegalActionTribunal()
  {
    System.Diagnostics.Debug.
      Assert(entities.ExistingCsePersonAccount.Populated);
    entities.ExistingObligation.Populated = false;
    entities.ExistingLegalAction.Populated = false;
    entities.ExistingTribunal.Populated = false;

    return ReadEach("ReadObligationLegalActionTribunal",
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
        entities.ExistingLegalAction.Identifier = db.GetInt32(reader, 4);
        entities.ExistingObligation.Description =
          db.GetNullableString(reader, 5);
        entities.ExistingLegalAction.CourtCaseNumber =
          db.GetNullableString(reader, 6);
        entities.ExistingLegalAction.TrbId = db.GetNullableInt32(reader, 7);
        entities.ExistingTribunal.Identifier = db.GetInt32(reader, 7);
        entities.ExistingTribunal.JudicialDivision =
          db.GetNullableString(reader, 8);
        entities.ExistingTribunal.Name = db.GetString(reader, 9);
        entities.ExistingTribunal.FipLocation = db.GetNullableInt32(reader, 10);
        entities.ExistingTribunal.JudicialDistrict = db.GetString(reader, 11);
        entities.ExistingTribunal.FipCounty = db.GetNullableInt32(reader, 12);
        entities.ExistingTribunal.FipState = db.GetNullableInt32(reader, 13);
        entities.ExistingObligation.Populated = true;
        entities.ExistingLegalAction.Populated = true;
        entities.ExistingTribunal.Populated = true;
        CheckValid<Obligation>("CpaType", entities.ExistingObligation.CpaType);

        return true;
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
        entities.ExistingObligation.Description =
          db.GetNullableString(reader, 5);
        entities.ExistingObligationType.Code = db.GetString(reader, 6);
        entities.ExistingObligationType.Name = db.GetString(reader, 7);
        entities.ExistingObligationType.Populated = true;
        entities.ExistingObligation.Populated = true;
        CheckValid<Obligation>("CpaType", entities.ExistingObligation.CpaType);
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

  private bool ReadServiceProvider1()
  {
    entities.ExistingServiceProvider.Populated = false;

    return Read("ReadServiceProvider1",
      (db, command) =>
      {
        db.SetString(
          command, "userId", export.ObligationAdmActionExemption.CreatedBy);
      },
      (db, reader) =>
      {
        entities.ExistingServiceProvider.SystemGeneratedId =
          db.GetInt32(reader, 0);
        entities.ExistingServiceProvider.UserId = db.GetString(reader, 1);
        entities.ExistingServiceProvider.LastName = db.GetString(reader, 2);
        entities.ExistingServiceProvider.FirstName = db.GetString(reader, 3);
        entities.ExistingServiceProvider.MiddleInitial =
          db.GetString(reader, 4);
        entities.ExistingServiceProvider.Populated = true;
      });
  }

  private bool ReadServiceProvider2()
  {
    entities.ExistingServiceProvider.Populated = false;

    return Read("ReadServiceProvider2",
      (db, command) =>
      {
        db.SetString(
          command, "userId",
          export.ObligationAdmActionExemption.LastUpdatedBy ?? "");
      },
      (db, reader) =>
      {
        entities.ExistingServiceProvider.SystemGeneratedId =
          db.GetInt32(reader, 0);
        entities.ExistingServiceProvider.UserId = db.GetString(reader, 1);
        entities.ExistingServiceProvider.LastName = db.GetString(reader, 2);
        entities.ExistingServiceProvider.FirstName = db.GetString(reader, 3);
        entities.ExistingServiceProvider.MiddleInitial =
          db.GetString(reader, 4);
        entities.ExistingServiceProvider.Populated = true;
      });
  }

  private bool ReadTribunal()
  {
    System.Diagnostics.Debug.Assert(entities.ExistingLegalAction.Populated);
    entities.ExistingTribunal.Populated = false;

    return Read("ReadTribunal",
      (db, command) =>
      {
        db.SetInt32(
          command, "identifier",
          entities.ExistingLegalAction.TrbId.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ExistingTribunal.JudicialDivision =
          db.GetNullableString(reader, 0);
        entities.ExistingTribunal.Name = db.GetString(reader, 1);
        entities.ExistingTribunal.FipLocation = db.GetNullableInt32(reader, 2);
        entities.ExistingTribunal.JudicialDistrict = db.GetString(reader, 3);
        entities.ExistingTribunal.Identifier = db.GetInt32(reader, 4);
        entities.ExistingTribunal.FipCounty = db.GetNullableInt32(reader, 5);
        entities.ExistingTribunal.FipState = db.GetNullableInt32(reader, 6);
        entities.ExistingTribunal.Populated = true;
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
    /// A value of DisplayAnyObligation.
    /// </summary>
    [JsonPropertyName("displayAnyObligation")]
    public Common DisplayAnyObligation
    {
      get => displayAnyObligation ??= new();
      set => displayAnyObligation = value;
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
    private Tribunal tribunal;
    private Fips fips;
    private ObligationAdmActionExemption obligationAdmActionExemption;
    private AdministrativeAction administrativeAction;
    private Common displayAnyObligation;
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
    /// A value of Foreign.
    /// </summary>
    [JsonPropertyName("foreign")]
    public FipsTribAddress Foreign
    {
      get => foreign ??= new();
      set => foreign = value;
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
    /// A value of UpdatedBy.
    /// </summary>
    [JsonPropertyName("updatedBy")]
    public ServiceProvider UpdatedBy
    {
      get => updatedBy ??= new();
      set => updatedBy = value;
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
    /// A value of Obligation.
    /// </summary>
    [JsonPropertyName("obligation")]
    public Obligation Obligation
    {
      get => obligation ??= new();
      set => obligation = value;
    }

    /// <summary>
    /// A value of PetitionerRespondentDetails.
    /// </summary>
    [JsonPropertyName("petitionerRespondentDetails")]
    public PetitionerRespondentDetails PetitionerRespondentDetails
    {
      get => petitionerRespondentDetails ??= new();
      set => petitionerRespondentDetails = value;
    }

    private FipsTribAddress foreign;
    private ObligationType obligationType;
    private LegalAction legalAction;
    private Tribunal tribunal;
    private Fips fips;
    private ServiceProvider updatedBy;
    private ServiceProvider createdBy;
    private ObligationAdmActionExemption obligationAdmActionExemption;
    private AdministrativeAction administrativeAction;
    private Obligation obligation;
    private PetitionerRespondentDetails petitionerRespondentDetails;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of DateWorkArea.
    /// </summary>
    [JsonPropertyName("dateWorkArea")]
    public DateWorkArea DateWorkArea
    {
      get => dateWorkArea ??= new();
      set => dateWorkArea = value;
    }

    /// <summary>
    /// A value of InitialisedObligationAdmActionExemption.
    /// </summary>
    [JsonPropertyName("initialisedObligationAdmActionExemption")]
    public ObligationAdmActionExemption InitialisedObligationAdmActionExemption
    {
      get => initialisedObligationAdmActionExemption ??= new();
      set => initialisedObligationAdmActionExemption = value;
    }

    /// <summary>
    /// A value of ObligationFound.
    /// </summary>
    [JsonPropertyName("obligationFound")]
    public Common ObligationFound
    {
      get => obligationFound ??= new();
      set => obligationFound = value;
    }

    /// <summary>
    /// A value of AdminActExemptionFound.
    /// </summary>
    [JsonPropertyName("adminActExemptionFound")]
    public Common AdminActExemptionFound
    {
      get => adminActExemptionFound ??= new();
      set => adminActExemptionFound = value;
    }

    /// <summary>
    /// A value of InitialisedToBlanks.
    /// </summary>
    [JsonPropertyName("initialisedToBlanks")]
    public LegalAction InitialisedToBlanks
    {
      get => initialisedToBlanks ??= new();
      set => initialisedToBlanks = value;
    }

    /// <summary>
    /// A value of InitialisedToSpacesTribunal.
    /// </summary>
    [JsonPropertyName("initialisedToSpacesTribunal")]
    public Tribunal InitialisedToSpacesTribunal
    {
      get => initialisedToSpacesTribunal ??= new();
      set => initialisedToSpacesTribunal = value;
    }

    /// <summary>
    /// A value of InitialisedToSpacesFips.
    /// </summary>
    [JsonPropertyName("initialisedToSpacesFips")]
    public Fips InitialisedToSpacesFips
    {
      get => initialisedToSpacesFips ??= new();
      set => initialisedToSpacesFips = value;
    }

    /// <summary>
    /// A value of InitialisedObligation.
    /// </summary>
    [JsonPropertyName("initialisedObligation")]
    public Obligation InitialisedObligation
    {
      get => initialisedObligation ??= new();
      set => initialisedObligation = value;
    }

    /// <summary>
    /// A value of InitialisedObligationType.
    /// </summary>
    [JsonPropertyName("initialisedObligationType")]
    public ObligationType InitialisedObligationType
    {
      get => initialisedObligationType ??= new();
      set => initialisedObligationType = value;
    }

    private DateWorkArea dateWorkArea;
    private ObligationAdmActionExemption initialisedObligationAdmActionExemption;
      
    private Common obligationFound;
    private Common adminActExemptionFound;
    private LegalAction initialisedToBlanks;
    private Tribunal initialisedToSpacesTribunal;
    private Fips initialisedToSpacesFips;
    private Obligation initialisedObligation;
    private ObligationType initialisedObligationType;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of ExistingForeign.
    /// </summary>
    [JsonPropertyName("existingForeign")]
    public FipsTribAddress ExistingForeign
    {
      get => existingForeign ??= new();
      set => existingForeign = value;
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
    /// A value of ExistingServiceProvider.
    /// </summary>
    [JsonPropertyName("existingServiceProvider")]
    public ServiceProvider ExistingServiceProvider
    {
      get => existingServiceProvider ??= new();
      set => existingServiceProvider = value;
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

    private FipsTribAddress existingForeign;
    private ObligationType existingObligationType;
    private ServiceProvider existingServiceProvider;
    private ObligationAdmActionExemption existingObligationAdmActionExemption;
    private AdministrativeAction existingAdministrativeAction;
    private CsePersonAccount existingCsePersonAccount;
    private Obligation existingObligation;
    private CsePerson existingCsePerson;
    private LegalAction existingLegalAction;
    private Tribunal existingTribunal;
    private Fips existingFips;
  }
#endregion
}
