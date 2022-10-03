// Program: CAB_GET_NOT_ENDED_ADDRESS, ID: 371210350, model: 746.
// Short name: SWE02210
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: CAB_GET_NOT_ENDED_ADDRESS.
/// </para>
/// <para>
/// This CAB retrieves CSE Person Address - verified and not end-dated.
/// </para>
/// </summary>
[Serializable]
public partial class CabGetNotEndedAddress: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the CAB_GET_NOT_ENDED_ADDRESS program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new CabGetNotEndedAddress(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of CabGetNotEndedAddress.
  /// </summary>
  public CabGetNotEndedAddress(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ------------------------------------------------------------
    //             M A I N T E N A N C E   L O G
    //   Date	 Developer   Request# Description
    // 01/30/04 JeHoward  PR# 199221 Initial development.
    // ------------------------------------------------------------------------------------------
    // This is a copy of 
    // SI_GET_CSE_PERSON_MAILING_ADDRESS
    // using only the first Read Each
    // 
    // This cab will only return verified
    // addresses with no end date (or
    // blanks).
    // ------------------------------------------------------------------------------------------
    local.Current.Date = Now().Date;

    // ---------------------------------------------
    // The following READ EACH should get the most recently
    // verified, not end dated, 'M'ailing address or
    // 'R'esidential address for the import CSE_PERSON
    // number (if one exists).  It should get the 'M'ailing
    // first (if it exists) or the 'R'esidential (if it exists)
    // because of the SORT ASCENDING on address type.
    // 'M'ailing and 'R'esidential should be the only two
    // valid address types.  It can be location_type =
    // 'D'omestic or 'F'oreign and these should be the
    // only two valid location_type(s) (Subtypes).
    // ---------------------------------------------
    if (ReadCsePersonAddress())
    {
      export.CsePersonAddress.Assign(entities.CsePersonAddress);

      // ---------------------------------------------
      // An address was found and placed in the
      // export view.  Work is done, return to the
      // using CAB.
      // ---------------------------------------------
    }

    // ---------------------------------------------
    // No address was found, simply return
    // an empty export view.
    // ---------------------------------------------
  }

  private bool ReadCsePersonAddress()
  {
    entities.CsePersonAddress.Populated = false;

    return Read("ReadCsePersonAddress",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", import.CsePerson.Number);
        db.SetNullableDate(
          command, "verifiedDate1", local.Default1.Date.GetValueOrDefault());
        db.SetNullableDate(
          command, "verifiedDate2", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CsePersonAddress.Identifier = db.GetDateTime(reader, 0);
        entities.CsePersonAddress.CspNumber = db.GetString(reader, 1);
        entities.CsePersonAddress.SendDate = db.GetNullableDate(reader, 2);
        entities.CsePersonAddress.Source = db.GetNullableString(reader, 3);
        entities.CsePersonAddress.Street1 = db.GetNullableString(reader, 4);
        entities.CsePersonAddress.Street2 = db.GetNullableString(reader, 5);
        entities.CsePersonAddress.City = db.GetNullableString(reader, 6);
        entities.CsePersonAddress.Type1 = db.GetNullableString(reader, 7);
        entities.CsePersonAddress.WorkerId = db.GetNullableString(reader, 8);
        entities.CsePersonAddress.VerifiedDate = db.GetNullableDate(reader, 9);
        entities.CsePersonAddress.EndDate = db.GetNullableDate(reader, 10);
        entities.CsePersonAddress.EndCode = db.GetNullableString(reader, 11);
        entities.CsePersonAddress.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 12);
        entities.CsePersonAddress.LastUpdatedBy =
          db.GetNullableString(reader, 13);
        entities.CsePersonAddress.CreatedTimestamp =
          db.GetNullableDateTime(reader, 14);
        entities.CsePersonAddress.CreatedBy = db.GetNullableString(reader, 15);
        entities.CsePersonAddress.State = db.GetNullableString(reader, 16);
        entities.CsePersonAddress.ZipCode = db.GetNullableString(reader, 17);
        entities.CsePersonAddress.Zip4 = db.GetNullableString(reader, 18);
        entities.CsePersonAddress.Zip3 = db.GetNullableString(reader, 19);
        entities.CsePersonAddress.Street3 = db.GetNullableString(reader, 20);
        entities.CsePersonAddress.Street4 = db.GetNullableString(reader, 21);
        entities.CsePersonAddress.Province = db.GetNullableString(reader, 22);
        entities.CsePersonAddress.PostalCode = db.GetNullableString(reader, 23);
        entities.CsePersonAddress.Country = db.GetNullableString(reader, 24);
        entities.CsePersonAddress.LocationType = db.GetString(reader, 25);
        entities.CsePersonAddress.County = db.GetNullableString(reader, 26);
        entities.CsePersonAddress.Populated = true;
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
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
    }

    private CsePerson csePerson;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of CsePersonAddress.
    /// </summary>
    [JsonPropertyName("csePersonAddress")]
    public CsePersonAddress CsePersonAddress
    {
      get => csePersonAddress ??= new();
      set => csePersonAddress = value;
    }

    private CsePersonAddress csePersonAddress;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of Default1.
    /// </summary>
    [JsonPropertyName("default1")]
    public DateWorkArea Default1
    {
      get => default1 ??= new();
      set => default1 = value;
    }

    /// <summary>
    /// A value of Current.
    /// </summary>
    [JsonPropertyName("current")]
    public DateWorkArea Current
    {
      get => current ??= new();
      set => current = value;
    }

    private DateWorkArea default1;
    private DateWorkArea current;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
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
    /// A value of CsePersonAddress.
    /// </summary>
    [JsonPropertyName("csePersonAddress")]
    public CsePersonAddress CsePersonAddress
    {
      get => csePersonAddress ??= new();
      set => csePersonAddress = value;
    }

    private CsePerson csePerson;
    private CsePersonAddress csePersonAddress;
  }
#endregion
}
