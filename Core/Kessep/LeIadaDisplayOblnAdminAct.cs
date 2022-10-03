// Program: LE_IADA_DISPLAY_OBLN_ADMIN_ACT, ID: 372594983, model: 746.
// Short name: SWE00784
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
/// A program: LE_IADA_DISPLAY_OBLN_ADMIN_ACT.
/// </para>
/// <para>
/// RESP: LGLENFAC
/// This action block creates OBLIGATION_ADMINISTRATIVE_ACTION and associates 
/// with all or specified obligations.
/// </para>
/// </summary>
[Serializable]
public partial class LeIadaDisplayOblnAdminAct: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the LE_IADA_DISPLAY_OBLN_ADMIN_ACT program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new LeIadaDisplayOblnAdminAct(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of LeIadaDisplayOblnAdminAct.
  /// </summary>
  public LeIadaDisplayOblnAdminAct(IContext context, Import import,
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
    // **************************************************************
    // 10/16/98     P. Sharp     Removed excessive reads. Removed duplicated 
    // code. Per Phase II assessment.
    export.Obligation.SystemGeneratedIdentifier =
      import.Obligation.SystemGeneratedIdentifier;
    export.AdministrativeAction.Type1 = import.AdministrativeAction.Type1;
    MoveObligationAdministrativeAction(import.ObligationAdministrativeAction,
      export.ObligationAdministrativeAction);
    MoveFips(import.Fips, export.Fips);
    MoveLegalAction(import.LegalAction, export.LegalAction);
    export.Tribunal.Identifier = import.Foreign.Identifier;

    if (AsChar(import.DisplayAnyObligation.Flag) == 'N')
    {
      if (import.Obligation.SystemGeneratedIdentifier == 0)
      {
        ExitState = "LE0000_OBLIGATION_REQD";

        return;
      }
      else
      {
      }
    }

    if (!IsEmpty(import.AdministrativeAction.Type1))
    {
      if (ReadAdministrativeAction())
      {
        if (AsChar(entities.ExistingAdministrativeAction.Indicator) == 'A')
        {
          ExitState = "LE0000_ONLY_MANUAL_ADM_ACT_ALLWD";

          return;
        }

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

    local.ObligationFound.Flag = "N";
    local.AdminActionFound.Flag = "N";

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
            export.Tribunal.Assign(entities.ExistingTribunal);
          }
          else
          {
            if (entities.ExistingTribunal.Identifier != import
              .Foreign.Identifier)
            {
              continue;
            }

            export.Tribunal.Assign(entities.ExistingTribunal);

            if (ReadFipsTribAddress())
            {
              export.Foreign.Country = entities.ExistingForeign.Country;
            }
          }

          local.ObligationFound.Flag = "Y";

          foreach(var item1 in ReadAdministrativeActionObligationAdministrativeAction())
            
          {
            if (Lt(local.InitialisedToZeros.TakenDate,
              import.ObligationAdministrativeAction.TakenDate))
            {
              if (Equal(entities.ExistingObligationAdministrativeAction.
                TakenDate, import.ObligationAdministrativeAction.TakenDate))
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
                ++local.AdminCount.Count;
              }
              else
              {
                continue;
              }
            }
            else
            {
              ++local.AdminCount.Count;
            }

            if (local.AdminCount.Count == 1)
            {
              MoveLegalAction(entities.ExistingLegalAction, export.LegalAction);
              MoveObligation(entities.ExistingObligation, export.Obligation);
              export.ObligationAdministrativeAction.Assign(
                entities.ExistingObligationAdministrativeAction);
              MoveAdministrativeAction(entities.ExistingAdministrativeAction,
                export.AdministrativeAction);

              if (ReadObligationType())
              {
                export.ObligationType.Assign(entities.ExistingObligationType);
              }

              local.AdminActionFound.Flag = "Y";

              if (IsEmpty(import.AdministrativeAction.Type1))
              {
                goto ReadEach;
              }
            }
            else if (local.AdminCount.Count > 1)
            {
              export.MoreAdminAct.Flag = "Y";

              goto ReadEach;
            }
          }
        }

ReadEach:
        ;
      }
      else
      {
        foreach(var item in ReadObligationObligationType2())
        {
          local.ObligationFound.Flag = "Y";

          if (ReadLegalAction())
          {
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

          foreach(var item1 in ReadAdministrativeActionObligationAdministrativeAction())
            
          {
            if (Lt(local.InitialisedToZeros.TakenDate,
              import.ObligationAdministrativeAction.TakenDate))
            {
              if (Equal(entities.ExistingObligationAdministrativeAction.
                TakenDate, import.ObligationAdministrativeAction.TakenDate))
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
                ++local.AdminCount.Count;
              }
              else
              {
                continue;
              }
            }
            else
            {
              ++local.AdminCount.Count;
            }

            if (local.AdminCount.Count == 1)
            {
              MoveLegalAction(entities.ExistingLegalAction, export.LegalAction);
              MoveObligation(entities.ExistingObligation, export.Obligation);
              export.ObligationAdministrativeAction.Assign(
                entities.ExistingObligationAdministrativeAction);
              MoveAdministrativeAction(entities.ExistingAdministrativeAction,
                export.AdministrativeAction);
              export.ObligationType.Assign(entities.ExistingObligationType);
              local.AdminActionFound.Flag = "Y";
              ReadObligationType();

              if (IsEmpty(import.AdministrativeAction.Type1))
              {
                goto Test;
              }
            }
            else if (local.AdminCount.Count > 1)
            {
              export.MoreAdminAct.Flag = "Y";

              goto Test;
            }
          }
        }
      }
    }
    else if (ReadObligationObligationType1())
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

      foreach(var item in ReadAdministrativeActionObligationAdministrativeAction())
        
      {
        if (Lt(local.InitialisedToZeros.TakenDate,
          import.ObligationAdministrativeAction.TakenDate))
        {
          if (Equal(entities.ExistingObligationAdministrativeAction.TakenDate,
            import.ObligationAdministrativeAction.TakenDate))
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

        export.ObligationAdministrativeAction.Assign(
          entities.ExistingObligationAdministrativeAction);
        MoveAdministrativeAction(entities.ExistingAdministrativeAction,
          export.AdministrativeAction);
        local.AdminActionFound.Flag = "Y";

        break;
      }
    }
    else
    {
      ExitState = "FN0000_OBLIGATION_NF";

      return;
    }

Test:

    if (AsChar(local.ObligationFound.Flag) == 'N')
    {
      ExitState = "LE0000_NO_OBLIGATION_FOUND";

      return;
    }

    if (export.LegalAction.Identifier != 0)
    {
      UseLeGetPetitionerRespondent();
    }

    if (AsChar(local.AdminActionFound.Flag) == 'N')
    {
      // No administrative action was found and no specific obligation was 
      // supplied by user (display flag would have been set to 'N'.  So blank
      // out the obligation and obligation type. Only keep the obligation  and
      // type if the user had supplied it.
      if (AsChar(import.DisplayAnyObligation.Flag) == 'Y')
      {
        MoveObligation(local.InitialisedObligation, export.Obligation);
        export.ObligationType.Assign(local.InitialisedObligationType);
      }

      ExitState = "LE0000_ADMIN_ACTION_NOT_AVAIL";
    }
    else if (ReadServiceProvider())
    {
      export.ActionTakenBy.Assign(entities.ExistingActionTakenBy);
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

  private static void MoveObligationAdministrativeAction(
    ObligationAdministrativeAction source,
    ObligationAdministrativeAction target)
  {
    target.TakenDate = source.TakenDate;
    target.Response = source.Response;
    target.ResponseDate = source.ResponseDate;
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
        entities.ExistingAdministrativeAction.Indicator =
          db.GetString(reader, 2);
        entities.ExistingAdministrativeAction.Populated = true;
      });
  }

  private IEnumerable<bool>
    ReadAdministrativeActionObligationAdministrativeAction()
  {
    System.Diagnostics.Debug.Assert(entities.ExistingObligation.Populated);
    entities.ExistingAdministrativeAction.Populated = false;
    entities.ExistingObligationAdministrativeAction.Populated = false;

    return ReadEach("ReadAdministrativeActionObligationAdministrativeAction",
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
        entities.ExistingAdministrativeAction.Type1 = db.GetString(reader, 0);
        entities.ExistingObligationAdministrativeAction.AatType =
          db.GetString(reader, 0);
        entities.ExistingAdministrativeAction.Description =
          db.GetString(reader, 1);
        entities.ExistingAdministrativeAction.Indicator =
          db.GetString(reader, 2);
        entities.ExistingObligationAdministrativeAction.OtyType =
          db.GetInt32(reader, 3);
        entities.ExistingObligationAdministrativeAction.ObgGeneratedId =
          db.GetInt32(reader, 4);
        entities.ExistingObligationAdministrativeAction.CspNumber =
          db.GetString(reader, 5);
        entities.ExistingObligationAdministrativeAction.CpaType =
          db.GetString(reader, 6);
        entities.ExistingObligationAdministrativeAction.TakenDate =
          db.GetDate(reader, 7);
        entities.ExistingObligationAdministrativeAction.ResponseDate =
          db.GetNullableDate(reader, 8);
        entities.ExistingObligationAdministrativeAction.CreatedBy =
          db.GetString(reader, 9);
        entities.ExistingObligationAdministrativeAction.LastUpdatedBy =
          db.GetNullableString(reader, 10);
        entities.ExistingObligationAdministrativeAction.LastUpdatedTstamp =
          db.GetNullableDateTime(reader, 11);
        entities.ExistingObligationAdministrativeAction.Response =
          db.GetNullableString(reader, 12);
        entities.ExistingAdministrativeAction.Populated = true;
        entities.ExistingObligationAdministrativeAction.Populated = true;
        CheckValid<ObligationAdministrativeAction>("CpaType",
          entities.ExistingObligationAdministrativeAction.CpaType);

        return true;
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

  private IEnumerable<bool> ReadObligationLegalActionTribunal()
  {
    System.Diagnostics.Debug.
      Assert(entities.ExistingCsePersonAccount.Populated);
    entities.ExistingTribunal.Populated = false;
    entities.ExistingObligation.Populated = false;
    entities.ExistingLegalAction.Populated = false;

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
        entities.ExistingTribunal.Populated = true;
        entities.ExistingObligation.Populated = true;
        entities.ExistingLegalAction.Populated = true;
        CheckValid<Obligation>("CpaType", entities.ExistingObligation.CpaType);

        return true;
      });
  }

  private bool ReadObligationObligationType1()
  {
    System.Diagnostics.Debug.
      Assert(entities.ExistingCsePersonAccount.Populated);
    entities.ExistingObligationType.Populated = false;
    entities.ExistingObligation.Populated = false;

    return Read("ReadObligationObligationType1",
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

  private IEnumerable<bool> ReadObligationObligationType2()
  {
    System.Diagnostics.Debug.
      Assert(entities.ExistingCsePersonAccount.Populated);
    entities.ExistingObligationType.Populated = false;
    entities.ExistingObligation.Populated = false;

    return ReadEach("ReadObligationObligationType2",
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
        db.SetString(
          command, "userId", export.ObligationAdministrativeAction.CreatedBy);
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
    /// A value of ObligationAdministrativeAction.
    /// </summary>
    [JsonPropertyName("obligationAdministrativeAction")]
    public ObligationAdministrativeAction ObligationAdministrativeAction
    {
      get => obligationAdministrativeAction ??= new();
      set => obligationAdministrativeAction = value;
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
    private AdministrativeAction administrativeAction;
    private Common displayAnyObligation;
    private Obligation obligation;
    private CsePerson csePerson;
    private ObligationAdministrativeAction obligationAdministrativeAction;
    private LegalAction legalAction;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of MoreAdminAct.
    /// </summary>
    [JsonPropertyName("moreAdminAct")]
    public Common MoreAdminAct
    {
      get => moreAdminAct ??= new();
      set => moreAdminAct = value;
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
    /// A value of LegalAction.
    /// </summary>
    [JsonPropertyName("legalAction")]
    public LegalAction LegalAction
    {
      get => legalAction ??= new();
      set => legalAction = value;
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

    private Common moreAdminAct;
    private FipsTribAddress foreign;
    private Tribunal tribunal;
    private Fips fips;
    private ObligationType obligationType;
    private ServiceProvider actionTakenBy;
    private LegalAction legalAction;
    private ObligationAdministrativeAction obligationAdministrativeAction;
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
    /// A value of AdminCount.
    /// </summary>
    [JsonPropertyName("adminCount")]
    public Common AdminCount
    {
      get => adminCount ??= new();
      set => adminCount = value;
    }

    /// <summary>
    /// A value of ObligationCount.
    /// </summary>
    [JsonPropertyName("obligationCount")]
    public Common ObligationCount
    {
      get => obligationCount ??= new();
      set => obligationCount = value;
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
    /// A value of InitialisedToBlanks.
    /// </summary>
    [JsonPropertyName("initialisedToBlanks")]
    public LegalAction InitialisedToBlanks
    {
      get => initialisedToBlanks ??= new();
      set => initialisedToBlanks = value;
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
    /// A value of AdminActionFound.
    /// </summary>
    [JsonPropertyName("adminActionFound")]
    public Common AdminActionFound
    {
      get => adminActionFound ??= new();
      set => adminActionFound = value;
    }

    /// <summary>
    /// A value of InitialisedToZeros.
    /// </summary>
    [JsonPropertyName("initialisedToZeros")]
    public ObligationAdministrativeAction InitialisedToZeros
    {
      get => initialisedToZeros ??= new();
      set => initialisedToZeros = value;
    }

    private Common adminCount;
    private Common obligationCount;
    private ObligationType initialisedObligationType;
    private Obligation initialisedObligation;
    private Tribunal initialisedToSpacesTribunal;
    private Fips initialisedToSpacesFips;
    private LegalAction initialisedToBlanks;
    private Common obligationFound;
    private Common adminActionFound;
    private ObligationAdministrativeAction initialisedToZeros;
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
    /// A value of ExistingObligationAdministrativeAction.
    /// </summary>
    [JsonPropertyName("existingObligationAdministrativeAction")]
    public ObligationAdministrativeAction ExistingObligationAdministrativeAction
    {
      get => existingObligationAdministrativeAction ??= new();
      set => existingObligationAdministrativeAction = value;
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

    private FipsTribAddress existingForeign;
    private Tribunal existingTribunal;
    private Fips existingFips;
    private ObligationType existingObligationType;
    private ServiceProvider existingActionTakenBy;
    private AdministrativeAction existingAdministrativeAction;
    private CsePersonAccount existingCsePersonAccount;
    private Obligation existingObligation;
    private CsePerson existingCsePerson;
    private ObligationAdministrativeAction existingObligationAdministrativeAction;
      
    private LegalAction existingLegalAction;
  }
#endregion
}
