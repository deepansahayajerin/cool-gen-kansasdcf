// Program: CAB_FN_READ_CSE_PERSON_ADDRESS, ID: 372305194, model: 746.
// Short name: SWE00050
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: CAB_FN_READ_CSE_PERSON_ADDRESS.
/// </summary>
[Serializable]
public partial class CabFnReadCsePersonAddress: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the CAB_FN_READ_CSE_PERSON_ADDRESS program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new CabFnReadCsePersonAddress(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of CabFnReadCsePersonAddress.
  /// </summary>
  public CabFnReadCsePersonAddress(IContext context, Import import,
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
    if (ReadCsePersonCsePersonAddress())
    {
      export.CsePersonAddress.Assign(entities.CsePersonAddress);
    }
    else
    {
      ExitState = "CSE_PERSON_ADDRESS_NF";
    }
  }

  private bool ReadCsePersonCsePersonAddress()
  {
    entities.CsePersonAddress.Populated = false;
    entities.CsePerson.Populated = false;

    return Read("ReadCsePersonCsePersonAddress",
      (db, command) =>
      {
        db.SetString(command, "numb", import.CsePersonsWorkSet.Number);
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CsePersonAddress.CspNumber = db.GetString(reader, 0);
        entities.CsePerson.Type1 = db.GetString(reader, 1);
        entities.CsePerson.TaxId = db.GetNullableString(reader, 2);
        entities.CsePersonAddress.Identifier = db.GetDateTime(reader, 3);
        entities.CsePersonAddress.Street1 = db.GetNullableString(reader, 4);
        entities.CsePersonAddress.Street2 = db.GetNullableString(reader, 5);
        entities.CsePersonAddress.City = db.GetNullableString(reader, 6);
        entities.CsePersonAddress.State = db.GetNullableString(reader, 7);
        entities.CsePersonAddress.ZipCode = db.GetNullableString(reader, 8);
        entities.CsePersonAddress.Zip4 = db.GetNullableString(reader, 9);
        entities.CsePersonAddress.Zip3 = db.GetNullableString(reader, 10);
        entities.CsePersonAddress.LocationType = db.GetString(reader, 11);
        entities.CsePersonAddress.Populated = true;
        entities.CsePerson.Populated = true;
        CheckValid<CsePerson>("Type1", entities.CsePerson.Type1);
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
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
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

    private CsePersonAddress csePersonAddress;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
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
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
    }

    private CsePersonAddress csePersonAddress;
    private CsePerson csePerson;
  }
#endregion
}
