// Program: OE_PCON_VALIDATE_CONTACT_DETAIL, ID: 371891961, model: 746.
// Short name: SWE00954
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
/// A program: OE_PCON_VALIDATE_CONTACT_DETAIL.
/// </para>
/// <para>
/// RESP: OBLGESTB
/// This action block validates the Contact details
/// </para>
/// </summary>
[Serializable]
public partial class OePconValidateContactDetail: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_PCON_VALIDATE_CONTACT_DETAIL program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OePconValidateContactDetail(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OePconValidateContactDetail.
  /// </summary>
  public OePconValidateContactDetail(IContext context, Import import,
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
    // This action block validates contact details
    // PROCESSING:
    // It is passed CSE_PERSON, CONTACT, CONTACT_ADDRESS and CONTACT_DETAIL 
    // values. It validates and returns a repeating group of errors encountered.
    // The validations are done in "natural" order of the screen field layout (
    // i.e. Top to Bottom, Left to Right) and they are highlighted in the
    // reverse order by the procedure step. This makes the validation action
    // block read more logically.
    // ACTION BLOCKS:
    // ENTITY TYPES USED:
    // 	CSE_PERSON		- R - -
    // 	CONTACT			- R - -
    // 	CONTACT_DETAIL		- R - -
    // DATABASE FILES USED:
    // CREATED BY:	govindaraj
    // DATE CREATED:	01/26/95
    // *********************************************
    // *********************************************
    // MAINTENANCE LOG
    // AUTHOR	DATE	CHGREQ	DESCRIPTION
    // govind	01/26/95	Initial coding
    // *********************************************
    export.CsePerson.Number = import.CsePerson.Number;
    export.Contact.Assign(import.Contact);
    export.ContactAddress.Assign(import.ContactAddress);

    export.Export1.Index = 0;
    export.Export1.Clear();

    for(import.Import1.Index = 0; import.Import1.Index < import.Import1.Count; ++
      import.Import1.Index)
    {
      if (export.Export1.IsFull)
      {
        break;
      }

      export.Export1.Update.DetailAction.ActionEntry =
        import.Import1.Item.Action.ActionEntry;
      export.Export1.Update.Detail.Assign(import.Import1.Item.Detail);
      export.Export1.Next();
    }

    export.LastErrorEntryNo.Count = 0;
    export.Errors.Index = -1;

    if (ReadCsePerson())
    {
      export.CsePerson.Number = entities.ExistingCsePerson.Number;
      local.CsePersonsWorkSet.Number = entities.ExistingCsePerson.Number;
      UseSiReadCsePerson();
    }
    else
    {
      ++export.Errors.Index;
      export.Errors.CheckSize();

      export.LastErrorEntryNo.Count = export.Errors.Index + 1;
      export.Errors.Update.DetailError.Count = 1;

      return;
    }

    if (Equal(import.UserAction.Command, "UPDATE") || Equal
      (import.UserAction.Command, "DELETE"))
    {
      if (!ReadContact())
      {
        ++export.Errors.Index;
        export.Errors.CheckSize();

        export.LastErrorEntryNo.Count = export.Errors.Index + 1;
        export.Errors.Update.DetailError.Count = 2;

        return;
      }
    }

    if (Equal(import.UserAction.Command, "CREATE") || Equal
      (import.UserAction.Command, "UPDATE"))
    {
      if (IsEmpty(export.Contact.CompanyName) && IsEmpty
        (export.Contact.NameLast))
      {
        // ---------------------------------------------
        // Either Company Name or Contact Name must be entered.
        // ---------------------------------------------
        ++export.Errors.Index;
        export.Errors.CheckSize();

        export.LastErrorEntryNo.Count = export.Errors.Index + 1;
        export.Errors.Update.DetailError.Count = 7;
      }

      if (!IsEmpty(export.ContactAddress.Street1) || !
        IsEmpty(export.ContactAddress.Street2) || !
        IsEmpty(export.ContactAddress.City) || !
        IsEmpty(export.ContactAddress.State) || !
        IsEmpty(export.ContactAddress.ZipCode5) || !
        IsEmpty(export.ContactAddress.ZipCode4))
      {
        if (IsEmpty(export.ContactAddress.Street1))
        {
          ++export.Errors.Index;
          export.Errors.CheckSize();

          export.LastErrorEntryNo.Count = export.Errors.Index + 1;
          export.Errors.Update.DetailError.Count = 3;
        }

        if (IsEmpty(export.ContactAddress.City))
        {
          ++export.Errors.Index;
          export.Errors.CheckSize();

          export.LastErrorEntryNo.Count = export.Errors.Index + 1;
          export.Errors.Update.DetailError.Count = 4;
        }

        local.StateCode.CodeName = "STATE CODE";
        local.StateCodeValue.Cdvalue = export.ContactAddress.State ?? Spaces
          (10);
        UseCabValidateCodeValue();

        if (AsChar(local.ValidStateCode.Flag) == 'N')
        {
          ++export.Errors.Index;
          export.Errors.CheckSize();

          export.LastErrorEntryNo.Count = export.Errors.Index + 1;
          export.Errors.Update.DetailError.Count = 5;
        }

        if (IsEmpty(export.ContactAddress.ZipCode5))
        {
          ++export.Errors.Index;
          export.Errors.CheckSize();

          export.LastErrorEntryNo.Count = export.Errors.Index + 1;
          export.Errors.Update.DetailError.Count = 6;
        }
        else
        {
          if (Length(TrimEnd(export.ContactAddress.ZipCode5)) > 0 && Length
            (TrimEnd(export.ContactAddress.ZipCode5)) < 5)
          {
            ++export.Errors.Index;
            export.Errors.CheckSize();

            export.LastErrorEntryNo.Count = export.Errors.Index + 1;
            export.Errors.Update.DetailError.Count = 18;
          }

          if (Length(TrimEnd(export.ContactAddress.ZipCode5)) > 0 && Verify
            (TrimEnd(export.ContactAddress.ZipCode5), "0123456789") != 0)
          {
            ++export.Errors.Index;
            export.Errors.CheckSize();

            export.LastErrorEntryNo.Count = export.Errors.Index + 1;
            export.Errors.Update.DetailError.Count = 19;
          }

          if (Length(TrimEnd(export.ContactAddress.ZipCode5)) == 0 && Length
            (TrimEnd(export.ContactAddress.ZipCode4)) > 0)
          {
            ++export.Errors.Index;
            export.Errors.CheckSize();

            export.LastErrorEntryNo.Count = export.Errors.Index + 1;
            export.Errors.Update.DetailError.Count = 20;
          }

          if (Length(TrimEnd(export.ContactAddress.ZipCode5)) > 0 && Length
            (TrimEnd(export.ContactAddress.ZipCode4)) > 0)
          {
            if (Length(TrimEnd(export.ContactAddress.ZipCode4)) < 4)
            {
              ++export.Errors.Index;
              export.Errors.CheckSize();

              export.LastErrorEntryNo.Count = export.Errors.Index + 1;
              export.Errors.Update.DetailError.Count = 21;
            }
            else if (Verify(export.ContactAddress.ZipCode4, "0123456789") != 0)
            {
              ++export.Errors.Index;
              export.Errors.CheckSize();

              export.LastErrorEntryNo.Count = export.Errors.Index + 1;
              export.Errors.Update.DetailError.Count = 22;
            }
          }
        }
      }

      if (Lt(new DateTime(1, 1, 1), export.Contact.VerifiedDate))
      {
        if (Lt(Now().Date, export.Contact.VerifiedDate))
        {
          ++export.Errors.Index;
          export.Errors.CheckSize();

          export.LastErrorEntryNo.Count = export.Errors.Index + 1;
          export.Errors.Update.DetailError.Count = 14;
        }
        else
        {
          export.Contact.VerifiedUserId = global.UserId;
        }
      }

      if (export.Contact.HomePhoneAreaCode.GetValueOrDefault() != 0 || export
        .Contact.HomePhone.GetValueOrDefault() != 0)
      {
        if (export.Contact.HomePhoneAreaCode.GetValueOrDefault() == 0)
        {
          ++export.Errors.Index;
          export.Errors.CheckSize();

          export.LastErrorEntryNo.Count = export.Errors.Index + 1;
          export.Errors.Update.DetailError.Count = 8;
        }

        if (export.Contact.HomePhone.GetValueOrDefault() == 0)
        {
          ++export.Errors.Index;
          export.Errors.CheckSize();

          export.LastErrorEntryNo.Count = export.Errors.Index + 1;
          export.Errors.Update.DetailError.Count = 9;
        }
      }

      if (export.Contact.WorkPhoneAreaCode.GetValueOrDefault() != 0 || export
        .Contact.WorkPhone.GetValueOrDefault() != 0 || !
        IsEmpty(export.Contact.WorkPhoneExt))
      {
        if (export.Contact.WorkPhoneAreaCode.GetValueOrDefault() == 0)
        {
          ++export.Errors.Index;
          export.Errors.CheckSize();

          export.LastErrorEntryNo.Count = export.Errors.Index + 1;
          export.Errors.Update.DetailError.Count = 10;
        }

        if (export.Contact.WorkPhone.GetValueOrDefault() == 0)
        {
          ++export.Errors.Index;
          export.Errors.CheckSize();

          export.LastErrorEntryNo.Count = export.Errors.Index + 1;
          export.Errors.Update.DetailError.Count = 11;
        }
      }

      if (export.Contact.FaxAreaCode.GetValueOrDefault() != 0 || export
        .Contact.Fax.GetValueOrDefault() != 0 || !
        IsEmpty(export.Contact.FaxExt))
      {
        if (export.Contact.FaxAreaCode.GetValueOrDefault() == 0)
        {
          ++export.Errors.Index;
          export.Errors.CheckSize();

          export.LastErrorEntryNo.Count = export.Errors.Index + 1;
          export.Errors.Update.DetailError.Count = 12;
        }

        if (export.Contact.Fax.GetValueOrDefault() == 0)
        {
          ++export.Errors.Index;
          export.Errors.CheckSize();

          export.LastErrorEntryNo.Count = export.Errors.Index + 1;
          export.Errors.Update.DetailError.Count = 13;
        }
      }
    }

    if (Equal(import.UserAction.Command, "DELETE"))
    {
      foreach(var item in ReadMarriageHistory())
      {
        ++export.Errors.Index;
        export.Errors.CheckSize();

        export.LastErrorEntryNo.Count = export.Errors.Index + 1;
        export.Errors.Update.DetailError.Count = 16;
      }

      foreach(var item in ReadHealthInsuranceCoverage())
      {
        ++export.Errors.Index;
        export.Errors.CheckSize();

        export.LastErrorEntryNo.Count = export.Errors.Index + 1;
        export.Errors.Update.DetailError.Count = 17;
      }
    }
  }

  private void UseCabValidateCodeValue()
  {
    var useImport = new CabValidateCodeValue.Import();
    var useExport = new CabValidateCodeValue.Export();

    useImport.CodeValue.Cdvalue = local.StateCodeValue.Cdvalue;
    useImport.Code.CodeName = local.StateCode.CodeName;

    Call(CabValidateCodeValue.Execute, useImport, useExport);

    local.ValidStateCode.Flag = useExport.ValidCode.Flag;
  }

  private void UseSiReadCsePerson()
  {
    var useImport = new SiReadCsePerson.Import();
    var useExport = new SiReadCsePerson.Export();

    useImport.CsePersonsWorkSet.Number = local.CsePersonsWorkSet.Number;

    Call(SiReadCsePerson.Execute, useImport, useExport);

    export.WorkSet.FormattedName = useExport.CsePersonsWorkSet.FormattedName;
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
        entities.ExistingContact.Populated = true;
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

  private IEnumerable<bool> ReadHealthInsuranceCoverage()
  {
    System.Diagnostics.Debug.Assert(entities.ExistingContact.Populated);
    entities.ExistingHealthInsuranceCoverage.Populated = false;

    return ReadEach("ReadHealthInsuranceCoverage",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "conHNumber", entities.ExistingContact.ContactNumber);
        db.SetNullableString(
          command, "cspHNumber", entities.ExistingContact.CspNumber);
      },
      (db, reader) =>
      {
        entities.ExistingHealthInsuranceCoverage.Identifier =
          db.GetInt64(reader, 0);
        entities.ExistingHealthInsuranceCoverage.PolicyEffectiveDate =
          db.GetNullableDate(reader, 1);
        entities.ExistingHealthInsuranceCoverage.CspHNumber =
          db.GetNullableString(reader, 2);
        entities.ExistingHealthInsuranceCoverage.ConHNumber =
          db.GetNullableInt32(reader, 3);
        entities.ExistingHealthInsuranceCoverage.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadMarriageHistory()
  {
    System.Diagnostics.Debug.Assert(entities.ExistingContact.Populated);
    entities.ExistingMarriageHistory.Populated = false;

    return ReadEach("ReadMarriageHistory",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "conINumber", entities.ExistingContact.ContactNumber);
        db.SetNullableString(
          command, "cspINumber", entities.ExistingContact.CspNumber);
      },
      (db, reader) =>
      {
        entities.ExistingMarriageHistory.CspRNumber = db.GetString(reader, 0);
        entities.ExistingMarriageHistory.MarriageDate =
          db.GetNullableDate(reader, 1);
        entities.ExistingMarriageHistory.CspINumber =
          db.GetNullableString(reader, 2);
        entities.ExistingMarriageHistory.ConINumber =
          db.GetNullableInt32(reader, 3);
        entities.ExistingMarriageHistory.Identifier = db.GetInt32(reader, 4);
        entities.ExistingMarriageHistory.Populated = true;

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
    /// <summary>A ImportGroup group.</summary>
    [Serializable]
    public class ImportGroup
    {
      /// <summary>
      /// A value of Action.
      /// </summary>
      [JsonPropertyName("action")]
      public Common Action
      {
        get => action ??= new();
        set => action = value;
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

      private Common action;
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
    /// A value of UserAction.
    /// </summary>
    [JsonPropertyName("userAction")]
    public Common UserAction
    {
      get => userAction ??= new();
      set => userAction = value;
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
    private Common userAction;
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

    /// <summary>A ErrorsGroup group.</summary>
    [Serializable]
    public class ErrorsGroup
    {
      /// <summary>
      /// A value of DetailNoteEntry.
      /// </summary>
      [JsonPropertyName("detailNoteEntry")]
      public Common DetailNoteEntry
      {
        get => detailNoteEntry ??= new();
        set => detailNoteEntry = value;
      }

      /// <summary>
      /// A value of DetailError.
      /// </summary>
      [JsonPropertyName("detailError")]
      public Common DetailError
      {
        get => detailError ??= new();
        set => detailError = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 50;

      private Common detailNoteEntry;
      private Common detailError;
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
    /// A value of LastErrorEntryNo.
    /// </summary>
    [JsonPropertyName("lastErrorEntryNo")]
    public Common LastErrorEntryNo
    {
      get => lastErrorEntryNo ??= new();
      set => lastErrorEntryNo = value;
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

    /// <summary>
    /// Gets a value of Errors.
    /// </summary>
    [JsonIgnore]
    public Array<ErrorsGroup> Errors => errors ??= new(ErrorsGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Errors for json serialization.
    /// </summary>
    [JsonPropertyName("errors")]
    [Computed]
    public IList<ErrorsGroup> Errors_Json
    {
      get => errors;
      set => Errors.Assign(value);
    }

    private CsePersonsWorkSet workSet;
    private CsePerson csePerson;
    private Common lastErrorEntryNo;
    private Contact contact;
    private ContactAddress contactAddress;
    private Array<ExportGroup> export1;
    private Array<ErrorsGroup> errors;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
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

    /// <summary>
    /// A value of ErrorNoteEntryNo.
    /// </summary>
    [JsonPropertyName("errorNoteEntryNo")]
    public Common ErrorNoteEntryNo
    {
      get => errorNoteEntryNo ??= new();
      set => errorNoteEntryNo = value;
    }

    /// <summary>
    /// A value of ValidCountryCode.
    /// </summary>
    [JsonPropertyName("validCountryCode")]
    public Common ValidCountryCode
    {
      get => validCountryCode ??= new();
      set => validCountryCode = value;
    }

    /// <summary>
    /// A value of CountryCodeValue.
    /// </summary>
    [JsonPropertyName("countryCodeValue")]
    public CodeValue CountryCodeValue
    {
      get => countryCodeValue ??= new();
      set => countryCodeValue = value;
    }

    /// <summary>
    /// A value of CountryCode.
    /// </summary>
    [JsonPropertyName("countryCode")]
    public Code CountryCode
    {
      get => countryCode ??= new();
      set => countryCode = value;
    }

    /// <summary>
    /// A value of ValidStateCode.
    /// </summary>
    [JsonPropertyName("validStateCode")]
    public Common ValidStateCode
    {
      get => validStateCode ??= new();
      set => validStateCode = value;
    }

    /// <summary>
    /// A value of StateCodeValue.
    /// </summary>
    [JsonPropertyName("stateCodeValue")]
    public CodeValue StateCodeValue
    {
      get => stateCodeValue ??= new();
      set => stateCodeValue = value;
    }

    /// <summary>
    /// A value of StateCode.
    /// </summary>
    [JsonPropertyName("stateCode")]
    public Code StateCode
    {
      get => stateCode ??= new();
      set => stateCode = value;
    }

    /// <summary>
    /// A value of CheckZip.
    /// </summary>
    [JsonPropertyName("checkZip")]
    public Common CheckZip
    {
      get => checkZip ??= new();
      set => checkZip = value;
    }

    private CsePersonsWorkSet csePersonsWorkSet;
    private Common errorNoteEntryNo;
    private Common validCountryCode;
    private CodeValue countryCodeValue;
    private Code countryCode;
    private Common validStateCode;
    private CodeValue stateCodeValue;
    private Code stateCode;
    private Common checkZip;
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
    /// A value of ExistingMarriageHistory.
    /// </summary>
    [JsonPropertyName("existingMarriageHistory")]
    public MarriageHistory ExistingMarriageHistory
    {
      get => existingMarriageHistory ??= new();
      set => existingMarriageHistory = value;
    }

    /// <summary>
    /// A value of ExistingHealthInsuranceCoverage.
    /// </summary>
    [JsonPropertyName("existingHealthInsuranceCoverage")]
    public HealthInsuranceCoverage ExistingHealthInsuranceCoverage
    {
      get => existingHealthInsuranceCoverage ??= new();
      set => existingHealthInsuranceCoverage = value;
    }

    private ContactDetail existingContactDetail;
    private ContactAddress existingContactAddress;
    private CsePerson existingCsePerson;
    private Contact existingContact;
    private MarriageHistory existingMarriageHistory;
    private HealthInsuranceCoverage existingHealthInsuranceCoverage;
  }
#endregion
}
