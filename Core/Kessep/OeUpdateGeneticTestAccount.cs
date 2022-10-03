// Program: OE_UPDATE_GENETIC_TEST_ACCOUNT, ID: 371791035, model: 746.
// Short name: SWE00967
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: OE_UPDATE_GENETIC_TEST_ACCOUNT.
/// </summary>
[Serializable]
public partial class OeUpdateGeneticTestAccount: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_UPDATE_GENETIC_TEST_ACCOUNT program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeUpdateGeneticTestAccount(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeUpdateGeneticTestAccount.
  /// </summary>
  public OeUpdateGeneticTestAccount(IContext context, Import import,
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
    // ---------------------------------------------------------------------------------------
    // This CAB is rewritten by Anand Katuri to accomodate new changes/
    // requirements. The old functionality used to be that any
    // GENETIC_TEST_ACCOUNT that has a GENETIC_TEST associated to it was not
    // allowed to change the OFFICE_SERVICE_PROVIDER or reassign a different
    // GENETIC_TEST_ACCOUNT to an existing OFFICE_SERVICE_PROVIDER.
    // 
    // The new functionality allows for both conditions to be met and is assumed
    // to be a part of the enhancements phase.
    // 
    // For eg. when an existing Attorney leaves the office and a new Attorney
    // comes in his/her place, the system would enable the transfers of
    // GENETIC_TEST_ACCOUNTS to the new ATTORNEY
    // 
    // 10/04/1999   Anand Katuri    H00075107
    // 02/19/2001   Madhu Kumar   WR 286 .
    // --------------------------------------------------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";

    // *** Get the OSP
    if (!ReadOfficeServiceProvider1())
    {
      ExitState = "OFFICE_SERVICE_PROVIDER_NF";

      return;
    }

    // *** If the new GTA account number is spaces, try disassociating the GTA
    if (IsEmpty(import.New1.AccountNumber))
    {
      if (ReadGeneticTestAccount1())
      {
        // *** Read only purpose
        if (ReadGeneticTest())
        {
          // *** Disassociate will result in -532 if GT's exist; Error out
          ExitState = "SP0000_GEN_TEST_ACCT_IN_USE";

          return;
        }
        else
        {
          // *** Peace; Disassociate the GTA; Will cascade to children
          DisassociateGeneticTestAccount();
        }
      }

      return;
    }

    if (ReadGeneticTestAccount1())
    {
      // *** No change needed if the GTA account numbers supplied are the same.
      if (Equal(entities.ExistingCurrent.AccountNumber,
        import.New1.AccountNumber))
      {
        return;
      }

      // *** Read only purpose
      if (ReadGeneticTest())
      {
        // *** If the old GTA has associated GT's, then old GT's have nowhere to
        // go after the proposed disaassociate. So give an error message.
        ExitState = "LE0000_CANT_XFER_TO_OSP_WITH_GTA";

        return;
      }
      else
      {
        // *** Peace; Disassociate the old GTA; Will cascade to children
        DisassociateGeneticTestAccount();
      }
    }

    if (ReadGeneticTestAccount2())
    {
      // *** Is this account already associated with another 
      // OFFICE_SERVICE_PROVIDER?
      if (ReadOfficeServiceProvider2())
      {
        // *** Transfer from old OSP to new OSP
        TransferGeneticTestAccount();
        ExitState = "ACO_NI0000_SUCCESSFUL_TRANSFER";
      }
      else
      {
        // *** Associate GTA to new OSP
        AssociateGeneticTestAccount();
      }
    }
    else
    {
      try
      {
        CreateGeneticTestAccount();
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "GENETIC_TEST_ACCOUNT_AE";

            break;
          case ErrorCode.PermittedValueViolation:
            ExitState = "GENETIC_TEST_ACCOUNT_PV";

            break;
          case ErrorCode.DatabaseError:
            break;
          default:
            throw;
        }
      }
    }
  }

  private void AssociateGeneticTestAccount()
  {
    System.Diagnostics.Debug.Assert(entities.OfficeServiceProvider.Populated);

    var ospEffectiveDate = entities.OfficeServiceProvider.EffectiveDate;
    var ospRoleCode = entities.OfficeServiceProvider.RoleCode;
    var offGeneratedId = entities.OfficeServiceProvider.OffGeneratedId;
    var spdGeneratedId = entities.OfficeServiceProvider.SpdGeneratedId;

    entities.New1.Populated = false;
    Update("AssociateGeneticTestAccount",
      (db, command) =>
      {
        db.SetNullableDate(command, "ospEffectiveDate", ospEffectiveDate);
        db.SetNullableString(command, "ospRoleCode", ospRoleCode);
        db.SetNullableInt32(command, "offGeneratedId", offGeneratedId);
        db.SetNullableInt32(command, "spdGeneratedId", spdGeneratedId);
        db.SetString(command, "accountNumber", entities.New1.AccountNumber);
      });

    entities.New1.OspEffectiveDate = ospEffectiveDate;
    entities.New1.OspRoleCode = ospRoleCode;
    entities.New1.OffGeneratedId = offGeneratedId;
    entities.New1.SpdGeneratedId = spdGeneratedId;
    entities.New1.Populated = true;
  }

  private void CreateGeneticTestAccount()
  {
    System.Diagnostics.Debug.Assert(entities.OfficeServiceProvider.Populated);

    var accountNumber = import.New1.AccountNumber;
    var createdBy = global.UserId;
    var createdTimestamp = Now();
    var lastUpdatedTimestamp = local.InitialisedToZeros.LastUpdatedTimestamp;
    var ospEffectiveDate = entities.OfficeServiceProvider.EffectiveDate;
    var ospRoleCode = entities.OfficeServiceProvider.RoleCode;
    var offGeneratedId = entities.OfficeServiceProvider.OffGeneratedId;
    var spdGeneratedId = entities.OfficeServiceProvider.SpdGeneratedId;

    entities.New1.Populated = false;
    Update("CreateGeneticTestAccount",
      (db, command) =>
      {
        db.SetString(command, "accountNumber", accountNumber);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetString(command, "lastUpdatedBy", "");
        db.SetDateTime(command, "lastUpdatedTmst", lastUpdatedTimestamp);
        db.SetNullableDate(command, "ospEffectiveDate", ospEffectiveDate);
        db.SetNullableString(command, "ospRoleCode", ospRoleCode);
        db.SetNullableInt32(command, "offGeneratedId", offGeneratedId);
        db.SetNullableInt32(command, "spdGeneratedId", spdGeneratedId);
      });

    entities.New1.AccountNumber = accountNumber;
    entities.New1.CreatedBy = createdBy;
    entities.New1.CreatedTimestamp = createdTimestamp;
    entities.New1.LastUpdatedBy = "";
    entities.New1.LastUpdatedTimestamp = lastUpdatedTimestamp;
    entities.New1.OspEffectiveDate = ospEffectiveDate;
    entities.New1.OspRoleCode = ospRoleCode;
    entities.New1.OffGeneratedId = offGeneratedId;
    entities.New1.SpdGeneratedId = spdGeneratedId;
    entities.New1.Populated = true;
  }

  private void DisassociateGeneticTestAccount()
  {
    Update("DisassociateGeneticTestAccount",
      (db, command) =>
      {
        db.SetString(
          command, "accountNumber", entities.ExistingCurrent.AccountNumber);
      });
  }

  private bool ReadGeneticTest()
  {
    entities.GeneticTest.Populated = false;

    return Read("ReadGeneticTest",
      (db, command) =>
      {
        db.SetNullableString(
          command, "gtaAccountNumber", entities.ExistingCurrent.AccountNumber);
      },
      (db, reader) =>
      {
        entities.GeneticTest.TestNumber = db.GetInt32(reader, 0);
        entities.GeneticTest.TestType = db.GetNullableString(reader, 1);
        entities.GeneticTest.GtaAccountNumber = db.GetNullableString(reader, 2);
        entities.GeneticTest.Populated = true;
      });
  }

  private bool ReadGeneticTestAccount1()
  {
    System.Diagnostics.Debug.Assert(entities.OfficeServiceProvider.Populated);
    entities.ExistingCurrent.Populated = false;

    return Read("ReadGeneticTestAccount1",
      (db, command) =>
      {
        db.SetNullableString(
          command, "ospRoleCode", entities.OfficeServiceProvider.RoleCode);
        db.SetNullableDate(
          command, "ospEffectiveDate",
          entities.OfficeServiceProvider.EffectiveDate.GetValueOrDefault());
        db.SetNullableInt32(
          command, "offGeneratedId",
          entities.OfficeServiceProvider.OffGeneratedId);
        db.SetNullableInt32(
          command, "spdGeneratedId",
          entities.OfficeServiceProvider.SpdGeneratedId);
        db.SetString(command, "accountNumber", import.Old.AccountNumber);
      },
      (db, reader) =>
      {
        entities.ExistingCurrent.AccountNumber = db.GetString(reader, 0);
        entities.ExistingCurrent.OspEffectiveDate =
          db.GetNullableDate(reader, 1);
        entities.ExistingCurrent.OspRoleCode = db.GetNullableString(reader, 2);
        entities.ExistingCurrent.OffGeneratedId =
          db.GetNullableInt32(reader, 3);
        entities.ExistingCurrent.SpdGeneratedId =
          db.GetNullableInt32(reader, 4);
        entities.ExistingCurrent.Populated = true;
      });
  }

  private bool ReadGeneticTestAccount2()
  {
    entities.New1.Populated = false;

    return Read("ReadGeneticTestAccount2",
      (db, command) =>
      {
        db.SetString(command, "accountNumber", import.New1.AccountNumber);
      },
      (db, reader) =>
      {
        entities.New1.AccountNumber = db.GetString(reader, 0);
        entities.New1.CreatedBy = db.GetString(reader, 1);
        entities.New1.CreatedTimestamp = db.GetDateTime(reader, 2);
        entities.New1.LastUpdatedBy = db.GetString(reader, 3);
        entities.New1.LastUpdatedTimestamp = db.GetDateTime(reader, 4);
        entities.New1.OspEffectiveDate = db.GetNullableDate(reader, 5);
        entities.New1.OspRoleCode = db.GetNullableString(reader, 6);
        entities.New1.OffGeneratedId = db.GetNullableInt32(reader, 7);
        entities.New1.SpdGeneratedId = db.GetNullableInt32(reader, 8);
        entities.New1.Populated = true;
      });
  }

  private bool ReadOfficeServiceProvider1()
  {
    entities.OfficeServiceProvider.Populated = false;

    return Read("ReadOfficeServiceProvider1",
      (db, command) =>
      {
        db.SetInt32(
          command, "spdGeneratedId", import.ServiceProvider.SystemGeneratedId);
        db.SetInt32(command, "offGeneratedId", import.Office.SystemGeneratedId);
        db.SetDate(
          command, "effectiveDate",
          import.OfficeServiceProvider.EffectiveDate.GetValueOrDefault());
        db.
          SetString(command, "roleCode", import.OfficeServiceProvider.RoleCode);
          
      },
      (db, reader) =>
      {
        entities.OfficeServiceProvider.SpdGeneratedId = db.GetInt32(reader, 0);
        entities.OfficeServiceProvider.OffGeneratedId = db.GetInt32(reader, 1);
        entities.OfficeServiceProvider.RoleCode = db.GetString(reader, 2);
        entities.OfficeServiceProvider.EffectiveDate = db.GetDate(reader, 3);
        entities.OfficeServiceProvider.WorkPhoneNumber = db.GetInt32(reader, 4);
        entities.OfficeServiceProvider.WorkFaxNumber =
          db.GetNullableInt32(reader, 5);
        entities.OfficeServiceProvider.DiscontinueDate =
          db.GetNullableDate(reader, 6);
        entities.OfficeServiceProvider.LastUpdatedBy =
          db.GetNullableString(reader, 7);
        entities.OfficeServiceProvider.LastUpdatedDtstamp =
          db.GetNullableDateTime(reader, 8);
        entities.OfficeServiceProvider.WorkPhoneExtension =
          db.GetNullableString(reader, 9);
        entities.OfficeServiceProvider.WorkPhoneAreaCode =
          db.GetInt32(reader, 10);
        entities.OfficeServiceProvider.Populated = true;
      });
  }

  private bool ReadOfficeServiceProvider2()
  {
    System.Diagnostics.Debug.Assert(entities.New1.Populated);
    entities.ExistingAnotherOfficeServiceProvider.Populated = false;

    return Read("ReadOfficeServiceProvider2",
      (db, command) =>
      {
        db.SetString(command, "roleCode", entities.New1.OspRoleCode ?? "");
        db.SetDate(
          command, "effectiveDate",
          entities.New1.OspEffectiveDate.GetValueOrDefault());
        db.SetInt32(
          command, "offGeneratedId",
          entities.New1.OffGeneratedId.GetValueOrDefault());
        db.SetInt32(
          command, "spdGeneratedId",
          entities.New1.SpdGeneratedId.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ExistingAnotherOfficeServiceProvider.SpdGeneratedId =
          db.GetInt32(reader, 0);
        entities.ExistingAnotherOfficeServiceProvider.OffGeneratedId =
          db.GetInt32(reader, 1);
        entities.ExistingAnotherOfficeServiceProvider.RoleCode =
          db.GetString(reader, 2);
        entities.ExistingAnotherOfficeServiceProvider.EffectiveDate =
          db.GetDate(reader, 3);
        entities.ExistingAnotherOfficeServiceProvider.Populated = true;
      });
  }

  private void TransferGeneticTestAccount()
  {
    System.Diagnostics.Debug.Assert(entities.OfficeServiceProvider.Populated);

    var ospEffectiveDate = entities.OfficeServiceProvider.EffectiveDate;
    var ospRoleCode = entities.OfficeServiceProvider.RoleCode;
    var offGeneratedId = entities.OfficeServiceProvider.OffGeneratedId;
    var spdGeneratedId = entities.OfficeServiceProvider.SpdGeneratedId;

    entities.New1.Populated = false;
    Update("TransferGeneticTestAccount",
      (db, command) =>
      {
        db.SetNullableDate(command, "ospEffectiveDate", ospEffectiveDate);
        db.SetNullableString(command, "ospRoleCode", ospRoleCode);
        db.SetNullableInt32(command, "offGeneratedId", offGeneratedId);
        db.SetNullableInt32(command, "spdGeneratedId", spdGeneratedId);
        db.SetString(command, "accountNumber", entities.New1.AccountNumber);
      });

    entities.New1.OspEffectiveDate = ospEffectiveDate;
    entities.New1.OspRoleCode = ospRoleCode;
    entities.New1.OffGeneratedId = offGeneratedId;
    entities.New1.SpdGeneratedId = spdGeneratedId;
    entities.New1.Populated = true;
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
    /// A value of Old.
    /// </summary>
    [JsonPropertyName("old")]
    public GeneticTestAccount Old
    {
      get => old ??= new();
      set => old = value;
    }

    /// <summary>
    /// A value of New1.
    /// </summary>
    [JsonPropertyName("new1")]
    public GeneticTestAccount New1
    {
      get => new1 ??= new();
      set => new1 = value;
    }

    /// <summary>
    /// A value of OfficeServiceProvider.
    /// </summary>
    [JsonPropertyName("officeServiceProvider")]
    public OfficeServiceProvider OfficeServiceProvider
    {
      get => officeServiceProvider ??= new();
      set => officeServiceProvider = value;
    }

    /// <summary>
    /// A value of CseOrganization.
    /// </summary>
    [JsonPropertyName("cseOrganization")]
    public CseOrganization CseOrganization
    {
      get => cseOrganization ??= new();
      set => cseOrganization = value;
    }

    /// <summary>
    /// A value of Office.
    /// </summary>
    [JsonPropertyName("office")]
    public Office Office
    {
      get => office ??= new();
      set => office = value;
    }

    /// <summary>
    /// A value of ServiceProvider.
    /// </summary>
    [JsonPropertyName("serviceProvider")]
    public ServiceProvider ServiceProvider
    {
      get => serviceProvider ??= new();
      set => serviceProvider = value;
    }

    private GeneticTestAccount old;
    private GeneticTestAccount new1;
    private OfficeServiceProvider officeServiceProvider;
    private CseOrganization cseOrganization;
    private Office office;
    private ServiceProvider serviceProvider;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of OfficeServiceProvider.
    /// </summary>
    [JsonPropertyName("officeServiceProvider")]
    public OfficeServiceProvider OfficeServiceProvider
    {
      get => officeServiceProvider ??= new();
      set => officeServiceProvider = value;
    }

    private OfficeServiceProvider officeServiceProvider;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of InitialisedToZeros.
    /// </summary>
    [JsonPropertyName("initialisedToZeros")]
    public GeneticTestAccount InitialisedToZeros
    {
      get => initialisedToZeros ??= new();
      set => initialisedToZeros = value;
    }

    private GeneticTestAccount initialisedToZeros;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of PersonGeneticTest.
    /// </summary>
    [JsonPropertyName("personGeneticTest")]
    public PersonGeneticTest PersonGeneticTest
    {
      get => personGeneticTest ??= new();
      set => personGeneticTest = value;
    }

    /// <summary>
    /// A value of GeneticTest.
    /// </summary>
    [JsonPropertyName("geneticTest")]
    public GeneticTest GeneticTest
    {
      get => geneticTest ??= new();
      set => geneticTest = value;
    }

    /// <summary>
    /// A value of ExistingAnotherOfficeServiceProvider.
    /// </summary>
    [JsonPropertyName("existingAnotherOfficeServiceProvider")]
    public OfficeServiceProvider ExistingAnotherOfficeServiceProvider
    {
      get => existingAnotherOfficeServiceProvider ??= new();
      set => existingAnotherOfficeServiceProvider = value;
    }

    /// <summary>
    /// A value of ExistingAnotherServiceProvider.
    /// </summary>
    [JsonPropertyName("existingAnotherServiceProvider")]
    public ServiceProvider ExistingAnotherServiceProvider
    {
      get => existingAnotherServiceProvider ??= new();
      set => existingAnotherServiceProvider = value;
    }

    /// <summary>
    /// A value of ExistingAnotherOffice.
    /// </summary>
    [JsonPropertyName("existingAnotherOffice")]
    public Office ExistingAnotherOffice
    {
      get => existingAnotherOffice ??= new();
      set => existingAnotherOffice = value;
    }

    /// <summary>
    /// A value of New1.
    /// </summary>
    [JsonPropertyName("new1")]
    public GeneticTestAccount New1
    {
      get => new1 ??= new();
      set => new1 = value;
    }

    /// <summary>
    /// A value of ExistingCurrent.
    /// </summary>
    [JsonPropertyName("existingCurrent")]
    public GeneticTestAccount ExistingCurrent
    {
      get => existingCurrent ??= new();
      set => existingCurrent = value;
    }

    /// <summary>
    /// A value of CseOrganization.
    /// </summary>
    [JsonPropertyName("cseOrganization")]
    public CseOrganization CseOrganization
    {
      get => cseOrganization ??= new();
      set => cseOrganization = value;
    }

    /// <summary>
    /// A value of Office.
    /// </summary>
    [JsonPropertyName("office")]
    public Office Office
    {
      get => office ??= new();
      set => office = value;
    }

    /// <summary>
    /// A value of ServiceProvider.
    /// </summary>
    [JsonPropertyName("serviceProvider")]
    public ServiceProvider ServiceProvider
    {
      get => serviceProvider ??= new();
      set => serviceProvider = value;
    }

    /// <summary>
    /// A value of OfficeServiceProvider.
    /// </summary>
    [JsonPropertyName("officeServiceProvider")]
    public OfficeServiceProvider OfficeServiceProvider
    {
      get => officeServiceProvider ??= new();
      set => officeServiceProvider = value;
    }

    private PersonGeneticTest personGeneticTest;
    private GeneticTest geneticTest;
    private OfficeServiceProvider existingAnotherOfficeServiceProvider;
    private ServiceProvider existingAnotherServiceProvider;
    private Office existingAnotherOffice;
    private GeneticTestAccount new1;
    private GeneticTestAccount existingCurrent;
    private CseOrganization cseOrganization;
    private Office office;
    private ServiceProvider serviceProvider;
    private OfficeServiceProvider officeServiceProvider;
  }
#endregion
}
