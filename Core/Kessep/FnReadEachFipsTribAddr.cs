// Program: FN_READ_EACH_FIPS_TRIB_ADDR, ID: 372019603, model: 746.
// Short name: SWE01933
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: FN_READ_EACH_FIPS_TRIB_ADDR.
/// </para>
/// <para>
/// This cab reads each FIPS/Tribunal Address for  a persistent import view of 
/// either a Tribunal or a FIPS entity, with a key for starting read, by
/// descending last updated time stamp.
/// RVW 1/27/97
/// </para>
/// </summary>
[Serializable]
public partial class FnReadEachFipsTribAddr: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_READ_EACH_FIPS_TRIB_ADDR program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnReadEachFipsTribAddr(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnReadEachFipsTribAddr.
  /// </summary>
  public FnReadEachFipsTribAddr(IContext context, Import import, Export export):
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
    // Date		Developer	Request #	Description
    // 10/08/98	D. JEAN			        Remove unused and persistent views
    //                                                 
    // and modified logic accordingly
    // ------------------------------------------------------------
    export.Export1.Index = 0;
    export.Export1.Clear();

    foreach(var item in ReadFipsTribAddress())
    {
      export.Export1.Update.Detail.Assign(entities.ExistingFipsTribAddress);
      export.Export1.Next();
    }
  }

  private IEnumerable<bool> ReadFipsTribAddress()
  {
    return ReadEach("ReadFipsTribAddress",
      (db, command) =>
      {
        db.SetNullableInt32(command, "fipState", import.Fips.State);
        db.SetNullableInt32(command, "fipCounty", import.Fips.County);
        db.SetNullableInt32(command, "fipLocation", import.Fips.Location);
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
        entities.ExistingFipsTribAddress.PostalCode =
          db.GetNullableString(reader, 16);
        entities.ExistingFipsTribAddress.Country =
          db.GetNullableString(reader, 17);
        entities.ExistingFipsTribAddress.PhoneNumber =
          db.GetNullableInt32(reader, 18);
        entities.ExistingFipsTribAddress.FaxNumber =
          db.GetNullableInt32(reader, 19);
        entities.ExistingFipsTribAddress.CreatedBy = db.GetString(reader, 20);
        entities.ExistingFipsTribAddress.CreatedTstamp =
          db.GetDateTime(reader, 21);
        entities.ExistingFipsTribAddress.LastUpdatedBy =
          db.GetNullableString(reader, 22);
        entities.ExistingFipsTribAddress.LastUpdatedTstamp =
          db.GetNullableDateTime(reader, 23);
        entities.ExistingFipsTribAddress.FipState =
          db.GetNullableInt32(reader, 24);
        entities.ExistingFipsTribAddress.FipCounty =
          db.GetNullableInt32(reader, 25);
        entities.ExistingFipsTribAddress.FipLocation =
          db.GetNullableInt32(reader, 26);
        entities.ExistingFipsTribAddress.Populated = true;

        return true;
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

    private Fips fips;
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
      /// A value of Detail.
      /// </summary>
      [JsonPropertyName("detail")]
      public FipsTribAddress Detail
      {
        get => detail ??= new();
        set => detail = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 25;

      private FipsTribAddress detail;
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

    private Array<ExportGroup> export1;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of Initialized.
    /// </summary>
    [JsonPropertyName("initialized")]
    public DateWorkArea Initialized
    {
      get => initialized ??= new();
      set => initialized = value;
    }

    private DateWorkArea initialized;
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
    /// A value of ExistingFipsTribAddress.
    /// </summary>
    [JsonPropertyName("existingFipsTribAddress")]
    public FipsTribAddress ExistingFipsTribAddress
    {
      get => existingFipsTribAddress ??= new();
      set => existingFipsTribAddress = value;
    }

    private Fips existingFips;
    private FipsTribAddress existingFipsTribAddress;
  }
#endregion
}
