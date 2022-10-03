// Program: SI_CREATE_IC_IS_REQ_CONTACT_ADDR, ID: 372513436, model: 746.
// Short name: SWE01129
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: SI_CREATE_IC_IS_REQ_CONTACT_ADDR.
/// </para>
/// <para>
/// RESP: SRVINIT
/// This CAB creates the incomming referral Interstate Contact Address which 
/// contains the information about an other state contact address.
/// </para>
/// </summary>
[Serializable]
public partial class SiCreateIcIsReqContactAddr: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_CREATE_IC_IS_REQ_CONTACT_ADDR program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiCreateIcIsReqContactAddr(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiCreateIcIsReqContactAddr.
  /// </summary>
  public SiCreateIcIsReqContactAddr(IContext context, Import import,
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
    // ************************************************************
    // 03/23/98	Siraj Konkader		ZDEL cleanup
    // ************************************************************
    // -----------------------------------------
    // 03/19/99 W.Campbell     Added set statements
    //                         for mandatory attributes in the
    //                         create statement.  Also added
    //                         a local timestamp view and
    //                         modified statements to use it.
    //                         Also added PV and AE exit states.
    // -----------------------------------------
    // 06/23/99  M. Lachowicz  Change property of READ
    //                         (Select Only or Cursor Only)
    // ------------------------------------------------------------
    // 09/02/99  C. Scroggins Added code for set statements for State,
    //           Zip Code, and Zip4 fields during the create.
    // ------------------------------------------------------------
    // 12/01/00  C. Scroggins Modified code to set interstate contact address 
    // type to CT.
    // ------------------------------------------------------------
    local.Current.Timestamp = Now();
    UseOeCabSetMnemonics();

    // 06/23/99 M.L
    //              Change property of the following READ to generate
    //              SELECT ONLY
    if (ReadInterstateContact())
    {
      try
      {
        CreateInterstateContactAddress();
        export.InterstateContactAddress.
          Assign(entities.InterstateContactAddress);
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "INTERSTATE_CONTACT_ADDRESS_AE";

            break;
          case ErrorCode.PermittedValueViolation:
            ExitState = "INTERSTATE_CONTACT_ADDRESS_PV";

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
      ExitState = "INTERSTATE_REQUEST_CONTACT_NF";
    }
  }

  private void UseOeCabSetMnemonics()
  {
    var useImport = new OeCabSetMnemonics.Import();
    var useExport = new OeCabSetMnemonics.Export();

    Call(OeCabSetMnemonics.Execute, useImport, useExport);

    local.MailingAddress.AddressType = useExport.MailingAddressType.AddressType;
    local.MaxDate.ExpirationDate = useExport.MaxDate.ExpirationDate;
  }

  private void CreateInterstateContactAddress()
  {
    System.Diagnostics.Debug.Assert(entities.InterstateContact.Populated);

    var icoContStartDt = entities.InterstateContact.StartDate;
    var intGeneratedId = entities.InterstateContact.IntGeneratedId;
    var startDate = import.InterstateCase.TransactionDate;
    var createdBy = global.UserId;
    var createdTimestamp = local.Current.Timestamp;
    var type1 = "CT";
    var street1 = import.InterstateCase.ContactAddressLine1;
    var street2 = import.InterstateCase.ContactAddressLine2 ?? "";
    var city = import.InterstateCase.ContactCity ?? "";
    var endDate = local.MaxDate.ExpirationDate;
    var state = import.InterstateCase.ContactState ?? "";
    var zipCode = import.InterstateCase.ContactZipCode5 ?? "";
    var zip4 = import.InterstateCase.ContactZipCode4 ?? "";
    var locationType = "D";

    CheckValid<InterstateContactAddress>("LocationType", locationType);
    entities.InterstateContactAddress.Populated = false;
    Update("CreateInterstateContactAddress",
      (db, command) =>
      {
        db.SetDate(command, "icoContStartDt", icoContStartDt);
        db.SetInt32(command, "intGeneratedId", intGeneratedId);
        db.SetDate(command, "startDate", startDate);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetString(command, "lastUpdatedBy", createdBy);
        db.SetDateTime(command, "lastUpdatedTimes", createdTimestamp);
        db.SetNullableString(command, "type", type1);
        db.SetNullableString(command, "street1", street1);
        db.SetNullableString(command, "street2", street2);
        db.SetNullableString(command, "city", city);
        db.SetNullableDate(command, "endDate", endDate);
        db.SetNullableString(command, "county", "");
        db.SetNullableString(command, "state", state);
        db.SetNullableString(command, "zipCode", zipCode);
        db.SetNullableString(command, "zip4", zip4);
        db.SetNullableString(command, "zip3", "");
        db.SetNullableString(command, "street3", "");
        db.SetNullableString(command, "street4", "");
        db.SetNullableString(command, "province", "");
        db.SetNullableString(command, "postalCode", "");
        db.SetNullableString(command, "country", "");
        db.SetString(command, "locationType", locationType);
      });

    entities.InterstateContactAddress.IcoContStartDt = icoContStartDt;
    entities.InterstateContactAddress.IntGeneratedId = intGeneratedId;
    entities.InterstateContactAddress.StartDate = startDate;
    entities.InterstateContactAddress.CreatedBy = createdBy;
    entities.InterstateContactAddress.CreatedTimestamp = createdTimestamp;
    entities.InterstateContactAddress.LastUpdatedBy = createdBy;
    entities.InterstateContactAddress.LastUpdatedTimestamp = createdTimestamp;
    entities.InterstateContactAddress.Type1 = type1;
    entities.InterstateContactAddress.Street1 = street1;
    entities.InterstateContactAddress.Street2 = street2;
    entities.InterstateContactAddress.City = city;
    entities.InterstateContactAddress.EndDate = endDate;
    entities.InterstateContactAddress.County = "";
    entities.InterstateContactAddress.State = state;
    entities.InterstateContactAddress.ZipCode = zipCode;
    entities.InterstateContactAddress.Zip4 = zip4;
    entities.InterstateContactAddress.Zip3 = "";
    entities.InterstateContactAddress.Street3 = "";
    entities.InterstateContactAddress.Street4 = "";
    entities.InterstateContactAddress.Province = "";
    entities.InterstateContactAddress.PostalCode = "";
    entities.InterstateContactAddress.Country = "";
    entities.InterstateContactAddress.LocationType = locationType;
    entities.InterstateContactAddress.Populated = true;
  }

  private bool ReadInterstateContact()
  {
    entities.InterstateContact.Populated = false;

    return Read("ReadInterstateContact",
      (db, command) =>
      {
        db.
          SetInt32(command, "intGeneratedId", import.Persistent.IntHGeneratedId);
          
        db.SetDate(
          command, "startDate",
          import.InterstateContact.StartDate.GetValueOrDefault());
        db.SetNullableDate(
          command, "endDate",
          import.InterstateContact.EndDate.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.InterstateContact.IntGeneratedId = db.GetInt32(reader, 0);
        entities.InterstateContact.StartDate = db.GetDate(reader, 1);
        entities.InterstateContact.EndDate = db.GetNullableDate(reader, 2);
        entities.InterstateContact.Populated = true;
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
    /// A value of Persistent.
    /// </summary>
    [JsonPropertyName("persistent")]
    public InterstateRequest Persistent
    {
      get => persistent ??= new();
      set => persistent = value;
    }

    /// <summary>
    /// A value of InterstateCase.
    /// </summary>
    [JsonPropertyName("interstateCase")]
    public InterstateCase InterstateCase
    {
      get => interstateCase ??= new();
      set => interstateCase = value;
    }

    /// <summary>
    /// A value of InterstateContact.
    /// </summary>
    [JsonPropertyName("interstateContact")]
    public InterstateContact InterstateContact
    {
      get => interstateContact ??= new();
      set => interstateContact = value;
    }

    private InterstateRequest persistent;
    private InterstateCase interstateCase;
    private InterstateContact interstateContact;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of InterstateContactAddress.
    /// </summary>
    [JsonPropertyName("interstateContactAddress")]
    public InterstateContactAddress InterstateContactAddress
    {
      get => interstateContactAddress ??= new();
      set => interstateContactAddress = value;
    }

    private InterstateContactAddress interstateContactAddress;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of MailingAddress.
    /// </summary>
    [JsonPropertyName("mailingAddress")]
    public HealthInsuranceCompanyAddress MailingAddress
    {
      get => mailingAddress ??= new();
      set => mailingAddress = value;
    }

    /// <summary>
    /// A value of MaxDate.
    /// </summary>
    [JsonPropertyName("maxDate")]
    public Code MaxDate
    {
      get => maxDate ??= new();
      set => maxDate = value;
    }

    /// <summary>
    /// A value of Current.
    /// </summary>
    [JsonPropertyName("current")]
    public DateWorkArea Current
    {
      get => current ??= new();
      set => current = value;
    }

    private HealthInsuranceCompanyAddress mailingAddress;
    private Code maxDate;
    private DateWorkArea current;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of InterstateContactAddress.
    /// </summary>
    [JsonPropertyName("interstateContactAddress")]
    public InterstateContactAddress InterstateContactAddress
    {
      get => interstateContactAddress ??= new();
      set => interstateContactAddress = value;
    }

    /// <summary>
    /// A value of Zdel.
    /// </summary>
    [JsonPropertyName("zdel")]
    public InterstateRequest Zdel
    {
      get => zdel ??= new();
      set => zdel = value;
    }

    /// <summary>
    /// A value of InterstateContact.
    /// </summary>
    [JsonPropertyName("interstateContact")]
    public InterstateContact InterstateContact
    {
      get => interstateContact ??= new();
      set => interstateContact = value;
    }

    private InterstateContactAddress interstateContactAddress;
    private InterstateRequest zdel;
    private InterstateContact interstateContact;
  }
#endregion
}
