// Program: OE_UPDATE_HEALTH_INS_COMPANY, ID: 371861930, model: 746.
// Short name: SWE00968
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: OE_UPDATE_HEALTH_INS_COMPANY.
/// </summary>
[Serializable]
public partial class OeUpdateHealthInsCompany: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_UPDATE_HEALTH_INS_COMPANY program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeUpdateHealthInsCompany(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeUpdateHealthInsCompany.
  /// </summary>
  public OeUpdateHealthInsCompany(IContext context, Import import, Export export)
    :
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
    // Date      Author          Reason
    // Jan 1995  Rebecca Grimes  Initial Development
    // 02/06/95  Sid             Rework
    // 11/14/02  MCA             Added code to end date health insurance 
    // coverage and
    //                           personal health insurance when the  health 
    // insurance company
    //                           is end dated.
    // ---------------------------------------------
    export.HealthInsuranceCompany.Assign(import.HealthInsuranceCompany);
    export.HealthInsuranceCompanyAddress.Assign(
      import.HealthInsuranceCompanyAddress);

    // ---------------------------------------------
    // The mailing address is the only address
    // needed to be displayed. So we set a local
    // attribute and set the address type to MAILING
    // and put this attribute in the search cond.
    // ---------------------------------------------
    UseOeCabSetMnemonics();

    // ---------------------------------------------
    // The MEDI Carrier Code if present must be
    // unique.
    // ---------------------------------------------
    if (!IsEmpty(import.HealthInsuranceCompany.CarrierCode))
    {
      foreach(var item in ReadHealthInsuranceCompany3())
      {
        if (entities.HealthInsuranceCompany.Identifier != import
          .HealthInsuranceCompany.Identifier)
        {
          ExitState = "OE0000_CARRIER_CODE_AE";

          return;
        }
      }
    }

    // ---------------------------------------------
    // Before updating a health insurance company
    // record, verify that a record does not already
    // exist for a combination of Company Name,
    // Street Address line 1 and line 2, City, and
    // State.  These values cannot be duplicated in
    // the database but are not identifiers.
    // ---------------------------------------------
    if (ReadHealthInsuranceCompany2())
    {
      if (entities.HealthInsuranceCompany.Identifier != import
        .HealthInsuranceCompany.Identifier)
      {
        ExitState = "HEALTH_INSURANCE_COMPANY_AE_RB";

        return;
      }
    }

    // ---------------------------------------------
    // Read the health insurance company record to
    // be updated.  Then update the record with the
    // new values.
    // ---------------------------------------------
    if (ReadHealthInsuranceCompany1())
    {
      try
      {
        UpdateHealthInsuranceCompany();
        ExitState = "ACO_NI0000_SUCCESSFUL_UPDATE";
        export.HealthInsuranceCompany.Assign(entities.HealthInsuranceCompany);

        // ---------------------------------------------
        // Update the Health Insurance Carrier Address
        // record for the health insurance company with
        // the new values.
        // ---------------------------------------------
        if (ReadHealthInsuranceCompanyAddress())
        {
          try
          {
            UpdateHealthInsuranceCompanyAddress();
            ExitState = "ACO_NI0000_SUCCESSFUL_UPDATE";
            export.HealthInsuranceCompanyAddress.Assign(
              entities.HealthInsuranceCompanyAddress);
          }
          catch(Exception e1)
          {
            switch(GetErrorCode(e1))
            {
              case ErrorCode.AlreadyExists:
                ExitState = "HEALTH_INSURANCE_CO_ADDR_NU_RB";

                break;
              case ErrorCode.PermittedValueViolation:
                ExitState = "HEALTH_INSURANCE_CARRIER_PV_RB";

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
          ExitState = "HEALTH_INSURANCE_CO_ADDR_NF_RB";
        }
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "HEALTH_INSURANCE_COMPANY_NU_RB";

            break;
          case ErrorCode.PermittedValueViolation:
            ExitState = "HEALTH_INSURANCE_COMPANY_PV_RB";

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
      ExitState = "HEALTH_INSURANCE_COMPANY_NF_RB";
    }
  }

  private void UseOeCabSetMnemonics()
  {
    var useImport = new OeCabSetMnemonics.Import();
    var useExport = new OeCabSetMnemonics.Export();

    Call(OeCabSetMnemonics.Execute, useImport, useExport);

    local.MailingAddressType.AddressType =
      useExport.MailingAddressType.AddressType;
  }

  private bool ReadHealthInsuranceCompany1()
  {
    entities.HealthInsuranceCompany.Populated = false;

    return Read("ReadHealthInsuranceCompany1",
      (db, command) =>
      {
        db.SetInt32(
          command, "identifier", import.HealthInsuranceCompany.Identifier);
      },
      (db, reader) =>
      {
        entities.HealthInsuranceCompany.Identifier = db.GetInt32(reader, 0);
        entities.HealthInsuranceCompany.CarrierCode =
          db.GetNullableString(reader, 1);
        entities.HealthInsuranceCompany.InsurancePolicyCarrier =
          db.GetNullableString(reader, 2);
        entities.HealthInsuranceCompany.ContactName =
          db.GetNullableString(reader, 3);
        entities.HealthInsuranceCompany.InsurerPhone =
          db.GetNullableInt32(reader, 4);
        entities.HealthInsuranceCompany.InsurerFax =
          db.GetNullableInt32(reader, 5);
        entities.HealthInsuranceCompany.CreatedBy = db.GetString(reader, 6);
        entities.HealthInsuranceCompany.CreatedTimestamp =
          db.GetDateTime(reader, 7);
        entities.HealthInsuranceCompany.LastUpdatedBy = db.GetString(reader, 8);
        entities.HealthInsuranceCompany.LastUpdatedTimestamp =
          db.GetDateTime(reader, 9);
        entities.HealthInsuranceCompany.InsurerFaxExt =
          db.GetNullableString(reader, 10);
        entities.HealthInsuranceCompany.InsurerPhoneExt =
          db.GetNullableString(reader, 11);
        entities.HealthInsuranceCompany.InsurerPhoneAreaCode =
          db.GetNullableInt32(reader, 12);
        entities.HealthInsuranceCompany.InsurerFaxAreaCode =
          db.GetNullableInt32(reader, 13);
        entities.HealthInsuranceCompany.StartDate = db.GetDate(reader, 14);
        entities.HealthInsuranceCompany.EndDate = db.GetDate(reader, 15);
        entities.HealthInsuranceCompany.Populated = true;
      });
  }

  private bool ReadHealthInsuranceCompany2()
  {
    entities.HealthInsuranceCompany.Populated = false;

    return Read("ReadHealthInsuranceCompany2",
      (db, command) =>
      {
        db.SetNullableString(
          command, "policyCarrier",
          import.HealthInsuranceCompany.InsurancePolicyCarrier ?? "");
        db.SetNullableString(
          command, "street1", import.HealthInsuranceCompanyAddress.Street1 ?? ""
          );
        db.SetNullableString(
          command, "street2", import.HealthInsuranceCompanyAddress.Street2 ?? ""
          );
        db.SetNullableString(
          command, "city", import.HealthInsuranceCompanyAddress.City ?? "");
        db.SetNullableString(
          command, "state", import.HealthInsuranceCompanyAddress.State ?? "");
      },
      (db, reader) =>
      {
        entities.HealthInsuranceCompany.Identifier = db.GetInt32(reader, 0);
        entities.HealthInsuranceCompany.CarrierCode =
          db.GetNullableString(reader, 1);
        entities.HealthInsuranceCompany.InsurancePolicyCarrier =
          db.GetNullableString(reader, 2);
        entities.HealthInsuranceCompany.ContactName =
          db.GetNullableString(reader, 3);
        entities.HealthInsuranceCompany.InsurerPhone =
          db.GetNullableInt32(reader, 4);
        entities.HealthInsuranceCompany.InsurerFax =
          db.GetNullableInt32(reader, 5);
        entities.HealthInsuranceCompany.CreatedBy = db.GetString(reader, 6);
        entities.HealthInsuranceCompany.CreatedTimestamp =
          db.GetDateTime(reader, 7);
        entities.HealthInsuranceCompany.LastUpdatedBy = db.GetString(reader, 8);
        entities.HealthInsuranceCompany.LastUpdatedTimestamp =
          db.GetDateTime(reader, 9);
        entities.HealthInsuranceCompany.InsurerFaxExt =
          db.GetNullableString(reader, 10);
        entities.HealthInsuranceCompany.InsurerPhoneExt =
          db.GetNullableString(reader, 11);
        entities.HealthInsuranceCompany.InsurerPhoneAreaCode =
          db.GetNullableInt32(reader, 12);
        entities.HealthInsuranceCompany.InsurerFaxAreaCode =
          db.GetNullableInt32(reader, 13);
        entities.HealthInsuranceCompany.StartDate = db.GetDate(reader, 14);
        entities.HealthInsuranceCompany.EndDate = db.GetDate(reader, 15);
        entities.HealthInsuranceCompany.Populated = true;
      });
  }

  private IEnumerable<bool> ReadHealthInsuranceCompany3()
  {
    entities.HealthInsuranceCompany.Populated = false;

    return ReadEach("ReadHealthInsuranceCompany3",
      (db, command) =>
      {
        db.SetNullableString(
          command, "carrierCode", import.HealthInsuranceCompany.CarrierCode ?? ""
          );
      },
      (db, reader) =>
      {
        entities.HealthInsuranceCompany.Identifier = db.GetInt32(reader, 0);
        entities.HealthInsuranceCompany.CarrierCode =
          db.GetNullableString(reader, 1);
        entities.HealthInsuranceCompany.InsurancePolicyCarrier =
          db.GetNullableString(reader, 2);
        entities.HealthInsuranceCompany.ContactName =
          db.GetNullableString(reader, 3);
        entities.HealthInsuranceCompany.InsurerPhone =
          db.GetNullableInt32(reader, 4);
        entities.HealthInsuranceCompany.InsurerFax =
          db.GetNullableInt32(reader, 5);
        entities.HealthInsuranceCompany.CreatedBy = db.GetString(reader, 6);
        entities.HealthInsuranceCompany.CreatedTimestamp =
          db.GetDateTime(reader, 7);
        entities.HealthInsuranceCompany.LastUpdatedBy = db.GetString(reader, 8);
        entities.HealthInsuranceCompany.LastUpdatedTimestamp =
          db.GetDateTime(reader, 9);
        entities.HealthInsuranceCompany.InsurerFaxExt =
          db.GetNullableString(reader, 10);
        entities.HealthInsuranceCompany.InsurerPhoneExt =
          db.GetNullableString(reader, 11);
        entities.HealthInsuranceCompany.InsurerPhoneAreaCode =
          db.GetNullableInt32(reader, 12);
        entities.HealthInsuranceCompany.InsurerFaxAreaCode =
          db.GetNullableInt32(reader, 13);
        entities.HealthInsuranceCompany.StartDate = db.GetDate(reader, 14);
        entities.HealthInsuranceCompany.EndDate = db.GetDate(reader, 15);
        entities.HealthInsuranceCompany.Populated = true;

        return true;
      });
  }

  private bool ReadHealthInsuranceCompanyAddress()
  {
    entities.HealthInsuranceCompanyAddress.Populated = false;

    return Read("ReadHealthInsuranceCompanyAddress",
      (db, command) =>
      {
        db.SetInt32(
          command, "hicIdentifier", entities.HealthInsuranceCompany.Identifier);
          
        db.SetNullableString(
          command, "addressType", local.MailingAddressType.AddressType ?? "");
      },
      (db, reader) =>
      {
        entities.HealthInsuranceCompanyAddress.HicIdentifier =
          db.GetInt32(reader, 0);
        entities.HealthInsuranceCompanyAddress.EffectiveDate =
          db.GetDate(reader, 1);
        entities.HealthInsuranceCompanyAddress.Street1 =
          db.GetNullableString(reader, 2);
        entities.HealthInsuranceCompanyAddress.Street2 =
          db.GetNullableString(reader, 3);
        entities.HealthInsuranceCompanyAddress.City =
          db.GetNullableString(reader, 4);
        entities.HealthInsuranceCompanyAddress.State =
          db.GetNullableString(reader, 5);
        entities.HealthInsuranceCompanyAddress.Province =
          db.GetNullableString(reader, 6);
        entities.HealthInsuranceCompanyAddress.PostalCode =
          db.GetNullableString(reader, 7);
        entities.HealthInsuranceCompanyAddress.ZipCode5 =
          db.GetNullableString(reader, 8);
        entities.HealthInsuranceCompanyAddress.ZipCode4 =
          db.GetNullableString(reader, 9);
        entities.HealthInsuranceCompanyAddress.Zip3 =
          db.GetNullableString(reader, 10);
        entities.HealthInsuranceCompanyAddress.Country =
          db.GetNullableString(reader, 11);
        entities.HealthInsuranceCompanyAddress.AddressType =
          db.GetNullableString(reader, 12);
        entities.HealthInsuranceCompanyAddress.CreatedBy =
          db.GetString(reader, 13);
        entities.HealthInsuranceCompanyAddress.CreatedTimestamp =
          db.GetDateTime(reader, 14);
        entities.HealthInsuranceCompanyAddress.LastUpdatedBy =
          db.GetString(reader, 15);
        entities.HealthInsuranceCompanyAddress.LastUpdatedTimestamp =
          db.GetDateTime(reader, 16);
        entities.HealthInsuranceCompanyAddress.Populated = true;
      });
  }

  private void UpdateHealthInsuranceCompany()
  {
    var carrierCode = import.HealthInsuranceCompany.CarrierCode ?? "";
    var insurancePolicyCarrier =
      import.HealthInsuranceCompany.InsurancePolicyCarrier ?? "";
    var contactName = import.HealthInsuranceCompany.ContactName ?? "";
    var insurerPhone =
      import.HealthInsuranceCompany.InsurerPhone.GetValueOrDefault();
    var insurerFax =
      import.HealthInsuranceCompany.InsurerFax.GetValueOrDefault();
    var lastUpdatedBy = global.UserId;
    var lastUpdatedTimestamp = Now();
    var insurerFaxExt = import.HealthInsuranceCompany.InsurerFaxExt ?? "";
    var insurerPhoneExt = import.HealthInsuranceCompany.InsurerPhoneExt ?? "";
    var insurerPhoneAreaCode =
      import.HealthInsuranceCompany.InsurerPhoneAreaCode.GetValueOrDefault();
    var insurerFaxAreaCode =
      import.HealthInsuranceCompany.InsurerFaxAreaCode.GetValueOrDefault();
    var startDate = import.HealthInsuranceCompany.StartDate;
    var endDate = import.HealthInsuranceCompany.EndDate;

    entities.HealthInsuranceCompany.Populated = false;
    Update("UpdateHealthInsuranceCompany",
      (db, command) =>
      {
        db.SetNullableString(command, "carrierCode", carrierCode);
        db.SetNullableString(command, "policyCarrier", insurancePolicyCarrier);
        db.SetNullableString(command, "contactName", contactName);
        db.SetNullableInt32(command, "insurerPhone", insurerPhone);
        db.SetNullableInt32(command, "insurerFax", insurerFax);
        db.SetString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetDateTime(command, "lastUpdatedTmst", lastUpdatedTimestamp);
        db.SetNullableString(command, "insurerFaxExt", insurerFaxExt);
        db.SetNullableString(command, "insurerPhoneExt", insurerPhoneExt);
        db.SetNullableInt32(command, "insurerPhArea", insurerPhoneAreaCode);
        db.SetNullableInt32(command, "insurerFaxArea", insurerFaxAreaCode);
        db.SetDate(command, "startDate", startDate);
        db.SetDate(command, "endDate", endDate);
        db.SetInt32(
          command, "identifier", entities.HealthInsuranceCompany.Identifier);
      });

    entities.HealthInsuranceCompany.CarrierCode = carrierCode;
    entities.HealthInsuranceCompany.InsurancePolicyCarrier =
      insurancePolicyCarrier;
    entities.HealthInsuranceCompany.ContactName = contactName;
    entities.HealthInsuranceCompany.InsurerPhone = insurerPhone;
    entities.HealthInsuranceCompany.InsurerFax = insurerFax;
    entities.HealthInsuranceCompany.LastUpdatedBy = lastUpdatedBy;
    entities.HealthInsuranceCompany.LastUpdatedTimestamp = lastUpdatedTimestamp;
    entities.HealthInsuranceCompany.InsurerFaxExt = insurerFaxExt;
    entities.HealthInsuranceCompany.InsurerPhoneExt = insurerPhoneExt;
    entities.HealthInsuranceCompany.InsurerPhoneAreaCode = insurerPhoneAreaCode;
    entities.HealthInsuranceCompany.InsurerFaxAreaCode = insurerFaxAreaCode;
    entities.HealthInsuranceCompany.StartDate = startDate;
    entities.HealthInsuranceCompany.EndDate = endDate;
    entities.HealthInsuranceCompany.Populated = true;
  }

  private void UpdateHealthInsuranceCompanyAddress()
  {
    System.Diagnostics.Debug.Assert(
      entities.HealthInsuranceCompanyAddress.Populated);

    var street1 = import.HealthInsuranceCompanyAddress.Street1 ?? "";
    var street2 = import.HealthInsuranceCompanyAddress.Street2 ?? "";
    var city = import.HealthInsuranceCompanyAddress.City ?? "";
    var state = import.HealthInsuranceCompanyAddress.State ?? "";
    var zipCode5 = import.HealthInsuranceCompanyAddress.ZipCode5 ?? "";
    var zipCode4 = import.HealthInsuranceCompanyAddress.ZipCode4 ?? "";
    var zip3 = import.HealthInsuranceCompanyAddress.Zip3 ?? "";
    var country = import.HealthInsuranceCompanyAddress.Country ?? "";
    var lastUpdatedBy = global.UserId;
    var lastUpdatedTimestamp = Now();

    entities.HealthInsuranceCompanyAddress.Populated = false;
    Update("UpdateHealthInsuranceCompanyAddress",
      (db, command) =>
      {
        db.SetNullableString(command, "street1", street1);
        db.SetNullableString(command, "street2", street2);
        db.SetNullableString(command, "city", city);
        db.SetNullableString(command, "state", state);
        db.SetNullableString(command, "zipCode5", zipCode5);
        db.SetNullableString(command, "zipCode4", zipCode4);
        db.SetNullableString(command, "zip3", zip3);
        db.SetNullableString(command, "country", country);
        db.SetString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetDateTime(command, "lastUpdatedTmst", lastUpdatedTimestamp);
        db.SetInt32(
          command, "hicIdentifier",
          entities.HealthInsuranceCompanyAddress.HicIdentifier);
        db.SetDate(
          command, "effectiveDate",
          entities.HealthInsuranceCompanyAddress.EffectiveDate.
            GetValueOrDefault());
      });

    entities.HealthInsuranceCompanyAddress.Street1 = street1;
    entities.HealthInsuranceCompanyAddress.Street2 = street2;
    entities.HealthInsuranceCompanyAddress.City = city;
    entities.HealthInsuranceCompanyAddress.State = state;
    entities.HealthInsuranceCompanyAddress.ZipCode5 = zipCode5;
    entities.HealthInsuranceCompanyAddress.ZipCode4 = zipCode4;
    entities.HealthInsuranceCompanyAddress.Zip3 = zip3;
    entities.HealthInsuranceCompanyAddress.Country = country;
    entities.HealthInsuranceCompanyAddress.LastUpdatedBy = lastUpdatedBy;
    entities.HealthInsuranceCompanyAddress.LastUpdatedTimestamp =
      lastUpdatedTimestamp;
    entities.HealthInsuranceCompanyAddress.Populated = true;
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
    /// A value of HealthInsuranceCompanyAddress.
    /// </summary>
    [JsonPropertyName("healthInsuranceCompanyAddress")]
    public HealthInsuranceCompanyAddress HealthInsuranceCompanyAddress
    {
      get => healthInsuranceCompanyAddress ??= new();
      set => healthInsuranceCompanyAddress = value;
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

    private HealthInsuranceCompanyAddress healthInsuranceCompanyAddress;
    private HealthInsuranceCompany healthInsuranceCompany;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of HealthInsuranceCompanyAddress.
    /// </summary>
    [JsonPropertyName("healthInsuranceCompanyAddress")]
    public HealthInsuranceCompanyAddress HealthInsuranceCompanyAddress
    {
      get => healthInsuranceCompanyAddress ??= new();
      set => healthInsuranceCompanyAddress = value;
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

    private HealthInsuranceCompanyAddress healthInsuranceCompanyAddress;
    private HealthInsuranceCompany healthInsuranceCompany;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
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
    /// A value of Infrastructure.
    /// </summary>
    [JsonPropertyName("infrastructure")]
    public Infrastructure Infrastructure
    {
      get => infrastructure ??= new();
      set => infrastructure = value;
    }

    /// <summary>
    /// A value of MailingAddressType.
    /// </summary>
    [JsonPropertyName("mailingAddressType")]
    public HealthInsuranceCompanyAddress MailingAddressType
    {
      get => mailingAddressType ??= new();
      set => mailingAddressType = value;
    }

    /// <summary>
    /// A value of CsePersons.
    /// </summary>
    [JsonPropertyName("csePersons")]
    public CsePersonsWorkSet CsePersons
    {
      get => csePersons ??= new();
      set => csePersons = value;
    }

    /// <summary>
    /// A value of Document.
    /// </summary>
    [JsonPropertyName("document")]
    public Document Document
    {
      get => document ??= new();
      set => document = value;
    }

    /// <summary>
    /// A value of SpDocKey.
    /// </summary>
    [JsonPropertyName("spDocKey")]
    public SpDocKey SpDocKey
    {
      get => spDocKey ??= new();
      set => spDocKey = value;
    }

    /// <summary>
    /// A value of FindDoc.
    /// </summary>
    [JsonPropertyName("findDoc")]
    public Common FindDoc
    {
      get => findDoc ??= new();
      set => findDoc = value;
    }

    private HealthInsuranceCoverage healthInsuranceCoverage;
    private Infrastructure infrastructure;
    private HealthInsuranceCompanyAddress mailingAddressType;
    private CsePersonsWorkSet csePersons;
    private Document document;
    private SpDocKey spDocKey;
    private Common findDoc;
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
    /// A value of ApplicantRecipient.
    /// </summary>
    [JsonPropertyName("applicantRecipient")]
    public CaseRole ApplicantRecipient
    {
      get => applicantRecipient ??= new();
      set => applicantRecipient = value;
    }

    /// <summary>
    /// A value of ChildCase.
    /// </summary>
    [JsonPropertyName("childCase")]
    public Case1 ChildCase
    {
      get => childCase ??= new();
      set => childCase = value;
    }

    /// <summary>
    /// A value of Child.
    /// </summary>
    [JsonPropertyName("child")]
    public CaseRole Child
    {
      get => child ??= new();
      set => child = value;
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
    /// A value of ChildCsePerson.
    /// </summary>
    [JsonPropertyName("childCsePerson")]
    public CsePerson ChildCsePerson
    {
      get => childCsePerson ??= new();
      set => childCsePerson = value;
    }

    /// <summary>
    /// A value of HealthInsuranceCompanyAddress.
    /// </summary>
    [JsonPropertyName("healthInsuranceCompanyAddress")]
    public HealthInsuranceCompanyAddress HealthInsuranceCompanyAddress
    {
      get => healthInsuranceCompanyAddress ??= new();
      set => healthInsuranceCompanyAddress = value;
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
    /// A value of PersonalHealthInsurance.
    /// </summary>
    [JsonPropertyName("personalHealthInsurance")]
    public PersonalHealthInsurance PersonalHealthInsurance
    {
      get => personalHealthInsurance ??= new();
      set => personalHealthInsurance = value;
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

    private CsePerson responsible;
    private CaseRole applicantRecipient;
    private Case1 childCase;
    private CaseRole child;
    private CaseRole caseRole;
    private CsePerson childCsePerson;
    private HealthInsuranceCompanyAddress healthInsuranceCompanyAddress;
    private HealthInsuranceCompany healthInsuranceCompany;
    private PersonalHealthInsurance personalHealthInsurance;
    private HealthInsuranceCoverage healthInsuranceCoverage;
  }
#endregion
}
