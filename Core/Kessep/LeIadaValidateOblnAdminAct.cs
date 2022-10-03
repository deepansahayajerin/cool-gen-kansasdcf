// Program: LE_IADA_VALIDATE_OBLN_ADMIN_ACT, ID: 372594982, model: 746.
// Short name: SWE00786
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
/// A program: LE_IADA_VALIDATE_OBLN_ADMIN_ACT.
/// </para>
/// <para>
/// RESP: LGLENFAC
/// This action block creates OBLIGATION_ADMINISTRATIVE_ACTION and associates 
/// with all or specified obligations.
/// </para>
/// </summary>
[Serializable]
public partial class LeIadaValidateOblnAdminAct: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the LE_IADA_VALIDATE_OBLN_ADMIN_ACT program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new LeIadaValidateOblnAdminAct(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of LeIadaValidateOblnAdminAct.
  /// </summary>
  public LeIadaValidateOblnAdminAct(IContext context, Import import,
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
    // *********************************************************
    // 10/16/98   P. Sharp  Removed redundant reads of fips, tribunal and legal 
    // action. Added validation to admin action that only manual action allowed.
    local.Current.Date = Now().Date;
    export.ErrorCodes.Index = -1;

    if (IsEmpty(import.LegalAction.CourtCaseNumber) && AsChar
      (import.AllObligation.Flag) != 'N')
    {
      // ---------------------------------------------
      // If court case no was not supplied, then the worker must choose an 
      // obligation. Dont allow taking an admin action on ALL obligations of ALL
      // court cases for the obligor.
      // ---------------------------------------------
      ++export.ErrorCodes.Index;
      export.ErrorCodes.CheckSize();

      export.ErrorCodes.Update.DetailErrorCode.Count = 12;
      export.LastErrorEntryNo.Count = export.ErrorCodes.Index + 1;
    }

    if (AsChar(import.AllObligation.Flag) != 'Y' && AsChar
      (import.AllObligation.Flag) != 'N')
    {
      ++export.ErrorCodes.Index;
      export.ErrorCodes.CheckSize();

      export.ErrorCodes.Update.DetailErrorCode.Count = 1;
      export.LastErrorEntryNo.Count = export.ErrorCodes.Index + 1;
    }

    if (!IsEmpty(import.AdministrativeAction.Type1))
    {
      if (ReadAdministrativeAction())
      {
        if (AsChar(entities.ExistingAdministrativeAction.Indicator) == 'A')
        {
          ++export.ErrorCodes.Index;
          export.ErrorCodes.CheckSize();

          export.ErrorCodes.Update.DetailErrorCode.Count = 13;
          export.LastErrorEntryNo.Count = export.ErrorCodes.Index + 1;
        }
      }
      else
      {
        ++export.ErrorCodes.Index;
        export.ErrorCodes.CheckSize();

        export.ErrorCodes.Update.DetailErrorCode.Count = 8;
        export.LastErrorEntryNo.Count = export.ErrorCodes.Index + 1;
      }
    }
    else
    {
      ++export.ErrorCodes.Index;
      export.ErrorCodes.CheckSize();

      export.ErrorCodes.Update.DetailErrorCode.Count = 8;
      export.LastErrorEntryNo.Count = export.ErrorCodes.Index + 1;
    }

    if (!Lt(local.InitialisedToZeros.TakenDate,
      import.ObligationAdministrativeAction.TakenDate))
    {
      ++export.ErrorCodes.Index;
      export.ErrorCodes.CheckSize();

      export.ErrorCodes.Update.DetailErrorCode.Count = 14;
      export.LastErrorEntryNo.Count = export.ErrorCodes.Index + 1;
    }

    if (Lt(local.Current.Date, import.ObligationAdministrativeAction.TakenDate))
    {
      ++export.ErrorCodes.Index;
      export.ErrorCodes.CheckSize();

      export.ErrorCodes.Update.DetailErrorCode.Count = 9;
      export.LastErrorEntryNo.Count = export.ErrorCodes.Index + 1;
    }

    if (Lt(local.InitialisedToZeros.TakenDate,
      import.ObligationAdministrativeAction.ResponseDate) || !
      IsEmpty(import.ObligationAdministrativeAction.Response))
    {
      if (!Lt(local.InitialisedToZeros.TakenDate,
        import.ObligationAdministrativeAction.ResponseDate) || Lt
        (import.ObligationAdministrativeAction.ResponseDate,
        import.ObligationAdministrativeAction.TakenDate) || Lt
        (local.Current.Date, import.ObligationAdministrativeAction.ResponseDate))
        
      {
        ++export.ErrorCodes.Index;
        export.ErrorCodes.CheckSize();

        export.ErrorCodes.Update.DetailErrorCode.Count = 10;
        export.LastErrorEntryNo.Count = export.ErrorCodes.Index + 1;
      }

      if (IsEmpty(import.ObligationAdministrativeAction.Response))
      {
        ++export.ErrorCodes.Index;
        export.ErrorCodes.CheckSize();

        export.ErrorCodes.Update.DetailErrorCode.Count = 11;
        export.LastErrorEntryNo.Count = export.ErrorCodes.Index + 1;
      }
    }

    if (!ReadCsePersonAccount())
    {
      if (ReadCsePerson())
      {
        ++export.ErrorCodes.Index;
        export.ErrorCodes.CheckSize();

        export.ErrorCodes.Update.DetailErrorCode.Count = 6;
        export.LastErrorEntryNo.Count = export.ErrorCodes.Index + 1;

        return;
      }
      else
      {
        ++export.ErrorCodes.Index;
        export.ErrorCodes.CheckSize();

        export.ErrorCodes.Update.DetailErrorCode.Count = 5;
        export.LastErrorEntryNo.Count = export.ErrorCodes.Index + 1;

        return;
      }
    }

    if (AsChar(import.AllObligation.Flag) == 'Y')
    {
      // ---------------------------------------------
      // Statement IF legal_action identifier = 0
      //                       error 02
      // has been deleted from here.
      // ---------------------------------------------
      if (!IsEmpty(import.Fips.StateAbbreviation))
      {
        if (!ReadLegalActionPerson1())
        {
          ++export.ErrorCodes.Index;
          export.ErrorCodes.CheckSize();

          export.ErrorCodes.Update.DetailErrorCode.Count = 15;
          export.LastErrorEntryNo.Count = export.ErrorCodes.Index + 1;

          return;
        }
      }
      else if (!ReadLegalActionPerson2())
      {
        ++export.ErrorCodes.Index;
        export.ErrorCodes.CheckSize();

        export.ErrorCodes.Update.DetailErrorCode.Count = 15;
        export.LastErrorEntryNo.Count = export.ErrorCodes.Index + 1;

        return;
      }
    }
    else
    {
      if (import.Obligation.SystemGeneratedIdentifier == 0)
      {
        ++export.ErrorCodes.Index;
        export.ErrorCodes.CheckSize();

        export.ErrorCodes.Update.DetailErrorCode.Count = 4;
        export.LastErrorEntryNo.Count = export.ErrorCodes.Index + 1;

        return;
      }

      if (!ReadObligation())
      {
        ++export.ErrorCodes.Index;
        export.ErrorCodes.CheckSize();

        export.ErrorCodes.Update.DetailErrorCode.Count = 7;
        export.LastErrorEntryNo.Count = export.ErrorCodes.Index + 1;

        return;
      }

      if (ReadLegalAction())
      {
        if (!ReadLegalActionPerson3())
        {
          ++export.ErrorCodes.Index;
          export.ErrorCodes.CheckSize();

          export.ErrorCodes.Update.DetailErrorCode.Count = 15;
          export.LastErrorEntryNo.Count = export.ErrorCodes.Index + 1;

          return;
        }
      }
      else
      {
        // ----- Not an error. Obligation has no associated legal action
      }
    }

    if (Equal(import.UserAction.Command, "DELETE"))
    {
      // ---------------------------------------------
      // No more validations needed for Delete command
      // ---------------------------------------------
      return;
    }

    if (AsChar(import.AllObligation.Flag) == 'Y')
    {
      local.ObligationFound.Flag = "N";

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

        local.ObligationFound.Flag = "Y";

        break;
      }

      if (AsChar(local.ObligationFound.Flag) == 'N')
      {
        ++export.ErrorCodes.Index;
        export.ErrorCodes.CheckSize();

        export.ErrorCodes.Update.DetailErrorCode.Count = 7;
        export.LastErrorEntryNo.Count = export.ErrorCodes.Index + 1;
      }
    }
    else if (!ReadObligationObligationType())
    {
      ++export.ErrorCodes.Index;
      export.ErrorCodes.CheckSize();

      export.ErrorCodes.Update.DetailErrorCode.Count = 7;
      export.LastErrorEntryNo.Count = export.ErrorCodes.Index + 1;
    }
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
        entities.ExistingFips.StateAbbreviation = db.GetString(reader, 3);
        entities.ExistingFips.CountyAbbreviation =
          db.GetNullableString(reader, 4);
        entities.ExistingFips.Populated = true;
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
        entities.ExistingLegalAction.Classification = db.GetString(reader, 1);
        entities.ExistingLegalAction.CourtCaseNumber =
          db.GetNullableString(reader, 2);
        entities.ExistingLegalAction.TrbId = db.GetNullableInt32(reader, 3);
        entities.ExistingLegalAction.Populated = true;
      });
  }

  private bool ReadLegalActionPerson1()
  {
    entities.ExistingLegalActionPerson.Populated = false;

    return Read("ReadLegalActionPerson1",
      (db, command) =>
      {
        db.SetNullableString(
          command, "courtCaseNo", import.LegalAction.CourtCaseNumber ?? "");
        db.SetNullableString(command, "cspNumber", import.CsePerson.Number);
        db.
          SetString(command, "stateAbbreviation", import.Fips.StateAbbreviation);
          
        db.SetNullableString(
          command, "countyAbbr", import.Fips.CountyAbbreviation ?? "");
      },
      (db, reader) =>
      {
        entities.ExistingLegalActionPerson.Identifier = db.GetInt32(reader, 0);
        entities.ExistingLegalActionPerson.CspNumber =
          db.GetNullableString(reader, 1);
        entities.ExistingLegalActionPerson.LgaRIdentifier =
          db.GetNullableInt32(reader, 2);
        entities.ExistingLegalActionPerson.LadRNumber =
          db.GetNullableInt32(reader, 3);
        entities.ExistingLegalActionPerson.AccountType =
          db.GetNullableString(reader, 4);
        entities.ExistingLegalActionPerson.Populated = true;
      });
  }

  private bool ReadLegalActionPerson2()
  {
    entities.ExistingLegalActionPerson.Populated = false;

    return Read("ReadLegalActionPerson2",
      (db, command) =>
      {
        db.SetNullableString(
          command, "courtCaseNo", import.LegalAction.CourtCaseNumber ?? "");
        db.SetNullableInt32(command, "trbId", import.Tribunal.Identifier);
        db.SetNullableString(command, "cspNumber", import.CsePerson.Number);
      },
      (db, reader) =>
      {
        entities.ExistingLegalActionPerson.Identifier = db.GetInt32(reader, 0);
        entities.ExistingLegalActionPerson.CspNumber =
          db.GetNullableString(reader, 1);
        entities.ExistingLegalActionPerson.LgaRIdentifier =
          db.GetNullableInt32(reader, 2);
        entities.ExistingLegalActionPerson.LadRNumber =
          db.GetNullableInt32(reader, 3);
        entities.ExistingLegalActionPerson.AccountType =
          db.GetNullableString(reader, 4);
        entities.ExistingLegalActionPerson.Populated = true;
      });
  }

  private bool ReadLegalActionPerson3()
  {
    entities.ExistingLegalActionPerson.Populated = false;

    return Read("ReadLegalActionPerson3",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "lgaRIdentifier", entities.ExistingLegalAction.Identifier);
        db.SetNullableString(command, "cspNumber", import.CsePerson.Number);
      },
      (db, reader) =>
      {
        entities.ExistingLegalActionPerson.Identifier = db.GetInt32(reader, 0);
        entities.ExistingLegalActionPerson.CspNumber =
          db.GetNullableString(reader, 1);
        entities.ExistingLegalActionPerson.LgaRIdentifier =
          db.GetNullableInt32(reader, 2);
        entities.ExistingLegalActionPerson.LadRNumber =
          db.GetNullableInt32(reader, 3);
        entities.ExistingLegalActionPerson.AccountType =
          db.GetNullableString(reader, 4);
        entities.ExistingLegalActionPerson.Populated = true;
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
        db.SetInt32(
          command, "dtyGeneratedId",
          import.ObligationType.SystemGeneratedIdentifier);
        db.
          SetInt32(command, "obId", import.Obligation.SystemGeneratedIdentifier);
          
      },
      (db, reader) =>
      {
        entities.ExistingObligation.CpaType = db.GetString(reader, 0);
        entities.ExistingObligation.CspNumber = db.GetString(reader, 1);
        entities.ExistingObligation.SystemGeneratedIdentifier =
          db.GetInt32(reader, 2);
        entities.ExistingObligation.DtyGeneratedId = db.GetInt32(reader, 3);
        entities.ExistingObligation.LgaId = db.GetNullableInt32(reader, 4);
        entities.ExistingObligation.Populated = true;
        CheckValid<Obligation>("CpaType", entities.ExistingObligation.CpaType);
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
    /// A value of UserAction.
    /// </summary>
    [JsonPropertyName("userAction")]
    public Common UserAction
    {
      get => userAction ??= new();
      set => userAction = value;
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
    /// A value of AllObligation.
    /// </summary>
    [JsonPropertyName("allObligation")]
    public Common AllObligation
    {
      get => allObligation ??= new();
      set => allObligation = value;
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
    private Tribunal tribunal;
    private Common userAction;
    private AdministrativeAction administrativeAction;
    private Common allObligation;
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
    /// <summary>A ErrorCodesGroup group.</summary>
    [Serializable]
    public class ErrorCodesGroup
    {
      /// <summary>
      /// A value of DetailErrorCode.
      /// </summary>
      [JsonPropertyName("detailErrorCode")]
      public Common DetailErrorCode
      {
        get => detailErrorCode ??= new();
        set => detailErrorCode = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 100;

      private Common detailErrorCode;
    }

    /// <summary>
    /// A value of LastErrorEntryNo.
    /// </summary>
    [JsonPropertyName("lastErrorEntryNo")]
    public Common LastErrorEntryNo
    {
      get => lastErrorEntryNo ??= new();
      set => lastErrorEntryNo = value;
    }

    /// <summary>
    /// Gets a value of ErrorCodes.
    /// </summary>
    [JsonIgnore]
    public Array<ErrorCodesGroup> ErrorCodes => errorCodes ??= new(
      ErrorCodesGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of ErrorCodes for json serialization.
    /// </summary>
    [JsonPropertyName("errorCodes")]
    [Computed]
    public IList<ErrorCodesGroup> ErrorCodes_Json
    {
      get => errorCodes;
      set => ErrorCodes.Assign(value);
    }

    private Common lastErrorEntryNo;
    private Array<ErrorCodesGroup> errorCodes;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
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
    /// A value of ValidCode.
    /// </summary>
    [JsonPropertyName("validCode")]
    public Common ValidCode
    {
      get => validCode ??= new();
      set => validCode = value;
    }

    /// <summary>
    /// A value of CodeValue.
    /// </summary>
    [JsonPropertyName("codeValue")]
    public CodeValue CodeValue
    {
      get => codeValue ??= new();
      set => codeValue = value;
    }

    /// <summary>
    /// A value of Code.
    /// </summary>
    [JsonPropertyName("code")]
    public Code Code
    {
      get => code ??= new();
      set => code = value;
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
    /// A value of InitialisedToZeros.
    /// </summary>
    [JsonPropertyName("initialisedToZeros")]
    public ObligationAdministrativeAction InitialisedToZeros
    {
      get => initialisedToZeros ??= new();
      set => initialisedToZeros = value;
    }

    private DateWorkArea current;
    private Common validCode;
    private CodeValue codeValue;
    private Code code;
    private Common obligationFound;
    private ObligationAdministrativeAction initialisedToZeros;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of ObligationTransaction.
    /// </summary>
    [JsonPropertyName("obligationTransaction")]
    public ObligationTransaction ObligationTransaction
    {
      get => obligationTransaction ??= new();
      set => obligationTransaction = value;
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
    /// A value of ExistingLegalActionDetail.
    /// </summary>
    [JsonPropertyName("existingLegalActionDetail")]
    public LegalActionDetail ExistingLegalActionDetail
    {
      get => existingLegalActionDetail ??= new();
      set => existingLegalActionDetail = value;
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

    /// <summary>
    /// A value of ExistingLegalActionPerson.
    /// </summary>
    [JsonPropertyName("existingLegalActionPerson")]
    public LegalActionPerson ExistingLegalActionPerson
    {
      get => existingLegalActionPerson ??= new();
      set => existingLegalActionPerson = value;
    }

    private ObligationTransaction obligationTransaction;
    private ObligationType existingObligationType;
    private Fips existingFips;
    private Tribunal existingTribunal;
    private LegalActionDetail existingLegalActionDetail;
    private AdministrativeAction existingAdministrativeAction;
    private CsePersonAccount existingCsePersonAccount;
    private Obligation existingObligation;
    private CsePerson existingCsePerson;
    private ObligationAdministrativeAction existingObligationAdministrativeAction;
      
    private LegalAction existingLegalAction;
    private LegalActionPerson existingLegalActionPerson;
  }
#endregion
}
