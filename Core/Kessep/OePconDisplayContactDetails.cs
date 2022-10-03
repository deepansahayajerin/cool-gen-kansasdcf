// Program: OE_PCON_DISPLAY_CONTACT_DETAILS, ID: 371891970, model: 746.
// Short name: SWE00952
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
/// A program: OE_PCON_DISPLAY_CONTACT_DETAILS.
/// </para>
/// <para>
/// RESP: OBLGESTB
/// This action block reads and populates the contact details.
/// </para>
/// </summary>
[Serializable]
public partial class OePconDisplayContactDetails: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_PCON_DISPLAY_CONTACT_DETAILS program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OePconDisplayContactDetails(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OePconDisplayContactDetails.
  /// </summary>
  public OePconDisplayContactDetails(IContext context, Import import,
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
    // *********************************************
    // SYSTEM:		KESSEP
    // MODULE:
    // MODULE TYPE:
    // DESCRIPTION:
    // This action block populates contact details for the given contact.
    // PROCESSING:
    // This action block is passed CSE_PERSON NUMBER and CONTACT CONTACT_NUMBER.
    // It reads CONTACT, latest CONTACT_ADDRESS and all the CONTACT_DETAIL
    // records and populates export views.
    // ACTION BLOCKS:
    // ENTITY TYPES USED:
    // 	CSE_PERSON		- R - -
    // 	CONTACT			- R - -
    // 	CONTACT_DETAIL		- R - -
    // 	CONTACT_ADDRESS		- R - -
    // DATABASE FILES USED:
    // CREATED BY:	govindaraj
    // DATE CREATED:	01/26/95
    // *********************************************
    // *********************************************
    // MAINTENANCE LOG
    // AUTHOR	DATE	CHGRQ#	DESCRIPTION
    // govind	01/26/95	Initial coding
    // *********************************************
    export.CsePerson.Number = import.CsePerson.Number;
    export.Contact.ContactNumber = import.Contact.ContactNumber;
    export.ScrollingAttributes.MinusFlag = "";
    export.ScrollingAttributes.PlusFlag = "";
    export.ContactDisplayed.Flag = "N";
    export.RespForHealthInsurance.Flag = "N";

    if (ReadCsePerson())
    {
      export.CsePerson.Number = entities.ExistingCsePerson.Number;
      export.WorkSet.Number = entities.ExistingCsePerson.Number;
      UseSiReadCsePerson();
    }
    else
    {
      ExitState = "CSE_PERSON_NF";

      return;
    }

    switch(TrimEnd(import.UserAction.Command))
    {
      case "DISPLAY":
        local.ContactFound.Flag = "N";

        if (ReadContact4())
        {
          export.Contact.Assign(entities.ExistingContact);
          local.ContactFound.Flag = "Y";
        }

        if (AsChar(local.ContactFound.Flag) == 'N')
        {
          ExitState = "CONTACT_NF";

          return;
        }

        break;
      case "PREV":
        local.ContactFound.Flag = "N";

        if (ReadContact3())
        {
          export.Contact.Assign(entities.ExistingContact);
          local.ContactFound.Flag = "Y";
        }

        if (AsChar(local.ContactFound.Flag) == 'N')
        {
          ExitState = "OE0098_NO_MORE_CONTACT";
        }

        break;
      case "NEXT":
        local.ContactFound.Flag = "N";

        if (ReadContact2())
        {
          export.Contact.Assign(entities.ExistingContact);
          local.ContactFound.Flag = "Y";
        }

        if (AsChar(local.ContactFound.Flag) == 'N')
        {
          ExitState = "OE0098_NO_MORE_CONTACT";
        }

        break;
      default:
        ExitState = "ACO_NE0000_INVALID_COMMAND";

        return;
    }

    if (AsChar(local.ContactFound.Flag) == 'Y')
    {
      export.ContactDisplayed.Flag = "Y";

      if (ReadContact1())
      {
        export.UpdatedStamps.LastUpdatedBy = entities.ExistingContact.CreatedBy;
        export.UpdatedStamps.LastUpdatedTimestamp =
          entities.ExistingContact.CreatedTimestamp;

        if (Lt(export.UpdatedStamps.LastUpdatedTimestamp,
          entities.ExistingContact.LastUpdatedTimestamp))
        {
          export.UpdatedStamps.LastUpdatedBy =
            entities.ExistingContact.LastUpdatedBy;
          export.UpdatedStamps.LastUpdatedTimestamp =
            entities.ExistingContact.LastUpdatedTimestamp;
        }

        if (ReadContactAddress())
        {
          export.ContactAddress.Assign(entities.ExistingContactAddress);

          if (Lt(export.UpdatedStamps.LastUpdatedTimestamp,
            entities.ExistingContactAddress.CreatedTimestamp))
          {
            export.UpdatedStamps.LastUpdatedBy =
              entities.ExistingContactAddress.CreatedBy;
            export.UpdatedStamps.LastUpdatedTimestamp =
              entities.ExistingContactAddress.CreatedTimestamp;
          }

          if (Lt(export.UpdatedStamps.LastUpdatedTimestamp,
            entities.ExistingContactAddress.LastUpdatedTimestamp))
          {
            export.UpdatedStamps.LastUpdatedBy =
              entities.ExistingContactAddress.LastUpdatedBy;
            export.UpdatedStamps.LastUpdatedTimestamp =
              entities.ExistingContactAddress.LastUpdatedTimestamp;
          }
        }

        export.Export1.Index = 0;
        export.Export1.Clear();

        foreach(var item in ReadContactDetail())
        {
          export.Export1.Update.Detail.Assign(entities.ExistingContactDetail);
          export.Export1.Update.DetailAction.ActionEntry = "";

          if (Lt(export.UpdatedStamps.LastUpdatedTimestamp,
            entities.ExistingContactDetail.CreatedTimestamp))
          {
            export.UpdatedStamps.LastUpdatedBy =
              entities.ExistingContactDetail.CreatedBy;
            export.UpdatedStamps.LastUpdatedTimestamp =
              entities.ExistingContactDetail.CreatedTimestamp;
          }

          if (Lt(export.UpdatedStamps.LastUpdatedTimestamp,
            entities.ExistingContactDetail.LastUpdatedTimestamp))
          {
            export.UpdatedStamps.LastUpdatedBy =
              entities.ExistingContactDetail.LastUpdatedBy;
            export.UpdatedStamps.LastUpdatedTimestamp =
              entities.ExistingContactDetail.LastUpdatedTimestamp;
          }

          export.Export1.Next();
        }

        if (ReadHealthInsuranceCoverage())
        {
          export.RespForHealthInsurance.Flag = "Y";
        }
      }
    }

    if (ReadContact5())
    {
      export.ScrollingAttributes.PlusFlag = "+";
    }

    if (ReadContact6())
    {
      export.ScrollingAttributes.MinusFlag = "-";
    }
  }

  private void UseSiReadCsePerson()
  {
    var useImport = new SiReadCsePerson.Import();
    var useExport = new SiReadCsePerson.Export();

    useImport.CsePersonsWorkSet.Number = export.WorkSet.Number;

    Call(SiReadCsePerson.Execute, useImport, useExport);

    export.WorkSet.Assign(useExport.CsePersonsWorkSet);
  }

  private bool ReadContact1()
  {
    entities.ExistingContact.Populated = false;

    return Read("ReadContact1",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.ExistingCsePerson.Number);
        db.SetInt32(command, "contactNumber", export.Contact.ContactNumber);
      },
      (db, reader) =>
      {
        entities.ExistingContact.CspNumber = db.GetString(reader, 0);
        entities.ExistingContact.ContactNumber = db.GetInt32(reader, 1);
        entities.ExistingContact.Fax = db.GetNullableInt32(reader, 2);
        entities.ExistingContact.NameTitle = db.GetNullableString(reader, 3);
        entities.ExistingContact.CompanyName = db.GetNullableString(reader, 4);
        entities.ExistingContact.RelationshipToCsePerson =
          db.GetNullableString(reader, 5);
        entities.ExistingContact.NameLast = db.GetNullableString(reader, 6);
        entities.ExistingContact.NameFirst = db.GetNullableString(reader, 7);
        entities.ExistingContact.MiddleInitial =
          db.GetNullableString(reader, 8);
        entities.ExistingContact.HomePhone = db.GetNullableInt32(reader, 9);
        entities.ExistingContact.WorkPhone = db.GetNullableInt32(reader, 10);
        entities.ExistingContact.CreatedBy = db.GetString(reader, 11);
        entities.ExistingContact.CreatedTimestamp = db.GetDateTime(reader, 12);
        entities.ExistingContact.LastUpdatedBy = db.GetString(reader, 13);
        entities.ExistingContact.LastUpdatedTimestamp =
          db.GetDateTime(reader, 14);
        entities.ExistingContact.WorkPhoneExt =
          db.GetNullableString(reader, 15);
        entities.ExistingContact.FaxExt = db.GetNullableString(reader, 16);
        entities.ExistingContact.WorkPhoneAreaCode =
          db.GetNullableInt32(reader, 17);
        entities.ExistingContact.HomePhoneAreaCode =
          db.GetNullableInt32(reader, 18);
        entities.ExistingContact.FaxAreaCode = db.GetNullableInt32(reader, 19);
        entities.ExistingContact.VerifiedDate = db.GetNullableDate(reader, 20);
        entities.ExistingContact.VerifiedUserId =
          db.GetNullableString(reader, 21);
        entities.ExistingContact.Populated = true;
      });
  }

  private bool ReadContact2()
  {
    entities.ExistingContact.Populated = false;

    return Read("ReadContact2",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.ExistingCsePerson.Number);
        db.SetInt32(command, "contactNumber", import.Contact.ContactNumber);
      },
      (db, reader) =>
      {
        entities.ExistingContact.CspNumber = db.GetString(reader, 0);
        entities.ExistingContact.ContactNumber = db.GetInt32(reader, 1);
        entities.ExistingContact.Fax = db.GetNullableInt32(reader, 2);
        entities.ExistingContact.NameTitle = db.GetNullableString(reader, 3);
        entities.ExistingContact.CompanyName = db.GetNullableString(reader, 4);
        entities.ExistingContact.RelationshipToCsePerson =
          db.GetNullableString(reader, 5);
        entities.ExistingContact.NameLast = db.GetNullableString(reader, 6);
        entities.ExistingContact.NameFirst = db.GetNullableString(reader, 7);
        entities.ExistingContact.MiddleInitial =
          db.GetNullableString(reader, 8);
        entities.ExistingContact.HomePhone = db.GetNullableInt32(reader, 9);
        entities.ExistingContact.WorkPhone = db.GetNullableInt32(reader, 10);
        entities.ExistingContact.CreatedBy = db.GetString(reader, 11);
        entities.ExistingContact.CreatedTimestamp = db.GetDateTime(reader, 12);
        entities.ExistingContact.LastUpdatedBy = db.GetString(reader, 13);
        entities.ExistingContact.LastUpdatedTimestamp =
          db.GetDateTime(reader, 14);
        entities.ExistingContact.WorkPhoneExt =
          db.GetNullableString(reader, 15);
        entities.ExistingContact.FaxExt = db.GetNullableString(reader, 16);
        entities.ExistingContact.WorkPhoneAreaCode =
          db.GetNullableInt32(reader, 17);
        entities.ExistingContact.HomePhoneAreaCode =
          db.GetNullableInt32(reader, 18);
        entities.ExistingContact.FaxAreaCode = db.GetNullableInt32(reader, 19);
        entities.ExistingContact.VerifiedDate = db.GetNullableDate(reader, 20);
        entities.ExistingContact.VerifiedUserId =
          db.GetNullableString(reader, 21);
        entities.ExistingContact.Populated = true;
      });
  }

  private bool ReadContact3()
  {
    entities.ExistingContact.Populated = false;

    return Read("ReadContact3",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.ExistingCsePerson.Number);
        db.SetInt32(command, "contactNumber", import.Contact.ContactNumber);
      },
      (db, reader) =>
      {
        entities.ExistingContact.CspNumber = db.GetString(reader, 0);
        entities.ExistingContact.ContactNumber = db.GetInt32(reader, 1);
        entities.ExistingContact.Fax = db.GetNullableInt32(reader, 2);
        entities.ExistingContact.NameTitle = db.GetNullableString(reader, 3);
        entities.ExistingContact.CompanyName = db.GetNullableString(reader, 4);
        entities.ExistingContact.RelationshipToCsePerson =
          db.GetNullableString(reader, 5);
        entities.ExistingContact.NameLast = db.GetNullableString(reader, 6);
        entities.ExistingContact.NameFirst = db.GetNullableString(reader, 7);
        entities.ExistingContact.MiddleInitial =
          db.GetNullableString(reader, 8);
        entities.ExistingContact.HomePhone = db.GetNullableInt32(reader, 9);
        entities.ExistingContact.WorkPhone = db.GetNullableInt32(reader, 10);
        entities.ExistingContact.CreatedBy = db.GetString(reader, 11);
        entities.ExistingContact.CreatedTimestamp = db.GetDateTime(reader, 12);
        entities.ExistingContact.LastUpdatedBy = db.GetString(reader, 13);
        entities.ExistingContact.LastUpdatedTimestamp =
          db.GetDateTime(reader, 14);
        entities.ExistingContact.WorkPhoneExt =
          db.GetNullableString(reader, 15);
        entities.ExistingContact.FaxExt = db.GetNullableString(reader, 16);
        entities.ExistingContact.WorkPhoneAreaCode =
          db.GetNullableInt32(reader, 17);
        entities.ExistingContact.HomePhoneAreaCode =
          db.GetNullableInt32(reader, 18);
        entities.ExistingContact.FaxAreaCode = db.GetNullableInt32(reader, 19);
        entities.ExistingContact.VerifiedDate = db.GetNullableDate(reader, 20);
        entities.ExistingContact.VerifiedUserId =
          db.GetNullableString(reader, 21);
        entities.ExistingContact.Populated = true;
      });
  }

  private bool ReadContact4()
  {
    entities.ExistingContact.Populated = false;

    return Read("ReadContact4",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.ExistingCsePerson.Number);
        db.SetInt32(command, "contactNumber", import.Contact.ContactNumber);
      },
      (db, reader) =>
      {
        entities.ExistingContact.CspNumber = db.GetString(reader, 0);
        entities.ExistingContact.ContactNumber = db.GetInt32(reader, 1);
        entities.ExistingContact.Fax = db.GetNullableInt32(reader, 2);
        entities.ExistingContact.NameTitle = db.GetNullableString(reader, 3);
        entities.ExistingContact.CompanyName = db.GetNullableString(reader, 4);
        entities.ExistingContact.RelationshipToCsePerson =
          db.GetNullableString(reader, 5);
        entities.ExistingContact.NameLast = db.GetNullableString(reader, 6);
        entities.ExistingContact.NameFirst = db.GetNullableString(reader, 7);
        entities.ExistingContact.MiddleInitial =
          db.GetNullableString(reader, 8);
        entities.ExistingContact.HomePhone = db.GetNullableInt32(reader, 9);
        entities.ExistingContact.WorkPhone = db.GetNullableInt32(reader, 10);
        entities.ExistingContact.CreatedBy = db.GetString(reader, 11);
        entities.ExistingContact.CreatedTimestamp = db.GetDateTime(reader, 12);
        entities.ExistingContact.LastUpdatedBy = db.GetString(reader, 13);
        entities.ExistingContact.LastUpdatedTimestamp =
          db.GetDateTime(reader, 14);
        entities.ExistingContact.WorkPhoneExt =
          db.GetNullableString(reader, 15);
        entities.ExistingContact.FaxExt = db.GetNullableString(reader, 16);
        entities.ExistingContact.WorkPhoneAreaCode =
          db.GetNullableInt32(reader, 17);
        entities.ExistingContact.HomePhoneAreaCode =
          db.GetNullableInt32(reader, 18);
        entities.ExistingContact.FaxAreaCode = db.GetNullableInt32(reader, 19);
        entities.ExistingContact.VerifiedDate = db.GetNullableDate(reader, 20);
        entities.ExistingContact.VerifiedUserId =
          db.GetNullableString(reader, 21);
        entities.ExistingContact.Populated = true;
      });
  }

  private bool ReadContact5()
  {
    entities.ExistingNext.Populated = false;

    return Read("ReadContact5",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.ExistingCsePerson.Number);
        db.SetInt32(command, "contactNumber", export.Contact.ContactNumber);
      },
      (db, reader) =>
      {
        entities.ExistingNext.CspNumber = db.GetString(reader, 0);
        entities.ExistingNext.ContactNumber = db.GetInt32(reader, 1);
        entities.ExistingNext.Populated = true;
      });
  }

  private bool ReadContact6()
  {
    entities.ExistingPrevious.Populated = false;

    return Read("ReadContact6",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.ExistingCsePerson.Number);
        db.SetInt32(command, "contactNumber", export.Contact.ContactNumber);
      },
      (db, reader) =>
      {
        entities.ExistingPrevious.CspNumber = db.GetString(reader, 0);
        entities.ExistingPrevious.ContactNumber = db.GetInt32(reader, 1);
        entities.ExistingPrevious.Populated = true;
      });
  }

  private bool ReadContactAddress()
  {
    System.Diagnostics.Debug.Assert(entities.ExistingContact.Populated);
    entities.ExistingContactAddress.Populated = false;

    return Read("ReadContactAddress",
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
        entities.ExistingContactAddress.Street1 =
          db.GetNullableString(reader, 3);
        entities.ExistingContactAddress.Street2 =
          db.GetNullableString(reader, 4);
        entities.ExistingContactAddress.City = db.GetNullableString(reader, 5);
        entities.ExistingContactAddress.State = db.GetNullableString(reader, 6);
        entities.ExistingContactAddress.Province =
          db.GetNullableString(reader, 7);
        entities.ExistingContactAddress.PostalCode =
          db.GetNullableString(reader, 8);
        entities.ExistingContactAddress.ZipCode5 =
          db.GetNullableString(reader, 9);
        entities.ExistingContactAddress.ZipCode4 =
          db.GetNullableString(reader, 10);
        entities.ExistingContactAddress.Zip3 = db.GetNullableString(reader, 11);
        entities.ExistingContactAddress.Country =
          db.GetNullableString(reader, 12);
        entities.ExistingContactAddress.AddressType =
          db.GetNullableString(reader, 13);
        entities.ExistingContactAddress.CreatedBy = db.GetString(reader, 14);
        entities.ExistingContactAddress.CreatedTimestamp =
          db.GetDateTime(reader, 15);
        entities.ExistingContactAddress.LastUpdatedBy =
          db.GetString(reader, 16);
        entities.ExistingContactAddress.LastUpdatedTimestamp =
          db.GetDateTime(reader, 17);
        entities.ExistingContactAddress.Populated = true;
      });
  }

  private IEnumerable<bool> ReadContactDetail()
  {
    System.Diagnostics.Debug.Assert(entities.ExistingContact.Populated);

    return ReadEach("ReadContactDetail",
      (db, command) =>
      {
        db.
          SetInt32(command, "conNumber", entities.ExistingContact.ContactNumber);
          
        db.SetString(command, "cspNumber", entities.ExistingContact.CspNumber);
      },
      (db, reader) =>
      {
        if (export.Export1.IsFull)
        {
          return false;
        }

        entities.ExistingContactDetail.ConNumber = db.GetInt32(reader, 0);
        entities.ExistingContactDetail.CspNumber = db.GetString(reader, 1);
        entities.ExistingContactDetail.Identifier = db.GetInt32(reader, 2);
        entities.ExistingContactDetail.ContactTime =
          db.GetNullableTimeSpan(reader, 3);
        entities.ExistingContactDetail.ContactDate =
          db.GetNullableDate(reader, 4);
        entities.ExistingContactDetail.ContactedUserid =
          db.GetNullableString(reader, 5);
        entities.ExistingContactDetail.CreatedBy = db.GetString(reader, 6);
        entities.ExistingContactDetail.CreatedTimestamp =
          db.GetDateTime(reader, 7);
        entities.ExistingContactDetail.LastUpdatedBy = db.GetString(reader, 8);
        entities.ExistingContactDetail.LastUpdatedTimestamp =
          db.GetDateTime(reader, 9);
        entities.ExistingContactDetail.Note = db.GetNullableString(reader, 10);
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

  private bool ReadHealthInsuranceCoverage()
  {
    System.Diagnostics.Debug.Assert(entities.ExistingContact.Populated);
    entities.HealthInsuranceCoverage.Populated = false;

    return Read("ReadHealthInsuranceCoverage",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "conHNumber", entities.ExistingContact.ContactNumber);
        db.SetNullableString(
          command, "cspHNumber", entities.ExistingContact.CspNumber);
      },
      (db, reader) =>
      {
        entities.HealthInsuranceCoverage.Identifier = db.GetInt64(reader, 0);
        entities.HealthInsuranceCoverage.CspHNumber =
          db.GetNullableString(reader, 1);
        entities.HealthInsuranceCoverage.ConHNumber =
          db.GetNullableInt32(reader, 2);
        entities.HealthInsuranceCoverage.Populated = true;
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
    /// A value of Starting.
    /// </summary>
    [JsonPropertyName("starting")]
    public ContactDetail Starting
    {
      get => starting ??= new();
      set => starting = value;
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
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
    }

    /// <summary>
    /// A value of UserAction.
    /// </summary>
    [JsonPropertyName("userAction")]
    public Common UserAction
    {
      get => userAction ??= new();
      set => userAction = value;
    }

    private ContactDetail starting;
    private Contact contact;
    private CsePerson csePerson;
    private Common userAction;
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
    /// A value of RespForHealthInsurance.
    /// </summary>
    [JsonPropertyName("respForHealthInsurance")]
    public Common RespForHealthInsurance
    {
      get => respForHealthInsurance ??= new();
      set => respForHealthInsurance = value;
    }

    /// <summary>
    /// A value of ContactDisplayed.
    /// </summary>
    [JsonPropertyName("contactDisplayed")]
    public Common ContactDisplayed
    {
      get => contactDisplayed ??= new();
      set => contactDisplayed = value;
    }

    /// <summary>
    /// A value of UpdatedStamps.
    /// </summary>
    [JsonPropertyName("updatedStamps")]
    public Contact UpdatedStamps
    {
      get => updatedStamps ??= new();
      set => updatedStamps = value;
    }

    /// <summary>
    /// A value of ScrollingAttributes.
    /// </summary>
    [JsonPropertyName("scrollingAttributes")]
    public ScrollingAttributes ScrollingAttributes
    {
      get => scrollingAttributes ??= new();
      set => scrollingAttributes = value;
    }

    /// <summary>
    /// A value of WorkSet.
    /// </summary>
    [JsonPropertyName("workSet")]
    public CsePersonsWorkSet WorkSet
    {
      get => workSet ??= new();
      set => workSet = value;
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

    private Common respForHealthInsurance;
    private Common contactDisplayed;
    private Contact updatedStamps;
    private ScrollingAttributes scrollingAttributes;
    private CsePersonsWorkSet workSet;
    private CsePerson csePerson;
    private Contact contact;
    private ContactAddress contactAddress;
    private Array<ExportGroup> export1;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of ContactFound.
    /// </summary>
    [JsonPropertyName("contactFound")]
    public Common ContactFound
    {
      get => contactFound ??= new();
      set => contactFound = value;
    }

    private Common contactFound;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of HealthInsuranceCoverage.
    /// </summary>
    [JsonPropertyName("healthInsuranceCoverage")]
    public HealthInsuranceCoverage HealthInsuranceCoverage
    {
      get => healthInsuranceCoverage ??= new();
      set => healthInsuranceCoverage = value;
    }

    /// <summary>
    /// A value of ExistingNext.
    /// </summary>
    [JsonPropertyName("existingNext")]
    public Contact ExistingNext
    {
      get => existingNext ??= new();
      set => existingNext = value;
    }

    /// <summary>
    /// A value of ExistingPrevious.
    /// </summary>
    [JsonPropertyName("existingPrevious")]
    public Contact ExistingPrevious
    {
      get => existingPrevious ??= new();
      set => existingPrevious = value;
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
    /// A value of ExistingContactAddress.
    /// </summary>
    [JsonPropertyName("existingContactAddress")]
    public ContactAddress ExistingContactAddress
    {
      get => existingContactAddress ??= new();
      set => existingContactAddress = value;
    }

    /// <summary>
    /// A value of ExistingContactDetail.
    /// </summary>
    [JsonPropertyName("existingContactDetail")]
    public ContactDetail ExistingContactDetail
    {
      get => existingContactDetail ??= new();
      set => existingContactDetail = value;
    }

    private HealthInsuranceCoverage healthInsuranceCoverage;
    private Contact existingNext;
    private Contact existingPrevious;
    private CsePerson existingCsePerson;
    private Contact existingContact;
    private ContactAddress existingContactAddress;
    private ContactDetail existingContactDetail;
  }
#endregion
}
