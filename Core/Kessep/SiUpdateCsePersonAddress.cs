// Program: SI_UPDATE_CSE_PERSON_ADDRESS, ID: 371735357, model: 746.
// Short name: SWE01247
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: SI_UPDATE_CSE_PERSON_ADDRESS.
/// </para>
/// <para>
/// RESP: SRVINIT
/// This PAD creates an address for a given CSE Person
/// </para>
/// </summary>
[Serializable]
public partial class SiUpdateCsePersonAddress: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_UPDATE_CSE_PERSON_ADDRESS program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiUpdateCsePersonAddress(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiUpdateCsePersonAddress.
  /// </summary>
  public SiUpdateCsePersonAddress(IContext context, Import import, Export export)
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
    // ------------------------------------------------------------
    //                 M A I N T E N A N C E   L O G
    // Date	  Developer	Description
    // 09/14/95  Ken Evans	Initial Development
    // 09/24/96  G Lofton	Add county
    // ------------------------------------------------------------
    // 12/09/98 W.Campbell     Removed set statements
    //                         from the UPDATE stmt for
    //                         CSE_PERSON_ADDRESS
    //                         for both VERIFIED_CODE
    //                         and START_DATE.  This
    //                         was done on IDCR 454.
    // -------------------------------------------------------
    // 12/15/98 W.Campbell     Added an export view for
    //                         CSE_PERSON_ADDRESS and
    //                         logic to populate it on a
    //                         successful UPDATE.
    //                         This was done on IDCR454.
    // ---------------------------------------------
    // 07/12/99  Marek Lachowicz	Change property of READ (Select Only)
    // ---------------------------------------------
    // This PAD updates an address for a CSE Person.
    // ---------------------------------------------
    local.Current.Timestamp = Now();

    if (Equal(import.CsePersonAddress.EndDate, null))
    {
      local.CsePersonAddress.EndDate = UseCabSetMaximumDiscontinueDate();
    }
    else
    {
      local.CsePersonAddress.EndDate = import.CsePersonAddress.EndDate;
    }

    // 07/12/99  M.L 	Change property of READ (Select Only)
    if (ReadCsePersonAddress())
    {
      // ---------------------------------------------
      // 12/09/98 W.Campbell -  Removed set statements
      // from the UPDATE stmt for CSE_PERSON_ADDRESS
      // for both VERIFIED_CODE and START_DATE.  This
      // was done on IDCR 454.
      // ---------------------------------------------
      try
      {
        UpdateCsePersonAddress();

        // ---------------------------------------------
        // 12/15/98 W.Campbell -  Added an export view for
        // CSE_PERSON_ADDRESS and logic to populate
        // it on a successful UPDATE.  This was done
        // on IDCR 454.
        // ---------------------------------------------
        MoveCsePersonAddress(entities.CsePersonAddress, export.CsePersonAddress);
          
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "CSE_PERSON_ADDRESS_NU";

            break;
          case ErrorCode.PermittedValueViolation:
            ExitState = "CSE_PERSON_ADDRESS_PV";

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
      ExitState = "CSE_PERSON_ADDRESS_NF";
    }
  }

  private static void MoveCsePersonAddress(CsePersonAddress source,
    CsePersonAddress target)
  {
    target.LastUpdatedTimestamp = source.LastUpdatedTimestamp;
    target.LastUpdatedBy = source.LastUpdatedBy;
  }

  private DateTime? UseCabSetMaximumDiscontinueDate()
  {
    var useImport = new CabSetMaximumDiscontinueDate.Import();
    var useExport = new CabSetMaximumDiscontinueDate.Export();

    Call(CabSetMaximumDiscontinueDate.Execute, useImport, useExport);

    return useExport.DateWorkArea.Date;
  }

  private bool ReadCsePersonAddress()
  {
    entities.CsePersonAddress.Populated = false;

    return Read("ReadCsePersonAddress",
      (db, command) =>
      {
        db.SetDateTime(
          command, "identifier",
          import.CsePersonAddress.Identifier.GetValueOrDefault());
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
        entities.CsePersonAddress.State = db.GetNullableString(reader, 14);
        entities.CsePersonAddress.ZipCode = db.GetNullableString(reader, 15);
        entities.CsePersonAddress.Zip4 = db.GetNullableString(reader, 16);
        entities.CsePersonAddress.Zip3 = db.GetNullableString(reader, 17);
        entities.CsePersonAddress.Street3 = db.GetNullableString(reader, 18);
        entities.CsePersonAddress.Street4 = db.GetNullableString(reader, 19);
        entities.CsePersonAddress.Province = db.GetNullableString(reader, 20);
        entities.CsePersonAddress.PostalCode = db.GetNullableString(reader, 21);
        entities.CsePersonAddress.Country = db.GetNullableString(reader, 22);
        entities.CsePersonAddress.LocationType = db.GetString(reader, 23);
        entities.CsePersonAddress.County = db.GetNullableString(reader, 24);
        entities.CsePersonAddress.Populated = true;
        CheckValid<CsePersonAddress>("LocationType",
          entities.CsePersonAddress.LocationType);
      });
  }

  private void UpdateCsePersonAddress()
  {
    System.Diagnostics.Debug.Assert(entities.CsePersonAddress.Populated);

    var sendDate = import.CsePersonAddress.SendDate;
    var source = import.CsePersonAddress.Source ?? "";
    var street1 = import.CsePersonAddress.Street1 ?? "";
    var street2 = import.CsePersonAddress.Street2 ?? "";
    var city = import.CsePersonAddress.City ?? "";
    var type1 = import.CsePersonAddress.Type1 ?? "";
    var workerId = import.CsePersonAddress.WorkerId ?? "";
    var verifiedDate = import.CsePersonAddress.VerifiedDate;
    var endDate = local.CsePersonAddress.EndDate;
    var endCode = import.CsePersonAddress.EndCode ?? "";
    var lastUpdatedTimestamp = local.Current.Timestamp;
    var lastUpdatedBy = global.UserId;
    var state = import.CsePersonAddress.State ?? "";
    var zipCode = import.CsePersonAddress.ZipCode ?? "";
    var zip4 = import.CsePersonAddress.Zip4 ?? "";
    var zip3 = import.CsePersonAddress.Zip3 ?? "";
    var street3 = import.CsePersonAddress.Street3 ?? "";
    var street4 = import.CsePersonAddress.Street4 ?? "";
    var province = import.CsePersonAddress.Province ?? "";
    var postalCode = import.CsePersonAddress.PostalCode ?? "";
    var country = import.CsePersonAddress.Country ?? "";
    var locationType = import.CsePersonAddress.LocationType;
    var county = import.CsePersonAddress.County ?? "";

    CheckValid<CsePersonAddress>("LocationType", locationType);
    entities.CsePersonAddress.Populated = false;
    Update("UpdateCsePersonAddress",
      (db, command) =>
      {
        db.SetNullableDate(command, "sendDate", sendDate);
        db.SetNullableString(command, "source", source);
        db.SetNullableString(command, "street1", street1);
        db.SetNullableString(command, "street2", street2);
        db.SetNullableString(command, "city", city);
        db.SetNullableString(command, "type", type1);
        db.SetNullableString(command, "workerId", workerId);
        db.SetNullableDate(command, "verifiedDate", verifiedDate);
        db.SetNullableDate(command, "endDate", endDate);
        db.SetNullableString(command, "endCode", endCode);
        db.
          SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTimestamp);
          
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetNullableString(command, "state", state);
        db.SetNullableString(command, "zipCode", zipCode);
        db.SetNullableString(command, "zip4", zip4);
        db.SetNullableString(command, "zip3", zip3);
        db.SetNullableString(command, "street3", street3);
        db.SetNullableString(command, "street4", street4);
        db.SetNullableString(command, "province", province);
        db.SetNullableString(command, "postalCode", postalCode);
        db.SetNullableString(command, "country", country);
        db.SetString(command, "locationType", locationType);
        db.SetNullableString(command, "county", county);
        db.SetDateTime(
          command, "identifier",
          entities.CsePersonAddress.Identifier.GetValueOrDefault());
        db.SetString(command, "cspNumber", entities.CsePersonAddress.CspNumber);
      });

    entities.CsePersonAddress.SendDate = sendDate;
    entities.CsePersonAddress.Source = source;
    entities.CsePersonAddress.Street1 = street1;
    entities.CsePersonAddress.Street2 = street2;
    entities.CsePersonAddress.City = city;
    entities.CsePersonAddress.Type1 = type1;
    entities.CsePersonAddress.WorkerId = workerId;
    entities.CsePersonAddress.VerifiedDate = verifiedDate;
    entities.CsePersonAddress.EndDate = endDate;
    entities.CsePersonAddress.EndCode = endCode;
    entities.CsePersonAddress.LastUpdatedTimestamp = lastUpdatedTimestamp;
    entities.CsePersonAddress.LastUpdatedBy = lastUpdatedBy;
    entities.CsePersonAddress.State = state;
    entities.CsePersonAddress.ZipCode = zipCode;
    entities.CsePersonAddress.Zip4 = zip4;
    entities.CsePersonAddress.Zip3 = zip3;
    entities.CsePersonAddress.Street3 = street3;
    entities.CsePersonAddress.Street4 = street4;
    entities.CsePersonAddress.Province = province;
    entities.CsePersonAddress.PostalCode = postalCode;
    entities.CsePersonAddress.Country = country;
    entities.CsePersonAddress.LocationType = locationType;
    entities.CsePersonAddress.County = county;
    entities.CsePersonAddress.Populated = true;
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
    /// A value of Current.
    /// </summary>
    [JsonPropertyName("current")]
    public DateWorkArea Current
    {
      get => current ??= new();
      set => current = value;
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

    private DateWorkArea current;
    private CsePersonAddress csePersonAddress;
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
