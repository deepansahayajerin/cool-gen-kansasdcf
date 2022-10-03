// Program: FN_CAB_READ_CSE_PERSON_ADDRESS, ID: 372164408, model: 746.
// Short name: SWE02209
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_CAB_READ_CSE_PERSON_ADDRESS.
/// </summary>
[Serializable]
public partial class FnCabReadCsePersonAddress: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_CAB_READ_CSE_PERSON_ADDRESS program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnCabReadCsePersonAddress(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnCabReadCsePersonAddress.
  /// </summary>
  public FnCabReadCsePersonAddress(IContext context, Import import,
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
    // =================================================
    // 1/30/99 - B  Adams  -  replaced the fn-get-active-cse-person-address
    //   with the si version.
    // =================================================
    export.AddressFound.Flag = "N";
    UseSiGetCsePersonMailingAddr();

    if (Equal(local.CsePersonAddress.Identifier, local.Initialized.Timestamp))
    {
      local.ActiveAddressFound.Flag = "N";
    }
    else
    {
      local.ActiveAddressFound.Flag = "Y";
    }

    switch(AsChar(local.ActiveAddressFound.Flag))
    {
      case 'Y':
        // =================================================
        // 1/26/99 - b adams  -  If an address has been returned that
        //   has already expired, then do not display it.
        // =================================================
        if (Lt(local.CsePersonAddress.EndDate, import.AsOfDate.Date))
        {
          export.AddressFound.Flag = "N";
        }
        else
        {
          export.CsePersonAddress.Assign(local.CsePersonAddress);
          export.AddressFound.Flag = "Y";
        }

        break;
      case 'N':
        if (ReadFipsTribAddress())
        {
          export.CsePersonAddress.Street1 = entities.FipsTribAddress.Street1;
          export.CsePersonAddress.Street2 = entities.FipsTribAddress.Street2;
          export.CsePersonAddress.City = entities.FipsTribAddress.City;
          export.CsePersonAddress.State = entities.FipsTribAddress.State;
          export.CsePersonAddress.ZipCode = entities.FipsTribAddress.ZipCode;
          export.CsePersonAddress.Zip4 = entities.FipsTribAddress.Zip4;
          export.CsePersonAddress.Zip3 = entities.FipsTribAddress.Zip3;
          export.CsePersonAddress.County = entities.FipsTribAddress.County;
          export.CsePersonAddress.Street3 = entities.FipsTribAddress.Street3;
          export.CsePersonAddress.Street4 = entities.FipsTribAddress.Street4;
          export.CsePersonAddress.Province = entities.FipsTribAddress.Province;
          export.CsePersonAddress.PostalCode =
            entities.FipsTribAddress.PostalCode;
          export.CsePersonAddress.Country = entities.FipsTribAddress.Country;
          export.AddressFound.Flag = "T";
        }

        break;
      default:
        break;
    }
  }

  private void UseSiGetCsePersonMailingAddr()
  {
    var useImport = new SiGetCsePersonMailingAddr.Import();
    var useExport = new SiGetCsePersonMailingAddr.Export();

    useImport.CsePerson.Number = import.CsePerson.Number;

    Call(SiGetCsePersonMailingAddr.Execute, useImport, useExport);

    local.CsePersonAddress.Assign(useExport.CsePersonAddress);
  }

  private bool ReadFipsTribAddress()
  {
    entities.FipsTribAddress.Populated = false;

    return Read("ReadFipsTribAddress",
      (db, command) =>
      {
        db.SetString(command, "number1", import.CsePerson.Number);
        db.SetString(command, "number2", import.CsePersonsWorkSet.Number);
      },
      (db, reader) =>
      {
        entities.FipsTribAddress.Identifier = db.GetInt32(reader, 0);
        entities.FipsTribAddress.Street1 = db.GetString(reader, 1);
        entities.FipsTribAddress.Street2 = db.GetNullableString(reader, 2);
        entities.FipsTribAddress.City = db.GetString(reader, 3);
        entities.FipsTribAddress.State = db.GetString(reader, 4);
        entities.FipsTribAddress.ZipCode = db.GetString(reader, 5);
        entities.FipsTribAddress.Zip4 = db.GetNullableString(reader, 6);
        entities.FipsTribAddress.Zip3 = db.GetNullableString(reader, 7);
        entities.FipsTribAddress.County = db.GetNullableString(reader, 8);
        entities.FipsTribAddress.Street3 = db.GetNullableString(reader, 9);
        entities.FipsTribAddress.Street4 = db.GetNullableString(reader, 10);
        entities.FipsTribAddress.Province = db.GetNullableString(reader, 11);
        entities.FipsTribAddress.PostalCode = db.GetNullableString(reader, 12);
        entities.FipsTribAddress.Country = db.GetNullableString(reader, 13);
        entities.FipsTribAddress.FipState = db.GetNullableInt32(reader, 14);
        entities.FipsTribAddress.FipCounty = db.GetNullableInt32(reader, 15);
        entities.FipsTribAddress.FipLocation = db.GetNullableInt32(reader, 16);
        entities.FipsTribAddress.Populated = true;
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
    /// A value of CsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("csePersonsWorkSet")]
    public CsePersonsWorkSet CsePersonsWorkSet
    {
      get => csePersonsWorkSet ??= new();
      set => csePersonsWorkSet = value;
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
    /// A value of AsOfDate.
    /// </summary>
    [JsonPropertyName("asOfDate")]
    public DateWorkArea AsOfDate
    {
      get => asOfDate ??= new();
      set => asOfDate = value;
    }

    private CsePersonsWorkSet csePersonsWorkSet;
    private CsePerson csePerson;
    private DateWorkArea asOfDate;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of AddressFound.
    /// </summary>
    [JsonPropertyName("addressFound")]
    public Common AddressFound
    {
      get => addressFound ??= new();
      set => addressFound = value;
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

    private Common addressFound;
    private CsePersonAddress csePersonAddress;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
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

    /// <summary>
    /// A value of ActiveAddressFound.
    /// </summary>
    [JsonPropertyName("activeAddressFound")]
    public Common ActiveAddressFound
    {
      get => activeAddressFound ??= new();
      set => activeAddressFound = value;
    }

    /// <summary>
    /// A value of Initialized.
    /// </summary>
    [JsonPropertyName("initialized")]
    public DateWorkArea Initialized
    {
      get => initialized ??= new();
      set => initialized = value;
    }

    private CsePersonAddress csePersonAddress;
    private Common activeAddressFound;
    private DateWorkArea initialized;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of FipsTribAddress.
    /// </summary>
    [JsonPropertyName("fipsTribAddress")]
    public FipsTribAddress FipsTribAddress
    {
      get => fipsTribAddress ??= new();
      set => fipsTribAddress = value;
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
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
    }

    private FipsTribAddress fipsTribAddress;
    private Fips fips;
    private CsePerson csePerson;
  }
#endregion
}
