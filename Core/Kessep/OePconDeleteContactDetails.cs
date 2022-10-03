// Program: OE_PCON_DELETE_CONTACT_DETAILS, ID: 371891962, model: 746.
// Short name: SWE00951
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: OE_PCON_DELETE_CONTACT_DETAILS.
/// </para>
/// <para>
/// RESP: OBLGESTB
/// This action block creates Contact details.
/// It creates CONTACT, CONTACT_ADDRESS and CONTACT_DETAIL records.
/// </para>
/// </summary>
[Serializable]
public partial class OePconDeleteContactDetails: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_PCON_DELETE_CONTACT_DETAILS program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OePconDeleteContactDetails(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OePconDeleteContactDetails.
  /// </summary>
  public OePconDeleteContactDetails(IContext context, Import import,
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
    // ---------------------------------------------
    // This action block needs only CSE_PERSON NUMBER and CONTACT CONTACT_NUMBER
    // as imports and exports nothing.
    // Cleanup import and export views after downloading with delete access.
    // ---------------------------------------------
    // *********************************************
    // SYSTEM:		KESSEP
    // MODULE:
    // MODULE TYPE:
    // DESCRIPTION:
    // This action block deletes the contact details.
    // PROCESSING:
    // This action block is passed CSE_PERSON NUMBER and CONTACT CONTACT_NUMBER.
    // It reads and delets CONTACT, CONTACT_ADDRESS and CONTACT_DETAIL records
    // for the contact.
    // ACTION BLOCKS:
    // ENTITY TYPES USED:
    // 	CSE_PERSON		- R - -
    // 	CONTACT			- R - D
    // 	CONTACT_ADDRESS		- R - D
    // 	CONTACT_DETAIL		- R - D
    // DATABASE FILES USED:
    // CREATED BY:	govindaraj
    // DATE CREATED:	01/26/95
    // *********************************************
    // *********************************************
    // MAINTENANCE LOG
    // AUTHOR	DATE	CHGRQ#	DESCRIPTION
    // govind	01/26/95	Initial coding
    // *********************************************
    if (!ReadCsePerson())
    {
      ExitState = "CSE_PERSON_NF";

      return;
    }

    if (!ReadContact())
    {
      ExitState = "CONTACT_NF";

      return;
    }

    foreach(var item in ReadContactAddress())
    {
      DeleteContactAddress();
    }

    foreach(var item in ReadContactDetail())
    {
      DeleteContactDetail();
    }

    DeleteContact();
  }

  private void DeleteContact()
  {
    Update("DeleteContact",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.ExistingContact.CspNumber);
        db.SetInt32(
          command, "contactNumber", entities.ExistingContact.ContactNumber);
      });
  }

  private void DeleteContactAddress()
  {
    Update("DeleteContactAddress",
      (db, command) =>
      {
        db.SetInt32(
          command, "conNumber", entities.ExistingContactAddress.ConNumber);
        db.SetString(
          command, "cspNumber", entities.ExistingContactAddress.CspNumber);
        db.SetDate(
          command, "effectiveDate",
          entities.ExistingContactAddress.EffectiveDate.GetValueOrDefault());
      });
  }

  private void DeleteContactDetail()
  {
    Update("DeleteContactDetail",
      (db, command) =>
      {
        db.SetInt32(
          command, "conNumber", entities.ExistingContactDetail.ConNumber);
        db.SetString(
          command, "cspNumber", entities.ExistingContactDetail.CspNumber);
        db.SetInt32(
          command, "identifier", entities.ExistingContactDetail.Identifier);
      });
  }

  private bool ReadContact()
  {
    entities.ExistingContact.Populated = false;

    return Read("ReadContact",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.ExistingCsePerson.Number);
        db.SetInt32(command, "contactNumber", import.Contact.ContactNumber);
      },
      (db, reader) =>
      {
        entities.ExistingContact.CspNumber = db.GetString(reader, 0);
        entities.ExistingContact.ContactNumber = db.GetInt32(reader, 1);
        entities.ExistingContact.Populated = true;
      });
  }

  private IEnumerable<bool> ReadContactAddress()
  {
    System.Diagnostics.Debug.Assert(entities.ExistingContact.Populated);
    entities.ExistingContactAddress.Populated = false;

    return ReadEach("ReadContactAddress",
      (db, command) =>
      {
        db.
          SetInt32(command, "conNumber", entities.ExistingContact.ContactNumber);
          
        db.SetString(command, "cspNumber", entities.ExistingContact.CspNumber);
      },
      (db, reader) =>
      {
        entities.ExistingContactAddress.ConNumber = db.GetInt32(reader, 0);
        entities.ExistingContactAddress.CspNumber = db.GetString(reader, 1);
        entities.ExistingContactAddress.EffectiveDate = db.GetDate(reader, 2);
        entities.ExistingContactAddress.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadContactDetail()
  {
    System.Diagnostics.Debug.Assert(entities.ExistingContact.Populated);
    entities.ExistingContactDetail.Populated = false;

    return ReadEach("ReadContactDetail",
      (db, command) =>
      {
        db.
          SetInt32(command, "conNumber", entities.ExistingContact.ContactNumber);
          
        db.SetString(command, "cspNumber", entities.ExistingContact.CspNumber);
      },
      (db, reader) =>
      {
        entities.ExistingContactDetail.ConNumber = db.GetInt32(reader, 0);
        entities.ExistingContactDetail.CspNumber = db.GetString(reader, 1);
        entities.ExistingContactDetail.Identifier = db.GetInt32(reader, 2);
        entities.ExistingContactDetail.Populated = true;

        return true;
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
    /// <summary>A ImportGroup group.</summary>
    [Serializable]
    public class ImportGroup
    {
      /// <summary>
      /// A value of DetailAction.
      /// </summary>
      [JsonPropertyName("detailAction")]
      public Common DetailAction
      {
        get => detailAction ??= new();
        set => detailAction = value;
      }

      /// <summary>
      /// A value of Detail.
      /// </summary>
      [JsonPropertyName("detail")]
      public ContactDetail Detail
      {
        get => detail ??= new();
        set => detail = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 5;

      private Common detailAction;
      private ContactDetail detail;
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
    /// A value of ContactAddress.
    /// </summary>
    [JsonPropertyName("contactAddress")]
    public ContactAddress ContactAddress
    {
      get => contactAddress ??= new();
      set => contactAddress = value;
    }

    /// <summary>
    /// Gets a value of Import1.
    /// </summary>
    [JsonIgnore]
    public Array<ImportGroup> Import1 => import1 ??= new(ImportGroup.Capacity);

    /// <summary>
    /// Gets a value of Import1 for json serialization.
    /// </summary>
    [JsonPropertyName("import1")]
    [Computed]
    public IList<ImportGroup> Import1_Json
    {
      get => import1;
      set => Import1.Assign(value);
    }

    private CsePerson csePerson;
    private Contact contact;
    private ContactAddress contactAddress;
    private Array<ImportGroup> import1;
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
      /// A value of DetailAction.
      /// </summary>
      [JsonPropertyName("detailAction")]
      public Common DetailAction
      {
        get => detailAction ??= new();
        set => detailAction = value;
      }

      /// <summary>
      /// A value of Detail.
      /// </summary>
      [JsonPropertyName("detail")]
      public ContactDetail Detail
      {
        get => detail ??= new();
        set => detail = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 5;

      private Common detailAction;
      private ContactDetail detail;
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
    /// A value of ContactAddress.
    /// </summary>
    [JsonPropertyName("contactAddress")]
    public ContactAddress ContactAddress
    {
      get => contactAddress ??= new();
      set => contactAddress = value;
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

    private CsePerson csePerson;
    private Contact contact;
    private ContactAddress contactAddress;
    private Array<ExportGroup> export1;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of ExistingContactDetail.
    /// </summary>
    [JsonPropertyName("existingContactDetail")]
    public ContactDetail ExistingContactDetail
    {
      get => existingContactDetail ??= new();
      set => existingContactDetail = value;
    }

    /// <summary>
    /// A value of ExistingContactAddress.
    /// </summary>
    [JsonPropertyName("existingContactAddress")]
    public ContactAddress ExistingContactAddress
    {
      get => existingContactAddress ??= new();
      set => existingContactAddress = value;
    }

    /// <summary>
    /// A value of ExistingContact.
    /// </summary>
    [JsonPropertyName("existingContact")]
    public Contact ExistingContact
    {
      get => existingContact ??= new();
      set => existingContact = value;
    }

    /// <summary>
    /// A value of ExistingCsePerson.
    /// </summary>
    [JsonPropertyName("existingCsePerson")]
    public CsePerson ExistingCsePerson
    {
      get => existingCsePerson ??= new();
      set => existingCsePerson = value;
    }

    private ContactDetail existingContactDetail;
    private ContactAddress existingContactAddress;
    private Contact existingContact;
    private CsePerson existingCsePerson;
  }
#endregion
}
