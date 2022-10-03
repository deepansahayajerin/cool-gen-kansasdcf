// Program: OE_MARH_CREATE_MARRIAGE_HISTORY, ID: 371884873, model: 746.
// Short name: SWE00943
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: OE_MARH_CREATE_MARRIAGE_HISTORY.
/// </para>
/// <para>
/// RESP: OBLGESTB
/// This action block creates marriage_history details.
/// </para>
/// </summary>
[Serializable]
public partial class OeMarhCreateMarriageHistory: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_MARH_CREATE_MARRIAGE_HISTORY program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeMarhCreateMarriageHistory(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeMarhCreateMarriageHistory.
  /// </summary>
  public OeMarhCreateMarriageHistory(IContext context, Import import,
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
    // This action block creates a MARRIAGE_HISTORY record.
    // PROCESSING:
    // This action block is passed the views of new marriage history,the CSE 
    // Person and the spouse (either another CSE person or a contact) involved.
    // It creates marriage_history record and associates it with the prime CSE
    // Person involved and the spouse (other CSE person/Contact).
    // ACTION BLOCKS:
    // ENTITY TYPES USED:
    // 	CSE_PERSON		- R - -
    // 	CONTACT			- R - -
    // 	MARRIAGE_HISTORY	C - - -
    // DATABASE FILES USED:
    // CREATED BY:		govindaraj.
    // DATE CREATED:		01-04-1995.
    // *********************************************
    // *********************************************
    // MAINTENANCE LOG
    // AUTHOR	DATE	CHG REQ#	DESCRIPTION
    // govind	01-04-95		Initial Coding
    // *********************************************
    if (!ReadCsePerson1())
    {
      ExitState = "OE0026_INVALID_CSE_PERSON_NO";

      return;
    }

    if (!IsEmpty(import.SpouseCsePerson.Number))
    {
      if (ReadCsePerson2())
      {
        local.Last.Identifier = 0;

        if (ReadMarriageHistory())
        {
          local.Last.Identifier = entities.ExistingLast.Identifier;
        }

        try
        {
          CreateMarriageHistory2();
          export.New1.Assign(entities.New1);
          export.UpdateTimestamps.LastUpdatedBy = global.UserId;
          export.UpdateTimestamps.LastUpdatedTimestamp = Now();
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              ExitState = "OE0010_CONT_IN_KEY_FOR_MHIST";

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
        ExitState = "OE0050_INVALID_SPOUSE_CSE_PERS";

        return;
      }
    }

    if (!IsEmpty(import.SpouseContact.NameLast) || !
      IsEmpty(import.SpouseContact.NameFirst) || !
      IsEmpty(import.SpouseContact.MiddleInitial))
    {
      // ---------------------------------------------
      // Create CONTACT record for the spouse
      // ---------------------------------------------
      MoveContact3(import.SpouseContact, local.NewSpouse);
      local.NewSpouse.RelationshipToCsePerson = "Curr/Prv Spouse";
      UseOePconCreateContactDetails();

      if (IsExitState("OE0140_CANNOT_ADD_CONTACT_AE"))
      {
        // ***   Okay, continue processing.
        ExitState = "ACO_NN0000_ALL_OK";
      }

      if (ReadContact())
      {
        local.Last.Identifier = 0;

        if (ReadMarriageHistory())
        {
          local.Last.Identifier = entities.ExistingLast.Identifier;
        }

        try
        {
          CreateMarriageHistory1();
          export.New1.Assign(entities.New1);
          export.UpdateTimestamps.LastUpdatedBy = global.UserId;
          export.UpdateTimestamps.LastUpdatedTimestamp = Now();
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              ExitState = "OE0010_CONT_IN_KEY_FOR_MHIST";

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
        ExitState = "OE0049_INVALID_SPOUSE_CONTACT";
      }
    }
  }

  private static void MoveContact1(Contact source, Contact target)
  {
    target.Fax = source.Fax;
    target.ContactNumber = source.ContactNumber;
    target.NameTitle = source.NameTitle;
    target.CompanyName = source.CompanyName;
    target.RelationshipToCsePerson = source.RelationshipToCsePerson;
    target.NameLast = source.NameLast;
    target.NameFirst = source.NameFirst;
    target.MiddleInitial = source.MiddleInitial;
    target.HomePhone = source.HomePhone;
    target.WorkPhone = source.WorkPhone;
  }

  private static void MoveContact2(Contact source, Contact target)
  {
    target.RelationshipToCsePerson = source.RelationshipToCsePerson;
    target.NameLast = source.NameLast;
    target.NameFirst = source.NameFirst;
    target.MiddleInitial = source.MiddleInitial;
  }

  private static void MoveContact3(Contact source, Contact target)
  {
    target.NameLast = source.NameLast;
    target.NameFirst = source.NameFirst;
    target.MiddleInitial = source.MiddleInitial;
  }

  private void UseOePconCreateContactDetails()
  {
    var useImport = new OePconCreateContactDetails.Import();
    var useExport = new OePconCreateContactDetails.Export();

    useImport.CsePerson.Number = import.Prime.Number;
    MoveContact2(local.NewSpouse, useImport.Contact);

    Call(OePconCreateContactDetails.Execute, useImport, useExport);

    MoveContact1(useExport.Contact, export.Spouse);
  }

  private void CreateMarriageHistory1()
  {
    System.Diagnostics.Debug.Assert(entities.Existing.Populated);

    var cspRNumber = entities.ExistingPrime.Number;
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
    var createdBy = global.UserId;
    var createdTimestamp = Now();
    var cspINumber = entities.Existing.CspNumber;
    var conINumber = entities.Existing.ContactNumber;
    var divorceCity = import.New1.DivorceCity ?? "";
    var marriageCertificateCity = import.New1.MarriageCertificateCity ?? "";
    var identifier = local.Last.Identifier + 1;

    entities.New1.Populated = false;
    Update("CreateMarriageHistory1",
      (db, command) =>
      {
        db.SetString(command, "cspRNumber", cspRNumber);
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
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetString(command, "lastUpdatedBy", createdBy);
        db.SetDateTime(command, "lastUpdatedTmst", createdTimestamp);
        db.SetNullableString(command, "cspINumber", cspINumber);
        db.SetNullableInt32(command, "conINumber", conINumber);
        db.SetNullableString(command, "divorceCity", divorceCity);
        db.SetNullableString(command, "marrCertCity", marriageCertificateCity);
        db.SetInt32(command, "identifier", identifier);
      });

    entities.New1.CspRNumber = cspRNumber;
    entities.New1.MarriageDate = marriageDate;
    entities.New1.DivorceCourtOrderNumber = divorceCourtOrderNumber;
    entities.New1.DivorcePetitionDate = divorcePetitionDate;
    entities.New1.MarriageCertificateState = marriageCertificateState;
    entities.New1.MarriageCountry = marriageCountry;
    entities.New1.DivorcePendingInd = divorcePendingInd;
    entities.New1.DivorceCounty = divorceCounty;
    entities.New1.DivorceState = divorceState;
    entities.New1.DivorceCountry = divorceCountry;
    entities.New1.MarriageCertificateCounty = marriageCertificateCounty;
    entities.New1.DivorceDate = divorceDate;
    entities.New1.SeparationDate = separationDate;
    entities.New1.CreatedBy = createdBy;
    entities.New1.CreatedTimestamp = createdTimestamp;
    entities.New1.LastUpdatedBy = createdBy;
    entities.New1.LastUpdatedTimestamp = createdTimestamp;
    entities.New1.CspNumber = null;
    entities.New1.CspINumber = cspINumber;
    entities.New1.ConINumber = conINumber;
    entities.New1.DivorceCity = divorceCity;
    entities.New1.MarriageCertificateCity = marriageCertificateCity;
    entities.New1.Identifier = identifier;
    entities.New1.Populated = true;
  }

  private void CreateMarriageHistory2()
  {
    var cspRNumber = entities.ExistingPrime.Number;
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
    var createdBy = global.UserId;
    var createdTimestamp = Now();
    var cspNumber = entities.ExistingSpouse.Number;
    var divorceCity = import.New1.DivorceCity ?? "";
    var marriageCertificateCity = import.New1.MarriageCertificateCity ?? "";
    var identifier = local.Last.Identifier + 1;

    entities.New1.Populated = false;
    Update("CreateMarriageHistory2",
      (db, command) =>
      {
        db.SetString(command, "cspRNumber", cspRNumber);
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
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetString(command, "lastUpdatedBy", createdBy);
        db.SetDateTime(command, "lastUpdatedTmst", createdTimestamp);
        db.SetNullableString(command, "cspNumber", cspNumber);
        db.SetNullableString(command, "divorceCity", divorceCity);
        db.SetNullableString(command, "marrCertCity", marriageCertificateCity);
        db.SetInt32(command, "identifier", identifier);
      });

    entities.New1.CspRNumber = cspRNumber;
    entities.New1.MarriageDate = marriageDate;
    entities.New1.DivorceCourtOrderNumber = divorceCourtOrderNumber;
    entities.New1.DivorcePetitionDate = divorcePetitionDate;
    entities.New1.MarriageCertificateState = marriageCertificateState;
    entities.New1.MarriageCountry = marriageCountry;
    entities.New1.DivorcePendingInd = divorcePendingInd;
    entities.New1.DivorceCounty = divorceCounty;
    entities.New1.DivorceState = divorceState;
    entities.New1.DivorceCountry = divorceCountry;
    entities.New1.MarriageCertificateCounty = marriageCertificateCounty;
    entities.New1.DivorceDate = divorceDate;
    entities.New1.SeparationDate = separationDate;
    entities.New1.CreatedBy = createdBy;
    entities.New1.CreatedTimestamp = createdTimestamp;
    entities.New1.LastUpdatedBy = createdBy;
    entities.New1.LastUpdatedTimestamp = createdTimestamp;
    entities.New1.CspNumber = cspNumber;
    entities.New1.CspINumber = null;
    entities.New1.ConINumber = null;
    entities.New1.DivorceCity = divorceCity;
    entities.New1.MarriageCertificateCity = marriageCertificateCity;
    entities.New1.Identifier = identifier;
    entities.New1.Populated = true;
  }

  private bool ReadContact()
  {
    entities.Existing.Populated = false;

    return Read("ReadContact",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.ExistingPrime.Number);
        db.SetInt32(command, "contactNumber", export.Spouse.ContactNumber);
      },
      (db, reader) =>
      {
        entities.Existing.CspNumber = db.GetString(reader, 0);
        entities.Existing.ContactNumber = db.GetInt32(reader, 1);
        entities.Existing.RelationshipToCsePerson =
          db.GetNullableString(reader, 2);
        entities.Existing.NameLast = db.GetNullableString(reader, 3);
        entities.Existing.NameFirst = db.GetNullableString(reader, 4);
        entities.Existing.MiddleInitial = db.GetNullableString(reader, 5);
        entities.Existing.Populated = true;
      });
  }

  private bool ReadCsePerson1()
  {
    entities.ExistingPrime.Populated = false;

    return Read("ReadCsePerson1",
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

  private bool ReadCsePerson2()
  {
    entities.ExistingSpouse.Populated = false;

    return Read("ReadCsePerson2",
      (db, command) =>
      {
        db.SetString(command, "numb", import.SpouseCsePerson.Number);
      },
      (db, reader) =>
      {
        entities.ExistingSpouse.Number = db.GetString(reader, 0);
        entities.ExistingSpouse.Populated = true;
      });
  }

  private bool ReadMarriageHistory()
  {
    entities.ExistingLast.Populated = false;

    return Read("ReadMarriageHistory",
      (db, command) =>
      {
        db.SetString(command, "cspRNumber", entities.ExistingPrime.Number);
      },
      (db, reader) =>
      {
        entities.ExistingLast.CspRNumber = db.GetString(reader, 0);
        entities.ExistingLast.Identifier = db.GetInt32(reader, 1);
        entities.ExistingLast.Populated = true;
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
    /// A value of New1.
    /// </summary>
    [JsonPropertyName("new1")]
    public MarriageHistory New1
    {
      get => new1 ??= new();
      set => new1 = value;
    }

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
    /// A value of SpouseCsePerson.
    /// </summary>
    [JsonPropertyName("spouseCsePerson")]
    public CsePerson SpouseCsePerson
    {
      get => spouseCsePerson ??= new();
      set => spouseCsePerson = value;
    }

    /// <summary>
    /// A value of SpouseContact.
    /// </summary>
    [JsonPropertyName("spouseContact")]
    public Contact SpouseContact
    {
      get => spouseContact ??= new();
      set => spouseContact = value;
    }

    private MarriageHistory new1;
    private CsePerson prime;
    private CsePerson spouseCsePerson;
    private Contact spouseContact;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of New1.
    /// </summary>
    [JsonPropertyName("new1")]
    public MarriageHistory New1
    {
      get => new1 ??= new();
      set => new1 = value;
    }

    /// <summary>
    /// A value of UpdateTimestamps.
    /// </summary>
    [JsonPropertyName("updateTimestamps")]
    public MarriageHistory UpdateTimestamps
    {
      get => updateTimestamps ??= new();
      set => updateTimestamps = value;
    }

    /// <summary>
    /// A value of Spouse.
    /// </summary>
    [JsonPropertyName("spouse")]
    public Contact Spouse
    {
      get => spouse ??= new();
      set => spouse = value;
    }

    private MarriageHistory new1;
    private MarriageHistory updateTimestamps;
    private Contact spouse;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of Last.
    /// </summary>
    [JsonPropertyName("last")]
    public MarriageHistory Last
    {
      get => last ??= new();
      set => last = value;
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

    private MarriageHistory last;
    private Contact newSpouse;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of ExistingLast.
    /// </summary>
    [JsonPropertyName("existingLast")]
    public MarriageHistory ExistingLast
    {
      get => existingLast ??= new();
      set => existingLast = value;
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
    /// A value of ExistingSpouse.
    /// </summary>
    [JsonPropertyName("existingSpouse")]
    public CsePerson ExistingSpouse
    {
      get => existingSpouse ??= new();
      set => existingSpouse = value;
    }

    /// <summary>
    /// A value of Existing.
    /// </summary>
    [JsonPropertyName("existing")]
    public Contact Existing
    {
      get => existing ??= new();
      set => existing = value;
    }

    private MarriageHistory existingLast;
    private MarriageHistory new1;
    private CsePerson existingPrime;
    private CsePerson existingSpouse;
    private Contact existing;
  }
#endregion
}
