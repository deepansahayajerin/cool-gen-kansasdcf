// Program: SI_CHECK_FOR_DUPLICATE_ADDRESS, ID: 371727803, model: 746.
// Short name: SWE01927
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: SI_CHECK_FOR_DUPLICATE_ADDRESS.
/// </para>
/// <para>
/// RESP: SRVINIT
/// This PAD creates an address for a given CSE Person
/// </para>
/// </summary>
[Serializable]
public partial class SiCheckForDuplicateAddress: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_CHECK_FOR_DUPLICATE_ADDRESS program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiCheckForDuplicateAddress(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiCheckForDuplicateAddress.
  /// </summary>
  public SiCheckForDuplicateAddress(IContext context, Import import,
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
    // ------------------------------------------------------------
    //                 M A I N T E N A N C E   L O G
    //   Date	  Developer		Description
    // 07/23/97  Sid		Check if a duplicate address already exists for the cse 
    // person.
    // ------------------------------------------------------------
    // 12/09/98  W. Campbell      Removed any code
    //                            which referenced
    //                            CSE_PERSON_ADDRESS
    //                            attributes: START_DATE and
    //                            VERIFIED_CODE.  This work
    //                            was done on IDCR 454.
    // ---------------------------------------------
    // 09/14/99 W.Campbell        Changed both of the READ
    //                            statements in this CAB.
    //                            One was changed to Select Only,
    //                            and the other was changed
    //                            back to the default condition of
    //                            both Cursor and Select, as it
    //                            should have never been made
    //                            a Select Only.  This was done to
    //                            fix the problem reported on
    //                            PR# H 00073561.
    // ---------------------------------------------
    // ---------------------------------------------
    // 09/14/99 W.Campbell - Changed the following
    // READ to Select Only.
    // ---------------------------------------------
    if (!ReadCsePerson())
    {
      // ***	Should not happen	***
      ExitState = "CSE_PERSON_NF";

      return;
    }

    // ---------------------------------------------
    // 09/14/99 W.Campbell - Changed the following
    // READ back to the default condition of both
    // Cursor and Select, as it should have never
    // been made a Select Only.  This was done to
    // fix the problem reported on PR# H 00073561.
    // ---------------------------------------------
    if (ReadCsePersonAddress())
    {
      export.DuplicateAddress.Flag = "Y";
    }
    else
    {
      export.DuplicateAddress.Flag = "N";
    }
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

  private bool ReadCsePersonAddress()
  {
    entities.CsePersonAddress.Populated = false;

    return Read("ReadCsePersonAddress",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.CsePerson.Number);
        db.SetNullableString(
          command, "type", import.CsePersonAddress.Type1 ?? "");
        db.SetString(
          command, "locationType", import.CsePersonAddress.LocationType);
        db.SetNullableString(
          command, "street1", import.CsePersonAddress.Street1 ?? "");
        db.
          SetNullableString(command, "city", import.CsePersonAddress.City ?? "");
          
        db.SetNullableString(
          command, "state", import.CsePersonAddress.State ?? "");
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
        entities.CsePersonAddress.CreatedTimestamp =
          db.GetNullableDateTime(reader, 16);
        entities.CsePersonAddress.CreatedBy = db.GetNullableString(reader, 17);
        entities.CsePersonAddress.State = db.GetNullableString(reader, 18);
        entities.CsePersonAddress.ZipCode = db.GetNullableString(reader, 19);
        entities.CsePersonAddress.Zip4 = db.GetNullableString(reader, 20);
        entities.CsePersonAddress.Zip3 = db.GetNullableString(reader, 21);
        entities.CsePersonAddress.Street3 = db.GetNullableString(reader, 22);
        entities.CsePersonAddress.Street4 = db.GetNullableString(reader, 23);
        entities.CsePersonAddress.Province = db.GetNullableString(reader, 24);
        entities.CsePersonAddress.PostalCode = db.GetNullableString(reader, 25);
        entities.CsePersonAddress.Country = db.GetNullableString(reader, 26);
        entities.CsePersonAddress.LocationType = db.GetString(reader, 27);
        entities.CsePersonAddress.County = db.GetNullableString(reader, 28);
        entities.CsePersonAddress.Populated = true;
        CheckValid<CsePersonAddress>("LocationType",
          entities.CsePersonAddress.LocationType);
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
    /// A value of DuplicateAddress.
    /// </summary>
    [JsonPropertyName("duplicateAddress")]
    public Common DuplicateAddress
    {
      get => duplicateAddress ??= new();
      set => duplicateAddress = value;
    }

    private Common duplicateAddress;
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
