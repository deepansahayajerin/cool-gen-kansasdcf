// Program: SI_CREATE_CSE_PERSON_ADDRESS, ID: 371727805, model: 746.
// Short name: SWE01124
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: SI_CREATE_CSE_PERSON_ADDRESS.
/// </para>
/// <para>
/// RESP: SRVINIT
/// This PAD creates an address for a given CSE Person
/// </para>
/// </summary>
[Serializable]
public partial class SiCreateCsePersonAddress: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_CREATE_CSE_PERSON_ADDRESS program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiCreateCsePersonAddress(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiCreateCsePersonAddress.
  /// </summary>
  public SiCreateCsePersonAddress(IContext context, Import import, Export export)
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
    //   Date	  Developer	  Description
    // 02/26/95  Helen Sharland  Initial Development
    // 09/24/96  G Lofton	  Add county
    // ------------------------------------------------------------
    // 12/09/98  W. Campbell     Removed set statements
    //                           for setting START_DATE and
    //                           VERIFIED_CODE from the
    //                           CREATE statement.  IDCR454.
    // ---------------------------------------------
    // 12/15/98  W. Campbell     Added
    //                           LAST_UPDATED_TIMESTAMP to the
    //                           export view. Work done on IDCR454.
    // ---------------------------------------------
    // 12/18/98 W. Campbell      Inserted set statements
    //                           to set END_DATE in the export view.
    //                           Also, replaced use of the
    //                           CURRENT_TIMESTAMP function with
    //                           a local view set to same.
    //                           Work done on IDCR454.
    // ---------------------------------------------
    // 06/22/99  M. Lachowicz    Change property of READ
    //                           (Select Only or Cursor Only)
    // ------------------------------------------------------------
    MoveCsePersonAddress(import.CsePersonAddress, export.CsePersonAddress);

    // ---------------------------------------------
    // 12/18/98 W. Campbell - Replaced use of the
    // CURRENT_TIMESTAMP function with
    // a local view set to same.  Work done on IDCR454.
    // ---------------------------------------------
    local.Current.Timestamp = Now();

    if (Equal(import.CsePersonAddress.EndDate, null))
    {
      local.CsePersonAddress.EndDate = UseCabSetMaximumDiscontinueDate1();
    }
    else
    {
      local.CsePersonAddress.EndDate = import.CsePersonAddress.EndDate;
    }

    // ---------------------------------------------
    // This PAD creates an address for a CSE Person.
    // ---------------------------------------------
    // 06/22/99 M.L
    //              Change property of the following READ to generate
    //              SELECT ONLY
    if (!ReadCsePerson())
    {
      ExitState = "CSE_PERSON_NF";

      return;
    }

    // ---------------------------------------------
    // 12/09/98 W. Campbell - Removed set statements
    // for setting START_DATE and VERIFIED_CODE from
    // the following CREATE statement.
    // Work done on IDCR454.
    // ---------------------------------------------
    try
    {
      CreateCsePersonAddress();
      export.CsePersonAddress.Assign(entities.CsePersonAddress);

      // ---------------------------------------------
      // 12/18/98 W. Campbell - inserted set statements
      // to set END_DATE in export view.
      // Work done on IDCR454.
      // ---------------------------------------------
      local.EndDate.Date = export.CsePersonAddress.EndDate;
      export.CsePersonAddress.EndDate = UseCabSetMaximumDiscontinueDate2();
    }
    catch(Exception e)
    {
      switch(GetErrorCode(e))
      {
        case ErrorCode.AlreadyExists:
          ExitState = "CSE_PERSON_ADDRESS_AE";

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

  private static void MoveCsePersonAddress(CsePersonAddress source,
    CsePersonAddress target)
  {
    target.LocationType = source.LocationType;
    target.ZdelStartDate = source.ZdelStartDate;
    target.SendDate = source.SendDate;
    target.Source = source.Source;
    target.Identifier = source.Identifier;
    target.Street1 = source.Street1;
    target.Street2 = source.Street2;
    target.City = source.City;
    target.Type1 = source.Type1;
    target.WorkerId = source.WorkerId;
    target.VerifiedDate = source.VerifiedDate;
    target.EndDate = source.EndDate;
    target.EndCode = source.EndCode;
    target.ZdelVerifiedCode = source.ZdelVerifiedCode;
    target.County = source.County;
    target.State = source.State;
    target.ZipCode = source.ZipCode;
    target.Zip4 = source.Zip4;
    target.Zip3 = source.Zip3;
    target.Street3 = source.Street3;
    target.Street4 = source.Street4;
    target.Province = source.Province;
    target.PostalCode = source.PostalCode;
    target.Country = source.Country;
  }

  private DateTime? UseCabSetMaximumDiscontinueDate1()
  {
    var useImport = new CabSetMaximumDiscontinueDate.Import();
    var useExport = new CabSetMaximumDiscontinueDate.Export();

    Call(CabSetMaximumDiscontinueDate.Execute, useImport, useExport);

    return useExport.DateWorkArea.Date;
  }

  private DateTime? UseCabSetMaximumDiscontinueDate2()
  {
    var useImport = new CabSetMaximumDiscontinueDate.Import();
    var useExport = new CabSetMaximumDiscontinueDate.Export();

    useImport.DateWorkArea.Date = local.EndDate.Date;

    Call(CabSetMaximumDiscontinueDate.Execute, useImport, useExport);

    return useExport.DateWorkArea.Date;
  }

  private void CreateCsePersonAddress()
  {
    var identifier = local.Current.Timestamp;
    var cspNumber = entities.CsePerson.Number;
    var sendDate = import.CsePersonAddress.SendDate;
    var source = import.CsePersonAddress.Source ?? "";
    var street1 = import.CsePersonAddress.Street1 ?? "";
    var street2 = import.CsePersonAddress.Street2 ?? "";
    var city = import.CsePersonAddress.City ?? "";
    var type1 = import.CsePersonAddress.Type1 ?? "";
    var workerId = global.UserId;
    var verifiedDate = import.CsePersonAddress.VerifiedDate;
    var endDate = local.CsePersonAddress.EndDate;
    var endCode = import.CsePersonAddress.EndCode ?? "";
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
    Update("CreateCsePersonAddress",
      (db, command) =>
      {
        db.SetDateTime(command, "identifier", identifier);
        db.SetString(command, "cspNumber", cspNumber);
        db.SetNullableDate(command, "zdelStartDate", null);
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
        db.SetNullableString(command, "zdelVerifiedCode", "");
        db.SetNullableDateTime(command, "lastUpdatedTmst", identifier);
        db.SetNullableString(command, "lastUpdatedBy", workerId);
        db.SetNullableDateTime(command, "createdTimestamp", identifier);
        db.SetNullableString(command, "createdBy", workerId);
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
      });

    entities.CsePersonAddress.Identifier = identifier;
    entities.CsePersonAddress.CspNumber = cspNumber;
    entities.CsePersonAddress.ZdelStartDate = null;
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
    entities.CsePersonAddress.ZdelVerifiedCode = "";
    entities.CsePersonAddress.LastUpdatedTimestamp = identifier;
    entities.CsePersonAddress.LastUpdatedBy = workerId;
    entities.CsePersonAddress.CreatedTimestamp = identifier;
    entities.CsePersonAddress.CreatedBy = workerId;
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

  private bool ReadCsePerson()
  {
    entities.CsePerson.Populated = false;

    return Read("ReadCsePerson",
      (db, command) =>
      {
        db.SetString(command, "numb", import.CsePerson.Number);
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CsePerson.Populated = true;
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
    /// A value of EndDate.
    /// </summary>
    [JsonPropertyName("endDate")]
    public DateWorkArea EndDate
    {
      get => endDate ??= new();
      set => endDate = value;
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

    /// <summary>
    /// A value of CsePersonAddress.
    /// </summary>
    [JsonPropertyName("csePersonAddress")]
    public CsePersonAddress CsePersonAddress
    {
      get => csePersonAddress ??= new();
      set => csePersonAddress = value;
    }

    private DateWorkArea endDate;
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
