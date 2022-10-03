// Program: FN_CAB_CHECK_ALT_ADDR, ID: 372084594, model: 746.
// Short name: SWE02218
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: FN_CAB_CHECK_ALT_ADDR.
/// </para>
/// <para>
/// This cab expects the Alternate Cse_person number AS Input and checks if this
/// cse_person has a valid alternate billing address or not
/// </para>
/// </summary>
[Serializable]
public partial class FnCabCheckAltAddr: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_CAB_CHECK_ALT_ADDR program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnCabCheckAltAddr(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnCabCheckAltAddr.
  /// </summary>
  public FnCabCheckAltAddr(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    if (!ReadCsePerson())
    {
      return;
    }

    if (AsChar(entities.Alternate.Type1) == 'C')
    {
      if (!ReadCsePersonAddress1())
      {
        ExitState = "FN0000_ALTERNATE_ADD_NF";
      }
    }
    else if (!ReadCsePersonAddress2())
    {
      // <<< FIND OUT IF A FIPS-TRIBUNAL ADDRESS EXISTS OR NOT FOR THIS 
      // ORGANIZATION >>>
      if (!ReadFipsTribAddress())
      {
        ExitState = "FN0000_ALTERNATE_ADD_NF";
      }
    }
  }

  private bool ReadCsePerson()
  {
    entities.Alternate.Populated = false;

    return Read("ReadCsePerson",
      (db, command) =>
      {
        db.SetString(command, "numb", import.Alternate.Number);
      },
      (db, reader) =>
      {
        entities.Alternate.Number = db.GetString(reader, 0);
        entities.Alternate.Type1 = db.GetString(reader, 1);
        entities.Alternate.Populated = true;
        CheckValid<CsePerson>("Type1", entities.Alternate.Type1);
      });
  }

  private bool ReadCsePersonAddress1()
  {
    entities.AlternateAdd.Populated = false;

    return Read("ReadCsePersonAddress1",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.Alternate.Number);
        db.SetNullableDate(
          command, "verifiedDate", import.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.AlternateAdd.Identifier = db.GetDateTime(reader, 0);
        entities.AlternateAdd.CspNumber = db.GetString(reader, 1);
        entities.AlternateAdd.ZdelStartDate = db.GetNullableDate(reader, 2);
        entities.AlternateAdd.Type1 = db.GetNullableString(reader, 3);
        entities.AlternateAdd.VerifiedDate = db.GetNullableDate(reader, 4);
        entities.AlternateAdd.EndDate = db.GetNullableDate(reader, 5);
        entities.AlternateAdd.Populated = true;
      });
  }

  private bool ReadCsePersonAddress2()
  {
    entities.AlternateAdd.Populated = false;

    return Read("ReadCsePersonAddress2",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.Alternate.Number);
        db.SetNullableDate(
          command, "endDate", import.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.AlternateAdd.Identifier = db.GetDateTime(reader, 0);
        entities.AlternateAdd.CspNumber = db.GetString(reader, 1);
        entities.AlternateAdd.ZdelStartDate = db.GetNullableDate(reader, 2);
        entities.AlternateAdd.Type1 = db.GetNullableString(reader, 3);
        entities.AlternateAdd.VerifiedDate = db.GetNullableDate(reader, 4);
        entities.AlternateAdd.EndDate = db.GetNullableDate(reader, 5);
        entities.AlternateAdd.Populated = true;
      });
  }

  private bool ReadFipsTribAddress()
  {
    entities.FipsTribAddress.Populated = false;

    return Read("ReadFipsTribAddress",
      (db, command) =>
      {
        db.SetNullableString(command, "cspNumber", entities.Alternate.Number);
      },
      (db, reader) =>
      {
        entities.FipsTribAddress.Identifier = db.GetInt32(reader, 0);
        entities.FipsTribAddress.FipState = db.GetNullableInt32(reader, 1);
        entities.FipsTribAddress.FipCounty = db.GetNullableInt32(reader, 2);
        entities.FipsTribAddress.FipLocation = db.GetNullableInt32(reader, 3);
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
    /// A value of Current.
    /// </summary>
    [JsonPropertyName("current")]
    public DateWorkArea Current
    {
      get => current ??= new();
      set => current = value;
    }

    /// <summary>
    /// A value of Alternate.
    /// </summary>
    [JsonPropertyName("alternate")]
    public CsePersonsWorkSet Alternate
    {
      get => alternate ??= new();
      set => alternate = value;
    }

    private DateWorkArea current;
    private CsePersonsWorkSet alternate;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of ZdelLocalCurrent.
    /// </summary>
    [JsonPropertyName("zdelLocalCurrent")]
    public DateWorkArea ZdelLocalCurrent
    {
      get => zdelLocalCurrent ??= new();
      set => zdelLocalCurrent = value;
    }

    private DateWorkArea zdelLocalCurrent;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of AlternateAdd.
    /// </summary>
    [JsonPropertyName("alternateAdd")]
    public CsePersonAddress AlternateAdd
    {
      get => alternateAdd ??= new();
      set => alternateAdd = value;
    }

    /// <summary>
    /// A value of Alternate.
    /// </summary>
    [JsonPropertyName("alternate")]
    public CsePerson Alternate
    {
      get => alternate ??= new();
      set => alternate = value;
    }

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

    private CsePersonAddress alternateAdd;
    private CsePerson alternate;
    private FipsTribAddress fipsTribAddress;
    private Fips fips;
  }
#endregion
}
