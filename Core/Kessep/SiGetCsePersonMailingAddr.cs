// Program: SI_GET_CSE_PERSON_MAILING_ADDR, ID: 371943134, model: 746.
// Short name: SWE02295
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: SI_GET_CSE_PERSON_MAILING_ADDR.
/// </para>
/// <para>
/// This CAB retrieves CSE Person Address.
/// </para>
/// </summary>
[Serializable]
public partial class SiGetCsePersonMailingAddr: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_GET_CSE_PERSON_MAILING_ADDR program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiGetCsePersonMailingAddr(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiGetCsePersonMailingAddr.
  /// </summary>
  public SiGetCsePersonMailingAddr(IContext context, Import import,
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
    // ------------------------------------------------------------
    //             M A I N T E N A N C E   L O G
    //   Date	 Developer   Request# Description
    // 01/14/99 W.Campbell  IDCR454  Initial development.
    // ------------------------------------------------------------
    // 01/20/99 W.Campbell  IDCR454  Added additional
    //                      qualification to all of the READ EACH
    //                      statements to get the algorithm right.
    // -----------------------------------------------------------
    // ---------------------------------------------
    // This CAB retrieves CSE Person Address based
    // on a 'consensus' interpretation of the
    // following algorithm. This algorithm was
    // provided in IDCR 454:
    // 1. Use most recent Verified Mailing address first.
    // 2. If no most recent verified mailing, use most
    //    recent verified Residential address
    // 3. If no most recent Verified Residential address
    //    use most recent updated(per Last Updated field)
    //    Mailing address.
    // 4. If no most recent updated(per Last Updated field)
    //    Mailing address use most recent updated
    //    (per Last Updated field) Residential address.
    // 5. If no most recent updated(per Last Updated field)
    //    Residential address use most recent ended
    //    Mailing address.
    // 6. If no most recent ended Mailing address use
    //    most recent ended Residential address.
    // ---------------------------------------------
    local.Current.Date = Now().Date;

    // ---------------------------------------------
    // The following READ EACH should get the most recently
    // verified, not end dated, 'M'ailing address or
    // 'R'esidential address for the import CSE_PERSON
    // number (if one exist).  It should get the 'M'ailing
    // first (if it exist) or the 'R'esidential (if it exist)
    // because of the SORT ASCENDING on address type.
    // 'M'ailing and 'R'esidential should be the only two
    // valid address types.  It can be location_type =
    // 'D'omestic or 'F'oreign and these should be the
    // only two valid location_type(s) (Subtypes).
    // ---------------------------------------------
    if (ReadCsePersonAddress3())
    {
      export.CsePersonAddress.Assign(entities.CsePersonAddress);

      // ---------------------------------------------
      // An address was found and placed in the
      // export view.  Work is done, return to the
      // using CAB.
      // ---------------------------------------------
      return;
    }

    // ---------------------------------------------
    // The following READ EACH should get the most recently
    // updated, not end dated, 'M'ailing address or
    // 'R'esidential address for the import CSE_PERSON
    // number (if one exist).  It should get the 'M'ailing
    // first (if it exist) or the 'R'esidential (if it exist)
    // because of the SORT ASCENDING on address type.
    // 'M'ailing and 'R'esidential should be the only two
    // valid address types.  It can be location_type =
    // 'D'omestic or 'F'oreign and these should be the
    // only two valid location_type(s) (Subtypes).
    // ---------------------------------------------
    if (ReadCsePersonAddress1())
    {
      export.CsePersonAddress.Assign(entities.CsePersonAddress);

      // ---------------------------------------------
      // An address was found and placed in the
      // export view.  Work is done, return to the
      // using CAB.
      // ---------------------------------------------
      return;
    }

    // ---------------------------------------------
    // The following READ EACH should get the most
    // recently end dated, 'M'ailing address or
    // 'R'esidential address for the import CSE_PERSON
    // number (if one exist).  It should get the 'M'ailing
    // first (if it exist) or the 'R'esidential (if it exist)
    // because of the SORT ASCENDING on address type.
    // 'M'ailing and 'R'esidential should be the only two
    // valid address types.  It can be location_type =
    // 'D'omestic or 'F'oreign and these should be the
    // only two valid location_type(s) (Subtypes).
    // ---------------------------------------------
    if (ReadCsePersonAddress2())
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

  private bool ReadCsePersonAddress1()
  {
    entities.CsePersonAddress.Populated = false;

    return Read("ReadCsePersonAddress1",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", import.CsePerson.Number);
        db.SetNullableDateTime(
          command, "lastUpdatedTmst",
          local.Default1.Timestamp.GetValueOrDefault());
        db.SetNullableDate(
          command, "endDate", local.Current.Date.GetValueOrDefault());
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
        CheckValid<CsePersonAddress>("LocationType",
          entities.CsePersonAddress.LocationType);
      });
  }

  private bool ReadCsePersonAddress2()
  {
    entities.CsePersonAddress.Populated = false;

    return Read("ReadCsePersonAddress2",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", import.CsePerson.Number);
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
        CheckValid<CsePersonAddress>("LocationType",
          entities.CsePersonAddress.LocationType);
      });
  }

  private bool ReadCsePersonAddress3()
  {
    entities.CsePersonAddress.Populated = false;

    return Read("ReadCsePersonAddress3",
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
        CheckValid<CsePersonAddress>("LocationType",
          entities.CsePersonAddress.LocationType);
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
