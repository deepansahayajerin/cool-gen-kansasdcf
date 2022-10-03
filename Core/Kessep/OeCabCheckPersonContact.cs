// Program: OE_CAB_CHECK_PERSON_CONTACT, ID: 371846341, model: 746.
// Short name: SWE00885
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: OE_CAB_CHECK_PERSON_CONTACT.
/// </para>
/// <para>
/// Resp: OBLGEST
/// This Common Action Block(CAB) determines if a contact exists for a 
/// CSE_PERSON based on the contact details available.
/// </para>
/// </summary>
[Serializable]
public partial class OeCabCheckPersonContact: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_CAB_CHECK_PERSON_CONTACT program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeCabCheckPersonContact(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeCabCheckPersonContact.
  /// </summary>
  public OeCabCheckPersonContact(IContext context, Import import, Export export):
    
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    MoveContact(import.Contact, export.Contact);

    if (import.Contact.ContactNumber != 0)
    {
      if (ReadContact2())
      {
        export.ContactExist.Flag = "Y";
        export.Contact.Assign(entities.Contact);
      }
      else
      {
        export.ContactExist.Flag = "N";
      }
    }
    else if (ReadContact1())
    {
      export.Contact.Assign(entities.Contact);
      export.ContactExist.Flag = "Y";
    }
    else
    {
      export.ContactExist.Flag = "N";
    }
  }

  private static void MoveContact(Contact source, Contact target)
  {
    target.ContactNumber = source.ContactNumber;
    target.NameLast = source.NameLast;
    target.NameFirst = source.NameFirst;
    target.MiddleInitial = source.MiddleInitial;
  }

  private bool ReadContact1()
  {
    entities.Contact.Populated = false;

    return Read("ReadContact1",
      (db, command) =>
      {
        db.SetNullableString(
          command, "firstName", import.Contact.NameFirst ?? "");
        db.
          SetNullableString(command, "lastName", import.Contact.NameLast ?? "");
          
        db.SetString(command, "cspNumber", import.CsePerson.Number);
      },
      (db, reader) =>
      {
        entities.Contact.CspNumber = db.GetString(reader, 0);
        entities.Contact.ContactNumber = db.GetInt32(reader, 1);
        entities.Contact.Fax = db.GetNullableInt32(reader, 2);
        entities.Contact.NameTitle = db.GetNullableString(reader, 3);
        entities.Contact.CompanyName = db.GetNullableString(reader, 4);
        entities.Contact.RelationshipToCsePerson =
          db.GetNullableString(reader, 5);
        entities.Contact.NameLast = db.GetNullableString(reader, 6);
        entities.Contact.NameFirst = db.GetNullableString(reader, 7);
        entities.Contact.MiddleInitial = db.GetNullableString(reader, 8);
        entities.Contact.HomePhone = db.GetNullableInt32(reader, 9);
        entities.Contact.WorkPhone = db.GetNullableInt32(reader, 10);
        entities.Contact.CreatedBy = db.GetString(reader, 11);
        entities.Contact.CreatedTimestamp = db.GetDateTime(reader, 12);
        entities.Contact.LastUpdatedBy = db.GetString(reader, 13);
        entities.Contact.LastUpdatedTimestamp = db.GetDateTime(reader, 14);
        entities.Contact.Populated = true;
      });
  }

  private bool ReadContact2()
  {
    entities.Contact.Populated = false;

    return Read("ReadContact2",
      (db, command) =>
      {
        db.SetInt32(command, "contactNumber", import.Contact.ContactNumber);
        db.SetString(command, "cspNumber", import.CsePerson.Number);
      },
      (db, reader) =>
      {
        entities.Contact.CspNumber = db.GetString(reader, 0);
        entities.Contact.ContactNumber = db.GetInt32(reader, 1);
        entities.Contact.Fax = db.GetNullableInt32(reader, 2);
        entities.Contact.NameTitle = db.GetNullableString(reader, 3);
        entities.Contact.CompanyName = db.GetNullableString(reader, 4);
        entities.Contact.RelationshipToCsePerson =
          db.GetNullableString(reader, 5);
        entities.Contact.NameLast = db.GetNullableString(reader, 6);
        entities.Contact.NameFirst = db.GetNullableString(reader, 7);
        entities.Contact.MiddleInitial = db.GetNullableString(reader, 8);
        entities.Contact.HomePhone = db.GetNullableInt32(reader, 9);
        entities.Contact.WorkPhone = db.GetNullableInt32(reader, 10);
        entities.Contact.CreatedBy = db.GetString(reader, 11);
        entities.Contact.CreatedTimestamp = db.GetDateTime(reader, 12);
        entities.Contact.LastUpdatedBy = db.GetString(reader, 13);
        entities.Contact.LastUpdatedTimestamp = db.GetDateTime(reader, 14);
        entities.Contact.Populated = true;
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

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
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
    /// A value of ContactExist.
    /// </summary>
    [JsonPropertyName("contactExist")]
    public Common ContactExist
    {
      get => contactExist ??= new();
      set => contactExist = value;
    }

    private Contact contact;
    private Common contactExist;
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
