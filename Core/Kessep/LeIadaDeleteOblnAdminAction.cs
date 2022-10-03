// Program: LE_IADA_DELETE_OBLN_ADMIN_ACTION, ID: 372594980, model: 746.
// Short name: SWE00783
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
/// A program: LE_IADA_DELETE_OBLN_ADMIN_ACTION.
/// </para>
/// <para>
/// RESP: LGLENFAC
/// This action block creates OBLIGATION_ADMINISTRATIVE_ACTION and associates 
/// with all or specified obligations.
/// </para>
/// </summary>
[Serializable]
public partial class LeIadaDeleteOblnAdminAction: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the LE_IADA_DELETE_OBLN_ADMIN_ACTION program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new LeIadaDeleteOblnAdminAction(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of LeIadaDeleteOblnAdminAction.
  /// </summary>
  public LeIadaDeleteOblnAdminAction(IContext context, Import import,
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
    // ********************************************************
    // 10/16/98    P. Sharp   Removed redundant reads of fips, legal action.
    export.Rollback.Flag = "N";

    if (AsChar(import.DeleteForAllObligation.Flag) == 'Y')
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

    if (AsChar(import.DeleteForAllObligation.Flag) == 'Y')
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

        if (ReadObligationAdministrativeAction())
        {
          if (ReadAdministrativeAppeal())
          {
            ExitState = "LE0000_ADMIN_APPEAL_PREVENT_DEL";

            return;
          }

          DeleteObligationAdministrativeAction();
        }
        else
        {
          export.Rollback.Flag = "Y";
          ExitState = "LE0000_OBLIG_ADMIN_ACT_NF";

          return;
        }
      }
    }
    else
    {
      // ---------------------------------------------
      // Delete only the given obligation
      // ---------------------------------------------
      if (ReadObligationObligationType())
      {
        if (ReadObligationAdministrativeAction())
        {
          if (ReadAdministrativeAppeal())
          {
            ExitState = "LE0000_ADMIN_APPEAL_PREVENT_DEL";

            return;
          }

          DeleteObligationAdministrativeAction();
        }
        else
        {
          ExitState = "LE0000_OBLIG_ADMIN_ACT_NF";
        }
      }
      else
      {
        ExitState = "FN0000_OBLIGATION_NF";
      }
    }
  }

  private void DeleteObligationAdministrativeAction()
  {
    bool exists;

    exists = Read("DeleteObligationAdministrativeAction#1",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "otyId",
          entities.ExistingObligationAdministrativeAction.OtyType);
        db.SetNullableString(
          command, "aatType",
          entities.ExistingObligationAdministrativeAction.AatType);
        db.SetNullableInt32(
          command, "obgGeneratedId",
          entities.ExistingObligationAdministrativeAction.ObgGeneratedId);
        db.SetNullableString(
          command, "cspNumber",
          entities.ExistingObligationAdministrativeAction.CspNumber);
        db.SetNullableString(
          command, "cpaType",
          entities.ExistingObligationAdministrativeAction.CpaType);
        db.SetNullableDate(
          command, "oaaTakenDate",
          entities.ExistingObligationAdministrativeAction.TakenDate.
            GetValueOrDefault());
      },
      null);

    if (exists)
    {
      throw DataError("Restrict violation on table \"CKT_ADMIN_APPEAL\".",
        "50001");
    }

    exists = Read("DeleteObligationAdministrativeAction#2",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "otyId",
          entities.ExistingObligationAdministrativeAction.OtyType);
        db.SetNullableString(
          command, "aatType",
          entities.ExistingObligationAdministrativeAction.AatType);
        db.SetNullableInt32(
          command, "obgGeneratedId",
          entities.ExistingObligationAdministrativeAction.ObgGeneratedId);
        db.SetNullableString(
          command, "cspNumber",
          entities.ExistingObligationAdministrativeAction.CspNumber);
        db.SetNullableString(
          command, "cpaType",
          entities.ExistingObligationAdministrativeAction.CpaType);
        db.SetNullableDate(
          command, "oaaTakenDate",
          entities.ExistingObligationAdministrativeAction.TakenDate.
            GetValueOrDefault());
      },
      null);

    if (exists)
    {
      throw DataError("Restrict violation on table \"CKT_ASSGN_OBG_AA\".",
        "50001");
    }

    Update("DeleteObligationAdministrativeAction#3",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "otyId",
          entities.ExistingObligationAdministrativeAction.OtyType);
        db.SetNullableString(
          command, "aatType",
          entities.ExistingObligationAdministrativeAction.AatType);
        db.SetNullableInt32(
          command, "obgGeneratedId",
          entities.ExistingObligationAdministrativeAction.ObgGeneratedId);
        db.SetNullableString(
          command, "cspNumber",
          entities.ExistingObligationAdministrativeAction.CspNumber);
        db.SetNullableString(
          command, "cpaType",
          entities.ExistingObligationAdministrativeAction.CpaType);
        db.SetNullableDate(
          command, "oaaTakenDate",
          entities.ExistingObligationAdministrativeAction.TakenDate.
            GetValueOrDefault());
      });

    Update("DeleteObligationAdministrativeAction#4",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "otyId",
          entities.ExistingObligationAdministrativeAction.OtyType);
        db.SetNullableString(
          command, "aatType",
          entities.ExistingObligationAdministrativeAction.AatType);
        db.SetNullableInt32(
          command, "obgGeneratedId",
          entities.ExistingObligationAdministrativeAction.ObgGeneratedId);
        db.SetNullableString(
          command, "cspNumber",
          entities.ExistingObligationAdministrativeAction.CspNumber);
        db.SetNullableString(
          command, "cpaType",
          entities.ExistingObligationAdministrativeAction.CpaType);
        db.SetNullableDate(
          command, "oaaTakenDate",
          entities.ExistingObligationAdministrativeAction.TakenDate.
            GetValueOrDefault());
      });

    Update("DeleteObligationAdministrativeAction#5",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "otyId",
          entities.ExistingObligationAdministrativeAction.OtyType);
        db.SetNullableString(
          command, "aatType",
          entities.ExistingObligationAdministrativeAction.AatType);
        db.SetNullableInt32(
          command, "obgGeneratedId",
          entities.ExistingObligationAdministrativeAction.ObgGeneratedId);
        db.SetNullableString(
          command, "cspNumber",
          entities.ExistingObligationAdministrativeAction.CspNumber);
        db.SetNullableString(
          command, "cpaType",
          entities.ExistingObligationAdministrativeAction.CpaType);
        db.SetNullableDate(
          command, "oaaTakenDate",
          entities.ExistingObligationAdministrativeAction.TakenDate.
            GetValueOrDefault());
      });

    Update("DeleteObligationAdministrativeAction#6",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "otyId",
          entities.ExistingObligationAdministrativeAction.OtyType);
        db.SetNullableString(
          command, "aatType",
          entities.ExistingObligationAdministrativeAction.AatType);
        db.SetNullableInt32(
          command, "obgGeneratedId",
          entities.ExistingObligationAdministrativeAction.ObgGeneratedId);
        db.SetNullableString(
          command, "cspNumber",
          entities.ExistingObligationAdministrativeAction.CspNumber);
        db.SetNullableString(
          command, "cpaType",
          entities.ExistingObligationAdministrativeAction.CpaType);
        db.SetNullableDate(
          command, "oaaTakenDate",
          entities.ExistingObligationAdministrativeAction.TakenDate.
            GetValueOrDefault());
      });

    Update("DeleteObligationAdministrativeAction#7",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "otyId",
          entities.ExistingObligationAdministrativeAction.OtyType);
        db.SetNullableString(
          command, "aatType",
          entities.ExistingObligationAdministrativeAction.AatType);
        db.SetNullableInt32(
          command, "obgGeneratedId",
          entities.ExistingObligationAdministrativeAction.ObgGeneratedId);
        db.SetNullableString(
          command, "cspNumber",
          entities.ExistingObligationAdministrativeAction.CspNumber);
        db.SetNullableString(
          command, "cpaType",
          entities.ExistingObligationAdministrativeAction.CpaType);
        db.SetNullableDate(
          command, "oaaTakenDate",
          entities.ExistingObligationAdministrativeAction.TakenDate.
            GetValueOrDefault());
      });

    Update("DeleteObligationAdministrativeAction#8",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "otyId",
          entities.ExistingObligationAdministrativeAction.OtyType);
        db.SetNullableString(
          command, "aatType",
          entities.ExistingObligationAdministrativeAction.AatType);
        db.SetNullableInt32(
          command, "obgGeneratedId",
          entities.ExistingObligationAdministrativeAction.ObgGeneratedId);
        db.SetNullableString(
          command, "cspNumber",
          entities.ExistingObligationAdministrativeAction.CspNumber);
        db.SetNullableString(
          command, "cpaType",
          entities.ExistingObligationAdministrativeAction.CpaType);
        db.SetNullableDate(
          command, "oaaTakenDate",
          entities.ExistingObligationAdministrativeAction.TakenDate.
            GetValueOrDefault());
      });

    Update("DeleteObligationAdministrativeAction#9",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "otyId",
          entities.ExistingObligationAdministrativeAction.OtyType);
        db.SetNullableString(
          command, "aatType",
          entities.ExistingObligationAdministrativeAction.AatType);
        db.SetNullableInt32(
          command, "obgGeneratedId",
          entities.ExistingObligationAdministrativeAction.ObgGeneratedId);
        db.SetNullableString(
          command, "cspNumber",
          entities.ExistingObligationAdministrativeAction.CspNumber);
        db.SetNullableString(
          command, "cpaType",
          entities.ExistingObligationAdministrativeAction.CpaType);
        db.SetNullableDate(
          command, "oaaTakenDate",
          entities.ExistingObligationAdministrativeAction.TakenDate.
            GetValueOrDefault());
      });

    Update("DeleteObligationAdministrativeAction#10",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "otyId",
          entities.ExistingObligationAdministrativeAction.OtyType);
        db.SetNullableString(
          command, "aatType",
          entities.ExistingObligationAdministrativeAction.AatType);
        db.SetNullableInt32(
          command, "obgGeneratedId",
          entities.ExistingObligationAdministrativeAction.ObgGeneratedId);
        db.SetNullableString(
          command, "cspNumber",
          entities.ExistingObligationAdministrativeAction.CspNumber);
        db.SetNullableString(
          command, "cpaType",
          entities.ExistingObligationAdministrativeAction.CpaType);
        db.SetNullableDate(
          command, "oaaTakenDate",
          entities.ExistingObligationAdministrativeAction.TakenDate.
            GetValueOrDefault());
      });

    Update("DeleteObligationAdministrativeAction#11",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "otyId",
          entities.ExistingObligationAdministrativeAction.OtyType);
        db.SetNullableString(
          command, "aatType",
          entities.ExistingObligationAdministrativeAction.AatType);
        db.SetNullableInt32(
          command, "obgGeneratedId",
          entities.ExistingObligationAdministrativeAction.ObgGeneratedId);
        db.SetNullableString(
          command, "cspNumber",
          entities.ExistingObligationAdministrativeAction.CspNumber);
        db.SetNullableString(
          command, "cpaType",
          entities.ExistingObligationAdministrativeAction.CpaType);
        db.SetNullableDate(
          command, "oaaTakenDate",
          entities.ExistingObligationAdministrativeAction.TakenDate.
            GetValueOrDefault());
      });

    Update("DeleteObligationAdministrativeAction#12",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "otyId",
          entities.ExistingObligationAdministrativeAction.OtyType);
        db.SetNullableString(
          command, "aatType",
          entities.ExistingObligationAdministrativeAction.AatType);
        db.SetNullableInt32(
          command, "obgGeneratedId",
          entities.ExistingObligationAdministrativeAction.ObgGeneratedId);
        db.SetNullableString(
          command, "cspNumber",
          entities.ExistingObligationAdministrativeAction.CspNumber);
        db.SetNullableString(
          command, "cpaType",
          entities.ExistingObligationAdministrativeAction.CpaType);
        db.SetNullableDate(
          command, "oaaTakenDate",
          entities.ExistingObligationAdministrativeAction.TakenDate.
            GetValueOrDefault());
      });
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

  private bool ReadAdministrativeAppeal()
  {
    System.Diagnostics.Debug.Assert(
      entities.ExistingObligationAdministrativeAction.Populated);
    entities.ExistingAdministrativeAppeal.Populated = false;

    return Read("ReadAdministrativeAppeal",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "otyId",
          entities.ExistingObligationAdministrativeAction.OtyType);
        db.SetNullableDate(
          command, "oaaTakenDate",
          entities.ExistingObligationAdministrativeAction.TakenDate.
            GetValueOrDefault());
        db.SetNullableString(
          command, "aatType",
          entities.ExistingObligationAdministrativeAction.AatType);
        db.SetNullableString(
          command, "cpaType",
          entities.ExistingObligationAdministrativeAction.CpaType);
        db.SetNullableString(
          command, "cspNumber",
          entities.ExistingObligationAdministrativeAction.CspNumber);
        db.SetNullableInt32(
          command, "obgGeneratedId",
          entities.ExistingObligationAdministrativeAction.ObgGeneratedId);
      },
      (db, reader) =>
      {
        entities.ExistingAdministrativeAppeal.Identifier =
          db.GetInt32(reader, 0);
        entities.ExistingAdministrativeAppeal.AatType =
          db.GetNullableString(reader, 1);
        entities.ExistingAdministrativeAppeal.ObgGeneratedId =
          db.GetNullableInt32(reader, 2);
        entities.ExistingAdministrativeAppeal.CspNumber =
          db.GetNullableString(reader, 3);
        entities.ExistingAdministrativeAppeal.CpaType =
          db.GetNullableString(reader, 4);
        entities.ExistingAdministrativeAppeal.OaaTakenDate =
          db.GetNullableDate(reader, 5);
        entities.ExistingAdministrativeAppeal.OtyId =
          db.GetNullableInt32(reader, 6);
        entities.ExistingAdministrativeAppeal.Populated = true;
        CheckValid<AdministrativeAppeal>("CpaType",
          entities.ExistingAdministrativeAppeal.CpaType);
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

  private bool ReadObligationAdministrativeAction()
  {
    System.Diagnostics.Debug.Assert(entities.ExistingObligation.Populated);
    entities.ExistingObligationAdministrativeAction.Populated = false;

    return Read("ReadObligationAdministrativeAction",
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
          command, "takenDt",
          import.ObligationAdministrativeAction.TakenDate.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ExistingObligationAdministrativeAction.OtyType =
          db.GetInt32(reader, 0);
        entities.ExistingObligationAdministrativeAction.AatType =
          db.GetString(reader, 1);
        entities.ExistingObligationAdministrativeAction.ObgGeneratedId =
          db.GetInt32(reader, 2);
        entities.ExistingObligationAdministrativeAction.CspNumber =
          db.GetString(reader, 3);
        entities.ExistingObligationAdministrativeAction.CpaType =
          db.GetString(reader, 4);
        entities.ExistingObligationAdministrativeAction.TakenDate =
          db.GetDate(reader, 5);
        entities.ExistingObligationAdministrativeAction.ResponseDate =
          db.GetNullableDate(reader, 6);
        entities.ExistingObligationAdministrativeAction.LastUpdatedBy =
          db.GetNullableString(reader, 7);
        entities.ExistingObligationAdministrativeAction.LastUpdatedTstamp =
          db.GetNullableDateTime(reader, 8);
        entities.ExistingObligationAdministrativeAction.Response =
          db.GetNullableString(reader, 9);
        entities.ExistingObligationAdministrativeAction.Populated = true;
        CheckValid<ObligationAdministrativeAction>("CpaType",
          entities.ExistingObligationAdministrativeAction.CpaType);
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
          command, "dtyGeneratedId",
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
        entities.ExistingTribunal.FipLocation = db.GetNullableInt32(reader, 5);
        entities.ExistingTribunal.Identifier = db.GetInt32(reader, 6);
        entities.ExistingTribunal.FipCounty = db.GetNullableInt32(reader, 7);
        entities.ExistingTribunal.FipState = db.GetNullableInt32(reader, 8);
        entities.ExistingTribunal.Populated = true;
        entities.ExistingObligation.Populated = true;
        CheckValid<Obligation>("CpaType", entities.ExistingObligation.CpaType);

        return true;
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
    /// A value of Foreign.
    /// </summary>
    [JsonPropertyName("foreign")]
    public Tribunal Foreign
    {
      get => foreign ??= new();
      set => foreign = value;
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
    /// A value of DeleteForAllObligation.
    /// </summary>
    [JsonPropertyName("deleteForAllObligation")]
    public Common DeleteForAllObligation
    {
      get => deleteForAllObligation ??= new();
      set => deleteForAllObligation = value;
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
    private Fips fips;
    private Tribunal foreign;
    private AdministrativeAction administrativeAction;
    private Common deleteForAllObligation;
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
    /// A value of Rollback.
    /// </summary>
    [JsonPropertyName("rollback")]
    public Common Rollback
    {
      get => rollback ??= new();
      set => rollback = value;
    }

    private Common rollback;
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
    /// A value of ExistingFips.
    /// </summary>
    [JsonPropertyName("existingFips")]
    public Fips ExistingFips
    {
      get => existingFips ??= new();
      set => existingFips = value;
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
    /// A value of ExistingAdministrativeAppeal.
    /// </summary>
    [JsonPropertyName("existingAdministrativeAppeal")]
    public AdministrativeAppeal ExistingAdministrativeAppeal
    {
      get => existingAdministrativeAppeal ??= new();
      set => existingAdministrativeAppeal = value;
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

    private ObligationType existingObligationType;
    private Fips existingFips;
    private Tribunal existingTribunal;
    private AdministrativeAppeal existingAdministrativeAppeal;
    private AdministrativeAction existingAdministrativeAction;
    private CsePersonAccount existingCsePersonAccount;
    private Obligation existingObligation;
    private CsePerson existingCsePerson;
    private ObligationAdministrativeAction existingObligationAdministrativeAction;
      
    private LegalAction existingLegalAction;
  }
#endregion
}
