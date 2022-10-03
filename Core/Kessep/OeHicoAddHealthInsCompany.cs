// Program: OE_HICO_ADD_HEALTH_INS_COMPANY, ID: 371861923, model: 746.
// Short name: SWE00927
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: OE_HICO_ADD_HEALTH_INS_COMPANY.
/// </para>
/// <para>
/// Resp:OBLGEST
/// </para>
/// </summary>
[Serializable]
public partial class OeHicoAddHealthInsCompany: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_HICO_ADD_HEALTH_INS_COMPANY program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeHicoAddHealthInsCompany(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeHicoAddHealthInsCompany.
  /// </summary>
  public OeHicoAddHealthInsCompany(IContext context, Import import,
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
    // Date      Author          Reason
    // Jan 1995  Rebecca Grimes  Initial Development
    // 02/02/94  Sid             Rework  and
    //                                   
    // Completion.
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
    // The MEDI Carrier Code if present must be unique.
    // ---------------------------------------------
    if (!IsEmpty(import.HealthInsuranceCompany.CarrierCode))
    {
      if (ReadHealthInsuranceCompany1())
      {
        ExitState = "OE0000_CARRIER_CODE_AE";

        return;
      }
    }

    // ---------------------------------------------
    // Before creating a health insurance company
    // record, verify that a record does not already
    // exist for a combination of Company Name,
    // Street Address line 1 and line 2, City, and
    // State.  These values cannot be duplicated in
    // the database but are not identifiers.
    // ---------------------------------------------
    if (ReadHealthInsuranceCompany2())
    {
      ExitState = "HEALTH_INSURANCE_COMPANY_AE_RB";

      return;
    }

    // ---------------------------------------------
    // The health Insurance Carrier does not exists.
    // Set the identifier to the last identifier
    // number plus one.
    // ---------------------------------------------
    export.HealthInsuranceCompany.Identifier = 0;
    ReadHealthInsuranceCompany3();
    export.HealthInsuranceCompany.Identifier =
      entities.HealthInsuranceCompany.Identifier + 1;

    // ---------------------------------------------
    // Create the health insurance company record.
    // ---------------------------------------------
    try
    {
      CreateHealthInsuranceCompany();
      ExitState = "ACO_NI0000_SUCCESSFUL_ADD";
      export.HealthInsuranceCompany.Assign(entities.HealthInsuranceCompany);

      // ---------------------------------------------
      // Create the health Insurance company address
      // record and associate it to the health
      // insurance company record.
      // ---------------------------------------------
      try
      {
        CreateHealthInsuranceCompanyAddress();
        export.HealthInsuranceCompanyAddress.Assign(
          entities.HealthInsuranceCompanyAddress);
        ExitState = "ACO_NI0000_SUCCESSFUL_ADD";
      }
      catch(Exception e1)
      {
        switch(GetErrorCode(e1))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "INSURANCE_COMPANY_ADDRESS_AE";

            break;
          case ErrorCode.PermittedValueViolation:
            ExitState = "INSURANCE_COMPANY_ADDRESS_PV";

            break;
          case ErrorCode.DatabaseError:
            break;
          default:
            throw;
        }
      }
    }
    catch(Exception e)
    {
      switch(GetErrorCode(e))
      {
        case ErrorCode.AlreadyExists:
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

  private void UseOeCabSetMnemonics()
  {
    var useImport = new OeCabSetMnemonics.Import();
    var useExport = new OeCabSetMnemonics.Export();

    Call(OeCabSetMnemonics.Execute, useImport, useExport);

    local.MailingAddressType.AddressType =
      useExport.MailingAddressType.AddressType;
  }

  private void CreateHealthInsuranceCompany()
  {
    var identifier = export.HealthInsuranceCompany.Identifier;
    var carrierCode = export.HealthInsuranceCompany.CarrierCode ?? "";
    var insurancePolicyCarrier =
      export.HealthInsuranceCompany.InsurancePolicyCarrier ?? "";
    var contactName = export.HealthInsuranceCompany.ContactName ?? "";
    var insurerPhone =
      export.HealthInsuranceCompany.InsurerPhone.GetValueOrDefault();
    var insurerFax =
      export.HealthInsuranceCompany.InsurerFax.GetValueOrDefault();
    var createdBy = global.UserId;
    var createdTimestamp = Now();
    var insurerFaxExt = export.HealthInsuranceCompany.InsurerFaxExt ?? "";
    var insurerPhoneExt = export.HealthInsuranceCompany.InsurerPhoneExt ?? "";
    var insurerPhoneAreaCode =
      export.HealthInsuranceCompany.InsurerPhoneAreaCode.GetValueOrDefault();
    var insurerFaxAreaCode =
      export.HealthInsuranceCompany.InsurerFaxAreaCode.GetValueOrDefault();
    var startDate = export.HealthInsuranceCompany.StartDate;
    var endDate = export.HealthInsuranceCompany.EndDate;

    entities.HealthInsuranceCompany.Populated = false;
    Update("CreateHealthInsuranceCompany",
      (db, command) =>
      {
        db.SetInt32(command, "identifier", identifier);
        db.SetNullableString(command, "carrierCode", carrierCode);
        db.SetNullableString(command, "policyCarrier", insurancePolicyCarrier);
        db.SetNullableString(command, "contactName", contactName);
        db.SetNullableString(command, "zdelContPerFrst", "");
        db.SetNullableString(command, "zdelContPerMi", "");
        db.SetNullableInt32(command, "insurerPhone", insurerPhone);
        db.SetNullableInt32(command, "insurerFax", insurerFax);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetString(command, "lastUpdatedBy", createdBy);
        db.SetDateTime(command, "lastUpdatedTmst", createdTimestamp);
        db.SetNullableString(command, "insurerFaxExt", insurerFaxExt);
        db.SetNullableString(command, "insurerPhoneExt", insurerPhoneExt);
        db.SetNullableInt32(command, "insurerPhArea", insurerPhoneAreaCode);
        db.SetNullableInt32(command, "insurerFaxArea", insurerFaxAreaCode);
        db.SetDate(command, "startDate", startDate);
        db.SetDate(command, "endDate", endDate);
      });

    entities.HealthInsuranceCompany.Identifier = identifier;
    entities.HealthInsuranceCompany.CarrierCode = carrierCode;
    entities.HealthInsuranceCompany.InsurancePolicyCarrier =
      insurancePolicyCarrier;
    entities.HealthInsuranceCompany.ContactName = contactName;
    entities.HealthInsuranceCompany.InsurerPhone = insurerPhone;
    entities.HealthInsuranceCompany.InsurerFax = insurerFax;
    entities.HealthInsuranceCompany.CreatedBy = createdBy;
    entities.HealthInsuranceCompany.CreatedTimestamp = createdTimestamp;
    entities.HealthInsuranceCompany.LastUpdatedBy = createdBy;
    entities.HealthInsuranceCompany.LastUpdatedTimestamp = createdTimestamp;
    entities.HealthInsuranceCompany.InsurerFaxExt = insurerFaxExt;
    entities.HealthInsuranceCompany.InsurerPhoneExt = insurerPhoneExt;
    entities.HealthInsuranceCompany.InsurerPhoneAreaCode = insurerPhoneAreaCode;
    entities.HealthInsuranceCompany.InsurerFaxAreaCode = insurerFaxAreaCode;
    entities.HealthInsuranceCompany.StartDate = startDate;
    entities.HealthInsuranceCompany.EndDate = endDate;
    entities.HealthInsuranceCompany.Populated = true;
  }

  private void CreateHealthInsuranceCompanyAddress()
  {
    var hicIdentifier = entities.HealthInsuranceCompany.Identifier;
    var effectiveDate = Now().Date;
    var street1 = export.HealthInsuranceCompanyAddress.Street1 ?? "";
    var street2 = export.HealthInsuranceCompanyAddress.Street2 ?? "";
    var city = export.HealthInsuranceCompanyAddress.City ?? "";
    var state = export.HealthInsuranceCompanyAddress.State ?? "";
    var province = export.HealthInsuranceCompanyAddress.Province ?? "";
    var postalCode = export.HealthInsuranceCompanyAddress.PostalCode ?? "";
    var zipCode5 = export.HealthInsuranceCompanyAddress.ZipCode5 ?? "";
    var zipCode4 = export.HealthInsuranceCompanyAddress.ZipCode4 ?? "";
    var zip3 = export.HealthInsuranceCompanyAddress.Zip3 ?? "";
    var country = export.HealthInsuranceCompanyAddress.Country ?? "";
    var addressType = local.MailingAddressType.AddressType ?? "";
    var createdBy = global.UserId;
    var createdTimestamp = Now();

    entities.HealthInsuranceCompanyAddress.Populated = false;
    Update("CreateHealthInsuranceCompanyAddress",
      (db, command) =>
      {
        db.SetInt32(command, "hicIdentifier", hicIdentifier);
        db.SetDate(command, "effectiveDate", effectiveDate);
        db.SetNullableString(command, "street1", street1);
        db.SetNullableString(command, "street2", street2);
        db.SetNullableString(command, "city", city);
        db.SetNullableString(command, "state", state);
        db.SetNullableString(command, "province", province);
        db.SetNullableString(command, "postalCode", postalCode);
        db.SetNullableString(command, "zipCode5", zipCode5);
        db.SetNullableString(command, "zipCode4", zipCode4);
        db.SetNullableString(command, "zip3", zip3);
        db.SetNullableString(command, "country", country);
        db.SetNullableString(command, "addressType", addressType);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetString(command, "lastUpdatedBy", createdBy);
        db.SetDateTime(command, "lastUpdatedTmst", createdTimestamp);
      });

    entities.HealthInsuranceCompanyAddress.HicIdentifier = hicIdentifier;
    entities.HealthInsuranceCompanyAddress.EffectiveDate = effectiveDate;
    entities.HealthInsuranceCompanyAddress.Street1 = street1;
    entities.HealthInsuranceCompanyAddress.Street2 = street2;
    entities.HealthInsuranceCompanyAddress.City = city;
    entities.HealthInsuranceCompanyAddress.State = state;
    entities.HealthInsuranceCompanyAddress.Province = province;
    entities.HealthInsuranceCompanyAddress.PostalCode = postalCode;
    entities.HealthInsuranceCompanyAddress.ZipCode5 = zipCode5;
    entities.HealthInsuranceCompanyAddress.ZipCode4 = zipCode4;
    entities.HealthInsuranceCompanyAddress.Zip3 = zip3;
    entities.HealthInsuranceCompanyAddress.Country = country;
    entities.HealthInsuranceCompanyAddress.AddressType = addressType;
    entities.HealthInsuranceCompanyAddress.CreatedBy = createdBy;
    entities.HealthInsuranceCompanyAddress.CreatedTimestamp = createdTimestamp;
    entities.HealthInsuranceCompanyAddress.LastUpdatedBy = createdBy;
    entities.HealthInsuranceCompanyAddress.LastUpdatedTimestamp =
      createdTimestamp;
    entities.HealthInsuranceCompanyAddress.Populated = true;
  }

  private bool ReadHealthInsuranceCompany1()
  {
    entities.HealthInsuranceCompany.Populated = false;

    return Read("ReadHealthInsuranceCompany1",
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

  private bool ReadHealthInsuranceCompany3()
  {
    entities.HealthInsuranceCompany.Populated = false;

    return Read("ReadHealthInsuranceCompany3",
      null,
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
    /// A value of MailingAddressType.
    /// </summary>
    [JsonPropertyName("mailingAddressType")]
    public HealthInsuranceCompanyAddress MailingAddressType
    {
      get => mailingAddressType ??= new();
      set => mailingAddressType = value;
    }

    private HealthInsuranceCompanyAddress mailingAddressType;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
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
#endregion
}
