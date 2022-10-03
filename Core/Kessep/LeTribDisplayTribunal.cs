// Program: LE_TRIB_DISPLAY_TRIBUNAL, ID: 372021817, model: 746.
// Short name: SWE00823
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: LE_TRIB_DISPLAY_TRIBUNAL.
/// </para>
/// <para>
/// RESP: LGLENFAC
/// This action block reads and populates tribunal and related entity type 
/// occurrences for display.
/// </para>
/// </summary>
[Serializable]
public partial class LeTribDisplayTribunal: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the LE_TRIB_DISPLAY_TRIBUNAL program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new LeTribDisplayTribunal(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of LeTribDisplayTribunal.
  /// </summary>
  public LeTribDisplayTribunal(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // *******************************************************************
    //   Date		Developer	Request #	Description
    // 09/15/2000	GVandy		PR 102557
    // Modified to return tribunal document header attributes.
    // *******************************************************************
    MoveTribunal(import.Tribunal, export.Tribunal);
    MoveFips2(import.Fips, export.Fips);

    if (export.Tribunal.Identifier == 0)
    {
      if (import.Fips.State != 0)
      {
        if (ReadFips1())
        {
          export.Fips.Assign(entities.ExistingFips);

          export.Export1.Index = 0;
          export.Export1.Clear();

          foreach(var item in ReadFipsTribAddress1())
          {
            MoveFipsTribAddress(entities.ExistingFipsTribAddress,
              export.Export1.Update.Detail);
            export.Export1.Next();
          }

          if (ReadTribunal2())
          {
            export.Tribunal.Assign(entities.ExistingTribunal);
          }
          else
          {
            MoveFips1(local.ClearFips, export.Fips);
            export.Tribunal.Assign(local.ClearTribunal);
            ExitState = "TRIBUNAL_NF";
          }

          return;
        }
        else
        {
          MoveFips1(local.ClearFips, export.Fips);
          export.Tribunal.Assign(local.ClearTribunal);
          ExitState = "FIPS_NF";

          return;
        }
      }
    }

    if (ReadTribunal1())
    {
      export.Tribunal.Assign(entities.ExistingTribunal);
    }
    else
    {
      ExitState = "TRIBUNAL_NF";

      return;
    }

    export.Export1.Index = 0;
    export.Export1.Clear();

    foreach(var item in ReadFipsTribAddress2())
    {
      MoveFipsTribAddress(entities.ExistingFipsTribAddress,
        export.Export1.Update.Detail);
      export.Export1.Next();
    }

    if (ReadFips2())
    {
      export.Fips.Assign(entities.ExistingFips);
    }
    else
    {
      export.Fips.Assign(local.Null1);
    }
  }

  private static void MoveFips1(Fips source, Fips target)
  {
    target.StateAbbreviation = source.StateAbbreviation;
    target.CountyAbbreviation = source.CountyAbbreviation;
    target.StateDescription = source.StateDescription;
    target.CountyDescription = source.CountyDescription;
    target.LocationDescription = source.LocationDescription;
  }

  private static void MoveFips2(Fips source, Fips target)
  {
    target.State = source.State;
    target.County = source.County;
    target.Location = source.Location;
  }

  private static void MoveFipsTribAddress(FipsTribAddress source,
    FipsTribAddress target)
  {
    target.Identifier = source.Identifier;
    target.FaxExtension = source.FaxExtension;
    target.FaxAreaCode = source.FaxAreaCode;
    target.PhoneExtension = source.PhoneExtension;
    target.AreaCode = source.AreaCode;
    target.Type1 = source.Type1;
    target.Street1 = source.Street1;
    target.Street2 = source.Street2;
    target.City = source.City;
    target.State = source.State;
    target.ZipCode = source.ZipCode;
    target.Zip4 = source.Zip4;
    target.Zip3 = source.Zip3;
    target.County = source.County;
    target.Street3 = source.Street3;
    target.Street4 = source.Street4;
    target.Province = source.Province;
    target.PostalCode = source.PostalCode;
    target.Country = source.Country;
    target.PhoneNumber = source.PhoneNumber;
    target.FaxNumber = source.FaxNumber;
  }

  private static void MoveTribunal(Tribunal source, Tribunal target)
  {
    target.Identifier = source.Identifier;
    target.Name = source.Name;
    target.JudicialDivision = source.JudicialDivision;
  }

  private bool ReadFips1()
  {
    entities.ExistingFips.Populated = false;

    return Read("ReadFips1",
      (db, command) =>
      {
        db.SetInt32(command, "state", import.Fips.State);
        db.SetInt32(command, "county", import.Fips.County);
        db.SetInt32(command, "location", import.Fips.Location);
      },
      (db, reader) =>
      {
        entities.ExistingFips.State = db.GetInt32(reader, 0);
        entities.ExistingFips.County = db.GetInt32(reader, 1);
        entities.ExistingFips.Location = db.GetInt32(reader, 2);
        entities.ExistingFips.StateDescription =
          db.GetNullableString(reader, 3);
        entities.ExistingFips.CountyDescription =
          db.GetNullableString(reader, 4);
        entities.ExistingFips.LocationDescription =
          db.GetNullableString(reader, 5);
        entities.ExistingFips.CreatedBy = db.GetString(reader, 6);
        entities.ExistingFips.CreatedTstamp = db.GetDateTime(reader, 7);
        entities.ExistingFips.LastUpdatedBy = db.GetNullableString(reader, 8);
        entities.ExistingFips.LastUpdatedTstamp =
          db.GetNullableDateTime(reader, 9);
        entities.ExistingFips.StateAbbreviation = db.GetString(reader, 10);
        entities.ExistingFips.CountyAbbreviation =
          db.GetNullableString(reader, 11);
        entities.ExistingFips.Populated = true;
      });
  }

  private bool ReadFips2()
  {
    System.Diagnostics.Debug.Assert(entities.ExistingTribunal.Populated);
    entities.ExistingFips.Populated = false;

    return Read("ReadFips2",
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
        entities.ExistingFips.StateDescription =
          db.GetNullableString(reader, 3);
        entities.ExistingFips.CountyDescription =
          db.GetNullableString(reader, 4);
        entities.ExistingFips.LocationDescription =
          db.GetNullableString(reader, 5);
        entities.ExistingFips.CreatedBy = db.GetString(reader, 6);
        entities.ExistingFips.CreatedTstamp = db.GetDateTime(reader, 7);
        entities.ExistingFips.LastUpdatedBy = db.GetNullableString(reader, 8);
        entities.ExistingFips.LastUpdatedTstamp =
          db.GetNullableDateTime(reader, 9);
        entities.ExistingFips.StateAbbreviation = db.GetString(reader, 10);
        entities.ExistingFips.CountyAbbreviation =
          db.GetNullableString(reader, 11);
        entities.ExistingFips.Populated = true;
      });
  }

  private IEnumerable<bool> ReadFipsTribAddress1()
  {
    return ReadEach("ReadFipsTribAddress1",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "fipLocation", entities.ExistingFips.Location);
        db.SetNullableInt32(command, "fipCounty", entities.ExistingFips.County);
        db.SetNullableInt32(command, "fipState", entities.ExistingFips.State);
      },
      (db, reader) =>
      {
        if (export.Export1.IsFull)
        {
          return false;
        }

        entities.ExistingFipsTribAddress.Identifier = db.GetInt32(reader, 0);
        entities.ExistingFipsTribAddress.FaxExtension =
          db.GetNullableString(reader, 1);
        entities.ExistingFipsTribAddress.FaxAreaCode =
          db.GetNullableInt32(reader, 2);
        entities.ExistingFipsTribAddress.PhoneExtension =
          db.GetNullableString(reader, 3);
        entities.ExistingFipsTribAddress.AreaCode =
          db.GetNullableInt32(reader, 4);
        entities.ExistingFipsTribAddress.Type1 = db.GetString(reader, 5);
        entities.ExistingFipsTribAddress.Street1 = db.GetString(reader, 6);
        entities.ExistingFipsTribAddress.Street2 =
          db.GetNullableString(reader, 7);
        entities.ExistingFipsTribAddress.City = db.GetString(reader, 8);
        entities.ExistingFipsTribAddress.State = db.GetString(reader, 9);
        entities.ExistingFipsTribAddress.ZipCode = db.GetString(reader, 10);
        entities.ExistingFipsTribAddress.Zip4 =
          db.GetNullableString(reader, 11);
        entities.ExistingFipsTribAddress.Zip3 =
          db.GetNullableString(reader, 12);
        entities.ExistingFipsTribAddress.County =
          db.GetNullableString(reader, 13);
        entities.ExistingFipsTribAddress.Street3 =
          db.GetNullableString(reader, 14);
        entities.ExistingFipsTribAddress.Street4 =
          db.GetNullableString(reader, 15);
        entities.ExistingFipsTribAddress.Province =
          db.GetNullableString(reader, 16);
        entities.ExistingFipsTribAddress.PostalCode =
          db.GetNullableString(reader, 17);
        entities.ExistingFipsTribAddress.Country =
          db.GetNullableString(reader, 18);
        entities.ExistingFipsTribAddress.PhoneNumber =
          db.GetNullableInt32(reader, 19);
        entities.ExistingFipsTribAddress.FaxNumber =
          db.GetNullableInt32(reader, 20);
        entities.ExistingFipsTribAddress.CreatedBy = db.GetString(reader, 21);
        entities.ExistingFipsTribAddress.CreatedTstamp =
          db.GetDateTime(reader, 22);
        entities.ExistingFipsTribAddress.LastUpdatedBy =
          db.GetNullableString(reader, 23);
        entities.ExistingFipsTribAddress.LastUpdatedTstamp =
          db.GetNullableDateTime(reader, 24);
        entities.ExistingFipsTribAddress.FipState =
          db.GetNullableInt32(reader, 25);
        entities.ExistingFipsTribAddress.FipCounty =
          db.GetNullableInt32(reader, 26);
        entities.ExistingFipsTribAddress.FipLocation =
          db.GetNullableInt32(reader, 27);
        entities.ExistingFipsTribAddress.TrbId =
          db.GetNullableInt32(reader, 28);
        entities.ExistingFipsTribAddress.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadFipsTribAddress2()
  {
    return ReadEach("ReadFipsTribAddress2",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "trbId", entities.ExistingTribunal.Identifier);
      },
      (db, reader) =>
      {
        if (export.Export1.IsFull)
        {
          return false;
        }

        entities.ExistingFipsTribAddress.Identifier = db.GetInt32(reader, 0);
        entities.ExistingFipsTribAddress.FaxExtension =
          db.GetNullableString(reader, 1);
        entities.ExistingFipsTribAddress.FaxAreaCode =
          db.GetNullableInt32(reader, 2);
        entities.ExistingFipsTribAddress.PhoneExtension =
          db.GetNullableString(reader, 3);
        entities.ExistingFipsTribAddress.AreaCode =
          db.GetNullableInt32(reader, 4);
        entities.ExistingFipsTribAddress.Type1 = db.GetString(reader, 5);
        entities.ExistingFipsTribAddress.Street1 = db.GetString(reader, 6);
        entities.ExistingFipsTribAddress.Street2 =
          db.GetNullableString(reader, 7);
        entities.ExistingFipsTribAddress.City = db.GetString(reader, 8);
        entities.ExistingFipsTribAddress.State = db.GetString(reader, 9);
        entities.ExistingFipsTribAddress.ZipCode = db.GetString(reader, 10);
        entities.ExistingFipsTribAddress.Zip4 =
          db.GetNullableString(reader, 11);
        entities.ExistingFipsTribAddress.Zip3 =
          db.GetNullableString(reader, 12);
        entities.ExistingFipsTribAddress.County =
          db.GetNullableString(reader, 13);
        entities.ExistingFipsTribAddress.Street3 =
          db.GetNullableString(reader, 14);
        entities.ExistingFipsTribAddress.Street4 =
          db.GetNullableString(reader, 15);
        entities.ExistingFipsTribAddress.Province =
          db.GetNullableString(reader, 16);
        entities.ExistingFipsTribAddress.PostalCode =
          db.GetNullableString(reader, 17);
        entities.ExistingFipsTribAddress.Country =
          db.GetNullableString(reader, 18);
        entities.ExistingFipsTribAddress.PhoneNumber =
          db.GetNullableInt32(reader, 19);
        entities.ExistingFipsTribAddress.FaxNumber =
          db.GetNullableInt32(reader, 20);
        entities.ExistingFipsTribAddress.CreatedBy = db.GetString(reader, 21);
        entities.ExistingFipsTribAddress.CreatedTstamp =
          db.GetDateTime(reader, 22);
        entities.ExistingFipsTribAddress.LastUpdatedBy =
          db.GetNullableString(reader, 23);
        entities.ExistingFipsTribAddress.LastUpdatedTstamp =
          db.GetNullableDateTime(reader, 24);
        entities.ExistingFipsTribAddress.FipState =
          db.GetNullableInt32(reader, 25);
        entities.ExistingFipsTribAddress.FipCounty =
          db.GetNullableInt32(reader, 26);
        entities.ExistingFipsTribAddress.FipLocation =
          db.GetNullableInt32(reader, 27);
        entities.ExistingFipsTribAddress.TrbId =
          db.GetNullableInt32(reader, 28);
        entities.ExistingFipsTribAddress.Populated = true;

        return true;
      });
  }

  private bool ReadTribunal1()
  {
    entities.ExistingTribunal.Populated = false;

    return Read("ReadTribunal1",
      (db, command) =>
      {
        db.SetInt32(command, "identifier", export.Tribunal.Identifier);
      },
      (db, reader) =>
      {
        entities.ExistingTribunal.JudicialDivision =
          db.GetNullableString(reader, 0);
        entities.ExistingTribunal.Name = db.GetString(reader, 1);
        entities.ExistingTribunal.FipLocation = db.GetNullableInt32(reader, 2);
        entities.ExistingTribunal.JudicialDistrict = db.GetString(reader, 3);
        entities.ExistingTribunal.CreatedBy = db.GetString(reader, 4);
        entities.ExistingTribunal.CreatedTstamp = db.GetDateTime(reader, 5);
        entities.ExistingTribunal.LastUpdatedBy =
          db.GetNullableString(reader, 6);
        entities.ExistingTribunal.LastUpdatedTstamp =
          db.GetNullableDateTime(reader, 7);
        entities.ExistingTribunal.Identifier = db.GetInt32(reader, 8);
        entities.ExistingTribunal.TaxIdSuffix = db.GetNullableString(reader, 9);
        entities.ExistingTribunal.TaxId = db.GetNullableString(reader, 10);
        entities.ExistingTribunal.DocumentHeader1 =
          db.GetNullableString(reader, 11);
        entities.ExistingTribunal.DocumentHeader2 =
          db.GetNullableString(reader, 12);
        entities.ExistingTribunal.DocumentHeader3 =
          db.GetNullableString(reader, 13);
        entities.ExistingTribunal.DocumentHeader4 =
          db.GetNullableString(reader, 14);
        entities.ExistingTribunal.DocumentHeader5 =
          db.GetNullableString(reader, 15);
        entities.ExistingTribunal.DocumentHeader6 =
          db.GetNullableString(reader, 16);
        entities.ExistingTribunal.FipCounty = db.GetNullableInt32(reader, 17);
        entities.ExistingTribunal.FipState = db.GetNullableInt32(reader, 18);
        entities.ExistingTribunal.Populated = true;
      });
  }

  private bool ReadTribunal2()
  {
    entities.ExistingTribunal.Populated = false;

    return Read("ReadTribunal2",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "fipLocation", entities.ExistingFips.Location);
        db.SetNullableInt32(command, "fipCounty", entities.ExistingFips.County);
        db.SetNullableInt32(command, "fipState", entities.ExistingFips.State);
      },
      (db, reader) =>
      {
        entities.ExistingTribunal.JudicialDivision =
          db.GetNullableString(reader, 0);
        entities.ExistingTribunal.Name = db.GetString(reader, 1);
        entities.ExistingTribunal.FipLocation = db.GetNullableInt32(reader, 2);
        entities.ExistingTribunal.JudicialDistrict = db.GetString(reader, 3);
        entities.ExistingTribunal.CreatedBy = db.GetString(reader, 4);
        entities.ExistingTribunal.CreatedTstamp = db.GetDateTime(reader, 5);
        entities.ExistingTribunal.LastUpdatedBy =
          db.GetNullableString(reader, 6);
        entities.ExistingTribunal.LastUpdatedTstamp =
          db.GetNullableDateTime(reader, 7);
        entities.ExistingTribunal.Identifier = db.GetInt32(reader, 8);
        entities.ExistingTribunal.TaxIdSuffix = db.GetNullableString(reader, 9);
        entities.ExistingTribunal.TaxId = db.GetNullableString(reader, 10);
        entities.ExistingTribunal.DocumentHeader1 =
          db.GetNullableString(reader, 11);
        entities.ExistingTribunal.DocumentHeader2 =
          db.GetNullableString(reader, 12);
        entities.ExistingTribunal.DocumentHeader3 =
          db.GetNullableString(reader, 13);
        entities.ExistingTribunal.DocumentHeader4 =
          db.GetNullableString(reader, 14);
        entities.ExistingTribunal.DocumentHeader5 =
          db.GetNullableString(reader, 15);
        entities.ExistingTribunal.DocumentHeader6 =
          db.GetNullableString(reader, 16);
        entities.ExistingTribunal.FipCounty = db.GetNullableInt32(reader, 17);
        entities.ExistingTribunal.FipState = db.GetNullableInt32(reader, 18);
        entities.ExistingTribunal.Populated = true;
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

    private Fips fips;
    private Tribunal tribunal;
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
      /// A value of DetailSelAddr.
      /// </summary>
      [JsonPropertyName("detailSelAddr")]
      public Common DetailSelAddr
      {
        get => detailSelAddr ??= new();
        set => detailSelAddr = value;
      }

      /// <summary>
      /// A value of DetailListAddrTp.
      /// </summary>
      [JsonPropertyName("detailListAddrTp")]
      public Standard DetailListAddrTp
      {
        get => detailListAddrTp ??= new();
        set => detailListAddrTp = value;
      }

      /// <summary>
      /// A value of DetailListStates.
      /// </summary>
      [JsonPropertyName("detailListStates")]
      public Standard DetailListStates
      {
        get => detailListStates ??= new();
        set => detailListStates = value;
      }

      /// <summary>
      /// A value of Detail.
      /// </summary>
      [JsonPropertyName("detail")]
      public FipsTribAddress Detail
      {
        get => detail ??= new();
        set => detail = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 6;

      private Common detailSelAddr;
      private Standard detailListAddrTp;
      private Standard detailListStates;
      private FipsTribAddress detail;
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

    private Fips fips;
    private Tribunal tribunal;
    private Array<ExportGroup> export1;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of Null1.
    /// </summary>
    [JsonPropertyName("null1")]
    public Fips Null1
    {
      get => null1 ??= new();
      set => null1 = value;
    }

    /// <summary>
    /// A value of ClearTribunal.
    /// </summary>
    [JsonPropertyName("clearTribunal")]
    public Tribunal ClearTribunal
    {
      get => clearTribunal ??= new();
      set => clearTribunal = value;
    }

    /// <summary>
    /// A value of ClearFips.
    /// </summary>
    [JsonPropertyName("clearFips")]
    public Fips ClearFips
    {
      get => clearFips ??= new();
      set => clearFips = value;
    }

    private Fips null1;
    private Tribunal clearTribunal;
    private Fips clearFips;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
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
    /// A value of ExistingFipsTribAddress.
    /// </summary>
    [JsonPropertyName("existingFipsTribAddress")]
    public FipsTribAddress ExistingFipsTribAddress
    {
      get => existingFipsTribAddress ??= new();
      set => existingFipsTribAddress = value;
    }

    private Fips existingFips;
    private Tribunal existingTribunal;
    private FipsTribAddress existingFipsTribAddress;
  }
#endregion
}
