// Program: OE_ATTY_DELETE_PRIV_ATTORNEY, ID: 372179495, model: 746.
// Short name: SWE00857
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: OE_ATTY_DELETE_PRIV_ATTORNEY.
/// </para>
/// <para>
/// RESP: OBLGESTB
/// This action block facilitates creation of PERSON_PRIVATE_ATTORNEY and 
/// PRIVATE_ATTORNEY_ADDRESS records.
/// </para>
/// </summary>
[Serializable]
public partial class OeAttyDeletePrivAttorney: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_ATTY_DELETE_PRIV_ATTORNEY program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeAttyDeletePrivAttorney(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeAttyDeletePrivAttorney.
  /// </summary>
  public OeAttyDeletePrivAttorney(IContext context, Import import, Export export)
    :
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
    // This action block deletes Person Private Attorney and Private Attorney 
    // Address records.
    // PROCESSING:
    // ACTION BLOCKS:
    // ENTITY TYPES USED:
    //  CSE_PERSON			- R - -
    //  CASE				- R - -
    //  PERSON_PRIVATE_ATTORNEY	- R - D
    //  PRIVATE_ATTORNEY_ADDRESS	- R - D
    // DATABASE FILES USED:
    // CREATED BY:	Govindaraj
    // DATE CREATED:	02/22/1995
    // *********************************************
    // *********************************************
    // MAINTENANCE LOG
    // AUTHOR	DATE		CHG REQ#	DESCRIPTION
    // govind	02/22/95			Initial coding
    // *********************************************
    // 	
    if (!ReadPersonPrivateAttorney())
    {
      ExitState = "PERSONS_ATTORNEY_NF";

      return;
    }

    foreach(var item in ReadPrivateAttorneyAddress())
    {
      DeletePrivateAttorneyAddress();
    }

    DeletePersonPrivateAttorney();
  }

  private void DeletePersonPrivateAttorney()
  {
    Update("DeletePersonPrivateAttorney",
      (db, command) =>
      {
        db.SetString(
          command, "cspNumber",
          entities.ExistingPersonPrivateAttorney.CspNumber);
        db.SetInt32(
          command, "identifier",
          entities.ExistingPersonPrivateAttorney.Identifier);
      });
  }

  private void DeletePrivateAttorneyAddress()
  {
    Update("DeletePrivateAttorneyAddress",
      (db, command) =>
      {
        db.SetInt32(
          command, "ppaIdentifier",
          entities.ExistingPrivateAttorneyAddress.PpaIdentifier);
        db.SetString(
          command, "cspNumber",
          entities.ExistingPrivateAttorneyAddress.CspNumber);
        db.SetDate(
          command, "effectiveDate",
          entities.ExistingPrivateAttorneyAddress.EffectiveDate.
            GetValueOrDefault());
      });
  }

  private bool ReadPersonPrivateAttorney()
  {
    entities.ExistingPersonPrivateAttorney.Populated = false;

    return Read("ReadPersonPrivateAttorney",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", import.CsePerson.Number);
        db.SetNullableString(command, "casNumber", import.Case1.Number);
        db.SetInt32(
          command, "identifier", import.PersonPrivateAttorney.Identifier);
      },
      (db, reader) =>
      {
        entities.ExistingPersonPrivateAttorney.CspNumber =
          db.GetString(reader, 0);
        entities.ExistingPersonPrivateAttorney.Identifier =
          db.GetInt32(reader, 1);
        entities.ExistingPersonPrivateAttorney.CasNumber =
          db.GetNullableString(reader, 2);
        entities.ExistingPersonPrivateAttorney.DateRetained =
          db.GetDate(reader, 3);
        entities.ExistingPersonPrivateAttorney.DateDismissed =
          db.GetDate(reader, 4);
        entities.ExistingPersonPrivateAttorney.LastName =
          db.GetNullableString(reader, 5);
        entities.ExistingPersonPrivateAttorney.FirstName =
          db.GetNullableString(reader, 6);
        entities.ExistingPersonPrivateAttorney.MiddleInitial =
          db.GetNullableString(reader, 7);
        entities.ExistingPersonPrivateAttorney.FirmName =
          db.GetNullableString(reader, 8);
        entities.ExistingPersonPrivateAttorney.Phone =
          db.GetNullableInt32(reader, 9);
        entities.ExistingPersonPrivateAttorney.FaxNumber =
          db.GetNullableInt32(reader, 10);
        entities.ExistingPersonPrivateAttorney.CreatedBy =
          db.GetString(reader, 11);
        entities.ExistingPersonPrivateAttorney.CreatedTimestamp =
          db.GetDateTime(reader, 12);
        entities.ExistingPersonPrivateAttorney.LastUpdatedBy =
          db.GetString(reader, 13);
        entities.ExistingPersonPrivateAttorney.LastUpdatedTimestamp =
          db.GetDateTime(reader, 14);
        entities.ExistingPersonPrivateAttorney.EmailAddress =
          db.GetNullableString(reader, 15);
        entities.ExistingPersonPrivateAttorney.BarNumber =
          db.GetNullableString(reader, 16);
        entities.ExistingPersonPrivateAttorney.ConsentIndicator =
          db.GetNullableString(reader, 17);
        entities.ExistingPersonPrivateAttorney.Note =
          db.GetNullableString(reader, 18);
        entities.ExistingPersonPrivateAttorney.Populated = true;
      });
  }

  private IEnumerable<bool> ReadPrivateAttorneyAddress()
  {
    System.Diagnostics.Debug.Assert(
      entities.ExistingPersonPrivateAttorney.Populated);
    entities.ExistingPrivateAttorneyAddress.Populated = false;

    return ReadEach("ReadPrivateAttorneyAddress",
      (db, command) =>
      {
        db.SetInt32(
          command, "ppaIdentifier",
          entities.ExistingPersonPrivateAttorney.Identifier);
        db.SetString(
          command, "cspNumber",
          entities.ExistingPersonPrivateAttorney.CspNumber);
      },
      (db, reader) =>
      {
        entities.ExistingPrivateAttorneyAddress.PpaIdentifier =
          db.GetInt32(reader, 0);
        entities.ExistingPrivateAttorneyAddress.CspNumber =
          db.GetString(reader, 1);
        entities.ExistingPrivateAttorneyAddress.EffectiveDate =
          db.GetDate(reader, 2);
        entities.ExistingPrivateAttorneyAddress.Street1 =
          db.GetNullableString(reader, 3);
        entities.ExistingPrivateAttorneyAddress.Street2 =
          db.GetNullableString(reader, 4);
        entities.ExistingPrivateAttorneyAddress.City =
          db.GetNullableString(reader, 5);
        entities.ExistingPrivateAttorneyAddress.State =
          db.GetNullableString(reader, 6);
        entities.ExistingPrivateAttorneyAddress.Province =
          db.GetNullableString(reader, 7);
        entities.ExistingPrivateAttorneyAddress.PostalCode =
          db.GetNullableString(reader, 8);
        entities.ExistingPrivateAttorneyAddress.ZipCode5 =
          db.GetNullableString(reader, 9);
        entities.ExistingPrivateAttorneyAddress.ZipCode4 =
          db.GetNullableString(reader, 10);
        entities.ExistingPrivateAttorneyAddress.Zip3 =
          db.GetNullableString(reader, 11);
        entities.ExistingPrivateAttorneyAddress.Country =
          db.GetNullableString(reader, 12);
        entities.ExistingPrivateAttorneyAddress.AddressType =
          db.GetNullableString(reader, 13);
        entities.ExistingPrivateAttorneyAddress.CreatedBy =
          db.GetString(reader, 14);
        entities.ExistingPrivateAttorneyAddress.CreatedTimestamp =
          db.GetDateTime(reader, 15);
        entities.ExistingPrivateAttorneyAddress.LastUpdatedBy =
          db.GetString(reader, 16);
        entities.ExistingPrivateAttorneyAddress.LastUpdatedTimestamp =
          db.GetDateTime(reader, 17);
        entities.ExistingPrivateAttorneyAddress.Populated = true;

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
    /// A value of PersonPrivateAttorney.
    /// </summary>
    [JsonPropertyName("personPrivateAttorney")]
    public PersonPrivateAttorney PersonPrivateAttorney
    {
      get => personPrivateAttorney ??= new();
      set => personPrivateAttorney = value;
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
    /// A value of Case1.
    /// </summary>
    [JsonPropertyName("case1")]
    public Case1 Case1
    {
      get => case1 ??= new();
      set => case1 = value;
    }

    private PersonPrivateAttorney personPrivateAttorney;
    private CsePerson csePerson;
    private Case1 case1;
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
    /// A value of ExistingPrivateAttorneyAddress.
    /// </summary>
    [JsonPropertyName("existingPrivateAttorneyAddress")]
    public PrivateAttorneyAddress ExistingPrivateAttorneyAddress
    {
      get => existingPrivateAttorneyAddress ??= new();
      set => existingPrivateAttorneyAddress = value;
    }

    /// <summary>
    /// A value of ExistingPersonPrivateAttorney.
    /// </summary>
    [JsonPropertyName("existingPersonPrivateAttorney")]
    public PersonPrivateAttorney ExistingPersonPrivateAttorney
    {
      get => existingPersonPrivateAttorney ??= new();
      set => existingPersonPrivateAttorney = value;
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
    /// A value of ExistingCase.
    /// </summary>
    [JsonPropertyName("existingCase")]
    public Case1 ExistingCase
    {
      get => existingCase ??= new();
      set => existingCase = value;
    }

    private PrivateAttorneyAddress existingPrivateAttorneyAddress;
    private PersonPrivateAttorney existingPersonPrivateAttorney;
    private CsePerson existingCsePerson;
    private Case1 existingCase;
  }
#endregion
}
