// Program: SI_READ_INCOME_SOURCE_DETAILS, ID: 371763116, model: 746.
// Short name: SWE01221
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: SI_READ_INCOME_SOURCE_DETAILS.
/// </para>
/// <para>
/// Read the Income Source, Employer, Income Source Contact, and Income History 
/// for a given Person.
/// </para>
/// </summary>
[Serializable]
public partial class SiReadIncomeSourceDetails: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_READ_INCOME_SOURCE_DETAILS program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiReadIncomeSourceDetails(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiReadIncomeSourceDetails.
  /// </summary>
  public SiReadIncomeSourceDetails(IContext context, Import import,
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
    // ---------------------------------------------------------------------------------
    //                 M A I N T E N A N C E   L O G
    //   Date	  Developer		Description
    // --------  ----------------	
    // -------------------------------------------------
    //   /  /    Unknown		Initial Development
    // 09/10/96  G. Lofton		Unemployment ind moved from
    // 				Income Source to Cse Person.
    // 10/16/98  W. Campbell           Added an IF and Read stmt
    //                                 
    // for CSE_PERSON_RESOURCE to
    // obtain
    //                                 
    // the resource_no.
    // 08/02/18  GVandy  		CQ61457 - Update SVES and 'O' type employer to work
    // 				with eIWO for SSA. Type 'O', code 'SA' income
    // 				sources are now associated to the SSA employer
    // 				record.
    // 				
    // ---------------------------------------------------------------------------------
    if (ReadCsePerson())
    {
      export.CsePerson.Assign(entities.CsePerson);
    }
    else
    {
      ExitState = "CSE_PERSON_NF";

      return;
    }

    if (ReadIncomeSource())
    {
      export.IncomeSource.Assign(entities.IncomeSource);
      local.DateWorkArea.Date = export.IncomeSource.EndDt;
      export.IncomeSource.EndDt = UseCabSetMaximumDiscontinueDate();

      if (ReadIncomeSourceContact2())
      {
        export.Phone.Assign(entities.IncomeSourceContact);
      }
      else
      {
        ExitState = "INCOME_SOURCE_CONTACT_NF";

        return;
      }

      if (ReadIncomeSourceContact1())
      {
        export.Fax.Assign(entities.IncomeSourceContact);
      }

      if (AsChar(entities.IncomeSource.Type1) == 'E' || AsChar
        (entities.IncomeSource.Type1) == 'M' || AsChar
        (entities.IncomeSource.Type1) == 'O' && Equal
        (entities.IncomeSource.Code, "SA"))
      {
        if (ReadEmployerEmployerAddress())
        {
          export.Employer.Assign(entities.Employer);
          export.IncomeSource.Name = entities.Employer.Name;
          export.NonEmployIncomeSourceAddress.Street1 =
            entities.EmployerAddress.Street1;
          export.NonEmployIncomeSourceAddress.Street2 =
            entities.EmployerAddress.Street2;
          export.NonEmployIncomeSourceAddress.City =
            entities.EmployerAddress.City;
          export.NonEmployIncomeSourceAddress.State =
            entities.EmployerAddress.State;
          export.NonEmployIncomeSourceAddress.ZipCode =
            entities.EmployerAddress.ZipCode;
          export.NonEmployIncomeSourceAddress.Zip4 =
            entities.EmployerAddress.Zip4;
          export.EmployerAddress.Note = entities.EmployerAddress.Note;
        }
        else
        {
          ExitState = "EMPLOYER_NF";
        }
      }
      else
      {
        if (ReadNonEmployIncomeSourceAddress())
        {
          export.NonEmployIncomeSourceAddress.Assign(
            entities.NonEmployIncomeSourceAddress);
        }
        else
        {
          ExitState = "OTHER_INCOME_SOURCE_ADDRSS_NF_RB";

          return;
        }

        // -----------------------------------------------------
        // 10/16/98 W. Campbell -  Following IF and Read stmt
        // added for CSE_PERSON_RESOURCE.
        // -----------------------------------------------------
        if (AsChar(entities.IncomeSource.Type1) == 'R')
        {
          if (ReadCsePersonResource())
          {
            export.CsePersonResource.ResourceNo =
              entities.CsePersonResource.ResourceNo;
          }
          else
          {
            ExitState = "CSE_PERSON_RESOURCE_NF";
          }
        }

        // -----------------------------------------------------
        // 10/16/98 W. Campbell -  End of IF and Read stmt
        // added for CSE_PERSON_RESOURCE.
        // -----------------------------------------------------
      }
    }
    else
    {
      ExitState = "INCOME_SOURCE_NF";
    }
  }

  private DateTime? UseCabSetMaximumDiscontinueDate()
  {
    var useImport = new CabSetMaximumDiscontinueDate.Import();
    var useExport = new CabSetMaximumDiscontinueDate.Export();

    useImport.DateWorkArea.Date = local.DateWorkArea.Date;

    Call(CabSetMaximumDiscontinueDate.Execute, useImport, useExport);

    return useExport.DateWorkArea.Date;
  }

  private bool ReadCsePerson()
  {
    entities.CsePerson.Populated = false;

    return Read("ReadCsePerson",
      (db, command) =>
      {
        db.SetString(command, "numb", import.CsePersonsWorkSet.Number);
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CsePerson.Type1 = db.GetString(reader, 1);
        entities.CsePerson.UnemploymentInd = db.GetNullableString(reader, 2);
        entities.CsePerson.FederalInd = db.GetNullableString(reader, 3);
        entities.CsePerson.Populated = true;
        CheckValid<CsePerson>("Type1", entities.CsePerson.Type1);
      });
  }

  private bool ReadCsePersonResource()
  {
    System.Diagnostics.Debug.Assert(entities.IncomeSource.Populated);
    entities.CsePersonResource.Populated = false;

    return Read("ReadCsePersonResource",
      (db, command) =>
      {
        db.SetInt32(
          command, "resourceNo",
          entities.IncomeSource.CprResourceNo.GetValueOrDefault());
        db.
          SetString(command, "cspNumber", entities.IncomeSource.CspNumber ?? "");
          
      },
      (db, reader) =>
      {
        entities.CsePersonResource.CspNumber = db.GetString(reader, 0);
        entities.CsePersonResource.ResourceNo = db.GetInt32(reader, 1);
        entities.CsePersonResource.Type1 = db.GetNullableString(reader, 2);
        entities.CsePersonResource.Populated = true;
      });
  }

  private bool ReadEmployerEmployerAddress()
  {
    System.Diagnostics.Debug.Assert(entities.IncomeSource.Populated);
    entities.EmployerAddress.Populated = false;
    entities.Employer.Populated = false;

    return Read("ReadEmployerEmployerAddress",
      (db, command) =>
      {
        db.SetInt32(
          command, "identifier",
          entities.IncomeSource.EmpId.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Employer.Identifier = db.GetInt32(reader, 0);
        entities.EmployerAddress.EmpId = db.GetInt32(reader, 0);
        entities.Employer.Name = db.GetNullableString(reader, 1);
        entities.Employer.PhoneNo = db.GetNullableString(reader, 2);
        entities.Employer.AreaCode = db.GetNullableInt32(reader, 3);
        entities.Employer.EiwoEndDate = db.GetNullableDate(reader, 4);
        entities.Employer.EiwoStartDate = db.GetNullableDate(reader, 5);
        entities.Employer.FaxAreaCode = db.GetNullableInt32(reader, 6);
        entities.Employer.FaxPhoneNo = db.GetNullableString(reader, 7);
        entities.Employer.EmailAddress = db.GetNullableString(reader, 8);
        entities.EmployerAddress.LocationType = db.GetString(reader, 9);
        entities.EmployerAddress.Street1 = db.GetNullableString(reader, 10);
        entities.EmployerAddress.Street2 = db.GetNullableString(reader, 11);
        entities.EmployerAddress.City = db.GetNullableString(reader, 12);
        entities.EmployerAddress.Identifier = db.GetDateTime(reader, 13);
        entities.EmployerAddress.State = db.GetNullableString(reader, 14);
        entities.EmployerAddress.ZipCode = db.GetNullableString(reader, 15);
        entities.EmployerAddress.Zip4 = db.GetNullableString(reader, 16);
        entities.EmployerAddress.Note = db.GetNullableString(reader, 17);
        entities.EmployerAddress.Populated = true;
        entities.Employer.Populated = true;
        CheckValid<EmployerAddress>("LocationType",
          entities.EmployerAddress.LocationType);
      });
  }

  private bool ReadIncomeSource()
  {
    entities.IncomeSource.Populated = false;

    return Read("ReadIncomeSource",
      (db, command) =>
      {
        db.SetString(command, "cspINumber", entities.CsePerson.Number);
        db.SetDateTime(
          command, "identifier",
          import.IncomeSource.Identifier.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.IncomeSource.Identifier = db.GetDateTime(reader, 0);
        entities.IncomeSource.Type1 = db.GetString(reader, 1);
        entities.IncomeSource.LastQtrIncome = db.GetNullableDecimal(reader, 2);
        entities.IncomeSource.LastQtr = db.GetNullableString(reader, 3);
        entities.IncomeSource.LastQtrYr = db.GetNullableInt32(reader, 4);
        entities.IncomeSource.Attribute2NdQtrIncome =
          db.GetNullableDecimal(reader, 5);
        entities.IncomeSource.Attribute2NdQtr = db.GetNullableString(reader, 6);
        entities.IncomeSource.Attribute2NdQtrYr =
          db.GetNullableInt32(reader, 7);
        entities.IncomeSource.Attribute3RdQtrIncome =
          db.GetNullableDecimal(reader, 8);
        entities.IncomeSource.Attribute3RdQtr = db.GetNullableString(reader, 9);
        entities.IncomeSource.Attribute3RdQtrYr =
          db.GetNullableInt32(reader, 10);
        entities.IncomeSource.Attribute4ThQtrIncome =
          db.GetNullableDecimal(reader, 11);
        entities.IncomeSource.Attribute4ThQtr =
          db.GetNullableString(reader, 12);
        entities.IncomeSource.Attribute4ThQtrYr =
          db.GetNullableInt32(reader, 13);
        entities.IncomeSource.SentDt = db.GetNullableDate(reader, 14);
        entities.IncomeSource.ReturnDt = db.GetNullableDate(reader, 15);
        entities.IncomeSource.ReturnCd = db.GetNullableString(reader, 16);
        entities.IncomeSource.Name = db.GetNullableString(reader, 17);
        entities.IncomeSource.Code = db.GetNullableString(reader, 18);
        entities.IncomeSource.CspINumber = db.GetString(reader, 19);
        entities.IncomeSource.CprResourceNo = db.GetNullableInt32(reader, 20);
        entities.IncomeSource.CspNumber = db.GetNullableString(reader, 21);
        entities.IncomeSource.EmpId = db.GetNullableInt32(reader, 22);
        entities.IncomeSource.SendTo = db.GetNullableString(reader, 23);
        entities.IncomeSource.WorkerId = db.GetNullableString(reader, 24);
        entities.IncomeSource.StartDt = db.GetNullableDate(reader, 25);
        entities.IncomeSource.EndDt = db.GetNullableDate(reader, 26);
        entities.IncomeSource.Note = db.GetNullableString(reader, 27);
        entities.IncomeSource.Populated = true;
        CheckValid<IncomeSource>("Type1", entities.IncomeSource.Type1);
        CheckValid<IncomeSource>("SendTo", entities.IncomeSource.SendTo);
      });
  }

  private bool ReadIncomeSourceContact1()
  {
    System.Diagnostics.Debug.Assert(entities.IncomeSource.Populated);
    entities.IncomeSourceContact.Populated = false;

    return Read("ReadIncomeSourceContact1",
      (db, command) =>
      {
        db.SetString(command, "csePerson", entities.IncomeSource.CspINumber);
        db.SetDateTime(
          command, "isrIdentifier",
          entities.IncomeSource.Identifier.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.IncomeSourceContact.IsrIdentifier = db.GetDateTime(reader, 0);
        entities.IncomeSourceContact.Identifier = db.GetInt32(reader, 1);
        entities.IncomeSourceContact.Type1 = db.GetString(reader, 2);
        entities.IncomeSourceContact.Name = db.GetNullableString(reader, 3);
        entities.IncomeSourceContact.ExtensionNo =
          db.GetNullableString(reader, 4);
        entities.IncomeSourceContact.Number = db.GetNullableInt32(reader, 5);
        entities.IncomeSourceContact.AreaCode = db.GetNullableInt32(reader, 6);
        entities.IncomeSourceContact.CsePerson = db.GetString(reader, 7);
        entities.IncomeSourceContact.EmailAddress =
          db.GetNullableString(reader, 8);
        entities.IncomeSourceContact.Populated = true;
      });
  }

  private bool ReadIncomeSourceContact2()
  {
    System.Diagnostics.Debug.Assert(entities.IncomeSource.Populated);
    entities.IncomeSourceContact.Populated = false;

    return Read("ReadIncomeSourceContact2",
      (db, command) =>
      {
        db.SetString(command, "csePerson", entities.IncomeSource.CspINumber);
        db.SetDateTime(
          command, "isrIdentifier",
          entities.IncomeSource.Identifier.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.IncomeSourceContact.IsrIdentifier = db.GetDateTime(reader, 0);
        entities.IncomeSourceContact.Identifier = db.GetInt32(reader, 1);
        entities.IncomeSourceContact.Type1 = db.GetString(reader, 2);
        entities.IncomeSourceContact.Name = db.GetNullableString(reader, 3);
        entities.IncomeSourceContact.ExtensionNo =
          db.GetNullableString(reader, 4);
        entities.IncomeSourceContact.Number = db.GetNullableInt32(reader, 5);
        entities.IncomeSourceContact.AreaCode = db.GetNullableInt32(reader, 6);
        entities.IncomeSourceContact.CsePerson = db.GetString(reader, 7);
        entities.IncomeSourceContact.EmailAddress =
          db.GetNullableString(reader, 8);
        entities.IncomeSourceContact.Populated = true;
      });
  }

  private bool ReadNonEmployIncomeSourceAddress()
  {
    System.Diagnostics.Debug.Assert(entities.IncomeSource.Populated);
    entities.NonEmployIncomeSourceAddress.Populated = false;

    return Read("ReadNonEmployIncomeSourceAddress",
      (db, command) =>
      {
        db.SetString(command, "csePerson", entities.IncomeSource.CspINumber);
        db.SetDateTime(
          command, "isrIdentifier",
          entities.IncomeSource.Identifier.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.NonEmployIncomeSourceAddress.IsrIdentifier =
          db.GetDateTime(reader, 0);
        entities.NonEmployIncomeSourceAddress.Street1 =
          db.GetNullableString(reader, 1);
        entities.NonEmployIncomeSourceAddress.Street2 =
          db.GetNullableString(reader, 2);
        entities.NonEmployIncomeSourceAddress.City =
          db.GetNullableString(reader, 3);
        entities.NonEmployIncomeSourceAddress.State =
          db.GetNullableString(reader, 4);
        entities.NonEmployIncomeSourceAddress.ZipCode =
          db.GetNullableString(reader, 5);
        entities.NonEmployIncomeSourceAddress.Zip4 =
          db.GetNullableString(reader, 6);
        entities.NonEmployIncomeSourceAddress.LocationType =
          db.GetString(reader, 7);
        entities.NonEmployIncomeSourceAddress.CsePerson =
          db.GetString(reader, 8);
        entities.NonEmployIncomeSourceAddress.Populated = true;
        CheckValid<NonEmployIncomeSourceAddress>("LocationType",
          entities.NonEmployIncomeSourceAddress.LocationType);
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
    /// A value of CsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("csePersonsWorkSet")]
    public CsePersonsWorkSet CsePersonsWorkSet
    {
      get => csePersonsWorkSet ??= new();
      set => csePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of IncomeSource.
    /// </summary>
    [JsonPropertyName("incomeSource")]
    public IncomeSource IncomeSource
    {
      get => incomeSource ??= new();
      set => incomeSource = value;
    }

    private CsePersonsWorkSet csePersonsWorkSet;
    private IncomeSource incomeSource;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of CsePersonResource.
    /// </summary>
    [JsonPropertyName("csePersonResource")]
    public CsePersonResource CsePersonResource
    {
      get => csePersonResource ??= new();
      set => csePersonResource = value;
    }

    /// <summary>
    /// A value of Employer.
    /// </summary>
    [JsonPropertyName("employer")]
    public Employer Employer
    {
      get => employer ??= new();
      set => employer = value;
    }

    /// <summary>
    /// A value of IncomeSource.
    /// </summary>
    [JsonPropertyName("incomeSource")]
    public IncomeSource IncomeSource
    {
      get => incomeSource ??= new();
      set => incomeSource = value;
    }

    /// <summary>
    /// A value of Phone.
    /// </summary>
    [JsonPropertyName("phone")]
    public IncomeSourceContact Phone
    {
      get => phone ??= new();
      set => phone = value;
    }

    /// <summary>
    /// A value of Fax.
    /// </summary>
    [JsonPropertyName("fax")]
    public IncomeSourceContact Fax
    {
      get => fax ??= new();
      set => fax = value;
    }

    /// <summary>
    /// A value of NonEmployIncomeSourceAddress.
    /// </summary>
    [JsonPropertyName("nonEmployIncomeSourceAddress")]
    public NonEmployIncomeSourceAddress NonEmployIncomeSourceAddress
    {
      get => nonEmployIncomeSourceAddress ??= new();
      set => nonEmployIncomeSourceAddress = value;
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
    /// A value of EmployerAddress.
    /// </summary>
    [JsonPropertyName("employerAddress")]
    public EmployerAddress EmployerAddress
    {
      get => employerAddress ??= new();
      set => employerAddress = value;
    }

    private CsePersonResource csePersonResource;
    private Employer employer;
    private IncomeSource incomeSource;
    private IncomeSourceContact phone;
    private IncomeSourceContact fax;
    private NonEmployIncomeSourceAddress nonEmployIncomeSourceAddress;
    private CsePerson csePerson;
    private EmployerAddress employerAddress;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of DateWorkArea.
    /// </summary>
    [JsonPropertyName("dateWorkArea")]
    public DateWorkArea DateWorkArea
    {
      get => dateWorkArea ??= new();
      set => dateWorkArea = value;
    }

    /// <summary>
    /// A value of Code.
    /// </summary>
    [JsonPropertyName("code")]
    public Code Code
    {
      get => code ??= new();
      set => code = value;
    }

    /// <summary>
    /// A value of CodeValue.
    /// </summary>
    [JsonPropertyName("codeValue")]
    public CodeValue CodeValue
    {
      get => codeValue ??= new();
      set => codeValue = value;
    }

    /// <summary>
    /// A value of ValidValue.
    /// </summary>
    [JsonPropertyName("validValue")]
    public Common ValidValue
    {
      get => validValue ??= new();
      set => validValue = value;
    }

    private DateWorkArea dateWorkArea;
    private Code code;
    private CodeValue codeValue;
    private Common validValue;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of CsePersonResource.
    /// </summary>
    [JsonPropertyName("csePersonResource")]
    public CsePersonResource CsePersonResource
    {
      get => csePersonResource ??= new();
      set => csePersonResource = value;
    }

    /// <summary>
    /// A value of EmployerAddress.
    /// </summary>
    [JsonPropertyName("employerAddress")]
    public EmployerAddress EmployerAddress
    {
      get => employerAddress ??= new();
      set => employerAddress = value;
    }

    /// <summary>
    /// A value of Employer.
    /// </summary>
    [JsonPropertyName("employer")]
    public Employer Employer
    {
      get => employer ??= new();
      set => employer = value;
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
    /// A value of IncomeSource.
    /// </summary>
    [JsonPropertyName("incomeSource")]
    public IncomeSource IncomeSource
    {
      get => incomeSource ??= new();
      set => incomeSource = value;
    }

    /// <summary>
    /// A value of IncomeSourceContact.
    /// </summary>
    [JsonPropertyName("incomeSourceContact")]
    public IncomeSourceContact IncomeSourceContact
    {
      get => incomeSourceContact ??= new();
      set => incomeSourceContact = value;
    }

    /// <summary>
    /// A value of NonEmployIncomeSourceAddress.
    /// </summary>
    [JsonPropertyName("nonEmployIncomeSourceAddress")]
    public NonEmployIncomeSourceAddress NonEmployIncomeSourceAddress
    {
      get => nonEmployIncomeSourceAddress ??= new();
      set => nonEmployIncomeSourceAddress = value;
    }

    private CsePersonResource csePersonResource;
    private EmployerAddress employerAddress;
    private Employer employer;
    private CsePerson csePerson;
    private IncomeSource incomeSource;
    private IncomeSourceContact incomeSourceContact;
    private NonEmployIncomeSourceAddress nonEmployIncomeSourceAddress;
  }
#endregion
}
