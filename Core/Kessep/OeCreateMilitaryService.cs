// Program: OE_CREATE_MILITARY_SERVICE, ID: 371920133, model: 746.
// Short name: SWE00890
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: OE_CREATE_MILITARY_SERVICE.
/// </para>
/// <para>
/// RESP: OBLGESTB
/// </para>
/// </summary>
[Serializable]
public partial class OeCreateMilitaryService: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_CREATE_MILITARY_SERVICE program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeCreateMilitaryService(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeCreateMilitaryService.
  /// </summary>
  public OeCreateMilitaryService(IContext context, Import import, Export export):
    
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
    //   Date  	Developer  	Description
    // 02/19/95 	Sid		Initial Code.
    // 02/04/96	Tom Redmond	Add BAQ and Retrofit
    // 04/02/96	T.O.Redmond	Remove Military Service BAQ ALlotment and rate of 
    // pay
    // ---------------------------------------------
    // ---------------------------------------------
    // Verify the CSE Person Number entered on the
    // screen that the military record is being
    // created against.
    // ---------------------------------------------
    export.MilitaryService.Assign(import.MilitaryService);

    if (ReadCsePerson())
    {
      export.CsePerson.Number = entities.CsePerson.Number;

      // ************************************************
      // *Find the Income Source before we can add any  *
      // *Military History.                             *
      // ************************************************
      if (!ReadIncomeSource())
      {
        ExitState = "INCOME_SOURCE_NF";

        return;
      }

      // ---------------------------------------------
      // Create the military service record for the
      // CSE Person identified and associate it to the
      // CSE Person, and the Income Source.
      // ---------------------------------------------
      try
      {
        CreateMilitaryService();
        export.MilitaryService.Assign(entities.ExistingMilitaryService);
        ExitState = "ACO_NI0000_SUCCESSFUL_ADD";

        if (ReadPersonIncomeHistory())
        {
          export.PersonIncomeHistory.
            Assign(entities.ExistingPersonIncomeHistory);
        }
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "MILITARY_SERVICE_AE";

            break;
          case ErrorCode.PermittedValueViolation:
            ExitState = "MILITARY_SERVICE_PV";

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
      ExitState = "CSE_PERSON_NF";
    }
  }

  private void CreateMilitaryService()
  {
    var effectiveDate = import.MilitaryService.EffectiveDate;
    var cspNumber = entities.CsePerson.Number;
    var startDate = export.MilitaryService.StartDate;
    var street1 = export.MilitaryService.Street1 ?? "";
    var street2 = export.MilitaryService.Street2 ?? "";
    var city = export.MilitaryService.City ?? "";
    var state = export.MilitaryService.State ?? "";
    var zipCode5 = export.MilitaryService.ZipCode5 ?? "";
    var zipCode4 = export.MilitaryService.ZipCode4 ?? "";
    var zip3 = export.MilitaryService.Zip3 ?? "";
    var country = export.MilitaryService.Country ?? "";
    var apo = export.MilitaryService.Apo ?? "";
    var expectedReturnDateToStates =
      export.MilitaryService.ExpectedReturnDateToStates;
    var overseasDutyStation = export.MilitaryService.OverseasDutyStation ?? "";
    var expectedDischargeDate = export.MilitaryService.ExpectedDischargeDate;
    var phone = export.MilitaryService.Phone.GetValueOrDefault();
    var branchCode = export.MilitaryService.BranchCode ?? "";
    var commandingOfficerLastName =
      export.MilitaryService.CommandingOfficerLastName ?? "";
    var commandingOfficerFirstName =
      export.MilitaryService.CommandingOfficerFirstName ?? "";
    var commandingOfficerMi = export.MilitaryService.CommandingOfficerMi ?? "";
    var currentUsDutyStation = export.MilitaryService.CurrentUsDutyStation ?? ""
      ;
    var dutyStatusCode = export.MilitaryService.DutyStatusCode ?? "";
    var rank = export.MilitaryService.Rank ?? "";
    var endDate = export.MilitaryService.EndDate;
    var createdBy = global.UserId;
    var createdTimestamp = Now();
    var phoneExt = export.MilitaryService.PhoneExt ?? "";
    var phoneAreaCode =
      export.MilitaryService.PhoneAreaCode.GetValueOrDefault();

    entities.ExistingIncomeSource.Populated = false;
    entities.ExistingMilitaryService.Populated = false;
    Update("CreateMilitaryService#1",
      (db, command) =>
      {
        db.SetDate(command, "effectiveDate", effectiveDate);
        db.SetString(command, "cspNumber", cspNumber);
        db.SetNullableDate(command, "startDate", startDate);
        db.SetNullableString(command, "street1", street1);
        db.SetNullableString(command, "street2", street2);
        db.SetNullableString(command, "city", city);
        db.SetNullableString(command, "state", state);
        db.SetNullableString(command, "province", "");
        db.SetNullableString(command, "postalCode", "");
        db.SetNullableString(command, "zipCode5", zipCode5);
        db.SetNullableString(command, "zipCode4", zipCode4);
        db.SetNullableString(command, "zip3", zip3);
        db.SetNullableString(command, "country", country);
        db.SetNullableString(command, "apo", apo);
        db.
          SetNullableDate(command, "expReturnDate", expectedReturnDateToStates);
          
        db.SetNullableString(command, "overseasDutyStn", overseasDutyStation);
        db.SetNullableDate(command, "expDischDate", expectedDischargeDate);
        db.SetNullableInt32(command, "phone", phone);
        db.SetNullableString(command, "branchCode", branchCode);
        db.SetNullableString(command, "coLastName", commandingOfficerLastName);
        db.
          SetNullableString(command, "coFirstName", commandingOfficerFirstName);
          
        db.SetNullableString(command, "coMi", commandingOfficerMi);
        db.SetNullableString(command, "currUsDutyStn", currentUsDutyStation);
        db.SetNullableString(command, "dutyStatusCode", dutyStatusCode);
        db.SetNullableString(command, "rank", rank);
        db.SetNullableDate(command, "endDate", endDate);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetString(command, "lastUpdatedBy", createdBy);
        db.SetDateTime(command, "lastUpdatedTmst", createdTimestamp);
        db.SetNullableString(command, "phoneCountryCode", "");
        db.SetNullableString(command, "phoneExt", phoneExt);
        db.SetNullableInt32(command, "phoneAreaCode", phoneAreaCode);
      });

    Update("CreateMilitaryService#3",
      (db, command) =>
      {
        db.SetNullableDate(command, "mseEffectiveDate", effectiveDate);
        db.SetNullableString(command, "cspSNumber", cspNumber);
        db.SetDateTime(
          command, "identifier",
          entities.ExistingIncomeSource.Identifier.GetValueOrDefault());
        db.SetString(
          command, "cspINumber", entities.ExistingIncomeSource.CspINumber);
      });

    entities.ExistingMilitaryService.EffectiveDate = effectiveDate;
    entities.ExistingMilitaryService.CspNumber = cspNumber;
    entities.ExistingMilitaryService.StartDate = startDate;
    entities.ExistingMilitaryService.Street1 = street1;
    entities.ExistingMilitaryService.Street2 = street2;
    entities.ExistingMilitaryService.City = city;
    entities.ExistingMilitaryService.State = state;
    entities.ExistingMilitaryService.ZipCode5 = zipCode5;
    entities.ExistingMilitaryService.ZipCode4 = zipCode4;
    entities.ExistingMilitaryService.Zip3 = zip3;
    entities.ExistingMilitaryService.Country = country;
    entities.ExistingMilitaryService.Apo = apo;
    entities.ExistingMilitaryService.ExpectedReturnDateToStates =
      expectedReturnDateToStates;
    entities.ExistingMilitaryService.OverseasDutyStation = overseasDutyStation;
    entities.ExistingMilitaryService.ExpectedDischargeDate =
      expectedDischargeDate;
    entities.ExistingMilitaryService.Phone = phone;
    entities.ExistingMilitaryService.BranchCode = branchCode;
    entities.ExistingMilitaryService.CommandingOfficerLastName =
      commandingOfficerLastName;
    entities.ExistingMilitaryService.CommandingOfficerFirstName =
      commandingOfficerFirstName;
    entities.ExistingMilitaryService.CommandingOfficerMi = commandingOfficerMi;
    entities.ExistingMilitaryService.CurrentUsDutyStation =
      currentUsDutyStation;
    entities.ExistingMilitaryService.DutyStatusCode = dutyStatusCode;
    entities.ExistingMilitaryService.Rank = rank;
    entities.ExistingMilitaryService.EndDate = endDate;
    entities.ExistingMilitaryService.CreatedBy = createdBy;
    entities.ExistingMilitaryService.CreatedTimestamp = createdTimestamp;
    entities.ExistingMilitaryService.LastUpdatedBy = createdBy;
    entities.ExistingMilitaryService.LastUpdatedTimestamp = createdTimestamp;
    entities.ExistingMilitaryService.PhoneCountryCode = "";
    entities.ExistingMilitaryService.PhoneExt = phoneExt;
    entities.ExistingMilitaryService.PhoneAreaCode = phoneAreaCode;
    entities.ExistingIncomeSource.MseEffectiveDate = effectiveDate;
    entities.ExistingIncomeSource.CspSNumber = cspNumber;
    entities.ExistingIncomeSource.Populated = true;
    entities.ExistingMilitaryService.Populated = true;
  }

  private bool ReadCsePerson()
  {
    entities.CsePerson.Populated = false;

    return Read("ReadCsePerson",
      (db, command) =>
      {
        db.SetString(command, "numb", import.CsePerson.Number);
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CsePerson.Populated = true;
      });
  }

  private bool ReadIncomeSource()
  {
    entities.ExistingIncomeSource.Populated = false;

    return Read("ReadIncomeSource",
      (db, command) =>
      {
        db.SetString(command, "cspINumber", entities.CsePerson.Number);
        db.SetDateTime(
          command, "identifier",
          import.IncomeSource.Identifier.GetValueOrDefault());
        db.SetString(command, "type", import.IncomeSource.Type1);
      },
      (db, reader) =>
      {
        entities.ExistingIncomeSource.Identifier = db.GetDateTime(reader, 0);
        entities.ExistingIncomeSource.Type1 = db.GetString(reader, 1);
        entities.ExistingIncomeSource.CspINumber = db.GetString(reader, 2);
        entities.ExistingIncomeSource.MseEffectiveDate =
          db.GetNullableDate(reader, 3);
        entities.ExistingIncomeSource.CspSNumber =
          db.GetNullableString(reader, 4);
        entities.ExistingIncomeSource.Populated = true;
        CheckValid<IncomeSource>("Type1", entities.ExistingIncomeSource.Type1);
      });
  }

  private bool ReadPersonIncomeHistory()
  {
    System.Diagnostics.Debug.Assert(entities.ExistingIncomeSource.Populated);
    entities.ExistingPersonIncomeHistory.Populated = false;

    return Read("ReadPersonIncomeHistory",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.CsePerson.Number);
        db.SetString(
          command, "cspINumber", entities.ExistingIncomeSource.CspINumber);
        db.SetDateTime(
          command, "isrIdentifier",
          entities.ExistingIncomeSource.Identifier.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ExistingPersonIncomeHistory.CspNumber =
          db.GetString(reader, 0);
        entities.ExistingPersonIncomeHistory.IsrIdentifier =
          db.GetDateTime(reader, 1);
        entities.ExistingPersonIncomeHistory.Identifier =
          db.GetDateTime(reader, 2);
        entities.ExistingPersonIncomeHistory.IncomeAmt =
          db.GetNullableDecimal(reader, 3);
        entities.ExistingPersonIncomeHistory.CheckMonthlyAmount =
          db.GetNullableDecimal(reader, 4);
        entities.ExistingPersonIncomeHistory.CspINumber =
          db.GetString(reader, 5);
        entities.ExistingPersonIncomeHistory.MilitaryBaqAllotment =
          db.GetNullableDecimal(reader, 6);
        entities.ExistingPersonIncomeHistory.Populated = true;
      });
  }
#endregion

#region Parameters.
  protected readonly Import import;
  protected readonly Export export;
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
    /// A value of IncomeSource.
    /// </summary>
    [JsonPropertyName("incomeSource")]
    public IncomeSource IncomeSource
    {
      get => incomeSource ??= new();
      set => incomeSource = value;
    }

    /// <summary>
    /// A value of MilitaryService.
    /// </summary>
    [JsonPropertyName("militaryService")]
    public MilitaryService MilitaryService
    {
      get => militaryService ??= new();
      set => militaryService = value;
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

    private IncomeSource incomeSource;
    private MilitaryService militaryService;
    private CsePerson csePerson;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of MilitaryService.
    /// </summary>
    [JsonPropertyName("militaryService")]
    public MilitaryService MilitaryService
    {
      get => militaryService ??= new();
      set => militaryService = value;
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
    /// A value of PersonIncomeHistory.
    /// </summary>
    [JsonPropertyName("personIncomeHistory")]
    public PersonIncomeHistory PersonIncomeHistory
    {
      get => personIncomeHistory ??= new();
      set => personIncomeHistory = value;
    }

    private MilitaryService militaryService;
    private CsePerson csePerson;
    private PersonIncomeHistory personIncomeHistory;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of ExistingIncomeSource.
    /// </summary>
    [JsonPropertyName("existingIncomeSource")]
    public IncomeSource ExistingIncomeSource
    {
      get => existingIncomeSource ??= new();
      set => existingIncomeSource = value;
    }

    /// <summary>
    /// A value of ExistingMilitaryService.
    /// </summary>
    [JsonPropertyName("existingMilitaryService")]
    public MilitaryService ExistingMilitaryService
    {
      get => existingMilitaryService ??= new();
      set => existingMilitaryService = value;
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
    /// A value of ExistingPersonIncomeHistory.
    /// </summary>
    [JsonPropertyName("existingPersonIncomeHistory")]
    public PersonIncomeHistory ExistingPersonIncomeHistory
    {
      get => existingPersonIncomeHistory ??= new();
      set => existingPersonIncomeHistory = value;
    }

    private IncomeSource existingIncomeSource;
    private MilitaryService existingMilitaryService;
    private CsePerson csePerson;
    private PersonIncomeHistory existingPersonIncomeHistory;
  }
#endregion
}
