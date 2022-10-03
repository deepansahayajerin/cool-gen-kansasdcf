// Program: SP_READ_CSE_ORG, ID: 371780617, model: 746.
// Short name: SWE01405
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
/// A program: SP_READ_CSE_ORG.
/// </para>
/// <para>
/// RESP: SRVPLAN
/// Read CSE Organization
/// </para>
/// </summary>
[Serializable]
public partial class SpReadCseOrg: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_READ_CSE_ORG program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpReadCseOrg(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpReadCseOrg.
  /// </summary>
  public SpReadCseOrg(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // *******************************************
    // ** DATE      *  DESCRIPTION
    // ** 05/11/95     R GREY		MODIFY
    // *******************************************
    if (IsEmpty(import.Search.Code))
    {
      export.Export1.Index = 0;
      export.Export1.Clear();

      foreach(var item in ReadCseOrganization2())
      {
        export.Export1.Update.CseOrganization.Assign(entities.CseOrganization);
        export.Export1.Update.Hidden.Assign(entities.CseOrganization);
        export.Export1.Next();
      }
    }
    else
    {
      export.Export1.Index = 0;
      export.Export1.Clear();

      foreach(var item in ReadCseOrganization1())
      {
        export.Export1.Update.CseOrganization.Assign(entities.CseOrganization);
        export.Export1.Update.Hidden.Assign(entities.CseOrganization);
        export.Export1.Next();
      }
    }
  }

  private IEnumerable<bool> ReadCseOrganization1()
  {
    return ReadEach("ReadCseOrganization1",
      (db, command) =>
      {
        db.SetString(command, "typeCode", import.Search.Type1);
        db.SetString(command, "organztnId", import.Search.Code);
      },
      (db, reader) =>
      {
        if (export.Export1.IsFull)
        {
          return false;
        }

        entities.CseOrganization.Code = db.GetString(reader, 0);
        entities.CseOrganization.Type1 = db.GetString(reader, 1);
        entities.CseOrganization.Name = db.GetString(reader, 2);
        entities.CseOrganization.TaxId = db.GetString(reader, 3);
        entities.CseOrganization.TaxSuffix = db.GetNullableInt32(reader, 4);
        entities.CseOrganization.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadCseOrganization2()
  {
    return ReadEach("ReadCseOrganization2",
      (db, command) =>
      {
        db.SetString(command, "typeCode", import.Search.Type1);
      },
      (db, reader) =>
      {
        if (export.Export1.IsFull)
        {
          return false;
        }

        entities.CseOrganization.Code = db.GetString(reader, 0);
        entities.CseOrganization.Type1 = db.GetString(reader, 1);
        entities.CseOrganization.Name = db.GetString(reader, 2);
        entities.CseOrganization.TaxId = db.GetString(reader, 3);
        entities.CseOrganization.TaxSuffix = db.GetNullableInt32(reader, 4);
        entities.CseOrganization.Populated = true;

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
    /// A value of Search.
    /// </summary>
    [JsonPropertyName("search")]
    public CseOrganization Search
    {
      get => search ??= new();
      set => search = value;
    }

    private CseOrganization search;
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
      /// A value of Common.
      /// </summary>
      [JsonPropertyName("common")]
      public Common Common
      {
        get => common ??= new();
        set => common = value;
      }

      /// <summary>
      /// A value of CseOrganization.
      /// </summary>
      [JsonPropertyName("cseOrganization")]
      public CseOrganization CseOrganization
      {
        get => cseOrganization ??= new();
        set => cseOrganization = value;
      }

      /// <summary>
      /// A value of Hidden.
      /// </summary>
      [JsonPropertyName("hidden")]
      public CseOrganization Hidden
      {
        get => hidden ??= new();
        set => hidden = value;
      }

      /// <summary>
      /// A value of HidVerifyDel.
      /// </summary>
      [JsonPropertyName("hidVerifyDel")]
      public CountyService HidVerifyDel
      {
        get => hidVerifyDel ??= new();
        set => hidVerifyDel = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 110;

      private Common common;
      private CseOrganization cseOrganization;
      private CseOrganization hidden;
      private CountyService hidVerifyDel;
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
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of CseOrganization.
    /// </summary>
    [JsonPropertyName("cseOrganization")]
    public CseOrganization CseOrganization
    {
      get => cseOrganization ??= new();
      set => cseOrganization = value;
    }

    private CseOrganization cseOrganization;
  }
#endregion
}
