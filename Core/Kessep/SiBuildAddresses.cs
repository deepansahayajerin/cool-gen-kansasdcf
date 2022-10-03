// Program: SI_BUILD_ADDRESSES, ID: 371735358, model: 746.
// Short name: SWE01100
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
/// A program: SI_BUILD_ADDRESSES.
/// </para>
/// <para>
/// RESP: SRVINIT
/// </para>
/// </summary>
[Serializable]
public partial class SiBuildAddresses: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_BUILD_ADDRESSES program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiBuildAddresses(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiBuildAddresses.
  /// </summary>
  public SiBuildAddresses(IContext context, Import import, Export export):
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
    //       M A I N T E N A N C E   L O G
    //   Date    Developer    Description
    // 09-06-95  K Evans      Initial development
    // 07-10-97  Sid	       Change in logic to read adabas address.
    // ---------------------------------------------
    // 	
    // 12/19/98  W.Campbell   Added
    //                        LAST_UPDATED_TIMESTAMP
    //                        to the export group view and
    //                        the database entity action view.
    //                        Work done on IDCR454.
    // -----------------------------------------
    export.Group.Index = -1;
    export.ForeignAddr.Text1 = "N";

    // * We need the AE "R"esidential address only.
    local.AddressType.Flag = "R";
    UseCabReadAdabasAddress();

    foreach(var item in ReadCsePersonAddress2())
    {
      ++export.Group.Index;
      export.Group.CheckSize();

      export.Group.Update.GdetailCsePersonAddress.Assign(
        entities.CsePersonAddress);

      if (export.Group.IsFull)
      {
        break;
      }
    }

    if (ReadCsePersonAddress1())
    {
      export.ForeignAddr.Text1 = "Y";
    }
  }

  private static void MoveCsePersonAddress(CsePersonAddress source,
    CsePersonAddress target)
  {
    target.LocationType = source.LocationType;
    target.Street1 = source.Street1;
    target.Street2 = source.Street2;
    target.City = source.City;
    target.Type1 = source.Type1;
    target.County = source.County;
    target.State = source.State;
    target.ZipCode = source.ZipCode;
    target.Zip4 = source.Zip4;
  }

  private void UseCabReadAdabasAddress()
  {
    var useImport = new CabReadAdabasAddress.Import();
    var useExport = new CabReadAdabasAddress.Export();

    useImport.CsePersonsWorkSet.Number = import.CsePersonsWorkSet.Number;
    useImport.AddressType.Flag = local.AddressType.Flag;

    Call(CabReadAdabasAddress.Execute, useImport, useExport);

    MoveCsePersonAddress(useExport.Ae, export.Ae);
  }

  private bool ReadCsePersonAddress1()
  {
    entities.CsePersonAddress.Populated = false;

    return Read("ReadCsePersonAddress1",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", import.CsePersonsWorkSet.Number);
      },
      (db, reader) =>
      {
        entities.CsePersonAddress.Identifier = db.GetDateTime(reader, 0);
        entities.CsePersonAddress.CspNumber = db.GetString(reader, 1);
        entities.CsePersonAddress.ZdelStartDate = db.GetNullableDate(reader, 2);
        entities.CsePersonAddress.SendDate = db.GetNullableDate(reader, 3);
        entities.CsePersonAddress.Source = db.GetNullableString(reader, 4);
        entities.CsePersonAddress.Street1 = db.GetNullableString(reader, 5);
        entities.CsePersonAddress.Street2 = db.GetNullableString(reader, 6);
        entities.CsePersonAddress.City = db.GetNullableString(reader, 7);
        entities.CsePersonAddress.Type1 = db.GetNullableString(reader, 8);
        entities.CsePersonAddress.WorkerId = db.GetNullableString(reader, 9);
        entities.CsePersonAddress.VerifiedDate = db.GetNullableDate(reader, 10);
        entities.CsePersonAddress.EndDate = db.GetNullableDate(reader, 11);
        entities.CsePersonAddress.EndCode = db.GetNullableString(reader, 12);
        entities.CsePersonAddress.ZdelVerifiedCode =
          db.GetNullableString(reader, 13);
        entities.CsePersonAddress.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 14);
        entities.CsePersonAddress.LastUpdatedBy =
          db.GetNullableString(reader, 15);
        entities.CsePersonAddress.State = db.GetNullableString(reader, 16);
        entities.CsePersonAddress.ZipCode = db.GetNullableString(reader, 17);
        entities.CsePersonAddress.Zip4 = db.GetNullableString(reader, 18);
        entities.CsePersonAddress.LocationType = db.GetString(reader, 19);
        entities.CsePersonAddress.County = db.GetNullableString(reader, 20);
        entities.CsePersonAddress.Populated = true;
        CheckValid<CsePersonAddress>("LocationType",
          entities.CsePersonAddress.LocationType);
      });
  }

  private IEnumerable<bool> ReadCsePersonAddress2()
  {
    entities.CsePersonAddress.Populated = false;

    return ReadEach("ReadCsePersonAddress2",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "endDate", import.LastAddr.EndDate.GetValueOrDefault());
        db.SetDateTime(
          command, "identifier",
          import.LastAddr.Identifier.GetValueOrDefault());
        db.SetString(command, "cspNumber", import.CsePersonsWorkSet.Number);
      },
      (db, reader) =>
      {
        entities.CsePersonAddress.Identifier = db.GetDateTime(reader, 0);
        entities.CsePersonAddress.CspNumber = db.GetString(reader, 1);
        entities.CsePersonAddress.ZdelStartDate = db.GetNullableDate(reader, 2);
        entities.CsePersonAddress.SendDate = db.GetNullableDate(reader, 3);
        entities.CsePersonAddress.Source = db.GetNullableString(reader, 4);
        entities.CsePersonAddress.Street1 = db.GetNullableString(reader, 5);
        entities.CsePersonAddress.Street2 = db.GetNullableString(reader, 6);
        entities.CsePersonAddress.City = db.GetNullableString(reader, 7);
        entities.CsePersonAddress.Type1 = db.GetNullableString(reader, 8);
        entities.CsePersonAddress.WorkerId = db.GetNullableString(reader, 9);
        entities.CsePersonAddress.VerifiedDate = db.GetNullableDate(reader, 10);
        entities.CsePersonAddress.EndDate = db.GetNullableDate(reader, 11);
        entities.CsePersonAddress.EndCode = db.GetNullableString(reader, 12);
        entities.CsePersonAddress.ZdelVerifiedCode =
          db.GetNullableString(reader, 13);
        entities.CsePersonAddress.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 14);
        entities.CsePersonAddress.LastUpdatedBy =
          db.GetNullableString(reader, 15);
        entities.CsePersonAddress.State = db.GetNullableString(reader, 16);
        entities.CsePersonAddress.ZipCode = db.GetNullableString(reader, 17);
        entities.CsePersonAddress.Zip4 = db.GetNullableString(reader, 18);
        entities.CsePersonAddress.LocationType = db.GetString(reader, 19);
        entities.CsePersonAddress.County = db.GetNullableString(reader, 20);
        entities.CsePersonAddress.Populated = true;
        CheckValid<CsePersonAddress>("LocationType",
          entities.CsePersonAddress.LocationType);

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
    /// A value of Common.
    /// </summary>
    [JsonPropertyName("common")]
    public Common Common
    {
      get => common ??= new();
      set => common = value;
    }

    /// <summary>
    /// A value of LastAddr.
    /// </summary>
    [JsonPropertyName("lastAddr")]
    public CsePersonAddress LastAddr
    {
      get => lastAddr ??= new();
      set => lastAddr = value;
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

    private Common common;
    private CsePersonAddress lastAddr;
    private CsePersonsWorkSet csePersonsWorkSet;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>A GroupGroup group.</summary>
    [Serializable]
    public class GroupGroup
    {
      /// <summary>
      /// A value of GdetailCommon.
      /// </summary>
      [JsonPropertyName("gdetailCommon")]
      public Common GdetailCommon
      {
        get => gdetailCommon ??= new();
        set => gdetailCommon = value;
      }

      /// <summary>
      /// A value of GdetailCsePersonAddress.
      /// </summary>
      [JsonPropertyName("gdetailCsePersonAddress")]
      public CsePersonAddress GdetailCsePersonAddress
      {
        get => gdetailCsePersonAddress ??= new();
        set => gdetailCsePersonAddress = value;
      }

      /// <summary>
      /// A value of GstatePrmt.
      /// </summary>
      [JsonPropertyName("gstatePrmt")]
      public WorkArea GstatePrmt
      {
        get => gstatePrmt ??= new();
        set => gstatePrmt = value;
      }

      /// <summary>
      /// A value of GenddtCdPrmt.
      /// </summary>
      [JsonPropertyName("genddtCdPrmt")]
      public WorkArea GenddtCdPrmt
      {
        get => genddtCdPrmt ??= new();
        set => genddtCdPrmt = value;
      }

      /// <summary>
      /// A value of GsrcePrmt.
      /// </summary>
      [JsonPropertyName("gsrcePrmt")]
      public WorkArea GsrcePrmt
      {
        get => gsrcePrmt ??= new();
        set => gsrcePrmt = value;
      }

      /// <summary>
      /// A value of GreturnPrmpt.
      /// </summary>
      [JsonPropertyName("greturnPrmpt")]
      public WorkArea GreturnPrmpt
      {
        get => greturnPrmpt ??= new();
        set => greturnPrmpt = value;
      }

      /// <summary>
      /// A value of GtypePrmpt.
      /// </summary>
      [JsonPropertyName("gtypePrmpt")]
      public WorkArea GtypePrmpt
      {
        get => gtypePrmpt ??= new();
        set => gtypePrmpt = value;
      }

      /// <summary>
      /// A value of GcntyPrmpt.
      /// </summary>
      [JsonPropertyName("gcntyPrmpt")]
      public WorkArea GcntyPrmpt
      {
        get => gcntyPrmpt ??= new();
        set => gcntyPrmpt = value;
      }

      /// <summary>
      /// A value of Ghidden.
      /// </summary>
      [JsonPropertyName("ghidden")]
      public CsePersonAddress Ghidden
      {
        get => ghidden ??= new();
        set => ghidden = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 5;

      private Common gdetailCommon;
      private CsePersonAddress gdetailCsePersonAddress;
      private WorkArea gstatePrmt;
      private WorkArea genddtCdPrmt;
      private WorkArea gsrcePrmt;
      private WorkArea greturnPrmpt;
      private WorkArea gtypePrmpt;
      private WorkArea gcntyPrmpt;
      private CsePersonAddress ghidden;
    }

    /// <summary>
    /// A value of Ae.
    /// </summary>
    [JsonPropertyName("ae")]
    public CsePersonAddress Ae
    {
      get => ae ??= new();
      set => ae = value;
    }

    /// <summary>
    /// A value of ForeignAddr.
    /// </summary>
    [JsonPropertyName("foreignAddr")]
    public WorkArea ForeignAddr
    {
      get => foreignAddr ??= new();
      set => foreignAddr = value;
    }

    /// <summary>
    /// A value of MaxPagenum.
    /// </summary>
    [JsonPropertyName("maxPagenum")]
    public Standard MaxPagenum
    {
      get => maxPagenum ??= new();
      set => maxPagenum = value;
    }

    /// <summary>
    /// Gets a value of Group.
    /// </summary>
    [JsonIgnore]
    public Array<GroupGroup> Group => group ??= new(GroupGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Group for json serialization.
    /// </summary>
    [JsonPropertyName("group")]
    [Computed]
    public IList<GroupGroup> Group_Json
    {
      get => group;
      set => Group.Assign(value);
    }

    private CsePersonAddress ae;
    private WorkArea foreignAddr;
    private Standard maxPagenum;
    private Array<GroupGroup> group;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of AddressType.
    /// </summary>
    [JsonPropertyName("addressType")]
    public Common AddressType
    {
      get => addressType ??= new();
      set => addressType = value;
    }

    /// <summary>
    /// A value of AbendData.
    /// </summary>
    [JsonPropertyName("abendData")]
    public AbendData AbendData
    {
      get => abendData ??= new();
      set => abendData = value;
    }

    private Common addressType;
    private AbendData abendData;
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
