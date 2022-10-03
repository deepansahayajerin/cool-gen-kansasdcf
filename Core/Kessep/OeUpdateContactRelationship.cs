// Program: OE_UPDATE_CONTACT_RELATIONSHIP, ID: 371180790, model: 746.
// Short name: SWE02004
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: OE_UPDATE_CONTACT_RELATIONSHIP.
/// </summary>
[Serializable]
public partial class OeUpdateContactRelationship: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_UPDATE_CONTACT_RELATIONSHIP program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeUpdateContactRelationship(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeUpdateContactRelationship.
  /// </summary>
  public OeUpdateContactRelationship(IContext context, Import import,
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
    if (ReadContact())
    {
      try
      {
        UpdateContact();
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "CONTACT_NU";

            break;
          case ErrorCode.PermittedValueViolation:
            ExitState = "CONTACT_PV";

            break;
          case ErrorCode.DatabaseError:
            break;
          default:
            throw;
        }
      }
    }
    else
    {
      ExitState = "CONTACT_NF";
    }
  }

  private bool ReadContact()
  {
    entities.Contact.Populated = false;

    return Read("ReadContact",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", import.CsePerson.Number);
        db.SetInt32(command, "contactNumber", import.Contact.ContactNumber);
      },
      (db, reader) =>
      {
        entities.Contact.CspNumber = db.GetString(reader, 0);
        entities.Contact.ContactNumber = db.GetInt32(reader, 1);
        entities.Contact.RelationshipToCsePerson =
          db.GetNullableString(reader, 2);
        entities.Contact.LastUpdatedBy = db.GetString(reader, 3);
        entities.Contact.LastUpdatedTimestamp = db.GetDateTime(reader, 4);
        entities.Contact.Populated = true;
      });
  }

  private void UpdateContact()
  {
    System.Diagnostics.Debug.Assert(entities.Contact.Populated);

    var relationshipToCsePerson = import.Contact.RelationshipToCsePerson ?? "";
    var lastUpdatedBy = global.UserId;
    var lastUpdatedTimestamp = Now();

    entities.Contact.Populated = false;
    Update("UpdateContact",
      (db, command) =>
      {
        db.
          SetNullableString(command, "relToCsePerson", relationshipToCsePerson);
          
        db.SetString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetDateTime(command, "lastUpdatedTmst", lastUpdatedTimestamp);
        db.SetString(command, "cspNumber", entities.Contact.CspNumber);
        db.SetInt32(command, "contactNumber", entities.Contact.ContactNumber);
      });

    entities.Contact.RelationshipToCsePerson = relationshipToCsePerson;
    entities.Contact.LastUpdatedBy = lastUpdatedBy;
    entities.Contact.LastUpdatedTimestamp = lastUpdatedTimestamp;
    entities.Contact.Populated = true;
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
    /// A value of Contact.
    /// </summary>
    [JsonPropertyName("contact")]
    public Contact Contact
    {
      get => contact ??= new();
      set => contact = value;
    }

    private CsePerson csePerson;
    private Contact contact;
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
    /// A value of Contact.
    /// </summary>
    [JsonPropertyName("contact")]
    public Contact Contact
    {
      get => contact ??= new();
      set => contact = value;
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

    private Contact contact;
    private CsePerson csePerson;
  }
#endregion
}
