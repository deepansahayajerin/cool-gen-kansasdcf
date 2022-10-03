// Program: OE_MARH_UPDATE_MARRIAGE_HISTORY, ID: 371884872, model: 746.
// Short name: SWE00946
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: OE_MARH_UPDATE_MARRIAGE_HISTORY.
/// </para>
/// <para>
/// RESP: OBLGESTB
/// This action block updates marriage history details for a CSE Person.
/// </para>
/// </summary>
[Serializable]
public partial class OeMarhUpdateMarriageHistory: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_MARH_UPDATE_MARRIAGE_HISTORY program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeMarhUpdateMarriageHistory(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeMarhUpdateMarriageHistory.
  /// </summary>
  public OeMarhUpdateMarriageHistory(IContext context, Import import,
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
    // *********************************************
    // SYSTEM:		KESSEP
    // MODULE:
    // MODULE TYPE:
    // DESCRIPTION:
    // This action block updates MARRIAGE_HISTORY record.
    // PROCESSING:
    // This action block is passed with new values of marriage history and if 
    // required new spouse (either a CSE Person or Contact). It updates
    // MARRIAGE_HISTORY record and associates with new spouse (CSE person or
    // Contact) if applicable.
    // ACTION BLOCKS:
    // ENTITY TYPES USED:
    // 	CSE_PERSON		- R - -
    // 	CONTACT			- R - -
    // 	MARRIAGE_HISTORY	- R U -
    // DATABASE FILES USED:
    // CREATED BY:		govindaraj.
    // DATE CREATED:		01-04-1995.
    // *********************************************
    // *********************************************
    // MAINTENANCE LOG
    // AUTHOR	DATE	CHG REQ#	DESCRIPTION
    // govind	01-04-95		Initial Coding
    // *********************************************
    if (!ReadCsePerson3())
    {
      ExitState = "OE0026_INVALID_CSE_PERSON_NO";

      return;
    }

    if (ReadMarriageHistory())
    {
      MoveMarriageHistory(entities.ExistingCurrent, export.UpdateTimestamp);

      if (!ReadCsePerson1())
      {
        ReadContact2();
      }
    }
    else
    {
      ExitState = "OE0061_NF_MARRIAGE_HISTORY";

      return;
    }

    if (ReadCsePerson2())
    {
      if (!IsEmpty(entities.ExistingNewSpouseCsePerson.Number))
      {
        UseCabGetClientDetails();
        export.NewSpouseContact.Assign(local.Blank);
      }
    }

    if (import.ExistingSpouseContact.ContactNumber != 0)
    {
      if (ReadContact1())
      {
        local.CurrentSpouse.Assign(entities.ExistingCurrentSpouseContact);

        if (IsEmpty(entities.ExistingNewSpouseCsePerson.Number))
        {
          export.NewSpouseContact.Assign(local.CurrentSpouse);
        }
        else
        {
          export.NewSpouseContact.Assign(local.Blank);
        }
      }
      else
      {
        ExitState = "INVALID_SPOUSE_CONTACT_NUMBER";

        return;
      }
    }

    if (!IsEmpty(import.NewSpouseContact.NameLast) || !
      IsEmpty(import.NewSpouseContact.NameFirst) || !
      IsEmpty(import.NewSpouseContact.MiddleInitial))
    {
      if (!Equal(import.NewSpouseContact.NameLast, local.CurrentSpouse.NameLast) ||
        !
        Equal(import.NewSpouseContact.NameFirst, local.CurrentSpouse.NameFirst) ||
        AsChar(import.NewSpouseContact.MiddleInitial) != AsChar
        (local.CurrentSpouse.MiddleInitial))
      {
        if (local.CurrentSpouse.ContactNumber > 0)
        {
          // ---------------------------------------------
          // Spouse contact name has been changed. Update contact details.
          // ---------------------------------------------
          MoveContact2(import.NewSpouseContact, local.NewSpouse);
          local.NewSpouse.ContactNumber =
            entities.ExistingCurrentSpouseContact.ContactNumber;
          UseOePconUpdateContactDetails();
        }
        else
        {
          // ---------------------------------------------
          // Spouse has been changed to a contact. Create Contact details.
          // ---------------------------------------------
          MoveContact2(import.NewSpouseContact, local.NewSpouse);
          local.NewSpouse.RelationshipToCsePerson = "Curr/Prv Spouse";
          UseOePconCreateContactDetails();
        }

        if (!ReadContact3())
        {
          ExitState = "OE0049_INVALID_SPOUSE_CONTACT";

          return;
        }
      }
    }

    // Update the existing record.
    try
    {
      UpdateMarriageHistory();

      if (!IsEmpty(import.NewSpouseCsePerson.Number) && !
        Equal(import.NewSpouseCsePerson.Number,
        import.ExistingSpouseCsePerson.Number))
      {
        if (!IsEmpty(entities.ExistingCurrentSpouseCsePerson.Number))
        {
          DisassociateMarriageHistory2();
        }
        else if (entities.ExistingCurrentSpouseContact.ContactNumber != 0)
        {
          DisassociateMarriageHistory1();
        }

        AssociateMarriageHistory2();
      }

      if (export.NewSpouseContact.ContactNumber != 0 && export
        .NewSpouseContact.ContactNumber != import
        .ExistingSpouseContact.ContactNumber)
      {
        if (!IsEmpty(entities.ExistingCurrentSpouseCsePerson.Number))
        {
          DisassociateMarriageHistory2();
        }
        else if (entities.ExistingCurrentSpouseContact.ContactNumber != 0)
        {
          DisassociateMarriageHistory1();
        }

        AssociateMarriageHistory1();
      }
    }
    catch(Exception e)
    {
      switch(GetErrorCode(e))
      {
        case ErrorCode.AlreadyExists:
          ExitState = "OE0017_ERR_UPD_MARRIAGE_HIST";

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

  private static void MoveContact1(Contact source, Contact target)
  {
    target.ContactNumber = source.ContactNumber;
    target.RelationshipToCsePerson = source.RelationshipToCsePerson;
    target.NameLast = source.NameLast;
    target.NameFirst = source.NameFirst;
    target.MiddleInitial = source.MiddleInitial;
  }

  private static void MoveContact2(Contact source, Contact target)
  {
    target.RelationshipToCsePerson = source.RelationshipToCsePerson;
    target.NameLast = source.NameLast;
    target.NameFirst = source.NameFirst;
    target.MiddleInitial = source.MiddleInitial;
  }

  private static void MoveMarriageHistory(MarriageHistory source,
    MarriageHistory target)
  {
    target.LastUpdatedBy = source.LastUpdatedBy;
    target.LastUpdatedTimestamp = source.LastUpdatedTimestamp;
  }

  private void UseCabGetClientDetails()
  {
    var useImport = new CabGetClientDetails.Import();
    var useExport = new CabGetClientDetails.Export();

    useImport.CsePerson.Number = import.NewSpouseCsePerson.Number;

    Call(CabGetClientDetails.Execute, useImport, useExport);

    export.NewSpouseCsePersonsWorkSet.Assign(useExport.CsePersonsWorkSet);
  }

  private void UseOePconCreateContactDetails()
  {
    var useImport = new OePconCreateContactDetails.Import();
    var useExport = new OePconCreateContactDetails.Export();

    useImport.CsePerson.Number = import.Prime.Number;
    MoveContact1(local.NewSpouse, useImport.Contact);

    Call(OePconCreateContactDetails.Execute, useImport, useExport);

    MoveContact1(useExport.Contact, export.NewSpouseContact);
  }

  private void UseOePconUpdateContactDetails()
  {
    var useImport = new OePconUpdateContactDetails.Import();
    var useExport = new OePconUpdateContactDetails.Export();

    useImport.CsePerson.Number = import.Prime.Number;
    MoveContact1(local.NewSpouse, useImport.Contact);

    Call(OePconUpdateContactDetails.Execute, useImport, useExport);

    MoveContact1(useExport.Contact, export.NewSpouseContact);
  }

  private void AssociateMarriageHistory1()
  {
    System.Diagnostics.Debug.Assert(entities.ExistingCurrent.Populated);
    System.Diagnostics.Debug.
      Assert(entities.ExistingNewSpouseContact.Populated);

    var cspINumber = entities.ExistingNewSpouseContact.CspNumber;
    var conINumber = entities.ExistingNewSpouseContact.ContactNumber;

    entities.ExistingCurrent.Populated = false;
    Update("AssociateMarriageHistory1",
      (db, command) =>
      {
        db.SetNullableString(command, "cspINumber", cspINumber);
        db.SetNullableInt32(command, "conINumber", conINumber);
        db.
          SetString(command, "cspRNumber", entities.ExistingCurrent.CspRNumber);
          
        db.SetInt32(command, "identifier", entities.ExistingCurrent.Identifier);
      });

    entities.ExistingCurrent.CspINumber = cspINumber;
    entities.ExistingCurrent.ConINumber = conINumber;
    entities.ExistingCurrent.Populated = true;
  }

  private void AssociateMarriageHistory2()
  {
    System.Diagnostics.Debug.Assert(entities.ExistingCurrent.Populated);

    var cspNumber = entities.ExistingNewSpouseCsePerson.Number;

    entities.ExistingCurrent.Populated = false;
    Update("AssociateMarriageHistory2",
      (db, command) =>
      {
        db.SetNullableString(command, "cspNumber", cspNumber);
        db.
          SetString(command, "cspRNumber", entities.ExistingCurrent.CspRNumber);
          
        db.SetInt32(command, "identifier", entities.ExistingCurrent.Identifier);
      });

    entities.ExistingCurrent.CspNumber = cspNumber;
    entities.ExistingCurrent.Populated = true;
  }

  private void DisassociateMarriageHistory1()
  {
    System.Diagnostics.Debug.Assert(entities.ExistingCurrent.Populated);
    entities.ExistingCurrent.Populated = false;
    Update("DisassociateMarriageHistory1",
      (db, command) =>
      {
        db.
          SetString(command, "cspRNumber", entities.ExistingCurrent.CspRNumber);
          
        db.SetInt32(command, "identifier", entities.ExistingCurrent.Identifier);
      });

    entities.ExistingCurrent.CspINumber = null;
    entities.ExistingCurrent.ConINumber = null;
    entities.ExistingCurrent.Populated = true;
  }

  private void DisassociateMarriageHistory2()
  {
    System.Diagnostics.Debug.Assert(entities.ExistingCurrent.Populated);
    entities.ExistingCurrent.Populated = false;
    Update("DisassociateMarriageHistory2",
      (db, command) =>
      {
        db.
          SetString(command, "cspRNumber", entities.ExistingCurrent.CspRNumber);
          
        db.SetInt32(command, "identifier", entities.ExistingCurrent.Identifier);
      });

    entities.ExistingCurrent.CspNumber = null;
    entities.ExistingCurrent.Populated = true;
  }

  private bool ReadContact1()
  {
    entities.ExistingCurrentSpouseContact.Populated = false;

    return Read("ReadContact1",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.ExistingPrime.Number);
        db.SetInt32(
          command, "contactNumber", import.ExistingSpouseContact.ContactNumber);
          
      },
      (db, reader) =>
      {
        entities.ExistingCurrentSpouseContact.CspNumber =
          db.GetString(reader, 0);
        entities.ExistingCurrentSpouseContact.ContactNumber =
          db.GetInt32(reader, 1);
        entities.ExistingCurrentSpouseContact.RelationshipToCsePerson =
          db.GetNullableString(reader, 2);
        entities.ExistingCurrentSpouseContact.NameLast =
          db.GetNullableString(reader, 3);
        entities.ExistingCurrentSpouseContact.NameFirst =
          db.GetNullableString(reader, 4);
        entities.ExistingCurrentSpouseContact.MiddleInitial =
          db.GetNullableString(reader, 5);
        entities.ExistingCurrentSpouseContact.Populated = true;
      });
  }

  private bool ReadContact2()
  {
    System.Diagnostics.Debug.Assert(entities.ExistingCurrent.Populated);
    entities.ExistingCurrentSpouseContact.Populated = false;

    return Read("ReadContact2",
      (db, command) =>
      {
        db.SetInt32(
          command, "contactNumber",
          entities.ExistingCurrent.ConINumber.GetValueOrDefault());
        db.SetString(
          command, "cspNumber", entities.ExistingCurrent.CspINumber ?? "");
      },
      (db, reader) =>
      {
        entities.ExistingCurrentSpouseContact.CspNumber =
          db.GetString(reader, 0);
        entities.ExistingCurrentSpouseContact.ContactNumber =
          db.GetInt32(reader, 1);
        entities.ExistingCurrentSpouseContact.RelationshipToCsePerson =
          db.GetNullableString(reader, 2);
        entities.ExistingCurrentSpouseContact.NameLast =
          db.GetNullableString(reader, 3);
        entities.ExistingCurrentSpouseContact.NameFirst =
          db.GetNullableString(reader, 4);
        entities.ExistingCurrentSpouseContact.MiddleInitial =
          db.GetNullableString(reader, 5);
        entities.ExistingCurrentSpouseContact.Populated = true;
      });
  }

  private bool ReadContact3()
  {
    entities.ExistingNewSpouseContact.Populated = false;

    return Read("ReadContact3",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.ExistingPrime.Number);
        db.SetInt32(
          command, "contactNumber", export.NewSpouseContact.ContactNumber);
      },
      (db, reader) =>
      {
        entities.ExistingNewSpouseContact.CspNumber = db.GetString(reader, 0);
        entities.ExistingNewSpouseContact.ContactNumber =
          db.GetInt32(reader, 1);
        entities.ExistingNewSpouseContact.RelationshipToCsePerson =
          db.GetNullableString(reader, 2);
        entities.ExistingNewSpouseContact.NameLast =
          db.GetNullableString(reader, 3);
        entities.ExistingNewSpouseContact.NameFirst =
          db.GetNullableString(reader, 4);
        entities.ExistingNewSpouseContact.MiddleInitial =
          db.GetNullableString(reader, 5);
        entities.ExistingNewSpouseContact.Populated = true;
      });
  }

  private bool ReadCsePerson1()
  {
    System.Diagnostics.Debug.Assert(entities.ExistingCurrent.Populated);
    entities.ExistingCurrentSpouseCsePerson.Populated = false;

    return Read("ReadCsePerson1",
      (db, command) =>
      {
        db.SetString(command, "numb", entities.ExistingCurrent.CspNumber ?? "");
      },
      (db, reader) =>
      {
        entities.ExistingCurrentSpouseCsePerson.Number =
          db.GetString(reader, 0);
        entities.ExistingCurrentSpouseCsePerson.Populated = true;
      });
  }

  private bool ReadCsePerson2()
  {
    entities.ExistingNewSpouseCsePerson.Populated = false;

    return Read("ReadCsePerson2",
      (db, command) =>
      {
        db.SetString(command, "numb", import.NewSpouseCsePerson.Number);
      },
      (db, reader) =>
      {
        entities.ExistingNewSpouseCsePerson.Number = db.GetString(reader, 0);
        entities.ExistingNewSpouseCsePerson.Populated = true;
      });
  }

  private bool ReadCsePerson3()
  {
    entities.ExistingPrime.Populated = false;

    return Read("ReadCsePerson3",
      (db, command) =>
      {
        db.SetString(command, "numb", import.Prime.Number);
      },
      (db, reader) =>
      {
        entities.ExistingPrime.Number = db.GetString(reader, 0);
        entities.ExistingPrime.Populated = true;
      });
  }

  private bool ReadMarriageHistory()
  {
    entities.ExistingCurrent.Populated = false;

    return Read("ReadMarriageHistory",
      (db, command) =>
      {
        db.SetString(command, "cspRNumber", entities.ExistingPrime.Number);
        db.SetInt32(command, "identifier", import.Existing.Identifier);
      },
      (db, reader) =>
      {
        entities.ExistingCurrent.CspRNumber = db.GetString(reader, 0);
        entities.ExistingCurrent.MarriageDate = db.GetNullableDate(reader, 1);
        entities.ExistingCurrent.DivorceCourtOrderNumber =
          db.GetNullableString(reader, 2);
        entities.ExistingCurrent.DivorcePetitionDate =
          db.GetNullableDate(reader, 3);
        entities.ExistingCurrent.MarriageCertificateState =
          db.GetNullableString(reader, 4);
        entities.ExistingCurrent.MarriageCountry =
          db.GetNullableString(reader, 5);
        entities.ExistingCurrent.DivorcePendingInd =
          db.GetNullableString(reader, 6);
        entities.ExistingCurrent.DivorceCounty =
          db.GetNullableString(reader, 7);
        entities.ExistingCurrent.DivorceState = db.GetNullableString(reader, 8);
        entities.ExistingCurrent.DivorceCountry =
          db.GetNullableString(reader, 9);
        entities.ExistingCurrent.MarriageCertificateCounty =
          db.GetNullableString(reader, 10);
        entities.ExistingCurrent.DivorceDate = db.GetNullableDate(reader, 11);
        entities.ExistingCurrent.SeparationDate =
          db.GetNullableDate(reader, 12);
        entities.ExistingCurrent.CreatedBy = db.GetString(reader, 13);
        entities.ExistingCurrent.CreatedTimestamp = db.GetDateTime(reader, 14);
        entities.ExistingCurrent.LastUpdatedBy = db.GetString(reader, 15);
        entities.ExistingCurrent.LastUpdatedTimestamp =
          db.GetDateTime(reader, 16);
        entities.ExistingCurrent.CspNumber = db.GetNullableString(reader, 17);
        entities.ExistingCurrent.CspINumber = db.GetNullableString(reader, 18);
        entities.ExistingCurrent.ConINumber = db.GetNullableInt32(reader, 19);
        entities.ExistingCurrent.DivorceCity = db.GetNullableString(reader, 20);
        entities.ExistingCurrent.MarriageCertificateCity =
          db.GetNullableString(reader, 21);
        entities.ExistingCurrent.Identifier = db.GetInt32(reader, 22);
        entities.ExistingCurrent.Populated = true;
      });
  }

  private void UpdateMarriageHistory()
  {
    System.Diagnostics.Debug.Assert(entities.ExistingCurrent.Populated);

    var marriageDate = import.New1.MarriageDate;
    var divorceCourtOrderNumber = import.New1.DivorceCourtOrderNumber ?? "";
    var divorcePetitionDate = import.New1.DivorcePetitionDate;
    var marriageCertificateState = import.New1.MarriageCertificateState ?? "";
    var marriageCountry = import.New1.MarriageCountry ?? "";
    var divorcePendingInd = import.New1.DivorcePendingInd ?? "";
    var divorceCounty = import.New1.DivorceCounty ?? "";
    var divorceState = import.New1.DivorceState ?? "";
    var divorceCountry = import.New1.DivorceCountry ?? "";
    var marriageCertificateCounty = import.New1.MarriageCertificateCounty ?? "";
    var divorceDate = import.New1.DivorceDate;
    var separationDate = import.New1.SeparationDate;
    var lastUpdatedBy = global.UserId;
    var lastUpdatedTimestamp = Now();
    var divorceCity = import.New1.DivorceCity ?? "";
    var marriageCertificateCity = import.New1.MarriageCertificateCity ?? "";

    entities.ExistingCurrent.Populated = false;
    Update("UpdateMarriageHistory",
      (db, command) =>
      {
        db.SetNullableDate(command, "marriageDate", marriageDate);
        db.SetNullableString(command, "divCtordNo", divorceCourtOrderNumber);
        db.SetNullableDate(command, "divPetitionDt", divorcePetitionDate);
        db.
          SetNullableString(command, "marrCertState", marriageCertificateState);
          
        db.SetNullableString(command, "marriageCountry", marriageCountry);
        db.SetNullableString(command, "divPendingInd", divorcePendingInd);
        db.SetNullableString(command, "divorceCounty", divorceCounty);
        db.SetNullableString(command, "divorceState", divorceState);
        db.SetNullableString(command, "divorceCountry", divorceCountry);
        db.SetNullableString(
          command, "marrCertCounty", marriageCertificateCounty);
        db.SetNullableDate(command, "divorceDate", divorceDate);
        db.SetNullableDate(command, "separationDate", separationDate);
        db.SetString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetDateTime(command, "lastUpdatedTmst", lastUpdatedTimestamp);
        db.SetNullableString(command, "divorceCity", divorceCity);
        db.SetNullableString(command, "marrCertCity", marriageCertificateCity);
        db.
          SetString(command, "cspRNumber", entities.ExistingCurrent.CspRNumber);
          
        db.SetInt32(command, "identifier", entities.ExistingCurrent.Identifier);
      });

    entities.ExistingCurrent.MarriageDate = marriageDate;
    entities.ExistingCurrent.DivorceCourtOrderNumber = divorceCourtOrderNumber;
    entities.ExistingCurrent.DivorcePetitionDate = divorcePetitionDate;
    entities.ExistingCurrent.MarriageCertificateState =
      marriageCertificateState;
    entities.ExistingCurrent.MarriageCountry = marriageCountry;
    entities.ExistingCurrent.DivorcePendingInd = divorcePendingInd;
    entities.ExistingCurrent.DivorceCounty = divorceCounty;
    entities.ExistingCurrent.DivorceState = divorceState;
    entities.ExistingCurrent.DivorceCountry = divorceCountry;
    entities.ExistingCurrent.MarriageCertificateCounty =
      marriageCertificateCounty;
    entities.ExistingCurrent.DivorceDate = divorceDate;
    entities.ExistingCurrent.SeparationDate = separationDate;
    entities.ExistingCurrent.LastUpdatedBy = lastUpdatedBy;
    entities.ExistingCurrent.LastUpdatedTimestamp = lastUpdatedTimestamp;
    entities.ExistingCurrent.DivorceCity = divorceCity;
    entities.ExistingCurrent.MarriageCertificateCity = marriageCertificateCity;
    entities.ExistingCurrent.Populated = true;
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
    /// A value of Prime.
    /// </summary>
    [JsonPropertyName("prime")]
    public CsePerson Prime
    {
      get => prime ??= new();
      set => prime = value;
    }

    /// <summary>
    /// A value of ExistingSpouseCsePerson.
    /// </summary>
    [JsonPropertyName("existingSpouseCsePerson")]
    public CsePerson ExistingSpouseCsePerson
    {
      get => existingSpouseCsePerson ??= new();
      set => existingSpouseCsePerson = value;
    }

    /// <summary>
    /// A value of ExistingSpouseContact.
    /// </summary>
    [JsonPropertyName("existingSpouseContact")]
    public Contact ExistingSpouseContact
    {
      get => existingSpouseContact ??= new();
      set => existingSpouseContact = value;
    }

    /// <summary>
    /// A value of Existing.
    /// </summary>
    [JsonPropertyName("existing")]
    public MarriageHistory Existing
    {
      get => existing ??= new();
      set => existing = value;
    }

    /// <summary>
    /// A value of NewSpouseCsePerson.
    /// </summary>
    [JsonPropertyName("newSpouseCsePerson")]
    public CsePerson NewSpouseCsePerson
    {
      get => newSpouseCsePerson ??= new();
      set => newSpouseCsePerson = value;
    }

    /// <summary>
    /// A value of NewSpouseContact.
    /// </summary>
    [JsonPropertyName("newSpouseContact")]
    public Contact NewSpouseContact
    {
      get => newSpouseContact ??= new();
      set => newSpouseContact = value;
    }

    /// <summary>
    /// A value of New1.
    /// </summary>
    [JsonPropertyName("new1")]
    public MarriageHistory New1
    {
      get => new1 ??= new();
      set => new1 = value;
    }

    private CsePerson prime;
    private CsePerson existingSpouseCsePerson;
    private Contact existingSpouseContact;
    private MarriageHistory existing;
    private CsePerson newSpouseCsePerson;
    private Contact newSpouseContact;
    private MarriageHistory new1;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of NewSpouseCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("newSpouseCsePersonsWorkSet")]
    public CsePersonsWorkSet NewSpouseCsePersonsWorkSet
    {
      get => newSpouseCsePersonsWorkSet ??= new();
      set => newSpouseCsePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of UpdateTimestamp.
    /// </summary>
    [JsonPropertyName("updateTimestamp")]
    public MarriageHistory UpdateTimestamp
    {
      get => updateTimestamp ??= new();
      set => updateTimestamp = value;
    }

    /// <summary>
    /// A value of NewSpouseContact.
    /// </summary>
    [JsonPropertyName("newSpouseContact")]
    public Contact NewSpouseContact
    {
      get => newSpouseContact ??= new();
      set => newSpouseContact = value;
    }

    private CsePersonsWorkSet newSpouseCsePersonsWorkSet;
    private MarriageHistory updateTimestamp;
    private Contact newSpouseContact;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of Blank.
    /// </summary>
    [JsonPropertyName("blank")]
    public Contact Blank
    {
      get => blank ??= new();
      set => blank = value;
    }

    /// <summary>
    /// A value of NewSpouse.
    /// </summary>
    [JsonPropertyName("newSpouse")]
    public Contact NewSpouse
    {
      get => newSpouse ??= new();
      set => newSpouse = value;
    }

    /// <summary>
    /// A value of CurrentSpouse.
    /// </summary>
    [JsonPropertyName("currentSpouse")]
    public Contact CurrentSpouse
    {
      get => currentSpouse ??= new();
      set => currentSpouse = value;
    }

    private Contact blank;
    private Contact newSpouse;
    private Contact currentSpouse;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of ChangedNew.
    /// </summary>
    [JsonPropertyName("changedNew")]
    public MarriageHistory ChangedNew
    {
      get => changedNew ??= new();
      set => changedNew = value;
    }

    /// <summary>
    /// A value of ExistingPrime.
    /// </summary>
    [JsonPropertyName("existingPrime")]
    public CsePerson ExistingPrime
    {
      get => existingPrime ??= new();
      set => existingPrime = value;
    }

    /// <summary>
    /// A value of ExistingCurrentSpouseCsePerson.
    /// </summary>
    [JsonPropertyName("existingCurrentSpouseCsePerson")]
    public CsePerson ExistingCurrentSpouseCsePerson
    {
      get => existingCurrentSpouseCsePerson ??= new();
      set => existingCurrentSpouseCsePerson = value;
    }

    /// <summary>
    /// A value of ExistingCurrentSpouseContact.
    /// </summary>
    [JsonPropertyName("existingCurrentSpouseContact")]
    public Contact ExistingCurrentSpouseContact
    {
      get => existingCurrentSpouseContact ??= new();
      set => existingCurrentSpouseContact = value;
    }

    /// <summary>
    /// A value of ExistingCurrent.
    /// </summary>
    [JsonPropertyName("existingCurrent")]
    public MarriageHistory ExistingCurrent
    {
      get => existingCurrent ??= new();
      set => existingCurrent = value;
    }

    /// <summary>
    /// A value of ExistingNewSpouseCsePerson.
    /// </summary>
    [JsonPropertyName("existingNewSpouseCsePerson")]
    public CsePerson ExistingNewSpouseCsePerson
    {
      get => existingNewSpouseCsePerson ??= new();
      set => existingNewSpouseCsePerson = value;
    }

    /// <summary>
    /// A value of ExistingNewSpouseContact.
    /// </summary>
    [JsonPropertyName("existingNewSpouseContact")]
    public Contact ExistingNewSpouseContact
    {
      get => existingNewSpouseContact ??= new();
      set => existingNewSpouseContact = value;
    }

    private MarriageHistory changedNew;
    private CsePerson existingPrime;
    private CsePerson existingCurrentSpouseCsePerson;
    private Contact existingCurrentSpouseContact;
    private MarriageHistory existingCurrent;
    private CsePerson existingNewSpouseCsePerson;
    private Contact existingNewSpouseContact;
  }
#endregion
}
