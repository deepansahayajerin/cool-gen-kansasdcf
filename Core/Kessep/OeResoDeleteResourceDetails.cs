// Program: OE_RESO_DELETE_RESOURCE_DETAILS, ID: 371818043, model: 746.
// Short name: SWE00961
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: OE_RESO_DELETE_RESOURCE_DETAILS.
/// </para>
/// <para>
/// RESP: OBLGESTB		
/// Update existing person resource and address for resource location and/or 
/// lien holder.
/// </para>
/// </summary>
[Serializable]
public partial class OeResoDeleteResourceDetails: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_RESO_DELETE_RESOURCE_DETAILS program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeResoDeleteResourceDetails(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeResoDeleteResourceDetails.
  /// </summary>
  public OeResoDeleteResourceDetails(IContext context, Import import,
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
    if (!ReadCsePerson())
    {
      ExitState = "CSE_PERSON_NF";

      return;
    }

    if (!ReadCsePersonResource())
    {
      ExitState = "CSE_PERSON_RESOURCE_NF";

      return;
    }

    // --------------------------------------------------
    // CSE_PERSON_RESOURCE is set for cascade delete so
    // all associations will be removed and all addresses
    // for lien holder and location will be deleted with
    // the one delete of resource.
    // --------------------------------------------------
    DeleteCsePersonResource();
  }

  private void DeleteCsePersonResource()
  {
    Update("DeleteCsePersonResource#1",
      (db, command) =>
      {
        db.SetNullableString(
          command, "cspCNumber", entities.ExistingCsePersonResource.CspNumber);
        db.SetNullableInt32(
          command, "cprCResourceNo",
          entities.ExistingCsePersonResource.ResourceNo);
      });

    Update("DeleteCsePersonResource#2",
      (db, command) =>
      {
        db.SetNullableString(
          command, "cspCNumber", entities.ExistingCsePersonResource.CspNumber);
        db.SetNullableInt32(
          command, "cprCResourceNo",
          entities.ExistingCsePersonResource.ResourceNo);
      });
  }

  private bool ReadCsePerson()
  {
    entities.ExistingCsePerson.Populated = false;

    return Read("ReadCsePerson",
      (db, command) =>
      {
        db.SetString(command, "numb", import.CsePerson.Number);
      },
      (db, reader) =>
      {
        entities.ExistingCsePerson.Number = db.GetString(reader, 0);
        entities.ExistingCsePerson.Populated = true;
      });
  }

  private bool ReadCsePersonResource()
  {
    entities.ExistingCsePersonResource.Populated = false;

    return Read("ReadCsePersonResource",
      (db, command) =>
      {
        db.SetInt32(command, "resourceNo", import.CsePersonResource.ResourceNo);
        db.SetString(command, "cspNumber", entities.ExistingCsePerson.Number);
      },
      (db, reader) =>
      {
        entities.ExistingCsePersonResource.CspNumber = db.GetString(reader, 0);
        entities.ExistingCsePersonResource.ResourceNo = db.GetInt32(reader, 1);
        entities.ExistingCsePersonResource.Populated = true;
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
    /// A value of CsePersonResource.
    /// </summary>
    [JsonPropertyName("csePersonResource")]
    public CsePersonResource CsePersonResource
    {
      get => csePersonResource ??= new();
      set => csePersonResource = value;
    }

    private CsePerson csePerson;
    private CsePersonResource csePersonResource;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of ExistingCsePerson.
    /// </summary>
    [JsonPropertyName("existingCsePerson")]
    public CsePerson ExistingCsePerson
    {
      get => existingCsePerson ??= new();
      set => existingCsePerson = value;
    }

    /// <summary>
    /// A value of ExistingCsePersonResource.
    /// </summary>
    [JsonPropertyName("existingCsePersonResource")]
    public CsePersonResource ExistingCsePersonResource
    {
      get => existingCsePersonResource ??= new();
      set => existingCsePersonResource = value;
    }

    private CsePerson existingCsePerson;
    private CsePersonResource existingCsePersonResource;
  }
#endregion
}
