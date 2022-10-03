// Program: OE_HICP_LIST_ALL_PERSONAL_HINS, ID: 371846345, model: 746.
// Short name: SWE00942
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
/// A program: OE_HICP_LIST_ALL_PERSONAL_HINS.
/// </para>
/// <para>
/// RESP: OBLGESTB
/// WRITTEN BY:SID
/// </para>
/// </summary>
[Serializable]
public partial class OeHicpListAllPersonalHins: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_HICP_LIST_ALL_PERSONAL_HINS program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeHicpListAllPersonalHins(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeHicpListAllPersonalHins.
  /// </summary>
  public OeHicpListAllPersonalHins(IContext context, Import import,
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
    export.HolderCase.Number = import.HolderCase.Number;
    export.HolderCsePerson.Number = import.HolderCsePerson.Number;

    if (ReadCsePerson())
    {
      export.HolderCsePerson.Number = entities.Responsible.Number;
      export.HolderHCsePerson.Number = entities.Responsible.Number;
    }
    else
    {
      ExitState = "CSE_PERSON_NF";

      return;
    }

    if (ReadCase())
    {
      export.HolderCase.Number = entities.Case1.Number;
      export.HolderHCase.Number = entities.Case1.Number;
    }
    else
    {
      ExitState = "CASE_NF";

      return;
    }

    if (ReadHealthInsuranceCoverage())
    {
      MoveHealthInsuranceCoverage(entities.HealthInsuranceCoverage,
        export.HolderHealthInsuranceCoverage);
      export.HolderHHealthInsuranceCoverage.Assign(
        entities.HealthInsuranceCoverage);

      if (AsChar(entities.HealthInsuranceCoverage.PolicyPaidByCsePersonInd) != 'Y'
        )
      {
        if (ReadContact())
        {
          export.HolderContact.Assign(entities.Contact);
        }
      }
    }
    else
    {
      ExitState = "HEALTH_INSURANCE_COVERAGE_NF_RB";

      return;
    }

    if (ReadHealthInsuranceCompany())
    {
      export.HolderHealthInsuranceCompany.
        Assign(entities.HealthInsuranceCompany);
    }

    // *** November 1, 1999  David Lowry
    // PR78706.  There may be multiple case role rows if an CH or AR  was end 
    // dated and then re-entered.
    export.Export1.Index = 0;
    export.Export1.Clear();

    foreach(var item in ReadPersonalHealthInsuranceCsePerson())
    {
      // ****************************************************************************
      // **   Beneficiary must be tied to case as "CHild role.   Active not 
      // important.
      // ****************************************************************************
      if (ReadCaseRole())
      {
        export.Export1.Update.DetailsPersonalHealthInsurance.Assign(
          entities.PersonalHealthInsurance);
        export.Export1.Update.H.CoverageVerifiedDate =
          entities.PersonalHealthInsurance.CoverageVerifiedDate;
        export.Export1.Update.Insured.Number = entities.Insured.Number;
        export.Export1.Update.InsuredH.Number = entities.Insured.Number;
        export.Export1.Update.DetailsCaseRole.Type1 = entities.CaseRole.Type1;
      }
      else
      {
        export.Export1.Next();

        continue;
      }

      local.New1.Number = entities.Insured.Number;
      UseSiReadCsePerson();
      export.Export1.Update.InsuredName.FormattedNameText =
        local.New1.FormattedName;
      export.Export1.Next();
    }

    if (export.Export1.IsEmpty)
    {
      ExitState = "OE0000_COVERED_PERSON_NF";
    }
    else
    {
      ExitState = "ACO_NI0000_SUCCESSFUL_DISPLAY";
    }
  }

  private static void MoveHealthInsuranceCoverage(
    HealthInsuranceCoverage source, HealthInsuranceCoverage target)
  {
    target.Identifier = source.Identifier;
    target.InsuranceGroupNumber = source.InsuranceGroupNumber;
    target.InsurancePolicyNumber = source.InsurancePolicyNumber;
  }

  private void UseSiReadCsePerson()
  {
    var useImport = new SiReadCsePerson.Import();
    var useExport = new SiReadCsePerson.Export();

    useImport.CsePersonsWorkSet.Number = local.New1.Number;

    Call(SiReadCsePerson.Execute, useImport, useExport);

    local.New1.Assign(useExport.CsePersonsWorkSet);
  }

  private bool ReadCase()
  {
    entities.Case1.Populated = false;

    return Read("ReadCase",
      (db, command) =>
      {
        db.SetString(command, "numb", import.HolderCase.Number);
      },
      (db, reader) =>
      {
        entities.Case1.Number = db.GetString(reader, 0);
        entities.Case1.Populated = true;
      });
  }

  private bool ReadCaseRole()
  {
    entities.CaseRole.Populated = false;

    return Read("ReadCaseRole",
      (db, command) =>
      {
        db.SetString(command, "casNumber", entities.Case1.Number);
        db.SetString(command, "cspNumber", entities.Insured.Number);
      },
      (db, reader) =>
      {
        entities.CaseRole.CasNumber = db.GetString(reader, 0);
        entities.CaseRole.CspNumber = db.GetString(reader, 1);
        entities.CaseRole.Type1 = db.GetString(reader, 2);
        entities.CaseRole.Identifier = db.GetInt32(reader, 3);
        entities.CaseRole.EndDate = db.GetNullableDate(reader, 4);
        entities.CaseRole.Populated = true;
        CheckValid<CaseRole>("Type1", entities.CaseRole.Type1);
      });
  }

  private bool ReadContact()
  {
    System.Diagnostics.Debug.Assert(entities.HealthInsuranceCoverage.Populated);
    entities.Contact.Populated = false;

    return Read("ReadContact",
      (db, command) =>
      {
        db.SetInt32(
          command, "contactNumber",
          entities.HealthInsuranceCoverage.ConHNumber.GetValueOrDefault());
        db.SetString(
          command, "cspNumber1",
          entities.HealthInsuranceCoverage.CspHNumber ?? "");
        db.SetString(command, "cspNumber2", entities.Responsible.Number);
      },
      (db, reader) =>
      {
        entities.Contact.CspNumber = db.GetString(reader, 0);
        entities.Contact.ContactNumber = db.GetInt32(reader, 1);
        entities.Contact.RelationshipToCsePerson =
          db.GetNullableString(reader, 2);
        entities.Contact.NameLast = db.GetNullableString(reader, 3);
        entities.Contact.NameFirst = db.GetNullableString(reader, 4);
        entities.Contact.MiddleInitial = db.GetNullableString(reader, 5);
        entities.Contact.CreatedTimestamp = db.GetDateTime(reader, 6);
        entities.Contact.Populated = true;
      });
  }

  private bool ReadCsePerson()
  {
    entities.Responsible.Populated = false;

    return Read("ReadCsePerson",
      (db, command) =>
      {
        db.SetString(command, "numb", import.HolderCsePerson.Number);
      },
      (db, reader) =>
      {
        entities.Responsible.Number = db.GetString(reader, 0);
        entities.Responsible.Populated = true;
      });
  }

  private bool ReadHealthInsuranceCompany()
  {
    System.Diagnostics.Debug.Assert(entities.HealthInsuranceCoverage.Populated);
    entities.HealthInsuranceCompany.Populated = false;

    return Read("ReadHealthInsuranceCompany",
      (db, command) =>
      {
        db.SetInt32(
          command, "identifier",
          entities.HealthInsuranceCoverage.HicIdentifier.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.HealthInsuranceCompany.Identifier = db.GetInt32(reader, 0);
        entities.HealthInsuranceCompany.CarrierCode =
          db.GetNullableString(reader, 1);
        entities.HealthInsuranceCompany.InsurancePolicyCarrier =
          db.GetNullableString(reader, 2);
        entities.HealthInsuranceCompany.InsurerPhone =
          db.GetNullableInt32(reader, 3);
        entities.HealthInsuranceCompany.InsurerFax =
          db.GetNullableInt32(reader, 4);
        entities.HealthInsuranceCompany.Populated = true;
      });
  }

  private bool ReadHealthInsuranceCoverage()
  {
    entities.HealthInsuranceCoverage.Populated = false;

    return Read("ReadHealthInsuranceCoverage",
      (db, command) =>
      {
        db.SetInt64(
          command, "identifier",
          import.HolderHealthInsuranceCoverage.Identifier);
        db.SetNullableString(command, "cspNumber", entities.Responsible.Number);
      },
      (db, reader) =>
      {
        entities.HealthInsuranceCoverage.Identifier = db.GetInt64(reader, 0);
        entities.HealthInsuranceCoverage.PolicyPaidByCsePersonInd =
          db.GetString(reader, 1);
        entities.HealthInsuranceCoverage.InsuranceGroupNumber =
          db.GetNullableString(reader, 2);
        entities.HealthInsuranceCoverage.InsurancePolicyNumber =
          db.GetNullableString(reader, 3);
        entities.HealthInsuranceCoverage.PolicyExpirationDate =
          db.GetNullableDate(reader, 4);
        entities.HealthInsuranceCoverage.CoverageCode1 =
          db.GetNullableString(reader, 5);
        entities.HealthInsuranceCoverage.CoverageCode2 =
          db.GetNullableString(reader, 6);
        entities.HealthInsuranceCoverage.CoverageCode3 =
          db.GetNullableString(reader, 7);
        entities.HealthInsuranceCoverage.CoverageCode4 =
          db.GetNullableString(reader, 8);
        entities.HealthInsuranceCoverage.CoverageCode5 =
          db.GetNullableString(reader, 9);
        entities.HealthInsuranceCoverage.CoverageCode6 =
          db.GetNullableString(reader, 10);
        entities.HealthInsuranceCoverage.CoverageCode7 =
          db.GetNullableString(reader, 11);
        entities.HealthInsuranceCoverage.PolicyEffectiveDate =
          db.GetNullableDate(reader, 12);
        entities.HealthInsuranceCoverage.CspHNumber =
          db.GetNullableString(reader, 13);
        entities.HealthInsuranceCoverage.ConHNumber =
          db.GetNullableInt32(reader, 14);
        entities.HealthInsuranceCoverage.CspNumber =
          db.GetNullableString(reader, 15);
        entities.HealthInsuranceCoverage.HicIdentifier =
          db.GetNullableInt32(reader, 16);
        entities.HealthInsuranceCoverage.OtherCoveredPersons =
          db.GetNullableString(reader, 17);
        entities.HealthInsuranceCoverage.Populated = true;
      });
  }

  private IEnumerable<bool> ReadPersonalHealthInsuranceCsePerson()
  {
    return ReadEach("ReadPersonalHealthInsuranceCsePerson",
      (db, command) =>
      {
        db.SetInt64(
          command, "hcvId", entities.HealthInsuranceCoverage.Identifier);
      },
      (db, reader) =>
      {
        if (export.Export1.IsFull)
        {
          return false;
        }

        entities.PersonalHealthInsurance.HcvId = db.GetInt64(reader, 0);
        entities.PersonalHealthInsurance.CspNumber = db.GetString(reader, 1);
        entities.Insured.Number = db.GetString(reader, 1);
        entities.PersonalHealthInsurance.CoverageVerifiedDate =
          db.GetNullableDate(reader, 2);
        entities.PersonalHealthInsurance.AlertFlagInsuranceExistsInd =
          db.GetNullableString(reader, 3);
        entities.PersonalHealthInsurance.CoverageCostAmount =
          db.GetNullableDecimal(reader, 4);
        entities.PersonalHealthInsurance.CoverageBeginDate =
          db.GetNullableDate(reader, 5);
        entities.PersonalHealthInsurance.CoverageEndDate =
          db.GetNullableDate(reader, 6);
        entities.PersonalHealthInsurance.PremiumVerifiedDate =
          db.GetNullableDate(reader, 7);
        entities.Insured.Populated = true;
        entities.PersonalHealthInsurance.Populated = true;

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
    /// <summary>
    /// A value of HolderCase.
    /// </summary>
    [JsonPropertyName("holderCase")]
    public Case1 HolderCase
    {
      get => holderCase ??= new();
      set => holderCase = value;
    }

    /// <summary>
    /// A value of HolderCsePerson.
    /// </summary>
    [JsonPropertyName("holderCsePerson")]
    public CsePerson HolderCsePerson
    {
      get => holderCsePerson ??= new();
      set => holderCsePerson = value;
    }

    /// <summary>
    /// A value of HolderHealthInsuranceCompany.
    /// </summary>
    [JsonPropertyName("holderHealthInsuranceCompany")]
    public HealthInsuranceCompany HolderHealthInsuranceCompany
    {
      get => holderHealthInsuranceCompany ??= new();
      set => holderHealthInsuranceCompany = value;
    }

    /// <summary>
    /// A value of HolderHealthInsuranceCoverage.
    /// </summary>
    [JsonPropertyName("holderHealthInsuranceCoverage")]
    public HealthInsuranceCoverage HolderHealthInsuranceCoverage
    {
      get => holderHealthInsuranceCoverage ??= new();
      set => holderHealthInsuranceCoverage = value;
    }

    /// <summary>
    /// A value of HolderContact.
    /// </summary>
    [JsonPropertyName("holderContact")]
    public Contact HolderContact
    {
      get => holderContact ??= new();
      set => holderContact = value;
    }

    private Case1 holderCase;
    private CsePerson holderCsePerson;
    private HealthInsuranceCompany holderHealthInsuranceCompany;
    private HealthInsuranceCoverage holderHealthInsuranceCoverage;
    private Contact holderContact;
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
      /// A value of Prompt.
      /// </summary>
      [JsonPropertyName("prompt")]
      public Common Prompt
      {
        get => prompt ??= new();
        set => prompt = value;
      }

      /// <summary>
      /// A value of H.
      /// </summary>
      [JsonPropertyName("h")]
      public PersonalHealthInsurance H
      {
        get => h ??= new();
        set => h = value;
      }

      /// <summary>
      /// A value of InsuredH.
      /// </summary>
      [JsonPropertyName("insuredH")]
      public CsePerson InsuredH
      {
        get => insuredH ??= new();
        set => insuredH = value;
      }

      /// <summary>
      /// A value of DetailsCaseRole.
      /// </summary>
      [JsonPropertyName("detailsCaseRole")]
      public CaseRole DetailsCaseRole
      {
        get => detailsCaseRole ??= new();
        set => detailsCaseRole = value;
      }

      /// <summary>
      /// A value of Detail.
      /// </summary>
      [JsonPropertyName("detail")]
      public Common Detail
      {
        get => detail ??= new();
        set => detail = value;
      }

      /// <summary>
      /// A value of Insured.
      /// </summary>
      [JsonPropertyName("insured")]
      public CsePerson Insured
      {
        get => insured ??= new();
        set => insured = value;
      }

      /// <summary>
      /// A value of InsuredName.
      /// </summary>
      [JsonPropertyName("insuredName")]
      public OeWorkGroup InsuredName
      {
        get => insuredName ??= new();
        set => insuredName = value;
      }

      /// <summary>
      /// A value of DetailsPersonalHealthInsurance.
      /// </summary>
      [JsonPropertyName("detailsPersonalHealthInsurance")]
      public PersonalHealthInsurance DetailsPersonalHealthInsurance
      {
        get => detailsPersonalHealthInsurance ??= new();
        set => detailsPersonalHealthInsurance = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 20;

      private Common prompt;
      private PersonalHealthInsurance h;
      private CsePerson insuredH;
      private CaseRole detailsCaseRole;
      private Common detail;
      private CsePerson insured;
      private OeWorkGroup insuredName;
      private PersonalHealthInsurance detailsPersonalHealthInsurance;
    }

    /// <summary>
    /// A value of HolderHCsePerson.
    /// </summary>
    [JsonPropertyName("holderHCsePerson")]
    public CsePerson HolderHCsePerson
    {
      get => holderHCsePerson ??= new();
      set => holderHCsePerson = value;
    }

    /// <summary>
    /// A value of HolderHCase.
    /// </summary>
    [JsonPropertyName("holderHCase")]
    public Case1 HolderHCase
    {
      get => holderHCase ??= new();
      set => holderHCase = value;
    }

    /// <summary>
    /// A value of HolderHHealthInsuranceCoverage.
    /// </summary>
    [JsonPropertyName("holderHHealthInsuranceCoverage")]
    public HealthInsuranceCoverage HolderHHealthInsuranceCoverage
    {
      get => holderHHealthInsuranceCoverage ??= new();
      set => holderHHealthInsuranceCoverage = value;
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
    /// A value of HolderCase.
    /// </summary>
    [JsonPropertyName("holderCase")]
    public Case1 HolderCase
    {
      get => holderCase ??= new();
      set => holderCase = value;
    }

    /// <summary>
    /// A value of HolderCsePerson.
    /// </summary>
    [JsonPropertyName("holderCsePerson")]
    public CsePerson HolderCsePerson
    {
      get => holderCsePerson ??= new();
      set => holderCsePerson = value;
    }

    /// <summary>
    /// A value of HolderHealthInsuranceCompany.
    /// </summary>
    [JsonPropertyName("holderHealthInsuranceCompany")]
    public HealthInsuranceCompany HolderHealthInsuranceCompany
    {
      get => holderHealthInsuranceCompany ??= new();
      set => holderHealthInsuranceCompany = value;
    }

    /// <summary>
    /// A value of HolderHealthInsuranceCoverage.
    /// </summary>
    [JsonPropertyName("holderHealthInsuranceCoverage")]
    public HealthInsuranceCoverage HolderHealthInsuranceCoverage
    {
      get => holderHealthInsuranceCoverage ??= new();
      set => holderHealthInsuranceCoverage = value;
    }

    /// <summary>
    /// A value of HolderContact.
    /// </summary>
    [JsonPropertyName("holderContact")]
    public Contact HolderContact
    {
      get => holderContact ??= new();
      set => holderContact = value;
    }

    private CsePerson holderHCsePerson;
    private Case1 holderHCase;
    private HealthInsuranceCoverage holderHHealthInsuranceCoverage;
    private Array<ExportGroup> export1;
    private Case1 holderCase;
    private CsePerson holderCsePerson;
    private HealthInsuranceCompany holderHealthInsuranceCompany;
    private HealthInsuranceCoverage holderHealthInsuranceCoverage;
    private Contact holderContact;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of New1.
    /// </summary>
    [JsonPropertyName("new1")]
    public CsePersonsWorkSet New1
    {
      get => new1 ??= new();
      set => new1 = value;
    }

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
    /// A value of OeWorkGroup.
    /// </summary>
    [JsonPropertyName("oeWorkGroup")]
    public OeWorkGroup OeWorkGroup
    {
      get => oeWorkGroup ??= new();
      set => oeWorkGroup = value;
    }

    private CsePersonsWorkSet new1;
    private CsePersonsWorkSet csePersonsWorkSet;
    private OeWorkGroup oeWorkGroup;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of Responsible.
    /// </summary>
    [JsonPropertyName("responsible")]
    public CsePerson Responsible
    {
      get => responsible ??= new();
      set => responsible = value;
    }

    /// <summary>
    /// A value of Insured.
    /// </summary>
    [JsonPropertyName("insured")]
    public CsePerson Insured
    {
      get => insured ??= new();
      set => insured = value;
    }

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
    /// A value of PersonalHealthInsurance.
    /// </summary>
    [JsonPropertyName("personalHealthInsurance")]
    public PersonalHealthInsurance PersonalHealthInsurance
    {
      get => personalHealthInsurance ??= new();
      set => personalHealthInsurance = value;
    }

    /// <summary>
    /// A value of HealthInsuranceCompany.
    /// </summary>
    [JsonPropertyName("healthInsuranceCompany")]
    public HealthInsuranceCompany HealthInsuranceCompany
    {
      get => healthInsuranceCompany ??= new();
      set => healthInsuranceCompany = value;
    }

    /// <summary>
    /// A value of CaseRole.
    /// </summary>
    [JsonPropertyName("caseRole")]
    public CaseRole CaseRole
    {
      get => caseRole ??= new();
      set => caseRole = value;
    }

    /// <summary>
    /// A value of Case1.
    /// </summary>
    [JsonPropertyName("case1")]
    public Case1 Case1
    {
      get => case1 ??= new();
      set => case1 = value;
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

    private CsePerson responsible;
    private CsePerson insured;
    private HealthInsuranceCoverage healthInsuranceCoverage;
    private PersonalHealthInsurance personalHealthInsurance;
    private HealthInsuranceCompany healthInsuranceCompany;
    private CaseRole caseRole;
    private Case1 case1;
    private Contact contact;
  }
#endregion
}
