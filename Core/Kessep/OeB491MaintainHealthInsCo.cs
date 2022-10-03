// Program: OE_B491_MAINTAIN_HEALTH_INS_CO, ID: 371175184, model: 746.
// Short name: SWE02488
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: OE_B491_MAINTAIN_HEALTH_INS_CO.
/// </summary>
[Serializable]
public partial class OeB491MaintainHealthInsCo: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_B491_MAINTAIN_HEALTH_INS_CO program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeB491MaintainHealthInsCo(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeB491MaintainHealthInsCo.
  /// </summary>
  public OeB491MaintainHealthInsCo(IContext context, Import import,
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
    export.CompaniesAdded.Count = import.CompaniesAdded.Count;
    export.CompaniesUpdated.Count = import.CompaniesUpdated.Count;

    if (ReadHealthInsuranceCompany2())
    {
      try
      {
        UpdateHealthInsuranceCompany();

        if (ReadHealthInsuranceCompanyAddress())
        {
          try
          {
            UpdateHealthInsuranceCompanyAddress();
            ++export.CompaniesUpdated.Count;
          }
          catch(Exception e1)
          {
            switch(GetErrorCode(e1))
            {
              case ErrorCode.AlreadyExists:
                ExitState = "HEALTH_INSURANCE_COMPANY_ADDR_NU";

                break;
              case ErrorCode.PermittedValueViolation:
                ExitState = "HEALTH_INSURANCE_COMPANY_ADDR_PV";

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
          ExitState = "HEALTH_INSURANCE_COMPANY_ADDR_NF";
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
      ReadHealthInsuranceCompany1();

      try
      {
        CreateHealthInsuranceCompany();

        try
        {
          CreateHealthInsuranceCompanyAddress();
          ++export.CompaniesAdded.Count;
        }
        catch(Exception e1)
        {
          switch(GetErrorCode(e1))
          {
            case ErrorCode.AlreadyExists:
              ExitState = "HEALTH_INSURANCE_COMPANY_ADDR_AE";

              break;
            case ErrorCode.PermittedValueViolation:
              ExitState = "HEALTH_INSURANCE_COMPANY_ADDR_PV";

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
            ExitState = "HEALTH_INSURANCE_COMPANY_AE_RB";

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
  }

  private void CreateHealthInsuranceCompany()
  {
    var identifier = local.Max.Identifier;
    var carrierCode = import.HealthInsuranceCompany.CarrierCode ?? "";
    var insurancePolicyCarrier =
      import.HealthInsuranceCompany.InsurancePolicyCarrier ?? "";
    var contactName = import.HealthInsuranceCompany.ContactName ?? "";
    var insurerPhone =
      import.HealthInsuranceCompany.InsurerPhone.GetValueOrDefault();
    var insurerFax =
      import.HealthInsuranceCompany.InsurerFax.GetValueOrDefault();
    var createdBy = import.ProgramProcessingInfo.Name;
    var createdTimestamp = Now();
    var insurerFaxExt = import.HealthInsuranceCompany.InsurerFaxExt ?? "";
    var insurerPhoneExt = import.HealthInsuranceCompany.InsurerPhoneExt ?? "";
    var insurerPhoneAreaCode =
      import.HealthInsuranceCompany.InsurerPhoneAreaCode.GetValueOrDefault();
    var insurerFaxAreaCode =
      import.HealthInsuranceCompany.InsurerFaxAreaCode.GetValueOrDefault();
    var startDate = import.ProgramProcessingInfo.ProcessDate;
    var endDate = import.Max.Date;

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
    var effectiveDate = import.ProgramProcessingInfo.ProcessDate;
    var street1 = import.HealthInsuranceCompanyAddress.Street1 ?? "";
    var street2 = import.HealthInsuranceCompanyAddress.Street2 ?? "";
    var city = import.HealthInsuranceCompanyAddress.City ?? "";
    var state = import.HealthInsuranceCompanyAddress.State ?? "";
    var zipCode5 = import.HealthInsuranceCompanyAddress.ZipCode5 ?? "";
    var zipCode4 = import.HealthInsuranceCompanyAddress.ZipCode4 ?? "";
    var zip3 = import.HealthInsuranceCompanyAddress.Zip3 ?? "";
    var addressType = "M";
    var createdBy = import.ProgramProcessingInfo.Name;
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
        db.SetNullableString(command, "province", "");
        db.SetNullableString(command, "postalCode", "");
        db.SetNullableString(command, "zipCode5", zipCode5);
        db.SetNullableString(command, "zipCode4", zipCode4);
        db.SetNullableString(command, "zip3", zip3);
        db.SetNullableString(command, "country", "");
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
    entities.HealthInsuranceCompanyAddress.ZipCode5 = zipCode5;
    entities.HealthInsuranceCompanyAddress.ZipCode4 = zipCode4;
    entities.HealthInsuranceCompanyAddress.Zip3 = zip3;
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
    local.Max.Populated = false;

    return Read("ReadHealthInsuranceCompany1",
      null,
      (db, reader) =>
      {
        local.Max.Identifier = db.GetInt32(reader, 0);
        local.Max.Populated = true;
      });
  }

  private bool ReadHealthInsuranceCompany2()
  {
    entities.HealthInsuranceCompany.Populated = false;

    return Read("ReadHealthInsuranceCompany2",
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

  private bool ReadHealthInsuranceCompanyAddress()
  {
    entities.HealthInsuranceCompanyAddress.Populated = false;

    return Read("ReadHealthInsuranceCompanyAddress",
      (db, command) =>
      {
        db.SetInt32(
          command, "hicIdentifier", entities.HealthInsuranceCompany.Identifier);
          
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
        entities.HealthInsuranceCompanyAddress.ZipCode5 =
          db.GetNullableString(reader, 6);
        entities.HealthInsuranceCompanyAddress.ZipCode4 =
          db.GetNullableString(reader, 7);
        entities.HealthInsuranceCompanyAddress.Zip3 =
          db.GetNullableString(reader, 8);
        entities.HealthInsuranceCompanyAddress.AddressType =
          db.GetNullableString(reader, 9);
        entities.HealthInsuranceCompanyAddress.CreatedBy =
          db.GetString(reader, 10);
        entities.HealthInsuranceCompanyAddress.CreatedTimestamp =
          db.GetDateTime(reader, 11);
        entities.HealthInsuranceCompanyAddress.LastUpdatedBy =
          db.GetString(reader, 12);
        entities.HealthInsuranceCompanyAddress.LastUpdatedTimestamp =
          db.GetDateTime(reader, 13);
        entities.HealthInsuranceCompanyAddress.Populated = true;
      });
  }

  private void UpdateHealthInsuranceCompany()
  {
    var insurancePolicyCarrier =
      import.HealthInsuranceCompany.InsurancePolicyCarrier ?? "";
    var contactName = import.HealthInsuranceCompany.ContactName ?? "";
    var insurerPhone =
      import.HealthInsuranceCompany.InsurerPhone.GetValueOrDefault();
    var insurerFax =
      import.HealthInsuranceCompany.InsurerFax.GetValueOrDefault();
    var lastUpdatedBy = import.ProgramProcessingInfo.Name;
    var lastUpdatedTimestamp = Now();
    var insurerFaxExt = import.HealthInsuranceCompany.InsurerFaxExt ?? "";
    var insurerPhoneExt = import.HealthInsuranceCompany.InsurerPhoneExt ?? "";
    var insurerPhoneAreaCode =
      import.HealthInsuranceCompany.InsurerPhoneAreaCode.GetValueOrDefault();
    var insurerFaxAreaCode =
      import.HealthInsuranceCompany.InsurerFaxAreaCode.GetValueOrDefault();

    entities.HealthInsuranceCompany.Populated = false;
    Update("UpdateHealthInsuranceCompany",
      (db, command) =>
      {
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
        db.SetInt32(
          command, "identifier", entities.HealthInsuranceCompany.Identifier);
      });

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
    var lastUpdatedBy = import.ProgramProcessingInfo.Name;
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
    /// A value of Max.
    /// </summary>
    [JsonPropertyName("max")]
    public DateWorkArea Max
    {
      get => max ??= new();
      set => max = value;
    }

    /// <summary>
    /// A value of ProgramProcessingInfo.
    /// </summary>
    [JsonPropertyName("programProcessingInfo")]
    public ProgramProcessingInfo ProgramProcessingInfo
    {
      get => programProcessingInfo ??= new();
      set => programProcessingInfo = value;
    }

    /// <summary>
    /// A value of CompaniesUpdated.
    /// </summary>
    [JsonPropertyName("companiesUpdated")]
    public Common CompaniesUpdated
    {
      get => companiesUpdated ??= new();
      set => companiesUpdated = value;
    }

    /// <summary>
    /// A value of CompaniesAdded.
    /// </summary>
    [JsonPropertyName("companiesAdded")]
    public Common CompaniesAdded
    {
      get => companiesAdded ??= new();
      set => companiesAdded = value;
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
    /// A value of HealthInsuranceCompanyAddress.
    /// </summary>
    [JsonPropertyName("healthInsuranceCompanyAddress")]
    public HealthInsuranceCompanyAddress HealthInsuranceCompanyAddress
    {
      get => healthInsuranceCompanyAddress ??= new();
      set => healthInsuranceCompanyAddress = value;
    }

    private DateWorkArea max;
    private ProgramProcessingInfo programProcessingInfo;
    private Common companiesUpdated;
    private Common companiesAdded;
    private HealthInsuranceCompany healthInsuranceCompany;
    private HealthInsuranceCompanyAddress healthInsuranceCompanyAddress;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of CompaniesUpdated.
    /// </summary>
    [JsonPropertyName("companiesUpdated")]
    public Common CompaniesUpdated
    {
      get => companiesUpdated ??= new();
      set => companiesUpdated = value;
    }

    /// <summary>
    /// A value of CompaniesAdded.
    /// </summary>
    [JsonPropertyName("companiesAdded")]
    public Common CompaniesAdded
    {
      get => companiesAdded ??= new();
      set => companiesAdded = value;
    }

    private Common companiesUpdated;
    private Common companiesAdded;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of Max.
    /// </summary>
    [JsonPropertyName("max")]
    public HealthInsuranceCompany Max
    {
      get => max ??= new();
      set => max = value;
    }

    private HealthInsuranceCompany max;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
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
    /// A value of HealthInsuranceCompanyAddress.
    /// </summary>
    [JsonPropertyName("healthInsuranceCompanyAddress")]
    public HealthInsuranceCompanyAddress HealthInsuranceCompanyAddress
    {
      get => healthInsuranceCompanyAddress ??= new();
      set => healthInsuranceCompanyAddress = value;
    }

    private HealthInsuranceCompany healthInsuranceCompany;
    private HealthInsuranceCompanyAddress healthInsuranceCompanyAddress;
  }
#endregion
}
