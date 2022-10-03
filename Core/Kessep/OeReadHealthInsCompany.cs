// Program: OE_READ_HEALTH_INS_COMPANY, ID: 371853942, model: 746.
// Short name: SWE00956
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: OE_READ_HEALTH_INS_COMPANY.
/// </summary>
[Serializable]
public partial class OeReadHealthInsCompany: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_READ_HEALTH_INS_COMPANY program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeReadHealthInsCompany(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeReadHealthInsCompany.
  /// </summary>
  public OeReadHealthInsCompany(IContext context, Import import, Export export):
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
    // 02/02/95  Sid             Rework/Completion.
    // ---------------------------------------------
    // ---------------------------------------------
    // The mailing address is the only address
    // needed to be displayed. So we set a local
    // attribute and set the address type to MAILING
    // and put this attribute in the search cond.
    // ---------------------------------------------
    UseOeCabSetMnemonics();
    export.HealthInsuranceCompany.Assign(import.HealthInsuranceCompany);
    export.HealthInsuranceCompanyAddress.Assign(
      import.HealthInsuranceCompanyAddress);

    // ---------------------------------------------
    // If the identifier exists, Read the record for
    // that identifier.
    // ---------------------------------------------
    if (import.HealthInsuranceCompany.Identifier != 0)
    {
      if (ReadHealthInsuranceCompany2())
      {
        ExitState = "ACO_NI0000_SUCCESSFUL_DISPLAY";
        export.HealthInsuranceCompany.Assign(entities.HealthInsuranceCompany);

        if (ReadHealthInsuranceCompanyAddress())
        {
          export.HealthInsuranceCompanyAddress.Assign(
            entities.HealthInsuranceCompanyAddress);

          return;
        }

        ExitState = "HEALTH_INS_CARRIER_ADDRESS_NF";
      }
      else
      {
        ExitState = "HEALTH_INSURANCE_COMPANY_NF_RB";
      }

      return;
    }

    // ---------------------------------------------
    // Read the data base by the carrier code if one
    // exists.
    // ---------------------------------------------
    if (!IsEmpty(import.HealthInsuranceCompany.CarrierCode))
    {
      if (ReadHealthInsuranceCompany1())
      {
        ExitState = "ACO_NI0000_SUCCESSFUL_DISPLAY";
        export.HealthInsuranceCompany.Assign(entities.HealthInsuranceCompany);

        if (ReadHealthInsuranceCompanyAddress())
        {
          export.HealthInsuranceCompanyAddress.Assign(
            entities.HealthInsuranceCompanyAddress);

          return;
        }

        ExitState = "HEALTH_INS_CARRIER_ADDRESS_NF";
      }
      else
      {
        ExitState = "HEALTH_INSURANCE_COMPANY_NF_RB";
      }

      return;
    }

    // ---------------------------------------------
    // Read the data base by a combination of
    // Company Name, Address Line 1, City, and State.
    // ---------------------------------------------
    if (!IsEmpty(import.HealthInsuranceCompany.InsurancePolicyCarrier))
    {
      if (ReadHealthInsuranceCompanyHealthInsuranceCompanyAddress())
      {
        export.HealthInsuranceCompany.Assign(entities.HealthInsuranceCompany);
        export.HealthInsuranceCompanyAddress.Assign(
          entities.HealthInsuranceCompanyAddress);
        ExitState = "ACO_NI0000_SUCCESSFUL_DISPLAY";

        return;
      }
    }
    else if (ReadHealthInsuranceCompanyAddressHealthInsuranceCompany())
    {
      export.HealthInsuranceCompany.Assign(entities.HealthInsuranceCompany);
      export.HealthInsuranceCompanyAddress.Assign(
        entities.HealthInsuranceCompanyAddress);
      ExitState = "ACO_NI0000_SUCCESSFUL_DISPLAY";

      return;
    }

    if (IsEmpty(entities.HealthInsuranceCompany.InsurancePolicyCarrier))
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

  private bool ReadHealthInsuranceCompanyAddressHealthInsuranceCompany()
  {
    entities.HealthInsuranceCompanyAddress.Populated = false;
    entities.HealthInsuranceCompany.Populated = false;

    return Read("ReadHealthInsuranceCompanyAddressHealthInsuranceCompany",
      (db, command) =>
      {
        db.SetNullableString(
          command, "city", import.HealthInsuranceCompanyAddress.City ?? "");
        db.SetNullableString(
          command, "street1", import.HealthInsuranceCompanyAddress.Street1 ?? ""
          );
        db.SetNullableString(
          command, "addressType", local.MailingAddressType.AddressType ?? "");
      },
      (db, reader) =>
      {
        entities.HealthInsuranceCompanyAddress.HicIdentifier =
          db.GetInt32(reader, 0);
        entities.HealthInsuranceCompany.Identifier = db.GetInt32(reader, 0);
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
        entities.HealthInsuranceCompany.CarrierCode =
          db.GetNullableString(reader, 17);
        entities.HealthInsuranceCompany.InsurancePolicyCarrier =
          db.GetNullableString(reader, 18);
        entities.HealthInsuranceCompany.ContactName =
          db.GetNullableString(reader, 19);
        entities.HealthInsuranceCompany.InsurerPhone =
          db.GetNullableInt32(reader, 20);
        entities.HealthInsuranceCompany.InsurerFax =
          db.GetNullableInt32(reader, 21);
        entities.HealthInsuranceCompany.CreatedBy = db.GetString(reader, 22);
        entities.HealthInsuranceCompany.CreatedTimestamp =
          db.GetDateTime(reader, 23);
        entities.HealthInsuranceCompany.LastUpdatedBy =
          db.GetString(reader, 24);
        entities.HealthInsuranceCompany.LastUpdatedTimestamp =
          db.GetDateTime(reader, 25);
        entities.HealthInsuranceCompany.InsurerFaxExt =
          db.GetNullableString(reader, 26);
        entities.HealthInsuranceCompany.InsurerPhoneExt =
          db.GetNullableString(reader, 27);
        entities.HealthInsuranceCompany.InsurerPhoneAreaCode =
          db.GetNullableInt32(reader, 28);
        entities.HealthInsuranceCompany.InsurerFaxAreaCode =
          db.GetNullableInt32(reader, 29);
        entities.HealthInsuranceCompany.StartDate = db.GetDate(reader, 30);
        entities.HealthInsuranceCompany.EndDate = db.GetDate(reader, 31);
        entities.HealthInsuranceCompanyAddress.Populated = true;
        entities.HealthInsuranceCompany.Populated = true;
      });
  }

  private bool ReadHealthInsuranceCompanyHealthInsuranceCompanyAddress()
  {
    entities.HealthInsuranceCompanyAddress.Populated = false;
    entities.HealthInsuranceCompany.Populated = false;

    return Read("ReadHealthInsuranceCompanyHealthInsuranceCompanyAddress",
      (db, command) =>
      {
        db.SetNullableString(
          command, "policyCarrier",
          import.HealthInsuranceCompany.InsurancePolicyCarrier ?? "");
        db.SetNullableString(
          command, "city", import.HealthInsuranceCompanyAddress.City ?? "");
        db.SetNullableString(
          command, "street1", import.HealthInsuranceCompanyAddress.Street1 ?? ""
          );
        db.SetNullableString(
          command, "addressType", local.MailingAddressType.AddressType ?? "");
      },
      (db, reader) =>
      {
        entities.HealthInsuranceCompany.Identifier = db.GetInt32(reader, 0);
        entities.HealthInsuranceCompanyAddress.HicIdentifier =
          db.GetInt32(reader, 0);
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
        entities.HealthInsuranceCompanyAddress.EffectiveDate =
          db.GetDate(reader, 16);
        entities.HealthInsuranceCompanyAddress.Street1 =
          db.GetNullableString(reader, 17);
        entities.HealthInsuranceCompanyAddress.Street2 =
          db.GetNullableString(reader, 18);
        entities.HealthInsuranceCompanyAddress.City =
          db.GetNullableString(reader, 19);
        entities.HealthInsuranceCompanyAddress.State =
          db.GetNullableString(reader, 20);
        entities.HealthInsuranceCompanyAddress.Province =
          db.GetNullableString(reader, 21);
        entities.HealthInsuranceCompanyAddress.PostalCode =
          db.GetNullableString(reader, 22);
        entities.HealthInsuranceCompanyAddress.ZipCode5 =
          db.GetNullableString(reader, 23);
        entities.HealthInsuranceCompanyAddress.ZipCode4 =
          db.GetNullableString(reader, 24);
        entities.HealthInsuranceCompanyAddress.Zip3 =
          db.GetNullableString(reader, 25);
        entities.HealthInsuranceCompanyAddress.Country =
          db.GetNullableString(reader, 26);
        entities.HealthInsuranceCompanyAddress.AddressType =
          db.GetNullableString(reader, 27);
        entities.HealthInsuranceCompanyAddress.CreatedBy =
          db.GetString(reader, 28);
        entities.HealthInsuranceCompanyAddress.CreatedTimestamp =
          db.GetDateTime(reader, 29);
        entities.HealthInsuranceCompanyAddress.LastUpdatedBy =
          db.GetString(reader, 30);
        entities.HealthInsuranceCompanyAddress.LastUpdatedTimestamp =
          db.GetDateTime(reader, 31);
        entities.HealthInsuranceCompanyAddress.Populated = true;
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
