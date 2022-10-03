// Program: CAB_READ_ADMIN_APPEAL_ADDRESS, ID: 372576544, model: 746.
// Short name: SWE00074
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
/// A program: CAB_READ_ADMIN_APPEAL_ADDRESS.
/// </para>
/// <para>
/// RESP: LGLENFAC
/// This action block reads all Administrative Appeal Addresses for a particular
/// Administrative Appeal.
/// </para>
/// </summary>
[Serializable]
public partial class CabReadAdminAppealAddress: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the CAB_READ_ADMIN_APPEAL_ADDRESS program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new CabReadAdminAppealAddress(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of CabReadAdminAppealAddress.
  /// </summary>
  public CabReadAdminAppealAddress(IContext context, Import import,
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
    // ---------------------------------------------
    // Date	By	IDCR#	Description
    // ??????	??????		Initial code
    // 102497	govind		Modified for Identifier being changed to a random number
    // 03/24/98	Siraj Konkader		ZDEL cleanup
    // --------------------------------------------
    MoveAdministrativeAppeal(import.AdministrativeAppeal,
      export.AdministrativeAppeal);
    export.CsePersonsWorkSet.Assign(import.CsePersonsWorkSet);

    if (import.AdministrativeAppeal.Identifier != 0)
    {
      if (ReadAdministrativeAppeal1())
      {
        export.AdministrativeAppeal.Assign(entities.AdministrativeAppeal);
      }
      else
      {
        ExitState = "LE0000_ADMINISTRATIVE_APPEAL_NF";

        return;
      }
    }
    else if (!IsEmpty(import.AdministrativeAppeal.Number))
    {
      if (ReadAdministrativeAppeal3())
      {
        export.AdministrativeAppeal.Assign(entities.AdministrativeAppeal);

        goto Test;
      }

      ExitState = "LE0000_ADMINISTRATIVE_APPEAL_NF";

      return;
    }
    else
    {
      if (ReadAdministrativeAppeal2())
      {
        export.AdministrativeAppeal.Assign(entities.AdministrativeAppeal);

        goto Test;
      }

      ExitState = "LE0000_ADMINISTRATIVE_APPEAL_NF";

      return;
    }

Test:

    if (ReadCsePerson())
    {
      local.CsePersonsWorkSet.Number = entities.CsePerson.Number;
      UseSiReadCsePerson();
    }
    else
    {
      ExitState = "CSE_PERSON_NF";

      return;
    }

    export.Export1.Index = 0;
    export.Export1.Clear();

    foreach(var item in ReadAdminAppealAppellantAddress())
    {
      export.Export1.Update.AdminAppealAppellantAddress.Assign(
        entities.AdminAppealAppellantAddress);
      export.Export1.Next();
    }
  }

  private static void MoveAdministrativeAppeal(AdministrativeAppeal source,
    AdministrativeAppeal target)
  {
    target.Identifier = source.Identifier;
    target.Number = source.Number;
  }

  private void UseSiReadCsePerson()
  {
    var useImport = new SiReadCsePerson.Import();
    var useExport = new SiReadCsePerson.Export();

    useImport.CsePersonsWorkSet.Number = local.CsePersonsWorkSet.Number;

    Call(SiReadCsePerson.Execute, useImport, useExport);

    export.CsePersonsWorkSet.Assign(useExport.CsePersonsWorkSet);
  }

  private IEnumerable<bool> ReadAdminAppealAppellantAddress()
  {
    return ReadEach("ReadAdminAppealAppellantAddress",
      (db, command) =>
      {
        db.SetInt32(
          command, "aapIdentifier", export.AdministrativeAppeal.Identifier);
      },
      (db, reader) =>
      {
        if (export.Export1.IsFull)
        {
          return false;
        }

        entities.AdminAppealAppellantAddress.AapIdentifier =
          db.GetInt32(reader, 0);
        entities.AdminAppealAppellantAddress.Type1 = db.GetString(reader, 1);
        entities.AdminAppealAppellantAddress.Street1 = db.GetString(reader, 2);
        entities.AdminAppealAppellantAddress.Street2 =
          db.GetNullableString(reader, 3);
        entities.AdminAppealAppellantAddress.City = db.GetString(reader, 4);
        entities.AdminAppealAppellantAddress.StateProvince =
          db.GetString(reader, 5);
        entities.AdminAppealAppellantAddress.Country =
          db.GetNullableString(reader, 6);
        entities.AdminAppealAppellantAddress.PostalCode =
          db.GetNullableString(reader, 7);
        entities.AdminAppealAppellantAddress.ZipCode = db.GetString(reader, 8);
        entities.AdminAppealAppellantAddress.Zip4 =
          db.GetNullableString(reader, 9);
        entities.AdminAppealAppellantAddress.Zip3 =
          db.GetNullableString(reader, 10);
        entities.AdminAppealAppellantAddress.Populated = true;

        return true;
      });
  }

  private bool ReadAdministrativeAppeal1()
  {
    entities.AdministrativeAppeal.Populated = false;

    return Read("ReadAdministrativeAppeal1",
      (db, command) =>
      {
        db.SetInt32(
          command, "adminAppealId", import.AdministrativeAppeal.Identifier);
      },
      (db, reader) =>
      {
        entities.AdministrativeAppeal.Identifier = db.GetInt32(reader, 0);
        entities.AdministrativeAppeal.Number = db.GetNullableString(reader, 1);
        entities.AdministrativeAppeal.Type1 = db.GetString(reader, 2);
        entities.AdministrativeAppeal.AppellantLastName =
          db.GetNullableString(reader, 3);
        entities.AdministrativeAppeal.AppellantFirstName =
          db.GetNullableString(reader, 4);
        entities.AdministrativeAppeal.AppellantMiddleInitial =
          db.GetNullableString(reader, 5);
        entities.AdministrativeAppeal.AppellantSuffix =
          db.GetNullableString(reader, 6);
        entities.AdministrativeAppeal.CreatedTstamp = db.GetDateTime(reader, 7);
        entities.AdministrativeAppeal.CspQNumber =
          db.GetNullableString(reader, 8);
        entities.AdministrativeAppeal.Populated = true;
      });
  }

  private bool ReadAdministrativeAppeal2()
  {
    entities.AdministrativeAppeal.Populated = false;

    return Read("ReadAdministrativeAppeal2",
      (db, command) =>
      {
        db.SetNullableString(
          command, "cspQNumber", import.CsePersonsWorkSet.Number);
      },
      (db, reader) =>
      {
        entities.AdministrativeAppeal.Identifier = db.GetInt32(reader, 0);
        entities.AdministrativeAppeal.Number = db.GetNullableString(reader, 1);
        entities.AdministrativeAppeal.Type1 = db.GetString(reader, 2);
        entities.AdministrativeAppeal.AppellantLastName =
          db.GetNullableString(reader, 3);
        entities.AdministrativeAppeal.AppellantFirstName =
          db.GetNullableString(reader, 4);
        entities.AdministrativeAppeal.AppellantMiddleInitial =
          db.GetNullableString(reader, 5);
        entities.AdministrativeAppeal.AppellantSuffix =
          db.GetNullableString(reader, 6);
        entities.AdministrativeAppeal.CreatedTstamp = db.GetDateTime(reader, 7);
        entities.AdministrativeAppeal.CspQNumber =
          db.GetNullableString(reader, 8);
        entities.AdministrativeAppeal.Populated = true;
      });
  }

  private bool ReadAdministrativeAppeal3()
  {
    entities.AdministrativeAppeal.Populated = false;

    return Read("ReadAdministrativeAppeal3",
      (db, command) =>
      {
        db.SetNullableString(
          command, "adminAppealNo", import.AdministrativeAppeal.Number ?? "");
      },
      (db, reader) =>
      {
        entities.AdministrativeAppeal.Identifier = db.GetInt32(reader, 0);
        entities.AdministrativeAppeal.Number = db.GetNullableString(reader, 1);
        entities.AdministrativeAppeal.Type1 = db.GetString(reader, 2);
        entities.AdministrativeAppeal.AppellantLastName =
          db.GetNullableString(reader, 3);
        entities.AdministrativeAppeal.AppellantFirstName =
          db.GetNullableString(reader, 4);
        entities.AdministrativeAppeal.AppellantMiddleInitial =
          db.GetNullableString(reader, 5);
        entities.AdministrativeAppeal.AppellantSuffix =
          db.GetNullableString(reader, 6);
        entities.AdministrativeAppeal.CreatedTstamp = db.GetDateTime(reader, 7);
        entities.AdministrativeAppeal.CspQNumber =
          db.GetNullableString(reader, 8);
        entities.AdministrativeAppeal.Populated = true;
      });
  }

  private bool ReadCsePerson()
  {
    System.Diagnostics.Debug.Assert(entities.AdministrativeAppeal.Populated);
    entities.CsePerson.Populated = false;

    return Read("ReadCsePerson",
      (db, command) =>
      {
        db.SetString(
          command, "numb", entities.AdministrativeAppeal.CspQNumber ?? "");
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CsePerson.Type1 = db.GetString(reader, 1);
        entities.CsePerson.Populated = true;
        CheckValid<CsePerson>("Type1", entities.CsePerson.Type1);
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
    /// A value of AdministrativeAppeal.
    /// </summary>
    [JsonPropertyName("administrativeAppeal")]
    public AdministrativeAppeal AdministrativeAppeal
    {
      get => administrativeAppeal ??= new();
      set => administrativeAppeal = value;
    }

    private CsePersonsWorkSet csePersonsWorkSet;
    private AdministrativeAppeal administrativeAppeal;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>A ExportGroup group.</summary>
    [Serializable]
    public class ExportGroup
    {
      /// <summary>
      /// A value of AdminAppealAppellantAddress.
      /// </summary>
      [JsonPropertyName("adminAppealAppellantAddress")]
      public AdminAppealAppellantAddress AdminAppealAppellantAddress
      {
        get => adminAppealAppellantAddress ??= new();
        set => adminAppealAppellantAddress = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 10;

      private AdminAppealAppellantAddress adminAppealAppellantAddress;
    }

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
    /// Gets a value of Export1.
    /// </summary>
    [JsonIgnore]
    public Array<ExportGroup> Export1 => export1 ??= new(ExportGroup.Capacity);

    /// <summary>
    /// Gets a value of Export1 for json serialization.
    /// </summary>
    [JsonPropertyName("export1")]
    [Computed]
    public IList<ExportGroup> Export1_Json
    {
      get => export1;
      set => Export1.Assign(value);
    }

    /// <summary>
    /// A value of AdministrativeAppeal.
    /// </summary>
    [JsonPropertyName("administrativeAppeal")]
    public AdministrativeAppeal AdministrativeAppeal
    {
      get => administrativeAppeal ??= new();
      set => administrativeAppeal = value;
    }

    /// <summary>
    /// A value of AppellantName.
    /// </summary>
    [JsonPropertyName("appellantName")]
    public WorkArea AppellantName
    {
      get => appellantName ??= new();
      set => appellantName = value;
    }

    private CsePersonsWorkSet csePersonsWorkSet;
    private Array<ExportGroup> export1;
    private AdministrativeAppeal administrativeAppeal;
    private WorkArea appellantName;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
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

    private CsePersonsWorkSet csePersonsWorkSet;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of ZdelClientDbf.
    /// </summary>
    [JsonPropertyName("zdelClientDbf")]
    public ZdelClientDbf ZdelClientDbf
    {
      get => zdelClientDbf ??= new();
      set => zdelClientDbf = value;
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
    /// A value of AdminAppealAppellantAddress.
    /// </summary>
    [JsonPropertyName("adminAppealAppellantAddress")]
    public AdminAppealAppellantAddress AdminAppealAppellantAddress
    {
      get => adminAppealAppellantAddress ??= new();
      set => adminAppealAppellantAddress = value;
    }

    /// <summary>
    /// A value of AdministrativeAppeal.
    /// </summary>
    [JsonPropertyName("administrativeAppeal")]
    public AdministrativeAppeal AdministrativeAppeal
    {
      get => administrativeAppeal ??= new();
      set => administrativeAppeal = value;
    }

    private ZdelClientDbf zdelClientDbf;
    private CsePerson csePerson;
    private AdminAppealAppellantAddress adminAppealAppellantAddress;
    private AdministrativeAppeal administrativeAppeal;
  }
#endregion
}
