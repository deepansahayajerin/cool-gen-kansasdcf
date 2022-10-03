// Program: SI_QUICK_CONTACT_INFORMATION, ID: 374541245, model: 746.
// Short name: SWE03105
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SI_QUICK_CONTACT_INFORMATION.
/// </summary>
[Serializable]
public partial class SiQuickContactInformation: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_QUICK_CONTACT_INFORMATION program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiQuickContactInformation(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiQuickContactInformation.
  /// </summary>
  public SiQuickContactInformation(IContext context, Import import,
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
    // : Date             Developer        WR#         Description
    // --------------------------------------------------------------------------------------
    //   06-18-2009       Linda Smith      211         Initial Development
    // NOTE:
    //       END of   M A I N T E N A N C E   L O G
    // --------------------------------------------------------------------------------------
    // ***********************************************************************************************************************************************
    // Action block:  The action block SI_QUICK_CONTACT_INFORMATION (SWE03105) 
    // is used to retrieve the Contact Information data for the QUICK system. 
    // In Production, this action block is called by a COBOL stored procedure 
    // that is stored in and executed by DB2. 
    // The stored procedure in DB2 is triggered by a call from a .NET web 
    // service.
    // PStep:  For the action block to be gen'd, it must have a PStep that calls
    // it.  That PStep is SI_QUICK_CI_TEST (QKCITSTP). 
    // The PStep is used to gen the action block and facilitate testing, but is 
    // never executed in Production. 
    // The PStep also uses the external action block 
    // SI_EAB_PROCESS_QUICK_INPUT_FILE (SWEXQR01) to read query records from a
    // data set.
    // Batch Job:  To facilitate testing of this action block, a batch job has 
    // been created.  The batch job is SRRUNQCI. 
    // SRRUNQCI executes the QKCITSTP PStep, which in turn executes the SWE03105
    // action block. 
    // SRRUNQCI is to be used only for testing and should never be migrated to 
    // or executed in the Production environment.
    // ***********************************************************************************************************************************************
    // IMPORTANT
    // !!********************************************
    // *    IF IMPORT OR EXPORT VIEWS ARE MODIFIED THEN THE  *
    // *    DB2 STORED PROCEDURE MUST BE UPDATED TO MATCH!!  *
    // *******************************************************
    ExitState = "ACO_NN0000_ALL_OK";
    UseSiQuickGetCpHeader();

    if (IsExitState("CASE_NF"))
    {
      export.QuickErrorMessages.ErrorCode = "406";
      export.QuickErrorMessages.ErrorMessage = "Case Not Found";

      return;
    }

    if (IsExitState("SC0000_DATA_NOT_DISPLAY_FOR_FV"))
    {
      export.QuickErrorMessages.ErrorCode = "407";
      export.QuickErrorMessages.ErrorMessage =
        "Disclosure prohibited on the case requested.";

      return;
    }

    if (ReadOffice())
    {
      local.PhoneAreaCode.Text3 =
        NumberToString(entities.ExistingOffice.MainPhoneAreaCode.
          GetValueOrDefault(), 3);
      local.PhoneNumber.Text7 =
        NumberToString(entities.ExistingOffice.MainPhoneNumber.
          GetValueOrDefault(), 7);
    }
    else
    {
      local.PhoneAreaCode.Text3 = "";
      local.PhoneNumber.Text7 = "";
    }

    if (ReadCaseAssignment())
    {
      if (ReadOfficeServiceProviderServiceProviderOffice())
      {
        export.QuickWorkerContactInfo.FirstName =
          entities.ExistingServiceProvider.FirstName;
        export.QuickWorkerContactInfo.MiddleInitial =
          entities.ExistingServiceProvider.MiddleInitial;
        export.QuickWorkerContactInfo.LastName =
          entities.ExistingServiceProvider.LastName;
        export.QuickWorkerContactInfo.Name = entities.ExistingOffice.Name;

        // Always want the contact phone number to be that of
        // call center - set to Office 329 phone number.
        // (Office does not have a phone extension attribute to retrieve data 
        // from.)
        export.QuickWorkerContactInfo.WorkPhoneAreaCode =
          local.PhoneAreaCode.Text3;
        export.QuickWorkerContactInfo.WorkPhoneNumber = local.PhoneNumber.Text7;

        if (Lt(0, entities.ExistingOffice.MainFaxPhoneNumber))
        {
          export.QuickWorkerContactInfo.MainFaxPhoneNumber =
            NumberToString(entities.ExistingOffice.MainFaxPhoneNumber.
              GetValueOrDefault(), 7);
        }

        if (Lt(0, entities.ExistingOffice.MainFaxAreaCode))
        {
          export.QuickWorkerContactInfo.MainFaxAreaCode =
            NumberToString(entities.ExistingOffice.MainFaxAreaCode.
              GetValueOrDefault(), 3);
        }

        export.QuickWorkerContactInfo.EmailAddress =
          entities.ExistingServiceProvider.EmailAddress ?? Spaces(50);

        // Check for Service Provider address first -
        // if no address is found then check for Office address.
        // Each sorted to get mailing address first (type of "M") -
        // if no mailing address then get residential address (type of "R")
        if (ReadServiceProviderAddress())
        {
          export.QuickWorkerContactInfo.Street1 =
            entities.ExistingServiceProviderAddress.Street1;
          export.QuickWorkerContactInfo.Street2 =
            entities.ExistingServiceProviderAddress.Street2 ?? Spaces(25);
          export.QuickWorkerContactInfo.City =
            entities.ExistingServiceProviderAddress.City;
          export.QuickWorkerContactInfo.StateProvince =
            entities.ExistingServiceProviderAddress.StateProvince;
          export.QuickWorkerContactInfo.Zip =
            entities.ExistingServiceProviderAddress.Zip ?? Spaces(5);
          export.QuickWorkerContactInfo.Zip4 =
            entities.ExistingServiceProviderAddress.Zip4 ?? Spaces(4);

          goto Read;
        }

        if (ReadOfficeAddress())
        {
          export.QuickWorkerContactInfo.Street1 =
            entities.ExistingOfficeAddress.Street1;
          export.QuickWorkerContactInfo.Street2 =
            entities.ExistingOfficeAddress.Street2 ?? Spaces(25);
          export.QuickWorkerContactInfo.City =
            entities.ExistingOfficeAddress.City;
          export.QuickWorkerContactInfo.StateProvince =
            entities.ExistingOfficeAddress.StateProvince;
          export.QuickWorkerContactInfo.Zip =
            entities.ExistingOfficeAddress.Zip ?? Spaces(5);
          export.QuickWorkerContactInfo.Zip4 =
            entities.ExistingOfficeAddress.Zip4 ?? Spaces(4);
        }
      }
      else
      {
        export.QuickWorkerContactInfo.WorkPhoneAreaCode =
          local.PhoneAreaCode.Text3;
        export.QuickWorkerContactInfo.WorkPhoneNumber = local.PhoneNumber.Text7;
      }

Read:

      export.QuickWorkerContactInfo.ContactTypeCode = "Caseworker";
    }
  }

  private void UseSiQuickGetCpHeader()
  {
    var useImport = new SiQuickGetCpHeader.Import();
    var useExport = new SiQuickGetCpHeader.Export();

    useImport.QuickInQuery.CaseId = import.QuickInQuery.CaseId;

    Call(SiQuickGetCpHeader.Execute, useImport, useExport);

    export.Case1.Number = useExport.Case1.Number;
    export.QuickCpHeader.Assign(useExport.QuickCpHeader);
  }

  private bool ReadCaseAssignment()
  {
    entities.ExistingCaseAssignment.Populated = false;

    return Read("ReadCaseAssignment",
      (db, command) =>
      {
        var date = Now().Date;

        db.SetDate(command, "effectiveDate", date);
        db.SetString(command, "casNo", export.Case1.Number);
      },
      (db, reader) =>
      {
        entities.ExistingCaseAssignment.EffectiveDate = db.GetDate(reader, 0);
        entities.ExistingCaseAssignment.DiscontinueDate =
          db.GetNullableDate(reader, 1);
        entities.ExistingCaseAssignment.CreatedTimestamp =
          db.GetDateTime(reader, 2);
        entities.ExistingCaseAssignment.SpdId = db.GetInt32(reader, 3);
        entities.ExistingCaseAssignment.OffId = db.GetInt32(reader, 4);
        entities.ExistingCaseAssignment.OspCode = db.GetString(reader, 5);
        entities.ExistingCaseAssignment.OspDate = db.GetDate(reader, 6);
        entities.ExistingCaseAssignment.CasNo = db.GetString(reader, 7);
        entities.ExistingCaseAssignment.Populated = true;
      });
  }

  private bool ReadOffice()
  {
    entities.ExistingOffice.Populated = false;

    return Read("ReadOffice",
      null,
      (db, reader) =>
      {
        entities.ExistingOffice.SystemGeneratedId = db.GetInt32(reader, 0);
        entities.ExistingOffice.MainPhoneNumber =
          db.GetNullableInt32(reader, 1);
        entities.ExistingOffice.MainFaxPhoneNumber =
          db.GetNullableInt32(reader, 2);
        entities.ExistingOffice.Name = db.GetString(reader, 3);
        entities.ExistingOffice.DiscontinueDate = db.GetNullableDate(reader, 4);
        entities.ExistingOffice.MainPhoneAreaCode =
          db.GetNullableInt32(reader, 5);
        entities.ExistingOffice.MainFaxAreaCode =
          db.GetNullableInt32(reader, 6);
        entities.ExistingOffice.OffOffice = db.GetNullableInt32(reader, 7);
        entities.ExistingOffice.Populated = true;
      });
  }

  private bool ReadOfficeAddress()
  {
    entities.ExistingOfficeAddress.Populated = false;

    return Read("ReadOfficeAddress",
      (db, command) =>
      {
        db.SetInt32(
          command, "offGeneratedId", entities.ExistingOffice.SystemGeneratedId);
          
      },
      (db, reader) =>
      {
        entities.ExistingOfficeAddress.OffGeneratedId = db.GetInt32(reader, 0);
        entities.ExistingOfficeAddress.Type1 = db.GetString(reader, 1);
        entities.ExistingOfficeAddress.Street1 = db.GetString(reader, 2);
        entities.ExistingOfficeAddress.Street2 =
          db.GetNullableString(reader, 3);
        entities.ExistingOfficeAddress.City = db.GetString(reader, 4);
        entities.ExistingOfficeAddress.StateProvince = db.GetString(reader, 5);
        entities.ExistingOfficeAddress.Zip = db.GetNullableString(reader, 6);
        entities.ExistingOfficeAddress.Zip4 = db.GetNullableString(reader, 7);
        entities.ExistingOfficeAddress.Populated = true;
      });
  }

  private bool ReadOfficeServiceProviderServiceProviderOffice()
  {
    System.Diagnostics.Debug.Assert(entities.ExistingCaseAssignment.Populated);
    entities.ExistingServiceProvider.Populated = false;
    entities.ExistingOfficeServiceProvider.Populated = false;
    entities.ExistingOffice.Populated = false;

    return Read("ReadOfficeServiceProviderServiceProviderOffice",
      (db, command) =>
      {
        var date = Now().Date;

        db.SetDate(
          command, "effectiveDate",
          entities.ExistingCaseAssignment.OspDate.GetValueOrDefault());
        db.SetString(
          command, "roleCode", entities.ExistingCaseAssignment.OspCode);
        db.SetInt32(
          command, "offGeneratedId", entities.ExistingCaseAssignment.OffId);
        db.SetInt32(
          command, "spdGeneratedId", entities.ExistingCaseAssignment.SpdId);
        db.SetNullableDate(command, "discontinueDate", date);
      },
      (db, reader) =>
      {
        entities.ExistingOfficeServiceProvider.SpdGeneratedId =
          db.GetInt32(reader, 0);
        entities.ExistingServiceProvider.SystemGeneratedId =
          db.GetInt32(reader, 0);
        entities.ExistingOfficeServiceProvider.OffGeneratedId =
          db.GetInt32(reader, 1);
        entities.ExistingOffice.SystemGeneratedId = db.GetInt32(reader, 1);
        entities.ExistingOfficeServiceProvider.RoleCode =
          db.GetString(reader, 2);
        entities.ExistingOfficeServiceProvider.EffectiveDate =
          db.GetDate(reader, 3);
        entities.ExistingOfficeServiceProvider.WorkPhoneNumber =
          db.GetInt32(reader, 4);
        entities.ExistingOfficeServiceProvider.WorkFaxNumber =
          db.GetNullableInt32(reader, 5);
        entities.ExistingOfficeServiceProvider.WorkFaxAreaCode =
          db.GetNullableInt32(reader, 6);
        entities.ExistingOfficeServiceProvider.WorkPhoneExtension =
          db.GetNullableString(reader, 7);
        entities.ExistingOfficeServiceProvider.WorkPhoneAreaCode =
          db.GetInt32(reader, 8);
        entities.ExistingServiceProvider.LastName = db.GetString(reader, 9);
        entities.ExistingServiceProvider.FirstName = db.GetString(reader, 10);
        entities.ExistingServiceProvider.MiddleInitial =
          db.GetString(reader, 11);
        entities.ExistingServiceProvider.EmailAddress =
          db.GetNullableString(reader, 12);
        entities.ExistingOffice.MainPhoneNumber =
          db.GetNullableInt32(reader, 13);
        entities.ExistingOffice.MainFaxPhoneNumber =
          db.GetNullableInt32(reader, 14);
        entities.ExistingOffice.Name = db.GetString(reader, 15);
        entities.ExistingOffice.DiscontinueDate =
          db.GetNullableDate(reader, 16);
        entities.ExistingOffice.MainPhoneAreaCode =
          db.GetNullableInt32(reader, 17);
        entities.ExistingOffice.MainFaxAreaCode =
          db.GetNullableInt32(reader, 18);
        entities.ExistingOffice.OffOffice = db.GetNullableInt32(reader, 19);
        entities.ExistingServiceProvider.Populated = true;
        entities.ExistingOfficeServiceProvider.Populated = true;
        entities.ExistingOffice.Populated = true;
      });
  }

  private bool ReadServiceProviderAddress()
  {
    entities.ExistingServiceProviderAddress.Populated = false;

    return Read("ReadServiceProviderAddress",
      (db, command) =>
      {
        db.SetInt32(
          command, "spdGeneratedId",
          entities.ExistingServiceProvider.SystemGeneratedId);
      },
      (db, reader) =>
      {
        entities.ExistingServiceProviderAddress.SpdGeneratedId =
          db.GetInt32(reader, 0);
        entities.ExistingServiceProviderAddress.Type1 = db.GetString(reader, 1);
        entities.ExistingServiceProviderAddress.Street1 =
          db.GetString(reader, 2);
        entities.ExistingServiceProviderAddress.Street2 =
          db.GetNullableString(reader, 3);
        entities.ExistingServiceProviderAddress.City = db.GetString(reader, 4);
        entities.ExistingServiceProviderAddress.StateProvince =
          db.GetString(reader, 5);
        entities.ExistingServiceProviderAddress.Zip =
          db.GetNullableString(reader, 6);
        entities.ExistingServiceProviderAddress.Zip4 =
          db.GetNullableString(reader, 7);
        entities.ExistingServiceProviderAddress.Populated = true;
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
    /// A value of QuickInQuery.
    /// </summary>
    [JsonPropertyName("quickInQuery")]
    public QuickInQuery QuickInQuery
    {
      get => quickInQuery ??= new();
      set => quickInQuery = value;
    }

    private QuickInQuery quickInQuery;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of QuickErrorMessages.
    /// </summary>
    [JsonPropertyName("quickErrorMessages")]
    public QuickErrorMessages QuickErrorMessages
    {
      get => quickErrorMessages ??= new();
      set => quickErrorMessages = value;
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
    /// A value of QuickCpHeader.
    /// </summary>
    [JsonPropertyName("quickCpHeader")]
    public QuickCpHeader QuickCpHeader
    {
      get => quickCpHeader ??= new();
      set => quickCpHeader = value;
    }

    /// <summary>
    /// A value of QuickWorkerContactInfo.
    /// </summary>
    [JsonPropertyName("quickWorkerContactInfo")]
    public QuickWorkerContactInfo QuickWorkerContactInfo
    {
      get => quickWorkerContactInfo ??= new();
      set => quickWorkerContactInfo = value;
    }

    private QuickErrorMessages quickErrorMessages;
    private Case1 case1;
    private QuickCpHeader quickCpHeader;
    private QuickWorkerContactInfo quickWorkerContactInfo;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of PhoneAreaCode.
    /// </summary>
    [JsonPropertyName("phoneAreaCode")]
    public WorkArea PhoneAreaCode
    {
      get => phoneAreaCode ??= new();
      set => phoneAreaCode = value;
    }

    /// <summary>
    /// A value of PhoneNumber.
    /// </summary>
    [JsonPropertyName("phoneNumber")]
    public WorkArea PhoneNumber
    {
      get => phoneNumber ??= new();
      set => phoneNumber = value;
    }

    private WorkArea phoneAreaCode;
    private WorkArea phoneNumber;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of ExistingServiceProviderAddress.
    /// </summary>
    [JsonPropertyName("existingServiceProviderAddress")]
    public ServiceProviderAddress ExistingServiceProviderAddress
    {
      get => existingServiceProviderAddress ??= new();
      set => existingServiceProviderAddress = value;
    }

    /// <summary>
    /// A value of ExistingOfficeAddress.
    /// </summary>
    [JsonPropertyName("existingOfficeAddress")]
    public OfficeAddress ExistingOfficeAddress
    {
      get => existingOfficeAddress ??= new();
      set => existingOfficeAddress = value;
    }

    /// <summary>
    /// A value of ExistingServiceProvider.
    /// </summary>
    [JsonPropertyName("existingServiceProvider")]
    public ServiceProvider ExistingServiceProvider
    {
      get => existingServiceProvider ??= new();
      set => existingServiceProvider = value;
    }

    /// <summary>
    /// A value of ExistingOfficeServiceProvider.
    /// </summary>
    [JsonPropertyName("existingOfficeServiceProvider")]
    public OfficeServiceProvider ExistingOfficeServiceProvider
    {
      get => existingOfficeServiceProvider ??= new();
      set => existingOfficeServiceProvider = value;
    }

    /// <summary>
    /// A value of ExistingOffice.
    /// </summary>
    [JsonPropertyName("existingOffice")]
    public Office ExistingOffice
    {
      get => existingOffice ??= new();
      set => existingOffice = value;
    }

    /// <summary>
    /// A value of ExistingCase.
    /// </summary>
    [JsonPropertyName("existingCase")]
    public Case1 ExistingCase
    {
      get => existingCase ??= new();
      set => existingCase = value;
    }

    /// <summary>
    /// A value of ExistingCaseAssignment.
    /// </summary>
    [JsonPropertyName("existingCaseAssignment")]
    public CaseAssignment ExistingCaseAssignment
    {
      get => existingCaseAssignment ??= new();
      set => existingCaseAssignment = value;
    }

    private ServiceProviderAddress existingServiceProviderAddress;
    private OfficeAddress existingOfficeAddress;
    private ServiceProvider existingServiceProvider;
    private OfficeServiceProvider existingOfficeServiceProvider;
    private Office existingOffice;
    private Case1 existingCase;
    private CaseAssignment existingCaseAssignment;
  }
#endregion
}
