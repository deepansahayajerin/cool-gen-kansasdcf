// Program: OE_FACL_READ_JAIL_ADDRESSES, ID: 373339598, model: 746.
// Short name: SWE00755
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
/// A program: OE_FACL_READ_JAIL_ADDRESSES.
/// </para>
/// <para>
/// This action block will read the jail_address records from database.
/// </para>
/// </summary>
[Serializable]
public partial class OeFaclReadJailAddresses: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_FACL_READ_JAIL_ADDRESSES program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeFaclReadJailAddresses(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeFaclReadJailAddresses.
  /// </summary>
  public OeFaclReadJailAddresses(IContext context, Import import, Export export):
    
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ------------------------------------------------------------------------
    // 04/05/2002             Vithal Madhira              PR# 140102,140103
    // Initial Development.
    // This CAB will be used only in FACL PSTEP. This CAB is used to READ  '
    // Jail_Address'  records based on a 'filter' criteria.
    // ----------------------------------------------------------------------
    if (!IsEmpty(import.Filter.City) && !IsEmpty(import.Filter.State))
    {
      export.Export2.Index = -1;
      export.Export2.Count = 0;

      foreach(var item in ReadJailAddresses1())
      {
        ++export.Export2.Index;
        export.Export2.CheckSize();

        export.Export2.Update.Export2Detail.Assign(entities.JailAddresses);

        if (export.Export2.IsFull)
        {
          export.HiddenGroupFull.Flag = "Y";

          return;
        }
      }
    }
    else if (IsEmpty(import.Filter.City) && !IsEmpty(import.Filter.State))
    {
      export.Export2.Index = -1;
      export.Export2.Count = 0;

      foreach(var item in ReadJailAddresses2())
      {
        ++export.Export2.Index;
        export.Export2.CheckSize();

        export.Export2.Update.Export2Detail.Assign(entities.JailAddresses);

        if (export.Export2.IsFull)
        {
          export.HiddenGroupFull.Flag = "Y";

          return;
        }
      }
    }
  }

  private IEnumerable<bool> ReadJailAddresses1()
  {
    entities.JailAddresses.Populated = false;

    return ReadEach("ReadJailAddresses1",
      (db, command) =>
      {
        db.SetNullableString(command, "city", import.Filter.City ?? "");
        db.SetNullableString(command, "state", import.Filter.State ?? "");
      },
      (db, reader) =>
      {
        entities.JailAddresses.Identifier = db.GetInt32(reader, 0);
        entities.JailAddresses.Street1 = db.GetNullableString(reader, 1);
        entities.JailAddresses.Street2 = db.GetNullableString(reader, 2);
        entities.JailAddresses.City = db.GetNullableString(reader, 3);
        entities.JailAddresses.State = db.GetNullableString(reader, 4);
        entities.JailAddresses.Province = db.GetNullableString(reader, 5);
        entities.JailAddresses.PostalCode = db.GetNullableString(reader, 6);
        entities.JailAddresses.ZipCode5 = db.GetNullableString(reader, 7);
        entities.JailAddresses.ZipCode4 = db.GetNullableString(reader, 8);
        entities.JailAddresses.ZipCode3 = db.GetNullableString(reader, 9);
        entities.JailAddresses.Country = db.GetNullableString(reader, 10);
        entities.JailAddresses.CreatedBy = db.GetString(reader, 11);
        entities.JailAddresses.CreatedTimestamp = db.GetDateTime(reader, 12);
        entities.JailAddresses.LastUpdatedBy = db.GetString(reader, 13);
        entities.JailAddresses.LastUpdatedTimestamp =
          db.GetDateTime(reader, 14);
        entities.JailAddresses.JailName = db.GetNullableString(reader, 15);
        entities.JailAddresses.Phone = db.GetNullableInt32(reader, 16);
        entities.JailAddresses.PhoneAreaCode = db.GetNullableInt32(reader, 17);
        entities.JailAddresses.PhoneExtension =
          db.GetNullableString(reader, 18);
        entities.JailAddresses.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadJailAddresses2()
  {
    entities.JailAddresses.Populated = false;

    return ReadEach("ReadJailAddresses2",
      (db, command) =>
      {
        db.SetNullableString(command, "state", import.Filter.State ?? "");
      },
      (db, reader) =>
      {
        entities.JailAddresses.Identifier = db.GetInt32(reader, 0);
        entities.JailAddresses.Street1 = db.GetNullableString(reader, 1);
        entities.JailAddresses.Street2 = db.GetNullableString(reader, 2);
        entities.JailAddresses.City = db.GetNullableString(reader, 3);
        entities.JailAddresses.State = db.GetNullableString(reader, 4);
        entities.JailAddresses.Province = db.GetNullableString(reader, 5);
        entities.JailAddresses.PostalCode = db.GetNullableString(reader, 6);
        entities.JailAddresses.ZipCode5 = db.GetNullableString(reader, 7);
        entities.JailAddresses.ZipCode4 = db.GetNullableString(reader, 8);
        entities.JailAddresses.ZipCode3 = db.GetNullableString(reader, 9);
        entities.JailAddresses.Country = db.GetNullableString(reader, 10);
        entities.JailAddresses.CreatedBy = db.GetString(reader, 11);
        entities.JailAddresses.CreatedTimestamp = db.GetDateTime(reader, 12);
        entities.JailAddresses.LastUpdatedBy = db.GetString(reader, 13);
        entities.JailAddresses.LastUpdatedTimestamp =
          db.GetDateTime(reader, 14);
        entities.JailAddresses.JailName = db.GetNullableString(reader, 15);
        entities.JailAddresses.Phone = db.GetNullableInt32(reader, 16);
        entities.JailAddresses.PhoneAreaCode = db.GetNullableInt32(reader, 17);
        entities.JailAddresses.PhoneExtension =
          db.GetNullableString(reader, 18);
        entities.JailAddresses.Populated = true;

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
    /// A value of Filter.
    /// </summary>
    [JsonPropertyName("filter")]
    public JailAddresses Filter
    {
      get => filter ??= new();
      set => filter = value;
    }

    private JailAddresses filter;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>A Export2Group group.</summary>
    [Serializable]
    public class Export2Group
    {
      /// <summary>
      /// A value of Export2Detail.
      /// </summary>
      [JsonPropertyName("export2Detail")]
      public JailAddresses Export2Detail
      {
        get => export2Detail ??= new();
        set => export2Detail = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 80;

      private JailAddresses export2Detail;
    }

    /// <summary>
    /// Gets a value of Export2.
    /// </summary>
    [JsonIgnore]
    public Array<Export2Group> Export2 => export2 ??= new(
      Export2Group.Capacity, 0);

    /// <summary>
    /// Gets a value of Export2 for json serialization.
    /// </summary>
    [JsonPropertyName("export2")]
    [Computed]
    public IList<Export2Group> Export2_Json
    {
      get => export2;
      set => Export2.Assign(value);
    }

    /// <summary>
    /// A value of HiddenGroupFull.
    /// </summary>
    [JsonPropertyName("hiddenGroupFull")]
    public Common HiddenGroupFull
    {
      get => hiddenGroupFull ??= new();
      set => hiddenGroupFull = value;
    }

    private Array<Export2Group> export2;
    private Common hiddenGroupFull;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of Blank.
    /// </summary>
    [JsonPropertyName("blank")]
    public JailAddresses Blank
    {
      get => blank ??= new();
      set => blank = value;
    }

    private JailAddresses blank;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of JailAddresses.
    /// </summary>
    [JsonPropertyName("jailAddresses")]
    public JailAddresses JailAddresses
    {
      get => jailAddresses ??= new();
      set => jailAddresses = value;
    }

    private JailAddresses jailAddresses;
  }
#endregion
}
