// Program: SI_LIST_FOREIGN_ADDRESSES, ID: 371801201, model: 746.
// Short name: SWE01183
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
/// A program: SI_LIST_FOREIGN_ADDRESSES.
/// </para>
/// <para>
/// RESP: SRVINIT
/// </para>
/// </summary>
[Serializable]
public partial class SiListForeignAddresses: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_LIST_FOREIGN_ADDRESSES program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiListForeignAddresses(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiListForeignAddresses.
  /// </summary>
  public SiListForeignAddresses(IContext context, Import import, Export export):
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
    // Date	    Developer    Description
    // 09191995	Sid	 Initial development
    // 07111996	Rao	 Changes to Read each
    // 012797      R. Welborn   inserted FIPS logic.
    // 03/24/98	Siraj Konkader		ZDEL cleanup
    // ---------------------------------------------
    // 	
    // 07/13/99  Marek Lachowicz   Change property of READ (Select Only)
    export.Group.Index = -1;

    // 07/13/99  M.L 	Change property of READ (Select Only)
    if (!ReadCsePerson())
    {
      // -----------------------------------------
      // 05/25/99 W.Campbell - Replaced zd exit states.
      // -----------------------------------------
      ExitState = "CSE_PERSON_NF";

      return;
    }

    foreach(var item in ReadCsePersonAddress())
    {
      ++export.Group.Index;
      export.Group.CheckSize();

      export.Group.Update.GdetailCsePersonAddress.Assign(
        entities.CsePersonAddress);
      MoveCsePersonAddress2(entities.CsePersonAddress, export.Group.Update.Ghet);
        

      if (export.Group.IsFull)
      {
        MoveCsePersonAddress1(entities.CsePersonAddress, export.LastAddr);

        return;
      }
    }
  }

  private static void MoveCsePersonAddress1(CsePersonAddress source,
    CsePersonAddress target)
  {
    target.Identifier = source.Identifier;
    target.EndDate = source.EndDate;
  }

  private static void MoveCsePersonAddress2(CsePersonAddress source,
    CsePersonAddress target)
  {
    target.VerifiedDate = source.VerifiedDate;
    target.EndDate = source.EndDate;
    target.EndCode = source.EndCode;
    target.ZdelVerifiedCode = source.ZdelVerifiedCode;
  }

  private bool ReadCsePerson()
  {
    entities.CsePerson.Populated = false;

    return Read("ReadCsePerson",
      (db, command) =>
      {
        db.SetString(command, "numb", import.Search.Number);
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CsePerson.Populated = true;
      });
  }

  private IEnumerable<bool> ReadCsePersonAddress()
  {
    entities.CsePersonAddress.Populated = false;

    return ReadEach("ReadCsePersonAddress",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "endDate", import.LastAddr.EndDate.GetValueOrDefault());
        db.SetDateTime(
          command, "identifier",
          import.LastAddr.Identifier.GetValueOrDefault());
        db.SetString(command, "cspNumber", import.Search.Number);
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
        entities.CsePersonAddress.Street3 = db.GetNullableString(reader, 15);
        entities.CsePersonAddress.Street4 = db.GetNullableString(reader, 16);
        entities.CsePersonAddress.Province = db.GetNullableString(reader, 17);
        entities.CsePersonAddress.PostalCode = db.GetNullableString(reader, 18);
        entities.CsePersonAddress.Country = db.GetNullableString(reader, 19);
        entities.CsePersonAddress.LocationType = db.GetString(reader, 20);
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
    /// A value of LastAddr.
    /// </summary>
    [JsonPropertyName("lastAddr")]
    public CsePersonAddress LastAddr
    {
      get => lastAddr ??= new();
      set => lastAddr = value;
    }

    /// <summary>
    /// A value of Search.
    /// </summary>
    [JsonPropertyName("search")]
    public CsePersonsWorkSet Search
    {
      get => search ??= new();
      set => search = value;
    }

    private CsePersonAddress lastAddr;
    private CsePersonsWorkSet search;
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
      /// A value of Ghet.
      /// </summary>
      [JsonPropertyName("ghet")]
      public CsePersonAddress Ghet
      {
        get => ghet ??= new();
        set => ghet = value;
      }

      /// <summary>
      /// A value of GpromptCountry.
      /// </summary>
      [JsonPropertyName("gpromptCountry")]
      public Common GpromptCountry
      {
        get => gpromptCountry ??= new();
        set => gpromptCountry = value;
      }

      /// <summary>
      /// A value of GpromptSourceCode.
      /// </summary>
      [JsonPropertyName("gpromptSourceCode")]
      public Common GpromptSourceCode
      {
        get => gpromptSourceCode ??= new();
        set => gpromptSourceCode = value;
      }

      /// <summary>
      /// A value of GpromptEndCode.
      /// </summary>
      [JsonPropertyName("gpromptEndCode")]
      public Common GpromptEndCode
      {
        get => gpromptEndCode ??= new();
        set => gpromptEndCode = value;
      }

      /// <summary>
      /// A value of GpromptReturnCode.
      /// </summary>
      [JsonPropertyName("gpromptReturnCode")]
      public Common GpromptReturnCode
      {
        get => gpromptReturnCode ??= new();
        set => gpromptReturnCode = value;
      }

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

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 4;

      private CsePersonAddress ghet;
      private Common gpromptCountry;
      private Common gpromptSourceCode;
      private Common gpromptEndCode;
      private Common gpromptReturnCode;
      private Common gdetailCommon;
      private CsePersonAddress gdetailCsePersonAddress;
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

    private CsePersonAddress lastAddr;
    private Array<GroupGroup> group;
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
